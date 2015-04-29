using System;
using System.Web;
using System.Xml.Serialization;

namespace SMServices.Sletat.DataModel
{
    /// <summary>
    /// Курорт
    /// </summary>
    public class Resort : IXmlCompatible
    {
        /// <summary>
        /// Id курорта
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Название
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Ключ страны
        /// </summary>
        public int CountryId { get; set; }

        public string ToXml()
        {
            return String.Format(@"<resort id=""{0}"" name=""{1}"" countryId=""{2}"" />", Id, HttpUtility.HtmlEncode(Name), CountryId);
        }
    }
}
