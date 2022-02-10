using System.Threading.Tasks;

namespace MvpApi.Common.Interfaces
{
    public interface IScrollableView
    {
        void ScrollToTop();

        void ScrollTo(object item);

        void ScrollTo(int index);

        void ScrollToEnd();
    }
}
