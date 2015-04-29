using System.Configuration;

namespace QDSearch.Configuration
{
    /// <summary>
    /// Настройка, показывающая классы перелетов
    /// </summary>
    public class CharterClassesCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// При переопределении в производном классе создает новый элемент <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </summary>
        /// <returns>
        /// Только что созданный объект <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new NameKeysElement();
        }

        /// <summary>
        /// При переопределении в производном классе возвращает ключ указанного элемента конфигурации.
        /// </summary>
        /// <returns>
        /// Объект <see cref="T:System.Object"/>, используемый в качестве ключа для указанного элемент <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </returns>
        /// <param name="element">Объект <see cref="T:System.Configuration.ConfigurationElement"/>, для которого возвращается ключ.</param>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((NameKeysElement)element).Name;
        }

        /// <summary>
        /// Возвращает NameKeysElement по его индексу в коллекции
        /// </summary>
        /// <param name="index"></param>
        /// <returns>NameKeysElement</returns>
        public NameKeysElement this[int index]
        {
            get { return (NameKeysElement)BaseGet(index); }
        }

        /// <summary>
        /// Возвращает NameKeysElement по его ключу в коллекции
        /// </summary>
        /// <param name="key">NameKeysElement</param>
        /// <returns></returns>
        public NameKeysElement this[object key]
        {
            get { return (NameKeysElement)BaseGet(key); }
        }

        /// <summary>
        /// Возвращает массив имен всех элементов NameKeysElement
        /// </summary>
        public string[] Names
        {
            get
            {
                var names = new string[Count];
                for (int i = 0; i < Count; i++)
                {
                    names[i] = this[i].Name;
                }
                return names;
            }
        }        
        
        /// <summary>
        /// Возвращает массив ключей всех елементов NameKeysElement
        /// </summary>
        public string[] Keys
        {
            get
            {
                var keys = new string[Count];
                for (int i = 0; i < Count; i++)
                {
                    keys[i] = this[i].Keys;
                }
                return keys;
            }
        }
    }
}
