using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using QDSearch;
using QDSearch.Extensions;
using QDSearch.Helpers;
using QDSearch.Repository.MtSearch;
using Seemplexity.Logic.Flights.DataModel;

namespace Seemplexity.Logic.Flights.Extensions
{
    public static class AirSeasonExtension
    {
        public static List<FlightVariant> GetCharterDates(this MtSearchDbDataContext dc, FlightVariantKeys flightParams, out string hash)
        {
            List<FlightVariant> result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, flightParams);
            if ((result = CacheHelper.GetCacheItem<List<FlightVariant>>(hash)) != null) 
                return result;

            var cacheDependencies = new List<string>();
            var charterSchedules = (from a in dc.GetAllAirSeasons()
                                    where a.AS_CHKEY == flightParams.CharterKey
                && a.AS_TIMEFROM != null
                && a.AS_TIMETO != null
                                    select a)
                .ToList();

            cacheDependencies.Add(QDSearch.Extensions.AirSeasonExtension.TableName);

            result = new List<FlightVariant>();
            foreach (var airSeason in charterSchedules)
            {
                var dateFrom = airSeason.AS_DATEFROM.Value > DateTime.Now ? airSeason.AS_DATEFROM.Value : DateTime.Now;
                for (var date = dateFrom; date <= airSeason.AS_DATETO; date = date.AddDays(1))
                {
                    var dayOfWeek = (int)date.DayOfWeek;
                    if (airSeason.AS_WEEK.Contains(dayOfWeek.ToString()))
                    {
                        var timeFrom = airSeason.AS_TIMEFROM;
                        var timeTo = airSeason.AS_TIMETO;
                        var dateTo = airSeason.AS_TIMEFROM <= airSeason.AS_TIMETO ? date : date.AddDays(1);
                        var departTime = new DateTime(date.Year, date.Month, date.Day, timeFrom.Value.Hour,
                            timeFrom.Value.Minute, 0);
                        var arrivalTime = new DateTime(dateTo.Year, dateTo.Month, dateTo.Day, timeTo.Value.Hour,
                            timeFrom.Value.Minute, 0);

                        var flightParamsByDate = new FlightVariant
                        {
                            FlightParamKeys = flightParams,
                            DepartTime = departTime,
                            ArrivalTime = arrivalTime
                        };
                        result.Add(flightParamsByDate);

                        //string hashOut;
                        // определяем есть ли цена на перелет в принципе
                        //var cost = dc.GetFlightCost(flightParams.CharterKey, flightParams.CharterClassKey,
                        //    flightParams.PartnerKey, flightParams.PacketKey, departTime, -1, out hashOut);

                        //cacheDependencies.Add(hashOut);

                        //if (cost != null && cost.Price != null)
                        //{
                            //var flightParamsByDate = new ParamsByDate()
                            //{
                            //    FlightParamKeys = flightParams,
                            //    DepartTime = departTime,
                            //    ArrivalTime = arrivalTime
                            //};
                            //result.Add(flightParamsByDate);
                        //}

                    }
                }
            }

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }
    }
}
