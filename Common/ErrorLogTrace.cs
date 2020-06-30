using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;

namespace WMS.Common
{
    public class ErrorLogTrace
    {
        public void ErrorMessage(string controllername, string methodname, string exception)
        {
            Configurations config = new Configurations();

            using (NpgsqlConnection connection = new NpgsqlConnection())
            {
                string query = " insert into wms.api_error_log(controller_name, method_name, exception_message)";
                query += " values(@controllername, @methodname, @exception); ";
                connection.ConnectionString = config.PostgresConnectionString;
                connection.Open();
                NpgsqlCommand cmd = new NpgsqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = query;

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new NpgsqlParameter("@controllername", controllername));
                cmd.Parameters.Add(new NpgsqlParameter("@methodname", methodname));
                cmd.Parameters.Add(new NpgsqlParameter("@exception", exception));

                cmd.ExecuteNonQuery();
                cmd.Dispose();
                connection.Close();

            }
        }
    }
}
