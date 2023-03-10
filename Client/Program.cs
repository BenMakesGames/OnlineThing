using BenMakesGames.PlayPlayMini;
using BenMakesGames.PlayPlayMini.Model;
using Client.GameStates;
using Client.Services;
using Serilog;
using Serilog.Extensions.Autofac.DependencyInjection;

FileSystemHelpers.EnsureDirectoryExists();

var gsmBuilder = new GameStateManagerBuilder();

gsmBuilder
    .SetWindowSize(1920 / 4, 1080 / 4, 2)
    .SetInitialGameState<Startup>()

    // TODO: set a better window title
    .SetWindowTitle("Client")

    // TODO: add any resources needed (refer to PlayPlayMini documentation for more info)
    .AddAssets(new IAsset[]
    {
        // new FontMeta(...)
        // new PictureMeta(...)
        // new SpriteSheetMeta(...)
        // new SongMeta(...)
        // new SoundEffectMeta(...)
    })

    // TODO: any additional service registration (refer to PlayPlayMini and/or Autofac documentation for more info)
    .AddServices(s => {
        var loggerConfig = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
            .WriteTo.Console()
#endif
            .MinimumLevel.Warning()
            .WriteTo.File($"{FileSystemHelpers.GameDataPath}{Path.DirectorySeparatorChar}Log.log", rollingInterval: RollingInterval.Day)
        ;

        s.RegisterSerilog(loggerConfig);
    })
;

gsmBuilder.Run();