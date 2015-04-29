using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.DataModel
{
    /// <summary>
    /// Виды направлений перелетов
    /// </summary>
    public enum FlightDirection
    {
        /// <summary>
        /// Не установлено
        /// </summary>
        Undefined,
        /// <summary>
        /// Прямой перелет
        /// </summary>
        DirectFlight,
        /// <summary>
        /// Обратный перелет
        /// </summary>
        BackFlight,
        /// <summary>
        /// Промежуточный перелет
        /// </summary>
        Intermediate
    }
}
