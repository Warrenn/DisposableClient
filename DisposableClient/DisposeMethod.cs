using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace DisposableClient
{
    public static class DisposeMethod
    {
        public static void DisposeCommunicationObject(object instance)
        {
            var communicationObject = instance as ICommunicationObject;
            if (communicationObject == null) return;
            var state = communicationObject.State;
            if (state == CommunicationState.Closed) return;
            try
            {
                if (state == CommunicationState.Faulted)
                {
                    communicationObject.Abort();
                    return;
                }
                communicationObject.Close();
            }
            catch (CommunicationException ex)
            {
                Trace.TraceError(ex.ToString());
                if (state == CommunicationState.Closed) return;
                communicationObject.Abort();
            }
        }
    }
}
