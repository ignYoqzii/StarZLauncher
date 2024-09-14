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

        public static void SetPresence(string state = "In the launcher", string details = "")
        {
            try
            {
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

        public static void IdlePresence()
        {
            try
            {
                DiscordClient.UpdateState("In the launcher");
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