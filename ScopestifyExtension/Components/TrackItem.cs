namespace ScopestifyExtension.Components;

using System;
using System.Linq;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

public partial class TrackItem : ListItem
{
    public TrackItem(FullTrack track, bool typeTag)
    {
        Command = new PlayTrackCommand(track.Uri, Utils.Text.TrackFullName(track), enqueue: false);
        Title = track.Name ?? "Unnamed track";
        Subtitle = string.Join(" • ", [Utils.Text.Artists(track), track.Album?.Name ?? "No album"]);
        Icon = new IconInfo(track.Album?.Images?.FirstOrDefault()?.Url ?? "\uEC4F");
        Tags = typeTag ? [new Tag("Track") { Icon = new IconInfo("\uEC4F") }] : [];
        MoreCommands =
        [
            new CommandContextItem(
                new PlayTrackCommand(track.Uri, Utils.Text.TrackFullName(track), enqueue: true)
            )
            {
                Title = "Add to queue",
                Icon = new IconInfo("\uE710"),
            },
            new CommandContextItem(new OpenUrlCommand(track.Uri ?? ""))
            {
                Title = "Open in Spotify",
            },
            new CommandContextItem(new OpenUrlCommand(track.Album?.Uri ?? ""))
            {
                Icon = new IconInfo("\uE93C"),
                Title =
                    track.Album?.Name == track.Name
                        ? "Open album in Spotify"
                        : $"Open {track.Album?.Name} in Spotify",
            },
        ];
        Details = new Details
        {
            HeroImage = new IconInfo(track.Album?.Images?.FirstOrDefault()?.Url ?? "\uEC4F"),
            Title = track.Name ?? "Unnamed track",
            Body = Utils.Text.Artists(track),
            Metadata =
            [
                track.Album != null
                    ? new DetailsElement
                    {
                        Key = $"Track {track.TrackNumber} on",
                        Data = new DetailsLink(track.Album.Uri ?? "", track.Album.Name),
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
                        TimeSpan.FromMilliseconds(track.DurationMs).ToString(@"m\:ss")
                    ),
                },
                new DetailsElement
                {
                    Key = "Track ID",
                    Data = new DetailsLink(track.Uri ?? "", track.Id ?? "?"),
                },
            ],
        };
    }
}
