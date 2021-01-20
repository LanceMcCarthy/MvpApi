﻿using System.Windows;
using System.Windows.Controls;
using MvpApi.Wpf.ViewModels;

namespace MvpApi.Wpf.Views
{
    public partial class AuthenticationView : UserControl
    {
        public AuthenticationView()
        {
            InitializeComponent();

            Loaded += AuthenticationView_Loaded;
        }

        private void AuthenticationView_Loaded(object sender, RoutedEventArgs e)
        {
            if(DataContext is AuthenticationViewModel vm)
            {

            }
        }

        private async void LoginButton_OnClick(object sender, RoutedEventArgs e)
        {
            await App.MainLoginWindow.SignInAsync();
        }
    }
}