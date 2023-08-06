﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace WebOptimizer.Dotless.Test
{
    public class CompilerTest
    {
        [Fact]
        public async Task Compile_Success()
        {
            var processor = new Compiler();
            var context = new Mock<IAssetContext>().SetupAllProperties();
            var asset = new Mock<IAsset>().SetupAllProperties();
            var env = new Mock<IWebHostEnvironment>();
            var fileProvider = new Mock<IFileProvider>();

            var inputFile = new PhysicalFileInfo(new FileInfo("foo.less"));

            context.Object.Content = new Dictionary<string, byte[]> {
                { "/file.less", "@foo: 1px; * {margin: @foo}".AsByteArray() },
            };

            context.Setup(s => s.HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment)))
                  .Returns(env.Object);

            context.SetupGet(s => s.Asset)
                        .Returns(asset.Object);

            env.SetupGet(e => e.WebRootFileProvider)
                 .Returns(fileProvider.Object);

            fileProvider.Setup(f => f.GetFileInfo(It.IsAny<string>()))
                   .Returns(inputFile);

            await processor.ExecuteAsync(context.Object);
            var result = context.Object.Content.First().Value;

            Assert.Equal("* {\n  margin: 1px;\n}", result.AsString().Trim());
        }

        [Fact]
        public async Task Compile_With_Imports_Success()
        {
            var processor = new Compiler();
            var context = new Mock<IAssetContext>().SetupAllProperties();
            var asset = new Mock<IAsset>().SetupAllProperties();
            var env = new Mock<IWebHostEnvironment>();
            var fileProvider = new Mock<IFileProvider>();

            string temp = Path.GetTempPath();
            var path = Path.Combine(temp, "bar.less");
            File.WriteAllText(path, ".test{ .active {color:red; }}");

            var inputFile = new PhysicalFileInfo(new FileInfo(path));

            context.Object.Content = new Dictionary<string, byte[]> {
                { "/file.less", "@import \"bar\"; @foo: 1px; * {margin: @foo}".AsByteArray() }
            };

            context.Setup(s => s.HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment)))
                  .Returns(env.Object);

            context.SetupGet(s => s.Asset)
                        .Returns(asset.Object);

            env.SetupGet(e => e.WebRootFileProvider)
                 .Returns(fileProvider.Object);

            fileProvider.Setup(f => f.GetFileInfo(It.IsAny<string>()))
                   .Returns(inputFile);

            await processor.ExecuteAsync(context.Object);
            var result = context.Object.Content.First().Value;

            Assert.Equal(".test .active {\n  color: red;\n}\n* {\n  margin: 1px;\n}", result.AsString().Trim());
        }

        [Fact]
        public async Task Compile_With_Imports_From_Non_Physical_File_Provider_Success()
        {
            var processor = new Compiler();
            var context = new Mock<IAssetContext>().SetupAllProperties();
            var asset = new Mock<IAsset>().SetupAllProperties();
            var env = new Mock<IWebHostEnvironment>();

            var fooFileInfo = new Mock<IFileInfo>().SetupAllProperties();
            fooFileInfo.SetupGet(x => x.Name).Returns("foo.less");
            fooFileInfo.SetupGet(x => x.Exists).Returns(true);
            fooFileInfo.SetupGet(x => x.IsDirectory).Returns(false);
            fooFileInfo.Setup(x => x.CreateReadStream())
                .Returns(() => GenerateStreamFromString("@import \"bar\"; @foo: 1px; * {margin: @foo}"));

            var barFileInfo = new Mock<IFileInfo>().SetupAllProperties();
            barFileInfo.SetupGet(x => x.Name).Returns("bar.less");
            barFileInfo.SetupGet(x => x.Exists).Returns(true);
            barFileInfo.SetupGet(x => x.IsDirectory).Returns(false);
            barFileInfo.Setup(x => x.CreateReadStream())
                .Returns(() => GenerateStreamFromString(".test{ .active {color:red; }}"));

            var fileProvider = new Mock<IFileProvider>();
            fileProvider.Setup(f => f.GetFileInfo(It.Is<string>(str => str.Contains("foo"))))
                .Returns(fooFileInfo.Object);
            fileProvider.Setup(f => f.GetFileInfo(It.Is<string>(str => str.Contains("bar"))))
                .Returns(barFileInfo.Object);

            context.Object.Content = new Dictionary<string, byte[]> {
                { "/file.less", "@import \"foo\";".AsByteArray() }
            };

            context.Setup(s => s.HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment)))
                .Returns(env.Object);

            context.SetupGet(s => s.Asset)
                .Returns(asset.Object);

            env.SetupGet(e => e.WebRootFileProvider)
                .Returns(fileProvider.Object);

            await processor.ExecuteAsync(context.Object);
            var result = context.Object.Content.First().Value;

            Assert.Equal(".test .active {\n  color: red;\n}\n* {\n  margin: 1px;\n}", result.AsString().Trim());
        }

        private static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;

            return stream;
        }
    }
}
