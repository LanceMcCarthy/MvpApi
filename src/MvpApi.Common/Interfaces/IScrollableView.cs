using System.Threading.Tasks;

namespace MvpApi.Common.Interfaces
{
    public interface IScrollableView
    {
        int GetCurrentVisibleIndex();

        object GetCurrentVisibleItem();

        void ScrollToTop();

        void ScrollTo(object item);

        void ScrollTo(int index);

        void ScrollToEnd();
    }
}
