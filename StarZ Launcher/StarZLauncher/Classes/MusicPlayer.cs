using NReco.VideoConverter;
using StarZLauncher.Windows;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;

namespace StarZLauncher.Classes
{
    public static class MusicPlayer
    {
        public static string MusicDirectoryPath { get; private set; }
        public static MediaPlayer MediaPlayer { get; private set; }
        public static bool IsPaused { get; private set; } = false;
        public static TimeSpan CurrentPosition { get; private set; } = TimeSpan.Zero;
        private static readonly Random Random = new();

        public static ObservableCollection<MusicItem> MusicItems { get; set; } = new ObservableCollection<MusicItem>();

        static MusicPlayer()
        {
            MusicDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarZ Launcher", "Musics");
            Directory.CreateDirectory(MusicDirectoryPath);
            MediaPlayer = new MediaPlayer();
            MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
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

        public static void PlayMusic(string filepath)
        {
            if (IsPaused)
            {
                MediaPlayer.Position = CurrentPosition;
                MediaPlayer.Play();
                IsPaused = false;
            }
            else
            {
                MediaPlayer.Open(new Uri(filepath));
                MediaPlayer.Play();
            }
        }

        public static void PauseMusic()
        {
            MediaPlayer.Pause();
            CurrentPosition = MediaPlayer.Position;
            IsPaused = true;
        }

        public static void StopMusic()
        {
            MediaPlayer.Stop();
            MediaPlayer.Close();
            IsPaused = false;
            CurrentPosition = TimeSpan.Zero;
        }

        private static void ShuffleAndPlayNext()
        {
            if (MusicItems.Count > 1)
            {
                var nextIndex = Random.Next(MusicItems.Count);
                PlayMusic(MusicItems[nextIndex].FilePath);
            }
            else if (MusicItems.Count == 1)
            {
                PlayMusic(MusicItems[0].FilePath);
            }
        }

        public static void DeleteMusic(MusicItem musicItem)
        {
            MusicItems.Remove(musicItem);
            Loader.LoadMusicFiles();
        }

        public static event PropertyChangedEventHandler PropertyChanged;

        private static void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        public static async Task DownloadMusicAsync(string videoUrl)
        {
            try
            {
                var youtube = new YoutubeClient();
                var video = await youtube.Videos.GetAsync(videoUrl);
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
                var audioStreamInfo = streamManifest.GetAudioStreams().OrderByDescending(s => s.Bitrate).FirstOrDefault();

                if (audioStreamInfo != null)
                {
                    var sanitizedTitle = SanitizeFileName(video.Title);
                    var tempFilePath = Path.Combine(MusicDirectoryPath, $"{sanitizedTitle}.temp");
                    var outputFilePath = Path.Combine(MusicDirectoryPath, $"{sanitizedTitle}.mp3");

                    // Download the audio stream to a temporary file
                    using (var httpClient = new HttpClient())
                    using (var response = await httpClient.GetAsync(audioStreamInfo.Url, HttpCompletionOption.ResponseHeadersRead))
                    using (var inputStream = await response.Content.ReadAsStreamAsync())
                    using (var outputStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 8192, useAsync: true))
                    {
                        await inputStream.CopyToAsync(outputStream);
                    }

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
                GC.Collect();
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
                // Get the highest resolution thumbnail
                var thumbnailUrl = video.Thumbnails.OrderByDescending(t => t.Resolution.Width * t.Resolution.Height)
                    .Select(t => t.Url)
                    .FirstOrDefault() ?? $"https://i.ytimg.com/vi/{video.Id}/hqdefault.jpg";

                using var httpClient = new HttpClient();
                byte[] imageData = await httpClient.GetByteArrayAsync(thumbnailUrl);

                // Use a new instance of the TagLib.File class
                using (var file = TagLib.File.Create(filePath))
                {
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
            Artist = GetArtistFromAudio(filePath);
            Image = LoadImageFromAudio(filePath);
        }

        private ImageSource LoadImageFromAudio(string filePath)
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

        private static string GetArtistFromAudio(string filePath)
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
    }
}