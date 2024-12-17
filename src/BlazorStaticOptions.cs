﻿using Markdig;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace BlazorStatic;

using System.Net;

/// <summary>
///     Options for configuring the BlazorStatic generation process.
/// </summary>
public class BlazorStaticOptions
{
    private readonly List<Func<Task>> _beforeFilesGenerationActions = [];
    /// <summary>
    ///     Output folder for generated files. Relative to the project root.
    /// </summary>
    public string OutputFolderPath { get; set; } = "output";
    /// <summary>
    ///     Allows to suppress file generation. Useful for website building, when you don't need the static files.
    /// </summary>
    public bool SuppressFileGeneration { get; set; }

    /// <summary>
    ///     List of routes to generate as static html files.
    /// </summary>
    public List<PageToGenerate> PagesToGenerate { get; } = [];

    /// <summary>
    ///     Allows to add non-parametrized razor pages to the list of pages to generate.
    ///     Non-parametrized razor page: @page "/about"
    ///     Parametrized razor page: @page "/docs/{slug}"
    /// </summary>
    public bool AddPagesWithoutParameters { get; set; } = true;

    /// <summary>
    ///     Name of the page used for index. For example @page "/blog" will be generated to blog/index.html
    /// </summary>
    public string IndexPageHtml { get; set; } = "index.html";

    /// <summary>
    ///     If set to true will generate a `sitemap.xml` file and place it in the root of the output folder.<br />
    ///     The sitemap file follows the Google model:<br />
    ///     https://developers.google.com/search/docs/crawling-indexing/sitemaps/build-sitemap#xml
    /// </summary>
    public bool ShouldGenerateSitemap { get; set; } = false;

    /// <summary>
    ///     Hostname of your site. Needed to generate the sitemap. <br />
    ///     E.g. 'https://username.github.io'
    /// </summary>
    public string? SiteUrl { get; set; }

    /// <summary>
    ///     Specifies the output folder for the sitemap.xml file, relative to the project root.
    ///     This folder should be within a static web assets directory (e.g., wwwroot or any
    ///     folder configured with <c>app.UseStaticFiles</c>), ensuring it is available for the running web application.
    ///     Default value: <c>"wwwroot"</c>.
    /// </summary>
    public string SitemapOutputFolderPath { get; set; } = "wwwroot";


    /// <summary>
    ///     Paths (files or dirs) relative to new location of output folder, that shouldn't be copied to output folder
    ///     Example: Need to ignore wwwroot/app.css (because "tailwindiezd" app.min.css is used)
    ///     Content to copy is "wwwroot" to -> "" (root of output folder)
    ///     IgnoredPathsOnContentCopy is "app.css" (this would be the path in output folder)
    /// </summary>
    public List<string> IgnoredPathsOnContentCopy { get; } = [];
    /// <summary>
    ///     Paths (files or dirs) relative to project root, that should be copied to output folder.
    ///     Content from RCLs (from _content/) and wwwroot is copied by default
    /// </summary>
    public List<ContentToCopy> ContentToCopyToOutput { get; } = [];

    /// <summary>
    ///     Allows to customize YamlDotNet Deserializer used for parsing front matter
    /// </summary>
    public IDeserializer FrontMatterDeserializer { get; set; } = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    /// <summary>
    ///     Allows to customize Markdig MarkdownPipeline used for parsing markdown files
    /// </summary>
    public MarkdownPipeline MarkdownPipeline { get; set; } = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseYamlFrontMatter()
        .Build();


    /// <summary>
    ///     Hooks up to the hot reload event to re-generate the outputted pages. It also re-evaluates the options set up in
    ///     Program.cs.
    ///     <para>Works with:</para>
    ///     - Changes in Razor files, C# code, CSS, etc.
    ///     - Changes to .md files, if you set up the watch for them in .csproj, for example:
    ///     <para>&#160;</para>
    ///     &lt;ItemGroup&gt;
    ///     &lt;Watch Include="Content/**/*" /&gt;
    ///     &lt;/ItemGroup&gt;
    ///     <para>&#160;</para>
    ///     Note: Hot reload re-generation will clear the list of PagesToGenerate and ContentToCopyToOutput,
    ///     but these lists will be re-populated through the options.
    /// </summary>
    public bool HotReloadEnabled { get; set; }

    public object? RSSFeedTitle { get; set; }

    public object? RSSFeedDescription { get; set; }

    public string RSSFeedOutputFolderPath { get; set; }

    public bool ShouldGenerateRSS { get; set;  }


    /// <summary>
    ///     Iterator for optional actions.
    ///     Is called in GenerateStaticPages, after all other pages are added
    /// </summary>
    internal IEnumerable<Func<Task>> GetBeforeFilesGenerationActions() => _beforeFilesGenerationActions;


