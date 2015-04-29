using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.DataModel
{
    /// <summary>
    /// Представлеие элемента фильтра
    /// </summary>
    public class SearchFilterItem
    {
        public FilterTour Tour { get; set; }
        /// <summary>
        /// Список ключей отелей
        /// </summary>
        /// <returns></returns>
        public List<FilterHotel> Hotels { get; set; }
    }
}
