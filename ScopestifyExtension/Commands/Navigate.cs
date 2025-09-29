namespace ScopestifyExtension.Commands;

using Microsoft.CommandPalette.Extensions.Toolkit;

internal sealed partial class NavigateTo(ListPage page) : InvokableCommand
{
    public override string Name => $"Go to {page.Name}";
    public override IconInfo Icon => page.Icon;

    private ListPage page = page;

    public override CommandResult Invoke()
    {
        return CommandResult.GoToPage(new GoToPageArgs { PageId = page.Id });
    }
}
