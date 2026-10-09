using System.Net;
using System.Net.Http;
using System.Security.Cryptography;

namespace Composa.App;

/// <summary>How much of a download has arrived, and of how much when the server said.</summary>
public readonly record struct DownloadProgress(long Received, long? Total);

public enum DownloadOutcome { Downloaded, Reused, Cancelled, Failed }

/// <param name="Path">Where the checked file is, when there is one.</param>
/// <param name="Problem">What went wrong, in words for the person, when the download failed.</param>
public sealed record DownloadResult(DownloadOutcome Outcome, string? Path = null, string? Problem = null)
{
    public bool Succeeded => Outcome is DownloadOutcome.Downloaded or DownloadOutcome.Reused;
}

/// <summary>
/// Fetches one file of a release into a folder, only ever because the person pressed Download. The
/// file is written beside its final name as <c>.part</c> and takes that name only once its SHA-256
/// matches the release's <c>sha256sums.txt</c>; anything else, a cancel included, leaves nothing
/// behind. It never runs or installs what it fetched.
/// </summary>
/// <param name="handler">Stands in for the network in tests; null uses a real connection.</param>
/// <param name="stallAfter">How long the connection may go without delivering anything; <see cref="StallTimeout"/> by default.</param>
public sealed class UpdateDownload(HttpMessageHandler? handler = null, TimeSpan? stallAfter = null)
{
    /// <summary>
    /// There is no limit on the whole download, which is 30 to 60 MB and may take minutes on a slow
    /// line; a connection that delivers nothing for this long, though, is not coming back.
    /// </summary>
    public static readonly TimeSpan StallTimeout = TimeSpan.FromSeconds(30);

    private readonly TimeSpan stallAfter = stallAfter ?? StallTimeout;

    public async Task<DownloadResult> Run(ReleaseInfo release, ReleaseAsset asset, string folder, IProgress<DownloadProgress>? progress, CancellationToken cancel)
    {
        if (!ReleaseAsset.IsOwnAsset(asset.Url))
            return Failed(L10n.F("{0} does not come from Composa's own release page, so it was not downloaded.", asset.Name));
        if (release.Checksums is not { } sums || !ReleaseAsset.IsOwnAsset(sums.Url))
            return Failed(L10n.T("This release has no list of checksums, so a download could not be checked. Download it from the release page instead."));

        using var client = handler == null ? new HttpClient() : new HttpClient(handler, disposeHandler: false);
        client.Timeout = Timeout.InfiniteTimeSpan; // Stalls are caught instead; see StallTimeout.
        client.DefaultRequestHeaders.Add("User-Agent", GitHubReleaseSource.UserAgent);

        using var stall = new CancellationTokenSource();
        using var either = CancellationTokenSource.CreateLinkedTokenSource(cancel, stall.Token);
        string? part = null;
        try
        {
            stall.CancelAfter(stallAfter);
            var listed = Checksums.Parse(await client.GetStringAsync(sums.Url, either.Token));
            if (!listed.TryGetValue(asset.Name, out var expected))
                return Failed(L10n.F("The release's list of checksums does not mention {0}, so it could not be checked. Download it from the release page instead.", asset.Name));

            // Nothing is asked of the network while files already there are hashed, which on a slow or
            // cloud-backed folder may take a while and is no stall.
            stall.CancelAfter(Timeout.InfiniteTimeSpan);
            Directory.CreateDirectory(folder);
            // Hashing a file already there reads up to 60 MB, which stays off the caller's thread.
            var (target, reuse) = await Task.Run(() => Place(folder, asset, expected), cancel);
            if (reuse) return new DownloadResult(DownloadOutcome.Reused, target);

            string actual;
            // Opened without sharing before it counts as this download's own, so a second Composa
            // fetching the same file fails to open it and never deletes the first one's.
            await using (var file = new FileStream(target + ".part", FileMode.Create, FileAccess.Write, FileShare.None, 1 << 16, useAsync: true))
            {
                part = file.Name;
                actual = await Fetch(client, asset, file, progress, stall, either.Token, cancel);
            }
            if (actual != expected)
                return Failed(L10n.T("The downloaded file did not match the release's checksum, so it was deleted. Try again, or download it from the release page."));
            await MoveIntoPlace(part, target);
            part = null;
            return new DownloadResult(DownloadOutcome.Downloaded, target);
        }
        catch (OperationCanceledException) when (cancel.IsCancellationRequested)
        {
            return new DownloadResult(DownloadOutcome.Cancelled);
        }
        catch (OperationCanceledException) when (stall.IsCancellationRequested)
        {
            return Failed(L10n.F("The download stalled: nothing arrived for {0:0} seconds. Try again later.", stallAfter.TotalSeconds));
        }
        catch (Exception error) when (Describe(error, folder) is { } problem)
        {
            return Failed(problem);
        }
        finally
        {
            if (part != null) await Discard(part);
        }
    }

    private static DownloadResult Failed(string problem) => new(DownloadOutcome.Failed, Problem: problem);

