using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

namespace ScopestifyExtension.Commands;

internal sealed partial class LikeCurrentTrack(bool remove = false) : InvokableCommand
{
    public override string Name =>
        remove ? "Remove currently playing from liked tracks" : "Like current track";
    public override IconInfo Icon => remove ? new("\uEA92") : new("\uEB51");

    private FullTrack? currentTrack;

    private async Task Run()
    {
        var spotify = AuthenticatedSpotifyClient.Get();

        var playback = await spotify.Player.GetCurrentlyPlaying(
            new PlayerCurrentlyPlayingRequest(PlayerCurrentlyPlayingRequest.AdditionalTypes.Track)
        );

        currentTrack = playback.Item as FullTrack;

        if (currentTrack == null)
        {
            throw new InvalidOperationException("No track currently playing");
        }

        var savedTracks = await spotify.Library.CheckTracks(
            new LibraryCheckTracksRequest([currentTrack.Id ?? ""])
        );

        if (savedTracks.Count > 0 && savedTracks[0] && !remove)
        {
            throw new InvalidOperationException(
                $"Track {Utils.Text.TrackFullName(currentTrack)} is already liked"
            );
        }
        else if (remove)
        {
            throw new InvalidOperationException(
                $"Track {Utils.Text.TrackFullName(currentTrack)} is not liked"
            );
        }

        if (remove)
        {
            await spotify.Library.RemoveTracks(
                new LibraryRemoveTracksRequest([currentTrack.Id ?? ""])
            );
        }
        else
        {
            await spotify.Library.SaveTracks(new LibrarySaveTracksRequest([currentTrack.Id ?? ""]));
        }
    }

    public override CommandResult Invoke()
    {
        try
        {
            Task.Run(Run).Wait();

            var config = new ConfigurationFile();
            if (config.PostLikeHook != "" && !remove)
            {
                try
                {
                    System.Diagnostics.Process.Start(
                        new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = config.PostLikeHook.Split(' ', 2)[0],
                            Arguments = config.PostLikeHook.Split(' ', 2)[1],
                            WorkingDirectory = config.PostLikeHookCwd ?? "",
                            UseShellExecute = true,
                            CreateNoWindow = true,
                            WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                        }
                    );
                }
                catch (Exception ex)
                {
                    return CommandResult.ShowToast(
                        new ToastArgs
                        {
                            Message =
                                $"Liked {Utils.Text.TrackFullName(currentTrack)}, but failed to run post-like hook: {ex.Message}",
                        }
                    );
                }
            }

            return CommandResult.ShowToast(
                new ToastArgs
                {
                    Message = remove
                        ? $"Removed {Utils.Text.TrackFullName(currentTrack)} from liked tracks"
                        : $"Liked {Utils.Text.TrackFullName(currentTrack)}",
                }
            );
        }
        catch (Exception ex)
        {
            return CommandResult.ShowToast(
                new ToastArgs { Message = ex.InnerException?.Message ?? ex.Message }
            );
        }
    }
}
