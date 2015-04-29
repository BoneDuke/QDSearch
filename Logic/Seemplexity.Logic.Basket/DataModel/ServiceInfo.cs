using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QDSearch.DataModel;
using QDSearch.Repository.MtSearch;

namespace Seemplexity.Logic.Basket.DataModel
{
    /// <summary>
    /// Информация по услуге в корзине
    /// </summary>
    public class ServiceInfo
    {
        /// <summary>
        /// Ключ из таблицы tp_services
        /// </summary>
        public int Key { get; set; }
        /// <summary>
        /// Класс услуги в качестве перечисления
        /// </summary>
        public ServiceClass ServiceClass { get; set; }
        /// <summary>
        /// Code услуги (для каждого класса он свой)
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// SubCode1 услуги (для каждого класса он свой)
        /// </summary>
        public int? SubCode1 { get; set; }

        /// <summary>
        /// SubCode2 услуги (для каждого класса он свой)
        /// </summary>
        public int? SubCode2 { get; set; }

        /// <summary>
        /// Ключ партнера
        /// </summary>
        public int PartnerKey { get; set; }

        /// <summary>
        /// Ключ пакета
        /// </summary>
        public int PacketKey { get; set; }
        /// <summary>
        /// День услуги в туре
        /// </summary>
        public short Day { get; set; }
        /// <summary>
        /// Дата начала предоставления услуги
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// Продолжительность услуги
        /// </summary>
        public short Days { get; set; }

        /// <summary>
        /// Стоимость услуги
        /// </summary>
        public decimal? Cost { get; set; }
        
        /// <summary>
        /// Валюта, в которой была расчитана услуга
        /// </summary>
        public Tuple<int, string> RateInfo { get; set; }

        /// <summary>
        /// Число человек по услуге
        /// </summary>
        public short NMen { get; set; }

        /// <summary>
        /// Атрибут услуги
        /// </summary>
        public int Attribute { get; set; }

    }
}
