using System;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using QDSearch.Configuration;
using QDSearch.ViewState;

namespace QDSearch.WebControls
{
    /// <summary>
    /// Иницирует обратные асинхроные вызовы страницы для обновления таймаутов PageState и SessionState.
    /// Таким образом предотвращает ситуацию когда пользователь оставил страницу открытой на длительное время,
    /// а его ссесия закрылась или ViewState удалился.
    /// Автоматически внедряется на страницу если mode равен InMsSql или SQLServer или StateServer, а режим таймаута для них выбран от минуты.
    /// Период срабатывания начинается от 3-х минут до окончания сессии.
    /// Для его корректной работы необходимо добавить следующую запись в web.config
    /// <system.webServer>
    ///    <handlers>
    ///        <add verb="*" path="checkstate.skhandler" name="StsStateKeeperHandler" type="Seemplexity.TravelSuite.ViewState.StsStateKeeperHandler"/>
    ///    </handlers>
    /// </system.webServer>
    /// </summary>
    [ToolboxData("<{0}:StsStateKeeper runat=server></{0}:StsStateKeeper>")]
    public class StsStateKeeper : WebControl
    {
        private StsPageStatePageAdapter _stsPageStatePageAdapter;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="stsPageStatePageAdapter">Адаптер</param>
        public StsStateKeeper(StsPageStatePageAdapter stsPageStatePageAdapter)
        {
            _stsPageStatePageAdapter = stsPageStatePageAdapter;
        }

        /// <summary>
        /// Событие загрузки
        /// </summary>
        /// <param name="e">Параметры события</param>
        protected override void OnLoad(EventArgs e)
        {
            //if (!Page.IsPostBack)
                AddStateKeeperScript();

            base.OnLoad(e);
        }

        private bool AddStateKeeperScript()
        {
            uint minAllowedTimeout = 0; // в минутах
            HttpSessionState session = HttpContext.Current.Session;
            if (session.Timeout > 0 && (session.Mode == SessionStateMode.StateServer || session.Mode == SessionStateMode.SQLServer))
                minAllowedTimeout = (uint)session.Timeout;

            ViewStateElement viewState = Globals.Settings.ViewState;
            if (viewState.Timeout > 0 && (viewState.Timeout < minAllowedTimeout || minAllowedTimeout == 0) && (viewState.ViewStateMode == ViewStateModes.InMsSql || viewState.ViewStateMode == ViewStateModes.InSession))
                minAllowedTimeout = viewState.Timeout;

            if (minAllowedTimeout <= 0)
                return false;

            uint delta = 180; // задержка до закрытия сессии сервером
            var alertTimeout = (int)(minAllowedTimeout * 60 - delta); // интервал в секундах по истечению которого произойдет обновление сессии
            while (alertTimeout <= 0)
            {
                delta -= 30;
                alertTimeout = (int)(minAllowedTimeout * 60 - delta);
            }
            if (alertTimeout <= 0)
                return false;


            string keeperScriptWs =
                String.Format(@"setInterval(function() {{$.post('{0}', $($get('__VIEWSTATE')).serialize()).fail(function() {{ location.reload(); }});}}, {1});",
                ResolveUrl(@"~/checkstate.skhandler"), alertTimeout * 1000);
            //String.Format(@"setInterval(function(){{location.reload();}}, {0});", alertTimeout * 1000);

            if (ScriptManager.GetCurrent(Page) != null)
                ScriptManager.RegisterStartupScript(this, GetType(), "keeperScriptWs", keeperScriptWs, true);
            else
                Page.ClientScript.RegisterStartupScript(GetType(), "keeperScriptWs", keeperScriptWs, true);

            return true;
        }
    }
}
