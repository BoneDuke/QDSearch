using System.Text;
using System.Web.Caching;
using QDSearch.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using QDSearch.Extensions;
using QDSearch.Helpers;
using System.Reflection;
using System.Threading;
using QDSearch.Repository.MtMain;
using QDSearch.Repository.SftWeb;

namespace QDSearch.Repository.MtSearch
{
    /// <summary>
    /// Класс с методами получения данных для фильтра
    /// </summary>
    public static class GetFilterMethods
    {

        /// <summary>
        /// Массив для хранения столбцов, по которым будет происходить сортировка
        /// </summary>
        public static readonly Dictionary<SortingColumn, string> SortingColumns = new Dictionary
            <SortingColumn, string>
        {
            {SortingColumn.TourDate, "pt_tourdate"},
            {SortingColumn.HotelName, "pt_hdname"},
            {SortingColumn.HotelCategoryName, "pt_hdstars"},
            {SortingColumn.HotelRegionName, "pt_ctname"},
            {SortingColumn.RoomTypeName, "pt_rmname"},
            {SortingColumn.RoomCategoryName, "pt_rcname"},
            {SortingColumn.AccomodationName, "pt_acname"},
            {SortingColumn.PansionName, "pt_pnname"},
            {SortingColumn.NightsCount, "pt_nights"},
            {SortingColumn.Price, "pt_price"},
            {SortingColumn.TourName, "pt_tourname"}
        };
        /*
        private const string CacheKeyPattern = @"^(.*)_(.*)_(.*)_(.*)$";
        private static readonly List<CacheLevel> CacheLevels = new List<CacheLevel>
        {
            new CacheLevel {MethodName = "GetTourTypes", Priority = 1},
            new CacheLevel {MethodName = "GetCitiesTo", Priority = 2},
            new CacheLevel {MethodName = "GetTours", Priority = 3},
            new CacheLevel {MethodName = "GetTourDates", Priority = 4},
            new CacheLevel {MethodName = "GetTourNights", Priority = 5},
            new CacheLevel {MethodName = "GetTourHotelClasses", Priority = 6},
            new CacheLevel {MethodName = "GetTourHotels", Priority = 7},
            new CacheLevel {MethodName = "GetTourPansions", Priority = 8},
            new CacheLevel {MethodName = "GetTourRooms", Priority = 9}
        };
         * */
        private static DateTime _prevDateCacheDirectionCheck = DateTime.Now;
        private static DateTime _prevDateCacheObjectCheck = DateTime.Now;
        /// <summary>
        /// Делегат для вызова потока очистки кэша направлений
        /// </summary>
        public delegate void ClearOldDirectionCacheDelegate();
        /// <summary>
        /// Делегат для вызова потока очистки кэша объектов
        /// </summary>
        public delegate void ClearOldObjectCacheDelegate();

        /// <summary>
        /// Очищает старый кэш направлений
        /// </summary>
        public static void ClearOldDrectionCache()
        {
            while (true)
            {
                //todo: перенести в web.config
                Thread.Sleep(60 * 1000);
                using (var dc = new MtSearchDbDataContext())
                {
                    dc.ClearOldDirectionCacheData();
                }
                
            }
        }

        /// <summary>
        /// Очищает старый кэш объектов
        /// </summary>
        public static void ClearOldObjectCache()
        {
            while (true)
            {
                //todo: перенести в web.config
                Thread.Sleep(60 * 1000);
                using (var dcSft = new SftWebDbDataContext())
                {
                    using (var dc = new MtSearchDbDataContext())
                    {
                        dcSft.ClearOldObjectCacheData(dc);
                    }
                }
            }
        }

        /// <summary>
        /// Очищает кэш по изменившимся объектам
        /// </summary>
        /// <param name="dcSft">Контекст базы Sft</param>
        /// <param name="dc">Контекст поисковой базы</param>
        public static void ClearOldObjectCacheData(this SftWebDbDataContext dcSft, MtSearchDbDataContext dc)
        {
            var currentTime = DateTime.Now;
            var commandBuilder = new StringBuilder();
            commandBuilder.AppendLine("select o.[Key], o.ServiceKey, o.Code, a.Name ");
            commandBuilder.AppendLine("from ObjectCacheChanges as o ");
            commandBuilder.AppendLine("join AvalonObjects as a on a.[Key] = o.[ObjectKey] ");
            commandBuilder.AppendLine(String.Format("where o.UpdateTime > '{0}'", _prevDateCacheObjectCheck.ToString("yyyy-MM-dd HH:mm:ss")));


            var cacheKeyObjectList = new List<CacheKeyObject>();
            using (var command = dcSft.Connection.CreateCommand())
            {
                command.CommandText = commandBuilder.ToString();
                dcSft.Connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var key = reader.GetInt32(0);
                        var hash = String.Format("{0}_{1}", CacheHelper.ObjectCacheChangesHash, key);
                        if (CacheHelper.IsCacheKeyExists(hash))
                            continue;

                        var obj = new CacheKeyObject()
                        {
                            Code = reader.GetInt32OrNull(2),
                            Name = reader.GetString(3),
                            ServiceKey = reader.GetInt32(1)
                        };

                        if (!cacheKeyObjectList.Contains(obj, new CacheQuotaKeyComparer()))
                            cacheKeyObjectList.Add(obj);

                        CacheHelper.AddCacheData(hash, key, null, Globals.Settings.Cache.MediumCacheTimeout);
                    }
                }
                dcSft.Connection.Close();
            }

            var cacheItemsToInvalidate = new List<string>();
            foreach (var obj in cacheKeyObjectList)
            {
                if (obj.Name == CacheHelper.AirSeasonHash ||
                    obj.Name == CacheHelper.CostHash ||
                    obj.Name == CacheHelper.CharterHash)
                {
                    if (obj.ServiceKey == 1 && obj.Code != null)
                    {
                        int? cityKeyFrom, cityKeyTo;
                        dc.GetCharterCityDirection(obj.Code.Value, out cityKeyFrom, out cityKeyTo);
                        cacheItemsToInvalidate.Add(String.Format("{0}_{1}_{2}", CacheHelper.CharterDirectionHash, cityKeyFrom.HasValue ? cityKeyFrom.Value : -1, cityKeyTo.HasValue ? cityKeyTo.Value : -1));
                    }
                }

                cacheItemsToInvalidate.Add(obj.Code.HasValue
                    ? String.Format("{0}_{1}_{2}", obj.Name, obj.ServiceKey, obj.Code.Value)
                    : String.Format("{0}_{1}", obj.Name, obj.ServiceKey));
            }

