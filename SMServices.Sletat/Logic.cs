using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Security;
using QDSearch;
using QDSearch.DataModel;
using QDSearch.Extensions;
using QDSearch.Helpers;
using QDSearch.Repository.MtMain;
using QDSearch.Repository.MtSearch;
using QDSearch.Repository.SftWeb;
using SMServices.Sletat.DataModel;
using Converters = SMServices.Sletat.Helpers.Converters;
using Service = SMServices.Sletat.DataModel.Service;


namespace SMServices.Sletat
{
    /// <summary>
    /// Класс для работы с сервисом слетать
    /// </summary>
    public static class SletatService
    {
        /// <summary>
        /// Возвращает список стран
        /// </summary>
        /// <param name="searchDc"></param>
        /// <param name="sftDc"></param>
        /// <param name="countryKey">Ключ страны (опциональный)</param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static List<Country> GetCountries(this MtSearchDbDataContext searchDc, SftWebDbDataContext sftDc, int? countryKey, out string hash)
        {
            List<Country> result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, countryKey);
            if ((result = CacheHelper.GetCacheItem<List<Country>>(hash)) != null) return result;

            string hashOut;
            var cacheDependencies = new List<string>();
            var countries = searchDc.GetCountriesTo(sftDc, null, out hashOut);
            cacheDependencies.Add(hashOut);
            result = new List<Country>(countries.Count);

            result.AddRange(countryKey == null
                ? countries.Select(c => new Country {Id = c.Key, Name = c.Value}).ToList()
                : countries.Where(c => c.Key == countryKey.Value)
                    .Select(c => new Country {Id = c.Key, Name = c.Value})
                    .ToList());
            cacheDependencies.Add(CountriesExtension.TableName);

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Города вылета
        /// </summary>
        /// <param name="sftDc"></param>
        /// <param name="cityKey">Ключ города (оциональный)</param>
        /// <param name="searchDc"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static List<City> GetDepartCities(this MtSearchDbDataContext searchDc, SftWebDbDataContext sftDc, int? cityKey, out string hash)
        {
            List<City> result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, cityKey);
            if ((result = CacheHelper.GetCacheItem<List<City>>(hash)) != null) return result;

            var cacheDependencies = new List<string>();
            string hashOut;
            var cities = searchDc.GetCitiesDeparture(sftDc, null, out hashOut);
            cacheDependencies.Add(hashOut);

            result = new List<City>(cities.Count);
            if (cityKey == null)
            {
                result.AddRange(cities.Select(city => new City
                {
                    Id = city.Key,
                    Name = city.Value.Trim(),
                    CountriesTo = searchDc.GetCountriesTo(sftDc, city.Key, out hashOut).Select(c => new CountryTo { Id = c.Key }).ToList()
                }));
            }
            else
            {
                result.AddRange(cities.Where(c => c.Key == cityKey.Value).Select(city => new City
                {
                    Id = city.Key,
                    Name = city.Value.Trim(),
                    CountriesTo = searchDc.GetCountriesTo(sftDc, city.Key, out hashOut).Select(c => new CountryTo { Id = c.Key }).ToList()
                }));
            }

            cacheDependencies.Add(hashOut);
            cacheDependencies.Add(CountriesExtension.TableName);
            cacheDependencies.Add(CitiesExtension.TableName);

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Курорты
        /// </summary>
        /// <param name="sftDc"></param>
        /// <param name="resortKey">Ключ курорта</param>
        /// <param name="searchDc"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static List<DataModel.Resort> GetResorts(this MtSearchDbDataContext searchDc, SftWebDbDataContext sftDc, int? resortKey, out string hash)
        {
            List<DataModel.Resort> result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, resortKey);
            if ((result = CacheHelper.GetCacheItem<List<DataModel.Resort>>(hash)) != null) return result;

