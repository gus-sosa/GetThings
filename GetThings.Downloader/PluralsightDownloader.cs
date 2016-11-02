using GetThins.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetThings.Downloader
{
    public class PluralsightDownloader : IDownloader
    {
        public bool CanDownloadResource(string resource)
        {
            return resource.Contains("pluralsight.com");
        }

        public bool Download(string resource, BaseInfo info)
        {
            throw new NotImplementedException();
        }
    }
}
