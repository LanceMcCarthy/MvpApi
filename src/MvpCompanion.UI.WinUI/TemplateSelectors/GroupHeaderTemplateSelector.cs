using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Telerik.UI.Xaml.Controls.Grid;

namespace MvpCompanion.UI.WinUI
{
    public class GroupHeaderTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TopLevelGroupTemplate { get; set; }
        public DataTemplate SecondLevelGroupTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, Microsoft.UI.Xaml.DependencyObject container)
        {
            if ((item as GroupHeaderContext).Level == 0)
            {
                return this.TopLevelGroupTemplate;
            }
            else
            {
                return this.SecondLevelGroupTemplate;
            }
        }
    }
}
