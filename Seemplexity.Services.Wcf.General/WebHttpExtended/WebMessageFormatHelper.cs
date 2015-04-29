using System.ServiceModel.Web;

namespace Seemplexity.Services.Wcf.WebHttpExtended
{
    internal static class WebMessageFormatHelper
    {
        internal static bool IsDefined(WebMessageFormat format)
        {
            return (format == WebMessageFormat.Xml || format == WebMessageFormat.Json);
        }
    }
}