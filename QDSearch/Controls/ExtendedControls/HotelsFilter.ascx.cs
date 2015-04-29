using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using QDSearch;
using QDSearch.Helpers;

namespace Controls.ExtendedControls
{
    public partial class HotelsFilter : UserControl
    {
        private QueryStringParametrs _queryStringParametrs;

        private static readonly object EventCheckedChanged = new object();
        private static readonly object EventFilterByArrNightsChanged = new object();

        /// <summary>
        /// Отображаемое название списка
        /// </summary>
        public string Title
        {
            get { return ControlTitle.Text; }
            set { ControlTitle.Text = value; }
        }

        /// <summary>
        /// Отображаемое название чебокса ALL
        /// </summary>
        public string AllOptionTitle
        {
            get { return ChbAllOptions.Text; }
            set { ChbAllOptions.Text = value; }
        }

        /// <summary>
        /// Gets or sets the field of the data source that provides the value of each list item.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:System.String"/> that specifies the field of the data source that provides the value of each list item. The default is <see cref="F:System.String.Empty"/>.
        /// </returns>
        [DefaultValue("")]
        public virtual string DataValueField
        {
            get
            {
                return ChblOptions.DataValueField;
            }
            set
            {
                ChblOptions.DataValueField = value;
            }
        }

        /// <summary>
        /// The data source to bind to. This allows a BaseDataBoundControl to bind 
        /// to arbitrary lists of data items.
        /// </summary> 
        public virtual string DataTextField
        {
            get
            {
                return ChblOptions.DataTextField;
            }
            set
            {
                ChblOptions.DataTextField = value;
            }
        }

        public virtual string DataTextFormatString
        {
            get
            {
                return ChblOptions.DataTextFormatString;
            }
            set
            {
                ChblOptions.DataTextFormatString = value;
            }
        }


        public object DataSource
        {
            get { return ChblOptions.DataSource; }
            set { ChblOptions.DataSource = value; }
        }

        /// <devdoc>
        ///    <para> 
        ///       Indicates the collection of items within the list.
        ///       This property
        ///       is read-only.</para>
        /// </devdoc> 

        public ListItemCollection Items
        {
            get
            {
                return ChblOptions.Items;
            }
        }
        /// <summary>
        /// Возвращает сотояние выбора отлей. В случае true выбраны все отели, в случае false выбрано либо часть отелей либо вообще ничего не выбрано.
        /// </summary>
        public bool IsAllOptionsSelected
        {
            get { return ChbAllOptions.Checked; }
        }

        /// <summary>
        /// выводить список отелей отобранных в том числе по продолжительности в ночах и выбранным датам заездов
        /// </summary>
        public bool IsFilterByArrNights
        {
            get { return ChbFilterByArrNights.Checked; }
        }

