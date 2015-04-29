using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Seemplexity.Logic.Flights.DataModel
{
    /// <summary>
    /// Параметры рейса на дату
    /// </summary>
    [DataContract]
    public class FlightVariant
    {
        public FlightVariant()
        {
            
        }
        public FlightVariant(FlightVariant from)
        {
            if (from.FlightParamKeys != null)
                FlightParamKeys = new FlightVariantKeys(from.FlightParamKeys);
            CharterClassCode = from.CharterClassCode;
            PortCodeFrom = from.PortCodeFrom;
            CityKeyFrom = from.CityKeyFrom;
            PortCodeTo = from.PortCodeTo;
            CityKeyTo = from.CityKeyTo;
            AirlineCode = from.AirlineCode;
            FlightNumber = from.FlightNumber;
            AircraftType = from.AircraftType;
            DurationMin = from.DurationMin;
            DurationMax = from.DurationMax;
            DepartTime = from.DepartTime;
            ArrivalTime = from.ArrivalTime;
            if (from.FlightQuotaPriceData != null)
                FlightQuotaPriceData = new QuotaPriceData(from.FlightQuotaPriceData);
        }
        /// <summary>
        /// Ключи параметров перелета
        /// </summary>
        [DataMember]
        public FlightVariantKeys FlightParamKeys { get; set; }
        /// <summary>
        /// Код класса перелета
        /// </summary>
        [DataMember]
        public string CharterClassCode { get; set; }
        /// <summary>
        /// Код города вылета
        /// </summary>
        [DataMember]
        public string PortCodeFrom { get; set; }
        /// <summary>
        /// Ключ города вылета
        /// </summary>
        public int CityKeyFrom { get; set; }
        /// <summary>
        /// Код города прилета
        /// </summary>
        [DataMember]
        public string PortCodeTo { get; set; }
        /// <summary>
        /// Ключ города прилета
        /// </summary>
        public int CityKeyTo { get; set; }
        /// <summary>
        /// Код авиакомпании
        /// </summary>
        [DataMember]
        public string AirlineCode { get; set; }
        /// <summary>
        /// Номер рейса
        /// </summary>
        [DataMember]
        public int FlightNumber { get; set; }
        /// <summary>
        /// Тип самолета
        /// </summary>
        [DataMember]
        public string AircraftType { get; set; }
        /// <summary>
        /// Минимальная продолжительность тура
        /// </summary>
        public short DurationMin { get; set; }
        /// <summary>
        /// Максимальная продолжительность тура
        /// </summary>
        public short DurationMax { get; set; }
        /// <summary>
        /// Время и дата вылета
        /// </summary>
        [DataMember]
        public DateTime DepartTime { get; set; }
        /// <summary>
        /// Время и дата прилета
        /// </summary>
        [DataMember]
        public DateTime ArrivalTime { get; set; }
        /// <summary>
        /// Информация о цене и квотах
        /// </summary>
        [DataMember]
        public QuotaPriceData FlightQuotaPriceData { get; set; }
        public override string ToString()
        {
            var result = String.Format("{0}_{1}_{2}_{3}_{4}_{5}", FlightParamKeys, DurationMin, DurationMax, DepartTime, ArrivalTime, FlightQuotaPriceData);
            return result;
        }
    }

    internal class FlightParamsByDateComparer : IEqualityComparer<FlightVariant>
    {
        public bool Equals(FlightVariant x, FlightVariant y)
        {
            var res = x.FlightParamKeys.CharterClassKey == y.FlightParamKeys.CharterClassKey
                       && x.FlightParamKeys.CharterKey == y.FlightParamKeys.CharterKey
                       && x.FlightParamKeys.PacketKey == y.FlightParamKeys.PacketKey
                       && x.FlightParamKeys.PartnerKey == y.FlightParamKeys.PartnerKey
                       && x.DepartTime == y.DepartTime;

            return res;
        }

        public int GetHashCode(FlightVariant obj)
        {
            //Check whether the object is null 
            if (ReferenceEquals(obj, null)) return 0;

            var res = obj.FlightParamKeys.CharterClassKey.GetHashCode()
                ^ obj.FlightParamKeys.CharterKey.GetHashCode()
                ^ obj.FlightParamKeys.PacketKey.GetHashCode()
                ^ obj.FlightParamKeys.PartnerKey.GetHashCode()
                ^ obj.DepartTime.GetHashCode();

            return res;
        }
    }

    internal class FlightParamsByDateTupleComparer : IEqualityComparer<Tuple<FlightVariant, FlightVariant>>
    {
        public bool Equals(Tuple<FlightVariant, FlightVariant> x, Tuple<FlightVariant, FlightVariant> y)
        {
            var comparer = new FlightParamsByDateComparer();
            var res1 = comparer.Equals(x.Item1, y.Item1) || x.Item1 == null && y.Item1 == null;
            var res2 = comparer.Equals(x.Item2, y.Item2) || x.Item2 == null && y.Item2 == null;
            return res1 && res2;
        }

        public int GetHashCode(Tuple<FlightVariant, FlightVariant> obj)
        {
            var comparer = new FlightParamsByDateComparer();
            if (ReferenceEquals(obj, null)) return 0;

            var res = comparer.GetHashCode(obj.Item1) ^ comparer.GetHashCode(obj.Item2);
            return res;
        }
    }

}
