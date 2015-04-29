using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using QDSearch;
using QDSearch.DataModel;
using QDSearch.Extensions;
using QDSearch.Helpers;
using QDSearch.Repository.MtMain;
using QDSearch.Repository.MtSearch;
using QDSearch.Repository.SftWeb;

namespace Controls
{
    public partial class ToursSearchFilter : UserControl
    {
        private class FilterDetails
        {
            public readonly Action<MtMainDbDataContext, MtSearchDbDataContext, SftWebDbDataContext, bool> LoadDataAction;
            public readonly Func<bool> SelectValueByQsAction;
            public bool IsOptional;

            public FilterDetails(Action<MtMainDbDataContext, MtSearchDbDataContext, SftWebDbDataContext, bool> loadDataAction, Func<bool> selectValueByQsAction, bool isOptional = false)
            {
                LoadDataAction = loadDataAction;
                SelectValueByQsAction = selectValueByQsAction;
                IsOptional = isOptional;
            }
        }

        #region fields...

        private static readonly object EventClick = new object();
        private static readonly object EventFilterSelectionChanged = new object();

        private Dictionary<Control, FilterDetails> _filterDetails;
        private QueryStringParametrs _queryStringParametrs;

        #endregion

        #region propities

        public int SelectedCityFromKey { get { return int.Parse(RblCitiesFromFilter.SelectedValue); } }

        public int SelectedCountryToKey { get { return int.Parse(DdlContriesToFilter.SelectedValue); } }

        public int[] SelectedTourTypeKeys
        {
            get { return ChblTourTypes.SelectedValues.Select(int.Parse).ToArray(); }
        }

        public int[] SelectedCitiesToKeys { get { return ChblCitiesToFilter.SelectedValues.Select(int.Parse).ToArray(); } }

        public int[] SelectedTourKeys
        {
            get
            {
                return DdlToursFilter.SelectedValue == "-1"
                    ? (from ListItem item in DdlToursFilter.Items
                       where item.Value != "-1"
                       select int.Parse(item.Value)).ToArray()
                    : (from ListItem item in DdlToursFilter.Items
                       where item.Selected && item.Value != "-1"
                       select int.Parse(item.Value)).ToArray();
            }
        }

        public DateTime[] SelectedArrivalDates { get { return ArrivalDatesFilter.SeletedArrivaleDays.ToArray(); } }

        public int[] SelectedNights { get { return ChblNightsFilter.SelectedValues.Select(int.Parse).ToArray(); } }

        public string[] SelectedHotelCategoriesKeys { get { return ChblHotelCategoriesFilter.SelectedValues.ToArray(); } }

        public int[] SelectedPansionKeys { get { return ChblPansionsFilter.SelectedValues.Select(int.Parse).ToArray(); } }

        public int[] SelectedHotelKeys { get { return UcHotelsFilter.SelectedValues.Select(int.Parse).ToArray(); } }

        public bool IsPriceForRoom { get { return PnPersons.Visible; } }

        public int[] SelectedRoomTypeKey
        {
            get
            {
                if (IsPriceForRoom) return null;

                return DdlRoomTypesFilter.SelectedValue == "-1"
                    ? (from ListItem item in DdlRoomTypesFilter.Items
                       where item.Value != "-1"
                       select int.Parse(item.Value)).ToArray()
                    : (from ListItem item in DdlRoomTypesFilter.Items
                       where item.Selected && item.Value != "-1"
                       select int.Parse(item.Value)).ToArray();
            }
        }

        public ushort? SelectedAdultsNumber { get { return IsPriceForRoom ? Converters.GetByteSafe(TbAdultsFilter.Text) : null; } }

        public ushort? SelectedChildsNumber { get { return IsPriceForRoom ? Converters.GetByteSafe(TbChildsFilter.Text) : null; } }

        public ushort? SelectedFistChildAge { get { return IsPriceForRoom ? Converters.GetByteSafe(TbFirstChildAge.Text) : null; } }

        public ushort? SelectedSecondChildAge { get { return IsPriceForRoom ? Converters.GetByteSafe(TbSecondChildAge.Text) : null; } }

        public QuotesStates SelectedRoomsQuotesStates
        {
            get
            {
                QuotesStates qs;
                return (ChRoomsEnabled.Checked ? QuotesStates.Availiable : QuotesStates.None) |
                       (ChRoomsNo.Checked ? QuotesStates.No : QuotesStates.None) |
                       (ChRoomsRQ.Checked ? QuotesStates.Request : QuotesStates.None);
            }
        }

        public QuotesStates SelectedAviaQuotesStates
        {
            get
            {
                return (ChAviaAvailiable.Checked ? QuotesStates.Availiable : QuotesStates.None) |
                       (ChAviaNo.Checked ? QuotesStates.No : QuotesStates.None) |
                       (ChAviaRQ.Checked ? QuotesStates.Request : QuotesStates.None);
            }
        }

        public int SelectedRateKey { get { return int.Parse(RblRates.SelectedValue); } }

        public uint? SelectedMaxPrice { get { return (uint?)Converters.GetIntSafe(TbMaxPrice.Text); } }

        public ushort SelectedRowsNumber { get { return ushort.Parse(DdlRowsNumber.SelectedValue); } }

        public bool IsCityFromChangedByUser
        {
            get { return (bool)(ViewState["IsCityFromChangedByUser"] ?? false); }
            set { ViewState["IsCityFromChangedByUser"] = value; }
        }

        #endregion

        #region events...

        /// <summary>
        /// Occurs when the search button is clicked.
        /// </summary>
        public event EventHandler SearchBottonClick
        {
            add
            {
                Events.AddHandler(EventClick, value);
            }
            remove
            {
                Events.RemoveHandler(EventClick, value);
            }
        }

