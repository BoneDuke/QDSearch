using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.DataModel
{
    /// <summary>
    /// Тип цены (за человека, за номер)
    /// </summary>
    [Serializable]
    public enum PriceForType : ushort
    {
        /// <summary>
        /// За человека
        /// </summary>
        PerMen = 0,
        /// <summary>
        /// За номер
        /// </summary>
        PerRoom = 1
    }
}
