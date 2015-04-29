using System;
using System.Web;
using System.Xml.Serialization;

namespace SMServices.Sletat.DataModel
{
    /// <summary>
    /// Питание
    /// </summary>
    public class Meal : IXmlCompatible
    {
        /// <summary>
        /// Id питания
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Навзание
        /// </summary>
        public string Name { get; set; }

        public string ToXml()
        {
            return String.Format(@"<meal id=""{0}"" name=""{1}"" />", Id, HttpUtility.HtmlEncode(Name));
        }
    }
}
