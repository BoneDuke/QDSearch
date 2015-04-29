using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.DataModel
{
    /// <summary>
    /// Курс для внутреннего использования в программе
    /// </summary>
    [Serializable]
    public class SimpleCourse
    {
        /// <summary>
        /// Курс
        /// </summary>
        public decimal Course { get; set; }
        /// <summary>
        /// Курс ЦБ
        /// </summary>
        public decimal CourseCb { get; set; }
        /// <summary>
        /// Дата с
        /// </summary>
        public DateTime DateFrom { get; set; }
        /// <summary>
        /// Дата по
        /// </summary>
        public DateTime DateTo { get; set; }
        /// <summary>
        /// Ключ валюты из которой конвертируем
        /// </summary>
        public int RateKeyFrom { get; set; }
        /// <summary>
        /// Ключ валюты в которую конвертируем
        /// </summary>
        public int RateKeyTo { get; set; }
    }

    /// <summary>
    /// Курс для конвертации на лету в поиске
    /// </summary>
    [Serializable]
    public class CrossCourse
    {
        /// <summary>
        /// Курс
        /// </summary>
        public decimal Course { get; set; }
        /// <summary>
        /// Код валюты из которой конвертируем
        /// </summary>
        public string RateCodeFrom { get; set; }
        /// <summary>
        /// Код валюты в которую конвертируем
        /// </summary>
        public string RateCodeTo { get; set; }
    }
}
