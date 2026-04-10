using System.Globalization;
using System.Text.Json;
using Shine.Media.Entities;
using UsersIconUploader.Infrastructure;
using UsersIconUploader.Services;

static void PrintUsage()
{
    Console.Error.WriteLine(
        """
        Usage:
          UsersIconUploader --folder <path> --output <path> [--media-api-base <url>] [--category <name>]

        Scans the folder for image files (non-recursive), calls Shine Media CreateMedia for each file,
        uploads each file to the returned URL (HTTP PUT), then writes a JSON array of media ids.

        Options:
          --folder, -f           Directory containing images to upload
          --output, -o           Output folder; writes uploaded_icons_<timestamp>.json there
          --media-api-base, -u   Shine Media API base URL (default: http://localhost:5210; or SHINE_MEDIA_API_BASE)
          --category, -c         Media category for CreateMedia (default: user-profile-image; or SHINE_MEDIA_CATEGORY)
          --media-type           Optional: Image | Video | Audio (default: Image)

        Environment:
          SHINE_MEDIA_API_BASE — overrides default media API URL when set
          SHINE_MEDIA_CATEGORY — overrides default category when set
        """);
}

static string? Env(string name) =>
    Environment.GetEnvironmentVariable(name) is { Length: > 0 } v ? v : null;

var argsList = args.ToList();
string? folder = null;
string? outputFolder = null;
string? mediaApiBase = Env("SHINE_MEDIA_API_BASE");
string? category = Env("SHINE_MEDIA_CATEGORY");
var mediaType = MediaType.Image;

for (var i = 0; i < argsList.Count; i++)
{
    var a = argsList[i];
    if (a is "-h" or "--help")
    {
        PrintUsage();
        return 0;
    }

    string TakeValue()
    {
        if (i + 1 >= argsList.Count)
            throw new ArgumentException($"Missing value after {a}");
        return argsList[++i];
    }

    switch (a)
    {
        case "--folder":
        case "-f":
            folder = TakeValue();
            break;
        case "--output":
        case "-o":
            outputFolder = TakeValue();
            break;
        case "--media-api-base":
        case "-u":
            mediaApiBase = TakeValue();
            break;
        case "--category":
        case "-c":
            category = TakeValue();
            break;
        case "--media-type":
            mediaType = Enum.Parse<MediaType>(TakeValue(), ignoreCase: true);
            break;
        default:
            throw new ArgumentException($"Unknown argument: {a}");
    }
}

if (folder is null || outputFolder is null)
{
    PrintUsage();
    return 1;
}

outputFolder = outputFolder.Trim();

const string defaultMediaApiBase = "http://localhost:5210";
mediaApiBase = string.IsNullOrWhiteSpace(mediaApiBase) ? defaultMediaApiBase : mediaApiBase.Trim();

const string defaultCategory = "user-profile-image";
category = string.IsNullOrWhiteSpace(category) ? defaultCategory : category.Trim();

var mediaClient = MediaClientFactory.Create(mediaApiBase);
var jobsClient = new ShineMediaImageProcessingJobsClient(mediaClient, category, mediaType);

var orchestrator = new AssetUploadOrchestrator(jobsClient, new PlaceholderAssetUploader());

var jobIds = await orchestrator.UploadFolderAndCollectJobIdsAsync(folder, CancellationToken.None)
    .ConfigureAwait(false);

Directory.CreateDirectory(outputFolder);
var stamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture);
var outputFilePath = Path.Combine(outputFolder, $"uploaded_icons_{stamp}.json");

var json = JsonSerializer.Serialize(jobIds, new JsonSerializerOptions { WriteIndented = true });
await File.WriteAllTextAsync(outputFilePath, json).ConfigureAwait(false);

Console.WriteLine($"Wrote {jobIds.Count} job id(s) to {outputFilePath}");
return 0;
