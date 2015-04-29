using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Seemplexity.Logic.Flights.DataModel
{
    /// <summary>
    /// Класс запроса на получение квоты и цены
    /// </summary>
    [DataContract]
    public class QuotaPriceQuery
    {
        public QuotaPriceQuery()
        {
            FlightParams = new List<QuotaPriceParams>();
        }

        [DataMember]
        public List<QuotaPriceParams> FlightParams { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string UserPassword { get; set; }
        public override string ToString()
        {
            return String.Format("{0}_{1}_{2}", String.Join("_", FlightParams), UserName, UserPassword);
        }
    }
}
