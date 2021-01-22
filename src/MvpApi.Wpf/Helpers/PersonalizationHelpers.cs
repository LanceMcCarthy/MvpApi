using System;
using System.Collections.Generic;
using System.Windows;

namespace MvpApi.Wpf.Helpers
{
    public static class PersonalizationHelpers
    {
        /// <summary>
        /// Helper for merging Telerik theme into the App Resources
        /// </summary>
        /// <param name="themeAssemblyName">Name of the theme to use (e.g. "Fluent" or "Office2013"). Query this class's ThemeNames list for available names to use.</param>
        public static void UpdateTheme(string themeAssemblyName)
        {
            Application.Current.Resources.MergedDictionaries.Clear();

            foreach (var fileName in ResourceDictionaryList)
            {
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri($"/Telerik.Windows.Themes.{themeAssemblyName};component/Themes/{fileName}", UriKind.RelativeOrAbsolute)
                });
            }

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("/Styles/GeneralStyles.xaml", UriKind.RelativeOrAbsolute)
            });
        }

        /// <summary>
        /// Helper for merging Telerik themes into a pre-defined ResourceDictionary. This is useful for custom controls that do not have access to the App resources
        /// </summary>
        /// <param name="source">The ResourceDictionary to merge the Telerik theme ResourceDictionaries into.</param>
        /// <param name="themeAssemblyName">Name of the theme to use (e.g. "Fluent" or "Office2013"). Query this class's ThemeNames list for available names to use.</param>
        public static void UpdateTheme(ResourceDictionary source, string themeAssemblyName)
        {
            if (source == null)
            {
                throw new Exception("You must pass a valid ResourceDictionary to reset the themes.");
            }

            source.MergedDictionaries.Clear();

            foreach (var fileName in ResourceDictionaryList)
            {
                source.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri($"/Telerik.Windows.Themes.{themeAssemblyName};component/Themes/{fileName}", UriKind.RelativeOrAbsolute)
                });
            }

            source.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("/Styles/GeneralStyles.xaml", UriKind.RelativeOrAbsolute)
            });
        }

        // This is the ensure we are merging the correct Resources
        private static readonly string[] ResourceDictionaryList =
        {
            "System.Windows.xaml",
            "Telerik.Windows.Controls.xaml",
            "Telerik.Windows.Controls.Data.xaml",
            "Telerik.Windows.Controls.Docking.xaml",
            "Telerik.Windows.Controls.FileDialogs.xaml",
            "Telerik.Windows.Controls.GridView.xaml",
            "Telerik.Windows.Controls.Input.xaml",
            "Telerik.Windows.Controls.Navigation.xaml",
            "Telerik.Windows.Controls.RichTextBox.xaml"
        };

        /// <summary>
        /// List of themes names that are currently supported.
        /// </summary>
        public static List<string> ThemeNames { get; } = new List<string>
        {
            "Fluent",
            "VisualStudio2019",
            "Crystal",
            "Expression_Dark",
            "Green",
            "Material",
            "Office2013",
            "Office2016",
            "Summer",
            "Transparent",
            "Vista",
            "VisualStudio2013",
            "Windows7",
            "Windows8",
        };
    }
}
