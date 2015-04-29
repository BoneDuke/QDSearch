using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using QDSearch.DataModel;
using QDSearch.Extensions;
using QDSearch.Helpers;
using QDSearch.Repository.MtMain;
using QDSearch.Repository.MtSearch;

namespace QDSearch.Repository
{
    /// <summary>
    /// Класс, в котором определены методы по проверке квот
    /// </summary>
    public static class CheckQuotaMethods
    {
        /// <summary>
        /// Проверяет квоту на конкретный день по параметрам. Для услуг с продолжительностью этот метод нужно вызвать на каждый день
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="serviceClass">Класс услуги</param>
        /// <param name="code">Code услуги (для авиаперелетов - Charter, для проживания - ключ отеля)</param>
        /// <param name="subCode1">SubCode1 услуги, для перелета - класс, для проживания - тип номера (Room)</param>
        /// <param name="subCode2">SubCode1 услуги, для перелета - не используется, для проживания - категория номера (RoomCategory)</param>
        /// <param name="partnerKey">Ключ партнера по услуге</param>
        /// <param name="date">Дата проверки квоты</param>
        /// <param name="duration">Продолжительность услуги (нужна для подбора квот на продолжительность)</param>
        /// <param name="isTheFirstDay">Является ли проверяемая дата первым днем или нет (нужно для подбора квот на заезд)</param>
        /// <param name="agentKey">КЛюч агентства</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static QuotaStatePlaces CheckServiceQuotaByDay(this MtSearchDbDataContext dc, ServiceClass serviceClass, Int32 code,
            Int32 subCode1, Int32? subCode2, Int32 partnerKey, int? agentKey, DateTime date, int duration, bool isTheFirstDay, out string hash)
        {
            QuotaStatePlaces result;
            hash = String.Format("{0}_{1}_{2}_{3}", MethodBase.GetCurrentMethod().Name, (int)serviceClass, code, 
                CacheHelper.GetCacheKeyHashed(new[]
                {
                    subCode1.ToString(), 
                    subCode2.ToString(),
                    partnerKey.ToString(), 
                    agentKey.ToString(),
                    date.ToString("yyyy-MM-dd"),
                    duration.ToString(),
                    isTheFirstDay.ToString()
                }));

            if ((result = CacheHelper.GetCacheItem<QuotaStatePlaces>(hash)) != null) return result;

            var cacheDependecies = new List<string>();
            result = new QuotaStatePlaces ();

            string hashOut;
            var plainQuotasByDate = dc.GetPlainQuotasObjects(serviceClass, code, subCode1, subCode2, partnerKey, agentKey, date, out hashOut);
            cacheDependecies.Add(hashOut);

            plainQuotasByDate =
                plainQuotasByDate.Where(q => q.Duration.Split(',').Contains(duration.ToString()) || q.Duration == String.Empty)
                    .ToList();

            if (!plainQuotasByDate.Any())
                result.QuotaState = QuotesStates.Request;

            var ssQoIds = new List<bool>();
            foreach (var plainQuota in plainQuotasByDate.OrderByDescending(p => p.SsId))
            {
                ////стоит какой-то стоп
                if (plainQuota.SsId != null)
                {
                    //стоит общий стоп
                    if (plainQuota.SsQdId == null)
                    {
                        // стоп только на Allotment, или еще и на Commitment
                        ssQoIds.Add(plainQuota.IsAllotmentAndCommitment);
                    }

                    if (result.QuotaState == QuotesStates.None)
                        result.QuotaState = QuotesStates.No;

                    continue;
                }

                //стопа на текущей строке нет, но есть общий стоп, который подходит под эту строку
                //в переменной result.QuotaState у нас и так уже QuotesStates.No, но проставлю еще раз для наглядности
                if (ssQoIds.Any(s => s || plainQuota.Type == QuotaType.Allotment))
                {
                    result.QuotaState = QuotesStates.No;
                    continue;
                }

                //наступил релиз-период
                //todo: сделать настройку в web.config
                if (plainQuota.Release != null && (plainQuota.Date.Subtract(DateTime.Now).Days < plainQuota.Release))
                {
                    if (result.QuotaState == QuotesStates.None || result.QuotaState == QuotesStates.No)
                        result.QuotaState = QuotesStates.Request;

                    continue;
                }

                // проверяем квоту на заезд или на период
                if (serviceClass == ServiceClass.Flight ||
                    (serviceClass == ServiceClass.Hotel &&
                     ((plainQuota.CheckInPlaces ?? 0) <= 0 || plainQuota.CheckInPlacesBusy == null)))
                {
                    var state = dc.GetQuotesStateByInt(serviceClass, plainQuota.Places - plainQuota.Busy, plainQuota.Places, out hashOut);
                    cacheDependecies.Add(hashOut);

                    // или на эту дату не было статуса квотирования, либо мы нашли статус на текущую дату и он лучше
                    if (QuotasExtension.OrderderQuotaStates[result.QuotaState] < QuotasExtension.OrderderQuotaStates[state])
                    {
                        result.QuotaState = state;
                        result.Places += (uint)(plainQuota.Places - plainQuota.Busy);
                    }
                        
                }
                else if (isTheFirstDay)
                {
                    // нашли квоту на заезд
                    var checkInState = dc.GetQuotesStateByInt(serviceClass, plainQuota.Places - plainQuota.Busy, plainQuota.Places, out hashOut);
                    cacheDependecies.Add(hashOut);

                    result.Places = (uint)(plainQuota.Places - plainQuota.Busy);
                    result.QuotaState = checkInState;
                    result.IsCheckInQuota = true;
                }
            }

            CacheHelper.AddCacheData(hash, result, cacheDependecies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Проверяет квоту на всю услугу по параметрам
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="serviceClass">Класс услуги</param>
        /// <param name="code">Code услуги (для авиаперелетов - Charter, для проживания - ключ отеля)</param>
        /// <param name="subCode1">SubCode1 услуги, для перелета - класс, для проживания - тип номера (Room)</param>
        /// <param name="subCode2">SubCode1 услуги, для перелета - не используется, для проживания - категория номера (RoomCategory)</param>
        /// <param name="partnerKey">Ключ партнера по услуге</param>
        /// <param name="serviceDateFrom">Дата начала услуги</param>
        /// <param name="serviceDateTo">Дата окончания услуги</param>
        /// <param name="agentKey">Ключ агентства</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static QuotaStatePlaces CheckServiceQuota(this MtSearchDbDataContext dc, ServiceClass serviceClass, Int32 code,
            Int32 subCode1, Int32? subCode2, Int32 partnerKey, int? agentKey, DateTime serviceDateFrom, DateTime serviceDateTo,
            out string hash)
        {
            hash = String.Format("{0}_{1}_{2}_{3}", MethodBase.GetCurrentMethod().Name, (int)serviceClass, code, CacheHelper.GetCacheKeyHashed(new[]
            {
                subCode1.ToString(), 
                subCode2.ToString(), 
                partnerKey.ToString(), 
                agentKey.ToString(),
                serviceDateFrom.ToString("yyyy-MM-dd"), 
                serviceDateTo.ToString("yyyy-MM-dd")
            }));

            var cacheDependecies = new List<string>();
            QuotaStatePlaces result;
            if ((result = CacheHelper.GetCacheItem<QuotaStatePlaces>(hash)) != null) return result;

            int duration;
            result = new QuotaStatePlaces() { IsCheckInQuota = false, Places = 0, QuotaState = QuotesStates.Request };
            if (serviceClass == ServiceClass.Flight)
            {
                DateTime linkedDate1, linkedDate2;
                if (serviceDateFrom <= serviceDateTo)
                {
                    linkedDate1 = serviceDateFrom;
                    linkedDate2 = serviceDateTo;
                }  
                else
                {
                    linkedDate1 = serviceDateTo;
                    linkedDate2 = serviceDateFrom;
                }
                duration = linkedDate2.Subtract(linkedDate1).Days + 1;
            }
            else
            {
                duration = serviceDateTo.Subtract(serviceDateFrom).Days + 1;
            }

            if (serviceClass == ServiceClass.Flight)
                serviceDateTo = serviceDateFrom;

            var quotasStatusesByDays = new Dictionary<DateTime, QuotaStatePlaces>(duration);
            for (var date = serviceDateFrom; date <= serviceDateTo; date = date.AddDays(1))
            {
                string hashOut;
                quotasStatusesByDays.Add(date, dc.CheckServiceQuotaByDay(serviceClass, code, subCode1, subCode2, partnerKey, agentKey, date, duration, date == serviceDateFrom, out hashOut));
                cacheDependecies.Add(hashOut);
            }

            if (quotasStatusesByDays.Values.Any(s => s.QuotaState == QuotesStates.No))
                result.QuotaState = QuotesStates.No;
            else if (quotasStatusesByDays.Values.Any(s => s.QuotaState == QuotesStates.Request))
                result.QuotaState = QuotesStates.Request;
            else if (quotasStatusesByDays.Values.Any(s => s.QuotaState == QuotesStates.Small))
                result.QuotaState = QuotesStates.Small;
            else if (quotasStatusesByDays.Values.Any(s => s.QuotaState == QuotesStates.Availiable))
                result.QuotaState = QuotesStates.Availiable;

            if (result.QuotaState == QuotesStates.Availiable || result.QuotaState == QuotesStates.Small)
                result.Places = quotasStatusesByDays.Values.Min(s => s.Places);

            if (quotasStatusesByDays.Values.Any(s => s.IsCheckInQuota))
            {
                var checkInState = quotasStatusesByDays.Values.Where(s => s.IsCheckInQuota).Select(s => s).SingleOrDefault();
                if (checkInState != null && QuotasExtension.OrderderQuotaStates[checkInState.QuotaState] > QuotasExtension.OrderderQuotaStates[result.QuotaState])
                    result = checkInState;
            }

            CacheHelper.AddCacheData(hash, result, cacheDependecies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Метод по проверке квот на перелет
        /// </summary>
        /// <param name="dc">Контекст БД</param>
        /// <param name="mainDc">Контекст основной БД</param>
        /// <param name="charterKey">Ключ перелета</param>
        /// <param name="cityKeyFrom">Ключ города вылета</param>
        /// <param name="cityKeyTo">Ключ города прилета</param>
        /// <param name="charterDay">День вылета в туре</param>
        /// <param name="partnerKey">Ключ партнера по перелету</param>
        /// <param name="agentKey">Ключ агентства</param>
        /// <param name="packetKey">Ключ пакета</param>
        /// <param name="tourDate">Дата тура</param>
        /// <param name="linkedDay">День вылета парного перелета</param>
        /// <param name="findFlight">Признак "подбирать или не подбирать" перелет</param>
        /// <param name="placesToCheck">Количество мест, которые должны быть в квоте, чтобы она считалась подходящей</param>
        /// <param name="flightGroups">Группы перелетов (эконом, бизнес, премиум)</param>
        /// <param name="hash">Хэш кэша</param>
        /// <param name="aviaQuotaMask">Маска квот на перелет</param>
        /// <returns></returns>
        public static Dictionary<int, FlightPlainInfo> CheckCharterQuota(this MtSearchDbDataContext dc,
            MtMainDbDataContext mainDc,
            Int32 charterKey, Int32 cityKeyFrom, Int32 cityKeyTo,
            Int32 charterDay, int partnerKey, int? agentKey, int packetKey, DateTime tourDate, Int32? linkedDay,
            bool findFlight, QuotesStates aviaQuotaMask, Int32 placesToCheck, IDictionary<int, IEnumerable<int>> flightGroups, out string hash)
        {
            Dictionary<int, FlightPlainInfo> result;
            hash = String.Format("{0}_{1}_{2}", MethodBase.GetCurrentMethod().Name, charterKey,
                CacheHelper.GetCacheKeyHashed(new[]
            {
                cityKeyFrom.ToString(),
                cityKeyTo.ToString(),
                partnerKey.ToString(),
                agentKey.ToString(),
                charterDay.ToString(), packetKey.ToString(), tourDate.ToString("yyyy-MM-dd"), linkedDay.ToString(),
                findFlight.ToString(),
                aviaQuotaMask.ToString(),
                placesToCheck.ToString(),
                String.Join("_", flightGroups.Keys),
                String.Join("_", flightGroups.Values)
            }));

            if ((result = CacheHelper.GetCacheItem<Dictionary<int, FlightPlainInfo>>(hash)) != null) return result;
            var cacheDependencies = new List<string>();
            string hashOut;

            var flightsInfo = dc.GetCharterAllQuota(mainDc, charterKey, cityKeyFrom, cityKeyTo,
            charterDay, partnerKey, agentKey, packetKey, tourDate, linkedDay,
            findFlight, flightGroups, out hashOut);

            result = new Dictionary<int, FlightPlainInfo>();
            foreach (var key in flightsInfo.Keys)
            {
                var quotaIsOk = false;
                var tempState = new FlightPlainInfo();
                foreach (var flightInfo in flightsInfo[key])
                {
                    
                    var quotaState = flightInfo.QuotaState;
                    var maskIsOk = ((quotaState.QuotaState == QuotesStates.Small && (aviaQuotaMask & QuotesStates.Availiable) == QuotesStates.Availiable)
                                     || (quotaState.QuotaState & aviaQuotaMask) == quotaState.QuotaState);

                    if (!maskIsOk)
                        continue;

                    if ((quotaState.QuotaState == QuotesStates.Availiable || quotaState.QuotaState == QuotesStates.Small) && quotaState.Places < placesToCheck && (aviaQuotaMask & QuotesStates.Request) != QuotesStates.Request)
                        continue;

                    if (QuotasExtension.OrderderQuotaStates[flightInfo.QuotaState.QuotaState] > QuotasExtension.OrderderQuotaStates[tempState.QuotaState.QuotaState]
                        || (tempState.QuotaState.QuotaState == flightInfo.QuotaState.QuotaState && flightInfo.QuotaState.Places > tempState.QuotaState.Places))
                    {
                        tempState = flightInfo;
                        quotaIsOk = true;
                    }

                    if (tempState.QuotaState.QuotaState == QuotesStates.Availiable)
                        break; 
                }
                if (quotaIsOk)
                    result.Add(key, new FlightPlainInfo(tempState));
            }

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        /// <summary>
        /// Метод по проверке квот на перелет (можно использовать для вывода информации о перелетах п строке)
        /// </summary>
        /// <param name="dc">Контекст БД</param>
        /// <param name="mainDc">Контекст основной БД</param>
        /// <param name="charterKey">Ключ перелета</param>
        /// <param name="cityKeyFrom">Ключ города вылета</param>
        /// <param name="cityKeyTo">Ключ города прилета</param>
        /// <param name="charterDay">День вылета в туре</param>
        /// <param name="partnerKey">Ключ партнера по перелету</param>
        /// <param name="agentKey">Ключ агентства</param>
        /// <param name="packetKey">Ключ пакета</param>
        /// <param name="tourDate">Дата тура</param>
        /// <param name="linkedDay">День вылета парного перелета</param>
        /// <param name="findFlight">Признак "подбирать или не подбирать" перелет</param>
        /// <param name="flightGroups">Группы перелетов (эконом, бизнес, премиум)</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static Dictionary<int, List<FlightPlainInfo>> GetCharterAllQuota(this MtSearchDbDataContext dc, 
            MtMainDbDataContext mainDc,
            Int32 charterKey, Int32 cityKeyFrom, Int32 cityKeyTo,
            Int32 charterDay, int partnerKey, int? agentKey, int packetKey, DateTime tourDate, Int32? linkedDay,
            bool findFlight, IDictionary<int, IEnumerable<int>> flightGroups, out string hash)
        {
            var cacheDependencies = new List<string>();
            string cacheDep;
            hash = String.Format("{0}_{1}_{2}", MethodBase.GetCurrentMethod().Name, charterKey, 
                CacheHelper.GetCacheKeyHashed(new[]
            {
                cityKeyFrom.ToString(),
                cityKeyTo.ToString(),
                partnerKey.ToString(),
                agentKey.ToString(),
                charterDay.ToString(), packetKey.ToString(), tourDate.ToString("yyyy-MM-dd"), linkedDay.ToString(),
                findFlight.ToString(),
                String.Join("_", flightGroups.Keys),
                String.Join("_", flightGroups.Values)
            }));

            Dictionary<int, List<FlightPlainInfo>> result;
            if ((result = CacheHelper.GetCacheItem<Dictionary<int, List<FlightPlainInfo>>>(hash)) != null) return result;

            var charterDate = tourDate.AddDays(charterDay - 1).Date;

            // в случае, если перелета с ключом charterKey в БД нет, не найдутся ctKeyFrom и ctKeyTo
            // при этом мы возьмем данные, которые стоят в БД - cityKeyFrom и cityKeyTo
            int? ctKeyFrom, ctKeyTo;
            dc.GetCharterCityDirection(charterKey, out ctKeyFrom, out ctKeyTo);
            if (ctKeyFrom.HasValue)
                cityKeyFrom = ctKeyFrom.Value;
            if (ctKeyTo.HasValue)
                cityKeyTo = ctKeyTo.Value;

            result =
                findFlight
                    ? dc.GetAltCharters(mainDc, cityKeyFrom, cityKeyTo, charterDate, packetKey, flightGroups, out cacheDep)
                    : dc.GetAltCharters(mainDc, charterKey, charterDate, packetKey, flightGroups, out cacheDep);
            cacheDependencies.Add(cacheDep);

            foreach (var key in flightGroups.Keys)
            {
                foreach (var flight in result[key])
                {
                    if (dc.IsStopByDrection(cityKeyFrom, cityKeyTo, charterDate,
                        linkedDay != null ? tourDate.AddDays(linkedDay.Value - 1) : charterDate, out cacheDep))
                    {
                        flight.QuotaState = new QuotaStatePlaces
                        {
                            QuotaState = QuotesStates.No
                        };
                    }
                    else
                    {
                        flight.QuotaState = dc.CheckServiceQuota(ServiceClass.Flight, flight.CharterKey, flight.ClassKey, null, flight.PartnerKey, agentKey, charterDate, linkedDay != null ? tourDate.AddDays(linkedDay.Value - 1) : charterDate, out cacheDep);
                    }
                    cacheDependencies.Add(cacheDep);

                    //if (flightInfo.QuotaState.QuotaState == QuotesStates.Undefined
                    //    || QuotasExtension.OrderderQuotaStates[tempState.QuotaState] > QuotasExtension.OrderderQuotaStates[flightInfo.QuotaState.QuotaState]
                    //    || (tempState.QuotaState == QuotesStates.Small && flightInfo.QuotaState.QuotaState == QuotesStates.Small && tempState.Places > flightInfo.QuotaState.Places))
                    //{
                    //    flightInfo.QuotaState = tempState;
                    //}

                    //if (flightInfo.QuotaState.QuotaState == QuotesStates.Availiable)
                    //    break;
                }
            }
                

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }
    }
}
