using CommunityToolkit.Mvvm.ComponentModel;

namespace KinTAP.Model;

    public enum TaskStatus
    {
        Pending, //muss erledigt werden
        CompletedByChild, //wurde vom Kind erledigt, muss bestätigt werden
        Confirmed //von Eltern Bestätigt
    }
    public enum TaskPriority
    {
        Important, // wichtige Aufgabe
        Desirable, // erwünscht
        Free // frei
    }

    public enum TaskRecurrenceType
    {
        None, // einmalig
        Daily, // täglich (jeden Tag)
        Weekly // wochentlich (nur bestimmte Tage der Woche)
    }

    [Flags]
    public enum DaysOfWeekFlags
    {
        None        = 0,        // 0000000 = 0
        Monday      = 1 << 0,   // 0000001 = 1
        Tuesday     = 1 << 1,   // 0000010 = 2
        Wednesday   = 1 << 2,   // 0000100 = 4
        Thursday    = 1 << 3,   // 0001000 = 8
        Friday      = 1 << 4,   // 0010000 = 16
        Saturday    = 1 << 5,   // 0100000 = 32
        Sunday      = 1 << 6,   // 1000000 = 64
        Weekdays    = Monday | Tuesday | Wednesday | Thursday | Friday, // 1 + 2 + 4 + 8 + 16 = 31
        Weekends    = Saturday | Sunday,    // 32 + 64 = 96
        All         = Weekdays | Weekends   // 31 + 96 = 127
    }
    // z.B. Monday + Wednesday + Friday     1 + 4 + 16 = 21
    //      Friday + Saturday               16 + 32 = 48
    // usw.

public partial class TaskItem : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string? _description = string.Empty;

    [ObservableProperty]
    private int _pointsValue;

    [ObservableProperty]
    private TaskStatus _status = TaskStatus.Pending;

    [ObservableProperty]
    private TaskPriority _priority = TaskPriority.Desirable;

    [ObservableProperty]
    private int? _assignedChildId;

    [ObservableProperty]
    private TimeSpan? _startTimeOfDay;

    [ObservableProperty]
    private TimeSpan? _endTimeOfDay;

    [ObservableProperty]
    private TaskRecurrenceType _recurrenceType = TaskRecurrenceType.None; // = TaskRecurrenceType.None;

    [ObservableProperty]
    private DaysOfWeekFlags _recurringDaysOfWeek = DaysOfWeekFlags.None; // = DaysOfWeekFlags.None;

    [ObservableProperty]
    private DateTime? _dueDate;

    [ObservableProperty]
    private DateTime? _specificDueDate;

    [ObservableProperty]
    private int? _sourceTemplateId;

    public TaskItem()
    {
        
    }        
}