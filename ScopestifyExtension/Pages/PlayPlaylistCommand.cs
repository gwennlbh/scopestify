using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

namespace ScopestifyExtension;

sealed partial class PlayPlaylistCommand(string uri, string name, bool? enqueue) : InvokableCommand
{
    public override string Name => enqueue ? "Add playlist's tracks to queue" : "Play playlist";
    public override IconInfo Icon => new("\uE768");

    private readonly SpotifyClient spotify = AuthenticatedSpotifyClient.Get();

    private readonly string uri = uri;
    private readonly bool enqueue = enqueue ?? false;
    private readonly string name = name;

    private async Task Run()
    {
        if (enqueue)
        {
            var playlist = await spotify.Playlists.Get(uri.Split(':').Last());
            var tracksPage = await spotify.Playlists.GetItems(playlist.Id);
            var tracks =
                tracksPage
                    ?.Items?.Where(t => t.Track.Type == ItemType.Track)
                    .Select(t => t.Track as FullTrack)
                    .OrderBy(t => t?.TrackNumber)
                    .ToArray() ?? [];

            foreach (var track in tracks)
            {
                if (track == null)
                    continue;
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
