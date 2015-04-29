using System;
using System.Linq;
using System.Web;
using QDSearch.Helpers;

public partial class MasterPageGeneral : System.Web.UI.MasterPage
{
    private static readonly string[] TrmServersIPs = { "192.168.12.22", "192.168.12.23", "192.168.12.24", "192.168.12.25", "192.168.0.20" };
    protected void Page_Init(object sender, EventArgs e)
    {
        bool isTerminalServer = false;
        bool clearDataCache = false;
        bool.TryParse(Request.QueryString["TerminalServer"], out isTerminalServer);
        bool.TryParse(Request.QueryString["DataCacheClear"], out clearDataCache);

        if (clearDataCache)
        {
            CacheHelper.RemoveCacheDataByKeyPart();
            Response.Redirect(ResolveClientUrl("~/"), true);
        }

        //todo: протянуть конфигурацию IP адрессов с индивидуальным CSS в конфиг.
        if (TrmServersIPs.Contains(Request.UserHostAddress) || isTerminalServer)
            LnkGeneralStyle.Href = ResolveClientUrl(@"~/styles/generalTrm.css");
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.Cache.SetCacheability(HttpCacheability.NoCache);
        Response.Cache.SetLastModified(DateTime.Now.ToUniversalTime());
        Response.Cache.SetMaxAge(TimeSpan.Zero);
        Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
        Response.Cache.SetValidUntilExpires(false);
        Response.Cache.SetNoStore();
    }
}
