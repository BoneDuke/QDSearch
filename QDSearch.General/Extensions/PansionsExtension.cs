using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
    /// Класс по работе с городами
    /// </summary>
    public static class PansionsExtension
    {
        private static readonly object LockPansions = new object();
        /// <summary>
        /// Название таблицы
        /// </summary>
        public const string TableName = "Pansion";

        /// <summary>
        /// Возвращает список питаний
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static List<Pansion> GetAllPansions(this MtSearchDbDataContext dc)
        {
            List<Pansion> pansions;
            if ((pansions = CacheHelper.GetCacheItem<List<Pansion>>(TableName)) != null) return pansions;
            lock (LockPansions)
            {
                if ((pansions = CacheHelper.GetCacheItem<List<Pansion>>(TableName)) != null) return pansions;

                pansions = (from p in dc.Pansions
                          select p)
                    .ToList<Pansion>();

                CacheHelper.AddCacheData(TableName, pansions, TableName);
            }
            return pansions;
        }

        /// <summary>
        /// Возвращает питание по ключу
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="pansionKey">Ключ питания</param>
        /// <returns></returns>
        public static Pansion GetPansionByKey(this MtSearchDbDataContext dc, int pansionKey)
        {
            Pansion result;

            var hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, pansionKey);
            if ((result = CacheHelper.GetCacheItem<Pansion>(hash)) != null) return result;

            var pansion = (from p in dc.GetAllPansions()
                        where p.PN_KEY == pansionKey
                        select p)
                .FirstOrDefault();

            CacheHelper.AddCacheData(hash, pansion, new List<string>() { TableName }, Globals.Settings.Cache.LongCacheTimeout);
            return pansion;
        }

        /// <summary>
        /// Возвращает питание по ключу
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="pansionKeys">Ключ питания</param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static IList<Pansion> GetPansionsByKeys(this MtSearchDbDataContext dc, IEnumerable<int> pansionKeys, out string hash)
        {
            List<Pansion> result;

            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, String.Join(",", pansionKeys));
            if ((result = CacheHelper.GetCacheItem<List<Pansion>>(hash)) != null) return result;

            var pansion = (from p in dc.GetAllPansions()
                           where pansionKeys.Contains(p.PN_KEY)
                           select p)
                .ToList();

            CacheHelper.AddCacheData(hash, pansion, new List<string>() { TableName }, Globals.Settings.Cache.LongCacheTimeout);
            return pansion;
        }
    }
}
