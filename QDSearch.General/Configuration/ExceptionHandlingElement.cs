using System.Configuration;

namespace QDSearch.Configuration
{
    /// <summary>
    /// ОТвечает за конфигурацию параметров обработки ошибок в Web.config
    /// </summary>
    public class ExceptionHandlingElement : ConfigurationElement
    {
        /// <summary>
        /// Включает/выключает отправку сообщений об ошибках на email.
        /// По умолчанию выключен.
        /// </summary>
        [ConfigurationProperty("sendExceptionEmails", DefaultValue = false, IsRequired = false)]
        public bool SendExceptionEmails
        {
            get { return (bool)this["sendExceptionEmails"]; }
            set { this["sendExceptionEmails"] = value; }
        }

        /// <summary>
        /// Почтовые ящики получателей сообщений с ошибками.
        /// В качестве разделителя используется ","
        /// По умолчанию pashkov@solvex.travel
        /// </summary>
        [ConfigurationProperty("emailsTo", DefaultValue = "pashkov@solvex.travel", IsRequired = false)]
        public string EmailsTo
        {
            get { return (string)this["emailsTo"]; }
            set { this["emailsTo"] = value; }
        }

        /// <summary>
        /// Почтовые ящики получателей сообщений с ошибками.
        /// По умолчанию pashkov@solvex.travel
        /// </summary>
        [ConfigurationProperty("emailSubject", DefaultValue = "pashkov@solvex.travel", IsRequired = false)]
        public string EmailSubject
        {
            get { return (string)this["emailSubject"]; }
            set { this["emailSubject"] = value; }
        }
    }
}
