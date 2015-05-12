﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using WcfServiceLibrary;

namespace UnitTestProject
{
    public class ServiceAgentTestService : ServiceAgent<ITestService>, ITestService
    {
        public ServiceAgentTestService()
        {
            channelFactory = new ChannelFactory<ITestService>(new BasicHttpBinding(), "http://localhost:7654/testservice");
            channelFactory.Open();
        }

        public DataContractTest PeformSomething(DataContractTest contract)
        {
            return Call(service => service.PeformSomething(contract));
        }
    }
}
