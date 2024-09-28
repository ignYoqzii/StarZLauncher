using DiscordRPC;
using System;
using System.Windows.Forms;

namespace StarZLauncher.Classes
{
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

        public static void SetPresence(string details = "")
        {
            try
            {
                string state = ConfigManager.GetDiscordRPCIdleStatus();
                DiscordClient.SetPresence(new RichPresence
                {
                    State = state,
                    Details = details,
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
            catch (Exception ex)
            {
                LogManager.Log($"{ex.Message}", "DiscordClient.txt");
            }
        }

        public static void IdlePresence(string state)
        {
            try
            {
                DiscordClient.UpdateState(state);
                DiscordClient.UpdateDetails("");
            }
            catch (Exception ex)
            {
                LogManager.Log($"{ex.Message}", "DiscordClient.txt");
            }
        }

        public static void TerminatePresence()
        {
            if (DiscordClient.IsDisposed) return;
            try
            {
                DiscordClient.ClearPresence();
                DiscordClient.Deinitialize();
                DiscordClient.Dispose();
            }
            catch (Exception ex)
            {
                LogManager.Log($"{ex.Message}", "DiscordClient.txt");
            }
        }
    }
}