using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Caching;
using QDSearch.Extensions;
using QDSearch.DataModel;
using QDSearch.Helpers;
using System.Globalization;
using System.Data;
using NLog;
using System.Diagnostics;
using QDSearch.Repository.MtMain;

namespace QDSearch.Repository.MtSearch
{
    /// <summary>
    /// Классs для работы с поиском
    /// </summary>
    public static class PriceSearchMethods
    {
        private const int RowsFromDbPortion = 100;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Поиск данных по конкретной цене
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="mainDc">Контекст основой БД</param>
        /// <param name="cityKeyFrom">Город вылета</param>
        /// <param name="countryKey">Страна прилета</param>
        /// <param name="priceKey">Ключ цены</param>
        /// <returns></returns>
        public static SearchResult PagingOnClient(this MtSearchDbDataContext dc, MtMainDbDataContext mainDc, int cityKeyFrom, int countryKey, long priceKey)
        {
            string hash;
            return PagingOnClient(dc, mainDc, cityKeyFrom, countryKey, priceKey, out hash);
        }

        /// <summary>
        /// Поиск данных по конкретной цене
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="mainDc">Контекст основной БД</param>
        /// <param name="cityKeyFrom">Город вылета</param>
        /// <param name="countryKey">Страна прилета</param>
        /// <param name="priceKey">Ключ цены</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static SearchResult PagingOnClient(this MtSearchDbDataContext dc, MtMainDbDataContext mainDc, int cityKeyFrom, int countryKey, long priceKey, out string hash)
        {
            SearchResult result;
            hash = String.Format("{0}_{1}_{2}_{3}", MethodBase.GetCurrentMethod().Name, cityKeyFrom, countryKey, priceKey);
            if ((result = CacheHelper.GetCacheItem<SearchResult>(hash)) != null) return result;

            // первоначальная инициализация
            result = new SearchResult
            {
                SortType = new Tuple<SortingColumn, SortingDirection>(SortingColumn.Price, SortingDirection.Asc),
                IsMorePages = false, // в дальнейшем страницы могут быть
                SearchItems = new List<SearchResultItem>(1)
            };
            var cacheDependencies = new List<string>
            {
                String.Format("{0}_{1}_{2}", CacheHelper.DirectionHash, cityKeyFrom, countryKey)
            };

            string hashOut;
            var resultItems = GetPriceItemsFromDb(dc, cityKeyFrom, countryKey, priceKey, out hashOut);
            cacheDependencies.Add(hashOut);

            foreach (var item in resultItems)
            {
                Dictionary<int, FlightPlainInfo> directCharterResult;
                if (item.FlightCalcInfo.CharterKey != null)
                {
                    directCharterResult = dc.CheckCharterQuota(mainDc, item.FlightCalcInfo.CharterKey.Value, item.FlightCityKeyFrom.Value, item.FlightCityKeyTo.Value,
                        item.FlightCalcInfo.CharterDay.Value,
                        item.FlightCalcInfo.CharterPartnerKey.Value, null, item.FlightCalcInfo.CharterPacketKey.Value, item.Date,
                        item.FlightCalcInfo.BackCharterDay, item.FlightCalcInfo.FindDirectFlight, QuotesStates.Availiable | QuotesStates.Request | QuotesStates.No, item.MainPlacesCount,
                        Globals.Settings.CharterClassesDictionary, out hashOut);

                    if (!cacheDependencies.Contains(hashOut))
                        cacheDependencies.Add(hashOut);
                }
                else
                {
                    directCharterResult = Globals.Settings.CharterClassesDictionary.Keys.ToDictionary(key => key, key => new FlightPlainInfo());
                }

                Dictionary<int, FlightPlainInfo> backCharterResult;
                if (item.FlightCalcInfo.BackCharterKey != null)
                {
                    backCharterResult = dc.CheckCharterQuota(mainDc, item.FlightCalcInfo.BackCharterKey.Value, item.BackFlightCityKeyFrom.Value, item.BackFlightCityKeyTo.Value,
                        item.FlightCalcInfo.BackCharterDay.Value,
                        item.FlightCalcInfo.BackCharterPartnerKey.Value, null, item.FlightCalcInfo.BackCharterPacketKey.Value, item.Date,
                        item.FlightCalcInfo.CharterDay, item.FlightCalcInfo.FindBackFlight, QuotesStates.Availiable | QuotesStates.Request | QuotesStates.No, item.MainPlacesCount,
                        Globals.Settings.CharterClassesDictionary, out hashOut);

                    if (!cacheDependencies.Contains(hashOut))
                        cacheDependencies.Add(hashOut);
                }
                else
                {
                    backCharterResult = Globals.Settings.CharterClassesDictionary.Keys.ToDictionary(key => key, key => new FlightPlainInfo());
                }

                foreach (var hotel in item.Hotels)
                {
                    hotel.QuotaState = dc.CheckServiceQuota(ServiceClass.Hotel, hotel.Key, hotel.RoomKey,
                        hotel.RoomCategoryKey, hotel.PartnerKey, null, hotel.DateFrom,
                        hotel.DateFrom.AddDays(hotel.NightsCount - 1), out hashOut);

                    if (!cacheDependencies.Contains(hashOut))
                        cacheDependencies.Add(hashOut);
                }

                item.DirectFlightsInfo = directCharterResult;
                item.BackFlightsInfo = backCharterResult;

                result.SearchItems.Add(item);
            }

            // заполняем только страницу для вывода информацией
            dc.FillResultItemsData(ref result.SearchItems);

            //зависимости для кэша - строки
            cacheDependencies.AddRange(new[]
            {
                CitiesExtension.TableName,
                HotelRoomsExtension.TableName,
                PansionsExtension.TableName,
                AccomodationsExtension.TableName,
                RoomsExtension.TableName,
                RoomCategoriesExtension.TableName,
                HotelsExtension.TableName,
                RealCoursesExtension.TableName
            });

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Метод поиска данных
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="mainDc">Контекст основной базы данны</param>
        /// <param name="cityKeyFrom">Ключ города начала поездки</param>
        /// <param name="countryKey">Ключ страны тура</param>
        /// <param name="tourKeys">Ключи туров</param>
        /// <param name="tourDates">Даты заездов</param>
        /// <param name="tourNights">Продолжительности туров в ночах</param>
        /// <param name="hotelKeys">Ключи отеле</param>
        /// <param name="pansionKeys">Ключи питаний</param>
        /// <param name="mainPlaces">Число основных мест (для стран за номер)</param>
        /// <param name="addPlaces">Число дополнительных мест (для стран за номер)</param>
        /// <param name="firstChildYears">Возраст первого ребенка (для стран за номер)</param>
        /// <param name="secondChildYears">Возраст второго ребенка (для стран за номер)</param>
        /// <param name="roomTypeKeys">Ключи типа номера (для стран за человека)</param>
        /// <param name="hotelQuotaMask">Маска квот для отелей</param>
        /// <param name="aviaQuotaMask">Маска квот для перелетов</param>
        /// <param name="rateKey">Ключ валюты</param>
        /// <param name="maxTourPrice">Максимальная стоимость тура</param>
        /// <param name="rowsPerPage">Число строк на странице</param>
        /// <param name="rowCounterFrom">Номер строки, с которой начинаем поиск</param>
        /// <param name="sortType">Колонка и направление сортировки</param>
        /// <returns></returns>
        public static SearchResult PagingOnClient(this MtSearchDbDataContext dc, MtMainDbDataContext mainDc, int cityKeyFrom, int countryKey,
            IEnumerable<int> tourKeys, IEnumerable<DateTime> tourDates, IEnumerable<int> tourNights,
            IEnumerable<int> hotelKeys, IEnumerable<int> pansionKeys, ushort? mainPlaces, ushort? addPlaces,
            ushort? firstChildYears, ushort? secondChildYears, IEnumerable<int> roomTypeKeys,
            QuotesStates hotelQuotaMask, QuotesStates aviaQuotaMask, int rateKey, uint? maxTourPrice, ushort rowsPerPage,
            uint rowCounterFrom, Tuple<SortingColumn, SortingDirection> sortType)
        {
            string hash;
            return PagingOnClient(dc, mainDc, cityKeyFrom, countryKey, tourKeys, tourDates, tourNights, hotelKeys, pansionKeys, mainPlaces, addPlaces, firstChildYears, secondChildYears, roomTypeKeys, hotelQuotaMask, aviaQuotaMask, rateKey, null, maxTourPrice, rowsPerPage, rowCounterFrom, sortType, out hash);
        }

        /// <summary>
        /// Метод поиска данных
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="mainDc">Контекст основной БД</param>
        /// <param name="cityKeyFrom">Ключ города начала поездки</param>
        /// <param name="countryKey">Ключ страны тура</param>
        /// <param name="tourKeys">Ключи туров</param>
        /// <param name="tourDates">Даты заездов</param>
        /// <param name="tourNights">Продолжительности туров в ночах</param>
        /// <param name="hotelKeys">Ключи отеле</param>
        /// <param name="pansionKeys">Ключи питаний</param>
        /// <param name="mainPlaces">Число основных мест (для стран за номер)</param>
        /// <param name="addPlaces">Число дополнительных мест (для стран за номер)</param>
        /// <param name="firstChildYears">Возраст первого ребенка (для стран за номер)</param>
        /// <param name="secondChildYears">Возраст второго ребенка (для стран за номер)</param>
        /// <param name="roomTypeKeys">Ключи типа номера (для стран за человека)</param>
        /// <param name="hotelQuotaMask">Маска квот для отелей</param>
        /// <param name="aviaQuotaMask">Маска квот для перелетов</param>
        /// <param name="rateKey">Ключ валюты</param>
        /// <param name="minTourPrice">Минимальная стоимость тура</param>
        /// <param name="maxTourPrice">Максимальная стоимость тура</param>
        /// <param name="rowsPerPage">Число строк на странице</param>
        /// <param name="rowCounterFrom">Номер строки, с которой начинаем поиск</param>
        /// <param name="sortType">Колонка и направление сортировки</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static SearchResult PagingOnClient(this MtSearchDbDataContext dc, MtMainDbDataContext mainDc, int cityKeyFrom, int countryKey,
            IEnumerable<int> tourKeys, IEnumerable<DateTime> tourDates, IEnumerable<int> tourNights,
            IEnumerable<int> hotelKeys, IEnumerable<int> pansionKeys, ushort? mainPlaces, ushort? addPlaces,
            ushort? firstChildYears, ushort? secondChildYears, IEnumerable<int> roomTypeKeys,
            QuotesStates hotelQuotaMask, QuotesStates aviaQuotaMask, int rateKey, uint? minTourPrice, uint? maxTourPrice, ushort rowsPerPage,
            uint rowCounterFrom, Tuple<SortingColumn, SortingDirection> sortType, out string hash)
        {
            Stopwatch sw = Stopwatch.StartNew();

            hash = ValidatePagingParams(cityKeyFrom, countryKey, tourKeys, tourDates, tourNights, hotelKeys,
                pansionKeys, mainPlaces, addPlaces, firstChildYears, secondChildYears, roomTypeKeys, hotelQuotaMask,
                aviaQuotaMask, rateKey, minTourPrice, maxTourPrice, rowsPerPage, rowCounterFrom, sortType);

            SearchResult result;
            if ((result = CacheHelper.GetCacheItem<SearchResult>(hash)) != null) return result;

            // первоначальная инициализация
            result = new SearchResult
            {
                SortType = sortType,
                IsMorePages = false, // в дальнейшем страницы могут быть
                SearchItems = new List<SearchResultItem>(rowsPerPage)
            };

            // не подошедшие параметры перелетов
            // charterKey, charterDay, partnerKey, packetKey, tourDate, linkedDay, findFlight
            var wrongCharterParams = new List<Tuple<int, int, int, int, DateTime, int?, short>> ();
            var wrongBackCharterParams = new List<Tuple<int, int, int, int, DateTime, int?, short>>(); 

            //Direction
            var cacheDependencies = new List<string>
            {
                String.Format("{0}_{1}_{2}", CacheHelper.DirectionHash, cityKeyFrom, countryKey)
            };
            uint currentRow = 0;
            var searchInBdRowFrom = (int) rowCounterFrom/RowsFromDbPortion*RowsFromDbPortion;

            while (true)
            {
                string cacheDep;
                var resultItems = GetPriceItemsFromDb(dc, cityKeyFrom, countryKey, tourKeys, tourDates, tourNights,
                hotelKeys, pansionKeys, mainPlaces, addPlaces, firstChildYears, secondChildYears, roomTypeKeys, rateKey,
                minTourPrice, maxTourPrice, sortType, (uint)searchInBdRowFrom, (uint)searchInBdRowFrom + RowsFromDbPortion, aviaQuotaMask, hotelQuotaMask, 
                wrongCharterParams, wrongBackCharterParams,
                out cacheDep);
                cacheDependencies.Add(cacheDep);

                // не нашли больше записей в БД, страница скорее всего не заполнена полностью, но все-равно выходим
                if (!resultItems.Any())
                    break;

                foreach (var item in resultItems.Skip((int)rowCounterFrom - searchInBdRowFrom))
                {
                    currentRow++;
                    Dictionary<int, FlightPlainInfo> directCharterResult;
                    var fitedDirectCharterKeys = new List<int>();
                    if (item.FlightCalcInfo.CharterKey != null)
                    {
                        directCharterResult = dc.CheckCharterQuota(mainDc, item.FlightCalcInfo.CharterKey.Value, item.FlightCityKeyFrom.Value, item.FlightCityKeyTo.Value,
                            item.FlightCalcInfo.CharterDay.Value,
                            item.FlightCalcInfo.CharterPartnerKey.Value, null, item.FlightCalcInfo.CharterPacketKey.Value, item.Date,
                            item.FlightCalcInfo.BackCharterDay, item.FlightCalcInfo.FindDirectFlight, aviaQuotaMask, item.MainPlacesCount,
                            Globals.Settings.CharterClassesDictionary, out cacheDep);

                        if (!cacheDependencies.Contains(cacheDep))
                            cacheDependencies.Add(cacheDep);

                        fitedDirectCharterKeys.AddRange(directCharterResult.Keys.ToList());

                        //fitedDirectCharterKeys.AddRange(from flightClassNum in Globals.Settings.CharterClassesDictionary.Keys let flightInfo = directCharterResult[flightClassNum] let quota = flightInfo.QuotaState.QuotaState == QuotesStates.Small ? QuotesStates.Availiable : flightInfo.QuotaState.QuotaState where (quota & aviaQuotaMask) == quota select flightClassNum);
                    }
                    else
                    {
                        //если перелетов в цене нет, мы выводим эту строку вне зависимости от статуса перелетов
                        directCharterResult = new Dictionary<int, FlightPlainInfo>();
                        fitedDirectCharterKeys.AddRange(Globals.Settings.CharterClassesDictionary.Keys);
                    }

                    //прямые перелеты нам не подошли
                    if (fitedDirectCharterKeys.Count == 0)
                    {
                        if (!wrongCharterParams.Exists(w => w.Item1 == item.FlightCalcInfo.CharterKey.Value
                                                            && w.Item2 == item.FlightCalcInfo.CharterDay.Value
                                                            && w.Item3 == item.FlightCalcInfo.CharterPartnerKey.Value
                                                            && w.Item4 == item.FlightCalcInfo.CharterPacketKey.Value
                                                            && w.Item5 == item.Date
                                                            && w.Item6 == item.FlightCalcInfo.BackCharterDay
                                                            && w.Item7 ==
                                                            (item.FlightCalcInfo.FindDirectFlight
                                                                ? (short) ServiceAttribute.CodeEdit
                                                                : (short) ServiceAttribute.None)
                            ))
                        {
                            // charterKey, charterDay, partnerKey, packetKey, tourDate, linkedDay, findFlight
                            wrongCharterParams.Add(new Tuple<int, int, int, int, DateTime, int?, short>(
                                item.FlightCalcInfo.CharterKey.Value,
                                item.FlightCalcInfo.CharterDay.Value,
                                item.FlightCalcInfo.CharterPartnerKey.Value,
                                item.FlightCalcInfo.CharterPacketKey.Value,
                                item.Date,
                                item.FlightCalcInfo.BackCharterDay,
                                item.FlightCalcInfo.FindDirectFlight ? (short)ServiceAttribute.CodeEdit : (short)ServiceAttribute.None));
                        }
                    }
                        

                    Dictionary<int, FlightPlainInfo> backCharterResult;
                    var fitedBackCharterKeys = new List<int>();
                    if (item.FlightCalcInfo.BackCharterKey != null)
                    {
                        backCharterResult = dc.CheckCharterQuota(mainDc, item.FlightCalcInfo.BackCharterKey.Value, item.BackFlightCityKeyFrom.Value, item.BackFlightCityKeyTo.Value,
                            item.FlightCalcInfo.BackCharterDay.Value,
                            item.FlightCalcInfo.BackCharterPartnerKey.Value, null, item.FlightCalcInfo.BackCharterPacketKey.Value, item.Date,
                            item.FlightCalcInfo.CharterDay, item.FlightCalcInfo.FindBackFlight, aviaQuotaMask, item.MainPlacesCount,
                            Globals.Settings.CharterClassesDictionary, out cacheDep);

                        if (!cacheDependencies.Contains(cacheDep))
                            cacheDependencies.Add(cacheDep);

                        fitedBackCharterKeys.AddRange(backCharterResult.Keys.ToList());

                        //fitedBackCharterKeys.AddRange(from flightClassNum in Globals.Settings.CharterClassesDictionary.Keys let flightInfo = backCharterResult[flightClassNum] let quota = flightInfo.QuotaState.QuotaState == QuotesStates.Small ? QuotesStates.Availiable : flightInfo.QuotaState.QuotaState where (quota & aviaQuotaMask) == quota select flightClassNum);
                    }
                    else
                    {
                        //если перелетов в цене нет, мы выводим эту строку вне зависимости от статуса перелетов
                        backCharterResult = new Dictionary<int, FlightPlainInfo>();
                        fitedBackCharterKeys.AddRange(Globals.Settings.CharterClassesDictionary.Keys);
                    }

                    //обратные перелеты нам не подошли
                    if (fitedBackCharterKeys.Count == 0)
                    {
                        if (!wrongBackCharterParams.Exists(w => w.Item1 == item.FlightCalcInfo.BackCharterKey.Value
                                                            && w.Item2 == item.FlightCalcInfo.BackCharterDay.Value
                                                            && w.Item3 == item.FlightCalcInfo.BackCharterPartnerKey.Value
                                                            && w.Item4 == item.FlightCalcInfo.BackCharterPacketKey.Value
                                                            && w.Item5 == item.Date
                                                            && w.Item6 == item.FlightCalcInfo.CharterDay
                                                            && w.Item7 ==
                                                            (item.FlightCalcInfo.FindBackFlight
                                                                ? (short) ServiceAttribute.CodeEdit
                                                                : (short) ServiceAttribute.None)
                            ))
                        {
                            // charterKey, charterDay, partnerKey, packetKey, tourDate, linkedDay, findFlight
                            wrongBackCharterParams.Add(new Tuple<int, int, int, int, DateTime, int?, short>(
                                item.FlightCalcInfo.BackCharterKey.Value,
                                item.FlightCalcInfo.BackCharterDay.Value,
                                item.FlightCalcInfo.BackCharterPartnerKey.Value,
                                item.FlightCalcInfo.BackCharterPacketKey.Value,
                                item.Date,
                                item.FlightCalcInfo.CharterDay,
                                item.FlightCalcInfo.FindBackFlight ? (short)ServiceAttribute.CodeEdit : (short)ServiceAttribute.None));
                        }
                    }

                    if (!fitedDirectCharterKeys.Any() || !fitedBackCharterKeys.Any())
                        continue;

                    var isHotelsQuotaOk = true;
                    foreach (var hotel in item.Hotels)
                    {
                        var hotelQuotaResult = dc.CheckServiceQuota(ServiceClass.Hotel, hotel.Key, hotel.RoomKey,
                            hotel.RoomCategoryKey, hotel.PartnerKey, null, hotel.DateFrom,
                            hotel.DateFrom.AddDays(hotel.NightsCount - 1), out cacheDep);

                        if (!cacheDependencies.Contains(cacheDep))
                            cacheDependencies.Add(cacheDep);

                        var tempQuotaState = hotelQuotaResult.QuotaState == QuotesStates.Small
                            ? QuotesStates.Availiable
                            : hotelQuotaResult.QuotaState;

                        if ((hotelQuotaMask & tempQuotaState) == 0)
                        {
                            isHotelsQuotaOk = false;
                            break;
                        }

                        hotel.QuotaState = hotelQuotaResult;
                    }

                    if (!isHotelsQuotaOk)
                        continue;

                    // такое может происходить при запросе из сервисов. Из онлайна приходит либо одно, либо другое
                    //todo: доделать для расскажки детей
                    //if (roomTypeKeys != null && roomTypeKeys.Any() && addPlaces != null)
                    //{
                    //    foreach (var hotel in item.Hotels)
                    //    {
                    //        var packetKey = mainDc.GetHotelPacketKey(item.PriceKey, hotel.Key, out cacheDep);
                    //        if (!cacheDependencies.Contains(cacheDep))
                    //            cacheDependencies.Add(cacheDep);

                    //        var subCodes1 = mainDc.GetHotelRoomsFromCosts(ServiceClass.Hotel, hotel.Key, item.Date, packetKey, hotel.PartnerKey,
                    //            out cacheDep);
                    //        if (!cacheDependencies.Contains(cacheDep))
                    //            cacheDependencies.Add(cacheDep);

                    //        var accmdKeys = dc.GetHotelRoomByKeys(subCodes1).Where(h => roomTypeKeys.Contains(h.HR_RMKEY)).Select(h => h.HR_ACKEY).Distinct().ToList();
                    //    }
                    //}

                    item.DirectFlightsInfo = directCharterResult;
                    item.BackFlightsInfo = backCharterResult;

                    if (result.SearchItems.Count < rowsPerPage)
                    {
                        result.SearchItems.Add(item);
                    }
                    else
                    {
                        result.IsMorePages = true;
                        result.NextPageRowCounter = rowCounterFrom + currentRow - 1;
                        break;
                    }
                }

                // собрали всю страницу полность, надо выходить
                if (result.IsMorePages)
                    break;

                searchInBdRowFrom = searchInBdRowFrom + RowsFromDbPortion;
            }

            // заполняем только страницу для вывода информацией
            dc.FillResultItemsData(ref result.SearchItems);

            //зависимости для кэша - строки
            cacheDependencies.AddRange(new[]
            {
                CitiesExtension.TableName,
                HotelRoomsExtension.TableName,
                PansionsExtension.TableName,
                AccomodationsExtension.TableName,
                RoomsExtension.TableName,
                RoomCategoriesExtension.TableName,
                HotelsExtension.TableName,
                RealCoursesExtension.TableName
            });

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);

