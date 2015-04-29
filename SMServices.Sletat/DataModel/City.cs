using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Xml.Serialization;

namespace SMServices.Sletat.DataModel
{
    /// <summary>
    /// Класс города
    /// </summary>
    public class City : IXmlCompatible
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Название города
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Страны прилета
        /// </summary>
        public List<CountryTo> CountriesTo { get; set; }

        public string ToXml()
        {
            var sb = new StringBuilder();
            sb.AppendFormat(@"<city id=""{0}"" name=""{1}"" >", Id, HttpUtility.HtmlEncode(Name));
            CountriesTo.ForEach(c => sb.Append(c.ToXml()));
            sb.Append(@"</city>");
            return sb.ToString();
        }
    }
}
