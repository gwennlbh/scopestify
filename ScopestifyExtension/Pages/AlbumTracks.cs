namespace ScopestifyExtension.Pages;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

internal sealed partial class AlbumTracks : ListPage
{
    private string trackId;
    private Tag? tagTrackInList;
    private FullAlbum? album;
    private SimpleTrack[] tracks = [];
    private FullTrack? track;
    private string errorMessage = "";

    public AlbumTracks(string trackId, Tag? tagInList = null)
    {
        this.trackId = trackId;
        this.tagTrackInList = tagInList;
        Id = "currently-playing-album";
        Name = "Currently playing track's album";
        Icon = Icons.MusicAlbum;
        ShowDetails = true;
    }

    public override CommandItem? EmptyContent =>
        new(new NoOpCommand())
        {
            Title =
                errorMessage != "" ? "An error occured"
                : album == null ? "No album found"
                : "Album has no tracks",
            Subtitle =
                errorMessage != "" ? errorMessage
                : album == null ? "Play a track from an album to see its details here"
                : "This album has no tracks",
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

        if (album == null || tracks.Length == 0)
        {
            return [];
        }

        Name = Utils.Text.AlbumFullName(album);

        var hasMultipleDiscs = tracks.Select(t => t.DiscNumber).Distinct().Count() > 1;

        return
        [
            .. tracks.Select(t => new Components.TrackItem(t, album, typeTag: false)
            {
                Section = hasMultipleDiscs ? $"Disc {t.DiscNumber}" : "",
                Icon = new IconInfo(t.TrackNumber.ToString().PadLeft(2, '0')),
                Subtitle = Utils.Text.Duration(t.DurationMs),
                Tags = t.Id == track?.Id && tagTrackInList != null ? [tagTrackInList] : [],
            }),
        ];
    }

    private async Task LoadData()
    {
        var spotify = AuthenticatedSpotifyClient.Get();

        track = await spotify.Tracks.Get(trackId);

        if (track?.Album == null)
        {
            album = null;
            tracks = [];
            return;
        }

        if (album != null && album.Id == track.Album.Id)
        {
            // Already loaded
            return;
        }

        album = await spotify.Albums.Get(track.Album.Id);

        if (album == null)
        {
            tracks = [];
            return;
        }

        var albumTracksPage = await spotify.Albums.GetTracks(album.Id);
        tracks = [.. await spotify.PaginateAll(albumTracksPage)];
        // tracks = await Task.WhenAll(
        //     simpleTracks.Select(async st => await spotify.Tracks.Get(st.Id))
        // );
    }
}
