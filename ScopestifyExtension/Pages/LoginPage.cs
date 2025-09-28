using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using ScopestifyExtension;
using SpotifyAPI.Web;

internal sealed partial class LoginPage : ListPage
{
    private PrivateUser? currentUser;

    public LoginPage()
    {
        Name = "Authenticate to Spotify";
        Icon = new("\uE8D7");


        Task.Run(GetCurrentUser).Wait();
    }

    public override ListItem[] GetItems()
    {
        return [
            new ListItem(new LoginCommand()) { Title = "Login to Spotify", Subtitle = "after having registered your App's credentials" },
            new ListItem(new RegisterAppFormPage()) {
                Title = "Register your App",
Subtitle = $"Secrets are stored at {AuthenticatedSpotifyClient.SecretsPath()}"
            },
            new ListItem(new NoOpCommand()) {
                Title = currentUser != null ? $"Logged in as {currentUser.DisplayName}" : "Not logged in",
                Subtitle = currentUser != null ? $"User ID: {currentUser.Id}" : "",
                Icon = new IconInfo(
                    currentUser?.Images.FirstOrDefault()?.Url ?? "\uE949"
                ),
            },
        ];
    }

    private async Task GetCurrentUser()
    {
        try
        {
            var spotify = AuthenticatedSpotifyClient.Get();
            currentUser = await spotify.UserProfile.Current();
        }
        catch (Exception)
        {
            currentUser = null;
        }
    }
}
