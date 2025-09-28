using System;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

namespace ScopestifyExtension;

internal sealed partial class AddToPlaylistCommand(string playlistId, string name)
    : InvokableCommand
{
    public override string Name => "Add to playlist";
    public override IconInfo Icon => new("\uF147");

    private readonly SpotifyClient spotify = AuthenticatedSpotifyClient.Get();

    private string playlistId = playlistId;
    private string name = name;

    private FullTrack? currentTrack;

    private async Task Run()
    {
        var playback = await spotify.Player.GetCurrentlyPlaying(
            new PlayerCurrentlyPlayingRequest(PlayerCurrentlyPlayingRequest.AdditionalTypes.Track)
        );

        currentTrack = playback.Item as FullTrack;

        await spotify.Playlists.AddItems(
            playlistId,
            new PlaylistAddItemsRequest(
                [
                    currentTrack?.Uri
                        ?? throw new InvalidOperationException("No track currently playing"),
                ]
            )
        );
    }

    public override CommandResult Invoke()
    {
        try
        {
            Task.Run(Run).Wait();
            return CommandResult.ShowToast(
                new ToastArgs { Message = $"Added {Utils.TrackFullName(currentTrack)} to {name}" }
            );
        }
        catch (Exception ex)
        {
            return CommandResult.ShowToast(new ToastArgs { Message = ex.Message });
        }
    }
}
