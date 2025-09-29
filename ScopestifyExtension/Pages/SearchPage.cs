namespace ScopestifyExtension;

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

internal sealed partial class SearchPage : DynamicListPage, IDisposable
{
    private readonly SpotifyClient spotify = AuthenticatedSpotifyClient.Get();

    private CancellationTokenSource _cancellation = new();
    private readonly Lock _lock = new();

    public void Dispose()
    {
        _cancellation.Cancel();
    }

    private PrivateUser? currentUser;
    private FullTrack[] tracks = [];
    private FullPlaylist[] playlists = [];
    private SimpleAlbum[] albums = [];
    private string errorMessage = "";

    private bool showTypes = true;

    public SearchPage()
    {
        Name = "Search on Spotify";
        Icon = new("\uE721");
        ShowDetails = true;
    }

    public override ICommandItem? EmptyContent =>
        new CommandItem(new NoOpCommand())
        {
            Title = SearchText == "" ? "Type to search on Spotify" : "No results found",
            Subtitle = "Use prefixes 't:', 'a:' or 'p:' to search only tracks, albums or playlists",
            Icon = new IconInfo("\uE721"),
        };

    public override void UpdateSearchText(string _old, string newText)
    {
        IsLoading = false;
        if (_old == newText)
        {
            return;
        }

        Debug.WriteLine($"Search text updated: '{_old}' -> '{newText}'");
        CancellationTokenSource cts;
        SearchText = newText;

        lock (_lock)
        {
            _cancellation.Cancel();
            _cancellation = new CancellationTokenSource();
            cts = _cancellation;
        }
        IsLoading = true;
        Task.Run(async () => await LoadData(newText)).Wait();
        IsLoading = false;
    }

    public override ListItem[] GetItems()
    {
        if (!string.IsNullOrEmpty(errorMessage))
        {
            return
            [
                new ListItem(new NoOpCommand())
                {
                    Title = errorMessage,
                    Icon = new IconInfo("\uE783"),
                },
            ];
        }

        ListItem[] items =
        [
            .. tracks.Select(track => new Components.TrackItem(track, typeTag: showTypes)),
            .. albums.Select(album => new Components.AlbumItem(album, typeTag: showTypes)),
            .. playlists
                .Where(playlist => playlist != null)
                .Select(playlist => new Components.PlaylistItem(
                    playlist,
                    currentUser,
                    typeTag: showTypes,
                    highlightYours: true
                )),
        ];

        // Sort by closest match to start of string
        return
        [
            .. items.OrderBy(item =>
            {
                var title = item.Title?.ToLower().Trim() ?? "";
                var search = SearchText.ToLower().Trim();

                if (item.Subtitle.Contains("Your Playlist"))
                {
                    return 0;
                }
                if (title == search)
                {
                    return 1;
                }
                if (item.Subtitle.Contains("playlist"))
                {
                    return 4;
                }
                if (title.StartsWith(search))
                {
                    return 2;
                }
                if (title.Contains(search))
                {
                    return 3;
                }
                return 5;
            }),
        ];
    }

    private async Task LoadData(string query)
    {
        if (currentUser == null)
        {
            try
            {
                currentUser = await spotify.UserProfile.Current();
            }
            catch (Exception)
            { /* ignore */
            }
        }

        Debug.WriteLine($"Loading data for search '{SearchText}'");
        if (string.IsNullOrWhiteSpace(query))
        {
            tracks = [];
            albums = [];
            playlists = [];
            errorMessage = "";
            RaiseItemsChanged();
            return;
        }

        try
        {
            IsLoading = true;

            var OnlyTracks = query.StartsWith("t:");
            var OnlyAlbums = query.StartsWith("a:");
            var OnlyPlaylists = query.StartsWith("p:");

            if (OnlyTracks || OnlyAlbums || OnlyPlaylists)
            {
                query = query[2..].Trim();
                showTypes = false;
            }
            else
            {
                showTypes = true;
            }

            var EnableTracksSearch = !OnlyAlbums && !OnlyPlaylists;
            var EnableAlbumsSearch = !OnlyTracks && !OnlyPlaylists;
            var EnablePlaylistsSearch = !OnlyTracks && !OnlyAlbums;

            var searchTrack = EnableTracksSearch
                ? await spotify.Search.Item(
                    new SearchRequest(SearchRequest.Types.Track, query),
                    _cancellation.Token
                )
                : null;
            var searchAlbums = EnableAlbumsSearch
                ? await spotify.Search.Item(
                    new SearchRequest(SearchRequest.Types.Album, query),
                    _cancellation.Token
                )
                : null;
            var searchPlaylist = EnablePlaylistsSearch
                ? await spotify.Search.Item(
                    new SearchRequest(SearchRequest.Types.Playlist, query),
                    _cancellation.Token
                )
                : null;

            IsLoading = false;

            tracks = [.. searchTrack?.Tracks.Items ?? []];
            Debug.WriteLine($"Found {tracks.Length} tracks");
            albums = [.. searchAlbums?.Albums.Items ?? []];
            Debug.WriteLine($"Found {albums.Length} albums");
            playlists = [.. searchPlaylist?.Playlists.Items ?? []];
            Debug.WriteLine($"Found {playlists.Length} playlists");
            RaiseItemsChanged(tracks.Length + albums.Length + playlists.Length);
            errorMessage = "";
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
            {
                // Ignore cancellations
                return;
            }
            errorMessage = $"Error searching for '{query}': {ex.Message}";
        }
    }
}
