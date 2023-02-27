using DiscordRPC;

namespace StarZLauncher.Classes
{
    // From Plextora#0033 / Modified by Yoqzii
    public static class DiscordRichPresenceManager
    {
        private static readonly string ClientId = "1071616808752259164";
        private static DiscordRpcClient? discordClient;

        public static DiscordRpcClient DiscordClient
        {
            get
            {
                if (discordClient == null)
                {
                    discordClient = new DiscordRpcClient(ClientId);
                }
                return discordClient;
            }
        }

        public static void IdlePresence()
        {
            DiscordClient.SetPresence(new RichPresence
            {
                State = "In the launcher",
                Timestamps = Timestamps.Now,
                Assets = new Assets
                {
                    LargeImageKey = "starz",
                    LargeImageText = "StarZ Launcher Logo"
                }
            });
        }

        public static void StopPresence()
        {
            if (discordClient == null || discordClient.IsDisposed) return;
            discordClient.ClearPresence();
            discordClient.Deinitialize();
            discordClient.Dispose();
        }
    }
}