    /// <summary>
    ///     Adds an async Func to the list of actions to be executed before files are generated.
    /// </summary>
    /// <param name="func">The asynchronous function to add.</param>
    public void AddBeforeFilesGenerationAction(Func<Task> func) => _beforeFilesGenerationActions.Add(func);

    /// <summary>
    ///     Adds an action to the list of actions to be executed before files are generated.
    /// </summary>
    /// <param name="action">The synchronous action to add.</param>
    public void AddBeforeFilesGenerationAction(Action action) =>
        AddBeforeFilesGenerationAction(() => {
            action();
            return Task.CompletedTask;
        });


    /// <summary>
    ///     Clears the list. Useful for hot-reload, since we don't want the actions to be there multiple times
    /// </summary>
    internal void ClearBeforeFilesGenerationActions()
    {
        _beforeFilesGenerationActions.Clear();
    }
}

/// <summary>
///     Options for configuring processing of md files with front matter.
///     Default values are set to work with posts (<see cref="ContentPath" /> and <see cref="PageUrl" /> ) .
/// </summary>
/// <typeparam name="TFrontMatter"></typeparam>
public class BlazorStaticContentOptions<TFrontMatter>
    where TFrontMatter : class, IFrontMatter
{
    /// <summary>
    ///     folder relative to project root where posts are stored.
    ///     Don't forget to copy the content to bin folder (use CopyToOutputDirectory in .csproj),
    ///     because that's where the app will look for the files.
    ///     Default is Content/Blog where posts are stored.
    /// </summary>
    public string ContentPath { get; set; } = Path.Combine("Content", "Blog");
    /// <summary>
    ///     folder in ContentPath where media files are stored.
    ///     Important for app.UseStaticFiles targeting the correct folder.
    ///     Null in case of no media folder
    /// </summary>
    public string? MediaFolderRelativeToContentPath { get; set; } = "media";

    /// <summary>
    ///     URL path for media files for posts.
    ///     Used in app.UseStaticFiles to target the correct folder
    ///     and in ParseAndAddPosts to generate correct URLs for images
    ///     changes ![alt](media/image.png) to ![alt](Content/Blog/media/image.png
    ///     leading slash / is necessary for RequestPath in app.UseStaticFiles,
    ///     is removed in ParseAndAddPosts.
    ///     Null in case of no media.
    /// </summary>
    public string? MediaRequestPath => MediaFolderRelativeToContentPath is null
        ? null
        : Path.Combine(ContentPath, MediaFolderRelativeToContentPath).Replace(@"\", "/");

    /// <summary>
    ///     pattern for blog post files in ContentPath
    /// </summary>
    public string PostFilePattern { get; set; } = "*.md";
    /// <summary>
    ///     Place where processed blog posts live (their HTML and front matter)
    /// </summary>
    public List<Post<TFrontMatter>> Posts { get; } = [];
    /// <summary>
    ///     tag pages will be generated from all tags found in blog posts
    /// </summary>
    public bool AddTagPagesFromPosts { get; set; } = true;

    /// <summary>
    /// Func to convert tag string to file-name/url.
    /// You might want to change this if you don't like non-alphanumerical chars in your url (like tags/.net%2FC%23)
    /// The default is WebUtility.UrlEncode, which makes changes, like:
    /// "nice tag" -> "nice+tag", "ci/cd" -> "ci%2Fcd", ".net/C# " -> ".net%2FC%23".
    /// Also don't forget to use the same encoder while creating tag links
    /// </summary>
    public Func<string,string> TagEncodeFunc { get; set; } = WebUtility.UrlEncode;

    /// <summary>
    ///     Should correspond to page that keeps the list of content.
    ///     For example: @page "/blog" -> PageUrl="blog"
    ///     This also serves as a generated folder name for the content.
    ///     Useful for avoiding magic strings in .razor files
    ///     "blog" as default value
    /// </summary>
    public string PageUrl { get; set; } = "blog";
    /// <summary>
    ///     Should correspond to @page "/tags" (here in relative path: "tags")
    ///     Useful for avoiding magic strings in .razor files
    /// </summary>
    public string TagsPageUrl { get; set; } = "tags";

    /// <summary>
    ///     Action to run after content is parsed and added to the collection.
    ///     Useful for editing data in the posts, such as changing image paths.
    /// </summary>
    public Action? AfterContentParsedAndAddedAction { get; set; }

    /// <summary>
    ///     Obsolete property. Use <see cref="AfterContentParsedAndAddedAction" /> instead. This property will be removed in
    ///     future versions.
    /// </summary>
    [Obsolete("Use AfterContentParsedAction instead. This property will be removed in future versions.")]
    public Action? AfterBlogParsedAndAddedAction
    {
        get => AfterContentParsedAndAddedAction;
        set => AfterContentParsedAndAddedAction = value;
    }
}
