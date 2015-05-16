using System;
using System.ServiceModel;
using DisposableClient;
using WcfServiceLibrary;

namespace UnitTestProject
{
    public class ManualExample : ClientBase<IService1>, IService1, IDisposable
    {
        public string GetData(int value)
        {
            return Channel.GetData(value);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            return Channel.GetDataUsingDataContract(composite);
        }

        public void Dispose()
        {
            DisposeMethod.DisposeCommunicationObject(this);
        }
    }
}
