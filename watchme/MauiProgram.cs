using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace watchme;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

            builder.Services.AddSingleton<WatchConnectivityManager>();
            builder.Services.AddTransient<WatchConnectivityViewModel>();
            builder.Services.AddTransient<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

//#if IOS
//    builder.Services.AddSingleton<WatchConnectivityManager>();
//#endif

        return builder.Build();
    }
}
