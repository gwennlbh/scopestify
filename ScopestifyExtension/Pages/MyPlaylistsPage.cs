using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        Id = "my_playlists";
        Icon = new("\uE90B");
        Title = "Play a playlist or add current track to a playlist";
        Name = "My Playlists";
        ShowDetails = true;

        Task.Run(LoadData).Wait();
    }

    public override ListItem[] GetItems()
    {
        // TODO make this configurabe
        var discoverWeeklyUri = "spotify:playlist:37i9dQZEVXcW4o4O4AqUHN"; // Discover Weekly
        var releaseRadarUri = "spotify:playlist:37i9dQZEVXbqiZEgvMyJMk"; // Release Radar

        CultureInfo culture = Thread.CurrentThread.CurrentCulture;

        return
        [
            new ListItem(new NoOpCommand())
            {
                Title = "Discover Weekly",
                Subtitle = "Updates every Monday",
                Tags = [new Tag("Automatic") { Icon = new IconInfo("\uE895") }],
                // Not sure about the image URL
                Icon = new IconInfo(
                    $"https://pickasso.spotifycdn.com/image/ab67c0de0000deef/dt/v1/img/dw/cover/{culture.TwoLetterISOLanguageName}"
                ),
                MoreCommands =
                [
                    new CommandContextItem(
                        new PlayPlaylistCommand(
                            discoverWeeklyUri,
                            "Discover Weekly",
                            enqueue: false
                        )
                    ),
                    new CommandContextItem(new OpenUrlCommand(discoverWeeklyUri))
                    {
                        Title = "Open in Spotify",
                    },
                ],
                Details = new Details
                {
                    Title = "Discover Weekly",
                    Body =
                        "Your shortcut to hidden gems, deep cuts, and future faves, updated every Monday. You'll know when you hear it.",
                    HeroImage = new IconInfo(
                        $"https://pickasso.spotifycdn.com/image/ab67c0de0000deef/dt/v1/img/dw/cover/{culture.TwoLetterISOLanguageName}"
                    ),
                    Metadata =
                    [
                        new DetailsElement
                        {
                            Key = "For",
                            Data = new DetailsLink(
                                currentUser?.Uri ?? "",
                                currentUser?.DisplayName ?? "Unknown user"
                            ),
                        },
                        new DetailsElement
                        {
                            Key = "Playlist ID",
                            Data = new DetailsLink(
                                discoverWeeklyUri,
                                discoverWeeklyUri.Replace("spotify:playlist:", "")
                            ),
                        },
                    ],
                },
            },
            new ListItem(new NoOpCommand())
            {
                Title = "Release Radar",
                Subtitle = "Updates every Friday",
                Tags = [new Tag("Automatic") { Icon = new IconInfo("\uE895") }],
                // Not sure about the image URL
                Icon = new IconInfo(
                    $"https://newjams-images.scdn.co/image/ab67647800003f8a/dt/v3/release-radar/ab6761610000e5ebe412a782245eb20d9626c601/{culture.TwoLetterISOLanguageName}"
                ),
                MoreCommands =
                [
                    new CommandContextItem(
                        new PlayPlaylistCommand(releaseRadarUri, "Release Radar", enqueue: false)
                    ),
                    new CommandContextItem(new OpenUrlCommand(releaseRadarUri))
                    {
                        Title = "Open in Spotify",
                    },
                ],
                Details = new Details
                {
                    Title = "Release Radar",
                    Body =
                        "A personalized playlist of new releases, updated every Friday. You'll know when you hear it.",
                    HeroImage = new IconInfo(
                        $"https://newjams-images.scdn.co/image/ab67647800003f8a/dt/v3/release-radar/ab6761610000e5ebe412a782245eb20d9626c601/{culture.TwoLetterISOLanguageName}"
                    ),
                    Metadata =
                    [
                        new DetailsElement
                        {
                            Key = "For",
                            Data = new DetailsLink(
                                currentUser?.Uri ?? "",
                                currentUser?.DisplayName ?? "Unknown user"
                            ),
                        },
                        new DetailsElement
                        {
                            Key = "Playlist ID",
                            Data = new DetailsLink(
                                releaseRadarUri,
                                releaseRadarUri.Replace("spotify:playlist:", "")
                            ),
                        },
                    ],
                },
            },
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
