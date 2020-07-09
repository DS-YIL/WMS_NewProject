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

		/*Name of Function : <<uploadExcel>>  Author :<<Prasanna>>  
		Date of Creation <<02-07-2020>>
		Purpose : <<Write briefly in one line or two lines>>
		Review Date :<<>>   Reviewed By :<<>>
		Sourcecode Copyright : Yokogawa India Limited
		*/

		[HttpGet]
		[Route("uploadExcel")]
		public IActionResult uploadExcel()
		{
			using (NpgsqlConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
			{

				DB.Open();
				//var filePath = @"http://10.29.15.183:100/WMSFiles/stageTest.xlsx";
				var filePath = @"D:\WMSFiles\stageTest.xlsx";
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
							"'" + row["D"].ToString() + "','" + row["DCI"].ToString() + "','SAP','" + DateTime.Now + "'," + dataloaderror + ", '" + Error_Description + "')";
						NpgsqlCommand dbcmd = DB.CreateCommand();
						dbcmd.CommandText = query;
						dbcmd.ExecuteNonQuery();
					}
					catch (Exception e)
					{
						var res = e;
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


					string query = "select * from wms.stag_po_sap where purchdoc !='' and material !=''  ";
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
								var id= pgsql.QuerySingleOrDefault<string>("Select id  from wms.wms_pomaterials where pono = '" + stag_data.purchdoc + "' and materialid='" + stag_data.material + "'", null, commandType: CommandType.Text);
								var updateqyery = "update wms.wms_pomaterials set materialqty = @materialqty where id=" + id+"";
								
									var re = Convert.ToInt32(pgsql.Execute(updateqyery, new

									{
										stag_data.materialqty

									}));
									

								
							}
						}
						catch (Exception e)
						{
							var res = e;
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
