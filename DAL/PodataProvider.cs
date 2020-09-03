using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using WMS.Common;
using WMS.Interfaces;
using System.Web;
using WMS.Models;
using System.Web;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Net.Sockets;
using System.Net;
using ZXing;
using ZXing.Common;
using ZXing.CoreCompat.System.Drawing;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;

namespace WMS.DAL
{
	public class PodataProvider : IPodataService<OpenPoModel>
	{
		Configurations config = new Configurations();
		ErrorLogTrace log = new ErrorLogTrace();
		/// <summary>
		/// check pono exists or not 
		/// </summary>
		/// <param name="PONO"></param>
		/// <returns></returns>
		public OpenPoModel CheckPoexists(string PONO)
		{

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					pgsql.Open();
					//WMSResource.checkponoexists.Replace("#pono", PONO);
					//string query = "select pono,suppliername as vendorname from wms.wms_polist where pono = '" + PONO + "'";
					string query = "select asno.asn as asnno,asno.pono,pl.suppliername as vendorname from wms.wms_asn asno left outer join wms.wms_polist pl on pl.pono = asno.pono where asno.asn = '" + PONO.Trim() + "'";
					var podata = pgsql.QueryFirstOrDefault<OpenPoModel>(
					   query, null, commandType: CommandType.Text);
					if (podata != null)
					{
						podata.ispono = false;
						return podata;
					}
					else
					{
						query = "select pono,suppliername as vendorname from wms.wms_polist where pono = '" + PONO.Trim() + "'";
						var podata1 = pgsql.QueryFirstOrDefault<OpenPoModel>(
						   query, null, commandType: CommandType.Text);
						if (podata1 != null)
						{
							podata1.ispono = true;
						}
						return podata1;
					}
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "CheckPoexists", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
			//throw new NotImplementedException();
		}


		/// <summary>
		/// get lst of open pono list
		/// </summary>
		/// <param name="loginid"></param>
		/// <param name="pono"></param>
		/// <param name="docno"></param>
		/// <param name="vendorid"></param>
		/// <returns></returns>
		public async Task<IEnumerable<OpenPoModel>> getOpenPoList(string loginid, string pono = null, string docno = null, string vendorid = null)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.openpolist.Replace("#projectmanager", loginid);
					if (pono != null)
					{
						query = query + " and wp.pono='" + pono + "'";
					}
					if (docno != null)
					{
						query = query + " and wp.documentno='" + docno + "'";
					}
					if (vendorid != null)
					{
						query = query + " and  wp.vendorid=" + vendorid;
					}
					query = query + " group by track.pono,wp.pono order by wp.pono desc ";

					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<OpenPoModel>(
					   query, null, commandType: CommandType.Text);
					return data;


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getOpenPoList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}


