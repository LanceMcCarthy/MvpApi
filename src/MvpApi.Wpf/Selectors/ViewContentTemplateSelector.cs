using System.Windows;
using System.Windows.Controls;
using MvpApi.Wpf.Models;

namespace MvpApi.Wpf.Selectors
{
    public class ViewContentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ProfileTemplate { get; set; }
        public DataTemplate HomeTemplate { get; set; }
        public DataTemplate KudosTemplate { get; set; }
        public DataTemplate SettingsTemplate { get; set; }
        public DataTemplate AuthenticationTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is NavItemModel navItem)
            {
                switch (navItem.Name)
                {
                    case ViewName.Profile:
                        return ProfileTemplate;
                    case ViewName.Home:
                        return HomeTemplate;
                    case ViewName.Kudos:
                        return KudosTemplate;
                    case ViewName.Settings:
                        return SettingsTemplate;
                    case ViewName.Auth:
                        return AuthenticationTemplate;
                }
            }

            return AuthenticationTemplate;
        }
    }

    
}