        /// <summary>
        /// Raises the SeachBottonClick event of the ToursSearchFilter control.
        /// </summary>
        /// <param name="e">The event data. </param>
        protected virtual void OnSearchBottonClick(EventArgs e)
        {
            var eventHandler = (EventHandler)Events[EventClick];
            if (eventHandler == null)
                return;
            eventHandler(this, e);
        }

        /// <summary>
        ///    Occurs when the filter selection changed upon the server postback. 
        /// </summary>
        public event EventHandler FilterSelectionChanged
        {
            add
            {
                Events.AddHandler(EventFilterSelectionChanged, value);
            }
            remove
            {
                Events.RemoveHandler(EventFilterSelectionChanged, value);
            }
        }

        /// <summary>
        /// Raises the FilterSelectionChanged event of the ToursSearchFilter control.
        /// </summary>
        /// <param name="e">The event data. </param>
        protected virtual void OnFilterSelectionChanged(EventArgs e)
        {
            var onChangeHandler = (EventHandler)Events[EventFilterSelectionChanged];
            if (onChangeHandler != null) onChangeHandler(this, e);
        }

        #endregion

        protected void Page_Init(object sender, EventArgs e)
        {
            _queryStringParametrs = new QueryStringParametrs(Request);
            Web.RegisterStartupScript(this, @"startupToursFilter();", true);

            _filterDetails = new Dictionary<Control, FilterDetails>
            {
                {DdlContriesToFilter, new FilterDetails((mtmDc, mtsDc, sftDc, isReinit) => LoadCountriesTo(mtsDc, sftDc), SetCountriesToByQs)},
                {LtCountryInfo, new FilterDetails((mtmDc, mtsDc, sftDc, isReinit) => LoadCountryToDesc(mtmDc), () => true)},
                {RblCitiesFromFilter, new FilterDetails((mtmDc, mtsDc, sftDc, isReinit) => LoadCitiesFrom(mtsDc, sftDc, isReinit), SetCitesFromByQs)},
                {ChblTourTypes, new FilterDetails((mtmDc, mtsDc, sftDc, isReinit) => LoadToursTypes(mtsDc, sftDc, isReinit), SetToursTypesByQs)},
                {ChblCitiesToFilter, new FilterDetails((mtmDc, mtsDc, sftDc, isReinit) => LoadCitiesTo(mtsDc, sftDc, isReinit), SetCitiesByQs)},
                {DdlToursFilter, new FilterDetails((mtmDc, mtsDc, sftDc, isReinit) => LoadTours(mtsDc, sftDc, isReinit), SetToursByQs)},
                {ArrivalDatesFilter, new FilterDetails((mtmDc, mtsDc, sftDc, isReinit) => LoadArrivalDates(mtsDc, sftDc, isReinit), SetArrivalDatesByQs)},
                {ChblNightsFilter, new FilterDetails((mtmDc, mtsDc, sftDc, isReinit) => LoadNights(mtsDc, sftDc, isReinit), SetNightsByQs)},
                {ChblHotelCategoriesFilter, new FilterDetails((mtmDc, mtsDc, sftDc, isReinit) => LoadHotelCategories(mtsDc, sftDc, isReinit), SetHotelCategoriesByQs)},
                {ChblPansionsFilter, new FilterDetails((mtmDc, mtsDc, sftDc, isReinit) => LoadPansions(mtsDc, sftDc, isReinit), SetPansionsByQs)},
                {UcHotelsFilter, new FilterDetails((mtmDc, mtsDc, sftDc, isReinit) => LoadHotels(mtsDc, sftDc, isReinit), SetHotelsByQs)},
                {DdlRoomTypesFilter, new FilterDetails(LoadRoomTypesAndMens, SetRoomTypesAndMensByQs)},
                {RblRates, new FilterDetails((mtmDc, mtsDc, sftDc, isReinit) =>  LoadRates(mtsDc, isReinit), SetRatesByQs)},
            };
        }

        protected void Page_Load(object sender, EventArgs e)
        {


            if (!Page.IsPostBack)
            {

                if (!_queryStringParametrs.IsEmpty)
                {
                    if (!_queryStringParametrs.IsParametrsValid || !InitFiltersByQs())
                    {
                        Web.ShowMessage(this, "Строка запроса не корректна.");
                        Response.Redirect(Web.GetPageUrl(), true);
                    }
                    if (_queryStringParametrs.ShowResults.HasValue && _queryStringParametrs.ShowResults.Value)
                        OnSearchBottonClick(EventArgs.Empty);
                    if (!_queryStringParametrs.ShowResults.HasValue)
                    {
                        //todo: добавить в конфиг, действие по умолчанию в случае параметризованного запроса искать или не искать.
                        OnSearchBottonClick(EventArgs.Empty);
                    }
                }
                else
                    InitFilters();
                InitQueryString();
            }
        }


        #region methods...

