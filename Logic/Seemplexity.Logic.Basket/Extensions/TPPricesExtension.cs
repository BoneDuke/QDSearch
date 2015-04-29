using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QDSearch;
using QDSearch.DataModel;
using QDSearch.Extensions;
using QDSearch.Helpers;
using QDSearch.Repository.MtMain;
using QDSearch.Repository.MtSearch;
using Seemplexity.Logic.Basket.DataModel;

namespace Seemplexity.Logic.Basket.Extensions
{
    public static class TPPricesExtension
    {
        /// <summary>
        /// Возвращает информацию по туру для корзины по ключу цены
        /// </summary>
        /// <param name="mainDc">Контекст основной БД</param>
        /// <param name="searchDc">Контекст поисковой БД</param>
        /// <param name="priceKey">Ключ цены (таблица tp_prices)</param>
        /// <returns></returns>
        public static PriceInfo GetPriceInfoByTPKey(this MtMainDbDataContext mainDc, MtSearchDbDataContext searchDc, int priceKey)
        {
            
            var priceStartDate = mainDc.GetTPPriceByKey(priceKey).TP_DateBegin;

            var commandBuilder = new StringBuilder();
            commandBuilder.AppendLine("select to_key, ts_key, ts_svkey, ts_code, ts_subcode1, ts_subcode2, ts_oppartnerkey, ts_oppacketkey, ts_day, ts_days, ts_men, ts_attribute ");
            commandBuilder.AppendLine("from tp_tours ");
            commandBuilder.AppendLine("join tp_services on ts_tokey = to_Key ");
            commandBuilder.AppendLine("join tp_servicelists on tl_tskey = ts_key ");
            commandBuilder.AppendLine("join tp_lists on ti_key = tl_tikey ");
            commandBuilder.AppendLine(String.Format("where tl_tikey in (select tp_tikey from tp_prices where tp_key = {0})", priceKey));
            commandBuilder.AppendLine("order by ts_day, ts_key");

            var priceInfo = new PriceInfo();
            using (var command = mainDc.Connection.CreateCommand())
            {
                command.CommandText = commandBuilder.ToString();
                command.CommandTimeout = 100;

                mainDc.Connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (priceInfo.Tour == null)
                        {
                            priceInfo.Tour = (from tour in searchDc.GetAllTPTours()
                                where tour.TO_Key == reader.GetInt32("to_key")
                                select tour).Single();
                        }

                        var sf = new ServiceInfo
                        {
                            Key = reader.GetInt32("ts_key"),
                            ServiceClass = (ServiceClass) reader.GetInt32("ts_svkey"),
                            Code = reader.GetInt32("ts_code"),
                            SubCode1 = reader.GetInt32OrNull("ts_subcode1"),
                            SubCode2 = reader.GetInt32OrNull("ts_subcode2"),
                            PartnerKey = reader.GetInt32("ts_oppartnerkey"),
                            PacketKey = reader.GetInt32("ts_oppacketkey"),
                            StartDate = priceStartDate.AddDays(reader.GetInt16("ts_day") - 1),
                            Days = reader.GetInt16("ts_days"),
                            NMen = reader.GetInt16("ts_men"),
                            Attribute = reader.GetInt32("ts_attribute"),
                            Day = reader.GetInt16("ts_day")
                        };

                        if (sf.ServiceClass != ServiceClass.Flight)
                            priceInfo.Services.Add(sf);
                        else
                        {
                            int? ctKeyFrom, ctKeyTo;
                            searchDc.GetCharterCityDirection(sf.Code, out ctKeyFrom, out ctKeyTo);

                            if (!ctKeyFrom.HasValue || !ctKeyTo.HasValue)
                                throw new KeyNotFoundException(String.Format("Перелет с ключом {0} не найден", sf.Code));

                            var findFlight = (sf.Attribute & (int) ServiceAttribute.CodeEdit) != (int) ServiceAttribute.CodeEdit;
                            var flightGroups = Globals.Settings.CharterClassesDictionary;
                            string hashOut;
                            var altCharters =
                                findFlight
                                ? searchDc.GetAltCharters(mainDc, ctKeyFrom.Value, ctKeyTo.Value, sf.StartDate, sf.PacketKey, flightGroups, out hashOut)
                                : searchDc.GetAltCharters(mainDc, sf.Code, sf.StartDate, sf.PacketKey, flightGroups, out hashOut);

                            foreach (var key in flightGroups.Keys)
                            {
                                var key1 = key;
                                priceInfo.Flights.AddRange(altCharters[key].Select(info => new FlightInfo()
                                {
                                    Key = sf.Key,
                                    Attribute = sf.Attribute,
                                    Code = info.CharterKey,
                                    SubCode1 = info.ClassKey,
                                    PartnerKey = info.PartnerKey,
                                    FlightTimeStart = info.FlightDateTimeFrom,
                                    FlightTimeEnd = info.FlightDateTimeTo,
                                    Days = sf.Days,
                                    Day = sf.Day,
                                    ServiceClass = sf.ServiceClass,
                                    SubCode2 = sf.SubCode2,
                                    PacketKey = sf.PacketKey,
                                    StartDate = sf.StartDate,
                                    NMen = sf.NMen,
                                    FlightGroupKey = key1
                                }));
                            }
                        }
                    }
                }
                mainDc.Connection.Close();
            }

