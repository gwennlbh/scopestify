using System;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

namespace ScopestifyExtension;

internal sealed partial class AddToPlaylistCommand : InvokableCommand
{
    public override string Name => "Add to playlist";
    public override IconInfo Icon => new("\uF147");

    private readonly SpotifyClient spotify = AuthenticatedSpotifyClient.Get();

    private string playlistId;

    private FullTrack? currentTrack;

    public AddToPlaylistCommand(string playlistId)
    {
        this.playlistId = playlistId;
    }

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
                new ToastArgs { Message = $"Added {currentTrack?.Name ?? "?"}" }
            );
        }
        catch (Exception ex)
        {
            return CommandResult.ShowToast(new ToastArgs { Message = ex.Message });
        }
    }
}