        private void InitQueryString()
        {
            // часть параметров инициализируются из JavaScript на стороне клиента

            var queryBuilder = new StringBuilder();
            queryBuilder.Append(Web.GetPageUrl());
            queryBuilder.AppendFormat(
                "?country={0}&departFrom={1}&dateFrom={2:yyyy-MM-dd}&dateTo={3:yyyy-MM-dd}&filterHotelsArrNights={4}&pageSize={5}",
                SelectedCountryToKey,
                SelectedCityFromKey, SelectedArrivalDates.Min(),
                SelectedArrivalDates.Max(), UcHotelsFilter.IsFilterByArrNights, SelectedRowsNumber);

            if (!ChblTourTypes.IsAllOptionsSelected)
                queryBuilder.AppendFormat("&tourtype={0}", String.Join(",", SelectedTourTypeKeys));

            if (!ChblCitiesToFilter.IsAllOptionsSelected)
                queryBuilder.AppendFormat("&city={0}", String.Join(",", SelectedCitiesToKeys));

            if (int.Parse(DdlToursFilter.SelectedValue) > 0)
                queryBuilder.AppendFormat("&tour={0}", String.Join(",", SelectedTourKeys));

            if (!ChblNightsFilter.IsAllOptionsSelected)
                queryBuilder.AppendFormat("&nights={0}", String.Join(",", SelectedNights));

            if (!ChblHotelCategoriesFilter.IsAllOptionsSelected)
                queryBuilder.AppendFormat("&stars={0}", Server.UrlEncode(String.Join(",", SelectedHotelCategoriesKeys)));

            if (!ChblPansionsFilter.IsAllOptionsSelected)
                queryBuilder.AppendFormat("&pansion={0}", String.Join(",", SelectedPansionKeys));

            if (!UcHotelsFilter.IsAllOptionsSelected)
                queryBuilder.AppendFormat("&hotel={0}", String.Join(",", SelectedHotelKeys));

            if (PnRoomTypes.Visible && int.Parse(DdlRoomTypesFilter.SelectedValue) > 0)
                queryBuilder.AppendFormat("&room={0}", String.Join(",", SelectedRoomTypeKey));

            BtnSearch.PostBackUrl = Uri.EscapeUriString(queryBuilder.ToString());
        }



        private static IEnumerable<KeyValuePair<int, string>> AddFirstCustomElement(int firstElemetnKey, string firstElementValue,
            IEnumerable<KeyValuePair<int, string>> dictionaryToAdd)
        {
            if (dictionaryToAdd == null) throw new ArgumentNullException("dictionaryToAdd");

            IDictionary<int, string> tmp = new Dictionary<int, string>();
            tmp.Add(new KeyValuePair<int, string>(firstElemetnKey, firstElementValue));
            return tmp.Concat(dictionaryToAdd);
        }

        private static string GetHotelNameHtml(string hotelName, string hotelClass, string resort, string url)
        {
            if (hotelName == null) throw new ArgumentNullException("hotelName");
            if (hotelClass == null) throw new ArgumentNullException("hotelClass");

            string hName = HttpUtility.HtmlEncode(String.IsNullOrWhiteSpace(resort)
                ? String.Format("{0} {1}", hotelName, hotelClass)
                : String.Format("{0} {1} ({2})", hotelName, hotelClass, resort));

            return String.IsNullOrWhiteSpace(url)
                ? hName
                : String.Format(@"<a href=""{0}"" target=""_blank"">{1}</a>", url, hName);
        }

        private void InitFilters(Control startFromFilter = null, bool isReinit = false, bool skipFirstFilter = true)
        {
            var finded = startFromFilter == null;

            using (var mtmDc = new MtMainDbDataContext())
            {
                using (var mtsDc = new MtSearchDbDataContext())
                {
                    using (var sftDc = new SftWebDbDataContext())
                    {
                        foreach (KeyValuePair<Control, FilterDetails> filterDetail in _filterDetails)
                        {
                            if (!skipFirstFilter)
                                finded = startFromFilter == filterDetail.Key;

                            if (finded)
                                filterDetail.Value.LoadDataAction(mtmDc, mtsDc, sftDc, isReinit);
                            else
                                finded = startFromFilter == filterDetail.Key;
                        }
                    }
                }
            }

            //todo: добавить инициализацию фильтров значениями по умолчанию из web.config
            // для тех фильтров которые не загружаются из БД
            if (startFromFilter == null) // для тех фильтров значение которых не меняются при изменении других фильтров
                DdlRowsNumber.SelectedValue = DdlRowsNumber.Items.FindByValue(Globals.Settings.TourFilters.DefaultToursNumberOnPage.ToString(CultureInfo.InvariantCulture)).Value;
        }

        private void ReInitChildFilters(Control currentFilter, bool skipCurrent = true)
        {
            if (currentFilter == null) throw new ArgumentNullException("currentFilter");

            InitFilters(currentFilter, true, skipCurrent);
        }

        private bool InitFiltersByQs()
        {
            using (var mtmDc = new MtMainDbDataContext())
            {
                using (var mtsDc = new MtSearchDbDataContext())
                {
                    using (var sftDc = new SftWebDbDataContext())
                    {
                        foreach (KeyValuePair<Control, FilterDetails> filterDetail in _filterDetails)
                        {
                            filterDetail.Value.LoadDataAction(mtmDc, mtsDc, sftDc, false);
                            if (!filterDetail.Value.SelectValueByQsAction())
                                return false;
                        }
                    }
                }
            }

            if (!SetAviaQuotesByQs()) return false;
            if (!SetRoomQuotesByQs()) return false;
            if (!SetMaxPriceByQs()) return false;
            if (!SetRowsNumberByQs()) return false;
            return true;
        }

        private void LoadCountriesTo(MtSearchDbDataContext mtsDc, SftWebDbDataContext sftDc)
        {
            if (mtsDc == null) throw new ArgumentNullException("mtsDc");

            DdlContriesToFilter.DataSource = mtsDc.GetCountriesTo(sftDc).ToDictionary(k => k.Key, v => HttpUtility.HtmlEncode(v.Value));
            DdlContriesToFilter.DataBind();
        }

