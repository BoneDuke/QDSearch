using System.ServiceModel;
using System.ServiceModel.Web;
using Seemplexity.Logic.Flights.DataModel;

namespace SMServices.Wcf.FlightSearchCityTravel
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IFlightSearchService" in both code and config file together.
    [ServiceContract]
    public interface IFlightSearchService
    {
        [OperationContract]
        [WebGet]
        string GetData(int value);

        /// <summary>
        /// Данные по квотам на перелеты
        /// </summary>
        /// <param name="query">Параметры запроса</param>
        /// <returns></returns>
        [OperationContract]
        QuotaPriceResult GetFlightQuotaPrices(QuotaPriceQuery query);

        /// <summary>
        /// Метод по выдаче расписаний рейсов
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebGet]
        FlightSchedule GetFlightSchedules();
    }
}
