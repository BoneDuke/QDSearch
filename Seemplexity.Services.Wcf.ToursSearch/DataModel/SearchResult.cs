using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Seemplexity.Services.Wcf.ToursSearch.DataModel
{
    [DataContract]
    public sealed class SearchResult
    {
        internal SearchResult(QDSearch.DataModel.SearchResult searchResult)
        {
            IsMorePages = searchResult.IsMorePages;
            NextPageRowCounter = searchResult.NextPageRowCounter;
            SearchItems =  SearchResultItem.GetSearchResultItemsList(searchResult.SearchItems);
            SortType = new Tuple<SortingColumn, SortingDirection>((SortingColumn) searchResult.SortType.Item1, (SortingDirection) searchResult.SortType.Item2);
        }
        /// <summary>
        /// Нужно ли выводить ссылку на след. страницы
        /// </summary>
        [DataMember]
        public bool IsMorePages { get; private set; }

        /// <summary>
        /// Номер строки, с которой будет начинаться след. страница
        /// </summary>
        [DataMember]
        public uint NextPageRowCounter { get; private set; }

        /// <summary>
        /// Элементы таблицы с результатами поиска
        /// </summary>
        [DataMember]
        public List<SearchResultItem> SearchItems { get; private set; }
        /// <summary>
        /// Тип сортировки
        /// </summary>
        [DataMember]
        public Tuple<SortingColumn, SortingDirection> SortType { get; private set; }
    }
}