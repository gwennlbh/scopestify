using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SpotifyAPI.Web;

namespace ScopestifyExtension.Commands;

internal sealed partial class FollowArtist(SimpleArtist artist, bool unfollow = false)
    : InvokableCommand
{
    private SimpleArtist artist = artist;
    private bool unfollow = unfollow;

    public override string Name => unfollow ? $"Unfollow {artist.Name}" : $"Follow {artist.Name}";

    public override IconInfo Icon => unfollow ? Icons.UserRemove : Icons.AddFriend;

    public override CommandResult Invoke()
    {
        try
        {
            Task.Run(async () =>
                {
                    var spotify = AuthenticatedSpotifyClient.Get();
                    var isFollowing = await spotify
                        .Follow.CheckCurrentUser(
                            new FollowCheckCurrentUserRequest(
                                FollowCheckCurrentUserRequest.Type.Artist,
                                [artist.Id]
                            )
                        )
                        .ContinueWith(results => results.Result.FirstOrDefault());

                    if (unfollow && isFollowing)
                    {
                        await spotify.Follow.Unfollow(
                            new UnfollowRequest(UnfollowRequest.Type.Artist, [artist.Id])
                        );
                    }
                    else if (!unfollow && isFollowing)
                    {
                        throw new InvalidOperationException(
                            $"You are already following {artist.Name}"
                        );
                    }
                    else if (!unfollow && !isFollowing)
                    {
                        await spotify.Follow.Follow(
                            new FollowRequest(FollowRequest.Type.Artist, [artist.Id])
                        );
                    }
                    else if (unfollow && !isFollowing)
                    {
                        throw new InvalidOperationException($"You are not following {artist.Name}");
                    }
                })
                .Wait();

            return CommandResult.ShowToast(
                new ToastArgs
                {
                    Message = unfollow ? $"Unfollowed {artist.Name}" : $"Followed {artist.Name}",
                    Result = CommandResult.Dismiss(),
                }
            );
        }
        catch (Exception ex)
        {
            return CommandResult.ShowToast(
                new ToastArgs
                {
                    Message = ex.InnerException?.Message ?? ex.Message,
                    Result = CommandResult.KeepOpen(),
                }
            );
        }
    }

    public FollowArtist(FullArtist artist, bool unfollow = false)
        : this(
            new SimpleArtist
            {
                Id = artist.Id,
                Name = artist.Name,
                Href = artist.Href,
                Uri = artist.Uri,
                ExternalUrls = artist.ExternalUrls,
                Type = artist.Type,
            },
            unfollow
        ) { }
}
