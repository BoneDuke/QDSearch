using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDSearch.DataModel
{
    /// <summary>
    /// Класс для работы со статусами квотирования
    /// </summary>
    public class QuotaStatePlaces
    {

        public QuotaStatePlaces()
        {
            QuotaState = QuotesStates.None;
        }

        public QuotaStatePlaces(QuotaStatePlaces from)
        {
            QuotaState = from.QuotaState;
            Places = from.Places;
            IsCheckInQuota = from.IsCheckInQuota;
        }

        /// <summary>
        /// Метод по определению равенства объектов
        /// </summary>
        /// <param name="other">Объект, с которым сравниваем</param>
        /// <returns></returns>
        protected bool Equals(QuotaStatePlaces other)
        {
            return QuotaState == other.QuotaState && Places == other.Places && IsCheckInQuota.Equals(other.IsCheckInQuota);
        }

        /// <summary>
        /// Играет роль хэш-функции для определенного типа.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) QuotaState;
                hashCode = (hashCode*397) ^ (int) Places;
                hashCode = (hashCode*397) ^ IsCheckInQuota.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Статус квотирования
        /// </summary>
        public QuotesStates QuotaState { get; set; }
        /// <summary>
        /// Число свободных мест
        /// </summary>
        public uint Places { get; set; }
        /// <summary>
        /// Является ли квота квотой на заезд
        /// </summary>
        public bool IsCheckInQuota { get; set; }

        /// <summary>
        /// Определяет, равен ли заданный объект текущему объекту.
        /// </summary>
        /// <returns>
        /// true, если указанный объект равен текущему объекту; в противном случае — false.
        /// </returns>
        /// <param name="obj">Объект, который требуется сравнить с текущим объектом.</param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((QuotaStatePlaces) obj);
        }
    }

    /// <summary>
    /// Тип квоты
    /// </summary>
    public enum QuotaType : short
    {
        /// <summary>
        /// Любой
        /// </summary>
        All = 0,
        /// <summary>
        /// Эллотмент
        /// </summary>
        Allotment = 1,
        /// <summary>
        /// Коммитмент
        /// </summary>
        Commitment = 2
    }

    /// <summary>
    /// Значения квотирования для фильтров поиска
    /// </summary>
    [Flags]
    [Serializable]
    public enum QuotesStates : byte
    {
        /// <summary>
        /// Значение отсутсвует, не инициализировано. Равносильно NULL. 
        /// Т.е физическое отсутсвие логического значения квоты. 
        /// </summary>
        None = 0,
        /// <summary>
        /// Есть места
        /// </summary>
        Availiable = 1,
        /// <summary>
        /// Нет мест места
        /// </summary>
        No = 2,
        /// <summary>
        /// Запрос
        /// </summary>
        Request = 4,
        /// <summary>
        /// Мало
        /// </summary>
        Small = 8
    }

    /// <summary>
    /// Параметры для определения числа квот "мало"
    /// </summary>
    [Serializable]
    public class QuotaSmallServiceParams
    {
        /// <summary>
        /// Параметр "и", являются ли значение всех остальных параметров обязательным
        /// </summary>
        public bool AndParam;
        /// <summary>
        /// Число мест в процентах
        /// </summary>
        public double? PercentParam;
        /// <summary>
        /// Число мест в абсолютной величине
        /// </summary>
        public uint? PlaceParam;
    }
}
