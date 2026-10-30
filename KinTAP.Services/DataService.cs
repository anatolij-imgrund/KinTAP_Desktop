///
/// Zentrale Verwaltung aller Anwendungsdaten (Kinder, Aufgaben, Belohnungen), 
/// verantwortlich für Generierung von Aufgaben für den aktuellen Tag, Laden und Speichern der Daten.
/// 

using System.Collections.ObjectModel;
using System.Diagnostics;
using KinTAP.Model;
using KinTAP.Persistence;

namespace KinTAP.Services;

public class DataService
{
    private readonly IDataPersistence _persistenceService;

    public ObservableCollection<Child> Children { get; private set; }
    public ObservableCollection<TaskItem> TaskDefinitions { get; private set; }
    public ObservableCollection<TaskItem> AssignedTasks { get; private set; }
    public ObservableCollection<Reward> Rewards { get; private set; }
    public ObservableCollection<ClaimedReward> ClaimedRewards { get; private set; }
    public ObservableCollection<TaskSubscription> Subscriptions { get; private set; }

    private int _lastChildId = 0;
    private int _lastTaskId = 0;
    private int _lastRewardId = 0;
    private int _lastClaimedRewardId = 0;
    private int _lastSubscriptionId = 0;

    public DataService(IDataPersistence persistenceService)
    {
        _persistenceService = persistenceService;

        Children = new ObservableCollection<Child>();
        TaskDefinitions = new ObservableCollection<TaskItem>();
        AssignedTasks = new ObservableCollection<TaskItem>();
        Rewards = new ObservableCollection<Reward>();
        ClaimedRewards = new ObservableCollection<ClaimedReward>();
        Subscriptions = new ObservableCollection<TaskSubscription>();
    }