            sw.Stop();
            var ts = sw.Elapsed;
            logger.Trace(String.Format("Время поиска {0:00}.{1:00}", ts.Seconds, ts.Milliseconds/10));

            return result;
        }

        /// <summary>
        /// Метод по заполнению текстовых данных в выходной результат поиска. Применяется только для записей, которые были найдены
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="resultItems">Список найденных записей</param>
        public static void FillResultItemsData(this MtSearchDbDataContext dc, ref List<SearchResultItem> resultItems)
        {
            string hashOut;
            var hotels = dc.GetHotelsByKeys(resultItems.SelectMany(r => r.Hotels).Select(h => h.Key).Distinct().ToList(), out hashOut);

            var cityKeys = hotels.Select(h => h.CtKey).Distinct().ToList();
            var cities = dc.GetCitiesByKeys(cityKeys, out hashOut);

            var pansionKeys = resultItems.SelectMany(r => r.Hotels).Select(h => h.PansionKey).Distinct().ToList();
            var pansions = dc.GetPansionsByKeys(pansionKeys, out hashOut);

            var tourKeys = resultItems.Select(r => r.TourKey).Distinct().ToList();
            var toursDescr = dc.GetTourListWithDescriptions(tourKeys, out hashOut);

            var tourStrings = dc.GetTourStringsByKeys(tourKeys, out hashOut);

            var rates = dc.GetCurrencies();

            foreach (var resultItem in resultItems)
            {
                foreach (var hotel in resultItem.Hotels)
                {
                    var hd = hotels.Single(h => h.Key == hotel.Key);
                    hotel.Name = hd.Name;
                    hotel.Url = hd.Http;
                    hotel.Stars = hd.Stars;
                    hotel.ResortKey = hd.CtKey;

                    hotel.ResortName = cities.Single(c => c.CT_KEY == hd.CtKey).CT_NAME;
                    hotel.PansionName = pansions.Single(p => p.PN_KEY == hotel.PansionKey).PN_NAME;
                }

                var tourString = tourStrings.Single(t => t.Item1 == resultItem.TourKey);
                resultItem.TourName = tourString.Item2;
                resultItem.TourUrl = tourString.Item3;

                resultItem.TourHasDescription = toursDescr.Single(t => t.Item1 == resultItem.TourKey).Item2;

                // получаем список валют для конвертации
                resultItem.PriceInRates = new Dictionary<string, double>(rates.Count);
                foreach (var rate in rates)
                {
                    resultItem.PriceInRates.Add(rate.Value, (double) dc.GetRateRealCourse(DateTime.Now.Date, dc.GetRateKeyByCode(resultItem.RateCode), rate.Key)*resultItem.Price);
                }
            }
        }

