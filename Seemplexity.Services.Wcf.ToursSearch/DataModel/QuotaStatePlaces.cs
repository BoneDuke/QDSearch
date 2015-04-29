using System.Runtime.Serialization;

namespace Seemplexity.Services.Wcf.ToursSearch.DataModel
{
    [DataContract]
    public sealed class QuotaStatePlaces
    {
        internal QuotaStatePlaces(QDSearch.DataModel.QuotaStatePlaces quotaStatePlaces)
        {
            QuotaState = (QuotesStates) quotaStatePlaces.QuotaState;
            Places = quotaStatePlaces.Places;
            IsCheckInQuota = quotaStatePlaces.IsCheckInQuota;
        }

        private bool Equals(QuotaStatePlaces other)
        {
            return QuotaState == other.QuotaState && Places == other.Places && IsCheckInQuota.Equals(other.IsCheckInQuota);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) QuotaState;
                hashCode = (hashCode*397) ^ (int) Places;
                hashCode = (hashCode*397) ^ IsCheckInQuota.GetHashCode();
                return hashCode;
            }
        }

        [DataMember]
        public QuotesStates QuotaState { get; set; }

        public uint Places { get; set; }
        public bool IsCheckInQuota { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((QuotaStatePlaces) obj);
        }
    }
}
