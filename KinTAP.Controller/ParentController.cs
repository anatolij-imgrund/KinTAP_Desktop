/// <summary>
/// Hier befindet sich die Funktionalität des Elternkontos: Erstellen und Bearbeiten von Kinderkonten, Aufgaben und Auszeichnungen...
/// </summary>

using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KinTAP.Model;
using KinTAP.Services;

namespace KinTAP.Controller
{
    public partial class ParentController : BaseController
    {
        private readonly DataService _dataService;

        public ObservableCollection<Child> Children => _dataService.Children;
        public ObservableCollection<TaskItem> AssignedTasks => _dataService.AssignedTasks;
        public ObservableCollection<TaskItem> TaskDefinitions => _dataService.TaskDefinitions;
        public ObservableCollection<Reward> Rewards => _dataService.Rewards;
        public ObservableCollection<ClaimedRewardInfo> PendingClaims { get; }
        public ObservableCollection<TaskItem> FilteredTaskDefinition { get; }

        public event Func<object, ConfirmEventArgs, Task>? RequestConfirmationAsync;

        public event Func<object?, Task<string?>>? RequestPhotoSelectionAsync;

        public ObservableCollection<TaskItem> FilteredAssignedTasks { get; private set; } = new ObservableCollection<TaskItem>();

        /// <summary>
        /// Hier und weiter im Code [ObservableProperty] als Teil der CommunityToolkit.Mvvm-Bibliothek
        /// ist für die automatische Aktualisierung der Variablen zuständig, beispielsweise im grafischen Teil.
        /// [NotifyCanExecuteChangedFor] sorgt dafür, dass bei jeder Änderung zugehörige Commands neu bewertet werden.
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveChildCommand))]
        private string _newChildName = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveChildCommand))]
        [NotifyCanExecuteChangedFor(nameof(CancelEditChildCommand))]
        [NotifyCanExecuteChangedFor(nameof(EditChildCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteChildCommand))]
        [NotifyCanExecuteChangedFor(nameof(SelectChildPhotoCommand))]
        private bool _isEditingChild = false;

        private int? _editingChildId = null;        

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveTaskDefinitionCommand))]
        private string _newTaskTitle = string.Empty;

        [ObservableProperty]
        private string _newTaskDescription = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveTaskDefinitionCommand))]
        private string _newTaskPointsValue = "0";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteChildCommand))]
        [NotifyCanExecuteChangedFor(nameof(EditChildCommand))]
        private Child? _selectedChild;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteTaskDefinitionCommand))]
        [NotifyCanExecuteChangedFor(nameof(EditTaskDefinitionCommand))]
        [NotifyCanExecuteChangedFor(nameof(AssignTaskCommand))]
        private TaskItem? _selectedTaskDefinition;

        [ObservableProperty]
        private TaskPriority _selectedTaskPriority = TaskPriority.Desirable;

        public List<PriorityDisplay> PriorityOptions { get; } = PriorityDisplay.GetAllPriorities();

        [ObservableProperty]
        private object? _selectedPriorityFilterItem;
    	private TaskPriority? _selectedPriorityFilter = null;
        public List<object> PriorityFilterOptions { get; }

        [ObservableProperty]
        private TimeSpan? _newTaskStartTimeOfDay = null;

        [ObservableProperty]
        private TimeSpan? _newTaskEndTimeOfDay = null;

        public bool IsWeeklyRecurrenceSelected => NewTaskRecurrenceType == TaskRecurrenceType.Weekly;
        public bool IsSpecificDateSelected => NewTaskRecurrenceType == TaskRecurrenceType.None;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsWeeklyRecurrenceSelected))]
        [NotifyPropertyChangedFor(nameof(IsSpecificDateSelected))]
        private TaskRecurrenceType _newTaskRecurrenceType = TaskRecurrenceType.None;

        [ObservableProperty]
        private bool _newTaskRecurrenceMonday = false;
        [ObservableProperty]
        private bool _newTaskRecurrenceTuesday = false;
        [ObservableProperty]
        private bool _newTaskRecurrenceWednesday = false;
        [ObservableProperty]
        private bool _newTaskRecurrenceThursday = false;
        [ObservableProperty]
        private bool _newTaskRecurrenceFriday = false;
        [ObservableProperty]
        private bool _newTaskRecurrenceSaturday = false;
        [ObservableProperty]
        private bool _newTaskRecurrenceSunday = false;

        [ObservableProperty]
        private DateTimeOffset? _newTaskSpecificDueDate = null;

        public Array RecurrenceTypeOptions => Enum.GetValues(typeof(TaskRecurrenceType));

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteAssignedTaskCommand))]
        [NotifyCanExecuteChangedFor(nameof(ConfirmTaskCompletionCommand))]
        [NotifyCanExecuteChangedFor(nameof(RejectTaskCompletionCommand))]
        private TaskItem? _selectedAssignedTask;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveTaskDefinitionCommand))]
        [NotifyCanExecuteChangedFor(nameof(CancelEditTaskDefinitionCommand))]
        [NotifyCanExecuteChangedFor(nameof(EditTaskDefinitionCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteTaskDefinitionCommand))]
        private bool _isEditingTaskDefinition = false;

        private int? _editingTaskDefinitionId = null;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveRewardCommand))]
        private string _newRewardName = string.Empty;

        [ObservableProperty]
        private string _newRewardDescription = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveRewardCommand))]
        private string _newRewardCost = "0";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteRewardCommand))]
        [NotifyCanExecuteChangedFor(nameof(EditRewardCommand))]
        private Reward? _selectedReward;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveRewardCommand))]
        [NotifyCanExecuteChangedFor(nameof(CancelEditRewardCommand))]
        [NotifyCanExecuteChangedFor(nameof(EditRewardCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteRewardCommand))]        
        private bool _isEditingReward = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(MarkRewardDeliveredCommand))]
        private ClaimedRewardInfo? _selectedPendingClaim;

        private int? _editingRewardId = null;

        /// <summary>
        /// Wird automatisch aufgerufen, wenn sich das ausgewählte Kind ändert. 
        /// Sorgt dafür, dass die gefilterten Aufgaben für das neue Kind geladen werden.
        /// </summary>
        partial void OnSelectedChildChanged(Child? value)
        {
            UpdateFilteredTasks();
        }

        /// <summary>
        /// Wird aufgerufen, wenn sich der ausgewählte Prioritätsfilter ändert. 
        /// Setzt den internen Filterwert und aktualisiert die gefilterten Aufgabenbeschreibungen.
        /// </summary>
        partial void OnSelectedPriorityFilterItemChanged(object? value)
        {
            if (value is PriorityDisplay pd)
            {
                _selectedPriorityFilter = pd.Value;
            }
            else
            {
                _selectedPriorityFilter = null;
            }

            UpdateFilteredTaskDefinitions();
        }

        /// <summary>
        /// Aktualisiert die Liste der gefilterten Aufgaben für das aktuell ausgewählte Kind. 
        /// Filtert alle Aufgaben, die dem Kind zugewiesen sind und entweder noch offen oder vom Kind erledigt wurden. 
        /// Die Aufgaben werden nach Fälligkeit, Status, Priorität und Startzeit sortiert.
        /// </summary>
        private void UpdateFilteredTasks()
        {
            FilteredAssignedTasks.Clear();

            if (SelectedChild != null)
            {
                var today = DateTime.Today;

                var taskForParentView = _dataService.AssignedTasks.Where(t => t.AssignedChildId == SelectedChild.Id
                    && (t.Status == Model.TaskStatus.Pending || t.Status == Model.TaskStatus.CompletedByChild))
                    .OrderBy(t => t.DueDate.HasValue && t.DueDate.Value.Date < today ? 0 : 1)
                    .ThenBy(t => t.Status)
                    .ThenBy(t => !t.DueDate.HasValue)
                    .ThenBy(t => t.Priority)
                    .ThenBy(t => t.StartTimeOfDay ?? TimeSpan.MaxValue)
                    .ToList();

                foreach (var task in taskForParentView)
                {
                    FilteredAssignedTasks.Add(task);
                }
            }
            Debug.WriteLine($"Die Liste mit Aufgaben für Kind: {SelectedChild?.Name ?? "kein Kind"} wurde erstellt. Aufgabenmenge: {FilteredAssignedTasks.Count}");
        }

        /// <summary>
        /// Konstruktor für Controller
        /// </summary>
        public ParentController(DataService dataService)
        {
            _dataService = dataService;
            FilteredAssignedTasks = new ObservableCollection<TaskItem>();
            PendingClaims = new ObservableCollection<ClaimedRewardInfo>();

            FilteredTaskDefinition = new ObservableCollection<TaskItem>();
            PriorityFilterOptions = new List<object>();
            PriorityFilterOptions.Add("Alle");
            PriorityFilterOptions.AddRange(PriorityDisplay.GetAllPriorities().Cast<object>());
            _selectedPriorityFilterItem = PriorityFilterOptions[0];

            LoadPendingClaims();
            UpdateFilteredTaskDefinitions();
        }

        private bool CanSaveChild() => !string.IsNullOrWhiteSpace(NewChildName);

        /// <summary>
        /// Hier und weiter im Code [RelayCommand] als Teil der CommunityToolkit.Mvvm-Bibliothek 
        /// ist für die automatische Generierung einer Command-Eigenschaft zuständig, die in der Benutzeroberfläche
        /// gebunden werden kann. CanExecute sagt von welche Methode Ausführbarkeit des Commands abhängig ist.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanSaveChild))] 

        /// <summary>
        /// Speichert ein neues Kind oder aktualisiert ein bestehendes Kind. 
        /// Bei einer Bearbeitung wird das Kind gesucht und aktualisiert, 
        /// ansonsten wird ein neues Kind mit einer neuen ID angelegt und zur Liste hinzugefügt. 
        /// Nach dem Speichern werden die Eingabefelder zurückgesetzt.
        /// </summary>
        private async Task SaveChildAsync()
        {
            if (!CanSaveChild())
            {
                return;
            }

            if (IsEditingChild)
            {
                if (_editingChildId == null)
                {
                    return;
                }

                var childToUpdate = Children.FirstOrDefault(c => c.Id == _editingChildId.Value);
                if (childToUpdate != null)
                {
                    childToUpdate.Name = NewChildName;
                    Debug.WriteLine($"Das Kind: {childToUpdate.Name} wurde aktualisiert.");

                    IsEditingChild = false;
                    _editingChildId = null;
                    NewChildName = string.Empty;

                    await _dataService.SaveChangesAsync();

                }
                else
                {
                    Debug.WriteLine($"Das Kind mit der ID: {_editingChildId} wurde nicht gefunden.");
                    CancelEditChild();
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(NewChildName))
                {
                    return;
                }

                var newChild = new Child
                {
                    Id = _dataService.GenerateNewChildId(),
                    Name = NewChildName,
                    Points = 0
                };

                _dataService.Children.Add(newChild);
                await _dataService.SaveChangesAsync();
                Debug.WriteLine($"Das Kind: {newChild.Name} wurde hinzugefügt.");
                NewChildName = string.Empty;
            }
        }
        
        private bool CanEditChild() => SelectedChild != null && !IsEditingChild;

        /// <summary>
        /// Setzt den Controller in den Bearbeitungsmodus für das aktuell ausgewählte Kind 
        /// und füllt die Eingabefelder mit den aktuellen Werten des Kindes.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanEditChild))]
        private void EditChild()
        {
            if (SelectedChild == null)
            {
                return;
            }

            IsEditingChild = true;
            _editingChildId = SelectedChild.Id;
            NewChildName = SelectedChild.Name;
        }

        private bool CanCancelEditChild() => IsEditingChild;

        /// <summary>
        /// Bricht die Bearbeitung eines Kindes ab und setzt die Eingabefelder sowie den Bearbeitungsmodus zurück.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanCancelEditChild))]
        private void CancelEditChild()
        {
            IsEditingChild = false;
            _editingChildId = null;
            NewChildName = string.Empty;
        }

        private bool CanDeleteChild() => SelectedChild != null && !IsEditingChild;

        /// <summary>
        /// Löscht das aktuell ausgewählte Kind nach einer Bestätigungsabfrage. 
        /// Dabei werden auch alle diesem Kind zugewiesenen Aufgaben entfernt. 
        /// Nach dem Löschen wird die Auswahl zurückgesetzt.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanDeleteChild))] // CanExecute blockiert den Button, wenn die Bedingung nicht erfüllt ist
        private async Task DeleteChildAsync()
        {
            if(SelectedChild == null)
            {
                return;
            }
            
            var args = new ConfirmEventArgs(
                "Kind löschen",
                $"Möchten Sie das Kind: {SelectedChild.Name} wirklich löschen?"
            );

            if(RequestConfirmationAsync != null)
            {
                await RequestConfirmationAsync(this, args);
            }
            else
            {
                Debug.WriteLine("");
                args.Confirmed = false;
            }

            if(!args.Confirmed)
            {
                return;
            }

            var childToRemove = SelectedChild;
            
            var tasksToRemove = AssignedTasks.Where(t => t.AssignedChildId == childToRemove.Id).ToList();
            if(tasksToRemove.Any())
            {
                Debug.WriteLine($"{tasksToRemove.Count} Aufgaben für das Kind: {childToRemove.Name} werden gelöscht.");
                foreach(var tasks in tasksToRemove)
                {
                    _dataService.AssignedTasks.Remove(tasks);
                }
                if(SelectedChild?.Id == childToRemove.Id)
                {
                    UpdateFilteredTasks();
                }
            }
            
            if(childToRemove != null)
            {
                _dataService.Children.Remove(childToRemove);
                var allTasksToSave = TaskDefinitions.Concat(AssignedTasks).ToList();
                await _dataService.SaveChangesAsync();
                SelectedChild = null;
                Debug.WriteLine($"Das Kind: {childToRemove.Name} wurde gelöscht.");
            }
            else
            {
                Debug.WriteLine("Das Kind wurde nicht gefunden.");
            }
        }

        private bool CanSelectChildPhoto() => IsEditingChild;

        /// <summary>
        /// Öffnet einen Dateiauswahldialog, kopiert das ausgewählte Foto in das Anwendungsverzeichnis und speichert den Pfad beim Kind.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanSelectChildPhoto))]
        private async Task SelectChildPhotoAsync()
        {
            if(!IsEditingChild || _editingChildId == null || RequestPhotoSelectionAsync == null)
            {
                return;
            }

            string? selectedFilePath = null;
            try
            {
                selectedFilePath = await RequestPhotoSelectionAsync(this);
                if(string.IsNullOrWhiteSpace(selectedFilePath))
                {
                    Debug.WriteLine("Fotoauswahl wurde vom User unterbrochen");
                    return;
                }

                string imageDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                "KinTAP",
                                "Images",
                                "Children");
                Directory.CreateDirectory(imageDir);

                string extension = Path.GetExtension(selectedFilePath);
                if(extension == null || !new[] {".png", ".jpg", ".jpeg", ".bmp", ".gif"}.Contains(extension.ToLowerInvariant()))
                {
                    Debug.WriteLine($"Fehler! {extension} ist ein ungültiges Format");
                    return;
                }

                string uniqueFilename = $"child_{_editingChildId}{extension}";
                string destinationPath = Path.Combine(imageDir, uniqueFilename);

                File.Copy(selectedFilePath, destinationPath, true);
                Debug.WriteLine($"Foto wurde in [{destinationPath}] copiert");

                var childToUpdate = _dataService.Children.FirstOrDefault(c => c.Id == _editingChildId.Value);
                if(childToUpdate != null)
                {
                    childToUpdate.ProfileImagePath = uniqueFilename;
                    await _dataService.SaveChangesAsync();
                    Debug.WriteLine($"Path für {uniqueFilename} des Kindes mit ID {_editingChildId} wurde gespeichert");
                    OnPropertyChanged(nameof(SelectedChild));
                }
                else
                {
                    Debug.WriteLine($"Fehler! das Kind mit ID {_editingChildId} wurde nicht gefunden. Kann nicht ausgewähltes Foto speichern");
                }
            }
            catch(IOException e)
            {
                Debug.WriteLine($"Fehler beim Filecopieren: {e.Message}");
            }
            catch(Exception e)
            {
                Debug.WriteLine($"Unerwartete Fehler: {e.Message}");
            }
        }

        private bool CanSaveTaskDefinition() => !string.IsNullOrWhiteSpace(NewTaskTitle) && int.TryParse(NewTaskPointsValue, out _);
        
        /// <summary>
        /// Speichert eine neue oder bearbeitete Aufgabenbeschreibung (Task-Template). 
        /// Bei Bearbeitung wird die bestehende Aufgabe aktualisiert, sonst wird eine neue Vorlage angelegt. 
        /// Nach dem Speichern werden die Eingabefelder zurückgesetzt und die Liste aktualisiert.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanSaveTaskDefinition))]
        private async Task SaveTaskDefinitionAsync()
        {
            if(!CanSaveTaskDefinition())
            {
                return;
            }

            if(!int.TryParse(NewTaskPointsValue, out int points) || points < 0)
            {
                Debug.WriteLine("Der Punktewert muss eine positive Zahl sein.");
                points = 0;
                NewTaskPointsValue = "";
                return;
            }

            if(string.IsNullOrWhiteSpace(NewTaskTitle))
            {
                Debug.WriteLine("Der Aufgabenname kann nicht leer sein.");
                return;
            }

            DaysOfWeekFlags daysOfWeek = DaysOfWeekFlags.None;
            if(NewTaskRecurrenceMonday)
            {daysOfWeek |= DaysOfWeekFlags.Monday;}
            if(NewTaskRecurrenceTuesday)
            {daysOfWeek |= DaysOfWeekFlags.Tuesday;}
            if(NewTaskRecurrenceWednesday)
            {daysOfWeek |= DaysOfWeekFlags.Wednesday;}
            if(NewTaskRecurrenceThursday)
            {daysOfWeek |= DaysOfWeekFlags.Thursday;}
            if(NewTaskRecurrenceFriday)
            {daysOfWeek |= DaysOfWeekFlags.Friday;}
            if(NewTaskRecurrenceSaturday)
            {daysOfWeek |= DaysOfWeekFlags.Saturday;}
            if(NewTaskRecurrenceSunday)
            {daysOfWeek |= DaysOfWeekFlags.Sunday;}

            DateTime? specificDate = NewTaskSpecificDueDate.HasValue ? NewTaskSpecificDueDate.Value.Date : null;

            if(IsEditingTaskDefinition)
            {
                if(_editingTaskDefinitionId == null)
                {
                    return;
                }

                var definitionToUpdate = TaskDefinitions.FirstOrDefault(td => td.Id == _editingTaskDefinitionId.Value);
                if(definitionToUpdate != null)
                {
                    definitionToUpdate.Title = NewTaskTitle;
                    definitionToUpdate.Description = NewTaskDescription;
                    definitionToUpdate.PointsValue = points;
                    definitionToUpdate.Priority = SelectedTaskPriority;
                    definitionToUpdate.StartTimeOfDay = NewTaskStartTimeOfDay;
                    definitionToUpdate.EndTimeOfDay = NewTaskEndTimeOfDay;
                    definitionToUpdate.RecurrenceType = NewTaskRecurrenceType;
                    definitionToUpdate.RecurringDaysOfWeek = daysOfWeek;
                    definitionToUpdate.SpecificDueDate = specificDate;

                    Debug.WriteLine($"Die Aufgabenbeschreibung: {definitionToUpdate.Title} wurde aktualisiert.");

                    IsEditingTaskDefinition = false;
                    _editingTaskDefinitionId = null;

                    CancelEditTaskDefinition();

                    var allTasksToSave = TaskDefinitions.Concat(AssignedTasks).ToList();
                    await _dataService.SaveChangesAsync();
                }
                else
                {
                    Debug.WriteLine($"Die Aufgabenbeschreibung mit der ID: {_editingTaskDefinitionId} wurde nicht gefunden.");
                    CancelEditTaskDefinition();
                }
            }
            else
            {
                var newTaskDefinition = new TaskItem
                {
                    Id = _dataService.GenerateNewTaskId(),
                    Title = NewTaskTitle,
                    Description = NewTaskDescription,
                    PointsValue = points,
                    Priority = SelectedTaskPriority,
                    AssignedChildId = null,
                    Status = Model.TaskStatus.Pending,
                    StartTimeOfDay = NewTaskStartTimeOfDay,
                    EndTimeOfDay = NewTaskEndTimeOfDay,
                    RecurrenceType = NewTaskRecurrenceType,
                    RecurringDaysOfWeek = daysOfWeek,
                    SpecificDueDate = specificDate
                };

                _dataService.TaskDefinitions.Add(newTaskDefinition);
                await _dataService.SaveChangesAsync();
                Debug.WriteLine($"Die Aufgabenbeschreibung: {newTaskDefinition.Title} wurde hinzugefügt.");
                CancelEditTaskDefinition();
                UpdateFilteredTaskDefinitions();
            }
        }

        /// <summary>
        /// Weist eine Aufgabenbeschreibung (Task-Template) dem aktuell ausgewählten Kind zu. 
        /// Es wird eine Instanz der Aufgabe erstellt und ggf. eine neue Abonnierung angelegt. 
        /// Die Aufgabe wird der gefilterten Liste hinzugefügt und gespeichert.
        /// </summary>
        [RelayCommand]
        private async Task AssignTaskAsync()
        {
            if(SelectedChild == null)
            {
                Debug.WriteLine("Kein Kind ausgewählt.");
                //todo: Nachricht für User anzeigen ? denke nicht nötig
                return;
            }
            if(SelectedTaskDefinition == null)
            {
                Debug.WriteLine("Keine Aufgabe ausgewählt.");
                //todo: Nachricht für User anzeigen ? auch nicht
                return;
            }

            int currentChildId = SelectedChild.Id;
            int currentTemplateId = SelectedTaskDefinition.Id;

            DateTime? dueDateForInstance;
            if (SelectedTaskDefinition.RecurrenceType == TaskRecurrenceType.None && !SelectedTaskDefinition.SpecificDueDate.HasValue)
            {
                dueDateForInstance = null;
                Debug.WriteLine($"Die Aufgabe: {SelectedTaskDefinition.Title} hat kein Fälligkeitsdatum.");
            }
            else
            {
                dueDateForInstance = DateTime.Today.Date;
                Debug.WriteLine($"Die Aufgabe: {SelectedTaskDefinition.Title} hat ein Fälligkeitsdatum: {dueDateForInstance.Value.Date}.");
            }

            var assignedTaskInstance = new TaskItem
            {
                Id = _dataService.GenerateNewTaskId(),
                Title = SelectedTaskDefinition.Title,
                Description = SelectedTaskDefinition.Description,
                PointsValue = SelectedTaskDefinition.PointsValue,
                Priority = SelectedTaskDefinition.Priority,
                StartTimeOfDay = SelectedTaskDefinition.StartTimeOfDay,
                EndTimeOfDay = SelectedTaskDefinition.EndTimeOfDay,
                AssignedChildId = currentChildId,
                Status = Model.TaskStatus.Pending,
                DueDate = null,
                SourceTemplateId = currentTemplateId,
                RecurrenceType = SelectedTaskDefinition.RecurrenceType,
                RecurringDaysOfWeek = SelectedTaskDefinition.RecurringDaysOfWeek,
                SpecificDueDate = SelectedTaskDefinition.SpecificDueDate
            };

            _dataService.AssignedTasks.Add(assignedTaskInstance);

            Debug.WriteLine($"Die Aufgabe: {assignedTaskInstance.Title} wurde dem Kind: {SelectedChild.Name} zugewiesen.");

            bool subscriptionExists = _dataService.Subscriptions.Any(s => s.ChildId == currentChildId && s.TaskTemplateId == currentTemplateId);

            if(!subscriptionExists)
            {
                var newSubscription = new TaskSubscription
                {
                    Id = _dataService.GenerateNewSubscriptionId(),
                    ChildId = currentChildId,
                    TaskTemplateId = currentTemplateId,
                    SubscriptionDate = DateTime.Now
                };
                _dataService.Subscriptions.Add(newSubscription);

                Debug.WriteLine($"Die Aufgabe: {assignedTaskInstance.Title} wurde dem Kind: {SelectedChild.Name} zugewiesen.");
            }
            else
            {
                Debug.WriteLine($"Die Aufgabe: {assignedTaskInstance.Title} wurde bereits dem Kind: {SelectedChild.Name} zugewiesen.");
            }
            
            if(assignedTaskInstance.AssignedChildId == SelectedChild?.Id)
            {
                FilteredAssignedTasks.Add(assignedTaskInstance);
            }
            
            var allTaskToSave = TaskDefinitions.Concat(AssignedTasks).ToList();
            
            await _dataService.SaveChangesAsync();

            SelectedTaskDefinition = null;
        }

        private bool CanDeleteTaskDefinition() => SelectedTaskDefinition != null && !IsEditingTaskDefinition;

        /// <summary>
        /// löscht die ausgewählte Aufgabenbeschreibung nach einer Bestätigungsabfrage.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanDeleteTaskDefinition))]
        private async Task DeleteTaskDefinitionAsync()
        {
            if(SelectedTaskDefinition == null || SelectedTaskDefinition.AssignedChildId != null)
            {
                Debug.WriteLine("Die Aufgabe kann nicht gelöscht werden.");
                return;
            }

            var args = new ConfirmEventArgs(
                "Aufgabe löschen",
                $"Möchten Sie die Aufgabe: {SelectedTaskDefinition.Title} wirklich löschen?"
            );
            
            if(RequestConfirmationAsync != null)
            {
                await RequestConfirmationAsync(this, args);
            }
            else
            {
                Debug.WriteLine("");
                args.Confirmed = false;
            }

            if(!args.Confirmed)
            {
                return;
            }

            var definitionToRemove = SelectedTaskDefinition;
            if(definitionToRemove != null)
            {
                _dataService.TaskDefinitions.Remove(definitionToRemove);
                var allTasksToSave = TaskDefinitions.Concat(AssignedTasks).ToList();
                await _dataService.SaveChangesAsync();
                SelectedTaskDefinition = null;
                Debug.WriteLine($"Die Aufgabe: {definitionToRemove.Title} wurde gelöscht.");
            }
            else
            {
                Debug.WriteLine("Die Aufgabe wurde nicht gefunden.");
            }
        }

        private bool CanDeleteAssignedTask() => SelectedAssignedTask != null;

        /// <summary>
        /// Löscht eine zugewiesene Aufgabe nach einer Bestätigungsabfrage und entfernt ggf. auch die zugehörige Abonnierung.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanDeleteAssignedTask))]
        private async Task DeleteAssignedTaskAsync()
        {
            if(SelectedAssignedTask is null)
            {
                return;
            }
            
            var args = new ConfirmEventArgs(
                "Aufgabe löschen",
                $"Möchten Sie die Aufgabe: {SelectedAssignedTask.Title} wirklich löschen?"
            );
            if(RequestConfirmationAsync != null)
            {
                await RequestConfirmationAsync(this, args);
            }
            else
            {
                Debug.WriteLine($"");
                args.Confirmed = true;
            }

            if(!args.Confirmed)
            {
                return;
            }

            var taskToRemove = SelectedAssignedTask;
            if(taskToRemove != null && taskToRemove.AssignedChildId.HasValue && taskToRemove.SourceTemplateId.HasValue)
            {
                int childId = taskToRemove.AssignedChildId.Value;
                int templateId = taskToRemove.SourceTemplateId.Value;

                bool removed = _dataService.AssignedTasks.Remove(taskToRemove);
                if(!removed)
                {
                    Debug.WriteLine($"Die Aufgabe: {taskToRemove.Title} konnte nicht gelöscht werden.");
                }
                else
                {
                    Debug.WriteLine($"Die Aufgabe: {taskToRemove.Title} wurde gelöscht.");
                }

                var subscriptionToRemove = _dataService.Subscriptions.FirstOrDefault(s => s.ChildId == childId && s.TaskTemplateId == templateId);
                if(subscriptionToRemove != null)
                {
                    bool subRemoved = _dataService.Subscriptions.Remove(subscriptionToRemove);
                    if(subRemoved)
                    {
                        Debug.WriteLine($"Die Abonnierung vom Kind {childId} für die Aufgabe: {taskToRemove.Title} wurde gelöscht.");
                    }
                    else
                    {
                        Debug.WriteLine($"Die Abonnierung vom Kind {childId} für die Aufgabe: {taskToRemove.Title} konnte nicht gelöscht werden.");
                    }
                }
                else
                {
                    Debug.WriteLine($"Die Abonnierung vom Kind {childId} für die Aufgabe: {taskToRemove.Title} wurde nicht gefunden.");
                }
                
                await _dataService.SaveChangesAsync();
                UpdateFilteredTasks();
            }
            else
            {
                Debug.WriteLine($"Die Aufgabe: {SelectedAssignedTask.Title} konnte nicht gelöscht werden.");
                bool removed = _dataService.AssignedTasks.Remove(taskToRemove!);
                if(removed)
                {
                    await _dataService.SaveChangesAsync();
                    UpdateFilteredTasks();
                    Debug.WriteLine($"Die Aufgabe: {taskToRemove!.Title} wurde gelöscht.");
                }
            }

            SelectedAssignedTask = null;
        }

        private bool CanEditTaskDefinition() => SelectedTaskDefinition != null && !IsEditingTaskDefinition;

        /// <summary>
        /// Setzt den Controller in den Bearbeitungsmodus für die ausgewählte Aufgabenbeschreibung und füllt die Eingabefelder mit den aktuellen Werten.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanEditTaskDefinition))]
        private void EditTaskDefinition()
        {
            if(SelectedTaskDefinition == null)
            {
                return;
            }

            IsEditingTaskDefinition = true;
            _editingTaskDefinitionId = SelectedTaskDefinition.Id;
            NewTaskTitle = SelectedTaskDefinition.Title;
            NewTaskDescription = SelectedTaskDefinition.Description ?? string.Empty;
            NewTaskPointsValue = SelectedTaskDefinition.PointsValue.ToString();
            SelectedTaskPriority = SelectedTaskDefinition.Priority;

            NewTaskStartTimeOfDay = SelectedTaskDefinition.StartTimeOfDay;
            NewTaskEndTimeOfDay = SelectedTaskDefinition.EndTimeOfDay;

            NewTaskRecurrenceType = SelectedTaskDefinition.RecurrenceType;
            NewTaskSpecificDueDate = SelectedTaskDefinition.SpecificDueDate.HasValue ? new DateTimeOffset(SelectedTaskDefinition.SpecificDueDate.Value.Date).DateTime.Date : null;

            var days = SelectedTaskDefinition.RecurringDaysOfWeek;
            NewTaskRecurrenceMonday = days.HasFlag(DaysOfWeekFlags.Monday);
            NewTaskRecurrenceTuesday = days.HasFlag(DaysOfWeekFlags.Tuesday);
            NewTaskRecurrenceWednesday = days.HasFlag(DaysOfWeekFlags.Wednesday);
            NewTaskRecurrenceThursday = days.HasFlag(DaysOfWeekFlags.Thursday);
            NewTaskRecurrenceFriday = days.HasFlag(DaysOfWeekFlags.Friday);
            NewTaskRecurrenceSaturday = days.HasFlag(DaysOfWeekFlags.Saturday);
            NewTaskRecurrenceSunday = days.HasFlag(DaysOfWeekFlags.Sunday);
        }

        /// <summary>
        /// Aktualisiert die Liste der gefilterten Aufgabenbeschreibungen nach Priorität.
        /// </summary>
        private void UpdateFilteredTaskDefinitions()
        {
            FilteredTaskDefinition.Clear();
            IEnumerable<TaskItem> sourceTasks = _dataService.TaskDefinitions;

            if (_selectedPriorityFilter.HasValue)
            {
                sourceTasks = sourceTasks.Where(t => t.Priority == _selectedPriorityFilter.Value);
            }

            var sortedTasks = sourceTasks.OrderBy(t => t.Title);

            foreach (var task in sortedTasks)
            {
                FilteredTaskDefinition.Add(task);
            }

            Debug.WriteLine($"{FilteredTaskDefinition.Count} Aufgaben wurden sortiert mit Eigenschaft: Priorität: {_selectedPriorityFilter?.ToString() ?? "Alle"}");
        }

        private bool CanCancelEditTaskDefinition() => IsEditingTaskDefinition;

        /// <summary>
        /// Bricht die Bearbeitung einer Aufgabenbeschreibung ab und setzt die Eingabefelder sowie den Bearbeitungsmodus zurück.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanCancelEditTaskDefinition))]
        private void CancelEditTaskDefinition()
        {
            IsEditingTaskDefinition = false;
            _editingTaskDefinitionId = null;
            NewTaskTitle = string.Empty;
            NewTaskDescription = string.Empty;
            NewTaskPointsValue = "0";
            SelectedTaskPriority = TaskPriority.Desirable;

            NewTaskStartTimeOfDay = null;
            NewTaskEndTimeOfDay = null;

            NewTaskRecurrenceType = TaskRecurrenceType.None;
            NewTaskSpecificDueDate = null;
            NewTaskRecurrenceMonday = false;
            NewTaskRecurrenceTuesday = false;
            NewTaskRecurrenceWednesday = false;
            NewTaskRecurrenceThursday = false;
            NewTaskRecurrenceFriday = false;
            NewTaskRecurrenceSaturday = false;
            NewTaskRecurrenceSunday = false;
        }

        private bool CanConfirmOrRejectTask() => SelectedAssignedTask != null && SelectedAssignedTask.Status == Model.TaskStatus.CompletedByChild;

        /// <summary>
        /// Bestätigt die Erledigung einer Aufgabe, vergibt die Punkte an das Kind und speichert die Änderung.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanConfirmOrRejectTask))]
        private async Task ConfirmTaskCompletionAsync()
        {
            if(SelectedAssignedTask is null || SelectedAssignedTask.AssignedChildId is null)
            {
                return;
            }
            
            if(!CanConfirmOrRejectTask())
            {
                return;
            }

            var child = _dataService.Children.FirstOrDefault(c => c.Id == SelectedAssignedTask.AssignedChildId.Value);

            if(child != null)
            {
                child.Points += SelectedAssignedTask!.PointsValue;
                Debug.WriteLine($"{SelectedAssignedTask.PointsValue} Punkte wurden dem Kind {child.Name} addiert. Summe: {child.Points}");
            }
            else
            {
                Debug.WriteLine($"Fehler. Kind wurde nicht gefunden.");
            }

            SelectedAssignedTask!.Status = Model.TaskStatus.Confirmed;
            Debug.WriteLine($"Aufgabe [{SelectedAssignedTask.Title}] wurde bestätigt");

            await _dataService.SaveChangesAsync();

            UpdateFilteredTasks();
        }

        /// <summary>
        /// Lehnt die Bestätigung einer Aufgabe ab und setzt den Status zurück auf "offen".
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanConfirmOrRejectTask))]
        private async Task RejectTaskCompletionAsync()
        {
            if(!CanConfirmOrRejectTask())
            {
                return;
            }

            SelectedAssignedTask!.Status = Model.TaskStatus.Pending;
            Debug.WriteLine($"Bestätigung für [{SelectedAssignedTask.Title}] wurde abgelehnt");

            await _dataService.SaveChangesAsync();
        }

        private bool CanSaveReward() => !string.IsNullOrWhiteSpace(NewRewardName) && int.TryParse(NewRewardCost, out _);

        /// <summary>
        /// Speichert eine neue oder bearbeitete Belohnung. 
        /// Bei Bearbeitung wird die bestehende Belohnung aktualisiert, sonst wird eine neue angelegt. 
        /// Nach dem Speichern werden die Eingabefelder zurückgesetzt.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanSaveReward))]        
        private async Task SaveRewardAsync()
        {
            if(!CanSaveReward())
            {
                return;
            }

            if(!int.TryParse(NewRewardCost, out int cost) || cost < 0)
            {
                Debug.WriteLine("Die Kosten müssen eine positive Zahl sein.");
                cost = 0;
                NewRewardCost = "";
                //todo: Nachricht für User zu basteln(?)
                return;
            }
            
            if(string.IsNullOrWhiteSpace(NewRewardName))
            {
                Debug.WriteLine("Der Belohnungstitel kann nicht leer sein.");
                //todo: Nachricht für User zu basteln(?)
                return;
            }

            if(IsEditingReward)
            {
                if(_editingRewardId == null)
                {
                    return;
                }

                var rewardToUpdate = Rewards.FirstOrDefault(r => r.Id == _editingRewardId.Value);
                if(rewardToUpdate != null)
                {
                    rewardToUpdate.Name = NewRewardName;
                    rewardToUpdate.Description = NewRewardDescription;
                    rewardToUpdate.Cost = cost;

                    Debug.WriteLine($"Die Belohnung: {rewardToUpdate.Name} wurde aktualisiert.");

                    CancelEditReward();

                    await _dataService.SaveChangesAsync();
                }
                else
                {
                    Debug.WriteLine($"Die Belohnung mit der ID: {_editingRewardId} wurde nicht gefunden.");
                    CancelEditReward();
                }
            }
            else
            {
                var newReward = new Reward
                {
                    Id = _dataService.GenerateNewRewardId(),
                    Name = NewRewardName,
                    Description = NewRewardDescription,
                    Cost = cost
                };

                _dataService.Rewards.Add(newReward);
                //SaveChanges();
                await _dataService.SaveChangesAsync();

                Debug.WriteLine($"Die Belohnung: {newReward.Name} wurde hinzugefügt.");
                CancelEditReward();
            }
        }

        private bool CanDeleteReward() => SelectedReward != null && !IsEditingReward;

        /// <summary>
        /// Löscht die ausgewählte Belohnung nach einer Bestätigungsabfrage.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanDeleteReward))]
        private async Task DeleteRewardAsync()
        {
            if(SelectedReward == null)
            {
                return;
            }

            var args = new ConfirmEventArgs(
                "Belohnung löschen",
                $"Möchten Sie die Belohnung: {SelectedReward.Name} wirklich löschen?"
            );
            
            if(RequestConfirmationAsync != null)
            {
                await RequestConfirmationAsync(this, args);
            }
            else
            {
                Debug.WriteLine("");
                args.Confirmed = false;
            }

            if(!args.Confirmed)
            {
                return;
            }

            var rewardToRemove = SelectedReward;
            if(rewardToRemove != null)
            {
                _dataService.Rewards.Remove(rewardToRemove);
                await _dataService.SaveChangesAsync();
                //SaveChanges();
                SelectedReward = null;
                Debug.WriteLine($"Die Belohnung: {rewardToRemove.Name} wurde gelöscht.");
            }
            else
            {
                Debug.WriteLine("Die Belohnung wurde nicht gefunden.");
            }
        }
    
        private bool CanEditReward() => SelectedReward != null && !IsEditingReward;

        /// <summary>
        /// Setzt den Controller in den Bearbeitungsmodus für die ausgewählte Belohnung und füllt die Eingabefelder mit den aktuellen Werten.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanEditReward))]
        private void EditReward()
        {
            if(SelectedReward == null)
            {
                return;
            }

            IsEditingReward = true;
            _editingRewardId = SelectedReward.Id;

            NewRewardName = SelectedReward.Name;
            NewRewardDescription = SelectedReward.Description;
            NewRewardCost = SelectedReward.Cost.ToString();
        }
    
        private bool CanCancelEditReward() => IsEditingReward;

        /// <summary>
        /// Bricht die Bearbeitung einer Belohnung ab und setzt die Eingabefelder sowie den Bearbeitungsmodus zurück.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanCancelEditReward))]
        private void CancelEditReward()
        {
            IsEditingReward = false;
            _editingRewardId = null;

            NewRewardName = string.Empty;
            NewRewardDescription = string.Empty;
            NewRewardCost = "0";
        }

        /// <summary>
        /// Lädt alle noch nicht ausgelieferten Belohnungsansprüche und erstellt eine Liste mit den relevanten Informationen für die Anzeige.
        /// </summary>
        private void LoadPendingClaims()
        {
            PendingClaims.Clear();

            var childLookup = _dataService.Children.ToDictionary(c => c.Id);
            var rewardLookup = _dataService.Rewards.ToDictionary(r => r.Id);
            var claimsToProcess = _dataService.ClaimedRewards.Where(cr => !cr.IsDelivered)
                                                             .OrderBy(cr => cr.ClaimedDate);
            foreach (var claim in claimsToProcess)
            {
                string childName = childLookup.TryGetValue(claim.ChildId, out var child) ? child.Name : "Unbekantes Kind";
                string rewardName = rewardLookup.TryGetValue(claim.RewardId, out var reward) ? reward.Name : "Unbekante Belohnung";
                int rewardCost = rewardLookup.TryGetValue(claim.RewardId, out var rwd) ? rwd.Cost : 0;

                var claimInfo = new ClaimedRewardInfo
                {
                    ClaimId = claim.Id,
                    ChildName = childName,
                    RewardName = rewardName,
                    Cost = rewardCost,
                    ClaimedDate = claim.ClaimedDate,
                    OriginalChildId = claim.ChildId,
                    OriginalRewardId = claim.RewardId
                };

                PendingClaims.Add(claimInfo);
            }
            Debug.WriteLine($"{PendingClaims.Count} Belohnungen wurden geladen");
        }
    
        private bool CanMarkRewardDelivered() => SelectedPendingClaim != null;

        /// <summary>
        /// Markiert eine ausgewählte Belohnung als ausgegeben, speichert die Änderung und entfernt den Anspruch aus der Liste.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanMarkRewardDelivered))]
        private async Task MarkRewardDeliveredAsync()
        {
            if(!CanMarkRewardDelivered() || SelectedPendingClaim == null)
            {
                return;
            }

            var originalClaim = _dataService.ClaimedRewards.FirstOrDefault(cr => cr.Id == SelectedPendingClaim.ClaimId);

            if(originalClaim != null)
            {
                originalClaim.IsDelivered = true;
                await _dataService.SaveChangesAsync();
                Debug.WriteLine($"Belohnung [{SelectedPendingClaim.RewardName}] wurde für Kind [{SelectedPendingClaim.ChildName}] als Gegeben markiert");
                PendingClaims.Remove(SelectedPendingClaim);
                SelectedPendingClaim = null;
            }
            else
            {
                Debug.WriteLine($"Fehler! Belohning mit ID {SelectedPendingClaim.ClaimId} wurde nicht gefunden");
            }
        }
    }
}