using System;
using System.Collections.Generic;
namespace QDSearch.DataModel
{
    /// <summary>
    /// Класс отеля
    /// </summary>
    [Serializable]
    public class Hotel
    {
        /// <summary>
        /// Ключ отеля
        /// </summary>
        public int Key { get; set; }
        /// <summary>
        /// Дата начала проживания
        /// </summary>
        public DateTime DateFrom { get; set; }
        /// <summary>
        /// Название отеля
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Категория отеля
        /// </summary>
        public string Stars { get; set; }
        /// <summary>
        /// Ключ курорта
        /// </summary>
        public int ResortKey { get; set; }
        /// <summary>
        /// Название курорта
        /// </summary>
        public string ResortName { get; set; }
        /// <summary>
        /// Ссылка на описание
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// Ключ записи HotelRoom
        /// </summary>
        public int HotelRoomKey { get; set; }
        /// <summary>
        /// Ключ питания
        /// </summary>
        public int PansionKey { get; set; }
        /// <summary>
        /// Название питания
        /// </summary>
        public string PansionName { get; set; }
        /// <summary>
        /// Число ночей
        /// </summary>
        public int NightsCount { get; set; }
        /// <summary>
        /// Ключ типа номера
        /// </summary>
        public int RoomKey { get; set; }
        /// <summary>
        /// Название типа номера
        /// </summary>
        public string RoomName { get; set; }
        /// <summary>
        /// Ключ категории номера
        /// </summary>
        public int RoomCategoryKey { get; set; }
        /// <summary>
        /// Название категории номера
        /// </summary>
        public string RoomCategoryName { get; set; }
        /// <summary>
        /// Ключ размещения
        /// </summary>
        public int AccomodationKey { get; set; }
        /// <summary>
        /// Название размещения
        /// </summary>
        public string AccomodationName { get; set; }
        /// <summary>
        /// Ключ партнера по отелю
        /// </summary>
        public int PartnerKey { get; set; }
        /// <summary>
        /// Используется для определения, есть ли на этот отель цена. Если нет, подкрашивается серым в фильтре
        /// </summary>
        public bool IsValidPrice { get; set; }
        /// <summary>
        /// Значение квоты по отелю
        /// </summary>
        public QuotaStatePlaces QuotaState;
    }

    /// <summary>
    /// Компаратор для получения distinct запроса
    /// </summary>
    public class DistinctEqualityComparer : IEqualityComparer<Hotel>
    {
        /// <summary>
        /// Определяет, равны ли два указанных объекта.
        /// </summary>
        /// <returns>
        /// true, если указанные объекты равны; в противном случае — false.
        /// </returns>
        /// <param name="x">Первый сравниваемый объект типа </param>
        /// <param name="y">Второй сравниваемый объект типа </param>
        public bool Equals(Hotel x, Hotel y)
        {
            return x.Key == y.Key;
        }

        /// <summary>
        /// Возвращает хэш-код указанного объекта.
        /// </summary>
        /// <returns>
        /// Хэш-код указанного объекта.
        /// </returns>
        /// <param name="obj">Объект <see cref="T:System.Object"/>, для которого необходимо возвратить хэш-код.</param>
        public int GetHashCode(Hotel obj)
        {
            return obj.Key.GetHashCode();
        }
    }
}
