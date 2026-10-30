///
/// Datendienst. Enthält den Pfad zum Speicherort und Methoden zum Laden und Speichern von Daten.
/// 

using System.Diagnostics;
using System.Text.Json;
using KinTAP.Model;

namespace KinTAP.Persistence

{
    public class JsonPersistenceService : IDataPersistence
    {
        private readonly string _dataFolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "KinTAP");

        private readonly string _childrenFilePath;
        private readonly string _tasksFilePath;
        private readonly string _rewardsFilePath;
        private readonly string _claimedRewardsFilePath;
        private readonly string _subscriptionFilePath;
        
        public JsonPersistenceService(string baseDataFolderPath)
        {
            _dataFolderPath = Path.Combine(baseDataFolderPath, "KinTAP");
            _childrenFilePath = Path.Combine(_dataFolderPath, "children.json");
            _tasksFilePath = Path.Combine(_dataFolderPath, "tasks.json");
            _rewardsFilePath = Path.Combine(_dataFolderPath, "rewards.json");
            _claimedRewardsFilePath = Path.Combine(_dataFolderPath, "claimed_rewards.json");
            _subscriptionFilePath = Path.Combine(_dataFolderPath, "subscriptions.json");

            Directory.CreateDirectory(_dataFolderPath);
        }

        public async  Task<List<Child>> LoadChildrenAsync()
        {
            return await LoadDataAsync<Child>(_childrenFilePath);
        }

        public async Task SaveChildrenAsync(List<Child> children)
        {
            await SaveDataAsync(children, _childrenFilePath);
        }

        public async Task<List<TaskItem>> LoadTasksAsync()
        {
            return await LoadDataAsync<TaskItem>(_tasksFilePath);
        }

        public async Task SaveTasksAsync(List<TaskItem> tasks)
        {
            await SaveDataAsync(tasks, _tasksFilePath);
        }

        public async Task<List<Reward>> LoadRewardsAsync()
        {
            return await LoadDataAsync<Reward>(_rewardsFilePath);
        }

        public async Task SaveRewardsAsync(List<Reward> rewards)
        {
            await SaveDataAsync(rewards, _rewardsFilePath);
        }

        public async Task<List<ClaimedReward>> LoadClaimedRewardsAsync()
        {
            return await LoadDataAsync<ClaimedReward>(_claimedRewardsFilePath);
        }

        public async Task SaveClaimedRewardsAsync(List<ClaimedReward> claimedRewards)
        {
            await SaveDataAsync(claimedRewards, _claimedRewardsFilePath);
        }

        private async Task<List<T>> LoadDataAsync<T>(string filePath)
        {
            if(!File.Exists(filePath))
            {
                return new List<T>();
            }

            try
            {
                string json = await File.ReadAllTextAsync(filePath);

                if(string.IsNullOrWhiteSpace(json))
                {
                    return new List<T>();
                }

                var result = JsonSerializer.Deserialize<List<T>>(json);
                return result ?? new List<T>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"(Async) Error loading data from {filePath}: {ex.Message}");
                return new List<T>();
            }
        }

        private async Task SaveDataAsync<T>(List<T> data, string filePath)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(data, options);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"(Async) Error saving data to {filePath}: {ex.Message}");
            }
        }

        public async Task<List<TaskSubscription>> LoadSubscriptionAsync()
        {
            return await LoadDataAsync<TaskSubscription>(_subscriptionFilePath);
        }

        public async Task SaveSubscriptionAsync(List<TaskSubscription> subscriptions)
        {
            await SaveDataAsync(subscriptions, _subscriptionFilePath);
        }
    }
}