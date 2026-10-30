using CommunityToolkit.Mvvm.ComponentModel;

namespace KinTAP.Model
{
    public partial class ClaimedReward : ObservableObject
    {
        private int _id;
        public int Id { get => _id; set => SetProperty(ref _id, value); }

        private int _childId;
        public int ChildId { get => _childId; set => SetProperty(ref _childId, value); }

        private int _rewardId;
        public int RewardId { get => _rewardId; set => SetProperty(ref _rewardId, value); }

        private DateTime _claimedDate;
        public DateTime ClaimedDate { get => _claimedDate; set => SetProperty(ref _claimedDate, value); }

        [ObservableProperty]
        private bool _isDelivered = false;

        public ClaimedReward()
        {
            
        }
    }
}