using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using CommonHelpers.Common;

namespace MvpApi.Wpf.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private string _selectedTheme = "Fluent";

        public SettingsViewModel()
        {
            //if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            //{

            //}

            if (Properties.Settings.Default.PreferredTheme != SelectedTheme)
            {
                _selectedTheme = Properties.Settings.Default.PreferredTheme;
            }
        }

        public DateTime SubmissionStartDate
        {
            get => ((ShellWindow)Application.Current.MainWindow).ViewModel.SubmissionStartDate;
            set => ((ShellWindow)Application.Current.MainWindow).ViewModel.SubmissionStartDate = value;
        }

        public DateTime SubmissionDeadline
        {
            get => ((ShellWindow)Application.Current.MainWindow).ViewModel.SubmissionDeadline;
            set => ((ShellWindow)Application.Current.MainWindow).ViewModel.SubmissionDeadline = value;
        }

        public async Task OnLoadedAsync()
        {
            if (Application.Current.MainWindow is ShellWindow sw && !sw.ViewModel.IsLoggedIn)
            {
                await sw.SignInAsync();
            }

            if (IsBusy)
            {
                IsBusy = false;
                IsBusyMessage = "";
            }
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
                    if (Application.Current.MainWindow is ShellWindow sw)
                    {
                        sw.UpdateTheme(_selectedTheme);

                        Properties.Settings.Default.PreferredTheme = _selectedTheme;

                        Debug.WriteLine($"Theme Updated to : {_selectedTheme}");
                    }
                    
                }
            }
        }
    }
}
