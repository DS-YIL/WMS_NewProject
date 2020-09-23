/*
    Name of File : <<StagingController>>  Author :<<Prasanna>>  
    Date of Creation <<02-07-2020>>
    Purpose : <<to save files from SAP system to WMS tables>>
    Review Date :<<>>   Reviewed By :<<>>
    Sourcecode Copyright : Yokogawa India Limited
*/

using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
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


		/*Name of Function : <<uploadInitialStockExcel>>  Author :<<Prasanna>>  
		Date of Creation <<16-09-2020>>
		Purpose : <<Write briefly in one line or two lines>>
		Review Date :<<>>   Reviewed By :<<>>
		Sourcecode Copyright : Yokogawa India Limited
		*/
		[HttpGet]
		[Route("uploadInitialStockExcel")]
		public IActionResult uploadInitialStockExcel()
		{
			using (NpgsqlConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
			{

				DB.Open();
				//var filePath = @"http://10.29.15.183:100/WMSFiles/stageTest1.xlsx";
				var filePath = @"D:\YILProjects\WMS\WMSFiles\WMS_InitialStockUpload.xlsx";
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

						if (string.IsNullOrEmpty((row["Material"].ToString())))
							Error_Description += "There is NO Material";
						if (string.IsNullOrEmpty(row["Material Description"].ToString()))
							Error_Description += "No material description";
						if (string.IsNullOrEmpty(row["Store"].ToString()))
							Error_Description += "No Store";
						if (string.IsNullOrEmpty(row["Rack"].ToString()))
							Error_Description += "No Rack";
						if (string.IsNullOrEmpty(row["Bin"].ToString()))
							Error_Description += "No Bin";
						if (string.IsNullOrEmpty(row["Quanity"].ToString()))
							Error_Description += "No Quanity";
						if (!string.IsNullOrEmpty(Error_Description))
							dataloaderror = true;

						var query = "INSERT INTO wms.st_initialstock (material,materialdescription,store,rack,bin,quanity,grn,receiveddate,shelflifeexpiration,dateofmanufacture,datasource,dataenteredby,dataenteredon,createddate,DataloadErrors ,Error_Description)VALUES('" + row["Material"].ToString() + "'," +
							"'" + row["Material Description"].ToString() + "','" + row["Store"].ToString() + "','" + row["Rack"].ToString() + "','" + row["Bin"].ToString() + "','" + row["Quanity"].ToString() + "'," +
							"'" + row["GRN"].ToString() + "','" + (Convert.ToDateTime(row["Received date"]).ToString("yyyy-MM-dd")) + "','" + (Convert.ToDateTime(row["Shelf life expiration"]).ToString("yyyy-MM-dd")) + "','" + (Convert.ToDateTime(row["Date of Manufacture"]).ToString("yyyy-MM-dd")) + "'," +
							"'" + row["DataSource"].ToString() + "','" + row["Data Entered By"].ToString() + "','" + (Convert.ToDateTime(row["Data Entered On"]).ToString("yyyy-MM-dd")) + "'," +
							"'" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "'," + dataloaderror + ", '" + Error_Description + "')";
						NpgsqlCommand dbcmd = DB.CreateCommand();
						dbcmd.CommandText = query;
						dbcmd.ExecuteNonQuery();
					}
					catch (Exception e)
					{
						var res = e;
						log.ErrorMessage("StagingController", "uploadInitialStockExcel", e.StackTrace.ToString());
						continue;
					}
				}

				DB.Close();
				loadStockData();
				return Ok(true);
			}
		}

		/*Name of Function : <<loadStockData>>  Author :<<Prasanna>>  
		Date of Creation <<16-09-2020>>
		Purpose : <<Write briefly in one line or two lines>>
		Review Date :<<>>   Reviewed By :<<>>
		Sourcecode Copyright : Yokogawa India Limited
		*/
		public IActionResult loadStockData()
		{
			using (NpgsqlConnection pgsql = new NpgsqlConnection(config.PostgresConnectionString))

			{
				{

					string query = "select * from wms.st_initialstock where material  !='' and materialdescription !='' and store !='' and rack !='' and bin  !='' and quanity  !='0' and dataloaderrors=false";
					pgsql.Open();
					var stagingList = pgsql.Query<StagingStockModel>(
					   query, null, commandType: CommandType.Text);
					foreach (StagingStockModel stag_data in stagingList)
					{
						try
						{
							// add master table data for store,rack,bin

							//Add locator masterdata
							string storeQuery = "Select locatorid from wms.wms_rd_locator where locatorname = '" + stag_data.store + "'";
							var storeId =pgsql.ExecuteScalar(storeQuery, null);
							if (storeId == null)
							{
								LocationModel store = new LocationModel();
								store.locatorname = stag_data.store;
								store.createdate = DateTime.Now;
								store.isexcelupload = true;
								//insert wms_rd_locator ##locatorname
								var insertStorequery = "INSERT INTO wms.wms_rd_locator(locatorid, locatorname, createdate,isexcelupload)VALUES(default, @locatorname,@createdate,@isexcelupload) returning locatorid";
								var Storeresults = pgsql.ExecuteScalar(insertStorequery, new
								{
									store.locatorname,
									store.createdate,
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
								var insertRackquery = "INSERT INTO wms.wms_rd_rack(rackid,racknumber, locatorid,createdate,isexcelupload)VALUES(default,@racknumber,@locatorid,@createdate,@isexcelupload)returning rackid";
								var rackresults = pgsql.ExecuteScalar(insertRackquery, new
								{
									store.racknumber,
									store.locatorid,
									store.createdate,
									store.isexcelupload
								});
								rackId = Convert.ToInt32(rackresults);
							}

							//Add Bin masterdata if not exist
							string binQuery = "Select binid from wms.wms_rd_bin where binnumber = '" + stag_data.bin + "' and locatorid=" + storeId + " and rackid=" + rackId + "";
							var binId = pgsql.ExecuteScalar(binQuery, null);
							if (binId == null)
							{
								LocationModel store = new LocationModel();
								store.binnumber = stag_data.bin;
								store.locatorid = Convert.ToInt32(storeId);
								store.rackid = Convert.ToInt32(rackId);
								store.createdate = DateTime.Now;
								store.isexcelupload = true;
								//insert wms_rd_locator ##locatorname
								var insertbinQuery = "INSERT INTO wms.wms_rd_bin(binid,binnumber, locatorid,rackid,createdate,isexcelupload)VALUES(default,@binnumber,@locatorid,@rackid,@createdate,@isexcelupload) returning binid";
								var binresults = pgsql.ExecuteScalar(insertbinQuery, new
								{
									store.binnumber,
									store.locatorid,
									store.rackid,
									store.createdate,
									store.isexcelupload
								});
								binId = Convert.ToInt32(binresults);
							}


							StockModel stock = new StockModel();
							stock.storeid = Convert.ToInt32(storeId);
							stock.rackid = Convert.ToInt32(rackId);
							stock.binid = Convert.ToInt32(binId);
							stock.totalquantity = Convert.ToInt32(stag_data.quanity);
							stock.shelflife = stag_data.shelflifeexpiration;
							stock.createddate = DateTime.Now;
							stock.materialid = stag_data.material;
							stock.initialstock = true;

							//insert wms_stock ##storeid, binid,rackid,totalquantity,shelflife ,createddate,materialid ,initialstock
							var insertquery = "INSERT INTO wms.wms_stock(storeid, binid,rackid,totalquantity,shelflife ,createddate,materialid ,initialstock)VALUES(@storeid, @binid,@rackid,@totalquantity,@shelflife ,@createddate,@materialid ,@initialstock)";
							var results = pgsql.ExecuteScalar(insertquery, new
							{
								stock.storeid,
								stock.binid,
								stock.rackid,
								stock.totalquantity,
								stock.shelflife,
								stock.createddate,
								stock.materialid,
								stock.initialstock
							});


						}
						catch (Exception e)
						{
							var res = e;
							log.ErrorMessage("StagingController", "loadStockData", e.StackTrace.ToString());
							continue;
						}
					}
					pgsql.Close();

					//throw new NotImplementedException();
				}
			}
			return Ok(true);

		}
	}
}
