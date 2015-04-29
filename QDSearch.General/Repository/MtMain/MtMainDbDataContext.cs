using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace QDSearch.Repository.MtMain
{
    /// <summary>
    /// Контекст основной БД
    /// </summary>
    public partial class MtMainDbDataContext
    {
        partial void OnCreated()
        {
            // необходимо для тестирования Wcf и других решений из под консольных приложений. 
            // ниже определяем из под чего загружена dll и в зависимости от этого читаем строку подключения.
            if (HttpRuntime.AppDomainAppId != null)
            {
                //is web app
                Connection.ConnectionString = Globals.Settings.MtMainDbConnectionString;
            }
            //else
            //{
            //    //is windows app
            //}
            Connection.StateChange += Connection_StateChange;
        }

        void Connection_StateChange(object sender, StateChangeEventArgs e)
        {
            if (e.CurrentState == ConnectionState.Open && Connection is SqlConnection && !string.IsNullOrWhiteSpace(Globals.Settings.MtMainDbCommandAfterOpenConnection))
            {
                var commandArithabortOn = new SqlCommand(Globals.Settings.MtMainDbCommandAfterOpenConnection, Connection as SqlConnection);
                commandArithabortOn.ExecuteNonQuery();
            }
        }

    }
}
