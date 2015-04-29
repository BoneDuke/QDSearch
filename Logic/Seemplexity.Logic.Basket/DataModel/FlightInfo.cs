using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QDSearch.DataModel;

namespace Seemplexity.Logic.Basket.DataModel
{
    public class FlightInfo : ServiceInfo
    {
        /// <summary>
        /// Направление перелета
        /// </summary>
        public FlightDirection Direction { get; set; }
        /// <summary>
        /// Группа класса перелета (бизнес, эконом)
        /// </summary>
        public int FlightGroupKey { get; set; }

        /// <summary>
        /// Время вылета
        /// </summary>
        public DateTime FlightTimeStart { get; set; }
        /// <summary>
        /// Время прилета
        /// </summary>
        public DateTime FlightTimeEnd { get; set; }
    }
}
