using System;
using System.Collections.Generic;
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
    /// Расширение по работе с таблицей Rooms
    /// </summary>
    public static class RoomsExtension
    {
        private static readonly object LockRooms = new object();
        /// <summary>
        /// Название таблицы в БД
        /// </summary>
        public const string TableName = "Rooms";

        /// <summary>
        /// Возвращает список всех room's
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static List<Room> GetAllRooms(this MtSearchDbDataContext dc)
        {
            List<Room> rooms;
            if ((rooms = CacheHelper.GetCacheItem<List<Room>>(TableName)) != default(List<Room>)) return rooms;
            lock (LockRooms)
            {
                if ((rooms = CacheHelper.GetCacheItem<List<Room>>(TableName)) != default(List<Room>)) return rooms;

                rooms = (from rm in dc.Rooms
                         select rm)
                    .ToList<Room>();

                CacheHelper.AddCacheData(TableName, rooms, TableName);
            }
            return rooms;
        }

        /// <summary>
        /// Возвращает список room'ов по ключам
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="roomKeys">Ключи записей</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static IList<Room> GetRoomsByKeys(this MtSearchDbDataContext dc, IEnumerable<int> roomKeys, out string hash)
        {
            List<Room> result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, String.Join(",", roomKeys));
            if ((result = CacheHelper.GetCacheItem<List<Room>>(hash)) != default(List<Room>)) return result;

            result = dc.GetAllRooms().Where(r => roomKeys.Contains(r.RM_KEY)).ToList();

            CacheHelper.AddCacheData(hash, result, new List<string>() { TableName }, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }
    }
}
