using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

namespace ScopestifyExtension;

internal sealed partial class LikeCurrentTrackCommand : InvokableCommand
{
    public override string Name => "Like current track";
    public override IconInfo Icon => new("\uEB51");

    private readonly SpotifyClient spotify = AuthenticatedSpotifyClient.Get();

    private FullTrack? currentTrack;

    private async Task Run()
    {
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

        if (savedTracks.Count > 0 && savedTracks[0])
        {
            throw new InvalidOperationException(
                $"Track {Utils.TrackFullName(currentTrack)} is already liked"
            );
        }

        await spotify.Library.SaveTracks(new LibrarySaveTracksRequest([currentTrack.Id ?? ""]));
    }

    public override CommandResult Invoke()
    {
        try
        {
            Task.Run(Run).Wait();

            var config = new ConfigurationFile();
            if (config.PostLikeHook != "")
            {
                try
                {
                    System.Diagnostics.Process.Start(
                        new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = config.PostLikeHook,
                            UseShellExecute = true,
                        }
                    );
                }
                catch (Exception ex)
                {
                    return CommandResult.ShowToast(
                        new ToastArgs
                        {
                            Message =
                                $"Liked {Utils.TrackFullName(currentTrack)}, but failed to run post-like hook: {ex.Message}",
                        }
                    );
                }
            }

            return CommandResult.ShowToast(
                new ToastArgs { Message = $"Liked {Utils.TrackFullName(currentTrack)}" }
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
