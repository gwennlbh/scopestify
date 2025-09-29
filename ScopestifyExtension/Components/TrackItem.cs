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
        Subtitle = Utils.Text.InfoLine(Utils.Text.Artists(track), track.Album?.Name ?? "No album");
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
            new CommandContextItem(new Pages.AlbumTracks(track.Id)) { Title = "See album tracks" },
            // TODO causes wayy to many data fetches, MyPlaylists' data should be cached
            // new CommandContextItem(new Pages.MyPlaylists(trackToAdd: track))
            // {
            //     Title = "Add to a playlist",
            //     Icon = Icons.Add,
            // },
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
                    Data = new DetailsLink(Utils.Text.Duration(track.DurationMs, "?")),
                },
                new DetailsElement
                {
                    Key = "Track ID",
                    Data = new DetailsLink(track.Uri ?? "", track.Id ?? "?"),
                },
            ],
        };
    }

    public TrackItem(SimpleTrack track, SimpleAlbum album, bool typeTag)
        : this(
            new FullTrack
            {
                Artists = track.Artists,
                Album = album,
                Name = track.Name,
                Uri = track.Uri,
                Id = track.Id,
                DurationMs = track.DurationMs,
                TrackNumber = track.TrackNumber,
            },
            typeTag
        ) { }

    public TrackItem(SimpleTrack track, FullAlbum album, bool typeTag)
        : this(
            new FullTrack
            {
                Artists = track.Artists,
                Album = new SimpleAlbum
                {
                    Name = album.Name,
                    Uri = album.Uri,
                    Id = album.Id,
                    Images = album.Images,
                    Artists = album.Artists,
                },
                Name = track.Name,
                Uri = track.Uri,
                Id = track.Id,
                DurationMs = track.DurationMs,
                TrackNumber = track.TrackNumber,
            },
            typeTag
        ) { }
}
