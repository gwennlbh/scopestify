using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

namespace ScopestifyExtension;

/// <summary>
/// Add the currently playing track to the specified playlist
/// </summary>
/// <param name="playlistId"></param>
/// <param name="name"></param>
/// <param name="trackId">If not specified, defaults to the currently playing track</param>
internal sealed partial class AddToPlaylistCommand(
    string playlistId,
    string name,
    string? trackId = null
) : InvokableCommand
{
    public override string Name => "Add current track";
    public override IconInfo Icon => new("\uF147");

    private string playlistId = playlistId;
    private string name = name;

    private string? trackId = trackId;

    private FullTrack? trackToAdd;

    private async Task Run()
    {
        var spotify = AuthenticatedSpotifyClient.Get();

        if (trackId != null)
        {
            trackToAdd = await spotify.Tracks.Get(trackId);
        }
        else
        {
            var playback = await spotify.Player.GetCurrentlyPlaying(
                new PlayerCurrentlyPlayingRequest(
                    PlayerCurrentlyPlayingRequest.AdditionalTypes.Track
                )
            );

            trackToAdd = playback.Item as FullTrack;
            // Check if track is already in the playlist
            if (trackToAdd == null)
            {
                throw new InvalidOperationException("No track currently playing");
            }
        }

        var playlistTracksPage = await spotify.Playlists.GetItems(playlistId);
        var playlistTracks = await spotify.PaginateAll(playlistTracksPage);
        var alreadyInPlaylist = playlistTracks
            .Where(t => t.Track.Type == ItemType.Track)
            .Select(t => t.Track as FullTrack)
            .Any(t => t?.Id == trackToAdd?.Id);

        if (alreadyInPlaylist)
        {
            throw new InvalidOperationException(
                $"{Utils.TrackFullName(trackToAdd)} is already in {name}"
            );
        }

        await spotify.Playlists.AddItems(
            playlistId,
            new PlaylistAddItemsRequest(
                [
                    trackToAdd?.Uri
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
                new ToastArgs { Message = $"Added {Utils.TrackFullName(trackToAdd)} to {name}" }
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
