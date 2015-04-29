using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using QDSearch;
using QDSearch.DataModel;
using QDSearch.Extensions;
using QDSearch.Helpers;
using QDSearch.Repository.MtMain;
using QDSearch.Repository.MtSearch;
using Seemplexity.Logic.Basket.DataModel;
using Seemplexity.Logic.Basket.Extensions;

namespace Seemplexity.Logic.Basket
{
    public static class Logic
    {
        /// <summary>
        /// Получает информацию по конкретному ключу цены (кэш не используется)
        /// </summary>
        /// <param name="mainDc">Контекст основной БД</param>
        /// <param name="searchDc">Конекст поисковой БД</param>
        /// <param name="priceKey">Ключ цены</param>
        /// <returns></returns>
        public static PriceInfo GetPriceInfo(this MtMainDbDataContext mainDc, MtSearchDbDataContext searchDc, int priceKey)
        {
            var result = mainDc.GetPriceInfoByTPKey(searchDc, priceKey);


            //var tpPrice = mainDc.GetTPPriceByKey(priceKey);
            //if (tpPrice != null)
            //{
            //    var countryCityKeys = mainDc.GetCountryCityKeysByTourKey(priceKey);

            //    var searchResult = searchDc.PagingOnClient(mainDc, countryCityKeys.Item2, countryCityKeys.Item1, priceKey);

            //    if (searchResult.SearchItems.Count > 0)
            //    {
            //        result = searchResult.SearchItems[0];
            //    }
            //}

            return result;
        }
    }
}