            var priceEndDate = priceStartDate;
            var tempDate = DateTime.MinValue;
            foreach (var sf in priceInfo.Services)
            {
                string hashOut;
                // делаем расчет стоимости услуги
                //todo: переделать получение цены по разным валютам
                sf.Cost = mainDc.GetServiceCost((int)sf.ServiceClass, sf.Code,
                    sf.SubCode1.HasValue ? sf.SubCode1.Value : 0, sf.SubCode2.HasValue ? sf.SubCode2.Value : 0,
                    sf.PartnerKey, sf.PacketKey, sf.StartDate, sf.Days, "E", sf.NMen, out hashOut);

                // получаем конечную дату тура
                if (sf.ServiceClass == ServiceClass.Hotel || sf.ServiceClass == ServiceClass.AddHotelService)
                    tempDate = priceStartDate.AddDays(sf.Day + Math.Max((int)sf.Days, 1) - 1);
                else
                    tempDate = priceStartDate.AddDays(sf.Day + Math.Max((int)sf.Days, 1) - 2);

                if (priceEndDate < tempDate)
                    priceEndDate = tempDate;
            }

            foreach (var fi in priceInfo.Flights)
            {
                string hashOut;
                // делаем расчет стоимости услуги
                //todo: переделать получение цены по разным валютам
                fi.Cost = mainDc.GetServiceCost((int)fi.ServiceClass, fi.Code,
                    fi.SubCode1.HasValue ? fi.SubCode1.Value : 0, fi.SubCode2.HasValue ? fi.SubCode2.Value : 0,
                    fi.PartnerKey, fi.PacketKey, fi.StartDate, priceInfo.Flights.Max(f => f.Day), "E", fi.NMen, out hashOut);

                // получаем направление перелета
                // первый перелет - прямой перелет
                // последний перелет - обратный
                // все остальные - промежуточные
                if (fi.Key == priceInfo.Flights.Min(f => f.Key))
                    fi.Direction = FlightDirection.DirectFlight;
                else if (fi.Key == priceInfo.Flights.Max(f => f.Key))
                    fi.Direction = FlightDirection.BackFlight;
                else
                    fi.Direction = FlightDirection.Intermediate;

                // получаем конечную дату тура
                tempDate = priceStartDate.AddDays(fi.Day - 1);
                if (priceEndDate < tempDate)
                    priceEndDate = tempDate;
            }
            priceInfo.TourDateBegin = priceStartDate;
            priceInfo.TourDateEnd = tempDate;

            var tst = priceInfo.GetTourHotelsAndPansions;

            return priceInfo;
        }
    }
}
