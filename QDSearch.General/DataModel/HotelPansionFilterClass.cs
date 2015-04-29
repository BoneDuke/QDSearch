using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.DataModel
{
    /// <summary>
    /// Класс для работы с нижним фильтром, продолжительности, отели, питания
    /// </summary>
    public class HotelPansionFilterClass
    {
        /// <summary>
        /// Продолжительность в ночах
        /// </summary>
        public int Nights { get; set; }
        /// <summary>
        /// Ключ отеля
        /// </summary>
        public int HotelKey { get; set; }
        /// <summary>
        /// Ключ питания
        /// </summary>
        public int PansionKey { get; set; }
    }
}
