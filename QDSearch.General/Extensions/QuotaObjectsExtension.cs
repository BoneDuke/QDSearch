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
    /// Расширение по работе с таблицей QuotaObjects
    /// </summary>
    public static class QuotaObjectsExtension
    {
        private static readonly object LockQuotaObjects = new object();
        /// <summary>
        /// Название таблицы в БД
        /// </summary>
        public const string TableName = "QuotaObjects";

        /// <summary>
        /// Возвращает список всех QuotaObject's
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static List<QuotaObject> GetAllQuotaObjects(this MtSearchDbDataContext dc)
        {
            List<QuotaObject> quotaObjects;
            if ((quotaObjects = CacheHelper.GetCacheItem<List<QuotaObject>>(TableName)) != null) return quotaObjects;
            lock (LockQuotaObjects)
            {
                if ((quotaObjects = CacheHelper.GetCacheItem<List<QuotaObject>>(TableName)) != null) return quotaObjects;

                quotaObjects = (from qo in dc.QuotaObjects
                    select qo)
                    .ToList<QuotaObject>();

                CacheHelper.AddCacheData(TableName, quotaObjects, null, Globals.Settings.Cache.MediumCacheTimeout);
            }
            return quotaObjects;
        }
    }
}
