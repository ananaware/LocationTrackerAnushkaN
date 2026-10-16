using LocationTrackerAnushkaN.Services;
using Microsoft.Extensions.Logging;

namespace LocationTrackerAnushkaN;

/// <summary>
/// Sets up and builds the app: fonts, maps, and services.
/// </summary>
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiMaps() // Turns on the .NET MAUI Maps control
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register our database service so pages can use it.
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<LocationTrackingService>();

        // Register the main page so it can receive the database service.
        builder.Services.AddSingleton<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}