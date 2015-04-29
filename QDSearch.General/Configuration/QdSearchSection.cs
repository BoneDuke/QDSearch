using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QDSearch.Configuration
{
    /// <summary>
    /// Класс отражающий настройки библиотеки в Web.config
    /// </summary>
    public class QdSearchSection : ConfigurationSection
    {
        /// <summary>
        /// Имя строки подключения к базе из коллекции строк в web.config. Задается в web.config.
        /// Используется для подключиения к основной базе MasterTour
        /// </summary>
        [ConfigurationProperty("mtMainDbConnectionStringName", IsRequired = true)]
        public string MtMainDbConnectionStringName
        {
            get { return (string)base["mtMainDbConnectionStringName"]; }
            set { base["mtMainDbConnectionStringName"] = value; }
        }

        /// <summary>
        /// Строка подключения к основной базе Мастер-Тур
        /// </summary>
        public string MtMainDbConnectionString
        {
            get
            {
                if (ConfigurationManager.ConnectionStrings.Count > 0 && ConfigurationManager.ConnectionStrings[MtMainDbConnectionStringName] != null)
                    return ConfigurationManager.ConnectionStrings[MtMainDbConnectionStringName].ToString();
                return String.Empty;
            }
        }

        /// <summary>
        /// Текст комманд выполняемые каждый раз при открытии соединения с основной базой Масетр-Тур. Задается в web.config.
        /// Например SET ANSI_WARNINGS ON; SET ARITHABORT ON;
        /// </summary>
        [ConfigurationProperty("mtMainDbCommandAfterOpenConnection", IsRequired = false)]
        public string MtMainDbCommandAfterOpenConnection
        {
            get { return (string)base["mtMainDbCommandAfterOpenConnection"]; }
            set { base["mtMainDbCommandAfterOpenConnection"] = value; }
        }
        
        /// <summary>
        /// Имя строки подключения к базе из коллекции строк в web.config. Задается в web.config.
        /// Используется для подключиения к поисковой базе MasterTour
        /// </summary>
        [ConfigurationProperty("mtSearchDbConnectionStringName", IsRequired = true)]
        public string MtSearchDbConnectionStringName
        {
            get { return (string)base["mtSearchDbConnectionStringName"]; }
            set { base["mtSearchDbConnectionStringName"] = value; }
        }

        /// <summary>
        /// Строка подключения к поисковой базе Мастер-Тур
        /// </summary>
        public string MtSearchDbConnectionString
        {
            get
            {
                if (ConfigurationManager.ConnectionStrings.Count > 0 && ConfigurationManager.ConnectionStrings[MtSearchDbConnectionStringName] != null)
                    return ConfigurationManager.ConnectionStrings[MtSearchDbConnectionStringName].ToString();
                return String.Empty;
            }
        }

        /// <summary>
        /// Текст комманд выполняемые каждый раз при открытии соединения с поисковой базой Мастер-Тур. Задается в web.config.
        /// Например SET ANSI_WARNINGS ON; SET ARITHABORT ON;
        /// </summary>
        [ConfigurationProperty("mtSearchDbCommandAfterOpenConnection", IsRequired = false)]
        public string MtSearchDbCommandAfterOpenConnection
        {
            get { return (string)base["mtSearchDbCommandAfterOpenConnection"]; }
            set { base["mtSearchDbCommandAfterOpenConnection"] = value; }
        }

        /// <summary>
        /// Имя строки подключения к базе из коллекции строк в web.config. Задается в web.config.
        /// Используется для подключиения к поисковой базе нового поиска
        /// </summary>
        [ConfigurationProperty("sftWebDbConnectionStringName", IsRequired = true)]
        public string SftWebDbConnectionStringName
        {
            get { return (string)base["sftWebDbConnectionStringName"]; }
            set { base["sftWebDbConnectionStringName"] = value; }
        }

        /// <summary>
        /// Строка подключения к поисковой базе нового поиска
        /// </summary>
        public string SftWebDbConnectionString
        {
            get
            {
                if (ConfigurationManager.ConnectionStrings.Count > 0 && ConfigurationManager.ConnectionStrings[SftWebDbConnectionStringName] != null)
                    return ConfigurationManager.ConnectionStrings[SftWebDbConnectionStringName].ToString();
                return String.Empty;
            }
        }

        /// <summary>
        /// Текст комманд выполняемые каждый раз при открытии соединения с поисковой базой нового поиска. Задается в web.config.
        /// Например SET ANSI_WARNINGS ON; SET ARITHABORT ON;
        /// </summary>
        [ConfigurationProperty("sftSearchDbCommandAfterOpenConnection", IsRequired = false)]
        public string SftSearchDbCommandAfterOpenConnection
        {
            get { return (string)base["sftSearchDbCommandAfterOpenConnection"]; }
            set { base["sftSearchDbCommandAfterOpenConnection"] = value; }
        }

        /// <summary>
        /// Устанавливает ключ города вылета. Задается в web.config.
        /// </summary>
        [ConfigurationProperty("HomeCityKey", IsRequired = false)]
        public int HomeCityKey
        {
            get { return (int)base["HomeCityKey"]; }
            set { base["HomeCityKey"] = value; }
        }

        /// <summary>
        /// Возвращает конфигурацию кэша для приложения
        /// </summary>
        [ConfigurationProperty("Cache", IsRequired = false)]
        public CacheElement Cache
        {
            get { return (CacheElement)base["Cache"]; }
            set { base["Cache"] = value; }
        }

        /// <summary>
        /// Возвращает настройку классов перелетов
        /// </summary>
        [ConfigurationProperty("CharterClasses", IsRequired = false, IsDefaultCollection = true)]
        public CharterClassesCollection CharterClasses
        {
            get { return (CharterClassesCollection)base["CharterClasses"]; }
            set { base["CharterClasses"] = value; }
        }

        private IDictionary<int, IEnumerable<int>> _charterClassesDictionary = null;

        /// <summary>
        /// Возвращает список классов перелетов
        /// </summary>
        public IDictionary<int, IEnumerable<int>> CharterClassesDictionary
        {
            get
            {
                if (_charterClassesDictionary == null)
                {
                    _charterClassesDictionary = new Dictionary<int, IEnumerable<int>>();
                    int i = 0;
                    foreach (var str in CharterClassesKeys.Split('|'))
                    {
                        var groups = new List<int>();
                        foreach (var gr in str.Split(','))
                        {
                            groups.Add(Int32.Parse(gr));
                        }
                        _charterClassesDictionary.Add(new KeyValuePair<int, IEnumerable<int>>(i, groups));
                        i++;
                    }
                }
                return _charterClassesDictionary;
            }
        }

        private string _charterClassesKeys = String.Empty;
        /// <summary>
        /// Возвращает список ключей классов перелетов, разделенных |
        /// </summary>
        public string CharterClassesKeys
        {
            get
            {
                if (_charterClassesKeys == String.Empty)
                {
                    _charterClassesKeys = CharterClasses.Cast<NameKeysElement>()
                        .Aggregate(String.Empty, (current, c) => String.Concat(current, c.Keys, "|"));
                    _charterClassesKeys = _charterClassesKeys.Remove(_charterClassesKeys.Length - 1);
                }
                return _charterClassesKeys;
            }
        }

        /// <summary>
        /// Возвращает конфигурацию обработки ошибок в приложении
        /// </summary>
        [ConfigurationProperty("exceptionHandling", IsRequired = false)]
        public ExceptionHandlingElement ExceptionHandling
        {
            get { return (ExceptionHandlingElement)base["exceptionHandling"]; }
            set { base["exceptionHandling"] = value; }
        }

        /// <summary>
        /// Возвращает конфигурацию фильтров для приложения
        /// </summary>
        [ConfigurationProperty("tourFilters", IsRequired = false)]
        public TourFiltersElement TourFilters
        {
            get { return (TourFiltersElement)base["tourFilters"]; }
            set { base["tourFilters"] = value; }
        }

        /// <summary>
        /// Возвращает конфигурацию ViewState для приложения
        /// </summary>
        [ConfigurationProperty("viewState", IsRequired = false)]
        public ViewStateElement ViewState
        {
            get { return (ViewStateElement)base["viewState"]; }
            set { base["viewState"] = value; }
        }        
        
    }
}
