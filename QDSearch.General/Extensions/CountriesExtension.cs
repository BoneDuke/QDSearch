using System.Reflection;
using System.Web.Caching;
using QDSearch.Helpers;
using QDSearch.Repository.MtSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.Extensions
{
    /// <summary>
    /// Класс по работе с городами
    /// </summary>
    public static class CountriesExtension
    {
        private static readonly object LockCountries = new object();
        /// <summary>
        /// Название таблицы в БД
        /// </summary>
        public const string TableName = "tbl_Country";

        /// <summary>
        /// Возвращает список всех отелей
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static List<tbl_Country> GetAllCountries(this MtSearchDbDataContext dc)
        {
            List<tbl_Country> countries;

            if ((countries = CacheHelper.GetCacheItem<List<tbl_Country>>(TableName)) != default(List<tbl_Country>)) return countries;

            lock (LockCountries)
            {
                if ((countries = CacheHelper.GetCacheItem<List<tbl_Country>>(TableName)) != default(List<tbl_Country>)) return countries;

                countries = (from c in dc.tbl_Countries
                          select c)
                    .ToList<tbl_Country>();

                CacheHelper.AddCacheData(TableName, countries, TableName);
            }
            return countries;
        }

        public static IList<tbl_Country> GetCountriesByKeys(this MtSearchDbDataContext dc, IEnumerable<int> countryKeys, out string hash)
        {
            List<tbl_Country> countries;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, String.Join(",", countryKeys));

            if ((countries = CacheHelper.GetCacheItem<List<tbl_Country>>(hash)) != default(List<tbl_Country>)) return countries;

            countries = (from c in dc.GetAllCountries()
                         where countryKeys.Contains(c.CN_KEY)
                            select c)
                .ToList<tbl_Country>();

            CacheHelper.AddCacheData(hash, countries, new List<string>() { TableName }, Globals.Settings.Cache.LongCacheTimeout);

            return countries;
        }

        /// <summary>
        /// Возвращает страну, в которой находится основной город туроператора
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static tbl_Country GetHomeCountry(this MtSearchDbDataContext dc, out string hash)
        {
            tbl_Country result;
            hash = String.Format("{0}", MethodBase.GetCurrentMethod().Name);
            if ((result = CacheHelper.GetCacheItem<tbl_Country>(hash)) != null) return result;

            var cacheDependencies = new List<string>();
            result = (from c in dc.GetAllCities()
                                  where c.CT_KEY == Globals.Settings.HomeCityKey
                                  select c.tbl_Country)
                                .Single();

            cacheDependencies.Add(TableName);
            cacheDependencies.Add(CitiesExtension.TableName);

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }
    }
}
