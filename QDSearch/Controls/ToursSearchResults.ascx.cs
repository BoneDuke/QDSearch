using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using QDSearch;
using QDSearch.DataModel;
using QDSearch.Helpers;
using QDSearch.Repository.MtMain;
using QDSearch.Repository.MtSearch;

namespace Controls
{
    public partial class ToursSearchResults : System.Web.UI.UserControl
    {
        public int? CityKeyFrom
        {
            get { return ViewState["CityKeyFrom"] as int?; }
            private set { ViewState["CityKeyFrom"] = value; }
        }

        public int? CountryKey
        {
            get { return ViewState["CountryKey"] as int?; }
            private set { ViewState["CountryKey"] = value; }
        }

        public int[] TourKeys
        {
            get { return ViewState["TourKeys"] as int[]; }
            private set { ViewState["TourKeys"] = value; }
        }

        public DateTime[] TourDates
        {
            get { return ViewState["TourDates"] as DateTime[]; }
            private set { ViewState["TourDates"] = value; }
        }

        public int[] TourNights
        {
            get { return ViewState["TourNights"] as int[]; }
            private set { ViewState["TourNights"] = value; }
        }

        public int[] HotelKeys
        {
            get { return ViewState["HotelKeys"] as int[]; }
            private set { ViewState["HotelKeys"] = value; }
        }

        public int[] PansionKeys
        {
            get { return ViewState["PansionKeys"] as int[]; }
            private set { ViewState["PansionKeys"] = value; }
        }

        public ushort? MainPlaces
        {
            get { return ViewState["MainPlaces"] as ushort?; }
            private set { ViewState["MainPlaces"] = value; }
        }

        public ushort? AddPlaces
        {
            get { return ViewState["AddPlaces"] as ushort?; }
            private set { ViewState["AddPlaces"] = value; }
        }

        public ushort? FirstChildYears
        {
            get { return ViewState["FirstChildYears"] as ushort?; }
            private set { ViewState["FirstChildYears"] = value; }
        }

        public ushort? SecondChildYears
        {
            get { return ViewState["SecondChildYears"] as ushort?; }
            private set { ViewState["SecondChildYears"] = value; }
        }

        public int[] RoomTypeKeys
        {
            get { return ViewState["RoomTypeKeys"] as int[]; }
            private set { ViewState["RoomTypeKeys"] = value; }
        }

        public QuotesStates HotelQuotaMask
        {
            get { return (QuotesStates)(ViewState["HotelQuotaMask"] ?? QuotesStates.None); }
            private set { ViewState["HotelQuotaMask"] = value; }
        }

        public QuotesStates AviaQuotaMask
        {
            get { return (QuotesStates)(ViewState["AviaQuotaMask"] ?? QuotesStates.None); }
            private set { ViewState["AviaQuotaMask"] = value; }
        }

        public int? RateKey
        {
            get { return ViewState["RateKey"] as int?; }
            private set { ViewState["RateKey"] = value; }
        }

        public uint? MaxTourPrice
        {
            get { return ViewState["MaxTourPrice"] as uint?; }
            private set { ViewState["MaxTourPrice"] = value; }
        }

        public ushort RowsPerPage
        {
            get { return (ushort)(ViewState["RowsPerPage"] ?? 20); }
            private set { ViewState["RowsPerPage"] = value; }
        }

        public uint CurrentPage
        {
            get { return (uint)(ViewState["CurrentPage"] ?? 0); }
            private set { ViewState["CurrentPage"] = value; }
        }

        public Tuple<SortingColumn, SortingDirection> CurrentSort
        {
            get
            {
                return
                    (ViewState["CurrentSort"] ??
                     new Tuple<SortingColumn, SortingDirection>(SortingColumn.Price, SortingDirection.Asc)) as
                        Tuple<SortingColumn, SortingDirection>; //todo сортировку по умолчанию вытащить в конфиг
            }
            private set { ViewState["CurrentSort"] = value; }
        }

