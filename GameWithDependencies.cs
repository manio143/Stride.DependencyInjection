using System;
using Microsoft.Extensions.DependencyInjection;
using Stride.Engine;

namespace Stride.DependencyInjection
{
    public static class GameWithDependencies
    {
        /// <summary>
        /// Adds a new <see cref="DependencyService"/> to <paramref name="game"/>'s Services,
        /// populates it with public game services and executes <paramref name="dependencyConfiguration"/>
        /// provided by the user.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="dependencyConfiguration"></param>
        public static void ConfigureDependencyInjection(this Game game, Action<IServiceCollection> dependencyConfiguration)
        {
            var dependencyService = new DependencyService(services => {
                services.AddSingleton(game);

                services.AddSingleton(game.Script);
                services.AddSingleton(game.SceneSystem);
                services.AddSingleton(game.Streaming);
                services.AddSingleton(game.Audio);
                services.AddSingleton(game.SpriteAnimation);
                services.AddSingleton(game.DebugTextSystem);
                services.AddSingleton(game.ProfilingSystem);
                services.AddSingleton(game.ProfilingSystem);
                services.AddSingleton(game.VRDeviceSystem);
                services.AddSingleton(game.GraphicsDeviceManager);

                dependencyConfiguration(services);
            });
            game.Services.AddService(dependencyService);
        }
    }
}
