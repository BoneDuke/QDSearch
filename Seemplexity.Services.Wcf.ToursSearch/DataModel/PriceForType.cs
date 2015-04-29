using System.Runtime.Serialization;

namespace Seemplexity.Services.Wcf.ToursSearch.DataModel
{
    [DataContract]
    public enum PriceForType : ushort
    {
        /// <summary>
        /// За человека
        /// </summary>
        [EnumMember]
        PerMen = 0,
        /// <summary>
        /// За номер
        /// </summary>
        [EnumMember]
        PerRoom = 1
    }
}