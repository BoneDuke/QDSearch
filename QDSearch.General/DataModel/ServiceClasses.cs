using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.DataModel
{

    /// <summary>
    /// Классы услуг
    /// </summary>
    [Serializable]
    public enum ServiceClass : uint
    {
        /// <summary>
        /// Перелет
        /// </summary>
        Flight = 1,
        /// <summary>
        /// Трансфер
        /// </summary>
        Transfer = 2,
        /// <summary>
        /// Проживание
        /// </summary>
        Hotel = 3,
        /// <summary>
        /// Экскурсия
        /// </summary>
        Excursion = 4,
        /// <summary>
        /// Виза
        /// </summary>
        Visa = 5,
        /// <summary>
        /// Страховка
        /// </summary>
        Insurance = 6,
        /// <summary>
        /// Доп. усуга в отеле
        /// </summary>
        AddHotelService = 8,
        /// <summary>
        /// Доплата
        /// </summary>
        Surcharge = 1004,
        
    }
}
