namespace ScopestifyExtension.Pages;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

internal sealed partial class CurrentlyPlaying : ListPage
{
    public override string Name => "Current Track";
    public override string Title => "Currently playing track";
    public override IconInfo Icon => Icons.MusicNote;
    public override bool ShowDetails => true;

    private FullTrack? currentTrack;
    private FullArtist[] artists = [];
    private bool liked;

    public CurrentlyPlaying()
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
            HeroImage = Icons.WithFallback(
                currentTrack.Album?.Images?.FirstOrDefault()?.Url,
                Icons.MusicNote
            ),
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
                    Data = new DetailsLink(Utils.Text.Duration(currentTrack.DurationMs, "?")),
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
            new ListItem(new Commands.LikeTrack(target: null, remove: liked))
            {
                Title = liked ? "Remove from liked tracks" : "Like track",
                Subtitle = liked
                    ? "Track is already in your Liked Songs"
                    : "Add to your Liked Songs",
                Details = details,
            },
            new ListItem(new MyPlaylists(trackToAdd: currentTrack))
            {
                Title = "Add to a playlist",
                Subtitle = "Add the current track to one of your playlists",
                Details = details,
            },
            new ListItem(
                new AlbumTracks(
                    currentTrack.Id,
                    tagInList: new Tag("Playing") { Icon = Icons.VolumeBars }
                )
            )
            {
                Title = "See album",
                Subtitle = Utils.Text.InfoLine(
                    currentTrack.Album?.Name ?? "Unknown album",
                    $"{currentTrack.Album.TotalTracks} tracks"
                ),
                Details = details,
                MoreCommands =
                [
                    new CommandContextItem(
                        new Commands.PlayAlbum(
                            currentTrack.Album?.Uri ?? "",
                            Utils.Text.AlbumFullName(currentTrack.Album),
                            enqueue: true
                        )
                    ),
                    new CommandContextItem(new OpenUrlCommand(currentTrack.Album?.Uri ?? ""))
                    {
                        Title = "Open in Spotify",
                    },
                ],
            },
            new ListItem(
                new OpenUrlCommand(artists.FirstOrDefault()?.Uri ?? "")
                {
                    Name = $"See {artists.FirstOrDefault()?.Name}",
                    Icon = Icons.WithFallback(
                        artists.FirstOrDefault()?.Images?.FirstOrDefault()?.Url,
                        Icons.Artist
                    ),
                }
            )
            {
                Icon = artists.Length > 1 ? Icons.Group : Icons.Artist,
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
                            Icon = Icons.WithFallback(
                                artist.Images?.FirstOrDefault()?.Url,
                                Icons.Artist
                            ),
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
