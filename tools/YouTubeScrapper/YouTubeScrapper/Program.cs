using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

ConfigureServices();

IReadOnlyCollection<ChannelProcessor> processors = Utility.GetConfiguration()
                                                          .ChannelProcessors
                                                          .Select(
                                                              static x => Activator.CreateInstance(
                                                                  Type.GetType(x.ProcessorName)
                                                                      .OrThrow(),
                                                                  [x.ChannelId]) as ChannelProcessor
                                                          )
                                                          .Where(static x => x is not null)
                                                          .OfType<ChannelProcessor>()
                                                          .ToList();

IReadOnlyCollection<string> channelsList = Utility.GetConfiguration()
                                                  .Channels;

CancellationToken cancellationToken = default;

YouTubeService ytService = await LoginToYouTubeAsync(cancellationToken);

IReadOnlyCollection<Channel> existingChannels = await ExecuteUntilAllAsync<Channel, Channel>(
    new ChannelsListRequestWrapper(ytService.Channels.List("id,snippet"), channelsList),
    NoParse,
    cancellationToken);

if (!existingChannels.Any())
{
    Services.Logger.LogWarning("No channels");

    return;
}

bool dumpVideos = Utility.GetConfiguration()
                         .DumpVideos;

foreach (Channel channel in existingChannels)
{
    IReadOnlyCollection<YouTubeVideoInfo> ytVideos = Utility.GetConfiguration()
                                                            .UseLocalData
            ? await GetVideosFromFileAsync(cancellationToken)
            : await GetVideosFromWebAsync(ytService, channel, cancellationToken);

    if (dumpVideos)
    {
        await File.WriteAllTextAsync($"{channel.Id}.json", JsonConvert.SerializeObject(ytVideos));
    }

    foreach (ChannelProcessor processor in processors)
    {
        IReadOnlyCollection<YouTubeVideoInfo> processed = processor.Process(ytVideos);

        string channelTable = CreateVideosTable(processed);

        await File.WriteAllTextAsync($"{channel.Id}.md", channelTable, cancellationToken);
    }
}

return;

static string CreateVideosTable(IReadOnlyCollection<YouTubeVideoInfo> videos)
{
    if (videos.Count == 0)
    {
        return string.Empty;
    }

    StringBuilder sb = new ();

    sb.AppendLine("---")
      .AppendLine("layout: page")
      .AppendLine("---")
      .AppendLine("")
      .AppendLine($"Link: [{videos.First().ChannelName}]({videos.First().ChannelUrl})")
      .AppendLine();

    string[] headers = ["Index", "Publish Date", "Title", "Description", "Tags", "Link"];
    string headersLine = headers.Join(" | ");

    sb.AppendLine($"| {headersLine} |");
    sb.AppendLine($"| {headers.Select(static x => new string('-', x.Length)).Join(" | ")} |");

    videos.ForEach(
        x => sb.AppendLine(
            $"| {new[] {
                x.Index.ToString(),
                x.PublishDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                x.VideoTitle,
                x.Description,
                x.Tags.Join(),
                $"[Click]({x.VideoUrl})" }.Join(" | ")} |"));

    return sb.ToString();
}

static async Task<YouTubeVideoInfo?> ToVideoInfo(YouTubeService ytService, Channel ytChannel, SearchResult ytVideo, CancellationToken cancellationToken = default)
{
    try
    {
        VideosResource.ListRequest videoRequest = ytService.Videos.List("snippet,contentDetails");
        videoRequest.Id = ytVideo.Id.VideoId;

        if (!Equals(ytVideo.Id.Kind, "youtube#video"))
        {
            switch (ytVideo.Id.Kind)
            {
                case "youtube#playlist":
                case "youtube#channel":
                    return null;

                default:
                    return null;
            }
        }

        VideoListResponse ytVideoDetails = await videoRequest.ExecuteAsync(cancellationToken);

        return new ()
        {
            ChannelId = ytChannel.Id,
            ChannelUrl = $"https://www.youtube.com/channel/{ytChannel.Id}",
            ChannelName = ytChannel.Snippet.Title,
            Index = 0,
            PublishDate = ytVideo.Snippet.PublishedAtDateTimeOffset,
            VideoUrl = $"https://www.youtube.com/watch?v={ytVideo.Id.VideoId}",
            VideoTitle = ytVideo.Snippet.Title,
            Description = ytVideoDetails.Items
                                        .First()
                                        .Snippet
                                        .Description,
            Category = string.Empty,
            Tags = []
        };
    }
    catch (Exception ex)
    {
        Services.Logger.LogError(ex, "Failed to parse item into Video");

        return null;
    }
}

