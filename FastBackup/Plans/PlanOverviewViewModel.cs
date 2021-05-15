using System.Collections.ObjectModel;
using FastBackup.Util;

namespace FastBackup.Plans
{
    public class PlanOverviewViewModel : ViewModelBase
    {
        private Repository _planRepository;

        public ObservableCollection<BackupPlan> Plans { get; set; } = new();

        public PlanOverviewViewModel()
        {
            _planRepository = new Repository();

            foreach (var plan in _planRepository.GetCollection<BackupPlan>().FindAll())
            {
                Plans.Add(plan);
            }
        }
    }
}
