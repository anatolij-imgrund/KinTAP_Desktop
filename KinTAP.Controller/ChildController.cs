/// <summary>
/// Hier befindet sich die Funktionalität des Kinderkontos...
/// </summary>

using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KinTAP.Model;
using KinTAP.Services;

namespace KinTAP.Controller
{
    public partial class ChildController : BaseController
    {
        private readonly DataService _dataService;

        public Child CurrentChild { get; private set; }

        public ObservableCollection<TaskItem> VisibleTasks { get; }

        public ObservableCollection<Reward> AvailableRewards => _dataService.Rewards;

        public ObservableCollection<Reward> MyClaimedRewards { get; }

        public ObservableCollection<string> AvailableAvatarPath { get; } = new();

        public ObservableCollection<string> AvailableThemeColorKeys { get; } = new();

        /// <summary>
        /// Hier und weiter im Code [ObservableProperty] als Teil der CommunityToolkit.Mvvm-Bibliothek
        /// ist für die automatische Aktualisierung der Variablen zuständig, beispielsweise im grafischen Teil.
        /// </summary>
        [ObservableProperty]
        private string _rewardStatusMessage = string.Empty;

        /// <summary>
        /// Konstruktor für Controller
        /// </summary>
        public ChildController(DataService dataService, int CurrentChildId)
        {
            _dataService = dataService;

            var foundChild = _dataService.Children.FirstOrDefault(c => c.Id == CurrentChildId);

            if (foundChild == null)
            {
                Debug.WriteLine($"Fehler! Das Kind mit ID: {CurrentChildId} nicht gefunden");
                CurrentChild = new Child { Id = -1, Name = "Unbekantes Kind" };
                VisibleTasks = new ObservableCollection<TaskItem>();
                MyClaimedRewards = new ObservableCollection<Reward>();
            }
            else
            {
                CurrentChild = foundChild;
                VisibleTasks = new ObservableCollection<TaskItem>();
                MyClaimedRewards = new ObservableCollection<Reward>();
                CurrentChild.PropertyChanged += CurrentChild_PropertyChanged;
            }
        }

        /// <summary>
        /// Bereitet den Controller asynchron vor: 
        /// generiert Aufgaben für den Tag, 
        /// lädt Aufgaben und Belohnungen, 
        /// füllt die Auswahlmöglichkeiten für Einstellungen und setzt ggf. den Avatarpfad.
        /// </summary>
        public async Task InitializeAsync()
        {
            if (CurrentChild == null || CurrentChild.Id == -1)
            {
                return;
            }
            Debug.WriteLine($"ChildController für {CurrentChild.Name} wird initializiert...");
            try
            {
                bool tasksGenerated = await _dataService.GenerateTasksForTodayAsync(CurrentChild.Id);
                if (tasksGenerated)
                {
                    Debug.WriteLine("Aufgabengenerieren ist abgeschlossen. Daten sind gespeichert...");
                }
                LoadVisibleTasks();
                LoadMyClaimedRewards();
                PopulateSettingsChoices();

                if (string.IsNullOrEmpty(CurrentChild.SelectedAvatarPath) || CurrentChild.SelectedAvatarPath == "/Assets/default_avatar.png")
                {
                    CurrentChild.SelectedAvatarPath = !string.IsNullOrEmpty(CurrentChild.ProfileImagePath) ? CurrentChild.ProfileImagePath : "/Assets/default_avatar.png";
                }
                Debug.WriteLine($"ChildController für {CurrentChild.Name} wurde initializiert");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fehler beim ChildControllerinitialisieren. {ex.Message}");
            }
        }

