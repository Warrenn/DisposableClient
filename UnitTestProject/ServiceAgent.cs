using System;
using System.ServiceModel;

namespace UnitTestProject
{
    public class ServiceAgent<TService> : IDisposable
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
                var communicationObject = client as ICommunicationObject;
                if ((communicationObject != null) &&
                    (communicationObject.State != CommunicationState.Closed))
                {
                    if (communicationObject.State == CommunicationState.Faulted)
                    {
                        communicationObject.Abort();
                    }
                    else
                    {
                        communicationObject.Close();
                    }
                }
            }
        }


        public void Dispose()
        {
            if (channelFactory.State == CommunicationState.Closed) return;
            if (channelFactory.State == CommunicationState.Faulted)
            {
                channelFactory.Abort();
                return;
            }
            channelFactory.Close();
        }
    }
}
