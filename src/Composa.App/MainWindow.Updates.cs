using System.Runtime.InteropServices;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Composa.App.Dialogs;
namespace Composa.App;

/// <summary>
/// Everything updating reaches outside the window through: the releases, the network, the Downloads
/// folder and the programs an install hands over to. A test stands in for each, so nothing reaches
/// GitHub and nothing is started.
/// </summary>
public sealed class UpdateEnvironment
{
    public IReleaseSource Source { get; init; } = new GitHubReleaseSource();

    /// <summary>Stands in for the network for downloads; null uses a real connection.</summary>
    public HttpMessageHandler? Http { get; init; }

    /// <summary>Where a download goes, asked afresh each time, since the folder may move.</summary>
    public Func<string> Folder { get; init; } = () => AppPaths.Downloads;

    public Func<InstallKind> Kind { get; init; } = () => Install.Current;

    public Func<ShellCommand, bool> Run { get; init; } = ShellCommand.Start;

    /// <summary>Opens a file with its default application; null uses the window's launcher.</summary>
    public Func<string, Task<bool>>? Open { get; init; }

    /// <summary>Shows a file in the file manager; null uses <see cref="FileReveal"/>.</summary>
    public Func<string, Task<bool>>? Reveal { get; init; }
}

/// <summary>
/// Telling the user that a newer version exists and, when they press Download, fetching the file
/// that replaces this install. Nothing here installs anything or replaces Composa's own files:
/// Install hands a checked download to its installer, and only when pressed.
/// </summary>
public sealed partial class MainWindow
{
    public UpdateEnvironment Updates { get; set; } = new();

    public UpdateNotice UpdateNotice => updateNotice;

    private UpdateCheck NewUpdateCheck() => new(Updates.Source, settings);

    private ReleaseVersion? pendingUpdateVersion;
    private string? pendingUpdateUrl;
    private ReleaseInfo? pendingRelease;
    private ReleaseAsset? pendingAsset;
    private InstallKind pendingKind;
    // The download under way, which closing the window cancels and waits for, so no .part is left behind.
    private Task? download;
    private CancellationTokenSource? downloadCancel;
    private string? downloadedPath;

    /// <summary>The strip's buttons, wired once whichever check shows it.</summary>
    private void WireUpdateNotice()
    {
        updateNotice.OpenReleasePage += () => UpdateNotice.OpenInBrowser(pendingUpdateUrl ?? AppInfo.ReleasesUrl);
        updateNotice.Skip += () =>
        {
            settings.SkippedVersion = pendingUpdateVersion?.ToString();
            settings.Save();
        };
        updateNotice.Download += () => _ = DownloadUpdate();
        updateNotice.CancelDownload += () => downloadCancel?.Cancel();
        updateNotice.ShowInFolder += () => _ = ShowDownload();
        updateNotice.Install += () => _ = InstallUpdate();
    }

    /// <summary>
    /// Started once the window is up, on a background task, and never awaited. A slow or
    /// unreachable network must not hold up a single frame of startup, and an automatic check that
    /// fails says nothing at all.
    /// </summary>
    private void StartUpdateCheck()
    {
        // The once-a-day limit lives in the settings file. Without it, as in the tests, every window
        // would ask GitHub again.
        if (!Settings.Persist) return;
        var check = NewUpdateCheck();
        if (!check.RunsAutomatically) return;
        _ = Task.Run(async () =>
        {
            var result = await check.Run(manual: false);
            if (!result.ShouldNotify) return;
            // Finding the install kind may ask rpm, which stays off the UI thread.
            var (kind, asset) = Downloadable(result);
            await Dispatcher.UIThread.InvokeAsync(() => Offer(result, kind, asset));
        });
    }

    /// <summary>The file this install would download from the release, if any: never for a build whose package manager owns updates.</summary>
    private (InstallKind Kind, ReleaseAsset? Asset) Downloadable(UpdateResult result)
    {
        var kind = Updates.Kind();
        return (kind, result.Release is { } release ? ReleaseAssets.Offered(UpdateCheck.Channel, kind, RuntimeInformation.ProcessArchitecture, release) : null);
    }

    private void Offer(UpdateResult result, InstallKind kind, ReleaseAsset? asset)
    {
        // Another check that finds the version already downloading or downloaded leaves that be.
        if (download != null) return;
        if (downloadedPath != null && pendingUpdateVersion?.Equals(result.Version) == true && File.Exists(downloadedPath))
        {
            ShowDownloaded();
            return;
        }
        pendingUpdateVersion = result.Version;
        pendingUpdateUrl = result.Url;
        pendingRelease = result.Release;
        pendingKind = kind;
        pendingAsset = asset;
        downloadedPath = null;
        updateNotice.Show(result.Version, asset != null);
    }

