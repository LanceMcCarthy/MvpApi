using Telerik.XamarinForms.DataControls.ListView;

namespace MvpCompanion.Maui.Selectors;

public class GroupHeaderTemplateSelector : DataTemplateSelector
{
    public DataTemplate TopLevelGroupTemplate { get; set; }

    public DataTemplate SecondLevelGroupTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        if (item is GroupHeaderContext { Level: 0 })
        {
            return TopLevelGroupTemplate;
        }
        else
        {
            return SecondLevelGroupTemplate;
        }
    }
}