        private bool SetCountriesToByQs()
        {
            if (_queryStringParametrs.IsEmpty || !_queryStringParametrs.IsParametrsValid)
                return false;
            if (!_queryStringParametrs.CountryToKey.HasValue)
                return true;

            var item = DdlContriesToFilter.Items.FindByValue(_queryStringParametrs.CountryToKey.ToString());
            if (item == null)
                return false;

            DdlContriesToFilter.SelectedValue = item.Value;

            return true;
        }

        private void LoadCountryToDesc(MtMainDbDataContext dc)
        {
            if (dc == null) throw new ArgumentNullException("dc");

            if (DdlContriesToFilter.Items.Count == 0)
            {
                LtCountryInfo.Text = String.Empty;
                return;
            }

            LtCountryInfo.Text = dc.GetCountryDescription(Convert.ToInt32(DdlContriesToFilter.SelectedValue));
        }

        private void LoadCitiesFrom(MtSearchDbDataContext mtsDc, SftWebDbDataContext sftDc, bool isReinit = false)
        {
            if (mtsDc == null) throw new ArgumentNullException("mtsDc");

            if (DdlContriesToFilter.Items.Count == 0)
            {
                RblCitiesFromFilter.Items.Clear();
                return;
            }

            // Код ниже реализует следующей алгоритм. При смене страны если пользователь ранее менял город вылета, выбирается ранее выбранный город вылета,
            // если такой город отсуствует в списке, пробуем выбрать город вылета из Web.config, если и этот город отсустствует в списке выбираем первый город из списка.
            // Если при смене города пользователь ранее не менял город вылета, то проем выбрать город вылета из Web.config, если не получилось то пробуем выбрать первый город из списака.


            // если это изменение фильтров пользователем и пользователь менял город вылета,
            // то запоминаем выбор пользователя, чтобы потом его повторить            
            ListItem selectedItem = null;
            if (isReinit && IsCityFromChangedByUser)
                selectedItem = RblCitiesFromFilter.SelectedItem;

            //грузим города вылета
            RblCitiesFromFilter.DataSource = mtsDc.GetCitiesDeparture(sftDc, SelectedCountryToKey).ToDictionary(k => k.Key, v => HttpUtility.HtmlEncode(v.Value));
            RblCitiesFromFilter.DataBind();

            // если это изменение фильтров пользователем и пользователь менял город вылета,
            // то пытаемся восстановить выбор пользователя
            if (isReinit && IsCityFromChangedByUser && selectedItem != null && RblCitiesFromFilter.Items.FindByValue(selectedItem.Value) != null)
                RblCitiesFromFilter.SelectedValue = selectedItem.Value;

            // если пользователь не менял город вылета, то пробуем выбрать город вылета по значению в Web.config
            if (!IsCityFromChangedByUser &&
                RblCitiesFromFilter.Items.FindByValue(
                    Globals.Settings.TourFilters.DefaultCityFromKey.ToString(CultureInfo.InvariantCulture)) != null)
                RblCitiesFromFilter.SelectedValue =
                    Globals.Settings.TourFilters.DefaultCityFromKey.ToString(CultureInfo.InvariantCulture);

            // если город вылета так и не выбран, но при этом список городов вылета содержит элементы,
            // пробуем еще раз выбрать город вылета из Web.config в случае не удачи, выбираем первый город вылета в списке.
            if (RblCitiesFromFilter.SelectedItem == null && RblCitiesFromFilter.Items.Count > 0)
            {
                if (RblCitiesFromFilter.Items.FindByValue(
                        Globals.Settings.TourFilters.DefaultCityFromKey.ToString(CultureInfo.InvariantCulture)) != null)
                    RblCitiesFromFilter.SelectedValue =
                        Globals.Settings.TourFilters.DefaultCityFromKey.ToString(CultureInfo.InvariantCulture);
                else
                    RblCitiesFromFilter.SelectedIndex = 0;
            }
        }

        private bool SetCitesFromByQs()
        {
            if (_queryStringParametrs.IsEmpty || !_queryStringParametrs.IsParametrsValid || !_queryStringParametrs.CityFromKey.HasValue)
                return false;

            var item = RblCitiesFromFilter.Items.FindByValue(_queryStringParametrs.CityFromKey.ToString());
            if (item == null)
                return false;

            RblCitiesFromFilter.SelectedValue = item.Value;
            return true;
        }

        private void LoadToursTypes(MtSearchDbDataContext mtsDc, SftWebDbDataContext sftDc, bool isReinit = true)
        {

            if (mtsDc == null) throw new ArgumentNullException("mtsDc");

            if (RblCitiesFromFilter.Items.Count == 0 || DdlContriesToFilter.Items.Count == 0)
            {
                ChblTourTypes.Items.Clear();
                return;
            }
            IList<string> selectedValues = new List<string>();
            if (isReinit && !ChblTourTypes.IsAllOptionsSelected)
                selectedValues = ChblTourTypes.SelectedValues;

            ChblTourTypes.DataSource =
                mtsDc.GetTourTypes(sftDc, SelectedCityFromKey, SelectedCountryToKey)
                    .ToDictionary(k => k.Key, v => HttpUtility.HtmlEncode(v.Value));
            ChblTourTypes.DataBind();

            if (isReinit && selectedValues.Any())
                ChblTourTypes.SelectItems(selectedValues);
        }

        private bool SetToursTypesByQs()
        {
            if (_queryStringParametrs.IsEmpty || !_queryStringParametrs.IsParametrsValid)
                return false;
            if (_queryStringParametrs.TourTypeKeys == null)
                return true;

            return ChblTourTypes.SelectItems(_queryStringParametrs.TourTypeKeys.Select(p => p.ToString(CultureInfo.InvariantCulture)).ToList());

        }