    public async Task LoadAllDataAsync()
    {
        try
        {
            var loadedChildren = await _persistenceService.LoadChildrenAsync();
            Children = new ObservableCollection<Child>(loadedChildren);

            var loadRewards = await _persistenceService.LoadRewardsAsync();
            Rewards = new ObservableCollection<Reward>(loadRewards);

            var allTasks = await _persistenceService.LoadTasksAsync();

            TaskDefinitions.Clear();
            AssignedTasks.Clear();
            foreach(var task in allTasks)
            {
                if(task.AssignedChildId != null)
                {
                    AssignedTasks.Add(task);
                }
                else
                {
                    TaskDefinitions.Add(task);
                }
            }

            var loadedClaims = await _persistenceService.LoadClaimedRewardsAsync();
            ClaimedRewards = new ObservableCollection<ClaimedReward>(loadedClaims);

            var loadedSubscriptions = await _persistenceService.LoadSubscriptionAsync();
            Subscriptions = new ObservableCollection<TaskSubscription>(loadedSubscriptions);

            _lastChildId = Children.Any() ? Children.Max(c => c.Id) : 0;
            _lastTaskId = TaskDefinitions.Select(t => t.Id)
                                        .Concat(AssignedTasks.Select(t => t.Id))
                                        .DefaultIfEmpty(0)
                                        .Max();
            _lastRewardId = Rewards.Any() ? Rewards.Max(r => r.Id) : 0;
            _lastClaimedRewardId = ClaimedRewards.Any() ? ClaimedRewards.Max(cr => cr.Id) : 0;
            _lastSubscriptionId = Subscriptions.Any() ? Subscriptions.Max(s => s.Id) : 0;

            Debug.WriteLine("(Async) Daten wurden erfolgreich geladen");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"(async) Fehler beim Laden: {ex.Message}");
            Children ??= new ObservableCollection<Child>();
            TaskDefinitions ??= new ObservableCollection<TaskItem>();
            AssignedTasks ??= new ObservableCollection<TaskItem>();
            Rewards ??= new ObservableCollection<Reward>();
            ClaimedRewards ??= new ObservableCollection<ClaimedReward>();
        }
    }

    public async Task SaveChangesAsync()
    {
        try
        {
            await _persistenceService.SaveChildrenAsync(Children.ToList());

            var allTasksToSave = TaskDefinitions.Concat(AssignedTasks).ToList();
            await _persistenceService.SaveTasksAsync(allTasksToSave);
            
            await _persistenceService.SaveRewardsAsync(Rewards.ToList());

            await _persistenceService.SaveClaimedRewardsAsync(ClaimedRewards.ToList());

            await _persistenceService.SaveSubscriptionAsync(Subscriptions.ToList());
            
            Debug.WriteLine("(Async) Die Daten wurden erfolgreich gespeichert.");            
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"(Async) Fehler beim Speichern: {ex.Message}");
        }
    }

    public int GenerateNewChildId()
    {
        return ++_lastChildId;
    }

    public int GenerateNewTaskId()
    {
        return ++_lastTaskId;
    }

    /// <summary>
    /// Konvertiert die Wochentage in ein bitweises Flag-System zur einfachen Speicherung und Kombination der geplanten Wochentage.
    /// </summary>
    private static DaysOfWeekFlags ConvertToDaysOfWeekFlags(DayOfWeek dayOfWeek)
    {
        switch (dayOfWeek)
        {
            case DayOfWeek.Monday: return DaysOfWeekFlags.Monday;
            case DayOfWeek.Tuesday: return DaysOfWeekFlags.Tuesday;
            case DayOfWeek.Wednesday: return DaysOfWeekFlags.Wednesday;
            case DayOfWeek.Thursday: return DaysOfWeekFlags.Thursday;
            case DayOfWeek.Friday: return DaysOfWeekFlags.Friday;
            case DayOfWeek.Saturday: return DaysOfWeekFlags.Saturday;
            case DayOfWeek.Sunday: return DaysOfWeekFlags.Sunday;
            default: return DaysOfWeekFlags.None;
        }
    }

    /// <summary>
    /// Generiert alle Aufgaben, die für den Moment geplant sind, in dem sich das Kind anmeldet.
    /// </summary>
    public async Task<bool> GenerateTasksForTodayAsync(int childId)
    {
        var today = DateTime.Today.Date;
        var todayFlag = ConvertToDaysOfWeekFlags(today.DayOfWeek);
        bool changesMade = false;

        var subscribedTemplateIds = Subscriptions.Where(s => s.ChildId == childId).Select(s => s.TaskTemplateId).ToHashSet();

        Debug.WriteLine($"für Kind {childId} für {today:dd.MM.yyyy} ({todayFlag}). Es sind {subscribedTemplateIds.Count} Abonnements vorhanden");

        var definitions = TaskDefinitions.ToList();
        var existingToday = AssignedTasks.Where(t => (t.AssignedChildId == childId) && (t.DueDate.HasValue) && (t.DueDate.Value.Date == today)).ToList();

        Debug.WriteLine($"für Kind {childId} für {today:dd.MM.yyyy} ({todayFlag}). Es wurde für heute {existingToday.Count} gefunden");

        var tasksToAdd = new List<TaskItem>();

        foreach (var definition in definitions)
        {
            try
            {
                if (definition == null)
                {
                    Debug.WriteLine($"!!! NULL definition object in list !!!");
                    continue;
                }
                Debug.WriteLine($"Processing definition ID: {definition.Id}, Title: {definition.Title ?? "NULL TITLE!"}, Type: {definition.RecurrenceType}");
                if (!subscribedTemplateIds.Contains(definition.Id))
                {
                    Debug.WriteLine($"* Aufgabe {definition.Title} mit ID {definition.Id} ist nicht abonniert und wird übersprungen");
                    continue;
                }

                bool shouldGenerate = false;

                switch (definition.RecurrenceType)
                {
                    case TaskRecurrenceType.Daily:
                        shouldGenerate = true;
                        break;
                    case TaskRecurrenceType.Weekly:
                        if (definition.RecurringDaysOfWeek.HasFlag(todayFlag))
                        {
                            shouldGenerate = true;
                        }
                        break;
                    case TaskRecurrenceType.None:
                        if (definition.SpecificDueDate.HasValue && (definition.SpecificDueDate.Value.ToString() == today.ToString()))
                        {
                            shouldGenerate = true;
                        }
                        break;
                }

                if (shouldGenerate)
                {
                    bool alreadyGeneratedForToday = existingToday.Any(t => t.SourceTemplateId == definition.Id);

                    if (!alreadyGeneratedForToday)
                    {
                        var newTaskInstance = new TaskItem
                        {
                            Id = GenerateNewTaskId(),
                            SourceTemplateId = definition.Id,
                            AssignedChildId = childId,
                            DueDate = today,
                            Status = Model.TaskStatus.Pending,
                            Title = definition.Title ?? string.Empty,
                            Description = definition.Description, // + $"{Environment.NewLine}Aufgabe generiert am {DateTime.Now:dd.MM.yyyy}",
                            PointsValue = definition.PointsValue,
                            Priority = definition.Priority,
                            StartTimeOfDay = definition.StartTimeOfDay,
                            EndTimeOfDay = definition.EndTimeOfDay,
                            RecurrenceType = TaskRecurrenceType.None,
                            RecurringDaysOfWeek = DaysOfWeekFlags.None,
                            SpecificDueDate = null
                        };

                        tasksToAdd.Add(newTaskInstance);
                        Debug.WriteLine($"* Aufgabe {newTaskInstance.Title} mit ID {newTaskInstance.Id} wurde generiert (original Aufgabe ID: {definition.Id})");
                    }
                }
            }
            catch (NullReferenceException nex)
            {
                Debug.WriteLine($"NullReference Fehler beim Generieren der Aufgabe mit ID {definition?.Id ?? -1}: {nex.Message}\n{nex.StackTrace}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fehler beim Generieren der Aufgabe mit ID {definition?.Id ?? -1}: {ex.Message}");
            }
        }

        if (tasksToAdd.Any())
        {
            Debug.WriteLine($"{tasksToAdd.Count} neue Aufgaben wurde hinzugefügt...");

            foreach (var task in tasksToAdd)
            {
                AssignedTasks.Add(task);
            }

            await SaveChangesAsync();
            changesMade = true;
        }
        else
        {
            Debug.WriteLine("keine neue Aufgabe gefunden");
        }

        return changesMade;
    }

    public int GenerateNewRewardId()
    {
        return ++_lastRewardId;
    }

    public int GenerateNewClaimedId()
    {
        return ++_lastClaimedRewardId;
    }

    public int GenerateNewSubscriptionId()
    {
        return ++_lastSubscriptionId;
    }
}
