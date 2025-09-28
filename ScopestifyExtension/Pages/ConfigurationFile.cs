using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

public class ConfigurationFile
{
    private IConfiguration? config;

    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string AccessToken { get; set; } = "";
    public string Scopes { get; set; } = "";
    public string PostLikeHook { get; set; } = "";

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
        Scopes = config["scopes"] ?? "";
        PostLikeHook = config["post_like_hook"] ?? "";
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
            ["scopes"] = Scopes,
            ["post_like_hook"] = PostLikeHook,
        };

        System.IO.File.WriteAllText(Path(), System.Text.Json.JsonSerializer.Serialize(toWrite));
    }
}
