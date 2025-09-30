namespace ScopestifyExtension.Pages;

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

internal sealed partial class MyPlaylists : ListPage
{
    private FullPlaylist[] playlists = [];
    private PrivateUser? currentUser;

    private FullTrack? trackToAdd;

    public MyPlaylists(FullTrack? trackToAdd)
    {
        Id = "my_playlists";
        Icon = Icons.Playlist;
        Title = "Play and manage your playlists";
        Name = "My Playlists";
        ShowDetails = true;

        this.trackToAdd = trackToAdd;

        Task.Run(LoadData).Wait();
    }

    public override ListItem[] GetItems()
    {
        CultureInfo culture = Thread.CurrentThread.CurrentCulture;

        return
        [
            new Components.PlaylistItem(
                Utils.Playlists.DiscoverWeekly(),
                currentUser,
                typeTag: false,
                highlightYours: false
            )
            {
                Subtitle = "Updates every Monday",
                Tags = [new Tag("Automatic") { Icon = Icons.Sync }],
                Command = new NoOpCommand(),
                MoreCommands =
                [
                    new CommandContextItem(
                        new Commands.PlayPlaylist(
                            Utils.Playlists.DiscoverWeeklyURI,
                            Utils.Playlists.DiscoverWeekly().Name,
                            enqueue: false
                        )
                    )
                    {
                        Title = "Play playlist",
                    },
                    new CommandContextItem(new OpenUrlCommand(Utils.Playlists.DiscoverWeeklyURI))
                    {
                        Title = "Open in Spotify",
                    },
                ],
            },
            new Components.PlaylistItem(
                Utils.Playlists.ReleaseRadar(),
                currentUser,
                typeTag: false,
                highlightYours: false
            )
            {
                Subtitle = "Updates every Friday",
                Tags = [new Tag("Automatic") { Icon = Icons.Sync }],
                Command = new NoOpCommand(),
                MoreCommands =
                [
                    new CommandContextItem(
                        new Commands.PlayPlaylist(
                            Utils.Playlists.ReleaseRadarURI,
                            Utils.Playlists.ReleaseRadar().Name,
                            enqueue: false
                        )
                    )
                    {
                        Title = "Play playlist",
                    },
                    new CommandContextItem(new OpenUrlCommand(Utils.Playlists.ReleaseRadarURI))
                    {
                        Title = "Open in Spotify",
                    },
                ],
            },
            .. playlists.Select(playlist => new Components.PlaylistItem(
                playlist,
                currentUser,
                typeTag: false,
                highlightYours: false
            )
            {
                Subtitle = Utils.Text.CountThing(playlist.Tracks?.Total ?? 0, "track"),
                Command = new Commands.AddToPlaylist(
                    playlist.Id ?? "",
                    playlist.Name ?? "",
                    trackId: trackToAdd?.Id
                )
                {
                    Name = trackToAdd != null ? $"Add {trackToAdd.Name}" : $"Add current track",
                },
                MoreCommands =
                [
                    new CommandContextItem(
                        new Commands.PlayPlaylist(
                            playlist?.Uri ?? "",
                            playlist?.Name ?? "",
                            enqueue: false
                        )
                    )
                    {
                        Title = "Play playlist",
                        Icon = Icons.Play,
                    },
                    new CommandContextItem(
                        new Commands.PlayPlaylist(
                            playlist?.Uri ?? "",
                            playlist?.Name ?? "",
                            enqueue: true
                        )
                    )
                    {
                        Title = "Add to queue",
                        Icon = Icons.Import,
                    },
                    new CommandContextItem(new OpenUrlCommand(playlist.Uri ?? ""))
                    {
                        Title = "Open in Spotify",
                    },
                ],
            }),
        ];
    }

    private async Task LoadData()
    {
        var spotify = AuthenticatedSpotifyClient.Get();

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
            playlists = [];
            new ToastStatusMessage(
                new StatusMessage { Message = ex.InnerException?.Message ?? ex.Message }
            ).Show();
        }

        // TODO make this configurable lol
        string[] additionalPlaylistIds =
        [
            "0PV1qbpUtOyZr3JSRUS1ZA", // slowburn (wont show up, idk why)
            "1fKLy4AW5FA2APBvqAzmiX", // contempl8 (wont show up, idk why)
        ];

        foreach (var playlistId in additionalPlaylistIds)
        {
            try
            {
                var playlist = await spotify.Playlists.Get(playlistId);
                if (playlist != null && !playlists.Any(p => p.Id == playlist.Id))
                {
                    playlists = [playlist, .. playlists];
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching playlist {playlistId}: {ex.Message}");
            }
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
