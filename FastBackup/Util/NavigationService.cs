using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace FastBackup.Util
{
    public class NavigationService
    {
        private ICollection<INavigatebleViewModel> ViewModels { get; set; } = new List<INavigatebleViewModel>();

        public List<INavigatebleViewModel> ViewModelList { get; }

        public INavigatebleViewModel CurrentPageViewModel { get; set; }

        public NavigationService()
        {
        }

        private void Navigate(Type type)
        {
            CurrentPageViewModel = ViewModelList.First(vm => vm.GetType() == type);
        }

        public event EventHandler? ViewModelChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
