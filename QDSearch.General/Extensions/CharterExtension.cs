using QDSearch.DataModel;
using QDSearch.Helpers;
using QDSearch.Repository.MtMain;
using QDSearch.Repository.MtSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Caching;

namespace QDSearch.Extensions
{
    /// <summary>
    /// Класс по работе с перелетами
    /// </summary>
    public static class CharterExtension
    {
        private static readonly object LockCharters = new object();
        /// <summary>
        /// Название таблицы
        /// </summary>
        public const string TableName = "Charter";

        /// <summary>
        /// Возвращает список перелетов
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <returns></returns>
        public static List<Charter> GetAllCharters(this MtSearchDbDataContext dc)
        {
            List<Charter> charters;
            if ((charters = CacheHelper.GetCacheItem<List<Charter>>(TableName)) != default(List<Charter>)) return charters;
            lock (LockCharters)
            {
                if ((charters = CacheHelper.GetCacheItem<List<Charter>>(TableName)) != default(List<Charter>)) return charters;

                charters = (from c in dc.Charters
                            select c)
                    .ToList<Charter>();

                CacheHelper.AddCacheData(TableName, charters, TableName);
            }
            return charters;
        }

        /// <summary>
        /// Возвращает объект типа перелет по его ключу
        /// </summary>
        /// <param name="dc">Контекст БД</param>
        /// <param name="charterKey">Ключ перелета</param>
        /// <returns></returns>
        public static Charter GetCharter(this MtSearchDbDataContext dc, int charterKey)
        {
            Charter result;
            var hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, charterKey);
            var cacheDependencies = new List<string>
            {
                String.Format("{0}_{1}_{2}", CacheHelper.CharterHash, (int) ServiceClass.Flight, charterKey)
            };

            if ((result = CacheHelper.GetCacheItem<Charter>(hash)) != default(Charter))
            {
                return result;
            }

            result = (from c in dc.GetAllCharters()
                      where c.CH_KEY == charterKey
                      select c)
                .SingleOrDefault();

            cacheDependencies.Add(TableName);
            CacheHelper.AddCacheData(hash, result, cacheDependencies, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Возвращает объект типа перелет по его ключу
        /// </summary>
        /// <param name="dc">Контекст БД</param>
        /// <param name="cityKeyFrom">Ключ города "туда"</param>
        /// <param name="cityKeyTo">Ключ города "обратно"</param>
        /// <returns></returns>
        public static IList<Charter> GetChartersByDirection(this MtSearchDbDataContext dc, int cityKeyFrom, int cityKeyTo)
        {
            List<Charter> result;
            var hash = String.Format("{0}_{1}_{2}", MethodBase.GetCurrentMethod().Name, cityKeyFrom, cityKeyTo);
            if ((result = CacheHelper.GetCacheItem<List<Charter>>(hash)) != default(List<Charter>))
            {
                return result;
            }

            var cacheDependencies = new List<string>
            {
                String.Format("{0}_{1}_{2}_{3}", CacheHelper.CharterHash, (int)ServiceClass.Flight, cityKeyFrom, cityKeyTo)
            };

            result = (from c in dc.GetAllCharters()
                      where c.CH_CITYKEYFROM == cityKeyFrom
                      && c.CH_CITYKEYTO == cityKeyTo
                      select c)
                .ToList();

            cacheDependencies.Add(TableName);

            CacheHelper.AddCacheData(hash, result, cacheDependencies, Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Получаем направление по ключу перелета
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="charterKey">Ключ перелета</param>
        /// <param name="cityKeyFrom">Город отправления</param>
        /// <param name="cityKeyTo">Город прилета</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void GetCharterCityDirection(this MtSearchDbDataContext dc, int charterKey, out int? cityKeyFrom, out int? cityKeyTo)
        {
            Tuple<int?, int?> result;

            var hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, charterKey);
            if ((result = CacheHelper.GetCacheItem<Tuple<int?, int?>>(hash)) != null)
            {
                cityKeyFrom = result.Item1;
                cityKeyTo = result.Item2;
                return;
            }

            var cacheDependencies = new List<string>
            {
                String.Format("{0}_{1}_{2}", CacheHelper.CharterHash, (int)ServiceClass.Flight, charterKey)
            };

            var charter = dc.GetCharter(charterKey);
            if (charter == null)
            {
                cityKeyFrom = cityKeyTo = null;
            }
            else
            {
                cityKeyFrom = charter.CH_CITYKEYFROM;
                cityKeyTo = charter.CH_CITYKEYTO;
            }

            var res = new Tuple<int?, int?>(cityKeyFrom, cityKeyTo);
            CacheHelper.AddCacheData(hash, res, cacheDependencies, Globals.Settings.Cache.LongCacheTimeout);
        }

        /// <summary>
        /// Возвращает список альтернативных перелетов
        /// </summary>
        /// <param name="dc">Контекст БД</param>
        /// <param name="mainDc">Контекст основной БД</param>
        /// <param name="charterKey">Ключ перелета</param>
        /// <param name="charterDate">Дата перелета</param>
        /// <param name="packetKey">Ключ пакета</param>
        /// <param name="flightGroups">Группы перелета (эконом, бизнес, премиум)</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static Dictionary<int, List<FlightPlainInfo>> GetAltCharters(
            this MtSearchDbDataContext dc, MtMainDbDataContext mainDc, int charterKey, DateTime charterDate,
            int packetKey, IDictionary<int, IEnumerable<int>> flightGroups, out string hash)
        {
            return dc.GetAltCharters(mainDc, charterKey, null, null, charterDate,
                packetKey, flightGroups, out hash);
        }

        /// <summary>
        /// Возвращает список альтернативных перелетов
        /// </summary>
        /// <param name="dc">Контекст БД</param>
        /// <param name="mainDc">Контекст основной БД</param>
        /// <param name="cityKeyFrom">Ключ города вылета</param>
        /// <param name="cityKeyTo">Ключ города прилета</param>
        /// <param name="charterDate">Дата перелета</param>
        /// <param name="packetKey">Ключ пакета</param>
        /// <param name="flightGroups">Группы перелета (эконом, бизнес, премиум)</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static Dictionary<int, List<FlightPlainInfo>> GetAltCharters(
            this MtSearchDbDataContext dc, MtMainDbDataContext mainDc, int cityKeyFrom, int cityKeyTo, DateTime charterDate,
            int packetKey, IDictionary<int, IEnumerable<int>> flightGroups, out string hash)
        {
            return dc.GetAltCharters(mainDc, null, cityKeyFrom, cityKeyTo, charterDate,
                packetKey, flightGroups, out hash);
        }

