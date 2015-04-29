using System;
using System.Collections.Generic;

namespace QDSearch.DataModel
{
    /// <summary>
    /// Класс, описывающий плоскую структуру перелета
    /// </summary>
    public class FlightPlainInfo
    {
        public FlightPlainInfo()
        {
            QuotaState = new QuotaStatePlaces();
        }

        public FlightPlainInfo(FlightPlainInfo from)
        {
            CharterKey = from.CharterKey;
            ClassKey = from.ClassKey;
            FlightDateTimeFrom = from.FlightDateTimeFrom;
            FlightDateTimeTo = from.FlightDateTimeTo;
            FlightNumber = from.FlightNumber;
            AirportFrom = from.AirportFrom;
            AirportTo = from.AirportTo;
            FlightClassName = from.FlightClassName;
            QuotaState = new QuotaStatePlaces(from.QuotaState);
            AirlineCode = from.AirlineCode;
            AirlineKey = from.AirlineKey;
            AirlineName = from.AirlineName;
            PartnerKey = from.PartnerKey;
            AircraftCode = from.AirlineCode;
        }

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
        public int PartnerKey { get; set; }
        public string AircraftCode { get; set; }
    }

    public class FlightPlainInfoCompare : IEqualityComparer<FlightPlainInfo>
    {
        public bool Equals(FlightPlainInfo x, FlightPlainInfo y)
        {
            bool equals = x.CharterKey == y.CharterKey
                          && x.ClassKey == y.ClassKey
                          && x.FlightDateTimeFrom == y.FlightDateTimeFrom
                          && x.FlightDateTimeTo == y.FlightDateTimeTo
                          && x.PartnerKey == y.PartnerKey
                          && x.AirlineKey == y.AirlineKey;
            return equals;
        }
        public int GetHashCode(FlightPlainInfo info)
        {
            //Check whether the object is null 
            if (ReferenceEquals(info, null)) return 0;

            var res = info.CharterKey.GetHashCode()
                ^ info.ClassKey.GetHashCode()
                ^ info.FlightDateTimeFrom.GetHashCode()
                ^ info.FlightDateTimeTo.GetHashCode()
                ^ info.PartnerKey.GetHashCode()
                ^ info.AirlineKey.GetHashCode();

            return res;
        }
    }
}
