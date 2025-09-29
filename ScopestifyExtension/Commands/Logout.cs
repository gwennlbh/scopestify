using Microsoft.CommandPalette.Extensions.Toolkit;

namespace ScopestifyExtension.Commands;

internal sealed partial class Logout : InvokableCommand
{
    public override string Name => "Logout from Spotify";
    public override IconInfo Icon => Icons.SignOut;

    public override CommandResult Invoke()
    {
        AuthenticatedSpotifyClient.LogOut();
        return CommandResult.ShowToast(
            new ToastArgs { Message = "Logged out", Result = CommandResult.GoHome() }
        );
    }
}
