using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using QDSearch;
using QDSearch.Extensions;
using QDSearch.Helpers;
using QDSearch.Repository.MtMain;
using QDSearch.Repository.MtSearch;
using Seemplexity.Logic.Flights.DataModel;

namespace Seemplexity.Logic.Flights
{
    public static class AviaScheduleLogic
    {
        public static IList<CharterSchedulePlainInfo> GetCharterSchedules(this MtSearchDbDataContext dc, MtMainDbDataContext mainDc, int cityKeyFrom, int cityKeyTo, out string hash)
        {
            List<CharterSchedulePlainInfo> result;
            hash = String.Format("{0}_{1}_{2}", MethodBase.GetCurrentMethod().Name, cityKeyFrom, cityKeyTo);
            if ((result = CacheHelper.GetCacheItem<List<CharterSchedulePlainInfo>>(hash)) != null) return result;

            result = new List<CharterSchedulePlainInfo>();

            string hashOut;
            var cacheDependencies = new List<string>();

            var charterKeys = dc.GetAllCharterKeys(out hashOut);
            cacheDependencies.Add(hashOut);

            var chartersTo = (from a in dc.GetAllAirSeasons()
                join ch in dc.GetAllCharters() on a.AS_CHKEY equals ch.CH_KEY
                where charterKeys.Contains(ch.CH_KEY)
                      && ch.CH_CITYKEYFROM == cityKeyFrom
                      && ch.CH_CITYKEYTO == cityKeyTo
                      && a.AS_DATEFROM.HasValue
                      && a.AS_DATETO.HasValue
                      && a.AS_TIMEFROM.HasValue
                      && a.AS_TIMETO.HasValue
                select
                    new CharterSchedulePlainInfoInternal(a.AS_ID, ch.CH_KEY, a.AS_DATEFROM.Value, a.AS_DATETO.Value, a.AS_WEEK,
                        a.AS_TIMEFROM.Value, a.AS_TIMETO.Value, ch.CH_PORTCODEFROM, ch.CH_PORTCODETO, ch.CH_AIRLINECODE,
                        ch.CH_FLIGHT, ch.CH_AIRCRAFT))
                .Distinct()
                .ToList();

            var chartersFrom = (from a in dc.GetAllAirSeasons()
                              join ch in dc.GetAllCharters() on a.AS_CHKEY equals ch.CH_KEY
                              where charterKeys.Contains(ch.CH_KEY)
                                    && ch.CH_CITYKEYFROM == cityKeyTo
                                    && ch.CH_CITYKEYTO == cityKeyFrom
                                    && a.AS_DATEFROM.HasValue
                                    && a.AS_DATETO.HasValue
                                    && a.AS_TIMEFROM.HasValue
                                    && a.AS_TIMETO.HasValue
                              select
                                  new CharterSchedulePlainInfoInternal(a.AS_ID, ch.CH_KEY, a.AS_DATEFROM.Value, a.AS_DATETO.Value, a.AS_WEEK,
                                      a.AS_TIMEFROM.Value, a.AS_TIMETO.Value, ch.CH_PORTCODEFROM, ch.CH_PORTCODETO, ch.CH_AIRLINECODE,
                                      ch.CH_FLIGHT, ch.CH_AIRCRAFT))
                .Distinct()
                .ToList();

            cacheDependencies.Add(AirSeasonExtension.TableName);
            cacheDependencies.Add(CharterExtension.TableName);

            var seasonKeysDone = new List<int>();
            foreach (var chto in chartersTo)
            {
                var chto1 = chto;
                var chsFrom = chartersFrom.Where(c => c.AirCraft == chto1.AirCraft
                                                      && c.AirlineCode == chto1.AirlineCode
                                                      && Math.Abs(c.FlightNumber - chto1.FlightNumber) == 1
                                                      && c.PortCodeFrom == chto1.PortCodeTo
                                                      && c.PortCodeTo == chto1.PortCodeFrom)
                    .ToList();

                var directCharterTimesFrom = chto.CharterDates.Select(d => d.Add(chto.TimeFrom.TimeOfDay)).ToList();

                foreach (var chFrom in chsFrom)
                {
                    var backCharterTimesFrom = chFrom.CharterDates.Select(d => d.Add(chFrom.TimeFrom.TimeOfDay)).ToList();

                    var fitedDates = new List<DateTime>();
                    foreach (var dt in backCharterTimesFrom)
                    {
                        // наши перелеты совершаются с разницей меньше суток, плюч у них сходятся параметры и разнятся номера на 1
                        // значит скорее всего они парные, других параметров проверить их парность у нас нет
                        fitedDates.AddRange(directCharterTimesFrom.Where(d => Math.Abs(d.Subtract(dt).TotalHours) >= 0 && Math.Abs(d.Subtract(dt).TotalHours) < 24));
                    }

                    if (fitedDates.Any())
                    {
                        result.Add(new CharterSchedulePlainInfo
                        {
                            DaysOfWeek = fitedDates.Select(d => d.DayOfWeek).Distinct().ToList(),
                            AirlineName = chto1.AirlineCode,
                            DateFrom = fitedDates.Min(),
                            DateTo = fitedDates.Max(),
                            AircraftName = chto1.AirCraft,
                            FlightNum = chto1.AirlineCode + " " + chto1.FlightNumber + "/" + chFrom.FlightNumber,
                            DirectCharterTime = chto1.TimeFrom.ToString("HH:mm") + " - " + chto1.TimeTo.ToString("HH:mm"),
                            DirectAirportFromName = chto1.PortCodeFrom,
                            DirectAirportToName = chto1.PortCodeTo,
                            BackCharterTime = chFrom.TimeFrom.ToString("HH:mm") + " - " + chFrom.TimeTo.ToString("HH:mm"),
                            BackAirportFromName = chFrom.PortCodeFrom,
                            BackAirportToName = chFrom.PortCodeTo
                        });
                        if (!seasonKeysDone.Contains(chto1.AirSeasonKey))
                            seasonKeysDone.Add(chto1.AirSeasonKey);

                        if (!seasonKeysDone.Contains(chFrom.AirSeasonKey))
                            seasonKeysDone.Add(chFrom.AirSeasonKey);
                    }
                }
            }

            foreach (var chto in chartersTo.Where(c => !seasonKeysDone.Contains(c.AirSeasonKey)))
            {
                var chto1 = chto;
                result.Add(new CharterSchedulePlainInfo
                {
                    DaysOfWeek = new List<DayOfWeek>(chto1.DaysOfWeek),
                    AirlineName = chto1.AirlineCode,
                    DateFrom = chto1.CharterDates.Min(),
                    DateTo = chto1.CharterDates.Max(),
                    AircraftName = chto1.AirCraft,
                    FlightNum = chto1.AirlineCode + " " + chto1.FlightNumber,
                    DirectCharterTime = chto1.TimeFrom.ToString("HH:mm") + " - " + chto1.TimeTo.ToString("HH:mm"),
                    DirectAirportFromName = chto1.PortCodeFrom,
                    DirectAirportToName = chto1.PortCodeTo,
                    BackCharterTime = "---",
                    BackAirportFromName = "---",
                    BackAirportToName = "---"
                });

                if (!seasonKeysDone.Contains(chto1.AirSeasonKey))
                    seasonKeysDone.Add(chto1.AirSeasonKey);
            }

            foreach (var chFrom in chartersFrom.Where(c => !seasonKeysDone.Contains(c.AirSeasonKey)))
            {
                var chFrom1 = chFrom;
                result.Add(new CharterSchedulePlainInfo
                {
                    DaysOfWeek = new List<DayOfWeek>(chFrom1.DaysOfWeek),
                    AirlineName = chFrom1.AirlineCode,
                    DateFrom = chFrom1.CharterDates.Min(),
                    DateTo = chFrom1.CharterDates.Max(),
                    AircraftName = chFrom1.AirCraft,
                    FlightNum = chFrom1.AirlineCode + " " + chFrom1.FlightNumber,
                    DirectCharterTime = "---",
                    DirectAirportFromName = "---",
                    DirectAirportToName = "---",
                    BackCharterTime = chFrom1.TimeFrom.ToString("HH:mm") + " - " + chFrom1.TimeTo.ToString("HH:mm"),
                    BackAirportFromName = chFrom1.PortCodeFrom,
                    BackAirportToName = chFrom1.PortCodeTo
                });

                if (!seasonKeysDone.Contains(chFrom1.AirSeasonKey))
                    seasonKeysDone.Add(chFrom1.AirSeasonKey);
            }

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        public static IList<Tuple<int, string>> GetCharterScheduleCitiesTo(this MtSearchDbDataContext dc, int countryKeyTo, int cityKeyFrom, out string hash)
        {
            List<Tuple<int, string>> result;
            hash = String.Format("{0}_{1}_{2}", MethodBase.GetCurrentMethod().Name, countryKeyTo, cityKeyFrom);
            if ((result = CacheHelper.GetCacheItem<List<Tuple<int, string>>>(hash)) != null) return result;

            string hashOut;
            var cacheDependencies = new List<string>();

            var charterKeys = dc.GetAllCharterKeys(out hashOut);
            cacheDependencies.Add(hashOut);

            result = (from ch in dc.GetAllCharters()
                      join ctto in dc.GetAllCities() on ch.CH_CITYKEYTO equals ctto.CT_KEY
                      join ctfrom in dc.GetAllCities() on ch.CH_CITYKEYFROM equals ctfrom.CT_KEY
                      where charterKeys.Contains(ch.CH_KEY)
                      && ctto.CT_CNKEY == countryKeyTo
                      && ctfrom.CT_KEY == cityKeyFrom
                      select new Tuple<int, string>(ch.CH_CITYKEYTO, ctto.CT_NAME))
                .Distinct()
                .ToList();

            cacheDependencies.Add(CharterExtension.TableName);
            cacheDependencies.Add(CitiesExtension.TableName);

            return result;
        }

        public static IList<Tuple<int, string>> GetCharterScheduleCitiesFrom(this MtSearchDbDataContext dc, int countryKeyTo, out string hash)
        {
            List<Tuple<int, string>> result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, countryKeyTo);
            if ((result = CacheHelper.GetCacheItem<List<Tuple<int, string>>>(hash)) != null) return result;

            string hashOut;
            var cacheDependencies = new List<string>();

            var charterKeys = dc.GetAllCharterKeys(out hashOut);
            cacheDependencies.Add(hashOut);

            result = (from ch in dc.GetAllCharters()
                      join ctto in dc.GetAllCities() on ch.CH_CITYKEYTO equals ctto.CT_KEY
                      join ctfrom in dc.GetAllCities() on ch.CH_CITYKEYFROM equals ctfrom.CT_KEY
                      where charterKeys.Contains(ch.CH_KEY)
                      && ctto.CT_CNKEY == countryKeyTo
                      select new Tuple<int, string>(ch.CH_CITYKEYFROM, ctfrom.CT_NAME))
                .Distinct()
                .ToList();

            cacheDependencies.Add(CharterExtension.TableName);
            cacheDependencies.Add(CitiesExtension.TableName);

            return result;
        }

