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
            .. playlists.Select(playlist => Utils.CreatePlaylistListItem(
                playlist,
                new AddToPlaylistCommand(playlist.Id ?? "", playlist.Name ?? ""),
                currentUser,
                null
            )),
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
