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

    public AddToPlaylistPage()
    {
        Icon = new("\uE710");
        Title = "Add current track to playlist";
        Name = "Add to playlist";
        ShowDetails = true;

        Task.Run(LoadData).Wait();
    }

    public override ListItem[] GetItems()
    {
        return
        [
            .. playlists.Select(playlist => new ListItem(
                new AddToPlaylistCommand(playlist.Id ?? "", playlist.Name ?? "")
            )
            {
                Title = playlist.Name ?? "Unnamed playlist",
                Subtitle = string.Join(
                    " • ",
                    new string[]
                    {
                        playlist.Owner?.Id != currentUser?.Id
                            ? $"By {playlist.Owner?.DisplayName}"
                            : "",
                        playlist.Tracks?.Total != null ? $"{playlist.Tracks.Total} tracks" : "",
                    }.Where(s => !string.IsNullOrEmpty(s))
                ),
                Icon = new IconInfo(playlist.Images?.FirstOrDefault()?.Url ?? "\uF147"),
                Details = new Details
                {
                    Title = playlist.Name ?? "Unnamed playlist",
                    Body = playlist.Description ?? "No description",
                    HeroImage = new IconInfo(playlist.Images?.FirstOrDefault()?.Url ?? "\uF147"),
                    Metadata =
                    [
                        new DetailsElement
                        {
                            Key = "By",
                            Data =
                                playlist.Owner?.Id == currentUser?.Id
                                    ? new DetailsLink("You")
                                    : new DetailsLink(
                                        playlist.Owner.ExternalUrls?["spotify"] ?? "",
                                        playlist.Owner?.DisplayName ?? "Unknown"
                                    ),
                        },
                        new DetailsElement
                        {
                            Key = "Tracks",
                            Data = new DetailsLink(playlist.Tracks?.Total.ToString() ?? "?"),
                        },
                        new DetailsElement
                        {
                            Key = "Playlist ID",
                            Data = new DetailsLink(
                                playlist.ExternalUrls?["spotify"] ?? "",
                                playlist.Id ?? "?"
                            ),
                        },
                    ],
                },
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
        }
        catch (Exception)
        { /* ignore */
        }
    }
}
