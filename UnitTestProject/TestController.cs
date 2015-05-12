using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WcfServiceLibrary;

namespace UnitTestProject
{
    public class TestController
    {
        private readonly ITestService service;

        public TestController(ITestService service)
        {
            this.service = service;
        }

        public void TestMethod()
        {
            for (var i = 0; i < 10000; i++)
            {
                var dc = new DataContractTest()
                {
                    Field1 = "F",
                    Field2 = 13
                };
                dc = service.PeformSomething(dc);
                dc.Field2 = 10;
            }

        }
    }
}
