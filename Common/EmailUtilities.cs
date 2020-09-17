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
			//Security operator to inventory clerk

			if (subjecttype == 1)
			{
				mailMessage.Subject = "Shipment received - PoNo." + emlSndngList.pono;
				string receivedby = this.getnamebyid(emlSndngList.employeeno);
				subbody = "Shipment for PONO " + emlSndngList.pono + " has been received.Please find the details below. <br/> Invoice No:" + emlSndngList.invoiceno + "<br/> Received By :" + receivedby + "<br/> Received On : " + emlSndngList.receiveddate;

			}
			//Inventory Clery to Quality User

			else if (subjecttype == 2)
			{
				mailMessage.Subject = "Pending for Quality Check - GR No." + emlSndngList.grnnumber;
				//mailMessage.Subject = "Pending for Quality Check - GR No." + emlSndngList.grnnumber + "<br/>Materials of GR No - " + emlSndngList.grnnumber + "are pending for quality check.";
				subbody = "Materials of GR No - " + emlSndngList.grnnumber + "are pending for quality check.";
			}
			//Quality user  to Inventory Clerk

			else if (subjecttype == 3)
			{
				mailMessage.Subject = "Completed Quality Check for - GR No." + emlSndngList.grnnumber;
				//mailMessage.Subject = "Completed Quality Check for - GR No." + emlSndngList.grnnumber + "<br/>Materials of GR No - " + emlSndngList.grnnumber + "are completed quality check.";
				subbody = "Materials of GR No - " + emlSndngList.grnnumber + "are completed quality check.";
			}
			//Project Manager to Inventoty Clerk

			else if (subjecttype == 4)
			{
				mailMessage.Subject = "Request for Materials - ID" + emlSndngList.material;
				string requestedby = this.getnamebyid(emlSndngList.createdby);
				subbody = "Please find the Masterials request details below. <br/> Requested By:" +requestedby + "<br/>Requested On:" + emlSndngList.createddate;
				subbody = mailMessage.Subject;

			}
			//Inventory Manager to Mroject manger

			else if (subjecttype == 5)
			{
				mailMessage.Subject = "Materials Issued for Request Id" + emlSndngList.requestid ;
				subbody = "The materials for Request Id " + emlSndngList.requestid + "has been issued.";
				subbody = mailMessage.Subject;
			}
			//Project Manager  Acknowledgement to Inventoty Clerk

			else if (subjecttype == 6)
			{
				//Acknowledge for Materialed received - ID

				mailMessage.Subject = "Acknowledge for Material Received - ID" + emlSndngList.requestid;
				subbody = "The materials recevied has been acknowdleged by < br /> Please click on below link for more details.";
				subbody = mailMessage.Subject;
			}
			else if (subjecttype == 7)
			{
                mailMessage.Subject = "Acknowledge Materials -  Request Id" + emlSndngList.requestid;
                subbody = mailMessage.Subject;

            }
			//Admin to Approver(MA & FA)

			else if (subjecttype == 8)
			{
				//mailMessage.Subject = "Materials Issued for Request ID" + emlSndngList.requestid;
				//subbody = "Please find the Masterials request details below. <br/> Requested By:" + emlSndngList.requestedby + "<br/>Requested On:" + emlSndngList.requestedon;
				//subbody = mailMessage.Subject;

				mailMessage.Subject = "Gatepass materials for Returnable - GatePass - ID" + emlSndngList.gatepassid;
				string requestedby = this.getnamebyid(emlSndngList.requestedby);
				subbody = "Please find the Returnable Masterials details below. <br/> Requested By:" + requestedby + "<br/>Requested On:" + emlSndngList.requestedon + "<br/>GatePass Type:" + emlSndngList.gatepasstype;
				//subbody = mailMessage.Subject;

			}
			//Admin to Approver(MA)

			else if (subjecttype == 9)

			{

                mailMessage.Subject = "Gatepass materials for Non-returnable - GatePass - ID" + emlSndngList.gatepassid;
				string requestedby = this.getnamebyid(emlSndngList.requestedby);
				subbody = "Please find the Non-returnable Masterials details below. <br/> Requested By:" + requestedby + "<br/>Requested On:" + emlSndngList.requestedon + "<br/>GatePass Type:" + emlSndngList.gatepasstype;
                //subbody = mailMessage.Subject;

            }
			else if (subjecttype == 10)
			{
                mailMessage.Subject = "Reserve material";
                subbody = mailMessage.Subject;
            }
			else if (subjecttype == 11)
			{
				mailMessage.Subject = "Reserve material";
				subbody = mailMessage.Subject;
			}
			else if (subjecttype == 12)
			{
				mailMessage.Subject = "Reserve material";
				subbody = mailMessage.Subject;
			}
			else if (subjecttype == 13)
			{
				mailMessage.Subject = "Put Away";
				subbody = "All materials are placed for GRN(s) :" + emlSndngList.jobcode;
			}
			else if (subjecttype == 14)
			{
				mailMessage.Subject = "Material Transfer";
				subbody = emlSndngList.transferbody;
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
			mailClient.DeliveryMethod = SmtpDeliveryMethod.Network;
			mailClient.EnableSsl = false;
			mailClient.Send(mailMessage);
			return true;
		}

		public string getnamebyid(string employeeId)
        {
			string name = string.Empty;
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = "select name from wms.employee where employeeno='" + employeeId + "'";


					pgsql.Open();
					employeeModel emp = new employeeModel();
					emp = pgsql.QueryFirstOrDefault<employeeModel>(
					   query, null, commandType: CommandType.Text);
					if (emp != null)
					{
						name = emp.name;

					}
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
					if(emp!=null)
                    {
						name = emp.name;
						
					}
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
