using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using QDSearch.Helpers;
using QDSearch.Repository.MtMain;

namespace QDSearch.Extensions
{
    /// <summary>
    /// Расширение по работе с туристами
    /// </summary>
    public static class TouristsExtension
    {
        /// <summary>
        /// Название таблицы
        /// </summary>
        public const string TableName = "tbl_Turist";

        //static TouristsExtension()
        //{
        //    CacheHelper.AddCacheData(TableName, "Fake Element", TableName);
        //}

        /// <summary>
        /// Получает туриста по его ключу
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="touristKey">Ключ туриста</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static tbl_Turist GetTouristByKey(this MtMainDbDataContext dc, int touristKey, out string hash)
        {
            tbl_Turist result;
            hash = String.Format("{0}_{1}", MethodBase.GetCurrentMethod().Name, touristKey);
            if ((result = CacheHelper.GetCacheItem<tbl_Turist>(hash)) != null) return result;

            result = (from t in dc.tbl_Turists
                where t.TU_KEY == touristKey
                select t).SingleOrDefault();

            CacheHelper.AddCacheData(hash, result, null, Globals.Settings.Cache.MediumCacheTimeout);
            return result;
        }
    }
}
