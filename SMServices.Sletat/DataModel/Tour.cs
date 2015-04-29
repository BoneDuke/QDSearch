using System;
using System.Globalization;
using System.Web;

namespace SMServices.Sletat.DataModel
{
    public class Tour : IXmlCompatible
    {
        public long OfferId { get; set; }
        public string TourName { get; set; }
        public int HotelId { get; set; }
        public string HotelUrl { get; set; }
        public int ResortId { get; set; }
        public int HotelCategoryId { get; set; }
        public int MealId { get; set; }
        public string HtPlaceName { get; set; }
        public string RoomTypeName { get; set; }
        public DateTime TourDate { get; set; }
        public int Nights { get; set; }
        public int Price { get; set; }
        public int HotelInStop { get; set; }
        public int TicketsIncluded { get; set; }
        public int HasEconomTicketsDpt { get; set; }
        public int HasEconomTicketsRtn { get; set; }
        public int HasBusinessTicketsDpt { get; set; }
        public int HasBusinessTicketsRtn { get; set; }
        public string TourUrl { get; set; }
        public string SpoUrl { get; set; }
        public int? FewPlacesInHotel { get; set; }
        public int? FewTicketsDptY { get; set; }
        public int? FewTicketsRtnY { get; set; }
        public int? FewTicketsDptB { get; set; }
        public int? FewTicketsRtnB { get; set; }
        public long Flags { get; set; }
        public string Description { get; set; }
        public string ReceivingParty { get; set; }
        public string EarlyBookingValidTill { get; set; }
        public string ToXml()
        {
            return
                string.Format(
                    @"<tour offerId=""{0}"" tourName=""{1}"" hotelId=""{2}"" hotelUrl=""{3}"" resortId=""{4}"" hotelCategoryId=""{5}"" mealId=""{6}"" htPlaceName=""{7}""
roomTypeName=""{8}"" tourDate=""{9:dd.MM.yyyy}"" nights=""{10}"" price=""{11}"" hotelIsInStop=""{12}"" ticketsIncluded=""{13}"" hasEconomTicketsDpt=""{14}"" hasEconomTicketsRtn=""{15}"" hasBusinessTicketsDpt=""{16}"" 
hasBusinessTicketsRtn=""{17}"" tourUrl=""{18}"" spoUrl=""{19}"" fewPlacesInHotel=""{20}"" fewTicketsDptY=""{21}"" fewTicketsRtnY=""{22}"" fewTicketsDptB=""{23}"" fewTicketsRtnB=""{24}"" flags=""{25}""
description=""{26}"" receivingParty=""{27}"" earlyBookingValidTill=""{28}"" />",
                                                                               OfferId,
                                                                               HttpUtility.HtmlEncode(TourName),
                                                                               HotelId,
                                                                               HttpUtility.HtmlEncode(HotelUrl),
                                                                               ResortId,
                                                                               HotelCategoryId,
                                                                               MealId,
                                                                               HttpUtility.HtmlEncode(HtPlaceName),
                                                                               HttpUtility.HtmlEncode(RoomTypeName),
                                                                               TourDate,
                                                                               Nights,
                                                                               Price,
                                                                               HotelInStop,
                                                                               TicketsIncluded,
                                                                               HasEconomTicketsDpt,
                                                                               HasEconomTicketsRtn,
                                                                               HasBusinessTicketsDpt,
                                                                               HasBusinessTicketsRtn,
                                                                               HttpUtility.HtmlEncode(TourUrl),
                                                                               HttpUtility.HtmlEncode(SpoUrl),
                                                                               FewPlacesInHotel,
                                                                               FewTicketsDptY,
                                                                               FewTicketsRtnY,
                                                                               FewTicketsDptB,
                                                                               FewTicketsRtnB,
                                                                               Flags,
                                                                               HttpUtility.HtmlEncode(Description),
                                                                               HttpUtility.HtmlEncode(ReceivingParty),
                                                                               EarlyBookingValidTill);
        }
    }
}
