using System;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace DisposableClient
{
    public class DisposableProxy<T> :
        RealProxy,
        IRemotingTypeInfo where T : class
    {
        private readonly T target;
        private readonly Action<T> disposeAction;

        private static readonly MethodInfo DisposeMethodInfo
            = typeof(IDisposable).GetMethod("Dispose");

        public DisposableProxy(T target, Action<T> disposeAction)
            : base(typeof(T))
        {
            this.disposeAction = disposeAction;
            this.target = target;
        }

        public static T CreateInstance(Action<T> dispose = null)
        {
            var instance = ConfigChannelFactory<T>.CreateChannel();
            return WrapInstance(instance, dispose);
        }

        public static T WrapInstance(T instance, Action<T> dispose = null)
        {
            dispose = dispose ?? DisposeMethod.DisposeCommunicationObject;
            var proxy = new DisposableProxy<T>(instance, dispose);
            return proxy.GetTransparentProxy() as T;
        }

        public override IMessage Invoke(IMessage msg)
        {
            var methodCall = (IMethodCallMessage)msg;
            var method = (MethodInfo)methodCall.MethodBase;
            if (DisposeMethodInfo == method)
            {
                disposeAction(target);
                return new ReturnMessage(null, null, 0, methodCall.LogicalCallContext, methodCall);
            }
            var result = method.Invoke(target, methodCall.InArgs);
            return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);
        }

        public bool CanCastTo(Type fromType, object o)
        {
            return
                fromType == typeof(IDisposable) ||
                o.GetType().IsInstanceOfType(fromType);
        }

        public string TypeName { get; set; }
    }
}
