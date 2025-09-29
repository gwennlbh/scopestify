using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

namespace ScopestifyExtension.Commands;

/// <summary>
/// Like or remove like for a track
/// </summary>
/// <param name="target">ID of the track to like. Set to null to use the currently playing track.</param>
/// <param name="remove"></param>
internal sealed partial class LikeTrack(string? target, bool remove = false) : InvokableCommand
{
    public override string Name =>
        remove && target == null ? "Remove currently playing from liked tracks"
        : !remove && target == null ? "Like current track"
        : remove && target != null ? "Remove from liked tracks"
        : "Like track";
    public override IconInfo Icon => remove ? new("\uEA92") : new("\uEB51");

    private FullTrack? track;

    private async Task Run()
    {
        var spotify = AuthenticatedSpotifyClient.Get();

        if (target != null)
        {
            track = await spotify.Tracks.Get(target);
            if (track == null)
            {
                throw new InvalidOperationException($"Track with ID {target} not found");
            }
        }
        else
        {
            var playback = await spotify.Player.GetCurrentlyPlaying(
                new PlayerCurrentlyPlayingRequest(
                    PlayerCurrentlyPlayingRequest.AdditionalTypes.Track
                )
            );

            track = playback.Item as FullTrack;

            if (track == null)
            {
                throw new InvalidOperationException("No track currently playing");
            }
        }

        var savedTracks = await spotify.Library.CheckTracks(
            new LibraryCheckTracksRequest([track.Id ?? ""])
        );

        if (savedTracks.Count > 0 && savedTracks[0] && !remove)
        {
            throw new InvalidOperationException(
                $"Track {Utils.Text.TrackFullName(track)} is already liked"
            );
        }
        else if (remove)
        {
            throw new InvalidOperationException(
                $"Track {Utils.Text.TrackFullName(track)} is not liked"
            );
        }

        if (remove)
        {
            await spotify.Library.RemoveTracks(new LibraryRemoveTracksRequest([track.Id ?? ""]));
        }
        else
        {
            await spotify.Library.SaveTracks(new LibrarySaveTracksRequest([track.Id ?? ""]));
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
                                $"Liked {Utils.Text.TrackFullName(track)}, but failed to run post-like hook: {ex.Message}",
                        }
                    );
                }
            }

            return CommandResult.ShowToast(
                new ToastArgs
                {
                    Message = remove
                        ? $"Removed {Utils.Text.TrackFullName(track)} from liked tracks"
                        : $"Liked {Utils.Text.TrackFullName(track)}",
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