        private void LoadCitiesTo(MtSearchDbDataContext mtsDc, SftWebDbDataContext sftDc, bool isReinit = true)
        {
            if (mtsDc == null) throw new ArgumentNullException("mtsDc");

            if (RblCitiesFromFilter.Items.Count == 0 || DdlContriesToFilter.Items.Count == 0 ||
                ChblTourTypes.Items.Count == 0)
            {
                ChblCitiesToFilter.Items.Clear();
                return;
            }
            IList<string> selectedValues = new List<string>();
            if (isReinit && !ChblCitiesToFilter.IsAllOptionsSelected)
                selectedValues = ChblCitiesToFilter.SelectedValues;

            ChblCitiesToFilter.DataSource = mtsDc.GetCitiesTo(sftDc, SelectedCityFromKey, SelectedCountryToKey,
                SelectedTourTypeKeys).ToDictionary(k => k.Key, v => HttpUtility.HtmlEncode(v.Value));
            ChblCitiesToFilter.DataBind();

            if (isReinit && selectedValues.Any())
                ChblCitiesToFilter.SelectItems(selectedValues);
        }

        private bool SetCitiesByQs()
        {
            if (_queryStringParametrs.IsEmpty || !_queryStringParametrs.IsParametrsValid)
                return false;
            if (_queryStringParametrs.CitiesToKeys == null)
                return true;

            return ChblCitiesToFilter.SelectItems(_queryStringParametrs.CitiesToKeys.Select(p => p.ToString(CultureInfo.InvariantCulture)).ToList());
        }

        private void LoadTours(MtSearchDbDataContext mtsDc, SftWebDbDataContext sftDc, bool isReinit = false)
        {
            if (mtsDc == null) throw new ArgumentNullException("mtsDc");

            if (RblCitiesFromFilter.Items.Count == 0 || DdlContriesToFilter.Items.Count == 0 ||
                ChblTourTypes.Items.Count == 0 || ChblCitiesToFilter.Items.Count == 0)
            {
                DdlToursFilter.Items.Clear();
                return;
            }

            ListItem selectedItem = null;
            if (isReinit)
                selectedItem = DdlToursFilter.SelectedItem;

            DdlToursFilter.DataSource = AddFirstCustomElement(-1, "- любой -",
                mtsDc.GetTours(sftDc, SelectedCityFromKey, SelectedCountryToKey, SelectedTourTypeKeys, SelectedCitiesToKeys));
            DdlToursFilter.DataBind();

            if (isReinit && DdlContriesToFilter.Items.FindByValue(selectedItem.Value) != null)
                DdlToursFilter.SelectedValue = selectedItem.Value;
        }

        private bool SetToursByQs()
        {
            if (_queryStringParametrs.IsEmpty || !_queryStringParametrs.IsParametrsValid)
                return false;
            if (_queryStringParametrs.TourKeys == null)
                return true;

            var item = DdlToursFilter.Items.FindByValue(_queryStringParametrs.TourKeys.First().ToString(CultureInfo.InvariantCulture));
            if (item == null)
                return false;

            DdlToursFilter.SelectedValue = item.Value;
            return true;
        }

        private void LoadArrivalDates(MtSearchDbDataContext mtsDc, SftWebDbDataContext sftDc, bool isReinit = false)
        {
            if (mtsDc == null) throw new ArgumentNullException("mtsDc");

            if (RblCitiesFromFilter.Items.Count == 0 || DdlContriesToFilter.Items.Count == 0 ||
                ChblTourTypes.Items.Count == 0 || ChblCitiesToFilter.Items.Count == 0
                || DdlToursFilter.Items.Count == 0)
            {
                ArrivalDatesFilter.ClearArrivalDays();
                return;
            }

            DateTime? dateFrom = null;
            DateTime? dateTo = null;
            if (isReinit && ArrivalDatesFilter.IsArrivalDateChangedByUser)
            {
                dateFrom = ArrivalDatesFilter.SelectedArrivalDateFrom;
                dateTo = ArrivalDatesFilter.SelectedArrivalDateTo;
            }

            // если это первая загрузка страницы подтягиваем инициализацию фильтра из Web.config
            if (!Page.IsPostBack)
            {
                ArrivalDatesFilter.MaxArrivalRange = Globals.Settings.TourFilters.MaxArrivalRange;
                ArrivalDatesFilter.DefaultArrivalRange = Globals.Settings.TourFilters.DefaultArrivalRange;
            }

            ArrivalDatesFilter.LoadArrivalDates(mtsDc.GetTourDates(sftDc, SelectedCityFromKey, SelectedCountryToKey, SelectedCitiesToKeys, SelectedTourKeys));

            if (isReinit && ArrivalDatesFilter.IsArrivalDateChangedByUser && dateFrom.HasValue && dateTo.HasValue && ArrivalDatesFilter.ArrivalDates.Contains(dateFrom.Value) && ArrivalDatesFilter.ArrivalDates.Contains(dateTo.Value))
            {
                ArrivalDatesFilter.SelectArrivalRange(dateFrom.Value, dateTo.Value);
            }
        }

