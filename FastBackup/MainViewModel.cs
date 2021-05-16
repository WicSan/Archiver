using FastBackup.Plans;
using FastBackup.Util;
using System.Collections.Generic;

namespace FastBackup
{
    public class MainViewModel : ViewModelBase
    {
        private NavigationService _navigationService;

        public ViewModelBase? CurrentPageViewModel => 
            (ViewModelBase?)_navigationService.CurrentPageViewModel;

        public MainViewModel()
        {
            _navigationService = new NavigationService();

            var models = new List<INavigatebleViewModel>()
            {
                new PlanOverviewViewModel(_navigationService),
                new CrupdatePlanViewModel(_navigationService),
            };
            _navigationService.ViewModels = models;

            _navigationService.ViewModelChanged += (_, _) => OnPropertyChanged(nameof(CurrentPageViewModel));

            _navigationService.Navigate(typeof(PlanOverviewViewModel));
        }
    }
}
