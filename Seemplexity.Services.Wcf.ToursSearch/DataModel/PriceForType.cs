using System.Runtime.Serialization;

namespace Seemplexity.Services.Wcf.ToursSearch.DataModel
{
    [DataContract]
    public enum PriceForType : ushort
    {
        /// <summary>
        /// �� ��������
        /// </summary>
        [EnumMember]
        PerMen = 0,
        /// <summary>
        /// �� �����
        /// </summary>
        [EnumMember]
        PerRoom = 1
    }
}