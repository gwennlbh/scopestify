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

        Subtitle = Utils.Text.InfoLine(
            playlist?.Owner?.Id == currentUser?.Id ? (highlightYours ? "Your playlist" : "")
                : playlist?.Owner?.Id != null ? $"By {playlist?.Owner?.DisplayName}"
                : "",
            Utils.Text.CountThing(playlist.Tracks?.Total ?? 0, "track")
        );

        Icon = Icons.WithFallback(playlist?.Images?.FirstOrDefault()?.Url, Icons.Playlist);

        Tags = typeTag ? [new Tag("Playlist") { Icon = Icons.Playlist }] : [];
        if (highlightYours && playlist?.Owner?.Id == currentUser?.Id)
        {
            Tags = [.. Tags, new Tag("Yours") { Icon = Icons.You }];
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
            },
            new CommandContextItem(
                new Commands.AddToPlaylist(playlist?.Id ?? "", playlist?.Name ?? "")
            )
            {
                Title = "Add current track",
            },
        ];

        Details = new Details
        {
            Title = playlist?.Name ?? "Unnamed playlist",
            Body = playlist?.Description ?? "No description",
            HeroImage = Icon,
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
