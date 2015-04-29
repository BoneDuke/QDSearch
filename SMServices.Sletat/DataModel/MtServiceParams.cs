using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMServices.Sletat.DataModel
{
    public class MtServiceParams
    {
        public int Key { get; set; }
        public int SvKey { get; set; }
        public int SubCode1 { get; set; }
        public int SubCode2 { get; set; }
        public int CtKey { get; set; }
        public int Day { get; set; }
        public int Days { get; set; }
        public int Men { get; set; }
        
        public int Code { get; set; }
        public int PrKey { get; set; }
        public int PkKey { get; set; }

        public string Name { get; set; }
        public int Attribute { get; set; }
    }
}
