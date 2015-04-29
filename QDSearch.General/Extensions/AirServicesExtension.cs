using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;
using QDSearch.Helpers;
using QDSearch.Repository.MtSearch;

namespace QDSearch.Extensions
{
    /// <summary>
    /// Расширение для работы с классами перелетов
    /// </summary>
    public static class AirServicesExtension
    {
        private static readonly object LockAirServices = new object();
        /// <summary>
        /// Название таблицы
        /// </summary>
        public const string TableName = "AirService";

        /// <summary>
        /// Возвращает список всех авиакомпаний по кодам перелетов
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="keys">Ключи записей</param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static List<AirService> GetAirServicesByKeys(this MtSearchDbDataContext dc, List<int> keys, out string hash)
        {
            List<AirService> result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, String.Join(",", keys));
            if ((result = CacheHelper.GetCacheItem<List<AirService>>(hash)) != null) return result;

            result = dc.GetAllAirServices().Where(a => keys.Contains(a.AS_KEY)).ToList();

            CacheHelper.AddCacheData(hash, result, new List<string>(){TableName}, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Возвращает список классов перелетов
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static List<AirService> GetAllAirServices(this MtSearchDbDataContext dc)
        {
            List<AirService> airServices;
            if ((airServices = CacheHelper.GetCacheItem<List<AirService>>(TableName)) != null) return airServices;

            lock (LockAirServices)
            {
                if ((airServices = CacheHelper.GetCacheItem<List<AirService>>(TableName)) != null) return airServices;

                airServices = (from a in dc.AirServices
                              select a)
                    .ToList<AirService>();

                CacheHelper.AddCacheData(TableName, airServices, TableName);
            }

            return airServices;
        }

        /// <summary>
        /// Возвращает класс перелета по его ключу
        /// </summary>
        /// <param name="dc">Контекст БД</param>
        /// <param name="key">Ключ класса перелета</param>
        /// <returns></returns>
        public static AirService GetAirServiceByKey(this MtSearchDbDataContext dc, int key)
        {
            AirService result;
            var hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, key);
            if ((result = CacheHelper.GetCacheItem<AirService>(hash)) != null) return result;

            result = dc.GetAllAirServices().Single(a => a.AS_KEY == key);

            CacheHelper.AddCacheData(hash, result, new List<string>() { TableName }, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }
    }
}