        protected Dictionary<uint, uint> SearchPages
        {
            get { return ((ViewState["NextRowKey"] as Dictionary<uint, uint>) ?? (ViewState["NextRowKey"] = new Dictionary<uint, uint>())) as Dictionary<uint, uint>; }
        }



        public static string GetTourRecord(SearchResultItem searchItem, bool isAlternatingRecord)
        {
            if (searchItem == null) throw new ArgumentNullException("searchItem");
            if (!searchItem.Hotels.Any()) throw new ArgumentException("Не инициализирован список отелей в SearchResultItem", "searchItem");

            return searchItem.Hotels.Count > 1 ? ManyHotelTourRecord(searchItem, isAlternatingRecord).ToString() : OneHotelTourRecord(searchItem, isAlternatingRecord).ToString();
        }

        public static StringBuilder ManyHotelTourRecord(SearchResultItem si, bool isAlternatingRecord)
        {
            var hNumber = si.Hotels.Count;
            var sb = new StringBuilder();
            //todo: фитча добавить в текущую документацию
            var dates = si.Date.Year == si.DateTourEnd.Year && si.Date.Year == DateTime.Now.Year
                ? String.Format(@"{0:dd.MM}&nbsp;&ndash;&nbsp;{1:dd.MM}", si.Date, si.DateTourEnd)
                : String.Format(@"{0:dd.MM.yy}&nbsp;&ndash;&nbsp;{1:dd.MM.yy}", si.Date, si.DateTourEnd);
            for (int i = 0; i < hNumber; i++)
            {
                if (i == 0)
                {
                    sb.AppendFormat(@"<tr {0}>", isAlternatingRecord ? @"class='TFS_TAltRow TManyHotelRow'" : @"class='TManyHotelRow'");
                    sb.AppendFormat(@"<td rowspan='{0}'>{1}</td>", hNumber, dates);
                    sb.AppendFormat(@"<td>{0}</td>", HttpUtility.HtmlEncode(si.Hotels[i].ResortName));
                    sb.AppendFormat(@"<td>{0}</td>", GetLinkNameHtml(HttpUtility.HtmlEncode(si.Hotels[i].Name), si.Hotels[i].Url, "HotelName", true));
                    sb.AppendFormat(@"<td>{0}</td>", HttpUtility.HtmlEncode(si.Hotels[i].Stars));
                    sb.AppendFormat(@"<td>{0}<br/>{1}</td>", HttpUtility.HtmlEncode(si.Hotels[i].RoomName),
                        HttpUtility.HtmlEncode(si.Hotels[i].RoomCategoryName));
                    sb.AppendFormat(@"<td>{0}</td>", HttpUtility.HtmlEncode(si.Hotels[i].AccomodationName));
                    sb.AppendFormat(@"<td>{0}</td>", HttpUtility.HtmlEncode(si.Hotels[i].PansionName));
                    sb.AppendFormat(@"<td>{0}</td>", HttpUtility.HtmlEncode(si.Hotels[i].NightsCount));
                    sb.AppendFormat(@"<td class='CellPrice' rowspan='{0}'>{1}</td>", hNumber, GetSimpleBasketLinkHtml(si.PriceInRates, si.PriceKey, si.FilterRateCode, si.IsPriceForRoom, si.Date, true));
                    sb.AppendFormat(@"<td rowspan='{0}'>{1}<div>{2}{3}</div></td>", hNumber,
                        GetLinkNameHtml(HttpUtility.HtmlEncode(si.TourName), si.TourUrl, "TourName", true),
                        GetComplexAspxLinkHtml(si.FlightCityKeyFrom == null ? 0 : si.FlightCityKeyFrom.Value, si.CountryKey, si.TourKey, si.HotelKey, si.Date,
                            si.FilterRateCode, true),
                        GetTourDetailsHtml(si.TourHasDescription, si.TourKey));
                    sb = GetHotelQuotasHtml(sb, si.Hotels[i].QuotaState, 0);
                    sb = GetAviaQuotasHtml(sb, hNumber, si.DirectFlightsInfo.ToDictionary(d => d.Key, d => d.Value.QuotaState), si.BackFlightsInfo.ToDictionary(d => d.Key, d => d.Value.QuotaState), si.FlightCityKeyFrom, si.FlightCityKeyTo,
                        si.Date, si.DateTourEnd);
                    sb.Append("</tr>");
                    continue;
                }
                sb.AppendFormat(@"<tr {0}>", isAlternatingRecord ? @"class='TFS_TAltRow TManyHotelRow'" : @"class='TManyHotelRow'");
                sb.AppendFormat(@"<td>{0}</td>", HttpUtility.HtmlEncode(si.Hotels[i].ResortName));
                sb.AppendFormat(@"<td>{0}</td>", GetLinkNameHtml(HttpUtility.HtmlEncode(si.Hotels[i].Name), si.Hotels[i].Url, "HotelName", true));
                sb.AppendFormat(@"<td>{0}</td>", HttpUtility.HtmlEncode(si.Hotels[i].Stars));
                sb.AppendFormat(@"<td>{0}<br/>{1}</td>", HttpUtility.HtmlEncode(si.Hotels[i].RoomName),
                    HttpUtility.HtmlEncode(si.Hotels[i].RoomCategoryName));
                sb.AppendFormat(@"<td>{0}</td>", HttpUtility.HtmlEncode(si.Hotels[i].AccomodationName));
                sb.AppendFormat(@"<td>{0}</td>", HttpUtility.HtmlEncode(si.Hotels[i].PansionName));
                sb.AppendFormat(@"<td>{0}</td>", HttpUtility.HtmlEncode(si.Hotels[i].NightsCount));
                sb = GetHotelQuotasHtml(sb, si.Hotels[i].QuotaState, 0);
                sb.Append("</tr>");
            }
            return sb;
        }

