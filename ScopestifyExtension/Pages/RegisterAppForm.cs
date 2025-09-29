namespace ScopestifyExtension.Pages;

using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

internal sealed partial class RegisterApp : ContentPage
{
    private readonly RegisterAppFormContent form = new();

    public override IContent[] GetContent() => [form];

    public RegisterApp()
    {
        Name = "Open";
        Title = "Sample Content";
        Icon = new IconInfo("\uECA5"); // Tiles
    }
}

internal sealed partial class RegisterAppFormContent : FormContent
{
    public RegisterAppFormContent()
    {
        TemplateJson = $$"""
{
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "type": "AdaptiveCard",
    "version": "1.6",
    "body": [
        {
            "type": "TextBlock",
            "text": "Register your App",
            "weight": "Bolder",
            "size": "Medium"
        },
        {
            "type": "TextBlock",
            "text": "To use Scopestify, you need to register an application on the Spotify Developer Dashboard to obtain a Client ID and Client Secret.",
            "wrap": true
        },
        {
            "type": "TextBlock",
            "text": "Be sure to add {{AuthenticatedSpotifyClient.callbackUri}} as an authorized Redirect URI!",
            "wrap": true,
            "color": "attention"
        },
        {
            "type": "Action.OpenUrl",
            "title": "Go to Spotify Developer Dashboard",
            "url": "https://developer.spotify.com/dashboard/applications"
        },
        {
            "type": "Input.Text",
            "label": "Client ID",
            "style": "text",
            "id": "ClientId",
            "isRequired": true,
            "errorMessage": "Client ID is required",
            "placeholder": "Enter your app's client ID"
        },
        {
            "type": "Input.Text",
            "label": "Client secret",
            "style": "text",
            "id": "ClientSecret",
            "isRequired": true,
            "errorMessage": "Client Secret is required",
            "placeholder": "Enter your app's client secret"
        }
    ],
    "actions": [
        {
            "type": "Action.Submit",
            "title": "Submit",
            "data": {
                "id": "1234567890"
            }
        }
    ]
}
""";
    }

    private static PrivateUser? user;
    private static string errorMessage = "";

    public override CommandResult SubmitForm(string payload)
    {
        var formInput = JsonNode.Parse(payload)?.AsObject();
        Debug.WriteLine($"Form submitted with formInput: {formInput}");
        if (formInput == null)
        {
            return CommandResult.GoHome();
        }

        new ConfigurationFile
        {
            ClientId = formInput["ClientId"]?.ToString() ?? "",
            ClientSecret = formInput["ClientSecret"]?.ToString() ?? "",
        }.Save();

        Task.Run(LogIn).Wait();

        if (user != null)
        {
            return CommandResult.ShowToast(
                new ToastArgs { Message = $"Logged in as {user.DisplayName}" }
            );
        }
        else
        {
            return CommandResult.ShowToast(
                new ToastArgs { Message = $"Login failed: {errorMessage}" }
            );
        }
    }

    private static async Task LogIn()
    {
        (user, errorMessage) = await AuthenticatedSpotifyClient.LogIn();
    }
}
