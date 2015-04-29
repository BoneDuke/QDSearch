using System.Runtime.Serialization;

namespace Seemplexity.Services.Wcf.ToursSearch.DataModel
{
    /// <summary>
    /// Направление сортировки
    /// </summary>
    [DataContract]
    public enum SortingDirection : ushort
    {
        /// <summary>
        /// По возрастанию
        /// </summary>
        [EnumMember]
        Asc,
        /// <summary>
        /// По убыванию
        /// </summary>
        [EnumMember]
        Desc
    }
}