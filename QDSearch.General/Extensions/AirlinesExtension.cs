using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using QDSearch.DataModel;
using QDSearch.Helpers;
using QDSearch.Repository.MtMain;
using QDSearch.Repository.MtSearch;

namespace QDSearch.Extensions
{
    /// <summary>
    /// Расширение для работы с таблицей авиакомпаний
    /// </summary>
    public static class AirlinesExtension
    {
        private static readonly object LockAirline = new object();
        /// <summary>
        /// Название таблицы в БД
        /// </summary>
        public const string TableName = "Airline";

        /// <summary>
        /// Возвращает список всех авиакомпаний по кодам перелетов
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="charterCodes"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static List<Airline> GetAllAirlinesByCharterKeys(this MtMainDbDataContext dc, List<string> charterCodes, out string hash)
        {
            List<Airline> result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, String.Join(",", charterCodes));
            if ((result = CacheHelper.GetCacheItem<List<Airline>>(hash)) != null) return result;

            result = dc.GetAllAirlines().Where(a => charterCodes.Contains(a.AL_CODE)).ToList();

            CacheHelper.AddCacheData(hash, result, new List<string>() { TableName }, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Возвращает список всех авиакомпаний
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static List<Airline> GetAllAirlines(this MtMainDbDataContext dc)
        {
            List<Airline> airlines;
            if ((airlines = CacheHelper.GetCacheItem<List<Airline>>(TableName)) != null) return airlines;

            lock (LockAirline)
            {
                if ((airlines = CacheHelper.GetCacheItem<List<Airline>>(TableName)) != null) return airlines;

                airlines = (from ar in dc.Airlines
                          select ar)
                    .ToList<Airline>();

                CacheHelper.AddCacheData(TableName, airlines, TableName);
            }
            return airlines;
        }
    }
}
