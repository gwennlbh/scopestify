using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using ScopestifyExtension;
using SpotifyAPI.Web;

internal sealed partial class LoginPage : ListPage
{
    private PrivateUser? currentUser;
    private Device? currentDevice;

    public LoginPage()
    {
        Name = "Authenticate to Spotify";
        Icon = new("\uE8D7");

        Task.Run(GetCurrentUser).Wait();
    }

    public override ListItem[] GetItems()
    {
        var items = Array.Empty<ListItem>();

        var config = new ConfigurationFile();

        if (currentUser != null)
        {
            items =
            [
                .. items,
                new ListItem(new NoOpCommand())
                {
                    Title = $"Logged in as {currentUser.DisplayName}",
                    Subtitle = string.Join(
                        " • ",
                        [
                            currentDevice != null
                                ? $"Currently playing on {currentDevice.Name}"
                                : "No device currently active",
                            $"User ID: {currentUser.Id}",
                        ]
                    ),
                    Icon = new IconInfo(currentUser?.Images.FirstOrDefault()?.Url ?? "\uE949"),
                },
            ];
        }

        if (config.ClientId != "" && config.ClientSecret != "")
        {
            items =
            [
                .. items,
                new ListItem(new LoginCommand())
                {
                    Title = "Login to Spotify",
                    Subtitle = "After having registered your App's credentials",
                },
            ];
        }

        items =
        [
            .. items,
            new ListItem(new RegisterAppFormPage())
            {
                Title = "Register your App",
                Subtitle = $"Secrets are stored at {ConfigurationFile.Path()}",
            },
        ];

        if (config.ClientId != "" && config.ClientSecret != "")
        {
            items =
            [
                .. items,
                new ListItem(
                    new ShowFileInFolderCommand(
                        // Get parent dir of config file
                        System.IO.Path.GetDirectoryName(ConfigurationFile.Path())
                        ?? ""
                    )
                )
                {
                    Title = "Reveal configuration file",
                    Subtitle = "Show the configuration file's folder in Explorer",
                },
            ];
        }

        if (currentUser == null)
        {
            items =
            [
                .. items,
                new ListItem(new NoOpCommand())
                {
                    Title = "Not logged in",
                    Icon = new IconInfo("\uE949"),
                },
            ];
        }

        return items;
    }

    private async Task GetCurrentUser()
    {
        try
        {
            var spotify = AuthenticatedSpotifyClient.Get();
            currentUser = await spotify.UserProfile.Current();
            var devices = await spotify.Player.GetCurrentPlayback();
            currentDevice = devices.Device;
        }
        catch (Exception)
        {
            currentUser = null;
        }
    }
}
