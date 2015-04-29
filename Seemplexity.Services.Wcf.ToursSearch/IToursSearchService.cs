using Seemplexity.Services.Wcf.ToursSearch.DataModel;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Seemplexity.Services.Wcf.ToursSearch
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IToursSearchService" in both code and config file together.
    [ServiceContract]
    public interface IToursSearchService
    {
        [OperationContract]
        [WebGet]
        string GetData(int value);

        /// <summary>
        /// Возвращает список стран туров
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebGet]
        IDictionary<int, string> GetCountriesTo();

        /// <summary>
        /// Описание по стране
        /// </summary>
        /// <param name="countryKey">Ключ страны</param>
        /// <returns></returns>
        [OperationContract]
        [WebGet]
        string GetCountryDescription(int countryKey);

        /// <summary>
        /// Возвращает список городов вылета
        /// </summary>
        /// <param name="countryKey">Ключ страны</param>
        /// <returns></returns>
        [OperationContract]
        [WebGet]
        IDictionary<int, string> GetCitiesFrom(int countryKey);

        /// <summary>
        /// Возвращает список типов туров
        /// </summary>
        /// <param name="cityKeyFrom">Город вылета</param>
        /// <param name="countryKey">Ключ страны</param>
        /// <returns></returns>
        [OperationContract]
        [WebGet]
        IDictionary<int, string> GetTourTypes(int cityKeyFrom, int countryKey);

        /// <summary>
        /// Возвращает список городов
        /// </summary>
        /// <param name="cityKeyFrom">Город вылета</param>
        /// <param name="countryKey">Ключ страны</param>
        /// <param name="tourTypes">Ключи типа тура</param>
        /// <returns></returns>
        [OperationContract]
        [WebGet]
        IDictionary<int, string> GetCitiesTo(int cityKeyFrom, int countryKey, List<int> tourTypes);

        /// <summary>
        /// Возвращает список туров
        /// </summary>
        /// <param name="cityKeyFrom">Ключ города вылета</param>
        /// <param name="countryKey">Ключ страны тура</param>
        /// <param name="tourTypes">Ключи типа тура</param>
        /// <param name="cityToKeys">Ключи регионов, передавать только в том случае, если реально выбраны пользователем</param>
        /// <returns></returns>
        [OperationContract]
        [WebGet]
        IDictionary<int, string> GetTours(int cityKeyFrom, int countryKey, List<int> tourTypes,
            List<int> cityToKeys);

        /// <summary>
        /// Возвращает список дат заездов
        /// </summary>
        /// <param name="cityKeyFrom">Город вылета</param>
        /// <param name="countryKey">Страна тура</param>
        /// <param name="cityToKeys">Города проживания</param>
        /// <param name="tourKeys">Ключи туров</param>
        /// <returns></returns>
        [OperationContract]
        [WebGet]
        IList<DateTime> GetTourDates(int cityKeyFrom, int countryKey, List<int> cityToKeys,
            List<int> tourKeys);

        /// <summary>
        /// Возвращает продолжительности в ночах
        /// </summary>
        /// <param name="cityKeyFrom">Город вылета</param>
        /// <param name="countryKey">Страна тура</param>
        /// <param name="cityToKeys">Список городв заездов</param>
        /// <param name="tourKeys">Ключи туров</param>
        /// <param name="tourDates">Даты</param>
        /// <returns></returns>
        [OperationContract]
        [WebGet]
        IEnumerable<int> GetTourNights(int cityKeyFrom, int countryKey, List<int> cityToKeys,
            List<int> tourKeys, List<DateTime> tourDates);

        /// <summary>
        /// Возвращает список категорий отелей
        /// </summary>
        /// <param name="cityKeyFrom">Город вылета</param>
        /// <param name="countryKey">Страна тура</param>
        /// <param name="cityToKeys">Ключи регионов, передавать только в том случае, если реально выбраны пользователем</param>
        /// <param name="tourKeys">Ключи туров</param>
        /// <param name="tourDates">Даты туров</param>
        /// <param name="tourNights">Список продолжительностей</param>
        /// <returns></returns>
        [OperationContract]
        [WebGet]
        IEnumerable<string> GetTourHotelClasses(int cityKeyFrom, int countryKey,
            List<int> cityToKeys, List<int> tourKeys,
            List<DateTime> tourDates, List<int> tourNights);

        /// <summary>
        /// Возвращает список питаний
        /// </summary>
        /// <param name="cityKeyFrom">Город вылета</param>
        /// <param name="countryKey">Страна тура</param>
        /// <param name="cityToKeys">Ключи регионов</param>
        /// <param name="tourKeys">Ключи туров</param>
        /// <param name="tourDates">Даты туров</param>
        /// <param name="tourNights">Продолжительности туров</param>
        /// <param name="hotelCats">Категории отелей</param>
        /// <returns></returns>
        [OperationContract]
        [WebGet]
        IDictionary<int, string> GetTourPansions(int cityKeyFrom, int countryKey, List<int> cityToKeys,
            List<int> tourKeys, List<DateTime> tourDates, List<int> tourNights, List<string> hotelCats);

        /// <summary>
        /// Возвращает список отелей
        /// </summary>
        /// <param name="cityKeyFrom">Город вылета</param>
        /// <param name="countryKey">Страна тура</param>
        /// <param name="cityToKeys">Ключи регионов, передавать только в том случае, если реально выбраны пользователем</param>
        /// <param name="tourKeys">Ключи туров</param>
        /// <param name="tourDates">Даты туров</param>
        /// <param name="tourNights">Список продолжительностей</param>
        /// <param name="hotelCats">Категории отелей, передавать только в том случае, если реально выбраны пользователем</param>
        /// <param name="pansionKeys">Ключи питаний</param>
        /// <returns></returns>
        [OperationContract]
        [WebGet]
        // ReSharper disable RedundantNameQualifier
        IEnumerable<Seemplexity.Services.Wcf.ToursSearch.DataModel.Hotel> GetTourHotels(int cityKeyFrom, int countryKey, List<int> cityToKeys, List<int> tourKeys,
            List<DateTime> tourDates, List<int> tourNights, List<string> hotelCats, List<int> pansionKeys);
        // ReSharper restore RedundantNameQualifier


        /// <summary>
        /// Описание по туру
        /// </summary>
        /// <param name="tourKey">Ключ рассчитанного тура (to_key из таблицы tp_tours)</param>
        /// <returns></returns>
        [OperationContract]
        [WebGet]
        string GetTourDescription(int tourKey);

        /// <summary>
        /// Цена за по стране
        /// </summary>
        /// <param name="countryKey">Ключ страны</param>
        /// <returns>true - за человека, false - за номер</returns>
        [OperationContract]
        [WebGet]
        bool IsCountryPriceForMen(int countryKey);

        /// <summary>
        /// Получает список комнат для направлений с ценой за человека
        /// </summary>
        /// <param name="cityKeyFrom">Город вылета</param>
        /// <param name="countryKey">Страна тура</param>
        /// <param name="cityToKeys">Ключ городов прилета</param>
        /// <param name="tourKeys">Ключи туров</param>
        /// <param name="tourDates">Даты заездов</param>
        /// <param name="tourNights">Продолжительности туров в ночах</param>
        /// <param name="hotelKeys">Ключи отелей</param>
        /// <param name="pansionKeys">Ключи питаний</param>
        /// <returns></returns>
        [OperationContract]
        [WebGet]
        IDictionary<int, string> GetTourRooms(int cityKeyFrom, int countryKey,
            List<int> cityToKeys, List<int> tourKeys, List<DateTime> tourDates,
            List<int> tourNights, List<int> hotelKeys, List<int> pansionKeys);

        /// <summary>
        /// Метод возвращает список всех валют, в которых посчитаны туры
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebGet]
        IDictionary<int, string> GetCurrencies();

        /// <summary>
        /// Метод поиска данных
        /// </summary>
        /// <param name="cityKeyFrom">Ключ города начала поездки</param>
        /// <param name="countryKey">Ключ страны тура</param>
        /// <param name="tourKeys">Ключи туров</param>
        /// <param name="tourDates">Даты заездов</param>
        /// <param name="tourNights">Продолжительности туров в ночах</param>
        /// <param name="hotelKeys">Ключи отеле</param>
        /// <param name="pansionKeys">Ключи питаний</param>
        /// <param name="adults">Число основных мест (для стран за номер)</param>
        /// <param name="childs">Число дополнительных мест (для стран за номер)</param>
        /// <param name="firstChildYears">Возраст первого ребенка (для стран за номер)</param>
        /// <param name="secondChildYears">Возраст второго ребенка (для стран за номер)</param>
        /// <param name="hotelQuotaMask">Маска квот для отелей</param>
        /// <param name="aviaQuotaMask">Маска квот для перелетов</param>
        /// <param name="rateKey">Ключ валюты</param>
        /// <param name="maxTourPrice">Максимальная стоимость тура</param>
        /// <param name="rowsPerPage">Число строк на странице</param>
        /// <param name="rowCounterFrom">Номер строки, с которой начинаем поиск</param>
        /// <param name="sortingColumn">Колонка по которой нужно выполнить сортировку</param>
        /// <param name="sortingDirection">Направление сортировки</param>
        /// <param name="flightTicketState">Фильтр по наличию авиобилета</param>
        /// <returns>SearchResult</returns>
        [OperationContract]
        [WebGet]
        SearchResult SearchToursWithRoomPrice(int cityKeyFrom, int countryKey,
            List<int> tourKeys, List<DateTime> tourDates, List<int> tourNights,
            List<int> hotelKeys, List<int> pansionKeys, ushort? adults, ushort? childs,
            ushort? firstChildYears, ushort? secondChildYears, QuotesStates hotelQuotaMask, QuotesStates aviaQuotaMask,
            int rateKey, uint? maxTourPrice, ushort rowsPerPage, uint rowCounterFrom, SortingColumn sortingColumn, SortingDirection sortingDirection);

        /// <summary>
        /// Метод поиска данных
        /// </summary>
        /// <param name="cityKeyFrom">Ключ города начала поездки</param>
        /// <param name="countryKey">Ключ страны тура</param>
        /// <param name="tourKeys">Ключи туров</param>
        /// <param name="tourDates">Даты заездов</param>
        /// <param name="tourNights">Продолжительности туров в ночах</param>
        /// <param name="hotelKeys">Ключи отеле</param>
        /// <param name="pansionKeys">Ключи питаний</param>
        /// <param name="roomTypeKeys">типы номеров</param>
        /// <param name="hotelQuotaMask">Маска квот для отелей</param>
        /// <param name="aviaQuotaMask">Маска квот для перелетов</param>
        /// <param name="rateKey">Ключ валюты</param>
        /// <param name="maxTourPrice">Максимальная стоимость тура</param>
        /// <param name="rowsPerPage">Число строк на странице</param>
        /// <param name="rowCounterFrom">Номер строки, с которой начинаем поиск</param>
        /// <param name="sortingColumn">Колонка по которой нужно выполнить сортировку</param>
        /// <param name="sortingDirection">Направление сортировки</param>
        /// <param name="flightTicketState">Фильтр по наличию авиобилета</param>
        /// <returns>SearchResult</returns>
        [OperationContract]
        [WebGet]
        SearchResult SearchToursWithPriceForMen(int cityKeyFrom, int countryKey, List<int> tourKeys,
            List<DateTime> tourDates, List<int> tourNights,
            List<int> hotelKeys, List<int> pansionKeys, List<int> roomTypeKeys,
            QuotesStates hotelQuotaMask, QuotesStates aviaQuotaMask, int rateKey, uint? maxTourPrice, ushort rowsPerPage,
            uint rowCounterFrom, SortingColumn sortingColumn, SortingDirection sortingDirection, FlightTicketState flightTicketState);
    }
}
