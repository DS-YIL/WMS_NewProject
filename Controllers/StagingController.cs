/*
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
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WMS.Common;
using WMS.Models;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace WMS.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class StagingController : ControllerBase
	{
		Configurations config = new Configurations();
		ErrorLogTrace log = new ErrorLogTrace();
		string url = "";
		private readonly IHttpContextAccessor _httpContextAccessor;
		public StagingController(IHttpContextAccessor _httpContextAccessor)
		{
			this._httpContextAccessor = _httpContextAccessor;
			url = _httpContextAccessor.HttpContext.Request.Host + _httpContextAccessor.HttpContext.Request.Path;
		}
		/*Name of Function : <<uploadExcel>>  Author :<<Prasanna>>  
		Date of Creation <<02-07-2020>>
		Purpose : <<fill Open  podata from ygs SAP excel to staging table>>
		Review Date :<<>>   Reviewed By :<<>>
		Sourcecode Copyright : Yokogawa India Limited
		*/


		[HttpGet]
		[Route("uploadPoDataExcel")]
		public IActionResult uploadPoDataExcel()
		{
			try
			{
				string serverPath = "";
				using (NpgsqlConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					serverPath = config.FilePath;
					var filePath = serverPath + "Yil_Po_Daily_report_" + DateTime.Now.ToString("dd-MM-yyyy").Replace("-", "_") + ".xlsx";
					//Added lines - Gayathri
					var filePath1 = serverPath + "ZGSDR00006_"+ DateTime.Now.ToString("dd-MM-yyyy").Replace("-", "_") + ".xlsx";
					var filePath2 = serverPath +"ZGMMR02023_" + DateTime.Now.ToString("dd-MM-yyyy").Replace("-", "_") + ".xlsx"; 
					//End - Gayathri
					DB.Open();
					var filePathstr = filePath;
					string[] filearr = filePathstr.Split("\\");
					string nameoffile = filearr[filearr.Length - 1];
					DataTable dtexcel = new DataTable();
					//Added lines - Gayathri
					DataTable dtexcel1 = new DataTable();
					DataTable dtexcel2 = new DataTable();
					//End - Gayathri
					string poitem = "";
					System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
					using (var stream1 = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
					{
						using (var reader = ExcelReaderFactory.CreateReader(stream1))
						{

							var result = reader.AsDataSet(new ExcelDataSetConfiguration()
							{
								ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
								{
									UseHeaderRow = true
								}
							});

							dtexcel = result.Tables[0];

						}
					}

					//Added Lines -Gayathri
					if (System.IO.File.Exists(filePath1))
                    {
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
					}

					if (System.IO.File.Exists(filePath2))
					{
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
					}
					//End - gayathri



					//bool hasHeaders = false;
					//string HDR = hasHeaders ? "Yes" : "No";
					//string strConn;
					//if (filePath.Substring(filePath.LastIndexOf('.')).ToLower() == ".xlsx")
					//    strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePath + ";Extended Properties=\"Excel 12.0;HDR=" + HDR + ";IMEX=0\"";
					//else
					//    strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filePath + ";Extended Properties=\"Excel 8.0;HDR=" + HDR + ";IMEX=0\"";


					//OleDbConnection conn = new OleDbConnection(strConn);
					//conn.Open();
					//DataTable schemaTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

					//DataRow schemaRow = schemaTable.Rows[0];
					//string sheet = schemaRow["TABLE_NAME"].ToString();
					//if (!sheet.EndsWith("_"))
					//{
					//    string query = "SELECT  * FROM [Sheet1$]";
					//    OleDbDataAdapter daexcel = new OleDbDataAdapter(query, conn);
					//    dtexcel.Locale = CultureInfo.CurrentCulture;
					//    daexcel.Fill(dtexcel);
					//}

					//conn.Close();
					string uploadcode = Guid.NewGuid().ToString();
					int i = 0;
					foreach (DataRow row in dtexcel.Rows)
					{


						try
						{
							StagingModel model = new StagingModel();
							model.pono = Conversion.toStr(row["Purch.Doc."]);
							model.itemdeliverydate = Conversion.TodtTime(row["Item Delivery Date"]);
							model.materialid = Conversion.toStr(row["Material"]);
							model.poitemdescription = Conversion.toStr(row["Short Text"]);
							model.poquantity = Conversion.Todecimaltype(row["PO Quantity"]);
							model.dci = Conversion.toStr(row["DCI"]);//if blank it is open po
							model.deliveredqty = Conversion.Todecimaltype(row["Delivered Qty"]);//already delivered qty
							model.vendorcode = Conversion.toStr(row["Vendor"]);
							model.vendorname = Conversion.toStr(row["Vendor Name"]);
							model.projectdefinition = Conversion.toStr(row["Project Definition"]);
							model.itemno = Conversion.toInt(row["Item"]);
							model.NetPrice = Conversion.Todecimaltype(row["Net Price(Inhouse)"]);//total value
							model.saleorderno = Conversion.toStr(row["Sales Order Number"]);
							model.solineitemno = Conversion.toStr(row["Sales Order Item Number"]);
							model.saleordertype = Conversion.toStr(row["Type"]);
							model.codetype = Conversion.toStr(row["A"]);
							model.costcenter = Conversion.toStr(row["Cost Center1"]);
							model.assetno = Conversion.toStr(row["Asset Number"]);
							model.projecttext = Conversion.toStr(row["Description"]);
							model.sloc = Conversion.toStr(row["SLoc"]);
							model.pocreatedby = Convert.ToString(row["PO created by (User Id)"]);
							model.mscode = Convert.ToString(row["MS Code"]);
							model.plant = Convert.ToString(row["Plnt"]);
							model.linkageno = Convert.ToString(row["Linkage Number"]);
							model.assetsubno = Convert.ToString(row["Asset Subnumber"]);
							model.orderno = Convert.ToString(row["Order Number"]);

							string Error_Description = "";
							bool dataloaderror = false;
							if (string.IsNullOrEmpty(model.pono.Replace('.', '#')))
								Error_Description += "There is NO PONO";
							if (string.IsNullOrEmpty(model.materialid))
								Error_Description += ", No material";
							if (model.poquantity < 1)
								Error_Description += ", No PO Quantity";
							if (!string.IsNullOrEmpty(Error_Description))
							{
								dataloaderror = true;
								model.dataloaderror = true;
								model.error_description = Error_Description;
							}


							//var query = "INSERT INTO wms.STAG_PO_SAP (PurchDoc,ItemDeliveryDate,Material,POQuantity,Vendor,VendorName,ProjectDefinition,Item,NetPrice,datasource,createddate,DataloadErrors ,Error_Description)VALUES" +
							//    "('" + row["po_no"].ToString() + "'," + "'" + (Convert.ToDateTime(row["item_delivery_date"])).ToString("yyyy-MM-dd") + "','" + row["ms_cd"].ToString() + "','" + row["po_quantity"].ToString() + "','" +
							//    row["vendor_cd"].ToString() + "','" + row["vendor_n"].ToString() + "'," + "'" + row["project_definition"].ToString() + "','" + row["po_item_no"].ToString() + "','" + row["po_item_amt"].ToString() + "','SAP','" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "'," + dataloaderror + ", '" + Error_Description + "')";
							//NpgsqlCommand dbcmd = DB.CreateCommand();
							//dbcmd.CommandText = query;
							//dbcmd.ExecuteNonQuery();
							poitem = model.pono + "-" + model.itemno.ToString();
							string poitemdescription = model.poitemdescription;
							var insertquery = "INSERT INTO wms.STAG_PO_SAP(PurchDoc,pocreatedby,ItemDeliveryDate,Material,poitemdescription,POQuantity,dci,deliveredqty,Vendor,VendorName,ProjectDefinition,Item,NetPrice,datasource,createddate,DataloadErrors ,Error_Description,uploadcode,saleorderno,solineitemno,saleordertype,codetype,costcenter,assetno,projecttext,sloc,mscode,plant,linkageno,assetsubno)";
							insertquery += " VALUES(@pono,@pocreatedby, @itemdeliverydate,@materialid,@poitemdescription,@poquantity,@dci,@deliveredqty,@vendorcode,@vendorname,@projectdefinition,@itemno,@NetPrice,'SAP',current_timestamp,@dataloaderror,@error_description,@uploadcode,@saleorderno,@solineitemno,@saleordertype,@codetype,@costcenter,@assetno,@projecttext,@sloc,@mscode,@plant,@linkageno,@assetsubno)";
							var results = DB.ExecuteScalar(insertquery, new
							{
								model.pono,
								model.pocreatedby,
								model.itemdeliverydate,
								model.materialid,
								poitemdescription,
								model.poquantity,
								model.dci,
								model.deliveredqty,
								model.vendorcode,
								model.vendorname,
								model.projectdefinition,
								model.itemno,
								model.NetPrice,
								dataloaderror,
								model.error_description,
								uploadcode,
								model.saleorderno,
								model.solineitemno,
								model.saleordertype,
								model.codetype,
								model.costcenter,
								model.assetno,
								model.projecttext,
								model.sloc,
								model.mscode,
								model.plant,
								model.assetsubno,
								model.linkageno,
								model.orderno
							});



						}
						catch (Exception e)
						{
							var res = e;
							log.ErrorMessage("StagingController", "uploadPoDataExcel", e.StackTrace.ToString(), "PO:" + poitem + "error:" + e.Message.ToString(), url);
							continue;
						}
					}

					DB.Close();
					AuditLog auditlog = new AuditLog();
					auditlog.filename = nameoffile;
					auditlog.filelocation = filePath;
					auditlog.uploadedon = DateTime.Now;
					auditlog.uploadedto = "STAG_PO_SAP";
					auditlog.modulename = "uploadPoData";


					//Added Gayathri
					//string upcode = Guid.NewGuid().ToString();
					int J = 0;
					if(dtexcel2.Rows.Count>0)
                    {
						foreach (DataRow row in dtexcel2.Rows)
						{

							string Error_Description = "";
							bool dataloaderror = false;
							MateriallabelModel slimports = new MateriallabelModel();
							slimports.saleorderno = Conversion.toStr(row["Sales Document"]);
							slimports.solineitemno = Conversion.toStr(row["Sales Document Item"]);

						slimports.material = Conversion.toStr(row["Material Number"]);
						slimports.gr = Conversion.toStr(row["Storage Location"]);
						slimports.plant = Conversion.toStr(row["Plant"]);
						slimports.serialno = Conversion.toStr(row["Serial Number"]);
						slimports.uploadcode = uploadcode;
						DateTime uploadedon = DateTime.Now;
						if (string.IsNullOrEmpty(slimports.saleorderno))
							Error_Description += " No saleorder";

						if (!string.IsNullOrEmpty(Error_Description))
						{
							dataloaderror = true;
							slimports.error_description = Error_Description;
							slimports.isloaderror = dataloaderror;

						}

						string soQuery = "Select saleorderno from wms.st_slno_imports where saleorderno = '" + slimports.saleorderno + "' and solineitemno = '" + slimports.solineitemno + "' and serialno = '" + slimports.serialno + "' ";
						var so = DB.ExecuteScalar(soQuery, null);
						if (so == null)
						{
							string stquery = WMSResource.insertstserialimport;
							var rslt = DB.Execute(stquery, new
							{
								slimports.saleorderno,
								slimports.solineitemno,
								slimports.material,
								slimports.gr,
								slimports.plant,
								slimports.serialno,
								slimports.uploadcode,
								uploadedon,
								slimports.error_description,
								slimports.isloaderror
							});

							}




						}
					}
					
					int K = 0;
					if(dtexcel1.Rows.Count>0)
                    {
						foreach (DataRow row in dtexcel1.Rows)
						{

						string Error_Description = "";
						bool dataloaderror = false;
						MateriallabelModel model = new MateriallabelModel();
						model.saleorderno = Conversion.toStr(row["Sales Document No."]);
						model.solineitemno = Conversion.toStr(row["Sales Order Item No."]);
						model.saleordertype = Conversion.toStr(row["Sales Document Type"]);
						model.customername = Conversion.toStr(row["Sold-to party"]) + " " + Conversion.toStr(row["Name: Sold-to party"]);
						model.shipto = Conversion.toStr(row["Ship-to party"]) + " " + Conversion.toStr(row["Name: Ship-to party"]);
						model.shippingpoint = Conversion.toStr(row["Shipping Point"]) + " " + Conversion.toStr(row["Text: Shipping Point"]);
						model.loadingdate = Conversion.TodtTime(row["Planned Billing Date"]);
						model.projectiddef = Conversion.toStr(row["Project definition(level 0)"]);
						model.projecttext = Conversion.toStr(row["Project text (Level 0)"]);
						model.partno = Conversion.toStr(row["Material"]);
						model.custpo = Conversion.toStr(row["PO number"]);
						model.costcenter = Conversion.toStr(row["Cost Center"]);
						model.costcentertext = Conversion.toStr(row["Text: Cost Center"]);
						model.saleordertypetext = Conversion.toStr(row["Text: Sales Document Type"]);
						model.customercode = Conversion.toStr(row["Sold-to party"]);
						model.custpolineitem = Conversion.toStr(row["PO Item No. (Sold-to)"]);
						model.serviceorderno = Conversion.toStr("Service Order Number");
						model.uploadcode = uploadcode;
						DateTime uploadedon = DateTime.Now;
						if (string.IsNullOrEmpty(model.saleorderno))
							Error_Description += " No saleorder";

						if (!string.IsNullOrEmpty(Error_Description))
						{
							dataloaderror = true;
							model.error_description = Error_Description;
							model.isloaderror = dataloaderror;

						}

						string soQuery = "Select saleorderno from wms.st_QTSO where saleorderno = '" + model.saleorderno + "' and solineitemno = '" + model.solineitemno + "'";
						var so = DB.ExecuteScalar(soQuery, null);
						if (so == null)
						{
							string stquery = WMSResource.insertqtso;
							var rslt = DB.Execute(stquery, new
							{
								model.saleorderno,
								model.solineitemno,
								model.saleordertype,
								model.customername,
								model.shipto,
								model.shippingpoint,
								model.loadingdate,
								model.projectiddef,
								model.partno,
								model.custpo,
								model.uploadcode,
								uploadedon,
								model.error_description,
								model.isloaderror,
								model.projecttext,
								model.saleordertypetext,
								model.customercode,
								model.custpolineitem,
								model.costcentertext,
								model.serviceorderno
							});


						}



						}
					}
					


					//End - Gayathri
					loadAuditLog(auditlog);
					loadPOData(uploadcode);

					//}
				}
			}
			catch (Exception e)
			{
				var res = e;
				log.ErrorMessage("StagingController", "uploadPoDataExcel", e.StackTrace.ToString(), "error:" + e.Message.ToString(), url);
			}
			return Ok(true);
		}


		/*Name of Function : <<loadPOData>>  Author :<<Prasanna>>  
		Date of Creation <<06-07-2020>>
		Purpose : <<fill po data from staging to base table>>
		Review Date :<<>>   Reviewed By :<<>>
		Sourcecode Copyright : Yokogawa India Limited
		*/
		public IActionResult loadPOData(string uploadcode)
		{
			using (NpgsqlConnection pgsql = new NpgsqlConnection(config.PostgresConnectionString))

			{
				{

					//string query = "select * from wms.stag_po_sap where purchdoc !='' and material !='' and poquantity !=0  ";
					string query = "select * from wms.stag_po_sap where DataloadErrors is not True and dci!='X'";
					pgsql.Open();
					var stagingList = pgsql.Query<StagingModel>(
					   query, null, commandType: CommandType.Text);
					foreach (StagingModel stag_data in stagingList)
					{
						try
						{
							stag_data.pono = stag_data.purchdoc;
							stag_data.projectmanager = stag_data.pocreatedby;
							stag_data.deliverydate = stag_data.itemdeliverydate;
							stag_data.vendorcode = stag_data.vendor;
							stag_data.suppliername = stag_data.vendorname;
							stag_data.projectcode = stag_data.projectdefinition;
							stag_data.materialid = stag_data.material;
							stag_data.materialqty = stag_data.poquantity;
							stag_data.itemno = stag_data.item;
							stag_data.itemamount = stag_data.NetPrice;
							var unitprice = stag_data.itemamount / stag_data.poquantity;
							string materialdescquery = WMSResource.getMateDescr.Replace("#materialid", stag_data.materialid.ToString());
							stag_data.materialdescription = pgsql.QuerySingleOrDefault<string>(
											materialdescquery, null, commandType: CommandType.Text);

							string query1 = "Select Count(*) as count from wms.wms_polist where pono = '" + stag_data.purchdoc + "'";
							int pocount = int.Parse(pgsql.ExecuteScalar(query1, null).ToString());
							bool isclosed = false;
							if (pocount == 0)
							{
								//insert wms_polist ##pono,deliverydate,vendorid,supliername
								var insertquery = "INSERT INTO wms.wms_polist(pono, vendorcode,suppliername,sloc,type,isclosed,uploadcode)VALUES(@pono, @vendorcode,@suppliername,@sloc,'po',@isclosed,@uploadcode)";
								var results = pgsql.ExecuteScalar(insertquery, new
								{
									stag_data.pono,
									stag_data.vendorcode,
									stag_data.suppliername,
									stag_data.sloc,
									uploadcode,
									isclosed
								});

							}
							string query2 = "Select Count(*) as count from wms.wms_project where pono = '" + stag_data.purchdoc + "'";
							int Projcount = int.Parse(pgsql.ExecuteScalar(query2, null).ToString());

							if (Projcount == 0)
							{

								//insert wms_project ##pono,jobname,projectcode,projectname,projectmanager,
								var insertquery = "INSERT INTO wms.wms_project(pono, jobname, projectcode,projectname,projectmanager,uploadcode)VALUES(@pono, @jobname,@projectcode,@projecttext,@projectmanager,@uploadcode)";
								var results = pgsql.ExecuteScalar(insertquery, new
								{
									stag_data.pono,
									stag_data.jobname,
									stag_data.projectcode,
									stag_data.projecttext,
									stag_data.projectmanager,
									uploadcode
								});
							}
							//else
							//{
							//	if (!string.IsNullOrEmpty(stag_data.projectmanager))
							//	{
							//		var updateqyery = "update wms.wms_project set projectmanager = @projectmanager  where pono = '" + stag_data.purchdoc + "'";
							//		var re = Convert.ToInt32(pgsql.Execute(updateqyery, new
							//		{
							//			stag_data.projectmanager
							//		}));
							//	}
							//}

							string queryasn = "Select Count(*) as count from wms.wms_asn where pono = '" + stag_data.purchdoc + "'";
							int asncountcount = int.Parse(pgsql.ExecuteScalar(queryasn, null).ToString());

							if (asncountcount == 0)
							{
								//insert wms_project ##pono,jobname,projectcode,projectname,projectmanager,

								string updatedby = "303268";
								var insertquery = "INSERT INTO wms.wms_asn(pono,deliverydate,updatedby,updatedon,deleteflag,uploadcode) VALUES (@pono,@itemdeliverydate,@updatedby,current_date,false,@uploadcode)";
								var results = pgsql.ExecuteScalar(insertquery, new
								{
									stag_data.pono,
									stag_data.itemdeliverydate,
									updatedby,
									uploadcode

								});

							}

							////Gayathri - Fetch data from Staging tables and insert in po_materials table
							//string labeldataquery = "select * from  wms.st_QTSO where uploadcode='" + uploadcode + "' and saleorderno='" + stag_data.saleorderno + "' and solineitemno='" + stag_data.solineitemno + "'";

							//var qtodata = pgsql.ExecuteScalar(labeldataquery, null);

							//string labelquery = "select * from wms.st_slno_imports where uploadcode='" + uploadcode + "' and saleorderno='" + stag_data.saleorderno + "' and solineitemno='" + stag_data.solineitemno + "'";

							//var stdata= pgsql.ExecuteScalar(labelquery, null);
							////End

							string query3 = "Select Count(*) as count from wms.wms_pomaterials where pono = '" + stag_data.purchdoc + "' and materialid='" + stag_data.material + "' and itemno = " + stag_data.itemno + "";
							int matcount = int.Parse(pgsql.ExecuteScalar(query3, null).ToString());

							if (matcount == 0)
							{
								var wmsqty = stag_data.poquantity - stag_data.deliveredqty;
								//insert wms_pomaterials ##pono,materialid,materialdescr,materilaqty,itemno,itemamount,item deliverydate,
								var insertquery = "INSERT INTO wms.wms_pomaterials(pono, materialid, materialdescription,materialqty,itemno,itemamount,itemdeliverydate,saleorderno,solineitemno,saleordertype,codetype,costcenter,assetno,poitemdescription,unitprice,deliveredqty,wmsqty)VALUES(@pono, @materialid, @materialdescription,@materialqty,@itemno,@itemamount,@itemdeliverydate,@saleorderno,@solineitemno,@saleordertype,@codetype,@costcenter,@assetno,@poitemdescription,@unitprice,@deliveredqty,@wmsqty)";
								var results = pgsql.ExecuteScalar(insertquery, new
								{
									stag_data.pono,
									stag_data.materialid,
									stag_data.materialdescription,
									stag_data.materialqty,
									stag_data.itemno,
									stag_data.itemamount,
									stag_data.itemdeliverydate,
									stag_data.saleorderno,
									stag_data.solineitemno,
									stag_data.saleordertype,
									stag_data.codetype,
									stag_data.costcenter,
									stag_data.assetno,
									stag_data.poitemdescription,
									unitprice,
									stag_data.deliveredqty,
									wmsqty
								});
							}
							//else
							//{
							//    var id = pgsql.QuerySingleOrDefault<string>("Select id  from wms.wms_pomaterials where pono = '" + stag_data.purchdoc + "' and materialid='" + stag_data.material + "'", null, commandType: CommandType.Text);
							//    var updateqyery = "update wms.wms_pomaterials set materialqty = @materialqty where id=" + id + "";

							//    var re = Convert.ToInt32(pgsql.Execute(updateqyery, new

							//    {
							//        stag_data.materialqty

							//    }));



							//}
						}
						catch (Exception e)
						{
							var res = e;
							log.ErrorMessage("StagingController", "loadPOData", e.StackTrace.ToString(), e.Message.ToString(), url);
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


		/*Name of Function : <<uploadInitialStockExcel>>  Author :<<Ramesh>>  
        Date of Creation <<16-09-2020>>
        Purpose : <<Upload initial stock from Excel to staging>>
        Review Date :<<>>   Reviewed By :<<>>
        Sourcecode Copyright : Yokogawa India Limited
        */

		[HttpGet]
		[Route("uploadInitialStockExcel")]
		public WMSHttpResponse uploadInitialStockExcel()
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

		/*Name of Function : <<uploadInitialStockExcelByUser>>  Author :<<Ramesh>>  
        Date of Creation <<16-09-2020>>
        Purpose : <<Upload initial stock from Excel to staging>>
        Review Date :<<>>   Reviewed By :<<>>
        Sourcecode Copyright : Yokogawa India Limited
        */
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
					string uploadedfilename = filename.Substring(index + 1);
					var folderName = Path.Combine("Resources", "documents");
					var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
					string storeQuery = "Select uploadedfilename from wms.st_initialstock where Lower(uploadedfilename) = Lower('" + uploadedfilename + "') limit 1";
					var fileexists = DB.ExecuteScalar(storeQuery, null);
					if (fileexists != null)
					{
						result.message = "FILEFOUND";
						return result;
					}


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
						initialstk.quantity = Conversion.Todecimaltype(row["Quantity"]);
						initialstk.quantitystr = Conversion.toStr(row["Quantity"]);
						initialstk.projectid = Conversion.toStr(row["Project Id"]);
						initialstk.pono = Conversion.toStr(row["PO No"]);
						initialstk.value = Conversion.Todecimaltype(row["Value"]);
						initialstk.valuestr = Conversion.toStr(row["Value"]);
						initialstk.grn = Conversion.toStr(row["GRN"]);
						initialstk.receiveddate = Conversion.TodtTime(row["Received date"]);
						initialstk.receiveddatestr = Conversion.toStr(row["Received date"]);
						initialstk.shelflifeexpiration = Conversion.TodtTime(row["Shelf life expiration"]);
						initialstk.shelflifeexpirationstr = Conversion.toStr(row["Shelf life expiration"]);
						initialstk.dateofmanufacture = Conversion.TodtTime(row["Date of Manufacture"]);
						initialstk.dateofmanufacturestr = Conversion.toStr(row["Date of Manufacture"]);
						initialstk.dataenteredon = Conversion.TodtTime(row["Data Entered On"]);
						initialstk.dataenteredonstr = Conversion.toStr(row["Data Entered On"]);
						initialstk.datasource = Conversion.toStr(row["DataSource"]);
						initialstk.dataenteredby = Conversion.toStr(row["Data Entered By"]);
						initialstk.createddate = createdate;
						initialstk.uploadedfilename = uploadedfilename;

						initialstk.unitprice = null;
						if (initialstk.value != null && initialstk.value > 0 && initialstk.quantity != null && initialstk.quantity > 0)
						{
							initialstk.unitprice = initialstk.value / initialstk.quantity;

						}
						initialstk.category = null;
						initialstk.uploadedby = uploadedby;
						initialstk.uploadbatchcode = uploadcode;
						string loctype = string.Empty;
						if (!string.IsNullOrEmpty(initialstk.store) || initialstk.store != "")
						{
							string locatorquery = "select locationtype from wms.wms_rd_locator where lower(locatorname) = Lower('" + initialstk.store + "')";
							var locationtype = DB.ExecuteScalar(locatorquery, null);
							if (locationtype != null)
							{
								loctype = locationtype.ToString();
							}
						}

						if (loctype == "Plant" && string.IsNullOrEmpty(initialstk.projectid) || initialstk.projectid == "")
						{
							initialstk.stocktype = "Plant Stock";
						}
						else
						{
							initialstk.stocktype = "Project Stock";
						}

						if (string.IsNullOrEmpty(initialstk.material) || initialstk.material == "")
							Error_Description += " No Material";
						if (string.IsNullOrEmpty(initialstk.materialdescription) || initialstk.materialdescription == "")
							Error_Description += " No Material Description";
						if (string.IsNullOrEmpty(initialstk.store) || initialstk.store == "")
							Error_Description += " No Store";
						if (string.IsNullOrEmpty(initialstk.rack) || initialstk.rack == "")
							Error_Description += " No Rack";
						if (initialstk.quantity == null || initialstk.quantity == 0)
							Error_Description += " No Quantity";
						if ((string.IsNullOrEmpty(initialstk.projectid) || initialstk.projectid == "") && initialstk.stocktype != "Plant Stock")
							Error_Description += " No Project Id";
						if (string.IsNullOrEmpty(initialstk.pono) || initialstk.pono == "")
							Error_Description += " No PONo";
						if (initialstk.pono.ToString().Trim().Contains("\\") || initialstk.pono.ToString().ToLower().Trim() == "reserved" || initialstk.pono.ToString().Trim().Contains("/") || initialstk.pono.ToString().Trim().Contains(","))
							Error_Description += " Invalid PO format";
						if (initialstk.value == null || initialstk.value == 0)
							Error_Description += " No value";
						if (!string.IsNullOrEmpty(Error_Description))
						{
							dataloaderror = true;
							exceptionrows = exceptionrows + 1;
							Error_Description_all += Error_Description + " For Row " + i.ToString() + "-";

						}


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
						if (!string.IsNullOrEmpty(initialstk.pono) && initialstk.pono != "" && initialstk.pono.ToString().Trim() != "reserved")
						{
							List<string> pos = new List<string>();
							if (initialstk.pono.Contains("/"))
							{

								string[] arr = initialstk.pono.Split('/');
								pos = arr.ToList();
							}
							else if (initialstk.pono.Contains(","))
							{
								string[] arr = initialstk.pono.Split(',');
								pos = arr.ToList();
							}
							else
							{
								pos.Add(initialstk.pono);
							}
							foreach (string str in pos)
							{
								string pono = str.Trim();
								string query2 = "Select Count(*) as count from wms.wms_project where pono = '" + pono + "'";
								int Projcount = int.Parse(DB.ExecuteScalar(query2, null).ToString());

								if (Projcount == 0)
								{
									string jobname = null;
									string projectcode = null;
									string projecttext = null;
									string projectmanager = null;

									string uploadtype = "Initial Stock";
									if (!string.IsNullOrEmpty(initialstk.projectid) && initialstk.projectid != "")
									{
										projectcode = initialstk.projectid;

									}

									string querypm = "Select Max(projectmanager) as projectmanager from wms.wms_project where  projectcode = '" + projectcode + "' group by projectmanager";
									var rsltt = DB.ExecuteScalar(querypm, null);
									if (rsltt != null)
									{
										projectmanager = rsltt.ToString();
									}

									//insert wms_project ##pono,jobname,projectcode,projectname,projectmanager,
									var insertquery = "INSERT INTO wms.wms_project(pono, jobname, projectcode,projectname,projectmanager,uploadcode,uploadtype)VALUES(@pono, @jobname,@projectcode,@projecttext,@projectmanager,@uploadcode,@uploadtype)";
									var results = DB.ExecuteScalar(insertquery, new
									{
										pono,
										jobname,
										projectcode,
										projecttext,
										projectmanager,
										uploadcode,
										uploadtype
									});
								}
							}

						}



						rowsinserted = rowsinserted + 1;

					}

					DB.Close();
					//result.message += "-Total_Rows_:" + rows + "-Inserted_rows_to_staging_table_:" + rowsinserted.ToString();
					result.message += "-Total Records_:" + rows;

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
					AuditLog auditlog = new AuditLog();
					auditlog.filename = uploadedfilename;
					auditlog.uploadedon = createdate;
					auditlog.uploadedby = uploadedby;
					auditlog.uploadedto = "st_initialstock";
					auditlog.modulename = "initialstock";
					auditlog.totalrecords = Conversion.toInt(rows);
					auditlog.exceptionrecords = Conversion.toInt(exceptionrows);
					auditlog.successrecords = Conversion.toInt(rows) - Conversion.toInt(exceptionrows);

					loadAuditLog(auditlog);

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
		function : <<loadStockData>>  Author :<<Ramesh>>  
		Date of Creation <<16-09-2020>>
		Purpose : <<upload stock data from staging to base tables>>
		Review Date :<<>>   Reviewed By :<<>>
		Sourcecode Copyright : Yokogawa India Limited
		*/
		public string loadStockData(string batchcode)
		{
			string insertmessage = "";
			using (NpgsqlConnection pgsql = new NpgsqlConnection(config.PostgresConnectionString))

			{
				{

					string query = "select * from wms.st_initialstock where material  !='' and materialdescription !='' and store !='' and rack !='' and quantity  !='0' and dataloaderrors=false and uploadbatchcode = '" + batchcode + "'";
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
							var storeId = pgsql.ExecuteScalar(storeQuery, null);
							if (storeId == null)
							{
								LocationModel store = new LocationModel();
								store.locatorname = stag_data.store;
								store.createdate = DateTime.Now;
								store.isexcelupload = true;
								//insert wms_rd_locator ##locatorname
								string locationtype = "";
								string storagelocationdesc = store.locatorname;
								if (store.locatorname.ToString().Trim().ToLower() == "ec c block" || store.locatorname.ToString().Trim().ToLower() == "ec unit 2")
								{
									locationtype = "Project";

								}
								else
								{
									locationtype = "Plant";
								}
								var insertStorequery = "INSERT INTO wms.wms_rd_locator(locatorid, locatorname, createdate,deleteflag,isexcelupload,locationtype,storagelocationdesc)VALUES(default, @locatorname,@createdate,@deleteflag,@isexcelupload,@locationtype,@storagelocationdesc) returning locatorid";
								var Storeresults = pgsql.ExecuteScalar(insertStorequery, new
								{
									store.locatorname,
									store.createdate,
									deleteflag,
									store.isexcelupload,
									locationtype,
									storagelocationdesc
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
							stock.poitemdescription = stag_data.materialdescription;
							stock.initialstock = true;
							string itemlocation = stag_data.store + "." + stag_data.rack;
							if (stag_data.bin != "" && stag_data.bin != null)
							{
								itemlocation += "." + stag_data.bin;

							}
							int? bindata = null;
							if (stock.binid > 0)
							{
								bindata = stock.binid;

							}

							//insert wms_stock ##storeid, binid,rackid,totalquantity,shelflife ,createddate,materialid ,initialstock
							var insertquery = "INSERT INTO wms.wms_stock(storeid, binid,rackid,itemlocation,totalquantity,availableqty,shelflife ,createddate,materialid ,initialstock,stcktype,unitprice,value,pono,projectid,createdby,uploadbatchcode,uploadedfilename,poitemdescription)VALUES(@storeid, @bindata,@rackid,@itemlocation,@totalquantity,@availableqty,@shelflife ,@createddate,@materialid ,@initialstock,@stocktype,@unitprice,@value,@pono,@projectid,@uploadedby,@uploadcode,@uploadedfilename,@poitemdescription)";
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
								stag_data.uploadedfilename,
								stock.poitemdescription
							});

							Trans.Commit();
							rowinserted = rowinserted + 1;




						}
						catch (Exception e)
						{
							Trans.Rollback();
							var res = e;
							insertmessage += e.Message.ToString();
							log.ErrorMessage("StagingController", "loadStockData", e.StackTrace.ToString(), e.Message.ToString(), url);
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


		/*
       function : <<loadStockData>>  Author :<<Ramesh>>  
       Date of Creation <<20-11-2020>>
       Purpose : <<upload material label data from excel to staging>>
       Review Date :<<>>   Reviewed By :<<>>
       Sourcecode Copyright : Yokogawa India Limited
       */
		[HttpGet]
		[Route("uploadDataExcel")]
		public IActionResult uploadDataExcel()
		{
			string serverPath = "";

			using (NpgsqlConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
			{
				serverPath = @"\\ZAWMS-001\StockExcel\";
				using (new NetworkConnection(serverPath, new NetworkCredential(@"administrator", "Wms@1234*")))
				{

					DB.Open();
					//var filePath = @"D:\Projects\WMS\Docs\label data\ZLMMP00001_ListofPO.xlsx";
					//var filePath = @"D:\A_StagingTable\ZLMMP00001_ListofPO.xlsx";
					//var filePath1 = @"D:\A_StagingTable\ZGSDR00006_QTSO-Sept-Oct2020.xlsx";
					//var filePath2 = @"D:\A_StagingTable\ZGMMR02023_slno_imports.xlsx";
					var filePath = serverPath + "ZLMMP00001_ListofPO.xlsx";
					var filePath1 = serverPath + "ZGSDR00006_QTSO-Sept-Oct2020.xlsx";
					var filePath2 = serverPath + "ZGMMR02023_slno_imports.xlsx";


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
					string upcode = Guid.NewGuid().ToString();

					try
					{
						int i = 0;

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
							//----//
							model.materialid = Conversion.toStr(row["Material"]);
							model.vendorcode = Conversion.toStr(row["Vendor"]);
							model.vendorname = Conversion.toStr(row["Vendor Name"]);
							model.materialqty = Conversion.toInt(row["PO Quantity"]);
							model.itemdeliverydate = Conversion.TodtTime(row["Item Delivery Date"]);
							model.projectcode = Conversion.toStr(row["Project Definition"]);
							model.assetno = Conversion.toStr(row["Asset Number"]);
							model.assetsubno = Conversion.toStr(row["Asset Subnumber"]);
							model.costcenter = Conversion.toStr(row["Cost Center1"]);


							model.uploadcode = upcode;
							///not required
							string materialQueryxx = "Select materialdescription from wms.\"MaterialMasterYGS\" where material = '" + model.materialid + "'";
							var materialdesc = DB.ExecuteScalar(materialQueryxx, null);
							if (materialdesc == null)
							{
								model.materialdescription = "-";
							}
							else
							{
								model.materialdescription = materialdesc.ToString();
							}
							////

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
							string materialQuery = "Select po from wms.WMS_ST_MaterialLabel where po = '" + model.po + "' and materialid = '" + model.materialid + "' and polineitemno = '" + model.polineitemno + "' ";
							var materialid = DB.ExecuteScalar(materialQuery, null);
							if (materialid == null)
							{

								string insertpoqry = WMSResource.materiallablestaginginsert;
								var rslt = DB.Execute(insertpoqry, new
								{
									model.po,
									model.polineitemno,
									model.mscode,
									model.saleorderno,
									model.solineitemno,
									model.insprec,
									model.linkageno,
									model.grno,
									model.codetype,
									model.description,
									model.error_description,
									model.isloaderror,
									model.uploadcode,
									model.vendorcode,
									model.vendorname,
									model.materialid,
									model.materialdescription,
									model.materialqty,
									model.itemdeliverydate,
									model.projectcode,
									model.assetno,
									model.assetsubno,
									model.costcenter
								});

							}








						}

						int J = 0;

						foreach (DataRow row in dtexcel2.Rows)
						{

							string Error_Description = "";
							bool dataloaderror = false;
							MateriallabelModel slimports = new MateriallabelModel();
							slimports.saleorderno = Conversion.toStr(row["Sales Document"]);
							slimports.solineitemno = Conversion.toStr(row["Sales Document Item"]);

							slimports.material = Conversion.toStr(row["Material Number"]);
							slimports.gr = Conversion.toStr(row["Storage Location"]);
							slimports.plant = Conversion.toStr(row["Plant"]);
							slimports.serialno = Conversion.toStr(row["Serial Number"]);
							slimports.uploadcode = upcode;
							DateTime uploadedon = DateTime.Now;
							if (string.IsNullOrEmpty(slimports.saleorderno))
								Error_Description += " No saleorder";

							if (!string.IsNullOrEmpty(Error_Description))
							{
								dataloaderror = true;
								slimports.error_description = Error_Description;
								slimports.isloaderror = dataloaderror;

							}

							string soQuery = "Select saleorderno from wms.st_slno_imports where saleorderno = '" + slimports.saleorderno + "' and solineitemno = '" + slimports.solineitemno + "' and serialno = '" + slimports.serialno + "' ";
							var so = DB.ExecuteScalar(soQuery, null);
							if (so == null)
							{
								string stquery = WMSResource.insertstserialimport;
								var rslt = DB.Execute(stquery, new
								{
									slimports.saleorderno,
									slimports.solineitemno,
									slimports.material,
									slimports.gr,
									slimports.plant,
									slimports.serialno,
									slimports.uploadcode,
									uploadedon,
									slimports.error_description,
									slimports.isloaderror
								});

							}




						}
						int K = 0;
						foreach (DataRow row in dtexcel1.Rows)
						{

							string Error_Description = "";
							bool dataloaderror = false;
							MateriallabelModel model = new MateriallabelModel();
							model.saleorderno = Conversion.toStr(row["Sales Document No."]);
							model.solineitemno = Conversion.toStr(row["Sales Order Item No."]);
							model.saleordertype = Conversion.toStr(row["Sales Document Type"]);
							model.customername = Conversion.toStr(row["Sold-to party"]) + " " + Conversion.toStr(row["Name: Sold-to party"]);
							model.shipto = Conversion.toStr(row["Ship-to party"]) + " " + Conversion.toStr(row["Name: Ship-to party"]);
							model.shippingpoint = Conversion.toStr(row["Shipping Point"]) + " " + Conversion.toStr(row["Text: Shipping Point"]);
							model.loadingdate = Conversion.TodtTime(row["Planned Billing Date"]);
							model.projectiddef = Conversion.toStr(row["Project definition(level 0)"]);
							model.projecttext = Conversion.toStr(row["Project text (Level 0)"]);
							model.partno = Conversion.toStr(row["Material"]);
							model.custpo = Conversion.toStr(row["PO number"]);
							model.uploadcode = upcode;
							DateTime uploadedon = DateTime.Now;
							if (string.IsNullOrEmpty(model.saleorderno))
								Error_Description += " No saleorder";

							if (!string.IsNullOrEmpty(Error_Description))
							{
								dataloaderror = true;
								model.error_description = Error_Description;
								model.isloaderror = dataloaderror;

							}

							string soQuery = "Select saleorderno from wms.st_QTSO where saleorderno = '" + model.saleorderno + "' and solineitemno = '" + model.solineitemno + "'";
							var so = DB.ExecuteScalar(soQuery, null);
							if (so == null)
							{
								string stquery = WMSResource.insertqtso;
								var rslt = DB.Execute(stquery, new
								{
									model.saleorderno,
									model.solineitemno,
									model.saleordertype,
									model.customername,
									model.shipto,
									model.shippingpoint,
									model.loadingdate,
									model.projectiddef,
									model.partno,
									model.custpo,
									model.uploadcode,
									uploadedon,
									model.error_description,
									model.isloaderror,
									model.projecttext
								});


							}



						}



					}
					catch (Exception ex)
					{
						var data = ex;
						log.ErrorMessage("StagingController", "uploadDataExcel", ex.StackTrace.ToString(), ex.Message.ToString(), url);

					}



					foreach (string str in file1arr)
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
					string rsltx1 = loadlabeldatatobase(upcode);

					return Ok(true);
				}


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
				string serialquery = "select * from wms.st_slno_imports  where isloaderror is NOT True and uploadcode = '" + batchcode + "'";
				string qtsoqry = "select * from wms.st_QTSO  where isloaderror is NOT True and uploadcode = '" + batchcode + "'";
				pgsql.Open();
				var stagingList = pgsql.Query<MateriallabelModel>(
				   query, null, commandType: CommandType.Text);
				var seriallist = pgsql.Query<MateriallabelModel>(
				  serialquery, null, commandType: CommandType.Text);
				var qtsolist = pgsql.Query<MateriallabelModel>(
				 qtsoqry, null, commandType: CommandType.Text);

				int rowinserted = 0;
				string uploadcode = batchcode;

				DateTime currentdate = DateTime.Now;
				DateTime deliverydate = DateTime.Now;

				foreach (MateriallabelModel stag_data in stagingList)
				{
					try
					{

						int itmno = Convert.ToInt32(stag_data.polineitemno);
						string materialQuery = "Select * from wms.wms_pomaterials where pono = '" + stag_data.po + "' and itemno =" + itmno;
						var podata = pgsql.QueryFirstOrDefault<MateriallabelModel>(materialQuery, null, commandType: CommandType.Text);
						if (podata != null)
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
								//stag_data.material,
								//stag_data.plant,
								//stag_data.saleordertype,
								//stag_data.customername,
								//stag_data.shippingpoint,
								//stag_data.loadingdate,
								//stag_data.gr,
								//stag_data.projectiddef,
								//stag_data.partno,
								//stag_data.custpo,
								stag_data.grno,
								stag_data.codetype,
								//stag_data.shipto,
								//stag_data.soldto,
								stag_data.uploadcode,
								stag_data.assetno,
								stag_data.assetsubno,
								stag_data.costcenter

							});
						}
						else
						{
							//string query1 = "Select Count(*) as count from wms.wms_polist where pono = '" + stag_data.po + "'";
							//int pocount = int.Parse(pgsql.ExecuteScalar(query1, null).ToString());
							//if (pocount == 0)
							//{
							//	//insert wms_polist ##pono,deliverydate,vendorid,supliername
							//	string type = "po";
							//	var insertquery = "INSERT INTO wms.wms_polist(pono,vendorcode,suppliername,type,uploadcode)VALUES(@po, @vendorcode,@vendorname,@type,@uploadcode)";
							//	var results = pgsql.ExecuteScalar(insertquery, new
							//	{
							//		stag_data.po,
							//		stag_data.vendorcode,
							//		stag_data.vendorname,
							//		type,
							//		uploadcode
							//	});

							//}

							//string query2 = "Select Count(*) as count from wms.wms_project where pono = '" + stag_data.po + "'";
							//int Projcount = int.Parse(pgsql.ExecuteScalar(query2, null).ToString());

							//if (Projcount == 0)
							//{
							//	//insert wms_project ##pono,jobname,projectcode,projectname,projectmanager,
							//	var insertquery = "INSERT INTO wms.wms_project(pono,projectcode,projectname,uploadcode)VALUES(@po,@projectcode,@description,@uploadcode)";
							//	var results = pgsql.ExecuteScalar(insertquery, new
							//	{
							//		stag_data.po,
							//		stag_data.projectcode,
							//		stag_data.description,
							//		uploadcode

							//	});

							//}

							//string queryasn = "Select Count(*) as count from wms.wms_asn where pono = '" + stag_data.po + "'";
							//int asncountcount = int.Parse(pgsql.ExecuteScalar(query2, null).ToString());

							//if (Projcount == 0)
							//{
							//	//insert wms_project ##pono,jobname,projectcode,projectname,projectmanager,
							//	deliverydate = deliverydate.AddDays(1);
							//	string updatedby = "303268";
							//	var insertquery = "INSERT INTO wms.wms_asn(pono,deliverydate,updatedby,updatedon,deleteflag,uploadcode) VALUES (@po,@deliverydate,@updatedby,current_date,false,@uploadcode)";
							//	var results = pgsql.ExecuteScalar(insertquery, new
							//	{
							//		stag_data.po,
							//		deliverydate,
							//		updatedby,
							//		uploadcode

							//	});

							//}

							//int rsltxxx = 0;
							//string podescription = stag_data.description;
							//stag_data.soldto = stag_data.customername;
							//var insrtqry = WMSResource.insertpoformatlabel;
							//int itemno = Conversion.toInt(stag_data.polineitemno);
							//string pono = stag_data.po;
							//rsltxxx = pgsql.Execute(insrtqry, new
							//{
							//	pono,
							//	stag_data.materialid,
							//	stag_data.materialdescription,
							//	stag_data.materialqty,
							//	itemno,
							//	stag_data.itemdeliverydate,
							//	podescription,
							//	stag_data.mscode,
							//	stag_data.saleorderno,
							//	stag_data.solineitemno,
							//	stag_data.linkageno,
							//	//stag_data.material,
							//	//stag_data.plant,
							//	//stag_data.saleordertype,
							//	//stag_data.customername,
							//	//stag_data.shippingpoint,
							//	//stag_data.loadingdate,
							//	//stag_data.gr,
							//	//stag_data.projectiddef,
							//	//stag_data.partno,
							//	//stag_data.custpo,
							//	stag_data.grno,
							//	stag_data.codetype,
							//	//stag_data.shipto,
							//	//stag_data.soldto,
							//	stag_data.uploadcode,
							//	stag_data.assetno,
							//	stag_data.assetsubno,
							//	stag_data.costcenter

							//});


						}

						rowinserted = rowinserted + 1;

					}
					catch (Exception e)
					{
						var res = e;
						insertmessage += e.Message.ToString();
						log.ErrorMessage("StagingController", "loadmatlabelDatatobase", e.StackTrace.ToString(), e.Message.ToString(), url);
						continue;
					}
				}
				foreach (MateriallabelModel stag_data in seriallist)
				{
					try
					{
						int itmno = Convert.ToInt32(stag_data.polineitemno);
						string materialQuery = "Select * from wms.wms_pomaterials where saleorderno = '" + stag_data.saleorderno + "' and solineitemno ='" + stag_data.solineitemno + "'";
						var podata = pgsql.QueryFirstOrDefault<MateriallabelModel>(materialQuery, null, commandType: CommandType.Text);
						if (podata != null)
						{

							int rslt = 0;
							string podescription = stag_data.description;
							stag_data.soldto = stag_data.customername;
							var updateqry = WMSResource.updatepomatbrserialexcel.Replace("#sono", stag_data.saleorderno).Replace("#solineitemno", stag_data.solineitemno);
							rslt = pgsql.Execute(updateqry, new
							{

								stag_data.material,
								stag_data.plant,
								stag_data.gr
							});
						}

					}
					catch (Exception e)
					{
						var res = e;
						insertmessage += e.Message.ToString();
						log.ErrorMessage("StagingController", "load_st_serialimports", e.StackTrace.ToString(), e.Message.ToString(), url);
						continue;
					}
				}
				foreach (MateriallabelModel stag_data in qtsolist)
				{

					try
					{
						int itmno = Convert.ToInt32(stag_data.polineitemno);
						string materialQuery = "Select * from wms.wms_pomaterials where saleorderno = '" + stag_data.saleorderno + "' and solineitemno ='" + stag_data.solineitemno + "'";
						var podata = pgsql.QueryFirstOrDefault<MateriallabelModel>(materialQuery, null, commandType: CommandType.Text);
						if (podata != null)
						{

							int rslt = 0;
							var updateqry = WMSResource.updatepomatbyqtsoexcel.Replace("#sono", stag_data.saleorderno).Replace("#solineitemno", stag_data.solineitemno);
							rslt = pgsql.Execute(updateqry, new
							{
								stag_data.saleordertype,
								stag_data.customername,
								stag_data.shippingpoint,
								stag_data.loadingdate,
								stag_data.projectiddef,
								stag_data.partno,
								stag_data.custpo,
								stag_data.shipto
							});
						}

					}
					catch (Exception e)
					{
						var res = e;
						insertmessage += e.Message.ToString();
						log.ErrorMessage("StagingController", "load_st_qtsoimports", e.StackTrace.ToString(), e.Message.ToString(), url);
						continue;
					}
				}
				insertmessage = rowinserted.ToString();
				pgsql.Close();
			}
			return insertmessage;

		}

		/*
		function : <<loadstoragelocations>>  Author :<<Ramesh>>  
		Date of Creation <<19-11-2020>>
		Purpose : <<Contains metadata of staging process>>
		Review Date :<<>>   Reviewed By :<<>>
		Sourcecode Copyright : Yokogawa India Limited
		*/

		[HttpGet]
		[Route("uploadStorageLocation")]
		public IActionResult loadstoragelocations()
		{

			using (NpgsqlConnection pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{

					string query = "select * from wms.wms_rd_storagelocationplant wrs";
					var data = pgsql.QueryAsync<storagelocationmodel>(
					  query, null, commandType: CommandType.Text);
					foreach (storagelocationmodel loc in data.Result)
					{
						bool deleteflag = false;
						//Add locator in masterdata
						string storeQuery = "Select locatorid from wms.wms_rd_locator where locatorname = '" + loc.storagelocation + "'";
						var storeId = pgsql.ExecuteScalar(storeQuery, null);
						if (storeId == null)
						{
							LocationModel store = new LocationModel();
							store.locatorname = loc.storagelocation;
							store.createdate = DateTime.Now;
							store.isexcelupload = false;
							//insert wms_rd_locator ##locatorname
							string locationtype = "";
							string storagelocationdesc = loc.descstoragelocation;
							if (store.locatorname.ToString().Trim().ToLower() == "ec c block" || store.locatorname.ToString().Trim().ToLower() == "ec unit 2")
							{
								locationtype = "Project";

							}
							else
							{
								locationtype = "Plant";
							}
							var insertStorequery = "INSERT INTO wms.wms_rd_locator(locatorid, locatorname, createdate,deleteflag,isexcelupload,locationtype,storagelocationdesc)VALUES(default, @locatorname,@createdate,@deleteflag,@isexcelupload,@locationtype,@storagelocationdesc) returning locatorid";
							var Storeresults = pgsql.ExecuteScalar(insertStorequery, new
							{
								store.locatorname,
								store.createdate,
								deleteflag,
								store.isexcelupload,
								locationtype,
								storagelocationdesc
							});
							storeId = Convert.ToInt32(Storeresults);
						}

					}



					return Ok(true);


				}
				catch (Exception e)
				{
					return Ok(false);
					var res = e;
					log.ErrorMessage("StagingController", "loadStockData", e.StackTrace.ToString(), e.Message.ToString(), url);

				}
				finally
				{
					pgsql.Close();

				}




			}


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
						logdata.modulename,
						logdata.successrecords,
						logdata.exceptionrecords,
						logdata.totalrecords
					});




				}
				catch (Exception e)
				{
					var res = e;
					log.ErrorMessage("StagingController", "loadStockData", e.StackTrace.ToString(), e.Message.ToString(), url);

				}
				finally
				{
					pgsql.Close();

				}


			}


		}

		/*function : <<uploadMaterialDataExcel>>  Author :<<Prasanna>>  
		Date of Creation <<01-12-2020>>
		Purpose : <<load datafrom excel to stag_material_sap table>>
		Review Date :<<>>   Reviewed By :<<>>
		Sourcecode Copyright : Yokogawa India Limited*/
		[HttpGet]
		[Route("uploadMaterialDataExcel")]
		public IActionResult uploadMaterialDataExcel()
		{
			string serverPath = "";
			using (NpgsqlConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
			{
				serverPath = "";
				//using (new NetworkConnection(serverPath, new NetworkCredential(@"administrator", "Wms@1234*")))
				//{
				//var filePath = serverPath + "stageTest1.xlsx";
				DB.Open();
				var filePath = @"D:\YILProjects\WMS\WMSFiles\MARC.xlsx";
				var filePathstr = filePath;
				string[] filearr = filePathstr.Split("\\");
				string nameoffile = filearr[filearr.Length - 1];
				DataTable dtexcel = new DataTable();
				System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
				using (var stream1 = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
				{
					using (var reader = ExcelReaderFactory.CreateReader(stream1))
					{

						var result = reader.AsDataSet(new ExcelDataSetConfiguration()
						{
							ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
							{
								UseHeaderRow = true
							}
						});

						dtexcel = result.Tables[0];

					}
				}

				string uploadcode = Guid.NewGuid().ToString();
				foreach (DataRow row in dtexcel.Rows)
				{

					try
					{
						MaterialStage model = new MaterialStage();
						model.material = Conversion.toStr(row["Material"]);
						model.hsncode = Conversion.toStr(row["Comm./imp. code no."]);
						model.plant = Conversion.toStr(row["Plant"]);
						string Error_Description = "";
						bool dataloaderror = false;
						if (string.IsNullOrEmpty(model.material))
							Error_Description += "There is NO material";
						if (string.IsNullOrEmpty(model.hsncode))
							Error_Description += "No hsn code";
						if (string.IsNullOrEmpty(model.plant))
							Error_Description += "No plant";
						if (!string.IsNullOrEmpty(Error_Description))
						{
							dataloaderror = true;
							model.dataloaderror = true;
							model.error_description = Error_Description;
						}

						var insertquery = "INSERT INTO wms.stag_material_sap(material,plant,hsncode,datasource,createddate,DataloadErrors ,Error_Description,uploadcode)";
						insertquery += " VALUES(@material, @plant,@hsncode,'SAP',current_timestamp,@dataloaderror,@error_description,@uploadcode)";
						var results = DB.ExecuteScalar(insertquery, new
						{
							model.material,
							model.plant,
							model.hsncode,
							dataloaderror,
							model.error_description,
							uploadcode
						});



					}
					catch (Exception e)
					{
						var res = e;
						log.ErrorMessage("StagingController", "uploadMaterialDataExcel", e.StackTrace.ToString(), "error:" + e.Message.ToString(), url);
						continue;
					}
				}

				DB.Close();

				updateMaterialMasterYGS();
				return Ok(true);
				//}
			}
		}

		/*function : <<updateMaterialMasterYGS>>  Author :<<Prasanna>>  
		Date of Creation <<01-12-2020>>
		Purpose : <<to update hsn code in materialmasterygs table from staging table>>
		Review Date :<<>>   Reviewed By :<<>>
		Sourcecode Copyright : Yokogawa India Limited*/
		public void updateMaterialMasterYGS()
		{
			using (NpgsqlConnection pgsql = new NpgsqlConnection(config.PostgresConnectionString))

			{
				string query = "UPDATE wms.\"MaterialMasterYGS\" SET hsncode = wms.stag_material_sap.hsncode FROM wms.stag_material_sap WHERE wms.stag_material_sap.material = wms.\"MaterialMasterYGS\".material";
				pgsql.Open();
				NpgsqlCommand command = new NpgsqlCommand(query, pgsql);
				Int64 count = (Int64)command.ExecuteNonQuery();
				//string query = "select * from wms.stag_material_sap where dataloaderrors is not True";

				//var stagingList = pgsql.Query<MaterialStage>(
				//   query, null, commandType: CommandType.Text);
				//foreach (MaterialStage stag_data in stagingList)
				//{
				//	try
				//	{
				//		string query1 = "Select Count(*) as count from wms.\"MaterialMasterYGS\" where material = '" + stag_data.material + "'";
				//		int pocount = int.Parse(pgsql.ExecuteScalar(query1, null).ToString());
				//		if (pocount > 0)
				//		{
				//			var updateqyery = "update wms.\"MaterialMasterYGS\" set hsncode = @hsncode where material='" + stag_data.material + "'";
				//			var re = Convert.ToInt32(pgsql.Execute(updateqyery, new

				//			{
				//				stag_data.hsncode

				//			}));
				//		}
				//	}
				//	catch (Exception e)
				//	{
				//		var res = e;
				//		log.ErrorMessage("StagingController", "updateMaterialMasterYGS", e.StackTrace.ToString());
				//		continue;
				//	}
				//}

				pgsql.Close();

				//throw new NotImplementedException();

			}
		}

		/*function : <<UpdateEmpDepDetails>>  Author :<<Prasanna>>  
		Date of Creation <<18-01-2021>>
		Purpose : <<to update Employee and department tables from intranet to wms>>
		Review Date :<<>>   Reviewed By :<<>>
		Sourcecode Copyright : Yokogawa India Limited*/
		[HttpGet]
		[Route("UpdateEmpDepDetails")]
		public IActionResult UpdateEmpDepDetails()
		{
			try
			{
				using (NpgsqlConnection pgsql = new NpgsqlConnection(config.PostgresConnectionString))

				{
					SqlConnection conn = new SqlConnection(config.IntranetConnectionString);
					SqlCommand cmd = new SqlCommand("select * from OrgDepartments", conn);
					conn.Open();
					SqlDataReader dr = cmd.ExecuteReader();
					int depexceptionrows = 0;
					int rows = 0;
					while (dr.Read())
					{
						rows = rows + 1;
						try
						{
							Orgdepartments dep = new Orgdepartments();
							dep.orgdepartmentid = Convert.ToInt16(dr["OrgDepartmentId"]);
							dep.orgdepartment = Convert.ToString(dr["OrgDepartment"]);
							dep.departmenthead = Convert.ToString(dr["DepartmentHead"]);
							dep.boolinuse = Convert.ToBoolean(dr["BoolInUse"]);
							string depquery = "select count(*) from wms.orgdepartments  where orgdepartmentid = " + dr["OrgDepartmentId"] + "";
							int depCnt = int.Parse(pgsql.ExecuteScalar(depquery, null).ToString());
							if (depCnt == 0)
							{
								var insertquery = "INSERT INTO wms.orgdepartments(orgdepartmentid ,orgdepartment ,departmenthead ,boolinuse)VALUES(@orgdepartmentid ,@orgdepartment ,@departmenthead ,@boolinuse)";
								var results = pgsql.ExecuteScalar(insertquery, new
								{

									dep.orgdepartmentid,
									dep.orgdepartment,
									dep.departmenthead,
									dep.boolinuse
								});
							}
							else
							{
								var updateqry = "update wms.orgdepartments set orgdepartment = @orgdepartment ,departmenthead = @departmenthead ,boolinuse = @boolinuse where orgdepartmentid=" + dr["OrgDepartmentId"] + "";
								var rslt = pgsql.Execute(updateqry, new
								{

									dep.orgdepartment,
									dep.departmenthead,
									dep.boolinuse
								});
							}
						}
						catch (Exception e)
						{
							depexceptionrows = depexceptionrows + 1;
							log.ErrorMessage("StagingController", "UpdateEmpDepDetails", "error:" + e.StackTrace.ToString(), e.Message.ToString(), url);
							continue;
						}
					}
					AuditLog auditlog = new AuditLog();
					auditlog.filename = "LoadEmpDepDetails";
					auditlog.uploadedon = DateTime.Now;
					auditlog.uploadedby = "Sch";
					auditlog.uploadedto = "orgdepartments";
					auditlog.modulename = "UpdateEmpDepDetails";
					auditlog.totalrecords = Conversion.toInt(rows);
					auditlog.exceptionrecords = Conversion.toInt(depexceptionrows);
					auditlog.successrecords = Conversion.toInt(rows) - Conversion.toInt(depexceptionrows);
					loadAuditLog(auditlog);
					conn.Close();

					//Employee
					int Empexceptionrows = 0;
					SqlCommand empcmd = new SqlCommand("select * from Employee", conn);
					conn.Open();
					SqlDataReader empdr = empcmd.ExecuteReader();
					int emprows = 0;
					while (empdr.Read())
					{
						emprows = emprows + 1;
						try
						{
							Employee empmodel = new Employee();
							empmodel.employeeno = Convert.ToString(empdr["EmployeeNo"]);
							empmodel.name = Convert.ToString(empdr["Name"]);
							empmodel.nickname = Convert.ToString(empdr["Nickname"]);
							empmodel.shortname = Convert.ToString(empdr["ShortName"]);
							empmodel.globalempno = Convert.ToString(empdr["GlobalEmpNo"]);
							empmodel.ygsaccountcode = Convert.ToString(empdr["YGSAccountCode"]);
							empmodel.domainid = Convert.ToString(empdr["DomainId"]);
							empmodel.ygscostcenter = Convert.ToString(empdr["YGSCostCenter"]);
							empmodel.costcenter = Convert.ToString(empdr["CostCenter"]);
							empmodel.orgdepartmentid = string.IsNullOrEmpty(empdr["OrgDepartmentId"].ToString()) ? (Nullable<short>)null : Convert.ToInt16(empdr["OrgDepartmentId"]);
							empmodel.orgofficeid = string.IsNullOrEmpty(empdr["OrgOfficeId"].ToString()) ? (Nullable<byte>)null : Convert.ToByte(empdr["OrgOfficeId"]);
							empmodel.sex = Convert.ToString(empdr["Sex"]);
							empmodel.maritalstatus = Convert.ToBoolean(empdr["MaritalStatus"]);
							empmodel.dob = string.IsNullOrEmpty(empdr["DOB"].ToString()) ? (DateTime?)null : Convert.ToDateTime(empdr["DOB"]);
							empmodel.boolcontract = Convert.ToBoolean(empdr["BoolContract"]);
							empmodel.doj = string.IsNullOrEmpty(empdr["DOJ"].ToString()) ? (DateTime?)null : Convert.ToDateTime(empdr["DOJ"]);
							empmodel.effectivedoj = string.IsNullOrEmpty(empdr["EffectiveDOJ"].ToString()) ? (DateTime?)null : Convert.ToDateTime(empdr["EffectiveDOJ"]);
							empmodel.confirmationduedate = string.IsNullOrEmpty(empdr["ConfirmationDueDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(empdr["ConfirmationDueDate"]);
							empmodel.confirmationdate = string.IsNullOrEmpty(empdr["ConfirmationDate"].ToString()) ? (DateTime?)null : Convert.ToDateTime(empdr["ConfirmationDate"]);
							empmodel.dol = string.IsNullOrEmpty(empdr["DOL"].ToString()) ? (DateTime?)null : Convert.ToDateTime(empdr["DOL"]);
							empmodel.departmentid = string.IsNullOrEmpty(empdr["DepartmentId"].ToString()) ? (Nullable<byte>)null : Convert.ToByte(empdr["DepartmentId"]);
							empmodel.groupid = string.IsNullOrEmpty(empdr["GroupId"].ToString()) ? (Nullable<short>)null : Convert.ToInt16(empdr["GroupId"]);
							empmodel.deptcode = Convert.ToString(empdr["DeptCode"]);
							empmodel.grade = Convert.ToString(empdr["Grade"]);
							empmodel.designation = Convert.ToString(empdr["Designation"]);
							empmodel.functionalroleid = string.IsNullOrEmpty(empdr["FunctionalRoleId"].ToString()) ? (Int16?)null : Convert.ToInt16(empdr["FunctionalRoleId"]);
							empmodel.email = Convert.ToString(empdr["EMail"]);
							empmodel.serialno = Convert.ToString(empdr["SerialNo"]);
							empmodel.bloodgroup = Convert.ToString(empdr["BloodGroup"]);
							empmodel.hodempno = Convert.ToString(empdr["HODEmpNo"]);
							empmodel.boolhod = Convert.ToBoolean(empdr["BoolHOD"]);
							empmodel.blockid = string.IsNullOrEmpty(empdr["BlockId"].ToString()) ? (Nullable<byte>)null : Convert.ToByte(empdr["BlockId"]);
							empmodel.floorid = string.IsNullOrEmpty(empdr["FloorId"].ToString()) ? (Nullable<short>)null : Convert.ToInt16(empdr["FloorId"]);
							empmodel.qualification = Convert.ToString(empdr["Qualification"]);
							empmodel.qualificationstring = Convert.ToString(empdr["QualificationString"]);
							empmodel.boolfurnishedcertificates = Convert.ToBoolean(empdr["BoolFurnishedCertificates"]);
							empmodel.prevemployment = Convert.ToString(empdr["PrevEmployment"]);
							empmodel.boolexecutive = Convert.ToBoolean(empdr["BoolExecutive"]);
							empmodel.mobileno = Convert.ToString(empdr["MobileNo"]);
							empmodel.basic = string.IsNullOrEmpty(empdr["Basic"].ToString()) ? (Nullable<decimal>)null : Convert.ToDecimal(empdr["Basic"]);
							empmodel.hra = string.IsNullOrEmpty(empdr["HRA"].ToString()) ? (Nullable<decimal>)null : Convert.ToDecimal(empdr["HRA"]);
							empmodel.medicalallowance = string.IsNullOrEmpty(empdr["MedicalAllowance"].ToString()) ? (Nullable<decimal>)null : Convert.ToDecimal(empdr["MedicalAllowance"]);
							empmodel.specialallowance = string.IsNullOrEmpty(empdr["SpecialAllowance"].ToString()) ? (Nullable<decimal>)null : Convert.ToDecimal(empdr["SpecialAllowance"]);
							empmodel.transportallowance = string.IsNullOrEmpty(empdr["TransportAllowance"].ToString()) ? (Nullable<decimal>)null : Convert.ToDecimal(empdr["TransportAllowance"]);
							empmodel.traineeallowance = string.IsNullOrEmpty(empdr["TraineeAllowance"].ToString()) ? (Nullable<decimal>)null : Convert.ToDecimal(empdr["TraineeAllowance"]);
							empmodel.personalpay = string.IsNullOrEmpty(empdr["PersonalPay"].ToString()) ? (Nullable<decimal>)null : Convert.ToDecimal(empdr["PersonalPay"]);
							empmodel.professionalallowance = string.IsNullOrEmpty(empdr["ProfessionalAllowance"].ToString()) ? (Nullable<decimal>)null : Convert.ToDecimal(empdr["ProfessionalAllowance"]);
							empmodel.pfno = string.IsNullOrEmpty(empdr["PFNo"].ToString()) ? (Nullable<int>)null : Convert.ToInt32(empdr["PFNo"]);
							empmodel.fpfno = string.IsNullOrEmpty(empdr["FPFNo"].ToString()) ? (Nullable<int>)null : Convert.ToInt32(empdr["FPFNo"]);
							empmodel.accountsdetails = string.IsNullOrEmpty(empdr["AccountsDetails"].ToString()) ? (string)null : Convert.ToString(empdr["AccountsDetails"]);
							empmodel.boolesi = Convert.ToBoolean(empdr["BoolESI"]);
							empmodel.iciciaccno = string.IsNullOrEmpty(empdr["ICICIAccNo"].ToString()) ? (string)null : Convert.ToString(empdr["ICICIAccNo"]);
							empmodel.medallbal = string.IsNullOrEmpty(empdr["MedAllBal"].ToString()) ? (decimal)0.0 : Convert.ToDecimal(empdr["MedAllBal"]);
							empmodel.pickuppointid = string.IsNullOrEmpty(empdr["PickupPointId"].ToString()) ? (Nullable<short>)null : Convert.ToInt16(empdr["PickupPointId"]);
							empmodel.homephone = Convert.ToString(empdr["HomePhone"]);
							empmodel.presentaddress = Convert.ToString(empdr["PresentAddress"]);
							empmodel.permanentaddress = Convert.ToString(empdr["PermanentAddress"]);
							empmodel.emergencycontactperson = Convert.ToString(empdr["EmergencyContactPerson"]);
							empmodel.emergencycontactno = Convert.ToString(empdr["EmergencyContactPerson"]);
							empmodel.boolhasproximitycard = Convert.ToBoolean(empdr["BoolHasProximityCard"]);
							empmodel.plstatus = string.IsNullOrEmpty(empdr["PLStatus"].ToString()) ? (Nullable<float>)null : float.Parse(empdr["PLStatus"].ToString());
							empmodel.leavesdeductedfromflexidaily = string.IsNullOrEmpty(empdr["LeavesDeductedFromFlexiDaily"].ToString()) ? (Nullable<float>)null : float.Parse(empdr["LeavesDeductedFromFlexiDaily"].ToString());
							empmodel.leavesdeductedfromflexiweekly = string.IsNullOrEmpty(empdr["LeavesDeductedFromFlexiWeekly"].ToString()) ? (Nullable<float>)null : float.Parse(empdr["LeavesDeductedFromFlexiWeekly"].ToString());
							empmodel.restrictedholidaysavailed = string.IsNullOrEmpty(empdr["RestrictedHolidaysAvailed"].ToString()) ? (byte)0 : Convert.ToByte(empdr["RestrictedHolidaysAvailed"]);
							empmodel.paternityleavesavailed = string.IsNullOrEmpty(empdr["PaternityLeavesAvailed"].ToString()) ? (byte)0 : Convert.ToByte(empdr["PaternityLeavesAvailed"]);
							empmodel.nameasinpassport = Convert.ToString(empdr["NameAsInPassport"]);
							empmodel.passportno = Convert.ToString(empdr["PassportNo"]);
							empmodel.passportissuedplace = Convert.ToString(empdr["PassportIssuedDate"]);
							empmodel.passportissueddate = string.IsNullOrEmpty(empdr["PassportIssuedDate"].ToString()) ? (Nullable<System.DateTime>)null : Convert.ToDateTime(empdr["PassportIssuedDate"]);
							empmodel.passportexpirydate = string.IsNullOrEmpty(empdr["PassportExpiryDate"].ToString()) ? (Nullable<System.DateTime>)null : Convert.ToDateTime(empdr["PassportExpiryDate"]);
							empmodel.addressasinpassport = Convert.ToString(empdr["AddressAsInPassport"]);
							empmodel.birthplace = Convert.ToString(empdr["BirthPlace"]);
							empmodel.panno = Convert.ToString(empdr["PanNo"]);
							empmodel.aadhaarno = Convert.ToString(empdr["AadhaarNo"]);
							empmodel.boolkannadiga = string.IsNullOrEmpty(empdr["BoolKannadiga"].ToString()) ? (Nullable<bool>)null : Convert.ToBoolean(empdr["BoolKannadiga"]);
							empmodel.communityid = string.IsNullOrEmpty(empdr["CommunityId"].ToString()) ? (Nullable<byte>)null : Convert.ToByte(empdr["CommunityId"]);
							empmodel.fathersname = Convert.ToString(empdr["FathersName"]);
							empmodel.spousename = Convert.ToString(empdr["SpouseName"]);
							empmodel.organizationid = string.IsNullOrEmpty(empdr["OrganizationId"].ToString()) ? (byte)0 : Convert.ToByte(empdr["OrganizationId"]);
							empmodel.boolintranetenabled = string.IsNullOrEmpty(empdr["BoolIntranetEnabled"].ToString()) ? (bool)false : Convert.ToBoolean(empdr["BoolIntranetEnabled"]);
							empmodel.uan = Convert.ToString(empdr["UAN"]);
							empmodel.expensecategoryid = string.IsNullOrEmpty(empdr["ExpenseCategoryId"].ToString()) ? (Nullable<byte>)null : Convert.ToByte(empdr["ExpenseCategoryId"]);
							empmodel.boolexpatriate = string.IsNullOrEmpty(empdr["BoolExpatriate"].ToString()) ? (bool)false : Convert.ToBoolean(empdr["BoolExpatriate"]);
							empmodel.pwd = Convert.ToString(empdr["PWD"]);

							string query1 = "select count(*) from wms.employee  where employeeno = '" + empdr["EmployeeNo"] + "'";
							int empCnt = int.Parse(pgsql.ExecuteScalar(query1, null).ToString());
							if (empCnt == 0)
							{
								var insertquery = "INSERT INTO wms.employee(employeeno ,name ,nickname ,shortname ,globalempno ,ygsaccountcode ,domainid ,ygscostcenter ,costcenter ,orgdepartmentid ,orgofficeid ,sex ,maritalstatus ,dob ,boolcontract ,doj ,effectivedoj ,confirmationduedate ,confirmationdate ,dol ,departmentid ,groupid ,deptcode ,grade ,designation ,functionalroleid ,email ,serialno ,bloodgroup ,hodempno ,boolhod ,blockid ,floorid ,qualification ,qualificationstring ,boolfurnishedcertificates ,prevemployment ,boolexecutive ,mobileno ,basic ,hra ,medicalallowance ,specialallowance ,transportallowance ,traineeallowance ,personalpay ,professionalallowance ,pfno ,fpfno ,accountsdetails ,boolesi ,iciciaccno ,medallbal ,pickuppointid ,homephone ,presentaddress ,permanentaddress ,emergencycontactperson ,emergencycontactno ,boolhasproximitycard ,plstatus ,leavesdeductedfromflexidaily ,leavesdeductedfromflexiweekly ,restrictedholidaysavailed ,paternityleavesavailed ,nameasinpassport ,passportno ,passportissuedplace ,passportissueddate ,passportexpirydate ,addressasinpassport ,birthplace ,panno ,aadhaarno ,boolkannadiga ,communityid ,fathersname ,spousename ,organizationid ,boolintranetenabled ,uan ,expensecategoryid ,boolexpatriate ,pwd ,roleid)VALUES(@employeeno ,@name ,@nickname ,@shortname ,@globalempno ,@ygsaccountcode ,@domainid ,@ygscostcenter ,@costcenter ,@orgdepartmentid ,@orgofficeid ,@sex ,@maritalstatus ,@dob ,@boolcontract ,@doj ,@effectivedoj ,@confirmationduedate ,@confirmationdate ,@dol ,@departmentid ,@groupid ,@deptcode ,@grade ,@designation ,@functionalroleid ,@email ,@serialno ,@bloodgroup ,@hodempno ,@boolhod ,@blockid ,@floorid ,@qualification ,@qualificationstring ,@boolfurnishedcertificates ,@prevemployment ,@boolexecutive ,@mobileno ,@basic ,@hra ,@medicalallowance ,@specialallowance ,@transportallowance ,@traineeallowance ,@personalpay ,@professionalallowance ,@pfno ,@fpfno ,@accountsdetails ,@boolesi ,@iciciaccno ,@medallbal ,@pickuppointid ,@homephone ,@presentaddress ,@permanentaddress ,@emergencycontactperson ,@emergencycontactno ,@boolhasproximitycard ,@plstatus ,@leavesdeductedfromflexidaily ,@leavesdeductedfromflexiweekly ,@restrictedholidaysavailed ,@paternityleavesavailed ,@nameasinpassport ,@passportno ,@passportissuedplace ,@passportissueddate ,@passportexpirydate ,@addressasinpassport ,@birthplace ,@panno ,@aadhaarno ,@boolkannadiga ,@communityid ,@fathersname ,@spousename ,@organizationid ,@boolintranetenabled ,@uan ,@expensecategoryid ,@boolexpatriate ,@pwd ,@roleid)";
								var results = pgsql.ExecuteScalar(insertquery, new
								{

									empmodel.employeeno,
									empmodel.name,
									empmodel.nickname,
									empmodel.shortname,
									empmodel.globalempno,
									empmodel.ygsaccountcode,
									empmodel.domainid,
									empmodel.ygscostcenter,
									empmodel.costcenter,
									empmodel.orgdepartmentid,
									empmodel.orgofficeid,
									empmodel.sex,
									empmodel.maritalstatus,
									empmodel.dob,
									empmodel.boolcontract,
									empmodel.doj,
									empmodel.effectivedoj,
									empmodel.confirmationduedate,
									empmodel.confirmationdate,
									empmodel.dol,
									empmodel.departmentid,
									empmodel.groupid,
									empmodel.deptcode,
									empmodel.grade,
									empmodel.designation,
									empmodel.functionalroleid,
									empmodel.email,
									empmodel.serialno,
									empmodel.bloodgroup,
									empmodel.hodempno,
									empmodel.boolhod,
									empmodel.blockid,
									empmodel.floorid,
									empmodel.qualification,
									empmodel.qualificationstring,
									empmodel.boolfurnishedcertificates,
									empmodel.prevemployment,
									empmodel.boolexecutive,
									empmodel.mobileno,
									empmodel.basic,
									empmodel.hra,
									empmodel.medicalallowance,
									empmodel.specialallowance,
									empmodel.transportallowance,
									empmodel.traineeallowance,
									empmodel.personalpay,
									empmodel.professionalallowance,
									empmodel.pfno,
									empmodel.fpfno,
									empmodel.accountsdetails,
									empmodel.boolesi,
									empmodel.iciciaccno,
									empmodel.medallbal,
									empmodel.pickuppointid,
									empmodel.homephone,
									empmodel.presentaddress,
									empmodel.permanentaddress,
									empmodel.emergencycontactperson,
									empmodel.emergencycontactno,
									empmodel.boolhasproximitycard,
									empmodel.plstatus,
									empmodel.leavesdeductedfromflexidaily,
									empmodel.leavesdeductedfromflexiweekly,
									empmodel.restrictedholidaysavailed,
									empmodel.paternityleavesavailed,
									empmodel.nameasinpassport,
									empmodel.passportno,
									empmodel.passportissuedplace,
									empmodel.passportissueddate,
									empmodel.passportexpirydate,
									empmodel.addressasinpassport,
									empmodel.birthplace,
									empmodel.panno,
									empmodel.aadhaarno,
									empmodel.boolkannadiga,
									empmodel.communityid,
									empmodel.fathersname,
									empmodel.spousename,
									empmodel.organizationid,
									empmodel.boolintranetenabled,
									empmodel.uan,
									empmodel.expensecategoryid,
									empmodel.boolexpatriate,
									empmodel.pwd,
									empmodel.roleid
								});
							}
							else
							{
								var updateqry = "update wms.employee set name = @name ,nickname = @nickname ,shortname = @shortname , globalempno = @globalempno ,ygsaccountcode = @ygsaccountcode , domainid = @domainid , ygscostcenter = @ygscostcenter , costcenter = @costcenter , orgdepartmentid = @orgdepartmentid , orgofficeid = @orgofficeid , sex = @sex , maritalstatus = @maritalstatus , dob = @dob , boolcontract = @boolcontract , doj = doj ,effectivedoj = @effectivedoj , confirmationduedate = @confirmationduedate , confirmationdate = @confirmationdate , dol = @dol , departmentid = @departmentid , groupid = @groupid , deptcode = @deptcode , grade = @grade , designation = @designation , functionalroleid = @functionalroleid , email = @email , serialno = @serialno , bloodgroup = @bloodgroup , hodempno = @hodempno , boolhod = @boolhod , blockid = @blockid , floorid = @floorid , qualification = @qualification , qualificationstring = @qualificationstring , boolfurnishedcertificates = @boolfurnishedcertificates , prevemployment = @prevemployment , boolexecutive = @boolexecutive , mobileno = @mobileno , basic = basic , hra = @hra ,medicalallowance = @medicalallowance , specialallowance = @specialallowance , transportallowance = @transportallowance , traineeallowance = @traineeallowance , personalpay = @personalpay , professionalallowance = @professionalallowance , pfno = @pfno , fpfno = @fpfno , accountsdetails = @accountsdetails , boolesi = @boolesi , iciciaccno = @iciciaccno , medallbal = @medallbal , pickuppointid = @pickuppointid , homephone = @homephone , presentaddress = @presentaddress , permanentaddress = @permanentaddress , emergencycontactperson = @emergencycontactperson , emergencycontactno = @emergencycontactno , boolhasproximitycard = @boolhasproximitycard , plstatus = @plstatus , leavesdeductedfromflexidaily = @leavesdeductedfromflexidaily , leavesdeductedfromflexiweekly = @leavesdeductedfromflexiweekly , restrictedholidaysavailed = @restrictedholidaysavailed , paternityleavesavailed = @paternityleavesavailed , nameasinpassport = @nameasinpassport , passportno = @passportno , passportissuedplace = @passportissuedplace , passportissueddate = @passportissueddate , passportexpirydate = @passportexpirydate , addressasinpassport = @addressasinpassport , birthplace = @birthplace , panno = @panno , aadhaarno = @aadhaarno , boolkannadiga = @boolkannadiga , communityid = @communityid , fathersname = @fathersname , spousename = @spousename , organizationid = @organizationid , boolintranetenabled = @boolintranetenabled , uan = @uan , expensecategoryid = @expensecategoryid , boolexpatriate = @boolexpatriate , pwd = @pwd, roleid = @roleid where employeeno= '" + empdr["EmployeeNo"] + "'";
								var rslt = pgsql.Execute(updateqry, new
								{

									empmodel.name,
									empmodel.nickname,
									empmodel.shortname,
									empmodel.globalempno,
									empmodel.ygsaccountcode,
									empmodel.domainid,
									empmodel.ygscostcenter,
									empmodel.costcenter,
									empmodel.orgdepartmentid,
									empmodel.orgofficeid,
									empmodel.sex,
									empmodel.maritalstatus,
									empmodel.dob,
									empmodel.boolcontract,
									empmodel.doj,
									empmodel.effectivedoj,
									empmodel.confirmationduedate,
									empmodel.confirmationdate,
									empmodel.dol,
									empmodel.departmentid,
									empmodel.groupid,
									empmodel.deptcode,
									empmodel.grade,
									empmodel.designation,
									empmodel.functionalroleid,
									empmodel.email,
									empmodel.serialno,
									empmodel.bloodgroup,
									empmodel.hodempno,
									empmodel.boolhod,
									empmodel.blockid,
									empmodel.floorid,
									empmodel.qualification,
									empmodel.qualificationstring,
									empmodel.boolfurnishedcertificates,
									empmodel.prevemployment,
									empmodel.boolexecutive,
									empmodel.mobileno,
									empmodel.basic,
									empmodel.hra,
									empmodel.medicalallowance,
									empmodel.specialallowance,
									empmodel.transportallowance,
									empmodel.traineeallowance,
									empmodel.personalpay,
									empmodel.professionalallowance,
									empmodel.pfno,
									empmodel.fpfno,
									empmodel.accountsdetails,
									empmodel.boolesi,
									empmodel.iciciaccno,
									empmodel.medallbal,
									empmodel.pickuppointid,
									empmodel.homephone,
									empmodel.presentaddress,
									empmodel.permanentaddress,
									empmodel.emergencycontactperson,
									empmodel.emergencycontactno,
									empmodel.boolhasproximitycard,
									empmodel.plstatus,
									empmodel.leavesdeductedfromflexidaily,
									empmodel.leavesdeductedfromflexiweekly,
									empmodel.restrictedholidaysavailed,
									empmodel.paternityleavesavailed,
									empmodel.nameasinpassport,
									empmodel.passportno,
									empmodel.passportissuedplace,
									empmodel.passportissueddate,
									empmodel.passportexpirydate,
									empmodel.addressasinpassport,
									empmodel.birthplace,
									empmodel.panno,
									empmodel.aadhaarno,
									empmodel.boolkannadiga,
									empmodel.communityid,
									empmodel.fathersname,
									empmodel.spousename,
									empmodel.organizationid,
									empmodel.boolintranetenabled,
									empmodel.uan,
									empmodel.expensecategoryid,
									empmodel.boolexpatriate,
									empmodel.pwd,
									empmodel.roleid
								});
							}
						}
						catch (Exception e)
						{
							Empexceptionrows = Empexceptionrows + 1;
							log.ErrorMessage("StagingController", "UpdateEmpDepDetails", "error:" + e.StackTrace.ToString(), e.Message.ToString(), url);
							continue;
						}

					}
					conn.Close();
					AuditLog auditlog1 = new AuditLog();
					auditlog1.filename = "LoadEmpDepDetails";
					auditlog1.uploadedon = DateTime.Now;
					auditlog1.uploadedby = "Sch";
					auditlog1.uploadedto = "employee";
					auditlog1.modulename = "UpdateEmpDepDetails";
					auditlog1.totalrecords = Conversion.toInt(emprows);
					auditlog1.exceptionrecords = Conversion.toInt(Empexceptionrows);
					auditlog1.successrecords = Conversion.toInt(emprows) - Conversion.toInt(Empexceptionrows);
					loadAuditLog(auditlog1);
				}
			}
			catch (Exception e)
			{
				var res = e;
				log.ErrorMessage("StagingController", "UpdateEmpDepDetails", "error:" + e.StackTrace.ToString(), e.Message.ToString());
			}
			return Ok(true);
		}


		/*function : <<uploadSODataExcel>>  Author :<<Gayathri>>  
		Date of Creation <<01-03-2021>>
		Purpose : <<Update SO data from excel>>
		Review Date :<<>>   Reviewed By :<<>>
		Sourcecode Copyright : Yokogawa India Limited*/
		[HttpGet]
		[Route("uploadSODataExcel")]
		public IActionResult uploadSODataExcel()
		{
			try
			{
				string serverPath = "";
				using (NpgsqlConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					serverPath = config.FilePath;
					//var filePath = serverPath + "Yil_Po_Daily_report_" + DateTime.Now.ToString("dd-MM-yyyy").Replace("-", "_") + ".xlsx";
					var filePath = @"D:\Projects\WMS\Docs\label data\QTSO_1stApril2020_31stFeb2021.xlsx";
					DB.Open();
					if(filePath!=null)
                    {
						var filePathstr = filePath;
						string[] filearr = filePathstr.Split("\\");
						string nameoffile = filearr[filearr.Length - 1];
						DataTable dtexcel = new DataTable();
						string poitem = "";
						System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
						using (var stream1 = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
						{
							using (var reader = ExcelReaderFactory.CreateReader(stream1))
							{

								var result = reader.AsDataSet(new ExcelDataSetConfiguration()
								{
									ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
									{
										UseHeaderRow = true
									}
								});

								dtexcel = result.Tables[0];

							}
						}



						string uploadcode = Guid.NewGuid().ToString();
						int i = 0;
						foreach (DataRow row in dtexcel.Rows)
						{
							SOdataExcel model = new SOdataExcel();

							try
							{
								
								model.sono = Conversion.toStr(row["Sales Document No."]);
								string projectdef = Conversion.toStr(row["WBS Element (Level 2)"]);
								if(projectdef.Length>0)
                                {
									model.projectdef = projectdef.Substring(0, projectdef.Length - 2);
								}
                                else
                                {
									model.projectdef = null;
                                }
								
								model.soitemno = Conversion.toStr(row["Sales Order Item No."]);
								model.serviceorderno = Conversion.toStr(row["Service Order Number"]);
								model.customercode = Conversion.toStr(row["Sold-to party"]);
								model.customername = Conversion.toStr(row["Name: Sold-to party"]);
								

								string error_description = "";
								bool dataloaderror = false;
								
								if (string.IsNullOrEmpty(model.projectdef))
									error_description += "There is NO Project Definition";
								

								var insertquery = "INSERT INTO wms.stag_so_sap(sono,soitemno,projectdef,serviceorderno,customercode,customername,error_description)";
								insertquery += " VALUES(@sono, @soitemno,@projectdef,@serviceorderno,@customercode,@customername,@error_description)";
								var results = DB.ExecuteScalar(insertquery, new
								{
									model.sono,
									model.soitemno,
									model.projectdef,
									model.serviceorderno,
									model.customercode,
									model.customername,
									error_description
									
								});



							}
							catch (Exception e)
							{
								var res = e;
								log.ErrorMessage("StagingController", "uploadSODataExcel", e.StackTrace.ToString(), "SO:" + model.sono + "error:" + e.Message.ToString(), url);
								continue;
							}
						}

						DB.Close();
						AuditLog auditlog = new AuditLog();
						auditlog.filename = nameoffile;
						auditlog.filelocation = filePath;
						auditlog.uploadedon = DateTime.Now;
						auditlog.uploadedto = "STAG_SO_SAP";
						auditlog.modulename = "uploadSOData";



						loadAuditLog(auditlog);
						
					}
				}
				
			}
			catch (Exception e)
			{
				var res = e;
				log.ErrorMessage("StagingController", "uploadSODataExcel", e.StackTrace.ToString(), "error:" + e.Message.ToString(), url);
			}
			return Ok(true);
		}

		/*function : <<uploadAssetDataExcel>>  Author :<<Gayathri>>  
		Date of Creation <<01-03-2021>>
		Purpose : <<Update Asset data from excel>>
		Review Date :<<>>   Reviewed By :<<>>
		Sourcecode Copyright : Yokogawa India Limited*/
		[HttpGet]
		[Route("uploadAssetDataExcel")]
		public IActionResult uploadAssetDataExcel()
		{
			try
			{
				string serverPath = "";
				using (NpgsqlConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					serverPath = config.FilePath;
					//var filePath = serverPath + "Yil_Po_Daily_report_" + DateTime.Now.ToString("dd-MM-yyyy").Replace("-", "_") + ".xlsx";
					var filePath = @"D:\Projects\WMS\Docs\label data\CostcenterMaster&AssetBOP2 codes.xlsx";
					DB.Open();
					var filePathstr = filePath;
					string[] filearr = filePathstr.Split("\\");
					string nameoffile = filearr[filearr.Length - 1];
					DataTable dtexcel = new DataTable();
					string poitem = "";
					System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
					using (var stream1 = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
					{
						using (var reader = ExcelReaderFactory.CreateReader(stream1))
						{

							var result = reader.AsDataSet(new ExcelDataSetConfiguration()
							{
								ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
								{
									UseHeaderRow = true
								}
							});

							dtexcel = result.Tables[1];

						}
					}



					string uploadcode = Guid.NewGuid().ToString();
					int i = 0;
					foreach (DataRow row in dtexcel.Rows)
					{


						try
						{
							AssetData model = new AssetData();
							model.material = Conversion.toStr(row["Material"]);
							model.materialdescription = Conversion.toStr(row["Material description"]);
							

							string error_description = "";
							bool dataloaderror = false;
							if (string.IsNullOrEmpty(model.material.Replace('.', '#')))
								error_description += "There is No Material";
							
							var insertquery = "INSERT INTO wms.stag_asset(material,materialdescription,eror_description)";
							insertquery += " VALUES(@material,@materialdescription, @error_description)";
							var results = DB.ExecuteScalar(insertquery, new
							{
								model.material,
								model.materialdescription,
								error_description
								
							});



						}
						catch (Exception e)
						{
							var res = e;
							log.ErrorMessage("StagingController", "uploadAssetDataExcel", e.StackTrace.ToString(), "PO:" + poitem + "error:" + e.Message.ToString(), url);
							continue;
						}
					}

					DB.Close();
					AuditLog auditlog = new AuditLog();
					auditlog.filename = nameoffile;
					auditlog.filelocation = filePath;
					auditlog.uploadedon = DateTime.Now;
					auditlog.uploadedto = "stag_asset";
					auditlog.modulename = "uploadAssetDataExcel";



					loadAuditLog(auditlog);
					loadPOData(uploadcode);

					//}
				}
			}
			catch (Exception e)
			{
				var res = e;
				log.ErrorMessage("StagingController", "uploadAssetDataExcel", e.StackTrace.ToString(), "error:" + e.Message.ToString(), url);
			}
			return Ok(true);
		}

		/*function : <<uploadCostCenterDataExcel>>  Author :<<Gayathri>>  
		Date of Creation <<01-03-2021>>
		Purpose : <<Update Cost Center data from excel>>
		Review Date :<<>>   Reviewed By :<<>>
		Sourcecode Copyright : Yokogawa India Limited*/
		[HttpGet]
		[Route("uploadCostCenterDataExcel")]
		public IActionResult uploadCostCenterDataExcel()
		{
			try
			{
				string serverPath = "";
				using (NpgsqlConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					serverPath = config.FilePath;
					//var filePath = serverPath + "Yil_Po_Daily_report_" + DateTime.Now.ToString("dd-MM-yyyy").Replace("-", "_") + ".xlsx";
					var filePath = @"D:\Projects\WMS\Docs\label data\CostcenterMaster&AssetBOP2 codes.xlsx";
					DB.Open();
					var filePathstr = filePath;
					string[] filearr = filePathstr.Split("\\");
					string nameoffile = filearr[filearr.Length - 1];
					DataTable dtexcel = new DataTable();
					string poitem = "";
					System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
					using (var stream1 = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
					{
						using (var reader = ExcelReaderFactory.CreateReader(stream1))
						{

							var result = reader.AsDataSet(new ExcelDataSetConfiguration()
							{
								ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
								{
									UseHeaderRow = true
								}
							});

							dtexcel = result.Tables[0];

						}
					}



					string uploadcode = Guid.NewGuid().ToString();
					int i = 0;
					foreach (DataRow row in dtexcel.Rows)
					{


						try
						{
							CostCenterData model = new CostCenterData();
							model.costcenter = Conversion.toStr(row["Cost Center "]);
							model.shorttext = Conversion.toStr(row["Short Text "]);
							
							string error_description = " ";
							bool dataloaderror = false;
							if (string.IsNullOrEmpty(model.costcenter))
								error_description += "There is NO Cost Center Data";
							
							var insertquery = "INSERT INTO wms.stag_costcenter(costcenter,shorttext,eror_description)";
							insertquery += " VALUES(@costcenter,@shorttext, @error_description)";
							var results = DB.ExecuteScalar(insertquery, new
							{
								model.costcenter,
								model.shorttext,
								error_description
							});



						}
						catch (Exception e)
						{
							var res = e;
							log.ErrorMessage("StagingController", "uploadCostCenterDataExcel", e.StackTrace.ToString(), "PO:" + poitem + "error:" + e.Message.ToString(), url);
							continue;
						}
					}

					DB.Close();
					AuditLog auditlog = new AuditLog();
					auditlog.filename = nameoffile;
					auditlog.filelocation = filePath;
					auditlog.uploadedon = DateTime.Now;
					auditlog.uploadedto = "stag_costcenter";
					auditlog.modulename = "uploadCostCenterDataExcel";



					loadAuditLog(auditlog);
					
				}
			}
			catch (Exception e)
			{
				var res = e;
				log.ErrorMessage("StagingController", "uploadCostCenterDataExcel", e.StackTrace.ToString(), "error:" + e.Message.ToString(), url);
			}
			return Ok(true);
		}


	}
}
