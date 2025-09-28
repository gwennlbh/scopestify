using System.Linq;
using SpotifyAPI.Web;

namespace ScopestifyExtension;

public class Utils
{
    public static string TrackFullName(FullTrack? track)
    {
        if (track == null)
        {
            return "Unknown track";
        }

        var artists = string.Join(", ", track.Artists?.Select(a => a.Name) ?? []);
        return $"{artists} – {track.Name}";
    }
}
