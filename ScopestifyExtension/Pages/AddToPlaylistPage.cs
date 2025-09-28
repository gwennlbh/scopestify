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

    public AddToPlaylistPage()
    {
        Icon = new("\uE710");
        Title = "Add current track to playlist";
        Name = "Add to playlist";

        Task.Run(GetPlaylists).Wait();
    }

    public override ListItem[] GetItems()
    {
        return
        [
            .. playlists.Select(playlist => new ListItem(
                new AddToPlaylistCommand(playlist.Id ?? "")
            )
            {
                Title = playlist.Name ?? "Unknown playlist",
                Subtitle = playlist.Description ?? "",
                Icon = new IconInfo(playlist.Images?.FirstOrDefault()?.Url ?? "\uF147"),
            }),
        ];
    }

    private async Task GetPlaylists()
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
    }
}
