using System;
using System.ServiceModel;
using System.Diagnostics;

namespace UnitTestProject.GeneratedProxies
{
    public partial class TestServiceClient : 
		System.ServiceModel.ClientBase<WcfServiceLibrary.ITestService>, 
		WcfServiceLibrary.ITestService,
        IDisposable
    {
        public virtual WcfServiceLibrary.DataContractTest PeformSomething (WcfServiceLibrary.DataContractTest contract)
        {		
			return base.Channel.PeformSomething(contract);
        }
        public void Dispose()
        {
            if (State == CommunicationState.Closed) return;
            try
            {
                if (State == CommunicationState.Faulted)
                {
                    Abort();
                    return;
                }
                Close();
            }
            catch (CommunicationException ex)
            {
                Trace.TraceError(ex.ToString());
                if (State == CommunicationState.Closed) return;
                Abort();
            }
        }
	}
}