static void ConfigureServices()
{
    ServiceCollection serviceCollection = new ();

    serviceCollection.AddLogging(
        static configure =>
        {
            configure.AddConsole();
            configure.SetMinimumLevel(LogLevel.Information);
        });

    ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

    Services.Logger = serviceProvider.GetService<ILogger<Program>>()
                                     .OrThrow();

    Services.Configuration = new ConfigurationBuilder()
                             .SetBasePath(AppContext.BaseDirectory)
                             .AddJsonFile("appsettings.json")
                             .Build();
}

static async Task<IReadOnlyCollection<TResult>> ExecuteUntilAllAsync<TModel, TResult>(
    ListRequestWrapper request,
    Func<TModel, Task<TResult?>> parse,
    CancellationToken cancellationToken = default)
{
    List<TResult> result = [];
    ListResponseWrapper? response = null;

    do
    {
        request.PageToken = response?.NextPageToken;

        response = await request.ExecuteAsync(cancellationToken);

        foreach (TModel item in response.GetItems<TModel>())
        {
            TResult? parsedItem = await parse(item);

            if (!Equals(parsedItem, default))
            {
                result.Add(parsedItem);
            }
        }
    }
    while (!string.IsNullOrWhiteSpace(response.NextPageToken));

    return result;
}

static async Task<YouTubeService> LoginToYouTubeAsync(CancellationToken cancellationToken = default)
{
    Services.Logger.LogDebug("Creating YT credentials");

    AppConfiguration config = Utility.GetConfiguration();

    string secretsFilePath = config.ClientSecretsFilePath;

    if (!File.Exists(secretsFilePath))
    {
        throw new InvalidOperationException("YouTube secrets file path does not exist. Set proper path to file.");
    }

    await using FileStream stream = new (secretsFilePath, FileMode.Open, FileAccess.Read);

    try
    {
        GoogleClientSecrets secrets = await GoogleClientSecrets.FromStreamAsync(stream, cancellationToken);

        UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            secrets.Secrets,
            [YouTubeService.Scope.Youtube],
            config.YouTubeUser,
            cancellationToken
        );

        return new (
            new ()
            {
                HttpClientInitializer = credential,
                ApplicationName = config.ApplicationName
            });
    }
    catch (Exception ex)
    {
        Services.Logger.LogError(ex, "Failed to login");

        throw;
    }
}

static Task<TModel?> NoParse<TModel>(TModel model)
{
    return Task.FromResult<TModel?>(model);
}

static async Task<IReadOnlyCollection<YouTubeVideoInfo>> GetVideosFromWebAsync(YouTubeService ytService, Channel channel, CancellationToken cancellationToken = default)
{
    return (await ExecuteUntilAllAsync<SearchResult, YouTubeVideoInfo>(
               new VideosListRequestWrapper(ytService.Search.List("id,snippet"), channel.Id),
               x => ToVideoInfo(ytService, channel, x, cancellationToken),
               cancellationToken))
           .OrderBy(static x => x.PublishDate ?? DateTime.MinValue)
           .ToList();
}

static async Task<IReadOnlyCollection<YouTubeVideoInfo>> GetVideosFromFileAsync(CancellationToken cancellationToken = default)
{
    string json = await File.ReadAllTextAsync(
        Utility.GetConfiguration()
               .LocalDataPath,
        cancellationToken);

    return JsonConvert.DeserializeObject<IReadOnlyCollection<YouTubeVideoInfo>>(json) ?? Array.Empty<YouTubeVideoInfo>();
}

public class YouTubeVideoInfo
{
    public required string ChannelId { get; init; }

    public required string ChannelUrl { get; init; }

    // ReSharper disable once MemberCanBeInternal
    public string? ChannelName { get; init; }

    public required int Index { get; init; }

    public required DateTimeOffset? PublishDate { get; init; }

    public required string VideoUrl { get; init; }

    public required string VideoTitle { get; init; }

    public required string Description { get; init; }

    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public required string Category { get; init; }

    public required string[] Tags { get; init; }
}

file static class Services
{
    // ReSharper disable once NullableWarningSuppressionIsUsed
    public static IConfiguration Configuration { get; set; } = null!;

    // ReSharper disable once NullableWarningSuppressionIsUsed
    public static ILogger Logger { get; set; } = null!;
}

file static class Utility
{
    public static AppConfiguration GetConfiguration()
    {
        return Services.Configuration
                       .GetSection(nameof(AppConfiguration))
                       .Get<AppConfiguration>()
                       .OrThrow();
    }
}

