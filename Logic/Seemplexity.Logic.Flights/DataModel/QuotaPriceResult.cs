using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Seemplexity.Logic.Flights.DataModel
{
    /// <summary>
    /// Данные по квотам и цене
    /// </summary>
    [DataContract]
    public class QuotaPriceResult
    {
        public QuotaPriceResult()
        {
            FlightData = new List<QuotaPriceData>();
        }
        [DataMember]
        public List<QuotaPriceData> FlightData { get; set; }
        ///// <summary>
        ///// Данные по перелету "туда"
        ///// </summary>
        //public QuotaPriceData FlightDataTo { get; set; }
        ///// <summary>
        ///// Данные по перелету "оттуда"
        ///// </summary>
        //public QuotaPriceData FlightDataFrom { get; set; }

        public override string ToString()
        {
            return String.Join("_", FlightData);
        }
    }
}
