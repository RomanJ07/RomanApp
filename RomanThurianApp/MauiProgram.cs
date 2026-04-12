using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using PokeApiNet;
using RomanThurianApp.Services;
using RomanThurianApp.ViewModels;

namespace RomanThurianApp;

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
        builder.Services.AddSingleton<ICapturedPokemonService, CapturedPokemonService>();
        builder.Services.AddSingleton<ITrainerTeamRepository, SqliteTrainerTeamRepository>();
        builder.Services.AddTransient<PokedexViewModel>();
        builder.Services.AddTransient<TrainerViewModel>();
        builder.Services.AddTransient<PokedexPage>();
        builder.Services.AddTransient<TrainerPage>();
        builder.Services.AddSingleton<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}