            CacheHelper.ReInitCacheDependencies(cacheItemsToInvalidate, true);
            _prevDateCacheObjectCheck = currentTime;
        }

        /// <summary>
        /// Очищает кэш по изменившимся направлениям
        /// </summary>
        /// <param name="dc">Контекст поисковой базы</param>
        public static void ClearOldDirectionCacheData(this MtSearchDbDataContext dc)
        {
            var currentTime = DateTime.Now;
            IEnumerable<CacheKeyDirection> result = (from queue in dc.mwReplQueues
                                                      where queue.rq_enddate > _prevDateCacheDirectionCheck.AddMinutes(-10)
                                                            && queue.rq_state == 5
                                                     select new CacheKeyDirection { Key = queue.rq_id, CountryKey = queue.rq_cnkey, CityKeyFrom = queue.rq_ctkeyfrom })
                .ToList();

            var cacheItemsToInvalidate = new List<string>();
            //очищаем кэш по полученным направлениям
            foreach (var r in result)
            {
                var hash = String.Format("{0}_{1}", CacheHelper.MwReplQueueHash, r.Key);
                if (CacheHelper.IsCacheKeyExists(hash))
                    continue;

                cacheItemsToInvalidate.Add(String.Format("{0}_{1}_{2}", CacheHelper.DirectionHash, r.CityKeyFrom, r.CountryKey));
                CacheHelper.AddCacheData(hash, r.Key, null, Globals.Settings.Cache.MediumCacheTimeout);
            }

            CacheHelper.ReInitCacheDependencies(cacheItemsToInvalidate, false);
            _prevDateCacheDirectionCheck = currentTime;
        }

        /*
        private static void CacheItemRemovedCallback(string key, Object value, CacheItemRemovedReason reason)
        {
            // удаляем весь кэш внизу только если этот элемент не был удален нами вручную. 
            // В этом случае записи и так будут удалены
            if (reason != CacheItemRemovedReason.Removed && Regex.IsMatch(key, CacheKeyPattern))
            {
                var match = Regex.Match(key, CacheKeyPattern);
                string methodName = match.Groups[1].Value;
                string countryCityKey = match.Groups[2].Value + "_" + match.Groups[3].Value;

                var priority = CacheLevels.Where(c => c.MethodName == methodName).Select(c => c.Priority).FirstOrDefault();
                var cacheMethods = CacheLevels.Where(c => c.Priority > priority).Select(c => c.MethodName);
                foreach (var method in cacheMethods)
                {
                    CacheHelper.RemoveCacheDataByKeyPart(method + "_" + countryCityKey);
                }
            }
        }
        */
        /// <summary>
        /// Цена за по стране
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="countryKey">Ключ страны</param>
        /// <returns>true - за человека, false - за номер</returns>
        //public static bool IsCountryPriceForRoom(this MtSearchDbDataContext dc, int countryKey)
        public static bool IsCountryPriceForMen(this MtSearchDbDataContext dc, int countryKey)
        {
            var hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, countryKey);
            if (CacheHelper.IsCacheKeyExists(hash))
                return CacheHelper.GetCacheItem<bool>(hash);

            //число туров за человека
            var priceFor0 = (from tour in dc.TP_Tours
                where tour.TO_IsEnabled > 0
                        && tour.TO_PriceFor == (int)PriceForType.PerMen && tour.TO_CNKey == countryKey
                select tour.TO_Key).Count();

            //число туров за номер
            var priceFor1 = (from tour in dc.TP_Tours
                where tour.TO_IsEnabled > 0
                        && tour.TO_PriceFor == (int)PriceForType.PerRoom && tour.TO_CNKey == countryKey
                select tour.TO_Key).Count();

            var result = priceFor0 >= priceFor1;

            CacheHelper.AddCacheData(hash, result, null, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Возвращает список городов вылета
        /// </summary>
        /// <param name="dc">Контекст базы данных AvalonWeb</param>
        /// <param name="sftDc">Контекст базы данных Sft</param>
        /// <param name="countryKey">Ключ страны</param>
        /// <returns></returns>
        public static IDictionary<int, string> GetCitiesDeparture(this MtSearchDbDataContext dc, SftWebDbDataContext sftDc, int countryKey)
        {
            string strEmpty;
            return GetCitiesDeparture(dc, sftDc, countryKey, out strEmpty);
        }

        /// <summary>
        /// Возвращает список городов вылета
        /// </summary>
        /// <param name="dc">Контекст базы данных AvalonWeb</param>
        /// <param name="sftDc">Контекст базы данных Sft</param>
        /// <param name="countryKey">Ключ страны</param>
        /// <param name="hash">СТрока со значением ключа кэша, для CacheDependency</param>
        /// <returns></returns>
        public static IDictionary<int, string> GetCitiesDeparture(this MtSearchDbDataContext dc, SftWebDbDataContext sftDc, int? countryKey, out string hash)
        {
            IDictionary<int, string> result;

            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, countryKey);
            if ((result = CacheHelper.GetCacheItem<IDictionary<int, string>>(hash)) != default(IDictionary<int, string>)) return result;

            var cityKeys = new List<int>();
            var commandBuilder = new StringBuilder();
            commandBuilder.AppendLine("select distinct CityKeyFrom ");
            commandBuilder.AppendLine("from SearchFilterDirections ");
            if (countryKey != null)
                commandBuilder.AppendLine(String.Format("where CountryKey = {0} ", countryKey));

            using (var command = sftDc.Connection.CreateCommand())
            {
                command.CommandText = commandBuilder.ToString();

                sftDc.Connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cityKeys.Add(reader.GetInt32(0));
                    }
                }
                sftDc.Connection.Close();
            }

            string hashOut;
            var cacheDependencies = new List<string>();
            result = (from c in dc.GetCitiesByKeys(cityKeys, out hashOut)
                      select new { Key = c.CT_KEY, Name = c.CT_NAME })
                      .Distinct()
                .OrderBy(v => v.Name)
                .ToDictionary(v => v.Key, v => v.Name);

            cacheDependencies.Add(hashOut);

            if (result.Count == 0)
                throw new ArgumentNullException(MethodBase.GetCurrentMethod().Name,
                    "Метод по запросу не вернул ни одного элемента.");

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Возвращает список стран туров
        /// </summary>
        /// <param name="dc">Контекст базы данных AvalonWeb</param>
        /// <param name="dcSft">Контекст базы данных Sft</param>
        /// <returns></returns>
        public static IDictionary<int, string> GetCountriesTo(this MtSearchDbDataContext dc, SftWebDbDataContext dcSft)
        {
            string strEmpty;
            return GetCountriesTo(dc, dcSft, null, out strEmpty);
        }


        /// <summary>
        /// Возвращает список стран туров
        /// </summary>
        /// <param name="dc">Контекст базы данных AvalonWeb</param>
        /// <param name="dcSft">Контекст базы данных Sft</param>
        /// <param name="cityDepartureKey">Ключ города вылета</param>
        /// <param name="hash">Строка кэша для использования CacheDependency</param>
        /// <returns></returns>
        public static IDictionary<int, string> GetCountriesTo(this MtSearchDbDataContext dc, SftWebDbDataContext dcSft, int? cityDepartureKey, out string hash)
        {
            Dictionary<int, string> result;

            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, cityDepartureKey);
            if ((result = CacheHelper.GetCacheItem<Dictionary<int, string>>(hash)) != null) return result;

            var countryKeys = new List<int>();
            var commandBuilder = new StringBuilder();
            commandBuilder.AppendLine("select distinct CountryKey ");
            commandBuilder.AppendLine("from SearchFilterDirections ");
            if (cityDepartureKey != null)
                commandBuilder.AppendLine(String.Format("where CityKeyFrom = {0}", cityDepartureKey));

            using (var command = dcSft.Connection.CreateCommand())
            {
                command.CommandText = commandBuilder.ToString();

                dcSft.Connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        countryKeys.Add(reader.GetInt32(0));
                    }
                }
                dcSft.Connection.Close();
            }

            string hashOut;
            var cacheDependencies = new List<string>();
            result = (from c in dc.GetCountriesByKeys(countryKeys, out hashOut)
                      select new { Key = c.CN_KEY, Name = c.CN_NAME })
                      .Distinct()
                .OrderBy(v => v.Name)
                .ToDictionary(v => v.Key, v => v.Name);

            cacheDependencies.Add(hashOut);

            if (((IDictionary<int, string>)result).Count == 0)
                throw new ArgumentNullException(MethodBase.GetCurrentMethod().Name,
                    "Метод по запросу не вернул ни одного элемента.");

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.MediumCacheTimeout);
            return result;
        }

        /// <summary>
        /// Возвращает список типов туров
        /// </summary>
        /// <param name="dc">Контекст базы данных AvalonWeb</param>
        /// <param name="sftDc">Контекст базы данных Sft</param>
        /// <param name="cityKeyFrom">Город вылета</param>
        /// <param name="countryKey">Ключ страны</param>
        /// <returns></returns>
        public static IDictionary<int, string> GetTourTypes(this MtSearchDbDataContext dc, SftWebDbDataContext sftDc, int cityKeyFrom, int countryKey)
        {
            string hash;
            return GetTourTypes(dc, sftDc, cityKeyFrom, countryKey, out hash);
        }

        /// <summary>
        /// Возвращает список типов туров
        /// </summary>
        /// <param name="dc">Контекст базы данных AvalonWeb</param>
        /// <param name="sftDc">Контекст базы данных Sft</param>
        /// <param name="cityKeyFrom">Город вылета</param>
        /// <param name="countryKey">Ключ страны</param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static IDictionary<int, string> GetTourTypes(this MtSearchDbDataContext dc, SftWebDbDataContext sftDc, int cityKeyFrom, int countryKey, out string hash)
        {
            Dictionary<int, string> result;

            hash = String.Format("{0}_{1}_{2}", MethodBase.GetCurrentMethod().Name, cityKeyFrom, countryKey);
            if ((result = CacheHelper.GetCacheItem<Dictionary<int, string>>(hash)) != null) return result;

            var cacheDependencies = new List<string>
            {
                String.Format("{0}_{1}_{2}", CacheHelper.DirectionHash, cityKeyFrom, countryKey)
            };

            var typeKeys = new List<int>();
            var commandBuilder = new StringBuilder();
            commandBuilder.AppendLine("select distinct tourtype ");
            commandBuilder.AppendLine(String.Format("from SearchFilter_{0}_{1}", countryKey, cityKeyFrom));

            using (var command = sftDc.Connection.CreateCommand())
            {
                command.CommandText = commandBuilder.ToString();

                sftDc.Connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        typeKeys.Add(reader.GetInt32(0));
                    }
                }
                sftDc.Connection.Close();
            }

            string hashOut;
            result = (from t in dc.GetTourTipsByKeys(typeKeys, out hashOut)
                      select new { Key = t.TP_KEY, Name = t.TP_NAME })
                      .Distinct()
                .OrderBy(v => v.Name)
                .ToDictionary(v => v.Key, v => v.Name);
            cacheDependencies.Add(hashOut);

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);

            return result;
        }

        //private static void AddToCacheForLongPeriod(string hash, object result)
        //{
        //    if (!Globals.Settings.Cache.Enabled) return;
        //    CacheHelper.AddCacheData(hash, result, Globals.Settings.Cache.LongCacheTimeout);
        //}

        //private static void AddToCacheForLongPeriod(string hash, object result, CacheDependency dependency)
        //{
        //    if (!Globals.Settings.Cache.Enabled) return;
        //    CacheHelper.AddCacheData(hash, result, dependency, Globals.Settings.Cache.LongCacheTimeout);
        //}

        /// <summary>
        /// Возвращает список городов
        /// </summary>
        /// <param name="dc">Контекст базы данных AvalonWeb</param>
        /// <param name="sftDc">Контекст базы данных Sft</param>
        /// <param name="cityKeyFrom">Город вылета</param>
        /// <param name="countryKey">Ключ страны</param>
        /// <param name="tourTypes">Ключи типа тура</param>
        /// <returns></returns>
        public static IDictionary<int, string> GetCitiesTo(this MtSearchDbDataContext dc, SftWebDbDataContext sftDc,
            int cityKeyFrom, int countryKey, IEnumerable<int> tourTypes)
        {
            string strEmpty;
            return GetCitiesTo(dc, sftDc, cityKeyFrom, countryKey, tourTypes, out strEmpty);
        }

        /// <summary>
        /// Возвращает список городов
        /// </summary>
        /// <param name="dc">Контекст базы данных AvalonWeb</param>
        /// <param name="sftDc">Контекст базы данных Sft</param>
        /// <param name="cityKeyFrom">Город вылета</param>
        /// <param name="countryKey">Ключ страны</param>
        /// <param name="tourTypes">Ключи типа тура</param>
        /// <param name="hash">Строка хэша кэша</param>
        /// <returns></returns>
        public static IDictionary<int, string> GetCitiesTo(this MtSearchDbDataContext dc, SftWebDbDataContext sftDc, int cityKeyFrom, int countryKey, IEnumerable<int> tourTypes, out string hash)
        {
            // ReSharper disable PossibleMultipleEnumeration
            //if (tourTypes == null || !tourTypes.Any())
            //    throw new ArgumentNullException("tourTypes",
            //        "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            Dictionary<int, string> result;

            hash = String.Format("{0}_{1}_{2}_{3}", MethodBase.GetCurrentMethod().Name, cityKeyFrom, countryKey, tourTypes != null ? CacheHelper.GetCacheKeyHashed(new[] { String.Join("_", tourTypes) }) : String.Empty);
            if ((result = CacheHelper.GetCacheItem<Dictionary<int, string>>(hash)) != null) return result;

            var cacheDependencies = new List<string>
            {
                String.Format("{0}_{1}_{2}", CacheHelper.DirectionHash, cityKeyFrom, countryKey)
            };

            var cityKeys = new List<int>();
            var commandBuilder = new StringBuilder();
            commandBuilder.AppendLine("select distinct ctkey ");
            commandBuilder.AppendLine(String.Format("from SearchFilter_{0}_{1} ", countryKey, cityKeyFrom));
            if (tourTypes != null)
                commandBuilder.AppendLine(String.Format("where tourtype in ({0}) ", String.Join(",", tourTypes)));

            using (var command = sftDc.Connection.CreateCommand())
            {
                command.CommandText = commandBuilder.ToString();

                sftDc.Connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cityKeys.Add(reader.GetInt32(0));
                    }
                }
                sftDc.Connection.Close();
            }

            string hashOut;
            result = (from c in dc.GetCitiesByKeys(cityKeys, out hashOut)
                      where c.CT_CNKEY == countryKey
                      select new { Key = c.CT_KEY, Name = c.CT_NAME })
                      .Distinct()
                .OrderBy(v => v.Name)
                .ToDictionary(v => v.Key, v => v.Name);
            cacheDependencies.Add(hashOut);

            //if (((IDictionary<int, string>)result).Count == 0)
            //    throw new ArgumentNullException(MethodBase.GetCurrentMethod().Name,
            //        "Метод по запросу не вернул ни одного элемента. Параметры: " + hash);

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Возвращает список туров
        /// </summary>
        /// <param name="dc">Контекст базы данных AvalonWeb</param>
        /// <param name="sftDc">Контекст базы данных Sft</param>
        /// <param name="cityKeyFrom">Ключ города вылета</param>
        /// <param name="countryKey">Ключ страны тура</param>
        /// <param name="tourTypes">Ключи типа тура</param>
        /// <param name="cityKeys">Ключи регионов, передавать только в том случае, если реально выбраны пользователем</param>
        /// <returns></returns>
        public static IDictionary<int, string> GetTours(this MtSearchDbDataContext dc, SftWebDbDataContext sftDc,
            int cityKeyFrom, int countryKey, IEnumerable<int> tourTypes, IEnumerable<int> cityKeys)
        {
            string strEmpty;
            return GetTours(dc, sftDc, cityKeyFrom, countryKey, tourTypes, cityKeys, out strEmpty);
        }

        /// <summary>
        /// Возвращает список туров
        /// </summary>
        /// <param name="dc">Контекст базы данных AvalonWeb</param>
        /// <param name="sftDc">Контекст базы данных Sft</param>
        /// <param name="cityKeyFrom">Ключ города вылета</param>
        /// <param name="countryKey">Ключ страны тура</param>
        /// <param name="tourTypes">Ключи типа тура</param>
        /// <param name="cityKeys">Ключи регионов, передавать только в том случае, если реально выбраны пользователем</param>
        /// <param name="hash">Строка хэша кэша</param>
        /// <returns></returns>
        public static IDictionary<int, string> GetTours(this MtSearchDbDataContext dc, SftWebDbDataContext sftDc, int cityKeyFrom, int countryKey, IEnumerable<int> tourTypes, IEnumerable<int> cityKeys, out string hash)
        {
            if (tourTypes == null || !tourTypes.Any())
                throw new ArgumentNullException("tourTypes",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            if (cityKeys == null || !cityKeys.Any())
                throw new ArgumentNullException("cityKeys",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            Dictionary<int, string> result;

            hash = String.Format("{0}_{1}_{2}_{3}", MethodBase.GetCurrentMethod().Name, cityKeyFrom, countryKey, CacheHelper.GetCacheKeyHashed(new[] { String.Join("_", tourTypes), String.Join("_", cityKeys) }));

            if ((result = CacheHelper.GetCacheItem<Dictionary<int, string>>(hash)) != null) return result;

            var cacheDependencies = new List<string>
            {
                String.Format("{0}_{1}_{2}", CacheHelper.DirectionHash, cityKeyFrom, countryKey)
            };

            var tourKeys = new List<int>();
            var commandBuilder = new StringBuilder();
            commandBuilder.AppendLine("select distinct tourkey ");
            commandBuilder.AppendLine(String.Format("from SearchFilter_{0}_{1} ", countryKey, cityKeyFrom));
            commandBuilder.AppendLine(String.Format("where tourtype in ({0}) ", String.Join(",", tourTypes)));
            commandBuilder.AppendLine(String.Format("and ctkey in ({0}) ", String.Join(",", cityKeys)));

            using (var command = sftDc.Connection.CreateCommand())
            {
                command.CommandText = commandBuilder.ToString();

                sftDc.Connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tourKeys.Add(reader.GetInt32(0));
                    }
                }
                sftDc.Connection.Close();
            }

            string hashOut;
            var tpTours = dc.GetTPToursByKeys(tourKeys, out hashOut);
            cacheDependencies.Add(hashOut);

            var tlTours = dc.GetTurListsByKeys(tpTours.Select(t => t.TO_TRKey).Distinct().ToList(), out hashOut).ToList();
            cacheDependencies.Add(hashOut);

            result = (from tp in tpTours
                      join tl in tlTours on tp.TO_TRKey equals tl.TL_KEY
                      where tourKeys.Contains(tp.TO_Key)
                      select new { Key = tp.TO_Key, Name = tl.TL_NAMEWEB })
                      .Distinct()
                .OrderBy(v => v.Name)
                .ToDictionary(v => v.Key, v => v.Name);

            //if (((IDictionary<int, string>)result).Count == 0)
            //    throw new ArgumentNullException(MethodBase.GetCurrentMethod().Name,
            //        "Метод по запросу не вернул ни одного элемента. Параметры: " + hash);

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Возвращает список дат заездов
        /// </summary>
        /// <param name="dc">Контекст базы данных AvalonWeb</param>
        /// <param name="sftDc">Контекст базы данных Sft</param>
        /// <param name="cityKeyFrom">Город вылета</param>
        /// <param name="countryKey">Страна тура</param>
        /// <param name="cityToKeys">Города проживания</param>
        /// <param name="tourKeys">Ключи туров</param>
        /// <returns></returns>
        public static IList<DateTime> GetTourDates(this MtSearchDbDataContext dc, SftWebDbDataContext sftDc,
            int cityKeyFrom, int countryKey, IEnumerable<int> cityToKeys, IEnumerable<int> tourKeys)
        {
            string strEmpty;
            return GetTourDates(dc, sftDc, cityKeyFrom, countryKey, cityToKeys, tourKeys, out strEmpty);
        }

        /// <summary>
        /// Возвращает список дат заездов
        /// </summary>
        /// <param name="dc">Контекст базы данных AvalonWeb</param>
        /// <param name="sftDc">Контекст базы данных Sft</param>
        /// <param name="cityKeyFrom">Город вылета</param>
        /// <param name="countryKey">Страна тура</param>
        /// <param name="cityToKeys">Города проживания</param>
        /// <param name="tourKeys">Ключи туров</param>
        /// <param name="hash">Строка хэша кэша</param>
        /// <returns></returns>
        public static IList<DateTime> GetTourDates(this MtSearchDbDataContext dc, SftWebDbDataContext sftDc, int cityKeyFrom, int countryKey, IEnumerable<int> cityToKeys, IEnumerable<int> tourKeys, out string hash)
        {
            if (tourKeys == null || !tourKeys.Any())
                throw new ArgumentNullException("tourKeys",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            List<DateTime> result;

            hash = String.Format("{0}_{1}_{2}_{3}", MethodBase.GetCurrentMethod().Name, cityKeyFrom, countryKey, CacheHelper.GetCacheKeyHashed(new[] { String.Join("_", tourKeys) }));

            if ((result = CacheHelper.GetCacheItem<List<DateTime>>(hash)) != null) return result;

            var cacheDependencies = new List<string>
            {
                String.Format("{0}_{1}_{2}", CacheHelper.DirectionHash, cityKeyFrom, countryKey)
            };
            result = new List<DateTime>();

            var commandBuilder = new StringBuilder();
            commandBuilder.AppendLine("select distinct tourdate ");
            commandBuilder.AppendLine(String.Format("from SearchFilter_{0}_{1} ", countryKey, cityKeyFrom));
            commandBuilder.AppendLine("where ctkey in (" + string.Join(",", cityToKeys) + ") ");
            commandBuilder.AppendLine("and tourkey in (" + string.Join(",", tourKeys) + ") ");
            commandBuilder.AppendLine(String.Format("and tourdate >= '{0}' ", DateTime.Now.Date.ToString("yyyy-MM-dd")));

            using (var command = sftDc.Connection.CreateCommand())
            {
                command.CommandText = commandBuilder.ToString();
                sftDc.Connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetDateTime(0));
                    }
                }
                sftDc.Connection.Close();
            }

            result = result.OrderBy(d => d).ToList();

            //if (((IList<DateTime>)result).Count == 0)
            //    throw new ArgumentNullException(MethodBase.GetCurrentMethod().Name,
            //        "Метод по запросу не вернул ни одного элемента. Параметры: " + hash);

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Внутренний метод для получения списка отелей и питаний для фильтров
        /// </summary>
        /// <param name="sftDc">Контекст базы данных Sft</param>
        /// <param name="cityKeyFrom">Город вылета</param>
        /// <param name="countryKey">Страна тура</param>
        /// <param name="cityToKeys">Города проживания</param>
        /// <param name="tourKeys">Ключи туров</param>
        /// <param name="tourDates">Даты заездов</param>
        /// <returns></returns>
        private static IEnumerable<HotelPansionFilterClass> GetHotelPansionFilterClasses(SftWebDbDataContext sftDc, int cityKeyFrom, int countryKey,
            IEnumerable<int> cityToKeys, IEnumerable<int> tourKeys, IEnumerable<DateTime> tourDates)
        {
            if (cityToKeys == null || !cityToKeys.Any())
                throw new ArgumentNullException("cityToKeys",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            if (tourKeys == null || !tourKeys.Any())
                throw new ArgumentNullException("tourKeys",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            if (tourDates == null || !tourDates.Any())
                throw new ArgumentNullException("tourDates",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");


            var hash = String.Format("{0}_{1}_{2}_{3}", MethodBase.GetCurrentMethod().Name, cityKeyFrom, countryKey, CacheHelper.GetCacheKeyHashed(new[] { String.Join("_", cityToKeys), String.Join("_", tourKeys), String.Join("_", tourDates) }));
            List<HotelPansionFilterClass> result;
            if ((result = CacheHelper.GetCacheItem<List<HotelPansionFilterClass>>(hash)) != null) return result;

            var cacheDependencies = new List<string>
            {
                String.Format("{0}_{1}_{2}", CacheHelper.DirectionHash, cityKeyFrom, countryKey)
            };
            result = new List<HotelPansionFilterClass>();

            var commandBuilder = new StringBuilder();
            commandBuilder.AppendLine("select distinct pnkey, hdkey, nights ");
            commandBuilder.AppendLine(String.Format("from SearchFilter_{0}_{1} ", countryKey, cityKeyFrom));
            commandBuilder.Append("where tourdate in (");
            foreach (var dateString in tourDates.Select(d => d.ToString("yyyy-MM-dd")))
                commandBuilder.AppendFormat("'{0}',", dateString);
            commandBuilder = commandBuilder.Remove(commandBuilder.Length - 1, 1);
            commandBuilder.AppendLine(") ");
            commandBuilder.AppendLine("and ctkey in (" + string.Join(",", cityToKeys) + ") ");
            commandBuilder.AppendLine("and tourkey in (" + string.Join(",", tourKeys) + ") ");

            using (var command = sftDc.Connection.CreateCommand())
            {
                command.CommandText = commandBuilder.ToString();
                sftDc.Connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new HotelPansionFilterClass
                        {
                            PansionKey = reader.GetInt32(0),
                            HotelKey = reader.GetInt32(1),
                            Nights = reader.GetInt32(2)
                        });
                    }
                }
                sftDc.Connection.Close();
            }

            if (result.Count == 0)
                throw new ArgumentNullException(MethodBase.GetCurrentMethod().Name,
                    "Метод по запросу не вернул ни одного элемента. Запрос: " + commandBuilder);

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Внутренний метод для получения списка отелей и питаний для фильтров
        /// </summary>
        /// <param name="dc">Контекст базы dc</param>
        /// <param name="sftDc">Контекст базы данных Sft</param>
        /// <param name="cityKeyFrom">Город вылета</param>
        /// <param name="countryKey">Страна тура</param>
        /// <param name="cityToKeys">Города проживания</param>
        /// <param name="tourKeys">Ключи туров</param>
        /// <param name="hotelCats">Категории отелей</param>
        /// <param name="pansionKeys">Ключи питаний</param>
        /// <param name="hotelKeys">Ключи отелей</param>
        /// <returns></returns>
        private static IEnumerable<Hotel> GetAllFilterHotels(MtSearchDbDataContext dc, SftWebDbDataContext sftDc, int cityKeyFrom, int countryKey,
            IEnumerable<int> cityToKeys, IEnumerable<int> tourKeys, IEnumerable<string> hotelCats, IEnumerable<int> pansionKeys, IEnumerable<int> hotelKeys)
        {
            if (cityToKeys == null || !cityToKeys.Any())
                throw new ArgumentNullException("cityToKeys",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            if (tourKeys == null || !tourKeys.Any())
                throw new ArgumentNullException("tourKeys",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            if (hotelCats == null || !hotelCats.Any())
                throw new ArgumentNullException("hotelCats",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            if (pansionKeys == null || !pansionKeys.Any())
                throw new ArgumentNullException("pansionKeys",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            if (hotelKeys == null || !hotelKeys.Any())
                throw new ArgumentNullException("hotelKeys",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            var hash = String.Format("{0}_{1}_{2}_{3}", MethodBase.GetCurrentMethod().Name, cityKeyFrom, countryKey, CacheHelper.GetCacheKeyHashed(new[] { String.Join("_", cityToKeys), String.Join("_", tourKeys), String.Join("_", hotelCats), String.Join("_", pansionKeys), String.Join("_", hotelKeys) }));
            List<Hotel> result;
            if ((result = CacheHelper.GetCacheItem<List<Hotel>>(hash)) != null) return result;

            var cacheDependencies = new List<string>
            {
                String.Format("{0}_{1}_{2}", CacheHelper.DirectionHash, cityKeyFrom, countryKey)
            };
            string hashOut;
            var hotelsByCountry = dc.GetHotelsByCountry(countryKey, out hashOut).ToList();
            cacheDependencies.Add(hashOut);

            var hotels = new List<int>();
            var commandBuilder = new StringBuilder();
            commandBuilder.AppendLine("select distinct hdkey ");
            commandBuilder.AppendLine(String.Format("from SearchFilter_{0}_{1} ", countryKey, cityKeyFrom));
            commandBuilder.AppendLine("where ctkey in (" + string.Join(",", cityToKeys) + ") ");
            commandBuilder.AppendLine("and tourkey in (" + string.Join(",", tourKeys) + ") ");
            commandBuilder.AppendLine("and pnkey in (" + string.Join(",", pansionKeys) + ") ");
            commandBuilder.AppendLine(String.Format("and tourdate >= '{0}' ", DateTime.Now.Date.ToString("yyyy-MM-dd")));
            commandBuilder.AppendLine("and hdkey not in (" + string.Join(",", hotelKeys) + ") ");

            using (var command = sftDc.Connection.CreateCommand())
            {
                command.CommandText = commandBuilder.ToString();
                sftDc.Connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var hotelKey = reader.GetInt32(0);
                        if (!hotels.Contains(hotelKey) && hotelsByCountry.Any(h => h.Key == hotelKey && hotelCats.Contains(h.Stars)))
                            hotels.Add(hotelKey);
                    }
                }
                sftDc.Connection.Close();
            }

            var hotelsByKeys = dc.GetHotelsByKeys(hotels, out hashOut);
            cacheDependencies.Add(hashOut);
            var resorts = dc.GetResortsByKeys(hotelsByKeys.Where(h => h.RsKey != null).Select(h => h.RsKey.Value).ToList(), out hashOut);
            cacheDependencies.Add(hashOut);

            result = (from hotel in hotelsByKeys
                      join resort in resorts on hotel.RsKey equals resort.RS_KEY into gj
                      from subHotel in gj.DefaultIfEmpty()
                select new Hotel
                {
                    Key = hotel.Key,
                    Name = hotel.Name,
                    ResortKey = (hotel.RsKey == null ? -1 : hotel.RsKey.Value),
                    ResortName = (subHotel == null ? String.Empty : subHotel.RS_NAME),
                    Url = hotel.Http,
                    Stars = hotel.Stars,
                    IsValidPrice = false
                })
            .OrderBy(h => h.Name)
            .ToList();

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }


        /// <summary>
        /// Возвращает продолжительности в ночах
        /// </summary>
        /// <param name="dc">Контекст базы данных AvalonWeb</param>
        /// <param name="sftDc">Контекст базы данных Sft</param>
        /// <param name="cityKeyFrom">Город вылета</param>
        /// <param name="countryKey">Страна тура</param>
        /// <param name="cityToKeys">Список городв заездов</param>
        /// <param name="tourKeys">Ключи туров</param>
        /// <param name="tourDates">Даты</param>
        /// <returns></returns>
        public static IEnumerable<int> GetTourNights(this MtSearchDbDataContext dc, SftWebDbDataContext sftDc, int cityKeyFrom, int countryKey, IEnumerable<int> cityToKeys, IEnumerable<int> tourKeys, IEnumerable<DateTime> tourDates)
        {
            string hash;
            return GetTourNights(dc, sftDc, cityKeyFrom, countryKey, cityToKeys, tourKeys, tourDates, out hash);
        }

        /// <summary>
        /// Возвращает продолжительности в ночах
        /// </summary>
        /// <param name="dc">Контекст базы данных AvalonWeb</param>
        /// <param name="sftDc">Контекст базы данных Sft</param>
        /// <param name="cityKeyFrom">Город вылета</param>
        /// <param name="countryKey">Страна тура</param>
        /// <param name="cityToKeys">Список городв заездов</param>
        /// <param name="tourKeys">Ключи туров</param>
        /// <param name="tourDates">Даты</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static IEnumerable<int> GetTourNights(this MtSearchDbDataContext dc, SftWebDbDataContext sftDc, int cityKeyFrom, int countryKey, IEnumerable<int> cityToKeys, IEnumerable<int> tourKeys, IEnumerable<DateTime> tourDates, out string hash)
        {
            if (tourKeys == null || !tourKeys.Any())
                throw new ArgumentNullException("tourKeys",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            if (tourDates == null || !tourDates.Any())
                throw new ArgumentNullException("tourDates",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            hash = String.Format("{0}_{1}_{2}_{3}", MethodBase.GetCurrentMethod().Name, cityKeyFrom, countryKey, CacheHelper.GetCacheKeyHashed(new[] { String.Join("_", cityToKeys), String.Join("_", tourKeys), String.Join("_", tourDates) }));

            List<int> result;
            if ((result = CacheHelper.GetCacheItem<List<int>>(hash)) != null)
                return result;

            var cacheDependencies = new List<string>
            {
                String.Format("{0}_{1}_{2}", CacheHelper.DirectionHash, cityKeyFrom, countryKey)
            };
            var globalFilter = GetHotelPansionFilterClasses(sftDc, cityKeyFrom, countryKey, cityToKeys, tourKeys, tourDates);
            //result = globalFilter.GroupBy(r => r.Nights).Select(g => g.Key).OrderBy(n => n).ToList();
            result = globalFilter.Select(r => r.Nights).Distinct().OrderBy(n => n).ToList();

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);

            return result;
        }

        /// <summary>
        /// Возвращает список категорий отелей
        /// </summary>
        /// <param name="dc">Контекст базы данных AvalonWeb</param>
        /// <param name="sftDc">Контекст базы данных Sft</param>
        /// <param name="cityKeyFrom">Город вылета</param>
        /// <param name="countryKey">Страна тура</param>
        /// <param name="cityToKeys">Ключи регионов, передавать только в том случае, если реально выбраны пользователем</param>
        /// <param name="tourKeys">Ключи туров</param>
        /// <param name="tourDates">Даты туров</param>
        /// <param name="tourNights">Список продолжительностей</param>
        /// <returns></returns>
        public static IEnumerable<string> GetTourHotelClasses(this MtSearchDbDataContext dc, SftWebDbDataContext sftDc, int cityKeyFrom, int countryKey, IEnumerable<int> cityToKeys, IEnumerable<int> tourKeys,
            IEnumerable<DateTime> tourDates, IEnumerable<int> tourNights)
        {
            string hash;
            return GetTourHotelClasses(dc, sftDc, cityKeyFrom, countryKey, cityToKeys, tourKeys, tourDates, tourNights, out hash);
        }

        /// <summary>
        /// Возвращает список категорий отелей
        /// </summary>
        /// <param name="dc">Контекст базы данных AvalonWeb</param>
        /// <param name="sftDc">Контекст базы данных Sft</param>
        /// <param name="cityKeyFrom">Город вылета</param>
        /// <param name="countryKey">Страна тура</param>
        /// <param name="cityToKeys">Ключи регионов, передавать только в том случае, если реально выбраны пользователем</param>
        /// <param name="tourKeys">Ключи туров</param>
        /// <param name="tourDates">Даты туров</param>
        /// <param name="tourNights">Список продолжительностей</param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetTourHotelClasses(this MtSearchDbDataContext dc, SftWebDbDataContext sftDc, int cityKeyFrom, int countryKey, IEnumerable<int> cityToKeys, IEnumerable<int> tourKeys,
            IEnumerable<DateTime> tourDates, IEnumerable<int> tourNights, out string hash)
        {
            if (cityToKeys == null || !cityToKeys.Any())
                throw new ArgumentNullException("cityToKeys",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            if (tourKeys == null || !tourKeys.Any())
                throw new ArgumentNullException("tourKeys",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            if (tourDates == null || !tourDates.Any())
                throw new ArgumentNullException("tourDates",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            if (tourNights == null || !tourNights.Any())
                throw new ArgumentNullException("tourNights",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            List<string> result;
            hash = String.Format("{0}_{1}_{2}_{3}", MethodBase.GetCurrentMethod().Name, cityKeyFrom, countryKey, CacheHelper.GetCacheKeyHashed(new[] { String.Join("_", cityToKeys), String.Join("_", tourKeys), String.Join("_", tourDates), String.Join("_", tourNights) }));
            if ((result = CacheHelper.GetCacheItem<List<string>>(hash)) != null)
                return result;

            var cacheDependencies = new List<string>
            {
                String.Format("{0}_{1}_{2}", CacheHelper.DirectionHash, cityKeyFrom, countryKey)
            };
            var hotelKeys = GetHotelPansionFilterClasses(sftDc, cityKeyFrom, countryKey, cityToKeys, tourKeys, tourDates)
                .Where(f => tourNights.Contains(f.Nights))
                .Select(f => f.HotelKey)
                .Distinct()
                .ToList();

            string hashOut;
            result = dc.GetHotelStars(hotelKeys, out hashOut);
            cacheDependencies.Add(hashOut);

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;

        }

        /// <summary>
        /// Возвращает список отелей
        /// </summary>
        /// <param name="dc">Контекст базы данных AvalonWeb</param>
        /// <param name="sftDc">Контекст базы данных Sft</param>
        /// <param name="cityKeyFrom">Город вылета</param>
        /// <param name="countryKey">Страна тура</param>
        /// <param name="cityToKeys">Ключи регионов, передавать только в том случае, если реально выбраны пользователем</param>
        /// <param name="tourKeys">Ключи туров</param>
        /// <param name="tourDates">Даты туров</param>
        /// <param name="tourNights">Список продолжительностей</param>
        /// <param name="hotelCats">Категории отелей, передавать только в том случае, если реально выбраны пользователем</param>
        /// <param name="pansionKeys">Ключи питаний</param>
        /// <param name="showAllHotels">Показывать все отели, на которые есть цены</param>
        /// <returns></returns>
        public static IEnumerable<Hotel> GetTourHotels(this MtSearchDbDataContext dc, SftWebDbDataContext sftDc, int cityKeyFrom, int countryKey, IEnumerable<int> cityToKeys, IEnumerable<int> tourKeys,
            IEnumerable<DateTime> tourDates, IEnumerable<int> tourNights, IEnumerable<string> hotelCats, IEnumerable<int> pansionKeys, bool showAllHotels = false)
        {
            string hash;
            return GetTourHotels(dc, sftDc, cityKeyFrom, countryKey, cityToKeys, tourKeys, tourDates, tourNights, hotelCats, pansionKeys, out hash, showAllHotels);
        }

        /// <summary>
        /// Возвращает список отелей
        /// </summary>
        /// <param name="dc">Контекст базы данных AvalonWeb</param>
        /// <param name="sftDc">Контекст базы данных Sft</param>
        /// <param name="cityKeyFrom">Город вылета</param>
        /// <param name="countryKey">Страна тура</param>
        /// <param name="cityToKeys">Ключи регионов, передавать только в том случае, если реально выбраны пользователем</param>
        /// <param name="tourKeys">Ключи туров</param>
        /// <param name="tourDates">Даты туров</param>
        /// <param name="tourNights">Список продолжительностей</param>
        /// <param name="hotelCats">Категории отелей, передавать только в том случае, если реально выбраны пользователем</param>
        /// <param name="pansionKeys">Ключи питаний</param>
        /// <param name="hash"></param>
        /// <param name="showAllHotels">Показывать все отели, на которые есть цены</param>
        /// <returns></returns>
        public static IEnumerable<Hotel> GetTourHotels(this MtSearchDbDataContext dc, SftWebDbDataContext sftDc, int cityKeyFrom, int countryKey, IEnumerable<int> cityToKeys, IEnumerable<int> tourKeys,
            IEnumerable<DateTime> tourDates, IEnumerable<int> tourNights, IEnumerable<string> hotelCats, IEnumerable<int> pansionKeys, out string hash, bool showAllHotels = false)
        {
            if (cityToKeys == null || !cityToKeys.Any())
                throw new ArgumentNullException("cityToKeys",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            if (tourKeys == null || !tourKeys.Any())
                throw new ArgumentNullException("tourKeys",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            if (tourDates == null || !tourDates.Any())
                throw new ArgumentNullException("tourDates",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            if (tourNights == null || !tourNights.Any())
                throw new ArgumentNullException("tourNights",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            if (hotelCats == null || !hotelCats.Any())
                throw new ArgumentNullException("hotelCats",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            if (pansionKeys == null || !pansionKeys.Any())
                throw new ArgumentNullException("pansionKeys",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            List<Hotel> result;
            hash = String.Format("{0}_{1}_{2}_{3}", MethodBase.GetCurrentMethod().Name, cityKeyFrom, countryKey, CacheHelper.GetCacheKeyHashed(new[] { String.Join("_", cityToKeys), String.Join("_", tourKeys), String.Join("_", tourDates), String.Join("_", tourNights), String.Join("_", hotelCats), String.Join("_", pansionKeys), showAllHotels.ToString() }));
            if ((result = (List<Hotel>)CacheHelper.GetCacheItem<List<Hotel>>(hash)) != null)
                return result;

            var cacheDependencies = new List<string>
            {
                String.Format("{0}_{1}_{2}", CacheHelper.DirectionHash, cityKeyFrom, countryKey)
            };
            var globalFilter = GetHotelPansionFilterClasses(sftDc, cityKeyFrom, countryKey, cityToKeys, tourKeys, tourDates).Where(f => tourNights.Contains(f.Nights) && pansionKeys.Contains(f.PansionKey)).Select(f => f).ToList();

            string hashOut;
            var hotels = dc.GetHotelsByKeys(globalFilter.Select(f => f.HotelKey).Distinct().ToList(), out hashOut).Where(h => hotelCats.Contains(h.Stars)).ToList();
            cacheDependencies.Add(hashOut);

            var resorts = dc.GetResortsByKeys(hotels.Where(h => h.RsKey != null).Select(h => h.RsKey.Value).ToList(), out hashOut);
            cacheDependencies.Add(hashOut);

            var hotelsTrue = (from filter in globalFilter
                              join hotel in hotels on filter.HotelKey equals hotel.Key
                              join resort in resorts on hotel.RsKey equals resort.RS_KEY into gj
                           from subHotel in gj.DefaultIfEmpty()
                           select new Hotel
                           {
                               Key = hotel.Key,
                               Name = hotel.Name,
                               ResortKey = (hotel.RsKey == null ? -1 : hotel.RsKey.Value),
                               ResortName = (subHotel == null ? String.Empty : subHotel.RS_NAME),
                               Url = hotel.Http,
                               Stars = hotel.Stars,
                               IsValidPrice = true
                           }
            )
            .Distinct(new DistinctEqualityComparer())
            .OrderBy(h => h.Name)
            .ToList();

            cacheDependencies.Add(HotelsExtension.TableName);
            cacheDependencies.Add(ResortsExtension.TableName);

            //if (hotelsTrue.Count == 0)
            //    throw new ArgumentNullException(MethodBase.GetCurrentMethod().Name,
            //        "Метод по запросу не вернул ни одного элемента: " + String.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}_{8}_{9}_{10}", MethodBase.GetCurrentMethod().Name, CacheHelper.DirectionHash, cityKeyFrom, countryKey, String.Join("_", cityToKeys), String.Join("_", tourKeys), String.Join("_", tourDates), String.Join("_", tourNights), String.Join("_", hotelCats), String.Join("_", pansionKeys), showAllHotels));
            
            if (!showAllHotels)
            {
                CacheHelper.AddCacheData(hash, hotelsTrue, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
                return hotelsTrue;
            }

            var hotelsFalse = GetAllFilterHotels(dc, sftDc, cityKeyFrom, countryKey, cityToKeys, tourKeys, hotelCats,
                pansionKeys, hotelsTrue.Select(h => h.Key).ToList());
            result = hotelsTrue.Union(hotelsFalse).OrderBy(h => h.Name).ToList();
            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Возвращает список питаний
        /// </summary>
        /// <param name="dc">Контекст базы данных AvalonWeb</param>
        /// <param name="sftDc">Контекст базы данных Sft</param>
        /// <param name="cityKeyFrom">Город вылета</param>
        /// <param name="countryKey">Страна тура</param>
        /// <param name="cityToKeys">Ключи регионов</param>
        /// <param name="tourKeys">Ключи туров</param>
        /// <param name="tourDates">Даты туров</param>
        /// <param name="tourNights">Продолжительности туров</param>
        /// <param name="hotelCats">Категории отелей</param>
        /// <returns></returns>
        public static IDictionary<int, string> GetTourPansions(this MtSearchDbDataContext dc, SftWebDbDataContext sftDc, int cityKeyFrom, int countryKey, IEnumerable<int> cityToKeys, IEnumerable<int> tourKeys, IEnumerable<DateTime> tourDates, IEnumerable<int> tourNights, IEnumerable<string> hotelCats)
        {
            string hash;
            return GetTourPansions(dc, sftDc, cityKeyFrom, countryKey, cityToKeys, tourKeys, tourDates, tourNights, hotelCats, out hash);
        }

        /// <summary>
        /// Возвращает список питаний
        /// </summary>
        /// <param name="dc">Контекст базы данных AvalonWeb</param>
        /// <param name="sftDc">Контекст базы данных Sft</param>
        /// <param name="cityKeyFrom">Город вылета</param>
        /// <param name="countryKey">Страна тура</param>
        /// <param name="cityToKeys">Ключи регионов</param>
        /// <param name="tourKeys">Ключи туров</param>
        /// <param name="tourDates">Даты туров</param>
        /// <param name="tourNights">Продолжительности туров</param>
        /// <param name="hotelCats">Категории отелей</param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static IDictionary<int, string> GetTourPansions(this MtSearchDbDataContext dc, SftWebDbDataContext sftDc, int cityKeyFrom, int countryKey, IEnumerable<int> cityToKeys, IEnumerable<int> tourKeys, IEnumerable<DateTime> tourDates, IEnumerable<int> tourNights, IEnumerable<string> hotelCats, out string hash)
        {
            if (cityToKeys == null || !cityToKeys.Any())
                throw new ArgumentNullException("cityToKeys",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            if (tourKeys == null || !tourKeys.Any())
                throw new ArgumentNullException("tourKeys",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            if (tourDates == null || !tourDates.Any())
                throw new ArgumentNullException("tourDates",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            if (tourNights == null || !tourNights.Any())
                throw new ArgumentNullException("tourNights",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            if (hotelCats == null || !hotelCats.Any())
                throw new ArgumentNullException("hotelCats",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            Dictionary<int, string> result;
            hash = String.Format("{0}_{1}_{2}_{3}", MethodBase.GetCurrentMethod().Name, cityKeyFrom, countryKey, CacheHelper.GetCacheKeyHashed(new[] { String.Join("_", cityToKeys), String.Join("_", tourKeys), String.Join("_", tourDates), String.Join("_", tourNights), String.Join("_", hotelCats) }));
            if ((result = CacheHelper.GetCacheItem<Dictionary<int, string>>(hash)) != null)
                return result;

            var cacheDependencies = new List<string>
            {
                String.Format("{0}_{1}_{2}", CacheHelper.DirectionHash, cityKeyFrom, countryKey)
            };
            var globalFilter = GetHotelPansionFilterClasses(sftDc, cityKeyFrom, countryKey, cityToKeys, tourKeys, tourDates).Where(f => tourNights.Contains(f.Nights)).ToList();
            string hashOut;
            var hotels = dc.GetHotelKeysStars(globalFilter.Select(f => f.HotelKey).Distinct().ToList(), out hashOut);
            cacheDependencies.Add(hashOut);

            result = (from filter in globalFilter
                join pansion in dc.GetAllPansions() on filter.PansionKey equals pansion.PN_KEY
                      join hotel in hotels on filter.HotelKey equals hotel.Item1
                where hotelCats.Contains(hotel.Item2)
                select new { Key = filter.PansionKey, Name = pansion.PN_NAME }
                )
                .Distinct()
                .OrderBy(p => p.Name)
                .ToDictionary(p => p.Key, p => p.Name);

            cacheDependencies.Add(PansionsExtension.TableName);

            //if (!resultLocal.Any())
            //    throw new ArgumentNullException(MethodBase.GetCurrentMethod().Name,
            //        "Метод по запросу не вернул ни одного элемента: " + String.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}_{8}", MethodBase.GetCurrentMethod().Name, CacheHelper.DirectionHash, cityKeyFrom, countryKey, String.Join("_", cityToKeys), String.Join("_", tourKeys), String.Join("_", tourDates), String.Join("_", tourNights), String.Join("_", hotelCats)));

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Получает список комнат для направлений с ценой за человека
        /// </summary>
        /// <param name="dc">Контекст базы данных AvalonWeb</param>
        /// <param name="sftDc">Контекст базы данных Sft</param>
        /// <param name="cityKeyFrom">Город вылета</param>
        /// <param name="countryKey">Страна тура</param>
        /// <param name="cityToKeys">Ключ городов прилета</param>
        /// <param name="tourKeys">Ключи туров</param>
        /// <param name="tourDates">Даты заездов</param>
        /// <param name="tourNights">Продолжительности туров в ночах</param>
        /// <param name="hotelKeys">Ключи отелей</param>
        /// <param name="pansionKeys">Ключи питаний</param>
        /// <returns></returns>
        public static IDictionary<int, string> GetTourRooms(this MtSearchDbDataContext dc, SftWebDbDataContext sftDc,
            int cityKeyFrom, int countryKey, 
            IEnumerable<int> cityToKeys, IEnumerable<int> tourKeys, IEnumerable<DateTime> tourDates,
            IEnumerable<int> tourNights, IEnumerable<int> hotelKeys, IEnumerable<int> pansionKeys)
        {
            if (cityToKeys == null || !cityToKeys.Any())
                throw new ArgumentNullException("cityToKeys",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            if (tourKeys == null || !tourKeys.Any())
                throw new ArgumentNullException("tourKeys",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            if (tourDates == null || !tourDates.Any())
                throw new ArgumentNullException("tourDates",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            if (tourNights == null || !tourNights.Any())
                throw new ArgumentNullException("tourNights",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            if (hotelKeys == null || !hotelKeys.Any())
                throw new ArgumentNullException("hotelKeys",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            if (pansionKeys == null || !pansionKeys.Any())
                throw new ArgumentNullException("pansionKeys",
                    "Если данный параметр null или не содержит элементов, значит у вас ошибка вызова или получения данных.");

            Dictionary<int, string> result;

            var hash = String.Format("{0}_{1}_{2}_{3}", MethodBase.GetCurrentMethod().Name, cityKeyFrom, countryKey, CacheHelper.GetCacheKeyHashed(new[] { String.Join("_", cityToKeys), String.Join("_", tourKeys), String.Join("_", tourDates), String.Join("_", tourNights), String.Join("_", hotelKeys), String.Join("_", pansionKeys) }));

            if ((result = CacheHelper.GetCacheItem<Dictionary<int, string>>(hash)) != null) return result;

            var cacheDependencies = new List<string>
            {
                String.Format("{0}_{1}_{2}", CacheHelper.DirectionHash, cityKeyFrom, countryKey)
            };
            var commandBuilder = new StringBuilder();
            commandBuilder.AppendLine("select distinct rmkey ");
            commandBuilder.AppendLine(String.Format("from SearchFilter_{0}_{1} ", countryKey, cityKeyFrom));
            commandBuilder.AppendLine("where hdkey in (" + string.Join(",", hotelKeys) + ") ");
            commandBuilder.AppendLine("and pnkey in (" + string.Join(",", pansionKeys) + ") ");
            commandBuilder.AppendLine("and tourkey in (" + string.Join(",", tourKeys) + ") ");
            commandBuilder.AppendLine("and tourdate in (");
            foreach (var dateString in tourDates.Select(d => d.ToString("yyyy-MM-dd")))
            {
                commandBuilder.AppendFormat("'{0}',", dateString);
            }
            commandBuilder = commandBuilder.Remove(commandBuilder.Length - 1, 1);
            commandBuilder.Append(") ");

            commandBuilder.AppendLine("and nights in (" + string.Join(",", tourNights) + ") ");
            commandBuilder.AppendLine("and ctkey in (" + string.Join(",", cityToKeys) + ") ");
            commandBuilder.AppendLine("and pnkey in (" + string.Join(",", pansionKeys) + ") ");
            commandBuilder.AppendLine("and rmkey is not null ");

            var keys = new List<int>();
            using (var command = sftDc.Connection.CreateCommand())
            {
                command.CommandText = commandBuilder.ToString();

                sftDc.Connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        keys.Add(reader.GetInt32(0));
                    }
                }
                sftDc.Connection.Close();
            }

            string hashOut;
            result = (from rooms in dc.GetRoomsByKeys(keys, out hashOut)
                      select new { Key = rooms.RM_KEY, Name = rooms.RM_NAME })
                .OrderBy(r => r.Name)
                .ToDictionary(r => r.Key, r => r.Name);

            cacheDependencies.Add(hashOut);

            if (result.Count == 0)
                throw new ArgumentNullException(MethodBase.GetCurrentMethod().Name,
                    "Метод по запросу не вернул ни одного элемента: " + commandBuilder);

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Метод возвращает список всех валют, в которых посчитаны туры
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static IDictionary<int, string> GetCurrencies(this MtSearchDbDataContext dc)
        {
            Dictionary<int, string> result;

            var hash = MethodBase.GetCurrentMethod().Name;
            if ((result = CacheHelper.GetCacheItem<Dictionary<int, string>>(hash)) != null) return result;

            result = new Dictionary<int, string>();

            var commandBuilder = new StringBuilder();
            commandBuilder.Append("SELECT DISTINCT [ra_key], [RA_CODE] ");
            commandBuilder.Append("FROM [dbo].[TP_Tours] ");
            commandBuilder.Append(
                "INNER JOIN [dbo].[Rates] ON [TO_Rate] = [RA_CODE] COLLATE SQL_Latin1_General_CP1251_CI_AS ");
            commandBuilder.Append("WHERE TO_IsEnabled = 1 ");
            commandBuilder.Append("ORDER BY [RA_CODE]");

            using (var command = dc.Connection.CreateCommand())
            {
                command.CommandText = commandBuilder.ToString();
                dc.Connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetInt32(0), reader.GetString(1));
                    }
                }
                dc.Connection.Close();
            }

            if (result.Count == 0)
                throw new ArgumentNullException(MethodBase.GetCurrentMethod().Name,
                    "Метод по запросу не вернул ни одного элемента: " + commandBuilder);

            CacheHelper.AddCacheData(hash, result, null, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        
        /// <summary>
        /// Список прайсов с описанием
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static IList<Tuple<int, bool>> GetTourListWithDescriptions(this MtSearchDbDataContext dc, IEnumerable<int> tourKeys, out string hash)
        {
            List<Tuple<int, bool>> result;

            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, String.Join(",", tourKeys));
            if ((result = CacheHelper.GetCacheItem<List<Tuple<int, bool>>>(hash)) != null) return result;

            string hashOut;
            var cacheDependencies = new List<string>();
            var tpTours = dc.GetTPToursByKeys(tourKeys, out hashOut);
            cacheDependencies.Add(hashOut);

            var tlTours = dc.GetTurListsByKeys(tpTours.Select(t => t.TO_TRKey).Distinct().ToList(), out hashOut);
            cacheDependencies.Add(hashOut);

            result = (from t in tpTours
                      join tl in tlTours on t.TO_TRKey equals tl.TL_KEY
                select new Tuple<int, bool>(t.TO_Key, !string.IsNullOrEmpty(tl.TL_DESCRIPTION)))
                .ToList();

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.MediumCacheTimeout);
            return result;
        }

        /// <summary>
        /// Описание по туру
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="tourKey">Ключ рассчитанного тура (to_key из таблицы tp_tours)</param>
        /// <returns></returns>
        public static string GetTourDescription(this MtSearchDbDataContext dc, int tourKey)
        {
            string result = String.Empty;
            var hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, tourKey);
            if (CacheHelper.IsCacheKeyExists(hash))
                return CacheHelper.GetCacheItem<string>(hash);

            string hashOut;
            var cacheDependencies = new List<string>();
            var tpTours = dc.GetTPToursByKeys(new [] { tourKey }, out hashOut);
            cacheDependencies.Add(hashOut);
            if (tpTours != null && tpTours.Count == 1)
            {
                var turList = dc.GetTurListByKey(tpTours[0].TO_TRKey, out hashOut);
                cacheDependencies.Add(hashOut);
                if (turList != null)
                {
                    result = turList.TL_DESCRIPTION;
                }
            }

            if (result == null)
                result = String.Empty;

            CacheHelper.AddCacheData(hash, result, null, Globals.Settings.Cache.MediumCacheTimeout);
            return result;
        }
    }
}