file static class Extensions
{
    public static TValue OrThrow<TValue>(this TValue? value, string? message = null)
    {
        return value ?? throw new ArgumentNullException(message);
    }

    public static void ForEach<TValue>(this IEnumerable<TValue> enumerable, Action<TValue> action)
    {
        foreach (TValue item in enumerable)
        {
            action(item);
        }
    }

    public static string Join(this IEnumerable<string> parts, string separator = ", ")
    {
        return string.Join(separator, parts);
    }
}

file class AppConfiguration
{
    public required bool DumpVideos { get; init; }

    public required bool UseLocalData { get; init; }

    public required string LocalDataPath { get; init; }

    public required string ClientSecretsFilePath { get; init; }

    public required string YouTubeUser { get; init; }

    public required string ApplicationName { get; init; }

    public required IReadOnlyCollection<string> Channels { get; init; }

    public required IReadOnlyCollection<ChannelProcessorMapping> ChannelProcessors { get; init; }
}

// ReSharper disable once ClassNeverInstantiated.Local
file class ChannelProcessorMapping
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    public required string ProcessorName { get; init; }

    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    public required string ChannelId { get; init; }
}

file abstract class ListRequestWrapper
{
    protected const long MaxNumberOfYtItems = 100;

    // ReSharper disable once UnusedMemberInSuper.Global
    // ReSharper disable once UnusedMember.Global
    public abstract long? MaxResults { get; set; }

    // ReSharper disable once UnusedMemberInSuper.Global
    // ReSharper disable once UnusedMember.Global
    public abstract string? PageToken { get; set; }

    public abstract Task<ListResponseWrapper> ExecuteAsync(CancellationToken cancellationToken = default);
}

file class VideosListRequestWrapper
        : ListRequestWrapper
{
    public VideosListRequestWrapper(SearchResource.ListRequest request, string channelId)
    {
        Request = request;

        request.ChannelId = channelId;
    }

    /// <inheritdoc />
    public override long? MaxResults
    {
        get { return Request.MaxResults; }
        set { Request.MaxResults = value; }
    }

    /// <inheritdoc />
    public override string? PageToken
    {
        get { return Request.PageToken; }
        set { Request.PageToken = value; }
    }

    private SearchResource.ListRequest Request { get; }

    /// <inheritdoc />
    public override async Task<ListResponseWrapper> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        MaxResults = MaxNumberOfYtItems;

        Request.Order = SearchResource.ListRequest.OrderEnum.Date;

        SearchListResponse? result = await Request.ExecuteAsync(cancellationToken);

        return new (
            new ReadOnlyCollection<object>(
                result.Items
                      .OfType<object>()
                      .ToList()),
            result.NextPageToken);
    }
}

file sealed class ChannelsListRequestWrapper
        : ListRequestWrapper
{
    public ChannelsListRequestWrapper(ChannelsResource.ListRequest request, IReadOnlyCollection<string> channelsList)
    {
        Request = request;

        MaxResults = MaxNumberOfYtItems;

        request.Id = new (channelsList);
    }

    /// <inheritdoc />
    public override long? MaxResults
    {
        get { return Request.MaxResults; }
        set { Request.MaxResults = value; }
    }

    /// <inheritdoc />
    public override string? PageToken
    {
        get { return Request.PageToken; }
        set { Request.PageToken = value; }
    }

    private ChannelsResource.ListRequest Request { get; }

    /// <inheritdoc />
    public override async Task<ListResponseWrapper> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        ChannelListResponse result = await Request.ExecuteAsync(cancellationToken);

        return new (
            new ReadOnlyCollection<object>(
                result.Items
                      .OfType<object>()
                      .ToList()),
            result.NextPageToken);
    }
}

file class ListResponseWrapper()
{
    public ListResponseWrapper(IReadOnlyCollection<object> items, string? nextPageToken)
            : this()
    {
        NextPageToken = nextPageToken;
        Items = items;
    }

    public string? NextPageToken { get; }

    private IReadOnlyCollection<object>? Items { get; }

    public IReadOnlyCollection<TModel> GetItems<TModel>()
    {
        return (Items?.OfType<TModel>() ?? []).ToList();
    }
}

public abstract class ChannelProcessor(string channelId)
{
    private string ChannelId { get; } = channelId;

    internal bool CanProcess(YouTubeVideoInfo video)
    {
        return video.ChannelId == ChannelId;
    }

    internal abstract IReadOnlyCollection<YouTubeVideoInfo> Process(IReadOnlyCollection<YouTubeVideoInfo> videos);
}

