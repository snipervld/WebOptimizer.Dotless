# A LESS compiler for ASP.NET Core using dotless

[![NuGet](https://img.shields.io/nuget/v/snipervld.WebOptimizer.Dotless.svg)](https://nuget.org/packages/snipervld.WebOptimizer.Dotless/)

This package compiles LESS files into CSS by hooking into the [LigerShark.WebOptimizer](https://github.com/ligershark/WebOptimizer) pipeline.

## Install
Add the NuGet package [snipervld.WebOptimizer.Dotless](https://nuget.org/packages/snipervld.WebOptimizer.Dotless/) to any ASP.NET Core project supporting .NET 6.0 or higher.

> &gt; dotnet add package snipervld.WebOptimizer.Dotless

### Versions
Version|Support
-|-
&gt;= 3.x|ASP.NET Core 6.0 and above


## Usage
Here's an example of how to compile `a.less` and `b.less` from inside the wwwroot folder and bundle them into a single .css file called `/all.css`:

In **Startup.cs**, modify the *ConfigureServices* method:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc();
    services.AddWebOptimizer(pipeline =>
    {
        pipeline.AddLessBundle("/all.css", "a.less", "b.less");
    });
}
```

...and add `app.UseWebOptimizer()` to the `Configure` method anywhere before `app.UseStaticFiles`, like so:

```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseWebOptimizer();

    app.UseStaticFiles();
    app.UseMvc(routes =>
    {
        routes.MapRoute(
            name: "default",
            template: "{controller=Home}/{action=Index}/{id?}");
    });
}
```

Now the path *`http://domain/all.css`* will return a compiled, bundled and minified CSS document based on the two source files.

You can also reference any .less files directly in the browser (*`http://domain/a.less`*) and a compiled and minified CSS document will be served. To set that up, do this:

```csharp
services.AddWebOptimizer(pipeline =>
{
    pipeline.CompileLessFiles();
});
```

Or if you just want to limit what .less files will be compiled, do this:

```csharp
services.AddWebOptimizer(pipeline =>
{
    pipeline.CompileLessFiles("/path/file1.less", "/path/file2.less");
});
```

## Setup TagHelpers
In `_ViewImports.cshtml` register the TagHelpers by adding `@addTagHelper *, WebOptimizer.Core` to the file. It may look something like this:

```text
@addTagHelper *, WebOptimizer.Core
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

## Differences From [twenzel's WebOptimizer.Dotless](https://github.com/twenzel/WebOptimizer.Dotless)
- Added an ability to pass css settings to minificator.
- It's now possible to include regular `.css` files into `.less` bundles. `.css` files are not processed by `LessEngine`.
- `LessEngine`s imports are reset between each `.less` file compilation, when there are multiple `.less` files per bundle. It fixes an issue, when multiple files with import statements are located in different directories, but due to `LessEngine`'s imports caching mechanism, `LessEngine` handles them incorrectly, e.g. if two `.less` files `dir/a.less` and `b.less` imports the same `root.less`, `LessEngine` tries to get content of `root.less` from incorrect location, when transforning `b.less`.
- Added a support for abstract `IFileProvider`s, e.g. `ManifestEmbeddedFileProvider`.
- Use custom `dotless.Core` compiler, which fixes bug in `ResetImports` method, whech prevents the user to compile multiple `.less` files in the same directory, which reference imports the same file.
- Drop support for old frameworks. Add support for .NET 7.0 and .NET 8.0.
