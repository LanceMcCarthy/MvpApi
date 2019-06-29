using System;
using System.Diagnostics;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Microsoft.Toolkit.Uwp.UI.Lottie;

namespace MvpApi.Uwp.Controls
{
    public sealed partial class LottieBusyIndicator : UserControl
    {
        public LottieBusyIndicator()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty LottieFilePathProperty = DependencyProperty.Register(
            "LottieFilePath", typeof(string), typeof(LottieBusyIndicator), new PropertyMetadata(default(string), OnLottieFilePathChanged));

        private static void OnLottieFilePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LottieBusyIndicator self && e.NewValue is string lottieFilePath)
            {
                self.LottiePlayer.Source = new LottieVisualSource { UriSource = new Uri(lottieFilePath) };
                self.LottiePlayer.AutoPlay = true;
            }
        }

        public string LottieFilePath
        {
            get => (string)GetValue(LottieFilePathProperty);
            set => SetValue(LottieFilePathProperty, value);
        }

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            "IsActive", typeof(bool), typeof(LottieBusyIndicator), new PropertyMetadata(default(bool), IsActiveChanged));

        private static async void IsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LottieBusyIndicator self && e.NewValue is bool isBusy)
            {
                if (isBusy)
                {
                    await self.LottiePlayer.PlayAsync(0, 100, true);
                }
                else
                {
                    self.LottiePlayer.Stop();
                }
            }
        }

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public static readonly DependencyProperty BusyMessageProperty = DependencyProperty.Register(
            "BusyMessage", typeof(string), typeof(LottieBusyIndicator), new PropertyMetadata("please wait...", BusyMessageChanged));

        private static void BusyMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LottieBusyIndicator self && e.NewValue is string message)
            {
                self.MessageTextBlock.Text = message;
            }
        }

        public string BusyMessage
        {
            get => (string)GetValue(BusyMessageProperty);
            set => SetValue(BusyMessageProperty, value);
        }
    }
}
