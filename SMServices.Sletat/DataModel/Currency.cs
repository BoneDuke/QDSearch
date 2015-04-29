using System;
using System.Web;

namespace SMServices.Sletat.DataModel
{
    /// <summary>
    /// Валюта
    /// </summary>
    public class Currency : IXmlCompatible
    {
        /// <summary>
        /// Ключ валюты
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Нпазвание валюты
        /// </summary>
        public string Name { get; set; }

        public string ToXml()
        {
            return String.Format(@"<currency id=""{0}"" name=""{1}"" />", Id, HttpUtility.HtmlEncode(Name));
        }
    }
}
