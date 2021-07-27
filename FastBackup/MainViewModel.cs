using FastBackup.Plans;
using FastBackup.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FastBackup
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IRepository _repository;

        public BackupPlanOverview PlanView { get; set; }

        public ObservableCollection<BackupPlan> Plans { get; set; } = new ObservableCollection<BackupPlan>();

        public MainViewModel(BackupPlanOverview view, IRepository repository)
        {
            LoadPlans(repository.GetCollection<BackupPlan>().FindAll());

            PlanView = view;
            this._repository = repository;
            ((BackupPlanViewModel)PlanView.DataContext).BackupPlan = Plans.FirstOrDefault() ?? new BackupPlan();

            ((BackupPlanViewModel)PlanView.DataContext).OnPlanSaved += MainViewModel_OnPlanSaved;
        }

        private void MainViewModel_OnPlanSaved(object? sender, System.EventArgs e)
        {
            Plans.Clear();
            LoadPlans(_repository.GetCollection<BackupPlan>().FindAll());
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
