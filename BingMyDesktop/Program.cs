using System;
using System.IO;
using System.Net;
using BingMyDesktop.Properties;
using Newtonsoft.Json;

namespace BingMyDesktop
{
    class Program
    {
        static void Main(string[] args)
        {
            var imageFileNameFullPath = GetImageFileNameFullPath();
            var imageFileInfo = MoveImage(imageFileNameFullPath);

            Wallpaper.Set(new Uri(imageFileInfo.FullName), Wallpaper.Style.Stretched);
        }

        private static FileInfo MoveImage(string imageFileNameFullPath)
        {
            var imageFinalRestingPlace = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                      System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
            var imageFileInfo = new FileInfo(imageFileNameFullPath);

            if (!Directory.Exists(imageFinalRestingPlace))
            {
                Directory.CreateDirectory(imageFinalRestingPlace);
            }

            var newImageFinalFullPath = Path.Combine(imageFinalRestingPlace, imageFileInfo.Name);
            var backupPath = GetBackupPath();

            if (File.Exists(newImageFinalFullPath) && !string.IsNullOrWhiteSpace(backupPath) && Directory.Exists(backupPath))
            {
                // rename to back-up name
                var oldFileInfo = new FileInfo(newImageFinalFullPath);
                try
                {
                    const string NewFileNameFormat = "{0}_{1:yyyyMMdd}.jpg";
                    var oldFileNewPlaceFullPath = Path.Combine(Settings.Default.BackupPath,
                                                               string.Format(NewFileNameFormat,
                                                                             Path.GetFileNameWithoutExtension(oldFileInfo.FullName),
                                                                             oldFileInfo.LastWriteTime));

                    oldFileInfo.MoveTo(oldFileNewPlaceFullPath);
                }
                catch (IOException)
                {
                    var defaultForegroundColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Another file with this name already exists. ({0})", oldFileInfo.FullName);
                    Console.ForegroundColor = defaultForegroundColor;
                    oldFileInfo.Delete();
                }
            }

            imageFileInfo.MoveTo(newImageFinalFullPath);
            
#if DEBUG
            Console.WriteLine("File moved to {0}.", imageFileInfo.FullName);
#endif
            return imageFileInfo;
        }

        private static string GetBackupPath()
        {
            const string MY_PICTURE_REPLACE_TEXT = "%mypictures%";
            const string BING_FOLDER = "Bing Wallpapers";

            var backupPath = Settings.Default.BackupPath ?? "";
            if (string.IsNullOrWhiteSpace(backupPath) || !backupPath.Contains(MY_PICTURE_REPLACE_TEXT)) 
                return backupPath;


            backupPath = backupPath.Replace(MY_PICTURE_REPLACE_TEXT, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
            backupPath = Path.Combine(backupPath, BING_FOLDER);

            if (!Directory.Exists(backupPath))
                Directory.CreateDirectory(backupPath);

            return backupPath;
        }

        private static string GetImageFileNameFullPath()
        {
            string imageFileNameFullPath;
            using (var client = new WebClient())
            {
                var response = client.DownloadString(Settings.Default.bingUrl);
#if DEBUG
                Console.WriteLine(response);
#endif

                dynamic result = JsonConvert.DeserializeObject(response);
                var bgImageUri = new UriBuilder(Settings.Default.bingScheme,
                                                Settings.Default.bingBase,
                                                Settings.Default.bingPort);

                var imageUrlAsString = $"{bgImageUri.Uri.ToString()}{result.images[0].url.ToString()}";

                Console.WriteLine("Getting {0}", imageUrlAsString);

                imageFileNameFullPath = Path.Combine(Path.GetTempPath(), "BingMyDesktop.jpg");
                client.DownloadFile(imageUrlAsString, imageFileNameFullPath);

#if DEBUG
                Console.WriteLine("File downloaded to {0}.", imageFileNameFullPath);
#endif
            }
            return imageFileNameFullPath;
        }
    }
}
