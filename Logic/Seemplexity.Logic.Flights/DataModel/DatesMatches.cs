using System.Runtime.Serialization;

namespace Seemplexity.Logic.Flights.DataModel
{
    [DataContract]
    public class DatesMatches
    {
        [DataMember]
        public FlightVariant FlightParamsTo { get; set; }
        [DataMember]
        public FlightVariant FlightParamsFrom { get; set; }

        public override string ToString()
        {
            var result = FlightParamsTo.ToString() + "_" + FlightParamsFrom.ToString();
            return result;
        }
    }
}
