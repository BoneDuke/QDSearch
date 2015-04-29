using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.DataModel
{
    /// <summary>
    /// Направление сортировки
    /// </summary>
    [Serializable]
    public enum SortingDirection : ushort
    {
        /// <summary>
        /// По возрастанию
        /// </summary>
        Asc,
        /// <summary>
        /// По убыванию
        /// </summary>
        Desc
    }

    /// <summary>
    /// Тип столбца, по которому производится сортировка в поиске
    /// </summary>
    [Serializable]
    public enum SortingColumn : ushort
    {
        /// <summary>
        /// Колонка дата заезда
        /// </summary>
        TourDate,
        /// <summary>
        /// Отеля
        /// </summary>
        HotelName,
        /// <summary>
        /// Категория
        /// </summary>
        HotelCategoryName,
        /// <summary>
        /// Регион
        /// </summary>
        HotelRegionName,
        /// <summary>
        /// Тип номера
        /// </summary>
        RoomTypeName,
        /// <summary>
        /// Категория номера
        /// </summary>
        RoomCategoryName,
        /// <summary>
        /// Размещение
        /// </summary>
        AccomodationName,
        /// <summary>
        /// Питание
        /// </summary>
        PansionName,
        /// <summary>
        /// Ночи
        /// </summary>
        NightsCount,
        /// <summary>
        /// Цена
        /// </summary>
        Price,
        /// <summary>
        /// Источник цены
        /// </summary>
        TourName
    }
}
