using System;
using dotless.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace WebOptimizer.Dotless
{
    /// <summary>
    /// Compiles Less files
    /// </summary>
    /// <seealso cref="WebOptimizer.IProcessor" />
    public class Compiler : IProcessor
    {
        /// <summary>
        /// Gets the custom key that should be used when calculating the memory cache key.
        /// </summary>
        public string CacheKey(HttpContext context, IAssetContext config) => String.Empty;

        /// <summary>
        /// Executes the processor on the specified configuration.
        /// </summary>
        public Task ExecuteAsync(IAssetContext context)
        {
            var content = new Dictionary<string, byte[]>();
            var env = context.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
            IFileProvider fileProvider = context.Asset.GetFileProvider(env);

            var engine = new EngineFactory().GetEngine(new CustomContainerFactory(fileProvider));

            foreach (string route in context.Content.Keys)
            {
                var fileName = Path.GetFileName(route);

                // Do not do transformations for .css files
                if (fileName?.EndsWith(".css", StringComparison.InvariantCultureIgnoreCase) ?? false)
                {
                    content[route] = context.Content[route];
                }
                else
                {
                    engine.CurrentDirectory = Path.GetDirectoryName(route);
                    engine.ResetImports();
                    var css = engine.TransformToCss(context.Content[route].AsString(), fileName);

                    content[route] = System.Text.Encoding.UTF8.GetBytes(css);
                }
            }

            context.Content = content;

            return Task.CompletedTask;
        }

        
        
    }
}
