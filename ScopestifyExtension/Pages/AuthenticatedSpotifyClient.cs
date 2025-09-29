namespace ScopestifyExtension;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

class AuthenticatedSpotifyClient
{
    private static EmbedIOAuthServer? authServer;

    private static PrivateUser? user;
    private static string errorMessage = "";

    public static Uri callbackUri = new("http://localhost:5543/callback");

    public static SpotifyClient Get()
    {
        var config = new ConfigurationFile();
        var tokenCreatedAt = config.TokenCreatedAt;
        var tokenExpiresIn = config.TokenExpiresIn;

        if (tokenCreatedAt != null && tokenExpiresIn != null)
        {
            Debug.WriteLine(
                $"Token created at {tokenCreatedAt} expires in {tokenExpiresIn} seconds"
            );

            // Refresh the access token if needed
            Task.Run(EnsureAccessTokenIsFresh).Wait();

            return new SpotifyClient(
                SpotifyClientConfig
                    .CreateDefault()
                    .WithAuthenticator(
                        new AuthorizationCodeAuthenticator(
                            config.ClientId,
                            config.ClientSecret,
                            new AuthorizationCodeTokenResponse
                            {
                                AccessToken = config.AccessToken,
                                RefreshToken = config.RefreshToken,
                                Scope = config.TokenScopes,
                                TokenType = config.TokenType,
                                ExpiresIn = tokenExpiresIn ?? 0,
                                CreatedAt = tokenCreatedAt ?? DateTime.Now,
                            }
                        )
                    )
            );
        }

        Debug.WriteLine(
            $"Invalid refresh token params, expires_in={tokenExpiresIn}, created_at={tokenCreatedAt}"
        );
        return new SpotifyClient(config.AccessToken);
    }

    public static void LogOut()
    {
        // Just blank out the saved access_token from the file
        var config = new ConfigurationFile { AccessToken = "" };
        config.Save();
    }

    public static async Task<string> EnsureAccessTokenIsFresh()
    {
        var config = new ConfigurationFile();

        var tokenCreatedAt = config.TokenCreatedAt;
        var tokenExpiresIn = config.TokenExpiresIn;

        if (tokenCreatedAt != null && tokenExpiresIn != null)
        {
            var tokenAge = DateTime.Now - tokenCreatedAt.Value;
            Debug.WriteLine($"Token age is {tokenAge.TotalSeconds} seconds");

            // If token expires in less than 5 seconds, refresh it
            if (tokenExpiresIn.Value - tokenAge.TotalSeconds < 5)
            {
                Debug.WriteLine("Refreshing access token...");

                var oauth = new OAuthClient(SpotifyClientConfig.CreateDefault());
                var newToken = await oauth.RequestToken(
                    new AuthorizationCodeRefreshRequest(
                        config.ClientId,
                        config.ClientSecret,
                        config.RefreshToken
                    )
                );

                config.AccessToken = newToken.AccessToken;
                config.RefreshToken = newToken.RefreshToken ?? config.RefreshToken;
                config.TokenScopes = newToken.Scope;
                config.TokenType = newToken.TokenType;
                config.TokenExpiresIn = newToken.ExpiresIn;
                config.TokenCreatedAt = newToken.CreatedAt;
                config.Save();

                return newToken.AccessToken;
            }
            else
            {
                Debug.WriteLine("Access token is still valid, no need to refresh.");
            }
        }
        else
        {
            Debug.WriteLine(
                $"Invalid refresh token params, expires_in={tokenExpiresIn}, created_at={tokenCreatedAt}"
            );
        }

        return config.AccessToken;
    }

    public static async Task<(PrivateUser? user, string errorMessage)> LogIn()
    {
        // Get client_id and client_secret from secrets.json
        var config = new ConfigurationFile();

        // Spin up a local HTTP server to listen for the OAuth callback
        authServer = new EmbedIOAuthServer(callbackUri, callbackUri.Port);

        // Make sure "http://localhost:5543/callback" is in your applications redirect URIs!
        var loginRequest = new LoginRequest(
            authServer.BaseUri,
            config.ClientId,
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
                Scopes.UserModifyPlaybackState,
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
        var config = new ConfigurationFile();

        await authServer?.Stop();

        var tokenResponse = await new OAuthClient(SpotifyClientConfig.CreateDefault()).RequestToken(
            new AuthorizationCodeTokenRequest(
                config.ClientId,
                config.ClientSecret,
                response.Code,
                callbackUri
            )
        );

        config.AccessToken = tokenResponse.AccessToken;
        config.RefreshToken = tokenResponse.RefreshToken;
        config.TokenScopes = tokenResponse.Scope;
        config.TokenType = tokenResponse.TokenType;
        config.TokenExpiresIn = tokenResponse.ExpiresIn;
        config.TokenCreatedAt = tokenResponse.CreatedAt;
        config.Save();

        var spotify = new SpotifyClient(
            SpotifyClientConfig
                .CreateDefault()
                .WithAuthenticator(
                    new AuthorizationCodeAuthenticator(
                        config.ClientId,
                        config.ClientSecret,
                        tokenResponse
                    )
                )
        );
        user = await spotify.UserProfile.Current();
    }

    private static async Task OnErrorReceived(object sender, string error, string state)
    {
        errorMessage = error;
        await authServer?.Stop();
    }
}
