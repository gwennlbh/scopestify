using System.Linq;
using SpotifyAPI.Web;

namespace ScopestifyExtension;

public class Utils
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
}
