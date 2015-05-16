using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using DisposableClient;
using WcfServiceLibrary;

namespace UnitTestProject
{
    public class ServiceAgentService1 : ServiceAgent<IService1>,IService1,IDisposable
    {
        public ServiceAgentService1()
        {
            channelFactory = new ChannelFactory<IService1>("IService1");
        }

        public string GetData(int value)
        {
            return Call(service => service.GetData(value));
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            return Call(service => service.GetDataUsingDataContract(composite));
        }

        public void Dispose()
        {
            DisposeMethod.DisposeCommunicationObject(channelFactory);
        }
    }
}