        public static StringBuilder OneHotelTourRecord(SearchResultItem si, bool isAlternatingRecord)
        {
            var ht = si.Hotels[0];
            var sb = new StringBuilder();
            sb.AppendFormat(@"<tr {0}>", isAlternatingRecord ? @"class='TFS_TAltRow'" : String.Empty);
            var dates = si.Date.Year == si.DateTourEnd.Year && si.Date.Year == DateTime.Now.Year
                ? String.Format(@"{0:dd.MM}&nbsp;&ndash;&nbsp;{1:dd.MM}", si.Date, si.DateTourEnd)
                : String.Format(@"{0:dd.MM.yy}&nbsp;&ndash;&nbsp;{1:dd.MM.yy}", si.Date, si.DateTourEnd);
            sb.AppendFormat(@"<td>{0}</td>", dates);
            sb.AppendFormat(@"<td>{0}</td>", HttpUtility.HtmlEncode(ht.ResortName));
            sb.AppendFormat(@"<td>{0}</td>", GetLinkNameHtml(HttpUtility.HtmlEncode(ht.Name), ht.Url, "HotelName", true));
            sb.AppendFormat(@"<td>{0}</td>", HttpUtility.HtmlEncode(ht.Stars));
            sb.AppendFormat(@"<td>{0}<br/>{1}</td>", HttpUtility.HtmlEncode(ht.RoomName),
                HttpUtility.HtmlEncode(ht.RoomCategoryName));
            sb.AppendFormat(@"<td>{0}</td>", HttpUtility.HtmlEncode(ht.AccomodationName));
            sb.AppendFormat(@"<td>{0}</td>", HttpUtility.HtmlEncode(ht.PansionName));
            sb.AppendFormat(@"<td>{0}</td>", HttpUtility.HtmlEncode(ht.NightsCount));
            sb.AppendFormat(@"<td class='CellPrice'>{0}</td>", GetSimpleBasketLinkHtml(si.PriceInRates, si.PriceKey, si.FilterRateCode, si.IsPriceForRoom, si.Date, true));
            sb.AppendFormat(@"<td>{0}<div>{1}{2}</div></td>",
                GetLinkNameHtml(HttpUtility.HtmlEncode(si.TourName), si.TourUrl, "TourName", true),
                GetComplexAspxLinkHtml(si.FlightCityKeyFrom == null ? 0 : si.FlightCityKeyFrom.Value, si.CountryKey, si.TourKey, si.HotelKey, si.Date, si.FilterRateCode, true),
                GetTourDetailsHtml(si.TourHasDescription, si.TourKey));
            sb = GetHotelQuotasHtml(sb, si.Hotels[0].QuotaState, 0);
            //todo: вынести список классов авиоперелетов в конфиг
            sb = GetAviaQuotasHtml(sb, 0, si.DirectFlightsInfo.ToDictionary(d => d.Key, d => d.Value.QuotaState), si.BackFlightsInfo.ToDictionary(d => d.Key, d => d.Value.QuotaState), si.FlightCityKeyFrom, si.FlightCityKeyTo,
                si.Date, si.DateTourEnd);
            return sb.AppendFormat("</tr>");
        }

