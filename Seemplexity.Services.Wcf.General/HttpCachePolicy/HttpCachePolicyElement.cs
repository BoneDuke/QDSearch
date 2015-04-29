using System;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.Web;

namespace Seemplexity.Services.Wcf.HttpCachePolicy
{
    public class HttpCachePolicyElement : BehaviorExtensionElement
    {
        /// <summary>
        /// The element specifies cache-related HTTP headers that http endpoint sends to Web clients,
        ///  which control how Web clients and proxy servers will cache the content.
        /// </summary>
        public override Type BehaviorType
        {
            get { return typeof(HttpCachePolicyBehavior); }
        }

        protected override object CreateBehavior()
        {
            var behavior = new HttpCachePolicyBehavior
            {
                CacheControlMode = CacheControlMode,
                CacheControlMaxAge = CacheControlMaxAge,
                HttpExpires = HttpExpires,
                CacheControlCustom = CacheControlCustom
            };
            return behavior;
        }

        [ConfigurationProperty("cacheControlMode", IsRequired = true)]
        public CacheControlModes CacheControlMode
        {
            get
            {
                return (this["cacheControlMode"] as CacheControlModes?) ?? CacheControlModes.NoControl;
            }
            set
            {
                this["cacheControlMode"] = value;
            }
        }

        [ConfigurationProperty("cacheControlMaxAge", IsRequired = false)]
        public TimeSpan CacheControlMaxAge
        {
            get
            {
                return (this["cacheControlMaxAge"] as TimeSpan?) ?? TimeSpan.Zero;
            }
            set
            {
                this["cacheControlMaxAge"] = value;
            }
        }

        [ConfigurationProperty("httpExpires", IsRequired = false)]
        public DateTime? HttpExpires
        {
            get
            {
                return this["httpExpires"] as DateTime?;
            }
            set
            {
                this["httpExpires"] = value;
            }
        }

        [ConfigurationProperty("cacheControlCustom", IsRequired = false)]
        public string CacheControlCustom
        {
            get
            {
                return this["cacheControlCustom"] as string;
            }
            set
            {
                this["cacheControlCustom"] = value;
            }
        }
    }
}