    /// <summary>Streams the file into <paramref name="file"/>, hashing it on the way, and returns its SHA-256.</summary>
    private async Task<string> Fetch(HttpClient client, ReleaseAsset asset, FileStream file, IProgress<DownloadProgress>? progress,
        CancellationTokenSource stall, CancellationToken either, CancellationToken cancel)
    {
        stall.CancelAfter(stallAfter);
        using var response = await client.GetAsync(asset.Url, HttpCompletionOption.ResponseHeadersRead, either);
        response.EnsureSuccessStatusCode();
        var total = response.Content.Headers.ContentLength ?? (asset.Size > 0 ? asset.Size : null);
        await using var body = await response.Content.ReadAsStreamAsync(either);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[1 << 16];
        long received = 0;
        progress?.Report(new DownloadProgress(0, total));
        while (true)
        {
            stall.CancelAfter(stallAfter);
            var read = await body.ReadAsync(buffer, either);
            if (read == 0) break;
            hash.AppendData(buffer, 0, read);
            await file.WriteAsync(buffer.AsMemory(0, read), cancel);
            received += read;
            progress?.Report(new DownloadProgress(received, total));
        }
        return Convert.ToHexStringLower(hash.GetHashAndReset());
    }

    /// <summary>
    /// Where the file goes. One already there with the right contents is used as it is, so pressing
    /// Download twice fetches once; one with the same name and other contents is someone's own and
    /// is left alone, the download taking the next free name, <c>composa_1.3.0_amd64 (1).deb</c>.
    /// </summary>
    private static (string Path, bool Reuse) Place(string folder, ReleaseAsset asset, string expected)
    {
        for (var copy = 0; ; copy++)
        {
            var path = Path.Combine(folder, copy == 0 ? asset.Name : Numbered(asset.Name, copy));
            if (!File.Exists(path)) return (path, false);
            // The size says most of the time, and is free.
            if (asset.Size > 0 && new FileInfo(path).Length != asset.Size) continue;
            if (HashOf(path) == expected) return (path, true);
        }
    }

    /// <summary>A name with a number before its extension, keeping a tarball's two extensions together.</summary>
    public static string Numbered(string name, int copy)
    {
        var extension = name.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase) ? name[^7..] : Path.GetExtension(name);
        return $"{name[..^extension.Length]} ({copy}){extension}";
    }

    private static string? HashOf(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            return Convert.ToHexStringLower(SHA256.HashData(stream));
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            return null; // Unreadable, so certainly not the file to reuse.
        }
    }

    /// <summary>
    /// Gives a checked download its name. A virus scanner on Windows often holds a file it has just
    /// seen written, so a move that fails is tried again for a moment before the checked file is
    /// given up.
    /// </summary>
    private static async Task MoveIntoPlace(string part, string target)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                File.Move(part, target);
                return;
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException && attempt < 10 && !File.Exists(target))
            {
                await Task.Delay(100);
            }
        }
    }

    /// <summary>
    /// Removes a partial file. On Windows a virus scanner often holds a file it has just seen written
    /// for a moment, so this tries a few times before leaving it.
    /// </summary>
    private static async Task Discard(string path)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            try
            {
                File.Delete(path);
                return;
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            {
                await Task.Delay(100);
            }
        }
    }

    /// <summary>What went wrong, in words, or null for an error that is not the network's or the disk's and so should not be hidden.</summary>
    public static string? Describe(Exception error, string folder) => error switch
    {
        HttpRequestException { StatusCode: HttpStatusCode.NotFound } =>
            L10n.T("GitHub no longer has this file. The release page may have a newer one."),
        HttpRequestException { StatusCode: { } status } =>
            L10n.F("GitHub refused the download ({0} {1}). Try again later.", (int)status, status),
        HttpRequestException => L10n.T("Could not reach GitHub. Check the connection and try again."),
        HttpIOException => L10n.T("The connection broke off before the download finished. Try again."),
        IOException io when IsDiskFull(io) => L10n.F("There is not enough space in {0} for the download.", folder),
        UnauthorizedAccessException or DirectoryNotFoundException => L10n.F("Composa cannot write to {0}.", folder),
        IOException io => L10n.F("Composa could not write the download to {0}: {1}", folder, io.Message),
        _ => null,
    };

    /// <summary>
    /// Disk full is reported differently everywhere: Windows as ERROR_DISK_FULL or ERROR_HANDLE_DISK_FULL
    /// in the low word of the HRESULT, Linux and macOS as the errno ENOSPC itself.
    /// </summary>
    private static bool IsDiskFull(IOException error) =>
        OperatingSystem.IsWindows() ? (error.HResult & 0xFFFF) is 112 or 39 : error.HResult == 28;
}

/// <summary>The lines <c>sha256sum</c> writes: a hash, a space, a space or an asterisk, and a file name.</summary>
public static class Checksums
{
    public static IReadOnlyDictionary<string, string> Parse(string text)
    {
        var sums = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var raw in text.Split('\n'))
        {
            var line = raw.TrimEnd('\r');
            if (line.Length < 67 || line[64] != ' ' || line[65] is not (' ' or '*')) continue;
            var hash = line[..64];
            if (!hash.All(Uri.IsHexDigit)) continue;
            sums[line[66..]] = hash.ToLowerInvariant();
        }
        return sums;
    }
}
