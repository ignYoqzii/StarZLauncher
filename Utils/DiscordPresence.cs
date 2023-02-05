using DiscordRPC;

namespace StarZLauncher.Utils;

public static class DiscordPresence
{
    public static readonly DiscordRpcClient DiscordClient = new("1071616808752259164");
    
    public static void IdlePresence()
    {
        DiscordClient.SetPresence(new RichPresence
        {
            State = "Idling in the client",
            Timestamps = Timestamps.Now,
            Buttons = new[]
            {
                new Button { Label = "Download StarZ Launcher", Url = "https://github.com/ignYoqzii/StarZLauncher" }
            },
            Assets = new Assets
            {
                LargeImageKey = "starz",
                LargeImageText = "StarZ Launcher Icon"
            }
        });
    }
    
    public static void StopPresence()
    {
        if (DiscordClient.IsDisposed) return;
        DiscordClient.ClearPresence();
        DiscordClient.Deinitialize();
        DiscordClient.Dispose();
    }
}