using System;

namespace DisposableClient
{
    public interface IProxyFactory<out T> : IDisposable
    {
        T CreateService();
    }
}
