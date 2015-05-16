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
    public class UnitTestLocal
    {

        public void TestBaseMethod(IUnityContainer container)
        {
            var host = new ServiceHost(typeof(TestService), new[] { new Uri("http://localhost:7654/testservice") });
            host.Open();

            var stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            var controller = container.Resolve<TestController>();
            controller.TestMethod();
            container.Dispose();
            stopwatch.Stop();
            Trace.Write(stopwatch.ElapsedTicks);
            host.Close();
        }

        [TestMethod]
        public void PerformanceTestServiceAgent()
        {
            var container = new UnityContainer();
            container.RegisterType<ITestService, ServiceAgentTestService>(new ContainerControlledLifetimeManager());
            TestBaseMethod(container);
        }

        [TestMethod]
        public void PerformanceTestWithDisposableIlOpCode()
        {
            var container = new UnityContainer();
            var factoryType = DisposableIlOpCode<ITestService>.CreateType();
            container.RegisterType(typeof(ITestService), factoryType, new ContainerControlledLifetimeManager(), new InjectionConstructor());
            TestBaseMethod(container);
        }

        [TestMethod]
        public void PerformanceTestWithCache()
        {
            ClientBase<ITestService>.CacheSetting = CacheSetting.AlwaysOn;
            var container = new UnityContainer();
            container.RegisterType<ITestService, TestServiceClient>(new ContainerControlledLifetimeManager());
            TestBaseMethod(container);
        }

        [TestMethod]
        public void PerformanceTestWithProxy()
        {
            var instance = DisposableProxy<ITestService>.CreateInstance();
            var container = new UnityContainer();
            container.RegisterInstance(instance, new ContainerControlledLifetimeManager());
            TestBaseMethod(container);
        }

        [TestMethod]
        public void PerformanceTestWithInterception()
        {
            var instance = (new ChannelFactory<ITestService>("ITestService")).CreateChannel();

            var proxy = Intercept.ThroughProxy(
                instance,
                new TransparentProxyInterceptor(),
                new[] { new DisposeInterceptBehavior<ITestService>() });

            var container = new UnityContainer();
            container.RegisterInstance(proxy, new ContainerControlledLifetimeManager());
            TestBaseMethod(container);
        }
    }
}
