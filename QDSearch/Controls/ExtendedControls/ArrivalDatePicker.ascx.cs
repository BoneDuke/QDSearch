using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using Telerik.Web.UI;
using Telerik.Web.UI.Calendar;

namespace Controls.ExtendedControls
{
    public partial class ArrivalDatePicker : UserControl
    {
        public const uint DefaultMaxArrivalRangeInDays = 31;
        public const uint DefaultArrivalRangeInDays = 14;
        public const uint DefaultPageSize = 20;

        public string Title
        {
            get { return LbFilterTitle.Text; }
            set { LbFilterTitle.Text = value; }
        }

        /// <summary>
        /// true - если дата заезда менялось пользователем из веб-формы, 
        /// false - если нет
        /// </summary>
        public bool IsArrivalDateChangedByUser
        {
            get { return (bool)(ViewState["IsArrivalDateChangedByUser"] ?? false); }
            set { ViewState["IsArrivalDateChangedByUser"] = value; }
        }

        /// <summary>
        /// Период по умолчанию в днях между датами "с" и "по". Используется для определения даты "по" при изменении даты "c",
        /// а также при определении даты "по" в при первичной инициализации дат заездов. Не может быть больше MaxArrivalRange.
        /// В случае привышения MaxArrivalRange, будет использоваться MaxArrivalRange вместо DefaultArrivalRange.
        /// </summary>
        public uint DefaultArrivalRange
        {
            get
            {
                var arrivalRange = ViewState["DefaultArrivalRange"] is uint
                    ? (uint)ViewState["DefaultArrivalRange"]
                    : DefaultArrivalRangeInDays;
                return arrivalRange > MaxArrivalRange ? MaxArrivalRange : arrivalRange;
            }
            set { ViewState["DefaultArrivalRange"] = value; }
        }

        /// <summary>
        /// Дата "до" по умолчанию. Рассчитывается на основе DefaultDateFrom плюс DefaultArrivalRange.
        /// В случае если DefaultArrivalRange больше MaxArrivalRange, используется MaxArrivalRange.
        /// </summary>
        //public DateTime DefaultDateTo
        //{
        //    get { return DefaultArrivalRange > MaxArrivalRange ? DefaultDateFrom.AddDays(MaxArrivalRange) : DefaultDateFrom.AddDays(DefaultArrivalRange); }
        //}

        /// <summary>
        /// Максимальный период в днях между датами "с" и "по". Используется для ограничения даты "по" при ее измении,
        /// а так же вместо DefaultArrivalRange в случае если он больше MaxArrivalRange.
        /// </summary>
        public uint MaxArrivalRange
        {
            get
            {
                return ViewState["MaxArrivalRange"] is uint
                           ? (uint)ViewState["MaxArrivalRange"]
                           : DefaultMaxArrivalRangeInDays;
            }
            set { ViewState["MaxArrivalRange"] = value; }
        }

        /// <summary>
        /// Не изменяемый список дат заездов которые загружены в календарь.
        /// ReadOnly = true;
        /// </summary>
        [ReadOnly(true)]
        public List<DateTime> ArrivalDates
        {
            get
            {
                return
                    clShared.SpecialDays.Cast<RadCalendarDay>()
                        .Where(d => d.IsSelectable && d.Date >= DateTime.Today)
                        .Select(d => d.Date)
                        .OrderBy(date => date).ToList();
            }
        }

        /// <summary>
        /// Первая доступная для выбора дата заезда.
        /// Всегда больше либо равно текущей дате.
        /// </summary>
        public DateTime? FirstArrivalDate
        {
            get { return (ArrivalDates != null && ArrivalDates.Any()) ? ArrivalDates.Min() : (DateTime?) null; }
        }

        /// <summary>
        /// Последняя доступная для выбора дата заезда
        /// </summary>
        public DateTime? LastArrivalDate
        {
            get { return (ArrivalDates != null && ArrivalDates.Any()) ? ArrivalDates.Max() : (DateTime?)null; }
        }

