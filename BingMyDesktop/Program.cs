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
            using (var client = new WebClient())
            {
                var response = client.DownloadString(Settings.Default.bingUrl);
                //Console.WriteLine(response);

                dynamic result = JsonConvert.DeserializeObject(response);
                var bgImageUri = new UriBuilder(Settings.Default.bingScheme,
                                                Settings.Default.bingBase,
                                                Settings.Default.bingPort, 
                                                result.images[0].url.ToString());

                Console.WriteLine("Getting {0}", bgImageUri);

                var imageFileNameFullPath = Path.Combine(Path.GetTempPath(), "BingMyDesktop.jpg");
                client.DownloadFile(bgImageUri.Uri, imageFileNameFullPath);

                Console.WriteLine("File downloaded to {0}.", imageFileNameFullPath);

                var imageFinalRestingPlace = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                                          System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
                var imageFileInfo = new FileInfo(imageFileNameFullPath);

                if (!Directory.Exists(imageFinalRestingPlace))
                {
                    Directory.CreateDirectory(imageFinalRestingPlace);
                }

                var newImageFinalFullPath = Path.Combine(imageFinalRestingPlace, imageFileInfo.Name);
                if (File.Exists(newImageFinalFullPath))
                {
                    // rename to back-up name
                    var oldFileInfo = new FileInfo(newImageFinalFullPath);
                    try
                    {
                        oldFileInfo.MoveTo(Path.Combine(oldFileInfo.Directory.FullName,
                            oldFileInfo.Name.Substring(0, oldFileInfo.Name.LastIndexOf('.')) + "_" +
                            oldFileInfo.CreationTime.ToString("yyyyMMdd") + ".jpg"));
                    }
                    catch (IOException)
                    {
                        Console.WriteLine("Another file with this name already exists. ({0})", oldFileInfo.FullName);
                        oldFileInfo.Delete();
                    }
                }

                imageFileInfo.MoveTo(newImageFinalFullPath);

                Console.WriteLine("File moved to {0}.", imageFileInfo.FullName);
            }

        }
    }
}
