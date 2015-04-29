using System;
using System.Web;
using System.Web.UI;

namespace QDSearch.ViewState
{
    /// <summary>
    /// Класс по работе с сессией
    /// </summary>
    public class StsStateKeeperHandler : IHttpHandler
    {
        /// <summary>
        /// Данный обработчик необходим для корректной работы StsStateKeeper.
        /// Он выполняет обработку запросов от StsStateKeeper на обновление состояния сессии и PageState
        /// </summary>
        #region IHttpHandler Members

        public bool IsReusable
        {
            // Верните значение false в том случае, если ваш управляемый обработчик не может быть повторно использован для другого запроса.
            // Обычно значение false соответствует случаю, когда некоторые данные о состоянии сохранены по запросу.
            get { return true; }
        }

        /// <summary>
        /// Метод по обработке запроса
        /// </summary>
        public void ProcessRequest(HttpContext context)
        {
            //todo: если такой GUID не существуют, доделать возврат ошибки и перегрузить страницу всвязи с устареванием сессии.
            //разместите здесь вашу реализацию обработчика.
            if (!String.IsNullOrWhiteSpace(context.Request["__VIEWSTATE"]))
            {
                var formatter = new ObjectStateFormatter();
                var pair = formatter.Deserialize(context.Request["__VIEWSTATE"]) as Pair;
                if (pair != null && pair.First != null && pair.First.ToString().Length == 36)
                {
                    if(StsSqlPageStatePersister.ResetPageStateTimeout(new Guid(pair.First.ToString())))
                        context.Response.Write("OK");
                }

            }
        }



        #endregion
    }
}
