using System;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.ServiceModel.Web;

namespace Seemplexity.Services.Wcf.WebHttpExtended
{
    /// <summary>
    /// Enables the WebHttpExtendedBehavior for an endpoint through configuration.
    /// </summary>
    public class WebHttpExtendedElement : BehaviorExtensionElement
    {
        ConfigurationPropertyCollection _properties;


        /// <summary>
        /// Gets or sets a value that indicates whether help is enabled.
        /// </summary>
        [ConfigurationProperty(WebConfigurationStrings.HelpEnabled)]
        public bool HelpEnabled
        {
            get { return (bool)base[WebConfigurationStrings.HelpEnabled]; }
            set { base[WebConfigurationStrings.HelpEnabled] = value; }
        }

        /// <summary>
        /// Gets and sets the default message body style.
        /// </summary>
        [ConfigurationProperty(WebConfigurationStrings.DefaultBodyStyle)]
        [InternalEnumValidator(typeof(WebMessageBodyStyleHelper))]
        public WebMessageBodyStyle DefaultBodyStyle
        {
            get { return (WebMessageBodyStyle)base[WebConfigurationStrings.DefaultBodyStyle]; }
            set { base[WebConfigurationStrings.DefaultBodyStyle] = value; }
        }

        /// <summary>
        /// Gets and sets the default outgoing response format.
        /// </summary>
        [ConfigurationProperty(WebConfigurationStrings.DefaultOutgoingResponseFormat)]
        [InternalEnumValidator(typeof(WebMessageFormatHelper))]
        public WebMessageFormat DefaultOutgoingResponseFormat
        {
            get { return (WebMessageFormat)base[WebConfigurationStrings.DefaultOutgoingResponseFormat]; }
            set { base[WebConfigurationStrings.DefaultOutgoingResponseFormat] = value; }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the message format can be automatically selected.
        /// </summary>
        [ConfigurationProperty(WebConfigurationStrings.AutomaticFormatSelectionEnabled)]
        public bool AutomaticFormatSelectionEnabled
        {
            get { return (bool)base[WebConfigurationStrings.AutomaticFormatSelectionEnabled]; }
            set { base[WebConfigurationStrings.AutomaticFormatSelectionEnabled] = value; }
        }

        /// <summary>
        /// Gets or sets the flag that specifies whether a FaultException is generated when an internal server error (HTTP status code: 500) occurs.
        /// </summary>
        [ConfigurationProperty(WebConfigurationStrings.FaultExceptionEnabled)]
        public bool FaultExceptionEnabled
        {
            get { return (bool)base[WebConfigurationStrings.FaultExceptionEnabled]; }
            set { base[WebConfigurationStrings.FaultExceptionEnabled] = value; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (_properties == null)
                {
                    var properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty(WebConfigurationStrings.HelpEnabled, typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(WebConfigurationStrings.DefaultBodyStyle, typeof(WebMessageBodyStyle), WebMessageBodyStyle.Bare, null, new InternalEnumValidator(typeof(WebMessageBodyStyleHelper)), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(WebConfigurationStrings.DefaultOutgoingResponseFormat, typeof(WebMessageFormat), WebMessageFormat.Xml, null, new InternalEnumValidator(typeof(WebMessageFormatHelper)), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(WebConfigurationStrings.AutomaticFormatSelectionEnabled, typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty(WebConfigurationStrings.FaultExceptionEnabled, typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    _properties = properties;
                }
                return _properties;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Configuration", "Configuration102:ConfigurationPropertyAttributeRule", MessageId = "System.ServiceModel.Configuration.WebHttpElement.BehaviorType", Justification = "Not a configurable property; a property that had to be overridden from abstract parent class")]
        public override Type BehaviorType
        {
            get { return typeof(WebHttpExtendedBehavior); }
        }

        protected override object CreateBehavior()
        {
            return new WebHttpExtendedBehavior
            {
                HelpEnabled = HelpEnabled,
                DefaultBodyStyle = DefaultBodyStyle,
                DefaultOutgoingResponseFormat = DefaultOutgoingResponseFormat,
                AutomaticFormatSelectionEnabled = AutomaticFormatSelectionEnabled,
                FaultExceptionEnabled = FaultExceptionEnabled,
            };
        }
    }
}
