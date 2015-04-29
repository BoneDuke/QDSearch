using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Controls.ExtendedControls
{
    public partial class CheckBoxListAll : UserControl
    {
        private static readonly object EventCheckedChanged = new object();

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

        public ListItemCollection Items
        {
            get
            {
                return ChblOptions.Items;
            }
        }

        /// <summary>
        /// Возвращает сотояние выбора опций. В случае true выбраны все опции, в случае false выбрано либо часть опциций либо вообще ничего не выбрано.
        /// </summary>
        public bool IsAllOptionsSelected
        {
            get { return ChbAllOptions.Checked; }
        }

        /// <summary>
        /// Возваращает список выбранных ключей
        /// </summary>
        public IList<string> SelectedValues
        {
            get
            {
                return (from ListItem item in ChblOptions.Items
                        where ChbAllOptions.Checked || item.Selected
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
            EventHandler handler = (EventHandler)Events[EventCheckedChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack) // && !IsViewStateEnabled)
            {

            }
        }

        /// <summary>
        /// Устанавливает опции в списке в состоянии выбрано по переданным ключам опций.
        /// </summary>
        /// <param name="valuesToSelect">список ключей которые нужно выбрать</param>
        /// <returns>true если все значения присутствуют в наборе, false если хотя бы один отсутсвует</returns>
        public bool SelectItems(IList<string> valuesToSelect)
        {
            if (valuesToSelect == null) throw new ArgumentNullException("valuesToSelect");
            if (!valuesToSelect.Any())
                return false;

            ClearSelection(Items);

            IList<ListItem> listItems = Items.Cast<ListItem>().ToList();
            if (valuesToSelect.All(v => listItems.Any(l => l.Value == v)) && listItems.All(l => valuesToSelect.Any(v => v == l.Value)) && listItems.Count != 1)
            {
                ChbAllOptions.Checked = true;
                return true;
            }

            bool ret = true;

            foreach (var valueToSelect in valuesToSelect)
            {
                ListItem listItem = Items.FindByValue(valueToSelect);
                if (listItem == null)
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
        /// Сбросить выбранные элементы.
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