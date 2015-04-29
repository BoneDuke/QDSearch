using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;
using QDSearch.Repository.MtMain;
using QDSearch.Repository.MtSearch;
using QDSearch.Helpers;

namespace QDSearch.Extensions
{
    /// <summary>
    /// Расширение по работе с расчитаными турами
    /// </summary>
    public static class TPToursExtension
    {
        private static readonly object LockTPTours = new object();
        /// <summary>
        /// Название таблицы в БД
        /// </summary>
        public const string TableName = "TP_Tours";

        /// <summary>
        /// Возвращает список всех TP_Tours's
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static List<TP_Tour> GetAllTPTours(this MtSearchDbDataContext dc)
        {
            List<TP_Tour> tours;
            if ((tours = CacheHelper.GetCacheItem<List<TP_Tour>>(TableName)) != null) return tours;
            lock (LockTPTours)
            {
                if ((tours = CacheHelper.GetCacheItem<List<TP_Tour>>(TableName)) != null) return tours;

                tours = (from t in dc.TP_Tours
                             select t)
                    .ToList<TP_Tour>();

                CacheHelper.AddCacheData(TableName, tours, null, Globals.Settings.Cache.MediumCacheTimeout);
            }
            return tours;
        }

        public static IList<TP_Tour> GetTPToursByKeys(this MtSearchDbDataContext dc, IEnumerable<int> toKeys, out string hash)
        {
            List<TP_Tour> result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, String.Join(",", toKeys));

            if ((result = CacheHelper.GetCacheItem<List<TP_Tour>>(hash)) != null) return result;

            result = dc.GetAllTPTours().Where(t => toKeys.Contains(t.TO_Key)).ToList();

            CacheHelper.AddCacheData(hash, result, new List<string>() { TableName }, Globals.Settings.Cache.MediumCacheTimeout);
            return result;
        }

        /// <summary>
        /// Возвращает название тура и его URL
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="tourKeys">Ключ тура</param>
        public static IList<Tuple<int, string, string>> GetTourStringsByKeys(this MtSearchDbDataContext dc, IEnumerable<int> tourKeys, out string hash)
        {
            List<Tuple<int, string, string>> result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, String.Join(",", tourKeys));
            if ((result = CacheHelper.GetCacheItem<List<Tuple<int, string, string>>>(hash)) != null) return result;

            string hashOut;
            var cacheDependencies = new List<string>();
            var tours = dc.GetTPToursByKeys(tourKeys, out hashOut);
            cacheDependencies.Add(hashOut);
            var tourList = dc.GetTurListsByKeys(tours.Select(t => t.TO_TRKey).Distinct().ToList(), out hashOut);
            cacheDependencies.Add(hashOut);

            result = (from tp in tours
                join tl in tourList on tp.TO_TRKey equals tl.TL_KEY
                select new Tuple<int, string, string>(tp.TO_Key, tl.TL_NAMEWEB, tl.TL_WEBHTTP))
                .Distinct()
                .ToList();

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }
    }
}
