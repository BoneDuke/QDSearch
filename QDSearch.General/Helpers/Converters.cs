using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Xml.Linq;
using QDSearch.DataModel;
using QDSearch.Extensions;
using QDSearch.Repository.MtSearch;
using System.Reflection;

namespace QDSearch.Helpers
{
    /// <summary>
    /// Класс с конрвертерами
    /// </summary>
    public static class Converters
    {
        /// <summary>
        /// Возвращает либо int, либо null если преобразовать строку не получилось
        /// </summary>
        /// <param name="strInt">Строка для конвертации</param>
        /// <returns></returns>
        public static int? GetIntSafe(string strInt)
        {
            int tmp;
            return (int.TryParse(strInt, out tmp) ? tmp : (int?)null);
        }          
        /// <summary>
        /// Возвращает либо int, либо null если преобразовать строку не получилось
        /// </summary>
        /// <param name="strLong">Строка для конвертации</param>
        /// <returns></returns>
        public static long? GetLongSafe(string strLong)
        {
            long tmp;
            return (long.TryParse(strLong, out tmp) ? tmp : (long?)null);
        }        
        /// <summary>
        /// Возвращает либо байт, либо null если преобразовать строку не получилось
        /// </summary>
        /// <param name="strByte">Строка для конвертации</param>
        /// <returns></returns>
        public static byte? GetByteSafe(string strByte)
        {
            byte tmp;
            return (byte.TryParse(strByte, out tmp) ? tmp : (byte?)null);
        }        
        /// <summary>
        /// Возвращает либо DateTime, либо null если преобразовать строку не получилось
        /// </summary>
        /// <param name="strDateTime">Строка для конвертации</param>
        /// <returns></returns>
        public static DateTime? GetDateTimeSafe(string strDateTime)
        {
            DateTime tmp;
            return (DateTime.TryParse(strDateTime, out tmp) ? tmp : (DateTime?)null);
        }

        /// <summary>
        /// Коневертирует строковое представление шестнадцатиричного массива в байт в эквивалетный массив байт
        /// Например "1accf3e4" в byte[] {1a, cc, f3, e4}
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            var hexAsBytes = new byte[hexString.Length / 2];
            for (int index = 0; index < hexAsBytes.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                hexAsBytes[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return hexAsBytes;
        }

        /// <summary>
        /// Конвертация массива в sql-массив
        /// </summary>
        /// <param name="array">Массив int</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static XElement ArrayIntToSqlXmlParametr(int[] array)
        {
            if (array == null)
                throw new ArgumentNullException("array", "Массив для конвертации не может быть NULL");

            if (array.Length == 0)
                return null;

            var root = new XElement("ArrayInt");
            foreach (var item in array)
                root.Add(new XElement("item", item));

            return root;
        }

        /// <summary>
        /// Конвертация массива в sql-массив
        /// </summary>
        /// <param name="array">Массив string</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static XElement ArrayStringToSqlXmlParametr(string[] array)
        {
            if (array == null)
                throw new ArgumentNullException("array", "Массив для конвертации не может быть NULL");

            if (array.Length == 0)
                return null;

            var root = new XElement("ArrayString");
            foreach (var item in array)
                root.Add(new XElement("item", item));

            return root;
        }

        /// <summary>
        /// Метод получает статус квотирования по числу мест осталось, и числу мест всего
        /// </summary>
        /// <param name="dc">Контекст базы данных</param>
        /// <param name="serviceClass">Класс услуги</param>
        /// <param name="quotaExistCount">Осталось мест</param>
        /// <param name="quotaAllCount">Всего мест</param>
        /// <param name="hash">Хэш кэша</param>
        /// <returns></returns>
        public static QuotesStates GetQuotesStateByInt(this MtSearchDbDataContext dc, ServiceClass serviceClass, int quotaExistCount, int quotaAllCount, out string hash)
        {
            hash = String.Format("{0}_{1}_{2}_{3}", MethodBase.GetCurrentMethod().Name, serviceClass, quotaExistCount, quotaAllCount);
            if (CacheHelper.IsCacheKeyExists(hash))
                return CacheHelper.GetCacheItem<QuotesStates>(hash);

            var cacheDependencies = new List<string>();
            QuotesStates result;
            //todo: добавить настрйоку в web.config
            if (quotaExistCount <= 0)
                result = QuotesStates.Request;
            else
            {
                string hashOut;
                result = dc.IsSmallQuotaState(serviceClass, (uint)quotaExistCount, (uint)quotaAllCount, out hashOut) ? QuotesStates.Small : QuotesStates.Availiable;
                cacheDependencies.Add(hashOut);
            }

            CacheHelper.AddCacheData(hash, result, cacheDependencies.ToList(), Globals.Settings.Cache.LongCacheTimeout);
            return result;
        }

        public static DayOfWeek GetDayOfWeekByInt(int dayNumber)
        {
            var result = new DayOfWeek();
            if (dayNumber < 0 || dayNumber >= 8)
                throw new ArgumentException(String.Format("Значение параметра dayNumber должно нахоится в диапазоне от 0 до 7. Текущее значение: {0}", dayNumber));

            switch (dayNumber)
            {
                case 1:
                    result = DayOfWeek.Monday;
                    break;
                case 2:
                    result = DayOfWeek.Tuesday;
                    break;
                case 3:
                    result = DayOfWeek.Wednesday;
                    break;
                case 4:
                    result = DayOfWeek.Thursday;
                    break;
                case 5:
                    result = DayOfWeek.Friday;
                    break;
                case 6:
                    result = DayOfWeek.Saturday;
                    break;
                case 7:
                    result = DayOfWeek.Sunday;
                    break;
            }
            return result;
        }

        /// <summary>
        /// Возвращает номер дня неделеи
        /// </summary>
        /// <param name="dt">Дата для преобразования</param>
        /// <returns></returns>
        public static int GetIntDayOfWeek(DateTime dt)
        {
            int res = 0;
            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    res = 7;
                    break;
                case DayOfWeek.Monday:
                    res = 1;
                    break;
                case DayOfWeek.Tuesday:
                    res = 2;
                    break;
                case DayOfWeek.Wednesday:
                    res = 3;
                    break;
                case DayOfWeek.Thursday:
                    res = 4;
                    break;
                case DayOfWeek.Friday:
                    res = 5;
                    break;
                case DayOfWeek.Saturday:
                    res = 6;
                    break;
            }
            return res;
        }
    }
}
