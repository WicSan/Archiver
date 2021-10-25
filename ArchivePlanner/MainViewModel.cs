using ArchivePlanner.Planning;
using ArchivePlanner.Planning.Model;
using ArchivePlanner.Util;
using FastBackup.Planning;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ArchivePlanner
{
    public class MainViewModel : ViewModelBase
    {
        private readonly PlanningRepository _repository;

        public BackupPlanOverview PlanView { get; set; }

        public ObservableCollection<BackupPlan> Plans { get; set; } = new ObservableCollection<BackupPlan>();

        public MainViewModel(BackupPlanOverview view, PlanningRepository repository)
        {
            LoadPlans(repository.GetAll<BackupPlan>());

            PlanView = view;
            _repository = repository;
            ((BackupPlanViewModel)PlanView.DataContext).BackupPlan = Plans.FirstOrDefault() ?? new FullBackupPlan();

            ((BackupPlanViewModel)PlanView.DataContext).OnPlanSaved += MainViewModel_OnPlanSaved;
        }

        private void MainViewModel_OnPlanSaved(object? sender, System.EventArgs e)
        {
            Plans.Clear();
            LoadPlans(_repository.GetAll<BackupPlan>());
        }

        private void LoadPlans(IEnumerable<BackupPlan> plans)
        {
            foreach (var plan in plans)
            {
                Plans.Add(plan);
            }
        }
    }
}
