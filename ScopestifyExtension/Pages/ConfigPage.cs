using System;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace ScopestifyExtension;

/// <summary>
/// Configuration page showing current settings and options
/// </summary>
internal sealed partial class ConfigPage : ListPage
{
    private ConfigurationFile config = new();

    public ConfigPage()
    {
        Name = "Configuration";
        Icon = new IconInfo("\uE713"); // Settings icon
        Title = "Scopestify Configuration";
        
        // Refresh config on page load
        config = new ConfigurationFile();
    }

    public override ListItem[] GetItems()
    {
        // Refresh configuration to get latest values
        config = new ConfigurationFile();

        var items = new ListItem[]
        {
            // Configuration file location
            new ListItem(new NoOpCommand())
            {
                Title = "Configuration File",
                Subtitle = ConfigurationFile.Path(),
                Icon = new IconInfo("\uE8A5"), // Document icon
            },

            // Open config file action
            new ListItem(new OpenConfigFileCommand())
            {
                Title = "Open Config File",
                Subtitle = "Open configuration file in default editor",
                Icon = new IconInfo("\uE70F"), // Edit icon
            },

            // Section separator
            new ListItem(new NoOpCommand())
            {
                Title = "Post-like Hook Configuration",
                Subtitle = "Command executed after liking a track",
                Icon = new IconInfo("\uE9CE"), // Hook/link icon
            },

            // Current post-like hook command
            new ListItem(new NoOpCommand())
            {
                Title = string.IsNullOrEmpty(config.PostLikeHook) ? "Not configured" : config.PostLikeHook,
                Subtitle = string.IsNullOrEmpty(config.PostLikeHook) ? 
                    "No post-like hook command set" : 
                    "Current post-like hook command",
                Icon = new IconInfo(string.IsNullOrEmpty(config.PostLikeHook) ? "\uE7BA" : "\uE73E"), // Info or CheckMark
            },

            // Current working directory
            new ListItem(new NoOpCommand())
            {
                Title = string.IsNullOrEmpty(config.PostLikeHookCwd) ? "Default directory" : config.PostLikeHookCwd,
                Subtitle = string.IsNullOrEmpty(config.PostLikeHookCwd) ? 
                    "No working directory specified" : 
                    "Current working directory for hook",
                Icon = new IconInfo("\uE8B7"), // Folder icon
            },

            // Configure post-like hook action
            new ListItem(new ConfigFormPage())
            {
                Title = "Configure Post-like Hook",
                Subtitle = "Set up command to run after liking tracks",
                Icon = new IconInfo("\uE8B8"), // FolderOpen icon (represents settings/configuration)
            },
        };

        return items;
    }
}