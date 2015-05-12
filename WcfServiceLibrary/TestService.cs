namespace WcfServiceLibrary
{
    public class TestService : ITestService
    {
        public DataContractTest PeformSomething(DataContractTest contract)
        {
            var counter = contract.Field2;
            for (var i = 0; i < counter; i++)
            {
                contract.Field1 += contract.Field1;
            }
            contract.Field3 = contract.Field1;
            return contract;
        }
    }
}
