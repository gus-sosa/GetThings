namespace GetThings.Downloader
{
    using Infrastructure;
    using System;
    using System.Diagnostics;
    using System.IO;

    public class PluralsightDownloader : IDownloader
    {
        public bool CanDownloadResource(string resource)
        {
            return resource.Contains("pluralsight.com") && ThridPartyProgramInstalled();
        }

        private bool ThridPartyProgramInstalled()
        {
            return true;//TODO: Check that node-pd (a program from node.js) is installed in the pc
        }

        public bool Download(string resource, BaseInfo info)
        {
            string idResource = GetId(resource);

            ConfigEnvironment(info);

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    Arguments = $"/C pd download {idResource}",
                    FileName = "cmd.exe"
                }
            };

            try
            {
                process.Start();
                process.WaitForExit();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private void ConfigEnvironment(BaseInfo info)
        {
            var pathDir = $"{ info.PathDirectory }{ GetId(info.Resource)}";
            if (!Directory.Exists(pathDir))
                Directory.CreateDirectory(pathDir);

            var procces = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    Arguments = $"/C pd config -u {info.Username} -p {info.Password} -s {pathDir}",
                    FileName = "cmd.exe"
                }
            };

            procces.Start();
            procces.WaitForExit();
        }

        private string GetId(string resource)
        {
            var segments = resource.Split('/');
            return segments[segments.Length - 1];
        }
    }
}
