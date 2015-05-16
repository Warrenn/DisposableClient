using System;
using System.Diagnostics;
using System.ServiceModel;
using DisposableClient;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestProject.GeneratedProxies;
using WcfServiceLibrary;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTestRemote
    {

        public void TestBaseMethod(IUnityContainer container)
        {
            var stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            var controller = container.Resolve<RemoteTestController>();
            controller.PerformMethod();
            container.Dispose();
            stopwatch.Stop();
            Trace.Write(stopwatch.ElapsedTicks);
        }

        [TestMethod]
        public void PerformanceTestServiceAgent()
        {
            var container = new UnityContainer();
            container.RegisterType<IService1, ServiceAgentService1>(new ContainerControlledLifetimeManager());
            TestBaseMethod(container);
        }

        [TestMethod]
        public void PerformanceTestWithDisposableService()
        {
            var container = new UnityContainer();
            var factoryType = DisposableIlOpCode<IService1>.CreateType();
            container.RegisterType(typeof(IService1), factoryType, new ContainerControlledLifetimeManager(), new InjectionConstructor());
            TestBaseMethod(container);
        }

        [TestMethod]
        public void PerformanceTestWithCache()
        {
            ClientBase<IService1>.CacheSetting = CacheSetting.AlwaysOn;
            var container = new UnityContainer();
            container.RegisterType<IService1, Service1Client>(new ContainerControlledLifetimeManager());
            TestBaseMethod(container);
        }

        [TestMethod]
        public void PerformanceTestWithInterception()
        {
            var instance =
                (new ChannelFactory<IService1>("IService1")).CreateChannel();

            var proxy = Intercept.ThroughProxy(
                instance,
                new TransparentProxyInterceptor(),
                new[] { new DisposeInterceptBehavior<IService1>() });

            var container = new UnityContainer();
            container.RegisterInstance(proxy, new ContainerControlledLifetimeManager());
            TestBaseMethod(container);
        }
    }
}