            var cacheDependencies = new List<string>();
            result = new List<DataModel.Resort>();
            if (resortKey == null)
            {
                string hashOut;
                var cities = searchDc.GetCitiesDeparture(sftDc, null, out hashOut);
                cacheDependencies.Add(hashOut);

                foreach (var city in cities)
                {
                    var countries = searchDc.GetCountriesTo(sftDc, city.Key, out hashOut);
                    if (!cacheDependencies.Contains(hashOut))
                        cacheDependencies.Add(hashOut);

                    foreach (var country in countries)
                    {
                        var resorts = searchDc.GetCitiesTo(sftDc, city.Key, country.Key, null, out hashOut);
                        if (!cacheDependencies.Contains(hashOut))
                            cacheDependencies.Add(hashOut);

                        result.AddRange(resorts.Select(r => new DataModel.Resort
                        {
                            CountryId = country.Key,
                            Id = r.Key,
                            Name = r.Value
                        }).ToList());
                    }
                }
            }
            else
            {
                var resort = searchDc.GetCityByKey(resortKey.Value);
                result.Add(new DataModel.Resort
                {
                    Id = resort.CT_KEY,
                    Name = resort.CT_NAME,
                    CountryId = resort.CT_CNKEY
                });
            }

            
            cacheDependencies.Add(CountriesExtension.TableName);
            cacheDependencies.Add(CitiesExtension.TableName);

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Категории отелей
        /// </summary>
        /// <param name="mainDc"></param>
        /// <param name="categoryKey">Ключ категории</param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static List<HotelCategory> GetHotelCategories(this MtMainDbDataContext mainDc, int? categoryKey, out string hash)
        {
            List<HotelCategory> result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, categoryKey);
            if ((result = CacheHelper.GetCacheItem<List<HotelCategory>>(hash)) != null) return result;

            result = new List<HotelCategory>();
            if (categoryKey == null)
            {
                result.AddRange(mainDc.GetAllHotelCats().Select(c => new HotelCategory
                {
                    Id = c.COH_Id,
                    Name = c.COH_Name
                }).ToList());
            }
            else
            {
                var hotelCat = mainDc.GetHotelCatByKey(categoryKey.Value);
                result.Add(new HotelCategory
                {
                    Id = hotelCat.COH_Id,
                    Name = hotelCat.COH_Name
                });
            }

