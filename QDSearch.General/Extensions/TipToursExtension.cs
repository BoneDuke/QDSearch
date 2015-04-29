using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;
using QDSearch.Helpers;
using QDSearch.Repository.MtMain;
using QDSearch.Repository.MtSearch;

namespace QDSearch.Extensions
{
    /// <summary>
    /// Расширение для работы с типами туров
    /// </summary>
    public static class TipToursExtension
    {
        private static readonly object LockTipTurs = new object();
        /// <summary>
        /// Название таблтицы в БД
        /// </summary>
        public const string TableName = "TipTur";

        public static IList<TipTur> GetTourTipsByKeys(this MtSearchDbDataContext dc, IEnumerable<int> tourTipKeys, out string hash)
        {
            List<TipTur> tourTips;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, String.Join(",", tourTipKeys));

            if ((tourTips = CacheHelper.GetCacheItem<List<TipTur>>(hash)) != null) return tourTips;

            tourTips = (from t in dc.GetAllTourTips()
                        where tourTipKeys.Contains(t.TP_KEY)
                        select t)
                .ToList<TipTur>();

            CacheHelper.AddCacheData(hash, tourTips, TableName);

            return tourTips;
        }

        /// <summary>
        /// Возвращает список всех TourTips
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static List<TipTur> GetAllTourTips(this MtSearchDbDataContext dc)
        {
            List<TipTur> tourTips;
            if ((tourTips = CacheHelper.GetCacheItem<List<TipTur>>(TableName)) != null) return tourTips;
            lock (LockTipTurs)
            {
                if ((tourTips = CacheHelper.GetCacheItem<List<TipTur>>(TableName)) != null) return tourTips;

                tourTips = (from t in dc.TipTurs
                         select t)
                    .ToList<TipTur>();

                CacheHelper.AddCacheData(TableName, tourTips, TableName);
            }
            return tourTips;
        }

        /// <summary>
        /// Возвращает ключи страны и города вылета по ключу цены
        /// </summary>
        /// <param name="mainDc">Контекст основной БД</param>
        /// <param name="priceKey">Ключ цены</param>
        /// <returns></returns>
        public static Tuple<int, int> GetCountryCityKeysByTourKey(this MtMainDbDataContext mainDc, int priceKey)
        {
            string hashOut;
            return mainDc.GetCountryCityKeysByTourKey(priceKey, false, out hashOut);
        }

        /// <summary>
        /// Возвращает ключи страны и города вылета по ключу цены
        /// </summary>
        /// <param name="mainDc">Контекст основной БД</param>
        /// <param name="priceKey">Ключ цены</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static Tuple<int, int> GetCountryCityKeysByTourKey(this MtMainDbDataContext mainDc, int priceKey, out string hash)
        {
            return mainDc.GetCountryCityKeysByTourKey(priceKey, true, out hash);
        }

        /// <summary>
        /// Возвращает ключи страны и города вылета по ключу цены
        /// </summary>
        /// <param name="mainDc">Контекст основной БД</param>
        /// <param name="priceKey">Ключ цены</param>
        /// <param name="useCache">Используется ли кэш или нет</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static Tuple<int, int> GetCountryCityKeysByTourKey(this MtMainDbDataContext mainDc, int priceKey, bool useCache, out string hash)
        {
            Tuple<int, int> result = null;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, priceKey);
            if (useCache && (result = CacheHelper.GetCacheItem<Tuple<int, int>>(hash)) != null) return result;

            var cacheDependencies = new List<string>();
            var commandBuilder = new StringBuilder();
            commandBuilder.AppendLine("select TO_CNKey, ISNULL(TL_CTDepartureKey, 0) ");
            commandBuilder.AppendLine("from tp_tours ");
            commandBuilder.AppendLine("join tbl_TurList on TL_Key = TO_TRKey ");
            commandBuilder.AppendLine(
                String.Format("where exists (select 1 from tp_prices where tp_tokey = to_key and tp_key = {0}) ", priceKey));

            using (var command = mainDc.Connection.CreateCommand())
            {
                command.CommandText = commandBuilder.ToString();

                mainDc.Connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result = new Tuple<int, int>(reader.GetInt32(0), reader.GetInt32(1));
                    }
                }
                mainDc.Connection.Close();
            }

            cacheDependencies.Add(TPToursExtension.TableName);
            cacheDependencies.Add(TurListsExtension.TableName);

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);

            return result;
        }
    }
}
