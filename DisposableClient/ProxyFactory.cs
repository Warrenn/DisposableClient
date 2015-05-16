using System;
using System.Diagnostics.Contracts;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace DisposableClient
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
    public class ProxyFactory<T> : IProxyFactory<T> where T : class
    {
        private readonly ChannelFactory<T> factory;
        private readonly T service;
       
        #region ctors
        public ProxyFactory()
        {
            factory = new ConfigChannelFactory<T>();
            service = factory.CreateChannel();
        }

        public ProxyFactory(string configurationName)
        {
            factory = new ChannelFactory<T>(configurationName);
            service = factory.CreateChannel();
        }

        public ProxyFactory(Binding binding, EndpointAddress endpointAddress)
        {
            factory = new ChannelFactory<T>(binding, endpointAddress);
            service = factory.CreateChannel(endpointAddress);
        }

        public ProxyFactory(Binding binding, EndpointAddress endpointAddress, Uri via)
        {
            factory = new ChannelFactory<T>(binding, endpointAddress);
            service = factory.CreateChannel(endpointAddress, via);
        }

        public ProxyFactory(T service)
        {
            Contract.Requires((service as ICommunicationObject) != null);
            this.service = service;
        }

        #endregion

        #region IProxyFactory<T> Members

        public T CreateService()
        {
            return service;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        public void Dispose()
        {
            DisposeMethod.DisposeCommunicationObject(service);
            DisposeMethod.DisposeCommunicationObject(factory);
        }

        #endregion
    }
}