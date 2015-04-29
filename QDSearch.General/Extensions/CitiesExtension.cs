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
    public static class CitiesExtension
    {
        private static readonly object LockCities = new object();
        /// <summary>
        /// Название таблицы в БД
        /// </summary>
        public const string TableName = "CityDictionary";

        /// <summary>
        /// Возвращает список всех отелей
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static List<CityDictionary> GetAllCities(this MtSearchDbDataContext dc)
        {
            List<CityDictionary> cities;
            if ((cities = CacheHelper.GetCacheItem<List<CityDictionary>>(TableName)) != null) return cities;
            lock (LockCities)
            {
                if ((cities = CacheHelper.GetCacheItem<List<CityDictionary>>(TableName)) != null) return cities;

                cities = (from c in dc.CityDictionaries
                           select c)
                    .ToList<CityDictionary>();
                cities.Add(new CityDictionary()
                {
                    CT_KEY = 0,
                    CT_NAME = "-Без перелета-"
                });

                CacheHelper.AddCacheData(TableName, cities, TableName);
            }
            return cities;
        }

        /// <summary>
        /// Возвращает города по ключу
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="cityKeys">Ключи городов</param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static IList<CityDictionary> GetCitiesByKeys(this MtSearchDbDataContext dc, IEnumerable<int> cityKeys, out string hash)
        {
            List<CityDictionary> result;

            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, String.Join(",", cityKeys));
            if ((result = CacheHelper.GetCacheItem<List<CityDictionary>>(hash)) != null) return result;

            result = (from c in dc.GetAllCities()
                      where cityKeys.Contains(c.CT_KEY)
                      select c)
                .ToList();

            CacheHelper.AddCacheData(hash, result, new List<string>() { TableName }, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Возвращает город по ключу
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="cityKey">Ключ города</param>
        /// <returns></returns>
        public static CityDictionary GetCityByKey(this MtSearchDbDataContext dc, int cityKey)
        {
            CityDictionary result;

            var hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, cityKey);
            if ((result = CacheHelper.GetCacheItem<CityDictionary>(hash)) != null) return result;

            result = (from c in dc.GetAllCities()
                        where c.CT_KEY == cityKey
                          select c)
                .FirstOrDefault();

            CacheHelper.AddCacheData(hash, result, new List<string>() { TableName }, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }
    }
}
