using System;
using System.Web;

namespace SMServices.Sletat.DataModel
{
    /// <summary>
    /// Класс страны
    /// </summary>
    public class Country : IXmlCompatible
    {
        /// <summary>
        /// Id страны
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Название страны
        /// </summary>
        public string Name { get; set; }

        public string ToXml()
        {
            return String.Format(@"<country id=""{0}"" name=""{1}"" />", Id, HttpUtility.HtmlEncode(Name));
        }
    }
}
