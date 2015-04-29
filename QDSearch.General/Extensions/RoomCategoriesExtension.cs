using System;
using System.Reflection;
using QDSearch.Helpers;
using QDSearch.Repository.MtSearch;
using System.Collections.Generic;
using System.Linq;

namespace QDSearch.Extensions
{
    /// <summary>
    /// Расширение по работе с таблицей RoomsCategory
    /// </summary>
    public static class RoomCategoriesExtension
    {
        private static readonly object LockRoomCategories = new object();
        /// <summary>
        /// Название таблицы
        /// </summary>
        public const string TableName = "RoomsCategory";

        ///// <summary>
        ///// Возвращает список всех категорий комнат
        ///// </summary>
        ///// <param name="dc">Контекст базы данных</param>
        ///// <returns></returns>
        //public static List<RoomsCategory> GetAllRoomCategories(this MtSearchDbDataContext dc)
        //{
        //    List<RoomsCategory> roomCategories;
        //    if ((roomCategories = CacheHelper.GetCacheItem<List<RoomsCategory>>(TableName)) != default(List<RoomsCategory>)) return roomCategories;
        //    lock (LockRoomCategories)
        //    {
        //        if ((roomCategories = CacheHelper.GetCacheItem<List<RoomsCategory>>(TableName)) != default(List<RoomsCategory>)) return roomCategories;

        //        roomCategories = (from rc in dc.RoomsCategories
        //                 select rc)
        //            .ToList<RoomsCategory>();

        //        CacheHelper.AddCacheData(TableName, roomCategories, TableName);
        //    }
        //    return roomCategories;
        //}

        /// <summary>
        /// Возвращает список RoomCategory по ключу
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="rcKeys">Ключ записи</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static IList<RoomsCategory> GetRoomCategoriesByKeys(this MtSearchDbDataContext dc, IEnumerable<int> rcKeys, out string hash)
        {
            List<RoomsCategory> result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, String.Join(",", rcKeys));
            if ((result = CacheHelper.GetCacheItem<List<RoomsCategory>>(hash)) != default(List<RoomsCategory>)) return result;

            result = dc.RoomsCategories.Where(r => rcKeys.Contains(r.RC_KEY)).ToList();

            if (!CacheHelper.IsCacheKeyExists(TableName))
                CacheHelper.AddCacheData(TableName, String.Empty, TableName);

            CacheHelper.AddCacheData(hash, result, new List<string>() { TableName }, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }
    }
}