        public static string GetTourDetailsHtml(bool hasDescription, int tourKey)
        {
            if (!hasDescription) return String.Empty;

            return String.Format(@"<img class='TourInfoImg' onclick='showTourDescWnd({0})' src='./styles/images/tour-info.png'/>", tourKey);
        }

        public static string GetLinkNameHtml(string name, string url, string cssClass, bool inNewWindow)
        {
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");

            var css = String.IsNullOrWhiteSpace(cssClass) ? String.Empty : String.Format(@"class='{0}'", cssClass);
            return String.IsNullOrWhiteSpace(url)
                ? String.Format(@"<span {0}>{1}</span>",
                    css, name)
                : inNewWindow
                    ? String.Format(@"<a {0} target='_blank' href='{1}'>{2}</a>", css, url, name)
                    : String.Format(@"<a {0} href='{1}'>{2}</a>", css, url, name);
        }

        public static string GetSimpleBasketLinkHtml(Dictionary<string, double> priceInRates, int priceKeyForBasket, string rateCode, bool isPriceForRoom, DateTime arrivalDate, bool inNewWindow)
        {
            //todo: вынести адрес simplebasket в конфиг
            return inNewWindow
                ? String.Format(
                    @"<a target='_blank' href='http://online.solvex.travel/SimpleBasket.aspx?priceKey={0}&date={1:yyyy-MM-dd}&r=1'>{2}/{3} <img alt='AddToBasket' class='BasketBtn' src='./styles/images/cart.png' /></a>",
                    priceKeyForBasket, arrivalDate, GetPriceHtml(priceInRates, rateCode), isPriceForRoom ? "номер" : "чел.")
                : String.Format(
                    @"<a href='http://online.solvex.travel/SimpleBasket.aspx?priceKey={0}&date={1:yyyy-MM-dd}&r=1'>{2}/{3} <img alt='AddToBasket' class='BasketBtn' src='./styles/images/cart.png' /></a>",
                    priceKeyForBasket, arrivalDate, GetPriceHtml(priceInRates, rateCode), isPriceForRoom ? "номер" : "чел.");
        }

        public static string GetPriceHtml(Dictionary<string, double> priceInRates, string visibleRateCode)
        {
            var priceHtml = new StringBuilder();
            foreach (var priceInRate in priceInRates)
            {
                if (priceInRate.Key == visibleRateCode)
                {
                    priceHtml.AppendFormat(@"<span priceRate='{0}' class='PriceWrapper'><span class='Price'>{1:N0}</span>&nbsp;<span class='Rate'>{0}</span></span>", priceInRate.Key, priceInRate.Value);
                    continue;
                }
                priceHtml.AppendFormat(@"<span priceRate='{0}' style='display:none;' class='PriceWrapper'><span class='Price'>{1:N0}</span>&nbsp;<span class='Rate'>{0}</span></span>", priceInRate.Key, priceInRate.Value);
            }
            return priceHtml.ToString();
        }

