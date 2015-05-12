using System.ServiceModel;

namespace WcfServiceLibrary
{
    [ServiceContract]
    public interface ITestService
    {
        [OperationContract]
        DataContractTest PeformSomething(DataContractTest contract);
    }
}