        public static IList<Tuple<int, string>> GetCharterScheduleCountriesTo(this MtSearchDbDataContext dc, out string hash)
        {
            List<Tuple<int, string>> result;
            hash = String.Format("{0}", MethodBase.GetCurrentMethod().Name);
            if ((result = CacheHelper.GetCacheItem<List<Tuple<int, string>>>(hash)) != null) return result;

            string hashOut;
            var cacheDependencies = new List<string>();

            var charterKeys = dc.GetAllCharterKeys(out hashOut);
            cacheDependencies.Add(hashOut);

            var homeCountry = dc.GetHomeCountry(out hashOut);
            cacheDependencies.Add(hashOut);

            result = (from ch in dc.GetAllCharters()
                join ct in dc.GetAllCities() on ch.CH_CITYKEYTO equals ct.CT_KEY
                join cn in dc.GetAllCountries() on ct.CT_CNKEY equals cn.CN_KEY
                      where charterKeys.Contains(ch.CH_KEY)
                      && cn.CN_KEY != homeCountry.CN_KEY
                select new Tuple<int, string>(cn.CN_KEY, cn.CN_NAME))
                .Distinct()
                .ToList();

            cacheDependencies.Add(CharterExtension.TableName);
            cacheDependencies.Add(CountriesExtension.TableName);
            cacheDependencies.Add(CitiesExtension.TableName);

            return result;
        }

