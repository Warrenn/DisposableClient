using System;
using System.Collections.Generic;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace DisposableClient
{
    public class DisposeInterceptBehavior<T> : IInterceptionBehavior where T : class
    {
        private readonly Action<T> dispose;

        public DisposeInterceptBehavior():this(DisposableFactory<T>.DisposeMethod)
        {
        }

        public DisposeInterceptBehavior(Action<T> dispose)
        {
            this.dispose = dispose;
        }

        public IEnumerable<Type> GetRequiredInterfaces()
        {
            return new[] {typeof (IDisposable)};
        }

        public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
        {
            if (input.MethodBase.Name != "Dispose") return getNext()(input, getNext);
            var returnValue = input.CreateMethodReturn(null, input.Arguments);
            dispose((T) input.Target);
            return returnValue;
        }

        public bool WillExecute
        {
            get { return true; }
        }
    }
}
