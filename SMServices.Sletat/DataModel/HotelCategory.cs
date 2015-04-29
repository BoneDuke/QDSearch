using System;
using System.Web;
using System.Xml.Serialization;

namespace SMServices.Sletat.DataModel
{
    /// <summary>
    /// Класс категории отеля
    /// </summary>
    public class HotelCategory : IXmlCompatible
    {
        /// <summary>
        /// Id категории
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Название категории
        /// </summary>
        public string Name { get; set; }

        public string ToXml()
        {
            return String.Format(@"<hotelCategory id=""{0}"" name=""{1}"" />", Id, HttpUtility.HtmlEncode(Name));
        }
    }
}
