using System;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace DisposableClient
{
    public class DisposableChannelInitializer : BindingElement
    {
        public DisposableChannelInitializer(BindingElement other)
            : base(other)
        {

        }

        public override BindingElement Clone()
        {
            return new DisposableChannelInitializer(this);
        }

        public override T GetProperty<T>(BindingContext context)
        {
            return context.GetInnerProperty<T>();
        }

        #region Overrides of BindingElement

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            return base.BuildChannelFactory<TChannel>(context);
        }

        #endregion
    }
}
