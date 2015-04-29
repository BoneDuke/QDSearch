using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.Helpers
{
    /// <summary>
    /// Расширение для датаридеров
    /// </summary>
    public static class DataReaderExtensions
    {
        /// <summary>
        /// Возвращает строку в том случае, если есть подозрение на null
        /// </summary>
        /// <param name="reader">Ридер</param>
        /// <param name="ordinal">Номер колонки</param>
        /// <returns></returns>
        public static string GetStringOrNull(this IDataReader reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }

        /// <summary>
        /// Возвращает decimal в том случае, если есть подозрение на null
        /// </summary>
        /// <param name="reader">Ридер</param>
        /// <param name="ordinal">Номер колонки</param>
        /// <returns></returns>
        public static decimal? GetDecimalOrNull(this IDataReader reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? null : (decimal?)reader.GetDecimal(ordinal);
        }

        /// <summary>
        /// Возвращает int в том случае, если есть подозрение на null
        /// </summary>
        /// <param name="reader">Ридер</param>
        /// <param name="ordinal">Номер колонки</param>
        /// <returns></returns>
        public static int? GetInt32OrNull(this IDataReader reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? null : (int?)reader.GetInt32(ordinal);
        }

        /// <summary>
        /// Возвращает short в том случае, если есть подозрение на null
        /// </summary>
        /// <param name="reader">Ридер</param>
        /// <param name="ordinal">Номер колонки</param>
        /// <returns></returns>
        public static short? GetInt16OrNull(this IDataReader reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? null : (short?)reader.GetInt16(ordinal);
        }

        /// <summary>
        /// Возвращает bool в том случае, если есть подозрение на null
        /// </summary>
        /// <param name="reader">Ридер</param>
        /// <param name="ordinal">Номер колонки</param>
        /// <returns></returns>
        public static bool? GetBooleanOrNull(this IDataReader reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? null : (bool?)reader.GetBoolean(ordinal);
        }

        /// <summary>
        /// Возвращает bool в том случае, если есть подозрение на null
        /// </summary>
        /// <param name="reader">Ридер</param>
        /// <param name="columnName">Название колонки</param>
        /// <returns></returns>
        public static bool? GetBooleanOrNull(this IDataReader reader, string columnName)
        {
            return reader.GetBooleanOrNull(reader.GetOrdinal(columnName));
        }

        /// <summary>
        /// Возвращает строку в том случае, если есть подозрение на null
        /// </summary>
        /// <param name="reader">Ридер</param>
        /// <param name="columnName">Название колонки</param>
        /// <returns></returns>
        public static string GetString(this IDataReader reader, string columnName)
        {
            return reader.GetString(reader.GetOrdinal(columnName));
        }

        /// <summary>
        /// Возвращает строку в том случае, если есть подозрение на null
        /// </summary>
        /// <param name="reader">Ридер</param>
        /// <param name="columnName">Название колонки</param>
        /// <returns></returns>
        public static string GetStringOrNull(this IDataReader reader, string columnName)
        {
            return reader.GetStringOrNull(reader.GetOrdinal(columnName));
        }

        /// <summary>
        /// Возвращает short без возможности null
        /// </summary>
        /// <param name="reader">Ридер</param>
        /// <param name="columnName">Название колонки</param>
        /// <returns></returns>
        public static short GetInt16(this IDataReader reader, string columnName)
        {
            return reader.GetInt16(reader.GetOrdinal(columnName));
        }

        /// <summary>
        /// Возвращает short в том случае, если есть подозрение на null
        /// </summary>
        /// <param name="reader">Ридер</param>
        /// <param name="columnName">Название колонки</param>
        /// <returns></returns>
        public static short? GetInt16OrNull(this IDataReader reader, string columnName)
        {
            return reader.GetInt16OrNull(reader.GetOrdinal(columnName));
        }

        /// <summary>
        /// Возвращает int без возможности null
        /// </summary>
        /// <param name="reader">Ридер</param>
        /// <param name="columnName">Название колонки</param>
        /// <returns></returns>
        public static int GetInt32(this IDataReader reader, string columnName)
        {
            return reader.GetInt32(reader.GetOrdinal(columnName));
        }

        /// <summary>
        /// Возвращает int без возможности null
        /// </summary>
        /// <param name="reader">Ридер</param>
        /// <param name="columnName">Название колонки</param>
        /// <returns></returns>
        public static long GetInt64(this IDataReader reader, string columnName)
        {
            return reader.GetInt64(reader.GetOrdinal(columnName));
        }

        /// <summary>
        /// Возвращает int с возможностью null
        /// </summary>
        /// <param name="reader">Ридер</param>
        /// <param name="columnName">Название колонки</param>
        /// <returns></returns>
        public static int? GetInt32OrNull(this IDataReader reader, string columnName)
        {
            return reader.GetInt32OrNull(reader.GetOrdinal(columnName));
        }

        /// <summary>
        /// Возвращает строку в том случае, если есть подозрение на null
        /// </summary>
        /// <param name="reader">Ридер</param>
        /// <param name="columnName">Название колонки</param>
        /// <returns></returns>
        public static double GetDouble(this IDataReader reader, string columnName)
        {
            return reader.GetDouble(reader.GetOrdinal(columnName));
        }

        /// <summary>
        /// Возвращает строку в том случае, если есть подозрение на null
        /// </summary>
        /// <param name="reader">Ридер</param>
        /// <param name="columnName">Название колонки</param>
        /// <returns></returns>
        public static decimal GetDecimal(this IDataReader reader, string columnName)
        {
            return reader.GetDecimal(reader.GetOrdinal(columnName));
        }

        /// <summary>
        /// Возвращает строку в том случае, если есть подозрение на null
        /// </summary>
        /// <param name="reader">Ридер</param>
        /// <param name="columnName">Название колонки</param>
        /// <returns></returns>
        public static DateTime GetDateTime(this IDataReader reader, string columnName)
        {
            return reader.GetDateTime(reader.GetOrdinal(columnName));
        }
    }
}
