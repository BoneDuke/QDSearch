using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Seemplexity.Services.Wcf.WebHttpExtended
{
    /// <summary>
    /// Расширенный WebHttpBehavior поддержкой QueryStringConverterExtended вместо стандартного
    /// </summary>
    public class WebHttpExtendedBehavior : WebHttpBehavior
    {
        protected override QueryStringConverter GetQueryStringConverter(OperationDescription operationDescription)
        {
            return new QueryStringConverterExtended();
        }
    }
}
