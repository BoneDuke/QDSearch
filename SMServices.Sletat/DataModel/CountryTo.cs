using System;

namespace SMServices.Sletat.DataModel
{
    /// <summary>
    /// Страна прилета
    /// </summary>
    public class CountryTo : IXmlCompatible
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        public string ToXml()
        {
            return String.Format(@"<countryTo id=""{0}"" />", Id);
        }
    }
}
