using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Seemplexity.Services.Wcf
{
    public abstract class HttpDispatchMessageInspector : IDispatchMessageInspector
    {
        object IDispatchMessageInspector.AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            if (!request.Properties.ContainsKey(HttpRequestMessageProperty.Name)) return InvalidHttpRequest.Value;

            var httpRequest = (HttpRequestMessageProperty)request.Properties[HttpRequestMessageProperty.Name];
            return AfterReceiveRequest(ref request, httpRequest, channel, instanceContext);
        }

        void IDispatchMessageInspector.BeforeSendReply(ref Message reply, object correlationState)
        {
            if (correlationState != null && correlationState is InvalidHttpRequest) return;

            HttpResponseMessageProperty httpResponse;
            if (reply.Properties.ContainsKey(HttpResponseMessageProperty.Name))
            {
                httpResponse = (HttpResponseMessageProperty)reply.Properties[HttpResponseMessageProperty.Name];
            }
            else
            {
                httpResponse = new HttpResponseMessageProperty();
                reply.Properties.Add(HttpResponseMessageProperty.Name, httpResponse);
            }

            BeforeSendReply(ref reply, httpResponse, correlationState);
        }

        public abstract object AfterReceiveRequest(ref Message request, HttpRequestMessageProperty httpRequest, IClientChannel channel, InstanceContext instanceContext);

        public abstract void BeforeSendReply(ref Message reply, HttpResponseMessageProperty httpResponse, object correlationState);

        #region Struct: InvalidHttpRequest

        // Used only to identify a http response that is missing a corresponding http request
        private struct InvalidHttpRequest
        {
            public static readonly InvalidHttpRequest Value = new InvalidHttpRequest();
        }

        #endregion
    }
}
