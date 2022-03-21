using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace MvpCompanion.Maui.WinUI;

public partial class App : MauiWinUIApplication
{
    public App()
    {
        this.InitializeComponent();

        Microsoft.Maui.Handlers.WindowHandler.ElementMapper.AppendToMapping(nameof(IWindow), (handler, view) =>
        {
            // Native WinUI app
            var nativeApp = handler.PlatformView as MvpCompanion.Maui.WinUI.App;

            // Maui app
            var app = view as MvpCompanion.Maui.App;

            //nativeWindow.Activate();

            //IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
            //WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            //AppWindow appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            //appWindow.MoveAndResize(new Windows.Graphics.RectInt32(0, 0, 500, 500));
        });
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        Microsoft.Maui.Essentials.Platform.OnLaunched(args);
    }
}

