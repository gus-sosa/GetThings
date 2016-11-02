namespace GetThings.Infrastructure.Ioc
{
    using Downloader;
    using Microsoft.Practices.Unity;
    using Notifier;
    using System;

    public class IoC
    {
        private static Lazy<IUnityContainer> container = new Lazy<IUnityContainer>(() =>
        {
            var container = new UnityContainer();
            InitializeContainer(container);
            return container;
        });

        private static void InitializeContainer(UnityContainer container)
        {
#if DEBUG
            container.RegisterType<IDownloader, PluralsightDownloader>("pluralsight-downloader", new ContainerControlledLifetimeManager())
                     .RegisterType<INotifier, EmailNotifier>("emailnotifier", new ContainerControlledLifetimeManager());
#else
#endif
        }

        private static IUnityContainer _container = null;
        public static IUnityContainer Container
        {
            get
            {
                if (_container != null)
                    return _container;

                _container = new UnityContainer();
                InitializeContainer(_container as UnityContainer);
                return _container;
            }
        }
    }
}
