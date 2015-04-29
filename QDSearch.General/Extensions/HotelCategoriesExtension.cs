using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;
using QDSearch.Helpers;
using QDSearch.Repository.MtMain;
using QDSearch.Repository.MtSearch;
using HotelDictionary = QDSearch.Repository.MtSearch.HotelDictionary;

namespace QDSearch.Extensions
{
    /// <summary>
    /// Класс для работы с отелями
    /// </summary>
    public static class HotelCategoriesExtension
    {
        private static readonly object LockHotelCats = new object();
        /// <summary>
        /// Название таблицы
        /// </summary>
        public const string TableName = "CategoriesOfHotel";

        /// <summary>
        /// Возвращает список всех категорий отелей
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static List<CategoriesOfHotel> GetAllHotelCats(this MtMainDbDataContext dc)
        {
            List<CategoriesOfHotel> hotelCats;
            if ((hotelCats = CacheHelper.GetCacheItem<List<CategoriesOfHotel>>(TableName)) != null) return hotelCats;
            lock (LockHotelCats)
            {
                if ((hotelCats = CacheHelper.GetCacheItem<List<CategoriesOfHotel>>(TableName)) != null) return hotelCats;

                hotelCats = (from c in dc.CategoriesOfHotels
                          select c)
                    .ToList<CategoriesOfHotel>();

                CacheHelper.AddCacheData(TableName, hotelCats, TableName);
            }
            return hotelCats;
        }

        /// <summary>
        /// Возвращает категорию отеля по ключу
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="hotelCatKey"></param>
        /// <returns></returns>
        public static CategoriesOfHotel GetHotelCatByKey(this MtMainDbDataContext dc, int hotelCatKey)
        {
            CategoriesOfHotel result;

            var hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, hotelCatKey);
            if ((result = CacheHelper.GetCacheItem<CategoriesOfHotel>(hash)) != null) return result;

            result = (from c in dc.GetAllHotelCats()
                         where c.COH_Id == hotelCatKey
                         select c)
                .FirstOrDefault();

            CacheHelper.AddCacheData(hash, result, new List<string>() { TableName }, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }
    }
}
