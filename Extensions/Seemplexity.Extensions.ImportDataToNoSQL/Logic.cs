using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net;
using QDSearch.DataModel;
using QDSearch.Repository.MtMain;
using QDSearch.Repository.MtSearch;
using QDSearch.Extensions;

namespace Seemplexity.Extensions.ImportDataToNoSQL
{
    public static class Logic
    {
        public static IList<SearchResultItem> ConvertCalcTourToNoSql(this MtMainDbDataContext mainDc, MtSearchDbDataContext searchDc, int tourKey)
        {
            var result = new List<SearchResultItem>();
            string hashOut;
            var tours = searchDc.GetTPToursByKeys(new[] {tourKey}, out hashOut);
            TP_Tour tpTour;
            if (tours != null && tours.Count == 1)
                tpTour = tours[0];
            else
                throw new ArgumentException(String.Format("Неправильный параметр tourKey"));

            var tourString = searchDc.GetTourStringsByKeys(new [] {tpTour.TO_Key}, out hashOut);
            var tpPrices = mainDc.TP_Prices.Where(t => t.TP_TOKey == tourKey && t.TP_Gross != null).ToList();
            foreach (var tpPrice in tpPrices)
            {
                var item = new SearchResultItem();
                // будет заполнен при выдаче результата в поиск
                item.PriceInRates = null;
                item.PriceKey = tpPrice.TP_Key;
                item.Price = tpPrice.TP_Gross.Value;
                item.RateCode = tpTour.TO_Rate;
                item.Date = tpPrice.TP_DateBegin;
                item.PriceFor = tpTour.TO_PriceFor == 0 ? PriceForType.PerMen : PriceForType.PerRoom;
                item.CountryKey = tpTour.TO_CNKey;

            }
            return result;
        }
    }
}
