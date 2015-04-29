using System.ServiceModel.Web;

namespace Seemplexity.Services.Wcf.WebHttpExtended
{
    internal static class WebMessageBodyStyleHelper
    {
        internal static bool IsDefined(WebMessageBodyStyle style)
        {
            return (style == WebMessageBodyStyle.Bare
                    || style == WebMessageBodyStyle.Wrapped
                    || style == WebMessageBodyStyle.WrappedRequest
                    || style == WebMessageBodyStyle.WrappedResponse);
        }
    }
}