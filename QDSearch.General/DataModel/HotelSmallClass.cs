using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.DataModel
{
    /// <summary>
    /// Нужен для того, чтобы не сериализовывать полный класс отеля
    /// </summary>
    public class HotelSmallClass
    {
        public int Key { get; set; }
        public string Name { get; set; }
        public int? RsKey { get; set; }
        public string Http { get; set; }
        public string Stars { get; set; }
        public int CtKey { get; set; }
        public int CnKey { get; set; }
    }
}
