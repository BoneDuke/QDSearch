using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.DataModel
{
    /// <summary>
    /// Результат поиска
    /// </summary>
    [Serializable]
    public class SearchResult
    {
        /// <summary>
        /// Конструктор класса
        /// </summary>
        public SearchResult()
        {
            SearchItems = new List<SearchResultItem>();
            SortType = new Tuple<SortingColumn, SortingDirection>(SortingColumn.Price, SortingDirection.Asc);
            PriceFor = PriceForType.PerRoom;
        }
        /// <summary>
        /// Нужно ли выводить ссылку на след. страницы
        /// </summary>
        public bool IsMorePages { get; set; }

        /// <summary>
        /// Номер строки, с которой будет начинаться след. страница
        /// </summary>
        public uint NextPageRowCounter { get; set; }

        /// <summary>
        /// Элементы таблицы с результатами поиска
        /// </summary>
        public List<SearchResultItem> SearchItems;
        /// <summary>
        /// Тип сортировки
        /// </summary>
        public Tuple<SortingColumn, SortingDirection> SortType;
        /// <summary>
        /// Тип цены (за номер или за человека)
        /// </summary>
        public PriceForType PriceFor;
    }
}
