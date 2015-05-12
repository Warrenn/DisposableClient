using System.Runtime.Serialization;

namespace WcfServiceLibrary
{
    [DataContract]
    public class DataContractTest
    {
        [DataMember]
        public string Field1 { get; set; }
        [DataMember]
        public int Field2 { get; set; }
        [DataMember]
        public string Field3 { get; set; }
    }
}
