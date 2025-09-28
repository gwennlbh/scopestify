using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Http;

namespace ScopestifyExtension;

internal sealed partial class MyPlaylistsPage : ListPage
{
    private SpotifyClient spotify = AuthenticatedSpotifyClient.Get();

    private FullPlaylist[] playlists = [];
    private PrivateUser? currentUser;

    public MyPlaylistsPage()
    {
        Icon = new("\uE90B");
        Title = "Play a playlist or add current track to a playlist";
        Name = "My Playlists";
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
                MoreCommands =
                [
                    new CommandContextItem(
                        new PlayPlaylistCommand(
                            playlist.Uri ?? "",
                            playlist.Name ?? "",
                            enqueue: false
                        )
                    ),
                    new CommandContextItem(new OpenUrlCommand(playlist.Uri ?? ""))
                    {
                        Title = "Open in Spotify",
                    },
                ],
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
                                        playlist.Owner.Uri ?? "",
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
                            Data = new DetailsLink(playlist.Uri ?? "", playlist.Id ?? "?"),
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
            playlistsFirstPage.Limit = 50; // max limit
            var playlistsAllPages = await spotify.PaginateAll(playlistsFirstPage);
            playlists = [.. playlistsAllPages];
            Debug.WriteLine(
                $"Fetched playlists: {string.Join(", ", playlists.Select(p => p.Name))}"
            );
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
