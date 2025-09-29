using Microsoft.CommandPalette.Extensions.Toolkit;

public static class Icons
{
    public static IconInfo WithFallback(string? contents, IconInfo fallback) =>
        contents == null || contents == "" ? fallback : new(contents);

    public static IconInfo Add = new("\uE710");
    public static IconInfo Import = new("\uE8B5");
    public static IconInfo Heart = new("\uEB51");
    public static IconInfo HeartBroken = new("\uEA92");
    public static IconInfo Permissions = new("\uE8D7");
    public static IconInfo SignOut = new("\uF3B1");
    public static IconInfo MusicAlbum = new("\uE93C");
    public static IconInfo Play = new("\uE768");
    public static IconInfo MusicInfo = new("\uE90B");
    public static IconInfo Contact = new("\uE77B");
    public static IconInfo VolumeBars = new("\uEBC5");
    public static IconInfo Search = new("\uE721");
    public static IconInfo MusicNote = new("\uEC4F");
    public static IconInfo Audio = new("\uE8D6");
    public static IconInfo CalculatorSubtract = new("\uE949");
    public static IconInfo Tiles = new("\uECA5");
    public static IconInfo Group = new("\uE902");
    public static IconInfo Sync = new("\uE895");
    public static IconInfo Error = new("\uE783");

    // Aliases

    public static IconInfo Playlist = MusicInfo;
    public static IconInfo You = Contact;

    // TODO differentiate from You
    public static IconInfo Artist = Contact;
}
