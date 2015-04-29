using System.Runtime.Serialization;

namespace Seemplexity.Services.Wcf.ToursSearch.DataModel
{
    /// <summary>
    /// Перечисление типов подборов перелетов в поиске
    /// </summary>
    [DataContract]
    public enum FlightTicketState
    {
        [EnumMember]
        All = 0,
        [EnumMember]
        IncludedOnly = 1,
        [EnumMember]
        ExcludedOnly = 2
    }
}