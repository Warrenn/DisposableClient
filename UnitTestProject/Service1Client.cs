using System;
using System.ServiceModel;
using System.Diagnostics;

namespace UnitTestProject.GeneratedProxies
{
    public partial class Service1Client : 
		System.ServiceModel.ClientBase<WcfServiceLibrary.IService1>, 
		WcfServiceLibrary.IService1,
        IDisposable
    {
        public virtual System.String GetData (System.Int32 value)
        {		
			return base.Channel.GetData(value);
        }
        public virtual WcfServiceLibrary.CompositeType GetDataUsingDataContract (WcfServiceLibrary.CompositeType composite)
        {		
			return base.Channel.GetDataUsingDataContract(composite);
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
