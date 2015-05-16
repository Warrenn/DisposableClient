using System;
using System.ServiceModel;
using DisposableClient;

namespace UnitTestProject
{
    public class ServiceAgent<TService>
    {
        protected ChannelFactory<TService> channelFactory;

        public T Call<T>(Func<TService, T> func)
        {
            var client = channelFactory.CreateChannel();
            try
            {
                return func(client);
            }
            finally
            {
                DisposeMethod.DisposeCommunicationObject(client);
            }
        }
    }
}