            CacheHelper.AddCacheData(hash, result, new List<string>() { HotelCategoriesExtension.TableName }, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Отели
        /// </summary>
        /// <param name="searchDc"></param>
        /// <param name="sftDc"></param>
        /// <param name="hotelKey">Ключ категории</param>
        /// <param name="mainDc"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static List<DataModel.Hotel> GetHotels(this MtMainDbDataContext mainDc, MtSearchDbDataContext searchDc, SftWebDbDataContext sftDc, int? hotelKey, out string hash)
        {
            List<DataModel.Hotel> result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, hotelKey);
            if ((result = CacheHelper.GetCacheItem<List<DataModel.Hotel>>(hash)) != null) return result;

            result = new List<DataModel.Hotel>();
            var cacheDependencies = new List<string>();
            if (hotelKey == null)
            {
                string hashOut;
                var hotelCities = (from r in searchDc.GetResorts(sftDc, null, out hashOut)
                    select r.Id)
                    .ToList();
                cacheDependencies.Add(hashOut);

                var hotels = (from h in searchDc.GetAllHotels()
                                join c in mainDc.GetAllHotelCats() on h.Stars equals c.COH_Name
                              where hotelCities.Contains(h.CtKey)
                                select
                                    new DataModel.Hotel { Id = h.Key, Name = h.Name, ResortId = h.CtKey, HotelCategoryId = c.COH_Id })
                    .ToList();

                result.AddRange(hotels);
            }
            else
            {
                string hashOut;
                var hotel = searchDc.GetHotelByKey(hotelKey.Value, out hashOut);
                cacheDependencies.Add(hashOut);

                if (hotel != null)
                {
                    var cat = mainDc.GetAllHotelCats().SingleOrDefault(c => c.COH_Name == hotel.Stars);
                    if (cat != null)
                    {
                        result.Add(new DataModel.Hotel
                        {
                            HotelCategoryId = cat.COH_Id,
                            Id = hotel.Key,
                            Name = hotel.Name,
                            ResortId = hotel.CtKey
                        });
                    }
                }
                
            }

            cacheDependencies.Add(HotelsExtension.TableName);
            cacheDependencies.Add(HotelCategoriesExtension.TableName);
            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Питания
        /// </summary>
        /// <param name="searchDc"></param>
        /// <param name="pansionKey">Ключ питания</param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static List<Meal> GetMeals(this MtSearchDbDataContext searchDc, int? pansionKey, out string hash)
        {
            List<Meal> result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, pansionKey);
            if ((result = CacheHelper.GetCacheItem<List<Meal>>(hash)) != null) return result;

            result = new List<Meal>();
            if (pansionKey == null)
            {
                result.AddRange(searchDc.GetAllPansions().Select(p => new Meal
                {
                    Id = p.PN_KEY,
                    Name = p.PN_NAME
                }).ToList());
            }
            else
            {
                var pansion = searchDc.GetPansionByKey(pansionKey.Value);
                result.Add(new Meal
                {
                    Id = pansion.PN_KEY,
                    Name = pansion.PN_NAME
                });
            }

            CacheHelper.AddCacheData(hash, result, new List<string>() { PansionsExtension.TableName }, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Валюты
        /// </summary>
        /// <param name="searchDc"></param>
        /// <param name="currencyKey">Ключ валюты</param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static List<Currency> GetCurrencies(this MtSearchDbDataContext searchDc, int? currencyKey, out string hash)
        {
            List<Currency> result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, currencyKey);
            if ((result = CacheHelper.GetCacheItem<List<Currency>>(hash)) != null) return result;

            result = new List<Currency>();
            if (currencyKey == null)
            {
                result.AddRange(searchDc.GetCurrencies().Select(c => new Currency
                {
                    Id = c.Key,
                    Name = c.Value
                }).ToList());
            }
            else
            {
                var currency = searchDc.GetCurrencies().Where(c => c.Key == currencyKey.Value).Select(c => new Currency
                {
                    Id = c.Key,
                    Name = c.Value
                }).SingleOrDefault();

                if (currency != null)
                    result.Add(currency);
            }

            CacheHelper.AddCacheData(hash, result, null, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        public static Tuple<SearchResult, List<Tour>> GetTours(this MtSearchDbDataContext searchDc, MtMainDbDataContext mainDc, SftWebDbDataContext sftDc, long offerId, int currencyId, out string hash)
        {
            Tuple<SearchResult, List<Tour>> result;
            hash = String.Format("{0}_{1}_{2}", MethodBase.GetCurrentMethod().Name, offerId, currencyId);

            if ((result = CacheHelper.GetCacheItem<Tuple<SearchResult, List<Tour>>>(hash)) != null)
                return result;

            string hashOut;
            var cacheDependencies = new List<string>();
            var searchResult = searchDc.GetSearchResult(mainDc, sftDc, offerId, currencyId, out hashOut);
            cacheDependencies.Add(hashOut);
            var tours = searchDc.ConvertSearchItemsToTours(mainDc, searchResult.SearchItems, currencyId);

            result = new Tuple<SearchResult, List<Tour>>(searchResult, tours);

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        public static Tuple<SearchResult, List<Tour>> GetTours(this MtSearchDbDataContext searchDc, MtMainDbDataContext mainDc, SftWebDbDataContext sftDc, 
            int count, int countryId, int departCityId, DateTime dateFrom, DateTime dateTo, ushort adults, ushort kids, IEnumerable<ushort> kidsAges, 
            int nightsMin, int nightsMax, IEnumerable<int> resorts, IEnumerable<int> hotelCategories, IEnumerable<int> hotels, IEnumerable<int> meals, 
            int currencyId, uint? priceMin, uint? priceMax, int? hotelsIsNotInStop, int? ticketsIncluded, int? hasTickets, out string hash)
        {
            Tuple<SearchResult, List<Tour>> result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name,
                CacheHelper.GetCacheKeyHashed(new[]
                {
                    count.ToString(), 
                    countryId.ToString(), 
                    departCityId.ToString(), 
                    dateFrom.ToString(), 
                    dateTo.ToString(), 
                    adults.ToString(), 
                    kids.ToString(), 
                    kidsAges != null ? String.Join("_", kidsAges) : String.Empty,
                    nightsMin.ToString(), 
                    nightsMax.ToString(), 
                    resorts != null ? String.Join("_", resorts) : String.Empty, 
                    hotelCategories != null ? String.Join("_", hotelCategories) : String.Empty,
                    hotels != null ? String.Join("_", hotels) : String.Empty, 
                    meals != null ? String.Join("_", meals) : String.Empty, 
                    currencyId.ToString(), 
                    priceMin.ToString(), 
                    priceMax.ToString(), 
                    hotelsIsNotInStop.ToString(), 
                    ticketsIncluded.ToString(), 
                    hasTickets.ToString()
                }));
            if ((result = CacheHelper.GetCacheItem<Tuple<SearchResult, List<Tour>>>(hash)) != null) return result;

            string hashOut;
            var cacheDependencies = new List<string>();
            var searchResult = searchDc.GetSearchResult(mainDc, sftDc, null, count, countryId, departCityId, dateFrom,
                dateTo, adults, kids, kidsAges, nightsMin, nightsMax, resorts, hotelCategories, hotels, meals,
                currencyId, priceMin, priceMax, hotelsIsNotInStop, ticketsIncluded, hasTickets, out hashOut);
            cacheDependencies.Add(hashOut);
            var tours = searchDc.ConvertSearchItemsToTours(mainDc, searchResult.SearchItems, currencyId);

            result = new Tuple<SearchResult, List<Tour>>(searchResult, tours);

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        private static SearchResult GetSearchResult(this MtSearchDbDataContext searchDc, MtMainDbDataContext mainDc, SftWebDbDataContext sftDc, long? offerId, int currencyId, out string hash)
        {
            SearchResult result;
            hash = String.Format("{0}_{1}_{2}", MethodBase.GetCurrentMethod().Name, offerId, currencyId);
            if ((result = CacheHelper.GetCacheItem<SearchResult>(hash)) != null) return result;

            var cacheDependencies = new List<string>();
            string hashOut;

            result = searchDc.GetSearchResult(mainDc, sftDc, offerId, 0, 0, 0, DateTime.Now, DateTime.Now, 0, 0, null, 0, 0, null,
                null, null, null, currencyId, null, null, null, null, null, out hashOut);
            cacheDependencies.Add(hashOut);

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        private static SearchResult GetSearchResult(this MtSearchDbDataContext searchDc, MtMainDbDataContext mainDc, SftWebDbDataContext sftDc, long? offerId, int count, int countryId, int departCityId, DateTime dateFrom, DateTime dateTo, ushort adults, ushort kids, IEnumerable<ushort> kidsAges, int nightsMin, int nightsMax, IEnumerable<int> resorts, IEnumerable<int> hotelCategories, IEnumerable<int> hotels, IEnumerable<int> meals, int currencyId, uint? priceMin, uint? priceMax, int? hotelsIsNotInStop, int? ticketsIncluded, int? hasTickets, out string hash)
        {
            SearchResult result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name,
                CacheHelper.GetCacheKeyHashed(new[]
                {
                    offerId.ToString(),
                    count.ToString(CultureInfo.InvariantCulture), 
                    countryId.ToString(CultureInfo.InvariantCulture), 
                    departCityId.ToString(CultureInfo.InvariantCulture), 
                    dateFrom.ToString(CultureInfo.InvariantCulture), 
                    dateTo.ToString(CultureInfo.InvariantCulture), 
                    adults.ToString(CultureInfo.InvariantCulture), 
                    kids.ToString(CultureInfo.InvariantCulture), 
                    kidsAges != null ? String.Join("_", kidsAges) : String.Empty,
                    nightsMin.ToString(CultureInfo.InvariantCulture), 
                    nightsMax.ToString(CultureInfo.InvariantCulture), 
                    resorts != null ? String.Join("_", resorts) : String.Empty, 
                    hotelCategories != null ? String.Join("_", hotelCategories) : String.Empty,  
                    hotels != null ? String.Join("_", hotels) : String.Empty, 
                    meals != null ? String.Join("_", meals) : String.Empty, 
                    currencyId.ToString(CultureInfo.InvariantCulture), 
                    priceMin.ToString(), 
                    priceMax.ToString(), 
                    hotelsIsNotInStop.ToString(), 
                    ticketsIncluded.ToString(), 
                    hasTickets.ToString()
                }));
            if ((result = CacheHelper.GetCacheItem<SearchResult>(hash)) != null) return result;


            result = new SearchResult();
            var cacheDependencies = new List<string>();
            string hashOut;
            var wrongQuery = false;
            if (offerId == null)
            {
                var tourTypes = searchDc.GetTourTypes(sftDc, departCityId, countryId, out hashOut).Keys.ToList();
                cacheDependencies.Add(hashOut);
                if (tourTypes == null || !tourTypes.Any())
                {
                    wrongQuery = true;
                    CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
                    return result;
                }

                if (resorts == null || !resorts.Any())
                {
                    resorts = searchDc.GetCitiesTo(sftDc, departCityId, countryId, tourTypes, out hashOut).Keys.ToList();
                    cacheDependencies.Add(hashOut);
                }
                if (resorts == null || !resorts.Any())
                {
                    wrongQuery = true;
                    CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
                    return result;
                }

                IList<int> tourKeys =
                    searchDc.GetTours(sftDc, departCityId, countryId, tourTypes, resorts, out hashOut).Keys.ToList();
                cacheDependencies.Add(hashOut);
                if (tourKeys == null || !tourKeys.Any())
                {
                    wrongQuery = true;
                    CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
                    return result;
                }

                IList<DateTime> tourDates =
                    searchDc.GetTourDates(sftDc, departCityId, countryId, resorts, tourKeys, out hashOut)
                        .Where(d => d >= dateFrom && d <= dateTo)
                        .ToList();
                cacheDependencies.Add(hashOut);
                if (tourDates == null || !tourDates.Any())
                {
                    wrongQuery = true;
                    CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
                    return result;
                }

                IList<int> tourNights =
                    searchDc.GetTourNights(sftDc, departCityId, countryId, resorts, tourKeys, tourDates, out hashOut)
                        .Where(n => n >= nightsMin && n <= nightsMax)
                        .ToList();
                cacheDependencies.Add(hashOut);
                if (tourNights == null || !tourNights.Any())
                {
                    wrongQuery = true;
                    CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
                    return result;
                }

                IList<string> hotelStars;
                if (hotelCategories == null || !hotelCategories.Any())
                {
                    hotelStars =
                        searchDc.GetTourHotelClasses(sftDc, departCityId, countryId, resorts, tourKeys, tourDates,
                            tourNights, out hashOut).ToList();
                    cacheDependencies.Add(hashOut);
                }
                else
                {
                    hotelStars = mainDc.GetAllHotelCats()
                            .Where(c => hotelCategories.Contains(c.COH_Id))
                            .Select(c => c.COH_Name)
                            .ToList();
                    cacheDependencies.Add(HotelCategoriesExtension.TableName);
                }
                if (hotelStars == null || !hotelStars.Any())
                {
                    wrongQuery = true;
                    CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
                    return result;
                }

                if (meals == null || !meals.Any())
                {
                    meals =
                        searchDc.GetTourPansions(sftDc, departCityId, countryId, resorts, tourKeys, tourDates,
                            tourNights,
                            hotelStars, out hashOut).Keys.ToList();
                    cacheDependencies.Add(hashOut);
                }
                if (meals == null || !meals.Any())
                {
                    wrongQuery = true;
                    CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
                    return result;
                }

                if (hotels == null || !hotels.Any())
                {
                    hotels =
                        searchDc.GetTourHotels(sftDc, departCityId, countryId, resorts, tourKeys, tourDates,
                            tourNights, hotelStars, meals, out hashOut)
                            .Select(h => h.Key)
                            .ToList();
                    cacheDependencies.Add(hashOut);
                }
                if (hotels == null || !hotels.Any())
                {
                    wrongQuery = true;
                    CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
                    return result;
                }

                IEnumerable<int> roomKeys = null;
                // если по стране цена за человека, меняем алгоритм
                if (searchDc.IsCountryPriceForMen(countryId))
                {
                    var rooms = searchDc.GetTourRooms(sftDc, departCityId, countryId, resorts, tourKeys,
                        tourDates, tourNights, hotels, meals);
                    roomKeys = (from r in searchDc.GetAllRooms()
                        where rooms.Keys.Contains(r.RM_KEY)
                              && r.RM_NPLACES == adults
                        select r.RM_KEY)
                        .ToList();
                }

                ushort? firstChildYears = null, secondChildYears = null;
                if (kidsAges != null)
                {
                    if (kidsAges.Any())
                        firstChildYears = kidsAges.ElementAtOrDefault(0);
                    if (kidsAges.Count() > 1)
                        secondChildYears = kidsAges.ElementAtOrDefault(1);
                }
                var hotelQuotaMask = QuotesStates.Request | QuotesStates.No | QuotesStates.Availiable;
                if (hotelsIsNotInStop != null && hotelsIsNotInStop == 1)
                    hotelQuotaMask = hotelQuotaMask - (byte)QuotesStates.No;

                var aviaQuotaMask = QuotesStates.None | QuotesStates.Request | QuotesStates.No | QuotesStates.Availiable;
                if (ticketsIncluded != null && ticketsIncluded == 1)
                {
                    aviaQuotaMask = aviaQuotaMask - (byte) QuotesStates.None;
                }

                if (hasTickets != null && hasTickets == 1)
                    aviaQuotaMask = aviaQuotaMask - (byte) QuotesStates.No - (byte) QuotesStates.Request;

                result = searchDc.PagingOnClient(mainDc, departCityId, countryId, tourKeys, tourDates, tourNights,
                    hotels, meals,
                    adults, kids, firstChildYears, secondChildYears, roomKeys, hotelQuotaMask, aviaQuotaMask,
                    currencyId, priceMin, priceMax, (ushort)count, 0,
                    new Tuple<SortingColumn, SortingDirection>(SortingColumn.Price, SortingDirection.Asc), out hashOut);
                cacheDependencies.Add(hashOut);
            }
            else
            {
                var countryCityKeys = mainDc.GetCountryCityKeysByTourKey((int)offerId.Value, out hashOut);
                cacheDependencies.Add(hashOut);

                result = searchDc.PagingOnClient(mainDc, countryCityKeys.Item2, countryCityKeys.Item1, offerId.Value, out hashOut);
                cacheDependencies.Add(hashOut);
            }

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        private static List<Tour> ConvertSearchItemsToTours(this MtSearchDbDataContext searchDc, MtMainDbDataContext mainDc, IEnumerable<SearchResultItem> resultItems, int currencyId)
        {
            var result = new List<Tour>(resultItems.Count());

            foreach (var item in resultItems)
            {
                int hotelCategoryKey;
                using (var mainContext = new MtMainDbDataContext())
                {
                    hotelCategoryKey = mainContext.GetAllHotelCats()
                            .Where(c => c.COH_Name == item.Hotels[0].Stars)
                            .Select(c => c.COH_Id)
                            .SingleOrDefault();
                }
                var description = searchDc.GetTourDescription(item.TourKey);
                //todo: убрать ссылку на солвекс, добавить настройку в web.config
                result.Add(new Tour
                {
                    OfferId = item.PriceKey,
                    TourName = item.TourName,
                    HotelId = item.Hotels[0].Key,
                    HotelUrl = item.Hotels[0].Url,
                    ResortId = item.Hotels[0].ResortKey,
                    HotelCategoryId = hotelCategoryKey,
                    MealId = item.Hotels[0].PansionKey,
                    HtPlaceName = item.Hotels[0].AccomodationName,
                    RoomTypeName = item.Hotels[0].RoomCategoryName,
                    TourDate = item.Date,
                    Nights = item.DateTourEnd.Subtract(item.Date).Days,
                    Price = (int)Math.Round((double)searchDc.GetRateRealCourse(DateTime.Now.Date, searchDc.GetRateKeyByCode(item.RateCode), currencyId) * item.Price),
                    HotelInStop = (int)Converters.GetHotelQuota(item.Hotels.Select(h => h.QuotaState.QuotaState).ToList()),
                    TicketsIncluded = (int)Converters.GetTicketsInPrice(item.DirectFlightsInfo.Values.Select(q => q.QuotaState.QuotaState).ToList(), item.BackFlightsInfo.Values.Select(q => q.QuotaState.QuotaState).ToList()),
                    HasEconomTicketsDpt = (int)Converters.GetCharterQuota(item.DirectFlightsInfo.Count > 0 ? item.DirectFlightsInfo[0].QuotaState.QuotaState : QuotesStates.None),
                    HasEconomTicketsRtn = (int)Converters.GetCharterQuota(item.DirectFlightsInfo.Count > 0 ? item.BackFlightsInfo[0].QuotaState.QuotaState : QuotesStates.None),
                    HasBusinessTicketsDpt = (int)Converters.GetCharterQuota(item.DirectFlightsInfo.Count > 1 ? item.DirectFlightsInfo[1].QuotaState.QuotaState : QuotesStates.None),
                    HasBusinessTicketsRtn = (int)Converters.GetCharterQuota(item.DirectFlightsInfo.Count > 1 ? item.BackFlightsInfo[1].QuotaState.QuotaState : QuotesStates.None),
                    TourUrl = String.Format("http://online.solvex.travel/SimpleBasket.aspx?priceKey={0}&date={1}", item.PriceKey, item.Date.ToString("yyyy-MM-dd")),
                    SpoUrl = item.TourUrl,
                    FewPlacesInHotel = Converters.IsFewPlaces(item.Hotels.Select(q => q.QuotaState).ToList()),
                    FewTicketsDptY = item.DirectFlightsInfo.Count > 0 ? Converters.IsFewPlaces(new List<QuotaStatePlaces>() { item.DirectFlightsInfo[0].QuotaState }) : null,
                    FewTicketsDptB = item.DirectFlightsInfo.Count > 1 ? Converters.IsFewPlaces(new List<QuotaStatePlaces>() { item.DirectFlightsInfo[1].QuotaState }) : null,
                    FewTicketsRtnY = item.DirectFlightsInfo.Count > 0 ? Converters.IsFewPlaces(new List<QuotaStatePlaces>() { item.BackFlightsInfo[0].QuotaState }) : null,
                    FewTicketsRtnB = item.DirectFlightsInfo.Count > 1 ? Converters.IsFewPlaces(new List<QuotaStatePlaces>() { item.BackFlightsInfo[1].QuotaState }) : null,
                    Flags = item.Hotels.Count > 1 ? (long)Flags.Combined : 0 | (long)Flags.EarlyBooking,
                    Description = description,
                    EarlyBookingValidTill = item.GetTourValidTo()
                });
            }

            return result;
        }

        public static ActualizedTour ActualizeTour(this MtSearchDbDataContext searchDc, MtMainDbDataContext mainDc, SftWebDbDataContext sftDc, long offerId, int currencyId, out string hash)
        {
            ActualizedTour result;
            hash = String.Format("{0}_{1}_{2}", MethodBase.GetCurrentMethod().Name, offerId, currencyId);
            if ((result = CacheHelper.GetCacheItem<ActualizedTour>(hash)) != null)
                return result;

            string hashOut;
            var searchResult = searchDc.GetSearchResult(mainDc, sftDc, offerId, currencyId, out hashOut);
            var tour = searchDc.ConvertSearchItemsToTours(mainDc, searchResult.SearchItems, currencyId);

            if (tour.Count == 0)
                throw new ApplicationException(MethodBase.GetCurrentMethod().Name + 
                    ". Данных по цене в БД не найдено. Параметры: " + String.Format("{0}_{1}", offerId, currencyId));

            string rateCode = searchDc.GetRateCodeByKey(currencyId);
            
            var cacheDependencies = new List<string> {hashOut};
            result = new ActualizedTour
            {
                FewBusinessTicketsDpt = tour[0].FewTicketsDptB,
                FewBusinessTicketsRtn = tour[0].FewTicketsRtnB,
                FewEconomTicketsDpt = tour[0].FewTicketsDptY,
                FewEconomTicketsRtn = tour[0].FewTicketsRtnY,
                FewPlacesInHotel = tour[0].FewPlacesInHotel,
                TicketsIsIncluded = tour[0].TicketsIncluded,
                HasBusinessTicketsDpt = tour[0].HasBusinessTicketsDpt,
                HasBusinessTicketsRtn = tour[0].HasBusinessTicketsRtn,
                HasEconomTicketsDpt = tour[0].HasEconomTicketsDpt,
                HasEconomTicketsRtn = tour[0].HasEconomTicketsRtn,
                TourUrl = tour[0].TourUrl,
                Price = tour[0].Price
            };

            var parameters = new List<MtServiceParams>();
            var commandBuilder = new StringBuilder();
            commandBuilder.AppendLine("select distinct ts_key, ts_svkey, ts_name, ts_day, ts_days, ts_attribute, ts_code, ts_subcode1, ts_subcode2, ts_ctkey, ts_men, ts_oppacketkey, ts_oppartnerkey ");
            commandBuilder.AppendLine("from tp_services ");
            commandBuilder.AppendLine(String.Format("where ts_Key in (select tl_tskey from tp_servicelists where tl_tikey in (select tp_tikey from tp_prices where tp_key = {0})) ", tour[0].OfferId));
            commandBuilder.AppendLine("order by ts_day asc ");

            using (var command = mainDc.Connection.CreateCommand())
            {
                command.CommandText = commandBuilder.ToString();

                mainDc.Connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var pars = new MtServiceParams()
                        {
                            Code = reader.GetInt32("ts_code"),
                            CtKey = reader.GetInt32("ts_ctkey"),
                            Day = reader.GetInt16("ts_day"),
                            Days = reader.GetInt16("ts_days"),
                            Men = reader.GetInt16("ts_men"),
                            PkKey = reader.GetInt32("ts_oppacketkey"),
                            PrKey = reader.GetInt32("ts_oppartnerkey"),
                            SubCode1 = reader.GetInt32("ts_subcode1"),
                            SubCode2 = reader.GetInt32("ts_subcode2"),
                            SvKey = reader.GetInt32("ts_svkey"),
                            Key = reader.GetInt32("ts_key"),
                            Name = reader.GetString("ts_name"),
                            Attribute = reader.GetInt32("ts_attribute")
                        };
                        parameters.Add(pars);
                    }
                }
                mainDc.Connection.Close();
            }
            

            foreach (var pars in parameters)
            {
                var type = Converters.ServiceClassToServiceType(pars.SvKey, pars.Day);
                if (type == ServiceType.Undefined) continue;

                var service = new Service
                {
                    Id = pars.Key,
                    Description = String.Empty,
                    Name = pars.Name,
                    IsIncluded = (pars.Attribute & (int)ServiceAttribute.NotCalculate) != (int)ServiceAttribute.NotCalculate,
                    Type = type
                };

                if (!service.IsIncluded)
                {
                    var brutto = mainDc.GetServiceCost(pars.SvKey, pars.Code, pars.SubCode1, pars.SubCode2, pars.PrKey,
                    pars.PkKey, searchResult.SearchItems[0].Date.AddDays(pars.Day - 1), pars.Days, rateCode, pars.Men, out hashOut);
                    if (brutto.HasValue)
                        service.Surcharge = (int)brutto;
                    else
                        service.Surcharge = 0;

                    cacheDependencies.Add(hashOut);
                }

                FlightPlainInfo flightInfo = null;
                QuotaStatePlaces quotaStatePlaces = null;
                // услуга - перелет туда и в поиске у нас есть о нем информация
                if (type == ServiceType.DptTransport && pars.SubCode2 == Globals.Settings.HomeCityKey && searchResult.SearchItems[0].FlightCalcInfo.CharterKey.HasValue)
                {
                    service.FlightClass = Converters.CharterClassToFlightClass(pars.SubCode1);

                    switch (service.FlightClass)
                    {
                        case FlightClass.Econom:
                            quotaStatePlaces = searchResult.SearchItems[0].DirectFlightsInfo[0].QuotaState;
                            break;
                        case FlightClass.Business:
                            quotaStatePlaces = searchResult.SearchItems[0].DirectFlightsInfo[1].QuotaState;
                            break;
                    }
                    if (quotaStatePlaces != null)
                    {
                        service.FlightAvailability = Converters.QuotaStateToQuotaAvailability(quotaStatePlaces.QuotaState);
                        if (quotaStatePlaces.QuotaState == QuotesStates.Small)
                            service.FlightPlacesCount = (int) quotaStatePlaces.Places;
                    }

                    if (searchResult.SearchItems[0].DirectFlightsInfo.Count > 0)
                    {
                        service.FlightStartDateTime = searchResult.SearchItems[0].DirectFlightsInfo[0].FlightDateTimeFrom;
                        service.FlightEndDateTime = searchResult.SearchItems[0].DirectFlightsInfo[0].FlightDateTimeTo;
                    }

                    flightInfo = searchResult.SearchItems[0].DirectFlightsInfo[0];
                }
                else if (type == ServiceType.RtnTransport && pars.CtKey == Globals.Settings.HomeCityKey && searchResult.SearchItems[0].FlightCalcInfo.BackCharterKey.HasValue)
                {
                    service.FlightClass = Converters.CharterClassToFlightClass(pars.SubCode1);

                    switch (service.FlightClass)
                    {
                        case FlightClass.Econom:
                            quotaStatePlaces = searchResult.SearchItems[0].BackFlightsInfo[0].QuotaState;
                            break;
                        case FlightClass.Business:
                            quotaStatePlaces = searchResult.SearchItems[0].BackFlightsInfo[1].QuotaState;
                            break;
                    }

                    if (searchResult.SearchItems[0].BackFlightsInfo.Count > 0)
                    {
                        service.FlightStartDateTime = searchResult.SearchItems[0].BackFlightsInfo[0].FlightDateTimeFrom;
                        service.FlightEndDateTime = searchResult.SearchItems[0].BackFlightsInfo[0].FlightDateTimeTo;
                    }

                    flightInfo = searchResult.SearchItems[0].BackFlightsInfo[0];
                }

                if (quotaStatePlaces != null)
                {
                    service.FlightAvailability = Converters.QuotaStateToQuotaAvailability(quotaStatePlaces.QuotaState);
                    if (quotaStatePlaces.QuotaState == QuotesStates.Small)
                        service.FlightPlacesCount = (int)quotaStatePlaces.Places;
                }

                if (flightInfo != null)
                {
                    service.FlightAirportFrom = flightInfo.AirportFrom;
                    service.FlightAirportTo = flightInfo.AirportTo;
                    service.FlightNum = flightInfo.AirlineCode + " " + flightInfo.FlightNumber;
                    service.FlightAirline = flightInfo.AirlineCode;
                    service.FlightAircraft = flightInfo.AircraftCode;
                }
                result.Services.Add(service);
            }

            foreach (var service in result.Services)
            {
                int compId = 0;
                switch (service.Type)
                {
                    case ServiceType.DptTransport:
                    {
                        var service1 = service;
                        compId = result.Services.Where(s => s.Type == ServiceType.RtnTransport
                                                                                 && s.FlightAirportFrom == service1.FlightAirportTo
                                                                                 && s.FlightAirportTo == service1.FlightAirportFrom)
                            .Select(s => s.Id)
                            .SingleOrDefault();
                    }
                        break;
                    case ServiceType.RtnTransport:
                    {
                        var service1 = service;
                        compId = result.Services.Where(s => s.Type == ServiceType.DptTransport
                                                                                 && s.FlightAirportFrom == service1.FlightAirportTo
                                                                                 && s.FlightAirportTo == service1.FlightAirportFrom)
                            .Select(s => s.Id)
                            .SingleOrDefault();
                    }
                        break;
                }
                service.FlightCompatibleIds = compId != 0 ? compId.ToString() : String.Empty;
            }

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }
    }
}
