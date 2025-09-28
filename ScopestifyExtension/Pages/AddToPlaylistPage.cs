using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

namespace ScopestifyExtension;

internal sealed partial class AddToPlaylistPage : ListPage
{
    private SpotifyClient spotify = AuthenticatedSpotifyClient.Get();

    private FullPlaylist[] playlists = [];
    private PrivateUser? currentUser;
    private FullTrack? currentTrack;

    public AddToPlaylistPage()
    {
        Icon = new("\uE710");
        Title = "Add current track to a playlist";
        Name = "Add to playlist";

        Task.Run(LoadData).Wait();

        Title = $"Add {Utils.TrackFullName(currentTrack)} to a playlist";
    }

    public override ListItem[] GetItems()
    {
        return
        [
            .. playlists.Select(playlist =>
            {
                var alreadyInPlaylist =
                    playlist.Tracks?.Items?.Any(t =>
                        t.Track.Type == ItemType.Track
                        && (t.Track as PlaylistTrack<FullTrack>)?.Track.Id == currentTrack?.Id
                    ) ?? false;

                return new ListItem(new AddToPlaylistCommand(playlist.Id ?? ""))
                {
                    Icon = new IconInfo(
                        alreadyInPlaylist
                            ? "\uE73E"
                            : playlist.Images?.FirstOrDefault()?.Url ?? "\uF147"
                    ),
                    Title = playlist.Name ?? "Unnamed playlist",
                    Subtitle = alreadyInPlaylist
                        ? "Already in playlist"
                        : string.Join(
                            " • ",
                            new string[]
                            {
                                playlist.Owner?.Id != currentUser?.Id
                                    ? $"By {playlist.Owner?.DisplayName}"
                                    : "",
                                playlist.Description ?? "",
                                playlist.Tracks?.Total != null
                                    ? $"{playlist.Tracks.Total} tracks"
                                    : "",
                            }.Where(s => !string.IsNullOrEmpty(s))
                        ),
                };
            }),
        ];
    }

    private async Task LoadData()
    {
        try
        {
            var playlistsFirstPage = await spotify.Playlists.CurrentUsers();
            var allPlaylists = await spotify.PaginateAll(playlistsFirstPage);
            playlists = [.. allPlaylists];
        }
        catch (Exception ex)
        {
            playlists = [new FullPlaylist { Name = "Error fetching playlists", Id = ex.Message }];
        }

        try
        {
            currentUser = await spotify.UserProfile.Current();
            currentTrack = await spotify
                .Player.GetCurrentlyPlaying(
                    new PlayerCurrentlyPlayingRequest(
                        PlayerCurrentlyPlayingRequest.AdditionalTypes.Track
                    )
                )
                .ContinueWith(t => t.Result.Item as FullTrack);
        }
        catch (Exception)
        { /* ignore */
        }
    }
}
