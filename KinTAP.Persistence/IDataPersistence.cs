///
/// Ein Interface bzw. eine Schnittstelle, die definiert, wie unser Datendienst aussehen soll und welche Funktionen er haben soll (Laden, Speichern).
/// 

using KinTAP.Model;

namespace KinTAP.Persistence;

public interface IDataPersistence
{
    Task<List<Child>> LoadChildrenAsync();
    Task SaveChildrenAsync(List<Child> children);

    Task<List<TaskItem>> LoadTasksAsync();
    Task SaveTasksAsync(List<TaskItem> tasks);

    Task<List<Reward>> LoadRewardsAsync();
    Task SaveRewardsAsync(List<Reward> rewards);

    Task<List<ClaimedReward>> LoadClaimedRewardsAsync();
    Task SaveClaimedRewardsAsync(List<ClaimedReward> claimedRewards);

    Task<List<TaskSubscription>> LoadSubscriptionAsync();
    Task SaveSubscriptionAsync(List<TaskSubscription> subscriptions);
}
