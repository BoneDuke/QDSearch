using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Seemplexity.Logic.Flights.DataModel
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
