namespace ScopestifyExtension.Components;

using System.Linq;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

public partial class AlbumItem : ListItem
{
    public AlbumItem(SimpleAlbum album, bool typeTag)
    {
        Command = new Commands.PlayAlbum(
            album.Uri,
            Utils.Text.AlbumFullName(album),
            enqueue: false
        );
        Title = album.Name ?? "Unnamed album";
        Subtitle = Utils.Text.Artists(new FullTrack { Artists = album.Artists });
        Icon = Icons.WithFallback(album.Images?.FirstOrDefault()?.Url, Icons.MusicAlbum);
        Tags = typeTag ? [new Tag("Album") { Icon = Icons.MusicAlbum }] : [];
        MoreCommands =
        [
            new CommandContextItem(
                new Commands.PlayAlbum(album.Uri, Utils.Text.AlbumFullName(album), enqueue: true)
            )
            {
                Title = "Add to queue",
                Icon = Icons.Import,
            },
            new CommandContextItem(new OpenUrlCommand(album.Uri ?? ""))
            {
                Title = "Open in Spotify",
            },
        ];
        Details = new Details
        {
            Title = album.Name ?? "Unnamed album",
            Body = $"Album by {Utils.Text.Artists(new FullTrack { Artists = album.Artists })}",
            HeroImage = Icon,
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
                    Data = new DetailsLink(album.TotalTracks.ToString() ?? "?"),
                },
                new DetailsElement
                {
                    Key = "Album ID",
                    Data = new DetailsLink(album.Uri ?? "", album.Id ?? "?"),
                },
            ],
        };
    }
}
