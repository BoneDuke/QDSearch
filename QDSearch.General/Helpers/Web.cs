using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace QDSearch.Helpers
{
    /// <summary>
    /// Статинчный класс, содержит хелперы для работы с вебом.
    /// </summary>
    public static class Web
    {
        /// <summary>
        /// Получает полный url адрес из HttpRequest без строки запроса 
        /// </summary>
        /// <returns>полный url текущего запроса без параметров</returns>
        public static string GetPageUrl()
        {
            var request = HttpContext.Current.Request;
            return String.IsNullOrWhiteSpace(request.Url.Query)
                ? request.Url.AbsoluteUri
                : request.Url.AbsoluteUri.Remove(request.Url.AbsoluteUri.IndexOf('?'),
                    request.Url.Query.Length);
        }

        /// <summary>
        /// добавляет ссылку на CSS файл для указанного контрола
        /// </summary>
        /// <param name="control">контрол для которого добавляется ссылка на CSS</param>
        /// <param name="url">url css файла</param>
        /// <exception cref="ArgumentNullException">Ошибка если один из параметров NULL</exception>
        public static void AddStyleRef(Control control, string url)
        {
            if (control == null) throw new ArgumentNullException("control");
            if (String.IsNullOrWhiteSpace(url)) throw new ArgumentNullException("url");

            string id = String.Format("{0}css", control.ID);
            if (control.Page.Header.FindControl(id) == null)
            {
                var cssLink = new HtmlLink { ID = id, Href = control.ResolveUrl(url) };
                cssLink.Attributes.Add("type", @"text/css");
                cssLink.Attributes.Add("rel", "stylesheet");
                control.Page.Header.Controls.Add(cssLink);
            }
        }

        /// <summary>
        /// Выводит сообщение на клиенте
        /// </summary>
        /// <param name="control">контрол для которого выводится сообщение, необходимо для корректного вывода в случае использования Ajax</param>
        /// <param name="message">текст сообщения</param>
        /// <exception cref="ArgumentNullException">Ошибка если один из параметров NULL</exception>
        public static void ShowMessage(Control control, string message)
        {
            if (control == null) throw new ArgumentNullException("control");
            if (String.IsNullOrWhiteSpace(message)) throw new ArgumentNullException("message");

            RegisterStartupScript(control, String.Format(@"alert('{0}');", message), false);
        }

        /// <summary>
        /// Выводит сообщение на клиенте с дальнейшей переадрисацией на указаный адрес
        /// </summary>
        /// <param name="control">контрол для которого выводится сообщение, необходимо для корректного вывода в случае использования Ajax</param>
        /// <param name="message">текст сообщения</param>
        /// <param name="targetUrl">URL страницы для перехода</param>
        /// <exception cref="ArgumentNullException">Ошибка если один из параметров NULL</exception>
        public static void ShowMessageWithRedirect(Control control, string message, string targetUrl)
        {
            if (control == null) throw new ArgumentNullException("control");
            if (String.IsNullOrWhiteSpace(message)) throw new ArgumentNullException("message");
            if (String.IsNullOrWhiteSpace(targetUrl)) throw new ArgumentNullException("targetUrl");

            RegisterStartupScript(control, String.Format(@"alert('{0}');window.location.href = '{1}'", message, targetUrl), false);
        }

        /// <summary>
        /// Выполняет прокрутку страницы до указанного елемента. Полезна когда елемент находится вне зоны видимости.
        /// </summary>
        /// <param name="parentControl">Контрол содержащий искомый элемент.</param>
        /// <param name="clientId">Id элемента на стороне клиента до которого нужно выполнить прокрутку.</param>
        public static void ScrollToElement(Control parentControl, string clientId)
        {
            // JavaScript метот выполняющий эту прокрутку. Должен быть включен на страницу либо через файл либо непосредственно.
            //function ScrollToElement(elementId) {
            //$('html, body').animate({ scrollTop: $(elementId).offset().top }, 2000); }

            if (parentControl == null) throw new ArgumentNullException("parentControl");
            if (String.IsNullOrWhiteSpace(clientId)) throw new ArgumentNullException("clientId");

            RegisterStartupScript(parentControl, String.Format(@"ScrollToElement('#{0}');", clientId), true);
        }

        /// <summary>
        /// Регистрирует скрипт который будет выполнен после полной загрузки страницы
        /// </summary>
        /// <param name="control">Control для которого выполняется регистрация клиентского скрипта.</param>
        /// <param name="script">Сам скрипт</param>
        /// <param name="useAddLoad">Оборачивать вызов скрипта в Sys.Application.add_load(script) или нет</param>
        /// <exception cref="ArgumentNullException">Генерируется в случае если один из параметров не инициализирова</exception>
        public static void RegisterStartupScript(Control control, string script, bool useAddLoad)
        {
            if (control == null) throw new ArgumentNullException("control");
            if (String.IsNullOrWhiteSpace(script)) throw new ArgumentNullException("script");

            if (useAddLoad)
                script = String.Format(@"Sys.Application.add_load(function(){{{0}}});", script);

            var scriptManager = ScriptManager.GetCurrent(control is Page ? control as Page : control.Page);
            var scriptKey = control.ID + script.GetHashCode() + useAddLoad.GetHashCode() + "_ScriptBlck";
            if (scriptManager != null)
            {
                if (scriptManager.GetRegisteredStartupScripts().All(p => p.Key != scriptKey))
                    ScriptManager.RegisterStartupScript(control, control.GetType(), scriptKey, script, true);
            }
            else
                if (!control.Page.ClientScript.IsStartupScriptRegistered(control.GetType(), scriptKey))
                    control.Page.ClientScript.RegisterStartupScript(control.GetType(), scriptKey, script, true);
        }

        /// <summary>
        /// Регистрирует файл со скриптами для указанного контрола и добовляет ссылку на файл на страницу
        /// </summary>
        /// <param name="control">Control для которого подключается файл со скриптами</param>
        /// <param name="url">ссылка на файл</param>
        public static void RegisterClientScriptInclude(Control control, string url)
        {
            if (control == null) throw new ArgumentNullException("control");
            if (String.IsNullOrWhiteSpace(url)) throw new ArgumentNullException("url");

            if (ScriptManager.GetCurrent(control is Page ? control as Page : control.Page) != null)
                ScriptManager.RegisterClientScriptInclude(control, control.GetType(), control.ID + url, control.ResolveClientUrl(url));
            else
                control.Page.ClientScript.RegisterClientScriptInclude(control.GetType(), control.ID + url, control.ResolveClientUrl(url));
        }
    }
}
