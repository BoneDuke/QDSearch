using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SMServices.Sletat.DataModel
{
    public class ActualizedTour : IXmlCompatible
    {
        public ActualizedTour()
        {
            Services = new List<Service>();
        }

        public int Price { get; set; }
        public int HotelIsInStop { get; set; }
        public int TicketsIsIncluded { get; set; }
        public int HasEconomTicketsDpt { get; set; }
        public int HasEconomTicketsRtn { get; set; }
        public int HasBusinessTicketsDpt { get; set; }
        public int HasBusinessTicketsRtn { get; set; }
        public int? FewPlacesInHotel { get; set; }

        public int? FewEconomTicketsDpt { get; set; }
        public int? FewEconomTicketsRtn { get; set; }
        public int? FewBusinessTicketsDpt { get; set; }
        public int? FewBusinessTicketsRtn { get; set; }

        public string TourUrl { get; set; }

        public List<Service> Services { get; set; }
        public string ToXml()
        {
            var sb = new StringBuilder();
            sb.AppendFormat(@"<actualizedTour price=""{0}"" ticketsIsIncluded=""{1}"" hotelIsInStop=""{2}"" hasEconomTicketsDpt=""{3}"" hasEconomTicketsRtn=""{4}""
hasBusinessTicketsDpt=""{5}"" hasBusinessTicketsRtn=""{6}"" fewPlacesInHotel=""{7}"" fewEconomTicketsDpt=""{8}"" fewEconomTicketsRtn=""{9}"" 
fewBusinessTicketsDpt=""{10}"" fewBusinessTicketsRtn=""{11}"" tourUrl=""{12}"" ><services>",
                                                                                            Price,
                                                                                            TicketsIsIncluded,
                                                                                            HotelIsInStop,
                                                                                            HasEconomTicketsDpt,
                                                                                            HasEconomTicketsRtn,
                                                                                            HasBusinessTicketsDpt,
                                                                                            HasBusinessTicketsRtn,
                                                                                            FewPlacesInHotel,
                                                                                            FewEconomTicketsDpt,
                                                                                            FewEconomTicketsRtn,
                                                                                            FewBusinessTicketsDpt,
                                                                                            FewBusinessTicketsRtn,
                                                                                            HttpUtility.HtmlEncode(TourUrl));
            Services.ForEach(c => sb.Append(c.ToXml()));
            sb.Append(@"</services></actualizedTour>");
            return sb.ToString();
        }
    }
}
