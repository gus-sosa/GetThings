namespace GetThins.Infrastructure
{
    public interface INotifier
    {
        void Notify(bool flag, BaseInfo info);
    }
}