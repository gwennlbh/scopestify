namespace ScopestifyExtension.Components;

using System;
using System.Linq;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

public partial class TrackItem : ListItem
{
    public TrackItem(FullTrack track, bool typeTag)
    {
        Command = new Commands.PlayTrack(
            track.Uri,
            Utils.Text.TrackFullName(track),
            enqueue: false
        );
        Title = track.Name ?? "Unnamed track";
        Subtitle = string.Join(" • ", [Utils.Text.Artists(track), track.Album?.Name ?? "No album"]);
        Icon = Icons.WithFallback(track.Album?.Images?.FirstOrDefault()?.Url, Icons.MusicNote);

        Tags = typeTag ? [new Tag("Track") { Icon = Icons.MusicNote }] : [];
        MoreCommands =
        [
            new CommandContextItem(
                new Commands.PlayTrack(track.Uri, Utils.Text.TrackFullName(track), enqueue: true)
            )
            {
                Title = "Add to queue",
            },
            new CommandContextItem(new OpenUrlCommand(track.Uri ?? ""))
            {
                Title = "Open in Spotify",
            },
            new CommandContextItem(new Commands.LikeTrack(track.Id))
            {
                Title = "Add to Liked Songs",
            },
            new CommandContextItem(new OpenUrlCommand(track.Album?.Uri ?? ""))
            {
                Title =
                    track.Album?.Name == track.Name
                        ? "Open album in Spotify"
                        : $"Open {track.Album?.Name} in Spotify",
            },
        ];
        Details = new Details
        {
            HeroImage = Icon,
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
