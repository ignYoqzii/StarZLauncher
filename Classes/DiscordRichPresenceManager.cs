using DiscordRPC;


namespace StarZLauncher.Classes
{
    // Handles the DiscordRPC initialization
    // From Plextora#0033 / Modified by Yoqzii
    public static class DiscordRichPresenceManager
    {
        private static readonly string ClientId = "1071616808752259164";
        private static DiscordRpcClient? discordClient;

        public static DiscordRpcClient DiscordClient
        {
            get
            {
                discordClient ??= new DiscordRpcClient(ClientId);
                return discordClient;
            }
        }

        public static void SetPresence()
        {
            DiscordClient.SetPresence(new RichPresence
            {
                State = "In the launcher",
                Details = "",
                Timestamps = Timestamps.Now,
                Assets = new Assets
                {
                    LargeImageKey = "starz",
                    LargeImageText = "StarZ Launcher",
                    SmallImageKey = "minecraft",
                    SmallImageText = "Minecraft For Windows"
                }
            });
        }


        public static void IdlePresence()
        {
            DiscordClient.UpdateState("In the launcher");
            DiscordClient.UpdateDetails("");
        }

        public static void TerminatePresence()
        {
            if (discordClient == null || discordClient.IsDisposed) return;
            discordClient.ClearPresence();
            discordClient.Deinitialize();
            discordClient.Dispose();
        }
    }
}