        private bool SetArrivalDatesByQs()
        {
            if (_queryStringParametrs.IsEmpty || !_queryStringParametrs.IsParametrsValid)
                return false;
            if (!_queryStringParametrs.ArrivalDateFrom.HasValue && !_queryStringParametrs.ArrivalDateTo.HasValue)
                return true;
            if (!_queryStringParametrs.ArrivalDateFrom.HasValue || !_queryStringParametrs.ArrivalDateTo.HasValue)
                return false;
            if (_queryStringParametrs.ArrivalDateTo < _queryStringParametrs.ArrivalDateFrom)
                return false;
            if (_queryStringParametrs.ArrivalDateTo < DateTime.Now.Date)
                return false;
            if (!ArrivalDatesFilter.ArrivalDates.Contains(_queryStringParametrs.ArrivalDateFrom.Value) || !ArrivalDatesFilter.ArrivalDates.Contains(_queryStringParametrs.ArrivalDateTo.Value))
                return false;

            var dateFrom = _queryStringParametrs.ArrivalDateFrom < DateTime.Now.Date ?
                (from arrivalDate in ArrivalDatesFilter.ArrivalDates
                 where arrivalDate >= DateTime.Now.Date
                 orderby arrivalDate
                 select arrivalDate).First()
                : _queryStringParametrs.ArrivalDateFrom.Value;
            var dateTo = _queryStringParametrs.ArrivalDateTo.Value;
            ArrivalDatesFilter.SelectArrivalRange(dateFrom, dateTo);
            return true;
        }

        private void LoadNights(MtSearchDbDataContext mtsDc, SftWebDbDataContext sftDc, bool isReinit = false)
        {
            if (RblCitiesFromFilter.Items.Count == 0 || DdlContriesToFilter.Items.Count == 0 ||
                ChblTourTypes.Items.Count == 0 || ChblCitiesToFilter.Items.Count == 0
                || DdlToursFilter.Items.Count == 0 || ArrivalDatesFilter.ArrivalDates == null ||
                !ArrivalDatesFilter.ArrivalDates.Any())
            {
                ChblNightsFilter.Items.Clear();
                return;
            }

            IList<string> selectedValues = new List<string>();
            if (isReinit && !ChblNightsFilter.IsAllOptionsSelected)
                selectedValues = ChblNightsFilter.SelectedValues;

            ChblNightsFilter.DataSource = mtsDc.GetTourNights(sftDc, SelectedCityFromKey, SelectedCountryToKey, SelectedCitiesToKeys, SelectedTourKeys, SelectedArrivalDates);
            ChblNightsFilter.DataBind();

            if (isReinit && selectedValues.Any())
                ChblNightsFilter.SelectItems(selectedValues);
        }

        private bool SetNightsByQs()
        {
            if (_queryStringParametrs.IsEmpty || !_queryStringParametrs.IsParametrsValid)
                return false;
            if (_queryStringParametrs.Nights == null)
                return true;

            return ChblNightsFilter.SelectItems(_queryStringParametrs.Nights.Select(p => p.ToString(CultureInfo.InvariantCulture)).ToList());
        }

        private void LoadHotelCategories(MtSearchDbDataContext mtsDc, SftWebDbDataContext sftDc, bool isReinit = false)
        {
            if (RblCitiesFromFilter.Items.Count == 0 || DdlContriesToFilter.Items.Count == 0 ||
                ChblTourTypes.Items.Count == 0 || ChblCitiesToFilter.Items.Count == 0
                || DdlToursFilter.Items.Count == 0 || ArrivalDatesFilter.ArrivalDates == null ||
                !ArrivalDatesFilter.ArrivalDates.Any()
                || ChblNightsFilter.Items.Count == 0)
            {
                ChblHotelCategoriesFilter.Items.Clear();
                return;
            }

            IList<string> selectedValues = new List<string>();
            if (isReinit && !ChblHotelCategoriesFilter.IsAllOptionsSelected)
                selectedValues = ChblHotelCategoriesFilter.SelectedValues;

            ChblHotelCategoriesFilter.DataSource = mtsDc.GetTourHotelClasses(sftDc, SelectedCityFromKey, SelectedCountryToKey,
                SelectedCitiesToKeys, SelectedTourKeys,
                SelectedArrivalDates, SelectedNights).Select(HttpUtility.HtmlEncode);
            ChblHotelCategoriesFilter.DataBind();

            if (isReinit && selectedValues.Any())
                ChblHotelCategoriesFilter.SelectItems(selectedValues);
        }

        private bool SetHotelCategoriesByQs()
        {
            if (_queryStringParametrs.IsEmpty || !_queryStringParametrs.IsParametrsValid)
                return false;
            if (_queryStringParametrs.HotelCategoriesKeys == null)
                return true;

            return ChblHotelCategoriesFilter.SelectItems(_queryStringParametrs.HotelCategoriesKeys.Select(p => p.ToString(CultureInfo.InvariantCulture)).ToList());
        }

        private void LoadPansions(MtSearchDbDataContext mtsDc, SftWebDbDataContext sftDc, bool isReinit = false)
        {
            if (RblCitiesFromFilter.Items.Count == 0 || DdlContriesToFilter.Items.Count == 0 ||
                ChblTourTypes.Items.Count == 0 || ChblCitiesToFilter.Items.Count == 0
                || DdlToursFilter.Items.Count == 0 || ArrivalDatesFilter.ArrivalDates == null ||
                !ArrivalDatesFilter.ArrivalDates.Any()
                || ChblNightsFilter.Items.Count == 0 || ChblHotelCategoriesFilter.Items.Count == 0)
            {
                ChblPansionsFilter.Items.Clear();
                return;
            }

            IList<string> selectedValues = new List<string>();
            if (isReinit && !ChblPansionsFilter.IsAllOptionsSelected)
                selectedValues = ChblPansionsFilter.SelectedValues;

            ChblPansionsFilter.DataSource = mtsDc.GetTourPansions(sftDc, SelectedCityFromKey, SelectedCountryToKey,
                SelectedCitiesToKeys, SelectedTourKeys, SelectedArrivalDates, SelectedNights,
                SelectedHotelCategoriesKeys).ToDictionary(k => k.Key, v => HttpUtility.HtmlEncode(v.Value));
            ChblPansionsFilter.DataBind();

            if (isReinit && selectedValues.Any())
                ChblPansionsFilter.SelectItems(selectedValues);
        }