		//Gayathri -get po numbers and qty
		public async Task<IEnumerable<POList>> getPOList(string postatus)
		{

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				List<POList> objpo = null;
				try
				{
					//if(postatus=="Instock")
					//               {
					//	objpo = new List<POList>();
					//	string query = WMSResource.getpolist;
					//	await pgsql.OpenAsync();
					//	var result= await pgsql.QueryAsync<POList>(
					//	   query, null, commandType: CommandType.Text);

					//	foreach(var podata in result)
					//                   {
					//		string poquery = "select count(*) from wms.wms_securityinward secinw join wms.wms_stock sk on secinw.inwmasterid = sk.inwmasterid where secinw.pono = '"+podata.POno+"'";

					//		int? po = pgsql.QuerySingleOrDefault<int?>(
					//							poquery, null, commandType: CommandType.Text);
					//		if(po>0)
					//                       {
					//			podata.status = "Instock";
					//			objpo.Add(podata);
					//		}

					//	}

					//	return objpo;
					//}
					//else if(postatus== "SecurityCheck")
					//               {
					//	objpo = new List<POList>();
					//	string query = WMSResource.getpolist;
					//	await pgsql.OpenAsync();
					//	var result = await pgsql.QueryAsync<POList>(
					//	   query, null, commandType: CommandType.Text);

					//	foreach (var podata in result)
					//	{
					//		string poquery = "select count(*) from wms.wms_securityinward secinw join wms.wms_stock sk on secinw.inwmasterid = sk.inwmasterid where secinw.pono = '" + podata.POno + "'";

					//		int? po = pgsql.QuerySingleOrDefault<int?>(
					//							poquery, null, commandType: CommandType.Text);

					//		if (po == 0)
					//		{
					//			poquery = "select count(*) from wms.wms_securityinward secinw join wms.wms_storeinward sinw on secinw.inwmasterid = sinw.inwmasterid where secinw.pono = '" + podata.POno + "'";

					//			 po = pgsql.QuerySingleOrDefault<int?>(
					//								poquery, null, commandType: CommandType.Text);

					//			if (po > 0)
					//			{
					//				podata.status = "Security Check";
					//				objpo.Add(podata);
					//			}
					//		}



					//	}

					//	return objpo;
					//}
					//else if(postatus== "Pending")
					//               {
					//	objpo = new List<POList>();
					//	string query = WMSResource.getpolist;
					//	await pgsql.OpenAsync();
					//	var result = await pgsql.QueryAsync<POList>(
					//	   query, null, commandType: CommandType.Text);

					//	foreach (var podata in result)
					//	{
					//		string poquery = "select count(*) from wms.wms_securityinward secinw join wms.wms_stock sk on secinw.inwmasterid = sk.inwmasterid where secinw.pono = '" + podata.POno + "'";

					//		int? po = pgsql.QuerySingleOrDefault<int?>(
					//							poquery, null, commandType: CommandType.Text);

					//		if (po == 0)
					//		{
					//			poquery = "select count(*) from wms.wms_securityinward secinw join wms.wms_storeinward sinw on secinw.inwmasterid = sinw.inwmasterid where secinw.pono = '" + podata.POno + "'";

					//			po = pgsql.QuerySingleOrDefault<int?>(
					//							   poquery, null, commandType: CommandType.Text);

					//			if (po == 0)
					//			{
					//				 poquery = "select count(*) from wms.wms_securityinward where pono ='" + podata.POno + "'";

					//				 po = pgsql.QuerySingleOrDefault<int?>(
					//									poquery, null, commandType: CommandType.Text);
					//				if (po == 0)
					//				{
					//					podata.status = "Pending";
					//					objpo.Add(podata);
					//				}
					//			}
					//		}
					//	}

					//	return objpo;
					//}

					if (postatus == "Instock")
					{
						string query = "select mats.pono,SUM(mats.materialqty) as qty from wms.wms_pomaterials mats where mats.pono in (select DISTINCT sinw.pono from wms.wms_stock sinw) group by mats.pono";
						await pgsql.OpenAsync();
						var result = await pgsql.QueryAsync<POList>(
						   query, null, commandType: CommandType.Text);
						if (result != null)
						{
							foreach (var data in result)
							{
								data.status = "Instock";
							}
							return result;
						}

					}
					else if (postatus == "SecurityCheck")
					{
						string query = "select mats.pono,SUM(mats.materialqty) as qty from wms.wms_pomaterials mats where mats.pono in (select DISTINCT sinw.pono from wms.wms_securityinward sinw where sinw.grnnumber is not null and sinw.pono not in (select  DISTINCT pono from wms.wms_stock))group by mats.pono";
						await pgsql.OpenAsync();
						var result = await pgsql.QueryAsync<POList>(
						   query, null, commandType: CommandType.Text);

						if (result != null)
						{
							foreach (var data in result)
							{
								data.status = "Security Check";
							}
							return result;
						}

					}
					else if (postatus == "Pending")
					{
						string query = "select mats.pono,SUM(mats.materialqty) as qty from wms.wms_pomaterials mats where mats.pono not in (select DISTINCT pono from wms.wms_securityinward) group by mats.pono";
						await pgsql.OpenAsync();
						var result = await pgsql.QueryAsync<POList>(
						   query, null, commandType: CommandType.Text);
						if (result != null)
						{
							foreach (var data in result)
							{
								data.status = "Pending";
							}
							return result;
						}

					}

					return null;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getPOList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}

		///<summary>
		///Generating barcode
		///</summary>
		public printMaterial generateBarcodeMaterial(printMaterial printMat)
		{
			try
			{
				string path = "";

				path = Environment.CurrentDirectory + @"\Barcodes\";

				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
				//generate barcode for material code and GRN No.
				var content = printMat.grnno + "-" + printMat.materialid;
				BarcodeWriter writer = new BarcodeWriter
				{
					Format = BarcodeFormat.QR_CODE,
					Options = new EncodingOptions
					{
						Height = 90,
						Width = 100,
						PureBarcode = false,
						Margin = 1,

					},
				};
				var bitmap = writer.Write(content);

				// write text and generate a 2-D barcode as a bitmap
				writer
					.Write(content)
					.Save(path + content + ".bmp");

				printMat.barcodePath = "./Barcodes/" + content + ".bmp";
				//printMat.barcodePath = "./assets/" + content + ".bmp";

				//Barcode design for material code
				//generate barcode for material code and GRN No.
				content = printMat.materialid;
				BarcodeWriter writerData = new BarcodeWriter
				{
					Format = BarcodeFormat.QR_CODE,
					Options = new EncodingOptions
					{
						Height = 90,
						Width = 100,
						PureBarcode = false,
						Margin = 1,

					},
				};
				bitmap = writerData.Write(content);

				// write text and generate a 2-D barcode as a bitmap
				writer
					.Write(content)
					.Save(path + content + ".bmp");

				printMat.materialcodePath = "./Barcodes/" + content + ".bmp";
				//printMat.materialcodePath = "./assets/" + content + ".bmp";

			}
			catch (Exception ex)
			{
				printMat.errorMsg = ex.Message;
				log.ErrorMessage("PODataProvider", "generateBarcodeMaterial", ex.StackTrace.ToString());
			}
			return printMat;
		}

		/// <summary>
		/// inserting barcode info
		/// </summary>
		/// <param name="dataobj"></param>
		/// <returns></returns>
		public string InsertBarcodeInfo(BarcodeModel dataobj)
		{
			try
			{
				//dataobj.docfile = ;
				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					var q1 = WMSResource.getinvoiceexists.Replace("#pono", dataobj.pono).Replace("#invno", dataobj.invoiceno);
					int count = int.Parse(DB.ExecuteScalar(q1, null).ToString());

					if (count >= 1)
					{
						return "2"; //for onvoice already exist
					}
					else
					{
						dataobj.createddate = System.DateTime.Now;
						string insertquery = WMSResource.insertbarcodedata;
						var result = "0";
						string insertqueryforinvoice = WMSResource.insertinvoicedata;
						dataobj.receiveddate = System.DateTime.Now;
						if (dataobj.pono == "NONPO")
						{
							string compare = "NP" + DateTime.Now.Year.ToString().Substring(2);
							string Query = WMSResource.getmaxnonpo;
							var data = DB.QueryFirstOrDefault<POList>(
							Query, null, commandType: CommandType.Text);
							if (data != null)
							{
								string[] poserial = data.POno.Split('P');
								int serial = Convert.ToInt32(poserial[1].Substring(2));
								int nextserial = serial + 1;
								string nextpo = "NP" + DateTime.Now.Year.ToString().Substring(2) + nextserial.ToString().PadLeft(5, '0');
								dataobj.pono = nextpo;
								
								var q2 = WMSResource.getinvoiceexists.Replace("#pono", dataobj.pono).Replace("#invno", dataobj.invoiceno); 
								int count1 = int.Parse(DB.ExecuteScalar(q2, null).ToString());

								if (count1 >= 1)
								{
									return "2"; //for onvoice already exist
								}
								string type = "NON PO";
								string insertpoqry = WMSResource.insertpo;
								DB.Execute(insertpoqry, new
								{
									dataobj.pono,
									dataobj.suppliername,
									type
									//barcodeid,
								});


							}
							else
							{
								string nextpo = "NP" + DateTime.Now.Year.ToString().Substring(2) + "00001";
								dataobj.pono = nextpo;
								var q2 = WMSResource.getinvoiceexists.Replace("#pono", dataobj.pono).Replace("#invno", dataobj.invoiceno);
								int count1 = int.Parse(DB.ExecuteScalar(q2, null).ToString());

								if (count1 >= 1)
								{
									return "2"; //for onvoice already exist
								}
								string type = "NON PO";
								string insertpoqry = WMSResource.insertpo;
								try
								{
									DB.Execute(insertpoqry, new
									{
										dataobj.pono,
										dataobj.suppliername,
										type
										//barcodeid,
									});
								}
								catch (Exception ex)
								{
									string msg = ex.Message;
								}

							}


						}
						dataobj.invoicedate = DateTime.Now;

						string filename = "";
						if (dataobj.pono.StartsWith("NP"))
						{
							filename = dataobj.pono + "_" + dataobj.docfile;
						}

						var results = DB.Execute(insertqueryforinvoice, new
						{
							dataobj.invoicedate,
							dataobj.departmentid,
							dataobj.invoiceno,
							dataobj.receiveddate,
							dataobj.receivedby,
							dataobj.pono,
							dataobj.deleteflag,
							dataobj.suppliername,
							dataobj.asnno,
							dataobj.inwardremarks,
							filename
							//barcodeid,
						});
						if (results != 0)
						{
							EmailModel emailmodel = new EmailModel();
							emailmodel.pono = dataobj.pono;
							emailmodel.ToEmailId = "developer1@in.yokogawa.com";
							emailmodel.FrmEmailId = "sushma.patil@in.yokogawa.com";
							EmailUtilities emailobj = new EmailUtilities();
							//emailobj.sendEmail(emailmodel, 1);

						}
						////}
						return (dataobj.pono);
					}

				}
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "InsertBarcodeInfo", Ex.StackTrace.ToString());
				return "0";
			}

		}

		//Get Invoice details based on PONO.
		public async Task<IEnumerable<InvoiceDetails>> getinvoiveforpo(string PONO)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getInvoiceDetails.Replace("#pono", PONO);
					return await pgsql.QueryAsync<InvoiceDetails>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getinvoiveforpo", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}

		//get Material details based on grn number
		public async Task<IEnumerable<MaterialDetails>> getMaterialDetails(string grnNo)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					List<MaterialDetails> objMaterial = new List<MaterialDetails>();
					MaterialDetails result = null;
					string getMatQuery = WMSResource.getmatforgrnno.Replace("#grn", grnNo);
					var MaterialList = await pgsql.QueryAsync<MaterialDetails>(
					   getMatQuery, null, commandType: CommandType.Text);

					if (MaterialList != null)
					{
						int totalissed = 0;
						foreach (MaterialDetails mtData in MaterialList)
						{
							if (mtData.materialid != null && mtData.materialid != "")
							{
								result = new MaterialDetails();
								result.materialid = mtData.materialid;
								result.materialdescription = mtData.materialdescription;
								//result.availableqty = mtData.availableqty;
								result.grnnumber = mtData.grnnumber;
								result.confirmqty = mtData.confirmqty;
								//To get issued qty get data from material issue, material reserve and gatepassmaterial table
								int issuedqty = 0;
								int reservedqty = 0;

								Enquirydata enquiryobj = new Enquirydata();
								string availableQtyqry = "select sum(availableqty) as availableqty from wms.wms_stock where materialid ='" + mtData.materialid + "' and pono='" + mtData.pono + "' and inwmasterid =" + mtData.inwmasterid;
								enquiryobj = pgsql.QuerySingleOrDefault<Enquirydata>(
												availableQtyqry, null, commandType: CommandType.Text);
								result.availableqty = enquiryobj.availableqty;

								ReportModel modelobj = new ReportModel();
								string matIssuedQuery = "select sum(issuedqty)as issuedqty from wms.wms_materialissue where itemid =" + mtData.itemid;
								modelobj = pgsql.QuerySingleOrDefault<ReportModel>(
												matIssuedQuery, null, commandType: CommandType.Text);
								if (modelobj != null)
								{
									issuedqty = modelobj.issuedqty;
								}
								//Get material reserved qty
								ReserveMaterialModel modeldataobj = new ReserveMaterialModel();
								string matReserveQuery = "select sum(reservedqty)as reservedqty from wms.wms_materialreserve where itemid =" + mtData.itemid;
								modeldataobj = pgsql.QuerySingleOrDefault<ReserveMaterialModel>(
												matReserveQuery, null, commandType: CommandType.Text);
								if (modeldataobj != null)
								{
									reservedqty = modeldataobj.reservedqty;
								}
								int gatepassissuedqty = 0;

								//get material in gatepass
								gatepassModel obj = new gatepassModel();
								//string matgateQuery = "select sum(gtmat.quantity)as quantity from wms.wms_gatepassmaterial gtmat left join wms.wms_gatepass gp on gp.gatepassid = gtmat.gatepassid where materialid = '" + mtData.materialid + "' and gp.approvedon != null and gp.approverstatus!=null";
								string matgateQuery = "select sum(gtmat.quantity)as quantity from wms.wms_gatepassmaterial gtmat left join wms.wms_gatepass gp on gp.gatepassid = gtmat.gatepassid left join wms.wms_materialissue matiss on matiss.itemid = '" + mtData.itemid + "'where materialid = '" + mtData.materialid + "' and gp.approvedon != null and gp.approverstatus != null and matiss.gatepassmaterialid = gtmat.gatepassmaterialid";
								//IssueRequestModel obj = new IssueRequestModel();
								//pgsql.Open();

								obj = pgsql.QuerySingle<gatepassModel>(
								   matgateQuery, null, commandType: CommandType.Text);

								gatepassissuedqty = obj.quantity;
								totalissed = Convert.ToInt32(issuedqty) + Convert.ToInt32(reservedqty) + gatepassissuedqty;
								result.issued = totalissed;


							}

						}
						objMaterial.Add(result);
					}





					//string reqDetailsQuery = WMSResource.getrequestDetailsMaterial.Replace("#grn", grnNo);



					//var requestDetails = await pgsql.QueryAsync<MaterialDetails>(
					//   reqDetailsQuery, null, commandType: CommandType.Text);
					//if(requestDetails != null)
					//{
					//    foreach(MaterialDetails matData in requestDetails)
					//    {
					//        string reserveQuery = WMSResource.getreserveQtyDetailsMaterial.Replace("#grn", matData.grnnumber);
					//        string issuedQuery = WMSResource.getissuedQtyDetailsMaterial.Replace("#grn", matData.grnnumber);
					//        var reserveDetails = await pgsql.QueryAsync<MaterialDetails>(
					//   reserveQuery, null, commandType: CommandType.Text);



					//        var issuedDetails = await pgsql.QueryAsync<MaterialDetails>(
					//  issuedQuery, null, commandType: CommandType.Text);
					//    }
					//    result.grnnumber = requestDetails.FirstOrDefault().grnnumber;
					//    result.reservedqty = requestDetails.FirstOrDefault().reservedqty;
					//    result.qtyavailable = requestDetails.FirstOrDefault().qtyavailable;
					//    result.qtytotal = requestDetails.FirstOrDefault().qtytotal;
					//}









					//var finaltempdata = requestDetails.Concat(reserveDetails);
					//var finaldata = finaltempdata.Concat(issuedDetails);





					//foreach(MaterialDetails mtdata in finaldata)
					//{
					//    mtdata.issued =Convert.ToInt32( mtdata.reservedqty) + Convert.ToInt32(mtdata.issuedqty);
					//}
					//return finaldata;



					return objMaterial;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getMaterialDetails", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}


		//Get location details based on material id
		public async Task<IEnumerable<LocationDetails>> getlocationdetails(string materialid, string grnnumber)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getLocationDetails.Replace("#materialid", materialid).Replace("#grn", grnnumber);
					return await pgsql.QueryAsync<LocationDetails>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getlocationdetails", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}


		//Get material requested, acknowledged and issued details
		public async Task<IEnumerable<ReqMatDetails>> getReqMatdetails(string materialid, string grnnumber)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				//ReqMatDetails obj = new ReqMatDetails();
				ReqMatDetails objs = null;
				List<ReqMatDetails> listobj = new List<ReqMatDetails>();
				try
				{
					string query = WMSResource.getMaterialRequestDetails.Replace("#materialid", materialid).Replace("#grnnumber", grnnumber);
					//await pgsql.OpenAsync();
					var obj = await pgsql.QueryAsync<ReqMatDetails>(
					   query, null, commandType: CommandType.Text);
					foreach (var matdata in obj)
					{
						string reservequery = "select max(emp.name) as requestername,max(emp1.name)  as approvedby,max(res.reservedon) as issuedon,sum(res.reservedqty) as quantity from wms.wms_materialreserve res left join wms.employee emp on res.reservedby = emp.employeeno left join wms.employee emp1 on emp1.employeeno = res.releasedby where res.itemid=" + matdata.itemid;
						var data = pgsql.QuerySingle<ReqMatDetails>(
						   reservequery, null, commandType: CommandType.Text);
						objs = new ReqMatDetails();
						objs.quantity = data.quantity;
						objs.type = "Project Reserve";
						objs.requestername = data.requestername;
						objs.issuedon = data.issuedon;
						objs.details = matdata.jobname;
						objs.approvername = data.approvername;
						if (objs.quantity != 0)
						{
							listobj.Add(objs);
						}

						string requstedquery = "select sum(issue.issuedqty)as quantity,max(emp.name) as requestername,max(emp1.name) as approvername from wms.wms_materialissue issue inner join wms.wms_materialrequest req on req.requestforissueid = issue.requestforissueid left join wms.employee emp on emp.employeeno = req.requesterid left join wms.employee emp1 on emp1.employeeno = req.approverid  where issue.itemid=" + matdata.itemid;
						var data1 = pgsql.QuerySingle<ReqMatDetails>(
					   requstedquery, null, commandType: CommandType.Text);
						objs = new ReqMatDetails();
						objs.quantity = data1.quantity;
						objs.type = "Project Requested";
						objs.requestername = data1.requestername;
						objs.issuedon = data1.issuedon;
						objs.details = matdata.jobname;
						objs.approvername = data1.approvername;
						objs.acknowledge = data1.requestername;
						if (objs.quantity != 0)
						{
							listobj.Add(objs);
						}
						//string gatepassquery = " select max(gate.gatepasstype)as gatepasstype,sum(mat.quantity)as quantity,max(gate.approvedon) as issuedon,max(emp.name) as requestername,max(emp1.name) as approvername from wms.wms_gatepass gate  inner join wms.wms_gatepassmaterial mat on mat.gatepassid = gate.gatepassid  left join wms.employee emp on emp.employeeno = gate.requestedby left join wms.employee emp1 on emp1.employeeno = gate.approvedby where mat.materialid = '" + obj.materialid + "' and gate.approvedon != null and gate.approverstatus!=null";
						string gatepassquery = "select max(gate.gatepasstype)as gatepasstype,sum(wmissue.issuedqty)as quantity,max(gate.approvedon) as issuedon,max(emp.name) as requestername,max(emp1.name) as approvername from wms.wms_gatepass gate  inner join wms.wms_gatepassmaterial mat on mat.gatepassid = gate.gatepassid  left join wms.employee emp on emp.employeeno = gate.requestedby left join wms.employee emp1 on emp1.employeeno = gate.approvedby join wms.wms_materialissue wmissue  on mat.gatepassmaterialid = wmissue.gatepassmaterialid where mat.materialid = '" + matdata.materialid + "' and gate.approvedon != null and gate.approverstatus != null";
						var data2 = pgsql.QuerySingleOrDefault<ReqMatDetails>(
					   gatepassquery, null, commandType: CommandType.Text);
						objs = new ReqMatDetails();
						if (data2.quantity != 0)
						{
							objs.quantity = data2.quantity;
							objs.type = data2.gatepasstype;
							objs.requestername = data2.requestername;
							objs.issuedon = data1.issuedon;
							objs.details = matdata.jobname;
							objs.approvername = data1.approvername;
							objs.acknowledge = data1.requestername;
							listobj.Add(objs);
						}

					}
					return listobj;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getReqMatdetails", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}

		public async Task<IEnumerable<OpenPoModel>> GetDeatilsForholdgr(string status)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					await pgsql.OpenAsync();
					string query = WMSResource.getHoldGRdetail.Replace("#status", status);
					var data = await pgsql.QueryAsync<OpenPoModel>(
						   query, null, commandType: CommandType.Text);
					return data;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetDeatilsForholdgr", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}



		/// <summary>
		/// get list of info for three way matching
		/// </summary>
		/// <param name="invoiceno"></param>
		/// <param name="pono"></param>
		/// <returns></returns>
		public async Task<IEnumerable<OpenPoModel>> GetDeatilsForthreeWaymatching(string invoiceno, string pono, bool isgrn, string grnno)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					inwardModel obj = new inwardModel();
					await pgsql.OpenAsync();
					if (!isgrn)
					{
						string query1 = WMSResource.getinwmasterid.Replace("#pono", pono).Replace("#invoiceno", invoiceno);
						obj = pgsql.QuerySingle<inwardModel>(
						query1, null, commandType: CommandType.Text);
					}
					else
					{
						string query1 = WMSResource.getinwardidbygrn.Replace("#grnno", grnno);
						obj = pgsql.QuerySingle<inwardModel>(
						query1, null, commandType: CommandType.Text);
					}


					List<OpenPoModel> datalist = new List<OpenPoModel>();
					if (obj.inwmasterid != 0)
					{
						string query = "";
						if (pono.StartsWith("NP"))
						{
							//Replace("#pono", pono).Replace("#invoiceno", invoiceno)
							query = WMSResource.receivequeryfornonpo;// + pono+"'";//li
							if (isgrn)
							{
								query += " where sinw.grnnumber = '" + grnno + "' and (sinw.holdgrstatus is NULL or sinw.holdgrstatus =  'accepted' or sinw.holdgrstatus = 'hold')";
							}
							else
							{
								query += " where sinw.pono = '" + pono + "'  and sinw.invoiceno = '" + invoiceno + "' and (sinw.holdgrstatus is NULL or sinw.holdgrstatus =  'accepted' or sinw.holdgrstatus = 'hold')";
							}

						}
						else
						{
							inwardModel objx = new inwardModel();
							if (!isgrn)
							{
								//string queryx = "select grnnumber,onhold,unholdedby from wms.wms_securityinward where pono = '" + pono + "' and invoiceno = '" + invoiceno + "'";
								string queryx = WMSResource.isgrnexistsquerybyinvoce.Replace("#pono",pono).Replace("#invno",invoiceno);
								objx = pgsql.QuerySingle<inwardModel>(
								 queryx, null, commandType: CommandType.Text);
							}
							else
							{
								//string queryx = "select grnnumber,onhold,unholdedby from wms.wms_securityinward where grnnumber = '" + grnno + "'";
								string queryx = WMSResource.isgrnexistsbygrn.Replace("#grnno", grnno);
								objx = pgsql.QuerySingle<inwardModel>(
								queryx, null, commandType: CommandType.Text);
							}
							if ((objx.grnnumber == null || objx.grnnumber == "") && !objx.onhold && (objx.unholdedby == null || objx.unholdedby == ""))
							{
								query = WMSResource.Getdetailsforthreewaymatching;
								if (isgrn)
								{
									query += " where sinw.grnnumber = '" + grnno + "'";
								}
								else
								{
									query += " where mat.pono = '" + pono + "'  and sinw.invoiceno = '" + invoiceno + "'";
								}
							}
							else
							{
								query = WMSResource.receivequeryfornonpo;
								if (isgrn)
								{
									query += " where sinw.grnnumber = '" + grnno + "' and (sinw.holdgrstatus is NULL or sinw.holdgrstatus =  'accepted' or sinw.holdgrstatus = 'hold')";
								}
								else
								{
									query += " where sinw.pono = '" + pono + "'  and sinw.invoiceno = '" + invoiceno + "' and (sinw.holdgrstatus is NULL or sinw.holdgrstatus =  'accepted' or sinw.holdgrstatus = 'hold')";
								}
							}

						}
						var data = await pgsql.QueryAsync<OpenPoModel>(
						   query, null, commandType: CommandType.Text);
						if (data.Count() == 0)
						{
							if (pono.StartsWith("NP"))
							{
								OpenPoModel po = new OpenPoModel();
								po.pono = pono;
								po.invoiceno = invoiceno;
								datalist.Add(po);
								data = datalist;
							}
						}
						if (data.Count() > 0)
						{

							foreach (OpenPoModel po in data)
							{
								var fdata = datalist.Where(o => o.Material == po.Material && o.Materialdescription == po.Materialdescription).FirstOrDefault();
								if (fdata == null)
								{
									string querya = "select sinw.pono,inw.materialid,Max(inw.materialqty) as materialqty,SUM(inw.confirmqty) as confirmqty from wms.wms_securityinward sinw";
									querya += " left outer join wms.wms_storeinward inw on inw.inwmasterid = sinw.inwmasterid";
									querya += " where sinw.pono = '" + po.pono + "' and inw.materialid = '" + po.Material + "'";
									querya += " group by sinw.pono,inw.materialid";
									var datax = await pgsql.QueryAsync<OpenPoModel>(
									querya, null, commandType: CommandType.Text);
									if (datax.Count() > 0)
									{
										po.isreceivedpreviosly = true;
										int pendingqty = 0;
										if (datax.FirstOrDefault().confirmqty > 0)
										{
											pendingqty = po.materialqty - datax.FirstOrDefault().confirmqty;
										}
										else
										{
											pendingqty = po.materialqty - datax.FirstOrDefault().receivedqty;
										}

										if (pendingqty < 0)
										{
											po.pendingqty = 0;
										}
										else
										{
											po.pendingqty = pendingqty;
										}

									}
									datalist.Add(po);
								}

							}
						}
					}


					return datalist;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetDeatilsForthreeWaymatching", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}


		/// <summary>
		/// get list of info for quality check
		/// Ramesh kumar 07/07/2020
		/// </summary>
		/// <param name="invoiceno"></param>
		/// <param name="pono"></param>
		/// <returns></returns>
		public async Task<IEnumerable<OpenPoModel>> Getqualitydetails(string grnnumber)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					await pgsql.OpenAsync();
					string query = WMSResource.getdataforqualitydetails.Replace("#grnno", grnnumber);// + pono+"'";//li
					var data = await pgsql.QueryAsync<OpenPoModel>(
					   query, null, commandType: CommandType.Text);
					return data;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "Getqualitydetails", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}
		/// <summary>
		/// to verify three way match and generate GRN No
		/// </summary>
		/// <param name="pono"></param>
		/// <param name="invoiceno"></param>
		/// <returns></returns>
		public async Task<OpenPoModel> VerifythreeWay(string pono, string invoiceno)
		{
			OpenPoModel verify = new OpenPoModel();
			sequencModel obj = new sequencModel();
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					await pgsql.OpenAsync();
					string lastinsertedgrn = WMSResource.lastinsertedgrn;
					iwardmasterModel info = new iwardmasterModel();
					string query = "";

					if (pono.StartsWith("NP"))
					{
						query = "select Count(*),grnnumber,pono from wms.wms_securityinward where invoiceno = '" + invoiceno + "' and pono = '" + pono + "' group by grnnumber,pono";

					}
					else
					{
						query = WMSResource.Verifythreewaymatch.Replace("#pono", pono).Replace("#invoiceno", invoiceno);

					}

					info = pgsql.QuerySingle<iwardmasterModel>(
					  query, null, commandType: CommandType.Text);
					if (info != null && info.grnnumber == null)
					{
						//iwardmasterModel infos = new iwardmasterModel();
						//string queryforgrn = WMSResource.verifyGRNgenerated.Replace("#pono", pono).Replace("#invoiceno", invoiceno);
						// infos = pgsql.QuerySingle<iwardmasterModel>(
						//   queryforgrn, null, commandType: CommandType.Text);
						//if (infos.grnnumber == null)
						//{
						//verify = s;
						int grnnextsequence = 0;
						string grnnumber = string.Empty;
						obj = pgsql.QuerySingle<sequencModel>(
					   lastinsertedgrn, null, commandType: CommandType.Text);
						if (obj.id != 0)
						{
							grnnextsequence = (Convert.ToInt32(obj.sequencenumber) + 1);
							grnnumber = obj.sequenceid + "-" + obj.year + "-" + grnnextsequence.ToString().PadLeft(6, '0');
							string updategrnnumber = WMSResource.updategrnnumber.Replace("#invoiceno", invoiceno).Replace("#pono", pono);
							using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
							{
								var results = DB.ExecuteScalar(updategrnnumber, new
								{
									grnnumber,

								});
							}
							verify.grnnumber = grnnumber;
							int id = obj.id;
							string updateseqnumber = WMSResource.updateseqnumber;
							using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
							{
								var results = DB.ExecuteScalar(updateseqnumber, new
								{
									grnnextsequence,
									id,

								});
							}
						}




						else
						{

						}
						string insertqueryforstatus = WMSResource.statusupdatebySecurity;
						using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						{
							var results = DB.ExecuteScalar(insertqueryforstatus, new
							{
								pono,

							});
						}
						//}
					}

					else
					{
						verify.grnnumber = info.grnnumber;
					}
					return verify;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "VerifythreeWay", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}
		/// <summary>
		/// to receive material
		/// Ramesh kumar 07/07/2020
		/// </summary>
		/// <param name="datamodel"></param>
		/// <returns></returns>

		public async Task<string> receivequantity(List<inwardModel> datamodel)
		{

			inwardModel obj = new inwardModel();
			inwardModel getgrnnoforpo = new inwardModel();
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					datamodel[0].receiveddate = System.DateTime.Now;
					await pgsql.OpenAsync();

					string query = WMSResource.getinwmasterid.Replace("#pono", datamodel[0].pono).Replace("#invoiceno", datamodel[0].invoiceno);
					obj = pgsql.QuerySingle<inwardModel>(
					   query, null, commandType: CommandType.Text);
					string getGRNno = WMSResource.getGRNNo.Replace("#pono", datamodel[0].pono);
					getgrnnoforpo = pgsql.QueryFirstOrDefault<inwardModel>(
					   getGRNno, null, commandType: CommandType.Text);
					int inwardid = 0;
					if (obj.inwmasterid != 0)
					{
						int loop = 0;
						bool isupdateprocess = false;

						string unholdedby = datamodel[0].unholdedby;
						if (unholdedby != null && unholdedby != "")
						{
							isupdateprocess = true;
						}
						foreach (var item in datamodel)
						{
							item.receiveddate = System.DateTime.Now;
							string insertforinvoicequery = WMSResource.receiveforinvoice;
							item.deleteflag = false;
							string materialid = item.Material;
							bool? qualitychecked = null;
							if (!item.qualitycheck)
							{
								qualitychecked = true;

							}

							using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
							{
								if (!isupdateprocess)
								{
									var results = DB.ExecuteScalar(insertforinvoicequery, new
									{
										obj.inwmasterid,
										item.receiveddate,
										item.receivedby,
										item.receivedqty,
										materialid,
										item.deleteflag,
										item.qualitycheck,
										qualitychecked,
										item.materialqty,
										item.receiveremarks

									});
									inwardid = Convert.ToInt32(results);

									if (inwardid != 0 && loop == 0)
									{
										if (item.onhold)
										{
											string qry = "Update wms.wms_securityinward set onhold = " + item.onhold + ",onholdremarks = '" + item.onholdremarks + "',holdgrstatus='hold' where pono = '" + item.pono + "' and invoiceno = '" + item.invoiceno + "'";
											var results11 = DB.ExecuteScalar(qry);
										}

									}
									loop++;

								}
								else
								{
									string qrry = WMSResource.updatereceiptunhold.Replace("#inwardid", item.inwardid.ToString());
									var results = DB.ExecuteScalar(qrry, new
									{
										item.receiveddate,
										item.receivedby,
										item.receivedqty,
										item.qualitycheck,
										qualitychecked,
										item.receiveremarks

									});


									if (loop == 0)
									{
										if (item.onhold)
										{
											string qry = "Update wms.wms_securityinward set onhold = " + item.onhold + ",onholdremarks = '" + item.onholdremarks + "',holdgrstatus='hold' where pono = '" + item.pono + "' and invoiceno = '" + item.invoiceno + "'";
											var results11 = DB.ExecuteScalar(qry);
										}
									}
									loop++;
								}


								//if (inwardid != 0)
								//{
								//    string insertqueryforqualitycheck =WMSResource.insertqueryforqualitycheck;

								//    var data = DB.ExecuteScalar(insertqueryforqualitycheck, new
								//    {
								//        inwardid,
								//        datamodel.quality,
								//        datamodel.qtype,
								//        datamodel.qcdate,
								//        datamodel.qcby,
								//        datamodel.remarks,
								//        datamodel.deleteflag,

								//    });
								//string insertqueryforstatusforqty = WMSResource.insertqueryforstatusforqty;

								//var data1 = DB.ExecuteScalar(insertqueryforstatusforqty, new
								//{
								//	item.pono,
								//	item.returnqty

								//});

							}
						}
					}
					if (inwardid != 0)
					{
						EmailModel emailmodel = new EmailModel();
						emailmodel.pono = datamodel[0].pono;
						emailmodel.jobcode = datamodel[0].projectname;
						emailmodel.ToEmailId = "developer1@in.yokogawa.com";
						emailmodel.FrmEmailId = "sushma.patil@in.yokogawa.com";
						EmailUtilities emailobj = new EmailUtilities();
						//emailobj.sendEmail(emailmodel, 2);
					}
					//}
					return (Convert.ToString(inwardid));
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "insertquantity", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="datamodel"></param>
		/// <returns></returns>
		public async Task<string> insertquantity(List<inwardModel> datamodel)
		{

			inwardModel obj = new inwardModel();
			inwardModel getgrnnoforpo = new inwardModel();
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					datamodel[0].receiveddate = System.DateTime.Now;
					await pgsql.OpenAsync();

					string query = WMSResource.getinwmasterid.Replace("#pono", datamodel[0].pono).Replace("#invoiceno", datamodel[0].invoiceno);
					obj = pgsql.QuerySingle<inwardModel>(
					   query, null, commandType: CommandType.Text);
					string getGRNno = WMSResource.getGRNNo.Replace("#pono", datamodel[0].pono);
					getgrnnoforpo = pgsql.QueryFirstOrDefault<inwardModel>(
					   getGRNno, null, commandType: CommandType.Text);
					int inwardid = 0;
					if (obj.inwmasterid != 0)
					{

						foreach (var item in datamodel)
						{
							string insertforinvoicequery = WMSResource.insertforinvoicequery;
							item.deleteflag = false;
							string materialid = item.Material;
							using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
							{
								var results = DB.ExecuteScalar(insertforinvoicequery, new
								{
									obj.inwmasterid,
									//item.poitemid,
									item.receiveddate,
									item.receivedby,
									item.receivedqty,
									item.returnqty,
									item.confirmqty,
									materialid,
									item.deleteflag,

								});
								inwardid = Convert.ToInt32(results);
								//if (inwardid != 0)
								//{
								//    string insertqueryforqualitycheck =WMSResource.insertqueryforqualitycheck;

								//    var data = DB.ExecuteScalar(insertqueryforqualitycheck, new
								//    {
								//        inwardid,
								//        datamodel.quality,
								//        datamodel.qtype,
								//        datamodel.qcdate,
								//        datamodel.qcby,
								//        datamodel.remarks,
								//        datamodel.deleteflag,

								//    });
								string insertqueryforstatusforqty = WMSResource.insertqueryforstatusforqty;

								var data1 = DB.ExecuteScalar(insertqueryforstatusforqty, new
								{
									item.pono,
									item.returnqty

								});

							}
						}
					}

					//}
					return (Convert.ToString(inwardid));
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "insertquantity", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}
		/// <summary>
		/// inserting material details to warehouse
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public string InsertStock(List<StockModel> data)
		{
			try
			{
				StockModel obj = new StockModel();
				string loactiontext = string.Empty;
				var result = 0;
				int inwmasterid = 0;
				foreach (var item in data)
				{



					using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
					{
						StockModel objs = new StockModel();
						pgsql.Open();
						string query = WMSResource.getinwardmasterid.Replace("#grnnumber", item.grnnumber);
						objs = pgsql.QueryFirstOrDefault<StockModel>(
						   query, null, commandType: CommandType.Text);
						if (objs != null)
							inwmasterid = objs.inwmasterid;
					}
					//foreach (var item in data) { 
					item.createddate = System.DateTime.Now;
					string insertquery = WMSResource.insertstock;
					int itemid = 0;
					//if (data.itemid == 0)
					//{
					string materialid = item.Material;
					item.availableqty = item.confirmqty;
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						result = Convert.ToInt32(DB.ExecuteScalar(insertquery, new
						{
							inwmasterid,
							item.pono,
							item.binid,
							item.vendorid,
							item.totalquantity,
							item.shelflife,
							item.availableqty,
							item.deleteflag,
							//data.itemreceivedfrom,
							item.itemlocation,
							item.createddate,
							item.createdby,
							item.stockstatus,
							materialid,
							item.inwardid
						}));
						if (result != 0)
						{
							itemid = Convert.ToInt32(result);
							string insertqueryforlocationhistory = WMSResource.insertqueryforlocationhistory;
							var results = DB.ExecuteScalar(insertqueryforlocationhistory, new
							{
								item.itemlocation,
								itemid,
								item.createddate,
								item.createdby,

							});
							string insertqueryforstatuswarehouse = WMSResource.insertqueryforstatuswarehouse;

							var data1 = DB.ExecuteScalar(insertqueryforstatuswarehouse, new
							{
								item.pono,

							});

							EmailModel emailmodel = new EmailModel();
							emailmodel.pono = item.pono;
							emailmodel.ToEmailId = "developer1@in.yokogawa.com";
							emailmodel.FrmEmailId = "sushma.patil@in.yokogawa.com";
							emailmodel.CC = "sushma.patil@in.yokogawa.com";
							EmailUtilities emailobj = new EmailUtilities();
							//emailobj.sendEmail(emailmodel, 4);
						}
					}
					//}

					//else
					//{
					//	itemid = data.itemid;
					//	string updatequery = WMSResource.updatelocation.Replace("#itemlocation", data.itemlocation).Replace("#itemid", Convert.ToString(itemid));

					//	using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					//	{
					//		result = DB.Execute(updatequery, new
					//		{
					//			data.binid,
					//			data.rackid

					//		});
					//	}
					//}
					using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
					{


						pgsql.Open();

						string selectqueryforloaction = WMSResource.getlocationasresponse.Replace("#itemid", itemid.ToString());
						obj = pgsql.QuerySingle<StockModel>(
							   selectqueryforloaction, null, commandType: CommandType.Text);
						if (obj.binnumber != null)
						{
							loactiontext = obj.binnumber;
						}
						else if (obj.racknumber != null)
						{
							loactiontext = obj.racknumber;
						}
						else
						{
							loactiontext = "no data";
						}
					}
				}
				return (loactiontext);

			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "InsertStock", Ex.StackTrace.ToString());
				return null;
			}


		}       /// <summary>
				/// to get search data and pass  query dynamically
				/// </summary>
				/// <param name="Result"></param>
				/// <returns></returns>
		public DataTable GetListItems(DynamicSearchResult Result)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					pgsql.OpenAsync();
					DataTable dataTable = new DataTable();
					IDbCommand selectCommand = pgsql.CreateCommand();
					string query = "";
					if (Result.tableName == "wms.wms_project")
					{
						query = "select distinct projectcode,projectname from " + Result.tableName + Result.searchCondition + "";
					}
					if (Result.tableName == "wms.wms_stock")
					{
						query = "select distinct itemlocation from " + Result.tableName + Result.searchCondition + "";
					}
					else
					{
						query = "select * from " + Result.tableName + Result.searchCondition + "";
					}

					if (!string.IsNullOrEmpty(Result.query))
						query = Result.query;
					selectCommand.CommandText = query;
					IDbDataAdapter dbDataAdapter = new NpgsqlDataAdapter();
					dbDataAdapter.SelectCommand = selectCommand;

					DataSet dataSet = new DataSet();

					dbDataAdapter.Fill(dataSet);
					return dataTable = dataSet.Tables[0];
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetListItems", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}

		}

		//Get material List- gayathri  GetMaterialItems
		public DataTable GetMaterialItems(DynamicSearchResult Result)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					pgsql.OpenAsync();
					DataTable dataTable = new DataTable();
					IDbCommand selectCommand = pgsql.CreateCommand();
					string query = "";
					query = "select Distinct(sk.materialid),ygs.unitprice as materialcost from " + Result.tableName + WMSResource.getgatepassunitprice + Result.searchCondition + "";
					if (!string.IsNullOrEmpty(Result.query))
						query = Result.query;

					selectCommand.CommandText = query;
					IDbDataAdapter dbDataAdapter = new NpgsqlDataAdapter();
					dbDataAdapter.SelectCommand = selectCommand;

					DataSet dataSet = new DataSet();

					dbDataAdapter.Fill(dataSet);
					return dataTable = dataSet.Tables[0];
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetMaterialItems", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}

		}

		/// <summary>
		/// material request by Project manager
		/// </summary>
		/// <param name="reqdata"></param>
		/// <returns></returns>
		public int IssueRequest(List<IssueRequestModel> reqdata)
		{
			int requestid = 0;
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				IssueRequestModel obj = new IssueRequestModel();
				pgsql.Open();
				string query = WMSResource.getnextrequestid;
				obj = pgsql.QuerySingle<IssueRequestModel>(
				   query, null, commandType: CommandType.Text);
				requestid = obj.requestid + 1;
			}
			try
			{

				var result = 0;
				foreach (var item in reqdata)
				{

					item.requesteddate = System.DateTime.Now;
					string insertquery = WMSResource.materialquest;
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						result = DB.Execute(insertquery, new
						{
							// item.paitemid,
							item.quantity,
							item.requesteddate,
							item.approveremailid,
							item.approverid,
							item.pono,
							item.materialid,
							item.requesterid,
							item.requestedquantity,
							requestid,
						});
					}



				}
				return (Convert.ToInt32(result));
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "IssueRequest", Ex.StackTrace.ToString());
				return 0;
			}
		}
		/// <summary>
		/// based on grnnumber will get lst of items
		/// </summary>
		/// <param name="grnnumber"></param>
		/// <returns></returns>
		public async Task<IEnumerable<inwardModel>> getitemdeatils(string grnnumber)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					await pgsql.OpenAsync();
					string queryforitemdetails = WMSResource.queryforitemdetails.Replace("#grnnumber", grnnumber);
					return await pgsql.QueryAsync<inwardModel>(
					   queryforitemdetails, null, commandType: CommandType.Text);
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getitemdeatils", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}

		}

		/// <summary>
		/// based on grnnumber will get lst of items for notification
		/// </summary>
		/// <param name="grnnumber"></param>
		/// <returns></returns>
		public async Task<IEnumerable<inwardModel>> getitemdeatilsnotif(string grnnumber)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					await pgsql.OpenAsync();
					string queryforitemdetails = WMSResource.getitemsfornotifypage.Replace("#grnnumber", grnnumber);
					return await pgsql.QueryAsync<inwardModel>(
					   queryforitemdetails, null, commandType: CommandType.Text);
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getitemdeatils", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}

		}
		/// <summary>
		/// requesting for material
		/// </summary>
		/// <param name="pono"></param>
		/// <param name="approverid"></param>
		/// <returns></returns>
		public async Task<IEnumerable<IssueRequestModel>> MaterialRequest(string pono, string approverid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				string materialrequestquery = WMSResource.materialrequestquery.Replace("#reqid", approverid);
				//if (pono != null)
				//{
				//	materialrequestquery = materialrequestquery + " where openpo.pono = '" + pono + "'";
				//}
				if (approverid != null)
				{
					materialrequestquery = materialrequestquery + " where  req.requesterid = '" + approverid + "' ";
				}
				materialrequestquery = materialrequestquery + " group by req.requestid order by req.requestid desc limit 50";
				try
				{
					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<IssueRequestModel>(
					   materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "MaterialIssue", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		/// <summary>
		/// acknowledge fro received item from Project manager
		/// </summary>
		/// <param name="dataobj"></param>
		/// <returns></returns>
		public int acknowledgeMaterialReceived(List<IssueRequestModel> dataobj)
		{

			try
			{
				var result = 0;
				//data.createddate = System.DateTime.Now;
				foreach (var item in dataobj)
				{
					string ackstatus = string.Empty;
					if (item.status == true)
					{
						ackstatus = "received";
					}
					else if (item.status == false)
					{
						ackstatus = "not received";
					}
					DateTime approveddate = System.DateTime.Now;

					int requestforissueid = item.requestforissueid;
					int requestid = item.requestid;
					string ackremarks = item.ackremarks;
					int issuedquantity = item.issuedquantity;
					string updateackstatus = WMSResource.updateackstatus;

					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{

						result = DB.Execute(updateackstatus, new
						{
							ackstatus,
							ackremarks,
							requestid,

						});
					}
					if (result != 0)
					{
						EmailModel emailmodel = new EmailModel();
						emailmodel.pono = item.pono;
						emailmodel.ToEmailId = "developer1@in.yokogawa.com";
						emailmodel.FrmEmailId = "sushma.patil@in.yokogawa.com";
						EmailUtilities emailobj = new EmailUtilities();
						//emailobj.sendEmail(emailmodel, 7);
					}
				}
				return (Convert.ToInt32(result));
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "acknowledgeMaterialReceived", Ex.StackTrace.ToString());
				return 0;
			}
			//try
			//{
			//    //data.createddate = System.DateTime.Now;
			//    string insertquery = "update  wms.wms_inward set ackstatus='item received',ackremarks=@remarks where inwardid=@inwardid and materialid=@material";
			//    using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
			//    {
			//        var result = DB.ExecuteScalar(insertquery, new
			//        {
			//            remarks,
			//            inwardid,
			//            material,
			//        });

			//        return (Convert.ToInt32(result));


			//    }
			//}
			//catch (Exception Ex)
			//{
			//    log.ErrorMessage("PODataProvider", "acknowledgeMaterialReceived", Ex.StackTrace.ToString());
			//    return 0;
			//}

		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="requesterid"></param>
		/// <returns></returns>

		public async Task<IEnumerable<IssueRequestModel>> GetMaterialissueList(string requesterid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.GetListForMaterialRequestByrequesterid.Replace("#requesterid", requesterid);

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<IssueRequestModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetMaterialList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		/// <summary>
		/// get list of material details based on approver id
		/// </summary>
		/// <param name="approverid"></param>
		/// <returns></returns>
		public async Task<IEnumerable<IssueRequestModel>> GetMaterialissueListforapprover(string approverid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.GetListForMaterialRequestByapproverid.Replace("#approverid", approverid);

					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);
					data = data.OrderByDescending(o => o.createddate);
					return data;


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetRequestList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		/// <summary>
		/// get list of materail details based on particlular requestid
		/// </summary>
		/// <param name="requestid"></param>
		/// <returns></returns>
		public async Task<IEnumerable<IssueRequestModel>> GetmaterialdetailsByrequestid(string requestid, string pono)
		{

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = "";
					if (string.IsNullOrEmpty(pono) || pono == "NULL" || pono == null || pono.Trim() == "")
						query = WMSResource.GetdetailsByrequestidWithoutPO.Replace("#requestid", requestid);
					else
						query = WMSResource.GetdetailsByrequestidWithPO.Replace("#requestid", requestid).Replace("#pono", pono);


					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<IssueRequestModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetRequestList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		/// <summary>
		/// get list of pono data
		/// </summary>
		/// <param name="pono"></param>
		/// <returns></returns>
		public async Task<IEnumerable<IssueRequestModel>> GetPonodetails(string pono)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.Getponodetailsformaterialissue.Replace("#pono", pono);

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<IssueRequestModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetPonodetails", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		/// <summary>
		/// inserting or updating requested qty by PM
		/// </summary>
		/// <param name="dataobj"></param>
		/// <returns></returns>
		public int updaterequestedqty(List<IssueRequestModel> dataobj)
		{

			int requestid = 1;
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				IssueRequestModel obj = new IssueRequestModel();
				pgsql.Open();
				string query = WMSResource.getnextrequestid;
				obj = pgsql.QueryFirstOrDefault<IssueRequestModel>(
				   query, null, commandType: CommandType.Text);
				if (obj != null)
					requestid = obj.requestid + 1;
			}
			try
			{

				var result = 0;
				foreach (var item in dataobj)
				{
					if (item.quantity > 0)
					{
						int qty = 0;
						item.requesteddate = System.DateTime.Now;
						string insertquery = WMSResource.materialquest;
						string materialid = item.material;
						item.requestedquantity = item.quantity;
						using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						{
							result = DB.Execute(insertquery, new
							{
								//item.paitemid,
								item.quantity,
								item.requesteddate,
								item.approveremailid,
								item.approverid,
								item.pono,
								materialid,
								item.requesterid,
								requestid,
								item.requestedquantity
							});
						}
						if (result != 0)
						{
							EmailModel emailmodel = new EmailModel();
							emailmodel.pono = item.pono;
							emailmodel.jobcode = item.projectname;
							emailmodel.ToEmailId = "developer1@in.yokogawa.com";
							emailmodel.FrmEmailId = "sushma.patil@in.yokogawa.com";
							EmailUtilities emailobj = new EmailUtilities();
							//emailobj.sendEmail(emailmodel, 5);
						}
						//if (result != 0)
						//{
						//	int availableqty = item.availableqty - item.requestedquantity;
						//	string updatequery = WMSResource.updatestock.Replace("#availableqty", Convert.ToString(availableqty)).Replace("#itemid", Convert.ToString(item.itemid));
						//	using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						//	{
						//		result = DB.Execute(updatequery, new
						//		{
						//		});
						//	}
						//}

					}

				}
				return (Convert.ToInt32(requestid));
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "IssueRequest", Ex.StackTrace.ToString());
				return 0;
			}

			//try
			//{
			//    var result = 0;
			//    //data.createddate = System.DateTime.Now;
			//    foreach (var item in dataobj)
			//    {
			//        int requestedquantity = item.requestedquantity;
			//        int inwardid = item.inwardid;
			//        string materialid = item.Material;
			//        string insertquery = "update  wms.wms_inward set requestedquantity=@requestedquantity where inwardid="+inwardid+ " and materialid='"+materialid+"'";

			//        using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
			//        {

			//            result = DB.Execute(insertquery, new
			//            {
			//                requestedquantity
			//            });
			//        }
			//    }
			//    return (Convert.ToInt32(result));
			//}
			//catch (Exception Ex)
			//{
			//    log.ErrorMessage("PODataProvider", "updaterequestedqty", Ex.StackTrace.ToString());
			//    return 0;
			//}
		}
		/// <summary>
		/// issued matreial list 
		/// </summary>
		/// <param name="dataobj"></param>
		/// <returns></returns>
		public int ApproveMaterialissue(List<IssueRequestModel> dataobj)
		{
			try
			{
				var result = 0;
				//data.createddate = System.DateTime.Now;
				foreach (var item in dataobj)
				{
					string approvedstatus = string.Empty;
					if (item.issuedqty != 0)
					{
						approvedstatus = "Approved";
					}
					//else
					//{
					//	approvedstatus = "rejected";
					//}
					DateTime approvedon = System.DateTime.Now;
					int itemid = 0;
					using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
					{
						IssueRequestModel obj = new IssueRequestModel();
						IssueRequestModel objs = new IssueRequestModel();
						pgsql.Open();
						string getitemidqry = WMSResource.getitemid.Replace("#materialid", item.materialid).Replace("#pono", item.pono);
						// string getmaterialidqry = WMSResource.getrequestforissueid.Replace("#materialid", item.materialid).Replace("#pono", item.pono);

						obj = pgsql.QueryFirstOrDefault<IssueRequestModel>(
						   getitemidqry, null, commandType: CommandType.Text);
						itemid = obj.itemid;
						//objs = pgsql.QuerySingle<IssueRequestModel>(
						//getmaterialidqry, null, commandType: CommandType.Text);

						//requestforissueid = objs.requestforissueid;
					}



					int requestforissueid = item.requestforissueid;
					string materialid = item.materialid;
					int issuedqty = item.issuedqty;
					DateTime itemissueddate = System.DateTime.Now;

					string updateapproverstatus = WMSResource.updateapproverstatus;

					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						if (item.issuedqty > 0)
						{
							result = DB.Execute(updateapproverstatus, new
							{
								approvedstatus,
								requestforissueid,
								approvedon,
								issuedqty,
								materialid,
								item.pono,
								itemid,
								item.itemreturnable,
								item.approvedby,
								itemissueddate,
								item.itemreceiverid,

							});
							int availableqty = item.availableqty - item.issuedqty;

							string insertqueryforstatusforqty = WMSResource.updateqtyafterissue.Replace("#itemid", Convert.ToString(item.itemid)).Replace("#issuedqty", Convert.ToString(item.issuedqty));

							var data1 = DB.ExecuteScalar(insertqueryforstatusforqty, new
							{

							});
						}


					}




				}

				return (Convert.ToInt32(result));
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "updaterequestedqty", Ex.StackTrace.ToString());
				return 0;
			}
		}
		/// <summary>
		/// get list of gatepass data
		/// </summary>
		/// <returns></returns>
		public async Task<IEnumerable<gatepassModel>> GetgatepassList()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getgatepasslist;

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<gatepassModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetgatepassList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		/// <summary>
		/// non returnable gatepass for outward entry
		/// Ramesh 23/07/2020
		/// </summary>
		/// <returns></returns>

		public async Task<IEnumerable<gatepassModel>> NonreturnGetgatepassList(string type)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getnonreturnablegatepassdata;

					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<gatepassModel>(
					   query, null, commandType: CommandType.Text);
					if (type == "1")
					{

						var data1 = data.Where(o => o.gatepasstype == "Returnable").ToList();
						return data1;

					}
					else
					{
						var data1 = data.Where(o => o.gatepasstype == "Non Returnable").ToList();
						return data1;
					}



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "NonreturnGetgatepassList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/// <summary>
		/// insert or update gatepass info
		/// </summary>
		/// <param name="dataobj"></param>
		/// <returns></returns>
		public int SaveOrUpdateGatepassDetails(gatepassModel dataobj)
		{
			try
			{
				//foreach(var item in dataobj._list)
				//{
				string remarks = dataobj.statusremarks;
				if (dataobj.gatepassid == 0)
				{
					dataobj.requestedon = System.DateTime.Now;
					string insertquery = WMSResource.insertgatepassdata;
					string fmapprovedstatus = "Pending";
					string approverstatus = "Pending";

					string insertgatepasshistory = WMSResource.insertgatepassapprovalhistory;
					dataobj.deleteflag = false;
					dataobj.fmapproverid = "400104";
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						var gatepassid = DB.ExecuteScalar(insertquery, new
						{

							dataobj.gatepasstype,
							dataobj.status,
							dataobj.requestedon,
							dataobj.referenceno,
							dataobj.vehicleno,
							dataobj.requestedby,
							dataobj.deleteflag,
							dataobj.vendorname,
							dataobj.reasonforgatepass,
							dataobj.approverid,
							dataobj.fmapproverid,
							fmapprovedstatus,
							approverstatus,
							remarks

						});
						if (dataobj.gatepasstype == "Returnable")
						{
							string approvername = dataobj.managername;
							int label = 1;
							//string approverstatus = "Pending";
							var gatepasshistory = DB.ExecuteScalar(insertgatepasshistory, new
							{

								dataobj.approverid,
								approvername,
								gatepassid,
								label,
								approverstatus
							});
							EmailModel emailmodel = new EmailModel();
							emailmodel.pono = dataobj.pono;
							emailmodel.ToEmailId = "developer1@in.yokogawa.com";
							emailmodel.FrmEmailId = "sushma.patil@in.yokogawa.com";
							EmailUtilities emailobj = new EmailUtilities();
							//emailobj.sendEmail(emailmodel, 8);
						}
						else if (dataobj.gatepasstype == "Non Returnable")
						{
							//string updategatepasshistoryfornonreturn = WMSResource.updategatepasshistoryfornonreturn;
							{
								string approvername = dataobj.managername;
								int label = 1;
								//string approverstatus = "Pending";
								var gatepasshistory = DB.ExecuteScalar(insertgatepasshistory, new
								{

									dataobj.approverid,
									approvername,
									gatepassid,
									label,
									approverstatus,
								});
								string approverid = "400104";
								approvername = "Lakshmi Prasanna";
								label = 2;
								var gatepassdata = DB.ExecuteScalar(insertgatepasshistory, new
								{

									approverid,
									gatepassid,
									label,
									approverstatus,
									approvername
								});
							}
							EmailModel emailmodel = new EmailModel();
							emailmodel.pono = dataobj.pono;
							emailmodel.ToEmailId = "developer1@in.yokogawa.com";
							emailmodel.FrmEmailId = "sushma.patil@in.yokogawa.com";
							EmailUtilities emailobj = new EmailUtilities();
							//emailobj.sendEmail(emailmodel, 9);
						}


						if (dataobj.gatepassid == 0)
							dataobj.gatepassid = Convert.ToInt32(gatepassid);
					}
				}
				else
				{
					dataobj.requestedon = System.DateTime.Now;
					string insertquery = WMSResource.updategatepass.Replace("#gatepassid", Convert.ToString(dataobj.gatepassid));

					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						var result = DB.ExecuteScalar(insertquery, new
						{

							dataobj.gatepasstype,
							dataobj.status,
							dataobj.requestedon,
							dataobj.referenceno,
							dataobj.vehicleno,
							dataobj.requestedby,
							dataobj.vendorname,
							dataobj.reasonforgatepass,
							remarks

						});
					}
				}
				foreach (var item in dataobj.materialList)
				{
					int itemid = 0;

					using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
					{

						string materialrequestquery = "select itemid from wms.wms_materialissue where gatepassmaterialid=" + item.gatepassmaterialid;

						pgsql.OpenAsync();
						gatepassModel gatemodel = new gatepassModel();
						gatemodel = pgsql.QueryFirstOrDefault<gatepassModel>(
								  materialrequestquery, null, commandType: CommandType.Text);
						if (gatemodel != null)
							itemid = gatemodel.itemid;
					}


					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						if (item.gatepassmaterialid == 0)
						{
							string insertquerymaterial = WMSResource.insertgatepassmaterial;

							var results = DB.ExecuteScalar(insertquerymaterial, new
							{

								dataobj.gatepassid,
								item.materialid,
								item.quantity,
								dataobj.deleteflag,
								item.remarks,
								item.materialcost,
								item.expecteddate,
								//item.returneddate,
								item.issuedqty
							});

						}
						else
						{
							string updatestockquery = "update wms.wms_stock set availableqty=availableqty+" + item.quantity + " where itemid=" + itemid;

							var result1 = DB.ExecuteScalar(updatestockquery, new
							{
							});

							string updatequery = WMSResource.updategatepassmaterial.Replace("#gatepassmaterialid", Convert.ToString(item.gatepassmaterialid));

							var result = DB.ExecuteScalar(updatequery, new
							{

								dataobj.gatepassid,
								item.materialid,
								item.quantity,
								item.remarks,
								item.materialcost,
								item.expecteddate,
								item.returneddate,

							});


						}
					}
				}
				return (1);
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "SaveOrUpdateGatepassDetails", Ex.StackTrace.ToString());
				return 0;
			}
		}
		/// <summary>
		/// check material in stock
		/// </summary>
		/// <param name="material"></param>
		/// <param name="qty"></param>
		/// <returns></returns>
		public string checkmaterialandqty(string material = null, int qty = 0)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				materialistModel obj = new materialistModel();
				string returnvalue = string.Empty;
				try
				{
					pgsql.Open();
					if (material != null && qty == 0)
					{
						string query = WMSResource.checkmaterial.Replace("#materialid", material);
						obj = pgsql.QueryFirstOrDefault<materialistModel>(
						   query, null, commandType: CommandType.Text);
						if (obj == null)
						{
							returnvalue = "material does not exists";
						}
						else
						{
							returnvalue = "true";
						}
					}
					else if (qty != 0 && material == null)
					{
						string query = WMSResource.checkqty.Replace("#availableqty", Convert.ToString(qty));
						obj = pgsql.QueryFirstOrDefault<materialistModel>(
					query, null, commandType: CommandType.Text);
						if (obj == null)
						{
							returnvalue = "qty not available";
						}
						else
						{
							returnvalue = "true";
						}
					}
					else if (material != null && qty != 0)
					{
						//string query = WMSResource.checkmaterialandqty.Replace("#availableqty", Convert.ToString(qty)).Replace("#materialid", material);
						string query = WMSResource.checkmaterialandqty.Replace("#materialid", material);
						obj = pgsql.QueryFirstOrDefault<materialistModel>(
					query, null, commandType: CommandType.Text);
						if (obj != null)
						{
							if (obj.availableqty >= qty)
							{
								returnvalue = "true";
							}
							else
							{
								if (obj.availableqty < qty)
								{
									returnvalue = "Material and quantity does not exists for " + material + " available qty is " + obj.availableqty;
								}
							}

						}
						else
						{
							returnvalue = "false";
						}
					}
					return returnvalue;


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "checkmaterialandqty", Ex.StackTrace.ToString());
					return "false";
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		/// <summary>
		/// delete gatepass
		/// </summary>
		/// <param name="gatepassmaterialid"></param>
		/// <returns></returns>
		public int deletegatepassmaterial(int gatepassmaterialid)
		{
			int returndata = 0;
			try
			{
				string insertquery = WMSResource.deletegatepassmaterial.Replace("#gatepassmaterialid", Convert.ToString(gatepassmaterialid));
				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					var data = DB.Execute(insertquery, new

					{


					});
					returndata = Convert.ToInt32(data);
				}
				return returndata;

			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "deletegatepassmaterial", Ex.StackTrace.ToString());
				return 0;
			}
		}
		/// <summary>
		/// update gatepass approver info
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		public int updategatepassapproverstatus(List<gatepassModel> model)
		{
			int returndata = 0;
			try
			{
				foreach (var item in model)
				{
					gatepassModel gatemodel = new gatepassModel();
					using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
					{


						string query = "select * from wms.wms_stock where materialid='" + item.materialid + "'and availableqty>0 order by createddate asc";

						pgsql.OpenAsync();
						gatemodel = pgsql.QueryFirstOrDefault<gatepassModel>(
						  query, null, commandType: CommandType.Text);
					}
					item.itemid = gatemodel.itemid;
					string updateapproverstatus = WMSResource.updategatepassmaterialissue;
					string approvedstatus = item.approverstatus;
					item.itemissueddate = System.DateTime.Now;
					//item.issuedqty = item.quantity;
					item.approvedon = System.DateTime.Now;
					Boolean itemreturnable = false;
					if (item.gatepasstype == "Returnable")
					{
						itemreturnable = true;
					}

					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{

						var result = DB.Execute(updateapproverstatus, new
						{
							approvedstatus,
							item.gatepassmaterialid,
							item.approvedon,
							item.issuedqty,
							item.materialid,
							item.pono,
							item.itemid,
							itemreturnable,
							item.approvedby,
							item.itemissueddate,
							item.itemreceiverid,

						});
					}
					string updateissueqty = "update  wms.wms_gatepassmaterial set issuedqty=" + item.issuedqty + " where gatepassmaterialid=" + item.gatepassmaterialid;
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						var result = DB.Execute(updateissueqty, new
						{

						});
					}
					//int qty = gatemodel.availableqty - item.issuedqty;
					//string updatestockavailable = WMSResource.updatestockavailable.Replace("#availableqty", Convert.ToString(qty)).Replace("#itemid", Convert.ToString(item.itemid));
					string updatestockavailable = WMSResource.updateqtyafterissue.Replace("#itemid", Convert.ToString(item.itemid)).Replace("#issuedqty", Convert.ToString(item.issuedqty));
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						var result = DB.Execute(updatestockavailable, new
						{

						});
					}


				}
				model[0].approvedon = System.DateTime.Now;
				model[0].status = "Issued";
				string insertquery = WMSResource.updategatepassapproverstatus.Replace("#gatepassid", Convert.ToString(model[0].gatepassid));
				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					var data = DB.Execute(insertquery, new

					{
						model[0].status,
						model[0].approverremarks,
						model[0].approverstatus,
						model[0].approvedon,

					});
					returndata = Convert.ToInt32(data);
				}

				return returndata;

			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "updategatepassapproverstatus", Ex.StackTrace.ToString());
				return 0;
			}
		}
		/// <summary>
		/// get list of material based on gatepassid
		/// </summary>
		/// <param name="gatepassid"></param>
		/// <returns></returns>
		public async Task<IEnumerable<gatepassModel>> GetmaterialList(int gatepassid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getgatepassmaterialdetailList.Replace("#gatepassid", Convert.ToString(gatepassid));

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<gatepassModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetmaterialList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		/*  Name of Function : <<name>>  Author :<<Pavithran>>  
			Date of Creation <<12-12-2019>>
			Purpose : <<Write briefly in one line or two lines>>
			Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<gatepassapprovalsModel>> getGatePassApprovalHistoryList(int gatepassid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getGatePassApprovalHistoryList.Replace("#gatepassid", Convert.ToString(gatepassid));

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<gatepassapprovalsModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getGatePassApprovalHistoryList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		/// <summary>
		/// updating print status
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		public int updateprintstatus(gatepassModel model)
		{
			{
				int returndata = 0;
				int data = 0;
				try
				{
					gatepassModel obj = new gatepassModel();

					using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
					{
						string query = WMSResource.getprintdetails.Replace("#gatepassid", Convert.ToString(model.gatepassid));

						pgsql.Open();
						obj = pgsql.QueryFirstOrDefault<gatepassModel>(
						   query, null, commandType: CommandType.Text);
					}
					if (obj.print == true)
					{
						string insertquery = WMSResource.printstatusupdate.Replace("#gatepassid", Convert.ToString(model.gatepassid));
						using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						{
							var data1 = DB.Execute(insertquery, new

							{
								model.printedby,
							});
							returndata = Convert.ToInt32(data);
						}
					}
					else
					{
						using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
						{
							string query = WMSResource.checkreprintalreadydone;

							query = query + " gatepassid=" + model.gatepassid + " order by reprintcount desc limit 1";

							pgsql.Open();
							obj = pgsql.QueryFirstOrDefault<gatepassModel>(
							   query, null, commandType: CommandType.Text);


						}
						string insertquery = WMSResource.insertreprintcount;
						using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						{

							if (obj == null)
							{ model.reprintcount = 1; }
							else
							{
								model.reprintcount = model.reprintcount + 1;
							}
							data = Convert.ToInt32(DB.ExecuteScalar(insertquery, new

							{

								model.gatepassid,
								model.reprintedby,
								model.reprintcount,

							}));
							returndata = Convert.ToInt32(data);
						}


						string updatequery = WMSResource.updatereprintcount.Replace("#reprinthistoryid", Convert.ToString(data));

						model.reprintcount = model.reprintcount + 1;
						using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						{
							var data1 = DB.Execute(updatequery, new
							{
								model.reprintcount,
							});
							returndata = Convert.ToInt32(data);
						}
					}
					return returndata;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "updateprintstatus", Ex.StackTrace.ToString());
					return 0;
				}
			}
		}
		//int returndata = 0;
		//try
		//{

		//    string insertquery = WMSResource.printstatusupdate.Replace("#gatepassid", Convert.ToString(model.gatepassid));
		//    using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
		//    {
		//        var data = DB.Execute(insertquery, new

		//        {  
		//            model.printedby,
		//        });
		//        returndata = Convert.ToInt32(data);
		//    }
		//    return returndata;

		//}
		//catch (Exception Ex)
		//{
		//    log.ErrorMessage("PODataProvider", "updateprintstatus", Ex.StackTrace.ToString());
		//    return 0;
		//}
		//}
		/// <summary>
		/// updating reprint status 
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		public int updatereprintstatus(reprintModel model)
		{
			reprintModel obj = new reprintModel();
			reprintModel secondobj = new reprintModel();
			int returndata = 0;
			try
			{
				int data = 0;
				model.inwmasterid = (model.inwmasterid == null) ? null : model.inwmasterid;
				model.gatepassid = (model.gatepassid == null) ? null : model.gatepassid;
				string insertquery = WMSResource.insertreprintcount;
				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					data = Convert.ToInt32(DB.ExecuteScalar(insertquery, new

					{
						model.inwmasterid,
						model.gatepassid,
						model.reprintcount,
						model.reprintedby,


					}));
					returndata = Convert.ToInt32(data);
				}
				using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				{
					string query = WMSResource.checkreprintalreadydone;
					if (model.inwmasterid != null)
					{
						query = query + " inwmasterid=" + model.inwmasterid + " order by reprintcount desc limit 1";
					}
					else if (model.gatepassid != null)
					{
						query = query + " gatepassid=" + model.gatepassid + " order by reprintcount desc limit 1";
					}
					pgsql.Open();
					obj = pgsql.QuerySingle<reprintModel>(
					   query, null, commandType: CommandType.Text);
				}
				string updatequery = WMSResource.updatereprintcount.Replace("#reprinthistoryid", Convert.ToString(data));
				int reprintcount = obj.reprintcount + 1;
				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					var data1 = DB.Execute(updatequery, new
					{
						reprintcount
					});
					returndata = Convert.ToInt32(data);
				}
				return returndata;

			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "updategatepassapproverstatus", Ex.StackTrace.ToString());
				return 0;
			}
		}
		/// <summary>
		/// get list based on ABC category
		/// </summary>
		/// <param name="categoryid"></param>
		/// <returns></returns>
		public async Task<IEnumerable<ReportModel>> GetreportBasedCategory(int categoryid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					// string query = WMSResource.getcategorylist.Replace("#categoryid", Convert.ToString(categoryid));
					string query = WMSResource.getcategorylist;

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<ReportModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetreportBasedCategory", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}

		}
		/// <summary>
		/// get list based on materail in category
		/// </summary>
		/// <param name="materailid"></param>
		/// <returns></returns>
		public async Task<IEnumerable<ReportModel>> GetreportBasedMaterial(string materailid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					// string query = WMSResource.getcategorylist.Replace("#categoryid", Convert.ToString(categoryid));
					string query = WMSResource.getcategorylistbymaterailid.Replace("#materialid", Convert.ToString(materailid));

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<ReportModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetreportBasedMaterial", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public int updateABCcategorydata(List<ABCCategoryModel> model)
		{
			int returndata = 0;
			try
			{

				int data = 0;

				foreach (var item in model)
				{
					if (item.categoryid != 0)
					{
						string updatequery = WMSResource.updateABCrange.Replace("#categoryid", item.categoryid.ToString());
						item.updatedon = System.DateTime.Now;

						using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						{
							data = Convert.ToInt32(DB.Execute(updatequery, new

							{

								item.updatedby,
								item.updatedon,

							}));
							returndata = Convert.ToInt32(data);

						}
						//break;
					}
				}

				//insert ABC category data
				foreach (var item in model)
				{
					if (item.categoryname == "C")
						item.minpricevalue = 0;
					//item.startdate = item.startdate.AddDays(1);
					//item.enddate = item.enddate.AddDays(1);
					string insertquery = WMSResource.insertABCrange;
					//item.createdon = System.DateTime.Now;
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						data = Convert.ToInt32(DB.ExecuteScalar(insertquery, new

						{
							item.categoryname,
							item.minpricevalue,
							item.maxpricevalue,
							item.createdby,
							item.startdate,
							item.enddate
						}));
						returndata = Convert.ToInt32(data);
					}
				}

				List<ReportModel> obj = new List<ReportModel>();
				//using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				//{


				//        // string query = WMSResource.getcategorylist.Replace("#categoryid", Convert.ToString(categoryid));
				//        string query = WMSResource.getcategorylist;

				//    await pgsql.OpenAsync();
				//    return await pgsql.QueryAsync<ReportModel>(
				//       query, null, commandType: CommandType.Text);



				//}
				return returndata;
			}

			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "updateABCcategorydata", Ex.StackTrace.ToString());
				return 0;
			}
		}

		public async Task<IEnumerable<ABCCategoryModel>> GetABCCategorydata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					// string query = WMSResource.getcategorylist.Replace("#categoryid", Convert.ToString(categoryid));
					string query = WMSResource.getabccategorydata;

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<ABCCategoryModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetABCCategorydata", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public async Task<IEnumerable<ReportModel>> GetABCavailableqtyList()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.GetallavlqtyABCList;

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<ReportModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetABCavailableqtyList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		//Ramesh (08/06/2020) returns Enquiry Details
		public async Task<Enquirydata> GetEnquirydata(string materialid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = "select stock.materialid, SUM(stock.availableqty) as availableqty, Max(op.materialdescription) as materialdescription from wms.wms_stock stock left outer join wms.\"MaterialMasterYGS\" op on  stock.materialid =op.material  where stock.materialid='" + materialid + "' group by stock.materialid";

					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<Enquirydata>(
					   query, null, commandType: CommandType.Text);
					return data.FirstOrDefault();
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetEnquirydata", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}


		//Ramesh (08/06/2020) returns all  Materials to count
		public async Task<IEnumerable<CycleCountList>> GetCyclecountList(int limita, int limitb, int limitc)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					Cyclecountconfigmodel config = new Cyclecountconfigmodel();
					await pgsql.OpenAsync();

					//Ramesh (08/06/2020) returns category A/B/C configuration

					string QueryABC = "select * from wms.wms_rd_category order by categoryid desc limit 3";
					var abcdata = await pgsql.QueryAsync<ABCCategoryModel>(QueryABC, null, commandType: CommandType.Text);
					foreach (ABCCategoryModel dt in abcdata)
					{
						if (dt.categoryname == "A")
						{
							config.amin = Convert.ToInt32(dt.minpricevalue);
							config.amax = Convert.ToInt32(dt.maxpricevalue);
						}
						else if (dt.categoryname == "B")
						{
							config.bmin = Convert.ToInt32(dt.minpricevalue);
							config.bmax = Convert.ToInt32(dt.maxpricevalue);
						}
						else if (dt.categoryname == "C")
						{
							config.cmin = Convert.ToInt32(dt.minpricevalue);
							config.cmax = Convert.ToInt32(dt.maxpricevalue);
						}
						config.startdate = dt.startdate;
						config.enddate = dt.enddate;

					}

					//Ramesh (08/06/2020) returns today counted list 
					string Querychecktodaycounted = "Select * from wms.cyclecount where counted_on = current_date";
					var todaycountdata = await pgsql.QueryAsync<CycleCountList>(Querychecktodaycounted, null, commandType: CommandType.Text);

					//Ramesh (08/06/2020) returns random A/B/C List
					string QueryA = "select sum(ws.availableqty) as availableqty,'A' AS category,ws.materialid AS materialid from wms.wms_stock ws WHERE ws.materialid IS NOT null and ws.unitprice::numeric >= " + config.amin + " group by ws.materialid order by random() limit " + limita + "";
					string QueryB = "select sum(ws.availableqty) as availableqty,'B' AS category,ws.materialid AS materialid from wms.wms_stock ws WHERE ws.materialid IS NOT null and ws.unitprice::numeric >= " + config.bmin + " and ws.unitprice::numeric <= " + config.bmax + " group by ws.materialid order by random() limit " + limitb + "";
					string QueryC = "select sum(ws.availableqty) as availableqty,'C' AS category,ws.materialid AS materialid from wms.wms_stock ws WHERE ws.materialid IS NOT null and ws.unitprice::numeric <= " + config.cmax + " group by ws.materialid order by random() limit " + limitc + "";
					var adata = await pgsql.QueryAsync<CycleCountList>(QueryA, null, commandType: CommandType.Text);
					var bdata = await pgsql.QueryAsync<CycleCountList>(QueryB, null, commandType: CommandType.Text);
					var cdata = await pgsql.QueryAsync<CycleCountList>(QueryC, null, commandType: CommandType.Text);
					var finaltempdata = adata.Concat(bdata);
					var finaldata = finaltempdata.Concat(cdata);
					foreach (CycleCountList cc in finaldata)
					{
						if (todaycountdata != null && todaycountdata.Count() > 0)
						{
							cc.todayscount = todaycountdata.Count();
						}
						else
						{
							cc.todayscount = 0;

						}
						string Querycheckcounted = "Select * from wms.cyclecount where materialid = '" + cc.materialid + "' and counted_on = current_date";
						var countdata = await pgsql.QueryAsync<CycleCountList>(Querycheckcounted, null, commandType: CommandType.Text);
						if (countdata != null && countdata.Count() > 0)
						{
							var dt = countdata.FirstOrDefault();
							cc.status = dt.status;
							cc.physicalqty = dt.physicalqty;
							cc.difference = dt.difference;
							cc.iscounted = true;

						}
					}

					return finaldata;

					//string query = WMSResource.getCyclecountList;
					//return await pgsql.QueryAsync<CycleCountList>(
					//query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetCyclecountList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		//Ramesh (08/06/2020) returns All counted Material list
		public async Task<IEnumerable<CycleCountList>> GetCyclecountPendingList()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					Cyclecountconfigmodel config = new Cyclecountconfigmodel();
					await pgsql.OpenAsync();
					string QueryA = "Select * from wms.cyclecount";
					var adata = await pgsql.QueryAsync<CycleCountList>(QueryA, null, commandType: CommandType.Text);
					return adata;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetCyclecountPendingList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		//Ramesh (08/06/2020) update or insert cycle count
		public int UpdateinsertCycleCount(List<CycleCountList> dataobj)
		{
			using (var DB = new NpgsqlConnection(config.PostgresConnectionString))
			{
				DB.OpenAsync();
				try
				{
					foreach (CycleCountList obj in dataobj)
					{
						//Ramesh (08/06/2020) approver action updation 
						if (obj.isapprovalprocess)
						{
							string status = obj.isapproved ? "Approved" : "Rejected";

							string insertquery = "update wms.cyclecount set status='" + status + "', remarks='" + obj.remarks + "',verified_on = current_date , verified_by = 'Ramesh' where id = '" + obj.id + "' ";
							var result = DB.ExecuteScalar(insertquery);



						}
						else
						{
							//Ramesh (08/06/2020) user count action updation/insertion 
							string selquery = "select * from wms.cyclecount where materialid = '" + obj.materialid + "' and counted_on = current_date ";
							var seldata = DB.QueryAsync<CycleCountList>(selquery, null, commandType: CommandType.Text);
							CycleCountList dt = new CycleCountList();
							if (seldata.Result.Count() > 0)
							{
								//Ramesh (08/06/2020) user count action updation 
								obj.difference = Math.Abs(obj.physicalqty - obj.availableqty);
								string insertquery = "update wms.cyclecount set category='" + obj.category + "', materialid= '" + obj.materialid + "', availableqty= " + obj.availableqty + ", physicalqty=" + obj.physicalqty + ", difference=" + obj.difference + ", status='Pending', counted_on = current_date , counted_by = 'Ramesh', verified_on = null , verified_by = null where materialid = '" + obj.materialid + "' ";
								var result = DB.ExecuteScalar(insertquery);

							}
							else
							{
								//Ramesh (08/06/2020) user count action insertion 
								obj.difference = Math.Abs(obj.physicalqty - obj.availableqty);
								string insertquery = "insert into wms.cyclecount(category, materialid, availableqty, physicalqty, difference, status, counted_on, counted_by, verified_on, verified_by) values('" + obj.category + "', '" + obj.materialid + "', " + obj.availableqty + ", " + obj.physicalqty + ", " + obj.difference + ", 'Pending', current_date , 'Ramesh', null, null)";
								var result = DB.ExecuteScalar(insertquery);
							}

						}


					}
					return 1;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "UpdateinsertCycleCount", Ex.StackTrace.ToString());
					return 0;
				}
				finally
				{
					DB.Close();
				}
			}
		}

		//Ramesh (08/06/2020) update cycle count configuration 
		public int UpdateCycleCountconfig(Cyclecountconfig dataobj)
		{
			try
			{
				//foreach(var item in dataobj._list)
				//{




				string insertquery = "update wms.cyclecountconfig set apercentage = " + dataobj.apercentage + ",bpercentage = " + dataobj.bpercentage + ",cpercentage = " + dataobj.cpercentage + ",cyclecount = " + dataobj.cyclecount + ",frequency = '" + dataobj.frequency + "',notificationtype='" + dataobj.notificationtype + "',notificationon='" + dataobj.notificationon + "' where id = 1";

				//string insertquery = WMSResource.updatecyclecountconfig.Replace("#cid", "1");

				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					var result = DB.ExecuteScalar(insertquery);
					//var result = DB.ExecuteScalar(insertquery, new
					//{

					//    dataobj.apercentage,
					//    dataobj.bpercentage,
					//    dataobj.cpercentage,
					//    dataobj.cyclecount,
					//    dataobj.frequency
					//});

				}
				return 1;

			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "UpdateCycleCountconfig", Ex.StackTrace.ToString());
				return 0;
			}
		}

		//Ramesh (08/06/2020) update cycle count configuration
		public async Task<Cyclecountconfig> GetCyclecountConfig()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					Cyclecountconfig config = new Cyclecountconfig();
					await pgsql.OpenAsync();
					int count = 0;
					//Ramesh (08/06/2020) get count of all materials on which cycle configuration and randon percentage will be applied
					string QueryALL = "select sum(availableqty) as availableqty,ws.materialid AS materialid from wms.wms_stock ws WHERE ws.materialid IS NOT null and ws.unitprice IS NOT null  group by materialid";
					var alldata = await pgsql.QueryAsync<CycleCountList>(QueryALL, null, commandType: CommandType.Text);

					//Ramesh (08/06/2020) get cycle configuration

					string QueryABC = "select * from wms.cyclecountconfig where id = 1";
					var abcdata = await pgsql.QueryAsync<Cyclecountconfig>(QueryABC, null, commandType: CommandType.Text);
					if (abcdata != null)
					{
						config = abcdata.FirstOrDefault();
					}

					//Ramesh (08/06/2020) get ABC configuration start and end date of cycle
					string QueryABC1 = "select * from wms.wms_rd_category order by categoryid desc limit 3";
					var abcdata1 = await pgsql.QueryAsync<ABCCategoryModel>(QueryABC1, null, commandType: CommandType.Text);
					ABCCategoryModel abcconfig = new ABCCategoryModel();
					if (abcdata1 != null && config != null)
					{
						abcconfig = abcdata1.FirstOrDefault();
						config.startdate = abcconfig.startdate;
						config.enddate = abcconfig.enddate;
					}
					if (alldata != null && config != null)
					{
						count = alldata.Count();
						config.countall = count;

					}


					return config;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetCyclecountConfig", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public async Task<IEnumerable<ReportModel>> GetABCListBycategory(string category)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.GetABCdetailsBycategory.Replace("abcname", category);

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<ReportModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetABCListBycategory", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public async Task<IEnumerable<FIFOModel>> GetFIFOList(string material)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getFIFOList;
					if (material == "null")
					{
						query = query + " order by createddate asc";
					}
					else
					{
						query = query + " and sk.materialid like'%" + material + "' order by createddate asc";
					}


					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<FIFOModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetFIFOList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public ReportModel checkloldestmaterial(string materialid, string createddate)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.checkoldestmaterial.Replace("#materialid", materialid).Replace("#createddate", Convert.ToString(createddate));


					pgsql.Open();
					return pgsql.QuerySingle<ReportModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "checkloldestmaterial", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public int FIFOitemsupdate(List<FIFOModel> model)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					foreach (var item in model)
					{

						string insertforinvoicequery = WMSResource.insertFIFOdata;
						using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						{
							var results = DB.ExecuteScalar(insertforinvoicequery, new
							{
								item.itemid,
								item.materialid,
								item.pono

							});
							int availableqty = item.availableqty - item.issuedqty;

							string insertqueryforstatusforqty = WMSResource.updateqtyafterissue.Replace("#itemid", Convert.ToString(item.itemid)).Replace("#issuedqty", Convert.ToString(item.issuedqty));

							var data1 = DB.ExecuteScalar(insertqueryforstatusforqty, new
							{

							});

						}
					}


					//}
					return (1);
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "FIFOitemsupdate", Ex.StackTrace.ToString());
					return 0;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}

		/// <summary>
		/// get list of todays expected shipments
		/// Ramesh
		/// </summary>
		/// <param name="deliverydate"></param>
		/// <returns></returns>
		public async Task<IEnumerable<OpenPoModel>> getASNList(string deliverydate)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getASNList;
					query = query + " where asno.deliverydate >= '" + deliverydate + " 00:00:00' and asno.deliverydate <= '" + deliverydate + " 23:59:59'";
					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<OpenPoModel>(
					   query, null, commandType: CommandType.Text);
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getASNList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public async Task<IEnumerable<OpenPoModel>> getASNListdata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					DateTime todayDate = DateTime.Now;
					string currentdatestr = todayDate.ToString("yyyy-MM-dd");
					DateTime weekbeforeDate = DateTime.Now.AddDays(-7);
					string weekbeforeDatestr = weekbeforeDate.ToString("yyyy-MM-dd");
					string query = WMSResource.getASNList;
					query = query + " where asno.deliverydate >= '" + weekbeforeDatestr + " 00:00:00' and asno.deliverydate <= '" + currentdatestr + " 23:59:59' order by asno.deliverydate";
					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<OpenPoModel>(
					   query, null, commandType: CommandType.Text);
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getASNList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public async Task<UserDashboardDetail> getUserDashboarddata(string empno)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					UserDashboardDetail detail = new UserDashboardDetail();
					string deliverydate = DateTime.Now.ToString("yyyy-MM-dd");
					string query = WMSResource.getASNList;
					query = query + " where asno.deliverydate >= '" + deliverydate + " 00:00:00' and asno.deliverydate <= '" + deliverydate + " 23:59:59'";
					await pgsql.OpenAsync();
					var expectedrcpts = await pgsql.QueryAsync<OpenPoModel>(
					   query, null, commandType: CommandType.Text);
					if (expectedrcpts != null && expectedrcpts.Count() > 0)
					{
						detail.pendingshipments = expectedrcpts.Count();

					}
					string receivedlistqry = WMSResource.getsecurityreceivedlist;
					receivedlistqry = receivedlistqry + " where sl.invoicedate <= '" + deliverydate + " 23:59:59' and sl.invoicedate >= '" + deliverydate + " 00:00:00'";
					var receivedrcpts = await pgsql.QueryAsync<SecurityInwardreceivedModel>(
					   receivedlistqry, null, commandType: CommandType.Text);
					if (receivedrcpts != null && receivedrcpts.Count() > 0)
					{
						detail.receivedshipments = receivedrcpts.Count();

					}
					string outinquery = WMSResource.getnonreturnablegatepassdata;
					var outindata = await pgsql.QueryAsync<gatepassModel>(
					   outinquery, null, commandType: CommandType.Text);
					if (outindata != null && outindata.Count() > 0)
					{
						var outwardata = outindata.Where(o => o.outwarddate == null);
						var inwarddata = outindata.Where(o => o.outwarddate != null && o.inwarddate == null);
						var countoutward = outwardata.GroupBy(item => item.gatepassid).Select(g => new { name = g.Key, count = g.Count() });
						var countinward = inwarddata.GroupBy(item => item.gatepassid).Select(g => new { name = g.Key, count = g.Count() });
						detail.pendingtoinward = countinward.Count();
						detail.pendingtooutward = countoutward.Count();

					}

					string materialrequestquery = WMSResource.getpendingreceiptslist;

					var pendingrcptsdata = await pgsql.QueryAsync<ddlmodel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					if (pendingrcptsdata != null && pendingrcptsdata.Count() > 0)
					{
						detail.pendingtoreceive = pendingrcptsdata.Count();

					}

					string materialacceptquery = WMSResource.getgrnlistdata;

					var mataccpt = await pgsql.QueryAsync<ddlmodel>(
					  materialacceptquery, null, commandType: CommandType.Text);

					if (mataccpt != null && mataccpt.Count() > 0)
					{
						detail.pendingtoaccetance = mataccpt.Count();

					}




					string materialputawayquery = WMSResource.getgrnlistdataforputaway;
					List<ddlmodel> returnlist = new List<ddlmodel>();
					var matputaway = await pgsql.QueryAsync<ddlmodel>(
					  materialputawayquery, null, commandType: CommandType.Text);
					if (matputaway != null && matputaway.Count() > 0)
					{
						foreach (ddlmodel ddl in matputaway)
						{
							var exixtedrow = returnlist.Where(o => o.text == ddl.text).FirstOrDefault();
							if (exixtedrow == null)
							{
								returnlist.Add(ddl);

							}

						}
						detail.pendingtoputaway = returnlist.Count();

					}

					string queryqc = WMSResource.getqualitycheckdropdown;// + pono+"'";//li
					var qcdata = await pgsql.QueryAsync<ddlmodel>(
					   queryqc, null, commandType: CommandType.Text);
					if (qcdata != null && qcdata.Count() > 0)
					{
						detail.pendingtoqualitycheck = qcdata.Count();

					}
					DateTime sevendaysbefore = DateTime.Now.AddDays(-7);
					string sevendaysbeforestr = sevendaysbefore.ToString("yyyy-MM-dd");
					string queryrsrv = "select itemid as value from wms.wms_materialreserve where reservedby = '" + empno + "' and reserveupto >= '" + sevendaysbeforestr + " 00:00:00' and reserveupto <= '" + deliverydate + " 23:59:59'";
					var rsvdata = await pgsql.QueryAsync<ddlmodel>(
					   queryrsrv, null, commandType: CommandType.Text);
					if (rsvdata != null && rsvdata.Count() > 0)
					{
						detail.reservedquantityforthisweek = rsvdata.Count();

					}




					return detail;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getASNList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public async Task<IEnumerable<IssueRequestModel>> GetItemlocationListBymterial(string material)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getitemlocationList.Replace("#materialid", material);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);
					data = data.OrderByDescending(o => o.createddate);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetItemlocationListBymterial", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		/// <summary>
		/// Get item location list for stock transfer
		/// Ramesh 29/07/2020
		/// </summary>
		/// <param name="material"></param>
		/// <returns></returns>
		public async Task<IEnumerable<IssueRequestModel>> GetItemlocationListforST(string material)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getitemlocationList.Replace("#materialid", material);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);
					data = data.OrderByDescending(o => o.createddate);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetItemlocationListBymterial", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public int updateissuedmaterial(List<IssueRequestModel> obj)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					foreach (var item in obj)
					{

						string insertforinvoicequery = WMSResource.insertFIFOdata;
						using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						{
							//string approvedstatus = string.Empty;
							//if (item.issuedqty != 0)
							//{
							//	approvedstatus = "approved";
							//}
							////else
							////{
							////	approvedstatus = "rejected";
							////}
							//DateTime approvedon = System.DateTime.Now;
							//int itemid = 0;
							//int reserveformaterialid = item.reserveformaterialid;
							//string materialid = item.materialid;
							//int issuedqty = item.issuedqty;
							//DateTime itemissueddate = System.DateTime.Now;
							//string updateapproverstatus = WMSResource.updateapproverstatus;



							//var result = DB.Execute(updateapproverstatus, new
							//{
							//	approvedstatus,
							//	reserveformaterialid,
							//	approvedon,
							//	issuedqty,
							//	materialid,
							//	item.pono,
							//	itemid,
							//	item.itemreturnable,
							//	item.approvedby,
							//	itemissueddate,
							//	item.itemreceiverid,

							//});

							int availableqty = item.availableqty - item.issuedquantity;

							string insertqueryforstatusforqty = WMSResource.updateqtyafterissue.Replace("#itemid", Convert.ToString(item.itemid)).Replace("#issuedqty", Convert.ToString(item.issuedqty));

							var data1 = DB.ExecuteScalar(insertqueryforstatusforqty, new
							{

							});

						}
					}


					//}
					return (1);
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "FIFOitemsupdate", Ex.StackTrace.ToString());
					return 0;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}

		//Name of Function : <<AssignRoles>>  Author :<<prasanna>>  
		//Date of Creation <<10-06-2020>>
		//Purpose : <<insert method to Asssign roles for employee >>
		//Review Date :<<>>   Reviewed By :<<>>
		public int assignRole(authUser model)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					model.createddate = System.DateTime.Now;
					string insertquery = WMSResource.insertAuthUserData;
					model.deleteflag = false;
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						var results = DB.ExecuteScalar(insertquery, new
						{
							model.employeeid,
							model.roleid,
							model.createddate,
							model.createdby,
							model.deleteflag
						});
						return (Convert.ToInt32(results));
					}
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "assignRole", Ex.StackTrace.ToString());
					return 0;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}


		//Name of Function : <<getuserAcessList>>  Author :<<prasanna>>  
		//Date of Creation <<11-06-2020>>
		//Purpose : <<function used to get Acessnames list based on employeeid,roleid >>
		//Review Date :<<>>   Reviewed By :<<>>
		public async Task<IEnumerable<userAcessNamesModel>> getuserAcessList(string employeeid, string roleid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getUserAcessNames.Replace("#employeeid", employeeid);
					query = query.Replace("#roleid", roleid);
					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<userAcessNamesModel>(
					  query, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getuserAcessList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		//Name of Function : <<getuserAcessList>>  Author :<<Ramesh>>  
		//Date of Creation <<28-07-2020>>
		//Purpose : <<function used to get role list based on employeeid>>
		//Review Date :<<>>   Reviewed By :<<>>
		public async Task<IEnumerable<userAcessNamesModel>> getuserroleList(string employeeid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getuserroles.Replace("#employeeid", employeeid);
					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<userAcessNamesModel>(
					  query, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getuserroleList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		//Name of Function : <<getdashboarddata>>  Author :<<Ramesh>>  
		//Date of Creation <<17-06-2020>>
		//Purpose : <<function to get dashboard data >>
		//Review Date :<<>>   Reviewed By :<<>>
		public async Task<DashboardModel> getdashboarddata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{


				try
				{
					DashboardModel result = new DashboardModel();
					result.graphdata = new List<DashboardGraphModel>();
					DateTime dt = DateTime.Now;
					int i = 1;
					while (i <= 7)
					{
						if (i == 1)
						{
							string date = dt.ToString("yyyy-MM-dd");
							string query1 = "Select Count(*) as todayexpextedcount from wms.openpolistview where deliverydate = current_date";
							string query2 = "select count(*) as todayreceivedcount from wms.wms_securityinward where receiveddate <= '" + date + " 23:59:59' and receiveddate >= '" + date + " 00:00:00' ";
							string query3 = "select count(*) as todaytoissuecount from wms.wms_materialrequest where requesteddate = '" + date + " 23:59:59' and requesteddate >= '" + date + " 00:00:00'";

							await pgsql.OpenAsync();
							var data1 = await pgsql.QueryAsync<DashboardModel>(query1, null, commandType: CommandType.Text);
							var data2 = await pgsql.QueryAsync<DashboardModel>(query2, null, commandType: CommandType.Text);
							var data3 = await pgsql.QueryAsync<DashboardModel>(query3, null, commandType: CommandType.Text);
							DashboardGraphModel md = new DashboardGraphModel();
							md.date = dt.ToString("dd/MM/yyyy");
							if (data1 != null)
							{
								result.todayexpextedcount = data1.FirstOrDefault().todayexpextedcount;
								md.expectedcount = data1.FirstOrDefault().todayexpextedcount;
							}
							if (data2 != null)
							{
								result.todayreceivedcount = data2.FirstOrDefault().todayreceivedcount;
								md.receivedcount = data2.FirstOrDefault().todayreceivedcount;
							}
							if (data3 != null)
							{
								result.todaytoissuecount = data3.FirstOrDefault().todaytoissuecount;
								md.toissuecount = data3.FirstOrDefault().todaytoissuecount;
							}
							result.graphdata.Add(md);
						}
						else
						{
							dt = dt.AddDays(-1);
							string date = dt.ToString("yyyy-MM-dd");
							string date1 = dt.ToString("dd/MM/yyyy");
							string query1 = "Select Count(*) as todayexpextedcount from wms.openpolistview where deliverydate = '" + date + "'";
							string query2 = "select count(*) as todayreceivedcount from wms.wms_securityinward where receiveddate <= '" + date + " 23:59:59' and receiveddate >= '" + date + " 00:00:00' ";
							string query3 = "select count(*) as todaytoissuecount from wms.wms_materialrequest where requesteddate = '" + date + " 23:59:59' and requesteddate >= '" + date + " 00:00:00'";
							var data1 = await pgsql.QueryAsync<DashboardModel>(query1, null, commandType: CommandType.Text);
							var data2 = await pgsql.QueryAsync<DashboardModel>(query2, null, commandType: CommandType.Text);
							var data3 = await pgsql.QueryAsync<DashboardModel>(query3, null, commandType: CommandType.Text);
							DashboardGraphModel md = new DashboardGraphModel();
							md.date = date1;
							if (data1 != null)
							{
								md.expectedcount = data1.FirstOrDefault().todayexpextedcount;
							}
							if (data2 != null)
							{
								md.receivedcount = data2.FirstOrDefault().todayreceivedcount;
							}
							if (data3 != null)
							{
								md.toissuecount = data3.FirstOrDefault().todaytoissuecount;
							}
							result.graphdata.Add(md);
						}

						i++;
					}



					return result;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getdashboarddata", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public async Task<IEnumerable<UserDashboardGraphModel>> getUserdashboardgraphdata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{


				try
				{

					string rcvqry = "SELECT receiveddate::date as graphdate, COUNT(*),'Receive' as type ";
					rcvqry += " from wms.wms_storeinward";
					rcvqry += " WHERE receiveddate > now() - interval '7 days'";
					rcvqry += " GROUP BY receiveddate::date";
					rcvqry += " ORDER BY receiveddate::date ASC";

					string qcqry = "SELECT qcdate::date as graphdate, COUNT(*),'Quality' as type ";
					qcqry += " from wms.wms_qualitycheck";
					qcqry += " WHERE qcdate > now() - interval '7 days'";
					qcqry += " GROUP BY qcdate::date";
					qcqry += " ORDER BY qcdate::date ASC";

					string accqry = "SELECT returnedon::date as graphdate, COUNT(*),'Accept' as type ";
					accqry += " from wms.wms_storeinward";
					accqry += " WHERE returnedon > now() - interval '7 days'";
					accqry += " GROUP BY returnedon::date";
					accqry += " ORDER BY returnedon::date ASC";

					string pwqry = "SELECT createddate::date as graphdate, COUNT(*),'Putaway' as type ";
					pwqry += " from wms.wms_stock";
					pwqry += " WHERE createddate > now() - interval '7 days'";
					pwqry += " GROUP BY createddate::date";
					pwqry += " ORDER BY createddate::date ASC";


					var data1 = await pgsql.QueryAsync<UserDashboardGraphModel>(rcvqry, null, commandType: CommandType.Text);
					var data2 = await pgsql.QueryAsync<UserDashboardGraphModel>(qcqry, null, commandType: CommandType.Text);
					var data3 = await pgsql.QueryAsync<UserDashboardGraphModel>(accqry, null, commandType: CommandType.Text);
					var data4 = await pgsql.QueryAsync<UserDashboardGraphModel>(pwqry, null, commandType: CommandType.Text);

					var data = data1.Concat(data2.Concat(data3.Concat(data4)));

					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getUserdashboardgraphdata", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public async Task<IEnumerable<UserDashboardGraphModel>> getWeeklyUserdashboardgraphdata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{


				try
				{

					string rcvqry = "SELECT date_part('year', receiveddate::date) as syear,";
					rcvqry += " date_part('week', receiveddate::date) AS sweek,COUNT(*) as count, 'Receive' as type";
					rcvqry += " FROM wms.wms_storeinward where receiveddate is not null and receiveddate > now() - interval '1 month' and extract(year from receiveddate) > 2000";
					rcvqry += " GROUP BY syear, sweek ORDER BY syear, sweek";

					string qcqry = "SELECT date_part('year', qcdate::date) as syear,";
					qcqry += " date_part('week', qcdate::date) AS sweek,COUNT(*) as count, 'Quality' as type";
					qcqry += " FROM wms.wms_qualitycheck where qcdate is not null and qcdate > now() - interval '1 month' and extract(year from qcdate) > 2000";
					qcqry += " GROUP BY syear, sweek ORDER BY syear, sweek";

					string accqry = "SELECT date_part('year', returnedon::date) as syear,";
					accqry += " date_part('week', returnedon::date) AS sweek,COUNT(*) as count, 'Accept' as type";
					accqry += " FROM wms.wms_storeinward where returnedon is not null and returnedon > now() - interval '1 month' and extract(year from returnedon) > 2000";
					accqry += " GROUP BY syear, sweek ORDER BY syear, sweek";

					string pwqry = "SELECT date_part('year', createddate::date) as syear,";
					pwqry += " date_part('week', createddate::date) AS sweek,COUNT(*) as count, 'Putaway' as type";
					pwqry += " FROM wms.wms_stock where createddate is not null and createddate > now() - interval '1 month' and extract(year from createddate) > 2000";
					pwqry += " GROUP BY syear, sweek ORDER BY syear, sweek";


					var data1 = await pgsql.QueryAsync<UserDashboardGraphModel>(rcvqry, null, commandType: CommandType.Text);
					var data2 = await pgsql.QueryAsync<UserDashboardGraphModel>(qcqry, null, commandType: CommandType.Text);
					var data3 = await pgsql.QueryAsync<UserDashboardGraphModel>(accqry, null, commandType: CommandType.Text);
					var data4 = await pgsql.QueryAsync<UserDashboardGraphModel>(pwqry, null, commandType: CommandType.Text);

					var data = data1.Concat(data2.Concat(data3.Concat(data4)));

					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getWeeklyUserdashboardgraphdata", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public async Task<IEnumerable<UserDashboardGraphModel>> getmonthlyUserdashboardgraphdata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{


				try
				{


					string rcvqry = "select to_char(receiveddate,'Mon') as smonth,";
					rcvqry += " extract(year from receiveddate) as syear,";
					rcvqry += " count(*) as count,";
					rcvqry += " 'Receive' as type";
					rcvqry += " from wms.wms_storeinward where receiveddate > now() - interval '1 year' and receiveddate is not null and extract(year from receiveddate) > 2000";
					rcvqry += " group by 1,2 order by to_char(receiveddate, 'Mon') DESC";

					string qcqry = "select to_char(qcdate,'Mon') as smonth,";
					qcqry += " extract(year from qcdate) as syear,";
					qcqry += " count(*) as count,";
					qcqry += " 'Quality' as type";
					qcqry += " from wms.wms_qualitycheck where qcdate > now() - interval '1 year' and qcdate is not null and extract(year from qcdate) > 2000";
					qcqry += " group by 1,2 order by to_char(qcdate, 'Mon') DESC";

					string accqry = "select to_char(returnedon,'Mon') as smonth,";
					accqry += " extract(year from returnedon) as syear,";
					accqry += " count(*) as count,";
					accqry += " 'Accept' as type";
					accqry += " from wms.wms_storeinward where returnedon > now() - interval '1 year' and returnedon is not null and extract(year from returnedon) > 2000";
					accqry += " group by 1,2 order by to_char(returnedon, 'Mon') DESC";

					string pwqry = "select to_char(createddate,'Mon') as smonth,";
					pwqry += " extract(year from createddate) as syear,";
					pwqry += " count(*) as count,";
					pwqry += " 'Putaway' as type";
					pwqry += " from wms.wms_stock where createddate > now() - interval '1 year' and createddate is not null and extract(year from createddate) > 2000";
					pwqry += " group by 1,2 order by to_char(createddate, 'Mon') DESC";


					var data1 = await pgsql.QueryAsync<UserDashboardGraphModel>(rcvqry, null, commandType: CommandType.Text);
					var data2 = await pgsql.QueryAsync<UserDashboardGraphModel>(qcqry, null, commandType: CommandType.Text);
					var data3 = await pgsql.QueryAsync<UserDashboardGraphModel>(accqry, null, commandType: CommandType.Text);
					var data4 = await pgsql.QueryAsync<UserDashboardGraphModel>(pwqry, null, commandType: CommandType.Text);

					var data = data1.Concat(data2.Concat(data3.Concat(data4)));

					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getmonthlyUserdashboardgraphdata", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		public async Task<IEnumerable<IssueRequestModel>> MaterialRequestdata(string pono, string approverid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string materialrequestquery = WMSResource.getmaterialdetailfprrequest;
					if (pono != null && pono != "undefined" && pono != "null")
					{
						materialrequestquery = materialrequestquery + " and sk.pono = '" + pono + "'";
					}
					//if (approverid != null)
					//{
					//	materialrequestquery = materialrequestquery + " and pro.projectmanager = '" + approverid + "' ";
					//}
					materialrequestquery = materialrequestquery + " group by sk.materialid";
					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<IssueRequestModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "MaterialRequestdata", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		public async Task<IEnumerable<IssueRequestModel>> getissuematerialdetails(int requestid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{


				try
				{
					string materialrequestquery = WMSResource.issuedqtydetails.Replace("#requestid", Convert.ToString(requestid));

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<IssueRequestModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getissuematerialdetails", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public async Task<int> insertResevematerial(List<ReserveMaterialModel> datamodel)
		{
			int reserveid = 0;
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				ReserveMaterialModel obj = new ReserveMaterialModel();
				pgsql.Open();
				string query = WMSResource.getnextreservetid;
				obj = pgsql.QueryFirstOrDefault<ReserveMaterialModel>(
				   query, null, commandType: CommandType.Text);
				if (obj == null)
					reserveid = 1;
				else
				{
					reserveid = obj.reserveid + 1;
				}

			}
			try
			{

				var result = 0;
				foreach (var item in datamodel)
				{

					if (item.quantity > 0)
					{

						item.materialid = item.material;
						item.reservedqty = item.quantity;
						item.reservedby = item.requesterid;

						string insertquery = WMSResource.insertreservematerial;
						//string itemnoquery = WMSResource.getitemiddata.Replace("#materialid", item.material);
						//if (item.pono != null)
						//{
						//	itemnoquery = itemnoquery + "and pono='" + item.pono + "'";
						//}
						using (var DB = new NpgsqlConnection(config.PostgresConnectionString))
						{

							//	//DB.Open();
							//	await DB.OpenAsync();
							//	var itemData = await DB.QueryAsync<StockModel>(
							//  itemnoquery, null, commandType: CommandType.Text);
							//	int remainingqty = item.quantity;
							//	itemData = itemData.OrderBy(o => o.createddate);

							//	foreach (StockModel data in itemData)
							//	{
							//if (item.pono == null)
							//{
							//	item.pono = data.pono;
							//}
							//if (data.availableqty >= remainingqty)
							//{
							//item.itemid = data.itemid;

							result = DB.Execute(insertquery, new
							{
								item.materialid,
								item.itemid,
								item.pono,
								item.reservedby,
								item.reservedqty,
								reserveid,
								item.reserveupto
							});
							//break;
							//}
							//else
							//{
							//remainingqty = item.quantity - data.availableqty;
							//item.itemid = data.itemid;

							//result = DB.Execute(insertquery, new
							//{
							//	item.materialid,
							//	item.itemid,
							//	item.pono,
							//	item.reservedby,
							//	item.reservedqty,
							//	reserveid,
							//	item.reserveupto
							//});

							//}

							//if (result != 0)
							//{
							//	int availableqty = item.availableqty - item.reservedqty;
							//	string updatequery = WMSResource.updatestock.Replace("#availableqty", Convert.ToString(availableqty)).Replace("#itemid", Convert.ToString(item.itemid));
							//	using (IDbConnection pgsql = new NpgsqlConnection(config.PostgresConnectionString))
							//	{
							//		result = pgsql.Execute(updatequery, new
							//		{
							//		});
							//	}
							//}

							//}



						}



					}
				}

				return (Convert.ToInt32(result));
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "insertResevematerial", Ex.StackTrace.ToString());
				return 0;
			}
		}


		public async Task<IEnumerable<ReserveMaterialModel>> GetReservedMaterialList(string reservedby)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{


				try
				{
					string materialrequestquery = WMSResource.getreservedmaterialList.Replace("#reservedby", reservedby);
					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<ReserveMaterialModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetReservedMaterialList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public async Task<IEnumerable<ReserveMaterialModel>> getissuematerialdetailsforreserved(int reservedid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{


				try
				{
					string materialrequestquery = WMSResource.Getreleasedqty.Replace("#reserveid", Convert.ToString(reservedid));

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<ReserveMaterialModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getissuematerialdetails", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public async Task<IEnumerable<ReserveMaterialModel>> GetReleasedmaterialList()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.GetreleasedmaterialList;

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<ReserveMaterialModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetReleasedmaterialList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public async Task<IEnumerable<ReserveMaterialModel>> GetmaterialdetailsByreserveid(string reserveid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getmaterialdetailsbyreserveid.Replace("#reserveid", reserveid);

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<ReserveMaterialModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetmaterialdetailsByreserveid", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		/// <summary>
		/// ApproveMaterialRelease
		/// </summary>
		/// <param name="dataobj"></param>
		/// <returns></returns>
		public int ApproveMaterialRelease(List<ReserveMaterialModel> dataobj)
		{
			try
			{
				var result = 0;
				//data.createddate = System.DateTime.Now;
				foreach (var item in dataobj)
				{
					string approvedstatus = string.Empty;
					if (item.issuedqty != 0)
					{
						approvedstatus = "approved";
					}
					//else
					//{
					//	approvedstatus = "rejected";
					//}
					DateTime approvedon = System.DateTime.Now;
					int itemid = 0;
					using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
					{
						IssueRequestModel obj = new IssueRequestModel();
						IssueRequestModel objs = new IssueRequestModel();
						pgsql.Open();
						string getitemidqry = WMSResource.getitemid.Replace("#materialid", item.materialid).Replace("#pono", item.pono);
						// string getmaterialidqry = WMSResource.getrequestforissueid.Replace("#materialid", item.materialid).Replace("#pono", item.pono);

						obj = pgsql.QueryFirstOrDefault<IssueRequestModel>(
						   getitemidqry, null, commandType: CommandType.Text);
						itemid = obj.itemid;
						//objs = pgsql.QuerySingle<IssueRequestModel>(
						//getmaterialidqry, null, commandType: CommandType.Text);

						//requestforissueid = objs.requestforissueid;
					}



					int reserveformaterialid = item.reserveformaterialid;
					string materialid = item.materialid;
					int issuedqty = item.issuedqty;
					DateTime itemissueddate = System.DateTime.Now;

					string updateapproverstatus = WMSResource.updateapproverstatusforrelease;

					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{

						result = DB.Execute(updateapproverstatus, new
						{
							approvedstatus,
							reserveformaterialid,
							approvedon,
							issuedqty,
							materialid,
							item.pono,
							itemid,
							item.itemreturnable,
							item.approvedby,
							itemissueddate,
							item.itemreceiverid,

						});
					}
				}
				return (Convert.ToInt32(result));
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "updaterequestedqty", Ex.StackTrace.ToString());
				return 0;
			}


		}

		public int acknowledgeMaterialReceivedforreserved(List<ReserveMaterialModel> dataobj)
		{
			try
			{
				var result = 0;
				//data.createddate = System.DateTime.Now;
				foreach (var item in dataobj)
				{
					string ackstatus = string.Empty;
					if (item.status == true)
					{
						ackstatus = "received";
					}
					else if (item.status == false)
					{
						ackstatus = "not received";
					}
					DateTime approveddate = System.DateTime.Now;

					int requestforissueid = item.reserveformaterialid;
					int reserveid = item.reserveid;
					string ackremarks = item.ackremarks;
					int issuedquantity = item.issuedqty;
					string updateackstatus = WMSResource.updateackstatusforreserved;

					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{

						result = DB.Execute(updateackstatus, new
						{
							ackstatus,
							ackremarks,
							reserveid,

						});
					}
				}
				return (Convert.ToInt32(result));
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "acknowledgeMaterialReceivedforreserved", Ex.StackTrace.ToString());
				return 0;
			}

		}
		/// <summary>
		/// Ramesh kumar
		/// </summary>
		/// <returns></returns>
		//get received po list based on current date
		public async Task<IEnumerable<SecurityInwardreceivedModel>> getSecurityreceivedList()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					DateTime dt = DateTime.Now;
					string date = dt.ToString("yyyy-MM-dd");
					string query = WMSResource.getsecurityreceivedlist;
					query = query + " where sl.invoicedate <= '" + date + " 23:59:59' and sl.invoicedate >= '" + date + " 00:00:00'";
					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<SecurityInwardreceivedModel>(
					   query, null, commandType: CommandType.Text);
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getSecurityreceivedList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public async Task<string> insertquantitycheck(List<inwardModel> datamodel)
		{

			inwardModel obj = new inwardModel();
			inwardModel getgrnnoforpo = new inwardModel();
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					await pgsql.OpenAsync();

					string checkedon = DateTime.Now.ToString("yyyy-MM-dd");
					foreach (var item in datamodel)
					{
						//string insertforquality = WMSResource.insertqualitycheck.Replace("#inwardid", item.inwardid.ToString());
						string insertforquality = WMSResource.savequalityquery;
						string materialid = item.Material;
						using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						{
							var results = DB.ExecuteScalar(insertforquality, new
							{
								item.inwardid,
								item.qualitypassedqty,
								item.qualityfailedqty,
								item.receivedby,
								item.remarks

							});
							string query = "UPDATE wms.wms_storeinward set  qualitychecked = True where inwardid = '" + item.inwardid + "'";
							DB.ExecuteScalar(query);
							//inwardid = Convert.ToInt32(results);
							//if (inwardid != 0)
							//{
							//    string insertqueryforqualitycheck =WMSResource.insertqueryforqualitycheck;

							//    var data = DB.ExecuteScalar(insertqueryforqualitycheck, new
							//    {
							//        inwardid,
							//        datamodel.quality,
							//        datamodel.qtype,
							//        datamodel.qcdate,
							//        datamodel.qcby,
							//        datamodel.remarks,
							//        datamodel.deleteflag,

							//    });
							if (Convert.ToInt32(results) != 0)
							{
								EmailModel emailmodel = new EmailModel();
								emailmodel.pono = item.pono;
								emailmodel.jobcode = item.projectname;
								emailmodel.ToEmailId = "developer1@in.yokogawa.com";
								emailmodel.FrmEmailId = "sushma.patil@in.yokogawa.com";
								EmailUtilities emailobj = new EmailUtilities();
								//emailobj.sendEmail(emailmodel, 3);
							}

						}
					}

					//}
					return "counted";
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "insertquantitycheck", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}



		//Ramesh
		public async Task<IEnumerable<ddlmodel>> getprojectlist()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getprojectlist;


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<ddlmodel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getprojectlist", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		public async Task<IEnumerable<ddlmodel>> getmatlist(string empno)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getmaterialfortransfer.Replace("#requestor", empno);


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<ddlmodel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getmatlist", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}







		public async Task<string> insertreturn(List<inwardModel> datamodel)
		{

			inwardModel obj = new inwardModel();
			inwardModel getgrnnoforpo = new inwardModel();
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					await pgsql.OpenAsync();

					string checkedon = DateTime.Now.ToString("yyyy-MM-dd");
					foreach (var item in datamodel)
					{
						string insertforquality = WMSResource.insertreturndata.Replace("#inwardid", item.inwardid.ToString());
						string materialid = item.Material;
						using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						{
							var results = DB.ExecuteScalar(insertforquality, new
							{
								item.confirmqty,
								item.returnqty,
								item.receivedby,
								item.returnremarks

							});
							//inwardid = Convert.ToInt32(results);
							//if (inwardid != 0)
							//{
							//    string insertqueryforqualitycheck =WMSResource.insertqueryforqualitycheck;

							//    var data = DB.ExecuteScalar(insertqueryforqualitycheck, new
							//    {
							//        inwardid,
							//        datamodel.quality,
							//        datamodel.qtype,
							//        datamodel.qcdate,
							//        datamodel.qcby,
							//        datamodel.remarks,
							//        datamodel.deleteflag,

							//    });
							string insertqueryforstatusforqty = WMSResource.insertqueryforstatusforqty;

							var data1 = DB.ExecuteScalar(insertqueryforstatusforqty, new
							{
								item.pono,
								item.returnqty

							});


						}
					}

					//}
					return "counted";
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "insertquantitycheck", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}




		public async Task<IEnumerable<dropdownModel>> Getlocationdata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = "select * from wms.wms_rd_locator where deleteflag=false order by locatorname asc";

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<dropdownModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "Getlocationdata", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		public async Task<IEnumerable<dropdownModel>> Getbindata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = "select * from wms.wms_rd_bin where deleteflag=false  order by binnumber asc";

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<dropdownModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "Getbindata", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		public async Task<IEnumerable<dropdownModel>> Getrackdata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = "select * from wms.wms_rd_rack where deleteflag=false order by racknumber asc";

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<dropdownModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "Getrackdata", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		/// <summary>
		/// Get list of materials 
		/// Ramesh Kumar 15/07/2020
		/// </summary>
		/// <returns></returns>
		public async Task<IEnumerable<Materials>> GetMaterialcombo()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = "select material,materialdescription,qualitycheck from wms.\"MaterialMasterYGS\"";

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<Materials>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetMaterialcombo", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public async Task<IEnumerable<employeeModel>> getapproverList(string empid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string approverlist = WMSResource.getimmediatemnger.Replace("#employeeno", empid);

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<employeeModel>(
					  approverlist, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getapproverList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public async Task<IEnumerable<gatepassModel>> getgatepassByapproverList(string empid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string approverlist = WMSResource.getgatepassapproverdata.Replace("#approverid", empid);

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<gatepassModel>(
					  approverlist, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getgatepassByapproverList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}


		public async Task<IEnumerable<pageModel>> Getpagesbyroleid(int roleid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string approverlist = "select * from wms.wms_pages where roleid = " + roleid + "";

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<pageModel>(
					  approverlist, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "Getpagesbyroleid", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}

		}

		public async Task<IEnumerable<pageModel>> Getpages()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string approverlist = "select * from wms.wms_pages";

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<pageModel>(
					  approverlist, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "Getpages", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}

		}

		public int GatepassapproveByMail(gatepassModel model)
		{
			try
			{
				model.approverremarks = string.Empty;
				model.fmapproverremarks = string.Empty;
				var result = 0;
				return result;

				string updateapproverstatus = string.Empty;


				if (model.categoryid == 1)
				{
					updateapproverstatus = WMSResource.updateApprovedstatusbymanager.Replace("#approverstatus", model.approverstatus);

					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{

						result = DB.Execute(updateapproverstatus, new
						{
							model.approverremarks,
							model.gatepassid

						});
						if (result == 1)
						{
							int label = 1;
							string approvername = model.approvedby;
							string insertgatepasshistory = WMSResource.insertgatepassapprovalhistory;
							string approverstatus = model.approverstatus;
							var gatepasshistory = DB.ExecuteScalar(insertgatepasshistory, new
							{

								model.approverid,
								approvername,
								model.gatepassid,
								label,
								approverstatus
							});
						}
					}
				}
				else if (model.categoryid == 2)
				{
					updateapproverstatus = WMSResource.updateApprovedstatusbyFMmanager.Replace("#fmapprovedstatus", model.fmapprovedstatus);

					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{

						var result1 = DB.Execute(updateapproverstatus, new
						{
							model.fmapproverremarks,
							model.gatepassid

						});
						if (result1 == 1)
						{
							int label = 2;
							string approvername = model.approvedby;
							string insertgatepasshistory = WMSResource.insertgatepassapprovalhistory;
							string approverstatus = model.fmapprovedstatus;
							var gatepasshistory = DB.ExecuteScalar(insertgatepasshistory, new
							{

								model.approverid,
								approvername,
								model.gatepassid,
								label,
								approverstatus
							});
						}
					}
				}

				return (Convert.ToInt32(result));
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "GatepassapproveByMail", Ex.StackTrace.ToString());
				return 0;
			}
		}


		public int GatepassapproveByManager(gatepassModel model)
		{
			try
			{
				var result = 0;

				string updateapproverstatus = string.Empty;

				if (model.categoryid == 1)
				{
					updateapproverstatus = WMSResource.updateApprovedstatusbymanager.Replace("#approverstatus", "Approved");

					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{

						result = DB.Execute(updateapproverstatus, new
						{
							model.approverremarks,
							model.gatepassid

						});
						if (result == 1)
						{
							int label = 1;
							string approvername = model.approvedby;
							string insertgatepasshistory = WMSResource.insertgatepassapprovalhistory;
							string approverstatus = "Approved";
							var gatepasshistory = DB.ExecuteScalar(insertgatepasshistory, new
							{

								model.approverid,
								approvername,
								model.gatepassid,
								label,
								approverstatus
							});
						}
					}
				}
				else if (model.categoryid == 2)
				{
					updateapproverstatus = WMSResource.updateApprovedstatusbyFMmanager.Replace("#fmapprovedstatus", "Approved");

					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{

						var result1 = DB.Execute(updateapproverstatus, new
						{
							model.fmapproverremarks,
							model.gatepassid

						});
						if (result1 == 1)
						{
							int label = 2;
							string approvername = model.approvedby;
							string insertgatepasshistory = WMSResource.insertgatepassapprovalhistory;
							string approverstatus = "Approved";
							var gatepasshistory = DB.ExecuteScalar(insertgatepasshistory, new
							{

								model.approverid,
								approvername,
								model.gatepassid,
								label,
								approverstatus
							});
						}
					}
				}

				return (Convert.ToInt32(result));
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "GatepassapproveByManager", Ex.StackTrace.ToString());
				return 0;
			}
		}

		public async Task<IEnumerable<safteyStockList>> getSafteyStockList()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string query = WMSResource.getSafteyStockList;

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<safteyStockList>(
					  query, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getSafteyStockList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}

		}


		public async Task<IEnumerable<StockModel>> GetBinList()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string approverlist = WMSResource.getbinlist;

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<StockModel>(
					  approverlist, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetBinList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/// <summary>
		/// Get list of materials for stock transfer 
		/// Ramesh Kumar 15/07/2020
		/// </summary>
		/// <returns></returns>
		public async Task<IEnumerable<Materials>> GetMaterialstockcombo()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = "Select distinct st.materialid as material,mat.materialdescription  from wms.wms_stock st left outer join wms.\"MaterialMasterYGS\" mat on mat.material = st.materialid";

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<Materials>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetMaterialstockcombo", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		/// <summary>
		/// Updating location for stock
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>

		public string UpdateStockTransfer(List<StockModel> data)
		{


			StockModel obj = new StockModel();
			string loactiontext = string.Empty;

			foreach (StockModel stck in data)
			{
				NpgsqlTransaction Trans = null;
				stocktransferModel transfer = new stocktransferModel();


				using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				{
					try
					{
						pgsql.Open();
						Trans = pgsql.BeginTransaction();
						StockModel objs = new StockModel();

						string query = "select * from wms.wms_stock where itemid = '" + stck.itemid + "'";
						objs = pgsql.QueryFirstOrDefault<StockModel>(
						   query, null, commandType: CommandType.Text);

						if (objs != null)
						{
							transfer.itemid = stck.itemid;
							transfer.materialid = objs.materialid;
							transfer.previousqty = objs.availableqty;
							transfer.previouslocation = objs.itemlocation;
							transfer.transferedon = System.DateTime.Now;
							transfer.transferedby = stck.createdby;
							transfer.transferedqty = stck.availableqty;
							transfer.currentlocation = stck.itemlocation;
							transfer.remarks = stck.remarks;
							int decavail = objs.availableqty - stck.availableqty;

							string query1 = "UPDATE wms.wms_stock set availableqty=" + decavail + "  where itemid = '" + stck.itemid + "'";
							pgsql.ExecuteScalar(query1);
							StockModel objs1 = new StockModel();
							string query2 = "select * from wms.wms_stock where pono = '" + objs.pono + "' and materialid = '" + objs.materialid + "' and itemlocation = '" + stck.itemlocation + "'";
							objs1 = pgsql.QueryFirstOrDefault<StockModel>(
							   query2, null, commandType: CommandType.Text);
							if (objs1 != null)
							{
								int availqty = objs1.availableqty + stck.availableqty;

								string query4 = "UPDATE wms.wms_stock set availableqty=" + availqty + "  where pono = '" + objs.pono + "' and materialid = '" + objs.materialid + "' and itemlocation = '" + stck.itemlocation + "'";
								pgsql.ExecuteScalar(query4);
								string stockinsertqry = WMSResource.stocktransferinternal;
								var resultsxx = pgsql.ExecuteScalar(stockinsertqry, new
								{
									transfer.itemid,
									transfer.materialid,
									transfer.previouslocation,
									transfer.previousqty,
									transfer.currentlocation,
									transfer.transferedqty,
									transfer.transferedon,
									transfer.transferedby,
									transfer.remarks

								});
							}
							else
							{
								string insertqueryx = WMSResource.insertstock;
								stck.createddate = System.DateTime.Now;
								int itemid = 0;
								var result = 0;
								result = Convert.ToInt32(pgsql.ExecuteScalar(insertqueryx, new
								{
									objs.inwmasterid,
									objs.pono,
									stck.binid,
									objs.vendorid,
									objs.totalquantity,
									objs.shelflife,
									stck.availableqty,
									objs.deleteflag,
									//data.itemreceivedfrom,
									stck.itemlocation,
									stck.createddate,
									stck.createdby,
									objs.stockstatus,
									objs.materialid
								}));
								if (result != 0)
								{
									itemid = Convert.ToInt32(result);
									string insertqueryforlocationhistory = WMSResource.insertqueryforlocationhistory;
									var results = pgsql.ExecuteScalar(insertqueryforlocationhistory, new
									{
										stck.itemlocation,
										itemid,
										stck.createddate,
										stck.createdby,

									});

									string stockinsertqry = WMSResource.stocktransferinternal;
									var resultsxx = pgsql.ExecuteScalar(stockinsertqry, new
									{
										transfer.itemid,
										transfer.materialid,
										transfer.previouslocation,
										transfer.previousqty,
										transfer.currentlocation,
										transfer.transferedqty,
										transfer.transferedon,
										transfer.transferedby,
										transfer.remarks

									});

								}

							}




						}
						Trans.Commit();
					}
					catch (Exception Ex)
					{
						Trans.Rollback();
						log.ErrorMessage("PODataProvider", "UpdateStockTransfer", Ex.StackTrace.ToString());
						return null;
					}
				}
			}




			return loactiontext;




		}

		public string InvStockTransfer(invstocktransfermodel data)
		{


			StockModel obj = new StockModel();
			string loactiontext = string.Empty;
			int x = 0;
			invstocktransfermodel transfer = new invstocktransfermodel();
			foreach (stocktransfermateriakmodel stck in data.materialdata)
			{
				NpgsqlTransaction Trans = null;



				using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				{
					try
					{
						pgsql.Open();
						Trans = pgsql.BeginTransaction();
						StockModel objs = new StockModel();
						if (x == 0)
						{

							string compare = "ST" + DateTime.Now.Year.ToString().Substring(2);
							string Query = "select transferid from wms.wms_invstocktransfer order by transferredon desc limit 1";
							var rslt = pgsql.QueryFirstOrDefault<invstocktransfermodel>(
							Query, null, commandType: CommandType.Text);
							if (rslt != null)
							{
								string[] poserial = rslt.transferid.Split('T');
								int serial = Convert.ToInt32(poserial[1].Substring(2));
								int nextserial = serial + 1;
								string nextid = "ST" + DateTime.Now.Year.ToString().Substring(2) + nextserial.ToString().PadLeft(6, '0');
								transfer.transferid = nextid;
							}
							else
							{
								string nextid = "ST" + DateTime.Now.Year.ToString().Substring(2) + "000001";
								transfer.transferid = nextid;
							}

							transfer.transferredby = data.transferredby;
							transfer.transferredon = System.DateTime.Now;
							if (data.sourceplant == data.destinationplant)
							{
								transfer.transfertype = "interstock";
							}
							else
							{
								transfer.transfertype = "intrastock";
							}
							transfer.sourceplant = data.sourceplant;
							transfer.destinationplant = data.destinationplant;
							transfer.remarks = data.remarks;
							string mainstockinsertqueryy = WMSResource.insertInvStocktransfer;
							var resultsxx = pgsql.ExecuteScalar(mainstockinsertqueryy, new
							{
								transfer.transferid,
								transfer.transferredby,
								transfer.transferredon,
								transfer.transfertype,
								transfer.sourceplant,
								transfer.destinationplant,
								transfer.remarks

							});

						}

						string query = "select * from wms.wms_stock where itemid = '" + stck.sourceitemid + "'";
						objs = pgsql.QueryFirstOrDefault<StockModel>(
						   query, null, commandType: CommandType.Text);

						if (objs != null)
						{

							int decavail = objs.availableqty - stck.transferqty;
							int? inwmasterid = null;
							int? inwardid = null;
							if (objs.inwmasterid > 0)
							{
								inwmasterid = objs.inwmasterid;

							}
							if (objs.inwmasterid > 0)
							{
								inwardid = objs.inwardid;

							}

							string query1 = "UPDATE wms.wms_stock set availableqty=" + decavail + "  where itemid = '" + stck.sourceitemid + "'";
							pgsql.ExecuteScalar(query1);
							StockModel objs1 = new StockModel();
							string query2 = "select * from wms.wms_stock where pono = '" + objs.pono + "' and materialid = '" + objs.materialid + "' and itemlocation = '" + stck.destinationlocation + "'";
							objs1 = pgsql.QueryFirstOrDefault<StockModel>(
							   query2, null, commandType: CommandType.Text);
							if (objs1 != null)
							{
								int availqty = objs1.availableqty + stck.transferqty;

								string query4 = "UPDATE wms.wms_stock set availableqty=" + availqty + "  where pono = '" + objs.pono + "' and materialid = '" + objs.materialid + "' and itemlocation = '" + stck.destinationlocation + "'";
								pgsql.ExecuteScalar(query4);
								string stockinsertqry = WMSResource.insertinvtransfermaterial;
								var resultsxx = pgsql.ExecuteScalar(stockinsertqry, new
								{
									transfer.transferid,
									stck.materialid,
									stck.sourcelocation,
									stck.sourceitemid,
									stck.destinationlocation,
									stck.destinationitemid,
									stck.transferqty

								});
							}
							else
							{
								string insertqueryx = WMSResource.insertstock;
								DateTime createddate = System.DateTime.Now;
								int? binid = null;
								int availableqty = stck.transferqty;
								string itemlocation = stck.destinationlocation;
								string createdby = transfer.transferredby;
								int itemid = 0;
								var result = 0;
								result = Convert.ToInt32(pgsql.ExecuteScalar(insertqueryx, new
								{
									inwmasterid,
									objs.pono,
									binid,
									objs.vendorid,
									objs.totalquantity,
									objs.shelflife,
									availableqty,
									objs.deleteflag,
									//data.itemreceivedfrom,
									itemlocation,
									createddate,
									createdby,
									objs.stockstatus,
									objs.materialid,
									inwardid
								}));
								if (result != 0)
								{
									itemid = Convert.ToInt32(result);
									string insertqueryforlocationhistory = WMSResource.insertqueryforlocationhistory;
									var results = pgsql.ExecuteScalar(insertqueryforlocationhistory, new
									{
										itemlocation,
										itemid,
										createddate,
										createdby,

									});

									string stockinsertqry = WMSResource.insertinvtransfermaterial;
									stck.destinationitemid = itemid;
									var resultsxx = pgsql.ExecuteScalar(stockinsertqry, new
									{
										transfer.transferid,
										stck.materialid,
										stck.sourcelocation,
										stck.sourceitemid,
										stck.destinationlocation,
										stck.destinationitemid,
										stck.transferqty

									});

								}

							}




						}
						else
						{
							Trans.Rollback();
							return null;
						}
						Trans.Commit();
						x++;

					}
					catch (Exception Ex)
					{
						Trans.Rollback();
						log.ErrorMessage("PODataProvider", "UpdateStockTransfer1", Ex.StackTrace.ToString());
						return null;
					}
				}
			}




			return loactiontext;




		}


		public async Task<IEnumerable<stocktransferModel>> getstocktransferdata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getinternalstocktransferdata;

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<stocktransferModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getstocktransferdata", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public async Task<IEnumerable<stocktransferModel>> getstocktransferdatagroup()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.Stocktransferbygroup;

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<stocktransferModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getstocktransferdatagroup", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		/// <summary>
		/// get stock transferdata
		/// Ramesh 18/07/2020
		/// </summary>
		/// <returns></returns>
		public async Task<IEnumerable<invstocktransfermodel>> getstocktransferdatagroup1()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.invstocktransfermainquery;

					await pgsql.OpenAsync();
					var result = await pgsql.QueryAsync<invstocktransfermodel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					if (result != null & result.Count() > 0)
					{
						foreach (invstocktransfermodel dt in result)
						{
							var matqry = WMSResource.getinvtransfermaterialdetail.Replace("#tid", dt.transferid);
							var result1 = await pgsql.QueryAsync<stocktransfermateriakmodel>(
								  matqry, null, commandType: CommandType.Text);
							if (result1 != null & result1.Count() > 0)
							{
								dt.materialdata = result1.ToList();
							}
						}

					}
					return result;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getstocktransferdatagroup1", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}


		public async Task<IEnumerable<ddlmodel>> getgrnlistforacceptance()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getgrnlistdata;

					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<ddlmodel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					//List<ddlmodel> senddata = new List<ddlmodel>();
					//foreach(ddlmodel grn in data)
					//               {
					//	var qry = "select inwardid as value,inwmasterid as text from wms.wms_storeinward where qualitycheckrequired = True and (qualitychecked = False or qualitychecked is null) and inwmasterid = " + grn.value + "";
					//	var data1 = await pgsql.QueryAsync<ddlmodel>(
					//     qry, null, commandType: CommandType.Text);
					//	if(data1 == null || data1.Count() == 0)
					//                   {
					//		ddlmodel md = new ddlmodel();
					//		md.value = grn.text;
					//		md.text = grn.text;
					//		senddata.Add(md);
					//                   }
					//}
					return data.OrderByDescending(o => o.value);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getgrnlistforacceptance", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public async Task<IEnumerable<ddlmodel>> getgrnlistforacceptanceputaway()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getgrnlistdataforputaway;
					List<ddlmodel> returnlist = new List<ddlmodel>();
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<ddlmodel>(
					  materialrequestquery, null, commandType: CommandType.Text);

					foreach (ddlmodel ddl in data)
					{
						var exixtedrow = returnlist.Where(o => o.text == ddl.text).FirstOrDefault();
						if (exixtedrow == null)
						{
							returnlist.Add(ddl);

						}

					}
					return returnlist.OrderByDescending(o => o.value);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getgrnlistforacceptanceputaway", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public async Task<IEnumerable<inwardModel>> getgrnlistforacceptancenotify(string type)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = "";
					if (type == "Pending")
					{
						materialrequestquery = WMSResource.grnlistfornotify;
					}
					else if (type == "Sent")
					{
						materialrequestquery = WMSResource.getnotifiedgrnlist;
					}
					else
					{
						materialrequestquery = WMSResource.grnlistfornotify;
					}

					List<inwardModel> returnlist = new List<inwardModel>();
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<inwardModel>(
					  materialrequestquery, null, commandType: CommandType.Text);




					foreach (inwardModel ddl in data)
					{
						string validatequery = WMSResource.validategrnlistfornotify.Replace("#inwmasterid", ddl.inwmasterid.ToString());
						var datax = await pgsql.QueryAsync<ddlmodel>(
									validatequery, null, commandType: CommandType.Text);
						if (datax == null || datax.Count() == 0)
						{
							returnlist.Add(ddl);
						}


					}
					return returnlist.OrderByDescending(o => o.inwmasterid);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getgrnlistforacceptanceputaway", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}


		public async Task<IEnumerable<ddlmodel>> getholdgrlist()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getHoldGRList;
					List<ddlmodel> returnlist = new List<ddlmodel>();
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<ddlmodel>(
					  materialrequestquery, null, commandType: CommandType.Text);

					return data;


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getholdgrlist", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public async Task<IEnumerable<ddlmodel>> getgrnlistforacceptanceqc()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getqualitycheckdropdown;
					List<ddlmodel> returnlist = new List<ddlmodel>();
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<ddlmodel>(
					materialrequestquery, null, commandType: CommandType.Text);
					return data;


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getgrnlistforacceptanceqc", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public async Task<IEnumerable<ddlmodel>> getgrnlistforacceptanceqcbydate(string fromdt, string todt)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getqcdropdownbydate.Replace("#fromdt", fromdt).Replace("#todate", todt);
					List<ddlmodel> returnlist = new List<ddlmodel>();
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<ddlmodel>(
					materialrequestquery, null, commandType: CommandType.Text);
					return data;


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getgrnlistforacceptanceqc", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public async Task<IEnumerable<inwardModel>> getnotifiedgrbydate(string fromdt, string todt)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = "";
					
				
				    materialrequestquery = WMSResource.getnotifiedgrbydate.Replace("#fromdate",fromdt).Replace("#todate",todt);
					List<inwardModel> returnlist = new List<inwardModel>();
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<inwardModel>(
					  materialrequestquery, null, commandType: CommandType.Text);




					foreach (inwardModel ddl in data)
					{
						string validatequery = WMSResource.validategrnlistfornotify.Replace("#inwmasterid", ddl.inwmasterid.ToString());
						var datax = await pgsql.QueryAsync<ddlmodel>(
									validatequery, null, commandType: CommandType.Text);
						if (datax == null || datax.Count() == 0)
						{
							returnlist.Add(ddl);
						}


					}
					return returnlist.OrderByDescending(o => o.inwmasterid);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getnotifiedgrbydate", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}


		public async Task<IEnumerable<ddlmodel>> pendingreceiptslist()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getpendingreceiptslist;

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<ddlmodel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "pendingreceiptslist", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}


		public int UnholdGRdata(UnholdGRModel datamodel)
		{
			int result = 0;
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{


					pgsql.OpenAsync();
					string status = datamodel.unholdaction == true ? "accepted" : "returned";
					string qry = "Update wms.wms_securityinward set onhold = False,holdgrstatus='" + status + "',unholdedby = '" + datamodel.unholdedby + "',unholdedon = current_date,unholdremarks = '" + datamodel.unholdremarks + "' where inwmasterid = " + datamodel.inwmasterid + "";
					var results11 = pgsql.ExecuteScalar(qry);
					result = 1;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "updateonholdrow", Ex.StackTrace.ToString());
					return 0;
				}
				finally
				{
					pgsql.Close();
				}

			}
			return result;
		}
		//current
		public int mattransfer(materialtransferMain datamodel)
		{
			int result = 0;
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					int transferqty = 0;
					string createdby = datamodel.transferedby;
					string remarks = datamodel.transferremarks;
					string materialid = "";
					string updatereturnqty = "";
					pgsql.OpenAsync();
					updatereturnqty = WMSResource.updatetransferdata;
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						var resultx = DB.ExecuteScalar(updatereturnqty, new
						{

							transferqty,
							createdby,
							remarks,
							datamodel.projectcode,
							materialid
						});
						int tid = Convert.ToInt32(resultx);
						if (tid != 0)
						{
							datamodel.transferid = tid;
							foreach (materialtransferTR trdata in datamodel.materialdata)
							{
								string insertqrry = WMSResource.inserttransfermaterials;

								var result11 = DB.Execute(insertqrry, new
								{

									datamodel.transferid,
									trdata.materialid,
									trdata.transferredqty

								});
							}
							result = 1;
						}

					}



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "updateonholdrow", Ex.StackTrace.ToString());
					return 0;
				}
				finally
				{
					pgsql.Close();
				}

			}
			return result;
		}

		public int mrnupdate(MRNsavemodel datamodel)
		{
			int result = 0;
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{


					pgsql.OpenAsync();
					string qry = "Update wms.wms_securityinward set isdirecttransferred = True,projectcode='" + datamodel.projectcode + "',mrnby = '" + datamodel.directtransferredby + "',mrnon = current_date,mrnremarks = '" + datamodel.mrnremarks + "' where grnnumber = '" + datamodel.grnnumber + "'";
					var results11 = pgsql.ExecuteScalar(qry);
					result = 1;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "mrnupdate", Ex.StackTrace.ToString());
					return 0;
				}
				finally
				{
					pgsql.Close();
				}

			}
			return result;
		}

		public async Task<string> updateonholdrow(updateonhold datamodel)
		{
			string result = "";
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{

					string pono = datamodel.invoiceno.Split('-')[0];
					string invno = datamodel.invoiceno.Split('-')[1];
					await pgsql.OpenAsync();
					string qry = "Update wms.wms_securityinward set onhold = " + datamodel.onhold + ",onholdremarks = '" + datamodel.remarks + "' where pono = '" + pono + "' and invoiceno = '" + invno + "'";
					var results11 = pgsql.ExecuteScalar(qry);
					result = "Saved";

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "updateonholdrow", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
			return result;
		}
		/// <summary>
		/// get organasation dropdown
		/// Ramesh Kumar (28/07/2020)
		/// </summary>
		/// <returns></returns>
		public async Task<IEnumerable<ddlmodel>> getdepartmentmasterdata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getdepartmentmasterdata;

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<ddlmodel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getdepartmentmasterdata", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/// <summary>
		/// get rba details
		/// Ramesh Kumar (28/07/2020)
		/// </summary>
		/// <returns></returns>
		public async Task<IEnumerable<rbamaster>> getrbadetails()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = "select * from wms.wms_rbamaster";

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<rbamaster>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getrbadetails", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}


		public int requesttoreserve(materialReservetorequestModel obj)
		{

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				NpgsqlTransaction Trans = null;
				try
				{


					pgsql.Open();
					Trans = pgsql.BeginTransaction();
					string query = "update wms.wms_materialreserve set requestedby = '" + obj.requestedby + "', requestedon = current_date  where reserveid = " + obj.reserveid + "";
					var results11 = pgsql.ExecuteScalar(query);

					string materialrequestquery = "select * from wms.wms_materialreserve where reserveid = " + obj.reserveid + "";
					var datalist = pgsql.Query<ReserveMaterialModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

					//result = DB.Execute(insertquery, new
					//{
					//	// item.paitemid,
					//	item.quantity,
					//	item.requesteddate,
					//	item.approveremailid,
					//	item.approverid,
					//	item.pono,
					//	item.materialid,
					//	item.requesterid,
					//	item.requestedquantity,
					//	requestid,
					//});
					List<IssueRequestModel> reqdata = new List<IssueRequestModel>();
					foreach (ReserveMaterialModel rv in datalist)
					{
						IssueRequestModel model = new IssueRequestModel();
						model.quantity = rv.reservedqty;
						model.requesteddate = System.DateTime.Now;
						model.approveremailid = null;
						model.approverid = null;
						model.pono = rv.pono;
						model.material = rv.materialid;
						model.requesterid = obj.requestedby;
						model.requestedquantity = 0;
						reqdata.Add(model);

					}
					int saverequest = updaterequestedqty(reqdata);
					if (saverequest == 0)
					{
						Trans.Rollback();
						return 0;
					}
					string query1 = "update wms.wms_materialrequest set reserveid = " + obj.reserveid + " where requestid = " + saverequest + "";
					var results111 = pgsql.ExecuteScalar(query1);


					Trans.Commit();
					return 1;


				}
				catch (Exception Ex)
				{
					Trans.Rollback();
					log.ErrorMessage("PODataProvider", "updateonholdrow", Ex.StackTrace.ToString());
					return 0;
				}
				finally
				{
					pgsql.Close();
				}



			}
		}
		public int updatematmovement(List<materialistModel> obj)
		{

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{

					foreach (materialistModel mat in obj)
					{
						pgsql.OpenAsync();
						string query = "";
						if (mat.movetype == "out")
						{
							query = "update wms.wms_gatepassmaterial set outwardqty = " + mat.outwardqty + ", outwarddate = '" + mat.outwarddatestring + "' , outwardedby='" + mat.movedby + "',outwardremarks='" + mat.remarks + "' where gatepassmaterialid = " + mat.gatepassmaterialid + "";
						}
						else
						{
							query = "update wms.wms_gatepassmaterial set inwardqty = " + mat.inwardqty + ", inwarddate = '" + mat.inwarddatestring + "' , inwardedby='" + mat.movedby + "',inwardremarks='" + mat.remarks + "' where gatepassmaterialid = " + mat.gatepassmaterialid + "";
						}
						var results11 = pgsql.ExecuteScalar(query);
					}

					return 1;


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "updateonholdrow", Ex.StackTrace.ToString());
					return 0;
				}
				finally
				{
					pgsql.Close();
				}



			}
		}

		/// <summary>
		/// get stock type
		/// Ramesh 22/07/2020
		/// </summary>
		public string getstocktype(locataionDetailsStock locdetails)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getpendingreceiptslist;

					pgsql.OpenAsync();
					string query = WMSResource.getstocktype.Replace("#locationid", locdetails.locationid).Replace("#locationname", locdetails.locationname).Replace("#stid", Convert.ToString(locdetails.storeid)).Replace("#rkid", Convert.ToString(locdetails.rackid)).Replace("#biid", Convert.ToString(locdetails.binid));
					string stocktype = pgsql.QueryFirstOrDefault<string>(
					   query, null, commandType: CommandType.Text);
					return stocktype;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getstocktype", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public int UpdateMaterialReserve()
		{
			int result = 0;
			List<ReserveMaterialModel> _listobj = new List<ReserveMaterialModel>();
			EmailUtilities emailutil = new EmailUtilities();
			EmailModel emailobj = new EmailModel();
			emailobj.FrmEmailId = "developer1@in.yokogawa.com";
			emailobj.CC = "sushma.patil@in.yokogawa.com";
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					pgsql.Open();
					string query = WMSResource.Getreservelist;
					string updatequery = string.Empty;
					string updatedon = WMSResource.updatedon;
					_listobj = pgsql.Query<ReserveMaterialModel>(
					  query, null, commandType: CommandType.Text).ToList();
					if (_listobj.Count != 0)
					{
						foreach (var items in _listobj)
						{
							var dateAndTime = DateTime.Now;
							var date = dateAndTime.Date;
							var reservedate = items.reserveupto.Date;
							if (reservedate == date)
							{
								updatequery = WMSResource.updatetostockreserveqty.Replace("@reservedqty", Convert.ToString(items.reservedqty)).Replace("@itemid", Convert.ToString(items.itemid));
								result = pgsql.Execute(updatequery, new
								{

								});
								updatedon = WMSResource.updatedon.Replace("@reserveformaterialid", Convert.ToString(items.reserveformaterialid));
								result = pgsql.Execute(updatedon, new
								{

								});
								emailobj.ToEmailId = items.email;
								emailutil.sendEmail(emailobj, 10);
							}
							else if (reservedate == date.AddDays(1))
							{
								emailobj.ToEmailId = items.email;
								emailutil.sendEmail(emailobj, 11);
							}
							else if (reservedate == date.AddDays(2))
							{
								emailobj.ToEmailId = items.email;
								emailutil.sendEmail(emailobj, 12);
							}
						}
					}
				}
				catch (Exception ex)
				{

					log.ErrorMessage("PODataProvider", "UpdateMaterialReserve", ex.StackTrace.ToString());
					return 0;
				}
				finally
				{
					pgsql.Close();
				}

			}

			return result;


		}

		//get stock details
		public stockCardPrint getstockdetails(string pono, string materialid)
		{
			stockCardPrint objstock = new stockCardPrint();
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					pgsql.Open();
					string query = WMSResource.GetStockDetails.Replace("#pono", pono).Replace("#materialid", materialid);
					string updatequery = string.Empty;
					//string updatedon = WMSResource.updatedon;
					objstock = pgsql.QuerySingle<stockCardPrint>(
					  query, null, commandType: CommandType.Text);

				}
				catch (Exception ex)
				{

					log.ErrorMessage("PODataProvider", "getstockdetails", ex.StackTrace.ToString());
					return objstock;
				}
				finally
				{
					pgsql.Close();
				}

			}

			return objstock;

		}
		/// <summary>
		/// inserting the data to retrunmaterial by pm
		/// </summary>
		/// <param name="_listobj"></param>
		/// <returns></returns>
		public int UpdateReturnqty(List<IssueRequestModel> _listobj)
		{
			int result = 0;
			int requestid = 0;
			if (_listobj.Count != 0)
			{
				try
				{
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						IssueRequestModel obj = new IssueRequestModel();
						DB.Open();
						string query = WMSResource.getnxtreturnid;
						obj = DB.QueryFirstOrDefault<IssueRequestModel>(
						   query, null, commandType: CommandType.Text);
						if (obj != null)
							requestid = obj.matreturnid + 1;
						else
							requestid = 1;

						string updatereturnqty = string.Empty;
						foreach (var item in _listobj)
						{
							item.matreturnid = requestid;

							if (item.returnqty != 0)
							{
								item.materialid = item.material;
								updatereturnqty = WMSResource.UpdateReturnqty;

								result = DB.Execute(updatereturnqty, new
								{
									item.materialissueid,
									item.requestforissueid,
									item.returnqty,
									item.createdby,
									item.requesttype,
									item.transferqty,
									item.requestid,
									item.remarks,
									item.materialid,
									item.matreturnid
								});
							}
						}
					}
				}
				catch (Exception ex)
				{
					log.ErrorMessage("PODataProvider", "UpdateReturnqty", ex.StackTrace.ToString());
					return 0;
				}

			}

			return result;
		}
		public int UpdateReturnmaterialTostock(List<IssueRequestModel> model)
		{
			int result = 0;
			if (model.Count != 0)
			{
				foreach (var item in model)
				{
					try
					{
						//if (item.returnqty != 0)
						//{
						string updatereturnqtytomaterialissue = WMSResource.updatereturnqtyByInvMngr.Replace("@returnid", Convert.ToString(item.returnid));
						using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						{
							result = DB.Execute(updatereturnqtytomaterialissue, new
							{

							});
						}
						//}
						if (result != 0)
						{
							int availableqty = item.confirmqty;
							string materialid = item.material;
							string createdby = item.createdby;
							//string updatereturnqtytostock = WMSResource.updatereturnmaterialToStock.Replace("@availableqty", Convert.ToString(item.returnqty)).Replace("@itemid", Convert.ToString(item.itemid));
							string updatereturnqtytostock = WMSResource.updatetostockbyinvmanger;
							using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
							{
								result = DB.Execute(updatereturnqtytostock, new
								{

									materialid,
									item.itemlocation,
									availableqty,
									createdby,
									item.returnid
								});
							}
						}
					}
					catch (Exception ex)
					{
						log.ErrorMessage("PODataProvider", "UpdateReturnmaterialTostock", ex.StackTrace.ToString());
						return 0;
					}
				}
			}
			return result;
		}


		public string updateputawayfilename(ddlmodel file)
		{
			string result = "";
			int rslt = 0;
			try
			{


				string grn = file.text;
				string filename = file.value;
				string updateqry = "update wms.wms_securityinward set putawayfilename = '" + filename + "' where grnnumber = '" + grn + "'";
				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					rslt = DB.Execute(updateqry);
					if (rslt != 0)
					{
						result = "saved";
					}
					else
					{
						result = "error";
					}
				}

			}
			catch (Exception ex)
			{
				log.ErrorMessage("PODataProvider", "updateputawayfilename", ex.StackTrace.ToString());
				return "error";
			}

			return result;
		}

		public string notifyputaway(notifymodel data)
		{
			string result = "";
			int rslt = 0;
			try
			{


				string grn = data.grnnumber;
				string remarks = data.notifyremarks;
				string notifyby = data.notifiedby;
				//inw.notifyremarks,inw.notifiedby,inw.notifiedtofinance,inw.notifiedon,inw.putawayfilename,
				string updateqry = "update wms.wms_securityinward set notifiedtofinance = True,notifiedon = current_date,notifiedby='" + notifyby + "', notifyremarks = '" + remarks + "' where grnnumber = '" + grn + "'";
				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					rslt = DB.Execute(updateqry);
					if (rslt != 0)
					{
						result = "saved";
						EmailModel emailmodel = new EmailModel();
						emailmodel.jobcode = data.grnnumber;
						emailmodel.ToEmailId = "ramesh.kumar@in.yokogawa.com";
						emailmodel.FrmEmailId = "developer1@in.yokogawa.com";
						emailmodel.CC = "sushma.patil@in.yokogawa.com";
						EmailUtilities emailobj = new EmailUtilities();
						//emailobj.sendEmail(emailmodel, 13);


					}
					else
					{
						result = "error";
					}
				}

			}
			catch (Exception ex)
			{
				log.ErrorMessage("PODataProvider", "notifyputaway", ex.StackTrace.ToString());
				return "error";
			}

			return result;
		}

		public string notifymultipleputaway(List<notifymodel> datalst)
		{
			string result = "";
			int rslt = 0;
			try
			{
				string grns = "";
				int loop = 1;

				foreach (notifymodel data in datalst)
				{
					string grn = data.grnnumber;
					string remarks = data.notifyremarks;
					string notifyby = data.notifiedby;
					//inw.notifyremarks,inw.notifiedby,inw.notifiedtofinance,inw.notifiedon,inw.putawayfilename,
					string updateqry = "update wms.wms_securityinward set notifiedtofinance = True,notifiedon = current_date,notifiedby='" + notifyby + "', notifyremarks = '" + remarks + "' where grnnumber = '" + grn + "'";
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						rslt = DB.Execute(updateqry);
						if (rslt != 0)
						{
							result = "saved";
							if (loop > 1)
							{
								grns += ", ";
							}
							grns += grn;

						}
						else
						{
							result = "error";
						}
					}

				}

				EmailModel emailmodel = new EmailModel();
				emailmodel.jobcode = grns;
				emailmodel.ToEmailId = "developer1@in.yokogawa.com";
				emailmodel.FrmEmailId = "sushma.patil@in.yokogawa.com";
				EmailUtilities emailobj = new EmailUtilities();
				//emailobj.sendEmail(emailmodel, 13);



			}
			catch (Exception ex)
			{
				log.ErrorMessage("PODataProvider", "notifyputaway", ex.StackTrace.ToString());
				return "error";
			}

			return result;
		}
		/// <summary>
		/// onload to display the returned already by PM
		/// </summary>
		public async Task<IEnumerable<IssueRequestModel>> GetReturnmaterialList()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string query = WMSResource.GetreturnList;
					string updatequery = string.Empty;
					//string updatedon = WMSResource.updatedon;
					return await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);

				}
				catch (Exception ex)
				{

					log.ErrorMessage("PODataProvider", "GetReturnmaterialList", ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}


		}
		/// <summary>
		/// based on request/return id we will get details for confirm
		/// </summary>
		/// <param name="requestid"></param>
		/// <returns></returns>
		public async Task<IEnumerable<IssueRequestModel>> GetReturnmaterialListForConfirm(string requestid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string query = WMSResource.getlistforconfirm.Replace("#returnid", requestid);
					string updatequery = string.Empty;
					//string updatedon = WMSResource.updatedon;
					return await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);

				}
				catch (Exception ex)
				{

					log.ErrorMessage("PODataProvider", "GetReturnmaterialListForConfirm", ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}


		}
		/// <summary>
		/// getreturn data by empno
		/// </summary>
		/// <param name="empno"></param>
		/// <returns></returns>
		public async Task<IEnumerable<IssueRequestModel>> getreturndata(string empno)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string query = WMSResource.getreturndata.Replace("#createdby", empno); ;
					string updatequery = string.Empty;
					//string updatedon = WMSResource.updatedon;
					return await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);

				}
				catch (Exception ex)
				{

					log.ErrorMessage("PODataProvider", "getreturndata", ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}

		/// <summary>
		/// get materials requested for return data based on material return requested id - Gayathri -> 14/08/2020
		/// </summary>
		/// <param name="empno"></param>
		/// <returns></returns>
		public async Task<IEnumerable<IssueRequestModel>> getmaterialreturnreqList(int matreturnid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string query = WMSResource.getmatreturndetails.Replace("@matreid", Convert.ToString(matreturnid));
					string updatequery = string.Empty;
					//string updatedon = WMSResource.updatedon;
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);
					return data;

				}
				catch (Exception ex)
				{

					log.ErrorMessage("PODataProvider", "gettransferdata", ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}

		/// <summary>
		/// get transferred data based on login id
		/// </summary>
		/// <param name="empno"></param>
		/// <returns></returns>
		public async Task<IEnumerable<materialtransferMain>> gettransferdata(string empno)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string query = WMSResource.gettransferdata.Replace("#createdby", empno);
					string updatequery = string.Empty;
					//string updatedon = WMSResource.updatedon;
					var data = await pgsql.QueryAsync<materialtransferMain>(
					   query, null, commandType: CommandType.Text);
					if (data != null && data.Count() > 0)
					{
						foreach (materialtransferMain dt in data)
						{
							string query1 = WMSResource.gettransferiddetail.Replace("#tid", dt.transferid.ToString());
							var datadetail = await pgsql.QueryAsync<materialtransferTR>(
							   query1, null, commandType: CommandType.Text);

							if (datadetail != null && datadetail.Count() > 0)
							{
								dt.materialdata = datadetail.ToList();
							}
						}
					}
					return data;

				}
				catch (Exception ex)
				{

					log.ErrorMessage("PODataProvider", "gettransferdata", ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}
		/// <summary>
		/// update/insert transfer material details
		/// </summary>
		/// <param name="_listobj"></param>
		/// <returns></returns>
		public int Updatetransferqty(List<IssueRequestModel> _listobj)
		{
			int result = 0;
			if (_listobj.Count != 0)
			{
				string updatereturnqty = string.Empty;
				foreach (var item in _listobj)
				{

					try
					{
						if (item.transferqty != 0)
						{
							item.materialid = item.material;
							updatereturnqty = WMSResource.updatetransferdata;
							using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
							{
								result = DB.Execute(updatereturnqty, new
								{

									item.transferqty,
									item.createdby,
									item.remarks,
									item.projectcode,
									item.materialid
								});
							}
						}
					}
					catch (Exception ex)
					{
						log.ErrorMessage("PODataProvider", "UpdateReturnqty", ex.StackTrace.ToString());
						return 0;
					}
				}
			}
			return result;
		}


		/// <summary>
		/// checkMatExists - check whether the material exists and generate qrcode for material
		/// Gayathri -  06/08/2020
		/// </summary>
		public string checkMatExists(string material)
		{
			string returnvalue = null;
			string path = null;
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					//pgsql.OpenAsync();
					string materialrequestquery = WMSResource.checkmatexists.Replace("#materialid", Convert.ToString(material));

					var obj = pgsql.QueryFirstOrDefault<MaterialData>(
				materialrequestquery, null, commandType: CommandType.Text);
					if (obj == null)
					{

						return returnvalue = "No material exists";
					}
					else
					{
						path = Environment.CurrentDirectory + @"\Barcodes\";

						if (!Directory.Exists(path))
						{
							Directory.CreateDirectory(path);
						}
						//generate barcode for material code and GRN No.
						var content = material;
						BarcodeWriter writer = new BarcodeWriter
						{
							Format = BarcodeFormat.QR_CODE,
							Options = new EncodingOptions
							{
								Height = 90,
								Width = 100,
								PureBarcode = false,
								Margin = 1,

							},
						};
						var bitmap = writer.Write(content);

						// write text and generate a 2-D barcode as a bitmap
						writer
							.Write(content)
							.Save(path + content + ".bmp");

						path = "./Barcodes/" + content + ".bmp";

						return path;
					}


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "checkMatExists", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		//Get podetails list Gayathri - 10/08/2020
		public async Task<IEnumerable<PODetails>> getPODetails()
		{
			//List<PODetails> objPO = new List<PODetails>();
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string getpoquery = WMSResource.getPODetails;

					var objPO = await pgsql.QueryAsync<PODetails>(
					   getpoquery, null, commandType: CommandType.Text);
					//objPO = pgsql.QueryAsync<List<PODetails>>(
					//			getpoquery, null, commandType: CommandType.Text);

					return objPO;
				}
				catch (Exception ex)
				{
					log.ErrorMessage("PODataProvider", "getPODetails", ex.StackTrace.ToString());
					return null;
				}
			}
		}

		//Ramesh
		public async Task<IEnumerable<testcrud>> gettestcrud()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string testgetquery = WMSResource.testcrudget;


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<testcrud>(
					  testgetquery, null, commandType: CommandType.Text);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "gettestcrud", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		public string posttestcrud(testcrud data)
		{
			string result = "";
			int rslt = 0;
			try
			{


				if (data.id == 0)
				{
					string insertqry = WMSResource.posttestcrud;
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						rslt = DB.Execute(insertqry, new
						{
							data.name,
							data.ismanager
						});

						if (rslt == 0)
						{
							result = "error";
						}
						else
						{
							result = "saved";
						}

					}


				}
				else
				{
					string insertqry = WMSResource.puttestcurd.Replace("#id", data.id.ToString());
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						rslt = DB.Execute(insertqry, new
						{
							data.name,
							data.ismanager
						});

						if (rslt == 0)
						{
							result = "error";
						}
						else
						{
							result = "Updated";
						}

					}

				}


			}
			catch (Exception ex)
			{
				log.ErrorMessage("PODataProvider", "notifyputaway", ex.StackTrace.ToString());
				return "error";
			}

			return result;
		}
		public string deletetestcurd(int id)
		{
			string result = "";
			int rslt = 0;
			try
			{

				string deleteqry = WMSResource.deletetestcurd.Replace("#id", id.ToString());
				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					rslt = DB.Execute(deleteqry);

					if (rslt == 0)
					{
						result = "error";
					}
					else
					{
						result = "Deleted";
					}

				}

				return result;
			}

			catch (Exception ex)
			{

				Console.WriteLine(ex.Message);
				return "error";

			}


		}
	}
}
