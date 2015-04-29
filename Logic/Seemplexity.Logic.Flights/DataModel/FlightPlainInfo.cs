using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QDSearch.DataModel;
using QDSearch.Extensions;
using QDSearch.Repository.MtSearch;

namespace Seemplexity.Logic.Flights.DataModel
{
    public class FlightPlainInfo
    {

        public int CharterKey { get; set; }
        public int ClassKey { get; set; }
        public DateTime FlightDateTimeFrom { get; set; }
        public DateTime FlightDateTimeTo { get; set; }
        public string FlightNumber { get; set; }
        public string AirportFrom { get; set; }
        public string AirportTo { get; set; }
        public string FlightClassName { get; set; }
        public QuotaStatePlaces QuotaState { get; set; }
        public string AirlineCode { get; set; }
        public int AirlineKey { get; set; }
        public string AirlineName { get; set; }
    }
}
