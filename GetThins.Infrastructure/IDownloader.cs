using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetThins.Infrastructure
{
    public interface IDownloader
    {
        bool CanDownloadResource(string resource);
        bool Download(string resource, BaseInfo info);
    }
}