        public static string GetComplexAspxLinkHtml(int cityKeyFrom, int countryKeyTo, int tourKey, int hotelKey, DateTime dateFrom, string filterRateCode, bool inNewWindow)
        {
            //todo: вынести адрес complex.aspx в конфиг
            //todo: название все цены вынести в конфиг
            return inNewWindow
                ? String.Format(
                    @"<a class='AllPricesLnk' target='_blank' href='http://online.solvex.travel/pricelist/complex.aspx?departFrom={0}&country={1}&tour={2}&hotel={3}&dateFrom={4:yyyy-MM-dd}&rate={5}&ratesSwitcher=1'><img src='./styles/images/all-prices.png' alt='все цены'/></a>",
                    cityKeyFrom, countryKeyTo, tourKey, hotelKey, dateFrom, filterRateCode)
                : String.Format(
                    @"<a class='AllPricesLnk' href='http://online.solvex.travel/pricelist/complex.aspx?departFrom={0}&country={1}&tour={2}&hotel={3}&dateFrom={4:yyyy-MM-dd}&rate={5}&ratesSwitcher=1'><img src='./styles/images/all-prices.png' alt='все цены' /></a>",
                    cityKeyFrom, countryKeyTo, tourKey, hotelKey, dateFrom, filterRateCode);
        }

        public static string GetQuotaText(QuotesStates qs)
        {
            switch (qs)
            {
                case QuotesStates.None:
                    return "-";
                case QuotesStates.Availiable:
                    return "есть";
                case QuotesStates.No:
                    return "нет";
                case QuotesStates.Request:
                    return "запрос";
                case QuotesStates.Small:
                    return "мало";
                default:
                    throw new ArgumentOutOfRangeException("qs", qs, "Событие получено от незарегестрированого элемента.");
            }
        }

        public static StringBuilder GetAviaQuotasHtml(StringBuilder sb, int mergedRows, Dictionary<int, QuotaStatePlaces> aviaQuotesTo, Dictionary<int, QuotaStatePlaces> aviaQuotesFrom, int? flightCityKeyFrom, int? flightCityKeyTo, DateTime dateFrom, DateTime dateTo)
        {
            if (sb == null) throw new ArgumentNullException("sb");
            //if (aviaQuotesTo == null) throw new ArgumentNullException("aviaQuotesTo");
            //if (aviaQuotesTo.Values.Any(q => q.QuotaState != QuotesStates.Undefined) && !flightCityKeyTo.HasValue) throw new ArgumentNullException("flightCityKeyTo", "Параметр не может быть null так как есть квоты.");
            //if (aviaQuotesFrom == null) throw new ArgumentNullException("aviaQuotesFrom");
            //if (aviaQuotesFrom.Values.Any(q => q.QuotaState != QuotesStates.Undefined) && !flightCityKeyFrom.HasValue) throw new ArgumentNullException("flightCityKeyFrom", "Параметр не может быть null так как есть квоты.");

            for (int i = 0; i < aviaQuotesTo.Count; i++)
            {
                var aviaQuote = aviaQuotesTo[i];
                var aviaQuotaState = aviaQuote.QuotaState;
                var imgHtml = String.Format(@"<img alt='{0}' src='{1}' />", aviaQuotaState,
                    String.Format(@"./styles/images/{0}-plain-forward.png", aviaQuotaState).ToLower());
                var linkHtml = String.Format(
                    @"<a target='_blank' href='http://online.solvex.travel/Extra/aviaquotes.aspx?cityFrom={0}&cityTo={1}&dateFrom={2:yyyy-MM-dd}&dateTo={3:yyyy-MM-dd}&showAnother=1'>{4}</a>",
                    flightCityKeyFrom, flightCityKeyTo, dateFrom, dateTo,
                    aviaQuotaState == QuotesStates.Small
                        ? String.Format("{0}<div class='TFS_QuotaPlaces'>{1}</div>", imgHtml, aviaQuote.Places)
                        : imgHtml);

                sb.AppendFormat(@"<td {0} class='TFS_FlightQouta TFS_FlightQoutaTo TFS_FqTo_{1}'>{2}</td>",
                    mergedRows > 1 ? String.Format(@"rowspan='{0}'", mergedRows) : String.Empty, aviaQuotaState,
                    aviaQuotaState == QuotesStates.None
                        ? @"&nbsp;"
                        : linkHtml);

                aviaQuote = aviaQuotesFrom[i];
                aviaQuotaState = aviaQuote.QuotaState;
                imgHtml = String.Format(@"<img alt='{0}' src='{1}' />", aviaQuotaState,
                    String.Format(@"./styles/images/{0}-plain.png", aviaQuotaState).ToLower());
                //todo: проверить корректность ссылка на обратный рейс
                linkHtml = String.Format(
                    @"<a target='_blank' href='http://online.solvex.travel/Extra/aviaquotes.aspx?cityFrom={0}&cityTo={1}&dateFrom={2:yyyy-MM-dd}&dateTo={3:yyyy-MM-dd}&showAnother=1'>{4}</a>",
                    flightCityKeyFrom, flightCityKeyTo, dateFrom, dateTo,
                    aviaQuotaState == QuotesStates.Small
                        ? String.Format("{0}<div class='TFS_QuotaPlaces'>{1}</div>", imgHtml, aviaQuote.Places)
                        : imgHtml);

                sb.AppendFormat(@"<td {0} class='TFS_FlightQouta TFS_FlightQoutaForward TFS_FqForward_{1}'>{2}</td>",
                    mergedRows > 1 ? String.Format(@"rowspan='{0}'", mergedRows) : String.Empty, aviaQuotaState,
                    aviaQuotaState == QuotesStates.None
                        ? @"&nbsp;"
                        : linkHtml);
            }
            return sb;
        }

