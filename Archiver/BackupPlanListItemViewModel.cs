using Archiver.Planning.Model;
using Archiver.Util;

namespace Archiver
{
    public class BackupPlanListItemViewModel : ViewModelBase
    {
        private BackupPlan _plan;

        public BackupPlanListItemViewModel(BackupPlan plan)
        {
            _plan = plan;
        }

        public BackupPlan Plan
        {
            get => _plan;
            set
            {
                _plan = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Name));
            }
        }

        public string Name => _plan.Name;

        public bool IsNew => _plan.Id == default;
    }
}
