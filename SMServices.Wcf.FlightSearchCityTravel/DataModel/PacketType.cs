using System;
using System.Runtime.Serialization;

namespace SMServices.Wcf.FlightSearchCityTravel.DataModel
{
    /// <summary>
    /// Тип пакета для подбора перелетов
    /// </summary>
    [Flags]
    [DataContract]
    public enum PacketType
    {
        /// <summary>
        /// Только туда - обратно
        /// </summary>
        [EnumMember]
        TwoWayCharters = 0,
        /// <summary>
        /// Только прямые рейсы
        /// </summary>
        [EnumMember]
        DirectCharters = 1,
        /// <summary>
        /// Только обратные рейсы
        /// </summary>
        [EnumMember]
        BackCharters = 2
    }
}
