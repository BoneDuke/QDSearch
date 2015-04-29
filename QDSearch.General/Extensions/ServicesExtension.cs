using System.Web.Caching;
using QDSearch.DataModel;
using QDSearch.Helpers;
using QDSearch.Repository.MtMain;
using QDSearch.Repository.MtSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QDSearch.Extensions
{
    /// <summary>
    /// Расширение по работе с класами услуг
    /// </summary>
    public static class ServicesExtension
    {
        private static readonly object LockServices = new object();
        /// <summary>
        /// Навание таблицы в БД
        /// </summary>
        public const string TableName = "Service";

        /// <summary>
        /// Получение списка всех классов услуг
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static List<Service> GetAllServices(this MtSearchDbDataContext dc)
        {
            List<Service> services;
            if ((services = CacheHelper.GetCacheItem<List<Service>>(TableName)) != default(List<Service>)) return services;
            lock (LockServices)
            {
                if ((services = CacheHelper.GetCacheItem<List<Service>>(TableName)) != default(List<Service>)) return services;

                services = (from s in dc.Services
                          select s)
                    .ToList<Service>();

                CacheHelper.AddCacheData(TableName, services, TableName);
            }
            return services;
        }

        /// <summary>
        /// Получение списка параметров для квоты "мало" по классам услуг
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static Dictionary<uint, QuotaSmallServiceParams> GetQuotaSmallServiceParams(this MtSearchDbDataContext dc)
        {
            Dictionary<uint, QuotaSmallServiceParams> serviceSmallParams;
            const string hash = "ServiceSmallParams";
            if ((serviceSmallParams = CacheHelper.GetCacheItem<Dictionary<uint, QuotaSmallServiceParams>>(hash)) != default(Dictionary<uint, QuotaSmallServiceParams>)) return serviceSmallParams;

            serviceSmallParams = (from s in dc.GetAllServices()
                where (s.SV_KEY == 1 || s.SV_KEY == 3)
                select s
                )
                .ToDictionary(s => (uint)s.SV_KEY, s => new QuotaSmallServiceParams
                {
                    AndParam = s.SV_LittleAnd.HasValue && s.SV_LittleAnd.Value,
                    PercentParam = (double?)s.SV_LittlePercent,
                    PlaceParam = (uint?)s.SV_LittlePlace    
                });

            CacheHelper.AddCacheData(hash, serviceSmallParams, new List<string>() { TableName }, Globals.Settings.Cache.LongCacheTimeout);

            return serviceSmallParams;
        }

        /// <summary>
        /// Является ли значение типом мало
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="serviceClass">Класс услуги</param>
        /// <param name="quotaExistCount">Число оставшихся квот</param>
        /// <param name="quotaAllCount">Число квот всего</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static bool IsSmallQuotaState(this MtSearchDbDataContext dc, ServiceClass serviceClass, uint quotaExistCount, uint quotaAllCount, out string hash)
        {
            hash = String.Format("{0}_{1}_{2}_{3}", MethodBase.GetCurrentMethod().Name, serviceClass, quotaExistCount, quotaAllCount);
            if (CacheHelper.IsCacheKeyExists(hash))
            {
                var result = CacheHelper.GetCacheItem<bool>(hash);
                return result;
            }

            QuotaSmallServiceParams pars = dc.GetQuotaSmallServiceParams()[(uint) serviceClass];
            if (pars == null)
                throw new ApplicationException("Ошибка метода: " + MethodBase.GetCurrentMethod().Name + "_" + serviceClass);

            bool res;
            bool? placeCondition = null;
            bool? percentCondition = null;
            if (pars.PlaceParam.HasValue)
                placeCondition = quotaExistCount <= pars.PlaceParam.Value;
            if (pars.PercentParam.HasValue)
                percentCondition = (quotaExistCount / (double)quotaAllCount)*100 <= pars.PercentParam.Value;

            if (pars.AndParam)
            {
                if (placeCondition.HasValue && placeCondition.Value == false ||
                    percentCondition.HasValue && percentCondition.Value == false)
                    res = false;
                else
                    res = true;
            }
            else
            {
                if (placeCondition.HasValue && placeCondition.Value ||
                    percentCondition.HasValue && percentCondition.Value)
                    res = true;
                else
                    res = false;
            }
            CacheHelper.AddCacheData(hash, res, new List<string>() { TableName }, Globals.Settings.Cache.LongCacheTimeout);

            return res;
        }

        /// <summary>
        /// Возвращает название класса услуги по значению enum
        /// </summary>
        /// <param name="dc">Контекст БД</param>
        /// <param name="serviceClass">Перечисление класса услуги</param>
        /// <returns></returns>
        public static string GetServiceClassName(this MtSearchDbDataContext dc, ServiceClass serviceClass)
        {
            var res = (from s in dc.GetAllServices()
                   where s.SV_KEY == (int)serviceClass
                select s.SV_NAME)
                .SingleOrDefault();

            return res;
        }
    }
}
