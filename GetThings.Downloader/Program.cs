using Arguments;
using GetThins.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetThings.Downloader
{
    class Program
    {
        static void Main(string[] args)
        {
            string username = "";
            string password = "";
            string pathDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string pathTempDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string dirFileInput = AppDomain.CurrentDomain.BaseDirectory + "\\input.txt";
            int retry = 10;
            IEnumerable<IDownloader> downloaders = null;
            IEnumerable<INotifier> notifiers = null;


            var processedArguments = ArgumentProcessor.Initialize(args)
                 .AddArgument(nameof(username))
                     .WithAction(parameter => username = parameter)
                 .AddArgument(nameof(password))
                     .WithAction(parameter => password = parameter)
                 .AddArgument(nameof(pathDirectory))
                     .WithAction(parameter => pathDirectory = parameter)
                 .AddArgument(nameof(pathTempDirectory))
                     .WithAction(parameter => pathTempDirectory = parameter)
                 .AddArgument(nameof(dirFileInput))
                     .WithAction(parameter => dirFileInput = parameter)
                 .AddArgument(nameof(retry))
                     .WithAction(parameter => retry = int.Parse(parameter))
                 .Process();

            using (var stream = new StreamReader(dirFileInput))
                while (true)
                {
                    var resource = stream.ReadLine();
                    if (string.IsNullOrEmpty(resource))
                        break;

                    var downloader = downloaders?.FirstOrDefault(d => d.CanDownloadResource(resource));
                    if (downloader == null)
                    {
                        Console.WriteLine($"The program does not know how to download this resource <<<{resource}>>>");
                        continue;
                    }

                    var info = new BaseInfo()
                    {
                        Username = username,
                        Password = password,
                        PathDirectory = pathDirectory,
                        PathTempDirectory = pathTempDirectory,
                        DirFileInput = dirFileInput,
                        Resource = resource
                    };
                    bool flag = false;
                    for (int i = 0; !flag && i < retry; i++)
                        flag = downloader.Download(resource, info);

                    foreach (var notifier in notifiers)
                        Task.Run(() => notifier.Notify(flag, info));
                }
        }
    }
}
