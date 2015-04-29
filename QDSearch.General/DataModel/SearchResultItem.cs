using System;
using System.Collections.Generic;
using System.Linq;
using QDSearch.Extensions;
using QDSearch.Repository.MtSearch;

namespace QDSearch.DataModel
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
    [Serializable]
    public class SearchResultItem
    {
        public SearchResultItem()
        {
            DirectFlightsInfo = new Dictionary<int, FlightPlainInfo>();
            BackFlightsInfo = new Dictionary<int, FlightPlainInfo>();
            FlightCalcInfo = new FlightCalculatedInfo();
        }

        /// <summary>
        /// Возвращает отсортированный список по параметрам
        /// </summary>
        /// <param name="list">Список для сортировки</param>
        /// <param name="sorter">Функция сортировки</param>
        /// <param name="direction">Направление</param>
        /// <typeparam name="TKey">Тип элемента списка</typeparam>
        /// <returns></returns>
        public static IOrderedEnumerable<SearchResultItem> Sort<TKey>(IEnumerable<SearchResultItem> list, Func<SearchResultItem, TKey> sorter, SortingDirection direction)
        {
            var result = direction == SortingDirection.Asc ? list.OrderBy(sorter) : list.OrderByDescending(sorter);
            return result;
        }

        /// <summary>
        /// Цена в различных валютах
        /// </summary>
        public Dictionary<string, double> PriceInRates;
        /// <summary>
        /// Ключ цены, нужее для генерации ссылки на корзину
        /// </summary>
        public int PriceKey { get; set; }
        /// <summary>
        /// Цена, как она лежит в базе. НАРУЖУ НЕ ВЫВОДИТЬ
        /// </summary>
        public double Price { get; set; }
        /// <summary>
        /// Валюта цены, как она лежит в базе. НАРУЖУ НЕ ВЫВОДИТЬ
        /// </summary>
        public string RateCode { get; set; }
        /// <summary>
        /// Дата цены
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Название тура
        /// </summary>
        public string TourName { get; set; }
        /// <summary>
        /// Ссылка на тур
        /// </summary>
        public string TourUrl { get; set; }
        /// <summary>
        /// Имеет ли тур описание
        /// </summary>
        public bool TourHasDescription { get; set; }
        /// <summary>
        /// Тип цены (за номер или за человека)
        /// </summary>
        public PriceForType PriceFor;

        /// <summary>
        /// Цена за номер
        /// </summary>
        public bool IsPriceForRoom
        {
            get
            {
                return PriceFor == PriceForType.PerRoom;
            }
        }

        #region Нужно для создания ссылки "все цены"
        /// <summary>
        /// Ключ страны
        /// </summary>
        public int CountryKey { get; set; }
        /// <summary>
        /// Ключ города вылета
        /// </summary>
        public int? FlightCityKeyFrom { get; set; }
        /// <summary>
        /// Ключ города прилета
        /// </summary>
        public int? FlightCityKeyTo { get; set; }

        /// <summary>
        /// Ключ города вылета обратного перелета
        /// </summary>
        public int? BackFlightCityKeyFrom { get; set; }
        /// <summary>
        /// Ключ города прилета обратного перелета
        /// </summary>
        public int? BackFlightCityKeyTo { get; set; }

        /// <summary>
        /// Ключ главного отеля
        /// </summary>
        public int HotelKey { get; set; }

        /// <summary>
        /// Ключ тура
        /// </summary>
        public int TourKey { get; set; }
        //public int HotelKey { get; set; }
        /// <summary>
        /// Дата окончания тура
        /// </summary>
        public DateTime DateTourEnd { get; set; }
        #endregion

        //todo: удалить по таску 232
        /// <summary>
        /// Код валюты фильтра, устаревшее, не использовать!!!
        /// </summary>
        [Obsolete]
        public string FilterRateCode { get; set; }

        /// <summary>
        /// Список отелей
        /// </summary>
        public List<Hotel> Hotels { get; set; }

        /// <summary>
        /// Информация о прямых перелетах, подошедших по фильтру квот, параметр не может быть null
        /// Формат структуры такой: ключ внешнего словаря - класс перелета (список классов задается в Web.config)
        /// в словаре может вообще не быть какого-то класса, это значит что по этому классу ни один перелет не подошел
        /// ключ внутреннего словаря - приоритет (чем меньше ключ, тем выше приоритет), в поиске нужно выводить перелет с самым высоким приоритетом
        /// если затем в корзине будем делать возможность подмены перелета, то ее адо будет делать и в поиске
        /// </summary>
        public Dictionary<int, FlightPlainInfo> DirectFlightsInfo { get; set; }

        /// <summary>
        /// Информация о прямых перелетах, подошедших по фильтру квот, параметр не может быть null
        /// структура аналогична DirectFlightsInfo
        /// </summary>
        public Dictionary<int, FlightPlainInfo> BackFlightsInfo { get; set; }

        public FlightCalculatedInfo FlightCalcInfo { get; set; }

        /// <summary>
        /// Квоты прямого перелета, по классам
        /// </summary>
        //public Dictionary<int, QuotaStatePlaces> CharterToQuota;
        /// <summary>
        /// Квоты обратного перелета, по классам
        /// </summary>
        //public Dictionary<int, QuotaStatePlaces> CharterFromQuota;


        /// <summary>
        /// Число основных мест в проживании
        /// </summary>
        public short MainPlacesCount { get; set; }

        /// <summary>
        /// Дата, до которой действителен тур
        /// </summary>
        /// <returns></returns>
        public string GetTourValidTo()
        {
            string result = String.Empty;
            using (var context = new MtSearchDbDataContext())
            {
                var date =
                    context.GetAllTPTours()
                        .Where(t => t.TO_Key == TourKey)
                        .Select(t => t.TO_DateValid)
                        .SingleOrDefault();

                if (date != null)
                    result = date.Value.ToString("dd.MM.yyyy");
            }
            return result;
        }
    }
}
