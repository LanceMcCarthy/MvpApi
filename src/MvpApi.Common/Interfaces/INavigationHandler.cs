using MvpApi.Common.Models.Navigation;

namespace MvpApi.Common.Interfaces
{
    public interface INavigationHandler
    {
        void LoadView(ViewType viewType);
    }
}
