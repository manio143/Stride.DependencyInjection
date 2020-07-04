using Microsoft.Extensions.DependencyInjection;
using System;

namespace Stride.DependencyInjection
{
    // The following class is holding a ServiceCollection and a ServiceProvider
    // from the Microsoft.Extensions.DependencyInjection library.
    // It is to be added as a service by using ConfigureDependencyInjection
    // on a Game instance.

    /// <summary>
    /// A service interfacing with DependencyInjection library.
    /// </summary>
    public sealed class DependencyService
    {
        private ServiceCollection Services { get; } = new ServiceCollection();

        private ServiceProvider serviceProvider;
        private uint changeTag;
        private uint instantiatedChangeTag;

        /// <summary>
        /// Creates a new <see cref="DependencyService"/>.
        /// </summary>
        public DependencyService() { }

        /// <summary>
        /// Creates a new <see cref="DependencyService"/> and initializes it with <paramref name="dependencyConfiguration"/>.
        /// </summary>
        /// <param name="dependencyConfiguration"></param>
        public DependencyService(Action<IServiceCollection> dependencyConfiguration)
        {
            dependencyConfiguration(Services);
        }

        /// <summary>
        /// ServiceProvider for dependency resolution and instantiation.
        /// </summary>
        public ServiceProvider ServiceProvider
        {
            get
            {
                // The ServiceProvider instance is constructed lazily
                // and updated whenever the Services collection is modified.
                EnsureServiceProvider();
                return serviceProvider;
            }
        }

        private void EnsureServiceProvider()
        {
            lock (serviceProvider)
            {
                if (serviceProvider == null || changeTag != instantiatedChangeTag)
                {
                    serviceProvider = Services.BuildServiceProvider();
                    instantiatedChangeTag = changeTag;
                }
            }
        }

        /// <summary>
        /// Executes the <paramref name="modification"/> on the internal service collection.
        /// </summary>
        /// <param name="modification"></param>
        public void ModifyServices(Action<IServiceCollection> modification)
        {
            modification(Services);
            changeTag++;
        }
    }
}
