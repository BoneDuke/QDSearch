using QDSearch.Helpers;
using QDSearch.Repository.MtSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace QDSearch.Extensions
{
    /// <summary>
    /// Расширение по работе с таблицей QuotaDetails
    /// </summary>
    public static class QuotaDetailsExtension
    {
        private static readonly object LockQuotaDetails = new object();
        /// <summary>
        /// Название таблтцы в БД
        /// </summary>
        public const string TableName = "QuotaDetails";

        /// <summary>
        /// Возвращает список всех QuotaDetails
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static List<QuotaDetail> GetAllQuotaDetails(this MtSearchDbDataContext dc)
        {
            List<QuotaDetail> quotaDetails;
            if ((quotaDetails = CacheHelper.GetCacheItem<List<QuotaDetail>>(TableName)) != default(List<QuotaDetail>)) return quotaDetails;
            lock (LockQuotaDetails)
            {
                if ((quotaDetails = CacheHelper.GetCacheItem<List<QuotaDetail>>(TableName)) != default(List<QuotaDetail>)) return quotaDetails;

                quotaDetails = (from q in dc.QuotaDetails
                                where q.QD_Date >= DateTime.Now.Date
                    select q)
                    .ToList();

                CacheHelper.AddCacheData(TableName, quotaDetails, null, Globals.Settings.Cache.MediumCacheTimeout);
            }
            return quotaDetails;
        }
    }
}
