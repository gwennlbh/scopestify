namespace ScopestifyExtension.Utils;

using System;
using System.Linq;
using SpotifyAPI.Web;

public class Text
{
    public static string Artists(SimpleTrack? track)
    {
        if (track == null)
        {
            return "Unknown artist";
        }

        return string.Join(" × ", track.Artists?.Select(a => a.Name) ?? []);
    }

    public static string Artists(FullTrack? track)
    {
        return Artists(track == null ? null : new SimpleTrack { Artists = track.Artists });
    }

    public static string Artists(SimpleAlbum? album)
    {
        return Artists(album == null ? null : new SimpleTrack { Artists = album.Artists });
    }

    public static string Artists(FullAlbum? album)
    {
        return Artists(album == null ? null : new SimpleTrack { Artists = album.Artists });
    }

    public static string TrackFullName(SimpleTrack? track)
    {
        if (track == null)
        {
            return "Unknown track";
        }

        return $"{Artists(track)} – {track.Name}";
    }

    public static string TrackFullName(FullTrack? track)
    {
        return TrackFullName(
            track == null ? null : new SimpleTrack { Artists = track.Artists, Name = track.Name }
        );
    }

    public static string AlbumFullName(FullAlbum? album)
    {
        return AlbumFullName(
            album == null ? null : new SimpleAlbum { Artists = album.Artists, Name = album.Name }
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

    public static string InfoLine(params string[] parts)
    {
        return string.Join(" • ", parts.Where(p => !string.IsNullOrEmpty(p)));
    }

    public static string Duration(int? ms, string fallback = "")
    {
        if (ms == null)
        {
            return fallback;
        }

        return TimeSpan.FromMilliseconds(ms.Value).ToString(@"m\:ss");
    }
}
