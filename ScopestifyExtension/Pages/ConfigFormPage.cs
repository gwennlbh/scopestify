using System.Text.Json.Nodes;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace ScopestifyExtension;

/// <summary>
/// Form page for configuring post-like hook settings
/// </summary>
internal sealed partial class ConfigFormPage : ContentPage
{
    private readonly ConfigFormContent form = new();

    public override IContent[] GetContent() => [form];

    public ConfigFormPage()
    {
        Name = "Configuration";
        Title = "Configure Post-like Hook";
        Icon = new IconInfo("\uE713"); // Settings icon
    }
}

internal sealed partial class ConfigFormContent : FormContent
{
    public ConfigFormContent()
    {
        var config = new ConfigurationFile();
        
        TemplateJson = $$$"""
{
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "type": "AdaptiveCard",
    "version": "1.6",
    "body": [
        {
            "type": "TextBlock",
            "text": "Configure Post-like Hook",
            "weight": "Bolder",
            "size": "Medium"
        },
        {
            "type": "TextBlock",
            "text": "Set up a command to run automatically after liking a track on Spotify. This could be used to log liked tracks, trigger notifications, or run custom scripts.",
            "wrap": true
        },
        {
            "type": "Input.Text",
            "label": "Post-like Hook Command",
            "style": "text",
            "id": "PostLikeHook",
            "placeholder": "e.g., echo 'Liked track!', powershell -Command 'Write-Host Track liked', or /usr/bin/logger 'Track liked'",
            "value": "{{{config.PostLikeHook}}}",
            "isMultiline": false
        },
        {
            "type": "TextBlock",
            "text": "The command to execute after liking a track. Leave empty to disable.",
            "size": "Small",
            "isSubtle": true,
            "wrap": true
        },
        {
            "type": "Input.Text",
            "label": "Working Directory",
            "style": "text",
            "id": "PostLikeHookCwd",
            "placeholder": "e.g., C:\\Scripts or /home/user/scripts",
            "value": "{{{config.PostLikeHookCwd}}}",
            "isMultiline": false
        },
        {
            "type": "TextBlock",
            "text": "Optional: Specify the working directory for the command.",
            "size": "Small",
            "isSubtle": true,
            "wrap": true
        }
    ],
    "actions": [
        {
            "type": "Action.Submit",
            "title": "Save Configuration",
            "data": {
                "action": "save"
            }
        }
    ]
}
""";
    }

    public override CommandResult SubmitForm(string payload)
    {
        var formInput = JsonNode.Parse(payload)?.AsObject();
        if (formInput == null)
        {
            return CommandResult.ShowToast(
                new ToastArgs { Message = "Failed to parse form data" }
            );
        }

        try
        {
            var config = new ConfigurationFile();
            config.PostLikeHook = formInput["PostLikeHook"]?.ToString() ?? "";
            config.PostLikeHookCwd = formInput["PostLikeHookCwd"]?.ToString() ?? "";
            config.Save();

            return CommandResult.ShowToast(
                new ToastArgs { Message = "Configuration saved successfully" }
            );
        }
        catch (System.Exception ex)
        {
            return CommandResult.ShowToast(
                new ToastArgs { Message = $"Failed to save configuration: {ex.Message}" }
            );
        }
    }
}