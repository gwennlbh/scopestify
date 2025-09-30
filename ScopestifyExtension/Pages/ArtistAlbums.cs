namespace ScopestifyExtension.Pages;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

internal sealed partial class ArtistAlbums : ListPage
{
    private string artistId;
    private FullArtist? artist;
    private SimpleAlbum[] albums = [];
    private string errorMessage = "";

    public ArtistAlbums(string artistId)
    {
        this.artistId = artistId;
        Id = $"artist-albums-{artistId}";
        Name = "Artist's albums";
        Icon = Icons.Artist;
        ShowDetails = true;
        // Does nothing, maybe because of ShowDetails = true?
        // Definition of GridProperties is commnented out below this class
        // GridProperties = new GridProperties(5, 4);
    }

    public override CommandItem? EmptyContent =>
        new(new NoOpCommand())
        {
            Title =
                errorMessage != "" ? "An error occured"
                : artist == null ? "No artist found"
                : albums.Length == 0 ? "Artist has no albums"
                : "No results",
            Subtitle =
                errorMessage != "" ? errorMessage
                : artist == null ? "Search for an artist to see their albums here"
                : albums.Length == 0 ? "This artist has no albums"
                : "",
            Icon = new IconInfo("🥀"),
        };

    public override ListItem[] GetItems()
    {
        try
        {
            Task.Run(LoadData).Wait();
        }
        catch (Exception ex)
        {
            errorMessage = ex.InnerException?.Message ?? ex.Message;
        }

        if (artist == null || albums.Length == 0)
        {
            return [];
        }

        Name = $"{artist.Name}'s albums";
        PlaceholderText = $"Albums by {artist.Name}";

        var items = albums
            .Select(album => new Components.AlbumItem(
                album,
                typeTag: false,
                mainCommand: new AlbumTracks(album.Id)
            )
            {
                Subtitle = Utils.Text.InfoLine(Utils.Albums.AlbumType(album), album.ReleaseDate),
                // Does nothing, idk why
                // Section = Utils.Albums.AlbumType(album),
            })
            .ToArray<ListItem>();

        return items;
    }

    private async Task LoadData()
    {
        var spotify = AuthenticatedSpotifyClient.Get();

        if (artist == null)
        {
            artist = await spotify.Artists.Get(artistId);
        }

        if (albums.Length == 0)
        {
            var albumsPage = await spotify.Artists.GetAlbums(
                artistId,
                new ArtistsAlbumsRequest
                {
                    Limit = 50,
                    IncludeGroupsParam =
                        ArtistsAlbumsRequest.IncludeGroups.Album
                        | ArtistsAlbumsRequest.IncludeGroups.Single
                        | ArtistsAlbumsRequest.IncludeGroups.Compilation,
                }
            );

            albums = albumsPage.Items?.ToArray() ?? [];
        }
    }
}

// class GridProperties(int height, int width) : IGridProperties
// {
//     public Windows.Foundation.Size TileSize { get; } =
//         new Windows.Foundation.Size { Height = height, Width = width };
// }
