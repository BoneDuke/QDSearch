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
    /// Расширение для работы с таблицей HotelRooms
    /// </summary>
    public static class HotelRoomsExtension
    {
        private static readonly object LockHotelRooms = new object();
        /// <summary>
        /// Название таблицы в БД
        /// </summary>
        public const string TableName = "HotelRooms";

        ///// <summary>
        ///// Возвращает список всех hotel room's
        ///// </summary>
        ///// <param name="dc">Контекст базы данных</param>
        ///// <returns></returns>
        //public static List<HotelRoom> GetAllHotelRooms(this MtSearchDbDataContext dc)
        //{
        //    List<HotelRoom> hotelRooms;
        //    if ((hotelRooms = CacheHelper.GetCacheItem<List<HotelRoom>>(TableName)) != null) return hotelRooms;
        //    lock (LockHotelRooms)
        //    {
        //        if ((hotelRooms = CacheHelper.GetCacheItem<List<HotelRoom>>(TableName)) != null) return hotelRooms;

        //        hotelRooms = (from hr in dc.HotelRooms
        //                  select hr)
        //            .ToList<HotelRoom>();

        //        CacheHelper.AddCacheData(TableName, hotelRooms, TableName);
        //    }
        //    return hotelRooms;
        //}

        /// <summary>
        /// Возвращает hotel room по ключам
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="hotelRoomKeys">Ключи hotel room</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static List<HotelRoom> GetHotelRoomByKeys(this MtSearchDbDataContext dc, IEnumerable<int> hotelRoomKeys, out string hash)
        {
            List<HotelRoom> result;

            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, String.Join("_", hotelRoomKeys));
            if ((result = CacheHelper.GetCacheItem<List<HotelRoom>>(hash)) != null) return result;

            result = (from h in dc.HotelRooms
                         where hotelRoomKeys.Contains(h.HR_KEY)
                         select h)
                         .ToList();

            if (!CacheHelper.IsCacheKeyExists(TableName))
                CacheHelper.AddCacheData(TableName, String.Empty, TableName);

            CacheHelper.AddCacheData(hash, result, new List<string>() { TableName }, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        ///// <summary>
        ///// Возвращает значения связанных таблиц
        ///// </summary>
        ///// <param name="dc"></param>
        ///// <param name="hotelRoom">Ключ hotel room</param>
        ///// <param name="hash">Хэш кэша</param>
        ///// <returns></returns>
        //public static HotelRoomNames GetHotelRoomNamesByKey(this MtSearchDbDataContext dc, HotelRoom hotelRoom, out string hash)
        //{
        //    HotelRoomNames result;

        //    hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, hotelRoom.HR_KEY);
        //    if ((result = CacheHelper.GetCacheItem<HotelRoomNames>(hash)) != null) return result;

        //    string hashOut;
        //    var cacheDependecies = new List<string>();

        //    Accmdmentype acmd = null;
        //    if (hotelRoom.HR_ACKEY != null)
        //    {
        //        acmd = dc.GetAccomodationByKey(hotelRoom.HR_ACKEY.Value, out hashOut);
        //        cacheDependecies.Add(hashOut);
        //    }
                
        //    var room = dc.GetRoomByKey(hotelRoom.HR_RMKEY, out hashOut);
        //    cacheDependecies.Add(hashOut);

        //    var roomCategory = dc.GetRoomCategoryByKey(hotelRoom.HR_RCKEY, out hashOut);
        //    cacheDependecies.Add(hashOut);

        //    result = new HotelRoomNames()
        //    {
        //        AccomodationKey = acmd != null ? acmd.AC_KEY : -1,
        //        AccomodationName = acmd != null ? acmd.AC_NAME : String.Empty,
        //        HotelRoomKey = hotelRoom.HR_KEY,
        //        RoomKey = room.RM_KEY,
        //        RoomName = room.RM_NAME,
        //        RoomCategoryKey = roomCategory.RC_KEY,
        //        RoomCategoryName = roomCategory.RC_NAME
        //    };

        //    CacheHelper.AddCacheData(hash, result, cacheDependecies.ToArray(), Globals.Settings.Cache.LongCacheTimeout);
        //    return result;
        //}
    }
}
