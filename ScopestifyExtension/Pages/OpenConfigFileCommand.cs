using System;
using System.Diagnostics;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace ScopestifyExtension;

/// <summary>
/// Command to open the configuration file in the default editor
/// </summary>
internal sealed partial class OpenConfigFileCommand : InvokableCommand
{
    public override string Name => "Open config file";
    public override IconInfo Icon => new("\uE70F"); // Edit icon

    public override CommandResult Invoke()
    {
        try
        {
            var configPath = ConfigurationFile.Path();
            
            // Ensure the file exists
            if (!System.IO.File.Exists(configPath))
            {
                var config = new ConfigurationFile();
                config.Save(); // Create the file if it doesn't exist
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = configPath,
                UseShellExecute = true
            });

            return CommandResult.ShowToast(
                new ToastArgs { Message = "Config file opened" }
            );
        }
        catch (Exception ex)
        {
            return CommandResult.ShowToast(
                new ToastArgs { Message = $"Failed to open config file: {ex.Message}" }
            );
        }
    }
}