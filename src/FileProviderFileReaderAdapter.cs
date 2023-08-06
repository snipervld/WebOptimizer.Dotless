using dotless.Core.Input;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace WebOptimizer.Dotless
{
    /// <summary>
    /// Represets an adapter class that adapts a <see cref="IFileProvider" /> to the <see cref="IFileReader" /> interface.
    /// </summary>
    /// <seealso cref="IFileReader" />
    public class FileProviderFileReaderAdapter : IFileReader
    {
        private readonly IFileProvider _fileProvider;

        /// <summary>
        /// Constructs adapter
        /// </summary>
        public FileProviderFileReaderAdapter(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        /// <inheritdoc />
        public bool UseCacheDependencies => false;

        /// <inheritdoc />
        public bool DoesFileExist(string fileName)
        {
            var fileInfo = _fileProvider.GetFileInfo(fileName);

            return fileInfo != null && fileInfo.Exists;
        }

        /// <inheritdoc />
        public byte[] GetBinaryFileContents(string fileName)
        {
            var fileInfo = _fileProvider.GetFileInfo(fileName);

            using (var s = fileInfo.CreateReadStream())
            using (var ms = new MemoryStream())
            {
                s.CopyTo(ms);

                return ms.ToArray();
            }
        }

        /// <inheritdoc />
        public string GetFileContents(string fileName)
        {
            var fileInfo = _fileProvider.GetFileInfo(fileName);

            using (var sr = new StreamReader(fileInfo.CreateReadStream()))
            {
                return sr.ReadToEnd();
            }
        }
    }
}