        public static StringBuilder GetHotelQuotasHtml(StringBuilder sb, QuotaStatePlaces hotelQuota, int mergedRows)
        {
            if (sb == null) throw new ArgumentNullException("sb");

            var hotelQuotaState = hotelQuota.QuotaState;
            var imgHtml = String.Format(@"<img alt='{0}' src='{1}' />", hotelQuotaState,
                String.Format(@"./styles/images/{0}-hotel.png", hotelQuotaState).ToLower());

            return sb.AppendFormat(@"<td class='TFS_HotelQuota TFS_Hq_{0}' {1}>{2}</td>", hotelQuotaState,
                mergedRows > 1 ? String.Format(@"rowspan='{0}'", mergedRows) : String.Empty,
                hotelQuotaState != QuotesStates.None
                    ? (hotelQuotaState == QuotesStates.Small
                        ? String.Format("{0}<div class='TFS_QuotaPlaces'>{1}</div>", imgHtml, hotelQuota.Places)
                        : imgHtml)
                    : @"&nbsp;");
        }


        protected void Page_Load(object sender, EventArgs e)
        {
        }

        public void ShowTours(int cityKeyFrom, int countryKey, int[] tourKeys,
            DateTime[] tourDates, int[] tourNights, int[] hotelKeys,
            int[] pansionKeys, ushort? mainPlaces, ushort? addPlaces, ushort? firstChildYears,
            ushort? secondChildYears, int[] roomTypeKeys, QuotesStates hotelQuotaMask, QuotesStates aviaQuotaMask,
            int rateKey, uint? maxTourPrice, ushort rowsPerPage)
        {
            if (tourKeys == null || !tourKeys.Any()) throw new ArgumentNullException("tourKeys");
            if (tourDates == null || !tourDates.Any()) throw new ArgumentNullException("tourDates");
            if (tourNights == null || !tourNights.Any()) throw new ArgumentNullException("tourNights");
            if (hotelKeys == null || !hotelKeys.Any()) throw new ArgumentNullException("hotelKeys");
            if (pansionKeys == null || !pansionKeys.Any()) throw new ArgumentNullException("pansionKeys");
            if ((roomTypeKeys != null && roomTypeKeys.Any() && mainPlaces.HasValue) || ((roomTypeKeys == null || !roomTypeKeys.Any()) && !mainPlaces.HasValue))
                throw new ArgumentException("Одноверменно не могут быть указаны значение количество взрослых и тип номера", "roomTypeKeys");
            if (addPlaces > 0 && !firstChildYears.HasValue)
                throw new ArgumentNullException("firstChildYears", "Не указан возраст первого ребенка");
            if (addPlaces > 1 && !secondChildYears.HasValue)
                throw new ArgumentNullException("secondChildYears", "Не указан возраст второго ребенка");

            CityKeyFrom = cityKeyFrom;
            CountryKey = countryKey;
            TourKeys = tourKeys;
            TourDates = tourDates;
            TourNights = tourNights;
            HotelKeys = hotelKeys;
            PansionKeys = pansionKeys;
            MainPlaces = mainPlaces;
            AddPlaces = addPlaces;
            FirstChildYears = firstChildYears;
            SecondChildYears = secondChildYears;
            RoomTypeKeys = roomTypeKeys;
            HotelQuotaMask = hotelQuotaMask;
            AviaQuotaMask = aviaQuotaMask;
            RateKey = rateKey;
            MaxTourPrice = maxTourPrice;
            RowsPerPage = rowsPerPage;

            using (var mtmDc = new MtMainDbDataContext())
            {
                using (var mtsDc = new MtSearchDbDataContext())
                {
                    var searchResult = mtsDc.PagingOnClient(mtmDc, CityKeyFrom.Value, CountryKey.Value, TourKeys, TourDates,
                        TourNights,
                        HotelKeys, PansionKeys, MainPlaces, AddPlaces, FirstChildYears, SecondChildYears,
                        RoomTypeKeys,
                        HotelQuotaMask, AviaQuotaMask | QuotesStates.None, RateKey.Value, MaxTourPrice, RowsPerPage, 0, CurrentSort);

                    CurrentSort = searchResult.SortType;
                    SearchPages.Clear();

                    if (!searchResult.SearchItems.Any())
                    {
                        LtMessage.Text =
                            @"<div id='SearchResultsMsg' class='TFS_SearchResultsMsg'><span>К сожалению по вашему запросу ничего не найдено. Попробуйте изменить параметры поиска. Например квоты или количество едущих людей и т.д.</span></div>";
                        Web.ShowMessage(this,
                            @"К сожалению по вашему запросу ничего не найдено. Попробуйте изменить параметры поиска. Например квоты или количество едущих людей и т.д.");
                        Web.ScrollToElement(this, "SearchResultsMsg");

                        //koshelev
                        //сделал обработку того, что ни один тур не найден
                        RepFindedTours.DataSource = null;
                        RepFindedTours.DataBind();

                        RepPagesTop.DataSource = null;
                        RepPagesTop.DataBind();
                        RepPagesBottom.DataSource = null;
                        RepPagesBottom.DataBind();
                        return;
                    }

                    LtMessage.Text = String.Empty;

                    // добовляем первую страницу
                    SearchPages.Add(1, 0);
                    CurrentPage = 1;

                    // инициализируем следующую страницу
                    if (searchResult.IsMorePages)
                        SearchPages.Add(CurrentPage + 1, searchResult.NextPageRowCounter);

                    RepPagesTop.DataSource = SearchPages;
                    RepPagesTop.DataBind();
                    RepPagesBottom.DataSource = SearchPages;
                    RepPagesBottom.DataBind();

                    RepFindedTours.DataSource = searchResult.SearchItems;
                    RepFindedTours.DataBind();
                }
            }
        }

