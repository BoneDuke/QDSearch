using System;
using System.Web;
using System.Xml.Serialization;

namespace SMServices.Sletat.DataModel
{
    /// <summary>
    /// Класс отеля
    /// </summary>
    public class Hotel : IXmlCompatible
    {
        /// <summary>
        /// Ключ отеля
        /// </summary>
        [XmlAttribute]
        public int Id { get; set; }
        /// <summary>
        /// Название отеля
        /// </summary>
        [XmlAttribute]
        public string Name { get; set; }
        /// <summary>
        /// Ключ категории отеля
        /// </summary>
        [XmlAttribute]
        public int HotelCategoryId { get; set; }
        /// <summary>
        /// Ключ курорта
        /// </summary>
        [XmlAttribute]
        public int ResortId { get; set; }

        public string ToXml()
        {
            return String.Format(@"<hotel id=""{0}"" name=""{1}"" hotelCategoryId=""{2}"" resortId=""{3}"" />", Id, HttpUtility.HtmlEncode(Name), HotelCategoryId, ResortId);
        }
    }
}
