using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.Caching
{
    /// <summary>
    /// Допустимые режимы хранения Cache
    /// </summary>
    public enum StorageModes
    {
        /// <summary>
        /// Режим по умолчанию. В этом случае кэш обрабатывается средствами ASP.NET
        /// </summary>
        AspNet = 0,
        /// <summary>
        /// В этом случае кэш хранится в MemoryCache (System.Runtime.Caching) (для тестирования например)
        /// </summary>
        Memory = 1,
        /// <summary>
        /// Сами элементы кэша хранятся на IIS, данные кэша хранятся в Redis
        /// </summary>
        Redis = 2
    }
}