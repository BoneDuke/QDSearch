using System.Data;
using System.Data.SqlClient;

namespace QDSearch.Repository.SftWeb
{

    partial class SftWebDbDataContext
    {
        partial void OnCreated()
        {
            Connection.ConnectionString = Globals.Settings.SftWebDbConnectionString;
            Connection.StateChange += Connection_StateChange;
        }

        void Connection_StateChange(object sender, System.Data.StateChangeEventArgs e)
        {
            if (e.CurrentState == ConnectionState.Open && Connection is SqlConnection && !string.IsNullOrWhiteSpace(Globals.Settings.SftSearchDbCommandAfterOpenConnection))
            {
                var commandArithabortOn = new SqlCommand(Globals.Settings.SftSearchDbCommandAfterOpenConnection, Connection as SqlConnection);
                commandArithabortOn.ExecuteNonQuery();
            }
        }
    }
}