    /// <summary>Help &gt; Check for Updates. Unlike the automatic check, this always reports what happened.</summary>
    private async Task CheckForUpdatesNow()
    {
        if (UpdateCheck.Channel == UpdateChannel.Managed)
        {
            await Prompts.Alert(this, L10n.T("Check for Updates"),
                L10n.F("Composa {0} was installed through your package manager, which is where updates come from. Use it to upgrade, rather than downloading a build that it does not know about.", AppInfo.Version));
            return;
        }

        var result = await NewUpdateCheck().Run(manual: true);
        switch (result.Outcome)
        {
            case UpdateOutcome.Available:
                var (kind, asset) = await Task.Run(() => Downloadable(result));
                Offer(result, kind, asset);
                break;
            case UpdateOutcome.Failed:
                // Someone who asked deserves an answer, even when the answer is that it did not work.
                await Prompts.Alert(this, L10n.T("Check for Updates"),
                    L10n.T("Could not reach the release page to check for a newer version. Please try again later."));
                break;
            default:
                await Prompts.Alert(this, L10n.T("Check for Updates"), L10n.F("Composa {0} is the latest version.", AppInfo.Version));
                break;
        }
    }

    /// <summary>Download, or Retry after a failure. One download at a time.</summary>
    private async Task DownloadUpdate()
    {
        if (download != null || pendingRelease is not { } release || pendingAsset is not { } asset || pendingUpdateVersion is not { } version) return;
        var kind = pendingKind;
        using var cancel = downloadCancel = new CancellationTokenSource();
        // Reports are posted to the UI thread and may trail the end of the download; only a download
        // still running moves the bar.
        var progress = new Progress<DownloadProgress>(received => { if (download != null) updateNotice.ShowProgress(version, received); });
        updateNotice.ShowProgress(version, new DownloadProgress(0, asset.Size > 0 ? asset.Size : null));

        var running = new UpdateDownload(Updates.Http).Run(release, asset, Updates.Folder(), progress, cancel.Token);
        download = running;
        DownloadResult result;
        try { result = await running; }
        catch (Exception error)
        {
            // UpdateDownload lets through what is neither the network's nor the disk's, so it is not
            // mistaken for either. It still has to leave the strip, which would otherwise stay in
            // Downloading with nothing to cancel.
            Console.Error.WriteLine(error);
            result = new DownloadResult(DownloadOutcome.Failed, Problem: L10n.F("The download failed: {0}: {1}", error.GetType().Name, error.Message));
        }
        finally
        {
            download = null;
            downloadCancel = null;
        }

        switch (result.Outcome)
        {
            case DownloadOutcome.Downloaded or DownloadOutcome.Reused:
                Installer.Prepare(kind, result.Path!);
                downloadedPath = result.Path;
                ShowDownloaded();
                break;
            case DownloadOutcome.Cancelled:
                updateNotice.Show(version, canDownload: true);
                break;
            default:
                updateNotice.ShowFailed(result.Problem ?? L10n.T("The download failed."));
                break;
        }
    }

    private void ShowDownloaded()
    {
        if (downloadedPath is not { } path || pendingUpdateVersion is not { } version) return;
        var kind = pendingKind;
        string? hint = !Installer.HasOne(kind) ? null
            : Installer.QuitsFirst(kind) ? L10n.T("Quit Composa, asking about unsaved work as Quit does, and start the setup, which upgrades it in place")
            : L10n.T("Open the package in your software installer");
        updateNotice.ShowReady(version, path, hint, Installer.TerminalCommand(kind, path));
    }

    private async Task ShowDownload()
    {
        if (downloadedPath is not { } path) return;
        var reveal = Updates.Reveal ?? (file => FileReveal.Show(file, Launcher));
        if (!await reveal(path)) ShowProblem(L10n.F("Couldn't open the folder {0}.", Path.GetDirectoryName(path)));
    }

    private Task<bool> OpenFile(string path) => Updates.Open?.Invoke(path) ?? Launcher.LaunchFileInfoAsync(new FileInfo(path));

    /// <summary>
    /// Hands the download to its installer. The Windows setup replaces files this process holds, so
    /// Composa quits first, asking about unsaved work exactly as Quit does; Cancel there stops the
    /// install too.
    /// </summary>
    private async Task InstallUpdate()
    {
        if (downloadedPath is not { } path) return;
        var kind = pendingKind;
        if (!File.Exists(path))
        {
            updateNotice.ShowFailed(L10n.F("{0} is no longer in {1}.", Path.GetFileName(path), Path.GetDirectoryName(path)));
            downloadedPath = null;
            return;
        }
        if (Installer.QuitsFirst(kind))
        {
            if (!await ConfirmQuit()) return;
            if (!await Installer.Start(kind, path, Updates.Run, OpenFile))
            {
                ShowProblem(L10n.F("Couldn't start {0}.", Path.GetFileName(path)));
                return;
            }
            closingConfirmed = true;
            Close();
            return;
        }
        if (!await Installer.Start(kind, path, Updates.Run, OpenFile))
            ShowProblem(L10n.F("No software installer opened {0}. Install it from a terminal with the command in the update strip.", Path.GetFileName(path)));
    }

    /// <summary>Cancels a download under way and waits until its partial file is gone.</summary>
    private async Task StopDownload()
    {
        if (download is not { } running) return;
        downloadCancel?.Cancel();
        // A download that failed unexpectedly is reported by DownloadUpdate; quitting only waits for it to end.
        try { await running; }
        catch (Exception) { }
    }
}
