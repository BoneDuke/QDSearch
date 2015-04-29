using System;
using System.Runtime.Serialization;

namespace Seemplexity.Services.Wcf.ToursSearch.DataModel
{
    /// <summary>
    /// Значения квотирования для фильтров поиска
    /// </summary>
    [Flags]
    [DataContract]
    public enum QuotesStates : byte
    {
        /// <summary>
        /// Значение отсутсвует, не инициализировано. Равносильно NULL. 
        /// Т.е физическое отсутсвие логического значения квоты. 
        /// В отличие от Undefined который обозначает что физически значение квоты есть, оно инициализировано неопределенным значением.
        /// </summary>
        None = 0,
        /// <summary>
        /// Есть места
        /// </summary>
        [EnumMember]
        Availiable = 1,
        /// <summary>
        /// Нет мест места
        /// </summary>
        [EnumMember]
        No = 2,
        /// <summary>
        /// Запрос
        /// </summary>
        [EnumMember]
        Request = 4,
        /// <summary>
        /// Мало
        /// </summary>
        [EnumMember]
        Small = 8,
        /// <summary>
        /// Неопределено
        /// </summary>
        [EnumMember]
        Undefined = 16
    }
}
