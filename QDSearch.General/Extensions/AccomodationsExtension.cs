using QDSearch.DataModel;
using QDSearch.Helpers;
using QDSearch.Repository.MtSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace QDSearch.Extensions
{
    /// <summary>
    /// Расширение для работы с таблицей AccmdMenType
    /// </summary>
    public static class AccomodationsExtension
    {
        private static readonly object LockAccomodation = new object();
        /// <summary>
        /// Название таблицы в БД
        /// </summary>
        public const string TableName = "Accmdmentype";

        ///// <summary>
        ///// Возвращает список всех accomodation's
        ///// </summary>
        ///// <param name="dc">Контекст базы данных</param>
        ///// <returns></returns>
        //public static List<Accmdmentype> GetAllAccomodations(this MtSearchDbDataContext dc)
        //{
        //    List<Accmdmentype> accmds;
        //    if ((accmds = CacheHelper.GetCacheItem<List<Accmdmentype>>(TableName)) != null) return accmds;

        //    lock (LockAccomodation)
        //    {
        //        if ((accmds = CacheHelper.GetCacheItem<List<Accmdmentype>>(TableName)) != null) return accmds;

        //        accmds = (from ac in dc.Accmdmentypes
        //                      select ac)
        //            .ToList<Accmdmentype>();

        //        CacheHelper.AddCacheData(TableName, accmds, TableName);
        //    }
        //    return accmds;
        //}

        /// <summary>
        /// Возвращает список accomodation по ключам
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="accmdKeys">Ключи записи</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static IList<Accmdmentype> GetAccomodationsByKeys(this MtSearchDbDataContext dc, IEnumerable<int> accmdKeys, out string hash)
        {
            List<Accmdmentype> result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, String.Join(",", accmdKeys));
            if ((result = CacheHelper.GetCacheItem<List<Accmdmentype>>(hash)) != null) return result;

            result = dc.Accmdmentypes.Where(a => accmdKeys.Contains(a.AC_KEY)).ToList();

            if (!CacheHelper.IsCacheKeyExists(TableName))
                CacheHelper.AddCacheData(TableName, String.Empty, TableName);

            CacheHelper.AddCacheData(hash, result, new List<string>() { TableName }, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }
    }
}
