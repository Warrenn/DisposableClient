using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace DisposableClient
{
    public class DisposableChannelFactory<T> : ChannelFactoryBase<T>
    {
        private readonly IChannelFactory<T> innerChannelFactory;

        public DisposableChannelFactory(BindingContext context)
        {
            innerChannelFactory = context.BuildInnerChannelFactory<T>();
        }

        protected override T OnCreateChannel(EndpointAddress address, Uri via)
        {
            var channel = innerChannelFactory.CreateChannel(address, via);
            return DisposableProxy<T>.WrapInstance(channel);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return innerChannelFactory.BeginOpen(timeout, callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            innerChannelFactory.EndOpen(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            innerChannelFactory.Open(timeout);
        }
    }
}
