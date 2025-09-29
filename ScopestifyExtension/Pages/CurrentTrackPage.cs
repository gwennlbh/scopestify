namespace ScopestifyExtension;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

internal sealed partial class CurrentTrackPage : ListPage
{
    public override string Name => "Current Track";
    public override string Title => "Currently playing track";
    public override IconInfo Icon => new("\uE8D6");
    public override bool ShowDetails => true;

    private FullTrack? currentTrack;
    private FullArtist[] artists = [];
    private bool liked;

    public CurrentTrackPage()
    {
        try
        {
            Task.Run(LoadData).Wait();
        }
        catch (Exception ex)
        {
            new ToastStatusMessage(ex.Message).Show();
        }
    }

    public override ListItem[] GetItems()
    {
        try
        {
            Task.Run(LoadData).Wait();
        }
        catch (Exception ex)
        {
            new ToastStatusMessage(ex.Message).Show();
        }

        if (currentTrack == null)
            return [];

        PlaceholderText = $"Currently playing {Utils.Text.TrackFullName(currentTrack)}";

        var details = new Details
        {
            HeroImage = new IconInfo(currentTrack.Album?.Images?.FirstOrDefault()?.Url ?? "\uEC4F"),
            Title = currentTrack.Name ?? "Unnamed track",
            Body = Utils.Text.Artists(currentTrack),
            Metadata =
            [
                currentTrack.Album != null
                    ? new DetailsElement
                    {
                        Key = $"Track {currentTrack.TrackNumber} on",
                        Data = new DetailsLink(
                            currentTrack.Album.Uri ?? "",
                            currentTrack.Album.Name
                        ),
                    }
                    : new DetailsElement()
                    {
                        Key = "Track is not on any album",
                        Data = new DetailsLink(""),
                    },
                new DetailsElement
                {
                    Key = "Duration",
                    Data = new DetailsLink(
                        TimeSpan.FromMilliseconds(currentTrack.DurationMs).ToString(@"m\:ss")
                    ),
                },
                new DetailsElement
                {
                    Key = "Track ID",
                    Data = new DetailsLink(currentTrack.Uri ?? "", currentTrack.Id ?? "?"),
                },
            ],
        };

        return
        [
            new ListItem(new LikeCurrentTrackCommand(remove: liked))
            {
                Title = liked ? "Remove from liked tracks" : "Like track",
                Subtitle = liked
                    ? "Track is already in your Liked Songs"
                    : "Add to your Liked Songs",
                Details = details,
            },
            new ListItem(new NavigateCommand(new MyPlaylistsPage()))
            {
                Title = "Add to a playlist",
                Subtitle = "Add the current track to one of your playlists",
                Details = details,
            },
            new ListItem(new OpenUrlCommand(currentTrack.Album.Uri ?? ""))
            {
                Icon = new IconInfo("\uE93C"),
                Title = "Open album",
                Subtitle = string.Join(
                    " • ",
                    [
                        currentTrack.Album?.Name ?? "Unknown album",
                        $"{currentTrack.Album.TotalTracks} tracks",
                    ]
                ),
                Details = details,
                MoreCommands =
                [
                    new CommandContextItem(
                        new PlayAlbumCommand(
                            currentTrack.Album?.Uri ?? "",
                            Utils.Text.AlbumFullName(currentTrack.Album),
                            enqueue: true
                        )
                    ),
                ],
            },
            new ListItem(
                new OpenUrlCommand(artists.FirstOrDefault()?.Uri ?? "")
                {
                    Name = $"See {artists.FirstOrDefault()?.Name}",
                    Icon = new IconInfo(
                        artists.FirstOrDefault()?.Images?.FirstOrDefault()?.Url ?? "\uE77B"
                    ),
                }
            )
            {
                Icon = new IconInfo("\uE716"),
                Title = "See artists",
                Subtitle = Utils.Text.Artists(currentTrack),
                Details = details,
                MoreCommands =
                [
                    .. artists
                        .Skip(1)
                        .Select(artist => new CommandContextItem(
                            new OpenUrlCommand(artist.Uri ?? "") { Name = $"See {artist.Name}" }
                        )
                        {
                            Title = $"See {artist.Name}",
                            Icon = new IconInfo(artist.Images?.FirstOrDefault()?.Url ?? "\uE77B"),
                        }),
                ],
            },
            new ListItem(new OpenUrlCommand(currentTrack.Uri))
            {
                Title = "Open in Spotify",
                Details = details,
            },
        ];
    }

    public override ICommandItem? EmptyContent =>
        new CommandItem(new NoOpCommand())
        {
            Title = "No track currently playing",
            Subtitle = "Play something on Spotify to see it here",
            Icon = new IconInfo("💤"),
        };

    private async Task LoadData()
    {
        var spotify = AuthenticatedSpotifyClient.Get();
        var playback = await spotify.Player.GetCurrentlyPlaying(
            new PlayerCurrentlyPlayingRequest(PlayerCurrentlyPlayingRequest.AdditionalTypes.Track)
        );

        if (playback.Item == null)
        {
            return;
        }

        currentTrack = playback.Item as FullTrack;

        liked = await spotify
            .Library.CheckTracks(new LibraryCheckTracksRequest([currentTrack?.Id ?? ""]))
            // Assume track is not liked if we can't check
            .ContinueWith(t => t.Result.FirstOrDefault());

        artists = [];

        foreach (var artist in currentTrack?.Artists ?? [])
        {
            var fullArtist = await spotify.Artists.Get(artist.Id);
            artists = [.. artists, fullArtist];
        }
    }
}
