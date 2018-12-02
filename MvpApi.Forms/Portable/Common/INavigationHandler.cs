using MvpApi.Forms.Portable.Models;

namespace MvpApi.Forms.Portable.Common
{
    public interface INavigationHandler
    {
        void LoadView(ViewType viewType);
    }
}
