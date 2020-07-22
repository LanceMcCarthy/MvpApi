using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using CommonHelpers.Common;

namespace MvpApi.Wpf.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                UseBetaEditor = true;
            }
        }

        public bool UseBetaEditor
        {
            get => (App.Current.MainWindow as ShellWindow).ViewModel.UseBetaEditor;
            set => (App.Current.MainWindow as ShellWindow).ViewModel.UseBetaEditor = value;
        }

        public DateTime SubmissionStartDate
        {
            get => (App.Current.MainWindow as ShellWindow).ViewModel.SubmissionStartDate;
            set => (App.Current.MainWindow as ShellWindow).ViewModel.SubmissionStartDate = value;
        }

        public DateTime SubmissionDeadline
        {
            get => (App.Current.MainWindow as ShellWindow).ViewModel.SubmissionDeadline;
            set => (App.Current.MainWindow as ShellWindow).ViewModel.SubmissionDeadline = value;
        }

        public async Task OnLoadedAsync()
        {
            if ((App.Current.MainWindow as ShellWindow).DataContext is ShellViewModel shellVm)
            {
                if (!shellVm.IsLoggedIn)
                {
                    await (App.Current.MainWindow as ShellWindow).SignInAsync();
                }

                if (IsBusy)
                {
                    IsBusy = false;
                    IsBusyMessage = "";
                }
            }
        }
    }
}
