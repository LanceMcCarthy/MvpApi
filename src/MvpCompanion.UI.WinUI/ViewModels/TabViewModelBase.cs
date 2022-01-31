using System.Threading.Tasks;
using CommonHelpers.Common;

namespace MvpCompanion.UI.WinUI.ViewModels
{
    public class TabViewModelBase : ViewModelBase
    {
        public virtual Task OnLoadedAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnUnloadedAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task<bool> OnCloseRequestedAsync()
        {
            return Task.FromResult(true);
        }
    }
}