        /// <summary>
        /// выбранные коды отелей
        /// </summary>
        public IList<string> SelectedValues
        {
            get
            {
                return (from ListItem item in ChblOptions.Items
                        where item.Enabled && (ChbAllOptions.Checked || item.Selected)
                        select item.Value).ToList();

            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the control displays vertically or horizontally.
        /// </summary>
        /// <returns>
        /// One of the <see cref="T:System.Web.UI.WebControls.RepeatDirection"/> values. The default is Vertical.
        /// </returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The specified display direction of the list is not one of the <see cref="T:System.Web.UI.WebControls.RepeatDirection"/> values. </exception>
        public RepeatDirection RepeatDirection
        {
            get { return ChblOptions.RepeatDirection; }
            set { ChblOptions.RepeatDirection = value; }
        }

        /// <devdoc> 
        ///    <para>Gets or sets a value that indicates whether the control is displayed in
        ///    <see langword='Table '/>or <see langword='Flow '/>layout.</para> 
        /// </devdoc>
        public RepeatLayout RepeatLayout
        {
            get { return ChblOptions.RepeatLayout; }
            set { ChblOptions.RepeatLayout = value; }
        }

        /// <devdoc> 
        ///    Occurs when the list selection is changed upon server
        ///    postback. 
        /// </devdoc>
        public event EventHandler CheckedChanged
        {
            add
            {
                Events.AddHandler(EventCheckedChanged, value);
            }
            remove
            {
                Events.RemoveHandler(EventCheckedChanged, value);
            }
        }

        protected virtual void OnCheckedChanged(EventArgs e)
        {
            var handler = (EventHandler)Events[EventCheckedChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <devdoc> 
        ///    Occurs when the FilterByArrNights state is changed upon server
        ///    postback. 
        /// </devdoc>
        public event EventHandler FilterByArrNightsChanged
        {
            add
            {
                Events.AddHandler(EventFilterByArrNightsChanged, value);
            }
            remove
            {
                Events.RemoveHandler(EventFilterByArrNightsChanged, value);
            }
        }

        protected virtual void OnFilterByArrNightsChanged(EventArgs e)
        {
            var handler = (EventHandler)Events[EventFilterByArrNightsChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            Web.RegisterClientScriptInclude(Page, ResolveClientUrl("~/scripts/hotelsFilter.js")); // регистрируем файл скриптов для контрола 
            Web.RegisterStartupScript(this, @"startupHotelsFilter();", true); // выполняем скрипт на стороне клиента сразу после загрузки контрола

            _queryStringParametrs = new QueryStringParametrs(Request); // необходимо для чтения параметров из строки запроса

            // инициализируем состояние фильтра отелей по ночам и продолжительностям
            // если это первая загрузка и есть строка параметров то берем из нее
            if (!Page.IsPostBack)
            {
                if (!_queryStringParametrs.IsEmpty && _queryStringParametrs.IsParametrsValid)
                    ChbFilterByArrNights.Checked = _queryStringParametrs.IsHotelsFiltredByArrNights;
                else
                    ChbFilterByArrNights.Checked = Globals.Settings.TourFilters.FilterByArrNights;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Устанавливает отели в списке в состоянии выбрано по переданным ключам отелей.
        /// </summary>
        /// <param name="valuesToSelect">список ключей отелей которые нужно выбрать</param>
        /// <returns>true если все ключи отелей присутствуют в наборе, false если хотя бы один отсутсвует</returns>
        public bool SelectItems(IList<string> valuesToSelect)
        {
            if (valuesToSelect == null) throw new ArgumentNullException("valuesToSelect");
            if (!valuesToSelect.Any())
                return false;

            ClearSelection(Items);

            var selectableListItems = Items.Cast<ListItem>().Where(l => l.Enabled).ToList();
            if (valuesToSelect.All(v => selectableListItems.Any(l => l.Value == v)) && selectableListItems.All(l => valuesToSelect.Any(v => v == l.Value)) && selectableListItems.Count != 1)
            {
                ChbAllOptions.Checked = true;
                return true;
            }

            var ret = true;

            foreach (var valueToSelect in valuesToSelect)
            {
                var listItem = Items.FindByValue(valueToSelect);
                if (listItem == null || !listItem.Enabled)
                {
                    ret = false;
                    continue;
                }
                listItem.Selected = true;
            }
            ChbAllOptions.Checked = ChblOptions.SelectedItem == null;
            return ret;
        }

        /// <summary>
        /// Очистить выбранные элементы.
        /// </summary>
        /// <param name="collection">Коллекция элементов в которой нужно сбросить выбор</param>
        protected void ClearSelection(ListItemCollection collection)
        {
            foreach (ListItem listItem in collection)
            {
                listItem.Selected = false;
            }
        }

        protected void ChblOptions_DataBound(object sender, EventArgs e)
        {
            ChbAllOptions.Checked = ChblOptions.SelectedItem == null;
        }
        protected void OptionsControl_CheckedChanged(object sender, EventArgs e)
        {
            if (sender == ChbFilterByArrNights)
            {
                OnFilterByArrNightsChanged(EventArgs.Empty);
                return;
            }

            if (sender == ChbAllOptions)
            {
                ChbAllOptions.Checked = true;
                ClearSelection(ChblOptions.Items);
            }
            if (sender == ChblOptions)
                ChbAllOptions.Checked = !(from ListItem item in ChblOptions.Items
                                          where item.Selected
                                          select item).Any();

            OnCheckedChanged(EventArgs.Empty);
        }

    }
}