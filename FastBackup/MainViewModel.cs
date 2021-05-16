using FastBackup.Plans;
using FastBackup.Util;
using System.Collections.Generic;
using System.Linq;

namespace FastBackup
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase _currentPageViewModel;

        private List<ViewModelBase> _viewModelList = new List<ViewModelBase>();

        public List<ViewModelBase> ViewModelList
        {
            get
            {
                return _viewModelList;
            }
        }

        public ViewModelBase CurrentPageViewModel
        {
            get
            {
                return _currentPageViewModel;
            }
            set
            {
                _currentPageViewModel = value;
                OnPropertyChanged("CurrentPageViewModel");
            }
        }

        public RelayCommand<ViewModelBase> ChangePageCommand { get; private set; }

        public MainViewModel()
        {
            ChangePageCommand = new RelayCommand<ViewModelBase>(p => ChangePageAction(p));

            _viewModelList.Add(new CrupdatePlanViewModel());
            _viewModelList.Add(new PlanOverviewViewModel());

            _currentPageViewModel = _viewModelList[0];
        }

        private void ChangePageAction(ViewModelBase? viewModel)
        {
            CurrentPageViewModel = ViewModelList.First(vm => vm == viewModel);
        }
    }
}
