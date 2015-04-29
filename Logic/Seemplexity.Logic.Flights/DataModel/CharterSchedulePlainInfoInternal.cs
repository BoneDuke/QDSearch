using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QDSearch.Helpers;

namespace Seemplexity.Logic.Flights.DataModel
{
    public class CharterSchedulePlainInfoInternal
    {
        public CharterSchedulePlainInfoInternal(int airSeasonKey, int charterKey, DateTime dateFrom, DateTime dateTo, string daysOfWeek, DateTime timeFrom, DateTime timeTo, string portCodeFrom, string portCodeTo, string airlineCode, string flightNumber, string airCraft)
        {
            AirSeasonKey = airSeasonKey;
            CharterKey = charterKey;
            DateFrom = dateFrom;
            DateTo = dateTo;
            TimeFrom = timeFrom;
            TimeTo = timeTo;
            PortCodeFrom = portCodeFrom;
            PortCodeTo = portCodeTo;
            AirlineCode = airlineCode;
            int flightNum = -1;
            Int32.TryParse(flightNumber, out flightNum);
            FlightNumber = flightNum;
            AirCraft = airCraft;

            DaysOfWeek = new List<DayOfWeek>();
            foreach (var day in daysOfWeek.ToCharArray())
            {
                int dayNum = -1;
                if (Int32.TryParse(day.ToString(), out dayNum))
                {
                    DaysOfWeek.Add(Converters.GetDayOfWeekByInt(dayNum));
                }
            }

            CharterDates = new List<DateTime>();
            if (dateFrom <= dateTo)
            {
                for (var dt = dateFrom; dt <= dateTo; dt = dt.AddDays(1))
                {
                    if (DaysOfWeek.Contains(dt.DayOfWeek))
                    {
                        CharterDates.Add(dt);
                    }
                }
            }
            
        }
        public int AirSeasonKey { get; set; }
        public int CharterKey { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public List<DayOfWeek> DaysOfWeek { get; set; }
        public DateTime TimeFrom { get; set; }
        public DateTime TimeTo { get; set; }
        public string PortCodeFrom { get; set; }
        public string PortCodeTo { get; set; }
        public string AirlineCode { get; set; }
        public int FlightNumber { get; set; }
        public string AirCraft { get; set; }

        public List<DateTime> CharterDates { get; set; } 
    }
}
