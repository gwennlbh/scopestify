namespace ScopestifyExtension.Pages;

using System.Linq;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

internal sealed partial class Artists(FullArtist[] artists) : ListPage
{
    public override string Name => "Artists";
    public override string Title => "Artists";
    public override IconInfo Icon => artists.Length > 1 ? Icons.Group : Icons.Artist;
    public override bool ShowDetails => true;

    private FullArtist[] artists = artists;

    public override ListItem[] GetItems() =>
        [.. artists.Select(artist => new Components.ArtistItem(artist, typeTag: false))];
}