// ReSharper disable once UnusedType.Global
public class NickChapasChannelProcessor
        : ChannelProcessor
{
    private static readonly IReadOnlyCollection<string> ForbiddenStarts =
    [
        "Use code", "Become a Patreon", "Workshops:", "Don't forget to comment, like", "Social Media:", "Follow me", "Connect on", "Keep coding",
        "Check out my courses",
        "Check out my workshops",
        "NDC Oslo",
        "NDC Minessota",
        "NDC Syndey",
        "Check out the",
        "Subscrib",
        "This video was sponsored",
        "https://",
        "http://"
    ];

    private static readonly IReadOnlyCollection<string> ForbiddenEnds =
    [
        ":"
    ];

    private static readonly IReadOnlyCollection<string> ForbiddenContains =
    [
        "a star on GitHub", "Give CSharpRepl a star", "github.com",
        "microsoft.com",
        "blog post:",
        "youtube.com",
        "geni.us",
        "specflow.org",
        "msdn.com",
        "sharplab.io",
        "amazonaws.com",
        "jetbrains.com",
        "medium.com",
        "__makeref",
        "a star:",
        "stackoverflow.com",
        "hanselman.com",
        "dusted.codes",
        "hashicorp.com",
        "youtu.be",
        "ohmyposh.dev",
        "andrewlock.net",
        "bit.ly",
        "reddit.com",
        "meziantou.net",
        "nindalf.com",
        "tiobe.com",
        "Social media:",
        "migeel.sk",
        "twitter.com",
        "journal.stuffwithstuff.com",
        "Announcement Blog:",
        "github.blog",
        "Links to what's being discussed:.",
        "This video is sponsored by abp.io.",
        "To get $25 worth of free AWS credits email dotnet-on-aws-feedback[at]amazon[dot]com with the subject line \"AWS CREDIT NICK CHAPSAS\".",
        "This video is sponsored by AWS",
        "atlassian.com",
        "Related videos",
        "Links to the REST API series",
        "is sponsored",
        "https://",
        "Workshops.",
        "Workshops",
        "Workshops.",
        "Workshops",
        "TBD",
        "k6.io",
        "Links to the REST API series:.",
        "This is only a fraction of what I show in my Integration Testing course so if you want to learn even more cool topics like this check https://nickchapsas.com out."
    ];

    private static readonly IReadOnlyCollection<string> RemovePhrases =
    [
        "Hello everybody, I'm Nick, and in this video, ",
        "Hello, everybody, I'm Nick, and in this video, ",
        "Hello everybody I'm Nick, and in this video, ",
        "Hello everybody, I'm Nick, and ",
        "Hello, everybody, I'm Nick, and in this video",
        "Hello, everybody, I'm Nick, and in this video",
        "Hello everybody I'm Nick and ",
        "Hello, everybody, I'm Nick, and ",
        "Hello, everybody. I'm Nick, and in this video, ",
        "Hello everybody I'm Nick and "
    ];

    /// <inheritdoc />
    public NickChapasChannelProcessor(string channelId)
            : base(channelId)
    {
    }

    /// <inheritdoc />
    internal override IReadOnlyCollection<YouTubeVideoInfo> Process(IReadOnlyCollection<YouTubeVideoInfo> videos)
    {
        List<YouTubeVideoInfo> result = new ();

        int idx = 0;

        foreach (YouTubeVideoInfo video in videos.Where(CanProcess))
        {
            List<string> descriptionLines = (video.Description)
                                            .Split(["\r\n", "\n"], StringSplitOptions.None)
                                            .Where(static x => ForbiddenStarts.All(y => !x.StartsWith(y, StringComparison.Ordinal)))
                                            .Where(static x => ForbiddenContains.All(y => !x.Contains(y)))
                                            .Where(static x => ForbiddenEnds.All(y => !x.EndsWith(y, StringComparison.Ordinal)))
                                            .Select(static x => x.Trim())
                                            .Where(static x => !string.IsNullOrWhiteSpace(x))
                                            .ToList();

            descriptionLines = SkipUntilHello(descriptionLines, "Hello everybody");
            descriptionLines = SkipUntilHello(descriptionLines, "Hello, everybody");
            descriptionLines = SkipUntilHello(descriptionLines, "Hello, everybody");

            string[] tags = video.Tags;

            try
            {
                if (descriptionLines.Count > 0
                    && descriptionLines.Last()
                                       .StartsWith("#", StringComparison.Ordinal))
                {
                    tags = descriptionLines.Last()
                                           .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                           .Select(static x => x.Replace("#", string.Empty))
                                           .ToArray();

                    descriptionLines.RemoveAt(descriptionLines.Count - 1);
                }

                if (tags.Length == 0)
                {
                    int shortsIdx = descriptionLines.FindIndex(static x => x.Contains(" #Shorts"));

                    if (idx >= 0)
                    {
                        descriptionLines[shortsIdx] = descriptionLines[shortsIdx]
                                .Replace(" #Shorts", ".");

                        tags = ["#shorts"];
                    }
                }
            }
            catch
            {
                // Ignore
            }

            descriptionLines = TryRemoveTimeStamps(descriptionLines, "timestamps");
            descriptionLines = TryRemoveTimeStamps(descriptionLines, "Timestamps");
            descriptionLines = TryRemoveTimeStamps(descriptionLines, "Timestamp");

            MoveSourceCodeToEnd(descriptionLines, "Get the source code");
            MoveSourceCodeToEnd(descriptionLines, "Source code: http");

            descriptionLines = descriptionLines.Select(static x => x.Trim())
                                               .Where(static x => !string.IsNullOrWhiteSpace(x))
                                               .ToList();

            string description = string.Join("<br/>", descriptionLines.Where(static x => !string.IsNullOrWhiteSpace(x)));
            description = RemovePhrases.Aggregate(description, Replace);

            if (description.Length > 0)
            {
                description = description.Substring(0, 1)
                                         .ToUpper()
                              + description.Substring(1);
            }

            if (description.Length > 0 && !description.EndsWith('.'))
            {
                description += ".";
            }

            description = description.Replace("result.s.", "results.")
                                     .Replace("of Code Cop I will take a look at a newsletter post", "In this video of Code Cop I will take a look at a newsletter post");

            if (!(tags.Length > 0) && video.Description.Contains("#") && !video.Description.Contains("C#"))
            {
                Services.Logger.LogWarning("It seems the tags were not parsed correctly.");
            }

            YouTubeVideoInfo newInfo = new ()
            {
                ChannelId = video.ChannelId,
                ChannelUrl = video.ChannelUrl,
                ChannelName = video.ChannelName,
                Index = ++idx,
                PublishDate = video.PublishDate,
                VideoUrl = video.VideoUrl,
                VideoTitle = video.VideoTitle.Replace("|", "\\|"),
                Description = description.Replace("|", "\\|")
                                         .Replace(":.", ".")
                                         .Replace("!.", "!"),
                Category = string.Empty,
                Tags = tags
            };

            result.Add(newInfo);
        }

        return result;
    }

    private static List<string> SkipUntilHello(List<string> lines, string keyword)
    {
        List<string> afterSkip = lines.SkipWhile(x => !x.StartsWith(keyword, StringComparison.Ordinal))
                                      .ToList();

        return afterSkip.Any()
                ? afterSkip
                : lines;
    }

    private static string Replace(string content, string text)
    {
        return content.Replace(text, string.Empty);
    }

    private static void MoveSourceCodeToEnd(List<string> lines, string keyword)
    {
        if (!lines.Any(x => x.Contains(keyword)))
        {
            return;
        }

        int idx = lines.FindIndex(x => x.Contains(keyword));

        string sourceCode = lines[idx];

        lines.RemoveAt(idx);

        lines.Add(sourceCode);
    }

    private static List<string> TryRemoveTimeStamps(List<string> descriptionLines, string keyword)
    {
        Regex tsRegex = new ("(\\d+\\:\\d+ .*)|(\\d+\\:\\d+ - .*)|(.* - \\d+\\:\\d+)|(.* \\d+\\:\\d+)", RegexOptions.Compiled);

        int tsIdx = descriptionLines.FindIndex(x => x.Contains(keyword, StringComparison.InvariantCultureIgnoreCase));

        int startIdx;

        if (tsIdx < 0)
        {
            int potentialIdx = descriptionLines.FindIndex(tsRegex.IsMatch);

            if (potentialIdx < 0)
            {
                return descriptionLines;
            }

            tsIdx = potentialIdx;
            startIdx = tsIdx;
        }
        else
        {
            startIdx = tsIdx++;
        }

        while (tsIdx < descriptionLines.Count && tsRegex.IsMatch(descriptionLines[tsIdx]))
        {
            tsIdx++;
        }

        tsIdx--;

        return descriptionLines.Select(
                                   (x, i) => i >= startIdx && i <= tsIdx
                                           ? string.Empty
                                           : x)
                               .Where(static x => !string.IsNullOrEmpty(x))
                               .ToList();
    }
}