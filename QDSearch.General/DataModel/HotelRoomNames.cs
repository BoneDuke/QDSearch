using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.DataModel
{
    /// <summary>
    /// Класс для получения строковых значений по ключу HotelRoom
    /// </summary>
    [Serializable]
    public class HotelRoomNames
    {
        /// <summary>
        /// Ключ HotelRoom
        /// </summary>
        public int HotelRoomKey { get; set; }
        /// <summary>
        /// Ключ размещения (HR_ACKey)
        /// </summary>
        public int AccomodationKey { get; set; }
        /// <summary>
        /// Строковое значение размещения
        /// </summary>
        public string AccomodationName { get; set; }
        /// <summary>
        /// Ключ типа размещения (HR_RMKey)
        /// </summary>
        public int RoomKey { get; set; }
        /// <summary>
        /// Строковое значение типа номера
        /// </summary>
        public string RoomName { get; set; }
        /// <summary>
        /// Ключ категории номера (HR_RCKey)
        /// </summary>
        public int RoomCategoryKey { get; set; }
        /// <summary>
        /// Строковое значение категории номера
        /// </summary>
        public string RoomCategoryName { get; set; }
    }
}
