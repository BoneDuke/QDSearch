using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using QDSearch;
using QDSearch.Extensions;
using QDSearch.Helpers;
using QDSearch.Repository.MtMain;
using QDSearch.Repository.MtSearch;
using Seemplexity.Logic.Flights.DataModel;
using Seemplexity.Logic.Flights;

namespace Seemplexity.Logic.Flights
{
    public static class AviaBookingLogic
    {
        /// <summary>
        /// Возвращает список авиаперелетов (для перелетов туда / обратно)
        /// </summary>
        /// <param name="dc">Контекст базы данных поисковой</param>
        /// <param name="mainDc">Контекст основой БД</param>
        /// <param name="dateFlightDirect">Дата рейса в город прилета</param>
        /// <param name="dateFlightBack">Дата рейса в город вылета</param>
        /// <param name="hash">Хэш кэша</param>
        /// <param name="cityKeyFrom">Ключ города вылета</param>
        /// <param name="cityKeyTo">Ключ города прилета</param>
        /// <returns></returns>
        public static IList<Tuple<FlightVariant, FlightVariant>> GetFlightsTwoWay(this MtSearchDbDataContext dc, MtMainDbDataContext mainDc, int cityKeyFrom, int cityKeyTo, DateTime dateFlightDirect, DateTime dateFlightBack,
            out string hash)
        {
            List<Tuple<FlightVariant, FlightVariant>> result;
            hash = String.Format("{0}_{1}_{2}_{3}_{4}", MethodBase.GetCurrentMethod().Name, cityKeyFrom, cityKeyTo, dateFlightDirect, dateFlightBack);
            if ((result = CacheHelper.GetCacheItem<List<Tuple<FlightVariant, FlightVariant>>>(hash)) != null) return result;

            var cacheDependencies = new List<string>();
            string hashOut;

            var packets = mainDc.GetFlightPackets(dc, out hashOut)
                .Where(p => p.Item2 == PacketType.TwoWayCharters)
                .ToList();
            cacheDependencies.Add(hashOut);

            var schedule = dc.GetFlightSchedules(mainDc, packets, out hashOut);
            cacheDependencies.Add(hashOut);

            result = schedule.Schedule.Where(s => s.FlightParamsFrom != null && s.FlightParamsTo != null
                                         && s.FlightParamsTo.CityKeyFrom == cityKeyFrom
                                         && s.FlightParamsTo.CityKeyTo == cityKeyTo
                                         && s.FlightParamsTo.DepartTime.Date >= dateFlightDirect

                                         && s.FlightParamsFrom.CityKeyFrom == cityKeyTo
                                         && s.FlightParamsFrom.CityKeyTo == cityKeyFrom
                                         && s.FlightParamsFrom.DepartTime.Date <= dateFlightBack)
                .Select(s => new Tuple<FlightVariant, FlightVariant>(s.FlightParamsTo, s.FlightParamsFrom))
                .Distinct(new FlightParamsByDateTupleComparer())
                .ToList();

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Возвращает список авиаперелетов
        /// </summary>
        /// <param name="dc">Контекст базы данных поисковой</param>
        /// <param name="mainDc">Контекст основой БД</param>
        /// <param name="dateFrom">Дата перелета от</param>
        /// <param name="dateTo">Дата перелета по</param>
        /// <param name="hash">Хэш кэша</param>
        /// <param name="cityKeyFrom">Ключ города вылета</param>
        /// <param name="cityKeyTo">Ключ города прилета</param>
        /// <returns></returns>
        public static IList<FlightVariant> GetFlightsOneWay(this MtSearchDbDataContext dc, MtMainDbDataContext mainDc, int cityKeyFrom, int cityKeyTo, DateTime dateFrom, DateTime dateTo,
            out string hash)
        {
            List<FlightVariant> result;
            hash = String.Format("{0}_{1}_{2}_{3}_{4}", MethodBase.GetCurrentMethod().Name, cityKeyFrom, cityKeyTo, dateFrom, dateTo);
            if ((result = CacheHelper.GetCacheItem<List<FlightVariant>>(hash)) != null) return result;

            var cacheDependencies = new List<string>();
            string hashOut;

            var packets = mainDc.GetFlightPackets(dc, out hashOut)
                .Where(p => p.Item2 != PacketType.TwoWayCharters)
                .ToList();
            cacheDependencies.Add(hashOut);

            var schedule = dc.GetFlightSchedules(mainDc, packets, out hashOut);
            cacheDependencies.Add(hashOut);

            result = schedule.Schedule.Where(s => s.FlightParamsFrom == null && s.FlightParamsTo != null
                                         && s.FlightParamsTo.CityKeyFrom == cityKeyFrom
                                         && s.FlightParamsTo.CityKeyTo == cityKeyTo
                                         && s.FlightParamsTo.DepartTime.Date >= dateFrom
                                         && s.FlightParamsTo.DepartTime.Date <= dateTo)
                .Select(s => s.FlightParamsTo)
                .Distinct(new FlightParamsByDateComparer())
                .ToList();

            result.AddRange(schedule.Schedule.Where(s => s.FlightParamsFrom != null && s.FlightParamsTo == null
                                                     && s.FlightParamsFrom.CityKeyFrom == cityKeyFrom
                                                     && s.FlightParamsFrom.CityKeyTo == cityKeyTo
                                                     && s.FlightParamsFrom.DepartTime.Date >= dateFrom
                                                     && s.FlightParamsFrom.DepartTime.Date <= dateTo)
                .Select(s => s.FlightParamsFrom)
                .Distinct(new FlightParamsByDateComparer())
                .ToList());

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Возвращает список перелетов туда / обратно
        /// </summary>
        /// <param name="dc">Контекст поисковой БД</param>
        /// <param name="mainDc">Контекст основной БД</param>
        /// <param name="cityKeyFrom">Ключ города вылета</param>
        /// <param name="cityKeyTo">Ключ города прилета</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static IList<Tuple<DateTime, DateTime>> GetFlightDatesTwoWay(this MtSearchDbDataContext dc, MtMainDbDataContext mainDc, int cityKeyFrom, int cityKeyTo, out string hash)
        {
            List<Tuple<DateTime, DateTime>> result;
            hash = String.Format("{0}_{1}_{2}", MethodBase.GetCurrentMethod().Name, cityKeyFrom, cityKeyTo);
            if ((result = CacheHelper.GetCacheItem<List<Tuple<DateTime, DateTime>>>(hash)) != null) return result;

            var cacheDependencies = new List<string>();
            string hashOut;

            var packets = mainDc.GetFlightPackets(dc, out hashOut)
                    .Where(p => p.Item2 == PacketType.TwoWayCharters)
                    .ToList();
            cacheDependencies.Add(hashOut);

            var schedule = dc.GetFlightSchedules(mainDc, packets, out hashOut);
            cacheDependencies.Add(hashOut);

            result = schedule.Schedule.Where(s => s.FlightParamsFrom != null && s.FlightParamsTo != null
                                                     && s.FlightParamsTo.CityKeyFrom == cityKeyFrom
                                                     && s.FlightParamsTo.CityKeyTo == cityKeyTo
                                                     && s.FlightParamsFrom.CityKeyFrom == cityKeyTo
                                                     && s.FlightParamsFrom.CityKeyTo == cityKeyFrom)
                .Select(s => new Tuple<DateTime, DateTime>(s.FlightParamsTo.DepartTime.Date, s.FlightParamsFrom.DepartTime.Date))
                .Distinct()
                .ToList();

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Возвращает список дат перелетов односторонних
        /// </summary>
        /// <param name="dc">Контекст поисковой БД</param>
        /// <param name="mainDc">Контекст основной БД</param>
        /// <param name="cityKeyFrom">Ключ города вылета</param>
        /// <param name="cityKeyTo">Ключ города прилета</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static IList<DateTime> GetFlightDatesOneWay(this MtSearchDbDataContext dc, MtMainDbDataContext mainDc, int cityKeyFrom, int cityKeyTo, out string hash)
        {
            List<DateTime> result;
            hash = String.Format("{0}_{1}_{2}", MethodBase.GetCurrentMethod().Name, cityKeyFrom, cityKeyTo);
            if ((result = CacheHelper.GetCacheItem<List<DateTime>>(hash)) != null) return result;

            var cacheDependencies = new List<string>();
            string hashOut;

            var packets = mainDc.GetFlightPackets(dc, out hashOut)
                    .Where(p => p.Item2 != PacketType.TwoWayCharters)
                    .ToList();
            cacheDependencies.Add(hashOut);

            var schedule = dc.GetFlightSchedules(mainDc, packets, out hashOut);
            cacheDependencies.Add(hashOut);

            var dates = schedule.Schedule.Where(s => s.FlightParamsFrom == null && s.FlightParamsTo != null
                                                     && s.FlightParamsTo.CityKeyFrom == cityKeyFrom
                                                     && s.FlightParamsTo.CityKeyTo == cityKeyTo)
                .Select(s => s.FlightParamsTo.DepartTime.Date)
                .Distinct()
                .ToList();

            dates.AddRange(schedule.Schedule.Where(s => s.FlightParamsFrom != null && s.FlightParamsTo == null
                                                     && s.FlightParamsFrom.CityKeyFrom == cityKeyFrom
                                                     && s.FlightParamsFrom.CityKeyTo == cityKeyTo)
                .Select(s => s.FlightParamsFrom.DepartTime.Date)
                .Distinct()
                .ToList());
            result = dates.Distinct().ToList();

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }



        /// <summary>
        /// Возвращает список типов пакетов для авиаперелетов
        /// </summary>
        /// <param name="dc">Контекст базы данных поисковой</param>
        /// <param name="mainDc">Контекст основой БД</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static IList<PacketType> GetFlightDirections(this MtSearchDbDataContext dc, MtMainDbDataContext mainDc, out string hash)
        {
            List<PacketType> result;
            hash = String.Format("{0}", MethodBase.GetCurrentMethod().Name);
            if ((result = CacheHelper.GetCacheItem<List<PacketType>>(hash)) != null) return result;

            var cacheDependencies = new List<string>();
            string hashOut;
            result = mainDc.GetFlightPackets(dc, out hashOut).Select(p => p.Item2).Distinct().ToList();
            cacheDependencies.Add(hashOut);

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Возвращает список стран вылета
        /// </summary>
        /// <param name="dc">Контекст поисковой БД</param>
        /// <param name="mainDc">Контекст основной БД</param>
        /// <param name="isOneWay">Искать по перелетам в одну сторону или туда/обратно</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static IList<Tuple<int, string>> GetFlightCountriesFrom(this MtSearchDbDataContext dc, MtMainDbDataContext mainDc, bool isOneWay, out string hash)
        {
            List<Tuple<int, string>> result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, isOneWay);
            if ((result = CacheHelper.GetCacheItem<List<Tuple<int, string>>>(hash)) != null) return result;

            var cacheDependencies = new List<string>();
            string hashOut;
            var cities = dc.GetFlightCitiesFrom(mainDc, isOneWay, null, out hashOut);
            cacheDependencies.Add(hashOut);

            result = (from cn in dc.GetAllCountries()
                      join ct in dc.GetAllCities() on cn.CN_KEY equals ct.CT_CNKEY
                      where cities.Select(c => c.Item1).ToList().Contains(ct.CT_KEY)
                      select new Tuple<int, string>(cn.CN_KEY, cn.CN_NAME))
                .Distinct()
                .OrderBy(c => c.Item2)
                .ToList();
            cacheDependencies.Add(CitiesExtension.TableName);
            cacheDependencies.Add(CountriesExtension.TableName);

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Возвращает список городов вылета
        /// </summary>
        /// <param name="dc">Контекст поисковой БД</param>
        /// <param name="mainDc">Контекст основной БД</param>
        /// <param name="isOneWay">Является ли перелет в одну сторону или туда/обратно</param>
        /// <param name="countryKey">Страна вылета</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static IList<Tuple<int, string>> GetFlightCitiesFrom(this MtSearchDbDataContext dc, MtMainDbDataContext mainDc, bool isOneWay, int? countryKey, out string hash)
        {
            List<Tuple<int, string>> result;
            hash = String.Format("{0}_{1}_{2}", MethodBase.GetCurrentMethod().Name, isOneWay, countryKey);
            if ((result = CacheHelper.GetCacheItem<List<Tuple<int, string>>>(hash)) != null) return result;

            var cacheDependencies = new List<string>();
            string hashOut;

            var packets =
                mainDc.GetFlightPackets(dc, out hashOut)
                    .Where(
                        p =>
                            !isOneWay && p.Item2 == PacketType.TwoWayCharters ||
                            isOneWay && p.Item2 != PacketType.TwoWayCharters)
                    .ToList();
            cacheDependencies.Add(hashOut);

            var schedule = dc.GetFlightSchedules(mainDc, packets, out hashOut);
            cacheDependencies.Add(hashOut);

            var cities = new List<int>();
            if (isOneWay)
            {
                cities.AddRange(schedule.Schedule.Where(s => s.FlightParamsFrom == null && s.FlightParamsTo != null)
                    .Select(s => s.FlightParamsTo.CityKeyFrom)
                    .Distinct()
                    .ToList());

                cities.AddRange(schedule.Schedule.Where(s => s.FlightParamsTo == null && s.FlightParamsFrom != null)
                    .Select(s => s.FlightParamsFrom.CityKeyFrom)
                    .Distinct()
                    .ToList());
            }
            else
            {
                cities.AddRange(schedule.Schedule.Where(s => s.FlightParamsFrom != null && s.FlightParamsTo != null)
                     .Select(s => s.FlightParamsTo.CityKeyFrom)
                     .Distinct()
                     .ToList());
            }

            result = (from c in dc.GetAllCities()
                      where cities.Contains(c.CT_KEY)
                      && (countryKey == null || c.CT_CNKEY == countryKey.Value)
                      select new Tuple<int, string>(c.CT_KEY, c.CT_NAME))
                .OrderBy(c => c.Item2)
                .ToList();

            cacheDependencies.Add(CitiesExtension.TableName);

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Возвращает список стран прилета
        /// </summary>
        /// <param name="dc">Контекст поисковой БД</param>
        /// <param name="mainDc">Контекст основной БД</param>
        /// <param name="isOneWay">Является ли перелет в одну сторону или туда/обратно</param>
        /// <param name="cityKeyFrom">Ключ города вылета</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static IList<Tuple<int, string>> GetFlightCountriesTo(this MtSearchDbDataContext dc, MtMainDbDataContext mainDc, bool isOneWay, int cityKeyFrom, out string hash)
        {
            List<Tuple<int, string>> result;
            hash = String.Format("{0}_{1}_{2}", MethodBase.GetCurrentMethod().Name, isOneWay, cityKeyFrom);
            if ((result = CacheHelper.GetCacheItem<List<Tuple<int, string>>>(hash)) != null) return result;

            var cacheDependencies = new List<string>();
            string hashOut;
            var cities = dc.GetFlightCitiesTo(mainDc, isOneWay, null, cityKeyFrom, out hashOut);
            cacheDependencies.Add(hashOut);

            result = (from cn in dc.GetAllCountries()
                      join ct in dc.GetAllCities() on cn.CN_KEY equals ct.CT_CNKEY
                      where cities.Select(c => c.Item1).ToList().Contains(ct.CT_KEY)
                      select new Tuple<int, string>(cn.CN_KEY, cn.CN_NAME))
                .Distinct()
                .OrderBy(c => c.Item2)
                .ToList();
            cacheDependencies.Add(CitiesExtension.TableName);
            cacheDependencies.Add(CountriesExtension.TableName);

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Возвращает список городов прилета
        /// </summary>
        /// <param name="dc">Контекст поисковой БД</param>
        /// <param name="mainDc">Контекст основной БД</param>
        /// <param name="isOneWay">Является ли перелет в одну сторону или туда/обратно</param>
        /// <param name="countryKey">Ключ страны прилета</param>
        /// <param name="cityKeyFrom">Ключ города вылета</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static IList<Tuple<int, string>> GetFlightCitiesTo(this MtSearchDbDataContext dc, MtMainDbDataContext mainDc, bool isOneWay, int? countryKey, int cityKeyFrom, out string hash)
        {
            List<Tuple<int, string>> result;
            hash = String.Format("{0}_{1}_{2}_{3}", MethodBase.GetCurrentMethod().Name, isOneWay, countryKey, cityKeyFrom);
            if ((result = CacheHelper.GetCacheItem<List<Tuple<int, string>>>(hash)) != null) return result;

            var cacheDependencies = new List<string>();
            string hashOut;

            var packets =
                mainDc.GetFlightPackets(dc, out hashOut)
                    .Where(
                        p =>
                            !isOneWay && p.Item2 == PacketType.TwoWayCharters ||
                            isOneWay && p.Item2 != PacketType.TwoWayCharters)
                    .ToList();
            cacheDependencies.Add(hashOut);

            var schedule = dc.GetFlightSchedules(mainDc, packets, out hashOut);
            cacheDependencies.Add(hashOut);

            var cities = new List<int>();
            if (isOneWay)
            {
                cities.AddRange(schedule.Schedule.Where(s => s.FlightParamsFrom == null && s.FlightParamsTo != null)
                    .Where(s => s.FlightParamsTo.CityKeyFrom == cityKeyFrom)
                    .Select(s => s.FlightParamsTo.CityKeyTo)
                    .Distinct()
                    .ToList());

                cities.AddRange(schedule.Schedule.Where(s => s.FlightParamsTo == null && s.FlightParamsFrom != null)
                    .Where(s => s.FlightParamsFrom.CityKeyFrom == cityKeyFrom)
                    .Select(s => s.FlightParamsFrom.CityKeyTo)
                    .Distinct()
                    .ToList());
            }
            else
            {
                cities.AddRange(schedule.Schedule.Where(s => s.FlightParamsFrom != null && s.FlightParamsTo != null)
                     .Where(s => s.FlightParamsFrom.CityKeyTo == cityKeyFrom && s.FlightParamsTo.CityKeyFrom == cityKeyFrom)
                     .Select(s => s.FlightParamsTo.CityKeyTo)
                     .Distinct()
                     .ToList());
            }

            result = (from c in dc.GetAllCities()
                      where cities.Contains(c.CT_KEY)
                      && (countryKey == null || c.CT_CNKEY == countryKey.Value)
                      select new Tuple<int, string>(c.CT_KEY, c.CT_NAME))
                .OrderBy(c => c.Item2)
                .ToList();

            cacheDependencies.Add(CitiesExtension.TableName);

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }
    }
}
