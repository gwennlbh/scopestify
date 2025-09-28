using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

public class ConfigurationFile
{
    private IConfiguration? config;

    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string AccessToken { get; set; } = "";
    public string RefreshToken { get; set; } = "";
    public string TokenScopes { get; set; } = "";
    public string TokenType { get; set; } = "";
    public int? TokenExpiresIn { get; set; }
    public DateTime? TokenCreatedAt { get; set; }
    public string PostLikeHook { get; set; } = "";
    public string PostLikeHookCwd { get; set; } = "";

    public ConfigurationFile()
    {
        Update();
    }

    public static string Path()
    {
        return System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".scopestify",
            "config.json"
        );
    }

    public void Update()
    {
        if (!System.IO.File.Exists(Path()))
        {
            return;
        }

        config = new ConfigurationBuilder().AddJsonFile(Path()).Build();

        ClientId = config["client_id"] ?? "";
        ClientSecret = config["client_secret"] ?? "";
        AccessToken = config["access_token"] ?? "";
        PostLikeHook = config["post_like_hook"] ?? "";
        PostLikeHookCwd = config["post_like_hook_cwd"] ?? "";
        RefreshToken = config["refresh_token"] ?? "";
        TokenScopes = config["token_scopes"] ?? "";
        TokenType = config["token_type"] ?? "";
        if (int.TryParse(config["token_expires_in"], out var expiresIn))
        {
            TokenExpiresIn = expiresIn;
        }
        if (DateTime.TryParse(config["token_created_at"], out var createdAt))
        {
            TokenCreatedAt = createdAt;
        }
    }

    public void Save()
    {
        if (!System.IO.File.Exists(Path()))
        {
            var dir = System.IO.Path.GetDirectoryName(Path());
            if (dir != null && !System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
            System.IO.File.WriteAllText(Path(), "{}");
        }

        var toWrite = new Dictionary<string, string>
        {
            ["client_id"] = ClientId,
            ["client_secret"] = ClientSecret,
            ["access_token"] = AccessToken,
            ["post_like_hook"] = PostLikeHook,
            ["post_like_hook_cwd"] = PostLikeHookCwd,
            ["refresh_token"] = RefreshToken,
            ["token_scopes"] = TokenScopes,
            ["token_type"] = TokenType,
            ["token_expires_in"] = TokenExpiresIn?.ToString() ?? "",
            ["token_created_at"] = TokenCreatedAt?.ToString("o") ?? "",
        };

        System.IO.File.WriteAllText(Path(), System.Text.Json.JsonSerializer.Serialize(toWrite));
    }
}
