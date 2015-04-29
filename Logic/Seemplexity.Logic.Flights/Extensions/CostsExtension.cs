using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using QDSearch;
using QDSearch.DataModel;
using QDSearch.Extensions;
using QDSearch.Helpers;
using QDSearch.Repository.MtSearch;
using Seemplexity.Logic.Flights.DataModel;

namespace Seemplexity.Logic.Flights.Extensions
{
    public static class CostsExtension
    {
        /// <summary>
        /// Возвращает список ключей пакетов, в которых встречаются перечисленные перелеты
        /// </summary>
        /// <param name="dc">Контекст поисковой БД</param>
        /// <param name="charterKeys">Список ключей перелетов</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static IList<Tuple<int, PacketType>> GetPacketKeysByCarterKeys(this MtSearchDbDataContext dc, IEnumerable<int> charterKeys, out string hash)
        {
            List<Tuple<int, PacketType>> result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, String.Join("_", charterKeys));
            if ((result = CacheHelper.GetCacheItem<List<Tuple<int, PacketType>>>(hash)) != default(List<Tuple<int, PacketType>>)) return result;

            var cacheDependencies = new List<string>();
            var packetKeys = (from c in dc.tbl_Costs
                      where c.CS_SVKEY == (int)ServiceClass.Flight
                            && (!c.CS_DATEEND.HasValue || DateTime.Now.Date <= c.CS_DATEEND.Value)
                            && charterKeys.Contains(c.CS_CODE)
                            && c.CS_PKKEY.HasValue
                      select c.CS_PKKEY.Value)
                .Distinct()
                .ToList();

            cacheDependencies.Add(QDSearch.Extensions.CostsExtension.TableName);

            CacheHelper.AddCacheData(hash, result, cacheDependencies, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        public static PriceValue GetFlightPriceByParams(this MtSearchDbDataContext dc, int code, int subCode1, int partnerKey, int packetKey, DateTime date, int duration, out string hash)
        {
            PriceValue result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, CacheHelper.GetCacheKeyHashed(new[] { code.ToString(), subCode1.ToString(), partnerKey.ToString(), packetKey.ToString(), date.ToString("yyyy-MM-dd"), duration.ToString() }));
            if ((result = CacheHelper.GetCacheItem<PriceValue>(hash)) != null)
                return result;

            string hashOut;
            var dayOfWeek = ((int) date.DayOfWeek).ToString();

            //koshelev,сортировка как в хранимке GetServiceCost
            //CS_CheckInDateBEG Desc, CS_CheckInDateEnd, CS_Date Desc, CS_DATEEND, CS_LONGMIN desc, 
			//CS_LONG, CS_DateSellBeg Desc, CS_DateSellEnd, CS_BYDAY,	CS_WEEK ASC
            var costsList = (from c in dc.GetFlightCostsByPacketKey(packetKey, out hashOut)
                where c.CS_CODE == code
                      && c.CS_SUBCODE1 == subCode1
                      && c.CS_PRKEY == partnerKey
                      && c.CS_PKKEY == packetKey
                      && (c.cs_DateSellBeg == null || c.cs_DateSellBeg <= DateTime.Now)
                      && (c.cs_DateSellEnd == null || c.cs_DateSellEnd >= DateTime.Now)
                      && (duration < 0 || c.CS_LONGMIN == null || c.CS_LONGMIN <= duration)
                      && (duration < 0 || c.CS_LONG == null || c.CS_LONG >= duration)
                      && (c.CS_DATE == null || c.CS_DATE <= date.Date)
                      && (c.CS_DATEEND == null || c.CS_DATEEND >= date.Date)
                      && (String.IsNullOrEmpty(c.CS_WEEK) || c.CS_WEEK.IndexOf(dayOfWeek) >= 0)
                select c)
                .OrderByDescending(c => c.CS_CHECKINDATEBEG)
                .ThenBy(c => c.CS_CHECKINDATEEND)
                .ThenByDescending(c => c.CS_DATE)
                .ThenBy(c => c.CS_DATEEND)
                .ThenByDescending(c => c.CS_LONGMIN)
                .ThenBy(c => c.CS_LONG)
                .ThenByDescending(c => c.cs_DateSellBeg)
                .ThenBy(c => c.cs_DateSellEnd)
                .ThenBy(c => c.CS_BYDAY)
                .ThenBy(c => c.CS_WEEK)
                .ToList();

            result = (from c in costsList
                      select new PriceValue { Price = c.CS_COST, Rate = c.CS_RATE })
                .FirstOrDefault() ?? new PriceValue
                {
                    Price = null,
                    Rate = String.Empty
                };

            CacheHelper.AddCacheData(hash, result, new List<string>() { hashOut }, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }
    }
}
