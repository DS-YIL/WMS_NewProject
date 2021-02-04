using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Npgsql;

namespace WMS.Common
{
	public class ErrorLogTrace
	{		
		public void ErrorMessage(string controllername, string methodname, string stacktrace, string exception, [Optional]string url)
		{
			Configurations config = new Configurations();
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				pgsql.Open();
				if(pgsql.State == System.Data.ConnectionState.Open)
                {
					exception = exception.Replace("'", String.Empty);
					string query = "insert into wms.api_error_log(controller_name,method_name,exception_message,stacktrace,occureddate,url)values('" + controllername + "', '" + methodname + "', '" + exception + "','" + stacktrace + "','" + DateTime.Now + "','" + url + "')";
					IDbCommand selectCommand = pgsql.CreateCommand();
					selectCommand.CommandText = query;
					selectCommand.ExecuteNonQuery();
					pgsql.Close();
				}
				

			}
		}
	}
	public class UrlClass
	{
		public string showURL(IHttpContextAccessor httpcontextaccessor)
		{
			var request = httpcontextaccessor.HttpContext.Request;

			var absoluteUri = string.Concat(
						request.Scheme,
						"://",
						request.Host.ToUriComponent(),
						request.PathBase.ToUriComponent(),
						request.Path.ToUriComponent(),
						request.QueryString.ToUriComponent());
			return absoluteUri;
		}
	}
}
