namespace GetThins.Infrastructure
{
    public interface IDownloader
    {
        bool CanDownloadResource(string resource);
        bool Download(string resource, BaseInfo info);
    }
}
