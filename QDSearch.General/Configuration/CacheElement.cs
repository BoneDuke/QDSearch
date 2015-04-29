using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using QDSearch.Caching;
using QDSearch.DataModel;

namespace QDSearch.Configuration
{
    /// <summary>
    /// Отвечает за конфигурацию дополнительных параметров кэша для приложения
    /// </summary>
    public class CacheElement : ConfigurationElement
    {
        /// <summary>
        /// Включает/выключает кэш.
        /// По умолчанию выключен.
        /// </summary>
        [ConfigurationProperty("Enabled", DefaultValue = false, IsRequired = false)]
        public bool Enabled
        {
            get { return (bool)this["Enabled"]; }
            set { this["Enabled"] = value; }
        }

        /// <summary>
        /// Время с момента занисения элемента в кэш для элементов которые практически не меняются, в течении которого элемент считается актуальным.
        /// В секундах. По умолчанию 1800 сек.
        /// Значение 0 означает бесконечность (NoAbsoluteExpiration = true), т.е. пока не удалят.
        /// </summary>
        [ConfigurationProperty("LongCacheTimeout", DefaultValue = "1800", IsRequired = false)]
        public uint LongCacheTimeout
        {
            get { return (uint)this["LongCacheTimeout"]; }
            set { this["LongCacheTimeout"] = value; }
        }

        /// <summary>
        /// Время с момента занисения элемента в кэш для элементов которые меняются, но редко, в течении которого элемент считается актуальным.
        /// В секундах. По умолчанию 600 сек.
        /// Значение 0 означает бесконечность (NoAbsoluteExpiration = true), т.е. пока не удалят.
        /// </summary>
        [ConfigurationProperty("MediumCacheTimeout", DefaultValue = "600", IsRequired = false)]
        public uint MediumCacheTimeout
        {
            get { return (uint)this["MediumCacheTimeout"]; }
            set { this["MediumCacheTimeout"] = value; }
        }

        /// <summary>
        /// Время с момента занисения элемента в кэш для элементов которые меняются часто, в течении которого элемент считается актуальным.
        /// В секундах. По умолчанию 180 сек.
        /// Значение 0 означает бесконечность (NoAbsoluteExpiration = true), т.е. пока не удалят.
        /// </summary>
        [ConfigurationProperty("ShortCacheTimeout", DefaultValue = "180", IsRequired = false)]
        public uint ShortCacheTimeout
        {
            get { return (uint)this["ShortCacheTimeout"]; }
            set { this["ShortCacheTimeout"] = value; }
        }

        /// <summary>
        /// Способ хранения кэша
        /// </summary>
        [ConfigurationProperty("StorageMode", DefaultValue = "AspNet", IsRequired = false)]
        public StorageModes StorageMode
        {
            get { return (StorageModes)this["StorageMode"]; }
            set { this["StorageMode"] = value; }
        }

        /// <summary>
        /// Способ устаревания кэша
        /// </summary>
        [ConfigurationProperty("ExpirationMode", DefaultValue = "Absolute", IsRequired = false)]
        public ExpirationModes ExpirationMode
        {
            get { return (ExpirationModes)this["ExpirationMode"]; }
            set { this["ExpirationMode"] = value; }
        }
    }
}
