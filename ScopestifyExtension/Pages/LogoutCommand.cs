using Microsoft.CommandPalette.Extensions.Toolkit;

namespace ScopestifyExtension;

internal sealed partial class LogoutCommand : InvokableCommand
{
    public override string Name => "Logout from Spotify";
    public override IconInfo Icon => new("\uF3B1");

    public override CommandResult Invoke()
    {
        AuthenticatedSpotifyClient.LogOut();
        return CommandResult.ShowToast(
            new ToastArgs { Message = "Logged out", Result = CommandResult.GoHome() }
        );
    }
}