        ///// <summary>
        ///// Возвращает список альтернативных перелетов
        ///// </summary>
        ///// <param name="dc">Контекст БД</param>
        ///// <param name="mainDc">Контекст основной БД</param>
        ///// <param name="cityKeyFrom">Ключ города вылета</param>
        ///// <param name="cityKeyTo">Ключ города прилета</param>
        ///// <param name="charterDate">Дата перелета</param>
        ///// <param name="flightGroups">Группы перелета (эконом, бизнес, премиум)</param>
        ///// <param name="hash">Хэш кэша</param>
        ///// <returns></returns>
        //public static Dictionary<int, List<FlightPlainInfo>> GetAltCharters(
        //    this MtSearchDbDataContext dc, MtMainDbDataContext mainDc, int cityKeyFrom, int cityKeyTo, DateTime charterDate,
        //    IDictionary<int, IEnumerable<int>> flightGroups, out string hash)
        //{
        //    return dc.GetAltCharters(null, cityKeyFrom, cityKeyTo, charterDate,
        //        flightGroups, out hash);
        //}

        /// <summary>
        /// Возвращает список альтернативных перелетов
        /// </summary>
        /// <param name="dc">Контекст БД</param>
        /// <param name="mainDc">Контекст основной БД</param>
        /// <param name="charterKey">Ключ перелета</param>
        /// <param name="cityKeyFrom">Ключ города вылета</param>
        /// <param name="cityKeyTo">Ключ города прилета</param>
        /// <param name="charterDate">Дата перелета</param>
        /// <param name="packetKey">КЛюч пакета</param>
        /// <param name="flightGroups">Группы перелета (эконом, бизнес, премиум)</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        private static Dictionary<int, List<FlightPlainInfo>> GetAltCharters(this MtSearchDbDataContext dc, MtMainDbDataContext mainDc, int? charterKey, int? cityKeyFrom, int? cityKeyTo, 
            DateTime charterDate, int packetKey, IDictionary<int, IEnumerable<int>> flightGroups, out string hash)
        {
            if (!charterKey.HasValue && !cityKeyFrom.HasValue && !cityKeyTo.HasValue)
                throw new ArgumentNullException(MethodBase.GetCurrentMethod().Name,
                    "Все три параметра не могут быть одновременно null");

            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name,
                CacheHelper.GetCacheKeyHashed(new[]
                {
                    charterKey.ToString(),
                    cityKeyFrom.ToString(),
                    cityKeyTo.ToString(),
                    charterDate.ToString("yyyy-MM-dd"), 
                    packetKey.ToString(), 
                    String.Join("_", flightGroups.Keys), 
                    String.Join("_", flightGroups.Values)
                }));

            Dictionary<int, List<FlightPlainInfo>> altCharters;
            if ((altCharters = CacheHelper.GetCacheItem<Dictionary<int, List<FlightPlainInfo>>>(hash)) != default(Dictionary<int, List<FlightPlainInfo>>))
                return altCharters;

