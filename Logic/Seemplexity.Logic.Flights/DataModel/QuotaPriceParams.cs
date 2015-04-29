using System;
using System.Runtime.Serialization;

namespace Seemplexity.Logic.Flights.DataModel
{
    /// <summary>
    /// Параметры для определения квот и цены по перелету
    /// </summary>
    [DataContract]
    public class QuotaPriceParams
    {
        /// <summary>
        /// Ключи параметров перелета
        /// </summary>
        [DataMember]
        public FlightVariantKeys FlightParamKeys { get; set; }
        /// <summary>
        /// Время и дата вылета
        /// </summary>
        [DataMember]
        public DateTime DepartTime { get; set; }
        /// <summary>
        /// Продолжительность (для пар перелетов)
        /// </summary>
        [DataMember]
        public int? Duration { get; set; }

        public override string ToString()
        {
            return String.Format("{0}_{1}_{2}", FlightParamKeys, DepartTime, Duration);
        }
    }
}
