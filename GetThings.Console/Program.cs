namespace GetThings.Downloader
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Practices.Unity;
    using Infrastructure;
    using Infrastructure.Ioc;
    using CommandLine;
    using CommandLine.Text;

    class Program
    {
        class Input
        {
            [Option('u', "username", HelpText = "Username for login", DefaultValue = "")]
            public string Username { get; set; }

            [Option('p', "password", HelpText = "Password for login", DefaultValue = "")]
            public string Password { get; set; }

            [Option('d', "pathDirectory", HelpText = "Directory where the resources will be downloaded")]
            public string PathDirectory { get; set; } = AppDomain.CurrentDomain.BaseDirectory + "download\\";

            [Option('t', "pathTempDirectory", HelpText = "Temp Directory. This should be a place where the application can create files.")]
            public string PathTempDirectory { get; set; } = AppDomain.CurrentDomain.BaseDirectory + "download\\";

            [Option('i', "DirFileInput", HelpText = "The path of the input file")]
            public string DirFileInput { get; set; } = AppDomain.CurrentDomain.BaseDirectory + "download\\input.txt";

            [Option('r', "retry", HelpText = "Number of times for trying to download a resource.", DefaultValue = 10)]
            public int Retry { get; set; }

            [ParserState]
            public IParserState ParserState { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
            }
        }

        static void Main(string[] args)
        {
            IEnumerable<IDownloader> downloaders = IoC.Container.ResolveAll<IDownloader>();
            IEnumerable<INotifier> notifiers = IoC.Container.ResolveAll<INotifier>();
            var input = new Input();
            Parser.Default.ParseArguments(args, input);

            using (var stream = new StreamReader(input.DirFileInput))
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
                        Username = input.Username,
                        Password = input.Password,
                        PathDirectory = input.PathDirectory,
                        PathTempDirectory = input.PathTempDirectory,
                        DirFileInput = input.DirFileInput,
                        Resource = resource
                    };
                    bool flag = false;
                    for (int i = 0; !flag && i < input.Retry; i++)
                        flag = downloader.Download(resource, info);

                    foreach (var notifier in notifiers)
                        Task.Run(() => notifier.Notify(flag, info));
                }
        }
    }
}