namespace ScopestifyExtension.Utils;

using System.Threading.Tasks;
using SpotifyAPI.Web;

public class Playback
{
    public static async Task<bool> HasPlaybackContext(SpotifyClient spotify)
    {
        var currentPlayback = await spotify.Player.GetCurrentPlayback();
        return currentPlayback?.Context != null;
    }
}
