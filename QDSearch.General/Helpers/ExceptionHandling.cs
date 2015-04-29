using System;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Net.Mail;

namespace QDSearch.Helpers
{

    /// <summary>
    /// Статический класс реализующий хелперы по обработке ошибок в веб приложении
    /// </summary>
    public static class ExceptionHandling
    {
        private readonly static object SyncObject = new object();

        /// <summary>
        /// Реализует отправку последней ошибки с сервера
        /// </summary>
        public static void SendErrorMessage()
        {
            SendErrorMessage(HttpContext.Current.Server.GetLastError());
        }
        /// <summary>
        /// Реализует отправку ошибки в сообщении.
        /// </summary>
        /// <param name="ex">Ошибка сообщение о которой нужно отправить</param>
        public static void SendErrorMessage(Exception ex)
        {
            SendErrorMessage(GetExceptionMsg(ex));
        }

        private static string GetExceptionMsg(Exception ex)
        {
            String errMessage = String.Empty;
            var request = HttpContext.Current.Request;
            errMessage += String.Format("\n__pingStamp: {0}\n",
                request.Params.AllKeys.Contains("__pingStamp")
                    ? request["__pingStamp"].ToString(CultureInfo.InvariantCulture)
                    : "не найден");
            errMessage += request.Params.AllKeys.Contains("__VIEWSTATE")
                ? "\n__VIEWSTATE есть"
                : "\n__VIEWSTATE нет";            
            errMessage += request.Params.AllKeys.Contains("__EVENTTARGET")
                ? "\n__EVENTTARGET есть\n"
                : "\n__EVENTTARGET нет\n";
            if (ex == null)
                return errMessage;
            errMessage += String.Format("\nMESSAGE: {0}\n\nEXCEPTION: {1}", ex.Message, ex);
            if (ex.InnerException != null)
            {
                errMessage += "\nInnerException:\n\n";
                errMessage += GetExceptionMsg(ex.InnerException);
            }
            return errMessage;
        }

        /// <summary>
        /// Реализует отправку сообщения об ошибке.
        /// </summary>
        /// <param name="errorMessage">Непосредственно само сообщение.</param>
        public static void SendErrorMessage(string errorMessage)
        {
            lock (SyncObject)
            {
                HttpRequest request = HttpContext.Current.Request;
                // Собираем необходимые данные
                string errMessage = String.Format("Main Error\nDate & Time: {0}\nURL: {1}\nURL Referrer: {2}\nQUERY: {3}\nBROWSER: {4}\nBROWSER Ver.: {5}\nBROWSER IsMobileDevice: {6}\nBROWSER IsMobileDevice: {7} {8}\nBROWSER JS: {9}\nBROWSER JS Ver: {10}\nBROWSER Platform: {11}\nHttpMethod: {12}\nUserAgent: {13}\nUserHostAddress: {14}\nUserHostName: {15}",
                                                  DateTime.Now.ToString("F"), request.Path, request.UrlReferrer, request.QueryString, request.Browser.Browser, request.Browser.Version, request.Browser.IsMobileDevice, request.Browser.MobileDeviceManufacturer, request.Browser.MobileDeviceModel,
                                                  request.Browser.JavaApplets, request.Browser.JScriptVersion, request.Browser.Platform, request.HttpMethod, request.UserAgent, request.UserHostAddress, request.UserHostName);

                //Добавляем информацию о пользователе, в случае если успешно прошел процесс аутентификации
                if (HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    errMessage = String.Format("{0}\nUser: {1}", errMessage, HttpContext.Current.User.Identity.Name);
                }

                errMessage = String.Format("{0}\n\n\n{1}", errMessage, errorMessage);

                //System.Web.Mail.MailMessage mail = new System.Web.Mail.MailMessage();
                var mail = new MailMessage();
                mail.To.Add(Globals.Settings.ExceptionHandling.EmailsTo);
                mail.Subject = Globals.Settings.ExceptionHandling.EmailSubject;
                mail.Priority = MailPriority.High;
                mail.IsBodyHtml = false;
                mail.Body = errMessage;

                // Здесь необходимо указать используемый SMTP сервер
                //System.Web.Mail.SmtpMail.SmtpServer = "mail.solvex.local";
                //System.Web.Mail.SmtpMail.Send(mail);
                new SmtpClient().Send(mail);

            }
        }
    }
}