        /// <summary>
        /// Первая дата заезда из выбранного диапозона дат заездов
        /// </summary>
        public DateTime? SelectedArrivalDateFrom
        {
            get { return clDatesFrom.SelectedDate; }
            private set { clDatesFrom.SelectedDate = value; }
        }
        /// <summary>
        /// Последняя дата заезда из выбранного диапозона дат заездов
        /// </summary>
        public DateTime? SelectedArrivalDateTo
        {
            get { return clDatesTo.SelectedDate; }
            private set { clDatesTo.SelectedDate = value; }
        }

        /// <summary>
        /// Список выбранных дат заездов
        /// </summary>
        public IList<DateTime> SeletedArrivaleDays
        {
            get { return ArrivalDates.Where(d => d >= SelectedArrivalDateFrom && d <= SelectedArrivalDateTo).ToList(); }
        }

        /// <summary>
        /// Событие отражающее, что произошло изменение выбранного диапозона дат заездов.
        /// </summary>
        public event EventHandler SelectedArrivalRangeChanged;

        protected void OnSelectedArrivalRangeChanged(EventArgs e)
        {
            EventHandler handler = SelectedArrivalRangeChanged;
            if (handler != null) handler(this, e);
        }

        protected void Page_Init(object sender, EventArgs e)
        {
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Очищает загруженные даты заездов.
        /// </summary>
        public void ClearArrivalDays()
        {
            clShared.SpecialDays.Clear();
            ClearSelectedArrivalRange();
            UpdateSelectionMaxRange();
            UpdateAvailableRange();
        }

        /// <summary>
        /// Сбрасывет выбранный период заездов
        /// </summary>
        protected void ClearSelectedArrivalRange()
        {
            clDatesFrom.SelectedDate = null;
            clDatesTo.SelectedDate = null;
        }

        /// <summary>
        /// Добавляет даты заездов в календарь. При этом происходит обновление доступного к выбору диапозона дат заездов.
        /// </summary>
        /// <param name="arrivalDates">добовляемые даты заездов</param>
        public void AddArrivalDates(IList<DateTime> arrivalDates)
        {
            if (arrivalDates == null) throw new ArgumentNullException("arrivalDates");

            // получаем сортированный массив дат заездов,
            // это нужно дальнешего заполнения отсутсвующих дней между датами заездов
            var arrivalsSorted = arrivalDates.Where(d => d.Date >= DateTime.Today).OrderBy(d => d).ToList();
            var arrivalsNmb = arrivalsSorted.Count;

            for (var i = 0; i < arrivalsNmb; i++)
            {
                //if(arrivalsNmb == 0)
                //    break;

                // добовляем очердную дату заезда в календарь
                var clDay = new RadCalendarDay(clShared) { Date = arrivalsSorted[i], IsSelectable = true };
                clShared.SpecialDays.Add(clDay);

                // проверяем есть ли следующая дата заезда
                if ((i + 1) >= arrivalsNmb)
                    break;

                // преверяем есть ли между текущей датой заезда и следующей разрыв в днях без вылета
                // если есть будем заполнять днями без перелетов между ними
                //if (clDay.Date.AddDays(1) == arrivalsSorted[i+1])
                //    break;

                for (var noArrivalDay = arrivalsSorted[i].AddDays(1); noArrivalDay < arrivalsSorted[i + 1]; noArrivalDay = noArrivalDay.AddDays(1))
                {
                    var clNoArrivDay = new RadCalendarDay(clShared)
                    {
                        Date = noArrivalDay,
                        IsSelectable = false,
                        IsDisabled = true
                    };
                    clShared.SpecialDays.Add(clNoArrivDay);
                }
            }
            UpdateSelectionMaxRange();
            UpdateAvailableRange();
        }

        /// <summary>
        /// Загружает указанные даты заездов в календарь. Старые даты ощищаюется, 
        /// обновляется допустимый период дат заезодов и выбранные даты заездов с учетом даты от и максимального периода по умолчанию.
        /// </summary>
        /// <param name="arrivalDates">даты заездов</param>
        public void LoadArrivalDates(IList<DateTime> arrivalDates)
        {
            ClearArrivalDays();
            AddArrivalDates(arrivalDates);
            SelectArrivalRange();
        }

        /// <summary>
        /// Задает период дат заездов по умолчанию из загруженных в календарь дат заездов.
        /// Дата С - первая дата заезда от текущего числа включительно.
        /// Дата ПО - дата С плюс DefaultArrivalRange
        /// </summary>
        /// <returns>true - даты заездов подобраны успешно, false - не удалось найти даты заездов отвечающим условиям подбора</returns>
        public bool SelectArrivalRange()
        {
            if (ArrivalDates != null && ArrivalDates.Any() && FirstArrivalDate.HasValue)
                return SelectArrivalRange(FirstArrivalDate.Value, FirstArrivalDate.Value.AddDays(DefaultArrivalRange));

            ClearSelectedArrivalRange();
            return false;
        }

        /// <summary>
        /// Задает период дат заездов из загруженных в календарь дат заездов. 
        /// Если дата dateFrom отстуствует среди загруженных дат заездов в каледарь, то выбирается ближайшая большая.
        /// Если дата dateTo отстуствует среди загруженных дат заездов в каледарь, то выбирается ближайшая меньшая.
        /// Если разница между датами С и ПО больше MaxArrivalRange, то в этом случае дата ПО автоматически обрезатеся до дата С плюс MaxArrivalRange.
        /// Если в заданном диапозоне нет дат заездов, то происходит сброс ранее выбранных дат и новые даты заездов не выбираеются.
        /// </summary>
        /// <param name="dateFrom">Дата начала выбираемого диапозона дат заездов включительно.</param>
        /// <param name="dateTo">Дата конца выбираемого диапозона дат заездов включительно.</param>
        /// <returns>true - даты заездов подобраны успешно, false - в указанном диапозен нет дат заездов</returns>
        public bool SelectArrivalRange(DateTime dateFrom, DateTime dateTo)
        {
            if (dateFrom < DateTime.Today) throw new ArgumentOutOfRangeException("dateFrom", dateFrom, "Дата ОТ не может быть меньше текущей даты");
            if (dateTo < DateTime.Today) throw new ArgumentOutOfRangeException("dateTo", dateTo, "Дата ДО не может быть меньше текущей даты");
            if (dateFrom > dateTo) throw new ArgumentOutOfRangeException("dateFrom", dateFrom, "Дата ОТ не может быть больше даты ДО.");
            if ((dateTo - dateFrom).Days > MaxArrivalRange)
                dateTo = dateFrom.AddDays(MaxArrivalRange);

            if (ArrivalDates == null || !ArrivalDates.Any())
            {
                ClearSelectedArrivalRange();
                return false;
            }

            var selectedDateFrom = ArrivalDates.Cast<DateTime?>().OrderBy(d => d).FirstOrDefault(d => d >= dateFrom.Date);
            if (!selectedDateFrom.HasValue)
            {
                ClearSelectedArrivalRange();
                return false;
            }
            var selectedDateTo = ArrivalDates.Cast<DateTime?>().OrderBy(d => d).LastOrDefault(d => d <= dateTo.Date && d >= selectedDateFrom);
            if (!selectedDateTo.HasValue)
            {
                ClearSelectedArrivalRange();
                return false;
            }
            clDatesFrom.SelectedDate = selectedDateFrom;
            clDatesTo.SelectedDate = selectedDateTo;
            return true;
        }

        /// <summary>
        /// Обновляет доступный для выбора диапозон дат в календаре.
        /// </summary>
        protected void UpdateSelectionMaxRange()
        {
            clShared.RangeMinDate = FirstArrivalDate ?? DateTime.Today;
            clShared.RangeMaxDate = LastArrivalDate ?? DateTime.Today;
        }

        /// <summary>
        /// Обновляет доступный для выбора диапозон дат в интерефейсе для пользователя.
        /// </summary>
        protected void UpdateAvailableRange()
        {
            pnArrivalRangeInfo.Visible = ArrivalDates != null && ArrivalDates.Any();
            if (pnArrivalRangeInfo.Visible)
                ltAvlArrivalDates.Text = String.Format(@"<span>{0:dd.MM.yyyy}&nbsp;&ndash;&nbsp;{1:dd.MM.yyyy}</span>",
                                                       FirstArrivalDate, LastArrivalDate);
        }

        protected void clDatesFrom_SelectedDateChanged(object sender, SelectedDateChangedEventArgs e)
        {
            if (!e.NewDate.HasValue)
            {
                SelectedArrivalDateFrom = e.OldDate ?? FirstArrivalDate;
                clDatesFrom.Focus();
                return;
            }

            if (!ArrivalDates.Any(d => d.Date == e.NewDate))
            {
                SelectedArrivalDateFrom = e.OldDate;
                clDatesFrom.Focus();
                QDSearch.Helpers.Web.ShowMessage(sender as Control, "В эту дату нету вылетов выберете другую дату");
                return;
            }

            if (!SelectedArrivalDateTo.HasValue || e.NewDate.Value > SelectedArrivalDateTo.Value)
                SelectedArrivalDateTo = e.NewDate;

            if ((SelectedArrivalDateTo.Value - e.NewDate.Value).Days > MaxArrivalRange)
            {
                SelectedArrivalDateTo =
                    ArrivalDates.Cast<DateTime?>()
                        .OrderBy(d => d)
                        .LastOrDefault(d => d <= e.NewDate.Value.AddDays(MaxArrivalRange) && d >= e.NewDate.Value);
                QDSearch.Helpers.Web.ShowMessage(sender as Control, String.Format(@"Выбран период заездов больше {0}. Дата ""ПО"" изменена на {1:dd.MM.yyyy}", MaxArrivalRange, SelectedArrivalDateTo));
            }
            IsArrivalDateChangedByUser = true;
            OnSelectedArrivalRangeChanged(new EventArgs());
        }

        protected void clDatesTo_SelectedDateChanged(object sender, SelectedDateChangedEventArgs e)
        {
            if (!e.NewDate.HasValue)
            {
                SelectedArrivalDateTo = e.OldDate ?? SelectedArrivalDateFrom ?? FirstArrivalDate;
                clDatesTo.Focus();
                return;
            }

            if (!ArrivalDates.Any(p => p.Date == e.NewDate))
            {
                SelectedArrivalDateTo = e.OldDate;
                clDatesTo.Focus();
                QDSearch.Helpers.Web.ShowMessage(sender as Control, "В эту дату нету вылетов выберете другую дату");
                return;
            }

            if (!SelectedArrivalDateFrom.HasValue || e.NewDate.Value < SelectedArrivalDateFrom.Value)
                SelectedArrivalDateFrom = e.NewDate;

            if ((e.NewDate.Value - SelectedArrivalDateFrom.Value).Days > MaxArrivalRange)
            {
                SelectedArrivalDateFrom = ArrivalDates.Cast<DateTime?>().OrderBy(d => d).FirstOrDefault(d => d >= e.NewDate.Value.AddDays(-MaxArrivalRange) && d <= e.NewDate);
                QDSearch.Helpers.Web.ShowMessage(sender as Control, String.Format(@"Выбран период заездов больше {0}. Дата ""C"" изменена на {1:dd.MM.yyyy}", MaxArrivalRange, SelectedArrivalDateFrom));
            }

            IsArrivalDateChangedByUser = true;
            OnSelectedArrivalRangeChanged(new EventArgs());
        }
    }
}