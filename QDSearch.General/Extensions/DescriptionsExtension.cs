using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Caching;
using QDSearch.Helpers;
using QDSearch.Repository.MtMain;

namespace QDSearch.Extensions
{
    /// <summary>
    /// Расширение для работы с таблицей Descriptions
    /// </summary>
    public static class DescriptionsExtension
    {
        private static readonly object LockDescription = new object();
        /// <summary>
        /// Название таблицы в БД
        /// </summary>
        public const string TableName = "Descriptions";
        /// <summary>
        /// Разрешены бронирования перелетов. Константа используется для туров в конструкторе туров
        /// </summary>
        public const int AllowCharterBooking = 115;
        /// <summary>
        /// Описание по стране. Используется в фильтре поиска
        /// </summary>
        public const int CountryDescription = 125;

        /// <summary>
        /// Возвращает список всех accomodation's
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static List<Description> GetAllDescriptions(this MtMainDbDataContext dc)
        {
            List<Description> descriptions;
            if ((descriptions = CacheHelper.GetCacheItem<List<Description>>(TableName)) != null) return descriptions;

            lock (LockDescription)
            {
                if ((descriptions = CacheHelper.GetCacheItem<List<Description>>(TableName)) != null) return descriptions;
                var descTypes = new[] { AllowCharterBooking, CountryDescription };

                descriptions = (from desc in dc.Descriptions
                                where descTypes.Contains(desc.DS_DTKey.Value)
                                select desc)
                    .ToList<Description>();

                CacheHelper.AddCacheData(TableName, descriptions, TableName);
            }
            return descriptions;
        }

        /// <summary>
        /// Возвращает список пакетов для бронирования авиаперелетов
        /// </summary>
        /// <param name="dc">Контекст БД</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static List<int> GetCharterBookingPackets(this MtMainDbDataContext dc, out string hash)
        {
            List<int> result;
            hash = String.Format("{0}", MethodBase.GetCurrentMethod().Name);
            if ((result = CacheHelper.GetCacheItem<List<int>>(hash)) != null) return result;

            result = (from d in dc.GetAllDescriptions()
                where d.DS_DTKey == AllowCharterBooking
                      && d.DS_Value.Contains("1")
                      && d.DS_PKKey.HasValue
                select d.DS_PKKey.Value)
                .ToList();

            CacheHelper.AddCacheData(hash, result, new List<string>() { TableName }, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Описание по стране
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="countryKey">Ключ страны</param>
        /// <returns></returns>
        public static string GetCountryDescription(this MtMainDbDataContext dc, int countryKey)
        {
            var hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, countryKey);
            if (CacheHelper.IsCacheKeyExists(hash))
                return CacheHelper.GetCacheItem<string>(hash);

            var result = (from d in dc.GetAllDescriptions()
                      where d.DS_DTKey == CountryDescription
                      && d.DS_PKKey == countryKey
                      select d.DS_Value)
                .FirstOrDefault();

            result = result ?? String.Empty;

            CacheHelper.AddCacheData(hash, result, new List<string>() { TableName }, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

    }
}
