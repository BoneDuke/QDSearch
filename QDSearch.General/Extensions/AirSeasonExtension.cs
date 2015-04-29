using System.Reflection;
using System.Web.Caching;
using QDSearch.DataModel;
using QDSearch.Helpers;
using QDSearch.Repository.MtSearch;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QDSearch.Extensions
{
    /// <summary>
    /// Класс по работе с расписаниями
    /// </summary>
    public static class AirSeasonExtension
    {
        private static readonly object LockAirSeasons = new object();
        /// <summary>
        /// Название таблицы
        /// </summary>
        public const string TableName = "AirSeason";

        public static List<AirSeason> GetAllAirSeasonsByCharterKeys(this MtSearchDbDataContext dc, List<int> charterKeys, out string hash)
        {
            List<AirSeason> result;
            hash = String.Format("{0}_{1}_{2}", MethodBase.GetCurrentMethod().Name, (int)ServiceClass.Flight, String.Join(",", charterKeys));
            if ((result = CacheHelper.GetCacheItem<List<AirSeason>>(hash)) != default(List<AirSeason>)) return result;

            var cacheDependencies = new List<string>
            {
                String.Format("{0}_{1}", CacheHelper.AirSeasonHash, (int)ServiceClass.Flight)
            };

            result = dc.GetAllAirSeasons().Where(a => charterKeys.Contains(a.AS_CHKEY)).ToList();

            CacheHelper.AddCacheData(hash, result, cacheDependencies, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Возвращает список расписаний
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static List<AirSeason> GetAllAirSeasons(this MtSearchDbDataContext dc)
        {
            List<AirSeason> airSeasons;
            if ((airSeasons = CacheHelper.GetCacheItem<List<AirSeason>>(TableName)) != default(List<AirSeason>)) return airSeasons;

            lock (LockAirSeasons)
            {
                if ((airSeasons = CacheHelper.GetCacheItem<List<AirSeason>>(TableName)) != default(List<AirSeason>)) return airSeasons;

                airSeasons = (from a in dc.AirSeasons
                              where a.AS_DATETO >= DateTime.Now.Date
                            select a)
                    .ToList<AirSeason>();

                CacheHelper.AddCacheData(TableName, airSeasons, TableName);
            }

            return airSeasons;
        }
    }
}