        /// <summary>
        /// Speichert Änderungen am Hintergrund und Avatar des Kindes.
        /// </summary>
        private async void CurrentChild_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Child.SelectedAvatarPath) || e.PropertyName == nameof(Child.SelectedThemeColor))
            {
                try
                {
                    Debug.WriteLine($"Die Einstelungen für {CurrentChild?.Name} werderden gespeichert");
                    await _dataService.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Fehler {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Füllt die Listen für auswählbare Avatare und Hintergrundfarben.
        /// </summary>
        private void PopulateSettingsChoices()
        {
            AvailableAvatarPath.Clear();
            AvailableThemeColorKeys.Clear();

            AvailableAvatarPath.Add("/Assets/default_avatar.png");

            if (!string.IsNullOrEmpty(CurrentChild?.ProfileImagePath))
            {
                AvailableAvatarPath.Add(CurrentChild.ProfileImagePath);
            }

            AvailableAvatarPath.Add("/Assets/Avatars/cat_avatar.png");
            AvailableAvatarPath.Add("/Assets/Avatars/dog_avatar.png");
            AvailableAvatarPath.Add("/Assets/Avatars/fox_avatar.png");
            AvailableAvatarPath.Add("/Assets/Avatars/chick_avatar.png");
            AvailableAvatarPath.Add("/Assets/Avatars/dino_avatar.png");
            AvailableAvatarPath.Add("/Assets/Avatars/robot_avatar.png");
            AvailableAvatarPath.Add("/Assets/Avatars/unicorn_avatar.png");
            AvailableAvatarPath.Add("/Assets/Avatars/bunny_avatar.png");
            AvailableAvatarPath.Add("/Assets/Avatars/astronaut_avatar.png");
            AvailableAvatarPath.Add("/Assets/Avatars/scientist_avatar.png");

            AvailableThemeColorKeys.Add("ThemeBrushDefault");
            AvailableThemeColorKeys.Add("ThemeBrushLightBlue");
            AvailableThemeColorKeys.Add("ThemeBrushPaleGreen");
            AvailableThemeColorKeys.Add("ThemeBrushLavender");
            AvailableThemeColorKeys.Add("ThemeBrushLightYellow");
        }

        /// <summary>
        /// Lädt alle Aufgaben für das Kind, die aktuell relevant sind. 
        /// Dabei wird geprüft, ob freie Aufgaben verfügbar sind und ob die Aufgabe zeitlich aktiv ist. 
        /// Die Aufgaben werden nach Priorität und Status sortiert.
        /// </summary>
        private void LoadVisibleTasks()
        {
            VisibleTasks.Clear();

            if (CurrentChild == null || CurrentChild.Id == -1)
            {
                return;
            }

            bool allowFreeTasks = AreAllImportantTasksHandled();
            Debug.WriteLine($"Kontrolle: Sind freie Aufgaben ferfügbar? [{allowFreeTasks}]");

            var tasksForChild = _dataService.AssignedTasks
                                            .Where(t => (t.AssignedChildId == CurrentChild.Id)
                                                   && (t.Status == Model.TaskStatus.Pending
                                                   || t.Status == Model.TaskStatus.CompletedByChild)
                                                   && IsTaskTimeActive(t)
                                                   && ((t.DueDate.HasValue && t.DueDate.Value.Date == DateTime.Today.Date)
                                                   || (!t.DueDate.HasValue && IsManualOneOffTemplate(t.SourceTemplateId)))
                                                   && (t.Priority == TaskPriority.Important
                                                   || t.Priority == TaskPriority.Desirable
                                                   || (t.Priority == TaskPriority.Free && allowFreeTasks)))
                                            .OrderBy(t => t.Priority)
                                            .ThenBy(t => t.Status);

            foreach (var task in tasksForChild)
            {
                VisibleTasks.Add(task);
            }
            Debug.WriteLine($"{VisibleTasks.Count} Aufgaben für {CurrentChild.Name} wurde geladen.");
        }

        /// <summary>
        /// Prüft, ob eine Aufgabe eine einmalige, manuell angelegte Vorlage ist.
        /// </summary>
        private bool IsManualOneOffTemplate(int? sourceTemplateId)
        {
            if (!sourceTemplateId.HasValue)
            {
                return false;
            }

            var template = _dataService.TaskDefinitions.FirstOrDefault(td => td.Id == sourceTemplateId.Value);

            return template != null && template.RecurrenceType == TaskRecurrenceType.None && !template.SpecificDueDate.HasValue;
        }

        /// <summary>
        /// Prüft, ob alle wichtigen Aufgaben erledigt wurden.
        /// </summary>
        private bool AreAllImportantTasksHandled()
        {
            if (CurrentChild == null)
            {
                return false;
            }

            var allAssigned = _dataService.AssignedTasks.Where(t => t.AssignedChildId == CurrentChild.Id);

            var importantTasks = allAssigned.Where(t => t.Priority == TaskPriority.Important).ToList();

            if (!importantTasks.Any())
            {
                return true;
            }

            bool allHandled = importantTasks.All(t => t.Status == Model.TaskStatus.Confirmed || !IsTaskTimeActive(t));

            return allHandled;
        }

        /// <summary>
        /// Prüft, ob eine Aufgabe aktuell bearbeitet werden kann.
        /// </summary>
        private bool IsTaskTimeActive(TaskItem task)
        {
            if (!task.StartTimeOfDay.HasValue && !task.EndTimeOfDay.HasValue)
            {
                return true;
            }

            var now = DateTime.Now.TimeOfDay;

            if (task.StartTimeOfDay.HasValue && now < task.StartTimeOfDay.Value)
            {
                return false;
            }

            if (task.EndTimeOfDay.HasValue && now >= task.EndTimeOfDay.Value)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Hier und weiter im Code [RelayCommand] als Teil der CommunityToolkit.Mvvm-Bibliothek 
        /// ist für die automatische Generierung einer Command-Eigenschaft zuständig, die in der Benutzeroberfläche
        /// gebunden werden kann.
        [RelayCommand]
        /// Lässt die Aufgabe als erledigt markieren, prüft, ob die Aufgabe noch offen ist, setzt den Status und speichert die Änderung.
        /// </summary>
        private async Task CompleteTaskAsync(TaskItem? taskToComplete)
        {
            if (taskToComplete == null || taskToComplete.Status != Model.TaskStatus.Pending)
            {
                return;
            }

            var originalTask = _dataService.AssignedTasks.FirstOrDefault(t => t.Id == taskToComplete.Id);
            if (originalTask == null || originalTask.Status != Model.TaskStatus.Pending)
            {
                Debug.WriteLine($"Aufgabe mit ID {taskToComplete.Id} wurde nicht gefunden");
                VisibleTasks.Remove(taskToComplete);
                return;
            }

            originalTask.Status = Model.TaskStatus.CompletedByChild;

            await _dataService.SaveChangesAsync();

            Debug.WriteLine($"Aufgabe: {originalTask.Title} - wurde von Kind: {CurrentChild.Name} - erledigt. Punktenstand: {CurrentChild.Points}");

            RewardStatusMessage = $"Gut gemacht, {CurrentChild.Name}! Bevor Du die Punkte bekommst, Jemand von der Eltern muss deine Arbeit bestätigen. Ein wenig Geduld =)";
        }

        /// <summary>
        /// Ist für Kauf einer Belohnung, sofern es genug Punkte gibt. 
        /// Die Methode zieht die Punkte ab, legt einen neuen Anspruch auf die Belohnung an und aktualisiert die Anzeige. 
        /// Ist das Guthaben zu niedrig, wird eine entsprechende Nachricht gesetzt.
        /// </summary>
        [RelayCommand]
        private async Task BuyRewardAsync(Reward? rewardToBuy)
        {
            if(rewardToBuy == null)
            {
                return;
            }
            if(CurrentChild == null)
            {
                return;
            }

            if(CurrentChild.Points >= rewardToBuy.Cost)
            {
                var newClaim = new ClaimedReward
                {
                    Id = _dataService.GenerateNewClaimedId(),
                    ChildId = CurrentChild.Id,
                    RewardId = rewardToBuy.Id,
                    ClaimedDate = DateTime.Now,
                    IsDelivered = false
                };
                _dataService.ClaimedRewards.Add(newClaim);
                CurrentChild.Points -= rewardToBuy.Cost;
                await _dataService.SaveChangesAsync();
                RewardStatusMessage = $"Du hast Dir \"{rewardToBuy.Name}\" bestellt! Du hast es wirklich verdient!";
                Debug.WriteLine($"Die Belohnung {rewardToBuy.Name} wurde von {CurrentChild.Name} gekauft. Das Kind hat {CurrentChild.Points} Punkten");
                LoadMyClaimedRewards();
            }
            else
            {
                RewardStatusMessage = $"Du hast, leider, nicht genug Punkte um \"{rewardToBuy.Name}\" zu bekommen.";
                Debug.WriteLine($"Das Kind {CurrentChild.Name} hat nicht genug Punkte um {rewardToBuy.Name} zu kaufen");
            }
        }

        /// <summary>
        /// Lädt alle vom Kind beanspruchten, aber noch nicht gekriegte Belohnungen und aktualisiert die entsprechende Liste.
        /// </summary>
        private void LoadMyClaimedRewards()
        {
            MyClaimedRewards.Clear();

            if (CurrentChild == null)
            {
                return;
            }

            var rewardsLookup = _dataService.Rewards.ToDictionary(r => r.Id);
            var claims = _dataService.ClaimedRewards.Where(cr => cr.ChildId == CurrentChild.Id && cr.IsDelivered == false)
                                                    .OrderByDescending(cr => cr.ClaimedDate);

            foreach (var claim in claims)
            {
                if (rewardsLookup.TryGetValue(claim.RewardId, out var reward))
                {
                    MyClaimedRewards.Add(reward);
                }
                else
                {
                    Debug.WriteLine($"{claim.RewardId} wurde nicht in Rewards gefunden, gibt es aber in ClaimedRewards");
                }
            }
            Debug.WriteLine($"{MyClaimedRewards.Count} Belohnengen für {CurrentChild.Name} wurden geladen");
        }
    }
}