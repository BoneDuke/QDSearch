using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.DataModel
{
    /// <summary>
    /// Класс по работе с объектами кэширования
    /// </summary>
    public class CacheKeyObject
    {
        /// <summary>
        /// Класс услуги
        /// </summary>
        public int ServiceKey { get; set; }
        /// <summary>
        /// Код услуги
        /// </summary>
        public int? Code { get; set; }
        /// <summary>
        /// Название таблицы
        /// </summary>
        public string Name { get; set; }
    }

    class CacheQuotaKeyComparer : IEqualityComparer<CacheKeyObject>
    {
        public bool Equals(CacheKeyObject x, CacheKeyObject y)
        {
            //Check whether the compared objects reference the same data. 
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null. 
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return (x.ServiceKey == y.ServiceKey && x.Code == y.Code && x.Name == y.Name);
        }

        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.

        public int GetHashCode(CacheKeyObject obj)
        {
            //Check whether the object is null 
            if (Object.ReferenceEquals(obj, null)) return 0;

            //Calculate the hash code for the product. 
            return obj.ServiceKey ^ (obj.Code == null ? 0 : obj.Code.Value) ^ (String.IsNullOrEmpty(obj.Name) ? 0 : obj.Name.GetHashCode());
        }
    }
}