        public void ClearSeachedTours()
        {
            RepFindedTours.DataSource = null;
            RepFindedTours.DataBind();
            RepPagesTop.DataSource = null;
            RepPagesTop.DataBind();
            RepPagesBottom.DataSource = null;
            RepPagesBottom.DataBind();
            SearchPages.Clear();
            CurrentPage = 0;
        }
        protected void RepPagesItem_Command(object sender, CommandEventArgs e)
        {
            var linkButton = sender as LinkButton;
            if (linkButton == null) throw new ArgumentNullException("sender");
            if (e == null) throw new ArgumentNullException("e");

            using (var mtmDc = new MtMainDbDataContext())
            {
                using (var mtsDc = new MtSearchDbDataContext())
                {
                    switch (e.CommandName)
                    {
                        case "PreviosPage":
                            CurrentPage--;
                            break;
                        case "PageToGo":
                            CurrentPage = uint.Parse(e.CommandArgument.ToString());
                            break;
                        case "NextPage":
                            CurrentPage++;
                            break;
                        default:
// ReSharper disable once NotResolvedInText
                            throw new ArgumentOutOfRangeException("e.CommandName", e.CommandName,
                                "Событие получено от незарегестрированого элемента.");
                    }

                    if (1 > CurrentPage || CurrentPage > SearchPages.Count)
                        throw new IndexOutOfRangeException(
                            String.Format("Индекс текущей страницы поиска вышел за границы диапозона - {0}", CurrentPage));

                    var searchResult = mtsDc.PagingOnClient(mtmDc, CityKeyFrom.Value, CountryKey.Value, TourKeys, TourDates,
                        TourNights,
                        HotelKeys, PansionKeys, MainPlaces, AddPlaces, FirstChildYears, SecondChildYears,
                        RoomTypeKeys,
                        HotelQuotaMask, AviaQuotaMask | QuotesStates.None, RateKey.Value, MaxTourPrice, RowsPerPage,
                        SearchPages[CurrentPage],
                        CurrentSort);

                    CurrentSort = searchResult.SortType;

                    if (searchResult.IsMorePages && !SearchPages.ContainsKey(CurrentPage + 1))
                        SearchPages.Add(CurrentPage + 1, searchResult.NextPageRowCounter);

                    RepPagesTop.DataSource = SearchPages;
                    RepPagesTop.DataBind();
                    RepPagesBottom.DataSource = SearchPages;
                    RepPagesBottom.DataBind();

                    RepFindedTours.DataSource = searchResult.SearchItems;
                    RepFindedTours.DataBind();

                    Web.ScrollToElement(this, RepPagesTop.ClientID);
                    linkButton.Focus();
                }
            }
        }

