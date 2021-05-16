using System;
using System.Collections.Generic;
using System.Linq;

namespace FastBackup.Util
{
    public class NavigationService
    {
        public void Navigate(Type type, object? param = null)
        {
            CurrentPageViewModel?.Leave();
            CurrentPageViewModel = ViewModels.First(vm => vm.GetType() == type);
            ViewModelChanged?.Invoke(this, new EventArgs());
            CurrentPageViewModel!.NavigateTo(param);
        }


        public ICollection<INavigatebleViewModel> ViewModels { get; set; } = new List<INavigatebleViewModel>();

        public INavigatebleViewModel? CurrentPageViewModel { get; private set; }


        public event EventHandler? ViewModelChanged;
    }
}
