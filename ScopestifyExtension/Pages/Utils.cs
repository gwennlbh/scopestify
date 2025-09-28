using System;
using System.Linq;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

namespace ScopestifyExtension;

public class Utils
{
    public static string Artists(FullTrack? track)
    {
        if (track == null)
        {
            return "Unknown artist";
        }

        return string.Join(" × ", track.Artists?.Select(a => a.Name) ?? []);
    }

    public static string TrackFullName(FullTrack? track)
    {
        if (track == null)
        {
            return "Unknown track";
        }

        return $"{Artists(track)} – {track.Name}";
    }

    public static ListItem CreateTrackListItem(FullTrack track, InvokableCommand primaryCommand, CommandContextItem[] moreCommands)
    {
        return new ListItem(primaryCommand)
        {
            Title = track.Name ?? "Unnamed track",
            Subtitle = string.Join(
                " • ",
                [Artists(track), track.Album?.Name ?? "No album"]
            ),
            Icon = new IconInfo(track.Album?.Images?.FirstOrDefault()?.Url ?? "\uEC4F"),
            MoreCommands = moreCommands,
            Details = new Details
            {
                HeroImage = new IconInfo(
                    track.Album?.Images?.FirstOrDefault()?.Url ?? "\uEC4F"
                ),
                Title = track.Name ?? "Unnamed track",
                Body = Artists(track),
                Metadata =
                [
                    new DetailsElement
                    {
                        Key = "Album",
                        Data =
                            track.Album != null
                                ? new DetailsLink(
                                    track.Album.ExternalUrls?["spotify"] ?? "",
                                    $"Track {track.TrackNumber} on {track.Album.Name}"
                                )
                                : new DetailsLink("No album"),
                    },
                    new DetailsElement
                    {
                        Key = "Duration",
                        Data = new DetailsLink(
                            TimeSpan.FromMilliseconds(track.DurationMs).ToString(@"m\:ss")
                        ),
                    },
                    new DetailsElement
                    {
                        Key = "Track ID",
                        Data = new DetailsLink(
                            track.ExternalUrls?["spotify"] ?? "",
                            track.Id ?? "?"
                        ),
                    },
                ],
            },
        };
    }

    public static ListItem CreateAlbumListItem(SimpleAlbum album, CommandContextItem[] moreCommands)
    {
        return new ListItem(
            new PlayAlbumCommand(album.Uri, album.Name ?? "Unnamed album", enqueue: false)
        )
        {
            Title = album.Name ?? "Unnamed album",
            Subtitle = Artists(new FullTrack { Artists = album.Artists }),
            Icon = new IconInfo(album.Images?.FirstOrDefault()?.Url ?? "\uE7C3"),
            MoreCommands = moreCommands,
            Details = new Details
            {
                Title = album.Name ?? "Unnamed album",
                Body = $"Album by {Artists(new FullTrack { Artists = album.Artists })}",
                HeroImage = new IconInfo(album.Images?.FirstOrDefault()?.Url ?? "\uE7C3"),
                Metadata =
                [
                    new DetailsElement
                    {
                        Key = "Released",
                        Data = new DetailsLink(album.ReleaseDate ?? "?"),
                    },
                    new DetailsElement
                    {
                        Key = "Tracks",
                        Data = new DetailsLink(album.TotalTracks.ToString()),
                    },
                    new DetailsElement
                    {
                        Key = "Album ID",
                        Data = new DetailsLink(
                            album.ExternalUrls?["spotify"] ?? "",
                            album.Id ?? "?"
                        ),
                    },
                ],
            },
        };
    }

    public static ListItem CreatePlaylistListItem(FullPlaylist playlist, InvokableCommand primaryCommand, PrivateUser? currentUser, CommandContextItem[]? moreCommands)
    {
        return new ListItem(primaryCommand)
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
            MoreCommands = moreCommands,
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
                                    playlist.Owner?.ExternalUrls?["spotify"] ?? "",
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
        };
    }
}
