using System.Collections.Generic;
using NUglify.Css;
using WebOptimizer;
using WebOptimizer.Dotless;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions methods for registration the Less compiler on the Asset Pipeline.
    /// </summary>
    public static class PipelineExtensions
    {
        /// <summary>
        /// Compile Less files on the asset pipeline.
        /// </summary>
        public static IAsset CompileLess(this IAsset asset)
        {
            asset.Processors.Add(new Compiler());
            return asset;
        }

        /// <summary>
        /// Compile Less files on the asset pipeline.
        /// </summary>
        public static IEnumerable<IAsset> CompileLess(this IEnumerable<IAsset> assets)
        {
            var list = new List<IAsset>();

            foreach (IAsset asset in assets)
            {
                list.Add(asset.CompileLess());
            }

            return list;
        }

        /// <summary>
        /// Compile Less files on the asset pipeline.
        /// </summary>
        /// <param name="pipeline">The asset pipeline.</param>
        /// <param name="route">The route where the compiled .css file will be available from.</param>
        /// <param name="sourceFiles">The path to the .less source files to compile.</param>
        public static IAsset AddLessBundle(this IAssetPipeline pipeline, string route, params string[] sourceFiles)
        {
            return pipeline.AddLessBundle(route, new CssSettings(), sourceFiles);
        }

        /// <summary>
        /// Compile Less files on the asset pipeline.
        /// </summary>
        /// <param name="pipeline">The asset pipeline.</param>
        /// <param name="route">The route where the compiled .css file will be available from.</param>
        /// <param name="cssSettings">The compiled css post-process settings.</param>
        /// <param name="sourceFiles">The path to the .less source files to compile.</param>
        public static IAsset AddLessBundle(this IAssetPipeline pipeline, string route, CssSettings cssSettings, params string[] sourceFiles)
        {
            return
                pipeline
                    .AddBundle(route, "text/css; charset=UTF-8", sourceFiles)
                    .EnforceFileExtensions(".less", ".css") // some .less bundles can include regular css files, so it's need to accept them too
                    .CompileLess()
                    .AdjustRelativePaths()
                    .Concatenate()
                    .FingerprintUrls()
                    .AddResponseHeader("X-Content-Type-Options", "nosniff")
                    .MinifyCss(cssSettings);
        }

        /// <summary>
        /// Compiles .less files into CSS and makes them servable in the browser.
        /// </summary>
        /// <param name="pipeline">The asset pipeline.</param>
        public static IEnumerable<IAsset> CompileLessFiles(this IAssetPipeline pipeline)
        {
            return pipeline.CompileLessFiles("**/*.less");
        }

        /// <summary>
        /// Compiles the specified .less files into CSS and makes them servable in the browser.
        /// </summary>
        /// <param name="pipeline">The pipeline object.</param>
        /// <param name="sourceFiles">A list of relative file names of the sources to compile.</param>
        public static IEnumerable<IAsset> CompileLessFiles(this IAssetPipeline pipeline, params string[] sourceFiles)
        {
            return pipeline.CompileLessFiles(new CssSettings(), sourceFiles);
        }

        /// <summary>
        /// Compiles the specified .less files into CSS and makes them servable in the browser.
        /// </summary>
        /// <param name="pipeline">The pipeline object.</param>
        /// <param name="cssSettings">The compiled css post-process settings.</param>
        /// <param name="sourceFiles">A list of relative file names of the sources to compile.</param>
        public static IEnumerable<IAsset> CompileLessFiles(this IAssetPipeline pipeline, CssSettings cssSettings, params string[] sourceFiles)
        {
            return
                pipeline
                    .AddFiles("text/css; charset=UFT-8", sourceFiles)
                    .EnforceFileExtensions(".less", ".css") // some .less bundles can include regular css files, so it's need to accept them too
                    .CompileLess()
                    .FingerprintUrls()
                    .AddResponseHeader("X-Content-Type-Options", "nosniff")
                    .MinifyCss(cssSettings);
        }
    }
}
