using System;
using System.Linq;
using System.Reflection;
using QDSearch.Helpers;
using QDSearch.Repository.MtSearch;
using System.Collections.Generic;
using System.Web.Caching;

namespace QDSearch.Extensions
{
    /// <summary>
    /// Расширение для работы с валютами
    /// </summary>
    public static class RatesExtension
    {
        
        private static readonly object LockRates = new object();
        /// <summary>
        /// Название таблицы в БД
        /// </summary>
        public const string TableName = "Rates";

        /// <summary>
        /// Возвращает ключ валюты по ее коду
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="rateCode">Код валюты</param>
        /// <returns></returns>
        public static int GetRateKeyByCode(this MtSearchDbDataContext dc, string rateCode)
        {
            int result;

            var hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, rateCode);
            if (CacheHelper.IsCacheKeyExists(hash))
            {
                result = CacheHelper.GetCacheItem<int>(hash);
                return result;
            }

            var rates = dc.GetAllRatesList();

            result = (from r in rates
                           where r.RA_CODE == rateCode
                           select r.ra_key)
                .SingleOrDefault();

            CacheHelper.AddCacheData(hash, result, new List<string>() { TableName }, Globals.Settings.Cache.LongCacheTimeout);

            return result;
        }
        /// <summary>
        /// Возвращает ключ и код национальной валюты 
        /// </summary>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static Tuple<int, string> GetNationalRateInfo(this MtSearchDbDataContext dc)
        {
            Tuple<int, string> result;

            var hash = String.Format("{0}", MethodBase.GetCurrentMethod().Name);
            if ((result = CacheHelper.GetCacheItem<Tuple<int, string>>(hash)) != null) return result;

            var rates = dc.GetAllRatesList();

            result = (from r in rates
                           where r.RA_National == 1
                           select new Tuple<int, string>(r.ra_key, r.RA_CODE))
                .SingleOrDefault();

            CacheHelper.AddCacheData(hash, result, new List<string>() { TableName }, Globals.Settings.Cache.LongCacheTimeout);

            return result;
        }

        /// <summary>
        /// Возвращает список всех валют
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static List<Rate> GetAllRatesList(this MtSearchDbDataContext dc)
        {
            List<Rate> rates;
            if ((rates = CacheHelper.GetCacheItem<List<Rate>>(TableName)) != null) return rates;
            lock (LockRates)
            {
                if ((rates = CacheHelper.GetCacheItem<List<Rate>>(TableName)) != null) return rates;

                rates = (from r in dc.Rates
                         select r)
                    .ToList<Rate>();

                CacheHelper.AddCacheData(TableName, rates, TableName);
            }
            return rates;
        }

        /// <summary>
        /// Возвращает код валюты по ее ключу
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="rateKey">Ключ валюты</param>
        /// <returns></returns>
        public static string GetRateCodeByKey(this MtSearchDbDataContext dc, int rateKey)
        {
            var hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, rateKey);
            if (CacheHelper.IsCacheKeyExists(hash))
                return CacheHelper.GetCacheItem<string>(hash);

            var result = (from r in dc.GetAllRatesList()
                where r.ra_key == rateKey
                select r.RA_CODE)
                .SingleOrDefault();

            CacheHelper.AddCacheData(hash, result, new List<string>() { TableName }, Globals.Settings.Cache.LongCacheTimeout);

            return result;
        }
    }
}
