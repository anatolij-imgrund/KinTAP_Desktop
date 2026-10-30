namespace KinTAP.Model
{
    public readonly struct PriorityDisplay
    {
        public TaskPriority Value { get; }
        public string DisplayName { get; }

        public PriorityDisplay(TaskPriority value, string displayName)
        {
            Value = value;
            DisplayName = displayName;
        }

        public override string ToString() => this.DisplayName;

        public static List<PriorityDisplay> GetAllPriorities()
        {
            return new List<PriorityDisplay>
            {
                new PriorityDisplay(TaskPriority.Important, "Wichtig"),
                new PriorityDisplay(TaskPriority.Desirable, "Wünschenswert"),
                new PriorityDisplay(TaskPriority.Free, "Frei")
            };
        }
    }
}