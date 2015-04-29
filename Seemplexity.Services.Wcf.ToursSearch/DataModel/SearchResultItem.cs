using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Seemplexity.Services.Wcf.ToursSearch.DataModel
{
    /*
     Поля для вывода в грид:
     * Date - DateTourEnd склеивается,
     * Hotel.Name (Hotel.Url) - построчно
     * Hotel.Stars - построчно
     * Hotel.ResortName - построчно
     * Hotel.RoomName - построчно
     * Hotel.RoomCategoryName - построчно
     * Hotel.AccomodationName - построчно
     * Hotel.PansionName - построчно
     * Hotel.NightsCount - построчно
     * PriceInFilterRate (PriceKey) - склеивается
     * FilterRateCode - склеивается
     * TourName (TourUrl) - склеивается
     * ссылка "Все цены" - CountryKey, CityKeyFrom, TourKey, HotelKey, Date, FilterRateCode, 
     * HotelQuota - склеивается
     * CharterToQuota - CharterFromQuota - склеивается
     */
    /// <summary>
    /// Строка в поиске
    /// </summary>
    [DataContract]
    public sealed class SearchResultItem
    {
        /// <summary>
        /// Цена в различных валютах
        /// </summary>
        [DataMember]
        public Dictionary<string, double> PriceInRates;
        /// <summary>
        /// Ключ цены, нужен для генерации ссылки на корзину
        /// </summary>
        [DataMember]
        public int PriceKey { get; private set; }
        /// <summary>
        /// Дата начала тура
        /// </summary>
        [DataMember]
        public DateTime DateTourStart { get; private set; }
        /// <summary>
        /// Название тура
        /// </summary>
        [DataMember]
        public string TourName { get; private set; }
        /// <summary>
        /// Ссылка на тур
        /// </summary>
        [DataMember]
        public string TourUrl { get; private set; }
        /// <summary>
        /// Имеет ли тур описание
        /// </summary>
        [DataMember]
        public bool TourHasDescription { get; private set; }
        /// <summary>
        /// Тип цены (за номер или за человека)
        /// </summary>
        [DataMember]
        public PriceForType PriceFor;

        #region Нужно для создания ссылки "все цены"
        /// <summary>
        /// Ключ страны
        /// </summary>
        [DataMember]
        public int CountryKey { get; private set; }
        /// <summary>
        /// Ключ города вылета
        /// </summary>
        [DataMember]
        public int? FlightCityKeyFrom { get; private set; }
        /// <summary>
        /// Ключ города прилета
        /// </summary>
        [DataMember]
        public int? FlightCityKeyTo { get; private set; }

        /// <summary>
        /// Ключ города вылета обратного перелета
        /// </summary>
        [DataMember]
        public int? BackFlightCityKeyFrom { get; private set; }
        /// <summary>
        /// Ключ города прилета обратного перелета
        /// </summary>
        [DataMember]
        public int? BackFlightCityKeyTo { get; private set; }

        /// <summary>
        /// Ключ тура
        /// </summary>
        [DataMember]
        public int TourKey { get; private set; }
        //public int HotelKey { get; private set; }
        /// <summary>
        /// Дата окончания тура
        /// </summary>
        [DataMember]
        public DateTime DateTourEnd { get; private set; }
        #endregion

        /// <summary>
        /// Список отелей
        /// </summary>
        [DataMember]
        public List<Hotel> Hotels { get; private set; }
        /// <summary>
        /// Квоты прямого перелета, по классам
        /// </summary>
        [DataMember]
        public Dictionary<string, QuotaStatePlaces> CharterToQuota;
        /// <summary>
        /// Квоты обратного перелета, по классам
        /// </summary>
        [DataMember]
        public Dictionary<string, QuotaStatePlaces> CharterFromQuota;
        /// <summary>
        /// Время вылета прямого вылета, если есть
        /// </summary>
        [DataMember]
        public DateTime? CharterDateTimeFrom { get; private set; }
        /// <summary>
        /// Время прилета прямого рейса, если есть
        /// </summary>
        [DataMember]
        public DateTime? CharterDateTimeTo { get; private set; }
        /// <summary>
        /// Время вылета обратного рейса, если есть
        /// </summary>
        [DataMember]
        public DateTime? BackCharterDateTimeFrom { get; private set; }
        /// <summary>
        /// Время прибытия прямого рейса, если есть
        /// </summary>
        [DataMember]
        public DateTime? BackCharterDateTimeTo { get; private set; }
        /// <summary>
        /// Число основных мест в проживании
        /// </summary>
        [DataMember]
        public short MainPlacesCount { get; private set; }

        internal SearchResultItem(QDSearch.DataModel.SearchResultItem searchResultItem)
        {
            PriceInRates = searchResultItem.PriceInRates;
            PriceKey = searchResultItem.PriceKey;
            DateTourStart = searchResultItem.Date;
            TourName = searchResultItem.TourName;
            TourUrl = searchResultItem.TourUrl;
            TourHasDescription = searchResultItem.TourHasDescription;
            PriceFor = (PriceForType)searchResultItem.PriceFor;
            CountryKey = searchResultItem.CountryKey;
            FlightCityKeyFrom = searchResultItem.FlightCityKeyFrom.Value;
            FlightCityKeyTo = searchResultItem.FlightCityKeyTo;
            BackFlightCityKeyFrom = searchResultItem.BackFlightCityKeyFrom;
            BackFlightCityKeyTo = searchResultItem.BackFlightCityKeyTo;
            TourKey = searchResultItem.TourKey;
            DateTourEnd = searchResultItem.DateTourEnd;
            Hotels = Hotel.GetHotelsList(searchResultItem.Hotels);
            CharterToQuota = searchResultItem.DirectFlightsInfo.ToDictionary(cq => QDSearch.Globals.Settings.CharterClasses.Names[cq.Key], cq => new QuotaStatePlaces(cq.Value.QuotaState));
            CharterFromQuota = searchResultItem.BackFlightsInfo.ToDictionary(cq => QDSearch.Globals.Settings.CharterClasses.Names[cq.Key], cq => new QuotaStatePlaces(cq.Value.QuotaState));
            CharterDateTimeFrom = searchResultItem.DirectFlightsInfo[0].FlightDateTimeFrom;
            CharterDateTimeTo = searchResultItem.DirectFlightsInfo[0].FlightDateTimeTo;
            BackCharterDateTimeFrom = searchResultItem.BackFlightsInfo[0].FlightDateTimeFrom;
            BackCharterDateTimeTo = searchResultItem.BackFlightsInfo[0].FlightDateTimeTo;
            MainPlacesCount = searchResultItem.MainPlacesCount;
        }

        internal static List<SearchResultItem> GetSearchResultItemsList(
            List<QDSearch.DataModel.SearchResultItem> searchResultItems)
        {
            // ReSharper disable RedundantNameQualifier            
            return
                searchResultItems.Select(i => new Seemplexity.Services.Wcf.ToursSearch.DataModel.SearchResultItem(i))
                    .ToList();
            // ReSharper restore RedundantNameQualifier

        }
    }
}
