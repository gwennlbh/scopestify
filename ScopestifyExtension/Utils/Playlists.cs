namespace ScopestifyExtension.Utils;

using System.Globalization;
using System.Linq;
using System.Threading;
using SpotifyAPI.Web;

public partial class Playlists
{
    public static string DiscoverWeeklyURI = "spotify:playlist:37i9dQZEVXcW4o4O4AqUHN";

    public static FullPlaylist DiscoverWeekly()
    {
        CultureInfo culture = Thread.CurrentThread.CurrentCulture;
        return new()
        {
            Name = "Discover Weekly",
            Uri = DiscoverWeeklyURI,
            Id = DiscoverWeeklyURI.Split(':').Last(),
            Images =
            [
                new Image
                {
                    Url =
                        $"https://pickasso.spotifycdn.com/image/ab67c0de0000deef/dt/v1/img/dw/cover/{culture.TwoLetterISOLanguageName}",
                    Height = 320,
                    Width = 320,
                },
            ],
            Collaborative = false,
            Description =
                "Your shortcut to hidden gems, deep cuts, and future faves, updated every Monday. You'll know when you hear it.",
            Owner = new PublicUser { DisplayName = "Spotify", Id = "" },
            Type = "playlist",
        };
    }

    public static string ReleaseRadarURI = "spotify:playlist:37i9dQZEVXbqiZEgvMyJMk"; // Release Radar

    public static FullPlaylist ReleaseRadar()
    {
        CultureInfo culture = Thread.CurrentThread.CurrentCulture;
        return new()
        {
            Name = "Release Radar",
            Uri = ReleaseRadarURI,
            Id = ReleaseRadarURI.Split(':').Last(),
            Images =
            [
                new Image
                {
                    Url =
                        $"https://newjams-images.scdn.co/image/ab67647800003f8a/dt/v3/release-radar/ab6761610000e5ebe412a782245eb20d9626c601/{culture.TwoLetterISOLanguageName}",
                    Height = 320,
                    Width = 320,
                },
            ],
            Collaborative = false,
            Description =
                "A personalized playlist of new releases, updated every Friday. You'll know when you hear it.",
            Owner = new PublicUser { DisplayName = "Spotify", Id = "" },
            Type = "playlist",
        };
    }
}
