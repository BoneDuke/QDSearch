using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.Configuration
{
    /// <summary>
    /// Отвечает за конфигурацию параметров Фильтров поиска в Web.config
    /// </summary>>
    public class TourFiltersElement : ConfigurationElement
    {
        /// <summary>
        /// Определяет количество дней по умолчанию в днях между датами заездов "с" и "по". Используется для определения даты "по" при изменении даты "c".
        /// Так же для выбора значений по умолчанию "с" и "по" при закгрузке дат заездов в календарь.
        /// </summary>
        [ConfigurationProperty("defaultArrivalRangeDays", DefaultValue = "0", IsRequired = false)]
        public uint DefaultArrivalRange
        {
            get { return (uint)this["defaultArrivalRangeDays"]; }
            set { this["defaultArrivalRangeDays"] = value; }
        }
        /// <summary>
        /// Максимально допустимый период в днях при выборе дней "c" "по"
        /// </summary>
        [ConfigurationProperty("maxArrivalRangeDays", DefaultValue = "30", IsRequired = false)]
        public uint MaxArrivalRange
        {
            get { return (uint)this["maxArrivalRangeDays"]; }
            set { this["maxArrivalRangeDays"] = value; }
        }        
        /// <summary>
        /// Задает количество туров на странице в результатах поиска в Web.config
        /// </summary>
        [ConfigurationProperty("defaultToursNumberOnPage", DefaultValue = "50", IsRequired = false)]
        public uint DefaultToursNumberOnPage
        {
            get { return (uint)this["defaultToursNumberOnPage"]; }
            set { this["defaultToursNumberOnPage"] = value; }
        } 
        /// <summary>
        /// Задает количество туров на странице в результатах поиска в Web.config
        /// </summary>
        [ConfigurationProperty("defaultCityFromKey", DefaultValue = "0", IsRequired = false)]
        public uint DefaultCityFromKey
        {
            get { return (uint)this["defaultCityFromKey"]; }
            set { this["defaultCityFromKey"] = value; }
        }        
        /// <summary>
        /// Задает количество туров на странице в результатах поиска в Web.config
        /// </summary>
        [ConfigurationProperty("defaultRateCode", DefaultValue = "рб", IsRequired = false)]
        public string DefaultRateCode
        {
            get { return (string)this["defaultRateCode"]; }
            set { this["defaultRateCode"] = value; }
        }        
        /// <summary>
        /// Включает фильтрацию списка отелей в фильтре отелей с учетом выбранных дат заездов и ночей в Web.config
        /// </summary>
        [ConfigurationProperty("filterByArrNights", DefaultValue = "False", IsRequired = false)]
        public bool FilterByArrNights
        {
            get { return (bool)this["filterByArrNights"]; }
            set { this["filterByArrNights"] = value; }
        }
    }
}
