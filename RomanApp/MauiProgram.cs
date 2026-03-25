using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PokeApiNet;
using RomanApp.Services;
using RomanApp.ViewModels;

namespace RomanApp;

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

        builder.Services.AddSingleton<PokeApiClient>();
        builder.Services.AddTransient<IPokeApiService, PokeApiService>();
        builder.Services.AddTransient<PokedexViewModel>();
        builder.Services.AddTransient<PokedexPage>();
        builder.Services.AddSingleton<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}