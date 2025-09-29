namespace ScopestifyExtension.Components;

using System.Linq;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

public partial class AlbumItem : ListItem
{
    public AlbumItem(SimpleAlbum album, bool typeTag)
    {
        Command = new Commands.PlayAlbum(album.Uri, Utils.Text.AlbumFullName(album), enqueue: false);
        Title = album.Name ?? "Unnamed album";
        Subtitle = Utils.Text.Artists(new FullTrack { Artists = album.Artists });
        Icon = new IconInfo(album.Images?.FirstOrDefault()?.Url ?? "\uE7C3");
        Tags = typeTag ? [new Tag("Album") { Icon = new IconInfo("\uE93C") }] : [];
        MoreCommands =
        [
            new CommandContextItem(
                new Commands.PlayAlbum(album.Uri, Utils.Text.AlbumFullName(album), enqueue: true)
            )
            {
                Title = "Add to queue",
                Icon = new IconInfo("\uE710"),
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
