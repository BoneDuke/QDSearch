using System;
using System.Linq;
using System.Security.Permissions;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.Adapters;
using System.Web.UI.WebControls;
using QDSearch.Configuration;
using QDSearch.WebControls;

namespace QDSearch.ViewState
{
    /// <summary>
    /// Допустимые режимы хранения PageState
    /// </summary>
    public enum ViewStateModes
    {
        /// <summary>
        /// Режим по умолчанию. В этом случае наши механизмы храниения PageState не используются.
        /// </summary>
        Default = 0,
        /// <summary>
        /// В этом режиме Page State хранится на странице в поле __VIEWSTATE.
        /// </summary>
        InPage = 1,
        /// <summary>
        /// В этом режиме Page State сохраняется в сессии пользователя.
        /// </summary>
        InSession = 2,
        /// <summary>
        /// В этом режиме Page State сохраняется в базе данных Ms SQL
        /// </summary>
        InMsSql = 3
    }


    /// <summary>
    /// Наш PageAdapter необходим для подключения реализации механизма сохранения PageState в нестандартных хранилищах
    /// Например MsSql, Memcache, NoSql, MySql
    /// </summary>
    [AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class StsPageStatePageAdapter : PageAdapter
    {
        /// <summary>
        /// Возвращает текстовое значение строки ViewState
        /// </summary>
        internal string ViewStateString
        {
            get { return ClientState; }
        }


        /// <summary>
        /// Необходима для подключения StsStateKeeper, в случае если PageState или Session хранится в InMsSql InSession StateServer SQLServer
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            ViewStateModes viewStateMode = Globals.Settings.ViewState.ViewStateMode;
            SessionStateMode stateMode = HttpContext.Current.Session == null ? SessionStateMode.Off : Page.Session.Mode;
            if (viewStateMode == ViewStateModes.InMsSql || viewStateMode == ViewStateModes.InSession || stateMode == SessionStateMode.StateServer || stateMode == SessionStateMode.SQLServer)
            {
                if (Page.IsPostBack)
                {
                    //todo: Сюда или в Init добавить обработку метки времени, это позволит понять что страница устарела.
                }
                //todo: проанализировать какой вариант лучше на практике.
                //Page.Form.Controls.Add(new StsStateKeeper(this));
                AddTimeStampField();
                AddStateKeeperScript();
            }

            base.OnLoad(e);
        }

        private void AddTimeStampField()
        {

            var scriptManager = ScriptManager.GetCurrent(Page);
            if (scriptManager != null)
            {
                if (scriptManager.GetRegisteredHiddenFields().All(p => p.Name == "__pingStamp"))
                    ScriptManager.RegisterHiddenField(Page, "__pingStamp", DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss.fff"));
            }
            else
                Page.ClientScript.RegisterHiddenField("__pingStamp", DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss.fff"));
        }

        private bool AddStateKeeperScript()
        {
            uint minAllowedTimeout = 0; // в минутах
            HttpSessionState session = HttpContext.Current.Session;
            if (session != null && session.Timeout > 0 && (session.Mode == SessionStateMode.StateServer || session.Mode == SessionStateMode.SQLServer))
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

            if (alertTimeout >= 2)
                alertTimeout = alertTimeout/2;

            string keeperScriptWs =
                String.Format(@"setInterval(function() {{
                                                        $.post('{0}', {{""__VIEWSTATE"":$($get('__VIEWSTATE')).val(), ""__pingStamp"":$($get('__pingStamp')).val()}}).done(function(data) {{
                                                            if(data != ""OK"") {{
                                                                alert(""Извините! Данные на странице устарели, страница будет перезагружена."");
                                                                window.location.href = '{2}';
                                                            }} 
                                                        }}).fail(function() {{
                                                            alert(""Извините! Произошла ошибка связи с сервером, страница будет перезагружена."");
                                                            window.location.href = '{2}'; 
                                                        }});
                                                        var d = new Date();
                                                        $('#__pingStamp').val(d.getFullYear() + ""-"" + d.getMonth() + ""-"" + d.getDate() + "" "" + d.getHours() + ""."" + d.getMinutes() + ""."" + d.getSeconds() + ""."" + d.getMilliseconds());
                                                    }}, {1});",
                Page.ResolveClientUrl(@"~/checkstate.skhandler"), alertTimeout * 1000, Page.ResolveClientUrl(@"~/"));

            //String.Format(@"setInterval(function(){{location.reload();}}, {0});", alertTimeout * 1000);

            //if (ScriptManager.GetCurrent(Page) != null)
            //    ScriptManager.RegisterStartupScript(Page, GetType(), "keeperScriptWs", keeperScriptWs, true);
            //else

            if (!Page.ClientScript.IsStartupScriptRegistered(GetType(), "keeperScriptWs"))
                Page.ClientScript.RegisterStartupScript(GetType(), "keeperScriptWs", keeperScriptWs, true);


            return true;
        }

        /// <summary>
        /// Возвращает PageStatePersister в зависимости он настроек ViewState mode в web.config
        /// </summary>
        /// <returns></returns>
        public override PageStatePersister GetStatePersister()
        {
            PageStatePersister pageStatePersister;
            switch (Globals.Settings.ViewState.ViewStateMode)
            {
                case ViewStateModes.Default:
                    pageStatePersister = base.GetStatePersister();
                    break;
                case ViewStateModes.InPage:
                    pageStatePersister = new HiddenFieldPageStatePersister(Page);
                    break;
                case ViewStateModes.InSession:
                    pageStatePersister = new SessionPageStatePersister(Page);
                    break;
                case ViewStateModes.InMsSql:
                    pageStatePersister = new StsSqlPageStatePersister(Page);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return pageStatePersister;
        }

    }
}
