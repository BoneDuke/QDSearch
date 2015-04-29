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
    /// Расширение для работы со стопами
    /// </summary>
    public static class StopSalesExtension
    {
        private static readonly object LockStopSales = new object();
        /// <summary>
        /// Название таблицы
        /// </summary>
        public const string TableName = "StopSales";

        /// <summary>
        /// Возвращает список всех StopSales's
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static List<StopSale> GetAllStopSales(this MtSearchDbDataContext dc)
        {
            List<StopSale> stopSales;
            if ((stopSales = CacheHelper.GetCacheItem<List<StopSale>>(TableName)) != null) return stopSales;
            lock (LockStopSales)
            {
                if ((stopSales = CacheHelper.GetCacheItem<List<StopSale>>(TableName)) != null) return stopSales;

                stopSales = (from ss in dc.StopSales
                             where ss.SS_Date >= DateTime.Now.Date
                          select ss)
                    .ToList<StopSale>();

                CacheHelper.AddCacheData(TableName, stopSales, TableName);
            }
            return stopSales;
        }

        /// <summary>
        /// Выбираем все стопы по объектам
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static Dictionary<int, List<StopSale>> GetAllStopSalesByObjects(this MtSearchDbDataContext dc)
        {
            Dictionary<int, List<StopSale>> stopSales;
            var hash = String.Format("{0}", MethodBase.GetCurrentMethod().Name);
            if ((stopSales = CacheHelper.GetCacheItem<Dictionary<int, List<StopSale>>>(hash)) != null) return stopSales;

            stopSales = (from ss in dc.GetAllStopSales()
                         where ss.SS_QOID.HasValue
                            group ss by ss.SS_QOID.Value
                                into g
                                select new { g.Key, Items = g.ToList() })
                    .ToDictionary(g => g.Key, g => g.Items);

            CacheHelper.AddCacheData(hash, stopSales, new List<string>() { TableName }, Globals.Settings.Cache.LongCacheTimeout);
            return stopSales;
        }
    }
}
