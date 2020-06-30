using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using WMS.Common;
using WMS.Models;
using WMS.Interfaces;
using Dapper;
namespace WMS.DAL
{
    public class ErrorLogDataProvider: IErrorLog<ErroLogModel>
    {
        Configurations config = new Configurations();
        ErrorLogTrace log = new ErrorLogTrace();
        public async Task<IEnumerable<ErroLogModel>> GetLogData()
        {

            using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
            {

                try
                {
                    await pgsql.OpenAsync();
                    string query = "select * from wms.api_error_log";
                    return await pgsql.QueryAsync<ErroLogModel>(
                       query, null, commandType: CommandType.Text);
                }
                catch (Exception Ex)
                {
                    log.ErrorMessage("APIErrorLogDataProvider", "GetLogData", Ex.StackTrace.ToString());
                    return null;
                }
                finally
                {
                    pgsql.Close();
                }
            }
            // throw new NotImplementedException();
        }
    }
}
