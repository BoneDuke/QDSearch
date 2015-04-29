using System;
using System.Runtime.Serialization;

namespace Seemplexity.Logic.Flights.DataModel
{
    /// <summary>
    /// Параметры перелетов без привязки к датам
    /// </summary>
    [DataContract]
    public class FlightVariantKeys
    {
        public FlightVariantKeys()
        {
            
        }
        public FlightVariantKeys(FlightVariantKeys from)
        {
            CharterKey = from.CharterKey;
            CharterClassKey = from.CharterClassKey;
            PartnerKey = from.PartnerKey;
            PacketKey = from.PacketKey;
        }

        /// <summary>
        /// Ключ из таблицы Charter
        /// </summary>
        [DataMember]
        public int CharterKey { get; set; }
        /// <summary>
        /// Ключ класса перелета
        /// </summary>
        [DataMember]
        public int CharterClassKey { get; set; }
        /// <summary>
        /// Ключ партнера
        /// </summary>
        [DataMember]
        public int PartnerKey { get; set; }
        /// <summary>
        /// Ключ пакета
        /// </summary>
        [DataMember]
        public int PacketKey { get; set; }

        public override string ToString()
        {
            return String.Format("{0}_{1}_{2}_{3}", CharterKey, CharterClassKey, PartnerKey, PacketKey);
        }
    }
}
