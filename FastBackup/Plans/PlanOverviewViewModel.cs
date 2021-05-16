using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using FastBackup.Util;

namespace FastBackup.Plans
{
    public class PlanOverviewViewModel : ViewModelBase, INavigatebleViewModel
    {
        private Repository _planRepository;
        private readonly NavigationService _navigationService;

        public PlanOverviewViewModel(NavigationService navigationService)
        {
            _planRepository = new Repository();
            _navigationService = navigationService;

            CreateCommand = new RelayCommand(Create);
        }

        public ObservableCollection<BackupPlan> Plans { get; set; } = new();

        public ICommand CreateCommand { get; set; }

        private void Create()
        {
            _navigationService.Navigate(typeof(CrupdatePlanViewModel));
        }

        public void NavigateTo(object? param)
        {
            foreach (var plan in _planRepository.GetCollection<BackupPlan>().FindAll())
            {
                if (Plans.All(p => !plan.Name.Equals(p.Name, System.StringComparison.InvariantCulture)))
                {
                    Plans.Add(plan);
                }
            }
        }
    }
}
