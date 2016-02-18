using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;

namespace DisposableClient
{
    public class ConfigChannelFactory<T> :
        ChannelFactory<T> 
    {
        public ConfigChannelFactory()
            : this(GetEndPointFromConfig().Name)
        {
        }

        public ConfigChannelFactory(string endpointConfigurationName)
            : base(endpointConfigurationName)
        {

        }

        public ConfigChannelFactory(string endpointConfigurationName, EndpointAddress remoteAddress)
            : base(endpointConfigurationName, remoteAddress)
        {

        }

        public ConfigChannelFactory(Binding binding)
            : base(binding)
        {

        }

        public ConfigChannelFactory(Binding binding, string remoteAddress)
            : base(binding, remoteAddress)
        {

        }

        public ConfigChannelFactory(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        {

        }

        public ConfigChannelFactory(ServiceEndpoint endpoint)
            : base(endpoint)
        {

        }
        
        public static ChannelEndpointElement GetEndPointFromConfig()
        {
            var contractType = typeof (T);

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var sectionGroup = ServiceModelSectionGroup.GetSectionGroup(config);
            if (sectionGroup == null)
            {
                throw new ConfigurationErrorsException();
            }

            var client = sectionGroup.Client;
            var endPoint = client.Endpoints.OfType<ChannelEndpointElement>()
                .FirstOrDefault(ep =>
                    (ep.Contract == contractType.Name) ||
                    (ep.Contract == contractType.AssemblyQualifiedName) ||
                    (ep.Contract == contractType.FullName));
            if (endPoint == null)
            {
                throw new ConfigurationErrorsException();
            }
            return endPoint;
        }

        public new static T CreateChannel()
        {
            var element = GetEndPointFromConfig();
            return CreateChannel(element.Name);
        }
    }
}