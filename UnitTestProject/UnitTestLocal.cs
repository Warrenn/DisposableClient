using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;
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
            stopwatch.Stop();
            Trace.Write(stopwatch.ElapsedTicks);

            host.Close();
        }

        [TestMethod]
        public void PerformanceTestServiceAgent()
        {
            var container = new UnityContainer();
            container.RegisterType<ITestService, ServiceAgentTestService>();
            TestBaseMethod(container);
        }

        [TestMethod]
        public void PerformanceTestWithDisposableService()
        {
            var container = new UnityContainer();
            var factoryType = DisposableFactory<ITestService>.CreateDisposableType();
            container.RegisterType(typeof(ITestService), factoryType, new InjectionConstructor());
            TestBaseMethod(container);
        }

        [TestMethod]
        public void PerformanceTestWithCache()
        {
            ClientBase<ITestService>.CacheSetting = CacheSetting.AlwaysOn;
            var container = new UnityContainer();
            container.RegisterType<ITestService, TestServiceClient>();
            TestBaseMethod(container);
        }

        [TestMethod]
        public void PerformanceTestWithInterception()
        {
            var instance = (new ChannelFactory<ITestService>(new BasicHttpBinding())).CreateChannel(new EndpointAddress("http://localhost:7654/testservice"));

            var proxy = Intercept.ThroughProxy(
                instance,
                new TransparentProxyInterceptor(),
                new[] {new DisposeInterceptBehavior<ITestService>()});

            var container = new UnityContainer();
            container.RegisterInstance(proxy);
            TestBaseMethod(container);
        }
    }
}
