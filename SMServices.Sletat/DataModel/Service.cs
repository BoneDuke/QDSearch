using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SMServices.Sletat.DataModel
{
    public class Service : IXmlCompatible
    {
        public int Id { get; set; }
        public ServiceType Type { get; set; }
        public string Name { get; set; }
        public bool IsIncluded { get; set; }
        public string Description { get; set; }
        public int Surcharge { get; set; }
        public string FlightCompatibleIds { get; set; }
        public FlightClass FlightClass { get; set; }
        public QuotaAvailability FlightAvailability { get; set; }
        public int FlightPlacesCount { get; set; }
        public string FlightAirportFrom { get; set; } 
        public string FlightAirportTo { get; set; }
        public string FlightNum { get; set; }
        public string FlightAirline { get; set; }
        public DateTime FlightStartDateTime { get; set; }
        public DateTime FlightEndDateTime { get; set; }
        public string FlightAircraft { get; set; }
        public string ToXml()
        {
            return
                String.Format(
                    @"<service id=""{0}"" type=""{1}"" name=""{2}"" isIncluded=""{3}"" description=""{4}"" surcharge=""{5}"" flightCompatibleIds=""{6}"" 
flightClass=""{7}"" flightAvailability=""{8}"" flightPlacesCount=""{9}"" flightAirportFrom=""{10}"" flightAirportTo=""{11}"" flightNum=""{12}"" flightAirline=""{13}"" 
flightStartDateTime=""{14:dd.MM.yyyy HH:mm}"" flightEndDateTime=""{15:dd.MM.yyyy HH:mm}"" flightAircraft=""{16}"" />", 
                                                                             Id,
                                                                             Type,
                                                                             HttpUtility.HtmlEncode(Name),
                                                                             IsIncluded,
                                                                             HttpUtility.HtmlEncode(Description), 
                                                                             Surcharge,
                                                                             HttpUtility.HtmlEncode(FlightCompatibleIds),
                                                                             FlightClass,
                                                                             FlightAvailability,
                                                                             FlightPlacesCount,
                                                                             HttpUtility.HtmlEncode(FlightAirportFrom),
                                                                             HttpUtility.HtmlEncode(FlightAirportTo),
                                                                             HttpUtility.HtmlEncode(FlightNum),
                                                                             HttpUtility.HtmlEncode(FlightAirline),
                                                                             FlightStartDateTime,
                                                                             FlightEndDateTime,
                                                                             HttpUtility.HtmlEncode(FlightAircraft));
        }
    }
}
