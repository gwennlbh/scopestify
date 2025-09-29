namespace ScopestifyExtension.Utils;

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SpotifyAPI.Web;

public class Devices
{
    /// <summary>
    /// Get the device to start playback on.
    /// </summary>
    /// <returns></returns>
    public static async Task<Device> GetDeviceToStartPlaybackOn(SpotifyClient spotify)
    {
        var devices = await spotify.Player.GetAvailableDevices();
        Debug.WriteLine(
            $"Available devices: {string.Join(", ", devices.Devices.Select(d => d.Name))}"
        );

        if (devices.Devices.Count == 0)
        {
            throw new Exception(
                "You don't have any devices linked to your account, open Spotify and play something"
            );
        }

        // Get device whose name is the machine name, or the first one
        return devices.Devices.Find(d => d.Name == Environment.MachineName)
            ?? devices.Devices.FirstOrDefault();
    }
}
