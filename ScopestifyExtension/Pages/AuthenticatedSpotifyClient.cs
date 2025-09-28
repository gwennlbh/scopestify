namespace ScopestifyExtension;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

class AuthenticatedSpotifyClient
{
    private static EmbedIOAuthServer? authServer;
    private static string clientId = "";
    private static string clientSecret = "";

    private static PrivateUser? user;
    private static string errorMessage = "";

    public static string SecretsPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            + "\\.scopestify\\secrets.json";
    }

    public static Uri callbackUri = new("http://localhost:5543/callback");

    public static SpotifyClient Get()
    {
        var accessToken = "";

        try
        {
            // Create secrets file if it doesn't exist
            if (!System.IO.File.Exists(SecretsPath()))
            {
                var dir = System.IO.Path.GetDirectoryName(SecretsPath());
                if (dir != null && !System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }
                System.IO.File.WriteAllText(
                    SecretsPath(),
                    "{\n  \"client_id\": \"\",\n  \"client_secret\": \"\",\n  \"access_token\": \"\"\n}"
                );
            }

            var secrets = System
                .Text.Json.JsonDocument.Parse(System.IO.File.ReadAllText(SecretsPath()))
                .RootElement;

            accessToken = secrets.GetProperty("access_token").GetString();
        }
        catch (Exception)
        {
            // TODO
        }

        return new SpotifyClient(accessToken ?? "");
    }

    public static void LogOut()
    {
        // Just blank out the saved access_token from the file
        var secretsPath = AuthenticatedSpotifyClient.SecretsPath();
        if (System.IO.File.Exists(secretsPath))
        {
            var existingSecrets = System
                .Text.Json.JsonDocument.Parse(System.IO.File.ReadAllText(secretsPath))
                .RootElement;

            var updatedSecrets = new Dictionary<string, string>
            {
                ["client_id"] = existingSecrets.GetProperty("client_id").GetString() ?? "",
                ["client_secret"] = existingSecrets.GetProperty("client_secret").GetString() ?? "",
                ["access_token"] = "",
                ["scopes"] = existingSecrets.GetProperty("scopes").GetString() ?? "",
            };

            System.IO.File.WriteAllText(
                AuthenticatedSpotifyClient.SecretsPath(),
                System.Text.Json.JsonSerializer.Serialize(updatedSecrets)
            );
        }
    }

    public static async Task<(PrivateUser? user, string errorMessage)> LogIn()
    {
        // Get client_id and client_secret from secrets.json
        var secretsText = System.IO.File.ReadAllText(SecretsPath());
        var secrets = System.Text.Json.JsonDocument.Parse(secretsText).RootElement;

        clientId = secrets.GetProperty("client_id").GetString() ?? "";
        clientSecret = secrets.GetProperty("client_secret").GetString() ?? "";

        // Spin up a local HTTP server to listen for the OAuth callback
        authServer = new EmbedIOAuthServer(
            AuthenticatedSpotifyClient.callbackUri,
            AuthenticatedSpotifyClient.callbackUri.Port
        );

        // Make sure "http://localhost:5543/callback" is in your applications redirect URIs!
        var loginRequest = new LoginRequest(
            authServer.BaseUri,
            clientId,
            LoginRequest.ResponseType.Code
        )
        {
            Scope =
            [
                Scopes.UserFollowModify,
                Scopes.UserLibraryRead,
                Scopes.UserLibraryModify,
                Scopes.UserFollowRead,
                Scopes.PlaylistModifyPublic,
                Scopes.PlaylistModifyPrivate,
                Scopes.UserReadPlaybackState,
            ],
        };

        var uri = loginRequest.ToUri();

        authServer.AuthorizationCodeReceived += OnAuthorizationCodeReceived;
        authServer.ErrorReceived += OnErrorReceived;
        await authServer.Start();
        BrowserUtil.Open(uri);

        // Wait until we have received the token
        while (user == null && string.IsNullOrEmpty(errorMessage))
        {
            await Task.Delay(500);
        }

        return (user, errorMessage);
    }

    private static async Task OnAuthorizationCodeReceived(
        object sender,
        AuthorizationCodeResponse response
    )
    {
        await authServer?.Stop();

        var config = SpotifyClientConfig.CreateDefault();
        var tokenResponse = await new OAuthClient(config).RequestToken(
            new AuthorizationCodeTokenRequest(
                clientId,
                clientSecret,
                response.Code,
                AuthenticatedSpotifyClient.callbackUri
            )
        );

        var updatedSecrets = new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["access_token"] = tokenResponse.AccessToken,
            ["scopes"] = tokenResponse.Scope ?? "",
        };

        System.IO.File.WriteAllText(
            AuthenticatedSpotifyClient.SecretsPath(),
            System.Text.Json.JsonSerializer.Serialize(updatedSecrets)
        );

        var spotify = new SpotifyClient(tokenResponse.AccessToken);
        user = await spotify.UserProfile.Current();
    }

    private static async Task OnErrorReceived(object sender, string error, string state)
    {
        errorMessage = error;
        await authServer?.Stop();
    }
}
