using System;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

namespace ScopestifyExtension;

internal sealed partial class LoginCommand : InvokableCommand
{

    public override string Name => "Login to Spotify";
    public override IconInfo Icon => new("\uE8D7");

    private static EmbedIOAuthServer? authServer;

    private static PrivateUser? user;
    private static string errorMessage = "";

    public override CommandResult Invoke()
    {
        try
        {
            Task.Run(Run).Wait();

            if (user != null)
            {
                return CommandResult.ShowToast(new ToastArgs { Message = $"Logged in as {user.DisplayName}" });
            }
            else
            {
                return CommandResult.ShowToast(new ToastArgs { Message = $"Login failed: {errorMessage}" });
            }
        }
        catch (Exception ex)
        {

            return CommandResult.ShowToast(new ToastArgs { Message = ex.Message });
        }
    }

    private async Task Run()
    {
        (user, errorMessage) = await AuthenticatedSpotifyClient.LogIn();
    }

}
