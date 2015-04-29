using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;
using QDSearch;
using QDSearch.DataModel;
using SMServices.Sletat.DataModel;

namespace SMServices.Sletat.Helpers
{
    public static class Converters
    {
        public static HotelQuota GetHotelQuota(IEnumerable<QuotesStates> quotaStates)
        {
            var result = HotelQuota.Available;
            foreach (var quotaState in quotaStates)
            {
                if (quotaState == QuotesStates.Request && (result == HotelQuota.Available))
                    result = HotelQuota.Request;
                else if (quotaState == QuotesStates.No && (result == HotelQuota.Request || result == HotelQuota.Available))
                    result = HotelQuota.NoPlaces;
            }
            return result;
        }

        public static int? IsFewPlaces(IEnumerable<QuotaStatePlaces> quotaStatePlaces)
        {
            int? result = null;
            foreach (var quotaState in quotaStatePlaces.Where(quotaState => quotaState.QuotaState == QuotesStates.Small))
            {
                if (result == null)
                    result = (int)quotaState.Places;
                else
                    result = Math.Min(result.Value, (int)quotaState.Places);
            }
            return result;
        }

        public static TicketsInPrice GetTicketsInPrice(IEnumerable<QuotesStates> charterQuotaTo, IEnumerable<QuotesStates> charterQuotaFrom)
        {
            var result = TicketsInPrice.NotIncluded;
            foreach (var quotaState in charterQuotaTo.Where(quotaState => quotaState != QuotesStates.None))
            {
                result = TicketsInPrice.Included;
            }
            foreach (var quotaState in charterQuotaFrom.Where(quotaState => quotaState != QuotesStates.None))
            {
                result = TicketsInPrice.Included;
            }
            return result;
        }

        public static CharterQuota GetCharterQuota(QuotesStates quotaState)
        {
            var result = CharterQuota.Available;

            if (quotaState == QuotesStates.Request && (result == CharterQuota.Available))
                result = CharterQuota.Request;
            else if ((quotaState == QuotesStates.No || quotaState == QuotesStates.None) && (result == CharterQuota.Request || result == CharterQuota.Available))
                result = CharterQuota.NoPlaces;

            return result;
        }

        public static ServiceType ServiceClassToServiceType(int serviceClass, int day)
        {
            var result = ServiceType.Undefined;
            switch (serviceClass)
            {
                case (int)ServiceClass.AddHotelService:
                    result = ServiceType.AdditionalService;
                    break;
                case (int)ServiceClass.Excursion:
                    result = ServiceType.Excursion;
                    break;
                case (int)ServiceClass.Flight:
                    result = day == 1 ? ServiceType.DptTransport : ServiceType.RtnTransport;
                    break;
                case (int)ServiceClass.Insurance:
                    result = ServiceType.Insurance;
                    break;
                case (int)ServiceClass.Surcharge:
                    result = ServiceType.Charge;
                    break;
                case (int)ServiceClass.Transfer:
                    result = ServiceType.Transfer;
                    break;
                case (int)ServiceClass.Visa:
                    result = ServiceType.Visa;
                    break;
            }
            return result;
        }

        public static FlightClass CharterClassToFlightClass(int serviceClass)
        {
            var result = FlightClass.Undefined;
            if (Globals.Settings.CharterClassesDictionary[0].Contains(serviceClass))
                result = FlightClass.Econom;
            else if (Globals.Settings.CharterClassesDictionary[1].Contains(serviceClass))
                result = FlightClass.Business;
            return result;
        }

        public static string ToString(this FlightClass flightClass)
        {
            var result = String.Empty;
            switch (flightClass)
            {
                case FlightClass.Business:
                    result = "Business".ToUpper();
                    break;
                case FlightClass.Econom:
                    result = "Econom".ToUpper();
                    break;
            }
            return result;
        }

        public static string ToString(this ServiceType serviceType)
        {
            var result = String.Empty;
            switch (serviceType)
            {
                  case ServiceType.AdditionalMeal:
                    result = "AdditionalMeal";
                    break;
                case ServiceType.AdditionalService:
                    result = "AdditionalService";
                    break;
                case ServiceType.Charge:
                    result = "Charge";
                    break;
                case ServiceType.DptTransport:
                    result = "DptTransport";
                    break;
                case ServiceType.Excursion:
                    result = "Excursion";
                    break;
                case ServiceType.Insurance:
                    result = "Insurance";
                    break;
                case ServiceType.NoGoGarantee:
                    result = "NoGoGarantee";
                    break;
                case ServiceType.RtnTransport:
                    result = "RtnTransport";
                    break;
                case ServiceType.Transfer:
                    result = "Transfer";
                    break;
                case ServiceType.Visa:
                    result = "Visa";
                    break;
            }
            return result;
        }

        public static QuotaAvailability QuotaStateToQuotaAvailability(QuotesStates quotaState)
        {
            var result = QuotaAvailability.Undefined;
            switch (quotaState)
            {
                case QuotesStates.Availiable:
                case QuotesStates.Small:
                    result = QuotaAvailability.Available;
                    break;
                case QuotesStates.No:
                    result = QuotaAvailability.NoPlaces;
                    break;
                case QuotesStates.Request:
                    result = QuotaAvailability.Request;
                    break;
            }
            return result;
        }
    }
}
