using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using QDSearch;
using QDSearch.DataModel;
using QDSearch.Extensions;
using QDSearch.Helpers;
using QDSearch.Repository;
using QDSearch.Repository.MtMain;
using QDSearch.Repository.MtSearch;
using Seemplexity.Logic.Flights.DataModel;
using Seemplexity.Logic.Flights.Extensions;

namespace Seemplexity.Logic.Flights
{
    public static class Logic
    {

        #region private
        /// <summary>
        /// Возвращает сопоставления пар перелетов
        /// </summary>
        /// <param name="dc">Контекст поисковой БД</param>
        /// <param name="mainDc">Контекст основной БД</param>
        /// <param name="packetType">Тип пакета (туда, обратно, туда/обратно)</param>
        /// <param name="flightVariants">Список перелетов</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        private static IEnumerable<DatesMatches> GetFlightMatches(this MtSearchDbDataContext dc, MtMainDbDataContext mainDc, PacketType packetType, IList<FlightVariant> flightVariants, out string hash)
        {
            List<DatesMatches> result;
            var hashBuilder = new StringBuilder();
            foreach (var flightVaraint in flightVariants)
            {
                hashBuilder.AppendFormat("{0}_", flightVaraint);
            }

            hash = String.Format("{0}_{1}_{2}", MethodBase.GetCurrentMethod().Name, packetType, hashBuilder);

            if ((result = CacheHelper.GetCacheItem<List<DatesMatches>>(hash)) != null)
                return result;

            result = new List<DatesMatches>();
            string hashOut;
            var cacheDependencies = new List<string>();
            if ((PacketType.TwoWayCharters & packetType) == packetType)
            {
                var twoWayTickets = (from flightDateTo in flightVariants
                                     join flightDateFrom in flightVariants on flightDateTo.CityKeyTo equals flightDateFrom.CityKeyFrom
                                     where flightDateTo.CityKeyFrom == Globals.Settings.HomeCityKey
                                         && flightDateTo.CityKeyFrom == flightDateFrom.CityKeyTo
                                         && flightDateTo.FlightParamKeys.CharterClassKey == flightDateFrom.FlightParamKeys.CharterClassKey
                                         &&
                                         ((flightDateTo.FlightNumber == flightDateFrom.FlightNumber + 1) ||
                                         (flightDateTo.FlightNumber == flightDateFrom.FlightNumber - 1) ||
                                         (flightDateTo.FlightNumber == flightDateFrom.FlightNumber &&
                                             flightDateTo.FlightParamKeys.CharterKey != flightDateFrom.FlightParamKeys.CharterKey))
                                         && (
                                             ((flightDateTo.ArrivalTime - flightDateTo.ArrivalTime.Date) <= (flightDateFrom.DepartTime - flightDateFrom.DepartTime.Date)
                                             && flightDateTo.ArrivalTime.Date.AddDays(flightDateTo.DurationMin - 1) <= flightDateFrom.DepartTime.Date
                                             && flightDateTo.ArrivalTime.Date.AddDays(flightDateTo.DurationMax - 1) >= flightDateFrom.DepartTime.Date)
                                             || ((flightDateTo.ArrivalTime - flightDateTo.ArrivalTime.Date) >
                                             (flightDateFrom.DepartTime - flightDateFrom.DepartTime.Date) &&
                                             flightDateTo.ArrivalTime.Date.AddDays(flightDateTo.DurationMin - 1) <=
                                             flightDateFrom.DepartTime.Date.AddDays(-1) &&
                                             flightDateTo.ArrivalTime.Date.AddDays(flightDateTo.DurationMax - 1) >=
                                             flightDateFrom.DepartTime.Date.AddDays(-1))
                                             )
                                     select new Tuple<FlightVariant, FlightVariant>(new FlightVariant(flightDateTo), new FlightVariant(flightDateFrom)))
                    .Distinct(new FlightParamsByDateTupleComparer())
                    .ToList();

                foreach (var pair in twoWayTickets)
                {
                    var query = new QuotaPriceQuery();

                    query.FlightParams.Add(new QuotaPriceParams
                    {
                        DepartTime = pair.Item1.DepartTime,
                        Duration = pair.Item2.DepartTime.Date.Subtract(pair.Item1.DepartTime.Date).Days + 1,
                        FlightParamKeys = new FlightVariantKeys
                        {
                            CharterKey = pair.Item1.FlightParamKeys.CharterKey,
                            CharterClassKey = pair.Item1.FlightParamKeys.CharterClassKey,
                            PacketKey = pair.Item1.FlightParamKeys.PacketKey,
                            PartnerKey = pair.Item1.FlightParamKeys.PartnerKey
                        }
                    });
                    query.FlightParams.Add(new QuotaPriceParams
                    {
                        DepartTime = pair.Item2.DepartTime,
                        Duration = pair.Item2.DepartTime.Date.Subtract(pair.Item1.DepartTime.Date).Days + 1,
                        FlightParamKeys = new FlightVariantKeys
                        {
                            CharterKey = pair.Item2.FlightParamKeys.CharterKey,
                            CharterClassKey = pair.Item2.FlightParamKeys.CharterClassKey,
                            PacketKey = pair.Item2.FlightParamKeys.PacketKey,
                            PartnerKey = pair.Item2.FlightParamKeys.PartnerKey
                        }
                    });

                    var quotaPrice = dc.GetFlightQuotaPrices(mainDc, query, out hashOut);
                    if (!cacheDependencies.Contains(hashOut))
                        cacheDependencies.Add(hashOut);

                    if (quotaPrice.FlightData.Count > 1)
                    {
                        pair.Item1.FlightQuotaPriceData = new QuotaPriceData
                        {
                            PriceValue = quotaPrice.FlightData[0].PriceValue,
                            QuotaPlaces = quotaPrice.FlightData[0].QuotaPlaces,
                            QuotaState = quotaPrice.FlightData[0].QuotaState
                        };
                        pair.Item2.FlightQuotaPriceData = new QuotaPriceData
                        {
                            PriceValue = quotaPrice.FlightData[1].PriceValue,
                            QuotaPlaces = quotaPrice.FlightData[1].QuotaPlaces,
                            QuotaState = quotaPrice.FlightData[1].QuotaState
                        };
                        result.Add(new DatesMatches
                        {
                            FlightParamsTo = pair.Item1,
                            FlightParamsFrom = pair.Item2
                        });
                    }
                }
            }
            else if ((PacketType.DirectCharters & packetType) == packetType)
            {
                var directTickets = (from flightDateTo in flightVariants
                                     where flightDateTo.CityKeyFrom == Globals.Settings.HomeCityKey
                                     select flightDateTo)
                    .Distinct(new FlightParamsByDateComparer())
                    .ToList();

                foreach (var flight in directTickets)
                {
                    var query = new QuotaPriceQuery();
                    query.FlightParams.Add(new QuotaPriceParams
                    {
                        DepartTime = flight.DepartTime,
                        FlightParamKeys = new FlightVariantKeys
                        {
                            CharterKey = flight.FlightParamKeys.CharterKey,
                            CharterClassKey = flight.FlightParamKeys.CharterClassKey,
                            PacketKey = flight.FlightParamKeys.PacketKey,
                            PartnerKey = flight.FlightParamKeys.PartnerKey
                        }
                    });

                    var quotaPrice = dc.GetFlightQuotaPrices(mainDc, query, out hashOut);
                    if (!cacheDependencies.Contains(hashOut))
                        cacheDependencies.Add(hashOut);

                    if (quotaPrice.FlightData.Count > 0)
                    {
                        flight.FlightQuotaPriceData = new QuotaPriceData
                        {
                            PriceValue = quotaPrice.FlightData[0].PriceValue,
                            QuotaPlaces = quotaPrice.FlightData[0].QuotaPlaces,
                            QuotaState = quotaPrice.FlightData[0].QuotaState
                        };
                        result.Add(new DatesMatches
                        {
                            FlightParamsTo = flight,
                            FlightParamsFrom = null
                        });
                    }
                }
            }
            else if ((PacketType.BackCharters & packetType) == packetType)
            {
                var backTickets = (from flightDateTo in flightVariants
                                   where flightDateTo.CityKeyTo == Globals.Settings.HomeCityKey
                                   select flightDateTo)
                    .Distinct(new FlightParamsByDateComparer())
                    .ToList();

                foreach (var flight in backTickets)
                {
                    var query = new QuotaPriceQuery();
                    query.FlightParams.Add(new QuotaPriceParams
                    {
                        DepartTime = flight.DepartTime,
                        FlightParamKeys = new FlightVariantKeys
                        {
                            CharterKey = flight.FlightParamKeys.CharterKey,
                            CharterClassKey = flight.FlightParamKeys.CharterClassKey,
                            PacketKey = flight.FlightParamKeys.PacketKey,
                            PartnerKey = flight.FlightParamKeys.PartnerKey
                        }
                    });

                    var quotaPrice = dc.GetFlightQuotaPrices(mainDc, query, out hashOut);
                    if (!cacheDependencies.Contains(hashOut))
                        cacheDependencies.Add(hashOut);

                    if (quotaPrice.FlightData.Count > 0)
                    {
                        flight.FlightQuotaPriceData = new QuotaPriceData
                        {
                            PriceValue = quotaPrice.FlightData[0].PriceValue,
                            QuotaPlaces = quotaPrice.FlightData[0].QuotaPlaces,
                            QuotaState = quotaPrice.FlightData[0].QuotaState
                        };
                        result.Add(new DatesMatches
                        {
                            FlightParamsTo = null,
                            FlightParamsFrom = flight
                        });
                    }
                }
            }

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Возвращает список перелетов по пакету
        /// </summary>
        /// <param name="dc">Контекст поисковой БД</param>
        /// <param name="mainDc">Контекст основной БД</param>
        /// <param name="packetKey">Ключ пакета</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        private static IList<FlightVariant> GetFlightParamsByDate(this MtSearchDbDataContext dc, MtMainDbDataContext mainDc, int packetKey, out string hash)
        {
            List<FlightVariant> result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, packetKey);
            if ((result = CacheHelper.GetCacheItem<List<FlightVariant>>(hash)) != null)
                return result;

            string hashOut;
            var cacheDependencies = new List<string>();
            result = new List<FlightVariant>();


            var charterCosts = (from ch in dc.GetAllCharters()
                                join cs in dc.GetFlightCostsByPacketKey(packetKey, out hashOut) on ch.CH_KEY equals cs.CS_CODE
                                select new
                                {
                                    CH_Key = ch.CH_KEY,
                                    CH_Flight = ch.CH_FLIGHT,
                                    CS_SubCode1 = cs.CS_SUBCODE1,
                                    CS_PRKey = cs.CS_PRKEY,
                                    CS_PKKey = cs.CS_PKKEY,
                                    CH_PortCodeFrom = ch.CH_PORTCODEFROM,
                                    CH_CityKeyFrom = ch.CH_CITYKEYFROM,
                                    CH_PortCodeTo = ch.CH_PORTCODETO,
                                    CH_CityKeyTo = ch.CH_CITYKEYTO,
                                    CH_AirlineCode = ch.CH_AIRLINECODE,
                                    CH_Aircraft = ch.CH_AIRCRAFT,
                                    CS_LongMin = cs.CS_LONGMIN,
                                    CS_Long = cs.CS_LONG
                                })
                .ToList();

            cacheDependencies.Add(hashOut);
            cacheDependencies.Add(CharterExtension.TableName);

            foreach (var charterCost in charterCosts)
            {
                int flightNumber;
                if (!Int32.TryParse(charterCost.CH_Flight, out flightNumber))
                    flightNumber = 0;

                var flightParamKeys = new FlightVariantKeys
                {
                    CharterKey = charterCost.CH_Key,
                    CharterClassKey = charterCost.CS_SubCode1.Value,
                    PartnerKey = charterCost.CS_PRKey,
                    PacketKey = charterCost.CS_PKKey.Value
                };

                var flightDatesTemp = dc.GetCharterDates(flightParamKeys, out hashOut);
                if (!cacheDependencies.Contains(hashOut))
                    cacheDependencies.Add(hashOut);

                var packetDatesTemp = mainDc.GetDatesByTours(new List<int> { packetKey }, out hashOut).Select(d => d.Item2).ToList();
                if (!cacheDependencies.Contains(hashOut))
                    cacheDependencies.Add(hashOut);

                foreach (var flightParamsByDate in flightDatesTemp.Where(fd => packetDatesTemp.Contains(fd.DepartTime.Date)))
                {
                    flightParamsByDate.CharterClassCode =
                        dc.GetAirServiceByKey(charterCost.CS_SubCode1.Value).AS_CODE;
                    flightParamsByDate.PortCodeFrom = charterCost.CH_PortCodeFrom;
                    flightParamsByDate.CityKeyFrom = charterCost.CH_CityKeyFrom;
                    flightParamsByDate.PortCodeTo = charterCost.CH_PortCodeTo;
                    flightParamsByDate.CityKeyTo = charterCost.CH_CityKeyTo;
                    flightParamsByDate.AirlineCode = charterCost.CH_AirlineCode;
                    flightParamsByDate.FlightNumber = flightNumber;
                    flightParamsByDate.AircraftType = charterCost.CH_Aircraft;

                    var durationMin = charterCost.CS_LongMin;
                    var durationMax = charterCost.CS_Long;
                    flightParamsByDate.DurationMin = durationMin == null ? (short)0 : durationMin.Value;
                    flightParamsByDate.DurationMax = durationMax ?? 999;

                    result.Add(flightParamsByDate);
                }
            }

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        #endregion

        /// <summary>
        /// Возвращает список пакетов, в которых лежат цены для бронирования авиабилетов
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="searchDc">Контекст поисковой БД</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns>Tuple int, PacketType - ключ пакета, тип пакета (направление туда, обратно или туда/обратно)</returns>
        public static IList<Tuple<int, PacketType>> GetFlightPackets(this MtMainDbDataContext dc, MtSearchDbDataContext searchDc, out string hash)
        {
            List<Tuple<int, PacketType>> result;
            hash = String.Format("{0}", MethodBase.GetCurrentMethod().Name);
            if ((result = CacheHelper.GetCacheItem<List<Tuple<int, PacketType>>>(hash)) != null) return result;

            var cacheDependencies = new List<string>();
            result = new List<Tuple<int, PacketType>>();

            string hashOut;
            var packetKeys = dc.GetCharterBookingPackets(out hashOut);
            cacheDependencies.Add(hashOut);

            var packetKeysDates = dc.GetDatesByTours(packetKeys, out hashOut);
            cacheDependencies.Add(hashOut);

            var allPacketServices = dc.GetClassServicesByTurListKeys(ServiceClass.Flight, packetKeysDates.Select(p => p.Item1).Distinct().ToList(), out hashOut);
            cacheDependencies.Add(hashOut);

            var homeCountryKey = (from ct in searchDc.GetAllCities()
                                  where ct.CT_KEY == Globals.Settings.HomeCityKey
                                  select ct.CT_CNKEY)
                .Single();

            foreach (var packetKey in packetKeysDates.Select(p => p.Item1).Distinct().ToList())
            {
                var services = (from s in allPacketServices
                                where s.TS_PKKEY == packetKey
                                select s)
                 .ToList();

                if (services.Count > 1)
                    result.Add(new Tuple<int, PacketType>(packetKey, PacketType.TwoWayCharters));
                else if (services.Count == 1)
                {
                    var serviceCountryKey = (from cn in searchDc.GetAllCities()
                                             where cn.CT_KEY == services[0].TS_SUBCODE2
                                             select cn.CT_CNKEY)
                        .FirstOrDefault();

                    result.Add(serviceCountryKey == homeCountryKey
                        ? new Tuple<int, PacketType>(packetKey, PacketType.DirectCharters)
                        : new Tuple<int, PacketType>(packetKey, PacketType.BackCharters));
                }
            }

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        
        /// <summary>
        /// Метод по выдаче расписаний рейсов
        /// </summary>
        /// <param name="dc">Контекст БД</param>
        /// <param name="mainDc">Контекст БД</param>
        /// <param name="hash">Строка кэша</param>
        /// <returns></returns>
        public static FlightSchedule GetFlightSchedules(this MtSearchDbDataContext dc, MtMainDbDataContext mainDc, out string hash)
        {
            return dc.GetFlightSchedules(mainDc, null, out hash);
        }


        /// <summary>
        /// Метод по выдаче расписаний рейсов
        /// </summary>
        /// <param name="dc">Контекст БД</param>
        /// <param name="mainDc">Контекст БД</param>
        /// <param name="packetKeys">Ключ пакетов, из которых будем брать данные (если пакеты не заданы, то параметр - null)</param>
        /// <param name="hash">Строка кэша</param>
        /// <returns></returns>
        public static FlightSchedule GetFlightSchedules(this MtSearchDbDataContext dc, MtMainDbDataContext mainDc, IList<Tuple<int, PacketType>> packetKeys, out string hash)
        {
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, packetKeys == null ? String.Empty : String.Join(",", packetKeys));

            FlightSchedule result;
            if ((result = CacheHelper.GetCacheItem<FlightSchedule>(hash)) != null) return result;

            string hashOut;
            result = new FlightSchedule();
            var cacheDependencies = new List<string>();

            IList<Tuple<int, PacketType>> packets;
            if (packetKeys == null)
            {
                packets = mainDc.GetFlightPackets(dc, out hashOut);
                cacheDependencies.Add(hashOut); 
            }
            else
                packets = packetKeys;

            foreach (var packet in packets)
            {
                var packetKey = packet.Item1;
                var packetType = packet.Item2;

                var flightVariants = dc.GetFlightParamsByDate(mainDc, packetKey, out hashOut);
                if (!cacheDependencies.Contains(hashOut))
                    cacheDependencies.Add(hashOut);

                result.Schedule.AddRange(dc.GetFlightMatches(mainDc, packetType, flightVariants, out hashOut));
                if (!cacheDependencies.Contains(hashOut))
                    cacheDependencies.Add(hashOut);
            }

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }
        

        /// <summary>
        /// Данные по квотам на перелеты
        /// </summary>
        /// <param name="dc">Контекст БД</param>
        /// <param name="mainDc">Контекст БД</param>
        /// <param name="query">Параметры запроса</param>
        /// <param name="hash">Строка кэша</param>
        /// <returns></returns>
        public static QuotaPriceResult GetFlightQuotaPrices(this MtSearchDbDataContext dc, MtMainDbDataContext mainDc, QuotaPriceQuery query, out string hash)
        {
            QuotaPriceResult result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, query);
            if ((result = CacheHelper.GetCacheItem<QuotaPriceResult>(hash)) != null) return result;

            // определяем агента по логину / паролю пользователя
            int? agentKey = null;
            if (!string.IsNullOrEmpty(query.UserName))
            {
                var user = mainDc.GetDupUser(query.UserName, query.UserPassword);
                if (user != null)
                    agentKey = user.US_PRKEY;
            }

            var cacheDependencies = new List<string>();
            result = new QuotaPriceResult();

            foreach (var flightPars in query.FlightParams)
            {
                string hashOut;
                var cost = dc.GetFlightPriceByParams(flightPars.FlightParamKeys.CharterKey,
                                        flightPars.FlightParamKeys.CharterClassKey, flightPars.FlightParamKeys.PartnerKey,
                                        flightPars.FlightParamKeys.PacketKey, flightPars.DepartTime.Date,
                                        flightPars.Duration != null
                                        ? flightPars.Duration.Value
                                        : 1,
                                        out hashOut);
                if (!cacheDependencies.Contains(hashOut))
                    cacheDependencies.Add(hashOut);

                if (cost != null && cost.Price != null)
                {
                    var quotaState = dc.CheckServiceQuotaByDay(ServiceClass.Flight,
                        flightPars.FlightParamKeys.CharterKey,
                        flightPars.FlightParamKeys.CharterClassKey,
                        null,
                        flightPars.FlightParamKeys.PartnerKey,
                        agentKey,
                        flightPars.DepartTime.Date,
                        flightPars.Duration != null
                            ? flightPars.Duration.Value
                            : 1,
                        true,
                        out hashOut);

                    if (!cacheDependencies.Contains(hashOut))
                        cacheDependencies.Add(hashOut);

                    result.FlightData.Add(new QuotaPriceData
                    {
                        QuotaState = quotaState.QuotaState,
                        QuotaPlaces = quotaState.Places,
                        PriceValue = cost
                    });
                }
            }

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Данные по квотам на перелеты
        /// </summary>
        /// <param name="dc">Контекст БД</param>
        /// <param name="mainDc">Контекст БД</param>
        /// <param name="query">Параметры запроса</param>
        /// <returns></returns>
        public static QuotaPriceResult GetFlightQuotaPrices(this MtSearchDbDataContext dc, MtMainDbDataContext mainDc, QuotaPriceQuery query)
        {
            string hash;
            return GetFlightQuotaPrices(dc, mainDc, query, out hash);
        }

        /// <summary>
        /// Метод по выдаче расписаний рейсов
        /// </summary>
        /// <param name="dc">Контекст БД</param>
        /// <param name="mainDc">Контекст БД</param>
        /// <returns></returns>
        public static FlightSchedule GetFlightSchedules(this MtSearchDbDataContext dc, MtMainDbDataContext mainDc)
        {
            string hash;
            return GetFlightSchedules(dc, mainDc, out hash);
        }

    }
}
