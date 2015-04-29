using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Seemplexity.Logic.Flights.DataModel
{
    public class CharterSchedulePlainInfo
    {
        public List<DayOfWeek> DaysOfWeek { get; set; }
        public string AirlineName { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string AircraftName { get; set; }
        public string FlightNum { get; set; }
        public string DirectCharterTime { get; set; }
        public string DirectAirportFromName { get; set; }
        public string DirectAirportToName { get; set; }
        public string BackCharterTime { get; set; }
        public string BackAirportFromName { get; set; }
        public string BackAirportToName { get; set; }
    }
}
