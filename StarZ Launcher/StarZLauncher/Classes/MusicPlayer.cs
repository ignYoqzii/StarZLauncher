using NReco.VideoConverter;
using StarZLauncher.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using static StarZLauncher.Windows.MainWindow;

namespace StarZLauncher.Classes
{
    public static class MusicPlayer
    {
        public static string MusicDirectoryPath { get; private set; }
        public static MediaPlayer MediaPlayer { get; private set; }
        public static bool IsPaused { get; private set; } = false;
        public static TimeSpan CurrentPosition { get; private set; } = TimeSpan.Zero;
        public static bool IsStopped { get; private set; } = true;
        private static readonly Random Random = new();

        public static ObservableCollection<MusicItem> MusicItems { get; set; } = new ObservableCollection<MusicItem>();

        static MusicPlayer()
        {
            MusicDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Musics");
            Directory.CreateDirectory(MusicDirectoryPath);
            MediaPlayer = new MediaPlayer();
            MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
        }

        public static void UpdateVolume(double newVolume)
        {
            MediaPlayer.Volume = newVolume;
        }

        private static async Task UpdateSongTime()
        {

            try
            {
                while (!IsStopped)
                {
                    TimeSpan currentPosition = MediaPlayer.Position;
                    Duration duration = MediaPlayer.NaturalDuration;

                    // Update UI elements on the main thread
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Format the TimeSpan as MM:SS
                        CurrentlyPlayingSongTime!.Text = currentPosition.ToString(@"mm\:ss");

                        if (duration.HasTimeSpan)
                        {
                            TimeSpan totalDuration = duration.TimeSpan;
                            double progressPercentage = (currentPosition.TotalSeconds / totalDuration.TotalSeconds) * 100;
                            CurrentlyPlayingSongProgress!.Value = progressPercentage;
                        }
                        else
                        {
                            CurrentlyPlayingSongProgress!.Value = 0;
                        }
                    });

                    await Task.Delay(1000);
                }
            }
            catch (Exception)
            {
                IsStopped = true; // Avoid crashing
            }
        }


        private static void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            ShuffleAndPlayNext();
        }

        public static void AddMusic()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = true,
                Filter = "Audio Files|*.mp3"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var filePath in openFileDialog.FileNames)
                {
                    var destinationPath = Path.Combine(MusicDirectoryPath, Path.GetFileName(filePath));
                    System.IO.File.Copy(filePath, destinationPath, true);
                }

                Loader.LoadMusicFiles();
            }
        }

        public static string GetArtistFromSongFile(string filePath)
        {
            try
            {
                var file = TagLib.File.Create(filePath);
                var artist = file.Tag.FirstPerformer;
                return string.IsNullOrEmpty(artist) ? "Unknown" : artist;
            }
            catch (Exception)
            {
                return "Unknown";
            }
        }

        public static ImageSource GetImageFromSongFile(string filePath)
        {
            var defaultImage = new BitmapImage(new Uri("/Resources/Unknow.png", UriKind.Relative));
            try
            {
                var file = TagLib.File.Create(filePath);
                var picture = file.Tag.Pictures.FirstOrDefault();
                if (picture != null)
                {
                    using var memoryStream = new MemoryStream(picture.Data.Data);
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memoryStream;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    return bitmapImage;
                }
            }
            catch (Exception)
            {
            }

            return defaultImage;
        }

        public static async void PlayMusic(string filepath)
        {
            if (IsPaused)
            {
                MediaPlayer.Position = CurrentPosition;
                MediaPlayer.Play();
                IsPaused = false;
                IsStopped = false;
                bool DiscordRPCisEnabled = ConfigManager.GetDiscordRPC();
                bool OfflineModeisEnabled = ConfigManager.GetOfflineMode();
                if (DiscordRPCisEnabled == true & OfflineModeisEnabled == false)
                {
                    string title = Path.GetFileNameWithoutExtension(filepath);
                    DiscordRichPresenceManager.DiscordClient.UpdateDetails($"Listening to {title}");
                    DiscordRichPresenceManager.DiscordClient.UpdateState("Using the Music Player");
                }
                MediaPlayer.Volume = CurrentlyPlayingSongVolumeSlider!.Value;
                await UpdateSongTime();
            }
            else
            {
                MediaPlayer.Open(new Uri(filepath));
                MediaPlayer.Play();
                CurrentlyPlayingSongTitle!.Text = Path.GetFileNameWithoutExtension(filepath);
                CurrentlyPlayingSongArtist!.Text = GetArtistFromSongFile(filepath);
                CurrentlyPlayingSongImage!.Source = GetImageFromSongFile(filepath);
                IsStopped = false;
                bool DiscordRPCisEnabled = ConfigManager.GetDiscordRPC();
                bool OfflineModeisEnabled = ConfigManager.GetOfflineMode();
                if (DiscordRPCisEnabled == true & OfflineModeisEnabled == false)
                {
                    string title = Path.GetFileNameWithoutExtension(filepath);
                    DiscordRichPresenceManager.DiscordClient.UpdateDetails($"Listening to {title}");
                    DiscordRichPresenceManager.DiscordClient.UpdateState("Using the Music Player");
                }
                MediaPlayer.Volume = CurrentlyPlayingSongVolumeSlider!.Value;
                await UpdateSongTime();
            }
        }

        public static void PauseMusic()
        {
            MediaPlayer.Pause();
            CurrentPosition = MediaPlayer.Position;
            IsPaused = true;
            IsStopped = true;
            bool DiscordRPCisEnabled = ConfigManager.GetDiscordRPC();
            bool OfflineModeisEnabled = ConfigManager.GetOfflineMode();
            if (DiscordRPCisEnabled == true & OfflineModeisEnabled == false)
            {
                string state = ConfigManager.GetDiscordRPCIdleStatus();
                DiscordRichPresenceManager.IdlePresence(state);
            }
        }

        public static void StopMusic()
        {
            // Stop and close the MediaPlayer
            MediaPlayer.Stop();
            MediaPlayer.Close();
            GC.Collect(0, GCCollectionMode.Forced);

            // Reset music-related state
            IsStopped = true;
            IsPaused = false;
            CurrentPosition = TimeSpan.Zero;

            // Update UI elements
            CurrentlyPlayingSongTime!.Text = "00:00";
            CurrentlyPlayingSongProgress!.Value = 0;
            CurrentlyPlayingSongTitle!.Text = "No Song Playing";
            CurrentlyPlayingSongArtist!.Text = "From Artist";
            CurrentlyPlayingSongImage!.Source = new BitmapImage(new Uri("/Resources/Unknow.png", UriKind.Relative));

            // Handle Discord RPC based on configuration
            bool discordRPCisEnabled = ConfigManager.GetDiscordRPC();
            bool offlineModeIsEnabled = ConfigManager.GetOfflineMode();

            if (discordRPCisEnabled && !offlineModeIsEnabled)
            {
                string state = ConfigManager.GetDiscordRPCIdleStatus();
                DiscordRichPresenceManager.IdlePresence(state);
            }
        }

        private static Queue<int> shuffledIndices = new();

        public static void ShuffleAndPlayNext()
        {
            // If the queue is empty, reshuffle
            if (shuffledIndices.Count == 0)
            {
                // Create a new list of indices and shuffle them
                var indices = Enumerable.Range(0, MusicItems.Count).OrderBy(x => Random.Next()).ToList();
                shuffledIndices = new Queue<int>(indices);
            }

            // If the queue is still empty, exit
            if (shuffledIndices.Count == 0)
            {
                // Optionally, handle the case where there are no items to play
                return;
            }

            // Dequeue the next index
            int nextIndex = shuffledIndices.Dequeue();

            // Play the selected song
            PlayMusic(MusicItems[nextIndex].FilePath);
        }

        public static void DeleteMusic(MusicItem musicItem)
        {
            MusicItems.Remove(musicItem);
            Loader.LoadMusicFiles();
        }

        public static event PropertyChangedEventHandler? PropertyChanged;

        private static void OnPropertyChanged(string propertyName)
        {
            PropertyChanged!.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        public static async Task DownloadMusicAsync(string videoUrl)
        {
            try
            {
                var youtube = new YoutubeClient();
                var video = await youtube.Videos.GetAsync(videoUrl);
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
                var audioStreamInfo = streamManifest.GetAudioOnlyStreams().OrderByDescending(s => s.Bitrate).FirstOrDefault();

                if (audioStreamInfo != null)
                {
                    var sanitizedTitle = SanitizeFileName(video.Title);
                    var tempFilePath = Path.Combine(MusicDirectoryPath, $"{sanitizedTitle}.temp");
                    var outputFilePath = Path.Combine(MusicDirectoryPath, $"{sanitizedTitle}.mp3");

                    // Download the audio stream to a temporary file using YoutubeClient's DownloadAsync
                    await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, tempFilePath);

                    // Convert the temporary file to MP3 format
                    ConvertToMp3(tempFilePath, outputFilePath);

                    // Delete the temporary file
                    File.Delete(tempFilePath);

                    // Embed the thumbnail and Author into the MP3 file
                    await EmbedThumbnailandAuthorAsync(outputFilePath, video);

                    Loader.LoadMusicFiles();
                }
                else
                {
                    StarZMessageBox.ShowDialog("No audio stream found for the provided video URL.", "Error", false);
                }
            }
            catch (Exception ex)
            {
                StarZMessageBox.ShowDialog($"An error occurred: {ex.Message}", "Error!", false);
            }
            finally
            {
                // Force garbage collection
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
                GC.WaitForPendingFinalizers();
            }
        }

        private static void ConvertToMp3(string inputFilePath, string outputFilePath)
        {
            try
            {
                // Define the path to the FFMpeg executable
                string ffmpegPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Musics");

                // Ensure the output directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));

                // Create the FFMpeg converter instance with the specified tool path
                var converter = new FFMpegConverter { FFMpegToolPath = ffmpegPath };

                // Configure conversion settings
                var settings = new ConvertSettings
                {
                    // Set audio bitrate
                    CustomOutputArgs = "-b:a 128k"
                };

                // Perform the conversion
                converter.ConvertMedia(new[] { new FFMpegInput(inputFilePath) }, outputFilePath, "mp3", settings);
            }
            catch (Exception ex)
            {
                // Handle exceptions, log the error, or show a message to the user
                StarZMessageBox.ShowDialog($"An error occurred during MP3 conversion: {ex.Message}", "Error", false);
            }
        }

        private static async Task EmbedThumbnailandAuthorAsync(string filePath, IVideo video)
        {
            try
            {
                // Get the thumbnail
                var thumbnailUrl = video.Thumbnails.Where(t => string.Equals(t.TryGetImageFormat(), "jpg", StringComparison.OrdinalIgnoreCase)).OrderByDescending(t => t.Resolution.Area).Select(t => t.Url).FirstOrDefault() ?? $"https://i.ytimg.com/vi/{video.Id}/hqdefault.jpg";

                using var httpClient = new HttpClient();
                byte[] imageData = await httpClient.GetByteArrayAsync(thumbnailUrl);

                // Use a new instance of the TagLib.File class
                using var file = TagLib.File.Create(filePath);
                // Create a new picture object with the image data
                var picture = new TagLib.Picture(new TagLib.ByteVector(imageData));

                // Assign the picture and author to the file's tag
                file.Tag.Pictures = new TagLib.IPicture[] { picture };
                file.Tag.Performers = video.Author.ChannelTitle.Split(',');

                // Explicitly save changes
                file.Tag.DateTagged = DateTime.Now;
                file.Save();
                file.Dispose();
            }
            catch (Exception ex)
            {
                StarZMessageBox.ShowDialog($"Failed to embed thumbnail: {ex.Message}", "Error", false);
            }
        }

        private static string SanitizeFileName(string fileName)
        {
            var invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (var invalidChar in invalidChars)
            {
                fileName = fileName.Replace(invalidChar.ToString(), "");
            }
            return fileName;
        }

    }

    public class MusicItem
    {
        public string FilePath { get; }
        public string Title { get; }
        public string Artist { get; }
        public ImageSource Image { get; }

        public MusicItem(string filePath)
        {
            FilePath = filePath;
            Title = Path.GetFileNameWithoutExtension(filePath);
            Artist = MusicPlayer.GetArtistFromSongFile(filePath);
            Image = MusicPlayer.GetImageFromSongFile(filePath);
        }
    }

    // For thumbnail embedding
    public static class StringExtensions
    {
        public static string? NullIfEmptyOrWhiteSpace(this string str) => !string.IsNullOrEmpty(str.Trim()) ? str : null;
    }

    public static class Url
    {
        public static string? TryExtractFileName(string url) => Regex.Match(url, @".+/([^?]*)").Groups[1].Value.NullIfEmptyOrWhiteSpace();
    }

    public static class GenericExtensions
    {
        public static TOut Pipe<TIn, TOut>(this TIn input, Func<TIn, TOut> transform) => transform(input);
    }

    public static class ThumbnailExtensions
    {
        public static string? TryGetImageFormat(this Thumbnail thumbnail) => Url.TryExtractFileName(thumbnail.Url)?.Pipe(Path.GetExtension)?.Trim('.');
    }
}