        private bool SetPansionsByQs()
        {
            if (_queryStringParametrs.IsEmpty || !_queryStringParametrs.IsParametrsValid)
                return false;
            if (_queryStringParametrs.PansionKeys == null)
                return true;

            return ChblPansionsFilter.SelectItems(_queryStringParametrs.PansionKeys.Select(p => p.ToString(CultureInfo.InvariantCulture)).ToList());
        }

        private void LoadHotels(MtSearchDbDataContext mtsDc, SftWebDbDataContext sftDc, bool isReinit = false)
        {
            if (RblCitiesFromFilter.Items.Count == 0 || DdlContriesToFilter.Items.Count == 0 ||
                ChblTourTypes.Items.Count == 0 || ChblCitiesToFilter.Items.Count == 0
                || DdlToursFilter.Items.Count == 0 || ArrivalDatesFilter.ArrivalDates == null ||
                !ArrivalDatesFilter.ArrivalDates.Any()
                || ChblNightsFilter.Items.Count == 0 || ChblHotelCategoriesFilter.Items.Count == 0
                || ChblPansionsFilter.Items.Count == 0)
            {
                UcHotelsFilter.Items.Clear();
                return;
            }

            IList<string> selectedValues = new List<string>();
            if (isReinit && !UcHotelsFilter.IsAllOptionsSelected)
                selectedValues = UcHotelsFilter.SelectedValues;

            var hotels =
                mtsDc.GetTourHotels(sftDc, SelectedCityFromKey, SelectedCountryToKey, SelectedCitiesToKeys, SelectedTourKeys,
                    SelectedArrivalDates,
                    SelectedNights, SelectedHotelCategoriesKeys, SelectedPansionKeys,
                    !UcHotelsFilter.IsFilterByArrNights).ToList();

            UcHotelsFilter.DataSource
                = hotels.ToDictionary(h => h.Key, h => GetHotelNameHtml(h.Name, h.Stars, h.ResortName, h.Url));
            UcHotelsFilter.DataBind();

            if (!UcHotelsFilter.IsFilterByArrNights)
            {
                foreach (var hotel in hotels.Where(h => !h.IsValidPrice))
                    UcHotelsFilter.Items.FindByValue(hotel.Key.ToString(CultureInfo.InvariantCulture)).Enabled = false;
            }

            if (isReinit && selectedValues.Any())
                UcHotelsFilter.SelectItems(selectedValues);
        }

        private bool SetHotelsByQs()
        {
            if (_queryStringParametrs.IsEmpty || !_queryStringParametrs.IsParametrsValid)
                return false;
            if (_queryStringParametrs.HotelKeys == null)
                return true;

            return UcHotelsFilter.SelectItems(_queryStringParametrs.HotelKeys.Select(p => p.ToString(CultureInfo.InvariantCulture)).ToList());
        }

        private void LoadRoomTypesAndMens(MtMainDbDataContext mtmDc, MtSearchDbDataContext mtsDc, SftWebDbDataContext sftDc, bool isReinit)
        {
            if (RblCitiesFromFilter.Items.Count == 0 || DdlContriesToFilter.Items.Count == 0 ||
                ChblTourTypes.Items.Count == 0 || ChblCitiesToFilter.Items.Count == 0
                || DdlToursFilter.Items.Count == 0 || ArrivalDatesFilter.ArrivalDates == null ||
                !ArrivalDatesFilter.ArrivalDates.Any()
                || ChblNightsFilter.Items.Count == 0 || ChblHotelCategoriesFilter.Items.Count == 0 ||
                UcHotelsFilter.Items.Count == 0 || ChblPansionsFilter.Items.Count == 0)
            {
                DdlRoomTypesFilter.Items.Clear();
                return;
            }


            // выбираем вариант поиска по номеру или по людям в зависимости от большинства туров с ценой за номер или за человека
            PnPersons.Visible = !(PnRoomTypes.Visible = mtsDc.IsCountryPriceForMen(SelectedCountryToKey));

            if (PnRoomTypes.Visible)
            {
                DdlRoomTypesFilter.DataSource = AddFirstCustomElement(-1, "- все -",
                    mtsDc.GetTourRooms(sftDc, SelectedCityFromKey, SelectedCountryToKey, SelectedCitiesToKeys, SelectedTourKeys, SelectedArrivalDates, SelectedNights,
                        SelectedHotelKeys, SelectedPansionKeys).ToDictionary(k => k.Key, v => HttpUtility.HtmlEncode(v.Value)));
                DdlRoomTypesFilter.DataBind();
            }
            else
                DdlRoomTypesFilter.Items.Clear();
        }

