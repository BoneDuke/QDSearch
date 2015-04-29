using System.Configuration;

namespace QDSearch.Configuration
{
    /// <summary>
    /// Элмент файла конфига типа "Name" "Keys"
    /// </summary>
    public class NameKeysElement : ConfigurationElement 
    {
        /// <summary>
        /// Свойство параметра Name
        /// </summary>
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name 
        {
            get { return (string) base["name"]; }
            set { base["Name"] = value; }
        }

        /// <summary>
        /// Свойство параметра Keys
        /// </summary>
        [ConfigurationProperty("keys", IsRequired = true)]
        public string Keys 
        {
            get { return (string)base["keys"]; }
            set { base["Keys"] = value; }
        } 
   }
}