        private static IList<int> GetAllCharterKeys(this MtSearchDbDataContext dc, out string hash)
        {
            List<int> result;
            hash = String.Format("{0}", MethodBase.GetCurrentMethod().Name);
            if ((result = CacheHelper.GetCacheItem<List<int>>(hash)) != null) return result;

            result = new List<int>();
            var cacheDependencies = new List<string>();

            var commandBuilder = new StringBuilder();
            commandBuilder.AppendLine("select CH_Key ");
            commandBuilder.AppendLine("from Charter ");
            commandBuilder.AppendLine("where CH_Key in (select distinct QO_Code ");
            commandBuilder.AppendLine("				 from QuotaObjects ");
            commandBuilder.AppendLine("				 where QO_SVKey = 1 ");
            commandBuilder.AppendLine("				 and QO_QTID in (select distinct QD_QTID from QuotaDetails where QD_Date > dateadd(day, -1, getdate()))) ");
            commandBuilder.AppendLine("and CH_Key in (select AS_CHKey from AirSeason where AS_DATETO > dateadd(day, -1, getdate())) ");
            commandBuilder.AppendLine("and CH_Key in (select CS_CODE from tbl_Costs where CS_SVKey = 1 and CS_DATEEND > dateadd(day, -1, getdate())) ");

            using (var command = dc.Connection.CreateCommand())
            {
                command.CommandText = commandBuilder.ToString();

                dc.Connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetInt32("CH_Key"));
                    }
                }
                dc.Connection.Close();
            }

            cacheDependencies.Add(CharterExtension.TableName);
            cacheDependencies.Add(CostsExtension.TableName);
            cacheDependencies.Add(AirSeasonExtension.TableName);
            cacheDependencies.Add(QuotaObjectsExtension.TableName);

            return result;
        }
    }
}
