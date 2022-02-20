using Telerik.Maui.Controls.Compatibility;

namespace MvpCompanion.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseTelerik()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("telerikfontexamples.ttf", "telerikfontexamples");
			});

		return builder.Build();
	}
}
