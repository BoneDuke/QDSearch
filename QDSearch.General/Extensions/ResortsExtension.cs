using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QDSearch.Repository.MtSearch;
using QDSearch.Helpers;
using System.Reflection;
using System.Web.Caching;

namespace QDSearch.Extensions
{
    /// <summary>
    /// Класс по работе с курортами
    /// </summary>
    public static class ResortsExtension
    {
        private static readonly object LockResorts = new object();
        /// <summary>
        /// Название таблицы в БД
        /// </summary>
        public const string TableName = "Resorts";

        /// <summary>
        /// Возвращает список всех отелей
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static List<Resort> GetAllResorts(this MtSearchDbDataContext dc)
        {
            List<Resort> resorts;
            if ((resorts = CacheHelper.GetCacheItem<List<Resort>>(TableName)) != default(List<Resort>)) return resorts;
            lock (LockResorts)
            {
                if ((resorts = CacheHelper.GetCacheItem<List<Resort>>(TableName)) != default(List<Resort>)) return resorts;

                resorts = (from r in dc.Resorts
                          select r)
                    .ToList<Resort>();

                CacheHelper.AddCacheData(TableName, resorts, TableName);
            }
            return resorts;
        }

        /// <summary>
        /// Возвращает курорт по ключу
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="resortKeys">Ключ курорта</param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static IList<Resort> GetResortsByKeys(this MtSearchDbDataContext dc, IEnumerable<int> resortKeys, out string hash)
        {
            List<Resort> result;

            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, String.Join(",", resortKeys));
            if ((result = CacheHelper.GetCacheItem<List<Resort>>(hash)) != null) return result;

            result = (from r in dc.GetAllResorts()
                      where resortKeys.Contains(r.RS_KEY)
                         select r)
                .ToList();

            CacheHelper.AddCacheData(hash, result, new List<string>() { TableName }, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }
    }
}
