using CommunityToolkit.Mvvm.ComponentModel;

namespace KinTAP.Model
{
    public partial class ClaimedRewardInfo : ObservableObject
    {
        public int ClaimId { get; init; }
        public string ChildName { get; init; } = "?";
        public string RewardName { get; init; } = "?";
        public int Cost { get; init; }
        public DateTime ClaimedDate { get; init; }
        public int OriginalRewardId { get; init; }
        public int OriginalChildId { get; init; }

        public ClaimedRewardInfo()
        {

        }
    }
}