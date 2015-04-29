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
    /// Класс для работы с таблицей QuotaParts
    /// </summary>
    public static class QuotaPartsExtension
    {
        private static readonly object LockQuotaParts = new object();
        /// <summary>
        /// 
        /// </summary>
        public const string TableName = "QuotaParts";

        /// <summary>
        /// Возвращает список всех QuotaParts's
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static List<QuotaPart> GetAllQuotaParts(this MtSearchDbDataContext dc)
        {
            List<QuotaPart> quotaParts;
            if ((quotaParts = CacheHelper.GetCacheItem<List<QuotaPart>>(TableName)) != null) return quotaParts;
            lock (LockQuotaParts)
            {
                if ((quotaParts = CacheHelper.GetCacheItem<List<QuotaPart>>(TableName)) != null) return quotaParts;

                quotaParts = (from qp in dc.QuotaParts
                              where qp.QP_Date >= DateTime.Now.Date
                                select qp)
                    .ToList<QuotaPart>();

                CacheHelper.AddCacheData(TableName, quotaParts, null, Globals.Settings.Cache.MediumCacheTimeout);
            }
            return quotaParts;
        }
    }
}
