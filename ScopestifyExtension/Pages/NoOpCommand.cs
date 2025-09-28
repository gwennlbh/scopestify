using Microsoft.CommandPalette.Extensions.Toolkit;

namespace ScopestifyExtension;

/// <summary>
/// A command that does nothing, used for display-only list items
/// </summary>
internal sealed partial class NoOpCommand : InvokableCommand
{
    public override string Name => "No operation";
    public override IconInfo Icon => new("\uE7BA"); // Info icon

    public override CommandResult Invoke()
    {
        return CommandResult.GoHome();
    }
}