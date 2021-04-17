using System.Collections.ObjectModel;
using FastBackup.Util;

namespace FastBackup.Plans
{
    public class PlanOverviewViewModel : ViewModelBase
    {
        private PlanRepository _planRepository;

        public ObservableCollection<BackupPlan> Plans { get; set; } = new();

        public PlanOverviewViewModel()
        {
            _planRepository = new PlanRepository();

            foreach (var plan in _planRepository.GetCollection<BackupPlan>().FindAll())
            {
                Plans.Add(plan);
            }
        }
    }
}
