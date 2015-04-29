using System.ServiceModel;
using QDSearch.Repository.MtMain;
using QDSearch.Repository.MtSearch;
using Seemplexity.Logic.Flights;
using Seemplexity.Logic.Flights.DataModel;

namespace SMServices.Wcf.FlightSearchCityTravel
{
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any)]
    public class FlightSearchService : IFlightSearchService
    {
        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        /// <summary>
        /// Данные по квотам на перелеты
        /// </summary>
        /// <param name="query">Параметры запроса</param>
        /// <returns></returns>
        public QuotaPriceResult GetFlightQuotaPrices(QuotaPriceQuery query)
        {
            QuotaPriceResult flightQuotaPrices;
            using (var mtsDc = new MtSearchDbDataContext())
            {
                using (var mtmDc = new MtMainDbDataContext())
                {
                    flightQuotaPrices = mtsDc.GetFlightQuotaPrices(mtmDc, query);
                }
            }
            return flightQuotaPrices;
        }

        /// <summary>
        /// Метод по выдаче расписаний рейсов
        /// </summary>
        /// <returns></returns>
        public FlightSchedule GetFlightSchedules()
        {
            FlightSchedule flightSchedule;
            using (var mtsDc = new MtSearchDbDataContext())
            {
                using (var mtmDc = new MtMainDbDataContext())
                {
                    flightSchedule = mtsDc.GetFlightSchedules(mtmDc);
                }
            }
            return flightSchedule;
        }
    }
}
