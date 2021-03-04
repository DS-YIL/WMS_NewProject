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
using System.Globalization;
using System.Linq.Expressions;

namespace WMS.Common
{

	public class EmailUtilities
	{
		Configurations config = new Configurations();
		ErrorLogTrace log = new ErrorLogTrace();
		EmailModel emailobj = new EmailModel();

		public enum Emailparameter
		{
			mailforqualitycheck = 1,
			mailforacceptance = 2,
			mailforputaway = 3,
			mailaftermaterialrequest = 4,
			mailaftermaterialreserve = 5
		}
		public bool sendEmail(EmailModel emlSndngList, int subjecttype, int roleid = 0)
		{
			string link = "";
			string linkurl = config.EmailLinkUrl;
			string tomainlstring = "";
			string toccstring = "";
			bool multipleemails = false;
			if (roleid > 0)
			{
				try
				{
					using (var DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						string query = WMSResource.dynamicemaildata.Replace("#roleid", roleid.ToString());

						var result = DB.QueryAsync<authUser>(
					   query, null, commandType: CommandType.Text);
						if (result != null && result.Result.Count() > 0)
						{
							int i = 0;
							int j = 0;
							foreach (authUser user in result.Result)
							{
                                if (user.emailnotification == true)
                                {
									if (i > 0)
									{
										multipleemails = true;
										tomainlstring += ",";

									}
									i++;

									tomainlstring += user.email.Trim();
								}
								if (user.emailccnotification == true)
								{
									if (j > 0)
									{
										toccstring += ",";

									}
									j++;

									toccstring += user.email.Trim();
								}

							}
						}

					}


				}
				catch (Exception e)

				{
					return false;
				}
				emlSndngList.ToEmailId = tomainlstring;
				emlSndngList.CC = toccstring;

			}
			emlSndngList.FrmEmailId = config.FromEmailId;
			if (config.EmailType.ToString().ToLower().Trim() == "test")
			{
				multipleemails = false;
				emlSndngList.ToEmailId = "ramesh.kumar@in.yokogawa.com";
				emlSndngList.CC = "ramesh.kumar@in.yokogawa.com";
			}
			MailMessage mailMessage = new MailMessage(emlSndngList.FrmEmailId, emlSndngList.ToEmailId);
			SmtpClient client = new SmtpClient();
			var subbody = string.Empty;
			//Security operator to inventory clerk(Receipts)

			if (subjecttype == 1)
			{
				mailMessage.Subject = "Shipment Received - GateEntryNo. :" + emlSndngList.inwmasterid;
				string receivedby = this.getnamebyid(emlSndngList.employeeno);
				string date = Convert.ToDateTime(emlSndngList.receiveddate).ToString("dd/MM/yyyy");
				subbody = "Shipment for GateEntryNo. - <b>" + emlSndngList.inwmasterid + "</b> has been received.Please find the details below. <br/> Invoice No : <b>" + emlSndngList.invoiceno + "</b><br/>POs : <b>" + emlSndngList.pono + "</b><br/> Received By : <b>" + receivedby + "</b><br/> Received On : <b>" + date + "</b>";
				link = linkurl + "WMS/Email/GRNPosting?GateEntryNo=" + emlSndngList.inwmasterid.Trim();

				//+ "-"+emlSndngList.invoiceno "SI-20-0000101;"SI-20-0000102"";
			}
			//Inventory Clerk(Receipt) to Quality User
			//quality user

			else if (subjecttype == 2)
			{
				mailMessage.Subject = "Pending for Quality Check - GRN No. :" + emlSndngList.grnnumber;
				//mailMessage.Subject = "Pending for Quality Check - GR No." + emlSndngList.grnnumber + "<br/>Materials of GR No - " + emlSndngList.grnnumber + "are pending for quality check.";
				subbody = "Materials with GRN No. - " + emlSndngList.grnnumber + " are pending for quality check.";
				//Redirect to QualityCheck Page
				link = linkurl + "WMS/Email/QualityCheck?GRNo=" + emlSndngList.grnnumber.Trim();
				//string decodeURL = decodeURL(link);
			}
			//Quality user  to Inventory Clerk
			//storeclerk

			else if (subjecttype == 3)
			{
				mailMessage.Subject = "Completed Quality Check for  GRN No. -" + emlSndngList.grnnumber;
				//mailMessage.Subject = "Completed Quality Check for - GR No." + emlSndngList.grnnumber + "<br/>Materials of GR No - " + emlSndngList.grnnumber + "are completed quality check.";
				subbody = "Quality Check has completed for the Materials with  GRN No. : " + emlSndngList.grnnumber + ".";
				//Redirect to Receipts Page
				link = linkurl + "WMS/Email/GRNPosting?GRNo=" + emlSndngList.grnnumber.Trim();
			}
			//Project Manager to Inventoty Clerk
			//Inventory Manager
			else if (subjecttype == 4)
			{
				mailMessage.Subject = "Issue Request for Materials with request id:" + emlSndngList.requestid;
				string requestedby = this.getnamebyid(emlSndngList.createdby);
				subbody = "Please find the Material request details below. <br/> Requested By:" + requestedby + "<br/>Requested On:" + emlSndngList.createddate;
				//subbody += "<br/>"+ mailMessage.Subject;
				link = linkurl + "WMS/Email/MaterialIssueDashboard?ReqId=" + emlSndngList.requestid;

			}
			//Project Manager to Inventoty Clerk
			//Inventory Manager
			else if (subjecttype == Conversion.toInt(Emailparameter.mailaftermaterialreserve))
			{
				mailMessage.Subject = "Reserve material with reserveid :" + emlSndngList.reserveid;
				string requestedby = this.getnamebyid(emlSndngList.createdby);
				subbody = "Please find the Material reserve details below.<br/> Reserve id:" + emlSndngList.reserveid + " <br/> Reserved By:" + requestedby + "<br/>Reserved On:" + emlSndngList.createddate + "<br/>Reserve upto:" + emlSndngList.reserveupto + "<br/>Project code:" + emlSndngList.projectcode;
				subbody += "<br/>";
				if (emlSndngList.remarks != null && emlSndngList.remarks != "")
				{
					subbody += "<br/>Remarks :" + emlSndngList.remarks;
					subbody += "<br/>";
				}
				subbody += "<table style=\"border: 1px solid black;border-collapse:collapse; \">";
				subbody += "<tr>";
				subbody += "<th style=\"border: 1px solid black;border-collapse:collapse;\">Material</th>";
				subbody += "<th style=\"border: 1px solid black;border-collapse:collapse;\">PO Item Description</th>";
				subbody += "<th style=\"border: 1px solid black;border-collapse:collapse;\">Reserve Quantity</th>";
				subbody += "</tr>";
				foreach (MaterialTransactionDetail dt in emlSndngList.reservedata)
				{
					subbody += "<tr>";
					subbody += "<td style=\"border: 1px solid black;border-collapse:collapse;\">" + dt.materialid + "</td>";
					subbody += "<td style=\"border: 1px solid black;border-collapse:collapse;\">" + dt.poitemdescription + "</td>";
					subbody += "<td style=\"border: 1px solid black;border-collapse:collapse;\">" + dt.reservedqty + "</td>";
					subbody += "</tr>";
				}
				subbody += "</table>";

				//link = linkurl + "WMS/Email/MaterialIssueDashboard?ReqId=" + emlSndngList.requestid;

			}
			//Inventory Manager to Project manager
			//project manager
			else if (subjecttype == 51)
			{
				mailMessage.Subject = "Materials Issued for Request Id" + emlSndngList.requestid ;
				subbody = "The materials for Request Id " + emlSndngList.requestid + " has been issued.";
				//subbody += mailMessage.Subject;
				link = linkurl + "WMS/Email/MaterialReqView?ReqId=" + emlSndngList.requestid;

			}
			//Project Manager  Acknowledgement to Inventoty Clerk

			else if (subjecttype == 6)
			{
				//Acknowledge for Material received - ID

				mailMessage.Subject = "Acknowledge for Material Received - ID" + emlSndngList.requestid;
				subbody = "The materials recevied has been acknowdleged by < br /> Please click on below link for more details.";
				subbody = mailMessage.Subject;
				link = "";

			}
			//else if (subjecttype == 7)
			//{
			//  mailMessage.Subject = "Acknowledge Materials -  Request Id" + emlSndngList.requestid;
			// subbody = mailMessage.Subject;
			// }

			//Admin to Approver(PM)

			else if (subjecttype == 8)
			{
				//mailMessage.Subject = "Materials Issued for Request ID" + emlSndngList.requestid;
				//subbody = "Please find the Masterials request details below. <br/> Requested By:" + emlSndngList.requestedby + "<br/>Requested On:" + emlSndngList.requestedon;
				//subbody = mailMessage.Subject;
				string requesteddte = emlSndngList.requestedon.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
				mailMessage.Subject = "Gatepass Materials for" + emlSndngList.gatepasstype + "- GatePass - ID : " + emlSndngList.gatepassid;
				string requestedby = this.getnamebyid(emlSndngList.requestedby);
				subbody = "Please find the " + emlSndngList.gatepasstype + " Gatepass Materials details below. <br/> Requested By : <b>" + requestedby + "</b><br/>Requested On : <b>" + requesteddte + "</b><br/>GatePass Type : <b>" + emlSndngList.gatepasstype + "</b>";
				//subbody = mailMessage.Subject;
				link = linkurl + "WMS/Email/GatePassPMList?GateId=" + emlSndngList.gatepassid.Trim();


			}


			//Admin to Approver(MA)

			else if (subjecttype == 9)

			{

				mailMessage.Subject = "Gatepass Materials for Non-returnable - GatePass ID - " + emlSndngList.gatepassid;
				string requestedby = this.getnamebyid(emlSndngList.requestedby);
				subbody = "Please find the Non-returnable Materials details below. <br/> Requested By : " + requestedby + "<br/>Requested On : " + emlSndngList.requestedon + "<br/>GatePass Type : " + emlSndngList.gatepasstype;
				//subbody = mailMessage.Subject;
				link = linkurl + "WMS/Email/GatePassPMList?GateId=" + emlSndngList.gatepassid.Trim();


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
				mailMessage.Subject = "Notify to Finance";
				subbody = "All materials are placed for GRN(s) :" + emlSndngList.jobcode;
				link = linkurl + "WMS/Email/GRNotification?GRNo=" + emlSndngList.jobcode.Trim();
			}
			else if (subjecttype == 14)
			{
				mailMessage.Subject = "Material Transfer with transfer id :" + emlSndngList.transferid;
				subbody = emlSndngList.transferbody;
				link = linkurl + "WMS/Email/materialtransferapproval?transferid=" + emlSndngList.transferid.Trim();
			}
			// Approverfor Returnable(PM)-Inventory Clerk

			else if (subjecttype == 15)

			{

				mailMessage.Subject = "Gate Pass Material Pending for Issue - GatePass  ID : " + emlSndngList.gatepassid;
				string requestedby = this.getnamebyid(emlSndngList.approvername);
				subbody = "Returnable Gate Pass was approved by PM and pending for issue.<br/>Please click on the below link to issue materials. <br/> Requested By : " + emlSndngList.approvername;
				//subbody = mailMessage.Subject;
				link = linkurl + "WMS/Email/GatePass?GateId=" + emlSndngList.gatepassid.Trim();


			}
			// Approver(PM)-Approver(FM) for Non Returnable gatepass

			else if (subjecttype == 16)

			{
				string requesteddte = emlSndngList.requestedon.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
				mailMessage.Subject = emlSndngList.gatepasstype + " Gatepass Materials with GatePass ID -" + emlSndngList.gatepassid + "are pending for FM approval ";
				string requestedby = emlSndngList.requestedby;
				subbody = "Please find the Material details below.<br/>GatePass Type : <b>" + emlSndngList.gatepasstype + "</b><br/>Requested By : <b>" + requestedby + "</b><br/>Requested On : <b>" + requesteddte + "</b><br/>GatePass Type : <b>" + emlSndngList.gatepasstype + "</b><br/>Approved By : <b>" + emlSndngList.approvername + "<b>";
				//subbody = mailMessage.Subject;
				link = linkurl + "WMS/Email/GatePassFMList?GateId=" + emlSndngList.gatepassid.Trim();


			}
			// Approver for Non - Returnable(FM)-Inventory Clerk

			else if (subjecttype == 17)

			{

				mailMessage.Subject = emlSndngList.gatepasstype + " Gate Pass Material Pending for Issue - GatePass ID : " + emlSndngList.gatepassid;
				string requestedby = this.getnamebyid(emlSndngList.requestedby);
				subbody = "Please click on the below link to issue materials. ";
				//subbody = mailMessage.Subject;
				link = linkurl + "WMS/Email/GatePass?GateId=" + emlSndngList.gatepassid.Trim();


			}
			else if (subjecttype == 18)
			{
				mailMessage.Subject = "Material Transfer with transfer id :" + emlSndngList.transferid;
				subbody = emlSndngList.transferbody;
				link = linkurl + "WMS/Email/materialtransfer?transferid=" + emlSndngList.transferid.Trim();
			}
			else if (subjecttype == 19)
			{
				mailMessage.Subject = "Material reurn with return id :" + emlSndngList.returnid;
				subbody = "Please click on the below link for material return details. ";
				link = linkurl + "WMS/Email/materialreturndashboard?returnid=" + emlSndngList.returnid.Trim();
			}
			else if (subjecttype == 20)
			{
				mailMessage.Subject = "Material Received for  GRN No. -" + emlSndngList.grnnumber;
				//mailMessage.Subject = "Completed Quality Check for - GR No." + emlSndngList.grnnumber + "<br/>Materials of GR No - " + emlSndngList.grnnumber + "are completed quality check.";
				subbody = "Material Received with  GRN No. : " + emlSndngList.grnnumber + " and ready for acceptance.";
				//Redirect to Receipts Page
				link = linkurl + "WMS/Email/GRNPosting?GRNo=" + emlSndngList.grnnumber.Trim();
			}
			else if (subjecttype == 21)
			{

				mailMessage.Subject = "Gate Pass sent for modification - GatePass  ID : " + emlSndngList.gatepassid;
				string requestedby = this.getnamebyid(emlSndngList.approvername);
				subbody = "Gate Pass sent for modification.GatePass  ID : " + emlSndngList.gatepassid;
				//subbody = mailMessage.Subject;
				link = linkurl + "WMS/Email/GatePass?GatepassId=" + emlSndngList.gatepassid.Trim();


			}
			else if (subjecttype == 22)
			{

				mailMessage.Subject = "Gate Pass issued - GatePass  ID : " + emlSndngList.gatepassid;
				string requestedby = this.getnamebyid(emlSndngList.approvername);
				subbody = "Gate Pass issued for GatePass  ID : " + emlSndngList.gatepassid;
				//subbody = mailMessage.Subject;
				link = linkurl + "WMS/Email/GatePass?GatepassId=" + emlSndngList.gatepassid.Trim();


			}
			//Project Manager to Inventoty Clerk
			//Inventory Manager
			else if (subjecttype == 23)
			{
				mailMessage.Subject = "Approval Request for Materials with request id:" + emlSndngList.requestid;
				string requestedby = this.getnamebyid(emlSndngList.createdby);
				subbody = "Please find the Material request details below. <br/> Requested By:" + requestedby + "<br/>Requested On:" + emlSndngList.createddate;
				//subbody += "<br/>"+ mailMessage.Subject;
				link = linkurl + "WMS/Email/MaterialRequestApproval?ReqId=" + emlSndngList.requestid;

			}
			else if (subjecttype == 24)
			{
				mailMessage.Subject = "Rejected Materials Request for Request Id" + emlSndngList.requestid;
				subbody = "The material request for Request Id " + emlSndngList.requestid + " has been rejected.";
				//subbody += mailMessage.Subject;
				link = linkurl + "WMS/Email/MaterialReqView?ReqId=" + emlSndngList.requestid;

			}
			else if (subjecttype == 25)
			{
				mailMessage.Subject = "Materials Issued for STO Request Id " + emlSndngList.requestid;
				subbody = "The materials for STO Request Id " + emlSndngList.requestid + " has been issued.";
				//subbody += mailMessage.Subject;
				link = linkurl + "WMS/Email/STOMaterialPutaway?ReqId=" + emlSndngList.requestid;

			}
			else if (subjecttype == 26)
			{
				mailMessage.Subject = "Materials Issued for Sub Contract Request Id " + emlSndngList.requestid;
				subbody = "The materials for Sub Contract Request Id " + emlSndngList.requestid + " has been issued.";
				//subbody += mailMessage.Subject;
				link = "";

			}
			else if (subjecttype == 27)
			{
				mailMessage.Subject = "Issue Request for STO Materials with request id:" + emlSndngList.requestid;
				string requestedby = this.getnamebyid(emlSndngList.createdby);
				subbody = "Please find the STO Material request details below. <br/> Requested By:" + requestedby + "<br/>Requested On:" + emlSndngList.createddate;
				//subbody += "<br/>"+ mailMessage.Subject;
				link = linkurl + "WMS/Email/IssueSTOMaterial?ReqId=" + emlSndngList.requestid;
			}
			else if (subjecttype == 28)
			{
				mailMessage.Subject = "Issue Request for Sub Contacting Materials with request id:" + emlSndngList.requestid;
				string requestedby = this.getnamebyid(emlSndngList.createdby);
				subbody = "Please find the Sub Contacting Material request details below. <br/> Requested By:" + requestedby + "<br/>Requested On:" + emlSndngList.createddate;
				//subbody += "<br/>"+ mailMessage.Subject;
				link = linkurl + "WMS/Email/IssueSubcontractingMaterial?ReqId=" + emlSndngList.requestid;
			}

			else if (subjecttype == 29)
			{
				mailMessage.Subject = "Approval Request for Intra Unit Transfer with request id:" + emlSndngList.requestid;
				string requestedby = this.getnamebyid(emlSndngList.createdby);
				subbody = "Please find the Intra Unit Transfer request details below. <br/> Requested By:" + requestedby + "<br/>Requested On:" + emlSndngList.createddate;
				//subbody += "<br/>"+ mailMessage.Subject;
				link = linkurl + "WMS/Email/ApproveSTOMaterial?ReqId=" + emlSndngList.requestid;
			}
			else if (subjecttype == 30)
			{
				mailMessage.Subject = "Approval Request for Sub Contacting Materials with request id:" + emlSndngList.requestid;
				string requestedby = this.getnamebyid(emlSndngList.createdby);
				subbody = "Please find the Sub Contacting Material request details below. <br/> Requested By:" + requestedby + "<br/>Requested On:" + emlSndngList.createddate;
				//subbody += "<br/>"+ mailMessage.Subject;
				link = linkurl + "WMS/Email/ApprovalSubcontractingMaterial?ReqId=" + emlSndngList.requestid;
			}
			else if (subjecttype == 31)
			{
				mailMessage.Subject = "Rejected "+emlSndngList.requesttype+" Materials Request for Request Id" + emlSndngList.requestid;
				subbody = "The material request for Request Id " + emlSndngList.requestid + " has been rejected.";
				//subbody += mailMessage.Subject;
				if(emlSndngList.requesttype == "STO")
                {
					link = linkurl + "WMS/Email/StockTransferOrder?ReqId=" + emlSndngList.requestid;
				}
				else if(emlSndngList.requesttype == "SubContract")
                {
					link = linkurl + "WMS/Email/SubContractTransferOrder?ReqId=" + emlSndngList.requestid;
				}
				

			}
			else if (subjecttype == 32)
			{
				mailMessage.Subject = "Materials Received for GRN No. -" + emlSndngList.grnnumber;
				//mailMessage.Subject = "Completed Quality Check for - GR No." + emlSndngList.grnnumber + "<br/>Materials of GR No - " + emlSndngList.grnnumber + "are completed quality check.";
				subbody = "Materials Received with  GRN No. : " + emlSndngList.grnnumber + " and ready to putaway.";
				//Redirect to Receipts Page
				link = linkurl + "WMS/Email/grnputaway?GRNo=" + emlSndngList.grnnumber.Trim();
			}


			
			//mailMessage.Subject = body;
			var body = string.Empty;
			
			string users = "";
			if (!string.IsNullOrEmpty(emlSndngList.CC))
				mailMessage.CC.Add(emlSndngList.CC);
			if (emlSndngList.ToEmpName == null)
				emlSndngList.ToEmpName = getname(emlSndngList.ToEmailId);
			if (emlSndngList.sendername == null)
            {
				//emlSndngList.sendername = getname(emlSndngList.FrmEmailId);
				emlSndngList.sendername = string.Empty;
			}
				
			if (multipleemails == true)
			{
				users = "All";
			}
			else
			{
				users = emlSndngList.ToEmpName;

			}
			body = WMSResource.emailbody.Replace("#user", users).Replace("#subbody", subbody).Replace("#sender", emlSndngList.sendername).Replace("#link", link);
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
					log.ErrorMessage("EmailUtilities", "getname", Ex.StackTrace.ToString(), Ex.Message.ToString());
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
					if (emp != null)
					{
						name = emp.name;

					}
					return name;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("EmailUtilities", "getname", Ex.StackTrace.ToString(), Ex.Message.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public bool sendCreatePOMail(string FrmEmailId, int RevisionId)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					var ipaddress = "http://10.29.15.68:99/SCM/MPRForm/" + RevisionId + "";
					EmailSend emlSndngList = new EmailSend();
					emlSndngList.Body = "<html><meta charset=\"ISO-8859-1\"><head><link rel = 'stylesheet' href = 'https://maxcdn.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css' ></head><body><div class='container'><b>Click here to redirect : </b>&nbsp<a href='" + ipaddress + "'>" + ipaddress + " </a></div></body></html>";
					string userquery = "select email from wms.employee where employeeno ='" + FrmEmailId + "'";
					User userdata = pgsql.QuerySingle<User>(
					   userquery, null, commandType: CommandType.Text);
					emlSndngList.FrmEmailId = userdata.email;
					emlSndngList.Subject = "STO PO Created from WMS";
					//checker
					string quer1 = "select email from wms.employee where employeeno ='" + config.POChecker + "'";
					User data1 = pgsql.QuerySingle<User>(
					   quer1, null, commandType: CommandType.Text);
					emlSndngList.ToEmailId = data1.email;
					if (emlSndngList.FrmEmailId != "NULL" && emlSndngList.ToEmailId != "NULL")
						this.EmailSending(emlSndngList);
					this.EmailSending(emlSndngList);

					string query2 = "select email from wms.employee where employeeno ='" + config.POApprover + "'";
					User data2 = pgsql.QuerySingle<User>(
					   query2, null, commandType: CommandType.Text);
					emlSndngList.ToEmailId = data2.email;
					if (emlSndngList.FrmEmailId != "NULL" && emlSndngList.ToEmailId != "NULL")
						this.EmailSending(emlSndngList);
					this.EmailSending(emlSndngList);
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("EmailUtilities", "getname", Ex.StackTrace.ToString(), Ex.Message.ToString());
					return false;
				}
				finally
				{
					pgsql.Close();
				}
			}
			return true;
		}
		public bool EmailSending(EmailSend emlSndngList)
		{
			try
			{
				if (!string.IsNullOrEmpty(emlSndngList.ToEmailId) && !string.IsNullOrEmpty(emlSndngList.FrmEmailId))
				{
					//var BCC = ConfigurationManager.AppSettings["BCC"];
					//var SMTPServer = ConfigurationManager.AppSettings["SMTPServer"];
					MailMessage mailMessage = new MailMessage();
					mailMessage.From = new MailAddress(emlSndngList.FrmEmailId.Trim(), ""); //From Email Id
					string[] ToMuliId = emlSndngList.ToEmailId.Split(',');
					foreach (string ToEMailId in ToMuliId)
					{
						if (!string.IsNullOrEmpty(ToEMailId))
							mailMessage.To.Add(new MailAddress(ToEMailId.Trim(), "")); //adding multiple TO Email Id
					}
					SmtpClient client = new SmtpClient();
					if (!string.IsNullOrEmpty(emlSndngList.Subject))
						mailMessage.Subject = emlSndngList.Subject;

					if (!string.IsNullOrEmpty(emlSndngList.CC))
					{
						string[] CCId = emlSndngList.CC.Split(',');

						foreach (string CCEmail in CCId)
						{
							if (!string.IsNullOrEmpty(CCEmail))
								mailMessage.CC.Add(new MailAddress(CCEmail.Trim(), "")); //Adding Multiple CC email Id
						}
					}

					if (!string.IsNullOrEmpty(emlSndngList.BCC))
					{
						string[] bccid = emlSndngList.BCC.Split(',');


						foreach (string bccEmailId in bccid)
						{
							if (!string.IsNullOrEmpty(bccEmailId))
								mailMessage.Bcc.Add(new MailAddress(bccEmailId.Trim(), "")); //Adding Multiple BCC email Id
						}
					}

					//if (!string.IsNullOrEmpty(BCC))
					//	mailMessage.Bcc.Add(new MailAddress(BCC.Trim(), ""));
					mailMessage.Body = emlSndngList.Body;
					mailMessage.IsBodyHtml = true;
					mailMessage.BodyEncoding = Encoding.UTF8;
					SmtpClient mailClient = new SmtpClient("10.29.15.9", 25);
					mailClient.EnableSsl = true;
					mailClient.DeliveryMethod = SmtpDeliveryMethod.Network;
					mailClient.Send(mailMessage);

				}
			}
			catch (Exception ex)
			{
				log.ErrorMessage("EmailTemplate",ex.StackTrace.ToString(), "sendEmail" + ";" + emlSndngList.ToEmailId.ToString() + "", ex.ToString());
			}
			return true;
		}

		/*Name of Class : <<EmailSend>>  Author :<<Prasanna>>  
	  Date of Creation <<22-01-2021>>
	  Purpose : <<to send email>>
	  Review Date :<<>>   Reviewed By :<<>>*/
		public class EmailSend
		{
			public string FrmEmailId { get; set; }
			public string ToEmailId { get; set; }
			public string CC { get; set; }
			public string BCC { get; set; }
			public string Subject { get; set; }
			public string Body { get; set; }
		}
	}
}
