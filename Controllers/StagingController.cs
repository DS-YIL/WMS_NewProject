﻿/*
    Name of File : <<StagingController>>  Author :<<Prasanna>>  
    Date of Creation <<02-07-2020>>
    Purpose : <<to save files from SAP system to WMS tables>>
    Review Date :<<>>   Reviewed By :<<>>
    Sourcecode Copyright : Yokogawa India Limited
*/

using Dapper;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WMS.Common;
using WMS.Models;

namespace WMS.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class StagingController : ControllerBase
	{
		Configurations config = new Configurations();
		ErrorLogTrace log = new ErrorLogTrace();
		/*Name of Function : <<uploadExcel>>  Author :<<Prasanna>>  
		Date of Creation <<02-07-2020>>
		Purpose : <<Write briefly in one line or two lines>>
		Review Date :<<>>   Reviewed By :<<>>
		Sourcecode Copyright : Yokogawa India Limited
		*/

		[HttpGet]
		[Route("uploadPoDataExcel")]
		public IActionResult uploadPoDataExcel()
		{
			using (NpgsqlConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
			{

				DB.Open();
				//var filePath = @"http://10.29.15.183:100/WMSFiles/stageTest1.xlsx";
				var filePath = @"D:\YILProjects\WMS\WMSFiles\stageTest1.xlsx";
				var filenamestr = @"D:\YILProjects\WMS\WMSFiles\stageTest1.xlsx";
				string[] fillenamearr = filenamestr.Split('\\');
				string filename = fillenamearr[fillenamearr.Length - 1];
				DataTable dtexcel = new DataTable();
				bool hasHeaders = false;
				string HDR = hasHeaders ? "Yes" : "No";
				string strConn;
				if (filePath.Substring(filePath.LastIndexOf('.')).ToLower() == ".xlsx")
					strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePath + ";Extended Properties=\"Excel 12.0;HDR=" + HDR + ";IMEX=0\"";
				else
					strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filePath + ";Extended Properties=\"Excel 8.0;HDR=" + HDR + ";IMEX=0\"";


				OleDbConnection conn = new OleDbConnection(strConn);
				conn.Open();
				DataTable schemaTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

				DataRow schemaRow = schemaTable.Rows[0];
				string sheet = schemaRow["TABLE_NAME"].ToString();
				if (!sheet.EndsWith("_"))
				{
					string query = "SELECT  * FROM [Sheet1$]";
					OleDbDataAdapter daexcel = new OleDbDataAdapter(query, conn);
					dtexcel.Locale = CultureInfo.CurrentCulture;
					daexcel.Fill(dtexcel);
				}

				conn.Close();


				foreach (DataRow row in dtexcel.Rows)
				{
					try
					{
						string Error_Description = "";
						bool dataloaderror = false;
						var PurchDoc = "Purch.Doc.".Replace('.', '#');
						var docdate = "Doc. Date".Replace('.', '#');
						var Relstat = "Rel.stat".Replace('.', '#');
						if (string.IsNullOrEmpty((row[PurchDoc].ToString()).Replace('.', '#')))
							Error_Description += "There is NO PONO";
						if (string.IsNullOrEmpty(row["Material"].ToString()))
							Error_Description += "No material";
						if (string.IsNullOrEmpty(row["PO Quantity"].ToString()))
							Error_Description += "No PO Quantity";
						if (!string.IsNullOrEmpty(Error_Description))
							dataloaderror = true;

						var query = "INSERT INTO wms.STAG_PO_SAP (POcreatedby,DocDate,PurchDoc,Item,A,I,Material,Shorttext,PGr,SLoc,ItemDeliveryDate,POQuantity,OPU," +
							"NetPrice,Crcy,NetValue,DeliveredQty,InvoicedQty,Vendor,VendorName,ProjectDefinition,WBSelement,SalesOrderNumber,SalesOrderItemNumber,OrderNumber,CostCenter1," +
							"AssetNumber,Relstat,D,DCI,datasource,createddate,DataloadErrors ,Error_Description)VALUES('" + row["PO created by (User Name)"].ToString() + "'," +
							"'" + (Convert.ToDateTime(row[docdate])).ToString("yyyy-MM-dd") + "','" + row[PurchDoc].ToString() + "','" + row["Item"].ToString() + "','" + row["A"].ToString() + "','" + row["I"].ToString() + "'," +
							"'" + row["Material"].ToString() + "','" + row["Short Text"].ToString() + "','" + row["PGr"].ToString() + "','" + row["SLoc"].ToString() + "'," +
							"'" + (Convert.ToDateTime(row["Item Delivery Date"])).ToString("yyyy-MM-dd") + "'," + row["PO Quantity"].ToString() + ",'" + row["OPU"].ToString() + "','" + row["Net Price"].ToString() + "','" + row["Crcy"].ToString() + "','" + row["Net Value"].ToString() + "'," +
							"'" + row["Delivered Qty"].ToString() + "'," + row["Invoiced Qty"].ToString() + ",'" + row["Vendor"].ToString() + "','" + row["Vendor Name"].ToString() + "','" + row["Project Definition"].ToString() + "','" + row["WBS Element"].ToString() + "','" + row["Sales Order Number"].ToString() + "'," +
							"'" + row["Sales Order Item Number"].ToString() + "','" + row["Order Number"].ToString() + "','" + row["Cost Center1"].ToString() + "','" + row["Asset Number"].ToString() + "','" + row[Relstat].ToString() + "'," +
							"'" + row["D"].ToString() + "','" + row["DCI"].ToString() + "','SAP','" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "'," + dataloaderror + ", '" + Error_Description + "')";
						NpgsqlCommand dbcmd = DB.CreateCommand();
						dbcmd.CommandText = query;
						dbcmd.ExecuteNonQuery();
						
					}
					catch (Exception e)
					{
						var res = e;
						log.ErrorMessage("StagingController", "uploadPoDataExcel", e.StackTrace.ToString());
						continue;
					}
				}

				DB.Close();
				AuditLog auditlog = new AuditLog();
				auditlog.filename = filename;
				auditlog.filelocation = filePath;
				auditlog.uploadedon = DateTime.Now;
				auditlog.uploadedto = "STAG_PO_SAP";
				auditlog.modulename = "uploadPoData";
				loadAuditLog(auditlog);
				loadPOData();
				return Ok(true);
			}
		}

		/*Name of Function : <<loadPOData>>  Author :<<Prasanna>>  
		Date of Creation <<06-07-2020>>
		Purpose : <<Write briefly in one line or two lines>>
		Review Date :<<>>   Reviewed By :<<>>
		Sourcecode Copyright : Yokogawa India Limited
		*/
		public IActionResult loadPOData()
		{
			using (NpgsqlConnection pgsql = new NpgsqlConnection(config.PostgresConnectionString))

			{
				{

					string query = "select * from wms.stag_po_sap where purchdoc !='' and material !='' and poquantity !=0  ";
					pgsql.Open();
					var stagingList = pgsql.Query<StagingModel>(
					   query, null, commandType: CommandType.Text);
					foreach (StagingModel stag_data in stagingList)
					{
						try
						{
							stag_data.pono = stag_data.purchdoc;
							stag_data.deliverydate = stag_data.itemdeliverydate;
							stag_data.vendorid = Convert.ToInt32(stag_data.vendor);
							stag_data.suppliername = stag_data.vendorname;
							stag_data.projectcode = stag_data.projectdefinition;
							stag_data.materialid = stag_data.material;
							stag_data.materialqty = stag_data.poquantity;
							stag_data.itemno = stag_data.item;
							string materialdescquery = WMSResource.getMateDescr.Replace("#materialid", stag_data.materialid.ToString());
							stag_data.materialdescription = pgsql.QuerySingleOrDefault<string>(
											materialdescquery, null, commandType: CommandType.Text);

							string query1 = "Select Count(*) as count from wms.wms_polist where pono = '" + stag_data.purchdoc + "'";
							int pocount = int.Parse(pgsql.ExecuteScalar(query1, null).ToString());
							if (pocount == 0)
							{
								//insert wms_polist ##pono,deliverydate,vendorid,supliername
								var insertquery = "INSERT INTO wms.wms_polist(pono, deliverydate, vendorid,suppliername)VALUES(@pono, @deliverydate, @vendorid,@suppliername)";
								var results = pgsql.ExecuteScalar(insertquery, new
								{
									stag_data.pono,
									stag_data.deliverydate,
									stag_data.vendorid,
									stag_data.suppliername
								});

							}

							string query2 = "Select Count(*) as count from wms.wms_project where pono = '" + stag_data.purchdoc + "'";
							int Projcount = int.Parse(pgsql.ExecuteScalar(query2, null).ToString());

							if (Projcount == 0)
							{
								//insert wms_project ##pono,jobname,projectcode,projectname,projectmanager,
								var insertquery = "INSERT INTO wms.wms_project(pono, jobname, projectcode,projectname,projectmanager)VALUES(@pono, @jobname,@projectcode,@projectname,@projectmanager)";
								var results = pgsql.ExecuteScalar(insertquery, new
								{
									stag_data.pono,
									stag_data.jobname,
									stag_data.projectcode,
									stag_data.projectname,
									stag_data.projectmanager,
								});

							}

							string query3 = "Select Count(*) as count from wms.wms_pomaterials where pono = '" + stag_data.purchdoc + "' and materialid='" + stag_data.material + "'";
							int matcount = int.Parse(pgsql.ExecuteScalar(query3, null).ToString());

							if (matcount == 0)
							{
								//insert wms_pomaterials ##pono,materialid,materialdescr,materilaqty,itemno,itemamount,item deliverydate,
								var insertquery = "INSERT INTO wms.wms_pomaterials(pono, materialid, materialdescription,materialqty,itemno,itemamount,itemdeliverydate)VALUES(@pono, @materialid, @materialdescription,@materialqty,@itemno,@itemamount,@itemdeliverydate)";
								var results = pgsql.ExecuteScalar(insertquery, new
								{
									stag_data.pono,
									stag_data.materialid,
									stag_data.materialdescription,
									stag_data.materialqty,
									stag_data.itemno,
									stag_data.itemamount,
									stag_data.itemdeliverydate
								});
							}
							else
							{
								var id = pgsql.QuerySingleOrDefault<string>("Select id  from wms.wms_pomaterials where pono = '" + stag_data.purchdoc + "' and materialid='" + stag_data.material + "'", null, commandType: CommandType.Text);
								var updateqyery = "update wms.wms_pomaterials set materialqty = @materialqty where id=" + id + "";

								var re = Convert.ToInt32(pgsql.Execute(updateqyery, new

								{
									stag_data.materialqty

								}));



							}
						}
						catch (Exception e)
						{
							var res = e;
							log.ErrorMessage("StagingController", "loadPOData", e.StackTrace.ToString());
							continue;
						}
					}
					pgsql.Close();

					//throw new NotImplementedException();
				}
			}
			return Ok(true);

		}

		private DataTable GetDataTable(string sql, string connectionString)
		{
			DataTable dt = new DataTable();

			using (OleDbConnection conn = new OleDbConnection(connectionString))
			{
				conn.Open();
				using (OleDbCommand cmd = new OleDbCommand(sql, conn))
				{
					using (OleDbDataReader rdr = cmd.ExecuteReader())
					{
						dt.Load(rdr);
						return dt;
					}
				}
			}
		}


		[HttpGet]
		[Route("uploadInitialStock")]
		public IActionResult uploadInitialStock()
		{
			//loadStockData();
			return Ok(true);
		}


			/*Name of Function : <<uploadInitialStockExcel>>  Author :<<Prasanna>>  
			Date of Creation <<16-09-2020>>
			Purpose : <<Write briefly in one line or two lines>>
			Review Date :<<>>   Reviewed By :<<>>
			Sourcecode Copyright : Yokogawa India Limited
			*/

	    [HttpGet]
		[Route("uploadInitialStockExcel")]
		public  WMSHttpResponse uploadInitialStockExcel()
		{

			WMSHttpResponse result = new WMSHttpResponse();


			using (NpgsqlConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
			{

				DB.Open();
				string serverPath = "";
				string rows = "";
				int rowsinserted = 0;
				try
				{
					//serverPath = @"\\ZAWMS-001\StockExcel\";
					//using (new NetworkConnection(serverPath, new NetworkCredential(@"administrator", "Wms@1234*")))
					//{


						var filePath = @"D:\A_StagingTable\initialStockUploadv1.xlsx";
						//var filePath = serverPath+ "StockstagecsvTest1.xlsx";


						DataTable dtexcel = new DataTable();
						bool hasHeaders = true;
						string HDR = hasHeaders ? "Yes" : "No";
						string strConn;
						if (filePath.Substring(filePath.LastIndexOf('.')).ToLower() == ".xlsx")
							strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePath + ";Extended Properties=\"Excel 12.0;HDR=" + HDR + ";IMEX=1\"";
						else
							strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filePath + ";Extended Properties=\"Excel 8.0;HDR=" + HDR + ";IMEX=1\"";


						OleDbConnection conn = new OleDbConnection(strConn);
						conn.Open();
						DataTable schemaTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

						DataRow schemaRow = schemaTable.Rows[0];
						string sheet = schemaRow["TABLE_NAME"].ToString();
						if (!sheet.EndsWith("_"))
						{

							string query = "SELECT  * FROM [" + sheet + "]";
							OleDbDataAdapter daexcel = new OleDbDataAdapter(query, conn);
							dtexcel.Locale = CultureInfo.CurrentCulture;
							daexcel.Fill(dtexcel);


						}


						conn.Close();
						rows = dtexcel.Rows.Count.ToString();
						foreach (DataRow row in dtexcel.Rows)
						{
							
						    string Error_Description = "";
						    bool dataloaderror = false;

							initialStock initialstk = new initialStock();
							initialstk.material = Conversion.toStr(row["Material"]);
							initialstk.materialdescription = Conversion.toStr(row["Material Description"]);
							initialstk.store = Conversion.toStr(row["Store"]);
							initialstk.rack = Conversion.toStr(row["Rack"]);
							initialstk.bin = Conversion.toStr(row["Bin"]);
							initialstk.quantity = Conversion.toInt(row["Quantity"]);
							initialstk.projectid = Conversion.toStr(row["Project Id"]);
							initialstk.pono = Conversion.toStr(row["PO No"]);
							initialstk.value = Conversion.Todecimaltype(row["Value"]);
							initialstk.grn = Conversion.toStr(row["GRN"]);
						    initialstk.receiveddate = Conversion.TodtTime(row["Received date"]);
						    initialstk.shelflifeexpiration = Conversion.TodtTime(row["Shelf life expiration"]);
						    initialstk.dateofmanufacture = Conversion.TodtTime(row["Date of Manufacture"]);
						    initialstk.dataenteredon = Conversion.TodtTime(row["Data Entered On"]);
							initialstk.datasource = Conversion.toStr(row["DataSource"]);
							initialstk.dataenteredby = Conversion.toStr(row["Data Entered By"]); ;
							initialstk.createddate = System.DateTime.Now;
							initialstk.stocktype = null;
							initialstk.category = null;
						initialstk.unitprice = null;


						if (string.IsNullOrEmpty(initialstk.material) || initialstk.material == "")
									Error_Description += " There is NO Material ";
								if (string.IsNullOrEmpty(initialstk.materialdescription) || initialstk.materialdescription == "")
									Error_Description += " No material description";
								if (string.IsNullOrEmpty(initialstk.store) || initialstk.store == "")
									Error_Description += " No Store";
								if (string.IsNullOrEmpty(initialstk.rack) || initialstk.rack == "")
									Error_Description += " No Rack";
								if (string.IsNullOrEmpty(initialstk.bin) || initialstk.bin == "")
									Error_Description += " No Bin";
								if (initialstk.quantity == null || initialstk.quantity == 0)
									Error_Description += " No Quantity";
							   if (string.IsNullOrEmpty(initialstk.projectid) || initialstk.projectid == "")
								    Error_Description += " No Project Id";
							   if (string.IsNullOrEmpty(initialstk.pono) || initialstk.pono == "")
								    Error_Description += " No PONo";
							if (!string.IsNullOrEmpty(Error_Description))
									dataloaderror = true;

							initialstk.DataloadErrors = dataloaderror;
							initialstk.error_description = Error_Description;


							string insertpoqry = WMSResource.InsertInitialStock;
							var rslt = DB.Execute(insertpoqry, new
							{
								initialstk.material,
								initialstk.materialdescription,
								initialstk.store,
								initialstk.rack,
								initialstk.bin,
								initialstk.quantity,
								initialstk.grn,
								initialstk.receiveddate,
								initialstk.shelflifeexpiration,
								initialstk.dateofmanufacture,
								initialstk.dataenteredon,
								initialstk.datasource,
								initialstk.dataenteredby,
								initialstk.createddate,
								initialstk.DataloadErrors,
								initialstk.error_description,
								initialstk.stocktype,
								initialstk.category,
								initialstk.unitprice,
								initialstk.projectid,
								initialstk.pono,
								initialstk.value
							});

							rowsinserted = rowsinserted + 1;
							
						}

						DB.Close();
						result.message = "Completed_Total_Rows_" + rows + "_Inserted_rows_" + rowsinserted.ToString();
						return result;

					//}
					
					
				}
				catch (Exception ex)
				{
					DB.Close();
					result.message = ex.Message;
					return result;
				}

				

              
				
			}
		}


		[HttpPost("uploadInitialStockExcelByUser"), DisableRequestSizeLimit]
		public WMSHttpResponse uploadInitialStockExcelByUser()
		{

			WMSHttpResponse result = new WMSHttpResponse();


			using (NpgsqlConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
			{

				DB.Open();
				string serverPath = "";
				string rows = "";
				int rowsinserted = 0;
				int exceptionrows = 0;
				string uploadcode = Guid.NewGuid().ToString();
				try
				{
					
					var postedfile = Request.Form.Files[0];
					string filename = postedfile.FileName;
					int index = filename.IndexOf('_');
					string uploadedby = filename.Substring(0, index);
					string uploadedfilename = filename.Substring(index+1);
					var folderName = Path.Combine("Resources", "documents");
					var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

					if (postedfile.Length > 0)
					{
						var fileName = ContentDispositionHeaderValue.Parse(postedfile.ContentDisposition).FileName.Trim('"');
						var fullPath = Path.Combine(pathToSave, fileName);
						var dbPath = Path.Combine(folderName, fileName);

						using (var stream = new FileStream(fullPath, FileMode.Create))
						{
							postedfile.CopyTo(stream);
						}

					}

					string path = Environment.CurrentDirectory + @"\Resources\documents\";

					var filePath = path + filename;



					DataTable dtexcel = new DataTable();
					bool hasHeaders = true;
					string HDR = hasHeaders ? "Yes" : "No";
					string strConn;
					if (filename.Substring(filename.LastIndexOf('.')).ToLower() == ".xlsx")
						strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePath + ";Extended Properties=\"Excel 12.0;HDR=" + HDR + ";IMEX=1\"";
					else
						strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filePath + ";Extended Properties=\"Excel 8.0;HDR=" + HDR + ";IMEX=1\"";


					OleDbConnection conn = new OleDbConnection(strConn);
					conn.Open();
					DataTable schemaTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

					DataRow schemaRow = schemaTable.Rows[0];
					string sheet = schemaRow["TABLE_NAME"].ToString();
					if (!sheet.EndsWith("_"))
					{

						string query = "SELECT  * FROM [" + sheet + "]";
						OleDbDataAdapter daexcel = new OleDbDataAdapter(query, conn);
						dtexcel.Locale = CultureInfo.CurrentCulture;
						daexcel.Fill(dtexcel);


					}


					conn.Close();
					rows = dtexcel.Rows.Count.ToString();
					string Error_Description_all = "";
					int i = 1;
					DateTime createdate = DateTime.Now; 
					foreach (DataRow row in dtexcel.Rows)
					{

						string Error_Description = "";
						bool dataloaderror = false;

						initialStock initialstk = new initialStock();
						initialstk.material = Conversion.toStr(row["Material"]);
						initialstk.materialdescription = Conversion.toStr(row["Material Description"]);
						initialstk.store = Conversion.toStr(row["Store"]);
						initialstk.rack = Conversion.toStr(row["Rack"]);
						initialstk.bin = Conversion.toStr(row["Bin"]);
						initialstk.quantity = Conversion.toInt(row["Quantity"]);
						initialstk.projectid = Conversion.toStr(row["Project Id"]);
						initialstk.pono = Conversion.toStr(row["PO No"]);
						initialstk.value = Conversion.Todecimaltype(row["Value"]);
						initialstk.grn = Conversion.toStr(row["GRN"]);
						initialstk.receiveddate = Conversion.TodtTime(row["Received date"]);
						initialstk.shelflifeexpiration = Conversion.TodtTime(row["Shelf life expiration"]);
						initialstk.dateofmanufacture = Conversion.TodtTime(row["Date of Manufacture"]);
						initialstk.dataenteredon = Conversion.TodtTime(row["Data Entered On"]);
						initialstk.datasource = Conversion.toStr(row["DataSource"]);
						initialstk.dataenteredby = Conversion.toStr(row["Data Entered By"]); ;
						initialstk.createddate = createdate;
						initialstk.uploadedfilename = uploadedfilename;
						if (string.IsNullOrEmpty(initialstk.projectid) || initialstk.projectid == "")
                        {
							initialstk.stocktype = "Plant Stock";
						}
                        else
                        {
							initialstk.stocktype = "Project Stock";
						}
							
						initialstk.unitprice = null;
						initialstk.category = null;
						initialstk.uploadedby = uploadedby;
						initialstk.uploadbatchcode = uploadcode;


						if (string.IsNullOrEmpty(initialstk.material) || initialstk.material == "")
							Error_Description += " No Material";
						if (string.IsNullOrEmpty(initialstk.materialdescription) || initialstk.materialdescription == "")
							Error_Description += " No Material Description";
						if (string.IsNullOrEmpty(initialstk.store) || initialstk.store == "")
							Error_Description += " No Store";
						if (string.IsNullOrEmpty(initialstk.rack) || initialstk.rack == "")
							Error_Description += " No Rack";
						if (string.IsNullOrEmpty(initialstk.bin) || initialstk.bin == "")
							Error_Description += " No Bin";
						if (initialstk.quantity == null || initialstk.quantity == 0)
							Error_Description += " No Quantity";
						if (string.IsNullOrEmpty(initialstk.projectid) || initialstk.projectid == "")
							Error_Description += " No Project Id";
						if (string.IsNullOrEmpty(initialstk.pono) || initialstk.pono == "")
							Error_Description += " No PONo";
						if (!string.IsNullOrEmpty(Error_Description))
                        {
							dataloaderror = true;
							exceptionrows = exceptionrows + 1;
							Error_Description_all +=  Error_Description + " For Row " + i.ToString()+"-";

						}
						i++;
							

						initialstk.DataloadErrors = dataloaderror;
						initialstk.error_description = Error_Description;


						string insertpoqry = WMSResource.InsertInitialStock;
                        var rslt = DB.Execute(insertpoqry, new
                        {
                            initialstk.material,
                            initialstk.materialdescription,
                            initialstk.store,
                            initialstk.rack,
                            initialstk.bin,
                            initialstk.quantity,
                            initialstk.grn,
                            initialstk.receiveddate,
                            initialstk.shelflifeexpiration,
                            initialstk.dateofmanufacture,
                            initialstk.dataenteredon,
                            initialstk.datasource,
                            initialstk.dataenteredby,
                            initialstk.createddate,
                            initialstk.DataloadErrors,
                            initialstk.error_description,
                            initialstk.stocktype,
                            initialstk.category,
                            initialstk.unitprice,
                            initialstk.projectid,
                            initialstk.pono,
                            initialstk.value,
							initialstk.uploadedby,
							initialstk.uploadbatchcode,
							initialstk.uploadedfilename
                        });

                        rowsinserted = rowsinserted + 1;

					}

					DB.Close();
					//result.message += "-Total_Rows_:" + rows + "-Inserted_rows_to_staging_table_:" + rowsinserted.ToString();
					result.message += "-Total Records_:" + rows;
					AuditLog auditlog = new AuditLog();
					auditlog.filename = uploadedfilename;
					auditlog.uploadedon = createdate;
					auditlog.uploadedby = uploadedby;
					auditlog.uploadedto = "st_initialstock";
					auditlog.modulename = "initialstock";
					loadAuditLog(auditlog);
					string msg = loadStockData(uploadcode);
					result.message += msg;
					if (string.IsNullOrEmpty(Error_Description_all))
					{
						result.message += "$EX$Exception Records: 0";
					}
					else
					{
						result.message += "$EX$Exception Records:" + exceptionrows + "";
					}
					result.message += "$viewdatalistcode$" + uploadcode;
					return result;

					//}


				}
				catch (Exception ex)
				{
					DB.Close();
					result.message = ex.Message;
					return result;
				}





			}
		}

		/*
		function : <<loadStockData>>  Author :<<Prasanna>>  
		Date of Creation <<16-09-2020>>
		Purpose : <<Write briefly in one line or two lines>>
		Review Date :<<>>   Reviewed By :<<>>
		Sourcecode Copyright : Yokogawa India Limited
		*/
		public string loadStockData(string batchcode)
		{
			string insertmessage = "";
			using (NpgsqlConnection pgsql = new NpgsqlConnection(config.PostgresConnectionString))

			{
				{

					string query = "select * from wms.st_initialstock where material  !='' and materialdescription !='' and store !='' and rack !='' and quantity  !='0' and dataloaderrors=false and uploadbatchcode = '"+batchcode+"'";
					pgsql.Open();
                    var stagingList = pgsql.Query<StagingStockModel>(
                       query, null, commandType: CommandType.Text);

					int rowinserted = 0;
					string uploadcode = batchcode;

					DateTime currentdate = DateTime.Now;
					
                    foreach (StagingStockModel stag_data in stagingList)
					{
						NpgsqlTransaction Trans = null;
						try
						{
							// add master table data for store,rack,bin

							Trans = pgsql.BeginTransaction();
							bool deleteflag = false;
							//Add locator in masterdata
							string storeQuery = "Select locatorid from wms.wms_rd_locator where locatorname = '" + stag_data.store + "'";
							var storeId =pgsql.ExecuteScalar(storeQuery, null);
							if (storeId == null)
							{
								LocationModel store = new LocationModel();
								store.locatorname = stag_data.store;
								store.createdate = DateTime.Now;
								store.isexcelupload = true;
								//insert wms_rd_locator ##locatorname
								var insertStorequery = "INSERT INTO wms.wms_rd_locator(locatorid, locatorname, createdate,deleteflag,isexcelupload)VALUES(default, @locatorname,@createdate,@deleteflag,@isexcelupload) returning locatorid";
								var Storeresults = pgsql.ExecuteScalar(insertStorequery, new
								{
									store.locatorname,
									store.createdate,
									deleteflag,
									store.isexcelupload
								});
								storeId = Convert.ToInt32(Storeresults);
							}
							
							//Add rack masterdata
							string rackQuery = "Select rackid from wms.wms_rd_rack where racknumber = '" + stag_data.rack + "' and locatorid=" + storeId + "";
							var rackId = pgsql.ExecuteScalar(rackQuery, null);
							if (rackId == null)
							{
								LocationModel store = new LocationModel();
								store.racknumber = stag_data.rack;
								store.locatorid = Convert.ToInt32(storeId);
								store.createdate = DateTime.Now;
								store.isexcelupload = true;
								//insert wms_rd_locator ##locatorname
								var insertRackquery = "INSERT INTO wms.wms_rd_rack(rackid,racknumber, locatorid,createdate,deleteflag,isexcelupload)VALUES(default,@racknumber,@locatorid,@createdate,@deleteflag,@isexcelupload)returning rackid";
								var rackresults = pgsql.ExecuteScalar(insertRackquery, new
								{
									store.racknumber,
									store.locatorid,
									store.createdate,
									deleteflag,
									store.isexcelupload
								});
								rackId = Convert.ToInt32(rackresults);
							}

							//Add Bin masterdata if not exist
								string binQuery = "Select binid from wms.wms_rd_bin where binnumber = '" + stag_data.bin + "' and locatorid=" + storeId + " and rackid=" + rackId + "";
								var binId = pgsql.ExecuteScalar(binQuery, null);
								if (binId == null && (stag_data.bin != null && stag_data.bin != ""))
								{
									LocationModel store = new LocationModel();
									store.binnumber = stag_data.bin;
									store.locatorid = Convert.ToInt32(storeId);
									store.rackid = Convert.ToInt32(rackId);
									store.createdate = DateTime.Now;
									store.isexcelupload = true;
									//insert wms_rd_locator ##locatorname
									var insertbinQuery = "INSERT INTO wms.wms_rd_bin(binid,binnumber, locatorid,rackid,createdate,deleteflag,isexcelupload)VALUES(default,@binnumber,@locatorid,@rackid,@createdate,@deleteflag,@isexcelupload) returning binid";
									var binresults = pgsql.ExecuteScalar(insertbinQuery, new
									{
										store.binnumber,
										store.locatorid,
										store.rackid,
										store.createdate,
										deleteflag,
										store.isexcelupload
									});
									binId = Convert.ToInt32(binresults);
									
								}

							
							



							//Add material master data
							string materialQuery = "Select material from wms.\"MaterialMasterYGS\" where material = '" + stag_data.material + "'";
							var materialid = pgsql.ExecuteScalar(materialQuery, null);
							if (materialid == null)
							{
								LocationModel store = new LocationModel();
								store.materialid = stag_data.material;
								store.materialdescription = stag_data.materialdescription;
								store.isexcelupload = true;
								store.locatorid = Convert.ToInt32(storeId);
								store.rackid = Convert.ToInt32(rackId);
								int? binid = null;
								bool qualitycheck = false;
								if (binId != null)
                                {
									binid = Convert.ToInt32(binId);
								}
								//insert wms_rd_locator ##locatorname
								int rslt = 0;
								var insertStorequery = "INSERT INTO wms.\"MaterialMasterYGS\" (material, materialdescription, storeid,rackid,binid,qualitycheck,stocktype,unitprice)VALUES(@materialid, @materialdescription,@locatorid,@rackid,@binid,@qualitycheck,@stocktype,@unitprice)";
								rslt = pgsql.Execute(insertStorequery, new
								{
									store.materialid,
									store.materialdescription,
									store.locatorid,
									store.rackid,
									binid,
									qualitycheck,
									stag_data.stocktype,
									stag_data.unitprice


								});
							}


							StockModel stock = new StockModel();
							stock.storeid = Convert.ToInt32(storeId);
							stock.rackid = Convert.ToInt32(rackId);
							stock.binid = Convert.ToInt32(binId);
							stock.totalquantity = stag_data.quantity;
							stock.availableqty = stag_data.quantity;
							stock.shelflife = stag_data.shelflifeexpiration;
							stock.createddate = currentdate;
							stock.materialid = stag_data.material;
							stock.initialstock = true;
							string itemlocation = stag_data.store + "." + stag_data.rack;
							if(stag_data.bin != "" && stag_data.bin != null)
                            {
								itemlocation += "." + stag_data.bin;

							}
							int? bindata = null;
							if(stock.binid > 0)
                            {
								bindata = stock.binid;

							}

							//insert wms_stock ##storeid, binid,rackid,totalquantity,shelflife ,createddate,materialid ,initialstock
							var insertquery = "INSERT INTO wms.wms_stock(storeid, binid,rackid,itemlocation,totalquantity,availableqty,shelflife ,createddate,materialid ,initialstock,stcktype,unitprice,value,pono,projectid,createdby,uploadbatchcode,uploadedfilename)VALUES(@storeid, @bindata,@rackid,@itemlocation,@totalquantity,@availableqty,@shelflife ,@createddate,@materialid ,@initialstock,@stocktype,@unitprice,@value,@pono,@projectid,@uploadedby,@uploadcode,@uploadedfilename)";
							var results = pgsql.ExecuteScalar(insertquery, new
							{
								stock.storeid,
								bindata,
								stock.rackid,
								itemlocation,
								stock.totalquantity,
								stock.availableqty,
								stock.shelflife,
								stock.createddate,
								stock.materialid,
								stock.initialstock,
								stag_data.stocktype,
								stag_data.unitprice,
								stag_data.value,
								stag_data.pono,
								stag_data.projectid,
								stag_data.uploadedby,
								uploadcode,
								stag_data.uploadedfilename
							});

							Trans.Commit();
							rowinserted = rowinserted + 1;

							


						}
						catch (Exception e)
						{
							Trans.Rollback();
							var res = e;
							insertmessage += e.Message.ToString();
							log.ErrorMessage("StagingController", "loadStockData", e.StackTrace.ToString());
							continue;
						}
					}
					//insertmessage = "-To Database - Inserted_rows_to_Stock_Table:" + rowinserted.ToString();
					insertmessage = "-Success Records:" + rowinserted.ToString();
					pgsql.Close();

					//throw new NotImplementedException();
				}
			}
			return insertmessage;

		}

		[HttpGet]
		[Route("uploadDataExcel")]
		public IActionResult uploadDataExcel()
		{
			using (NpgsqlConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
			{

				DB.Open();
				//var filePath = @"D:\Projects\WMS\Docs\label data\ZLMMP00001_ListofPO.xlsx";
				var filePath = @"D:\A_StagingTable\ZLMMP00001_ListofPO.xlsx";
				var filePath1 = @"D:\A_StagingTable\ZGSDR00006_QTSO-Sept-Oct2020.xlsx";
				var filePath2 = @"D:\A_StagingTable\ZGMMR02023_slno_imports.xlsx";


				/////////for audit purpose/////////
				string[] file1arr = new string[3];
				file1arr[0] = filePath;
				file1arr[1] = filePath1;
				file1arr[2] = filePath1;
				/////////for audit purpose/////////




				DataTable dtexcel = new DataTable();
				DataTable dtexcel1 = new DataTable();
				DataTable dtexcel2 = new DataTable();
				

				// For .net core, the next line requires the NuGet package, 
				// System.Text.Encoding.CodePages
				System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
				using (var stream1 = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
				{
					using (var reader = ExcelReaderFactory.CreateReader(stream1))
					{

						// 2. Use the AsDataSet extension method
						var result = reader.AsDataSet(new ExcelDataSetConfiguration()
						{
							ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
							{
								UseHeaderRow = true
							}
						});

						// The result of each spreadsheet is in result.Tables
						dtexcel = result.Tables[0];

					}
				}

				using (var stream2 = System.IO.File.Open(filePath1, FileMode.Open, FileAccess.Read))
				{
					using (var reader = ExcelReaderFactory.CreateReader(stream2))
					{

						// 2. Use the AsDataSet extension method
						var result = reader.AsDataSet(new ExcelDataSetConfiguration()
						{
							ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
							{
								UseHeaderRow = true
							}
						});

						// The result of each spreadsheet is in result.Tables
						dtexcel1 = result.Tables[0];

					}
				}

				using (var stream3 = System.IO.File.Open(filePath2, FileMode.Open, FileAccess.Read))
				{
					using (var reader = ExcelReaderFactory.CreateReader(stream3))
					{

						// 2. Use the AsDataSet extension method
						var result = reader.AsDataSet(new ExcelDataSetConfiguration()
						{
							ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
							{
								UseHeaderRow = true
							}
						});

						// The result of each spreadsheet is in result.Tables
						dtexcel2 = result.Tables[0];

					}
				}





				int i = 0;
				string upcode = Guid.NewGuid().ToString();
				foreach (DataRow row in dtexcel.Rows)
				{

					string Error_Description = "";
					bool dataloaderror = false;

					MateriallabelModel model = new MateriallabelModel();
					model.po = Conversion.toStr(row["Purch.Doc."]);
					model.polineitemno = Conversion.toStr(row["Item"]);
					model.description = Conversion.toStr(row["Description"]);
					model.mscode = Conversion.toStr(row["MS Code"]);
					model.saleorderno = Conversion.toStr(row["Sales Order Number"]);
					model.solineitemno = Conversion.toStr(row["Sales Order Item Number"]);
					model.linkageno = Conversion.toStr(row["Linkage Number"]);
					model.codetype = Conversion.toStr(row["A"]);
					model.uploadcode = upcode;

					if (string.IsNullOrEmpty(model.po))
						Error_Description += " No pono";
					if (string.IsNullOrEmpty(model.polineitemno))
						Error_Description += " No po line item";
					
					if (!string.IsNullOrEmpty(Error_Description))
					{
						dataloaderror = true;
						model.error_description = Error_Description;
						model.isloaderror = dataloaderror;

					}
					string materialQuery = "Select po from wms.WMS_ST_MaterialLabel where po = '" + model.po + "' and polineitemno = '" + model.polineitemno + "' ";
					var materialid = DB.ExecuteScalar(materialQuery, null);
					if(materialid == null)
                    {
						if (!string.IsNullOrEmpty(model.saleorderno) && !string.IsNullOrEmpty(model.solineitemno))
						{
							/////2023
							DataRow[] dr = dtexcel2.Select("[Sales Document] = '" + model.saleorderno + "' AND [Sales Document Item] = '" + model.solineitemno + "'");
							if (dr.Length > 0)
							{
								model.material = Conversion.toStr(dr[0]["Material Number"]);
								model.gr = Conversion.toStr(dr[0]["Storage Location"]);
								model.plant = Conversion.toStr(dr[0]["Plant"]);
								//model.serialno = Conversion.toStr(dr[0]["Serial Number"]);
							}
							/////0006
							DataRow[] dr1 = dtexcel1.Select("[Sales Document No.] = '" + model.saleorderno + "' AND [Sales Order Item No.] = '" + model.solineitemno + "'");
							if (dr1.Length > 0)
							{
								model.saleordertype = Conversion.toStr(dr1[0]["Sales Document Type"]);
								model.customername = Conversion.toStr(dr1[0]["Sold-to party"]) + " " + Conversion.toStr(dr1[0]["Name: Sold-to party"]);
								model.shipto = Conversion.toStr(dr1[0]["Ship-to party"]) + " " + Conversion.toStr(dr1[0]["Name: Ship-to party"]);
								model.shippingpoint = Conversion.toStr(dr1[0]["Shipping Point"]) + " " + Conversion.toStr(dr1[0]["Text: Shipping Point"]);
								model.loadingdate = Conversion.TodtTime(dr1[0]["Planned Billing Date"]);
								model.projectiddef = Conversion.toStr(dr1[0]["Project definition(level 0)"]);
								model.partno = Conversion.toStr(dr1[0]["Material"]);
								model.custpo = Conversion.toStr(dr1[0]["PO number"]);


							}
						}

						//@po,@polineitemno,@serialno,@material,@mscode,@saleorderno,@solineitemno,@saleordertype,@insprec,@linkageno,@customername,@shipto,@plant,
						//@gr,@shippingpoint,@projectiddef,@loadingdate,@custpo,@partno,@grnno,@codetype,@description

						string insertpoqry = WMSResource.materiallablestaginginsert;
						var rslt = DB.Execute(insertpoqry, new
						{
							model.po,
							model.polineitemno,
							model.serialno,
							model.material,
							model.mscode,
							model.saleorderno,
							model.solineitemno,
							model.saleordertype,
							model.insprec,
							model.linkageno,
							model.customername,
							model.shipto,
							model.plant,
							model.gr,
							model.shippingpoint,
							model.projectiddef,
							model.loadingdate,
							model.custpo,
							model.partno,
							model.grno,
							model.codetype,
							model.description,
							model.error_description,
							model.isloaderror,
							model.uploadcode
						});

					}


					





				}


				foreach(string str in file1arr)
                {
					string[] filearr = str.Split("\\");
					string nameoffile = filearr[filearr.Length - 1];
					AuditLog auditlog = new AuditLog();
					auditlog.filename = nameoffile;
					auditlog.filelocation = str;
					auditlog.uploadedon = DateTime.Now;
					auditlog.uploadedto = "wms_st_materiallabel";
					auditlog.modulename = "materiallabelinfo";
					loadAuditLog(auditlog);

				}
				string rsltx = loadlabeldatatobase(upcode);
				return Ok(true);

				
			}
		}


		/*
		function : <<loadlabeldatatobase>>  Author :<<Ramesh>>  
		Date of Creation <<20-11-2020>>
		Purpose : <<From materiallabel staging to base table>>
		Review Date :<<>>   Reviewed By :<<>>
		Sourcecode Copyright : Yokogawa India Limited
		*/
		public string loadlabeldatatobase(string batchcode)
		{
			string insertmessage = "";
			using (NpgsqlConnection pgsql = new NpgsqlConnection(config.PostgresConnectionString))

			{
					string query = "select * from wms.WMS_ST_MaterialLabel  where isloaderror is NOT True and uploadcode = '" + batchcode + "'";
					pgsql.Open();
					var stagingList = pgsql.Query<MateriallabelModel>(
					   query, null, commandType: CommandType.Text);

					int rowinserted = 0;
					string uploadcode = batchcode;

					DateTime currentdate = DateTime.Now;

					foreach (MateriallabelModel stag_data in stagingList)
					{
						NpgsqlTransaction Trans = null;
						try
						{
							int itmno = Convert.ToInt32(stag_data.polineitemno);
							string materialQuery = "Select * from wms.wms_pomaterials where pono = '" + stag_data.po + "' and itemno ="+ itmno;
							var podata = pgsql.QueryFirstOrDefault<MateriallabelModel>(materialQuery, null, commandType: CommandType.Text);
							if (podata == null)
							{
								
								int rslt = 0;
								string podescription = stag_data.description;
								stag_data.soldto = stag_data.customername;
								var updateqry = WMSResource.updatepoformatlabel.Replace("#idx", podata.id.ToString()); ;
								rslt = pgsql.Execute(updateqry, new
								{
									podescription,
									stag_data.mscode,
									stag_data.saleorderno,
									stag_data.solineitemno,
									stag_data.linkageno,
									stag_data.material,
									stag_data.plant,
									stag_data.saleordertype,
									stag_data.customername,
									stag_data.shippingpoint,
									stag_data.loadingdate,
									stag_data.gr,
									stag_data.projectiddef,
									stag_data.partno,
									stag_data.custpo,
									stag_data.grno,
									stag_data.codetype,
									stag_data.shipto,
									stag_data.soldto,
									stag_data.uploadcode,

								});
							}

							rowinserted = rowinserted + 1;

						}
						catch (Exception e)
						{
							var res = e;
							insertmessage += e.Message.ToString();
							log.ErrorMessage("StagingController", "loadmatlabelDatatobase", e.Message.ToString());
							continue;
						}
					}
					insertmessage = rowinserted.ToString();
					pgsql.Close();
			}
			return insertmessage;

		}


		/*
		function : <<loadAuditLog>>  Author :<<Ramesh>>  
		Date of Creation <<19-11-2020>>
		Purpose : <<Contains metadata of staging process>>
		Review Date :<<>>   Reviewed By :<<>>
		Sourcecode Copyright : Yokogawa India Limited
		*/

		public void loadAuditLog(AuditLog logdata)
		{
			
			using (NpgsqlConnection pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
						try
						{
					        var insertquery = WMSResource.insertauditlog;
							var results = pgsql.ExecuteScalar(insertquery, new
							{
								logdata.filename,
								logdata.filelocation,
								logdata.uploadedon,
								logdata.uploadedby,
								logdata.uploadedto,
								logdata.modulename
							});




						}
						catch (Exception e)
						{
							var res = e;
							log.ErrorMessage("StagingController", "loadStockData", e.StackTrace.ToString());
							
						}
                finally
                {
					pgsql.Close();

				}
					

					
				
			}
			

		}
	}
}
