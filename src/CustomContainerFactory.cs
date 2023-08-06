using dotless.Core;
using dotless.Core.configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace WebOptimizer.Dotless
{
    /// <summary>
    /// Represents custom <see cref="ContainerFactory" />, which registers <see cref="FileProviderFileReaderAdapter" /> adapter instead of file reader from <see cref="DotlessConfiguration.LessSource" />.
    /// </summary>
    public class CustomContainerFactory : ContainerFactory
    {
        private readonly IFileProvider _fileProvider;

        /// <summary>
        /// Constructs custom container factory
        /// <paramref name="fileProvider">Custom file provider, which will be used by <see cref="FileProviderFileReaderAdapter" /> adapter.</paramref>
        /// </summary>
        public CustomContainerFactory(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        /// <inheritdoc />
        protected override void RegisterServices(IServiceCollection services, DotlessConfiguration configuration)
        {
            configuration.LessSource = typeof(FileProviderFileReaderAdapter);
            base.RegisterServices(services, configuration);

            services.AddSingleton<IFileProvider>(_fileProvider);
        }
    }
}

