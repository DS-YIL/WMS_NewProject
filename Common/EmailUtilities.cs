using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using WMS.Models;
using WMS.Common;
using Dapper;
using System.Data;

namespace WMS.Common
{

	public class EmailUtilities
	{
		Configurations config = new Configurations();
		ErrorLogTrace log = new ErrorLogTrace();
		EmailModel emailobj = new EmailModel();
		public bool sendEmail(EmailModel emlSndngList, int subjecttype)
		{
			MailMessage mailMessage = new MailMessage(emlSndngList.FrmEmailId, emlSndngList.ToEmailId);
			SmtpClient client = new SmtpClient();
			var subbody = string.Empty;
			if (subjecttype == 1)
			{
				mailMessage.Subject = "Shipment received - PONO:" + emlSndngList.pono;
				subbody = "Materials for PONO " + emailobj.pono + " with INVOICE NO –" + emlSndngList.invoiceno + " has been received.";
			}
			else if (subjecttype == 2)
			{
				mailMessage.Subject = "Request for materials - Jobcode:" + emlSndngList.jobcode;
				subbody = mailMessage.Subject;
			}
			else if (subjecttype == 3)
			{
				mailMessage.Subject = "Material Issued - Jobcode:" + emlSndngList.jobcode;
				subbody = mailMessage.Subject;
			}
			else if (subjecttype == 4)
			{
				mailMessage.Subject = "Acknowledge materials - Jobcode:" + emlSndngList.jobcode;
				subbody = mailMessage.Subject;
			}


			if (!string.IsNullOrEmpty(emlSndngList.CC))
				mailMessage.CC.Add(emlSndngList.CC);
			//mailMessage.Subject = body;
			var body = string.Empty;
			if (emlSndngList.ToEmpName == null)
				emlSndngList.ToEmpName = getname(emlSndngList.ToEmailId);
			if (emlSndngList.sendername == null)
				emlSndngList.sendername = getname(emlSndngList.FrmEmailId);
			body = WMSResource.emailbody.Replace("#user", emlSndngList.ToEmpName).Replace("#subbody", subbody).Replace("#sender", emlSndngList.sendername);
			mailMessage.Body = body;
			mailMessage.IsBodyHtml = true;
			mailMessage.BodyEncoding = Encoding.UTF8;
			SmtpClient mailClient = new SmtpClient("10.29.15.9", 25);
			mailClient.EnableSsl = false;
			mailClient.Send(mailMessage);
			return true;
		}
		public string getname(string emailid)
		{
			string name = string.Empty;
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = "select name from wms.employee where email='" + emailid + "'";


					pgsql.Open();
					employeeModel emp = new employeeModel();
					emp = pgsql.QueryFirstOrDefault<employeeModel>(
					   query, null, commandType: CommandType.Text);
					name = emp.name;
					return name;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("EmailUtilities", "getname", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

	}
}
