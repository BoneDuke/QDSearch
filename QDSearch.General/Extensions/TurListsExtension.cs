using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using QDSearch.Repository.MtMain;
using QDSearch.Helpers;
using QDSearch.Repository.MtSearch;

namespace QDSearch.Extensions
{
    /// <summary>
    /// Расширение для работы с турами
    /// </summary>
    public static class TurListsExtension
    {
        private static readonly object LockTurLists = new object();
        /// <summary>
        /// Название таблицы в БД
        /// </summary>
        public const string TableName = "tbl_TurList";

        public static tbl_TurList GetTurListByKey(this MtSearchDbDataContext dc, int tourKey, out string hash)
        {
            tbl_TurList tour;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, tourKey);

            if ((tour = CacheHelper.GetCacheItem<tbl_TurList>(hash)) != null) return tour;

            tour = dc.GetAllTurLists().SingleOrDefault(tp => tp.TL_KEY == tourKey);

            //CacheHelper.AddCacheData(hash, tour, new[] { TableName }, Globals.Settings.Cache.MediumCacheTimeout);
            CacheHelper.AddCacheData(hash, tour, new List<string>() { TableName }, Globals.Settings.Cache.MediumCacheTimeout);
            return tour;
        }

        public static IList<tbl_TurList> GetTurListsByKeys(this MtSearchDbDataContext dc, IEnumerable<int> tourKeys, out string hash)
        {
            List<tbl_TurList> result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, String.Join(",", tourKeys));

            if ((result = CacheHelper.GetCacheItem<List<tbl_TurList>>(hash)) != null) return result;

            result = dc.GetAllTurLists().Where(t => tourKeys.Contains(t.TL_KEY)).ToList();

            CacheHelper.AddCacheData(hash, result, new List<string>() { TableName }, Globals.Settings.Cache.MediumCacheTimeout);
            return result;
        }


        /// <summary>
        /// Возвращает список всех tbl_TurLists's
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static List<tbl_TurList> GetAllTurLists(this MtSearchDbDataContext dc)
        {
            List<tbl_TurList> tours;
            if ((tours = CacheHelper.GetCacheItem<List<tbl_TurList>>(TableName)) != default(List<tbl_TurList>)) return tours;
            lock (LockTurLists)
            {
                if ((tours = CacheHelper.GetCacheItem<List<tbl_TurList>>(TableName)) != default(List<tbl_TurList>)) return tours;

                tours = new List<tbl_TurList>();

                var commandBuilder = new StringBuilder();
                commandBuilder.AppendLine("select TL_Key, TL_WEBHTTP, TL_Description, TL_TIP, TL_NameWeb ");
                commandBuilder.AppendLine("from tbl_TurList ");
                commandBuilder.AppendLine("where TL_Key in (select TO_TRKey from tp_tours) ");
                commandBuilder.AppendLine(String.Format("or TL_Key in (select DS_PKKey from Descriptions where DS_DTKey = {0} and DS_Value like '1')", DescriptionsExtension.AllowCharterBooking));

                using (var command = dc.Connection.CreateCommand())
                {
                    command.CommandText = commandBuilder.ToString();

                    dc.Connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tours.Add(new tbl_TurList()
                            {
                                TL_KEY = reader.GetInt32("TL_Key"),
                                TL_WEBHTTP = reader.GetStringOrNull("TL_WEBHTTP"),
                                TL_DESCRIPTION = reader.GetStringOrNull("TL_Description"),
                                TL_TIP = reader.GetInt32("TL_Tip"),
                                TL_NAMEWEB = reader.GetStringOrNull("TL_NameWeb")
                            });
                        }
                    }
                    dc.Connection.Close();
                }

                CacheHelper.AddCacheData(TableName, tours, TableName);
                    
            }
            return tours;
        }


    }
}
