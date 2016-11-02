using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetThins.Infrastructure
{
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
        }

        public static IUnityContainer Container { get; set; }
    }
}
