using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

namespace ScopestifyExtension;

internal sealed partial class AddToPlaylistPage : ListPage
{
    private SpotifyClient spotify = AuthenticatedSpotifyClient.Get();

    private Paging<FullPlaylist>? playlistsPage;
    private FullPlaylist[] playlists = [];
    private PrivateUser? currentUser;

    public AddToPlaylistPage()
    {
        Icon = new("\uE710");
        Title = "Add current track to playlist";
        Name = "Add to playlist";

        Task.Run(LoadData).Wait();
    }

    public override ListItem[] GetItems()
    {
        HasMoreItems = playlistsPage?.Next != null;

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
                        playlist.Description ?? "",
                        playlist.Tracks?.Total != null ? $"{playlist.Tracks.Total} tracks" : "",
                    }.Where(s => !string.IsNullOrEmpty(s))
                ),
                Icon = new IconInfo(playlist.Images?.FirstOrDefault()?.Url ?? "\uF147"),
            }),
        ];
    }

    private async Task LoadData()
    {
        try
        {
            playlistsPage = await spotify.Playlists.CurrentUsers();
            playlists = [.. playlistsPage.Items ?? []];
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

    public override void LoadMore()
    {
        Task.Run(async () =>
            {
                if (playlistsPage?.Next != null)
                {
                    playlistsPage = await spotify.NextPage(playlistsPage);
                    playlists = [.. playlists, .. playlistsPage.Items ?? []];
                    RaiseItemsChanged(playlists.Length);
                }
            })
            .Wait();
    }
}
