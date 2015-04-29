using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.DataModel
{
    /// <summary>
    /// Класс, определяющий параметры цены
    /// </summary>
    public class PriceValue
    {
        public PriceValue()
        {
            
        }
        public PriceValue(PriceValue from)
        {
            Price = from.Price;
            Rate = from.Rate;
        }

        /// <summary>
        /// Значение цены
        /// </summary>
        public double? Price { get; set; }
        /// <summary>
        /// Значение валюты
        /// </summary>
        public string Rate { get; set; }

        /// <summary>
        /// Возвращает строку, представляющую текущий объект.
        /// </summary>
        /// <returns>
        /// Строка, представляющая текущий объект.
        /// </returns>
        public override string ToString()
        {
            return String.Format("{0}_{1}", Price, Rate);
        }
    }
}
