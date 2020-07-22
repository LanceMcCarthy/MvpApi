using System;
using System.Collections.Generic;
using System.Windows;
using Windows.Storage;
using CommonHelpers.Common;
using MvpApi.Services.Utilities;

namespace MvpApi.Wpf.ViewModels
{
    public class ShellViewModel : ViewModelBase
    {
        private MvpApi.Common.Models.ProfileViewModel mvp;
        private string profileImagePath;
        private bool isLoggedIn;
        private bool useBetaEditor;
        private DateTime submissionStartDate = ServiceConstants.SubmissionStartDate;
        private DateTime submissionDeadline = ServiceConstants.SubmissionDeadline;
        private string _selectedTheme = "Fluent";

        public ShellViewModel()
        {
            //if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            //{
            //    Mvp = DesignTimeHelpers.GenerateSampleMvp();
            //    IsLoggedIn = true;
            //    ProfileImagePath = "/Images/MvpIcon.png";
            //}

            if (Properties.Settings.Default.PreferredTheme != SelectedTheme)
            {
                SelectedTheme = Properties.Settings.Default.PreferredTheme;
            }
        }

        public string ProfileImagePath
        {
            get => profileImagePath;
            set
            {
                profileImagePath = value;

                // Manually invoke PropertyChanged to ensure image is reloaded, even if the file path is the same.
                OnPropertyChanged();
            }
        }

        public MvpApi.Common.Models.ProfileViewModel Mvp
        {
            get => mvp;
            set => SetProperty(ref mvp, value);
        }

        public bool IsLoggedIn
        {
            get => isLoggedIn;
            set => SetProperty(ref isLoggedIn, value);
        }
        
        public bool UseBetaEditor
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.TryGetValue(nameof(UseBetaEditor), out object rawValue))
                {
                    useBetaEditor = (bool)rawValue;
                }
                else
                {
                    ApplicationData.Current.LocalSettings.Values[nameof(UseBetaEditor)] = useBetaEditor;
                }
                
                return useBetaEditor;
            }
            set
            {
                if (SetProperty(ref useBetaEditor, value))
                {
                    ApplicationData.Current.LocalSettings.Values[nameof(UseBetaEditor)] = useBetaEditor;
                }
            }
        }

        public DateTime SubmissionStartDate
        {
            get => submissionStartDate;
            set => SetProperty(ref submissionStartDate, value);
        }

        public DateTime SubmissionDeadline
        {
            get => submissionDeadline;
            set => SetProperty(ref submissionDeadline, value);
        }

        public List<string> Themes { get; } = new List<string>
        {
            "Crystal",
            "Expression_Dark",
            "Fluent",
            "Green",
            "Material",
            "Office_Black",
            "Office_Blue",
            "Office_Silver",
            "Office2013",
            "Office2016",
            "Office2016_Touch",
            "Summer",
            "Transparent",
            "Vista",
            "VisualStudio2013",
            "VisualStudio2019",
            "Windows7",
            "Windows8",
            "Windows8Touch"
        };

        public string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (SetProperty(ref _selectedTheme, value))
                {
                    UpdateTheme(_selectedTheme);
                }
            }
        }

        public static void UpdateTheme(string assemblyName)
        {
            Application.Current.Resources.MergedDictionaries.Clear();

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"/Telerik.Windows.Themes.{assemblyName};component/Themes/System.Windows.xaml", UriKind.RelativeOrAbsolute)
            });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"/Telerik.Windows.Themes.{assemblyName};component/Themes/Telerik.Windows.Controls.xaml", UriKind.RelativeOrAbsolute)
            });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"/Telerik.Windows.Themes.{assemblyName};component/Themes/Telerik.Windows.Controls.Input.xaml", UriKind.RelativeOrAbsolute)
            });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"/Telerik.Windows.Themes.{assemblyName};component/Themes/Telerik.Windows.Controls.GridView.xaml", UriKind.RelativeOrAbsolute)
            });
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"/Telerik.Windows.Themes.{assemblyName};component/Themes/Telerik.Windows.Controls.Navigation.xaml", UriKind.RelativeOrAbsolute)
            });

            Properties.Settings.Default.PreferredTheme = assemblyName;
        }
    }
}
