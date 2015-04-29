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
    /// Класс для работы с отелями
    /// </summary>
    public static class HotelsExtension
    {
        private static readonly object LockHotels = new object();
        /// <summary>
        /// Название таблицы
        /// </summary>
        public const string TableName = "HotelDictionary";

        /// <summary>
        /// Возвращает список всех отелей
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static List<HotelSmallClass> GetAllHotels(this MtSearchDbDataContext dc)
        {
            List<HotelSmallClass> hotels;
            if ((hotels = CacheHelper.GetCacheItem<List<HotelSmallClass>>(TableName)) != null) return hotels;
            lock (LockHotels)
            {
                if ((hotels = CacheHelper.GetCacheItem<List<HotelSmallClass>>(TableName)) != null) return hotels;

                hotels = (from h in dc.HotelDictionaries
                          select new HotelSmallClass()
                          {
                              Key = h.HD_KEY,
                              Name = h.HD_NAME,
                              RsKey = h.HD_RSKEY,
                              Http = h.HD_HTTP,
                              Stars = h.HD_STARS,
                              CtKey = h.HD_CTKEY,
                              CnKey = h.HD_CNKEY
                          })
                    .ToList();

                CacheHelper.AddCacheData(TableName, hotels, TableName);
            }
            return hotels;
        }

        /// <summary>
        /// Возвращает список отелей по стране
        /// </summary>
        /// <param name="dc">Контекст БД</param>
        /// <param name="countryKey">Ключ страны</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static IList<HotelSmallClass> GetHotelsByCountry(this MtSearchDbDataContext dc, int countryKey, out string hash)
        {
            List<HotelSmallClass> result;

            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, countryKey);
            if ((result = CacheHelper.GetCacheItem<List<HotelSmallClass>>(hash)) != null) return result;

            result = dc.GetAllHotels().Where(h => h.CnKey == countryKey).ToList();
            CacheHelper.AddCacheData(hash, result, new List<string>() { TableName }, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        ///// <summary>
        ///// Возвращает список отелей по стране
        ///// </summary>
        ///// <param name="dc">Контекст БД</param>
        ///// <param name="hotelKeys">Ключи отелей</param>
        ///// <returns></returns>
        //public static IList<HotelSmallClass> GetHotelsByKeys(this MtSearchDbDataContext dc, IEnumerable<int> hotelKeys)
        //{
        //    return dc.GetAllHotels().Where(h => hotelKeys.Contains(h.Key)).ToList();
        //}

        public static List<string> GetHotelStars(this MtSearchDbDataContext dc, IEnumerable<int> hotelKeys, out string hash)
        {
            List<string> result;

            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, String.Join(",", hotelKeys));
            if ((result = CacheHelper.GetCacheItem<List<string>>(hash)) != null) return result;

            string hashOut;
            var cacheDependencies = new List<string>();
            result = dc.GetHotelKeysStars(hotelKeys, out hashOut).Select(h => h.Item2).Distinct().OrderByDescending(s => s).ToList();
            cacheDependencies.Add(hashOut);

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Возвращает список соответствий ключей отелей и звездности
        /// </summary>
        /// <param name="dc">Контекст БД</param>
        /// <param name="hotelKeys">Ключи отелей</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static List<Tuple<int, string>> GetHotelKeysStars(this MtSearchDbDataContext dc, IEnumerable<int> hotelKeys, out string hash)
        {
            List<Tuple<int, string>> result;

            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, String.Join(",", hotelKeys));
            if ((result = CacheHelper.GetCacheItem<List<Tuple<int, string>>>(hash)) != null) return result;

            result = dc.GetAllHotels().Where(h => hotelKeys.Contains(h.Key) && !String.IsNullOrEmpty(h.Stars)).Select(h => new Tuple<int, string>(h.Key, h.Stars)).ToList();

            CacheHelper.AddCacheData(hash, result, new List<string>() { TableName }, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Возвращает отель по ключу
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="hotelKey">Ключ отеля</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static HotelSmallClass GetHotelByKey(this MtSearchDbDataContext dc, int hotelKey, out string hash)
        {
            HotelSmallClass result;

            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, hotelKey);
            if ((result = CacheHelper.GetCacheItem<HotelSmallClass>(hash)) != null) return result;

            result = (from h in dc.GetAllHotels()
                      where h.Key == hotelKey
                      select h)
                .SingleOrDefault();

            CacheHelper.AddCacheData(hash, result, new List<string>() { TableName }, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Возвращает отели по ключам
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="hotelKeys">Ключи отелей</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static IList<HotelSmallClass> GetHotelsByKeys(this MtSearchDbDataContext dc, IEnumerable<int> hotelKeys, out string hash)
        {
            List<HotelSmallClass> result;

            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, String.Join(",", hotelKeys));
            if ((result = CacheHelper.GetCacheItem<List<HotelSmallClass>>(hash)) != null) return result;

            result = (from h in dc.GetAllHotels()
                         where hotelKeys.Contains(h.Key) 
                select h)
                .ToList();

            CacheHelper.AddCacheData(hash, result, new List<string>() { TableName }, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }
    }
}
