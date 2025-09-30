namespace ScopestifyExtension.Pages;

using System.Linq;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

internal sealed partial class TrackArtists(string trackId, string name = "") : ListPage
{
    private FullArtist[] artists;

    public override string Name => name == "" ? "Artists of track" : $"Artists of {name}";
    public override string Title => Name;
    public override IconInfo Icon => Icons.Group;

    public override ListItem[] GetItems()
    {
        Task.Run(async () =>
            {
                var spotify = AuthenticatedSpotifyClient.Get();
                var track = await spotify.Tracks.Get(trackId);
                artists =
                    (track?.Artists == null || track.Artists.Count == 0)
                        ? []
                        : await Task.WhenAll(
                            track.Artists.Select(async artistRef =>
                                await spotify.Artists.Get(artistRef.Id)
                            )
                        );
            })
            .Wait();

        return [.. artists.Select(a => new Components.ArtistItem(a, typeTag: false))];
    }
}
