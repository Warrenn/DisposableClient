using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WcfServiceLibrary;

namespace UnitTestProject
{
    public class RemoteTestController
    {
        private readonly IService1 service;

        public RemoteTestController(IService1 service)
        {
            this.service = service;
        }

        public void PerformMethod()
        {
            for (var i = 0; i < 10000; i++)
            {
                var dc = new CompositeType
                {
                    BoolValue = false,
                    StringValue = "Asdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdfasdf"
                };
                dc = service.GetDataUsingDataContract(dc);
                dc.StringValue = "";
            }
            for (var i = 0; i < 10000; i++)
            {
                var d =service.GetData(3);
                d.Trim();
            }

        }
    }
}