            var cacheDependencies = new List<string>();
            if (charterKey.HasValue)
            {
                cacheDependencies.Add(String.Format("{0}_{1}_{2}", CacheHelper.AirSeasonHash, (int)ServiceClass.Flight, charterKey));
                cacheDependencies.Add(String.Format("{0}_{1}_{2}", CacheHelper.CostHash, (int)ServiceClass.Flight, charterKey));
                cacheDependencies.Add(String.Format("{0}_{1}_{2}", CacheHelper.CharterHash, (int)ServiceClass.Flight, charterKey));
            }
            if (cityKeyFrom.HasValue && cityKeyTo.HasValue)
            {
                cacheDependencies.Add(String.Format("{0}_{1}_{2}", CacheHelper.CharterDirectionHash, cityKeyFrom, cityKeyTo));
            }
            string hashOut;

            var charterKeys = new List<int>();
            var charters = new List<Charter>();
            if (charterKey != null)
            {
                charters.Add(dc.GetCharter(charterKey.Value));
                charterKeys.Add(charterKey.Value);
            }
            else if (cityKeyFrom != null && cityKeyTo != null)
            {
                charters.AddRange(dc.GetChartersByDirection(cityKeyFrom.Value, cityKeyTo.Value));
                charterKeys.AddRange(charters.Select(c => c.CH_KEY).ToList());
            }

            var airSeasons = dc.GetAllAirSeasonsByCharterKeys(charterKeys, out hashOut);
            cacheDependencies.Add(hashOut);

            var costs = dc.GetFlightCostsByPacketKey(packetKey, out hashOut)
                .Where(c => (c.CS_DATE == null || c.CS_DATE <= charterDate)
                    && (c.CS_DATEEND == null || c.CS_DATEEND >= charterDate)
                    && charterKeys.Contains(c.CS_CODE)).ToList();
            cacheDependencies.Add(hashOut);
           
            var airlines = mainDc.GetAllAirlinesByCharterKeys(charters.Select(c => c.CH_AIRLINECODE).Distinct().ToList(), out hashOut);
            cacheDependencies.Add(hashOut);

            var airlineServices = dc.GetAirServicesByKeys(costs.Where(c => c.CS_SUBCODE1.HasValue).Select(c => c.CS_SUBCODE1.Value).Distinct().ToList(), out hashOut);
            cacheDependencies.Add(hashOut);

            altCharters = new Dictionary<int, List<FlightPlainInfo>>();

            foreach (int key in flightGroups.Keys)
            {
                int key1 = key;

                var chs = (from ch in charters
                           join a in airSeasons on ch.CH_KEY equals a.AS_CHKEY
                           join c in costs on ch.CH_KEY equals c.CS_CODE
                           join al in airlines on ch.CH_AIRLINECODE equals al.AL_CODE
                           join aserv in airlineServices on c.CS_SUBCODE1.Value equals aserv.AS_KEY
                           where (!c.CS_DATEEND.HasValue || (charterDate >= c.CS_DATE.Value && charterDate <= c.CS_DATEEND.Value))
                                 && (!c.CS_CHECKINDATEEND.HasValue || (charterDate >= c.CS_CHECKINDATEBEG.Value && charterDate <= c.CS_CHECKINDATEEND.Value))
                                 && flightGroups[key1].Contains(c.CS_SUBCODE1.Value)
                                 && (c.CS_PKKEY == packetKey)
                                 && (a.AS_WEEK == String.Empty || a.AS_WEEK.IndexOf(Converters.GetIntDayOfWeek(charterDate).ToString()) >= 0)
                                 && charterDate >= a.AS_DATEFROM 
                                 && charterDate <= a.AS_DATETO
                           select new FlightPlainInfo()
                           {
                               AirlineCode = al.AL_CODE,
                               AirlineKey = al.al_key,
                               AirlineName = al.AL_NAME,
                               AirportFrom = ch.CH_PORTCODEFROM,
                               AirportTo = ch.CH_PORTCODETO,
                               CharterKey = ch.CH_KEY,
                               ClassKey = c.CS_SUBCODE1.Value,
                               FlightClassName = aserv.AS_NAMERUS,
                               FlightDateTimeFrom = new DateTime(charterDate.Year, charterDate.Month, charterDate.Day, a.AS_TIMEFROM.HasValue ? a.AS_TIMEFROM.Value.Hour : 0, a.AS_TIMEFROM.HasValue ? a.AS_TIMEFROM.Value.Minute : 0, 0),
                               FlightDateTimeTo = new DateTime(charterDate.Year, charterDate.Month, charterDate.Day, a.AS_TIMETO.HasValue ? a.AS_TIMETO.Value.Hour : 0, a.AS_TIMETO.HasValue ? a.AS_TIMETO.Value.Minute : 0, 0),
                               FlightNumber = ch.CH_FLIGHT,
                               PartnerKey = c.CS_PRKEY,
                               AircraftCode = ch.CH_AIRCRAFT,
                               QuotaState = null
                           })
                    .Distinct(new FlightPlainInfoCompare())
                    .ToList();

                altCharters.Add(key1, chs);
            }

            CacheHelper.AddCacheData(hash, altCharters, null, Globals.Settings.Cache.LongCacheTimeout);
            return altCharters;
        }
    }
}
