using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Telerik.UI.Xaml.Controls.Grid;

namespace MvpCompanion.UI.WinUI.Selectors
{
    public class GroupHeaderTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TopLevelGroupTemplate { get; set; }

        public DataTemplate SecondLevelGroupTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if(item is GroupHeaderContext { Level: 0 })
            {
                return TopLevelGroupTemplate;
            }
            else
            {
                return SecondLevelGroupTemplate;
            }
        }
    }
}
