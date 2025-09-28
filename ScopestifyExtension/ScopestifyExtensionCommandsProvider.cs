// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace ScopestifyExtension;

public partial class ScopestifyExtensionCommandsProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;

    public ScopestifyExtensionCommandsProvider()
    {
        DisplayName = "Scopestify";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        _commands =
        [
            new CommandItem(new MyPlaylistsPage()) { Title = "See your playlists" },
            new CommandItem(new LikeCurrentTrackCommand()) { Title = "Like current track" },
            new CommandItem(new SearchPage()) { Title = "Search on Spotify" },
            new CommandItem(new LoginPage()) { Title = "Authenticate with Spotify" },
            new CommandItem(new CurrentTrackPage()) { Title = "See current track" },
        ];
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }
}
