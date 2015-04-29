using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web;

namespace Seemplexity.Services.Wcf.HttpCachePolicy
{
    public class HttpCachePolicyBehavior : IEndpointBehavior
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new HttpCachePolicyMessageInspector(this));
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            var binding = endpoint.Binding;
            if (binding.Scheme != "http" && binding.Scheme != "https")
            {
                throw new InvalidOperationException("The http cache policy behavior is only compatible with http and https bindings.");
            }
        }

        public CacheControlModes CacheControlMode { get; set; }

        public TimeSpan CacheControlMaxAge { get; set; }

        public DateTime? HttpExpires { get; set; }

        public string CacheControlCustom { get; set; }
    }
}
