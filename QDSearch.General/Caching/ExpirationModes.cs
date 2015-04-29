using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.Caching
{
    /// <summary>
    /// Варианты устаревания кэша
    /// </summary>
    public enum ExpirationModes
    {
        /// <summary>
        /// Время задается в абсолютных величинах
        /// </summary>
        Absolute = 0,
        /// <summary>
        /// Время задается в виде плавающего окна
        /// </summary>
        Sliding = 1
    }
}
