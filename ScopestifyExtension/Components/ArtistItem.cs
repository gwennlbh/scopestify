namespace ScopestifyExtension.Components;

using System.Linq;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

public partial class ArtistItem : ListItem
{
    public ArtistItem(FullArtist artist, bool typeTag)
    {
        Command = new Pages.ArtistAlbums(artist.Id) { Name = $"See {artist.Name}'s albums" };
        Title = artist.Name ?? "Unnamed artist";
        Subtitle = string.Join(", ", artist.Genres ?? []);
        Icon = Icons.WithFallback(artist.Images?.FirstOrDefault()?.Url, Icons.Artist);
        Tags = typeTag ? [new Tag("Artist") { Icon = Icons.Artist }] : [];
        MoreCommands =
        [
            new CommandContextItem(new Commands.FollowArtist(artist))
            {
                Title = "Follow",
                Icon = Icons.AddFriend,
            },
            new CommandContextItem(new OpenUrlCommand(artist.Uri ?? ""))
            {
                Title = "Open in Spotify",
            },
        ];
        Details = new Details
        {
            Title = artist.Name ?? "Unnamed artist",
            Body = string.Join(", ", artist.Genres ?? []),
            HeroImage = Icon,
            Metadata =
            [
                new DetailsElement
                {
                    Key = "Followers",
                    Data = new DetailsLink(artist.Followers.Total.ToString() ?? "?"),
                },
                new DetailsElement
                {
                    Key = "Popularity",
                    Data = new DetailsLink((artist.Popularity.ToString() ?? "?") + "/100"),
                },
            ],
        };
    }
}
