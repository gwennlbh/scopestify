using System;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

namespace ScopestifyExtension;

sealed partial class PlayTrackCommand(
    string uri,
    string name,
    string? contextUri = null,
    bool? enqueue = null
) : InvokableCommand
{
    public override string Name => enqueue ? "Add track to queue" : "Play track";
    public override IconInfo Icon => new("\uE768");

    private readonly SpotifyClient spotify = AuthenticatedSpotifyClient.Get();

    private readonly string uri = uri;
    private readonly string? contextUri = contextUri;
    private readonly string name = name;
    private readonly bool enqueue = enqueue ?? false;

    private async Task Run()
    {
        if (enqueue)
        {
            await spotify.Player.AddToQueue(new PlayerAddToQueueRequest(uri));
        }
        else
        {
            await spotify.Player.ResumePlayback(
                new PlayerResumePlaybackRequest { Uris = [uri], ContextUri = contextUri }
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
