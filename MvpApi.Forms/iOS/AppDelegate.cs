using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using UIKit;

// This is the one for the Calendar
[assembly: Xamarin.Forms.ExportRenderer(typeof(Telerik.XamarinForms.Input.RadCalendar), typeof(Telerik.XamarinForms.InputRenderer.iOS.CalendarRenderer))]

// for your convenience, here are the rest of the renders
[assembly: Xamarin.Forms.ExportRenderer(typeof(Telerik.XamarinForms.Chart.RadCartesianChart), typeof(Telerik.XamarinForms.ChartRenderer.iOS.CartesianChartRenderer))]
[assembly: Xamarin.Forms.ExportRenderer(typeof(Telerik.XamarinForms.Chart.RadPieChart), typeof(Telerik.XamarinForms.ChartRenderer.iOS.PieChartRenderer))]
[assembly: Xamarin.Forms.ExportRenderer(typeof(Telerik.XamarinForms.Input.RadCalendar), typeof(Telerik.XamarinForms.InputRenderer.iOS.CalendarRenderer))]
[assembly: Xamarin.Forms.ExportRenderer(typeof(Telerik.XamarinForms.DataControls.RadListView), typeof(Telerik.XamarinForms.DataControlsRenderer.iOS.ListViewRenderer))]
[assembly: Xamarin.Forms.ExportRenderer(typeof(Telerik.XamarinForms.Primitives.RadSideDrawer), typeof(Telerik.XamarinForms.PrimitivesRenderer.iOS.SideDrawerRenderer))]
[assembly: Xamarin.Forms.ExportRenderer(typeof(Telerik.XamarinForms.Input.RadDataForm), typeof(Telerik.XamarinForms.InputRenderer.iOS.DataFormRenderer))]
namespace MvpApi.Forms.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            // Required in the iOS project so that it doesn't get optimized out by the linker
            new Telerik.XamarinForms.InputRenderer.iOS.CalendarRenderer();

            // The rest of the renderers
            new Telerik.XamarinForms.DataControlsRenderer.iOS.ListViewRenderer();
            new Telerik.XamarinForms.ChartRenderer.iOS.PieChartRenderer();
            new Telerik.XamarinForms.ChartRenderer.iOS.CartesianChartRenderer();
            new Telerik.XamarinForms.PrimitivesRenderer.iOS.SideDrawerRenderer();
            new Telerik.XamarinForms.InputRenderer.iOS.DataFormRenderer();

            global::Xamarin.Forms.Forms.Init();

            // MUST be done in every target app, after calling Xamarin.Forms.Forms.Init()
            Telerik.XamarinForms.Common.iOS.TelerikForms.Init();
            
            LoadApplication(new Portable.App());

            return base.FinishedLaunching(app, options);
        }
    }
}
