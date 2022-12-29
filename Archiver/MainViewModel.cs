using Archiver.Planning;
using Archiver.Planning.Model;
using Archiver.Util;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Archiver
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IBackupPlanOverviewViewModelFactory _viewModelFactory;
        private readonly IRepository<BackupPlan> _repository;
        private BackupPlanListItemViewModel _selectedItem = null!;

        public BackupPlanOverview PlanView { get; set; }

        public ObservableCollection<BackupPlanListItemViewModel> Plans { get; private set; } = new ObservableCollection<BackupPlanListItemViewModel>();

        public RelayCommand AddCommand { get; }

        public MainViewModel(IBackupPlanOverviewViewModelFactory viewModelFactory, IRepository<BackupPlan> repository)
        {
            _viewModelFactory = viewModelFactory;
            _repository = repository;

            AddCommand = new RelayCommand(AddNewPlan);

            PlanView = new BackupPlanOverview();
            AddNewPlan();
        }

        public BackupPlanListItemViewModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (value == null)
                {
                    return;
                }

                var viewModel = _viewModelFactory.CreateModel(value.Plan);
                PlanView.DataContext = viewModel;
                viewModel.OnSavePlan += MainViewModel_OnSavePlan;
                viewModel.OnCancel += MainViewModel_OnCancel;

                _selectedItem = value;
                OnPropertyChanged();
            }
        }

        public async Task LoadPlans()
        {
            bool isFirst = true;
            await foreach (var plan in _repository.GetAllAsync())
            {
                var viewModel = new BackupPlanListItemViewModel(plan);
                Plans.Add(viewModel);

                if (isFirst)
                {
                    isFirst = false;
                    SelectedItem = viewModel;
                    Plans.RemoveAt(0);
                }
            }
        }

        private void MainViewModel_OnCancel(object? sender, System.EventArgs e)
        {
            if (SelectedItem!.IsNew)
            {
                if(Plans.Count > 1)
                {
                    SelectedItem = Plans.ElementAt(Plans.Count - 2);
                    Plans.RemoveAt(Plans.Count - 1);
                }
            }
            else
            {
                PlanView.DataContext = _viewModelFactory.CreateModel(SelectedItem.Plan);
            }
        }

        private async void MainViewModel_OnSavePlan(object? sender, BackupPlan plan)
        {
            if(plan.Id == default)
            {
                plan.Id = System.Guid.NewGuid();
            }
            await _repository.UpsertAsync(plan);

            SelectedItem!.Plan = plan;
        }

        private void AddNewPlan()
        {
            var newPlan = new BackupPlan
            {
                Name = $"Backup {Plans.Count}",
            };

            var newItem = new BackupPlanListItemViewModel(newPlan);

            SelectedItem = newItem;
            Plans.Add(newItem);
        }
    }
}
