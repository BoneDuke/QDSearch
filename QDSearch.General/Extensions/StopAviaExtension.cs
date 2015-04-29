using System.Web.Caching;
using QDSearch.DataModel;
using QDSearch.Helpers;
using QDSearch.Repository.MtSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QDSearch.Extensions
{
    /// <summary>
    /// Расширение по работе с запретами перелетов
    /// </summary>
    public static class StopAviaExtension
    {
        private static readonly object LockStopAvia = new object();
        /// <summary>
        /// Название таблицы в БД
        /// </summary>
        public const string TableName = "StopAvia";

        /// <summary>
        /// Получение списка всех стопов на перелеты
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static List<StopAvia> GetAllStopAvia(this MtSearchDbDataContext dc)
        {
            List<StopAvia> stopAvia;
            if ((stopAvia = CacheHelper.GetCacheItem<List<StopAvia>>(TableName)) != default(List<StopAvia>)) return stopAvia;
            lock (LockStopAvia)
            {
                if ((stopAvia = CacheHelper.GetCacheItem<List<StopAvia>>(TableName)) != default(List<StopAvia>)) return stopAvia;

                stopAvia = (from s in dc.StopAvias
                            where s.SA_DEND >= DateTime.Now.Date
                            select s)
                    .ToList<StopAvia>();

                CacheHelper.AddCacheData(TableName, stopAvia, TableName);
            }
            return stopAvia;
        }

        /// <summary>
        /// Существует ли стоп по направлению
        /// </summary>
        /// <param name="dc">Контекст БД</param>
        /// <param name="cityKeyFrom">КЛюч города вылета</param>
        /// <param name="cityKeyTo">Ключ города прилета</param>
        /// <param name="tourStartDate">Дата начала тура</param>
        /// <param name="tourEndDate">Дата окончания тура</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static bool IsStopByDrection(this MtSearchDbDataContext dc, int cityKeyFrom, int cityKeyTo, DateTime tourStartDate, DateTime tourEndDate, out string hash)
        {
            var result = false;

            var stopAvia = dc.GetAllStopAvia();
            hash = TableName;

            DateTime linkedDate1, linkedDate2;
            int cityToStop, cityFromStop;
            if (tourStartDate <= tourEndDate)
            {
                linkedDate1 = tourStartDate;
                linkedDate2 = tourEndDate;
                cityToStop = cityKeyTo;
                cityFromStop = cityKeyFrom;
            }
            else
            {
                linkedDate1 = tourEndDate;
                linkedDate2 = tourStartDate;
                cityToStop = cityKeyFrom;
                cityFromStop = cityKeyTo;
            }

            // так как для перелетов параметры serviceDateFrom == serviceDateTo
            if (stopAvia.Any(
                    sa =>
                        sa.SA_DBEG == linkedDate1
                        && sa.SA_DEND == linkedDate2
                        && sa.SA_CTKEYFROM == cityFromStop &&
                        sa.SA_CTKEYTO == cityToStop
                        && sa.SA_STOP == 1))
            {
                result = true;
            }

            return result;
        }
    }
}
