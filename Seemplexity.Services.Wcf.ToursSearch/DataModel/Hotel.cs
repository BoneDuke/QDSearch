using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Seemplexity.Services.Wcf.ToursSearch.DataModel
{
    /// <summary>
    /// Класс отеля
    /// </summary>
    [DataContract]
    public sealed class Hotel
    {
        /// <summary>
        /// Ключ отеля
        /// </summary>
        [DataMember]
        public int Key { get; private set; }
        /// <summary>
        /// Название отеля
        /// </summary>
        [DataMember]
        public string Name { get; private set; }
        /// <summary>
        /// Категория отеля
        /// </summary>
        [DataMember]
        public string Stars { get; private set; }
        /// <summary>
        /// Ключ курорта
        /// </summary>
        [DataMember]
        public int ResortKey { get; private set; }
        /// <summary>
        /// Название курорта
        /// </summary>
        [DataMember]
        public string ResortName { get; private set; }
        /// <summary>
        /// Ссылка на описание
        /// </summary>
        [DataMember]
        public string Url { get; private set; }

        internal Hotel(QDSearch.DataModel.Hotel hotel)
        {
            if (hotel == null) throw new ArgumentNullException("hotel");

            Key = hotel.Key;
            Name = hotel.Name;
            Stars = hotel.Stars;
            ResortKey = hotel.ResortKey;
            ResortName = hotel.ResortName;
            Url = hotel.Url;
        }

        internal static List<Hotel> GetHotelsList(List<QDSearch.DataModel.Hotel> hotels)
        {
            return hotels.Select(h => new Hotel(h)).ToList();
        }
    }
}