        protected void HeaderCol_OnCommand(object sender, CommandEventArgs e)
        {
            var linkButton = sender as LinkButton;
            if (linkButton == null) throw new ArgumentNullException("sender");
            if (e == null) throw new ArgumentNullException("e");

            if (e.CommandName != "Sort")
                return;

            var column = (SortingColumn)Enum.Parse(typeof(SortingColumn), e.CommandArgument.ToString());

            if (column == CurrentSort.Item1)
                CurrentSort = new Tuple<SortingColumn, SortingDirection>(CurrentSort.Item1,
                    CurrentSort.Item2 == SortingDirection.Asc ? SortingDirection.Desc : SortingDirection.Asc);
            else
                CurrentSort = new Tuple<SortingColumn, SortingDirection>(column, SortingDirection.Asc);

            using (var mtmDc = new MtMainDbDataContext())
            {
                using (var dc = new MtSearchDbDataContext())
                {
                    var searchResult = dc.PagingOnClient(mtmDc, CityKeyFrom.Value, CountryKey.Value, TourKeys, TourDates,
                        TourNights,
                        HotelKeys, PansionKeys, MainPlaces, AddPlaces, FirstChildYears, SecondChildYears, RoomTypeKeys,
                        HotelQuotaMask, AviaQuotaMask | QuotesStates.None, RateKey.Value, MaxTourPrice, RowsPerPage,
                        SearchPages[CurrentPage],
                        CurrentSort);

                    RepFindedTours.DataSource = searchResult.SearchItems;
                    RepFindedTours.DataBind();
                }
            }

            Web.ScrollToElement(this, RepPagesTop.ClientID);
            linkButton.Focus();
        }
    }
}