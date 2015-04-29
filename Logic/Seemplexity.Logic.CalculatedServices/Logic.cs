using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
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

namespace Seemplexity.Logic.CalculatedServices
{
    public static class Logic
    {
        public static SearchResultItem GetPriceInfo(this MtMainDbDataContext mainDc, MtSearchDbDataContext searchDc, int priceKey, out string hash)
        {
            SearchResultItem result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, priceKey);
            if ((result = CacheHelper.GetCacheItem<SearchResultItem>(hash)) != null) return result;

            var cacheDependencies = new List<string>();
            result = new SearchResultItem();

            var tpPrice = mainDc.GetTPPriceByKey(priceKey);
            if (tpPrice != null)
            {
                var tpTour = searchDc.GetAllTPTours().Single(t => t.TO_Key == tpPrice.TP_TOKey);

                string hashOut;
                var countryCityKeys = mainDc.GetCountryCityKeysByTourKey((int)priceKey, out hashOut);
                cacheDependencies.Add(hashOut);

                var searchResult = searchDc.PagingOnClient(mainDc, countryCityKeys.Item2, countryCityKeys.Item1, priceKey, out hashOut);
                cacheDependencies.Add(hashOut);

                if (searchResult.SearchItems.Count > 0)
                {
                    result = searchResult.SearchItems[0];
                }
            }
            

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }
    }
}
