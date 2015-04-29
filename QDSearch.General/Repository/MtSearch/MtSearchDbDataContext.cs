using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace QDSearch.Repository.MtSearch
{
    /// <summary>
    /// Контекст поисковой БД
    /// </summary>
    partial class MtSearchDbDataContext
    {
        partial void OnCreated()
        {
            // необходимо для тестирования Wcf и других решений из под консольных приложений. 
            // ниже определяем из под чего загружена dll и в зависимости от этого читаем строку подключения.
            if (HttpRuntime.AppDomainAppId != null)
            {
                //is web app
                Connection.ConnectionString = Globals.Settings.MtSearchDbConnectionString;
            }
            //else
            //{
            //    //is windows app
            //}
            Connection.StateChange += Connection_StateChange;
        }

        void Connection_StateChange(object sender, System.Data.StateChangeEventArgs e)
        {
            if (e.CurrentState == ConnectionState.Open && Connection is SqlConnection && !string.IsNullOrWhiteSpace(Globals.Settings.MtSearchDbCommandAfterOpenConnection))
            {
                var commandArithabortOn = new SqlCommand(Globals.Settings.MtSearchDbCommandAfterOpenConnection, Connection as SqlConnection);
                commandArithabortOn.ExecuteNonQuery();
            }
        }
    }
}
