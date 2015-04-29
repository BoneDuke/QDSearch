using System;
using System.Configuration;
using QDSearch.ViewState;

namespace QDSearch.Configuration
{
    /// <summary>
    /// Отвечает за конфигурацию ViewState в Web.config
    /// </summary>
    public class ViewStateElement : ConfigurationElement
    {
        /// <summary>
        /// Возвращает задает место хранения ViewState в Web.config
        /// Варианты Default|InPage|InSession|InMsSql
        /// </summary>
        [ConfigurationProperty("mode", DefaultValue = "Default", IsRequired = false)]
        public ViewStateModes ViewStateMode
        {
            get
            {
                return (ViewStateModes)base["mode"];
            }
            set
            {
                base["mode"] = value;
            }
        }
        /// <summary>
        /// Возвращает задает использовать кэш при хранении данных в Sql
        /// </summary>
        [ConfigurationProperty("useCacheForSqlMode", DefaultValue = "false", IsRequired = false)]
        //[RegexStringValidator(@"true|false")]
        public bool UseCacheForSqlMode
        {
            get
            {
                return (bool)base["useCacheForSqlMode"];
            }
            set
            {
                base["useCacheForSqlMode"] = value;
            }
        }
        /// <summary>
        /// Возвращает задает использовать сжатие при перед сохранением ViewState хранилище.
        /// </summary>
        [ConfigurationProperty("useCompression", DefaultValue = "false", IsRequired = false)]
        //[RegexStringValidator(@"true|false")]
        public bool UseCompression
        {
            get
            {
                return (bool)base["useCompression"];
            }
            set
            {
                base["useCompression"] = value;
            }
        }
        /// <summary>
        /// Применять заданный режим хранения ViewState избирательно, 
        /// т.е. только к указанным страницам. 
        /// Все остальные страницы будут хранить ViewState по умолчанию т.е. в HiddenField на странице
        /// </summary>
        [ConfigurationProperty("applayByCustomPageOnly", DefaultValue = "false", IsRequired = false)]
        //[RegexStringValidator(@"true|false")]
        public bool ApplayByCustomPageOnly
        {
            get
            {
                return (bool)base["applayByCustomPageOnly"];
            }
            set
            {
                base["applayByCustomPageOnly"] = value;
            }
        }

        /// <summary>
        /// Задает таймаут для Page State в минутах в web.config
        /// </summary>
        [ConfigurationProperty("timeout", DefaultValue = "60", IsRequired = false)]
        public uint Timeout
        {
            get
            {
                return (uint)this["timeout"];
            }
            set
            {
                this["timeout"] = value;
            }
        }

        /// <summary>
        /// Имя строки подключения к базе из коллекции строк в web.config. Задается в web.config.
        /// Используется для подключиения к базе с ViewState если mode=InMsSql
        /// </summary>
        [ConfigurationProperty("pageStateDbConnectionStringName", IsRequired = false)]
        public string PageStateDbConnectionStringName
        {
            get
            {
                return (string)base["pageStateDbConnectionStringName"];
            }
            set { base["pageStateDbConnectionStringName"] = value; }
        }
        /// <summary>
        /// Стрка подключения к БД для работы с PageState
        /// </summary>
        public string PageStateDbConnectionString
        {
            get
            {
                if (ConfigurationManager.ConnectionStrings.Count > 0 && ConfigurationManager.ConnectionStrings[PageStateDbConnectionStringName] != null)
                    return ConfigurationManager.ConnectionStrings[PageStateDbConnectionStringName].ToString();
                return String.Empty;
            }
        }
    }
}
