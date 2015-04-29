using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using QDSearch.DataModel;
using QDSearch.Helpers;
using QDSearch.Repository.MtMain;

namespace QDSearch.Extensions
{
    /// <summary>
    /// Расширение по работе с услугами в конструкторе туров
    /// </summary>
    public static class TurServicesExtension
    {
        private static readonly object LockTurServices = new object();
        /// <summary>
        /// Название таблицы в БД
        /// </summary>
        public const string TableName = "TurService";

        /// <summary>
        /// Возращает все услугипо заданному классу из списка туров
        /// </summary>
        /// <param name="dc">Контекст БД</param>
        /// <param name="serviceClass">Класс услуги</param>
        /// <param name="turListKeys">Список ключей туров</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static List<TurService> GetClassServicesByTurListKeys(this MtMainDbDataContext dc, ServiceClass serviceClass, IEnumerable<int> turListKeys, out string hash)
        {
            List<TurService> result;
            hash = String.Format("{0}_{1}_{2}", MethodBase.GetCurrentMethod().Name, serviceClass, String.Join("_", turListKeys));
            if ((result = CacheHelper.GetCacheItem<List<TurService>>(hash)) != null) 
                return result;

            result = (from serv in dc.TurServices
                      where turListKeys.Contains(serv.TS_TRKEY) 
                      && serv.TS_SVKEY == (int) serviceClass
                      select serv)
                .ToList();

            CacheHelper.AddCacheData(hash, result, TableName);
            return result;
        }

        ///// <summary>
        ///// Возвращает список всех услуг по конкретному туру
        ///// </summary>
        ///// <param name="dc">Контекст БД</param>
        ///// <param name="turListKey">Ключ тура</param>
        ///// <param name="hash">Хэш кэша</param>
        ///// <returns></returns>
        //public static List<TurService> GetServicesByTurListKey(this MtMainDbDataContext dc, int turListKey, out string hash)
        //{
        //    List<TurService> result;
        //    hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, turListKey);
        //    if ((result = CacheHelper.GetCacheItem<List<TurService>>(hash)) != null) return result;

        //    result = (from serv in dc.TurServices
        //        where serv.TS_TRKEY == turListKey
        //        select serv)
        //        .ToList();

        //    CacheHelper.AddCacheData(hash, result, TableName);
        //    return result;
        //}
    }
}