        /// <summary>
        /// Метод по выгрузке данных из БД
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="cityKeyFrom">Ключ города начала поездки</param>
        /// <param name="countryKey">Ключ страны тура</param>
        /// <param name="tourKeys">Ключи туров</param>
        /// <param name="tourDates">Даты заездов</param>
        /// <param name="tourNights">Продолжительности туров в ночах</param>
        /// <param name="hotelKeys">Ключи отеле</param>
        /// <param name="pansionKeys">Ключи питаний</param>
        /// <param name="mainPlaces">Число основных мест (для стран за номер)</param>
        /// <param name="addPlaces">Число дополнительных мест (для стран за номер)</param>
        /// <param name="firstChildYears">Возраст первого ребенка (для стран за номер)</param>
        /// <param name="secondChildYears">Возраст второго ребенка (для стран за номер)</param>
        /// <param name="roomTypeKeys">Ключи типа номера (для стран за человека)</param>
        /// <param name="hotelQuotaMask">Маска квот для отелей</param>
        /// <param name="rateKey">Ключ валюты</param>
        /// <param name="minTourPrice">Минимальная стоимость тура</param>
        /// <param name="maxTourPrice">Максимальная стоимость тура</param>
        /// <param name="sortType">Колонка и направление сортировки</param>
        /// <param name="rowFrom">Строка, с которой начинается поиск</param>
        /// <param name="rowTo">Строка, до которой идет поиск</param>
        /// <param name="aviaQuotaMask">Статус перелетов</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        private static IEnumerable<SearchResultItem> GetPriceItemsFromDb(MtSearchDbDataContext dc, int cityKeyFrom,
            int countryKey, IEnumerable<int> tourKeys,
            IEnumerable<DateTime> tourDates, IEnumerable<int> tourNights, IEnumerable<int> hotelKeys,
            IEnumerable<int> pansionKeys, ushort? mainPlaces,
            ushort? addPlaces, ushort? firstChildYears, ushort? secondChildYears, IEnumerable<int> roomTypeKeys,
            int rateKey, uint? minTourPrice, uint? maxTourPrice, Tuple<SortingColumn, SortingDirection> sortType, uint rowFrom, uint rowTo,
            QuotesStates aviaQuotaMask, QuotesStates hotelQuotaMask, 
            IEnumerable<Tuple<int, int, int, int, DateTime, int?, short>> wrongDirectCharters,
            IEnumerable<Tuple<int, int, int, int, DateTime, int?, short>> wrongBackCharters,
            out string hash)
        {
            List<SearchResultItem> resultItems;

            hash = String.Format("{0}_{1}_{2}_{3}", MethodBase.GetCurrentMethod().Name,
                cityKeyFrom,
                countryKey,
                CacheHelper.GetCacheKeyHashed(new[]
                {
                    String.Join("_", tourKeys),
                    String.Join("_", tourDates),
                    String.Join("_", tourNights),
                    String.Join("_", hotelKeys),
                    String.Join("_", pansionKeys),
                    mainPlaces.ToString(),
                    addPlaces.ToString(),
                    firstChildYears.ToString(),
                    secondChildYears.ToString(),
                    roomTypeKeys != null ? String.Join("_", roomTypeKeys) : "",
                    rateKey.ToString(CultureInfo.InvariantCulture),
                    minTourPrice.ToString(),
                    maxTourPrice.ToString(),
                    sortType.Item1.ToString(),
                    sortType.Item2.ToString(),
                    rowFrom.ToString(),
                    rowTo.ToString(),
                    aviaQuotaMask.ToString(),
                    hotelQuotaMask.ToString(),
                    wrongDirectCharters.ToString(),
                    wrongBackCharters.ToString()
                }));

            if ((resultItems = CacheHelper.GetCacheItem<List<SearchResultItem>>(hash)) != null) return resultItems;

            var cacheDependencies = new List<string>
            {
                String.Format("{0}_{1}_{2}", CacheHelper.DirectionHash, cityKeyFrom, countryKey)
            };
            resultItems = new List<SearchResultItem>();

            string filterStringSingleHotel, filterStringMultiHotel, filterStringTemp;
            CreatePagingCommandStrings(dc, PagingCommandType.TableXx, tourKeys, tourDates, tourNights, hotelKeys, pansionKeys, mainPlaces,
                addPlaces,
                firstChildYears, secondChildYears, roomTypeKeys, rateKey, minTourPrice, maxTourPrice, aviaQuotaMask, hotelQuotaMask, out filterStringSingleHotel);
            CreatePagingCommandStrings(dc, PagingCommandType.MultiHotelsTable, tourKeys, tourDates, tourNights, hotelKeys, pansionKeys, mainPlaces,
                addPlaces,
                firstChildYears, secondChildYears, roomTypeKeys, rateKey, minTourPrice, maxTourPrice, aviaQuotaMask, hotelQuotaMask, out filterStringMultiHotel);
            CreatePagingCommandStrings(dc, PagingCommandType.TableTemp, tourKeys, tourDates, tourNights, hotelKeys, pansionKeys, mainPlaces,
                addPlaces,
                firstChildYears, secondChildYears, roomTypeKeys, rateKey, minTourPrice, maxTourPrice, aviaQuotaMask, hotelQuotaMask, out filterStringTemp);

            var sortingString = GetFilterMethods.SortingColumns[sortType.Item1];
            if (sortType.Item2 == SortingDirection.Asc)
                sortingString = sortingString + " asc";
            else
                sortingString = sortingString + " desc";

            var commandBuilder = new StringBuilder();

            if (wrongDirectCharters.Any())
            {
                commandBuilder.AppendLine("create table #wrongCharterParams (xkey int primary key identity(1,1), xcharterKey int, xcharterDay int, xpartnerKey int, xpacketKey int, xtourDate DateTime, xlinkedDay int, xfindFlight int)");
                foreach (var tup in wrongDirectCharters)
                {
                    // charterKey, charterDay, partnerKey, packetKey, tourDate, linkedDay, findFlight
                    commandBuilder.AppendLine("insert into #wrongCharterParams (xcharterKey, xcharterDay, xpartnerKey, xpacketKey, xtourDate, xlinkedDay, xfindFlight) ");
                    commandBuilder.AppendLine(String.Format("values ({0}, {1}, {2}, {3}, '{4}', {5}, {6})", tup.Item1, tup.Item2, tup.Item3, tup.Item4, tup.Item5.ToString("yyyy-MM-dd"), tup.Item6.HasValue ? tup.Item6.Value.ToString() : "null", tup.Item7));
                }
            }

            if (wrongBackCharters.Any())
            {
                commandBuilder.AppendLine("create table #wrongBackCharterParams (xkey int primary key identity(1,1), xcharterKey int, xcharterDay int, xpartnerKey int, xpacketKey int, xtourDate DateTime, xlinkedDay int, xfindFlight int)");
                foreach (var tup in wrongBackCharters)
                {
                    // charterKey, charterDay, partnerKey, packetKey, tourDate, linkedDay, findFlight
                    commandBuilder.AppendLine("insert into #wrongBackCharterParams (xcharterKey, xcharterDay, xpartnerKey, xpacketKey, xtourDate, xlinkedDay, xfindFlight) ");
                    commandBuilder.AppendLine(String.Format("values ({0}, {1}, {2}, {3}, '{4}', {5}, {6})", tup.Item1, tup.Item2, tup.Item3, tup.Item4, tup.Item5.ToString("yyyy-MM-dd"), tup.Item6.HasValue ? tup.Item6.Value.ToString() : "null", tup.Item7));
                }
            }

            commandBuilder.AppendLine("select pt_hdkey, pt_hdday, pt_hdnights, pt_hdpartnerkey, pt_pricekey, pt_cnkey, pt_ctkeyfrom, pt_ctkeyto, pt_ctkeybackfrom, pt_ctkeybackto, pt_tourkey, pt_topricefor, pt_tourdate, pt_price, pt_rate, pt_days, pt_hrkey, pt_pnkey, pt_nights, pt_chkey,  pt_chbackkey, pt_chday, pt_chbackday, pt_chprkey, pt_chbackprkey, pt_chpkkey, pt_chbackpkkey, pt_directFlightAttribute, pt_backFlightAttribute, pt_mainplaces, pt_hddetails, pt_hdname, pt_hdstars, pt_ctname, pt_rmname, pt_rcname, pt_acname, pt_pnname, pt_tourname, pt_hotelkeys, pt_pansionkeys, pt_roomtypekeys, pt_addplaces ");
            commandBuilder.AppendLine("into #mwPriceDataTable ");
            commandBuilder.AppendLine("from mwPriceDataTable ");
            commandBuilder.AppendLine(String.Format("where {0} ", filterStringMultiHotel));
            commandBuilder.AppendLine(String.Format("and pt_ctkeyfrom = {0} ", cityKeyFrom));
            if (wrongDirectCharters.Any())
            {
                commandBuilder.AppendLine("and not exists (select 1 from #wrongCharterParams where xcharterKey = pt_chkey and xcharterDay = pt_chday and xpartnerKey = pt_chprkey and xpacketKey = pt_chpkkey and xtourDate = pt_tourdate and ((xlinkedDay is null and pt_chbackday is null) or xlinkedDay = pt_chbackday) and xfindFlight = pt_directFlightAttribute & 2)");
            }
            if (wrongBackCharters.Any())
            {
                commandBuilder.AppendLine("and not exists (select 1 from #wrongBackCharterParams where xcharterKey = pt_chbackkey and xcharterDay = pt_chbackday and xpartnerKey = pt_chbackprkey and xpacketKey = pt_chbackpkkey and xtourDate = pt_tourdate and ((xlinkedDay is null and pt_chday is null) or xlinkedDay = pt_chday) and xfindFlight = pt_backFlightAttribute & 2)");
            }
            
            commandBuilder.AppendLine("");

            commandBuilder.AppendLine("select * ");
            commandBuilder.AppendLine("from ");
            commandBuilder.AppendLine("( ");
            commandBuilder.AppendLine(String.Format("select Row_Number() OVER (order by {0}) as rowNum, * ", sortingString));
            commandBuilder.AppendLine("from");
            commandBuilder.AppendLine("( ");
            commandBuilder.AppendLine("select pt_hdkey, pt_hdday, pt_hdnights, pt_hdpartnerkey, pt_pricekey, pt_cnkey, pt_ctkeyfrom, pt_ctkeyto, pt_ctkeybackfrom, pt_ctkeybackto, pt_tourkey, pt_topricefor, pt_tourdate, pt_price, pt_rate, pt_days, pt_hrkey, pt_pnkey, pt_nights, pt_chkey, pt_chbackkey, pt_chday, pt_chbackday, pt_chprkey, pt_chbackprkey, pt_chpkkey, pt_chbackpkkey, pt_directFlightAttribute, pt_backFlightAttribute, pt_mainplaces, pt_hddetails, pt_hdname, pt_hdstars, pt_ctname, pt_rmname, pt_rcname, pt_acname, pt_pnname, pt_tourname ");
            commandBuilder.AppendLine(String.Format("from mwPriceDataTable_{0}_{1} ", countryKey, cityKeyFrom));
            commandBuilder.AppendLine(String.Format("where {0} ", filterStringSingleHotel));
            if (wrongDirectCharters.Any())
            {
                commandBuilder.AppendLine("and not exists (select 1 from #wrongCharterParams where xcharterKey = pt_chkey and xcharterDay = pt_chday and xpartnerKey = pt_chprkey and xpacketKey = pt_chpkkey and xtourDate = pt_tourdate and ((xlinkedDay is null and pt_chbackday is null) or xlinkedDay = pt_chbackday) and xfindFlight = pt_directFlightAttribute & 2)");
            }
            if (wrongBackCharters.Any())
            {
                commandBuilder.AppendLine("and not exists (select 1 from #wrongBackCharterParams where xcharterKey = pt_chbackkey and xcharterDay = pt_chbackday and xpartnerKey = pt_chbackprkey and xpacketKey = pt_chbackpkkey and xtourDate = pt_tourdate and ((xlinkedDay is null and pt_chday is null) or xlinkedDay = pt_chday) and xfindFlight = pt_backFlightAttribute & 2)");
            }

            commandBuilder.AppendLine("union ");
            commandBuilder.AppendLine(
                "select pt_hdkey, pt_hdday, pt_hdnights, pt_hdpartnerkey, pt_pricekey, pt_cnkey, pt_ctkeyfrom, pt_ctkeyto, pt_ctkeybackfrom, pt_ctkeybackto, pt_tourkey, pt_topricefor, pt_tourdate, pt_price, pt_rate, pt_days, pt_hrkey, pt_pnkey, pt_nights, pt_chkey, pt_chbackkey, pt_chday, pt_chbackday, pt_chprkey, pt_chbackprkey, pt_chpkkey, pt_chbackpkkey, pt_directFlightAttribute, pt_backFlightAttribute, pt_mainplaces, pt_hddetails, pt_hdname, pt_hdstars, pt_ctname, pt_rmname, pt_rcname, pt_acname, pt_pnname, pt_tourname ");
            commandBuilder.AppendLine("from #mwPriceDataTable ");
            commandBuilder.AppendLine(String.Format("where {0} ", filterStringTemp));
            commandBuilder.AppendLine(") as a ");
            commandBuilder.AppendLine(") as b ");
            commandBuilder.AppendLine(String.Format("where rowNum > {0} and rowNum <= {1}", rowFrom, rowTo));
            commandBuilder.AppendLine("order by rowNum");

            using (var command = dc.Connection.CreateCommand())
            {
                command.CommandText = commandBuilder.ToString();
                //todo: Сделать константу в конфиге
                command.CommandTimeout = 100;

                dc.Connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = dc.GetResultItemFromReader(reader);
                        if (item != null && resultItems.All(r => r.PriceKey != item.PriceKey))
                            resultItems.Add(item);
                    }
                }
                dc.Connection.Close();
            }

            string hashOut;
            var hotelRoomKeys = resultItems.SelectMany(r => r.Hotels).Select(h => h.HotelRoomKey).Distinct().ToList();
            var hotelRooms = dc.GetHotelRoomByKeys(hotelRoomKeys, out hashOut);
            if (!cacheDependencies.Contains(hashOut))
                cacheDependencies.Add(hashOut);

            var accmds = dc.GetAccomodationsByKeys(hotelRooms.Where(h => h.HR_ACKEY != null).Select(h => h.HR_ACKEY.Value).Distinct().ToList(), out hashOut);
            if (!cacheDependencies.Contains(hashOut))
                cacheDependencies.Add(hashOut);

            var rooms = dc.GetRoomsByKeys(hotelRooms.Select(h => h.HR_RMKEY).Distinct().ToList(), out hashOut);
            if (!cacheDependencies.Contains(hashOut))
                cacheDependencies.Add(hashOut);

            var roomCategories = dc.GetRoomCategoriesByKeys(hotelRooms.Select(h => h.HR_RCKEY).Distinct().ToList(), out hashOut);
            if (!cacheDependencies.Contains(hashOut))
                cacheDependencies.Add(hashOut);

            foreach (var item in resultItems)
            {
                foreach (var hotel in item.Hotels)
                {
                    var hotelRoom = hotelRooms.Single(h => h.HR_KEY == hotel.HotelRoomKey);
                    var accmd = hotelRoom.HR_ACKEY != null ? accmds.Single(a => a.AC_KEY == hotelRoom.HR_ACKEY.Value) : null;

                    var room = rooms.Single(r => r.RM_KEY == hotelRoom.HR_RMKEY);
                    var roomCategory = roomCategories.Single(r => r.RC_KEY == hotelRoom.HR_RCKEY);

                    hotel.AccomodationKey = accmd == null ? -1 : accmd.AC_KEY;
                    hotel.AccomodationName = accmd == null ? String.Empty : accmd.AC_NAME;
                    hotel.RoomKey = room.RM_KEY;
                    hotel.RoomName = room.RM_NAME;
                    hotel.RoomCategoryKey = roomCategory.RC_KEY;
                    hotel.RoomCategoryName = roomCategory.RC_NAME;
                }
            }

            CacheHelper.AddCacheData(hash, resultItems, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return resultItems;
        }

        /// <summary>
        /// Метод по выгрузке данных из БД
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="cityKeyFrom">Ключ города начала поездки</param>
        /// <param name="countryKey">Ключ страны тура</param>
        /// <param name="priceKey">Ключ цены</param>
        /// <param name="hash">Хэш кэша</param>
        private static IEnumerable<SearchResultItem> GetPriceItemsFromDb(MtSearchDbDataContext dc, int cityKeyFrom,
            int countryKey, long priceKey, out string hash)
        {
            List<SearchResultItem> resultItems;

            hash = String.Format("{0}_{1}_{2}_{3}", MethodBase.GetCurrentMethod().Name,
                cityKeyFrom,
                countryKey,
                priceKey);

            if ((resultItems = CacheHelper.GetCacheItem<List<SearchResultItem>>(hash)) != null) return resultItems;

            var cacheDependencies = new List<string>
            {
                String.Format("{0}_{1}_{2}", CacheHelper.DirectionHash, cityKeyFrom, countryKey)
            };
            resultItems = new List<SearchResultItem>();

            using (var command = dc.Connection.CreateCommand())
            {
                var commandBuilder = new StringBuilder();
                
                commandBuilder.AppendLine("select pt_key, pt_hdkey, pt_hdday, pt_hdnights, pt_hdpartnerkey, pt_pricekey, pt_cnkey, pt_ctkeyfrom, pt_ctkeyto, pt_tourkey, pt_topricefor, pt_tourdate, pt_price, pt_rate, pt_days, pt_hrkey, pt_pnkey, pt_nights, pt_chkey, pt_chbackkey, pt_chday, pt_chbackday, pt_chprkey, pt_chbackprkey, pt_chpkkey, pt_chbackpkkey, pt_directFlightAttribute, pt_backFlightAttribute, pt_mainplaces, pt_hddetails, pt_hdname, pt_hdstars, pt_ctname, pt_rmname, pt_rcname, pt_acname, pt_pnname, pt_tourname ");
                commandBuilder.AppendLine(String.Format("from mwPriceDataTable_{0}_{1} ", countryKey, cityKeyFrom));
                commandBuilder.AppendLine(String.Format("where pt_pricekey = {0} ", priceKey));

                command.CommandText = commandBuilder.ToString();
                command.CommandTimeout = 100;

                dc.Connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = dc.GetResultItemFromReader(reader);
                        if (item != null && resultItems.All(r => r.PriceKey != item.PriceKey))
                            resultItems.Add(item);
                    }
                }
                dc.Connection.Close();
            }

            CacheHelper.AddCacheData(hash, resultItems, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return resultItems;
        }

        /// <summary>
        /// Метод по заполнению структуры из ридера. Эта структура затем идет в метод по организации поиска (Paging)
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="reader">Ридер с данными</param>
        private static SearchResultItem GetResultItemFromReader(this MtSearchDbDataContext dc, IDataReader reader)
        {
            // koshelev
            // валидация неправильно посчитанных или выставленных цен, с неправильными ценами непонятно что делать
            if (reader.GetInt32OrNull("pt_days") == null)
                return null;

            var resultItem = new SearchResultItem
            {
                PriceKey = reader.GetInt32("pt_pricekey"),
                Price = reader.GetDouble("pt_price"),
                RateCode = reader.GetStringOrNull("pt_rate"),
                Date = reader.GetDateTime("pt_tourdate"),
                CountryKey = reader.GetInt32("pt_cnkey"),
                FlightCityKeyFrom = reader.GetInt32("pt_ctkeyfrom"),
                FlightCityKeyTo = reader.GetInt32OrNull("pt_ctkeyto"),
                BackFlightCityKeyFrom = reader.GetInt32OrNull("pt_ctkeybackfrom"),
                BackFlightCityKeyTo = reader.GetInt32OrNull("pt_ctkeybackto"),
                HotelKey = reader.GetInt32("pt_hdkey"),
                TourKey = reader.GetInt32("pt_tourkey"),
                PriceFor = (PriceForType)reader.GetInt16("pt_topricefor"),
                MainPlacesCount = (short)reader.GetInt32("pt_mainplaces")
            };

            resultItem.FlightCalcInfo.CharterKey = reader.GetInt32OrNull("pt_chkey");
            resultItem.FlightCalcInfo.CharterDay = reader.GetInt32OrNull("pt_chday");
            resultItem.FlightCalcInfo.CharterPartnerKey = reader.GetInt32OrNull("pt_chprkey");
            resultItem.FlightCalcInfo.CharterPacketKey = reader.GetInt32OrNull("pt_chpkkey");

            if (resultItem.FlightCalcInfo.CharterKey == null)
                resultItem.FlightCityKeyFrom = null;

            resultItem.FlightCalcInfo.BackCharterKey = reader.GetInt32OrNull("pt_chbackkey");
            resultItem.FlightCalcInfo.BackCharterDay = reader.GetInt32OrNull("pt_chbackday");
            resultItem.FlightCalcInfo.BackCharterPartnerKey = reader.GetInt32OrNull("pt_chbackprkey");
            resultItem.FlightCalcInfo.BackCharterPacketKey = reader.GetInt32OrNull("pt_chbackpkkey");


            if (reader.GetInt32OrNull("pt_directFlightAttribute") == null ||
                (reader.GetInt32("pt_directFlightAttribute") & 2) == 0)
                resultItem.FlightCalcInfo.FindDirectFlight = false;
            else
                resultItem.FlightCalcInfo.FindDirectFlight = true;

            if (reader.GetInt32OrNull("pt_backFlightAttribute") == null ||
                (reader.GetInt32("pt_backFlightAttribute") & 2) == 0)
                resultItem.FlightCalcInfo.FindBackFlight = false;
            else
                resultItem.FlightCalcInfo.FindBackFlight = true;

            resultItem.DateTourEnd = resultItem.Date.AddDays(reader.GetInt32("pt_days") - 1);

            //многоотельные туры
            if (reader.GetStringOrNull("pt_hddetails").IndexOf(',') >= 0)
            {
                resultItem.Hotels = new List<Hotel>();
                foreach (var hotelString in reader.GetStringOrNull("pt_hddetails").Split(','))
                {
                    var hotelParts = hotelString.Split(':');
                    var hotel = new Hotel()
                    {
                        Key = Int32.Parse(hotelParts[0]),
                        DateFrom = resultItem.Date.AddDays(Int32.Parse(hotelParts[3]) - 1),
                        NightsCount = Int32.Parse(hotelParts[4]),
                        PartnerKey = Int32.Parse(hotelParts[5]),
                        HotelRoomKey = Int32.Parse(hotelParts[6]),
                        PansionKey = Int32.Parse(hotelParts[7])
                    };

                    resultItem.Hotels.Add(hotel);
                }
            }
            else
            {
                resultItem.Hotels = new List<Hotel>(1);
                var hotel = new Hotel
                {
                    Key = reader.GetInt32("pt_hdkey"),
                    DateFrom = resultItem.Date.AddDays(reader.GetInt32("pt_hdday") - 1),
                    HotelRoomKey = reader.GetInt32("pt_hrkey"),
                    PansionKey = reader.GetInt32("pt_pnkey"),
                    NightsCount = reader.GetInt32("pt_nights"),
                    PartnerKey = reader.GetInt32("pt_hdpartnerkey")
                };

                resultItem.Hotels.Add(hotel);
            }
            return resultItem;
        }

        /// <summary>
        /// Метод по формированию строки запроса к БД с нужными параметрами
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="commandType">Тип запроса к БД. Может быть запрос к одной из трех табиц (mwPriceDataTable, mwPriceDataTable_x_x, #mwPriceDataTable)</param>
        /// <param name="tourKeys">Ключи туров</param>
        /// <param name="tourDates">Даты заездов</param>
        /// <param name="tourNights">Продолжительности туров в ночах</param>
        /// <param name="hotelKeys">Ключи отеле</param>
        /// <param name="pansionKeys">Ключи питаний</param>
        /// <param name="mainPlaces">Число основных мест (для стран за номер)</param>
        /// <param name="addPlaces">Число дополнительных мест (для стран за номер)</param>
        /// <param name="firstChildYears">Возраст первого ребенка (для стран за номер)</param>
        /// <param name="secondChildYears">Возраст второго ребенка (для стран за номер)</param>
        /// <param name="roomTypeKeys">Ключи типа номера (для стран за человека)</param>
        /// <param name="hotelQuotaMask">Маска квот для отелей</param>
        /// <param name="rateKey">Ключ валюты</param>
        /// <param name="minTourPrice">Минимальная стоимость тура</param>
        /// <param name="maxTourPrice">Максимальная стоимость тура</param>
        /// <param name="flightTicketState">Подбирать только цены с включенными перелетами или с выключенными</param>
        /// <param name="filterString">Выходной параметр, результат работы метода</param>
        /// <returns></returns>
        private static void CreatePagingCommandStrings(MtSearchDbDataContext dc, PagingCommandType commandType, IEnumerable<int> tourKeys, IEnumerable<DateTime> tourDates,
            IEnumerable<int> tourNights, IEnumerable<int> hotelKeys, IEnumerable<int> pansionKeys, ushort? mainPlaces, ushort? addPlaces,
            ushort? firstChildYears, ushort? secondChildYears, IEnumerable<int> roomTypeKeys, int rateKey, uint? minTourPrice, uint? maxTourPrice, QuotesStates aviaQuotaMask,
            QuotesStates hotelQuotaMask, 
            out string filterString)
        {
            var hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name,
                CacheHelper.GetCacheKeyHashed(new[]
                {
                    commandType.ToString(),
                    String.Join("_", tourKeys),
                    String.Join("_", tourDates),
                    String.Join("_", tourNights),
                    String.Join("_", hotelKeys),
                    String.Join("_", pansionKeys),
                    mainPlaces.ToString(),
                    addPlaces.ToString(),
                    firstChildYears.ToString(),
                    secondChildYears.ToString(),
                    roomTypeKeys != null ? String.Join("_", roomTypeKeys) : "",
                    rateKey.ToString(CultureInfo.InvariantCulture),
                    minTourPrice.ToString(),
                    maxTourPrice.ToString(),
                    aviaQuotaMask.ToString(),
                    hotelQuotaMask.ToString()
                }));

            if (CacheHelper.IsCacheKeyExists(hash))
            {
                filterString = CacheHelper.GetCacheItem<string>(hash);
                return;
            }

            if (mainPlaces.HasValue && !addPlaces.HasValue)
                addPlaces = 0;

            var filterParams = new List<string>();
            if (commandType == PagingCommandType.TableXx || commandType == PagingCommandType.MultiHotelsTable)
            {
                filterParams.Add("pt_isenabled = 1 and pt_tourvalid > getdate() ");
                filterParams.Add("pt_tourkey in (" + string.Join(",", tourKeys) + ")");
            }

            #region tourdates
            var tempFilterBuilder = new StringBuilder();
            if (commandType == PagingCommandType.TableXx || commandType == PagingCommandType.MultiHotelsTable)
            {
                tempFilterBuilder.Append("pt_tourdate in (");
                foreach (var dateString in tourDates.Select(d => d.ToString("yyyy-MM-dd")))
                    tempFilterBuilder.AppendFormat("'{0}',", dateString);
                tempFilterBuilder = tempFilterBuilder.Remove(tempFilterBuilder.Length - 1, 1);
                tempFilterBuilder.Append(")");
                filterParams.Add(tempFilterBuilder.ToString());
            }
            tempFilterBuilder.Clear();
            #endregion tourdates

            #region pt_nights

            if (commandType == PagingCommandType.TableXx || commandType == PagingCommandType.MultiHotelsTable)
            {
                filterParams.Add("pt_nights in (" + string.Join(",", tourNights) + ")");
            }
            
            #endregion pt_nights

            #region hotelKeys
            switch (commandType)
            {
                case PagingCommandType.TableXx:
                    filterParams.Add("pt_hdkey in (" + string.Join(",", hotelKeys) + ")");
                    break;
                case PagingCommandType.TableTemp:
                {
                    var hotelKeyFilters = new List<string>(hotelKeys.Count());
                    hotelKeyFilters.AddRange(hotelKeys.Select(hotelKey => String.Format("pt_hotelkeys like '%,{0},%'", hotelKey)));
                    filterParams.Add("(" + string.Join(" or ", hotelKeyFilters) + ")");
                }
                    break;
            }
            #endregion hotelKeys

            #region pansionKeys
            switch (commandType)
            {
                case PagingCommandType.TableXx:
                    filterParams.Add("pt_pnkey in (" + string.Join(",", pansionKeys) + ")");
                    break;
                case PagingCommandType.TableTemp:
                {
                    var pansionKeyFilters = new List<string>(pansionKeys.Count());
                    pansionKeyFilters.AddRange(pansionKeys.Select(pansionKey => String.Format("pt_pansionkeys like '%,{0},%'", pansionKey)));
                    filterParams.Add("(" + string.Join(" or ", pansionKeyFilters) + ")");
                }
                    break;
            }
            #endregion pansionKeys

            #region mainplaces
            if (mainPlaces.HasValue && (roomTypeKeys == null || !roomTypeKeys.Any()))
            {
                if (commandType == PagingCommandType.MultiHotelsTable || commandType == PagingCommandType.TableXx)
                    filterParams.Add(String.Format("pt_mainplaces = {0}", mainPlaces.Value));
            }
            #endregion mainplaces

            #region addplaces
            if (addPlaces.HasValue && (roomTypeKeys == null || !roomTypeKeys.Any()))
            {
                if (commandType == PagingCommandType.MultiHotelsTable || commandType == PagingCommandType.TableXx)
                {
                    filterParams.Add(String.Format("pt_addplaces = {0}", addPlaces.Value));

                    if (addPlaces.Value > 0 && !firstChildYears.HasValue)
                        throw new ArgumentNullException("firstChildYears",
                            "При указании числа детей указывать их возраст обязательно!");

                    if (addPlaces.Value > 1 && !secondChildYears.HasValue)
                        throw new ArgumentNullException("secondChildYears",
                            "При указании числа детей указывать их возраст обязательно!");

                    if (firstChildYears.HasValue && !secondChildYears.HasValue)
                    {
                        filterParams.Add(String.Format("pt_childagefrom <= {0} and {0} <= isnull(pt_childageto, 200)", firstChildYears.Value));
                    }
                    if (firstChildYears.HasValue && secondChildYears.HasValue)
                    {
                        filterParams.Add(String.Format(" (({0} between pt_childagefrom and isnull(pt_childageto, 200) and {1} between pt_childagefrom2 and isnull(pt_childageto2, 200)) or ({1} between pt_childagefrom and isnull(pt_childageto, 200) and {0} between pt_childagefrom2 and isnull(pt_childageto2, 200)))", firstChildYears.Value, secondChildYears.Value));
                    }
                }   
            }
            #endregion addplaces

            #region roomTypeKeys
            if (roomTypeKeys != null && roomTypeKeys.Any())
            {
                if (commandType == PagingCommandType.TableXx || commandType == PagingCommandType.MultiHotelsTable)
                {
                    filterParams.Add("((pt_main > 0 and (pt_childageto = 0 or isnull(pt_childageto, 100) > 16)))");
                }
                
                if (commandType == PagingCommandType.TableXx)
                {
                    filterParams.Add(String.Format("pt_rmkey in ({0})", String.Join(",", roomTypeKeys))); 
                }
                else if (commandType == PagingCommandType.TableTemp)
                {
                    var roomTypeKeyFilters = new List<string>(roomTypeKeys.Count());
                    roomTypeKeyFilters.AddRange(roomTypeKeys.Select(roomTypeKey => String.Format("pt_roomtypekeys like '%,{0},%'", roomTypeKey)));
                    filterParams.Add("(" + String.Format("({0})", string.Join(" or ", roomTypeKeyFilters)) + ")");
                }
            }
            #endregion roomTypeKeys

            #region minmaxprice
            if (minTourPrice.HasValue || maxTourPrice.HasValue)
            {
                string rateCodeTours = String.Empty;
                int rateKeyTours = 0;

                var commandBuilder = new StringBuilder();
                commandBuilder.Append("SELECT [ra_key], [RA_CODE] ");
                commandBuilder.Append("from [Rates] ");
                commandBuilder.Append("join (select top 1 to_rate, count(1) as c ");
                commandBuilder.Append("from tp_tours ");
                commandBuilder.AppendFormat("where to_key in ({0}) ", string.Join(",", tourKeys));
                commandBuilder.Append("group by TO_Rate ");
                commandBuilder.Append("order by c desc) as t on t.TO_Rate = RA_CODE COLLATE SQL_Latin1_General_CP1251_CI_AS");

                using (var command = dc.Connection.CreateCommand())
                {
                    command.CommandText = commandBuilder.ToString();

                    dc.Connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rateKeyTours = reader.GetInt32(0);
                            rateCodeTours = reader.GetString(1);
                        }
                    }
                    dc.Connection.Close();
                }

                if (rateCodeTours == String.Empty)
                    throw new ApplicationException(MethodBase.GetCurrentMethod().Name +
                                                   ". Не найдена валюта для конвертации туров");

                var course = dc.GetRateRealCourse(DateTime.Now.Date, rateKey, rateKeyTours);

                if (minTourPrice.HasValue)
                {
                    if (commandType == PagingCommandType.MultiHotelsTable || commandType == PagingCommandType.TableXx)
                    {
                        var minTourPriceForFilter = minTourPrice.Value * course;
                        filterParams.Add(String.Format("pt_price >= {0}", minTourPriceForFilter.ToString(CultureInfo.InvariantCulture).Replace(",", ".")));
                    }
                }

                if (maxTourPrice.HasValue)
                {
                    if (commandType == PagingCommandType.MultiHotelsTable || commandType == PagingCommandType.TableXx)
                    {
                        var maxTourPriceForFilter = maxTourPrice.Value * course;
                        filterParams.Add(String.Format("pt_price <= {0}", maxTourPriceForFilter.ToString(CultureInfo.InvariantCulture).Replace(",", ".")));
                    }
                }
            }
            #endregion

            #region flightsIncludedOnly

            if (commandType == PagingCommandType.MultiHotelsTable || commandType == PagingCommandType.TableXx)
            {
                if (aviaQuotaMask == QuotesStates.Availiable)
                {
                    filterParams.Add("pt_chkey is not null and exists (select 1 from QuotaObjects where QO_SVKey = 1 and QO_Code = pt_chkey)");
                    filterParams.Add("pt_chbackkey is not null and exists (select 1 from QuotaObjects where QO_SVKey = 1 and QO_Code = pt_chbackkey)");
                }
            }

            #endregion flightsIncludedOnly

            #region hotelquotes

            if (hotelQuotaMask == QuotesStates.Availiable)
            {

                //if (commandType == PagingCommandType.MultiHotelsTable)
                //{
                //    filterParams.Add(String.Format(
                //        "(pt_hddetails is not null and charindex(',', pt_hddetails, 0) > 0 and (select count(1) from QuotaObjects where QO_SVKey = {0} and exists (select 1 from mwGetHotelKeysFromHotelDetails(pt_hddetails) where QO_Code = xHdKey and QO_SubCode1 in (0, xRmKey) and QO_SubCode2 in (0, xRcKey))) >= (select len(pt_hddetails)-len(replace(pt_hddetails,',','')) + 1))",
                //        (int)ServiceClass.Hotel));
                //}

                if (commandType == PagingCommandType.TableXx || commandType == PagingCommandType.MultiHotelsTable)
                {
                    filterParams.Add(String.Format("exists (select 1 from QuotaObjects join QuotaDetails on QD_QTID = QO_QTID where QO_SVKey = {0} and QO_Code = pt_hdkey and QD_Places > QD_Busy and QD_Date between dateadd(d, pt_hdday, pt_tourdate) and dateadd(d, pt_hdday + pt_hdnights, pt_tourdate))", (int)ServiceClass.Hotel));
                }

            }

            #endregion

            filterString = String.Join(" AND ", filterParams);
            CacheHelper.AddCacheData(hash, filterString, null, Globals.Settings.Cache.LongCacheTimeout);
        }

        /// <summary>
        /// Метод валидации параметров поиска
        /// </summary>
        /// <param name="cityKeyFrom">Ключ города начала поездки</param>
        /// <param name="countryKey">Ключ страны тура</param>
        /// <param name="tourKeys">Ключи туров</param>
        /// <param name="tourDates">Даты заездов</param>
        /// <param name="tourNights">Продолжительности туров в ночах</param>
        /// <param name="hotelKeys">Ключи отеле</param>
        /// <param name="pansionKeys">Ключи питаний</param>
        /// <param name="mainPlaces">Число основных мест (для стран за номер)</param>
        /// <param name="addPlaces">Число дополнительных мест (для стран за номер)</param>
        /// <param name="firstChildYears">Возраст первого ребенка (для стран за номер)</param>
        /// <param name="secondChildYears">Возраст второго ребенка (для стран за номер)</param>
        /// <param name="roomTypeKeys">Ключи типа номера (для стран за человека)</param>
        /// <param name="hotelQuotaMask">Маска квот для отелей</param>
        /// <param name="aviaQuotaMask">Маска квот для перелетов</param>
        /// <param name="rateKey">Ключ валюты</param>
        /// <param name="minTourPrice">Минимальная стоимость тура</param>
        /// <param name="maxTourPrice">Максимальная стоимость тура</param>
        /// <param name="rowsPerPage">Число строк на странице</param>
        /// <param name="rowCounterFrom">Номер строки, с которой начинаем поиск</param>
        /// <param name="sortType">Колонка и направление сортировки</param>
        /// <returns></returns>
        private static string ValidatePagingParams(int cityKeyFrom, int countryKey, IEnumerable<int> tourKeys, IEnumerable<DateTime> tourDates,
            IEnumerable<int> tourNights, IEnumerable<int> hotelKeys, IEnumerable<int> pansionKeys, ushort? mainPlaces, ushort? addPlaces,
            ushort? firstChildYears, ushort? secondChildYears, IEnumerable<int> roomTypeKeys, QuotesStates hotelQuotaMask,
            QuotesStates aviaQuotaMask, int rateKey, uint? minTourPrice, uint? maxTourPrice, ushort rowsPerPage, uint rowCounterFrom, Tuple<SortingColumn, SortingDirection> sortType)
        {
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

            var hash = String.Format("{0}_{1}_{2}_{3}", MethodBase.GetCurrentMethod().Name, cityKeyFrom, countryKey,
                CacheHelper.GetCacheKeyHashed(new[]
                {
                    String.Join("_", tourKeys),
                    String.Join("_", tourDates),
                    String.Join("_", tourNights),
                    String.Join("_", hotelKeys),
                    String.Join("_", pansionKeys),
                    mainPlaces.ToString(),
                    addPlaces.ToString(),
                    firstChildYears.ToString(),
                    secondChildYears.ToString(),
                    roomTypeKeys != null ? String.Join("_", roomTypeKeys) : "",
                    hotelQuotaMask.ToString(),
                    aviaQuotaMask.ToString(),
                    rateKey.ToString(CultureInfo.InvariantCulture),
                    minTourPrice.ToString(),
                    maxTourPrice.ToString(),
                    rowsPerPage.ToString(CultureInfo.InvariantCulture),
                    rowCounterFrom.ToString(CultureInfo.InvariantCulture),
                    sortType != null ? sortType.Item1 + "_" + sortType.Item2 : String.Empty
                }));

            return hash;
        }

    }
}
