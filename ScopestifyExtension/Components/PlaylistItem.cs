using System.Linq;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

namespace ScopestifyExtension.Components;

public partial class PlaylistItem : ListItem
{
    public PlaylistItem(
        FullPlaylist playlist,
        PrivateUser? currentUser,
        bool typeTag,
        bool highlightYours
    )
    {
        var hasTracks = playlist.Tracks?.Total > 0;

        Command = hasTracks
            ? new Commands.PlayPlaylist(playlist?.Uri ?? "", playlist?.Name ?? "", enqueue: false)
            : new NoOpCommand();

        Title = playlist?.Name ?? "Unnamed playlist";

        Subtitle = string.Join(
            " • ",
            new string[]
            {
                playlist?.Owner?.Id == currentUser?.Id ? (highlightYours ? "Your playlist" : "")
                : playlist?.Owner?.Id != null ? $"By {playlist?.Owner?.DisplayName}"
                : "",
                playlist?.Tracks?.Total != null ? $"{playlist?.Tracks.Total} tracks" : "",
            }.Where(s => !string.IsNullOrEmpty(s))
        );

        Icon = new IconInfo(playlist?.Images?.FirstOrDefault()?.Url ?? "\uF147");

        Tags = typeTag ? [new Tag("Playlist") { Icon = new IconInfo("\uE90B") }] : [];
        if (highlightYours && playlist?.Owner?.Id == currentUser?.Id)
        {
            Tags = [.. Tags, new Tag("Yours") { Icon = new IconInfo("\uE77B") }];
        }

        MoreCommands =
        [
            new CommandContextItem(new OpenUrlCommand(playlist.Uri ?? ""))
            {
                Title = "Open in Spotify",
            },
            new CommandContextItem(
                new Commands.PlayPlaylist(playlist?.Uri ?? "", playlist?.Name ?? "", enqueue: true)
            )
            {
                Title = "Add to queue",
                Icon = new IconInfo("\uE710"),
            },
            new CommandContextItem(
                new Commands.AddToPlaylist(playlist?.Id ?? "", playlist?.Name ?? "")
            )
            {
                Title = "Add current track",
                Icon = new IconInfo("\uE8B5"),
            },
        ];

        Details = new Details
        {
            Title = playlist?.Name ?? "Unnamed playlist",
            Body = playlist?.Description ?? "No description",
            HeroImage = new IconInfo(playlist?.Images?.FirstOrDefault()?.Url ?? "\uF147"),
            Metadata =
            [
                new DetailsElement
                {
                    Key = "By",
                    Data = new DetailsLink(
                        playlist.Owner.Uri ?? "",
                        playlist.Owner.DisplayName ?? "Unknown user"
                    ),
                },
                new DetailsElement
                {
                    Key = "Tracks",
                    Data = new DetailsLink(playlist?.Tracks?.Total.ToString() ?? "?"),
                },
                new DetailsElement
                {
                    Key = "Playlist ID",
                    Data = new DetailsLink(playlist?.Uri ?? "", playlist?.Id ?? "?"),
                },
            ],
        };
    }
}
