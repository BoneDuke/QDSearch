using System;
using System.Web;
using System.Web.Configuration;
using QDSearch.Caching;
using QDSearch.Configuration;

namespace QDSearch
{
    /// <summary>
    /// Глобальный статический класс приложения содержит в себе доступ к глобальным парамтерам приложения.
    /// </summary>
    public static class Globals
    {
        private static string _customConfigurationSectionName;

        /// <summary>
        /// Имя раздела конфигурации библиотеки. По умолчанию - QDSSection
        /// </summary>
        public static string CustomConfigurationSectionName
        {
            get
            {
                if (String.IsNullOrEmpty(_customConfigurationSectionName))
                    _customConfigurationSectionName = "QDSSection";
                return _customConfigurationSectionName;
            }
            set { _customConfigurationSectionName = value; }
        }

        /// <summary>
        /// Задает место хранения Cache в памяти или на IIS
        /// Определяется автоматически в зависимости от того куда загружается dll,
        /// в Win прилолжение или IIS
        /// Варианты AspNet|Memory|Redis
        /// </summary>
        public static StorageModes CacheMode
        {
            get
            {
                // необходимо для тестирования Wcf и других решений из под консольных приложений. 
                // ниже определяем из под чего загружена dll и в зависимости от этого определяем вариант использования кеша.
                if (HttpRuntime.AppDomainAppId != null)
                {
                    //is web app
                    return Settings.Cache.StorageMode;
                }
                //is windows app
                return StorageModes.Memory;
            }
        }

        /// <summary>
        /// Дополнительные настройки приложения
        /// </summary>
        public readonly static QdSearchSection Settings = (QdSearchSection)WebConfigurationManager.GetSection(CustomConfigurationSectionName);
    }
}
