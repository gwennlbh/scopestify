namespace ScopestifyExtension.Utils;

using System.Linq;
using SpotifyAPI.Web;

public class Text
{
    public static string Artists(FullTrack? track)
    {
        if (track == null)
        {
            return "Unknown artist";
        }

        return string.Join(" × ", track.Artists?.Select(a => a.Name) ?? []);
    }

    public static string TrackFullName(FullTrack? track)
    {
        if (track == null)
        {
            return "Unknown track";
        }

        return $"{Artists(track)} – {track.Name}";
    }

    public static string AlbumFullName(FullAlbum? album)
    {
        return AlbumFullName(
            album == null ? null : new FullAlbum { Artists = album.Artists, Name = album.Name }
        );
    }

    public static string AlbumFullName(SimpleAlbum? album)
    {
        if (album == null)
        {
            return "Unknown album";
        }

        return $"{Artists(new FullTrack { Artists = album.Artists })} – {album.Name}";
    }
}
