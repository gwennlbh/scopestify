using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

namespace ScopestifyExtension;

sealed partial class PlayAlbumCommand(string uri, string name, bool? enqueue) : InvokableCommand
{
    public override string Name => enqueue ? "Add album's tracks to queue" : "Play album";
    public override IconInfo Icon => new("\uE768");

    private readonly SpotifyClient spotify = AuthenticatedSpotifyClient.Get();

    private readonly string uri = uri;
    private readonly bool enqueue = enqueue ?? false;
    private readonly string name = name;

    private async Task Run()
    {
        if (enqueue)
        {
            var album = await spotify.Albums.Get(uri.Split(':').Last());
            var tracksPage = await spotify.Albums.GetTracks(album.Id);
            var tracksAllPages = await spotify.PaginateAll(tracksPage);
            var tracks = tracksAllPages?.OrderBy(t => t.TrackNumber).ToArray() ?? [];
            foreach (var track in tracks)
            {
                await spotify.Player.AddToQueue(new PlayerAddToQueueRequest(track.Uri));
            }
            return;
        }
        else
        {
            await spotify.Player.ResumePlayback(
                new PlayerResumePlaybackRequest { ContextUri = uri }
            );
        }
    }

    public override CommandResult Invoke()
    {
        try
        {
            Task.Run(Run).Wait();
            return CommandResult.ShowToast(
                new ToastArgs { Message = enqueue ? $"Queuing {name}" : $"Playing {name}" }
            );
        }
        catch (Exception ex)
        {
            return CommandResult.ShowToast(new ToastArgs { Message = ex.Message });
        }
    }
}
