using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

namespace ScopestifyExtension.Utils;

public static partial class Albums
{
    public static string AlbumType(SimpleAlbum album)
    {
        return album.AlbumType switch
        {
            "album" => "Album",
            "single" => "Single",
            "compilation" => "Compilation",
            _ => "Unknown",
        };
    }

    public static string AlbumType(FullAlbum album)
    {
        return AlbumType(new SimpleAlbum { AlbumType = album.AlbumType });
    }
}
