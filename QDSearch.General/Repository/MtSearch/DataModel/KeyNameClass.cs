using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.Repository.MtSearch.DataModel
{
    public class KeyNameClass
    {
        public KeyNameClass()
        {
        }

        /// <summary>
        /// Ключ записи
        /// </summary>
        public int Key { get; set; }
        /// <summary>
        /// Название записи
        /// </summary>
        public string Name { get; set; }
    }
}
