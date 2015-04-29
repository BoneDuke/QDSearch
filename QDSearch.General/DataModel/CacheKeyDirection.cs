using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.DataModel
{
    /// <summary>
    /// Направление расчета цен
    /// </summary>
    public class CacheKeyDirection
    {
        /// <summary>
        /// Ключ записи в таблице mwReplQueue
        /// </summary>
        public int Key { get; set; }
        /// <summary>
        /// Ключ страны
        /// </summary>
        public int CountryKey { get; set; }
        /// <summary>
        /// Ключ города вылета
        /// </summary>
        public int CityKeyFrom { get; set; }
    }

    class CacheKeyMainPartComparer : IEqualityComparer<CacheKeyDirection>
    {
        public bool Equals(CacheKeyDirection x, CacheKeyDirection y)
        {
            //Check whether the compared objects reference the same data. 
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null. 
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return (x.CountryKey == y.CountryKey && x.CityKeyFrom == y.CityKeyFrom);
        }

        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.

        public int GetHashCode(CacheKeyDirection obj)
        {
            //Check whether the object is null 
            if (Object.ReferenceEquals(obj, null)) return 0;

            //Calculate the hash code for the product. 
            return obj.CountryKey ^ obj.CityKeyFrom;
        }
    }
}
