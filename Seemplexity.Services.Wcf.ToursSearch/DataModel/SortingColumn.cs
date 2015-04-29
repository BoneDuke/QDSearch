using System.Runtime.Serialization;

namespace Seemplexity.Services.Wcf.ToursSearch.DataModel
{
    /// <summary>
    /// Тип столбца, по которому производится сортировка в поиске
    /// </summary>
    [DataContract]
    public enum SortingColumn : ushort
    {
        /// <summary>
        /// Колонка дата заезда
        /// </summary>
        [EnumMember]
        TourDate,
        /// <summary>
        /// Отеля
        /// </summary>
        [EnumMember]
        HotelName,
        /// <summary>
        /// Категория
        /// </summary>
        [EnumMember]
        HotelCategoryName,
        /// <summary>
        /// Регион
        /// </summary>
        [EnumMember]
        HotelRegionName,
        /// <summary>
        /// Тип номера
        /// </summary>
        [EnumMember]
        RoomTypeName,
        /// <summary>
        /// Категория номера
        /// </summary>
        [EnumMember]
        RoomCategoryName,
        /// <summary>
        /// Размещение
        /// </summary>
        [EnumMember]
        AccomodationName,
        /// <summary>
        /// Питание
        /// </summary>
        [EnumMember]
        PansionName,
        /// <summary>
        /// Ночи
        /// </summary>
        [EnumMember]
        NightsCount,
        /// <summary>
        /// Цена
        /// </summary>
        [EnumMember]
        Price,
        /// <summary>
        /// Источник цены
        /// </summary>
        [EnumMember]
        TourName
    }
}
