using Microsoft.UI.Xaml;

namespace MvpCompanion.UI.WinUI
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.Title = "MVP Companion";
            this.ExtendsContentIntoTitleBar = true;
        }
    }
}
