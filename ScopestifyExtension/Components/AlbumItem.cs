namespace ScopestifyExtension.Components;

using System.Linq;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

public partial class AlbumItem : ListItem
{
    public AlbumItem(SimpleAlbum album, bool typeTag, Command? mainCommand = null)
    {
        Command defaultMainCommand = new Commands.PlayAlbum(
            album.Uri,
            Utils.Text.AlbumFullName(album),
            enqueue: false
        );

        Command = mainCommand ?? defaultMainCommand;
        Title = album.Name ?? "Unnamed album";
        Subtitle = Utils.Text.Artists(album);
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
        if (mainCommand != null)
        {
            MoreCommands =
            [
                new CommandContextItem(defaultMainCommand) { Title = "Play album" },
                .. MoreCommands,
            ];
        }

        Details = new Details
        {
            Title = album.Name ?? "Unnamed album",
            Body = $"{Utils.Albums.AlbumType(album)} by {Utils.Text.Artists(album)}",
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
                    Key = "Type",
                    Data = new DetailsLink(Utils.Albums.AlbumType(album)),
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
