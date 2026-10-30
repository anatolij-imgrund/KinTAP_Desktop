///
/// Hilfsklasse zur Verwaltung von Dialogfenstern für den Benutzer.
/// 

namespace KinTAP.Controller
{
    public class ConfirmEventArgs : EventArgs
    {
        public string Title { get; }
        public string Message { get; }

        public bool Confirmed { get; set; }

        public ConfirmEventArgs(string title, string message)
        {
            Title = title;
            Message = message;
            Confirmed = false;
        }
    }
}