        private bool SetRoomTypesAndMensByQs()
        {
            if (_queryStringParametrs.IsEmpty || !_queryStringParametrs.IsParametrsValid)
                return false;

            if (_queryStringParametrs.RoomTypeKeys == null && !_queryStringParametrs.AdultsNumber.HasValue &&
                !_queryStringParametrs.ChildsNumber.HasValue && !_queryStringParametrs.FirstChildAge.HasValue &&
                !_queryStringParametrs.SecondChildAge.HasValue)
                return true;

            if (_queryStringParametrs.RoomTypeKeys != null && (_queryStringParametrs.AdultsNumber.HasValue ||
                                                              _queryStringParametrs.ChildsNumber.HasValue ||
                                                              _queryStringParametrs.FirstChildAge.HasValue ||
                                                              _queryStringParametrs.SecondChildAge.HasValue))
                return false;

            if (IsPriceForRoom && _queryStringParametrs.AdultsNumber.HasValue)
            {
                TbAdultsFilter.Text = _queryStringParametrs.AdultsNumber.ToString();
                if (_queryStringParametrs.ChildsNumber > 0)
                {
                    if (!_queryStringParametrs.FirstChildAge.HasValue) return false;

                    TbChildsFilter.Text = _queryStringParametrs.ChildsNumber.ToString();
                    TbFirstChildAge.Text = _queryStringParametrs.FirstChildAge.ToString();
                    if (_queryStringParametrs.ChildsNumber > 1)
                    {
                        if (!_queryStringParametrs.SecondChildAge.HasValue) return false;

                        TbSecondChildAge.Text = _queryStringParametrs.SecondChildAge.ToString();
                    }
                    else
                    {
                        TbSecondChildAge.Text = String.Empty;
                    }
                }
                else
                {
                    TbChildsFilter.Text = "0";
                    TbFirstChildAge.Text = String.Empty;
                }

                return true;
            }

            if (IsPriceForRoom || _queryStringParametrs.RoomTypeKeys == null) return false;
            var item = DdlRoomTypesFilter.Items.FindByValue(_queryStringParametrs.RoomTypeKeys.ToString());
            if (item == null)
                return false;

            DdlRoomTypesFilter.SelectedValue = item.Value;
            return true;
        }

        private void LoadRates(MtSearchDbDataContext dc, bool isReinit = false)
        {
            ListItem selectedItem = null;
            if (isReinit)
                selectedItem = RblRates.SelectedItem;

            RblRates.DataSource = dc.GetCurrencies().OrderByDescending(r => r.Value);
            RblRates.DataBind();

            if (RblRates.Items.Count == 0)
                return;

            if (isReinit && RblRates.Items.FindByValue(selectedItem.Value) != null)
                RblRates.SelectedValue = selectedItem.Value;
            else
            {
                if (Page.IsPostBack)
                    return;

                var listItem = RblRates.Items.FindByText(
                    Globals.Settings.TourFilters.DefaultRateCode);
                if (listItem != null)
                    listItem.Selected = true;
                else
                    RblCitiesFromFilter.Items[0].Selected = true;
            }
        }

        private bool SetRatesByQs()
        {
            if (_queryStringParametrs.IsEmpty || !_queryStringParametrs.IsParametrsValid)
                return false;

            if (!_queryStringParametrs.RateKey.HasValue)
                return true;

            var item = RblRates.Items.FindByValue(_queryStringParametrs.RateKey.ToString());
            if (item == null)
                return false;

            RblRates.SelectedValue = item.Value;
            return true;
        }

        private bool SetMaxPriceByQs()
        {
            if (_queryStringParametrs.IsEmpty || !_queryStringParametrs.IsParametrsValid)
                return false;

            if (!_queryStringParametrs.MaxPrice.HasValue)
                return true;

            TbMaxPrice.Text = _queryStringParametrs.MaxPrice.ToString();
            return true;
        }

        private bool SetAviaQuotesByQs()
        {
            if (_queryStringParametrs.IsEmpty || !_queryStringParametrs.IsParametrsValid)
                return false;

            if (!_queryStringParametrs.AviaQuotesStates.HasValue)
                return true;

            ChAviaAvailiable.Checked = _queryStringParametrs.AviaQuotesStates.Value.HasFlag(QuotesStates.Availiable);
            ChAviaNo.Checked = _queryStringParametrs.AviaQuotesStates.Value.HasFlag(QuotesStates.No);
            ChAviaRQ.Checked = _queryStringParametrs.AviaQuotesStates.Value.HasFlag(QuotesStates.Request);
            return true;
        }

        private bool SetRoomQuotesByQs()
        {
            if (_queryStringParametrs.IsEmpty || !_queryStringParametrs.IsParametrsValid)
                return false;

            if (!_queryStringParametrs.RoomsQuotesStates.HasValue)
                return true;

            ChRoomsEnabled.Checked = _queryStringParametrs.RoomsQuotesStates.Value.HasFlag(QuotesStates.Availiable);
            ChRoomsNo.Checked = _queryStringParametrs.RoomsQuotesStates.Value.HasFlag(QuotesStates.No);
            ChRoomsRQ.Checked = _queryStringParametrs.RoomsQuotesStates.Value.HasFlag(QuotesStates.Request);
            return true;
        }

        private bool SetRowsNumberByQs()
        {
            //todo: переделать контрол как у слетать в строчку.
            if (_queryStringParametrs.IsEmpty || !_queryStringParametrs.IsParametrsValid)
                return false;
            if (!_queryStringParametrs.RowsNumber.HasValue)
                return true;

            var item = DdlRowsNumber.Items.FindByValue(_queryStringParametrs.RowsNumber.ToString());
            if (item == null)
                return false;

            DdlRowsNumber.SelectedValue = item.Value;
            return true;
        }

        #endregion

        #region event handlers...

        protected void Filter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender == null) throw new ArgumentNullException("sender");

            if (sender == RblCitiesFromFilter)
                IsCityFromChangedByUser = true;

            ReInitChildFilters(sender as Control);
            InitQueryString();
            OnFilterSelectionChanged(EventArgs.Empty);
        }

        protected void UcHotelsFilter_OnFilterByArrNightsChanged(object sender, EventArgs e)
        {
            if (sender == null) throw new ArgumentNullException("sender");

            ReInitChildFilters(sender as Control, false);
            InitQueryString();
            OnFilterSelectionChanged(EventArgs.Empty);
        }

        protected void BtnSearch_Click(object sender, EventArgs e)
        {
            OnSearchBottonClick(EventArgs.Empty);
        }

        #endregion
    }
}