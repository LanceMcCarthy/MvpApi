namespace MvpApi.Common.Interfaces
{
    public interface IExpandableItemView
    {

        void CollapseRow(object item);

        void CollapseAll();
        void ExpandRow(object item);

        void ExpandAll();

        void ToggleRowDetail(object item);
    }
}
