using CommunityToolkit.Mvvm.ComponentModel;

namespace KinTAP.Model
{
    public partial class TaskSubscription : ObservableObject
    {
        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private int _childId;

        [ObservableProperty]
        private int _taskTemplateId;

        [ObservableProperty]
        private DateTime _subscriptionDate;

        public TaskSubscription()
        {
            SubscriptionDate = DateTime.Now;
        }
    }
}