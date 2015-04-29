using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QDSearch.Repository.MtMain;

namespace QDSearch.Extensions
{
    public static class DogovorsExtension
    {
        public const string TableName = "tbl_Dogovor";

        public static tbl_Dogovor GetDogovorByKey(this MtMainDbDataContext dc, int dogovorKey)
        {
            var result = (from d in dc.tbl_Dogovors
                          where d.DG_Key == dogovorKey
                          select d)
                          .SingleOrDefault();

            return result;
        }
    }
}
