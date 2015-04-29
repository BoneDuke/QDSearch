using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QDSearch.DataModel;
using QDSearch.Extensions;
using QDSearch.Repository.MtMain;
using QDSearch.Repository.MtSearch;

namespace Seemplexity.Logic.Basket.DataModel
{
    public class PriceInfo
    {
        public PriceInfo()
        {
            Services = new List<ServiceInfo>();
            Flights = new List<FlightInfo>();
        }
        /// <summary>
        /// Список услуг, входящих в тур, с их параметрами (кроме перелетов)
        /// </summary>
        public List<ServiceInfo> Services { get; set; }
        /// <summary>
        /// Список возможных перелетов
        /// </summary>
        public List<FlightInfo> Flights { get; set; }
        /// <summary>
        /// Тур
        /// </summary>
        public TP_Tour Tour { get; set; }
        /// <summary>
        /// Дата начала тура
        /// </summary>
        public DateTime TourDateBegin { get; set; }
        /// <summary>
        /// Дата окончания тура
        /// </summary>
        public DateTime TourDateEnd { get; set; }

        public List<Tuple<HotelSmallClass, Pansion>> GetTourHotelsAndPansions
        {
            get
            {
                var result = new List<Tuple<HotelSmallClass, Pansion>>();

                using (var searchDc = new MtSearchDbDataContext())
                {
                    foreach (var sf in Services.Select(s => s).Where(s => s.ServiceClass == ServiceClass.Hotel).OrderBy(s => s.Day))
                    {
                        string hashOut;
                        var hotels = searchDc.GetHotelsByKeys(new []{sf.Code}, out hashOut);

                        Pansion pansion = null;
                        if (sf.SubCode2.HasValue && hotels != null && hotels.Count > 0)
                            pansion = searchDc.GetPansionByKey(sf.SubCode2.Value);
                        result.Add(new Tuple<HotelSmallClass, Pansion>(hotels[0], pansion));
                    }
                }
                return result;
            }
        }
    }
}
