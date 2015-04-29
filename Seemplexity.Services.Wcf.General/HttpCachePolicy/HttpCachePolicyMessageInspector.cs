using System;
using System.Globalization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Web;

namespace Seemplexity.Services.Wcf.HttpCachePolicy
{
    public class HttpCachePolicyMessageInspector : HttpDispatchMessageInspector
    {
        private readonly HttpCachePolicyBehavior _behavior;

        public HttpCachePolicyMessageInspector(HttpCachePolicyBehavior behavior)
        {
            _behavior = behavior;
        }

        public override object AfterReceiveRequest(ref Message request, HttpRequestMessageProperty httpRequest, IClientChannel channel, InstanceContext instanceContext)
        {
            // We only support caching of GET requests.
            if (!String.Equals(httpRequest.Method, "GET", StringComparison.Ordinal))
                return null;

            return GetCacheControlValueString(_behavior);
        }

        public override void BeforeSendReply(ref Message reply, HttpResponseMessageProperty httpResponse, object correlationState)
        {
            if (_behavior.CacheControlMode == CacheControlModes.DisableCache)
            {
                httpResponse.Headers.Set("Expires", "-1");
                httpResponse.Headers.Set("Pragma", "no-cache");
                httpResponse.Headers.Set("Last-Modified", DateTime.Now.ToUniversalTime().ToString("R"));
            }
            if (_behavior.CacheControlMode == CacheControlModes.UseExpires
                && _behavior.HttpExpires.HasValue)
                httpResponse.Headers.Set("Expires", _behavior.HttpExpires.Value.ToUniversalTime().ToString("R"));

            var cacheControlStr = correlationState as string;
            if (!String.IsNullOrEmpty(cacheControlStr))
                httpResponse.Headers.Set("Cache-Control", cacheControlStr);
        }

        private static string GetCacheControlValueString(HttpCachePolicyBehavior behavior)
        {
            var cacheControlStr = new StringBuilder();

            if (behavior.CacheControlMode == CacheControlModes.NoControl
                || behavior.CacheControlMode == CacheControlModes.UseExpires)
                return behavior.CacheControlCustom;

            if (behavior.CacheControlMode == CacheControlModes.DisableCache)
                cacheControlStr.Append("no-cache, no-store, must-revalidate,");

            if (behavior.CacheControlMode == CacheControlModes.UseMaxAge)
                cacheControlStr.AppendFormat("max-age={0},", behavior.CacheControlMaxAge.TotalSeconds);

            cacheControlStr.Append(behavior.CacheControlCustom);
            if (cacheControlStr.Length > 0
                && cacheControlStr[cacheControlStr.Length - 1] == ',')
                cacheControlStr.Remove(cacheControlStr.Length - 1, 1);

            return cacheControlStr.ToString();
        }
    }
}
