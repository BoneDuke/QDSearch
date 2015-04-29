using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using QDSearch.DataModel;

namespace Seemplexity.Logic.Flights.DataModel
{
    /// <summary>
    /// Данные по цене и квотам на перелет
    /// </summary>
    [DataContract]
    public class QuotaPriceData
    {
        public QuotaPriceData()
        {
            
        }
        public QuotaPriceData(QuotaPriceData from)
        {
            QuotaState = from.QuotaState;
            QuotaPlaces = from.QuotaPlaces;
            if (from.PriceValue != null)
                PriceValue = new PriceValue(from.PriceValue);
        }

        [XmlIgnore]
        public QuotesStates QuotaState { get; set; }
        /// <summary>
        /// Число мест на рейсе
        /// </summary>
        [DataMember]
        public uint QuotaPlaces { get; set; }
        /// <summary>
        /// Цена рейса
        /// </summary>
        [DataMember]
        public PriceValue PriceValue { get; set; }

        public override string ToString()
        {
            return String.Format("{0}_{1}", QuotaPlaces, PriceValue);
        }
    }
}
