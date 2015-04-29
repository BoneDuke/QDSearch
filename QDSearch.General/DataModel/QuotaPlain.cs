using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.DataModel
{
    /// <summary>
    /// Класс, представляющий квоты, полученные из БД
    /// </summary>
    public class QuotaPlain
    {
        /// <summary>
        /// Ключ QuoaDetail, если на него стоит стоп
        /// </summary>
        public int? SsQdId { get; set; }
        /// <summary>
        /// Тип квоты
        /// </summary>
        public QuotaType Type { get; set; }
        /// <summary>
        /// Число мест если квоты на заезд
        /// </summary>
        public int? CheckInPlaces { get; set; }
        /// <summary>
        /// Число занятых мест если квота на заезд
        /// </summary>
        public int? CheckInPlacesBusy { get; set; }
        /// <summary>
        /// Число мест в квоте, если квота на период
        /// </summary>
        public int Places { get; set; }
        /// <summary>
        /// Число занятых мест, если квота на заезд
        /// </summary>
        public int Busy { get; set; }
        /// <summary>
        /// Если квота на продолжительность, то здесь ставится на какую продолжительность
        /// </summary>
        public string Duration { get; set; }
        /// <summary>
        /// Для стопа - стоп на элотмент и комитмент или на только на элотмент
        /// </summary>
        public bool IsAllotmentAndCommitment { get; set; }
        /// <summary>
        /// Ключ объекта квотирования (QuotaObject)
        /// </summary>
        public int QoId { get; set; }
        /// <summary>
        /// Ключ стопа
        /// </summary>
        public int? SsId { get; set; }
        /// <summary>
        /// Ключ таблицы QuotaDetail
        /// </summary>
        public int QdId { get; set; }
        /// <summary>
        /// Ключ партнера
        /// </summary>
        public int PartnerKey { get; set; }
        /// <summary>
        /// Ключ агента
        /// </summary>
        public int AgentKey { get; set; }
        /// <summary>
        /// Ключ SubCode1
        /// </summary>
        public int SubCode1 { get; set; }
        /// <summary>
        /// Ключ SubCode2
        /// </summary>
        public int SubCode2 { get; set; }
        /// <summary>
        /// Дата квоты
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Релиз квоты
        /// </summary>
        public short? Release { get; set; }
    }
}
