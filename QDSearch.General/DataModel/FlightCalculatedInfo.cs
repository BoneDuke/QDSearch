using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.DataModel
{
    public class FlightCalculatedInfo
    {
        /// <summary>
        /// Ключ прямого перелета, если есть
        /// </summary>
        public int? CharterKey { get; set; }
        /// <summary>
        /// День прямого перелета, если есть
        /// </summary>
        public int? CharterDay { get; set; }
        /// <summary>
        /// Ключ партнера прямого рейса, если есть
        /// </summary>
        public int? CharterPartnerKey { get; set; }
        /// <summary>
        /// Ключ пакета прямого рейса, если есть
        /// </summary>
        public int? CharterPacketKey { get; set; }
        /// <summary>
        /// Признак "подбирать ли прямой рейс"
        /// </summary>
        public bool FindDirectFlight { get; set; }
        /// <summary>
        /// Ключ обратного рейса, если есть
        /// </summary>
        public int? BackCharterKey { get; set; }
        /// <summary>
        /// День обратного рейса, если есть
        /// </summary>
        public int? BackCharterDay { get; set; }
        /// <summary>
        /// Ключ партнера обратного рейса, если есть
        /// </summary>
        public int? BackCharterPartnerKey { get; set; }
        /// <summary>
        /// Ключ пакета обратного рейса, если есть
        /// </summary>
        public int? BackCharterPacketKey { get; set; }
        /// <summary>
        /// Признак "подбирать обратный рейс"
        /// </summary>
        public bool FindBackFlight { get; set; }
    }
}
