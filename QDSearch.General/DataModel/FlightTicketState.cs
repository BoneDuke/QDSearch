using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.DataModel
{
    /// <summary>
    /// Перечисление типов подборов перелетов в поиске
    /// </summary>
    public enum FlightTicketState
    {
        /// <summary>
        /// Подбирать все перелеты
        /// </summary>
        All,
        /// <summary>
        /// Подбирать только с перелетами
        /// </summary>
        IncludedOnly,
        /// <summary>
        /// Подбирать только без перелетов
        /// </summary>
        ExcludedOnly
    }
}
