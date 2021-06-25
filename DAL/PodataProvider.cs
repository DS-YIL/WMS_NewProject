/*
    Name of File : <<PodataProvider>>  Author :<<Shashikala>>  
    Date of Creation <<12-12-2019>>
    Purpose : <<All code Functionalities except login are written in this DAL>>
    Review Date :<<>>   Reviewed By :<<>>
    Version : 0.1 
    Sourcecode Copyright : Yokogawa India Limited
*/

using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using WMS.Common;
using WMS.Interfaces;
using WMS.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Net.Sockets;
using System.Net;
using ZXing;
using ZXing.Common;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using ZXing.QrCode.Internal;
using System.Data.OleDb;
using static WMS.Common.EmailUtilities;
using System.Globalization;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Configuration;

/*
    Name of namespace : <<WMS>>  Author :<<Shashikala>>  
    Date of Creation <<12-12-2019>>
    Purpose : <<All code Functionalities except login are written in this DAL>>
    Review Date :<<>>   Reviewed By :<<>>
    Sourcecode Copyright : Yokogawa India Limited
*/

namespace WMS.DAL
{

	/*
    Name of Class : <<PodataProvider>>  Author :<<Shashikala>>  
    Date of Creation <<12-12-2019>>
    Purpose : <<All code Functionalities except login are written in this class>>
    Review Date :<<>>   Reviewed By :<<>>
   
*/

	public class PodataProvider : IPodataService<OpenPoModel>
	{
		Configurations config = new Configurations();
		ErrorLogTrace log = new ErrorLogTrace();

		string url = "";
		private readonly IHttpContextAccessor _httpContextAccessor;
		public PodataProvider(IHttpContextAccessor _httpContextAccessor)
		{
			this._httpContextAccessor = _httpContextAccessor;
			url = _httpContextAccessor.HttpContext.Request.Host + _httpContextAccessor.HttpContext.Request.Path;
		}
		/*
    Name of Function : <<CheckPoexists>>  Author :<<Ramesh>>  
    Date of Creation <<12-12-2019>>
    Purpose : <<check pono exists or not >>
	<param name="PONO"></param>
    Review Date :<<>>   Reviewed By :<<>>
	*/

		public OpenPoModel CheckPoexists(string PONO)
		{

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					OpenPoModel returndata = new OpenPoModel();
					pgsql.Open();
					//string query = "select asno.asn as asnno,asno.pono,pl.suppliername as vendorname from wms.wms_asn asno left outer join wms.wms_polist pl on pl.pono = asno.pono where asno.asn = '" + PONO.Trim() + "'";
					string query = "select pomat.asnno,pomat.pono,pl.suppliername as vendorname,max(pomat.invoiceno) as invoiceno from wms.wms_pomaterials pomat left outer join wms.wms_polist pl on pl.pono = pomat.pono where pomat.asnno = '" + PONO.Trim() + "' group by pomat.asnno,pomat.pono,pl.suppliername";
					var podata = pgsql.QueryAsync<OpenPoModel>(
					   query, null, commandType: CommandType.Text);

					int count = podata.Result.Count();

					if (podata != null && (podata.Result.Count() > 0))
					{
						//List<OpenPoModel> podataList = podata.Result.ToList();
						//returndata = podata.Result.FirstOrDefault();
						string postr = "";
						int i = 0;
						foreach (OpenPoModel model in podata.Result)
						{
							returndata = model;
							if (i > 0)
							{
								postr += ",";
							}
							postr += model.pono;
							returndata.pono = postr;
							i++;


						}
						returndata.ispono = false;
						returndata.isasn = true;
						returndata.issupplier = false;
						return returndata;

					}
					else
					{
						query = "select pono,suppliername as vendorname from wms.wms_polist where pono = '" + PONO.Trim() + "'";
						var podata1 = pgsql.QueryFirstOrDefault<OpenPoModel>(
						   query, null, commandType: CommandType.Text);
						if (podata1 != null)
						{
							podata1.ispono = true;
							podata1.isasn = false;
							podata1.issupplier = false;
						}
						else
						{
							query = "select suppliername as vendorname from wms.wms_polist where LOWER(suppliername) = LOWER('" + PONO.Trim() + "') ";
							var podata2 = pgsql.QueryFirstOrDefault<OpenPoModel>(
							   query, null, commandType: CommandType.Text);
							if (podata2 != null)
							{
								podata2.ispono = false;
								podata2.isasn = false;
								podata2.issupplier = true;
							}
							return podata2;
						}
						return podata1;
					}
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "CheckPoexists", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
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


		/*
		Name of Function : <<getOpenPoList>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get lst of open pono list>>
		<param name="loginid"></param>
		 <param name="pono"></param>
		 <param name="docno"></param>
		 <param name="vendorid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					query = query + " group by track.pono,wp.pono order by wp.pono asc ";

					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<OpenPoModel>(
					   query, null, commandType: CommandType.Text);


					return data;


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getOpenPoList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}

		public async Task<IEnumerable<OpenPoModel>> getdashboardlist(string loginid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.PMDashboardqry.Replace("#projectmanager", loginid);


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<OpenPoModel>(
					   query, null, commandType: CommandType.Text);


					return data;


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getOpenPoList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}

		/*
		Name of Function : <<getPODataList>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get po numbers>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<POList>> getPODataList(string suppliername)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				//List<POList> objpo = null;
				try
				{
					//objpo = new List<POList>();
					string query = "select pono,suppliername from wms.wms_polist where (suppliername='" + suppliername + "' AND type='po') ";
					//string query = "select asno.asn as asnno,asno.pono,pl.suppliername as vendorname from wms.wms_asn asno left outer join wms.wms_polist pl on pl.pono = asno.pono where pl.suppliername = '#suppliername'";

					await pgsql.OpenAsync();
					var objpo = await pgsql.QueryAsync<POList>(
					   query, null, commandType: CommandType.Text);
					return objpo;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getPOList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}

			}
		}

		/*
		Name of Function : <<getMaterialDetails>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get Material details based on grn number>>
		<param name="grnNo"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
								decimal? issuedqty = 0;
								decimal? reservedqty = 0;

								Enquirydata enquiryobj = new Enquirydata();
								string availableQtyqry = "select sum(availableqty) as availableqty from wms.wms_stock where materialid ='" + mtData.materialid + "' and pono='" + mtData.pono + "' and inwmasterid = '" + mtData.inwmasterid + "'";
								enquiryobj = pgsql.QuerySingleOrDefault<Enquirydata>(
												availableQtyqry, null, commandType: CommandType.Text);
								result.availableqty = enquiryobj.availableqty;

								ReportModel modelobj = new ReportModel();
								string matIssuedQuery = "select sum(iss.issuedqty)as issuedqty from wms.wms_materialissue iss" +
									" join wms.wms_stock sk on sk.pono='" + mtData.pono + "' where iss.itemid = sk.itemid and sk.materialid='" + mtData.materialid + "' and sk.inwmasterid= '" + mtData.inwmasterid + "'";
								modelobj = pgsql.QuerySingleOrDefault<ReportModel>(
												matIssuedQuery, null, commandType: CommandType.Text);
								if (modelobj != null)
								{
									issuedqty = modelobj.issuedqty;
								}
								//Get material reserved qty
								ReserveMaterialModel modeldataobj = new ReserveMaterialModel();
								string matReserveQuery = "select sum(reser.reservequantity )as reservedqty from wms.materialreservedetails reser join wms.wms_stock sk on sk.pono='" + mtData.pono + "' where reser.itemid =sk.itemid and sk.materialid='" + mtData.materialid + "' and sk.inwmasterid='" + mtData.inwmasterid + "'";
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
								////string matgateQuery = "select sum(matiss.issuedqty)as quantity from wms.wms_gatepassmaterial gtmat join wms.wms_materialissue matiss on matiss.gatepassmaterialid = gtmat.gatepassmaterialid join wms.wms_gatepass gp on gp.gatepassid = gtmat.gatepassid join wms.wms_stock sk on sk.pono='" + mtData.pono + "' where sk.inwmasterid ="+ mtData.inwmasterid+ "and matiss.itemid =sk.itemid and gtmat.materialid ='"+mtData.materialid+"'";
								//IssueRequestModel obj = new IssueRequestModel();
								//pgsql.Open();

								//obj = pgsql.QuerySingle<gatepassModel>(
								//   matgateQuery, null, commandType: CommandType.Text);

								//gatepassissuedqty = obj.quantity;
								//totalissed = Convert.ToInt32(issuedqty) + Convert.ToInt32(reservedqty) + gatepassissuedqty;
								totalissed = Convert.ToInt32(issuedqty) + Convert.ToInt32(reservedqty);
								result.issued = totalissed;


							}
							objMaterial.Add(result);
						}
						//objMaterial.Add(result);
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
					log.ErrorMessage("PODataProvider", "getMaterialDetails", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}


		/*
        Name of Function : <<getPOList>>  Author :<<Gayathri>>  
        Date of Creation <<12-12-2019>>
        Purpose : <<get po numbers and qty>>
        <param: name=postatus - Displaying data based on status selected></param>
        Review Date :<<>>   Reviewed By :<<>>
        */
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
						//If PO status is security check get materials
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
					log.ErrorMessage("PODataProvider", "getPOList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}


		/*
		Name of Function : <<generateBarcodeMaterial>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Generate QRCode>>
		<param name="printMat"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public printMaterial generateBarcodeMaterial(printMaterial printMat)
		{
			printMaterial objprint = new printMaterial();
			MateriallabelModel objserial = new MateriallabelModel();
			MateriallabelModel objdata = new MateriallabelModel();
			try
			{

				string path = "";

				path = Environment.CurrentDirectory + @"\Barcodes\";

				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}

				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					//Check if the material is already printed
					string query = "select * from wms.wms_pomaterials where pono='" + printMat.pono + "' and materialid='" + printMat.materialid + "' and itemno='" + printMat.lineitemno + "'";
					// string query = "Select * from wms.wms_securityinward sinw join wms.wms_printstatusmaterial psmat on psmat.inwmasterid=sinw.inwmasterid where sinw.pono='" + printMat.pono + "' and sinw.invoiceno='" + printMat.invoiceno + "' and psmat.materialid='" + printMat.materialid + "'";
					//string query = "Select * from wms.wms_securityinward sinw left join wms.wms_st_materiallabel psmat on psmat.po = sinw.pono where sinw.pono = '" + printMat.pono + "' and sinw.invoiceno = '" + printMat.invoiceno + "'  and psmat.mscode = '" + printMat.materialid + "'";

					objprint = DB.QueryFirstOrDefault<printMaterial>(
						   query, null, commandType: CommandType.Text);

					//For F-type get the based on order No.
					if (objprint.codetype == "F")
					{
						string querydata = "select * from wms.st_QTSO where serviceorderno='" + objprint.serviceorderno + "' limit 1";
						objdata = DB.QueryFirstOrDefault<MateriallabelModel>(
							   querydata, null, commandType: CommandType.Text);

						if (objdata != null)
						{
							string queryserial = "select * from wms.st_slno_imports where saleorderno='" + objdata.saleorderno + "' and solineitemno= '" + objdata.solineitemno + "' limit 1";
							objserial = DB.QueryFirstOrDefault<MateriallabelModel>(
								   queryserial, null, commandType: CommandType.Text);
						}



					}
					//For N-type get data based from st_QTSO based on project definition
					else if (objprint.codetype == "N")
					{
						if (objprint.projectiddef != null)
						{
							string querydata = "select * from wms.st_QTSO where projectiddef='" + objprint.projectiddef + "' limit 1";
							objdata = DB.QueryFirstOrDefault<MateriallabelModel>(
								   querydata, null, commandType: CommandType.Text);

							if (objdata != null)
							{
								string queryserial = "select * from wms.st_slno_imports where saleorderno='" + objdata.saleorderno + "' and solineitemno= '" + objdata.solineitemno + "' limit 1";
								objserial = DB.QueryFirstOrDefault<MateriallabelModel>(
									   queryserial, null, commandType: CommandType.Text);
							}

						}




					}
					else
					{
						if (objprint.saleorderno != null)
						{
							string queryserial = "select * from wms.st_slno_imports where saleorderno='" + objprint.saleorderno + "' and solineitemno= '" + objprint.solineitemno + "' limit 1";
							objserial = DB.QueryFirstOrDefault<MateriallabelModel>(
								   queryserial, null, commandType: CommandType.Text);

							string querydata = "select * from wms.st_QTSO where saleorderno='" + objprint.saleorderno + "' and solineitemno= '" + objprint.solineitemno + "' limit 1";
							objdata = DB.QueryFirstOrDefault<MateriallabelModel>(
								   querydata, null, commandType: CommandType.Text);
						}

					}

					//Get YGS GR No.
					string quesryygsgr = "select sapgr from wms.wms_sapgr where wmsgr='" + printMat.grnno + "'";
					var ygsgr = DB.QueryFirstOrDefault<string>(
							   quesryygsgr, null, commandType: CommandType.Text);

					//Check the length of soline item number and append Zero
					if (printMat.solineitemno != null)
					{
						if (objprint.solineitemno.Length <= 5)
						{
							int length = objprint.solineitemno.Length;
							int countlength = 6 - length;
							objprint.solineitemno = objprint.solineitemno.PadLeft(6, '0');
						}
					}
					if (objserial != null)
					{
						objprint.serialno = objserial.serialno ?? "-";
					}
					if (objdata != null)
					{
						objprint.saleordertype = objdata.saleordertype;
						objprint.customername = objdata.customername;
						objprint.shipto = objdata.shipto;
						objprint.shippingpoint = objdata.shippingpoint;
						objprint.loadingdate = Convert.ToString(objdata.loadingdate);
						objprint.projectiddef = objdata.projectiddef;
						objprint.projecttext = objdata.projecttext;
						objprint.partno = objdata.partno;
						objprint.custpo = objdata.custpo;
						objprint.costcenter = objdata.costcenter;
						objprint.costcentertext = objdata.costcentertext;
						objprint.saleordertypetext = objdata.saleordertypetext;
						objprint.customercode = objdata.customercode;
						objprint.custpolineitem = objdata.custpolineitem;
						objprint.serviceorderno = objdata.serviceorderno;
					}

					objprint.grnno = printMat.grnno;
					objprint.ygsgr = ygsgr;
					objprint.currentdate = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
					objprint.noofpieces = printMat.noofpieces;
					objprint.boxno = printMat.boxno;
					objprint.totalboxes = printMat.totalboxes;
					objprint.insprec = "Not Required";
					objprint.order = objprint.saleorderno + "-" + objprint.solineitemno;
					objprint.qty = objprint.noofpieces + "/" + printMat.receivedqty + " ST " + objprint.boxno + " OF " + objprint.totalboxes + " BOXES";


				}
				PrintUtilities objprntmat = new PrintUtilities();
				//generate barcodes in material label
				//Material barcode
				printMat.materialbarcode = "./Barcodes/" + objprint.materialid + ".bmp";
				var content = objprint.materialid;
				objprint.materialbarcode = objprntmat.generatebarcode(path, content);

				//order barcode
				printMat.soiembarcode = "./Barcodes/" + objprint.saleorderno + "_" + objprint.solineitemno + ".bmp";
				content = objprint.saleorderno + "-" + objprint.solineitemno;
				objprint.soiembarcode = objprntmat.generatebarcode(path, content);

				//plant barcode
				printMat.plantbarcodepath = "./Barcodes/" + objprint.plant + ".bmp";
				content = objprint.plant;
				objprint.plantbarcodepath = objprntmat.generatebarcode(path, content);

				//sp barcode
				printMat.storagebarcodepath = "./Barcodes/" + objprint.spbarcode + ".bmp";
				content = objprint.storagelocation;
				objprint.storagebarcodepath = objprntmat.generatebarcode(path, content);

				//Linkage barcode
				printMat.linkagebarcodepath = "./Barcodes/" + objprint.linkageno + ".bmp";
				content = objprint.linkageno;
				objprint.linkagebarcodepath = objprntmat.generatebarcode(path, content);

				int noofprints = 1;
				bool isprint = true;
				bool isonholdgr = false;
				//Save data in database
				string insertqueryforinvoice = WMSResource.insertmatbarcodelabeldata;
				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					int qtyinbox = objprint.noofpieces;
					string matbarcodepath = objprint.materialcodePath;
					string soitembcpath = objprint.soiembarcode;
					string plantbarcodepath = objprint.plantbarcodepath;
					string spbarcode = objprint.storagebarcodepath;
					string linkagebarcodepath = objprint.linkagebarcodepath;
					var results = DB.ExecuteScalar(insertqueryforinvoice, new
					{
						objprint.pono,
						printMat.inwardid,
						noofprints,
						qtyinbox,
						isprint,
						objprint.totalboxes,
						objprint.boxno,
						isonholdgr,
						matbarcodepath,
						soitembcpath,
						plantbarcodepath,
						spbarcode,
						linkagebarcodepath,
						printMat.receivedqty,
						objprint.itemno,
						objprint.saleorderno,
						objprint.solineitemno,
						printMat.printerid
					});

				}
			}
			catch (Exception Ex)
			{
				printMat.errorMsg = Ex.Message;
				log.ErrorMessage("PODataProvider", "generateBarcodeMaterial", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
			}
			return objprint;
		}

		/*
       Name of Function : <<generateBarcodeMatonhold>>  Author :<<Gayathri>>  
       Date of Creation <<12-12-2019>>
       Purpose : <<Generate barcode and qrcode label required for Material label and get the get required to display on the material label>>
       <param name="printMat"></param>
       Review Date :<<>>   Reviewed By :<<>>
       */
		public printMaterial generateBarcodeMatonhold(printMaterial printMat)
		{
			printMaterial objprint = new printMaterial();
			try
			{
				PrintUtilities objptutlities = new PrintUtilities();
				string path = "";

				path = Environment.CurrentDirectory + @"\Barcodes\";

				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}

				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					//Check if the material is already printed
					string query = "Select * from wms.wms_securityinward sinw join wms.wms_printstatusmaterial psmat on psmat.inwmasterid=sinw.inwmasterid where sinw.pono='" + printMat.pono + "' and sinw.invoiceno='" + printMat.invoiceno + "' and psmat.materialid='" + printMat.materialid + "'";

					objprint = DB.QueryFirstOrDefault<printMaterial>(
						   query, null, commandType: CommandType.Text);



				}
				var content = printMat.grnno + "-" + printMat.materialid;
				printMat.barcodePath = "./Barcodes/" + content + ".bmp";
				printMat.materialcodePath = objptutlities.generatebarcode(printMat.barcodePath, content);

				////generate barcode for material code and GRN No.

				//BarcodeWriter writer = new BarcodeWriter
				//{
				//    Format = BarcodeFormat.QR_CODE,
				//    Options = new EncodingOptions
				//    {
				//        Height = 90,
				//        Width = 100,
				//        PureBarcode = false,
				//        Margin = 1,

				//    },
				//};
				//var bitmap = writer.Write(content);

				//// write text and generate a 2-D barcode as a bitmap
				//writer
				//    .Write(content)
				//    .Save(path + content + ".bmp");

				//printMat.barcodePath = "./Barcodes/" + content + ".bmp";

				//Barcode design for material code
				//generate barcode for material code and GRN No.

				content = printMat.materialid;
				printMat.materialcodePath = objptutlities.generateqrcode(printMat.barcodePath, content);

				//BarcodeWriter writerData = new BarcodeWriter
				//{
				//    Format = BarcodeFormat.QR_CODE,
				//    Options = new EncodingOptions
				//    {
				//        Height = 90,
				//        Width = 100,
				//        PureBarcode = false,
				//        Margin = 1,

				//    },
				//};

				//bitmap = writerData.Write(content);

				//// write text and generate a 2-D barcode as a bitmap
				//writer
				//    .Write(content)
				//    .Save(path + content + ".bmp");

				//printMat.materialcodePath = "./Barcodes/" + content + ".bmp";



			}
			catch (Exception Ex)
			{
				printMat.errorMsg = Ex.Message;
				log.ErrorMessage("PODataProvider", "generateBarcodeMaterial", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
			}
			return objprint;
		}


		/*
		Name of Function : <<printBarcodeMaterial>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Printing barcode- printBarcodeMaterial>>
		<param name="printMat"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public string printBarcodeMaterial(printMaterial printMat)
		{
			try
			{
				string path = Environment.CurrentDirectory + @"\PRNFiles\";
				bool result = false;
				string printResult = null;
				path = path + printMat.materialid + "-" + string.Format("{0:ddMMyyyyhhmm}", DateTime.Now) + ".prn";
				FileMode fileType = FileMode.OpenOrCreate;
				//for (int i = 0; i < printQty; i++)
				//{
				// if (File.Exists(path))
				if (Directory.Exists(path))
				{
					fileType = FileMode.Append;
				}

				using (FileStream fs = new FileStream(path, fileType))
				{
					using (TextWriter tw = new StreamWriter(fs))
					{
						tw.WriteLine("<xpml><page quantity='0' pitch='33.0 mm'></xpml>SIZE 94.10 mm, 38 mm");
						tw.WriteLine("GAP 3 mm, 0 mm");
						//tw.WriteLine("SET RIBBON ON");
						tw.WriteLine("DIRECTION 0,0");
						tw.WriteLine("REFERENCE 0,0");
						tw.WriteLine("OFFSET 0 mm");
						tw.WriteLine("SET PEEL OFF");
						tw.WriteLine("SET CUTTER OFF");
						tw.WriteLine("SET PARTIAL_CUTTER OFF");
						tw.WriteLine("<xpml></page></xpml><xpml><page quantity='1' pitch='33.0 mm'></xpml>SET TEAR ON");
						tw.WriteLine("ON");
						tw.WriteLine("CLS");
						tw.WriteLine("BOX 9,15,741,289,3");
						tw.WriteLine("BAR 492,15, 3, 272");
						tw.WriteLine("BAR 214,15, 3, 272");
						tw.WriteLine("BAR 215,222, 525, 3");
						tw.WriteLine("BAR 9,151, 731, 3");
						tw.WriteLine("BAR 215,86, 525, 3");
						tw.WriteLine("QRCODE 144,251,L,3,A,180,M2,S7,\"" + printMat.materialid + "\"");
						tw.WriteLine("QRCODE 144,106,L,3,A,180,M2,S7,\"" + printMat.grnno + "-" + printMat.materialid + "\"");
						tw.WriteLine("CODEPAGE 1252");
						tw.WriteLine("TEXT 731,268,\"0\",180,9,9,\"Material Code: \"");
						tw.WriteLine("TEXT 731,195,\"0\",180,8,9,\"Received Date: \"");
						tw.WriteLine("TEXT 732,124,\"0\",180,6,6,\"WMS GRN No. - Material Code: \"");
						tw.WriteLine("TEXT 704,56,\"0\",180,9,9,\"Quantity\"");
						tw.WriteLine("TEXT 482,265,\"0\",180,14,9,\"" + printMat.grnno + "\"");
						tw.WriteLine("TEXT 484,124,\"0\",180,9,6,\"" + printMat.grnno + "-" + printMat.materialid + "\"");
						tw.WriteLine("TEXT 486,59,\"0\",180,13,9,\"" + printMat.noofprint + "/" + printMat.noofprint + "\"");
						tw.WriteLine("TEXT 485,199,\"0\",180,13,11,\"" + printMat.receiveddate + "\"");

						tw.WriteLine("PRINT 1,1");
						tw.WriteLine("<xpml></page></xpml><xpml><end/></xpml>");
					}


				}
				//}
				try
				{
					//Convert.ToString(ConfigurationManager.AppSettings["PrinterName"].ToString());
					//string printerName = ConfigurationManager.AppSettings["CTMajor_AdminPrinter"].ToString();
					//string printerName = "10.29.11.25";
					string printerName = "10.29.2.48";
					PrintUtilities objIdentification = new PrintUtilities();
					printResult = "success";
					printResult = objIdentification.PrintQRCode(path, printerName);
					// path =  @"D:\Transmitter\ECheckSheetAPI\ECheckSheetAPI\print\";
					// result = RawPrinterHelper.SendFileToPrinter(printerName, path);



				}

				catch (Exception Ex)
				{
					throw Ex;
				}

				if (printResult == "success")
				{
					//update count wms_reprinthistory table            
					//this._poService.updateSecurityPrintHistory(model);
					return "success";
				}
				else
				{
					return "Error Occured";
				}



			}
			catch (Exception Ex)
			{
				printMat.errorMsg = Ex.Message;
				log.ErrorMessage("PODataProvider", "generateBarcodeMaterial", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
			}
			return "success";
		}

		/*
		Name of Function : <<InsertBarcodeInfo>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Inserting barcode info>>
		<param name="dataobj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public PrintHistoryModel InsertBarcodeInfo(BarcodeModel dataobj)
		{
			PrintHistoryModel objreprint = new PrintHistoryModel();
			try
			{

				//dataobj.docfile = ;
				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					if (dataobj.pono == null)
					{
						//If PO is empty
						foreach (var podata in dataobj.polist)
						{
							if (dataobj.pono == null)
							{
								dataobj.pono = podata.POno;
							}
							else
							{
								dataobj.pono = dataobj.pono + "," + podata.POno;
							}
						}

					}
					var q1 = WMSResource.getinvoiceexists.Replace("#pono", dataobj.pono).Replace("#invno", dataobj.invoiceno);
					int count = int.Parse(DB.ExecuteScalar(q1, null).ToString());

					if (count >= 1)
					{
						var q2 = WMSResource.getprintdetailsforinvoice.Replace("#pono", dataobj.pono).Replace("#invno", dataobj.invoiceno);
						BarcodeModel data = DB.QueryFirstOrDefault<BarcodeModel>(
							q2, null, commandType: CommandType.Text);
						objreprint.inwmasterid = data.inwmasterid;
						objreprint.pono = data.pono;
						objreprint.gateentrytime = data.receiveddate;
						objreprint.vehicleno = data.vehicleno;
						objreprint.transporterdetails = data.transporterdetails;
						if (data.print == true)
						{
							objreprint.result = "3";
							return objreprint; //for invoice already exist and if data is printed
						}
						else
						{
							objreprint.result = "2"; //for invoice already exist
							return objreprint;

						}


					}
					else
					{
						dataobj.createddate = System.DateTime.Now;
						var result = "0";
						string insertqueryforinvoice = WMSResource.insertinvoicedata;
						dataobj.receiveddate = System.DateTime.Now;
						if (dataobj.pono == "NONPO")
						{
							string compare = "NP" + DateTime.Now.Year.ToString().Substring(2);
							string Query = WMSResource.getmaxnonpo;
							var data = DB.QueryFirstOrDefault<POList>(
							Query, null, commandType: CommandType.Text);
							if (data.POno != null)
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
									objreprint.result = "2"; //for invoice already exist
									return objreprint;

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
									objreprint.result = "2"; //for invoice already exist
									return objreprint;
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
								catch (Exception Ex)
								{
									string msg = Ex.Message;
								}

							}


						}
						dataobj.invoicedate = DateTime.Now;

						string filename = "";
						if (dataobj.pono.StartsWith("NP"))
						{
							filename = dataobj.pono + "_" + dataobj.docfile;
						}

						var results = DB.ExecuteScalar(insertqueryforinvoice, new
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
							dataobj.grnnumber,
							dataobj.createdby,
							filename,
							dataobj.inwmasterid,
							dataobj.vehicleno,
							dataobj.transporterdetails
							//barcodeid,
						});

						if (results != null && results != "")
						{
							dataobj.inwmasterid = results.ToString();
							dataobj.barcode = dataobj.inwmasterid;
							//insert bar code data
							dataobj.createddate = DateTime.Now;
							string insertbarcodequery = WMSResource.insertbarcodedata;//to insert bar code data
							var barcodeResult = DB.Execute(insertbarcodequery, new
							{
								dataobj.barcode,
								dataobj.createdby,
								dataobj.createddate,
								dataobj.deleteflag,
								dataobj.pono,
								dataobj.invoiceno,
								dataobj.inwmasterid

							});

							EmailModel emailmodel = new EmailModel();
							emailmodel.pono = dataobj.pono;
							emailmodel.invoiceno = dataobj.invoiceno;
							emailmodel.receivedby = dataobj.receivedby;
							emailmodel.receiveddate = dataobj.receiveddate;
							emailmodel.asnno = dataobj.asnno;
							emailmodel.suppliername = dataobj.suppliername;
							emailmodel.grnnumber = dataobj.grnnumber;
							emailmodel.employeeno = dataobj.receivedby;
							emailmodel.inwmasterid = dataobj.inwmasterid;
							EmailUtilities emailobj = new EmailUtilities();
							emailobj.sendEmail(emailmodel, 1, 3);

						}


						////}
						//Adding the required data to reprint model

						objreprint.inwmasterid = dataobj.inwmasterid;
						objreprint.pono = dataobj.pono;
						objreprint.gateentrytime = dataobj.createddate;
						objreprint.vehicleno = dataobj.vehicleno;
						objreprint.transporterdetails = dataobj.transporterdetails;
						return (objreprint);
					}

				}
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "InsertBarcodeInfo", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				string errorstring = Ex.Message;
				if (errorstring.Contains("duplicate key"))
				{
					objreprint.result = "2"; //for invoice already exist
					return objreprint;
				}
				else
				{
					objreprint.result = "Error:" + Ex.Message;
					return objreprint;

				}

			}

		}

		/*
		Name of Function : <<getinvoiveforpo>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get Invoice details based on PONO.>>
		<param name="PONO"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "getinvoiveforpo", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}

		/*
		Name of Function : <<getMaterialDetails>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get Material details based on grn number>>
		<param name="grnNo"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<MaterialDetails>> getMaterialDetails(string grnNo, string pono)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					//List<MaterialDetails> objMaterial = new List<MaterialDetails>();
					MaterialDetails result = null;
					//string getMatQuery = WMSResource.getmatforgrnno.Replace("#grn", grnNo);
					string getMatQuery = WMSResource.getmateriallistforTracking.Replace("#grn", grnNo).Replace("#pono", pono);
					var MaterialList = await pgsql.QueryAsync<MaterialDetails>(
					   getMatQuery, null, commandType: CommandType.Text);

					//if (MaterialList != null)
					//{
					//    int totalissed = 0;
					//    foreach (MaterialDetails mtData in MaterialList)
					//    {
					//        totalissed = Convert.ToInt32(mtData.issuedqty) + Convert.ToInt32(mtData.reservequantity);
					//        mtData.issued = totalissed;
					//        //if (mtData.materialid != null && mtData.materialid != "")
					//        //{
					//        //    result = new MaterialDetails();
					//        //    result.materialid = mtData.materialid;
					//        //    result.materialdescription = mtData.materialdescription;
					//        //    //result.availableqty = mtData.availableqty;
					//        //    result.grnnumber = mtData.grnnumber;
					//        //    result.confirmqty = mtData.confirmqty;
					//        //    //To get issued qty get data from material issue, material reserve and gatepassmaterial table
					//        //    int issuedqty = 0;
					//        //    int reservedqty = 0;

					//        //    Enquirydata enquiryobj = new Enquirydata();
					//        //    string availableQtyqry = "select sum(availableqty) as availableqty from wms.wms_stock where materialid ='" + mtData.materialid + "' and pono='" + mtData.pono + "' and inwmasterid = '" + mtData.inwmasterid + "'";
					//        //    enquiryobj = pgsql.QuerySingleOrDefault<Enquirydata>(
					//        //                    availableQtyqry, null, commandType: CommandType.Text);
					//        //    result.availableqty = enquiryobj.availableqty;

					//        //    ReportModel modelobj = new ReportModel();
					//        //    string matIssuedQuery = "select sum(iss.issuedqty)as issuedqty from wms.wms_materialissue iss" +
					//        //        " join wms.wms_stock sk on sk.pono='" + mtData.pono + "' where iss.itemid = sk.itemid and sk.materialid='" + mtData.materialid + "' and sk.inwmasterid= '" + mtData.inwmasterid + "'";
					//        //    modelobj = pgsql.QuerySingleOrDefault<ReportModel>(
					//        //                    matIssuedQuery, null, commandType: CommandType.Text);
					//        //    if (modelobj != null)
					//        //    {
					//        //        issuedqty = modelobj.issuedqty;
					//        //    }
					//        //    //Get material reserved qty
					//        //    ReserveMaterialModel modeldataobj = new ReserveMaterialModel();
					//        //    string matReserveQuery = "select sum(reser.reservequantity )as reservedqty from wms.materialreservedetails reser join wms.wms_stock sk on sk.pono='" + mtData.pono + "' where reser.itemid =sk.itemid and sk.materialid='" + mtData.materialid + "' and sk.inwmasterid='" + mtData.inwmasterid + "'";
					//        //    modeldataobj = pgsql.QuerySingleOrDefault<ReserveMaterialModel>(
					//        //                    matReserveQuery, null, commandType: CommandType.Text);
					//        //    if (modeldataobj != null)
					//        //    {
					//        //        reservedqty = modeldataobj.reservedqty;
					//        //    }
					//        //    int gatepassissuedqty = 0;
					//        //    totalissed = Convert.ToInt32(issuedqty) + Convert.ToInt32(reservedqty);
					//        //    result.issued = totalissed;


					//        //}
					//        //objMaterial.Add(result);
					//    }
					//    //objMaterial.Add(result);
					//}





					//return objMaterial;
					return MaterialList;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getMaterialDetails", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}

		/*
		Name of Function : <<getlocationdetails>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get location details based on material id>>
		<param name="materialid"></param>
		 <param name="grnnumber"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "getlocationdetails", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}


		/*
      Name of Function : <<getReqMatdetails>>  Author :<<Ramesh>>  
      Date of Creation <<12-12-2019>>
      Purpose : <<Get material requessted, acknowledged and issued details>>
      <param name="materialid"></param>
      <param name="grnnumber"></param>
      <param name="pono"></param>
      Review Date :<<>>   Reviewed By :<<>>
      */


		public async Task<IEnumerable<ReqMatDetails>> getReqMatdetails(string materialid, string grnnumber, string pono)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string query = WMSResource.materialTrackingissedDetail.Replace("#material", materialid).Replace("#grno", grnnumber).Replace("#pono", pono);
					await pgsql.OpenAsync();
					var obj = await pgsql.QueryAsync<ReqMatDetails>(
					   query, null, commandType: CommandType.Text);
					return obj;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getReqMatdetails", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();

			}

		}


		/*
    Name of Function : <<getReserveMatdetails>>  Author :<<Ramesh>>  
    Date of Creation <<29-11-2019>>
    Purpose : <<Get material reserve detail for material tracking>>
    <param name="materialid"></param>
     <param name="grnnumber"></param>
    <param name="pono"></param>
    Review Date :<<>>   Reviewed By :<<>>
    */


		public async Task<IEnumerable<ReqMatDetails>> getReserveMatdetails(string materialid, string grnnumber, string pono)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string query = WMSResource.materialtrackingReservedlist.Replace("#material", materialid).Replace("#grno", grnnumber).Replace("#pono", pono);
					await pgsql.OpenAsync();
					var obj = await pgsql.QueryAsync<ReqMatDetails>(
					   query, null, commandType: CommandType.Text);
					return obj;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getReserveMatdetails", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();

			}

		}

		/*
		Name of Function : <<getReqMatdetails>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get material requested, acknowledged and issued details>>
		<param name="materialid"></param>
		 <param name="grnnumber"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ReqMatDetails>> getReqMatdetails_old(string materialid, string grnnumber)
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
					foreach (ReqMatDetails matdata in obj)
					{
						string reservequery = "select max(emp.name) as requestername,max(emp1.name)  as approvedby,max(res.reservedon) as issuedon,sum(resdetails.reservequantity) as quantity from wms.materialreservedetails resdetails join wms.materialreserve res on resdetails.reserveid = res.reserveid join wms.wms_stock sk on sk.pono='" + matdata.pono + "' join wms.employee emp on res.reservedby = emp.employeeno join wms.employee emp1 on emp1.employeeno = res.reservedby where resdetails.itemid =sk.itemid and sk.materialid='" + materialid + "' and sk.inwmasterid='" + matdata.inwmasterid + "'";
						//"select max(emp.name) as requestername,max(emp1.name)  as approvedby,max(res.reservedon) as issuedon,sum(res.reservedqty) as quantity from wms.wms_materialreserve res left join wms.employee emp on res.reservedby = emp.employeeno join wms.wms_stock sk on sk.pono='"+ matdata.pono+ "'  left join wms.employee emp1 on emp1.employeeno = res.releasedby where res.itemid=sk.itemid and sk.materialid='"+ materialid + "' and sk.inwmasterid="+matdata.inwmasterid;
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

						string requstedquery = "select sum(issue.issuedqty)as quantity,max(emp.name) as requestername,max(emp1.name) as approvername,max(issue.itemissueddate) as issuedon from wms.wms_materialissue issue inner join wms.materialrequestdetails matreqdetails on matreqdetails.id = issue.requestmaterialid inner join wms.materialrequest req on matreqdetails.requestid = req.requestid  left join wms.employee emp on emp.employeeno = req.requesterid join wms.wms_stock sk on sk.pono='" + matdata.pono + "' left join wms.employee emp1 on emp1.employeeno = req.approverid  where issue.itemid=sk.itemid and sk.materialid='" + materialid + "' and sk.inwmasterid='" + matdata.inwmasterid + "'";
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
						string gatepassquery = "select gp.gatepasstype as type,sum(matiss.issuedqty) as quantity,max(gp.approvedon) as issuedon,max(emp.name) as requestername, max(emp1.name) as approvername from wms.wms_gatepassmaterial gtmat join wms.wms_materialissue matiss on matiss.gatepassmaterialid = gtmat.gatepassmaterialid join wms.wms_gatepass gp on gp.gatepassid = gtmat.gatepassid left join wms.employee emp on emp.employeeno = gp.requestedby left join wms.employee emp1 on emp1.employeeno = gp.approverid join wms.wms_stock sk on sk.pono = '" + matdata.pono + "' where sk.inwmasterid = '" + matdata.inwmasterid + "' and matiss.itemid = sk.itemid and gtmat.materialid = '" + materialid + "' group by gp.gatepasstype";
						//"select max(gate.gatepasstype)as gatepasstype,sum(wmissue.issuedqty)as quantity,max(gate.approvedon) as issuedon,max(emp.name) as requestername,max(emp1.name) as approvername from wms.wms_gatepass gate  inner join wms.wms_gatepassmaterial mat on mat.gatepassid = gate.gatepassid  left join wms.employee emp on emp.employeeno = gate.requestedby left join wms.employee emp1 on emp1.employeeno = gate.approvedby join wms.wms_materialissue wmissue  on mat.gatepassmaterialid = wmissue.gatepassmaterialid where mat.materialid = '" + matdata.materialid + "' and gate.approvedon != null and gate.approverstatus != null";
						var data2 = await pgsql.QueryAsync<ReqMatDetails>(
					   gatepassquery, null, commandType: CommandType.Text);

						if (data2 != null)
						{
							foreach (var gpdata in data2)
							{
								if (gpdata.quantity != 0)
								{
									objs = new ReqMatDetails();
									objs.quantity = gpdata.quantity;
									objs.type = gpdata.type;
									objs.requestername = gpdata.requestername;
									objs.issuedon = gpdata.issuedon;
									objs.details = gpdata.jobname;
									objs.approvername = gpdata.approvername;
									objs.acknowledge = gpdata.requestername;
									listobj.Add(objs);
								}
							}
						}


					}
					return listobj;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getReqMatdetails", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}


		/*
		Name of Function : <<GetDeatilsForholdgr>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get details of Hold GR based on status>>
		<param name="status"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "GetDeatilsForholdgr", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}


		/*
		Name of Function : <<GetDeatilsForthreeWaymatching>>  Author :<<LP>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get list of info for three way matching>>
		<param name="invoiceno"></param>
		 <param name="pono"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public async Task<IEnumerable<OpenPoModel>> GetDeatilsForthreeWaymatching(string invoiceno, string inwmasterid, bool isgrn, string grnno)
		{


			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					inwardModel obj = new inwardModel();
					await pgsql.OpenAsync();
					if (!isgrn)
					{
						//string query1 = WMSResource.getinwmasterid.Replace("#pono", pono).Replace("#invoiceno", invoiceno);
						string query1 = WMSResource.getpobyinwardmasterid.Replace("#inw", inwmasterid);
						obj = pgsql.QuerySingle<inwardModel>(
						query1, null, commandType: CommandType.Text);
					}
					else
					{
						//string query1 = WMSResource.getinwardidbygrn.Replace("#grnno", grnno);
						string query1 = WMSResource.getponobygrn.Replace("#grn", grnno);
						obj = pgsql.QuerySingle<inwardModel>(
						query1, null, commandType: CommandType.Text);
					}

					string pono = obj.pono;
					List<OpenPoModel> datalist = new List<OpenPoModel>();
					if (obj.pono != null && obj.pono != "")
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
								query += " where sinw.pono = '" + pono + "' and inw.materialid is NOT NULL  and sinw.invoiceno = '" + invoiceno + "' and (sinw.holdgrstatus is NULL or sinw.holdgrstatus =  'accepted' or sinw.holdgrstatus = 'hold')";
							}

						}
						else
						{
							inwardModel objx = new inwardModel();
							if (!isgrn)
							{
								//string queryx = "select grnnumber,onhold,unholdedby from wms.wms_securityinward where pono = '" + pono + "' and invoiceno = '" + invoiceno + "'";
								//string queryx = WMSResource.isgrnexistsquerybyinvoce.Replace("#pono", pono).Replace("#invno", invoiceno);
								string queryx = WMSResource.isgrnexistbyinwardmasterid.Replace("#inw", inwmasterid);
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
							string poforquery = pono;
							poforquery = poforquery.Trim();
							poforquery = poforquery.Replace(" ", "");
							poforquery = poforquery.Replace(",", "','");

							if ((objx.grnnumber == null || objx.grnnumber == "") && !objx.onhold && (objx.unholdedby == null || objx.unholdedby == ""))
							{


								//query = WMSResource.Getdetailsforthreewaymatching;
								query = WMSResource.getMaterialsforreceipt.Replace("#invoice", invoiceno).Replace("#inw", inwmasterid);
								if (isgrn)
								{
									query += " where sinw.grnnumber = '" + grnno + "'";
									if (obj.asnno != null && obj.asnno != "")
									{
										query += " and  mat.asnno = '" + obj.asnno + "'";
									}
								}
								else
								{
									//query += " where mat.pono = '" + pono + "'  and sinw.invoiceno = '" + invoiceno + "'";
									query += " where mat.pono in ('" + poforquery + "')";
									if (obj.asnno != null && obj.asnno != "")
									{
										query += " and mat.asnno = '" + obj.asnno + "'";
									}
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
									query += " where inw.pono in ('" + poforquery + "')  and sinw.inwmasterid = '" + inwmasterid + "' and (sinw.holdgrstatus is NULL or sinw.holdgrstatus =  'accepted' or sinw.holdgrstatus = 'hold')";
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
								po.inwmasterid = inwmasterid;
								po.invoiceno = invoiceno;
								datalist.Add(po);
								data = datalist;
							}
						}
						if (data.Count() > 0)
						{

							foreach (OpenPoModel po in data)
							{
								if (obj.asnno != null && obj.asnno != "")
								{
									po.isasn = true;
								}
								else
								{
									po.isasn = false;
								}
								string descriptionstr = null;
								if (po.poitemdescription != null)
								{
									descriptionstr = po.poitemdescription.Replace("\'", "''");
								}
								var fdata = datalist.Where(o => o.Material == po.Material && o.poitemdescription == descriptionstr && o.pono == po.pono && o.lineitemno == po.lineitemno && o.asnno == po.asnno).FirstOrDefault();
								if (fdata == null)
								{
									string querya = "select inw.pono,inw.materialid,Max(inw.materialqty) as materialqty,SUM(inw.confirmqty) as confirmqty,SUM(inw.receivedqty) as receivedqty from wms.wms_storeinward inw";
									querya += " where inw.pono = '" + po.pono + "' and inw.materialid = '" + po.Material + "'";
									if (!pono.StartsWith("NP"))
									{
										querya += " and inw.lineitemno = '" + po.lineitemno + "'";
									}
									querya += " group by inw.pono,inw.materialid";
									var datax = await pgsql.QueryAsync<OpenPoModel>(
									querya, null, commandType: CommandType.Text);
									if (datax.Count() > 0)
									{
										po.isreceivedpreviosly = true;
										decimal? pendingqty = 0;
										if (datax.FirstOrDefault().confirmqty > 0)
										{
											pendingqty = po.pendingqty - datax.FirstOrDefault().confirmqty;
										}
										else
										{
											pendingqty = po.pendingqty - datax.FirstOrDefault().receivedqty;
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
									else
									{
										po.pendingqty = po.materialqty;

									}
									datalist.Add(po);
								}

							}
						}
					}


					return datalist.OrderBy(o => o.Material);
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetDeatilsForthreeWaymatching", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}

		/*
		Name of Function : <<Getqualitydetails>>  Author :<<Ramesh kumar>>  
		Date of Creation <<07/07/2020>>
		Purpose : <<get list of info for quality check>>
		<param name="invoiceno"></param>
		 <param name="pono"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "Getqualitydetails", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}

		/*
		Name of Function : <<VerifythreeWay>>  Author :<<LP>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<To verify three way match and generate GRN No>>
		<param name="invoiceno"></param>
		 <param name="pono"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<OpenPoModel> VerifythreeWay(string inwmasterid, string invoiceno, string type)
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
					string query = "select grnnumber, pono, receivedby from wms.wms_securityinward where inwmasterid = '" + inwmasterid + "'";

					//if (pono.StartsWith("NP"))
					//{
					//	query = "select grnnumber,pono from wms.wms_securityinward where invoiceno = '" + invoiceno + "' and pono = '" + pono + "' group by grnnumber,pono";

					//}
					//else
					//{
					//	query = WMSResource.Verifythreewaymatch.Replace("#pono", pono).Replace("#invoiceno", invoiceno);

					//}

					info = pgsql.QuerySingle<iwardmasterModel>(
					  query, null, commandType: CommandType.Text);
					if (info != null && info.grnnumber == null)
					{
						int grnnextsequence = 0;
						string grnnumber = string.Empty;
						obj = pgsql.QuerySingle<sequencModel>(
					   lastinsertedgrn, null, commandType: CommandType.Text);
						if (obj.id != 0)
						{
							grnnextsequence = (Convert.ToInt32(obj.sequencenumber) + 1);
							grnnumber = obj.sequenceid + "-" + obj.year + "-" + grnnextsequence.ToString().PadLeft(6, '0');
							string updategrnnumber = WMSResource.updategrnnumber.Replace("#inw", inwmasterid);
							using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
							{
								var results = DB.ExecuteScalar(updategrnnumber, new
								{
									grnnumber,

								});
								verify.grnnumber = grnnumber;
								int id = obj.id;
								string updateseqnumber = WMSResource.updateseqnumber;

								var results1 = DB.ExecuteScalar(updateseqnumber, new
								{
									grnnextsequence,
									id,

								});
								string wmsgr = grnnumber;
								string updatedby = info.receivedby;
								string insertspgr = WMSResource.insertsapGR;

								var data1 = DB.ExecuteScalar(insertspgr, new
								{
									wmsgr,
									updatedby

								});
							}

						}




						else
						{

						}
						if (info.pono.Contains(","))
						{
							string[] pos = info.pono.Split(",");
							foreach (string str in pos)
							{
								string pono = str.Trim();
								string insertqueryforstatus = WMSResource.statusupdatebySecurity;
								using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
								{
									var results = DB.ExecuteScalar(insertqueryforstatus, new
									{
										pono

									});
								}

							}

						}
						else
						{
							string insertqueryforstatus = WMSResource.statusupdatebySecurity;
							using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
							{
								var results = DB.ExecuteScalar(insertqueryforstatus, new
								{
									info.pono

								});
							}

						}

						//}
					}

					else
					{
						verify.grnnumber = info.grnnumber;
					}
					//if (inwardid != 0)

					if (type == "1")
					{
						EmailModel emailmodel = new EmailModel();
						emailmodel.grnnumber = verify.grnnumber;
						EmailUtilities emailobj = new EmailUtilities();
						emailobj.sendEmail(emailmodel, 2, 9);
					}
					else if (type == "2")
					{
						EmailModel emailmodel = new EmailModel();
						emailmodel.grnnumber = verify.grnnumber;
						EmailUtilities emailobj = new EmailUtilities();
						emailobj.sendEmail(emailmodel, 20, 3);
					}
					else if (type == "3")
					{
						EmailModel emailmodel = new EmailModel();
						emailmodel.grnnumber = verify.grnnumber;
						EmailUtilities emailobj = new EmailUtilities();
						emailobj.sendEmail(emailmodel, 2, 9);

						EmailModel emailmodel1 = new EmailModel();
						emailmodel1.grnnumber = verify.grnnumber;
						EmailUtilities emailobj1 = new EmailUtilities();
						emailobj1.sendEmail(emailmodel1, 20, 3);
					}



					return verify;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "VerifythreeWay", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}

		/*
		Name of Function : <<receivequantity>>  Author :<<Ramesh kumar>>  
		Date of Creation <<07/07/2020>>
		Purpose : <<to receive material>>
		<param name="datamodel"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public async Task<string> receivequantity(List<inwardModel> datamodel)
		{

			//inwardModel obj = new inwardModel();
			inwardModel getgrnnoforpo = new inwardModel();

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				NpgsqlTransaction Trans = null;

				try
				{

					datamodel[0].receiveddate = System.DateTime.Now;
					await pgsql.OpenAsync();
					Trans = pgsql.BeginTransaction();

					//string query = WMSResource.getinwmasterid.Replace("#pono", datamodel[0].pono).Replace("#invoiceno", datamodel[0].invoiceno);
					//obj = pgsql.QuerySingle<inwardModel>(
					// query, null, commandType: CommandType.Text);
					//string getGRNno = WMSResource.getGRNNo.Replace("#pono", datamodel[0].pono);
					//getgrnnoforpo = pgsql.QueryFirstOrDefault<inwardModel>(
					//   getGRNno, null, commandType: CommandType.Text);
					int inwardid = 0;
					if (datamodel != null && datamodel.Count() > 0)
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
							decimal? materialqty = item.pendingqty;

							if (!isupdateprocess)
							{

								var results = pgsql.ExecuteScalar(insertforinvoicequery, new
								{
									item.inwmasterid,
									item.receiveddate,
									item.receivedby,
									item.receivedqty,
									materialid,
									item.deleteflag,
									item.qualitycheck,
									qualitychecked,
									materialqty,
									item.receiveremarks,
									item.pono,
									item.lineitemno,
									item.poitemdescription,
									item.unitprice,
									item.saleorderno,
									item.solineitemno,
									item.projectid


								});
								inwardid = Convert.ToInt32(results);

								if (inwardid != 0 && loop == 0)
								{
									if (item.onhold)
									{
										string qry = "Update wms.wms_securityinward set onhold = " + item.onhold + ",onholdremarks = '" + item.onholdremarks + "',holdgrstatus='hold' where inwmasterid = '" + item.inwmasterid + "'";
										var results11 = pgsql.ExecuteScalar(qry);
									}

								}
								loop++;

							}
							else
							{
								string qrry = WMSResource.updatereceiptunhold.Replace("#inwardid", item.inwardid.ToString());
								var results = pgsql.ExecuteScalar(qrry, new
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
										string qry = "Update wms.wms_securityinward set onhold = " + item.onhold + ",onholdremarks = '" + item.onholdremarks + "',holdgrstatus='hold' where inwmasterid = '" + item.inwmasterid + "'";
										var results11 = pgsql.ExecuteScalar(qry);
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
						Trans.Commit();
					}

					return "Saved" + (Convert.ToString(inwardid));
				}
				catch (Exception Ex)
				{
					Trans.Rollback();
					log.ErrorMessage("PODataProvider", "insertquantity", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return Ex.Message;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}


		/*
		Name of Function : <<insertquantity>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Insert data into store Inward >>
		<param name="datamodel"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					if (obj.inwmasterid != null && obj.inwmasterid != "")
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
					log.ErrorMessage("PODataProvider", "insertquantity", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}

		/*
		Name of Function : <<InsertStock>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<inserting material details to warehouse>>
		<param name="data"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string InsertStock(List<StockModel> data)
		{
			try
			{
				StockModel obj = new StockModel();
				string loactiontext = string.Empty;
				var result = 0;
				//int inwmasterid = 0;
				string inwmasterid = "";
				decimal? value = 0;
				decimal? unitprice = 0;
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

						//foreach (var item in data) { 
						item.createddate = System.DateTime.Now;

						//if (data.itemid == 0)
						//{


						//Get unit price and value from pomaterials table
						//string getprice = WMSResource.getpricedetails.Replace("#pono", item.pono).Replace("#material", item.Material);
						//var objdata = pgsql.QueryFirstOrDefault<pricedetails>(
						//	   getprice, null, commandType: CommandType.Text);
						// value = objdata.itemamount;
						// unitprice = objdata.unitprice;
					}

					string insertquery = WMSResource.insertStockFromPutaway;
					int itemid = 0;
					string materialid = item.Material;
					item.availableqty = item.confirmqty;
					value = item.confirmqty * item.unitprice;
					unitprice = item.unitprice;
					item.receivedtype = "Put Away";
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						result = Convert.ToInt32(DB.ExecuteScalar(insertquery, new
						{
							inwmasterid,
							item.pono,
							item.binid,
							item.rackid,
							item.storeid,
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
							item.inwardid,
							item.stocktype,
							item.lineitemno,
							item.receivedtype,
							item.poitemdescription,
							value,
							unitprice,
							item.projectid,
							item.saleorderno,
							item.solineitemno,
							item.initialputawayqty

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
				log.ErrorMessage("PODataProvider", "InsertStock", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return null;
			}


		}

		/*
		Name of Function : <<InsertStockIS>>  Author :<<Ramesh>>  
		Date of Creation <<15-01-2021>>
		Purpose : <<Put away initial Stock Materials>>
		<param name="data"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string InsertStockIS(initialStock data)
		{
			try
			{
				StockModel obj = new StockModel();
				string loactiontext = string.Empty;
				var result = 0;
				//int inwmasterid = 0;
				string inwmasterid = "";
				decimal? value = 0;
				decimal? unitprice = 0;
				using (IDbConnection pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				{








					//Add material master data
					string materialQuery = "Select material from wms.\"MaterialMasterYGS\" where material = '" + data.material + "'";
					var materialid = pgsql.ExecuteScalar(materialQuery, null);
					if (materialid == null)
					{
						LocationModel store = new LocationModel();
						store.materialid = data.material;
						store.materialdescription = data.materialdescription;
						store.isexcelupload = true;
						store.locatorid = Convert.ToInt32(data.locations[0].locatorid);
						store.rackid = Convert.ToInt32(data.locations[0].rackid);
						int? binid = null;
						bool qualitycheck = false;
						int? binId = data.locations[0].rackid;
						if (binId != null && binId > 0)
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
							data.stocktype,
							data.unitprice


						});
					}
				}
				//Add locator in masterdata


				foreach (var item in data.locations)
				{




					DateTime? createddate = System.DateTime.Now;
					string insertquery = WMSResource.inserttoStockIS;
					int itemid = 0;
					string materialid = data.material;
					decimal? availableqty = item.quantity;
					decimal? totalquantity = item.quantity;
					value = data.value;
					unitprice = data.value / item.quantity;
					string receivedtype = "Initial Stock";
					string pono = data.pono;
					int? storeid = item.locatorid;
					DateTime? shelflife = data.shelflifeexpiration;
					bool deleteflag = false;
					string itemlocation = item.locatorname + "." + item.racknumber;
					string createdby = data.createdby;
					string receivedid = data.stockid.ToString();
					string uploadbatchcode = data.uploadbatchcode;
					string uploadedfilename = data.uploadedfilename;
					string poitemdescription = data.materialdescription;
					if (item.binnumber != null && item.binnumber.ToString().Trim() != "")
					{
						itemlocation += "." + item.binnumber;

					}
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						result = Convert.ToInt32(DB.ExecuteScalar(insertquery, new
						{

							pono,
							item.binid,
							item.rackid,
							storeid,
							totalquantity,
							shelflife,
							availableqty,
							deleteflag,
							itemlocation,
							createddate,
							createdby,
							materialid,
							item.stocktype,
							receivedtype,
							poitemdescription,
							value,
							unitprice,
							receivedid,
							uploadbatchcode,
							uploadedfilename

						}));
						if (result != 0)
						{
							itemid = Convert.ToInt32(result);
							string insertqueryforlocationhistory = WMSResource.insertqueryforlocationhistory;
							var results = DB.ExecuteScalar(insertqueryforlocationhistory, new
							{
								itemlocation,
								itemid,
								createddate,
								createdby,

							});
							string insertqueryforstatuswarehouse = WMSResource.insertqueryforstatuswarehouse;

							var data1 = DB.ExecuteScalar(insertqueryforstatuswarehouse, new
							{
								pono,

							});


						}
					}


				}
				return "Location Updated";

			}
			catch (Exception Ex)
			{
				return Ex.Message;
				log.ErrorMessage("PODataProvider", "InsertStock", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return null;
			}


		}

		/*
		Name of Function : <<InsertmatSTO>>  Author :<<Gayathri>>  
		Date of Creation <<13-01-2021>>
		Purpose : <<inserting material details to warehouse>>
		<param name="data"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string InsertmatSTO(List<StockModel> data)
		{
			try
			{
				StockModel obj = new StockModel();
				string loactiontext = string.Empty;
				var result = 0;
				//int inwmasterid = 0;
				string inwmasterid = "";
				decimal? value = 0;
				decimal? unitprice = 0;
				foreach (var item in data)
				{




					string insertquery = "INSERT INTO wms.wms_stock(receivedtype,receivedid,stockstatus,pono,binid,rackid ,storeid, vendorid,totalquantity,shelflife,availableqty,";
					insertquery += "deleteflag,itemlocation,createddate,createdby,materialid,stcktype,lineitemno,poitemdescription,value,unitprice)";
					insertquery += "VALUES(@receivedtype,@receivedid,@stockstatus,@pono,@binid,@rackid,@storeid,@vendorid,@totalquantity,@shelflife,@availableqty,@deleteflag,";
					insertquery += "@itemlocation,@createddate,@createdby,@materialid,@stocktype,@lineitemno,@poitemdescription,@value,@unitprice)";
					insertquery += "returning itemid";
					int itemid = 0;
					string materialid = item.Material;
					item.availableqty = item.confirmqty;
					item.totalquantity = item.confirmqty;
					value = item.confirmqty * item.unitprice;
					unitprice = item.unitprice;
					item.receivedtype = "STO";
					item.createddate = DateTime.Now;
					string receivedid = Convert.ToString(item.id);
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						result = Convert.ToInt32(DB.ExecuteScalar(insertquery, new
						{
							item.receivedtype,
							receivedid,
							item.stockstatus,
							item.pono,
							item.binid,
							item.rackid,
							item.storeid,
							item.vendorid,
							item.totalquantity,
							item.shelflife,
							item.availableqty,
							item.deleteflag,
							item.itemlocation,
							item.createddate,
							item.createdby,
							materialid,
							item.stocktype,
							item.lineitemno,
							item.poitemdescription,
							value,
							unitprice

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
				log.ErrorMessage("PODataProvider", "InsertmatSTO", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return null;
			}


		}



		/*
		Name of Function : <<GetListItems>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<to get search data and pass  query dynamically>>
		<param name="Result"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public DataTable GetListItems(DynamicSearchResult Result)
		{


			try
			{
				using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				{
					pgsql.Open();
					DataTable dataTable = new DataTable();
					if (pgsql.State == System.Data.ConnectionState.Open)
					{

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
						dataTable = dataSet.Tables[0];
					}
					return dataTable;



				}
			}
			catch (Exception Ex)
			{
				//log.ErrorMessage("PODataProvider", "GetListItems", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return null;
			}

			//throw new NotImplementedException();


		}

		/*
		Name of Function : <<GetMaterialItems>>  Author :<<gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<GetMaterialItems>>
		<param name="Result"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					query = "select Distinct(sk.materialid),ygs.unitprice as materialcost from " + Result.tableName + WMSResource.getgatepassunitprice + Result.searchCondition + " limit 500";
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
					log.ErrorMessage("PODataProvider", "GetMaterialItems", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}

		}

		/*
		Name of Function : <<IssueRequest>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<material request by Project manager>>
		<param name="reqdata"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
				//requestid = obj.requestid + 1;
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
							item.projectcode,
							item.remarks
						});
					}



				}
				return (Convert.ToInt32(result));
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "IssueRequest", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return 0;
			}
		}

		/*
		Name of Function : <<getitemdeatils>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<based on grnnumber will get lst of items>>
		<param name="grnnumber"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<inwardModel>> getitemdeatils(string grnnumber)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					await pgsql.OpenAsync();
					string queryforitemdetails = WMSResource.queryforitemdetails.Replace("#grnnumber", grnnumber);
					var data = await pgsql.QueryAsync<inwardModel>(
					   queryforitemdetails, null, commandType: CommandType.Text);
					return data;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getitemdeatils", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}

		}

		/*
		Name of Function : <<getitemdeatilsnotif>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<based on grnnumber will get lst of items for notification>>
		<param name="grnnumber"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "getitemdeatils", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}

		}

		/*
		Name of Function : <<MaterialRequest_old>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<requesting for material>>
		<param name="pono"></param>
		 <param name="approverid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> MaterialRequest_old(string pono, string approverid)
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
					log.ErrorMessage("PODataProvider", "MaterialIssue", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<MaterialRequest>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<requesting for material>>
		<param name="approverid"></param>
		 <param name="pono"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<MaterialTransaction>> MaterialRequest(string pono, string approverid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				string materialrequestquery = WMSResource.getmaterialrequests.Replace("#reqid", approverid);

				try
				{
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<MaterialTransaction>(
					   materialrequestquery, null, commandType: CommandType.Text);
					foreach (MaterialTransaction trans in data)
					{
						trans.materialdata = new List<MaterialTransactionDetail>();
						string materialrequestdataquery = WMSResource.getmaterialrequestdata.Replace("#requestid", trans.requestid);
						var data1 = await pgsql.QueryAsync<MaterialTransactionDetail>(
						materialrequestdataquery, null, commandType: CommandType.Text);
						trans.materialdata = data1.ToList();

					}
					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "MaterialIssue", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}


		/*
		Name of Function : <<getuserauthdata>>  Author :<<Ramesh>>  
		Date of Creation <<25-01-2021>>
		Purpose : <<get role assigned users>>
		<param name=""></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<authUser>> getuserauthdata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				string query = WMSResource.getauthusers;

				try
				{
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<authUser>(
					   query, null, commandType: CommandType.Text);
					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getuserauthdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		/*
		Name of Function : <<getuserauthdetails>>  Author :<<Ramesh>>  
		Date of Creation <<25-01-2021>>
		Purpose : <<get role assigned user details>>
		<param name=""></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<authUser>> getuserauthdetails(string employeeid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				string query = WMSResource.getauthuserdetails.Replace("#empno", employeeid);

				try
				{
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<authUser>(
					   query, null, commandType: CommandType.Text);
					foreach (authUser user in data)
					{
						string querytr = WMSResource.getsubrolebyroleid.Replace("#rid", user.roleid.ToString());
						var trdata = await pgsql.QueryAsync<subrolemodel>(
					   querytr, null, commandType: CommandType.Text);
						if (trdata.Count() > 0)
						{
							user.subrolelist = trdata.ToList();
						}
						if (user.subroleid != null && user.subroleid.Trim() != "")
						{
							string querytr1 = WMSResource.getsubrolebysubroleid.Replace("#subroles", user.subroleid);
							var trdata1 = await pgsql.QueryAsync<subrolemodel>(
						   querytr1, null, commandType: CommandType.Text);
							if (trdata1.Count() > 0)
							{
								user.selectedsubrolelist = trdata1.ToList();
							}
						}
					}
					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getuserauthdetails", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getuserauthdetailsbyrole>>  Author :<<Ramesh>>  
		Date of Creation <<25-01-2021>>
		Purpose : <<get usrs assigned by role>>
		<param name=""></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<authUser>> getuserauthdetailsbyrole(int roleid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				string query = WMSResource.getauthuserdetailsbyrole.Replace("#roleid", roleid.ToString());

				try
				{
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<authUser>(
					   query, null, commandType: CommandType.Text);
					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getuserauthdetailsbyrole", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getsubroledata>>  Author :<<Ramesh>>  
		Date of Creation <<25-01-2021>>
		Purpose : <<get sub role details>>
		<param name=""></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<subrolemodel>> getsubroledata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				string query = WMSResource.getsubroledata;

				try
				{
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<subrolemodel>(
					   query, null, commandType: CommandType.Text);
					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getsubroledata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<acknowledgeMaterialReceived>>  Author :<<Amulya>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<acknowledge for received item from Project manager>>
		<param name="dataobj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					string requestid = item.requestid;
					string ackremarks = item.ackremarks;
					decimal? issuedquantity = item.issuedquantity;
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
						emailmodel.requestid = item.requestid;
						EmailUtilities emailobj = new EmailUtilities();
						emailobj.sendEmail(emailmodel, 6, 3);
					}
				}
				return (Convert.ToInt32(result));
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "acknowledgeMaterialReceived", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
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
			//    log.ErrorMessage("PODataProvider", "acknowledgeMaterialReceived", Ex.StackTrace.ToString(), Ex.Message.ToString(),url);
			//    return 0;
			//}

		}

		/*
		Name of Function : <<GetMaterialissueList>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get material issue list>>
		<param name="requesterid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

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
					log.ErrorMessage("PODataProvider", "GetMaterialList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetMaterialissueListforapprover_old>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get list of material details based on approver id>>
		<param name="approverid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> GetMaterialissueListforapprover_old(string approverid)
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
					log.ErrorMessage("PODataProvider", "GetRequestList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetMaterialissueListforapprover>>  Author :<<LP>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get list of material details based on approver id>>
		<param name="approverid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> GetMaterialissueListforapprover(string approverid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getmaterialissuelist;

					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);
					data = data.OrderByDescending(o => o.requestid);
					return data;


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetRequestList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetmaterialdetailsByrequestid_old>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get list of materail details based on particlular requestid>>
		<param name="requestid"></param>
		 <param name="pono"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> GetmaterialdetailsByrequestid_old(string requestid, string pono)
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
					log.ErrorMessage("PODataProvider", "GetRequestList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetmaterialdetailsByrequestid>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : << get list of materail details based on particlular requestid>>
		<param name="requestid"></param>
		 <param name="pono"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> GetmaterialdetailsByrequestid(string requestid, string pono)
		{

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = "";
					if (string.IsNullOrEmpty(pono) || pono == "NULL" || pono == null || pono.Trim() == "")
						query = WMSResource.getrequestdetailswithoutpo.Replace("#requestid", requestid);
					else
					{
						query = WMSResource.getrequestdetailswithoutpo.Replace("#requestid", requestid);
						query += " and rq.pono = '" + pono + "'";
					}



					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					   query, null, commandType: CommandType.Text);

					foreach (IssueRequestModel mdl in data)
					{

						if (mdl.storeavailableqty != null && mdl.mrntotalissuedqty != null)
						{

							decimal? diff = mdl.storeavailableqty - mdl.mrntotalissuedqty;
							if (diff < 0)
							{
								mdl.storeavailableqty = 0;
							}
							else
							{
								mdl.storeavailableqty = diff;

							}
						}
						if (mdl.storeavailableqty != null && mdl.mrnissuedqty != null)
						{

							if (mdl.issuedqty != null)
							{
								mdl.issuedqty = mdl.issuedqty + mdl.mrnissuedqty;
							}
							else
							{
								mdl.issuedqty = mdl.mrnissuedqty;
							}


						}

					}

					return data;




				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetRequestList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetPonodetails>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get list of pono data>>
		 <param name="pono"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "GetPonodetails", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<updaterequestedqty_old>>  Author :<<Amulya>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<inserting or updating requested qty by PM>>
		<param name="dataobj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int updaterequestedqty_old(List<IssueRequestModel> dataobj)
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
				{

				}
				//requestid = obj.requestid + 1;
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
								item.material,
								materialid,
								item.createddate,
								item.createdby,
								item.requesterid,
								requestid,
								item.requestedquantity,
								item.projectcode,
								item.remarks
							});
						}
						if (result != 0)
						{
							EmailModel emailmodel = new EmailModel();
							emailmodel.pono = item.pono;
							emailmodel.jobcode = item.projectname;
							emailmodel.material = item.material;
							emailmodel.createdby = item.requesterid;
							emailmodel.createddate = item.requesteddate;
							EmailUtilities emailobj = new EmailUtilities();
							emailobj.sendEmail(emailmodel, 4, 3);
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
				log.ErrorMessage("PODataProvider", "IssueRequest", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return 0;
			}


		}

		/*
		Name of Function : <<updaterequestedqty>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<inserting or updating requested qty by PM>>
		<param name="dataobj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int updaterequestedqty(List<IssueRequestModel> dataobj)
		{

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				NpgsqlTransaction Trans = null;

				try
				{
					pgsql.Open();
					Trans = pgsql.BeginTransaction();
					string calltype = dataobj[0].calltype;
					string mattype = dataobj[0].materialtype;
					string rsvid = dataobj[0].reserveid;
					MaterialTransaction mainmodel = new MaterialTransaction();
					if (mattype == "Project Stock")
					{
						mainmodel.requesttype = "Project";
						string storeQuery = "select projectmanager from wms.wms_project wp where projectcode = '" + dataobj[0].projectcode + "' and projectmanager is not null limit 1";
						var projectmanagerid = pgsql.ExecuteScalar(storeQuery, null);
						if (projectmanagerid != null)
						{
							if (projectmanagerid.ToString().Trim() == dataobj[0].requesterid.ToString().Trim())
							{
								mainmodel.isapprovalrequired = false;
								mainmodel.approverid = null;
								mainmodel.isapproved = null;
								mainmodel.approvalremarks = null;
								mainmodel.approvedon = null;

							}
							else
							{
								mainmodel.isapprovalrequired = true;
								mainmodel.approverid = projectmanagerid.ToString().Trim();
								mainmodel.isapproved = null;
								mainmodel.approvalremarks = null;
								mainmodel.approvedon = null;

							}
						}
					}
					else if (mattype == "PLOS")
					{
						mainmodel.requesttype = "PLOS";
						mainmodel.isapprovalrequired = true;
						mainmodel.approverid = dataobj[0].managerid;
						mainmodel.isapproved = null;
						mainmodel.approvalremarks = null;
						mainmodel.approvedon = null;
					}
					else
					{
						mainmodel.requesttype = "Plant";
						mainmodel.isapprovalrequired = true;
						mainmodel.approverid = dataobj[0].managerid;
						mainmodel.isapproved = null;
						mainmodel.approvalremarks = null;
						mainmodel.approvedon = null;
					}





					mainmodel.projectcode = dataobj[0].projectcode;
					mainmodel.approveremailid = dataobj[0].approveremailid;
					mainmodel.remarks = dataobj[0].remarks;
					mainmodel.requesterid = dataobj[0].requesterid;
					mainmodel.requesteddate = System.DateTime.Now;
					string insertmatquery = WMSResource.insertmaterialrequest;
					string materials = "";
					var result = pgsql.ExecuteScalar(insertmatquery, new
					{
						mainmodel.approveremailid,
						mainmodel.approverid,
						mainmodel.requesterid,
						mainmodel.projectcode,
						mainmodel.remarks,
						mainmodel.isapprovalrequired,
						mainmodel.isapproved,
						mainmodel.approvalremarks,
						mainmodel.approvedon,
						mainmodel.requesttype
					});
					if (result != null)
					{
						var stindex = 0;
						foreach (var item in dataobj)
						{
							if (stindex > 0)
							{
								materials += ", ";

							}
							materials += item.material;
							MaterialTransactionDetail detail = new MaterialTransactionDetail();
							detail.id = Guid.NewGuid().ToString();
							detail.requestid = result.ToString();
							detail.materialid = item.materialid;
							detail.requestedquantity = item.quantity;
							detail.poitemdescription = item.Materialdescription;
							detail.materialcost = item.materialcost;
							detail.pono = item.pono;
							string insertdataqry = WMSResource.insertmaterialrequestdetails;
							var result1 = pgsql.Execute(insertdataqry, new
							{

								detail.id,
								detail.requestid,
								detail.materialid,
								detail.requestedquantity,
								detail.poitemdescription,
								detail.pono


							});

							stindex++;


						}
						if (calltype == "fromreserve")
						{
							string query1 = "update wms.materialrequest set reserveid = '" + rsvid + "' where requestid = '" + result.ToString() + "'";
							var results111 = pgsql.ExecuteScalar(query1);
						}

						EmailModel emailmodel = new EmailModel();
						emailmodel.pono = dataobj[0].pono;
						emailmodel.requestid = result.ToString();
						emailmodel.jobcode = dataobj[0].projectcode;
						emailmodel.material = materials;
						emailmodel.createdby = dataobj[0].requesterid;
						emailmodel.createddate = DateTime.Now;

						EmailUtilities emailobj = new EmailUtilities();
						if (mainmodel.isapprovalrequired)
						{

							string userquery = "select  * from wms.employee where employeeno='" + mainmodel.approverid + "'";
							User userdata = pgsql.QuerySingle<User>(
							   userquery, null, commandType: CommandType.Text);
							emailmodel.ToEmailId = userdata.email;
							if (mattype == "Project Stock")
							{
								emailobj.sendEmail(emailmodel, 23);
							}
							else
							{
								emailobj.sendEmail(emailmodel, 36);
							}

						}
						else
						{
							emailobj.sendEmail(emailmodel, 4, 3);
						}

						Trans.Commit();
						return 1;

					}
					else
					{
						Trans.Rollback();
						return 0;
					}




				}
				catch (Exception Ex)
				{
					Trans.Rollback();
					log.ErrorMessage("PODataProvider", "MaterialRequest", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return 0;
				}
				finally
				{
					pgsql.Close();
				}

			}

		}

		/*
		Name of Function : <<mattransferapprove>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<mat transfer approve>>
		<param name="datamodel"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string matrequestapprove(List<MaterialTransaction> datamodel)
		{
			string result = "";
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				NpgsqlTransaction Trans = null;
				try
				{

					pgsql.Open();
					Trans = pgsql.BeginTransaction();

					List<EmailModel> emailmodels = new List<EmailModel>();
					foreach (MaterialTransaction data in datamodel)
					{
						if (data.approvalcheck == "1")
						{
							data.isapproved = true;
						}
						else if (data.approvalcheck == "0")
						{
							data.isapproved = false;
						}
						else
						{
							data.isapproved = null;
						}
						string insertdataqry = WMSResource.materialrequestapprovalqry.Replace("#requestid", data.requestid);
						var result1 = pgsql.Execute(insertdataqry, new
						{
							data.isapproved,
							data.approvalremarks
						});

						EmailModel emailmodel = new EmailModel();
						emailmodel.requestid = data.requestid;
						emailmodel.jobcode = data.projectcode;
						emailmodel.createdby = data.requesterid;
						emailmodel.createddate = Convert.ToDateTime(data.requesteddate);
						if (data.isapproved == true)
						{
							emailmodel.isapproved = true;
						}
						else
						{
							string userquery = "select  * from wms.employee where employeeno='" + data.requesterid + "'";
							User userdata = pgsql.QuerySingle<User>(
							   userquery, null, commandType: CommandType.Text);
							emailmodel.ToEmailId = userdata.email;
							emailmodel.isapproved = false;
						}
						emailmodels.Add(emailmodel);

					}

					result = "saved";
					Trans.Commit();
					foreach (EmailModel mdl in emailmodels)
					{
						EmailUtilities emailobj = new EmailUtilities();
						if (mdl.isapproved)
						{
							emailobj.sendEmail(mdl, 4, 3);
						}
						else
						{
							emailobj.sendEmail(mdl, 24);

						}

					}






				}
				catch (Exception Ex)
				{
					Trans.Rollback();
					log.ErrorMessage("PODataProvider", "mattransferapprove", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return Ex.Message;
				}
				finally
				{
					pgsql.Close();
				}

			}
			return result;
		}

		/*
		Name of Function : <<stomatrequestapprove>>  Author :<<Ramesh>>  
		Date of Creation <<12-02-2021>>
		Purpose : <<mat transfer approve>>
		<param name="datamodel"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string stomatrequestapprove(List<invstocktransfermodel> datamodel)
		{
			string result = "";
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				NpgsqlTransaction Trans = null;
				try
				{

					pgsql.Open();
					Trans = pgsql.BeginTransaction();

					List<EmailModel> emailmodels = new List<EmailModel>();
					foreach (invstocktransfermodel data in datamodel)
					{
						if (data.approvalcheck == "1")
						{
							data.isapproved = true;
						}
						else if (data.approvalcheck == "0")
						{
							data.isapproved = false;
						}
						else
						{
							data.isapproved = null;
						}
						string insertdataqry = WMSResource.stosubcontractiongapprovalquery.Replace("#requestid", data.transferid);
						var result1 = pgsql.Execute(insertdataqry, new
						{
							data.isapproved,
							data.approvalremarks
						});

						EmailModel emailmodel = new EmailModel();
						emailmodel.requestid = data.transferid;
						emailmodel.jobcode = data.projectcode;
						emailmodel.createdby = data.transferredby;
						emailmodel.requesttype = data.transfertype;
						emailmodel.createddate = Convert.ToDateTime(data.transferredon);
						if (data.isapproved == true)
						{
							emailmodel.isapproved = true;
						}
						else
						{
							string userquery = "select  * from wms.employee where employeeno='" + data.transferredby + "'";
							User userdata = pgsql.QuerySingle<User>(
							   userquery, null, commandType: CommandType.Text);
							emailmodel.ToEmailId = userdata.email;
							emailmodel.isapproved = false;
						}
						emailmodels.Add(emailmodel);

					}

					result = "saved";
					Trans.Commit();
					foreach (EmailModel mdl in emailmodels)
					{
						EmailUtilities emailobj = new EmailUtilities();
						if (mdl.isapproved)
						{
							if (mdl.requesttype == "STO")
							{
								emailobj.sendEmail(mdl, 27, 3);
							}
							else
							{
								emailobj.sendEmail(mdl, 28, 3);
							}
						}
						else
						{
							emailobj.sendEmail(mdl, 31);

						}

					}






				}
				catch (Exception Ex)
				{
					Trans.Rollback();
					log.ErrorMessage("PODataProvider", "mattransferapprove", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return Ex.Message;
				}
				finally
				{
					pgsql.Close();
				}

			}
			return result;
		}

		/*
		Name of Function : <<stomatrequestapprove>>  Author :<<Ramesh>>  
		Date of Creation <<12-02-2021>>
		Purpose : <<mat transfer approve>>
		<param name="datamodel"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string updatepm(List<assignpmmodel> datamodel)
		{
			string result = "";
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				NpgsqlTransaction Trans = null;
				try
				{

					pgsql.Open();
					Trans = pgsql.BeginTransaction();


					foreach (assignpmmodel data in datamodel)
					{
						string projectmanager = data.selectedemployeeview.employeeno;
						string insertdataqry = WMSResource.updatePM.Replace("#projectcode", data.projectcode);
						var result1 = pgsql.Execute(insertdataqry, new
						{
							projectmanager
						});

					}

					result = "saved";
					Trans.Commit();







				}
				catch (Exception Ex)
				{
					Trans.Rollback();
					log.ErrorMessage("PODataProvider", "mattransferapprove", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return Ex.Message;
				}
				finally
				{
					pgsql.Close();
				}

			}
			return result;
		}

		/*
		Name of Function : <<ApproveMaterialissue>>  Author :<<Amulya>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<issued matreial list >>
		<param name="dataobj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int ApproveMaterialissue(List<IssueRequestModel> dataobj)
		{
			NpgsqlTransaction Trans = null;
			try
			{
				var result = 0;

				using (var DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					//data.createddate = System.DateTime.Now;
					DB.Open();
					Trans = DB.BeginTransaction();
					foreach (var item in dataobj)
					{
						var createdate = Convert.ToDateTime(item.createddate).ToString("yyyy-MM-dd");
						string descriptionstr = null;
						if (item.Materialdescription != null)
						{
							descriptionstr = item.Materialdescription.Replace("\'", "''");
						}
						if (item.itemlocation != "ON FLOOR")
						{
							string stockquery = "";
							if (item.materialtype == "Project")
							{
								stockquery = "select * from wms.wms_stock where projectid='" + item.projectid + "' and pono='" + item.pono + "' and materialid = '" + item.materialid + "' and lower(poitemdescription) = lower('" + descriptionstr + "') and availableqty > 0 and itemlocation = '" + item.itemlocation + "' and createddate::DATE = '" + createdate + "' order by itemid";
							}
							else if (item.materialtype == "PLOS")
							{
								stockquery = "select * from wms.wms_stock where  materialid = '" + item.materialid + "' and lower(poitemdescription) = lower('" + descriptionstr + "') and availableqty > 0 and receivedtype = 'Material Return' and itemlocation = '" + item.itemlocation + "' and createddate::DATE = '" + createdate + "' ";
								if (item.saleorderno != null && item.saleorderno.Trim() != "")
								{
									stockquery += " and saleorderno = '" + item.saleorderno + "'";
								}
								stockquery += "order by itemid";
							}
							else
							{
								stockquery = "select * from wms.wms_stock where  materialid = '" + item.materialid + "' and lower(poitemdescription) = lower('" + descriptionstr + "') and availableqty > 0 and stcktype = 'Plant Stock' and itemlocation = '" + item.itemlocation + "' and createddate::DATE = '" + createdate + "' ";
								if (item.saleorderno != null && item.saleorderno.Trim() != "")
								{
									stockquery += " and saleorderno = '" + item.saleorderno + "'";
								}
								stockquery += "order by itemid";
							}

							var stockdata = DB.QueryAsync<StockModel>(stockquery, null, commandType: CommandType.Text);
							if (stockdata != null)
							{
								decimal? quantitytoissue = item.issuedqty;
								decimal? issuedqty = 0;
								foreach (StockModel itm in stockdata.Result)
								{
									string approvedstatus = string.Empty;
									if (item.issuedqty != 0)
									{
										approvedstatus = "Approved";
									}
									DateTime approvedon = System.DateTime.Now;

									int requestforissueid = item.requestforissueid;
									string requestmaterialid = item.requestmaterialid;
									string materialid = item.materialid;
									if (quantitytoissue <= itm.availableqty)
									{
										issuedqty = quantitytoissue;
									}
									else
									{
										issuedqty = itm.availableqty;
									}

									quantitytoissue = quantitytoissue - issuedqty;

									DateTime itemissueddate = System.DateTime.Now;

									string updateapproverstatus = WMSResource.updateapproverstatus;
									string requestid = null;
									if (item.requesttype == "MaterialRequest")
									{
										requestid = requestmaterialid;

									}
									else
									{
										requestid = item.requestid;
									}

									if (item.issuedqty > 0)
									{
										result = DB.Execute(updateapproverstatus, new
										{
											approvedstatus,
											requestmaterialid,
											approvedon,
											issuedqty,
											materialid,
											itm.pono,
											itm.itemid,
											item.itemreturnable,
											item.approvedby,
											itemissueddate,
											item.itemreceiverid,
											itm.itemlocation,
											requestid,
											item.requesttype

										});
										decimal? availableqty = itm.availableqty - item.issuedqty;

										string insertqueryforstatusforqty = WMSResource.updateqtyafterissue.Replace("#itemid", Convert.ToString(itm.itemid)).Replace("#issuedqty", Convert.ToString(issuedqty));

										var data1 = DB.ExecuteScalar(insertqueryforstatusforqty, new
										{

										});


									}

									if (quantitytoissue <= 0)
									{
										break;
									}

								}
							}

						}
						else
						{
							string stockquery = "";
							stockquery = "select * from wms.wms_storeinward ws where ws.projectid = '" + item.projectid + "' and ws.pono = '" + item.pono + "' and ws.materialid = '" + item.materialid + "'";
							stockquery += " and Lower(ws.poitemdescription) = Lower('" + descriptionstr + "') and ws.confirmqty > 0 ";
							stockquery += " and ws.inwardid not in (select distinct ws2.inwardid from wms.wms_stock ws2 where ws2.inwardid is not null)";
							var stockdata = DB.QueryAsync<StockModel>(stockquery, null, commandType: CommandType.Text);
							if (stockdata != null)
							{
								decimal? quantitytoissue = item.issuedqty;
								decimal? issuedqty = 0;
								foreach (StockModel itm in stockdata.Result)
								{
									string approvedstatus = string.Empty;
									if (item.issuedqty != 0)
									{
										approvedstatus = "Approved";
									}
									DateTime approvedon = System.DateTime.Now;

									int requestforissueid = item.requestforissueid;
									string requestmaterialid = item.requestmaterialid;
									string materialid = item.materialid;
									if (quantitytoissue <= itm.confirmqty)
									{
										issuedqty = quantitytoissue;
									}
									else
									{
										issuedqty = itm.confirmqty;
									}

									quantitytoissue = quantitytoissue - issuedqty;

									DateTime itemissueddate = System.DateTime.Now;

									string updateapproverstatus = WMSResource.updateapproverstatus;
									string requestid = null;
									if (item.requesttype == "MaterialRequest")
									{
										requestid = requestmaterialid;

									}
									else
									{
										requestid = item.requestid;
									}

									if (item.issuedqty > 0)
									{
										string insertqry = WMSResource.issueMRNMaterials;
										string mrnby = dataobj[0].approvedby;
										string mrnremarks = "";
										string projectcode = item.projectid;
										string requesttype = "MaterialRequest";
										decimal? acceptedqty = itm.confirmqty;
										result = DB.Execute(insertqry, new
										{
											itm.inwardid,
											projectcode,
											mrnby,
											mrnremarks,
											acceptedqty,
											issuedqty,
											requestid,
											requesttype
										});


									}

									if (quantitytoissue <= 0)
									{
										break;
									}

								}
							}

						}
					}

					if (dataobj[0].requesttype == "MaterialRequest")
					{
						string requestid = dataobj[0].requestid;
						string approvedby = dataobj[0].approvedby;
						string updaterequest = "update wms.materialrequest set issuedby = '" + approvedby + "',issuedon=current_date where requestid='" + requestid + "'";

						var data2 = DB.ExecuteScalar(updaterequest, new
						{

						});
						//EmailModel emailmodelx = new EmailModel();
						////emailmodel.pono = datamodel[0].pono;
						////emailmodel.jobcode = datamodel[0].projectname;
						//emailmodelx.materialissueid = dataobj[0].materialissueid;
						//emailmodelx.requestid = dataobj[0].requestid;
						////emailmodel.ToEmailId = "developer1@in.yokogawa.com";
						//emailmodelx.FrmEmailId = "ramesh.kumar@in.yokogawa.com";
						////emailmodel.CC = "sushma.patil@in.yokogawa.com";
						//EmailUtilities emailobjx = new EmailUtilities();
						//emailobjx.sendEmail(emailmodelx,51);
					}
					if (dataobj[0].requesttype == "STO" || dataobj[0].requesttype == "SubContract")
					{
						string requestid = dataobj[0].transferid;
						string approvedby = dataobj[0].approvedby;
						string updaterequest = "update wms.wms_invstocktransfer set status = 'Issued',issuedon=current_date where transferid='" + requestid + "'";

						var data2 = DB.ExecuteScalar(updaterequest, new
						{

						});
					}
					string mailto = "";
					string mailtocc = "";
					string userquery = "select  * from wms.employee where employeeno='" + dataobj[0].createdby + "'";
					User userdata = DB.QuerySingle<User>(
					   userquery, null, commandType: CommandType.Text);
					string userquery1 = "select  * from wms.employee where employeeno='" + dataobj[0].approvedby + "'";
					User userdata1 = DB.QuerySingle<User>(
					   userquery1, null, commandType: CommandType.Text);
					mailto = userdata.email;
					mailtocc = userdata1.email;
					EmailModel emailmodel = new EmailModel();
					emailmodel.materialissueid = dataobj[0].materialissueid;
					emailmodel.requestid = dataobj[0].requestid;
					if (dataobj[0].requesttype == "STO" || dataobj[0].requesttype == "SubContract")
					{
						emailmodel.requestid = dataobj[0].transferid;
					}
					emailmodel.ToEmailId = mailto;

					Trans.Commit();
					emailmodel.CC = mailtocc;
					EmailUtilities emailobj = new EmailUtilities();
					if (dataobj[0].requesttype == "STO")
					{
						emailobj.sendEmail(emailmodel, 25);
					}
					else if (dataobj[0].requesttype == "SubContract")
					{
						emailobj.sendEmail(emailmodel, 26);
					}
					else
					{
						emailobj.sendEmail(emailmodel, 51);

					}

				}




				return (Convert.ToInt32(result));
			}
			catch (Exception Ex)
			{
				Trans.Rollback();
				log.ErrorMessage("PODataProvider", "updaterequestedqty", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return 0;
			}
		}

		/*
		Name of Function : <<GetgatepassList>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get list of gatepass data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "GetgatepassList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<NonreturnGetgatepassList>>  Author :<<Ramesh>>  
		Date of Creation <<23/07/2020>>
		Purpose : <<non returnable gatepass for outward entry>>
		<param name="type"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

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
					log.ErrorMessage("PODataProvider", "NonreturnGetgatepassList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<outingatepassreport>>  Author :<<Ramesh>>  
		Date of Creation <<23/07/2020>>
		Purpose : <<Report (out)>>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public async Task<IEnumerable<outwardinwardreportModel>> outingatepassreport()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.outinreportquery;

					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<outwardinwardreportModel>(
					   query, null, commandType: CommandType.Text);

					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "outingatepassreport", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<SaveOrUpdateGatepassDetails>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<insert or update gatepass info>>
		<param name="dataobj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int SaveOrUpdateGatepassDetails(gatepassModel dataobj)
		{
			NpgsqlTransaction Trans = null;

			try
			{
				//foreach(var item in dataobj._list)
				//{
				string status = "Pending";
				string remarks = dataobj.statusremarks;
				EmailModel emailmodel = new EmailModel();
				string mailto = "";

				using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				{
					pgsql.OpenAsync();
					Trans = pgsql.BeginTransaction();
					if (dataobj.gatepassid == "" || dataobj.gatepassid == null)
					{
						dataobj.requestedon = System.DateTime.Now;
						string insertquery = WMSResource.insertgatepassdata;
						string fmapprovedstatus = "";
						string approverstatus = "Pending";
						string authstatus = "Pending";

						string insertgatepasshistory = WMSResource.insertgatepassapprovalhistory;
						dataobj.deleteflag = false;
						dataobj.fmapproverid = null;
						if (dataobj.gatepasstype == "Non Returnable")
						{
							dataobj.fmapproverid = null;
							dataobj.fmapprovedstatus = "Pending";

						}
						dataobj.status = status;
						dataobj.approverstatus = approverstatus;
						var gatepassid = pgsql.ExecuteScalar(insertquery, new
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
							dataobj.requestid,
							dataobj.fmapprovedstatus,
							dataobj.approverstatus,
							authstatus,
							remarks,
							dataobj.otherreason,
							dataobj.isnonproject,
							dataobj.projectid,
							dataobj.materialtype

						});
						dataobj.gatepassid = gatepassid.ToString();
						if (dataobj.gatepasstype == "Returnable")
						{
							string approvername = dataobj.managername;
							int label = 1;
							//string approverstatus = "Pending";
							var gatepasshistory = pgsql.ExecuteScalar(insertgatepasshistory, new
							{

								dataobj.approverid,
								approvername,
								gatepassid,
								label,
								approverstatus,
								remarks
							});
							string userquery = "select  * from wms.employee where employeeno='" + dataobj.approverid + "'";
							User userdata = pgsql.QuerySingle<User>(
							   userquery, null, commandType: CommandType.Text);
							mailto = userdata.email;

							emailmodel.pono = dataobj.pono;
							emailmodel.requestid = dataobj.requestid;
							emailmodel.gatepassid = dataobj.gatepassid;
							emailmodel.gatepasstype = dataobj.gatepasstype;
							emailmodel.ToEmailId = mailto;

							emailmodel.requestedon = dataobj.requestedon;
							emailmodel.requestedby = dataobj.requestedby;



						}
						else if (dataobj.gatepasstype == "Non Returnable")
						{
							//string updategatepasshistoryfornonreturn = WMSResource.updategatepasshistoryfornonreturn;
							{
								string approvername = dataobj.managername;
								int label = 1;
								//string approverstatus = "Pending";
								var gatepasshistory = pgsql.ExecuteScalar(insertgatepasshistory, new
								{

									dataobj.approverid,
									approvername,
									gatepassid,
									label,
									approverstatus,
									remarks
								});
								//string approverid = config.FinanceEmployeeNo;
								//approvername = config.FinanceEmployeeName;
								//label = 2;
								//var gatepassdata = pgsql.ExecuteScalar(insertgatepasshistory, new
								//{

								//	approverid,
								//	gatepassid,
								//	label,
								//	approverstatus,
								//	approvername
								//});
							}

							string userquery = "select  * from wms.employee where employeeno='" + dataobj.approverid + "'";
							User userdata = pgsql.QuerySingle<User>(
							   userquery, null, commandType: CommandType.Text);
							mailto = userdata.email;

							emailmodel.pono = dataobj.pono;
							emailmodel.requestid = dataobj.requestid;
							emailmodel.gatepassid = dataobj.gatepassid;
							emailmodel.gatepasstype = dataobj.gatepasstype;
							emailmodel.ToEmailId = mailto;

							emailmodel.requestedon = dataobj.requestedon;
							emailmodel.requestedby = dataobj.requestedby;


						}


						if (dataobj.gatepassid == "" || dataobj.gatepassid == null)
							dataobj.gatepassid = gatepassid.ToString();

					}
					else
					{
						dataobj.requestedon = System.DateTime.Now;
						string insertquery = WMSResource.updategatepass.Replace("#gatepassid", Convert.ToString(dataobj.gatepassid));


						var result = pgsql.ExecuteScalar(insertquery, new
						{

							dataobj.gatepasstype,
							dataobj.status,
							dataobj.requestedon,
							dataobj.referenceno,
							dataobj.vehicleno,
							dataobj.requestedby,
							dataobj.vendorname,
							dataobj.reasonforgatepass,
							remarks,
							dataobj.otherreason

						});

						string userquery = "select  * from wms.employee where employeeno='" + dataobj.approverid + "'";
						User userdata = pgsql.QuerySingle<User>(
						   userquery, null, commandType: CommandType.Text);
						mailto = userdata.email;

						emailmodel.pono = dataobj.pono;
						emailmodel.requestid = dataobj.requestid;
						emailmodel.gatepassid = dataobj.gatepassid;
						emailmodel.gatepasstype = dataobj.gatepasstype;
						emailmodel.ToEmailId = mailto;

						emailmodel.requestedon = dataobj.requestedon;
						emailmodel.requestedby = dataobj.requestedby;

					}

					foreach (var item in dataobj.materialList)
					{
						int itemid = 0;
						if (item.gatepassmaterialid == 0)
						{
							string insertquerymaterial = WMSResource.insertgatepassmaterial;
							dataobj.deleteflag = false;
							var results = pgsql.ExecuteScalar(insertquerymaterial, new
							{

								dataobj.gatepassid,
								item.materialid,
								item.quantity,
								dataobj.deleteflag,
								item.remarks,
								item.materialcost,
								item.expecteddate,
								//item.returneddate,
								item.issuedqty,
								item.materialdescription,
								item.pono

							});

						}
						else
						{
							//string updatestockquery = "update wms.wms_stock set availableqty=availableqty+" + item.quantity + " where itemid=" + itemid;

							//var result1 = DB.ExecuteScalar(updatestockquery, new
							//{
							//});
							if (item.quantity <= 0)
							{
								string insertquery = WMSResource.deletegatepassmaterial.Replace("#gatepassmaterialid", Convert.ToString(item.gatepassmaterialid));
								var data = pgsql.Execute(insertquery, new
								{

								});

							}
							string updatequery = WMSResource.updategatepassmaterial.Replace("#gatepassmaterialid", Convert.ToString(item.gatepassmaterialid));

							var result = pgsql.ExecuteScalar(updatequery, new
							{

								dataobj.gatepassid,
								item.materialid,
								item.quantity,
								item.remarks,
								item.materialcost,
								item.expecteddate,
								item.returneddate,
								item.materialdescription

							});


						}

					}
					Trans.Commit();
					EmailUtilities emailobj = new EmailUtilities();

					emailmodel.gatepassid = dataobj.gatepassid;
					emailmodel.gatepasstype = dataobj.gatepasstype;
					emailobj.sendEmail(emailmodel, 8);

				}


				return (1);
			}
			catch (Exception Ex)
			{
				Trans.Rollback();
				log.ErrorMessage("PODataProvider", "SaveOrUpdateGatepassDetails", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return 0;
			}
		}

		/*
		Name of Function : <<checkmaterialandqty>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<check material in stock>>
		<param name="material"></param>
		 <param name="qty"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string checkmaterialandqty(string material = null, decimal? qty = 0)
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
									//returnvalue = "Material and quantity does not exists for " + material + " available qty is " + obj.availableqty;
									returnvalue = "Available quantity for " + material + "  is " + obj.availableqty;
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
					log.ErrorMessage("PODataProvider", "checkmaterialandqty", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return "false";
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<deletegatepassmaterial>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<delete gatepass>>
		<param name="gatepassmaterialid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
				log.ErrorMessage("PODataProvider", "deletegatepassmaterial", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return 0;
			}
		}

		/*
		Name of Function : <<updategatepassapproverstatus>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<update gatepass approver info>>
		<param name="model"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int updategatepassapproverstatus_old(List<gatepassModel> model)
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
					string updateapproverstatus = WMSResource.updategatepassmaterialissue;
					string approvedstatus = item.approverstatus;
					item.itemissueddate = System.DateTime.Now;
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
				log.ErrorMessage("PODataProvider", "updategatepassapproverstatus", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return 0;
			}
		}

		/*
			Name of Function : <<updategatepassapproverstatus>>  Author :<<Ramesh>>  
			Date of Creation <<12-12-2019>>
			Purpose : <<update gatepass approver info>>
			<param name="model"></param>
			Review Date :<<>>   Reviewed By :<<>>
			*/
		public int updategatepassapproverstatus(List<gatepassModel> model)
		{
			int returndata = 0;
			NpgsqlTransaction Trans = null;
			try
			{
				using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				{

					pgsql.OpenAsync();
					Trans = pgsql.BeginTransaction();
					foreach (var item in model)
					{

						var createdate = Convert.ToDateTime(item.createddate).ToString("yyyy-MM-dd");
						if (item.materialdescription != null)
						{
							item.materialdescription = item.materialdescription.Replace("\'", "''");
						}
						string stockquery = "";
						if (item.materialtype == "Project")
						{
							stockquery = "select * from wms.wms_stock where projectid='" + item.projectid + "' and pono = '" + item.pono + "' and materialid = '" + item.materialid + "' and lower(poitemdescription) = lower('" + item.materialdescription + "') and availableqty > 0 and itemlocation = '" + item.itemlocation + "' and createddate::DATE = '" + createdate + "' order by itemid";
						}
						else
						{
							stockquery = "select * from wms.wms_stock where  materialid = '" + item.materialid + "' and lower(poitemdescription) = lower('" + item.materialdescription + "') and availableqty > 0 and stcktype = 'Plant Stock' and itemlocation = '" + item.itemlocation + "' and createddate::DATE = '" + createdate + "' ";
							if (item.saleorderno != null && item.saleorderno.Trim() != "")
							{
								stockquery += " and saleorderno = '" + item.saleorderno + "'";
							}
							stockquery += "order by itemid";
						}

						var stockdata = pgsql.QueryAsync<StockModel>(stockquery, null, commandType: CommandType.Text);
						if (stockdata != null)
						{
							decimal? quantitytoissue = item.issuedqty;
							decimal? issuedqty = 0;
							decimal? totalissued = 0;
							foreach (StockModel itm in stockdata.Result)
							{
								if (quantitytoissue <= itm.availableqty)
								{
									issuedqty = quantitytoissue;
								}
								else
								{
									issuedqty = itm.availableqty;
								}

								quantitytoissue = quantitytoissue - issuedqty;
								totalissued = totalissued + issuedqty;
								string updateapproverstatus = WMSResource.updategatepassmaterialissue;
								string approvedstatus = item.approverstatus;
								item.itemissueddate = System.DateTime.Now;
								item.approvedon = System.DateTime.Now;
								Boolean itemreturnable = false;
								if (item.gatepasstype == "Returnable")
								{
									itemreturnable = true;
								}

								var result1 = pgsql.Execute(updateapproverstatus, new
								{
									approvedstatus,
									item.gatepassmaterialid,
									item.approvedon,
									issuedqty,
									item.materialid,
									item.pono,
									itm.itemid,
									itemreturnable,
									item.approvedby,
									item.itemissueddate,
									item.itemreceiverid,

								});

								string updateissueqty = "update  wms.wms_gatepassmaterial set issuedqty=" + totalissued + " where gatepassmaterialid=" + item.gatepassmaterialid;
								var result2 = pgsql.Execute(updateissueqty);
								string updatestockavailable = WMSResource.updateqtyafterissue.Replace("#itemid", Convert.ToString(itm.itemid)).Replace("#issuedqty", Convert.ToString(issuedqty));
								var result3 = pgsql.Execute(updatestockavailable);

								if (quantitytoissue <= 0)
								{
									break;
								}


							}

						}
						else
						{
							Trans.Rollback();
							log.ErrorMessage("PODataProvider", "updategatepassapproverstatus", "No material in stock", "", url);
							return 0;
						}
						model[0].approvedon = System.DateTime.Now;
						model[0].status = "Issued";
						string insertquery = WMSResource.updategatepassapproverstatus.Replace("#gatepassid", Convert.ToString(model[0].gatepassid));
						var data = pgsql.Execute(insertquery, new
						{
							model[0].status,
							model[0].approverremarks,
							model[0].approverstatus,
							model[0].approvedon,

						});
						returndata = Convert.ToInt32(data);


					}
					string userquery = WMSResource.getRequesterEmail.Replace("#gatepassid", model[0].gatepassid.ToString());
					User userdata = pgsql.QuerySingle<User>(
					   userquery, null, commandType: CommandType.Text);
					string mailto = userdata.email;
					EmailUtilities emailobj = new EmailUtilities();
					EmailModel emailmodel = new EmailModel();
					emailmodel.gatepassid = model[0].gatepassid.ToString();
					emailmodel.ToEmailId = mailto;
					emailobj.sendEmail(emailmodel, 22);
					Trans.Commit();
					return returndata;

				}
			}
			catch (Exception Ex)
			{
				Trans.Rollback();
				log.ErrorMessage("PODataProvider", "updategatepassapproverstatus", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return 0;
			}
		}
		/// <summary>
		/// get list of material based on gatepassid
		/// </summary>
		/// <param name="gatepassid"></param>
		/// <returns></returns>

		/*
		Name of Function : <<GetmaterialList>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get list of material based on gatepassid>>
		<param name="gatepassid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<gatepassModel>> GetmaterialList(string gatepassid)
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
					log.ErrorMessage("PODataProvider", "GetmaterialList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		/*
		Name of Function : <<getGatePassApprovalHistoryList>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get gatepass approval history based on gatepassid>>
		<param name="gatepassid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<gatepassapprovalsModel>> getGatePassApprovalHistoryList(string gatepassid)
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
					log.ErrorMessage("PODataProvider", "getGatePassApprovalHistoryList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<updateprintstatus>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<updating print status>>
		<param name="model"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "updateprintstatus", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
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
		//    log.ErrorMessage("PODataProvider", "updateprintstatus", Ex.StackTrace.ToString(), Ex.Message.ToString(),url);
		//    return 0;
		//}
		//}

		/*
		Name of Function : <<updatereprintstatus>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<updating reprint status >>
		<param name="model"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
				string barcodeid = null;
				string insertquery = WMSResource.insertReprintHistory;
				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					data = Convert.ToInt32(DB.ExecuteScalar(insertquery, new

					{
						model.gatepassid,
						model.reprintedby,
						model.reprintcount,
						barcodeid,
						model.noofprint,
						model.inwmasterid,
						model.inwardid,
						model.printerid
					}));
					returndata = Convert.ToInt32(data);
				}
				using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				{
					string query = WMSResource.checkreprintalreadydone;
					if (model.inwardid != null)
					{
						query = query + " inwardid= '" + model.inwardid + "' order by reprintcount desc limit 1";
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
				log.ErrorMessage("PODataProvider", "updategatepassapproverstatus", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return 0;
			}
		}

		/*
		Name of Function : <<GetreportBasedCategory>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get list based on ABC category>>
		<param name="categoryid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "GetreportBasedCategory", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}

		}

		/*
		Name of Function : <<GetreportBasedMaterial>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get list based on materail in category>>
		<param name="materailid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "GetreportBasedMaterial", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<updateABCcategorydata>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<update ABC categorydata>>
		<param name="model"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
				log.ErrorMessage("PODataProvider", "updateABCcategorydata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return 0;
			}
		}

		/*
		Name of Function : <<GetABCCategorydata>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get ABC Category data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "GetABCCategorydata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetABCavailableqtyList>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get ABC available qty List>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "GetABCavailableqtyList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetEnquirydata>>  Author :<<Ramesh>>  
		Date of Creation <<08/06/2020>>
		Purpose : <<returns Enquiry Details>>
		<param name="materialid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "GetEnquirydata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetCyclecountList>>  Author :<<Ramesh>>  
		Date of Creation <<08/06/2020>>
		Purpose : <<returns all  Materials to count>>
		<param name="limita"></param>
		 <param name="limitb"></param>
		 <param name="limitc"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<CycleCountList>> GetCyclecountList(int limita, int limitb, int limitc)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					Cyclecountconfigmodel config = new Cyclecountconfigmodel();
					await pgsql.OpenAsync();

					//Ramesh (08/06/2020) returns category A/B/C configuration

					string QueryABC = "select * from wms.wms_rd_category where deleteflag is false";
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
					log.ErrorMessage("PODataProvider", "GetCyclecountList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetCyclecountPendingList>>  Author :<<Ramesh>>  
		Date of Creation <<08/06/2020>>
		Purpose : <<returns All counted Material list>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<CycleCountList>> GetCyclecountPendingList()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					Cyclecountconfigmodel config = new Cyclecountconfigmodel();
					await pgsql.OpenAsync();
					//string QueryA = "Select * from wms.cyclecount";
					string QueryA = "Select cy.*,(pomat.unitprice * cy.availableqty ) as value from wms.cyclecount cy inner join wms.wms_pomaterials pomat  on cy.materialid = pomat.material";
					var adata = await pgsql.QueryAsync<CycleCountList>(QueryA, null, commandType: CommandType.Text);
					return adata;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetCyclecountPendingList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<UpdateinsertCycleCount>>  Author :<<Ramesh>>  
		Date of Creation <<08/06/2020>>
		Purpose : <<update or insert cycle count>>
		<param name="dataobj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
								obj.difference = obj.physicalqty - obj.availableqty;
								string insertquery = "update wms.cyclecount set category='" + obj.category + "', materialid= '" + obj.materialid + "', availableqty= " + obj.availableqty + ", physicalqty=" + obj.physicalqty + ", difference=" + obj.difference + ", status='Pending', counted_on = current_date , counted_by = 'Ramesh', verified_on = null , verified_by = null where materialid = '" + obj.materialid + "' ";
								var result = DB.ExecuteScalar(insertquery);

							}
							else
							{
								//Ramesh (08/06/2020) user count action insertion 
								obj.difference = obj.physicalqty - obj.availableqty;
								string insertquery = "insert into wms.cyclecount(category, materialid, availableqty, physicalqty, difference, status, counted_on, counted_by, verified_on, verified_by) values('" + obj.category + "', '" + obj.materialid + "', " + obj.availableqty + ", " + obj.physicalqty + ", " + obj.difference + ", 'Pending', current_date , 'Ramesh', null, null)";
								var result = DB.ExecuteScalar(insertquery);
							}

						}


					}
					return 1;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "UpdateinsertCycleCount", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return 0;
				}
				finally
				{
					DB.Close();
				}
			}
		}

		/*
		Name of Function : <<UpdateCycleCountconfig>>  Author :<<Ramesh>>  
		Date of Creation <<08/06/2020>>
		Purpose : <<update cycle count configuration >>
		<param name="dataobj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
				log.ErrorMessage("PODataProvider", "UpdateCycleCountconfig", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return 0;
			}
		}


		/*
		Name of Function : <<GetCyclecountConfig>>  Author :<<Ramesh>>  
		Date of Creation <<08/06/2020>>
		Purpose : <<update cycle count configuration>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					string QueryABC1 = "select * from wms.wms_rd_category where deleteflag is false";
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
					log.ErrorMessage("PODataProvider", "GetCyclecountConfig", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetABCListBycategory>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get ABC List By category>>
		<param name="category"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "GetABCListBycategory", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetFIFOList>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get FIFI list of material>>
		<param name="material"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "GetFIFOList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<checkloldestmaterial>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<check oldest material>>
		<param name="materialid"></param>
		 <param name="createddate"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public ReportModel checkloldestmaterial(string materialid, string createddate)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.checkoldestmaterial.Replace("#materialid", materialid).Replace("#createddate", Convert.ToString(createddate));


					pgsql.Open();
					return pgsql.QueryFirstOrDefault<ReportModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "checkloldestmaterial", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<checkloldestmaterialwithdesc>>  Author :<<Ramesh>>  
		Date of Creation <<04-02-2021>>
		Purpose : <<check oldest material>>
		<param name="materialid"></param>
		 <param name="createddate"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public ReportModel checkoldmaterialwithdesc(string materialid, string createddate, string description)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string descriptionstr = null;
					if (description != null)
					{
						descriptionstr = description.Replace("\'", "''");
					}
					string query = WMSResource.checkoldmaterialwithdesc.Replace("#materialid", materialid).Replace("#desc", descriptionstr).Replace("#createddate", Convert.ToString(createddate));


					pgsql.Open();
					return pgsql.QueryFirstOrDefault<ReportModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "checkloldestmaterialwithdesc", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<checkoldmaterialwithdescstore>>  Author :<<Ramesh>>  
		Date of Creation <<04-02-2021>>
		Purpose : <<check oldest material>>
		<param name="materialid"></param>
		 <param name="createddate"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public ReportModel checkoldmaterialwithdescstore(string materialid, string createddate, string description, string store)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string descriptionstr = null;
					if (description != null)
					{
						descriptionstr = description.Replace("\'", "''");
					}
					string query = WMSResource.checkoldmaterialwithstore.Replace("#materialid", materialid).Replace("#store", store).Replace("#desc", descriptionstr).Replace("#createddate", Convert.ToString(createddate));


					pgsql.Open();
					return pgsql.QueryFirstOrDefault<ReportModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "checkoldmaterialwithdescstore", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<FIFOitemsupdate>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<FIFO items update>>
		<param name="model"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
							decimal? availableqty = item.availableqty - item.issuedqty;

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
					log.ErrorMessage("PODataProvider", "FIFOitemsupdate", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return 0;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}

		/*
		In Use
		Name of Function : <<getASNList>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get list of todays expected shipments>>
		<param name="deliverydate"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<OpenPoModel>> getASNList(string deliverydate)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getASNList;
					query = query + " where pomat.itemdeliverydate >= '" + deliverydate + " 00:00:00' and pomat.itemdeliverydate <= '" + deliverydate + " 23:59:59'";
					query = query + " group by pomat.pono, pomat.asnno,pomat.itemdeliverydate order by pomat.asnno";

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<OpenPoModel>(
					   query, null, commandType: CommandType.Text);
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getASNList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getASNListdata>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get ASN List data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					query = query + " where pomat.itemdeliverydate >= '" + weekbeforeDatestr + " 00:00:00' and pomat.itemdeliverydate <= '" + currentdatestr + " 23:59:59'";
					query = query + " group by pomat.pono, pomat.asnno,pomat.itemdeliverydate order by pomat.itemdeliverydate desc";
					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<OpenPoModel>(
					   query, null, commandType: CommandType.Text);
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getASNList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getUserDashboarddata>>  Author :<<LP>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get User Dashboard data>>
		<param name="empno"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<UserDashboardDetail> getUserDashboarddata(string empno)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					UserDashboardDetail detail = new UserDashboardDetail();
					string deliverydate = DateTime.Now.ToString("yyyy-MM-dd");
					string query = WMSResource.getASNList;
					query = query + " where pomat.itemdeliverydate >= '" + deliverydate + " 00:00:00' and pomat.itemdeliverydate <= '" + deliverydate + " 23:59:59'";
					query = query + " group by pomat.pono, pomat.asnno,pomat.itemdeliverydate order by pomat.itemdeliverydate desc";


					await pgsql.OpenAsync();
					var expectedrcpts = await pgsql.QueryAsync<OpenPoModel>(
					   query, null, commandType: CommandType.Text);
					if (expectedrcpts != null && expectedrcpts.Count() > 0)
					{
						detail.pendingshipments = expectedrcpts.Count();

					}
					string receivedlistqry = WMSResource.getsecurityreceivedlist;
					receivedlistqry = receivedlistqry + " where sl.isdirectdelivered is not true and sl.deleteflag is not true and sl.invoicedate <= '" + deliverydate + " 23:59:59' and sl.invoicedate >= '" + deliverydate + " 00:00:00'";
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
					string queryrsrv = "select itemid as value from wms.materialreserve where reservedby = '" + empno + "' and reserveupto >= '" + sevendaysbeforestr + " 00:00:00' and reserveupto <= '" + deliverydate + " 23:59:59'";
					var rsvdata = await pgsql.QueryAsync<ddlmodel>(
					   queryrsrv, null, commandType: CommandType.Text);
					if (rsvdata != null && rsvdata.Count() > 0)
					{
						detail.reservedquantityforthisweek = rsvdata.Count();

					}

					string queryohhold = WMSResource.getHoldGRdetail.Replace("#status", "hold");
					var datahold = await pgsql.QueryAsync<OpenPoModel>(
						   queryohhold, null, commandType: CommandType.Text);


					detail.pendingonhold = datahold.Count();

					string materialrequestqueryx = WMSResource.grnlistfornotify;

					List<inwardModel> returnlistx = new List<inwardModel>();
					var datapendingtonotify = await pgsql.QueryAsync<inwardModel>(
					  materialrequestqueryx, null, commandType: CommandType.Text);




					foreach (inwardModel ddl in datapendingtonotify)
					{
						string validatequery = WMSResource.validategrnlistfornotify.Replace("#inwmasterid", ddl.inwmasterid.ToString());
						var datax = await pgsql.QueryAsync<ddlmodel>(
									validatequery, null, commandType: CommandType.Text);
						if (datax == null || datax.Count() == 0)
						{
							returnlistx.Add(ddl);
						}


					}


					detail.pendingnotifytofinance = returnlistx.Count();





					return detail;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getASNList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetItemlocationListBymterial_old>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get Item location List By mterial_old>>
		<param name="material"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> GetItemlocationListBymterial_old(string material)
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
					log.ErrorMessage("PODataProvider", "GetItemlocationListBymterial", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetItemlocationListBymterial>>  Author :<<Ramesh>>  
		Date of Creation <<06_10_2020>>
		Purpose : <<get itemlocation to issue materials>>
		<param name="material"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public async Task<IEnumerable<IssueRequestModel>> GetItemlocationListBymterial(string material)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getitemlocationforissue.Replace("#materialid", material);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetItemlocationListBymterial", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetItemlocationListBymterialanddesc>>  Author :<<Ramesh>>  
		Date of Creation <<29_01_2021>>
		Purpose : <<get itemlocation to issue materials>>
		<param name="material,description"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public async Task<IEnumerable<IssueRequestModel>> GetItemlocationListBymterialanddesc(string material, string description)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string descriptionstr = null;
					if (description != null)
					{
						descriptionstr = description.Replace("\'", "''");
					}
					string query = WMSResource.getitemlocationbymaterialdesc.Replace("#materialid", material).Replace("#desc", descriptionstr);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);
					data = data.OrderByDescending(o => o.initialstock);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetItemlocationListBymterialanddesc", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetItemLocationforplantstock>>  Author :<<Ramesh>>  
		Date of Creation <<11_05_2021>>
		Purpose : <<get itemlocation to issue materials>>
		<param name="material,description"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public async Task<IEnumerable<IssueRequestModel>> GetItemLocationforplantstock(string material, string description)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string descriptionstr = null;
					if (description != null)
					{
						descriptionstr = description.Replace("\'", "''");
					}
					string query = WMSResource.getitemlocationsforplantstock.Replace("#materialid", material).Replace("#desc", descriptionstr);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);
					data = data.OrderByDescending(o => o.initialstock);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetItemLocationforplantstock", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetItemLocationforplosstock>>  Author :<<Ramesh>>  
		Date of Creation <<21_05_2021>>
		Purpose : <<get itemlocation to issue materials>>
		<param name="material,description"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public async Task<IEnumerable<IssueRequestModel>> GetItemLocationforplosstock(string material, string description)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string descriptionstr = null;
					if (description != null)
					{
						descriptionstr = description.Replace("\'", "''");
					}
					string query = WMSResource.getitemlocationsforplosstock.Replace("#materialid", material).Replace("#desc", descriptionstr);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);
					data = data.OrderByDescending(o => o.initialstock);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetItemLocationforplosstock", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetItemlocationListBymterialanddesc>>  Author :<<Ramesh>>  
		Date of Creation <<29_01_2021>>
		Purpose : <<get itemlocation to issue materials>>
		<param name="material,description"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public async Task<IEnumerable<IssueRequestModel>> GetItemlocationListBymterialanddescpo(string material, string description, string projectid, string pono)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string descriptionstr = null;
					if (description != null)
					{
						descriptionstr = description.Replace("\'", "''");
					}
					string query = WMSResource.getitemlocationbymaterialdescpo.Replace("#materialid", material).Replace("#desc", descriptionstr).Replace("#projectid", projectid).Replace("#pono", pono);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetItemlocationListBymterialanddescpo", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetItemlocationwithStore>>  Author :<<Ramesh>>  
		Date of Creation <<26_05_2021>>
		Purpose : <<get itemlocation to issue materials>>
		<param name="material,description"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public async Task<IEnumerable<IssueRequestModel>> GetItemlocationwithStore(string material, string description, string projectid, string pono)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string descriptionstr = null;
					if (description != null)
					{
						descriptionstr = description.Replace("\'", "''");
					}
					string query = WMSResource.getItemlocationwithStore.Replace("#materialid", material).Replace("#desc", descriptionstr).Replace("#projectid", projectid).Replace("#pono", pono);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);
					foreach (IssueRequestModel mdl in data)
					{
						if (mdl.mrntotalissuedqty != null && mdl.mrntotalissuedqty > 0)
						{
							decimal? avlqty = mdl.availableqty - mdl.mrntotalissuedqty;
							if (avlqty < 0)
							{
								mdl.availableqty = 0;

							}
							else
							{
								mdl.availableqty = avlqty;
							}

						}
					}
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetItemlocationwithStore", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetItemlocationListBymterialanddesc>>  Author :<<Ramesh>>  
		Date of Creation <<29_01_2021>>
		Purpose : <<get itemlocation to issue materials>>
		<param name="material,description"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public async Task<IEnumerable<IssueRequestModel>> GetItemLocationListByMaterialdescstore(string material, string description, string store, string projectid, string pono)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string descriptionstr = null;
					if (description != null)
					{
						descriptionstr = description.Replace("\'", "''");
					}
					string query = WMSResource.getitemlocationbystock.Replace("#materialid", material).Replace("#desc", descriptionstr).Replace("#store", store).Replace("#project", projectid).Replace("#pono", pono);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);
					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetItemlocationListBymterialanddesc", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getplantstockmaterialdetails>>  Author :<<Ramesh>>  
		Date of Creation <<11_05_2021>>
		Purpose : <<get itemlocation to issue materials>>
		<param name="material,description"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public async Task<IssueRequestModel> getplantstockmaterialdetails(string material, string description)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string descriptionstr = null;
					if (description != null)
					{
						descriptionstr = description.Replace("\'", "''");
					}
					string query = WMSResource.getMaterialdetailsforplantstock.Replace("#materialid", material).Replace("#description", descriptionstr);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);
					return data.FirstOrDefault();

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getplantstockmaterialdetails", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		/*
		Name of Function : <<getplantstockmaterialdetails>>  Author :<<Ramesh>>  
		Date of Creation <<11_05_2021>>
		Purpose : <<get itemlocation to issue materials>>
		<param name="material,description"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public async Task<IssueRequestModel> getplosstockmaterialdetails(string material, string description)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string descriptionstr = null;
					if (description != null)
					{
						descriptionstr = description.Replace("\'", "''");
					}
					string query = WMSResource.getMaterialdetailsforplosstock.Replace("#materialid", material).Replace("#description", descriptionstr);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);
					return data.FirstOrDefault();

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getplosstockmaterialdetails", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetItemLocationListByMaterialdescpono>>  Author :<<Ramesh>>  
		Date of Creation <<19_02_2021>>
		Purpose : <<get itemlocation For PO Report>>
		<param name="material,description"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public async Task<IEnumerable<IssueRequestModel>> GetItemLocationListByMaterialdescpono(string material, string description, string pono)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string descriptionstr = null;
					if (description != null)
					{
						descriptionstr = description.Replace("\'", "''");
					}
					string query = WMSResource.getitemlocationbystock.Replace("#materialid", material).Replace("#desc", descriptionstr).Replace("#pono", pono);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);
					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetItemLocationListByMaterialdescpono", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getItemlocationListByIssueId>>  Author :<<Ramesh>>  
		Date of Creation <<10_01_2020>>
		Purpose : <<Get list of Material issued by issueid>>
		<param name="requestforissueid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		/// <summary>
		/// get materiallistfor stock transfer
		/// Ramesh  kumar 12_10_2020
		/// </summary>
		/// <param name="material"></param>
		/// <returns></returns>
		public async Task<IEnumerable<IssueRequestModel>> GetItemlocationListBymterialsourcelocation(string material, string description)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string descriptionstr = null;
					if (description != null)
					{
						descriptionstr = description.Replace("\'", "''");
					}
					string query = WMSResource.getitemlocationforstocktransfer.Replace("#materialid", material).Replace("#description", descriptionstr);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetItemlocationListBymterial", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		/// <summary>
		/// Get list of Material issued by issueid
		/// </summary>
		/// <param name="requestforissueid"></param>
		/// Revised by Ramesh 10_01_2020
		/// <returns></returns>
		public async Task<IEnumerable<IssueRequestModel>> getItemlocationListByIssueId(string requestforissueid, string requesttype)
		{

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					//string query = WMSResource.getitemlocationListBysIssueId.Replace("#requestforissueid", requestforissueid);
					//string query = WMSResource.getitemlocationListBysIssueId_v1.Replace("#requestforissueid", requestforissueid);
					string query = WMSResource.getitemlocationListBysIssueId_v2.Replace("#requestforissueid", requestforissueid).Replace("#type", requesttype);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					 query, null, commandType: CommandType.Text);
					data = data.OrderByDescending(o => o.createddate);

					IEnumerable<IssueRequestModel> result = data.GroupBy(c => new { c.itemlocation, c.createddate }).Select(t => new IssueRequestModel
					{

						availableqty = t.Sum(u => u.availableqty),
						issuedqty = t.Sum(u => u.issuedqty),
						pono = t.First().pono,
						materialid = t.First().materialid,
						itemid = t.First().itemid,
						Materialdescription = t.First().Materialdescription,
						material = t.First().material,
						itemlocation = t.Key.itemlocation,
						createddate = t.Key.createddate,
						stocktype = t.First().stocktype
					});
					return result;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetItemlocationListBymterial", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/// <summary>
		/// Get list of Material issued by issueid
		/// </summary>
		/// <param name="requestforissueid"></param>
		/// Revised by Ramesh 10_01_2020
		/// <returns></returns>
		public async Task<IEnumerable<IssueRequestModel>> getItemlocationListByIssueIdWithStore(string requestforissueid, string requesttype)
		{

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getissuedlocationwithStore.Replace("#requestforissueid", requestforissueid).Replace("#type", requesttype);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					 query, null, commandType: CommandType.Text);
					foreach (IssueRequestModel mdl in data)
					{
						if (mdl.itemlocation == "ON FLOOR")
						{
							decimal? diff = mdl.availableqty - mdl.mrntotalissuedqty;
							if (diff < 0)
							{
								mdl.availableqty = 0;
							}
							else
							{
								mdl.availableqty = diff;

							}
						}

					}
					data = data.OrderByDescending(o => o.createddate);

					IEnumerable<IssueRequestModel> result = data.GroupBy(c => new { c.itemlocation, c.createddate }).Select(t => new IssueRequestModel
					{

						availableqty = t.Sum(u => u.availableqty),
						issuedqty = t.Sum(u => u.issuedqty),
						pono = t.First().pono,
						materialid = t.First().materialid,
						itemid = t.First().itemid,
						Materialdescription = t.First().Materialdescription,
						material = t.First().material,
						itemlocation = t.Key.itemlocation,
						createddate = t.Key.createddate,
						stocktype = t.First().stocktype
					});



					return result;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getItemlocationListByIssueIdWithStore", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/// <summary>
		/// Get list of Material issued by issueid
		/// </summary>
		/// <param name="requestforissueid"></param>
		/// Revised by Ramesh 10_01_2020
		/// <returns></returns>
		public async Task<IEnumerable<IssueRequestModel>> getItemlocationListByPlantIssueId(string requestforissueid, string requesttype)
		{

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{

					string query = WMSResource.getPlantstockissuedlocations.Replace("#requestforissueid", requestforissueid).Replace("#type", requesttype);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					 query, null, commandType: CommandType.Text);

					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetItemlocationListBymterial", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/// <summary>
		/// Get list of Material issued by issueid
		/// </summary>
		/// <param name="requestforissueid"></param>
		/// Revised by Ramesh 10_01_2020
		/// <returns></returns>
		public async Task<IEnumerable<IssueRequestModel>> getItemlocationListByPlosIssueId(string requestforissueid, string requesttype)
		{

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{

					string query = WMSResource.getPlosstockissuedlocations.Replace("#requestforissueid", requestforissueid).Replace("#type", requesttype);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					 query, null, commandType: CommandType.Text);

					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetItemlocationListBymterial", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getItemlocationListByGatepassmaterialid>>  Author :<<LP>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get Item location List By Gatepass materialid>>
		<param name="gatepassmaterialid"></param>
		*/
		public async Task<IEnumerable<IssueRequestModel>> getItemlocationListByGatepassmaterialid(string gatepassmaterialid)
		{

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getItemlocationListByGatepassmaterialid_v1.Replace("#gatepassmaterialid", gatepassmaterialid);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					 query, null, commandType: CommandType.Text);
					data = data.OrderByDescending(o => o.createddate);

					//IEnumerable<IssueRequestModel> result = data.GroupBy(c => new { c.itemlocation, c.createddate }).Select(t => new IssueRequestModel
					//{

					//	availableqty = t.Sum(u => u.availableqty),
					//	issuedqty = t.Sum(u => u.issuedqty),
					//	pono = t.First().pono,
					//	materialid = t.First().materialid,
					//	itemid = t.First().itemid,
					//	Materialdescription = t.First().Materialdescription,
					//	material = t.First().material,
					//	itemlocation = t.Key.itemlocation,
					//	createddate = t.Key.createddate
					//});
					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetItemlocationListBymterial", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetItemlocationListforST>>  Author :<<Ramesh>>  
		Date of Creation <<29/07/2020>>
		Purpose : <<Get item location list for stock transfer>>
		<param name="material"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "GetItemlocationListBymterial", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<updateissuedmaterial>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<update issued material details>>
		<param name="obj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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

							decimal? availableqty = item.availableqty - item.issuedquantity;

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
					log.ErrorMessage("PODataProvider", "FIFOitemsupdate", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return 0;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}

		/*
		Name of Function : <<AssignRoles>>  Author :<<prasanna>>  
		Date of Creation <<10-06-2020>>
		Purpose : <<insert method to Asssign roles for employee>>
		<param name="model"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "assignRole", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
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
					log.ErrorMessage("PODataProvider", "getuserAcessList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
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
					var result = await pgsql.QueryAsync<userAcessNamesModel>(
					  query, null, commandType: CommandType.Text);
					List<userAcessNamesModel> AcList = new List<userAcessNamesModel>();
					AcList = result.ToList();
					var MatRes = AcList.Where(li => li.roleid == 5).ToList();
					var InvRes = AcList.Where(li => li.roleid == 2).ToList();
					var aaprRes = AcList.Where(li => li.roleid == 8).ToList();
					var PMRes = AcList.Where(li => li.roleid == 11).ToList();
					if (MatRes.Count() == 0 && employeeid != "000000" && employeeid != "000001")
					{
						userAcessNamesModel res = new userAcessNamesModel();
						res.roleid = 5;
						res.accessname = "Material Requester";
						res.subroleid = "1,2";
						AcList.Add(res);
					}
					if (InvRes.Count() == 0 && employeeid != "000000" && employeeid != "000001")
					{
						userAcessNamesModel res1 = new userAcessNamesModel();
						res1.roleid = 2;
						res1.accessname = "Inventory Enquiry";
						AcList.Add(res1);
					}
					if (PMRes.Count() == 0 && employeeid != "000000" && employeeid != "000001")
					{
						string querypm = "select projectmanager from wms.wms_project wp where projectmanager = '" + employeeid + "'";
						var rsltt = pgsql.ExecuteScalar(querypm, null);
						if (rsltt != null)
						{
							userAcessNamesModel res1 = new userAcessNamesModel();
							res1.roleid = 11;
							res1.accessname = "Project Manager";
							AcList.Add(res1);
						}
					}
					if (aaprRes.Count() == 0 && employeeid != "000000" && employeeid != "000001")
					{
						string empstr = config.FinanceEmployeeNo;
						string querypm = "select employeeno from wms.employee e where hodempno = '" + employeeid + "'";
						var rsltt = pgsql.ExecuteScalar(querypm, null);
						if (rsltt != null)
						{
							userAcessNamesModel res1 = new userAcessNamesModel();
							res1.roleid = 8;
							res1.accessname = "Approver";
							AcList.Add(res1);
						}
						else if (empstr.Contains(employeeid))
						{
							userAcessNamesModel res1 = new userAcessNamesModel();
							res1.roleid = 8;
							res1.accessname = "Approver";
							res1.isFinancemember = true;
							AcList.Add(res1);
						}
					}

					return AcList;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getuserroleList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
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
					log.ErrorMessage("PODataProvider", "getdashboarddata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getUserdashboardgraphdata>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get User  dashboard graph data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<UserDashboardGraphModel>> getUserdashboardgraphdata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{


				try
				{

					string rcvqry = "SELECT receiveddate::date as graphdate, COUNT(*),'Receive' as type ";
					rcvqry += " from wms.wms_storeinward ";
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
					log.ErrorMessage("PODataProvider", "getUserdashboardgraphdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getManagerdashboardgraphdata>>  Author :<<Gayathri>>  
		Date of Creation <<>>
		Purpose : <<get Manager  dashboard graph data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<ManagerDashboard> getManagerdashboardgraphdata(DashBoardFilters filters)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{


				try
				{

					//Get count of po and invoice for pending receipts
					string penqry = "select count(*) as pendingcount from wms.wms_securityinward where receiveddate>= '" + filters.fromDate + "' and receiveddate<= '" + filters.toDate + "' and grnnumber is null and  onhold is NOT True and (holdgrstatus is NULL or holdgrstatus =  'accepted') ";

					//Get count of po and invoice for on hold receipts
					string onholdqry = "select count(*) as onholdcount from wms.wms_securityinward where receiveddate>= '" + filters.fromDate + "' and receiveddate<= '" + filters.toDate + "' and grnnumber is null and onhold = true ";

					//Get count of po and invoice for complete receipts
					string completeqry = "select count(*) as completedcount from wms.wms_securityinward where receiveddate>= '" + filters.fromDate + "' and receiveddate<= '" + filters.toDate + "' and grnnumber is not null and onhold is null ";

					//Get count of quality check completed
					string qualitycompl = "select count(*),COUNT(*) OVER () as  qualitycompcount from wms.wms_storeinward where receiveddate>= '" + filters.fromDate + "' and receiveddate<= '" + filters.toDate + "'  and qualitychecked =true group by inwmasterid";

					//Get count of quality check pending
					string qualitypending = "select count(*),COUNT(*) OVER () as qualitypendcount from wms.wms_storeinward stinw left outer join wms.wms_qualitycheck qc on qc.inwardid = stinw.inwardid where stinw.receiveddate>= '" + filters.fromDate + "' and stinw.receiveddate<= '" + filters.toDate + "'  and stinw.qualitycheckrequired =true and stinw.receivedqty > 0 and qc.qcby is null  group by inwmasterid";

					//Get count of pending GRN's - putaway 
					//string putawaypend = " select count(*),COUNT(*) OVER () as putawaypendcount from wms.wms_securityinward secinw  where secinw.inwmasterid not in (select distinct inwmasterid  from wms.wms_stock where inwmasterid is not null order by inwmasterid desc) and receiveddate> now() - interval '1 month' group by secinw.inwmasterid ";
					string putawaypend = " select sinw.grnnumber,COUNT(*) OVER() as putawaypendcount from wms.wms_storeinward stinw join wms.wms_securityinward sinw on stinw.inwmasterid = sinw.inwmasterid where stinw.returnedby is not null and sinw.isdirecttransferred is NOT true and stinw.inwardid not in (select distinct inwardid from wms.wms_stock where inwardid is not null   order by inwardid desc) and stinw.receiveddate>= '" + filters.fromDate + "' and stinw.receiveddate<= '" + filters.toDate + "' group by sinw.grnnumber";

					//Get count of completed GRN's - putaway 
					string putawaycomp = " select sinw.grnnumber,COUNT(*) OVER () as putawaycompcount from wms.wms_storeinward stinw  join wms.wms_securityinward sinw on stinw.inwmasterid = sinw.inwmasterid where stinw.returnedby is not null and sinw.isdirecttransferred is NOT true and stinw.inwardid  in (select distinct inwardid from wms.wms_stock where inwardid is not null   order by inwardid desc) and stinw.receiveddate>= '" + filters.fromDate + "' and stinw.receiveddate<= '" + filters.toDate + "' group by sinw.grnnumber";

					//Get count of In progress GRN's - putaway
					string putawayinprogres = " select sinw.grnnumber,COUNT(*) OVER () as putawayinprocount from wms.wms_storeinward stinw join wms.wms_securityinward sinw on stinw.inwmasterid = sinw.inwmasterid where stinw.returnedby is not null and sinw.isdirecttransferred is NOT true and stinw.inwardid not in (select distinct inwardid from wms.wms_stock where inwardid is not null   order by inwardid desc) and stinw.receiveddate>= '" + filters.fromDate + "' and stinw.receiveddate<= '" + filters.toDate + "'  group by sinw.grnnumber";

					//Get count of pending GRN's - Acceptance 
					string acceptancepenqry = "select count(*),COUNT(*) OVER () as acceptancependcount from wms.wms_storeinward stin where receiveddate>= '" + filters.fromDate + "' and receiveddate<= '" + filters.toDate + "' and returnqty is null and confirmqty is null   group by(inwmasterid)";

					//Get count of Accepted GRN's - Acceptance 
					string acceptancecomptqry = "select count(*),COUNT(*) OVER () as acceptancecompcount from wms.wms_storeinward stin where receiveddate>= '" + filters.fromDate + "' and receiveddate<= '" + filters.toDate + "' and returnqty is not null and confirmqty is not null  group by(inwmasterid)";

					var data1 = await pgsql.QueryAsync<ManagerDashboard>(penqry, null, commandType: CommandType.Text);
					var data2 = await pgsql.QueryAsync<ManagerDashboard>(onholdqry, null, commandType: CommandType.Text);
					var data3 = await pgsql.QueryAsync<ManagerDashboard>(completeqry, null, commandType: CommandType.Text);
					var data4 = await pgsql.QueryAsync<ManagerDashboard>(qualitycompl, null, commandType: CommandType.Text);
					var data5 = await pgsql.QueryAsync<ManagerDashboard>(qualitypending, null, commandType: CommandType.Text);
					var data6 = await pgsql.QueryAsync<ManagerDashboard>(putawaypend, null, commandType: CommandType.Text);
					var data7 = await pgsql.QueryAsync<ManagerDashboard>(putawaycomp, null, commandType: CommandType.Text);
					var data8 = await pgsql.QueryAsync<ManagerDashboard>(putawayinprogres, null, commandType: CommandType.Text);
					var data9 = await pgsql.QueryAsync<ManagerDashboard>(acceptancepenqry, null, commandType: CommandType.Text);
					var data10 = await pgsql.QueryAsync<ManagerDashboard>(acceptancecomptqry, null, commandType: CommandType.Text);

					var data = new ManagerDashboard();
					if (data1.Count() > 0)
					{
						data.pendingcount = data1.Count() > 0 ? data1.FirstOrDefault().pendingcount : 0;
					}
					if (data2.Count() > 0)
					{
						data.onholdcount = data2.Count() > 0 ? data2.FirstOrDefault().onholdcount : 0;
					}
					if (data3.Count() > 0)
					{
						data.completedcount = data3.Count() > 0 ? data3.FirstOrDefault().completedcount : 0;
					}

					if (data4.Count() > 0)
					{
						data.qualitycompcount = data4.FirstOrDefault().qualitycompcount;
					}
					if (data5.Count() > 0)
					{
						data.qualitypendcount = data5.Count() > 0 ? data5.FirstOrDefault().qualitypendcount : 0;
					}
					if (data6.Count() > 0)
					{
						data.putawaypendcount = data6.Count() > 0 ? data6.FirstOrDefault().putawaypendcount : 0;
					}

					if (data7.Count() > 0)
					{
						data.putawaycompcount = data7.FirstOrDefault().putawaycompcount;
					}
					if (data8.Count() > 0)
					{
						data.putawayinprocount = data8.Count() > 0 ? data8.FirstOrDefault().putawayinprocount : 0;
					}
					if (data9.Count() > 0)
					{
						data.acceptancependcount = data9.Count() > 0 ? data9.FirstOrDefault().acceptancependcount : 0;
					}
					if (data10.Count() > 0)
					{
						data.acceptancecompcount = data10.FirstOrDefault().acceptancecompcount;
					}


					return data;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getManagerdashboardgraphdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}




		/*
       Name of Function : <<getWeeklyUserdashboardReceive>>  Author :<<LP>>  
       Date of Creation <<12-12-2019>>
       Purpose : <<get Weekly User dashboard graphdata>>
       Review Date :<<>>   Reviewed By :<<>>
       */
		public async Task<IEnumerable<GraphModelNew>> getWeeklyUserdashboardReceive(DashBoardFilters filters)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					List<GraphModelNew> rcvdata = new List<GraphModelNew>();
					string rcvquery = WMSResource.dataforreceivedgraph.Replace("fromdate", filters.fromDate).Replace("todate", filters.toDate);
					var data1 = await pgsql.QueryAsync<GraphModelNew>(rcvquery, null, commandType: CommandType.Text);
					if (data1 != null && data1.Count() > 0)
					{
						int i = 1;
						foreach (GraphModelNew grph in data1)
						{
							string week = grph.sweek;
							var insertcheck = rcvdata.Where(o => o.sweek == week).FirstOrDefault();
							if (insertcheck == null)
							{
								GraphModelNew obj = new GraphModelNew();
								obj.displayweek = "Week" + i;
								obj.sweek = grph.sweek;
								obj.smonth = grph.smonth;
								obj.syear = grph.syear;
								obj.total = data1.Where(o => o.sweek == grph.sweek).Count().ToString();
								obj.received = data1.Where(o => o.sweek == grph.sweek && o.grnnumber != null).Count().ToString();
								obj.pending = data1.Where(o => o.sweek == grph.sweek && o.grnnumber == null).Count().ToString();
								rcvdata.Add(obj);
								i++;
							}
						}
					}

					return rcvdata;
				}
				catch (Exception Ex)
				{
					string msg = Ex.Message;
					return null;
				}

			}

		}


		/*
      Name of Function : <<getWeeklyUserdashboardQuality>>  Author :<<LP>>  
      Date of Creation <<12-12-2019>>
      Purpose : <<get Weekly User dashboard graphdata>>
      Review Date :<<>>   Reviewed By :<<>>
      */
		public async Task<IEnumerable<GraphModelNew>> getWeeklyUserdashboardQuality(DashBoardFilters filters)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					List<GraphModelNew> rcvdata = new List<GraphModelNew>();
					string rcvquery = WMSResource.dataforqualitygraph.Replace("fromdate", filters.fromDate).Replace("todate", filters.toDate); ;
					var data1 = await pgsql.QueryAsync<GraphModelNew>(rcvquery, null, commandType: CommandType.Text);
					if (data1 != null && data1.Count() > 0)
					{
						int i = 1;
						foreach (GraphModelNew grph in data1)
						{
							string week = grph.sweek;
							var insertcheck = rcvdata.Where(o => o.sweek == week).FirstOrDefault();
							if (insertcheck == null)
							{
								GraphModelNew obj = new GraphModelNew();
								obj.displayweek = "Week" + i;
								obj.sweek = grph.sweek;
								obj.smonth = grph.smonth;
								obj.syear = grph.syear;
								obj.total = data1.Where(o => o.sweek == grph.sweek).Count().ToString();
								obj.received = data1.Where(o => o.sweek == grph.sweek && o.qcby != null).Count().ToString();
								obj.pending = data1.Where(o => o.sweek == grph.sweek && o.qcby == null).Count().ToString();
								rcvdata.Add(obj);
								i++;
							}
						}
					}

					return rcvdata;
				}
				catch (Exception Ex)
				{
					string msg = Ex.Message;
					return null;
				}

			}

		}


		/*
    Name of Function : <<getWeeklyUserdashboardAccept>>  Author :<<LP>>  
    Date of Creation <<12-12-2019>>
    Purpose : <<get Weekly User dashboard graphdata>>
    Review Date :<<>>   Reviewed By :<<>>
    */
		public async Task<IEnumerable<GraphModelNew>> getWeeklyUserdashboardAccept(DashBoardFilters filters)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					List<GraphModelNew> rcvdata = new List<GraphModelNew>();
					string rcvquery = WMSResource.dataforqualitygraph.Replace("fromdate", filters.fromDate).Replace("todate", filters.toDate); ;
					var data1 = await pgsql.QueryAsync<GraphModelNew>(rcvquery, null, commandType: CommandType.Text);
					if (data1 != null && data1.Count() > 0)
					{
						int i = 1;
						foreach (GraphModelNew grph in data1)
						{
							string week = grph.sweek;
							var insertcheck = rcvdata.Where(o => o.sweek == week).FirstOrDefault();
							if (insertcheck == null)
							{
								GraphModelNew obj = new GraphModelNew();
								obj.displayweek = "Week" + i;
								obj.sweek = grph.sweek;
								obj.smonth = grph.smonth;
								obj.syear = grph.syear;
								obj.total = data1.Where(o => o.sweek == grph.sweek).Count().ToString();
								obj.received = data1.Where(o => o.sweek == grph.sweek && o.confirmqty != null).Count().ToString();
								obj.pending = data1.Where(o => o.sweek == grph.sweek && o.confirmqty == null).Count().ToString();
								rcvdata.Add(obj);
								i++;
							}
						}
					}

					return rcvdata;
				}
				catch (Exception Ex)
				{
					string msg = Ex.Message;
					return null;
				}

			}

		}

		/*
   Name of Function : <<getWeeklyUserdashboardPutaway>>  Author :<<LP>>  
   Date of Creation <<12-12-2019>>
   Purpose : <<get Weekly User dashboard graphdata>>
   Review Date :<<>>   Reviewed By :<<>>
   */
		public async Task<IEnumerable<GraphModelNew>> getWeeklyUserdashboardPutaway(DashBoardFilters filters)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					List<GraphModelNew> rcvdata = new List<GraphModelNew>();
					string rcvquery = WMSResource.dataforputawaygraph.Replace("fromdate", filters.fromDate).Replace("todate", filters.toDate);
					var data1 = await pgsql.QueryAsync<GraphModelNew>(rcvquery, null, commandType: CommandType.Text);
					if (data1 != null && data1.Count() > 0)
					{
						int i = 1;
						foreach (GraphModelNew grph in data1)
						{
							string week = grph.sweek;
							var insertcheck = rcvdata.Where(o => o.sweek == week).FirstOrDefault();
							if (insertcheck == null)
							{
								GraphModelNew obj = new GraphModelNew();
								obj.displayweek = "Week" + i;
								obj.sweek = grph.sweek;
								obj.smonth = grph.smonth;
								obj.syear = grph.syear;
								obj.total = data1.Where(o => o.sweek == grph.sweek).Count().ToString();
								obj.received = data1.Where(o => o.sweek == grph.sweek && o.status == "Received").Count().ToString();
								obj.pending = data1.Where(o => o.sweek == grph.sweek && o.status == "Pending").Count().ToString();
								rcvdata.Add(obj);
								i++;
							}
						}
					}

					return rcvdata;
				}
				catch (Exception Ex)
				{
					string msg = Ex.Message;
					return null;
				}

			}

		}


		/*
   Name of Function : <<getWeeklyUserdashboardRequest>>  Author :<<LP>>  
   Date of Creation <<12-12-2019>>
   Purpose : <<get Weekly User dashboard graphdata>>
   Review Date :<<>>   Reviewed By :<<>>
   */
		public async Task<IEnumerable<GraphModelNew>> getWeeklyUserdashboardRequest(DashBoardFilters filters)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					List<GraphModelNew> rcvdata = new List<GraphModelNew>();
					string rcvquery = WMSResource.dataforrequestgraph.Replace("fromdate", filters.fromDate).Replace("todate", filters.toDate);
					var data1 = await pgsql.QueryAsync<GraphModelNew>(rcvquery, null, commandType: CommandType.Text);
					if (data1 != null && data1.Count() > 0)
					{
						int i = 1;
						foreach (GraphModelNew grph in data1)
						{
							string week = grph.sweek;
							var insertcheck = rcvdata.Where(o => o.sweek == week).FirstOrDefault();
							if (insertcheck == null)
							{
								GraphModelNew obj = new GraphModelNew();
								obj.displayweek = "Week" + i;
								obj.sweek = grph.sweek;
								obj.smonth = grph.smonth;
								obj.syear = grph.syear;
								obj.total = data1.Where(o => o.sweek == grph.sweek).Count().ToString();
								obj.received = data1.Where(o => o.sweek == grph.sweek && o.issuedon != null && o.requestid != null).Count().ToString();
								obj.pending = data1.Where(o => o.sweek == grph.sweek && o.issuedon == null && o.requestid != null).Count().ToString();
								rcvdata.Add(obj);
								i++;
							}
						}
					}

					return rcvdata;
				}
				catch (Exception Ex)
				{
					string msg = Ex.Message;
					return null;
				}

			}

		}

		/*
   Name of Function : <<getWeeklyUserdashboardReturn>>  Author :<<LP>>  
   Date of Creation <<12-12-2019>>
   Purpose : <<get Weekly User dashboard graphdata>>
   Review Date :<<>>   Reviewed By :<<>>
   */
		public async Task<IEnumerable<GraphModelNew>> getWeeklyUserdashboardReturn(DashBoardFilters filters)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					List<GraphModelNew> rcvdata = new List<GraphModelNew>();
					string rcvquery = WMSResource.dataforreturngraph.Replace("fromdate", filters.fromDate).Replace("todate", filters.toDate); ;
					var data1 = await pgsql.QueryAsync<GraphModelNew>(rcvquery, null, commandType: CommandType.Text);
					if (data1 != null && data1.Count() > 0)
					{
						int i = 1;
						foreach (GraphModelNew grph in data1)
						{
							string week = grph.sweek;
							var insertcheck = rcvdata.Where(o => o.sweek == week).FirstOrDefault();
							if (insertcheck == null)
							{
								GraphModelNew obj = new GraphModelNew();
								obj.displayweek = "Week" + i;
								obj.sweek = grph.sweek;
								obj.smonth = grph.smonth;
								obj.syear = grph.syear;
								obj.total = data1.Where(o => o.sweek == grph.sweek).Count().ToString();
								obj.received = data1.Where(o => o.sweek == grph.sweek && o.returnid != null && o.confirmstatus == "Accepted").Count().ToString();
								obj.pending = data1.Where(o => o.sweek == grph.sweek && o.returnid != null && o.confirmstatus == null).Count().ToString();
								rcvdata.Add(obj);
								i++;
							}
						}
					}

					return rcvdata;
				}
				catch (Exception Ex)
				{
					string msg = Ex.Message;
					return null;
				}

			}

		}


		/*
  Name of Function : <<getWeeklyUserdashboardReserve>>  Author :<<LP>>  
  Date of Creation <<12-12-2019>>
  Purpose : <<get Weekly User dashboard graphdata>>
  Review Date :<<>>   Reviewed By :<<>>
  */
		public async Task<IEnumerable<GraphModelNew>> getWeeklyUserdashboardReserve(DashBoardFilters filters)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					List<GraphModelNew> rcvdata = new List<GraphModelNew>();
					string rcvquery = WMSResource.dataforreservegraph.Replace("fromdate", filters.fromDate).Replace("todate", filters.toDate); ;
					var data1 = await pgsql.QueryAsync<GraphModelNew>(rcvquery, null, commandType: CommandType.Text);
					if (data1 != null && data1.Count() > 0)
					{
						int i = 1;
						foreach (GraphModelNew grph in data1)
						{
							string week = grph.sweek;
							var insertcheck = rcvdata.Where(o => o.sweek == week).FirstOrDefault();
							if (insertcheck == null)
							{
								GraphModelNew obj = new GraphModelNew();
								obj.displayweek = "Week" + i;
								obj.sweek = grph.sweek;
								obj.smonth = grph.smonth;
								obj.syear = grph.syear;
								obj.total = data1.Where(o => o.sweek == grph.sweek).Count().ToString();
								obj.received = data1.Where(o => o.sweek == grph.sweek && o.reserveid != null).Count().ToString();
								obj.pending = data1.Where(o => o.sweek == grph.sweek && o.reserveid == null).Count().ToString();
								rcvdata.Add(obj);
								i++;
							}
						}
					}

					return rcvdata;
				}
				catch (Exception Ex)
				{
					string msg = Ex.Message;
					return null;
				}

			}

		}


		/*
  Name of Function : <<getWeeklyUserdashboardtransfer>>  Author :<<LP>>  
  Date of Creation <<12-12-2019>>
  Purpose : <<get Weekly User dashboard graphdata>>
  Review Date :<<>>   Reviewed By :<<>>
  */
		public async Task<IEnumerable<GraphModelNew>> getWeeklyUserdashboardtransfer(DashBoardFilters filters)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					List<GraphModelNew> rcvdata = new List<GraphModelNew>();
					string rcvquery = WMSResource.datafortransfergraph.Replace("fromdate", filters.fromDate).Replace("todate", filters.toDate); ;
					var data1 = await pgsql.QueryAsync<GraphModelNew>(rcvquery, null, commandType: CommandType.Text);
					if (data1 != null && data1.Count() > 0)
					{
						int i = 1;
						foreach (GraphModelNew grph in data1)
						{
							string week = grph.sweek;
							var insertcheck = rcvdata.Where(o => o.sweek == week).FirstOrDefault();
							if (insertcheck == null)
							{
								GraphModelNew obj = new GraphModelNew();
								obj.displayweek = "Week" + i;
								obj.sweek = grph.sweek;
								obj.smonth = grph.smonth;
								obj.syear = grph.syear;
								obj.total = data1.Where(o => o.sweek == grph.sweek).Count().ToString();
								obj.received = data1.Where(o => o.sweek == grph.sweek && o.approvaldate != null).Count().ToString();
								obj.pending = data1.Where(o => o.sweek == grph.sweek && o.approvaldate == null).Count().ToString();
								rcvdata.Add(obj);
								i++;
							}
						}
					}

					return rcvdata;
				}
				catch (Exception Ex)
				{
					string msg = Ex.Message;
					return null;
				}

			}

		}



		/*
        Name of Function : <<getWeeklyUserdashboardgraphdata>>  Author :<<LP>>  
        Date of Creation <<12-12-2019>>
        Purpose : <<get Weekly User dashboard graphdata>>
        Review Date :<<>>   Reviewed By :<<>>
        */
		public async Task<IEnumerable<UserDashboardGraphModel>> getWeeklyUserdashboardgraphdata(DashBoardFilters filters)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{


				try
				{

					string rcvqry = "SELECT date_part('year', receiveddate::date) as syear,";
					rcvqry += " date_part('week', receiveddate::date) AS sweek,COUNT(*) as count, 'Receive' as type";
					rcvqry += " FROM wms.wms_securityinward where receiveddate is not null and receiveddate > now() - interval '1 month' and extract(year from receiveddate) > 2000";
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
					log.ErrorMessage("PODataProvider", "getWeeklyUserdashboardgraphdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getmonthlyUserdashboardgraphdata>>  Author :<<LP>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get monthly User dashboard graphdata>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<UserDashboardGraphModel>> getmonthlyUserdashboardgraphdata(DashBoardFilters filters)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{


				try
				{
					string rcvqry = "SELECT date_part('month', receiveddate::date) as smonth,";
					rcvqry += " concat('week',date_part('week', receiveddate::date) ) AS sweek,COUNT(*) as count, 'Receive' as type";
					rcvqry += " FROM  wms.wms_securityinward where receiveddate is not null and receiveddate > now() - interval '1 month'  and grnnumber is not null and onhold is null";
					rcvqry += " GROUP BY smonth, sweek ORDER BY smonth , sweek";


					string qcqry = "SELECT date_part('month', qcdate::date) as smonth,";
					qcqry += " concat('week',date_part('week', qcdate::date) ) AS sweek,COUNT(*) as count, 'Quality' as type";
					qcqry += " FROM wms.wms_qualitycheck where qcdate is not null and qcdate > now() - interval '1 month' ";
					qcqry += " GROUP BY smonth, sweek ORDER BY smonth, sweek";

					string accqry = "SELECT date_part('month', returnedon::date) as smonth,";
					accqry += " concat('week',date_part('week', returnedon::date) ) AS sweek,COUNT(*) as count, 'Accept' as type";
					accqry += " FROM wms.wms_storeinward where returnedon is not null and returnedon > now() - interval '1 month'  ";
					accqry += " GROUP BY smonth, sweek ORDER BY smonth, sweek";

					string pwqry = "SELECT date_part('month', createddate::date) as smonth,";
					pwqry += " concat('week',date_part('week', createddate::date) ) AS sweek,COUNT(*) as count, 'Putaway' as type";
					pwqry += " FROM wms.wms_stock where createddate is not null and createddate > now() - interval '1 month' and  initialstock is not True ";
					pwqry += " GROUP BY smonth, sweek ORDER BY smonth, sweek";



					var data1 = await pgsql.QueryAsync<UserDashboardGraphModel>(rcvqry, null, commandType: CommandType.Text);
					var data2 = await pgsql.QueryAsync<UserDashboardGraphModel>(qcqry, null, commandType: CommandType.Text);
					var data3 = await pgsql.QueryAsync<UserDashboardGraphModel>(accqry, null, commandType: CommandType.Text);
					var data4 = await pgsql.QueryAsync<UserDashboardGraphModel>(pwqry, null, commandType: CommandType.Text);

					var data = data1.Concat(data2.Concat(data3.Concat(data4)));

					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getmonthlyUserdashboardgraphdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<MaterialRequestdata>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Material Request data>>
		<param name="approverid"></param>
		 <param name="pono"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> MaterialRequestdata(string pono, string approverid, string projectcode)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string materialrequestquery = WMSResource.getmaterialdetailfprrequest.Replace("#manager", approverid);
					if (pono != null && pono != "undefined" && pono != "null")
					{
						materialrequestquery = materialrequestquery + " and (sk.pono = '" + pono + "' or po.suppliername = '" + pono + "')";
					}
					if (projectcode != null && projectcode != "undefined" && projectcode != "null")
					{
						materialrequestquery = materialrequestquery + " and prj.projectcode = '" + projectcode + "'";
					}
					materialrequestquery = materialrequestquery + " group by  sk.poitemdescription, sk.materialid";
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					return data.Where(o => o.availableqty > 0);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "MaterialRequestdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<MaterialRequestdata>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Material Request data>>
		<param name="approverid"></param>
		 <param name="pono"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> MaterialRequestdataforgatepass(string pono, string projectcode)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					pono = pono.Replace("\"", "\'");
					string materialrequestquery = WMSResource.getmaterialsforgatepass.Replace("#projectid", projectcode).Replace("#pono", pono);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					return data.Where(o => o.availableqty > 0);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "MaterialRequestdataforgatepass", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getmaterialswithstore>>  Author :<<Ramesh>>  
		Date of Creation <<26-05-2021>>
		Purpose : <<Material Request data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> getmaterialswithstore(string pono, string projectcode)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					pono = pono.Replace("\"", "\'");
					string materialrequestquery = WMSResource.getMateralfromstockstore.Replace("#projectid", projectcode).Replace("#pono", pono);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					foreach (IssueRequestModel mdl in data)
					{
						if (mdl.mrnissuedqty != null && mdl.mrnissuedqty > 0)
						{
							decimal? avlqty = mdl.availableqty - mdl.mrnissuedqty;
							if (avlqty < 0)
							{
								mdl.availableqty = 0;
							}
							else
							{
								mdl.availableqty = mdl.availableqty - mdl.mrnissuedqty;
							}
						}
					}
					IEnumerable<IssueRequestModel> result = data.GroupBy(c => new { c.materialid, c.Materialdescription, c.pono }).Select(t => new IssueRequestModel
					{
						availableqty = t.Sum(u => u.availableqty),
						pono = t.First().pono,
						materialid = t.First().materialid,
						Materialdescription = t.First().Materialdescription,
						material = t.First().material,
						unitprice = t.First().unitprice,
					});
					return result;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getmaterialswithstore", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
	Name of Function : <<MaterialRequestdata>>  Author :<<Ramesh>>  
	Date of Creation <<12-12-2019>>
	Purpose : <<Material Request data>>
	<param name="approverid"></param>
	 <param name="pono"></param>
	Review Date :<<>>   Reviewed By :<<>>
	*/
		public async Task<IEnumerable<IssueRequestModel>> MaterialRequestdataforsto(string pono, string projectcode, string store)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string materialrequestquery = WMSResource.getmaterialsforsto.Replace("#projectid", projectcode).Replace("#pono", pono).Replace("#storeid", store);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					return data.Where(o => o.availableqty > 0);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "MaterialRequestdataforgatepass", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		/*
		Name of Function : <<MaterialReservedata>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Material Reserve data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> MaterialReservedata(string projectcode)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string materialrequestquery = WMSResource.getmaterialstoreserve.Replace("#projectcode", projectcode);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "MaterialRequestdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<MaterialReservedata>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Material Reserve data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> MaterialReservedata_v1()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string materialrequestquery = WMSResource.getplantstockmaterials;
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "MaterialRequestdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getgatepassmaterialrequestList>>  Author :<<Ramesh>>  
		Date of Creation <<12-01-2021>>
		Purpose : <<Material list for gate pass>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> getgatepassmaterialrequestList()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string materialrequestquery = WMSResource.getdefaultmaterialforgatepass;
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "MaterialRequestdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getempnamebycode>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get emp name by code>>
		<param name="empno"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<User> getempnamebycode(string empno)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					await pgsql.OpenAsync();

					string userquery = "select  * from wms.employee where employeeno='" + empno + "'";
					return pgsql.QuerySingle<User>(
					   userquery, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getempnamebycode", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getissuematerialdetails>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get issue material details>>
		<param name="requestid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "getissuematerialdetails", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<insertResevematerial_old>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<insert Resevematerial_old>>
		<param name="datamodel"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<int> insertResevematerial_old(List<ReserveMaterialModel> datamodel)
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
					//reserveid = obj.reserveid + 1;
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
						string itemnoquery = WMSResource.getitemiddata.Replace("#materialid", item.material);
						if (item.pono != null)
						{
							itemnoquery = itemnoquery + "and pono='" + item.pono + "'";
						}
						using (var DB = new NpgsqlConnection(config.PostgresConnectionString))
						{

							DB.Open();
							//await DB.OpenAsync();
							var itemData = await DB.QueryAsync<StockModel>(
							itemnoquery, null, commandType: CommandType.Text);
							decimal? remainingqty = item.quantity;
							itemData = itemData.OrderBy(o => o.createddate);
							decimal? reservedqty = item.reservedqty;
							decimal? Totalreservedqty = 0;
							foreach (StockModel data in itemData)
							{
								if (item.pono == null)
								{
									item.pono = data.pono;
								}
								if (reservedqty >= 0)
								{
									if (reservedqty <= data.availableqty)
										reservedqty = reservedqty;
									else
										reservedqty = data.availableqty;
									item.itemid = data.itemid;

									result = DB.Execute(insertquery, new
									{
										item.materialid,
										item.itemid,
										item.pono,
										item.reservedby,
										reservedqty,
										reserveid,
										item.reserveupto,
										item.projectcode,
										item.remarks
									});


									//update stock table
									if (result != 0)
									{
										//int availableqty = item.availableqty - reserveQty;
										string updatequery = WMSResource.updateqtyafterissue.Replace("#itemid", Convert.ToString(item.itemid)).Replace("#issuedqty", Convert.ToString(reservedqty));
										//WMSResource.updatestock.Replace("#availableqty", Convert.ToString(availableqty)).Replace("#itemid", Convert.ToString(item.itemid));
										using (IDbConnection pgsql = new NpgsqlConnection(config.PostgresConnectionString))
										{
											result = pgsql.Execute(updatequery, new
											{
											});
										}
										Totalreservedqty += reservedqty;
										reservedqty = item.reservedqty - reservedqty;
									}
									//break;

								}
								if (Totalreservedqty >= item.reservedqty)
									break;

							}
						}
					}
				}

				return (Convert.ToInt32(result));
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "insertResevematerial", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return 0;
			}
		}


		/*
		Name of Function : <<insertResevematerial>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Reserve material + Email-->>
		<param name="datamodel"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<int> insertResevematerial(List<ReserveMaterialModel> datamodel)
		{
			int reserveid = 0;
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				NpgsqlTransaction Trans = null;

				try
				{
					pgsql.Open();
					Trans = pgsql.BeginTransaction();
					string materials = "";
					MaterialTransaction mainmodel = new MaterialTransaction();

					mainmodel.pono = datamodel[0].pono;
					mainmodel.projectcode = datamodel[0].projectcode;
					mainmodel.remarks = datamodel[0].remarks;
					mainmodel.reservedby = datamodel[0].requesterid;
					mainmodel.reserveupto = datamodel[0].reserveupto;
					string insertmatquery = WMSResource.insertmaterialreserve;
					var result = pgsql.ExecuteScalar(insertmatquery, new
					{

						mainmodel.pono,
						mainmodel.reservedby,
						mainmodel.reserveupto,
						mainmodel.projectcode,
						mainmodel.remarks
					});
					if (result != null)
					{
						List<MaterialTransactionDetail> reservelistformail = new List<MaterialTransactionDetail>();
						int stindex = 0;
						foreach (var item in datamodel)
						{
							if (stindex > 0)
							{
								materials += ", ";
							}
							materials += item.material;

							MaterialTransactionDetail detail = new MaterialTransactionDetail();
							detail.id = Guid.NewGuid().ToString();
							detail.reserveid = result.ToString();
							detail.materialid = item.material;
							detail.reservedqty = item.quantity;
							detail.itemid = item.itemid;
							detail.poitemdescription = item.materialdescription;
							reservelistformail.Add(detail);
							string descriptionstr = null;
							if (item.materialdescription != null)
							{
								descriptionstr = item.materialdescription.Replace("\'", "''");
							}
							string itemnoquery = WMSResource.getitemiddata.Replace("#materialid", item.material).Replace("#desc", descriptionstr);
							var itemData = await pgsql.QueryAsync<StockModel>(
							itemnoquery, null, commandType: CommandType.Text);
							decimal? remainingqty = item.quantity;
							itemData = itemData.OrderBy(o => o.createddate);
							decimal? reservedqty = item.quantity;
							decimal? Totalreservedqty = 0;
							string insertdataqry = WMSResource.insertmaterialreservedetails;
							foreach (StockModel data in itemData)
							{
								if (item.pono == null)
								{
									item.pono = data.pono;
								}
								if (reservedqty >= 0)
								{
									if (reservedqty <= data.availableqty)
										reservedqty = reservedqty;
									else
										reservedqty = data.availableqty;
									item.itemid = data.itemid;
									string id = Guid.NewGuid().ToString();
									var result1 = pgsql.Execute(insertdataqry, new
									{

										id,
										detail.reserveid,
										detail.materialid,
										data.itemid,
										reservedqty,
										data.poitemdescription
									});




									//update stock table
									if (result1 != 0)
									{
										string updatequery = WMSResource.updateqtyafterissue.Replace("#itemid", Convert.ToString(item.itemid)).Replace("#issuedqty", Convert.ToString(reservedqty));
										var result3 = pgsql.Execute(updatequery, new
										{
										});

										Totalreservedqty += reservedqty;
										reservedqty = item.quantity - reservedqty;
									}
									//break;

								}
								stindex++;
								if (Totalreservedqty >= item.quantity)
									break;

							}






						}
						EmailModel emailmodel = new EmailModel();
						emailmodel.pono = datamodel[0].pono;
						emailmodel.projectcode = datamodel[0].projectcode;
						emailmodel.remarks = datamodel[0].remarks;
						emailmodel.material = materials;
						emailmodel.createdby = datamodel[0].requesterid;
						emailmodel.createddate = DateTime.Now;
						emailmodel.reserveid = result.ToString();
						emailmodel.reserveupto = datamodel[0].reserveupto.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
						emailmodel.reservedata = reservelistformail;

						EmailUtilities emailobj = new EmailUtilities();
						emailobj.sendEmail(emailmodel, Conversion.toInt(Emailparameter.mailaftermaterialreserve), 3);
						Trans.Commit();
						return 1;

					}
					else
					{
						Trans.Rollback();
						return 0;
					}




				}
				catch (Exception Ex)
				{
					Trans.Rollback();
					log.ErrorMessage("PODataProvider", "MaterialRequest", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return 0;
				}
				finally
				{
					pgsql.Close();
				}

			}


		}

		/*
		Name of Function : <<GetReservedMaterialList_old>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get Reserved MaterialList_old>>
		<param name="reservedby"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ReserveMaterialModel>> GetReservedMaterialList_old(string reservedby)
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
					log.ErrorMessage("PODataProvider", "GetReservedMaterialList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getOpenPoList>>  Author :<<Ramesh>>  
		Date of Creation <<05-10-2020>>
		Purpose : <<get material reserve list>>
		<param name="reservedby"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<MaterialTransaction>> GetReservedMaterialList(string reservedby)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				string materialrequestquery = WMSResource.getmaterialreserves.Replace("#reservedby", reservedby);

				try
				{
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<MaterialTransaction>(
					   materialrequestquery, null, commandType: CommandType.Text);
					foreach (MaterialTransaction trans in data)
					{
						trans.materialdata = new List<MaterialTransactionDetail>();
						string materialrequestdataquery = WMSResource.getmaterialreservedata.Replace("#reserveid", trans.reserveid);
						var data1 = await pgsql.QueryAsync<MaterialTransactionDetail>(
						materialrequestdataquery, null, commandType: CommandType.Text);
						trans.materialdata = data1.ToList();

					}
					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetReservedMaterialList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}

		}

		/*
		Name of Function : <<getissuematerialdetailsforreserved>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get issue material details for reserved>>
		<param name="reservedid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

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
					log.ErrorMessage("PODataProvider", "getissuematerialdetails", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetReleasedmaterialList>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get Released material List>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "GetReleasedmaterialList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetmaterialdetailsByreserveid>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get material details By reserveid>>
		<param name="reserveid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "GetmaterialdetailsByreserveid", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<ApproveMaterialRelease>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Approve Material Release>>
		<param name="dataobj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					decimal? issuedqty = item.issuedqty;
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
				log.ErrorMessage("PODataProvider", "updaterequestedqty", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return 0;
			}


		}


		/*
		Name of Function : <<acknowledgeMaterialReceivedforreserved>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<acknowledge Material Received for reserved>>
		<param name="dataobj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					string reserveid = item.reserveid;
					string ackremarks = item.ackremarks;
					decimal? issuedquantity = item.issuedqty;
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
				log.ErrorMessage("PODataProvider", "acknowledgeMaterialReceivedforreserved", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return 0;
			}

		}

		/*
		Name of Function : <<getSecurityreceivedList>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get received po list based on current date>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<SecurityInwardreceivedModel>> getSecurityreceivedList()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					DateTime dt = DateTime.Now;
					string date = dt.ToString("yyyy-MM-dd");
					string query = WMSResource.getsecurityreceivedlist;
					query = query + " where sl.isdirectdelivered is not true and sl.deleteflag is not true and sl.invoicedate <= '" + date + " 23:59:59' and sl.invoicedate >= '" + date + " 00:00:00'";
					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<SecurityInwardreceivedModel>(
					   query, null, commandType: CommandType.Text);
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getSecurityreceivedList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<insertquantitycheck>>  Author :<<Amulya>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<insert quantity check>>
		<param name="datamodel"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<string> insertquantitycheck(List<inwardModel> datamodel)
		{

			inwardModel obj = new inwardModel();
			inwardModel getgrnnoforpo = new inwardModel();
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				var results = 0;
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
							results = Convert.ToInt32(DB.ExecuteScalar(insertforquality, new
							{
								item.inwardid,
								item.qualitypassedqty,
								item.qualityfailedqty,
								item.receivedby,
								item.remarks

							})
							);
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


						}

					}
					if (Convert.ToInt32(results) != 0)
					{
						EmailModel emailmodel = new EmailModel();
						emailmodel.pono = datamodel[0].pono;
						emailmodel.jobcode = datamodel[0].projectname;
						emailmodel.grnnumber = datamodel[0].grnnumber;
						EmailUtilities emailobj = new EmailUtilities();
						emailobj.sendEmail(emailmodel, 3, 3);
					}

					//}
					return "counted";
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "insertquantitycheck", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}


		/*
		Name of Function : <<getprojectlist>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get project list>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ddlmodel>> getprojectlist()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getAllprojectlist;


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<ddlmodel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					var senddata = data.Where(o => o.value != null);
					return senddata;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getprojectlist", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		/*
		Name of Function : <<getprojectlistfortransfer>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get project list>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ddlmodel>> getprojectlistfortransfer()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getAllprojectlistfortransfer;


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<ddlmodel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					var senddata = data.Where(o => o.projectmanager != null);
					return senddata;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getprojectlistfortransfer", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		/*
		Name of Function : <<getprojectlist>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get project list>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ddlmodel>> getgatepassreason()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getGatePassReason;


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<ddlmodel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getprojectlist", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}


		/*
		Name of Function : <<getprojectlisttoassign>>  Author :<<Ramesh>>  
		Date of Creation <<01-02-2021>>
		Purpose : <<get project list to assign>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<AssignProjectModel>> getprojectlisttoassign(string empno)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string query = WMSResource.getprojecttoassign.Replace("#empno", empno);


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<AssignProjectModel>(
					  query, null, commandType: CommandType.Text);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getprojectlisttoassign", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}
		/*
		Name of Function : <<getstorelist>>  Author :<<Ramesh>>  
		Date of Creation <<24-03-2021>>
		Purpose : <<get store list for master page>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<locataionDetailsStock>> getstorelist()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string query = WMSResource.getstoredata;
					int plantid = Conversion.toInt(config.plantid);

					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<locataionDetailsStock>(
					  query, null, commandType: CommandType.Text);
					data = data.Where(o => o.plantid == plantid);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getstorelist", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}
		/*
		Name of Function : <<getracklist>>  Author :<<Ramesh>>  
		Date of Creation <<24-03-2021>>
		Purpose : <<get store list for master page>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<locataionDetailsStock>> getracklist()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string query = WMSResource.getracklist;
					int plantid = Conversion.toInt(config.plantid);

					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<locataionDetailsStock>(
					  query, null, commandType: CommandType.Text);
					data = data.Where(o => o.plantid == plantid);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getracklist", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}
		/*
		Name of Function : <<getbinlist>>  Author :<<Ramesh>>  
		Date of Creation <<24-03-2021>>
		Purpose : <<get store list for master page>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<locataionDetailsStock>> getbinlistdata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string query = WMSResource.getbinlistdata;
					int plantid = Conversion.toInt(config.plantid);

					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<locataionDetailsStock>(
					  query, null, commandType: CommandType.Text);
					data = data.Where(o => o.plantid == plantid);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getbinlist", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}
		/*
		Name of Function : <<getstorelist>>  Author :<<Ramesh>>  
		Date of Creation <<24-03-2021>>
		Purpose : <<get store list for master page>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string addupdatestore(locataionDetailsStock store)
		{
			string result = "";
			int rslt = 0;
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					if (store.storeid == null || store.storeid == 0)
					{
						string insertqry = WMSResource.AddnewStore;
						using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						{
							rslt = DB.Execute(insertqry, new
							{
								store.storename,
								store.plantid,
								store.createdby,
								store.storedescription
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
						string insertqry = WMSResource.updateStore.Replace("#storeid", store.storeid.ToString());
						bool? deleteflag = false;
						if (store.isactive == false)
						{
							deleteflag = true;

						}
						using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						{
							rslt = DB.Execute(insertqry, new
							{
								deleteflag,
								store.storename,
								store.plantid,
								store.storedescription,
								store.modifiedby
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

					return result;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "addupdatestore", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return Ex.Message.ToString();
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		/*
		Name of Function : <<addupdaterack>>  Author :<<Ramesh>>  
		Date of Creation <<24-03-2021>>
		Purpose : <<get store list for master page>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string addupdaterack(locataionDetailsStock rack)
		{
			string result = "";
			int rslt = 0;
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					if (rack.rackid == null || rack.rackid == 0)
					{
						string insertqry = WMSResource.Addnewrack;
						using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						{
							rslt = DB.Execute(insertqry, new
							{
								rack.rackname,
								rack.storeid,
								rack.createdby

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
						string insertqry = WMSResource.updaterack.Replace("#rackid", rack.rackid.ToString());
						bool? deleteflag = false;
						if (rack.isactive == false)
						{
							deleteflag = true;

						}
						using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						{
							rslt = DB.Execute(insertqry, new
							{
								rack.rackname,
								rack.storeid,
								deleteflag,
								rack.modifiedby
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

					return result;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "addupdaterack", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return Ex.Message.ToString();
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		/*
		Name of Function : <<addupdatebin>>  Author :<<Ramesh>>  
		Date of Creation <<24-03-2021>>
		Purpose : <<get store list for master page>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string addupdatebin(locataionDetailsStock bin)
		{
			string result = "";
			int rslt = 0;
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					if (bin.binid == null || bin.binid == 0)
					{
						string insertqry = WMSResource.Addnewbin;
						using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						{
							rslt = DB.Execute(insertqry, new
							{
								bin.storeid,
								bin.binname,
								bin.rackid,
								bin.createdby

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
						string insertqry = WMSResource.updatebin.Replace("#binid", bin.binid.ToString());
						bool? deleteflag = false;
						if (bin.isactive == false)
						{
							deleteflag = true;

						}
						using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						{
							rslt = DB.Execute(insertqry, new
							{
								bin.storeid,
								bin.binname,
								deleteflag,
								bin.rackid,
								bin.modifiedby
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

					return result;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "addupdatebin", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return Ex.Message.ToString();
				}
				finally
				{
					pgsql.Close();
				}


			}
		}



		/*
		Name of Function : <<getprojectlisttoassignpm>>  Author :<<Ramesh>>  
		Date of Creation <<17-02-2021>>
		Purpose : <<get project list to assign>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<assignpmmodel>> getprojectlisttoassignpm()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string query = WMSResource.getProjectsforAdmin;


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<assignpmmodel>(
					  query, null, commandType: CommandType.Text);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getprojectlisttoassignpm", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		/*
		Name of Function : <<getprojectlistbymanager>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get project list by manager>>
		<param name="empno"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ddlmodel>> getprojectlistbymanager(string empno)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string globalid = DateTime.Now.Ticks.ToString();
					string userquery = "select  * from wms.employee where employeeno='" + empno + "'";
					User userdata = pgsql.QuerySingle<User>(
					   userquery, null, commandType: CommandType.Text);
					if (userdata != null)
					{
						var gid = userdata.globalempno;
						if (gid != null && gid.Trim() != "")
						{
							globalid = gid;
						}
					}
					string materialrequestquery = WMSResource.getprojectlist.Replace("#manager", empno);


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<ddlmodel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getprojectlist", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}


		/*
		Name of Function : <<getmatlist>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get material list>>
		<param name="querytext"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ddlmodel>> getmatlist(string querytext = "")
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = "select material as value,materialdescription as text from wms.\"MaterialMasterYGS\" ";
					if (querytext != "" && querytext != null)
					{
						materialrequestquery += " where material ilike '" + querytext + "%' or materialdescription ilike '" + querytext + "%' ";

					}
					materialrequestquery += "limit 10";



					//string materialrequestquery = WMSResource.getmaterialfortransfer.Replace("#requestor", empno);


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<ddlmodel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getmatlist", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		/*
		Name of Function : <<getmatlistbyproject>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get mat list by project>>
		<param name="projectcode"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ddlmodel>> getmatlistbyproject(string projectcode)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getmaterialsbyprojectcode.Replace("#projectcode", projectcode);


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<ddlmodel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getmatlistbyproject", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}


		/*
				Name of Function : <<insertreturn>>  Author :<<Ramesh>>  
				Date of Creation <<12-12-2019>>
				Purpose : <<insert return>>
				<param name="datamodel"></param>
				Review Date :<<>>   Reviewed By :<<>>
				*/
		public async Task<string> insertreturn(List<inwardModel> datamodel)
		{

			inwardModel obj = new inwardModel();
			inwardModel getgrnnoforpo = new inwardModel();
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					await pgsql.OpenAsync();
					string grnnumber = datamodel[0].grnnumber;
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

							string insertqueryforstatusforqty = WMSResource.insertqueryforstatusforqty;

							var data1 = DB.ExecuteScalar(insertqueryforstatusforqty, new
							{
								item.pono,
								item.returnqty

							});

							if (item.pono.StartsWith("NP"))
							{
								string closepoqry = WMSResource.closepoquery.Replace("#pono", item.pono);
								DB.ExecuteScalar(closepoqry, null);
							}
							else
							{
								string isallreceived = WMSResource.isallreceivedqueryforpo.Replace("#pono", item.pono);
								bool isreceived = (bool)DB.ExecuteScalar(isallreceived, null);
								if (isreceived)
								{
									string closepoqry = WMSResource.closepoquery.Replace("#pono", item.pono);
									DB.ExecuteScalar(closepoqry, null);
								}

							}







						}
					}

					EmailModel emailmodel1 = new EmailModel();
					emailmodel1.grnnumber = grnnumber;
					emailmodel1.FrmEmailId = "ramesh.kumar@yokogawa.com";
					EmailUtilities emailobj1 = new EmailUtilities();
					emailobj1.sendEmail(emailmodel1, 32, 3);
					return "counted";
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "insertquantitycheck", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}



		/*
		Name of Function : <<Getlocationdata>>  Author :<<Shashikala>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get location data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "Getlocationdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getPOReportdata>>  Author :<<Ramesh>>  
		Date of Creation <<19-02-2019>>
		Purpose : <<Get Report data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<POReportModel>> getPOReportdata(string empno, string projectcode, string pono)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getPOReportData;
					if (pono != null && pono != "null")
					{
						materialrequestquery = materialrequestquery.Replace("#pofilter", " and ws.pono = '" + pono + "'");

					}
					else
					{
						materialrequestquery = materialrequestquery.Replace("#pofilter", string.Empty);

					}
					if (projectcode != null && projectcode != "null")
					{
						materialrequestquery = materialrequestquery.Replace("#projectfilter", " and prj.projectcode = '" + projectcode + "'");

					}
					else
					{
						materialrequestquery = materialrequestquery.Replace("#projectfilter", string.Empty);

					}
					if (empno != null && empno != "null")
					{
						materialrequestquery = materialrequestquery.Replace("#managerfilter", " and prj.projectmanager = '" + empno + "'");
					}
					else
					{
						materialrequestquery = materialrequestquery.Replace("#managerfilter", string.Empty);

					}
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<POReportModel>(
					  materialrequestquery, null, commandType: CommandType.Text);


					return data.OrderBy(o => o.pono);



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getPOReportdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getsubconannexuredata>>  Author :<<Ramesh>>  
		Date of Creation <<23-02-2019>>
		Purpose : <<Get Report data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<stocktransfermateriakmodel>> getsubconannexuredata(string empno, string subconno)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getannexuredata;
					if (subconno != null && subconno != "null")
					{
						materialrequestquery = materialrequestquery + " and wi.transferid = '" + subconno + "'";

					}



					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<stocktransfermateriakmodel>(
					  materialrequestquery, null, commandType: CommandType.Text);

					if (empno != null && empno != "null")
					{
						string querypmb = "select au.createdby as approverid from wms.auth_users au where au.isdelegatemember is true and au.deleteflag is not true and au.employeeid = '" + empno + "' limit 1";
						var rslttmb = pgsql.ExecuteScalar(querypmb, null);
						if (rslttmb == null)
						{
							data = data.Where(o => o.projectmanager == empno);
						}
						else
						{
							string pmanager = rslttmb.ToString();
							data = data.Where(o => o.projectmanager == empno || o.projectmanager == pmanager);
						}


					}

					return data.OrderBy(o => o.projectid);



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getPOReportdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getPOReportdetail>>  Author :<<Ramesh>>  
		Date of Creation <<19-02-2019>>
		Purpose : <<Get Report data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<POReportModel>> getPOReportdetail(string materialid, string description, string pono, string querytype, string requesttype, string projectcode, string empno)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string descriptionstr = description.Replace("\'", "''");
					string materialrequestquery = "";
					if (querytype == "available")
					{
						materialrequestquery = WMSResource.poreportitemlocationAQ.Replace("#materialid", materialid).Replace("#desc", descriptionstr).Replace("#pono", pono);
					}
					else if (querytype == "issue")
					{
						materialrequestquery = WMSResource.poreportitemlocationIQ.Replace("#materialid", materialid).Replace("#desc", descriptionstr).Replace("#pono", pono).Replace("#type", requesttype);
					}
					else if (querytype == "reserve")
					{
						materialrequestquery = WMSResource.poreportitemlocationRQ.Replace("#materialid", materialid).Replace("#desc", descriptionstr).Replace("#pono", pono);
					}


					if (projectcode != null && projectcode != "null")
					{
						materialrequestquery = materialrequestquery.Replace("#projectfilter", " and prj.projectcode = '" + projectcode + "'");

					}
					else
					{
						materialrequestquery = materialrequestquery.Replace("#projectfilter", string.Empty);

					}
					if (empno != null && empno != "null")
					{
						materialrequestquery = materialrequestquery.Replace("#managerfilter", " and prj.projectmanager = '" + empno + "'");
					}
					else
					{
						materialrequestquery = materialrequestquery.Replace("#managerfilter", string.Empty);

					}
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<POReportModel>(
					  materialrequestquery, null, commandType: CommandType.Text);



					return data.OrderBy(o => o.pono);



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getPOReportdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<Getbindata>>  Author :<<Shashikala>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get bin data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<dropdownModel>> Getbindata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					//string materialrequestquery = "select * from wms.wms_rd_bin where deleteflag=false  order by binnumber asc";
					string materialrequestquery = "select binnumber, Max(binid) as binid  from wms.wms_rd_bin where deleteflag = false group by binnumber  order by binnumber asc";

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<dropdownModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "Getbindata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<Getdestinationlocationforist>>  Author :<<Ramesh>>  
		Date of Creation <<24-02-2021>>
		Purpose : <<Get bin data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<dropdownModel>> Getdestinationlocationforist(int store)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{

					string materialrequestquery = WMSResource.getdestinationlocationforIST.Replace("#locatorid", store.ToString());

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<dropdownModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "Getdestinationlocationforist", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<Getrackdata>>  Author :<<Shashikala>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get rack data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<dropdownModel>> Getrackdata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					//string materialrequestquery = "select * from wms.wms_rd_rack where deleteflag=false order by racknumber asc";
					string materialrequestquery = "select racknumber,Max(rackid) as rackid  from wms.wms_rd_rack where deleteflag=false group by racknumber  order by racknumber asc";
					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<dropdownModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "Getrackdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<Getbindataforputaway>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get bin data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<dropdownModel>> Getbindataforputaway()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = "select * from wms.wms_rd_bin where deleteflag=false  order by binnumber asc";
					//string materialrequestquery = "select binnumber, Max(binid) as binid  from wms.wms_rd_bin where deleteflag = false group by binnumber  order by binnumber asc";

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<dropdownModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "Getbindata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<Getrackdataforputaway>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get rack data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<dropdownModel>> Getrackdataforputaway()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = "select * from wms.wms_rd_rack where deleteflag=false order by racknumber asc";
					//string materialrequestquery = "select racknumber,Max(rackid) as rackid  from wms.wms_rd_rack where deleteflag=false group by racknumber  order by racknumber asc";
					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<dropdownModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "Getrackdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}


		/*
		Name of Function : <<Getrackdata>>  Author :<<Ramesh Kumar>>  
		Date of Creation <<15/07/2020>>
		Purpose : <<Get list of materials >>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "GetMaterialcombo", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
	Name of Function : <<getapproverList>>  Author :<<Ramesh Kumar>>  
	Date of Creation <<15/07/2020>>
	Purpose : <<get approver List >>
	<param name="empid"></param>
	Review Date :<<>>   Reviewed By :<<>>
	*/
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
					log.ErrorMessage("PODataProvider", "getapproverList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getgatepassByapproverList>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get gatepass By approver List>>
		<param name="empid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "getgatepassByapproverList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<Getpagesbyroleid>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Getpages by roleid>>
		<param name="roleid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "Getpagesbyroleid", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}

		}

		/*
		Name of Function : <<Getpages>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get pages>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "Getpages", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}

		}


		/*
		Name of Function : <<GetInitialStockPutawayMaterials>>  Author :<<Ramesh>>  
		Date of Creation <<13-01-2021>>
		Purpose : <<Get Material to put away got in initial stock without location>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<initialStock>> GetInitialStockPutawayMaterials()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string matlist = WMSResource.initialstockputawaymaterial;

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<initialStock>(
					  matlist, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetInitialStockPutawayMaterials", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}

		}

		/*
		Name of Function : <<GatepassapproveByMail>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Gatepass approve By Mail>>
		<param name="model"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
				log.ErrorMessage("PODataProvider", "GatepassapproveByMail", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return 0;
			}
		}

		/*
		Name of Function : <<GatepassapproveByManager>>  Author :<<Amulya>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Gate pass approve By Manager>>
		<param name="model"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int GatepassapproveByManager(gatepassModel model)
		{
			try
			{
				var result = 0;
				string mailto = "";



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
							string remarks = model.approverremarks;
							string insertgatepasshistory = WMSResource.insertgatepassapprovalhistory;
							string approverstatus = model.approverstatus;
							var gatepasshistory = DB.ExecuteScalar(insertgatepasshistory, new
							{
								model.gatepassid,
								model.approverid,
								approvername,
								approverstatus,
								label,
								remarks,

							});
							EmailModel emailmodel = new EmailModel();
							emailmodel.approvername = model.approvedby;
							emailmodel.approverid = model.approverid;
							emailmodel.gatepassid = model.gatepassid;
							emailmodel.approverstatus = model.approverstatus;
							emailmodel.gatepasstype = model.gatepasstype;
							emailmodel.requestedby = model.requestedby;
							emailmodel.requestedon = model.requestedon;

							EmailUtilities emailobj = new EmailUtilities();

							if (model.approverstatus == "Approved")//Inventory mangers
							{
								//var query = WMSResource.getINVMangerList;
								//List<User> data = DB.Query<User>(query, null, commandType: CommandType.Text).ToList();
								//foreach (User usr in data)
								//{
								//	if (!string.IsNullOrEmpty(usr.email))
								//		emailmodel.ToEmailId += usr.email + ",";
								//}
								emailobj.sendEmail(emailmodel, 35, 4);
							}
							else
							{
								string userquery = WMSResource.getRequesterEmail.Replace("#gatepassid", model.gatepassid);
								User userdata = DB.QuerySingle<User>(
								   userquery, null, commandType: CommandType.Text);
								mailto = userdata.email;
								emailmodel.ToEmailId = mailto;
								emailobj.sendEmail(emailmodel, 21);
							}



						}
					}
				}
				else if (model.categoryid == 3)//Authorization
				{
					model.approverstatus = model.authstatus;
					updateapproverstatus = WMSResource.updateAuthstatusbyIM.Replace("#authstatus", model.authstatus);

					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{

						result = DB.Execute(updateapproverstatus, new
						{
							model.authid,
							model.authremarks,
							model.gatepassid

						});
						if (result == 1)
						{
							int label = 3;
							string approvername = model.approvedby;
							string remarks = model.authremarks;
							string insertgatepasshistory = WMSResource.insertgatepassapprovalhistory;
							string approverstatus = model.approverstatus;
							var gatepasshistory = DB.ExecuteScalar(insertgatepasshistory, new
							{
								model.gatepassid,
								model.approverid,
								approvername,
								approverstatus,
								label,
								remarks
							});
							EmailModel emailmodel = new EmailModel();
							emailmodel.approvername = model.approvedby;
							emailmodel.authname = model.authname;
							emailmodel.approverid = model.approverid;
							emailmodel.gatepassid = model.gatepassid;
							emailmodel.approverstatus = model.approverstatus;
							emailmodel.gatepasstype = model.gatepasstype;
							emailmodel.requestedby = model.requestedby;
							emailmodel.requestedon = model.requestedon;

							EmailUtilities emailobj = new EmailUtilities();
							if (model.gatepasstype == "Returnable")
							{
								string userquery = WMSResource.getRequesterEmail.Replace("#gatepassid", model.gatepassid);
								User userdata = DB.QuerySingle<User>(
								   userquery, null, commandType: CommandType.Text);
								mailto = userdata.email;
								if (model.approverstatus == "Approved")
								{
									if (model.isnonproject != true)
									{
										emailmodel.CC = mailto;
										emailobj.sendEmail(emailmodel, 15, 3);
									}
									else
									{
										emailobj.sendEmail(emailmodel, 33, 3);
										emailmodel.ToEmailId = mailto;
										emailobj.sendEmail(emailmodel, 34);
									}

								}
								else
								{
									emailmodel.ToEmailId = mailto;
									emailobj.sendEmail(emailmodel, 21);
								}

							}
							else
							{
								if (model.approverstatus == "Approved")
								{
									//string userquery = WMSResource.getFMapprovermail.Replace("#gatepassid", model.gatepassid);
									//User userdata = DB.QuerySingle<User>(
									//   userquery, null, commandType: CommandType.Text);
									//mailto = userdata.email;
									mailto = config.FinanceApproverEmail;
									emailmodel.ToEmailId = mailto;
									emailobj.sendEmail(emailmodel, 16);
								}
								else
								{
									string userquery = WMSResource.getRequesterEmail.Replace("#gatepassid", model.gatepassid);
									User userdata = DB.QuerySingle<User>(
									   userquery, null, commandType: CommandType.Text);
									mailto = userdata.email;
									emailmodel.ToEmailId = mailto;
									emailobj.sendEmail(emailmodel, 21);
								}

							}

						}
					}
				}
				else if (model.categoryid == 2)
				{
					updateapproverstatus = WMSResource.updateApprovedstatusbyFMmanager.Replace("#fmapprovedstatus", model.fmapprovedstatus);

					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{

						model.fmapproverid = model.approverid;

						var result1 = DB.Execute(updateapproverstatus, new
						{
							model.fmapproverid,
							model.fmapproverremarks,
							model.gatepassid

						});
						if (result1 == 1)
						{
							int label = 2;
							string approvername = model.approvedby;
							string remarks = model.fmapproverremarks;
							string insertgatepasshistory = WMSResource.insertgatepassapprovalhistory;
							string approverstatus = model.fmapprovedstatus;
							var gatepasshistory = DB.ExecuteScalar(insertgatepasshistory, new
							{

								model.gatepassid,
								model.approverid,
								approvername,
								approverstatus,
								label,
								remarks

							});
							EmailModel emailmodel = new EmailModel();
							emailmodel.approvername = model.approvedby;
							emailmodel.authname = model.authname;
							emailmodel.approverid = model.approverid;
							emailmodel.gatepassid = model.gatepassid;
							emailmodel.approverstatus = model.fmapprovedstatus;
							emailmodel.requestedby = model.requestedby;
							emailmodel.requestedon = model.requestedon;

							EmailUtilities emailobj = new EmailUtilities();

							if (model.fmapprovedstatus == "Approved")
							{
								if (model.isnonproject != true)
								{
									string userquery = WMSResource.getFMapprovermail.Replace("#gatepassid", model.gatepassid);
									User userdata = DB.QuerySingle<User>(
									   userquery, null, commandType: CommandType.Text);
									mailto = userdata.email;
									emailobj.sendEmail(emailmodel, 15, 3);
								}
								else
								{


									emailobj.sendEmail(emailmodel, 33, 3);
									string userquery = WMSResource.getRequesterEmail.Replace("#gatepassid", model.gatepassid);
									User userdata = DB.QuerySingle<User>(
									   userquery, null, commandType: CommandType.Text);
									mailto = userdata.email;
									emailmodel.ToEmailId = mailto;
									emailobj.sendEmail(emailmodel, 34);
								}

							}
							else
							{

								//Inv manager
								//var query = WMSResource.getINVMangerList;
								//List<User> data = DB.Query<User>(query, null, commandType: CommandType.Text).ToList();
								//foreach (User usr in data)
								//{
								//	if (!string.IsNullOrEmpty(usr.email) && usr.employeeno == model.authid)
								//		emailmodel.ToEmailId += usr.email + ",";
								//}
								string userquery = WMSResource.getRequesterEmail.Replace("#gatepassid", model.gatepassid);
								User userdata = DB.QuerySingle<User>(
								   userquery, null, commandType: CommandType.Text);
								mailto = userdata.email;
								emailmodel.CC = mailto;
								emailobj.sendEmail(emailmodel, 21, 4);
							}
							//emailobj.sendEmail(emailmodel, 17, 3);


						}
					}
				}

				return (Convert.ToInt32(result));
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "GatepassapproveByManager", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return 0;
			}
		}

		/*
		Name of Function : <<getSafteyStockList>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get Saftey Stock List>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "getSafteyStockList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}

		}

		/*
		Name of Function : <<GetBinList>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get Bin List>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<StockModel>> GetBinList()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					//string approverlist = WMSResource.getbinlist;
					string approverlist = WMSResource.getbinlist_v1;

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<StockModel>(
					  approverlist, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetBinList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetMaterialstockcombo>>  Author :<<Ramesh>>  
		Date of Creation <<15/07/2020>>
		Purpose : <<Get list of materials for stock transfer >>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<Materials>> GetMaterialstockcombo(int store)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = "select ws.materialid as material,ws.poitemdescription as materialdescription,sum(ws.availableqty) as availableqty from wms.wms_stock ws";
					materialrequestquery += " where ws.availableqty > 0 and ws.storeid = " + store + " group by ws.materialid,ws.poitemdescription";

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<Materials>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetMaterialstockcombo", Ex.StackTrace.ToString(), Ex.Message.ToString(), url); return null;
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		/*
	Name of Function : <<getMaterialforstocktransferorder>>  Author :<<Gayathri>>  
	Date of Creation <<22/01/2021>>
	Purpose : <<Get list of materials for pomaterials table >>
	Review Date :<<>>   Reviewed By :<<>>
	*/
		public async Task<IEnumerable<Materials>> getMaterialforstocktransferorder()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					//string materialrequestquery = "select materialid as material, poitemdescription as poitemdesc,unitprice from wms.wms_pomaterials";

					//string materialrequestquery= "select  matmtygs .material as material,matmtygs .materialdescription as materialdescription, pomat.poitemdescription as poitemdesc, pomat.unitprice from wms.wms_pomaterials pomat right join wms.\"MaterialMasterYGS\" matmtygs on pomat.materialid = matmtygs.material limit 10";
					string materialrequestquery = WMSResource.getMaterialforSTO;
					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<Materials>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetMaterialstockcombo", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}


		/*
		Name of Function : <<UpdateStockTransfer>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Updating location for stock>>
		<param name="data"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

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
							decimal? decavail = objs.availableqty - stck.availableqty;

							string query1 = "UPDATE wms.wms_stock set availableqty=" + decavail + "  where itemid = '" + stck.itemid + "'";
							pgsql.ExecuteScalar(query1);
							StockModel objs1 = new StockModel();
							string query2 = "select * from wms.wms_stock where pono = '" + objs.pono + "' and materialid = '" + objs.materialid + "' and itemlocation = '" + stck.itemlocation + "'";
							objs1 = pgsql.QueryFirstOrDefault<StockModel>(
							   query2, null, commandType: CommandType.Text);
							if (objs1 != null)
							{
								decimal? availqty = objs1.availableqty + stck.availableqty;

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
						log.ErrorMessage("PODataProvider", "UpdateStockTransfer", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
						return null;
					}
				}
			}




			return loactiontext = "success";




		}

		/*
		Name of Function : <<InvStockTransfer>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Inv Stock Transfer>>
		<param name="data"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string InvStockTransfer(invstocktransfermodel data)
		{

			StockModel obj = new StockModel();
			string loactiontext = string.Empty;
			string trsfrid = string.Empty;
			string crtdby = string.Empty;
			string requesttyp = string.Empty;
			string mattype = data.materialtype;
			string mailto = "";
			string mailcc = "";
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

							transfer.transferredby = data.transferredby;
							transfer.transferredon = System.DateTime.Now;
							transfer.transfertype = data.transfertype;
							if (transfer.transfertype != "SubContract" && data.sourceplant == data.destinationplant)
								transfer.transfertype = "IST";

							if (transfer.transfertype != "SubContract" && data.sourceplant != data.destinationplant)
								transfer.transfertype = "STO";
							requesttyp = transfer.transfertype;
							transfer.sourceplant = data.sourceplant;
							transfer.destinationplant = data.destinationplant;
							transfer.vendorcode = data.vendorcode;
							transfer.vendorname = data.vendorname;
							transfer.remarks = data.remarks;
							transfer.sourcelocationcode = data.sourcelocationcode;
							transfer.destinationlocationcode = data.destinationlocationcode;
							transfer.status = "Pending";
							transfer.materialtype = data.materialtype;
							if (transfer.transfertype == "STO" || transfer.transfertype == "SubContract")
							{
								transfer.projectcode = data.projectcode;
								transfer.approverid = data.approverid;
								transfer.isapprovalrequired = true;
								transfer.isapproved = null;
								//transfer.pono = data.pono;
								string userquery = "select  * from wms.employee where employeeno='" + data.approverid + "'";
								User userdata = pgsql.QuerySingle<User>(
								   userquery, null, commandType: CommandType.Text);
								mailto = userdata.email;
								string userquery1 = "select  * from wms.employee where employeeno='" + data.transferredby + "'";
								User userdata1 = pgsql.QuerySingle<User>(
								   userquery1, null, commandType: CommandType.Text);
								mailcc = userdata1.email;

							}

							string mainstockinsertqueryy = WMSResource.insertInvStocktransfer;
							var resultsxx = pgsql.ExecuteScalar(mainstockinsertqueryy, new
							{
								transfer.transferredby,
								transfer.transferredon,
								transfer.transfertype,
								transfer.sourceplant,
								transfer.destinationplant,
								transfer.vendorcode,
								transfer.vendorname,
								transfer.remarks,
								transfer.status,
								transfer.projectcode,
								transfer.approverid,
								transfer.isapprovalrequired,
								transfer.isapproved,
								transfer.approvedon,
								transfer.approvalremarks,
								transfer.sourcelocationcode,
								transfer.destinationlocationcode,
								transfer.materialtype


							});
							trsfrid = resultsxx.ToString();
							crtdby = transfer.transferredby;
							transfer.transferid = resultsxx.ToString();
							if (transfer.transfertype == "STO" || transfer.transfertype == "SubContract")
							{
								string stogrinsert = WMSResource.insertstosubcontrctgr;
								var resultsxx1 = pgsql.ExecuteScalar(stogrinsert, new
								{
									transfer.transferid,
									transfer.transfertype

								});
							}


						}

						if (transfer.sourceplant == transfer.destinationplant)
						{
							string descriptionstr = null;
							if (stck.materialdescription != null)
							{
								descriptionstr = stck.materialdescription.Replace("\'", "''");
							}

							string query = "select * from wms.wms_stock where materialid ='" + stck.materialid + "' and lower(poitemdescription) = '" + descriptionstr.ToLower() + "' and  itemlocation = '" + stck.sourcelocation + "' and availableqty > 0 order by itemid";

							var stockdata = pgsql.QueryAsync<StockModel>(query, null, commandType: CommandType.Text);
							if (stockdata != null)
							{
								decimal? quantitytotransfer = stck.transferqty;
								decimal? issuedqty = 0;
								foreach (StockModel itm in stockdata.Result)
								{

									if (quantitytotransfer <= itm.availableqty)
									{
										issuedqty = quantitytotransfer;
									}
									else
									{
										issuedqty = itm.availableqty;
									}
									string inwmasterid = null;
									int? inwardid = null;
									decimal? unitprice = 0;
									decimal? value = 0;
									if (itm.inwmasterid != null && itm.inwmasterid != "")
									{
										inwmasterid = itm.inwmasterid;

									}
									if (itm.inwmasterid != null && itm.inwmasterid != "")
									{
										inwardid = itm.inwardid;

									}
									value = itm.unitprice * issuedqty;
									unitprice = itm.unitprice;
									quantitytotransfer = quantitytotransfer - issuedqty;

									string insertqueryforstatusforqty = WMSResource.updateqtyafterissue.Replace("#itemid", Convert.ToString(itm.itemid)).Replace("#issuedqty", Convert.ToString(issuedqty));
									var data1 = pgsql.ExecuteScalar(insertqueryforstatusforqty);
									StockModel objs1 = new StockModel();
									string descriptionstr1 = null;
									if (stck.materialdescription != null)
									{
										descriptionstr1 = stck.materialdescription.Replace("\'", "''");
									}
									string query2 = "select * from wms.wms_stock where pono = '" + itm.pono + "' and materialid = '" + itm.materialid + "' and lower(poitemdescription) = '" + descriptionstr1.ToLower() + "' and itemlocation = '" + stck.destinationlocation + "' order by itemid";
									objs1 = pgsql.QueryFirstOrDefault<StockModel>(
									   query2, null, commandType: CommandType.Text);
									if (objs1 != null)
									{
										decimal? availqty = objs1.availableqty + issuedqty;

										string query4 = "UPDATE wms.wms_stock set availableqty=" + availqty + "  where itemid = " + objs1.itemid + "";
										pgsql.ExecuteScalar(query4);
										string stockinsertqry = WMSResource.insertinvtransfermaterial;
										int sourceitemid = itm.itemid;
										int destinationitemid = objs1.itemid;
										decimal? transferqty = issuedqty;
										var resultsxx = pgsql.ExecuteScalar(stockinsertqry, new
										{
											transfer.transferid,
											stck.materialid,
											stck.sourcelocation,
											sourceitemid,
											stck.destinationlocation,
											destinationitemid,
											transferqty,
											stck.materialdescription

										});
									}
									else
									{
										string insertqueryx = WMSResource.insertstock;
										DateTime createddate = System.DateTime.Now;

										int? binid = null;
										int? rackid = null;
										int? storeid = null;
										if (stck.binid > 0)
										{
											binid = stck.binid;
										}
										if (stck.rackid > 0)
										{
											rackid = stck.rackid;
										}
										if (stck.storeid > 0)
										{
											storeid = stck.storeid;
										}
										//int availableqty = stck.transferqty;
										decimal? availableqty = issuedqty;
										string itemlocation = stck.destinationlocation;
										string createdby = transfer.transferredby;
										string stocktype = itm.stcktype;
										int itemid = 0;
										var result = 0;
										result = Convert.ToInt32(pgsql.ExecuteScalar(insertqueryx, new
										{
											inwmasterid,
											itm.pono,
											binid,
											rackid,
											storeid,
											itm.vendorid,
											itm.totalquantity,
											itm.shelflife,
											availableqty,
											itm.deleteflag,
											itemlocation,
											createddate,
											createdby,
											itm.stockstatus,
											stck.materialid,
											inwardid,
											stocktype,
											itm.lineitemno,
											itm.receivedtype,
											itm.poitemdescription,
											value,
											unitprice

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
											var poitemdesc = stck.materialdescription;
											int sourceitemid = itm.itemid;
											int destinationitemid = result;
											string stockinsertqry = WMSResource.insertinvtransfermaterial;
											stck.destinationitemid = itemid;
											decimal? transferqty = issuedqty;
											var resultsxx = pgsql.ExecuteScalar(stockinsertqry, new
											{
												transfer.transferid,
												stck.materialid,
												stck.sourcelocation,
												sourceitemid,
												stck.destinationlocation,
												destinationitemid,
												transferqty,
												poitemdesc
											});

										}

									}
									if (quantitytotransfer <= 0)
									{
										break;
									}


								}
							}
						}
						else
						{

							//For STO directly add material data in wms_invtransfermaterial table
							//foreach (var matdata in data.materialdata)
							//{
							var poitemdesc = stck.materialdescription;
							string stockinsertqry = WMSResource.insertinvtransfermaterialSTO;
							decimal? value = stck.materialcost;
							//string query2 = "select * from wms.material_valuation where projectcode = '"+data.projectcode+"' and material = '"+stck.materialid+"' order by id desc limit 1";
							//MaterialValuation objs1 = pgsql.QueryFirstOrDefault<MaterialValuation>(
							//   query2, null, commandType: CommandType.Text);
							//if(objs1 != null)
							//                     {
							//	if(objs1.value != null && objs1.value != 0 && objs1.quantity != null && objs1.quantity != 0)
							//                         {
							//		value = Math.Abs(Convert.ToDecimal(objs1.value)) / Math.Abs(Convert.ToInt32(objs1.quantity));

							//	}	

							//                     }


							var resultsxx = pgsql.ExecuteScalar(stockinsertqry, new
							{
								transfer.transferid,
								stck.materialid,
								stck.transferqty,
								stck.projectid,
								stck.requireddate,
								poitemdesc,
								value,
								stck.pono

							});
							//}


						}




						Trans.Commit();
						x++;

					}
					catch (Exception Ex)
					{
						Trans.Rollback();
						log.ErrorMessage("PODataProvider", "UpdateStockTransfer1", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
						return null;
					}
				}
			}

			if (requesttyp == "STO" || requesttyp == "SubContract")
			{



				EmailModel emailmodel = new EmailModel();
				emailmodel.requestid = trsfrid;
				emailmodel.createdby = crtdby;
				emailmodel.ToEmailId = mailto;
				emailmodel.createddate = DateTime.Now;

				EmailUtilities emailobj = new EmailUtilities();
				if (requesttyp == "STO")
				{
					if (mattype == "plant")
					{
						emailobj.sendEmail(emailmodel, 29);
					}
					else
					{
						emailobj.sendEmail(emailmodel, 37);
					}
				}
				else
				{
					if (mattype == "plant")
					{
						emailobj.sendEmail(emailmodel, 30);
					}
					else
					{
						emailobj.sendEmail(emailmodel, 38);
					}

				}

			}


			return loactiontext = "success";




		}

		/*
		Name of Function : <<getstocktransferdata>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get stock transfer data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "getstocktransferdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getstocktransferdatagroup>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get stock transfer data group>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "getstocktransferdatagroup", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getstocktransferdatagroup1>>  Author :<<Ramesh>>  
		Date of Creation <<18/07/2020>>
		Purpose : <<get stock transferdata>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<invstocktransfermodel>> getstocktransferdatagroup1(string transfertype)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.invstocktransfermainquery.Replace("#transfertype", transfertype);

					await pgsql.OpenAsync();
					var result = await pgsql.QueryAsync<invstocktransfermodel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					if (result != null & result.Count() > 0)
					{
						foreach (invstocktransfermodel dt in result)
						{
							var matqry = WMSResource.getinvtransfermaterialdetail.Replace("#tid", dt.transferid).Replace("#transfertype", transfertype);
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
					log.ErrorMessage("PODataProvider", "getstocktransferdatagroup1", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getgrnlistforacceptance>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get grn list for acceptance>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "getgrnlistforacceptance", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getgrnlistforacceptanceputaway>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get grn list for acceptance put away>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ddlmodel>> getgrnlistforacceptanceputaway()
		{
			List<ddlmodel> returnlist = new List<ddlmodel>();
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getgrnlistdataforputaway;

					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<ddlmodel>(
					  materialrequestquery, null, commandType: CommandType.Text);

					foreach (ddlmodel ddl in data)
					{
						try
						{
							var exixtedrow = returnlist.Where(o => o.text == ddl.text).FirstOrDefault();
							if (exixtedrow == null)
							{
								var List = data.Where(li => li.text == ddl.text).ToList();
								if (List.Count > 0)
								{
									ddl.pos = string.Join(",", (List.Select(c => c.pos).ToList()).Distinct().ToArray());
									ddl.projects = string.Join(",", (List.Select(c => c.projects).ToList()).Distinct().ToArray());
								}
								returnlist.Add(ddl);
							}
							//else
							//{
							//	if (exixtedrow.pos != null && exixtedrow.pos.Trim() != "")
							//	{
							//		exixtedrow.pos += "," + exixtedrow.pos;

							//	}
							//	if (exixtedrow.projects != null && exixtedrow.projects.Trim() != "")
							//	{
							//		exixtedrow.projects += "," + exixtedrow.projects;

							//	}
							//}
						}
						catch (Exception Ex)
						{
							log.ErrorMessage("PODataProvider", "getgrnlistforacceptanceputaway", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
							continue;
						}
					}

					return returnlist.OrderByDescending(o => o.value);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getgrnlistforacceptanceputaway", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return returnlist.OrderByDescending(o => o.value);
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getmrnlist>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get grn list for acceptance put away>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<MRNsavemodel>> getmrnlist()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getMRNList;
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<MRNsavemodel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getmrnlist", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getgrnlistforacceptancenotify>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get grn list for acceptance notify>>
		<param name="type"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "getgrnlistforacceptanceputaway", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getholdgrlist>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get hold gr list>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "getholdgrlist", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getgrnlistforacceptanceqc>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<getgrnlistforacceptanceqc>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "getgrnlistforacceptanceqc", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getgrnlistforacceptanceqcbydate>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get grn list for acceptance qc by date>>
		<param name="fromdt"></param>
		 <param name="todt"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "getgrnlistforacceptanceqc", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getnotifiedgrbydate>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get notified gr by date>>
		<param name="fromdt"></param>
		 <param name="todt"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<inwardModel>> getnotifiedgrbydate(string fromdt, string todt)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = "";


					materialrequestquery = WMSResource.getnotifiedgrbydate.Replace("#fromdate", fromdt).Replace("#todate", todt);
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
					log.ErrorMessage("PODataProvider", "getnotifiedgrbydate", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<pendingreceiptslist>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<pending receipts list>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "pendingreceiptslist", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<pendingreceiptslist>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<pending receipts list>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ddlmodel>> pendingstogr()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string getstogrquery = WMSResource.getstogr;

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<ddlmodel>(
					  getstogrquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "pendingstogr", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getInitialstockfilename>>  Author :<<Ramesh>>  
		Date of Creation <<15-01-2021>>
		Purpose : <<pending intial stock putaway file names>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ddlmodel>> getInitialstockfilename()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					List<ddlmodel> files = new List<ddlmodel>();
					ddlmodel file = new ddlmodel();
					file.value = "ALL";
					file.text = "ALL";
					files.Add(file);
					string query = WMSResource.getfilenamesforis;

					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<ddlmodel>(
					  query, null, commandType: CommandType.Text);
					files = files.Concat(data).ToList();

					return files;


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getInitialstockfilename", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<UnholdGRdata>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Unhold GR data>>
		<param name="datamodel"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int UnholdGRdata(UnholdGRModel datamodel)
		{
			int result = 0;
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{


					pgsql.OpenAsync();
					string status = datamodel.unholdaction == true ? "accepted" : "returned";
					string qry = "Update wms.wms_securityinward set onhold = False,holdgrstatus='" + status + "',unholdedby = '" + datamodel.unholdedby + "',unholdedon = current_date,unholdremarks = '" + datamodel.unholdremarks + "' where inwmasterid = '" + datamodel.inwmasterid + "'";
					var results11 = pgsql.ExecuteScalar(qry);
					result = 1;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "updateonholdrow", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
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
		/*
		Name of Function : <<mattransfer>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<mat transfer>>
		<param name="datamodel"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int mattransfer(materialtransferMain datamodel)
		{
			int result = 0;
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				NpgsqlTransaction Trans = null;
				try
				{
					int transferqty = 0;
					string createdby = datamodel.transferedby;
					string remarks = datamodel.transferremarks;
					string materialid = "";
					string updatereturnqty = "";
					string fromprojectcode = datamodel.projectcodefrom;
					string mailto = "";

					pgsql.Open();
					Trans = pgsql.BeginTransaction();
					updatereturnqty = WMSResource.updatetransferdata;
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						var resultx = DB.ExecuteScalar(updatereturnqty, new
						{
							transferqty,
							createdby,
							remarks,
							datamodel.projectcode,
							materialid,
							datamodel.approvallevel,
							datamodel.finalapprovallevel,
							fromprojectcode
						});
						string tid = resultx.ToString();
						if (tid != "")
						{
							datamodel.transferid = tid;


							string rsltqry = WMSResource.inserttransferapproval;
							if (datamodel.finalapprovallevel > 1)
							{
								for (int i = 2; i <= datamodel.finalapprovallevel; i++)
								{
									if (i == 2)
									{
										string userquery = "select  * from wms.employee where employeeno='" + datamodel.projectmanagerfrom + "'";
										User userdata = pgsql.QuerySingle<User>(
										   userquery, null, commandType: CommandType.Text);
										mailto = userdata.email;
										if (datamodel != null)
										{
											string approverid = datamodel.projectmanagerfrom;
											string approvername = userdata.name;
											string approveremail = userdata.email;
											int approvallevel = 1;
											var resultxyy = DB.ExecuteScalar(rsltqry, new
											{
												datamodel.transferid,
												approverid,
												approvername,
												approveremail,
												approvallevel

											});

										}

									}
									if (i == 3)
									{
										string userquery = "select  * from wms.employee where employeeno='" + datamodel.projectmanagerto + "'";
										User userdata = pgsql.QuerySingle<User>(
										   userquery, null, commandType: CommandType.Text);
										if (datamodel != null)
										{
											string approverid = datamodel.projectmanagerto;
											string approvername = userdata.name;
											string approveremail = userdata.email;
											int approvallevel = 2;
											var resultxyy = DB.ExecuteScalar(rsltqry, new
											{
												datamodel.transferid,
												approverid,
												approvername,
												approveremail,
												approvallevel

											});

										}

									}

								}



							}


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
							Trans.Commit();
							EmailModel emailmodel = new EmailModel();
							emailmodel.transferid = datamodel.transferid.ToString();
							emailmodel.transferbody = "Material Transfer request initiated for approval with Transferid :" + datamodel.transferid.ToString();
							//emailmodel.ToEmailId = "developer1@in.yokogawa.com";
							emailmodel.ToEmailId = mailto;

							EmailUtilities emailobj = new EmailUtilities();
							emailobj.sendEmail(emailmodel, 14);
						}

					}



				}
				catch (Exception Ex)
				{
					Trans.Rollback();
					log.ErrorMessage("PODataProvider", "updateonholdrow", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return 0;
				}
				finally
				{
					pgsql.Close();
				}

			}
			return result;
		}


		/*
		Name of Function : <<mattransferapprove>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<mat transfer approve>>
		<param name="datamodel"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int mattransferapprove(List<materialtransferMain> datamodel)
		{
			int result = 0;
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				NpgsqlTransaction Trans = null;
				try
				{

					pgsql.Open();
					Trans = pgsql.BeginTransaction();

					List<EmailModel> emailmodels = new List<EmailModel>();
					foreach (materialtransferMain data in datamodel)
					{
						string mailto = "";
						DateTime todayDate = DateTime.Now;
						string currentdatestr = todayDate.ToString("yyyy-MM-dd");
						if (data.isapproved)
						{
							string delegatemanager = "";
							string querypmb = "select au.createdby as approverid from wms.auth_users au where au.isdelegatemember is true and au.deleteflag is not true and au.employeeid = '" + data.approverid + "' limit 1";
							var rslttmb = pgsql.ExecuteScalar(querypmb, null);


							string query = "update wms.wms_materialtransferapproval set approvaldate ='" + currentdatestr + "'";
							if (rslttmb == null)
							{
								query += " ,isapproved = " + data.isapproved + ", remarks = '" + data.approvalremarks + "'  where transferid = '" + data.transferid + "' and approverid = '" + data.approverid + "'";
							}
							else
							{
								delegatemanager = rslttmb.ToString();
								query += " ,isapproved = " + data.isapproved + ", remarks = '" + data.approvalremarks + "'  where transferid = '" + data.transferid + "' and (approverid = '" + data.approverid + "' or approverid = '" + delegatemanager + "' )";
							}


							var rslt = pgsql.ExecuteScalar(query);
							int nextlevel = data.approvallevel + 1;


							string query1 = "update wms.wms_transfermaterial set approvallevel = " + nextlevel + " where transferid = '" + data.transferid + "'";
							var rslt1 = pgsql.ExecuteScalar(query1);

							string userquery = "select approveremail from wms.wms_materialtransferapproval where approvallevel = " + nextlevel + "";
							var nextmailobj = pgsql.Query<materialtransferapproverModel>(
							userquery, null, commandType: CommandType.Text).ToList();
							if (nextmailobj != null && nextmailobj.Count() > 0)
							{
								mailto = nextmailobj[0].approveremail;
								EmailModel emailmodel1 = new EmailModel();
								emailmodel1.isnextapprover = true;
								emailmodel1.transferid = data.transferid.ToString();
								emailmodel1.transferbody = "Material Transfer request initiated for approval with Transferid :" + data.transferid.ToString();
								//emailmodel1.ToEmailId = "developer1@in.yokogawa.com";
								emailmodel1.ToEmailId = mailto;
								emailmodel1.FrmEmailId = "developer1@yokogawa.com";
								emailmodel1.CC = "ramesh.kumar@yokogawa.com";
								emailmodels.Add(emailmodel1);


							}
							else
							{

								mailto = data.requesteremail;
								EmailModel emailmodel1 = new EmailModel();
								emailmodel1.isnextapprover = false;
								emailmodel1.transferid = data.transferid.ToString();
								emailmodel1.transferbody = "Material Transfer request approved with Transferid :" + data.transferid.ToString();
								//emailmodel1.ToEmailId = "developer1@in.yokogawa.com";
								emailmodel1.ToEmailId = mailto;
								emailmodel1.FrmEmailId = "developer1@yokogawa.com";
								emailmodels.Add(emailmodel1);
							}

						}
						else if (!data.isapproved)
						{
							string delegatemanager = "";
							string querypmb = "select au.createdby as approverid from wms.auth_users au where au.isdelegatemember is true and au.deleteflag is not true and au.employeeid = '" + data.approverid + "' limit 1";
							var rslttmb = pgsql.ExecuteScalar(querypmb, null);
							string query = "update wms.wms_materialtransferapproval set approvaldate ='" + currentdatestr + "'";
							if (rslttmb == null)
							{
								query += " ,isapproved = " + data.isapproved + ", remarks = '" + data.approvalremarks + "'  where transferid = '" + data.transferid + "' and approverid = '" + data.approverid + "'";
							}
							else
							{
								delegatemanager = rslttmb.ToString();
								query += " ,isapproved = " + data.isapproved + ", remarks = '" + data.approvalremarks + "'  where transferid = '" + data.transferid + "' and (approverid = '" + data.approverid + "' or approverid = '" + delegatemanager + "')";
							}


							var rslt = pgsql.ExecuteScalar(query);



							string query1 = "update wms.wms_transfermaterial set approvallevel = 5 where transferid = '" + data.transferid + "'";
							var rslt1 = pgsql.ExecuteScalar(query1);
							mailto = data.requesteremail;
							EmailModel emailmodel1 = new EmailModel();
							emailmodel1.transferid = data.transferid.ToString();
							emailmodel1.isnextapprover = false;
							emailmodel1.transferbody = "Material Transfer request rejected with Transferid :" + data.transferid.ToString();
							//emailmodel1.ToEmailId = "developer1@in.yokogawa.com";
							emailmodel1.ToEmailId = mailto;
							emailmodel1.FrmEmailId = "ramesh.kumar@yokogawa.com";
							emailmodels.Add(emailmodel1);

						}
					}

					result = 1;
					Trans.Commit();
					foreach (EmailModel mdl in emailmodels)
					{
						EmailUtilities emailobj = new EmailUtilities();
						if (mdl.isnextapprover)
						{
							emailobj.sendEmail(mdl, 14);
						}
						else
						{
							emailobj.sendEmail(mdl, 18);
						}

					}





				}
				catch (Exception Ex)
				{
					Trans.Rollback();
					log.ErrorMessage("PODataProvider", "mattransferapprove", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return 0;
				}
				finally
				{
					pgsql.Close();
				}

			}
			return result;
		}

		/*
		Name of Function : <<getitemdeatils>>  Author :<<Prasanna>>  
		Date of Creation <<11-05-2021>>
		Purpose : <<based on inwardid  will get lst of items>>
		<param name="grnnumber"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<inwardModel>> getMRNmaterials(string grnnumber)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					await pgsql.OpenAsync();
					string queryforitemdetails = WMSResource.getmrnmaterialsbygr.Replace("#grnnumber", grnnumber);
					var data = await pgsql.QueryAsync<inwardModel>(
					   queryforitemdetails, null, commandType: CommandType.Text);
					return data;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getMRNmaterials", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}

		}

		/*
		Name of Function : <<mrnupdate>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<mrn update>>
		<param name="datamodel"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int mrnupdate(List<MRNsavemodel> datamodel)
		{
			int result = 0;
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{


					pgsql.OpenAsync();

					string insertqry = WMSResource.updateMRNMaterials;
					foreach (MRNsavemodel item in datamodel)
					{
						//string qry = "Update wms.wms_securityinward set isdirecttransferred = True,projectcode='" + datamodel.projectcode + "',mrnby = '" + datamodel.directtransferredby + "',mrnon = current_date,mrnremarks = '" + datamodel.mrnremarks + "' where grnnumber = '" + datamodel.grnnumber + "'";
						//var results11 = pgsql.ExecuteScalar(qry);
						string mrnby = item.directtransferredby;
						result = pgsql.Execute(insertqry, new
						{
							item.inwardid,
							item.projectcode,
							mrnby,
							item.mrnremarks,
							item.acceptedqty,
							item.issuedqty,
						});
					}
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "mrnupdate", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return 0;
				}
				finally
				{
					pgsql.Close();
				}

			}
			return result;
		}


		/*
		Name of Function : <<updateonholdrow>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<update onhold row>>
		<param name="datamodel"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "updateonholdrow", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
			return result;
		}

		/*
		Name of Function : <<updateinitialstockdata>>  Author :<<Ramesh>>  
		Date of Creation <<11-01-2021>>
		Purpose : <<update initial stock exception data>>
		<param name="datamodel"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string updateprojectmember(AssignProjectModel datamodel)
		{
			string result = "";
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					List<string> emplist = new List<string>();
					if (datamodel.projectmember != null)
					{
						if (datamodel.projectmember.Contains(","))
						{
							string[] arr = datamodel.projectmember.Split(',');
							emplist = arr.ToList();

						}
						else
						{
							emplist.Add(datamodel.projectmember);
						}
					}

					foreach (string str in emplist)
					{
						string storeQuery = "Select authid from wms.auth_users au where employeeid = '" + str + "' and roleid = 5";
						var storeId = pgsql.ExecuteScalar(storeQuery, null);
						if (storeId == null)
						{
							authUser model = new authUser();
							model.employeeid = str;
							model.createddate = DateTime.Now;
							model.roleid = 5;
							model.createdby = datamodel.modifiedby;
							model.deleteflag = false;
							model.emailccnotification = false;
							model.emailccnotification = false;
							model.subroleid = "1,2";
							model.plantid = datamodel.plantid;
							bool isdelegatemember = false;
							string insertquery = WMSResource.insertAuthUser;
							//model.createdby = model.modifiedby;
							var resultsx = pgsql.ExecuteScalar(insertquery, new
							{
								model.employeeid,
								model.roleid,
								model.createddate,
								model.createdby,
								model.deleteflag,
								model.emailnotification,
								model.emailccnotification,
								model.subroleid,
								model.plantid,
								isdelegatemember
							});
						}
					}

					var insertbinQuery = WMSResource.updateprojectmember.Replace("#projectcode", datamodel.projectcode);
					var results = pgsql.ExecuteScalar(insertbinQuery, new
					{
						datamodel.projectmember,
						datamodel.modifiedby
					});
					result = "saved";

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "updateprojectmember", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					result = Ex.Message;

				}
			}
			return result;
		}

		/*
	    Name of Function : <<updateinitialstockdata>>  Author :<<Ramesh>>  
		Date of Creation <<11-01-2021>>
		Purpose : <<update initial stock exception data>>
		<param name="datamodel"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string updateinitialstockdata(StockModel stag_data)
		{
			string result = "";
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				NpgsqlTransaction Trans = null;
				try
				{
					pgsql.Open();
					Trans = pgsql.BeginTransaction();
					bool deleteflag = false;
					//Add locator in masterdata
					string storeQuery = "Select locatorid from wms.wms_rd_locator where locatorname = '" + stag_data.locatorname + "'";
					var storeId = pgsql.ExecuteScalar(storeQuery, null);
					if (storeId == null)
					{
						LocationModel store = new LocationModel();
						store.locatorname = stag_data.locatorname;
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
					string rackQuery = "Select rackid from wms.wms_rd_rack where racknumber = '" + stag_data.racknumber + "' and locatorid=" + storeId + "";
					var rackId = pgsql.ExecuteScalar(rackQuery, null);
					if (rackId == null)
					{
						LocationModel store = new LocationModel();
						store.racknumber = stag_data.racknumber;
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
					string binQuery = "Select binid from wms.wms_rd_bin where binnumber = '" + stag_data.binnumber + "' and locatorid=" + storeId + " and rackid=" + rackId + "";
					var binId = pgsql.ExecuteScalar(binQuery, null);
					if (binId == null && (stag_data.binnumber != null && stag_data.binnumber != ""))
					{
						LocationModel store = new LocationModel();
						store.binnumber = stag_data.binnumber;
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
					string materialQuery = "Select material from wms.\"MaterialMasterYGS\" where material = '" + stag_data.Material + "'";
					var materialid = pgsql.ExecuteScalar(materialQuery, null);
					if (materialid == null)
					{
						LocationModel store = new LocationModel();
						store.materialid = stag_data.Material;
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
					stock.totalquantity = stag_data.availableqty;
					stock.availableqty = stag_data.availableqty;
					stock.shelflife = null;
					stock.createddate = DateTime.Now;
					stock.materialid = stag_data.Material;
					stock.poitemdescription = stag_data.materialdescription;
					stock.initialstock = true;
					string itemlocation = stag_data.locatorname + "." + stag_data.racknumber;
					if (stag_data.binnumber != "" && stag_data.binnumber != null)
					{
						itemlocation += "." + stag_data.binnumber;

					}
					int? bindata = null;
					if (stock.binid > 0)
					{
						bindata = stock.binid;

					}

					string uploadedby = stag_data.createdby;
					string uploadcode = stag_data.uploadbatchcode;
					string receivedid = stag_data.stockid.ToString();
					string receivedtype = "Initial Stock";
					stag_data.unitprice = stag_data.value / stag_data.availableqty;


					//insert wms_stock ##storeid, binid,rackid,totalquantity,shelflife ,createddate,materialid ,initialstock
					var insertquery = "INSERT INTO wms.wms_stock(storeid, binid,rackid,itemlocation,totalquantity,availableqty,shelflife ,createddate,materialid ,initialstock,stcktype,unitprice,value,pono,projectid,createdby,uploadbatchcode,uploadedfilename,poitemdescription,receivedid,receivedtype)VALUES(@storeid, @bindata,@rackid,@itemlocation,@totalquantity,@availableqty,@shelflife ,@createddate,@materialid ,@initialstock,@stocktype,@unitprice,@value,@pono,@projectid,@uploadedby,@uploadcode,@uploadedfilename,@poitemdescription,@receivedid,@receivedtype)";
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
						uploadedby,
						uploadcode,
						stag_data.uploadedfilename,
						stock.poitemdescription,
						receivedid,
						receivedtype

					});
					if (!string.IsNullOrEmpty(stag_data.pono) && stag_data.pono != "" && stag_data.pono.ToString().Trim() != "reserved")
					{
						List<string> pos = new List<string>();
						if (stag_data.pono.Contains("/"))
						{

							string[] arr = stag_data.pono.Split('/');
							pos = arr.ToList();
						}
						else if (stag_data.pono.Contains(","))
						{
							string[] arr = stag_data.pono.Split(',');
							pos = arr.ToList();
						}
						else
						{
							pos.Add(stag_data.pono);
						}
						foreach (string str in pos)
						{
							string pono = str.Trim();
							string query2 = "Select Count(*) as count from wms.wms_project where pono = '" + pono + "'";
							int Projcount = int.Parse(pgsql.ExecuteScalar(query2, null).ToString());

							if (Projcount == 0)
							{
								string jobname = null;
								string projectcode = null;
								string projecttext = null;
								string projectmanager = null;

								string uploadtype = "Initial Stock";
								if (!string.IsNullOrEmpty(stag_data.projectid) && stag_data.projectid != "")
								{
									projectcode = stag_data.projectid;

								}

								string querypm = "Select Max(projectmanager) as projectmanager from wms.wms_project where  projectcode = '" + projectcode + "' group by projectmanager";
								var rsltt = pgsql.ExecuteScalar(querypm, null);
								if (rsltt != null)
								{
									projectmanager = rsltt.ToString();
								}

								//insert wms_project ##pono,jobname,projectcode,projectname,projectmanager,
								var insertqueryx = "INSERT INTO wms.wms_project(pono, jobname, projectcode,projectname,projectmanager,uploadcode,uploadtype)VALUES(@pono, @jobname,@projectcode,@projecttext,@projectmanager,@uploadcode,@uploadtype)";
								var resultsx = pgsql.ExecuteScalar(insertqueryx, new
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



					Trans.Commit();


					result = "Saved";

				}
				catch (Exception Ex)
				{
					Trans.Rollback();
					log.ErrorMessage("PODataProvider", "updateinitialstockdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return Ex.Message;
				}
				finally
				{
					pgsql.Close();
				}

			}
			return result;
		}

		/*
		Name of Function : <<getdepartmentmasterdata>>  Author :<<Ramesh>>  
		Date of Creation <<28/07/2020>>
		Purpose : <<get organasation dropdown>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "getdepartmentmasterdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<stomatrequestapprove>>  Author :<<Ramesh>>  
		Date of Creation <<12-02-2021>>
		Purpose : <<mat transfer approve>>
		<param name="datamodel"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string updaterba(List<rbamaster> datamodel)
		{
			string result = "";
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				NpgsqlTransaction Trans = null;
				try
				{

					pgsql.Open();
					Trans = pgsql.BeginTransaction();

					foreach (rbamaster data in datamodel)
					{


						string insertdataqry = WMSResource.updaterba.Replace("#roleid", data.roleid.ToString());
						var result1 = pgsql.Execute(insertdataqry, new
						{
							data.inv_enquiry,
							data.inv_reports,
							data.gate_entry,
							data.gate_entry_barcode,
							data.inv_receipt_alert,
							data.receive_material,
							data.put_away,
							data.material_return,
							data.material_transfer,
							data.gate_pass,
							data.gatepass_inout,
							data.gatepass_approval,
							data.material_issue,
							data.material_request,
							data.material_reservation,
							data.abc_classification,
							data.cyclecount_configuration,
							data.cycle_counting,
							data.cyclecount_approval,
							data.admin_access,
							data.masterdata_creation,
							data.masterdata_updation,
							data.masterdata_approval,
							data.printbarcodes,
							data.modified_by,
							data.pmdashboard_view,
							data.quality_check,
							data.min,
							data.direct_transfer_view,
							data.notify_to_finance,
							data.gr_process,
							data.material_transfer_approval,
							data.asn_view,
							data.internal_stock_transfer,
							data.miscellanous,
							data.materialrequest_approval,
							data.assign_pm,
							data.annexure_report,
							data.initialstock_upload,
							data.inventory_management,
							data.all_reports

						});


					}
					result = "saved";
					Trans.Commit();
				}
				catch (Exception Ex)
				{
					Trans.Rollback();
					log.ErrorMessage("PODataProvider", "updaterba", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return Ex.Message;
				}
				finally
				{
					pgsql.Close();
				}

			}
			return result;
		}

		/*
		Name of Function : <<getrbadetails>>  Author :<<Ramesh>>  
		Date of Creation <<28/07/2020>>
		Purpose : <<get rba details>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<rbamaster>> getrbadetails()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = "select rm.rolename,wr.* from wms.wms_rbamaster wr left outer join wms.rolemaster rm on wr.roleid = rm.roleid order by rm.roleid";

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<rbamaster>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getrbadetails", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<insertdatacsv>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<insert data csv>>
		<param name="obj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int insertdatacsv(ddlmodel obj)
		{
			ddlmodel mdl = new ddlmodel();

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = "select material as value,materialdescription as text from  wms.\"MaterialMasterYGS\"";
					var datalist = pgsql.Query<ddlmodel>(
					materialrequestquery, null, commandType: CommandType.Text);
					var filePath = @"D:\A_StagingTable\StockstagecsvTest1.xlsx";
					//var filePath = "D:\A_StagingTable\StockstagecsvTest1.xlsx";
					foreach (ddlmodel ddl in datalist)
					{




						Random rnd = new Random();
						int str = rnd.Next(1, 3);
						int rk = rnd.Next(1, 6);
						int bn = rnd.Next(1, 6);
						int qty = rnd.Next(50, 500);
						string store = "store" + str.ToString();
						string rack = "a" + rk.ToString();
						string bin = "bin" + bn.ToString();
						//making query    
						string query = "INSERT INTO wms.testcsv (material, material_description, store,rack, bin,quantity,grn,received_date,shelf_life_expiration,date_of_Manufacture,datasource,data_entered_by,data_entered_on) ";
						query += " VALUES('" + ddl.value + "','" + ddl.text + "','" + store + "','" + rack + "','" + bin + "'," + qty + ",'" + string.Empty + "','22/09/2020','22/12/2020','22/05/2020','" + string.Empty + "','303268','22/05/2020')";
						var resultsx = pgsql.ExecuteScalar(query);
					}

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "insertdatacsv", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return 0;
				}
				finally
				{
					pgsql.Close();
				}



			}


			return 1;


		}


		/*
		Name of Function : <<requesttoreserve>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<request to reserve>>
		<param name="obj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int requesttoreserve(materialReservetorequestModel obj)
		{

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				NpgsqlTransaction Trans = null;
				try
				{


					pgsql.Open();
					Trans = pgsql.BeginTransaction();
					string query = "update wms.materialreserve set requestedby = '" + obj.requestedby + "', requestedon = current_date  where reserveid = '" + obj.reserveid + "'";
					var results11 = pgsql.ExecuteScalar(query);

					string materialrequestquery = WMSResource.getreservedatabyid.Replace("#reserveid", obj.reserveid.ToString());
					var datalist = pgsql.Query<ReserveMaterialModel>(
					  materialrequestquery, null, commandType: CommandType.Text);


					List<IssueRequestModel> reqdata = new List<IssueRequestModel>();
					foreach (ReserveMaterialModel rv in datalist)
					{
						decimal? reservedqty = rv.reservedqty;
						int itemid = rv.itemid;


						string queryxx = "update wms.wms_stock set availableqty = availableqty + " + reservedqty + " where itemid = '" + itemid + "'";
						var results11xx = pgsql.ExecuteScalar(queryxx);
						IssueRequestModel model = new IssueRequestModel();
						model.quantity = rv.reservedqty;
						model.requesteddate = System.DateTime.Now;
						model.approveremailid = null;
						model.approverid = null;
						model.pono = rv.pono;
						model.materialid = rv.materialid;
						model.requesterid = obj.requestedby;
						model.requestedquantity = 0;
						model.projectcode = rv.projectcode;
						model.calltype = "fromreserve";
						model.reserveid = obj.reserveid;
						model.Materialdescription = rv.poitemdescription;
						reqdata.Add(model);

					}
					int saverequest = updaterequestedqty(reqdata);
					if (saverequest == 0)
					{
						Trans.Rollback();
						return 0;
					}



					Trans.Commit();
					return 1;


				}
				catch (Exception Ex)
				{
					Trans.Rollback();
					log.ErrorMessage("PODataProvider", "updateonholdrow", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return 0;
				}
				finally
				{
					pgsql.Close();
				}



			}
		}

		/*
		Name of Function : <<updatematmovement>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<update mat movement>>
		<param name="obj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int updatematmovement(List<materialistModel> obj)
		{

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				var status = "";
				try
				{
					pgsql.OpenAsync();

					foreach (materialistModel mat in obj)
					{
						//gatepassid integer references wms.wms_gatepass(gatepassid) not null,
						//gatepassmaterialid integer references wms.wms_gatepassmaterial(gatepassmaterialid) not null,
						//outwarddate TIMESTAMP without time zone null,
						//outwardby varchar(50) null,
						//outwardremarks text null,
						//outwardqty integer null,
						//inwarddate TIMESTAMP without time zone null,
						//inwardby varchar(50) null,
						//intwardremarks text null,
						//inwardqty integer null
						int results11 = 0;
						string query = "";
						int gatepassmaterialid = Convert.ToInt32(mat.gatepassmaterialid);
						if (mat.movetype == "out")
						{
							status = "OutBound";
							query = "insert into wms.outwatdinward(gatepassid, gatepassmaterialid, outwarddate, outwardby, outwardremarks, outwardqty,type)";
							query += " values('" + mat.gatepassid + "', " + gatepassmaterialid + ", '" + mat.outwarddatestring + "', '" + mat.movedby + "', '" + mat.remarks + "', " + mat.outwardqty + ", '" + mat.type + "')";
							var resultsx = pgsql.ExecuteScalar(query);
							//query = WMSResource.outwardinsertquery;
							//results11 = pgsql.Execute(query, new
							//{
							//	mat.gatepassid,
							//	gatepassmaterialid,
							//	mat.outwarddatestring,
							//	mat.movedby,
							//	mat.remarks,
							//	mat.outwardqty
							//});

							//query = "update wms.wms_gatepassmaterial set outwardqty = " + mat.outwardqty + ", outwarddate = '" + mat.outwarddatestring + "' , outwardedby='" + mat.movedby + "',outwardremarks='" + mat.remarks + "' where gatepassmaterialid = " + mat.gatepassmaterialid + "";
						}

						else if (mat.movetype == "in")
						{
							status = "InBound";
							query = "insert into wms.outwatdinward(gatepassid, gatepassmaterialid,inwardqty, securityinwardby, securityinwarddate, securityinwardremarks,type)";
							query += " values('" + mat.gatepassid + "', " + gatepassmaterialid + "," + mat.inwardqty + ",'" + mat.movedby + "', '" + mat.inwarddatestring + "', '" + mat.remarks + "', '" + mat.type + "')";
							var resultsx = pgsql.ExecuteScalar(query);
							//query = "update wms.wms_gatepassmaterial set inwardqty = " + mat.inwardqty + ", inwarddate = '" + mat.inwarddatestring + "' , inwardedby='" + mat.movedby + "',inwardremarks='" + mat.remarks + "' where gatepassmaterialid = " + mat.gatepassmaterialid + "";
							//results11 = pgsql.ExecuteScalar(query);
							//query = WMSResource.inwardinsertquery;
							//results11 = pgsql.Execute(query, new
							//{
							//	mat.gatepassid,
							//	gatepassmaterialid,
							//	mat.movedby,
							//	mat.remarks,
							//	mat.inwardqty
							//});
						}
						else
						{
							query = WMSResource.inwardinsertquery;
							results11 = pgsql.Execute(query, new
							{
								mat.gatepassid,
								gatepassmaterialid,
								mat.movedby,
								mat.remarks,
								mat.inwardqty
							});

						}

					}
					if (!string.IsNullOrEmpty(status))
					{
						var updatequery = "update wms.wms_invstocktransfer set status ='" + status + "' where transferid ='" + obj[0].gatepassid + "'";
						var resultsx = pgsql.ExecuteScalar(updatequery);
					}


					return 1;


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "updateonholdrow", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return 0;
				}
				finally
				{
					pgsql.Close();
				}



			}
		}


		/*
		Name of Function : <<getstocktype>>  Author :<<Ramesh>>  
		Date of Creation <<22/07/2020>>
		Purpose : <<get stock type>>
		<param name="locdetails"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "getstocktype", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<UpdateMaterialReserve>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Update Material Reserve>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int UpdateMaterialReserve()
		{
			int result = 0;
			List<ReserveMaterialModel> _listobj = new List<ReserveMaterialModel>();
			EmailUtilities emailobj = new EmailUtilities();
			EmailModel emailmodel = new EmailModel();

			emailmodel.CC = "sushma.patil@yokogawa.com";
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
								emailmodel.ToEmailId = items.email;
								emailobj.sendEmail(emailmodel, 10);
							}
							else if (reservedate == date.AddDays(1))
							{
								emailmodel.ToEmailId = items.email;
								emailobj.sendEmail(emailmodel, 11);
							}
							else if (reservedate == date.AddDays(2))
							{
								emailmodel.ToEmailId = items.email;
								emailobj.sendEmail(emailmodel, 12);
							}
						}
					}
				}
				catch (Exception Ex)
				{

					log.ErrorMessage("PODataProvider", "UpdateMaterialReserve", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return 0;
				}
				finally
				{
					pgsql.Close();
				}

			}

			return result;


		}

		/*
		Name of Function : <<getstockdetails>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get stock details>>
		<param name="materialid"></param>
		 <param name="pono"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

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
				catch (Exception Ex)
				{

					log.ErrorMessage("PODataProvider", "getstockdetails", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return objstock;
				}
				finally
				{
					pgsql.Close();
				}

			}

			return objstock;

		}/*
		Name of Function : <<UpdateReturnqty_old>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<inserting the data to retrunmaterial by pm>>
		<param name="_listobj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int UpdateReturnqty_old(List<IssueRequestModel> _listobj)
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
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "UpdateReturnqty", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return 0;
				}

			}

			return result;
		}

		/*
		Name of Function : <<UpdateReturnqty>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<inserting the data to retrunmaterial by pm>>
		<param name="_listobj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int UpdateReturnqty(List<IssueRequestModel> _listobj)
		{
			int result = 0;

			if (_listobj.Count != 0)
			{
				NpgsqlTransaction Trans = null;

				try
				{

					using (var DB = new NpgsqlConnection(config.PostgresConnectionString))
					{



						IssueRequestModel obj = new IssueRequestModel();
						DB.Open();
						Trans = DB.BeginTransaction();
						string query = WMSResource.getnxtreturnid;
						string createdby = _listobj[0].createdby;
						string mainquery = WMSResource.insertmaterialreturnquery;
						var rslt = DB.ExecuteScalar(mainquery, new
						{
							_listobj[0].reason,
							createdby
						});
						if (rslt != null)
						{
							var returnid = rslt.ToString();
							string updatereturnqty = string.Empty;
							foreach (var item in _listobj)
							{
								string id = Guid.NewGuid().ToString();


								if (item.returnqty != 0)
								{
									item.materialid = item.material;
									updatereturnqty = WMSResource.insertmaterialreturndetails;
									string poitemdescription = item.Materialdescription;

									result = DB.Execute(updatereturnqty, new
									{
										id,
										returnid,
										item.materialid,
										poitemdescription,
										item.returnqty,
										item.projectcode,
										item.pono,
										item.materialcost,
										item.remarks


									});
								}
							}

							Trans.Commit();
							EmailModel emailmodel = new EmailModel();
							emailmodel.returnid = rslt.ToString();

							emailmodel.CC = "ramesh.kumar@yokogawa.com";
							EmailUtilities emailobj = new EmailUtilities();
							emailobj.sendEmail(emailmodel, 19, 3);

						}
						else
						{
							Trans.Rollback();
						}
					}
				}
				catch (Exception Ex)
				{
					Trans.Rollback();
					log.ErrorMessage("PODataProvider", "UpdateReturnqty", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return 0;
				}

			}

			return result;
		}

		/*
		Name of Function : <<UpdateReturnmaterialTostock>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Update Returnmaterial To stock>>
		<param name="model"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int UpdateReturnmaterialTostock(List<IssueRequestModel> model)
		{
			int result = 0;
			if (model.Count > 0)
			{
				NpgsqlTransaction Trans = null;
				using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				{
					try
					{
						pgsql.Open();
						Trans = pgsql.BeginTransaction();
						string updatereturnqtytomaterialissue = WMSResource.updatereturnqtyByInvMngr.Replace("@returnid", Convert.ToString(model[0].returnid));
						result = pgsql.Execute(updatereturnqtytomaterialissue, new
						{

						});
						if (result != 0)
						{
							foreach (var item in model)
							{
								decimal? availableqty = item.confirmqty;
								string materialid = item.material;
								string createdby = item.createdby;
								string returnid = item.id;
								int? binid = null;
								if (item.binid != 0)
								{
									binid = item.binid;

								}

								//string updatereturnqtytostock = WMSResource.updatereturnmaterialToStock.Replace("@availableqty", Convert.ToString(item.returnqty)).Replace("@itemid", Convert.ToString(item.itemid));
								string updatereturnqtytostock = WMSResource.updatetostockbyinvmanger;
								string poitemdescription = item.poitemdescription;
								string receivedtype = "Material Return";
								decimal? unitprice = item.materialcost / availableqty;
								result = pgsql.Execute(updatereturnqtytostock, new
								{

									materialid,
									item.itemlocation,
									availableqty,
									createdby,
									returnid,
									item.stocktype,
									item.storeid,
									item.rackid,
									binid,
									item.pono,
									item.projectcode,
									receivedtype,
									poitemdescription,
									item.materialcost,
									unitprice

								});

							}
						}

						Trans.Commit();

					}
					catch (Exception Ex)
					{
						Trans.Rollback();
						log.ErrorMessage("PODataProvider", "UpdateReturnmaterialTostock", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
						return 0;

					}
					finally
					{
						pgsql.Close();
					}



				}

				//   foreach (var item in model)
				//  {
				//	try
				//	{
				//                    string updatereturnqtytomaterialissue = WMSResource.updatereturnqtyByInvMngr.Replace("@returnid", Convert.ToString(item.returnid));
				//                    using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				//                    {
				//                        result = DB.Execute(updatereturnqtytomaterialissue, new
				//                        {

				//                        });
				//                    }
				//                    if (result != 0)
				//                    {
				//                        int availableqty = item.confirmqty;
				//			string materialid = item.material;
				//			string createdby = item.createdby;
				//		    string returnid = item.id;
				//			int? binid = null;
				//			if(item.binid != 0)
				//                        {
				//				binid = item.binid;

				//			}

				//			//string updatereturnqtytostock = WMSResource.updatereturnmaterialToStock.Replace("@availableqty", Convert.ToString(item.returnqty)).Replace("@itemid", Convert.ToString(item.itemid));
				//			string updatereturnqtytostock = WMSResource.updatetostockbyinvmanger;
				//			using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				//			{
				//				result = DB.Execute(updatereturnqtytostock, new
				//				{

				//					materialid,
				//					item.itemlocation,
				//					availableqty,
				//					createdby,
				//					returnid,
				//					item.stocktype,
				//					item.storeid,
				//					item.rackid,
				//					binid
				//				});
				//			}
				//		}
				//	}
				//	catch (Exception Ex)
				//	{
				//		log.ErrorMessage("PODataProvider", "UpdateReturnmaterialTostock", Ex.StackTrace.ToString(), Ex.Message.ToString(),url);
				//		return 0;
				//	}
				//}
			}
			return result;
		}

		/*
		Name of Function : <<updateputawayfilename>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<update putaway filename>>
		<param name="file"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "updateputawayfilename", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return "error";
			}

			return result;
		}

		/*
		Name of Function : <<notifyputaway>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<notify put away>>
		<param name="data"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
						EmailUtilities emailobj = new EmailUtilities();
						emailobj.sendEmail(emailmodel, 13, 10);


					}
					else
					{
						result = "error";
					}
				}

			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "notifyputaway", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return "error";
			}

			return result;
		}

		/*
		Name of Function : <<notifymultipleputaway>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<notify multiple putaway>>
		<param name="datalst"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
				EmailUtilities emailobj = new EmailUtilities();
				emailobj.sendEmail(emailmodel, 13, 10);



			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "notifyputaway", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return "error";
			}

			return result;
		}

		/*
		Name of Function : <<GetReturnmaterialList>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : << onload to display the returned already by PM>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> GetReturnmaterialList()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string query = WMSResource.GetreturnList_v1;
					string updatequery = string.Empty;
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);
					return data;

				}
				catch (Exception Ex)
				{

					log.ErrorMessage("PODataProvider", "GetReturnmaterialList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}


		}

		/*
		Name of Function : <<GetReturnmaterialListForConfirm>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<based on request/return id we will get details for confirm>>
		<param name="requestid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
				catch (Exception Ex)
				{

					log.ErrorMessage("PODataProvider", "GetReturnmaterialListForConfirm", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}


		}

		/*
		Name of Function : <<getreturndata_old>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<getreturn data by empno>>
		<param name="empno"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> getreturndata_old(string empno)
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
				catch (Exception Ex)
				{

					log.ErrorMessage("PODataProvider", "getreturndata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}

		/*
		Name of Function : <<getreturndata>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get return data>>
		<param name="empno"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<MaterialReturn>> getreturndata(string empno)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string query = WMSResource.getmaterialreturnquery.Replace("#createdby", empno);
					var data = await pgsql.QueryAsync<MaterialReturn>(
					  query, null, commandType: CommandType.Text);
					if (data != null && data.Count() > 0)
					{
						foreach (MaterialReturn mat in data)
						{
							string query1 = WMSResource.getmaterialreturndetailquery.Replace("#returnid", mat.returnid); ;
							var data1 = await pgsql.QueryAsync<MaterialReturnTR>(
							  query1, null, commandType: CommandType.Text);
							if (data1 != null && data1.Count() > 0)
							{
								mat.materialdata = data1.ToList();
							}
						}


					}



					return data;

				}
				catch (Exception Ex)
				{

					log.ErrorMessage("PODataProvider", "getreturndata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}

		/*
		Name of Function : <<getmaterialreturnreqList>>  Author :<<Gayathri>>  
		Date of Creation <<14/08/2020>>
		Purpose : <<get materials requested for return data based on material return requested id >>
		<param name="matreturnid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> getmaterialreturnreqList(string matreturnid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					//string query = WMSResource.getmatreturndetails.Replace("@matreid", Convert.ToString(matreturnid));
					string query = WMSResource.getmaterialreturndetails.Replace("#returnid", Convert.ToString(matreturnid));
					string updatequery = string.Empty;
					//string updatedon = WMSResource.updatedon;
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);
					return data;

				}
				catch (Exception Ex)
				{

					log.ErrorMessage("PODataProvider", "gettransferdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}

		/*
		Name of Function : <<gettransferdata>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get transferred data based on login id>>
		<param name="empno"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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

							string query2 = WMSResource.getapproverdatabyid.Replace("#transferid", dt.transferid.ToString());
							var datadetail1 = await pgsql.QueryAsync<materialtransferapproverModel>(
							   query2, null, commandType: CommandType.Text);

							if (datadetail1 != null && datadetail1.Count() > 0)
							{
								dt.approverdata = datadetail1.ToList();
							}
						}
					}
					return data;

				}
				catch (Exception Ex)
				{

					log.ErrorMessage("PODataProvider", "gettransferdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}

		/*
		Name of Function : <<gettransferdataforapproval>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get transferred data based on login id>>
		<param name="empno"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<materialtransferMain>> gettransferdataforapproval(string empno)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string query = WMSResource.gettransferdataforapproval.Replace("#approver", empno);
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

							string query2 = WMSResource.getapproverdatabyid.Replace("#transferid", dt.transferid.ToString());
							var datadetail1 = await pgsql.QueryAsync<materialtransferapproverModel>(
							   query2, null, commandType: CommandType.Text);

							if (datadetail1 != null && datadetail1.Count() > 0)
							{
								dt.approverdata = datadetail1.ToList();
							}
						}
					}
					return data.OrderByDescending(o => o.transferid);

				}
				catch (Exception Ex)
				{

					log.ErrorMessage("PODataProvider", "gettransferdataforapproval", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}

		/*
		Name of Function : <<getrequestdataforapproval>>  Author :<<Ramesh>>  
		Date of Creation <<01-02-2021>>
		Purpose : <<get requested data for approval>>
		<param name="empno"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<MaterialTransaction>> getrequestdataforapproval(string empno)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				string materialrequestquery = WMSResource.getMaterialRequestForApproval.Replace("#reqid", empno);

				try
				{
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<MaterialTransaction>(
					   materialrequestquery, null, commandType: CommandType.Text);
					foreach (MaterialTransaction trans in data)
					{
						trans.materialdata = new List<MaterialTransactionDetail>();
						string materialrequestdataquery = WMSResource.getmaterialrequestdata.Replace("#requestid", trans.requestid);
						var data1 = await pgsql.QueryAsync<MaterialTransactionDetail>(
						materialrequestdataquery, null, commandType: CommandType.Text);
						trans.materialdata = data1.ToList();

					}
					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getrequestdataforapproval", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getrequestdataforSTOapproval>>  Author :<<Ramesh>>  
		Date of Creation <<12-02-2021>>
		Purpose : <<get requested data for approval>>
		<param name="empno"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<invstocktransfermodel>> getrequestdataforSTOapproval(string empno, string type)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				string materialrequestquery = WMSResource.getSTOSubConForApproval.Replace("#reqid", empno).Replace("#type", type);

				try
				{
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<invstocktransfermodel>(
					   materialrequestquery, null, commandType: CommandType.Text);
					foreach (invstocktransfermodel trans in data)
					{
						trans.materialdata = new List<stocktransfermateriakmodel>();
						string materialrequestdataquery = WMSResource.getSTOSubconMatDetails.Replace("#requestid", trans.transferid).Replace("#type", type);
						var data1 = await pgsql.QueryAsync<stocktransfermateriakmodel>(
						materialrequestdataquery, null, commandType: CommandType.Text);
						trans.materialdata = data1.ToList();

					}
					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getrequestdataforapproval", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getdirecttransferdata>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get direct transferred data>>
		<param name="empno"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<DirectTransferMain>> getdirecttransferdata(string empno)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					//string query = WMSResource.directtransfermainquery.Replace("#empno", empno);
					string query = WMSResource.directtransferreportmainquery.Replace("#empno", empno);
					string updatequery = string.Empty;
					//string updatedon = WMSResource.updatedon;
					var data = await pgsql.QueryAsync<DirectTransferMain>(
					   query, null, commandType: CommandType.Text);
					if (data != null && data.Count() > 0)
					{
						foreach (DirectTransferMain dt in data)
						{
							string query1 = WMSResource.directtransfermatdetail.Replace("#inw", dt.inwmasterid.ToString());
							var datadetail = await pgsql.QueryAsync<DirectTransferTR>(
							   query1, null, commandType: CommandType.Text);

							if (datadetail != null && datadetail.Count() > 0)
							{
								dt.materialdata = datadetail.ToList();
							}
						}
					}
					return data;

				}
				catch (Exception Ex)
				{

					log.ErrorMessage("PODataProvider", "gettransferdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}


		/*
	Name of Function : <<getSTORequestdatalist>>  Author :<<Ramesh>>  
	Date of Creation <<09-02-2021>>
	Purpose : <<get STO request data>>
	Review Date :<<>>   Reviewed By :<<>>
	*/
		public async Task<IEnumerable<STOrequestTR>> getSTORequestdatalist(string transferid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string query1 = WMSResource.getSTORequestMaterialsForPutaway.Replace("#transferid", transferid);

					var datadetail = await pgsql.QueryAsync<STOrequestTR>(
					query1, null, commandType: CommandType.Text);




					return datadetail;

				}
				catch (Exception Ex)
				{

					log.ErrorMessage("PODataProvider", "getSTORequestdatalist", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}




		/*
	Name of Function : <<STORequestlist>>  Author :<<Gayathri>>  
	Date of Creation <<12-01-2021>>
	Purpose : <<get STO request data>>
	Review Date :<<>>   Reviewed By :<<>>
	*/
		public async Task<IEnumerable<STORequestdata>> STORequestlist()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					//string query = WMSResource.getSTORequestlist;
					//query += "and status ='Issued' order by transferid desc";
					string query = WMSResource.getSTORequestForPutaway;
					var data = await pgsql.QueryAsync<STORequestdata>(
					   query, null, commandType: CommandType.Text);

					return data;

				}
				catch (Exception Ex)
				{

					log.ErrorMessage("PODataProvider", "STORequestlist", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}



		/*
		Name of Function : <<Updatetransferqty>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : << update/insert transfer material details>>
		<param name="_listobj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					catch (Exception Ex)
					{
						log.ErrorMessage("PODataProvider", "UpdateReturnqty", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
						return 0;
					}
				}
			}
			return result;
		}

		/*
		Name of Function : <<checkMatExists>>  Author :<<Gayathri>>  
		Date of Creation <<06/08/2020>>
		Purpose : <<check whether the material exists and generate qrcode for material>>
		<param name="material"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
						var writer = new BarcodeWriter
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
					log.ErrorMessage("PODataProvider", "checkMatExists", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getPODetails>>  Author :<<Ramesh>>  
		Date of Creation <<10/08/2020>>
		Purpose : <<Get podetails list>>
		<param name="empno"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<PODetails>> getPODetails(string empno)
		{
			//List<PODetails> objPO = new List<PODetails>();
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string getpoquery = WMSResource.getPODetails.Replace("#manager", empno);

					var objPO = await pgsql.QueryAsync<PODetails>(
					   getpoquery, null, commandType: CommandType.Text);
					//objPO = pgsql.QueryAsync<List<PODetails>>(
					//			getpoquery, null, commandType: CommandType.Text);

					return objPO;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getPODetails", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
			}
		}

		/*
		Name of Function : <<getPODetailsByProjectCode>>  Author :<<Ramesh>>  
		Date of Creation <<18/02/2021>>
		Purpose : <<Get podetails list>>
		<param name="empno"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<PODetails>> getPODetailsByProjectCode(string empno, string projectcode)
		{
			//List<PODetails> objPO = new List<PODetails>();
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string getpoquery = WMSResource.getPODetailsByProjectCode_v1.Replace("#projectcode", projectcode);

					var objPO = await pgsql.QueryAsync<PODetails>(
					   getpoquery, null, commandType: CommandType.Text);
					//objPO = pgsql.QueryAsync<List<PODetails>>(
					//			getpoquery, null, commandType: CommandType.Text);
					objPO = objPO.Where(o => o.projectmanager == empno || o.projectmember.Contains(empno));
					return objPO;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getPODetailsByProjectCode", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
			}
		}

		/*
		Name of Function : <<getStorePODetailsByProjectCode>>  Author :<<Ramesh>>  
		Date of Creation <<26/05/2021>>
		Purpose : <<Get podetails list>>
		<param name="empno"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<PODetails>> getStorePODetailsByProjectCode(string empno, string projectcode)
		{
			//List<PODetails> objPO = new List<PODetails>();
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string getpoquery = WMSResource.getStorePODetailsByProjectCode_v1.Replace("#projectcode", projectcode);

					var objPO = await pgsql.QueryAsync<PODetails>(
					   getpoquery, null, commandType: CommandType.Text);
					//objPO = pgsql.QueryAsync<List<PODetails>>(
					//			getpoquery, null, commandType: CommandType.Text);

					objPO = objPO.Where(o => o.projectmanager == empno || o.projectmember.Contains(empno));

					return objPO;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getStorePODetailsByProjectCode", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
			}
		}


		/*
		Name of Function : <<getPODetailsByProjectCode>>  Author :<<Ramesh>>  
		Date of Creation <<18/02/2021>>
		Purpose : <<Get podetails list>>
		<param name="empno"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<PODetails>> getPODetailsbyprojectcodeformiscissue(string projectcode)
		{
			//List<PODetails> objPO = new List<PODetails>();
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string getpoquery = WMSResource.podetailsformiscissue.Replace("#projectcode", projectcode);

					var objPO = await pgsql.QueryAsync<PODetails>(
					   getpoquery, null, commandType: CommandType.Text);
					//objPO = pgsql.QueryAsync<List<PODetails>>(
					//			getpoquery, null, commandType: CommandType.Text);

					return objPO;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getPODetailsByProjectCode", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
			}
		}

		/*
		Name of Function : <<gettestcrud>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get test crud>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
					log.ErrorMessage("PODataProvider", "gettestcrud", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		/*
		Name of Function : <<get initial stock>>  Author :<<Ramesh>>  
		Date of Creation <<11-11-2019>>
		Purpose : <<get currently uploaded initial stock>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<StockModel>> getinitialstock(string code)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string testgetquery = WMSResource.initialstockviewdata.Replace("#code", code);


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<StockModel>(
					  testgetquery, null, commandType: CommandType.Text);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getinitialstock", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		/*
		Name of Function : <<get initial stock>>  Author :<<Ramesh>>  
		Date of Creation <<11-11-2019>>
		Purpose : <<get currently uploaded initial stock>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<StockModel>> getinitialstockall(string code)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					//string testgetquery = WMSResource.getallinitialstockdata.Replace("#code", code);
					string testgetquery = WMSResource.initialstockunionqueryforreport.Replace("#code", code);


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<StockModel>(
					  testgetquery, null, commandType: CommandType.Text);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getinitialstock", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		/*
		Name of Function : <<get initial stock Exception>>  Author :<<Ramesh>>  
		Date of Creation <<11-11-2019>>
		Purpose : <<get currently uploaded initial stock>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<StockModel>> getinitialstockEX(string code)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string testgetquery = WMSResource.initialstockExceptions.Replace("#code", code);


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<StockModel>(
					  testgetquery, null, commandType: CommandType.Text);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "gettestcrud", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		/*
		Name of Function : <<get Material in hand>>  Author :<<Ramesh>>  
		Date of Creation <<17-11-2019>>
		Purpose : <<get Materials in stock>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<MaterialinHand>> getmatinhand(inventoryFilters filters)
		{
			List<MaterialinHand> objmat = new List<MaterialinHand>();
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string testgetquery = WMSResource.inventoryreportqry;
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<MaterialinHand>(
					 testgetquery, null, commandType: CommandType.Text);


					objmat = data.ToList();
					return objmat;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getmatinhand", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return objmat;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		/*
		Name of Function : <<get Material in hand>>  Author :<<Ramesh>>  
		Date of Creation <<17-11-2019>>
		Purpose : <<get Materials in stock>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<MaterialinHand>> getmatinhand_old(inventoryFilters filters)
		{
			List<MaterialinHand> objmat = new List<MaterialinHand>();
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string testgetquery = WMSResource.inhandmaterial;

					if (!string.IsNullOrEmpty(filters.itemlocation))

						//Get materials in stock table 
						await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<MaterialinHand>(
					  testgetquery, null, commandType: CommandType.Text);

					foreach (var mat in data)
					{
						//if objmat is empty insert inventory data into list 
						//If data is already present check if that data (po item description and material id) is present in the list and add data.
						if (objmat.Count > 0)
						{
							if (mat.receivedtype == "Put Away")
							{
								//If po item description is already present update material and availanle qty and value column
								if (objmat.Any(x => x.poitemdescription == mat.poitemdescription))

								{
									decimal? value = (mat.unitprice) * (mat.availableqty);
									objmat.Where(x => x.poitemdescription == mat.poitemdescription).ToList().ForEach(w =>
									{
										w.availableqty = w.availableqty + mat.availableqty;
										w.value = w.value + value;
										if (w.material.Contains(mat.material))
										{

										}
										else
										{
											w.material = w.material + ',' + mat.material;
										}
									});

								}
								else
								{
									var matdata = "select po.suppliername, matygs.hsncode from wms.wms_polist po left outer join wms.\"MaterialMasterYGS\" matygs on matygs.material ='" + mat.pono;
									matdata += "' where po.pono='" + mat.material + "'";
									var datamat = pgsql.QueryFirstOrDefault<MaterialinHand>(
									 matdata, null, commandType: CommandType.Text);

									if (datamat != null)
									{
										datamat.value = (mat.unitprice) * (mat.availableqty);
										datamat.material = mat.material;
										datamat.poitemdescription = mat.poitemdescription;
										datamat.pono = mat.pono;
										datamat.availableqty = mat.availableqty;
										objmat.Add(datamat);
									}
									else
									{
										objmat.Add(mat);
									}

								}




							}
							else
							{
								//If po item description is already present update material and availanle qty and value column
								if (objmat.Any(x => x.poitemdescription == mat.poitemdescription))

								{

									decimal? value = (mat.unitprice) * (mat.availableqty);
									objmat.Where(x => x.poitemdescription == mat.poitemdescription).ToList().ForEach(w =>
									{
										w.availableqty = w.availableqty + mat.availableqty;
										w.value = w.value + value;
										if (w.material.Contains(mat.material))
										{

										}
										else
										{
											w.material = w.material + ',' + mat.material;
										}
									});



								}
								else
								{
									mat.value = (mat.unitprice) * (mat.availableqty);
									mat.materialdescription = "-";
									mat.hsncode = "-";
									mat.suppliername = "-";
									//mat.projectname = "-";
									objmat.Add(mat);
								}

							}
						}
						else
						{
							//If data is not present in list add data directly into list
							if (mat.receivedtype == "Put Away")
							{
								var matdata = "select po.suppliername, mat.hsncode from wms.wms_polist po left outer join wms.\"MaterialMasterYGS\" matygs on matygs.material ='" + mat.pono;
								matdata += "' where po.pono='" + mat.material + "'";
								var datamat = pgsql.QueryFirstOrDefault<MaterialinHand>(
								 matdata, null, commandType: CommandType.Text);

								datamat.value = (mat.unitprice) * (mat.availableqty);
								datamat.material = mat.material;
								datamat.poitemdescription = mat.poitemdescription;
								datamat.pono = mat.pono;
								datamat.availableqty = mat.availableqty;
								objmat.Add(datamat);

							}
							else
							{
								mat.value = (mat.unitprice) * (mat.availableqty);
								mat.materialdescription = "-";
								mat.hsncode = "-";
								mat.suppliername = "-";
								//mat.projectname = "-";
								objmat.Add(mat);
							}
						}

					}

					return objmat;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getmatinhand", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return objmat;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		/*
		Name of Function : <<get Material in hand location>>  Author :<<Ramesh>>  
		Date of Creation <<17-11-2019>>
		Purpose : <<get Materials in stock locations>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<matlocations>> getmatinhandlocation(string poitemdescription, string materialid, string projectid, string pono, string sono)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string descriptionstr = null;
					if (poitemdescription != null)
					{
						descriptionstr = poitemdescription.Replace("\'", "''");
					}
					string testgetquery = "";

					testgetquery = WMSResource.inhandmatwithoutproject.Replace("#poitemdescription", descriptionstr).Replace("#materialid", materialid);
					if (projectid != null && projectid != "")
					{
						testgetquery += " and stk.projectid = '" + projectid + "'";

					}
					else
					{
						testgetquery += " and (stk.projectid is null or stk.projectid = '')";
					}
					if (pono != null && pono != "")
					{
						testgetquery += " and stk.pono = '" + pono + "'";

					}
					else
					{
						testgetquery += " and (stk.pono is null or stk.pono = '')";
					}
					if (sono != null && sono != "" && sono.ToLower() != "null")
					{
						testgetquery += " and stk.saleorderno = '" + sono + "'";

					}
					else
					{
						testgetquery += " and (stk.saleorderno is null or stk.saleorderno = '')";
					}
					testgetquery += "  group by stk.itemlocation";




					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<matlocations>(
					  testgetquery, null, commandType: CommandType.Text);

					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getmatinhandlocation", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		/*
		Name of Function : <<get initial stock Report>>  Author :<<Ramesh>>  
		Date of Creation <<13-11-2019>>
		Purpose : <<get initial stock uploaded by loggedin user>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<StockModel>> getinitialstockReport(string code)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string testgetquery = WMSResource.initialstockreport.Replace("#code", code);


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<StockModel>(
					  testgetquery, null, commandType: CommandType.Text);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getinitialstockReport", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		/*
		Name of Function : <<get initial stock Report Group by every insertion>>  Author :<<Ramesh>>  
		Date of Creation <<13-11-2019>>
		Purpose : <<get initial stock uploaded by loggedin user>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<StockModel>> getinitialstockReportGroup(string code)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string testgetquery = WMSResource.initialstockreportgroupby;


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<StockModel>(
					  testgetquery, null, commandType: CommandType.Text);
					return data.OrderByDescending(o => o.createddate);



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getinitialstockReport", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		/*
			Name of File : <<name>>  Author :<<Amulya>>  
			Date of Creation <<17-11-2020>>
			Purpose : <<Get Data from wms.st_initialstock table>>
			Review Date :<<>>   Reviewed By :<<  >>
			Sourcecode Copyright : Yokogawa India Limited
		*/
		public async Task<IEnumerable<StockModel>> getinitialstockload(string code)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string testgetquery = WMSResource.initialstockloadgroupby.Replace("#code", code);


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<StockModel>(
					  testgetquery, null, commandType: CommandType.Text);
					return data.OrderByDescending(o => o.createddate);



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getinitialstockReport", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		/*
		Name of Function : <<posttestcrud>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<post test crud>>
		<param name="data"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "notifyputaway", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return "error";
			}

			return result;
		}

		/*
		Name of Function : <<deleteuserAuth>>  Author :<<Ramesh>>  
		Date of Creation <<27-01-2021>>
		Purpose : <<User role setting>>
		<param name="data"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/


		public string deleteuserAuth(authUser data)
		{
			string result = "";
			try
			{
				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					data.modifiedon = DateTime.Now;
					string updatequery = WMSResource.deleteauthuser.Replace("#empid", data.employeeid);
					var resultsx = DB.ExecuteScalar(updatequery, new
					{
						data.modifiedon,
						data.modifiedby
					});
					result = "deleted";
				}


			}
			catch (Exception ex)
			{
				result = ex.Message;
			}


			return result;

		}


		/*
		Name of Function : <<deletedeligatePM>>  Author :<<Ramesh>>  
		Date of Creation <<27-01-2021>>
		Purpose : <<User role setting>>
		<param name="data"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string deletedeligatePM(authUser data)
		{
			string result = "";
			try
			{
				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					data.modifiedon = DateTime.Now;
					string updatequery = "delete from wms.auth_users where authid = " + data.authid + "";
					var resultsx = DB.ExecuteScalar(updatequery);
					result = "deleted";
				}


			}
			catch (Exception ex)
			{
				result = ex.Message;
			}


			return result;

		}

		/*
		Name of Function : <<updateuserAuth>>  Author :<<Ramesh>>  
		Date of Creation <<27-01-2021>>
		Purpose : <<User role setting>>
		<param name="data"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string updateuserAuth(List<authUser> data)
		{
			string result = "";
			int rslt = 0;
			try
			{



				foreach (authUser model in data)
				{

					model.createddate = System.DateTime.Now;
					model.modifiedon = System.DateTime.Now;

					string subroleid = null;
					if (model.selectedsubrolelist != null && model.selectedsubrolelist.Count() > 0)
					{
						subroleid = "";
						int i = 0;
						foreach (subrolemodel mdl in model.selectedsubrolelist)
						{
							if (i > 0)
							{
								subroleid += ",";
							}
							subroleid += mdl.subroleid.ToString();
							i++;
						}
					}
					else if (model.subroleid != null && model.subroleid != "")
					{
						subroleid = model.subroleid;
					}


					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{

						if (model.authid == 0)
						{
							string insertquery = WMSResource.insertAuthUser;
							model.createdby = model.createdby;
							var results = DB.ExecuteScalar(insertquery, new
							{
								model.employeeid,
								model.roleid,
								model.createddate,
								model.createdby,
								model.deleteflag,
								model.emailnotification,
								model.emailccnotification,
								subroleid,
								model.plantid,
								model.isdelegatemember
							});

						}
						else
						{
							string updatequery = WMSResource.updateauthuser.Replace("#aid", model.authid.ToString());
							var resultsx = DB.ExecuteScalar(updatequery, new
							{

								model.deleteflag,
								model.emailnotification,
								model.emailccnotification,
								subroleid,
								model.plantid,
								model.isdelegatemember,
								model.modifiedon,
								model.modifiedby
							});

						}

					}
				}


				result = "saved";
			}
			catch (Exception ex)
			{
				log.ErrorMessage("PODataProvider", "updateuserAuth", ex.StackTrace.ToString(), ex.Message.ToString(), url);
				return "error";
			}


			return result;
		}

		/*
		Name of Function : <<deletetestcurd>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<delete test curd>>
		<param name="id"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
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

			catch (Exception Ex)
			{

				Console.WriteLine(Ex.Message);
				return "error";

			}


		}

		/*
		Name of Function : <<updateSecurityPrintHistory>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<update Security Print History>>
		<param name="model"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string updateSecurityPrintHistory(PrintHistoryModel model)
		{


			var data = 0;
			model.reprintedon = DateTime.Now;
			string insertquery = WMSResource.insertSecurityPrintHistory;
			using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
			{
				//Get inwmasterid, print status from security inward table
				//string secquery = "select inwmasterid,print  from wms.wms_securityinward where pono ='" + model.pono + "' and invoiceno ='" + model.invoiceNo + "'";
				string secquery = "select inwmasterid,print  from wms.wms_securityinward where inwmasterid='" + model.inwmasterid + "'";
				var securityData = DB.QueryFirstOrDefault<inwardModel>(
						   secquery, null, commandType: CommandType.Text);

				//Check whether the reprint data for this PO and Invoice already exists in barcode table
				string barcodequery = "select barcodeid from wms.wms_barcode where  barcode ='" + model.inwmasterid + "'";
				var barcodeData = DB.QueryFirstOrDefault<BarcodeModel>(
						   barcodequery, null, commandType: CommandType.Text);

				model.reprintcount = 1;
				model.inwmasterid = securityData.inwmasterid;
				model.barcodeid = barcodeData.barcodeid;

				//If print status is true in security inward table update data in reprint history table else update print status in security inward table
				if (securityData.print == true)
				{
					//updating data in reprint history table

					//Check if the data is already reprinted 
					string query = WMSResource.getbarcodereprintdata.Replace("#barcode", Convert.ToString(model.inwmasterid));
					var barcodereprintData = DB.QueryFirstOrDefault<BarcodeModel>(
						   query, null, commandType: CommandType.Text);
					//if(barcodereprintData!=null)
					// {
					//	string updatequery=WMSResource.updatereprintdata.Replace("#reprintedby", Convert.ToString(model.reprintedby)).Replace("#reprintcount", Convert.ToString(barcodereprintData.reprintcount+1)).Replace("#barcodeid", Convert.ToString(barcodereprintData.barcodeid));
					//	data = DB.Execute(updatequery, new
					//	{

					//		model.reprintedby,

					//	});

					//}
					//else
					//   {
					int noofprint = 1;

					data = Convert.ToInt32(DB.ExecuteScalar(insertquery, new
					{
						model.inwmasterid,
						model.reprintedon,
						model.reprintedby,
						model.reprintcount,
						model.barcodeid,
						noofprint

					}));
				}

				//}
				else
				{
					securityData.print = true;
					securityData.printedby = model.reprintedby;
					securityData.printedon = model.reprintedon;
					string printedby = model.reprintedby;
					string updateqry = "update wms.wms_securityinward set print =" + securityData.print + ",  printedon = current_date, printedby = '" + model.reprintedby + "' where inwmasterid = '" + model.inwmasterid + "'";
					//WMSResource.updateSecurityinwardprint.Replace("#print", Convert.ToString(securityData.print)).Replace("#printedby", Convert.ToString(printedby)).Replace("#pono", Convert.ToString(model.pono)).Replace("#invno", Convert.ToString(model.invoiceNo));
					data = DB.Execute(updateqry, new
					{
						securityData.print,
						printedby
					});


				}

			}
			if (data != 0)
				return "Sucess";
			else
				return "Error";


		}

		/*
		Name of Function : <<updateQRcodePrintHistory>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<update QRcode Print History>>
		<param name="printMat"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string updateQRcodePrintHistory(printMaterial printMat)
		{

			try
			{
				var data = 0;
				DateTime createddate = DateTime.Now;
				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					//Once printing is successfull add data to barcode table
					string barcode = printMat.grnno + "-" + printMat.materialid;

					string barcodeqry = "select barcodeid from wms.wms_barcode where  barcode ='" + barcode + "' and pono='" + printMat.pono + "' and invoiceno='" + printMat.invoiceno + "'";
					var bar = DB.QueryFirstOrDefault<BarcodeModel>(
							   barcodeqry, null, commandType: CommandType.Text);
					if (bar == null)
					{

						//insert bar code data

						bool deleteflag = false;
						string createdby = printMat.printedby;
						string insertbarcodequery = WMSResource.insertbarcodedata;//to insert bar code data
						var barcodeResult = DB.Execute(insertbarcodequery, new
						{
							barcode,
							createdby,
							createddate,
							deleteflag,
							printMat.pono,
							printMat.invoiceno,
							printMat.inwmasterid
						});
						//bar.barcodeid = barcodeResult;
						barcodeqry = "select barcodeid from wms.wms_barcode where  barcode ='" + barcode + "' and pono='" + printMat.pono + "' and invoiceno='" + printMat.invoiceno + "'";
						bar = DB.QueryFirstOrDefault<BarcodeModel>(
								  barcodeqry, null, commandType: CommandType.Text);

					}


					string queryx = WMSResource.isgrnexistsquerybyinvoce.Replace("#pono", printMat.pono).Replace("#invno", printMat.invoiceno);
					var objx = DB.QuerySingle<inwardModel>(
					 queryx, null, commandType: CommandType.Text);
					if (objx.inwmasterid != null && objx.inwmasterid != "")
					{
						//check if print is true
						string barcodequery = "select isprint from wms.wms_printstatusmaterial where  inwmasterid ='" + objx.inwmasterid + "' and materialid='" + printMat.materialid + "'";
						var barcodeData = DB.QueryFirstOrDefault<printMaterial>(
								   barcodequery, null, commandType: CommandType.Text);
						if (barcodeData != null)
						{
							if (barcodeData.isprint == true)
							{
								string gatepassid = null;
								//Add data in reprint history table
								string insertquery = WMSResource.insertReprintHistory;
								int reprintcount = 0;
								string reprintedby = printMat.printedby;
								data = Convert.ToInt32(DB.ExecuteScalar(insertquery, new
								{

									gatepassid,
									reprintedby,
									reprintcount,
									bar.barcodeid,
									printMat.noofprint,
									objx.inwmasterid,
									printMat.inwardid,
									printMat.printerid
								}));

								string query = WMSResource.checkreprintalreadydone;
								if (printMat.inwardid != null)
								{
									query = query + " inwardid= '" + printMat.inwardid + "' order by reprintcount desc limit 1";
								}
								var res = DB.QuerySingle<reprintModel>(
								  query, null, commandType: CommandType.Text);

								string updatequery = WMSResource.updatereprintcount.Replace("#reprinthistoryid", Convert.ToString(data));
								reprintcount = res.reprintcount + 1;
								using (IDbConnection pgsql = new NpgsqlConnection(config.PostgresConnectionString))
								{
									var data1 = DB.Execute(updatequery, new
									{
										reprintcount
									});
								}

							}
							else
							{
								//Update data in print status material table -insertprintmaterial
								string insertquery = WMSResource.insertprintmaterial;
								int printcount = 1;
								bool isprint = true;
								string reprintedby = printMat.printedby;
								data = Convert.ToInt32(DB.ExecuteScalar(insertquery, new
								{
									objx.inwmasterid,
									createddate,
									printMat.printedby,
									printcount,
									printMat.noofprint,
									bar.barcodeid,
									isprint,
									printMat.materialid

								}));

							}

						}

						else
						{
							//Update data in print status material table -insertprintmaterial
							string insertquery = WMSResource.insertprintmaterial;
							int printcount = 1;
							bool isprint = true;
							string reprintedby = printMat.printedby;
							data = Convert.ToInt32(DB.ExecuteScalar(insertquery, new
							{
								objx.inwmasterid,
								createddate,
								printMat.printedby,
								printcount,
								printMat.noofprint,
								bar.barcodeid,
								isprint,
								printMat.materialid

							}));
						}







					}

					return "success";
				}
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "gettransferdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return null;
			}

			return "success";
		}

		/*
		Name of Function : <<getMaterialtransferdetails>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get Material transfer details>>
		<param name="filterparams"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<materialtransferMain>> getMaterialtransferdetails(materilaTrasFilterParams filterparams)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string query = WMSResource.getMaterialTransferDetails;
					if (!string.IsNullOrEmpty(filterparams.ToDate))
						query += " where ts.createdon::date <= '" + filterparams.ToDate + "'";
					if (!string.IsNullOrEmpty(filterparams.FromDate))
						query += "  and ts.createdon::date >= '" + filterparams.FromDate + "'";
					query += " order by ts.transferid desc";
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
				catch (Exception Ex)
				{

					log.ErrorMessage("PODataProvider", "gettransferdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}

		/*
		Name of Function : <<getmaterialrequestdashboardList>>  Author :<<Amulya>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get material request dashboardList>>
		<param name="filterparams"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public async Task<IEnumerable<MaterialTransaction>> getmaterialrequestdashboardList(materialRequestFilterParams filterparams)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string query = WMSResource.getMaterialRequestDashboardDetails;
					if (!string.IsNullOrEmpty(filterparams.ToDate))
						query += " where req.requesteddate::date <= '" + filterparams.ToDate + "'";
					if (!string.IsNullOrEmpty(filterparams.FromDate))
						query += "  and req.requesteddate::date >= '" + filterparams.FromDate + "'";
					query += " order by req.requestid desc";
					var data = await pgsql.QueryAsync<MaterialTransaction>(
					   query, null, commandType: CommandType.Text);

					if (data != null && data.Count() > 0)
					{
						foreach (MaterialTransaction trans in data)
						{
							try
							{
								trans.materialdata = new List<MaterialTransactionDetail>();
								string materialrequestdataquery = WMSResource.getmaterialrequestdata.Replace("#requestid", trans.requestid);
								var data1 = await pgsql.QueryAsync<MaterialTransactionDetail>(
								materialrequestdataquery, null, commandType: CommandType.Text);
								trans.materialdata = data1.ToList();

							}
							catch (Exception Ex)
							{
								return null;
							}
						}
					}
					return data;

				}
				catch (Exception Ex)
				{

					log.ErrorMessage("PODataProvider", "gettransferdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}

		/*
		In Use
		Name of Function : <<getmaterialreservedashboardList>>  Author :<<Amulya>>  
		Date of Creation <<12-12-2020>>
		Purpose : <<get material reserve dashboard List>>
		<param name="filterparams"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<materialreserveMain>> getmaterialreservedashboardList(materialResFilterParams filterparams)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string query = WMSResource.getMaterialReserveDashboardDetails;
					if (!string.IsNullOrEmpty(filterparams.ToDate))
						query += " where mrs.reservedon::date <= '" + filterparams.ToDate + "'";
					if (!string.IsNullOrEmpty(filterparams.FromDate))
						query += "  and mrs.reservedon::date >= '" + filterparams.FromDate + "'";
					//query += " order by mrs.requestedby asc";
					var data = await pgsql.QueryAsync<materialreserveMain>(
					   query, null, commandType: CommandType.Text);

					if (data != null && data.Count() > 0)
					{
						foreach (materialreserveMain dt in data)
						{
							string query1 = WMSResource.getreserveiddetail.Replace("#rsid", dt.reserveid.ToString());
							var datadetail = await pgsql.QueryAsync<materialreserveMS>(
							   query1, null, commandType: CommandType.Text);

							if (datadetail != null && datadetail.Count() > 0)
							{
								dt.materialdata = datadetail.ToList();
							}
						}
					}
					return data.OrderByDescending(o => o.reserveid);

				}
				catch (Exception Ex)
				{

					log.ErrorMessage("PODataProvider", "gettransferdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}

		/*
     Name of Function : <<getmateriallabeldetail>>  Author :<<Ramesh>>  
     Date of Creation <<20-11-2020>>
     Purpose : <<get material label detail>>
     Review Date :<<>>   Reviewed By :<<>>
     */

		public async Task<MateriallabelModel> getmateriallabeldetail(string pono, int lineitemno, string materialid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string lblquery = "Select *  from wms.wms_pomaterials where  pono = '" + pono + "' and materialid = '" + materialid + "' and itemno =" + lineitemno;


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<MateriallabelModel>(
					  lblquery, null, commandType: CommandType.Text);
					return data.FirstOrDefault();

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getmateriallabeldetail", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}


		/*
		Name of Function : <<getmaterialreturndashboardlist>>  Author :<<Amulya>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get material return dashboardlist>>
		<param name="filterparams"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public async Task<IEnumerable<materialreturnMain>> getmaterialreturndashboardlist(materialRetFilterParams filterparams)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string query = WMSResource.getMaterialReturnDashboardDetails;
					if (!string.IsNullOrEmpty(filterparams.ToDate))
						query += " where mrt.createdon::date <= '" + filterparams.ToDate + "'";
					if (!string.IsNullOrEmpty(filterparams.FromDate))
						query += "  and mrt.createdon::date >= '" + filterparams.FromDate + "'";

					var data = await pgsql.QueryAsync<materialreturnMain>(
					   query, null, commandType: CommandType.Text);

					if (data != null && data.Count() > 0)
					{
						foreach (materialreturnMain dt in data)
						{
							string query1 = WMSResource.getreturniddetail.Replace("#rid", dt.returnid.ToString());
							var datadetail = await pgsql.QueryAsync<materialreturnMT>(
							   query1, null, commandType: CommandType.Text);

							if (datadetail != null && datadetail.Count() > 0)
							{
								dt.materialdata = datadetail.ToList();
							}
						}
					}
					return data.OrderByDescending(o => o.returnid);

				}
				catch (Exception Ex)
				{

					log.ErrorMessage("PODataProvider", "gettransferdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}


		/*
			Name of File : <<name>>  Author :<<Amulya>>  
			Date of Creation <<18-11-2020>>
			Purpose : <<Get Data of GR Reports>>
			Review Date :<<>>   Reviewed By :<<  >>
			Sourcecode Copyright : Yokogawa India Limited
		*/

		public async Task<IEnumerable<grReports>> getGRListdata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string grreportquery = WMSResource.GetGRReportDataList;

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<grReports>(
					  grreportquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetGRReport", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}


		public async Task<IEnumerable<grReports>> addEditReports(string wmsgr)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string editquery = WMSResource.editGRReports.Replace("#wmsgr", wmsgr);

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<grReports>(
					  editquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "editGRReport", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}


		public string EditReports(grReports data)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string editquery = WMSResource.addSAPGR.Replace("#sapgr", data.sapgr).Replace("#wmsgr", data.wmsgr);

					pgsql.Open();
					pgsql.Execute(editquery);
					return "updated";

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "editquery", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		//public string addEditGRReports(grReports data)
		//{
		//    throw new NotImplementedException();
		//}

		public async Task<pmDashboardCards> getPMdashboarddata(DashBoardFilters filters)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{


				try
				{

					//Get count of total material requested
					string totalqry = " select count(*) as totalmaterialrequest from  wms.materialrequest where requesteddate>='" + filters.fromDate + "' and requesteddate<= '" + filters.toDate + "' and requestid is not null ";

					//Get count of issued material requested
					string issuedqry = "select count(*) as issuedmaterialrequest from  wms.materialrequest where requesteddate>='" + filters.fromDate + "' and requesteddate<= '" + filters.toDate + "' and issuedon >= '" + filters.fromDate + "' and issuedon<= '" + filters.toDate + "' and requestid is not null ";

					//Get count of pending material requested
					string pendingqry = "select  count(*) as pendingmaterialrequest from  wms.materialrequest  where requesteddate >='" + filters.fromDate + "' and requesteddate<= '" + filters.toDate + "' and issuedon is null and requestid is not null ";

					//Get count of total material return
					string materialtotalreturndqry = "select count(*) as totalmaterialreturn from  wms.wms_materialreturn where createdon >='" + filters.fromDate + "' and createdon<= '" + filters.toDate + "' and returnid is not null";

					//Get count of approved material return
					string materialappreturndqry = "select count(*) as approvedmaterialreturn from  wms.wms_materialreturn where createdon >='" + filters.fromDate + "' and createdon<= '" + filters.toDate + "' and returnid is not null and confirmstatus ='Accepted'";

					//Get count of pending material return 
					string materialpendreturndqry = " select count(*) as pendingmaterialreturn from  wms.wms_materialreturn where createdon >='" + filters.fromDate + "' and createdon<= '" + filters.toDate + "' and returnid is not null and confirmstatus is null ";

					//Get count of total material reserved 
					string materialreservededqry = "select count(*) as totalmaterialreserved from  wms.materialreserve where reservedon >='" + filters.fromDate + "' and reservedon<= '" + filters.toDate + "' and reserveid is not null";

					//Get count of total material returned
					//string materialreturnedqry = "select count(*) as totalmaterialreturned from   wms.wms_materialreturn where createdon >='" + filters.fromDate + "' and createdon<= '" + filters.toDate + "' and returnid is not null and confirmstatus ='Accepted'";
					string materialtotaltransferqry = "select count(*) as totalmaterialtransfer from   wms.wms_transfermaterial where createdon >='" + filters.fromDate + "' and createdon<= '" + filters.toDate + "'";

					//Get count of approved material transfer 
					string materialtransferapprovedqry = "select count(*) as approvedmaterialtransfer from wms.wms_materialtransferapproval tra join wms.wms_transfermaterial tr on tr.transferid = tra.transferid where tr.createdon >= '" + filters.fromDate + "' and tr.createdon <= '" + filters.toDate + "' and tra.approvaldate >= '" + filters.fromDate + "' and tra.approvaldate<= '" + filters.toDate + "' and tra.approverid is not null and tra.approvallevel =2  ";


					var data1 = await pgsql.QueryAsync<pmDashboardCards>(totalqry, null, commandType: CommandType.Text);
					var data2 = await pgsql.QueryAsync<pmDashboardCards>(issuedqry, null, commandType: CommandType.Text);
					var data3 = await pgsql.QueryAsync<pmDashboardCards>(pendingqry, null, commandType: CommandType.Text);
					var data4 = await pgsql.QueryAsync<pmDashboardCards>(materialtotalreturndqry, null, commandType: CommandType.Text);
					var data5 = await pgsql.QueryAsync<pmDashboardCards>(materialappreturndqry, null, commandType: CommandType.Text);
					var data6 = await pgsql.QueryAsync<pmDashboardCards>(materialpendreturndqry, null, commandType: CommandType.Text);
					var data7 = await pgsql.QueryAsync<pmDashboardCards>(materialreservededqry, null, commandType: CommandType.Text);
					var data8 = await pgsql.QueryAsync<pmDashboardCards>(materialtotaltransferqry, null, commandType: CommandType.Text);
					var data9 = await pgsql.QueryAsync<pmDashboardCards>(materialtransferapprovedqry, null, commandType: CommandType.Text);

					var data = new pmDashboardCards();
					data.totalmaterialrequest = data1.Count() > 0 ? data1.FirstOrDefault().totalmaterialrequest : 0;
					data.issuedmaterialrequest = data2.Count() > 0 ? data2.FirstOrDefault().issuedmaterialrequest : 0;
					data.pendingmaterialrequest = data3.Count() > 0 ? data3.FirstOrDefault().pendingmaterialrequest : 0;
					if (data4.Count() > 0)
					{
						data.totalmaterialreturn = data4.FirstOrDefault().totalmaterialreturn;
					}

					data.approvedmaterialreturn = data5.Count() > 0 ? data5.FirstOrDefault().approvedmaterialreturn : 0;
					data.pendingmaterialreturn = data6.Count() > 0 ? data6.FirstOrDefault().pendingmaterialreturn : 0;

					//if (data7.Count() > 0)
					//{
					//    data.pendingmaterialreturn = data7.FirstOrDefault().pendingmaterialreturn;
					//}

					data.totalmaterialreserved = data7.Count() > 0 ? data7.FirstOrDefault().totalmaterialreserved : 0;
					data.totalmaterialtransfer = data8.Count() > 0 ? data8.FirstOrDefault().totalmaterialtransfer : 0;
					data.approvedmaterialtransfer = data9.Count() > 0 ? data9.FirstOrDefault().approvedmaterialtransfer : 0;


					return data;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getPMdashboarddata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}


		public async Task<IEnumerable<UserDashboardGraphModel>> getUserdashboardgraphPMdata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{


				try
				{

					string rcvqry = "SELECT requesteddate::date as graphdate, COUNT(*),'Request' as type ";
					rcvqry += " from wms.materialrequest";
					rcvqry += " WHERE requesteddate > now() - interval '7 days'";
					rcvqry += " GROUP BY requesteddate::date";
					rcvqry += " ORDER BY requesteddate::date ASC";

					string qcqry = "SELECT createdon::date as graphdate, COUNT(*),'Return' as type ";
					qcqry += " from wms.wms_materialreturn";
					qcqry += " WHERE createdon > now() - interval '7 days'";
					qcqry += " GROUP BY createdon::date";
					qcqry += " ORDER BY createdon::date ASC";

					string accqry = "SELECT reservedon::date as graphdate, COUNT(*),'Reserve' as type ";
					accqry += " from  wms.materialreserve";
					accqry += " WHERE reservedon > now() - interval '7 days'";
					accqry += " GROUP BY reservedon::date";
					accqry += " ORDER BY reservedon::date ASC";

					string pwqry = "SELECT createddate::date as graphdate, COUNT(*),'Returned' as type ";
					pwqry += " from wms.wms_materialreturn";
					pwqry += " WHERE createdon > now() - interval '7 days'";
					pwqry += " GROUP BY createdon::date";
					pwqry += " ORDER BY createdon::date ASC";


					var data1 = await pgsql.QueryAsync<UserDashboardGraphModel>(rcvqry, null, commandType: CommandType.Text);
					var data2 = await pgsql.QueryAsync<UserDashboardGraphModel>(qcqry, null, commandType: CommandType.Text);
					var data3 = await pgsql.QueryAsync<UserDashboardGraphModel>(accqry, null, commandType: CommandType.Text);
					var data4 = await pgsql.QueryAsync<UserDashboardGraphModel>(pwqry, null, commandType: CommandType.Text);

					var data = data1.Concat(data2.Concat(data3.Concat(data4)));

					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getUserdashboardgraphPMdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public async Task<IEnumerable<UserDashboardGraphModel>> getmonthlyUserdashboardIEgraphdata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{


				try
				{


					string mrvqry = "SELECT date_part('month', requesteddate::date) as smonth,";
					mrvqry += " concat('week',date_part('week', requesteddate::date) ) AS sweek,COUNT(*) as count, 'Request' as type";
					mrvqry += " FROM  wms.materialrequest where requesteddate is not null and requesteddate > now() - interval '1 month'  and requestid is not null ";
					mrvqry += " GROUP BY smonth, sweek ORDER BY smonth , sweek";


					string mrnvqry = "SELECT date_part('month', createdon::date) as smonth,";
					mrnvqry += " concat('week',date_part('week', createdon::date) ) AS sweek,COUNT(*) as count, 'Return' as type";
					mrnvqry += " FROM  wms.wms_materialreturn where createdon is not null and createdon > now() - interval '1 month'  and returnid is not null";
					mrnvqry += " GROUP BY smonth, sweek ORDER BY smonth , sweek";


					string mrsvqry = "SELECT date_part('month', reservedon::date) as smonth,";
					mrsvqry += " concat('week',date_part('week', reservedon::date) ) AS sweek,COUNT(*) as count, 'Reserve' as type";
					mrsvqry += " FROM  wms.materialreserve where reservedon is not null and reservedon > now() - interval '1 month'  and reserveid is not null";
					mrsvqry += " GROUP BY smonth, sweek ORDER BY smonth , sweek";

					string mrtvqry = "SELECT date_part('month', createdon::date) as smonth,";
					mrtvqry += " concat('week',date_part('week', createdon::date) ) AS sweek,COUNT(*) as count, 'Transfer' as type";
					mrtvqry += " FROM  wms.wms_transfermaterial where createdon is not null and createdon > now() - interval '1 month'  and materialid is not null";
					mrtvqry += " GROUP BY smonth, sweek ORDER BY smonth , sweek";
					var data1 = await pgsql.QueryAsync<UserDashboardGraphModel>(mrvqry, null, commandType: CommandType.Text);

					var data2 = await pgsql.QueryAsync<UserDashboardGraphModel>(mrnvqry, null, commandType: CommandType.Text);
					var data3 = await pgsql.QueryAsync<UserDashboardGraphModel>(mrsvqry, null, commandType: CommandType.Text);
					var data4 = await pgsql.QueryAsync<UserDashboardGraphModel>(mrtvqry, null, commandType: CommandType.Text);

					var data = data1.Concat(data2.Concat(data3.Concat(data4)));
					//var data = data1;
					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getmonthlyUserdashboardIEgraphdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		//public string addEditGRReports(grReports data)
		//{
		//    throw new NotImplementedException();
		//}

		public async Task<IEnumerable<UserDashboardGraphModel>> getUserdashIEgraphdata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{


				try
				{

					string mrqvqry = "SELECT requesteddate::date as graphdate, COUNT(*),'Request' as type";
					mrqvqry += " from wms.materialrequest";
					mrqvqry += " WHERE requesteddate > now() - interval '7 days'";
					mrqvqry += " GROUP BY requesteddate::date";
					mrqvqry += " ORDER BY requesteddate::date ASC";

					string mrnqry = "SELECT createdon::date as graphdate, COUNT(*),'Return' as type ";
					mrnqry += " from wms.wms_materialreturn";
					mrnqry += " WHERE createdon > now() - interval '7 days'";
					mrnqry += " GROUP BY createdon::date";
					mrnqry += " ORDER BY createdon::date ASC";

					string mrsqry = "SELECT reservedon::date as graphdate, COUNT(*),'Reserve' as type ";
					mrsqry += " from  wms.materialreserve";
					mrsqry += " WHERE reservedon > now() - interval '7 days'";
					mrsqry += " GROUP BY reservedon::date";
					mrsqry += " ORDER BY reservedon::date ASC";

					string mrtqry = "SELECT createdon::date as graphdate, COUNT(*),'Transfer' as type ";
					mrtqry += " from wms.wms_transfermaterial";
					mrtqry += " WHERE createdon > now() - interval '7 days'";
					mrtqry += " GROUP BY createdon::date";
					mrtqry += " ORDER BY createdon::date ASC";

					var data1 = await pgsql.QueryAsync<UserDashboardGraphModel>(mrqvqry, null, commandType: CommandType.Text);
					var data2 = await pgsql.QueryAsync<UserDashboardGraphModel>(mrnqry, null, commandType: CommandType.Text);
					var data3 = await pgsql.QueryAsync<UserDashboardGraphModel>(mrsqry, null, commandType: CommandType.Text);
					var data4 = await pgsql.QueryAsync<UserDashboardGraphModel>(mrtqry, null, commandType: CommandType.Text);

					var data = data1.Concat(data2.Concat(data3.Concat(data4)));

					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getUserdashboardIEgraphdata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public async Task<invDashboardCards> getInvdashboarddata(DashBoardFilters filters)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{


				try
				{


					//Get count of total material requested
					string totalqry = " select count(*) as totalmaterialrequests from  wms.materialrequest where requesteddate>='" + filters.fromDate + "' and requesteddate<= '" + filters.toDate + "' and requestid is not null ";

					//Get count of issued material requested
					string issuedqry = "select count(*) as issuedmaterialrequests from  wms.materialrequest where requesteddate>='" + filters.fromDate + "' and requesteddate<= '" + filters.toDate + "' and issuedon >='" + filters.fromDate + "' and issuedon<= '" + filters.toDate + "' and requestid is not null ";

					//string issuedqry = "select count(*) as issuedmaterialrequests from  wms.materialrequest where issuedon >='" + filters.fromDate + "' and issuedon<= '" + filters.toDate + "' and requestid is not null ";

					//Get count of pending material requested
					string pendingqry = "select  count(*) as pendingmaterialrequests from  wms.materialrequest  where requesteddate >='" + filters.fromDate + "' and requesteddate<= '" + filters.toDate + "' and issuedon is null and requestid is not null ";

					//string pendingqry = "select  count(*) OVER () as pendingmaterialrequests from  wms.materialrequest req join wms.materialreserve res on req.reserveid =res.reserveid is not null where reservedon >='" + filters.fromDate + "' and reservedon<= '" + filters.toDate + "' and requestid is not null ";

					//Get count of total material reserved 
					string materialreservededqry = "select count(*) as totalmaterialreserved from  wms.materialreserve where reservedon >='" + filters.fromDate + "' and reservedon<= '" + filters.toDate + "' and reserveid is not null";

					//Get count of total material return
					string materialtotalreturndqry = "select count(*) as totalmaterialreturn from  wms.wms_materialreturn where createdon >='" + filters.fromDate + "' and createdon<= '" + filters.toDate + "' and returnid is not null";

					//Get count of approved material return
					string materialappreturndqry = "select count(*) as approvedmaterialreturn from  wms.wms_materialreturn where createdon >='" + filters.fromDate + "' and createdon<= '" + filters.toDate + "' and returnid is not null and confirmstatus ='Accepted'";

					//Get count of pending material return 
					string materialpendreturndqry = " select count(*) as pendingmaterialreturn from  wms.wms_materialreturn where createdon >='" + filters.fromDate + "' and createdon<= '" + filters.toDate + "' and returnid is not null and confirmstatus is null ";


					//Get count of total material transfer 
					string materialtotaltransferqry = "select count(*) as totalmaterialtransfer from   wms.wms_transfermaterial where createdon >='" + filters.fromDate + "' and createdon<= '" + filters.toDate + "'";

					//Get count of approved material transfer 
					string materialtransferapprovedqry = "select count(*) as approvedmaterialtransfer from wms.wms_materialtransferapproval tra join wms.wms_transfermaterial tr on tr.transferid = tra.transferid where tr.createdon >= '" + filters.fromDate + "' and tr.createdon <= '" + filters.toDate + "' and tra.approvaldate >= '" + filters.fromDate + "' and tra.approvaldate<= '" + filters.toDate + "' and tra.approverid is not null  and tra.approvallevel =2";

					var data1 = await pgsql.QueryAsync<invDashboardCards>(totalqry, null, commandType: CommandType.Text);
					var data2 = await pgsql.QueryAsync<invDashboardCards>(issuedqry, null, commandType: CommandType.Text);
					var data3 = await pgsql.QueryAsync<invDashboardCards>(pendingqry, null, commandType: CommandType.Text);
					var data4 = await pgsql.QueryAsync<invDashboardCards>(materialreservededqry, null, commandType: CommandType.Text);
					var data5 = await pgsql.QueryAsync<invDashboardCards>(materialtotalreturndqry, null, commandType: CommandType.Text);
					var data6 = await pgsql.QueryAsync<invDashboardCards>(materialappreturndqry, null, commandType: CommandType.Text);
					var data7 = await pgsql.QueryAsync<invDashboardCards>(materialpendreturndqry, null, commandType: CommandType.Text);
					var data8 = await pgsql.QueryAsync<invDashboardCards>(materialtotaltransferqry, null, commandType: CommandType.Text);
					var data9 = await pgsql.QueryAsync<invDashboardCards>(materialtransferapprovedqry, null, commandType: CommandType.Text);

					var data = new invDashboardCards();
					data.totalmaterialrequests = data1.Count() > 0 ? data1.FirstOrDefault().totalmaterialrequests : 0;
					data.issuedmaterialrequests = data2.Count() > 0 ? data2.FirstOrDefault().issuedmaterialrequests : 0;
					data.pendingmaterialrequests = data3.Count() > 0 ? data3.FirstOrDefault().pendingmaterialrequests : 0;
					if (data4.Count() > 0)
					{
						data.totalmaterialreserved = data4.FirstOrDefault().totalmaterialreserved;
					}

					data.totalmaterialreturn = data5.Count() > 0 ? data5.FirstOrDefault().totalmaterialreturn : 0;
					data.approvedmaterialreturn = data6.Count() > 0 ? data6.FirstOrDefault().approvedmaterialreturn : 0;
					data.pendingmaterialreturn = data7.Count() > 0 ? data7.FirstOrDefault().pendingmaterialreturn : 0;

					data.totalmaterialtransfer = data8.Count() > 0 ? data8.FirstOrDefault().totalmaterialtransfer : 0;
					data.approvedmaterialtransfer = data9.Count() > 0 ? data9.FirstOrDefault().approvedmaterialtransfer : 0;



					return data;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getInvdashboarddata", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}


		/* Name of Function : <<get initial stock>>  Author :<<prasanna>>  
		 Date of Creation <<21-12-2019>>
		 Purpose : <<get  initial stock for MiscellanousIssues list>>
		 <param name="datamodel"></param>
		 Review Date :<<>>   Reviewed By :<<>>
		 */

		public async Task<IEnumerable<StockModel>> getMiscellanousIssueList(bool initialStock)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					//string query = WMSResource.getMiscellanousIssuesList;
					//if (initialStock)
					//	query += " where st.initialstock=true group by st.poitemdescription, st.itemlocation";
					//else
					//	query += " group by st.poitemdescription,st.itemlocation";
					string query1 = " select false as isselected,projectid,materialid as material,pono,poitemdescription,itemlocation,sum(availableqty) as availableqty,sum(availableqty::decimal(19,5) * unitprice::decimal(19,5) ) as value from wms.wms_stock ws where availableqty > 0";
					query1 += " and projectid is not null and projectid != ''";
					if (initialStock)
						query1 += " and initialstock=true group by projectid,materialid,pono,poitemdescription,itemlocation";
					else
						query1 += " group by projectid,materialid,pono,poitemdescription,itemlocation";
					await pgsql.OpenAsync();
					var result = await pgsql.QueryAsync<StockModel>(
					  query1, null, commandType: CommandType.Text);
					return result;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getMiscellanousIssueList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}


		/* Name of Function : <<get initial stock>>  Author :<<prasanna>>  
		 Date of Creation <<21-12-2019>>
		 Purpose : <<get  initial stock for MiscellanousIssues list>>
		 <param name="datamodel"></param>
		 Review Date :<<>>   Reviewed By :<<>>
		 */

		public async Task<IEnumerable<StockModel>> getMiscellanousIssueListdata(string initialStock, string pono, string projectid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					//string query = WMSResource.getMiscellanousIssuesList;
					//if (initialStock)
					//	query += " where st.initialstock=true group by st.poitemdescription, st.itemlocation";
					//else
					//	query += " group by st.poitemdescription,st.itemlocation";
					string query1 = " select false as isselected,projectid,materialid as material,pono,poitemdescription,itemlocation,sum(availableqty) as availableqty,sum(availableqty::decimal(19,5) * unitprice::decimal(19,5) ) as value from wms.wms_stock ws where availableqty > 0";
					query1 += " and projectid = '" + projectid + "' and pono = '" + pono + "'";
					if (initialStock == "True")
						query1 += " and initialstock=true group by projectid,materialid,pono,poitemdescription,itemlocation";
					else
						query1 += " group by projectid,materialid,pono,poitemdescription,itemlocation";
					await pgsql.OpenAsync();
					var result = await pgsql.QueryAsync<StockModel>(
					  query1, null, commandType: CommandType.Text);
					return result;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getMiscellanousIssueList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/* Name of Function : <<getMiscellanousIssueListdatanofilter>>  Author :<<Ramesh>>  
		 Date of Creation <<30-04-2021>>
		 Purpose : <<get  initial stock for MiscellanousIssues list>>
		 <param name="datamodel"></param>
		 Review Date :<<>>   Reviewed By :<<>>
		 */

		public async Task<IEnumerable<StockModel>> getMiscellanousIssueListdatanofilter()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					//string query = WMSResource.getMiscellanousIssuesList;
					//if (initialStock)
					//	query += " where st.initialstock=true group by st.poitemdescription, st.itemlocation";
					//else
					//	query += " group by st.poitemdescription,st.itemlocation";
					string query1 = " select max(itemid) as itemid,projectid,materialid as material,pono,poitemdescription,itemlocation,sum(availableqty) as availableqty,sum(availableqty::decimal(19,5) * unitprice::decimal(19,5) ) as value from wms.wms_stock ws where availableqty > 0";
					query1 += " and initialstock=true group by projectid,materialid,pono,poitemdescription,itemlocation";

					await pgsql.OpenAsync();
					var result = await pgsql.QueryAsync<StockModel>(
					  query1, null, commandType: CommandType.Text);
					return result;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getMiscellanousIssueList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}


		/* Name of Function : <<get initial stock>>  Author :<<prasanna>>  
		 Date of Creation <<21-12-2019>>
		 Purpose : <<get  initial stock for MiscellanousIssues list>>
		 <param name="datamodel"></param>
		 Review Date :<<>>   Reviewed By :<<>>
		 */

		public async Task<IEnumerable<StockModel>> getMiscellanousIssueListdatahistory()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{

					string query1 = WMSResource.getMiscIssuedlist;

					await pgsql.OpenAsync();
					var result = await pgsql.QueryAsync<StockModel>(
					  query1, null, commandType: CommandType.Text);
					return result;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getMiscellanousIssueList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/* Name of Function : <<miscellanousIssueDataUpdatek>>  Author :<<prasanna>>  
		 Date of Creation <<21-12-2019>>
		 Purpose : <<miscellanousIssueDataUpdate>>
		 <param name="datamodel"></param>
		 Review Date :<<>>   Reviewed By :<<>>
		 */
		public bool miscellanousIssueDataUpdate(miscellanousIssueData data)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string qry = WMSResource.updateMisQty.Replace("#IssuedQty", data.MiscellanousIssueQty).Replace("#projectid", "'" + data.ProjectId + "'").Replace("#itemid", "'" + data.itemid + "'");
					var result1 = pgsql.Execute(qry);
					string transactiontype = "Miscellanous Issue";
					var issuedqty = Conversion.Todecimaltype(data.MiscellanousIssueQty);
					var reason = Convert.ToInt32(data.Reason);
					var remarks = data.Remarks;
					DateTime createddate = DateTime.Now;
					string insertpry = WMSResource.updateStockLog;
					pgsql.ExecuteScalar(insertpry, new
					{
						data.itemid,
						transactiontype,
						issuedqty,
						reason,
						remarks,
						createddate,
						data.createdby,
					});
				}

				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "miscellanousIssueDataUpdate", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return false;
				}
				finally
				{
					pgsql.Close();
				}
				return true;
			}
		}

		/* Name of Function : <<miscellanousIssueDataUpdatek>>  Author :<<prasanna>>  
		 Date of Creation <<21-12-2019>>
		 Purpose : <<miscellanousIssueDataUpdate>>
		 <param name="datamodel"></param>
		 Review Date :<<>>   Reviewed By :<<>>
		 */
		public string multiplemiscellanousIssueDataUpdate(List<miscellanousIssueData> dataobj)
		{
			using (var DB = new NpgsqlConnection(config.PostgresConnectionString))
			{
				string result = "";

				try
				{
					string uploadcode = Guid.NewGuid().ToString();
					foreach (var item in dataobj)
					{
						string materialdescription = null;
						if (item.materialdescription != null)
						{
							materialdescription = item.materialdescription.Replace("\'", "''");
						}
						string stockquery = "";
						if (item.ProjectId != null && item.ProjectId.Trim() != "" && item.pono != null && item.pono.Trim() != "")
						{
							stockquery = "select * from wms.wms_stock where materialid = '" + item.material + "' and lower(poitemdescription) = lower('" + materialdescription + "') and availableqty > 0 and itemlocation = '" + item.itemlocation + "' and  projectid = '" + item.ProjectId + "' and pono = '" + item.pono + "' order by itemid";
						}
						else
						{
							stockquery = "select * from wms.wms_stock where materialid = '" + item.material + "' and lower(poitemdescription) = lower('" + materialdescription + "') and availableqty > 0 and itemlocation = '" + item.itemlocation + "' ";
							if (item.ProjectId == null || item.ProjectId.Trim() == "")
							{
								stockquery += " and (projectid is null or projectid = '')";
							}
							else
							{
								stockquery += " and projectid = '" + item.ProjectId + "'";

							}
							if (item.pono == null || item.pono.Trim() == "")
							{
								stockquery += " and (pono is null or pono = '')";
							}
							else
							{
								stockquery += " and pono = '" + item.pono + "'";

							}
							stockquery += " order by itemid";
						}
						var stockdata = DB.QueryAsync<StockModel>(stockquery, null, commandType: CommandType.Text);
						if (stockdata != null)
						{
							decimal? quantitytoissue = item.issuedqty;
							decimal? issuedqty = 0;
							foreach (StockModel itm in stockdata.Result)
							{
								DateTime approvedon = System.DateTime.Now;
								string materialid = item.material;
								if (quantitytoissue <= itm.availableqty)
								{
									issuedqty = quantitytoissue;
								}
								else
								{
									issuedqty = itm.availableqty;
								}

								quantitytoissue = quantitytoissue - issuedqty;
								string insertqueryforstatusforqty = WMSResource.updateqtyafterissue.Replace("#itemid", Convert.ToString(itm.itemid)).Replace("#issuedqty", Convert.ToString(issuedqty));
								var data1 = DB.ExecuteScalar(insertqueryforstatusforqty, new
								{

								});
								string transactiontype = "Miscellanous Issue";
								var reason = Convert.ToInt32(dataobj[0].Reason);
								var remarks = dataobj[0].Remarks;
								DateTime createddate = DateTime.Now;
								string insertpry = WMSResource.updateStockLog;
								string projectid = item.ProjectId;

								DB.ExecuteScalar(insertpry, new
								{
									itm.itemid,
									transactiontype,
									issuedqty,
									reason,
									remarks,
									createddate,
									dataobj[0].createdby,
									projectid,
									item.pono,
									uploadcode
								});

								if (quantitytoissue <= 0)
								{
									break;
								}


							}



						}
					}


					result = "saved";
				}

				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "miscellanousIssueDataUpdate", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					result = "";
					return result;
				}
				finally
				{
					DB.Close();
				}
				return result;
			}
		}

		/* Name of Function : <<miscellanousIssueDataUpdatek>>  Author :<<prasanna>>  
		 Date of Creation <<21-12-2019>>
		 Purpose : <<miscellanousIssueDataUpdate>>
		 <param name="datamodel"></param>
		 Review Date :<<>>   Reviewed By :<<>>
		 */
		public string updatelocationdata(matlocations dataobj)
		{
			using (var DB = new NpgsqlConnection(config.PostgresConnectionString))
			{
				string result = "";
				try
				{

					string insertqueryforstatusforqty = WMSResource.updatelocationdata.Replace("#itemid", Convert.ToString(dataobj.itemids));

					var data1 = DB.ExecuteScalar(insertqueryforstatusforqty, new
					{
						dataobj.storeid,
						dataobj.binid,
						dataobj.rackid,
						dataobj.itemlocation

					});



					result = "saved";
				}

				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "miscellanousIssueDataUpdate", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					result = "";
					return result;
				}
				finally
				{
					DB.Close();
				}
				return result;
			}
		}
		/* Name of Function : <<getMiscellanousReceiptsList>>  Author :<<prasanna>>  
		 Date of Creation <<22-12-2019>>
		 Purpose : <<get  initial stock for Miscellanous Receipts list>>
		 <param name="datamodel"></param>
		 Review Date :<<>>   Reviewed By :<<>>
		 */

		public async Task<IEnumerable<StockModel>> getMiscellanousReceiptsList()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getMiscellanousIssuesList;

					query += " where st.receivedtype='Miscellanous Receipt' group by st.poitemdescription,  st.itemlocation";

					await pgsql.OpenAsync();
					var result = await pgsql.QueryAsync<StockModel>(
					  query, null, commandType: CommandType.Text);
					return result;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getMiscellanousReceiptsList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<updateMiscellanousReceipt>>  Author :<<Prasanna>>  
		Date of Creation <<27-12-2019>>
		Purpose : <<inserting material details to warehouse>>
		<param name="data"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string updateMiscellanousReceipt(StockModel item)
		{
			try
			{
				StockModel obj = new StockModel();
				string loactiontext = string.Empty;
				var result = 0;
				string inwmasterid = null;
				using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				{
					StockModel objs = new StockModel();
					pgsql.Open();
					item.createddate = System.DateTime.Now;
					string insertquery = WMSResource.insertmiscreceipt;
					int itemid = 0;
					string materialid = item.Material;
					item.totalquantity = item.availableqty;
					item.receivedtype = "Miscellanous Receipt";
					var unitprice = item.value / item.availableqty;
					if (item.projectid != null && item.projectid != "")
					{
						item.stocktype = "Project Stock";
					}
					else
					{
						item.stocktype = "Plant Stock";
					}
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						result = Convert.ToInt32(DB.ExecuteScalar(insertquery, new
						{
							inwmasterid,
							item.pono,
							item.binid,
							item.rackid,
							item.storeid,
							item.vendorid,
							item.totalquantity,
							item.shelflife,
							item.availableqty,
							item.deleteflag,
							item.itemlocation,
							item.createddate,
							item.createdby,
							item.stockstatus,
							materialid,
							item.inwardid,
							item.stocktype,
							item.lineitemno,
							item.receivedtype,
							item.poitemdescription,
							item.value,
							unitprice,
							item.projectid
						}));

						itemid = Convert.ToInt32(result);
						string transactiontype = "Miscellanous Receipt";
						DateTime createddate = DateTime.Now;
						string insertpry = WMSResource.updateStockLog;
						int? issuedqty = null;
						var remarks = "";
						int? reason = null;
						string uploadcode = null;
						pgsql.ExecuteScalar(insertpry, new
						{
							itemid,
							transactiontype,
							issuedqty,
							reason,
							remarks,
							createddate,
							item.createdby,
							item.pono,
							item.projectid,
							uploadcode

						});
						return "Sucess";

					}
				}
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "updateMiscellanousReceipt", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return null;
			}


		}


		/*
			Name of Function : <<InsertStock>>  Author :<<Ramesh>>  
			Date of Creation <<12-12-2019>>
			Purpose : <<inserting material details to warehouse>>
			<param name="data"></param>
			Review Date :<<>>   Reviewed By :<<>>
			*/
		public DataTable getMaterialMasterList()
		{
			DataTable dataTable = new DataTable();
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				IDbCommand selectCommand = pgsql.CreateCommand();
				selectCommand.CommandText = WMSResource.getMaterialMasterList;
				IDbDataAdapter dbDataAdapter = new NpgsqlDataAdapter();
				dbDataAdapter.SelectCommand = selectCommand;

				DataSet dataSet = new DataSet();

				dbDataAdapter.Fill(dataSet);
				dataTable = dataSet.Tables[0];
			}
			return dataTable;
		}


		public bool updateMaterialMaster(materilaMasterYgs materialDetails)
		{
			try
			{
				using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				{
					string materialQuery = "Select material from wms.\"MaterialMasterYGS\" where material = '" + materialDetails.material + "'";
					var materialid = pgsql.ExecuteScalar(materialQuery, null);
					if (materialid == null)
					{
						var insertStorequery = WMSResource.insertmaterialMaster;
						var rslt = pgsql.Execute(insertStorequery, new
						{
							materialDetails.material,
							materialDetails.materialdescription,
							materialDetails.storeid,
							materialDetails.rackid,
							materialDetails.binid,
							materialDetails.qualitycheck,
							materialDetails.stocktype,
							materialDetails.unitprice,
							materialDetails.hsncode
						});
					}
					else
					{
						string updateQry = WMSResource.updateMaterialMaster.Replace("#material", materialDetails.material);

						var results = pgsql.ExecuteScalar(updateQry, new
						{
							materialDetails.materialdescription,
							materialDetails.storeid,
							materialDetails.rackid,
							materialDetails.binid,
							materialDetails.qualitycheck,
							materialDetails.stocktype,
							materialDetails.unitprice,
							materialDetails.hsncode

						});
					}
				}
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "updateMaterialMaster", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return false;
			}
			return true;
		}
		/*
		Name of Function : <<GPReasonMTAdd>>  Author :<<Gayathri>>  
		Date of Creation <<29-12-2019>>
		Purpose : <<inserting Gatepass reason master data into rd_reason table>>
		<param name="GPReasonMTData"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string GPReasonMTAdd(GPReasonMTData reasondata)
		{
			string GPResult = "Error";
			try
			{


				using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				{
					if (reasondata.reasonid != 0)
					{
						//Update reason in rd_reason based on reasonid
						string updatequery = WMSResource.updateGPReason.Replace("#reason", "'" + reasondata.reason + "'").Replace("#createdby", "'" + reasondata.createdby + "'");
						updatequery += "where reasonid = " + reasondata.reasonid;
						var result1 = pgsql.Execute(updatequery);

					}
					else
					{
						string insertquery = WMSResource.insertGPReason;
						reasondata.type = "GatePass";
						pgsql.ExecuteScalar(insertquery, new
						{
							reasondata.reason,
							reasondata.type,
							reasondata.createdby

						});
					}

					GPResult = "Success";
				}
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "GPReasonMTAdd", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return null;
			}
			return GPResult;
		}


		/*
		Name of Function : <<MiscellanousReasonAdd>>  Author :<<Gayathri>>  
		Date of Creation <<20-01-2021>>
		Purpose : <<inserting Miscellanous reason master data into rd_reason table>>
		<param name="GPReasonMTData"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string MiscellanousReasonAdd(GPReasonMTData reasondata)
		{
			string GPResult = "Error";
			try
			{


				using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				{
					if (reasondata.reasonid != 0)
					{
						//Update reason in rd_reason based on reasonid
						string updatequery = WMSResource.updateGPReason.Replace("#reason", "'" + reasondata.reason + "'").Replace("#createdby", "'" + reasondata.createdby + "'");
						updatequery += "where reasonid = " + reasondata.reasonid;
						var result1 = pgsql.Execute(updatequery);

					}
					else
					{
						string insertquery = WMSResource.insertGPReason;
						reasondata.type = "Miscellanous";
						pgsql.ExecuteScalar(insertquery, new
						{
							reasondata.reason,
							reasondata.type,
							reasondata.createdby

						});
					}

					GPResult = "Success";
				}
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "GPReasonMTAdd", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return null;
			}
			return GPResult;
		}


		/*
		Name of Function : <<createplant>>  Author :<<Gayathri>>  
		Date of Creation <<15-01-2021>>
		Purpose : <<inserting plant name master data into rd_plant table>>
		<param name="PlantMTdata"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string createplant(PlantMTdata reasondata)
		{
			string plantResult = "Error";
			try
			{


				using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				{
					if (reasondata.plantid != 0)
					{
						//Update reason in rd_reason based on reasonid
						string updatequery = WMSResource.updateplantname.Replace("#plantname", "'" + reasondata.plantname + "'").Replace("#createdby", "'" + reasondata.createdby + "'");
						updatequery += " where plantid = " + reasondata.plantid;
						var result1 = pgsql.Execute(updatequery);

					}
					else
					{
						string insertquery = WMSResource.insertplantname;
						pgsql.ExecuteScalar(insertquery, new
						{
							reasondata.plantname,
							reasondata.createdby

						});
					}

					plantResult = "Success";
				}
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "createplant", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return null;
			}
			return plantResult;
		}

		/*
	Name of Function : <<getGPReasonData>>  Author :<<Gayathri>>  
	Date of Creation <<29-12-2019>>
	Purpose : <<Get the list of Gate Pass reasons>>
	<param name="GPReasonMTData"></param>
	Review Date :<<>>   Reviewed By :<<>>
	*/
		public async Task<IEnumerable<GPReasonMTData>> getGPReasonData()
		{
			try
			{
				using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				{
					string GPDataquery = WMSResource.getGPReasons;
					var gpresult = await pgsql.QueryAsync<GPReasonMTData>(
					  GPDataquery, null, commandType: CommandType.Text);
					return gpresult;
				}
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "getGPReasonData", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return null;
			}
			//return objgp;
		}


		/*
Name of Function : <<getMiscellanousReasonData>>  Author :<<Gayathri>>  
Date of Creation <<29-12-2019>>
Purpose : <<Get the list of Gate Pass reasons>>
<param name="GPReasonMTData"></param>
Review Date :<<>>   Reviewed By :<<>>
*/
		public async Task<IEnumerable<GPReasonMTData>> getMiscellanousReasonData()
		{
			try
			{
				using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				{
					string GPDataquery = WMSResource.getMiscellanousReasons;
					var gpresult = await pgsql.QueryAsync<GPReasonMTData>(
					  GPDataquery, null, commandType: CommandType.Text);
					return gpresult;
				}
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "getMiscellanousReasonData", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return null;
			}
			//return objgp;
		}


		/*
	Name of Function : <<getplantnameData>>  Author :<<Gayathri>>  
	Date of Creation <<29-12-2019>>
	Purpose : <<Get the list of plant names>>
	Review Date :<<>>   Reviewed By :<<>>
	*/
		public async Task<IEnumerable<PlantMTdata>> getplantnameData()
		{
			try
			{
				using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				{
					string GPDataquery = WMSResource.getPlantnames;
					var gpresult = await pgsql.QueryAsync<PlantMTdata>(
					  GPDataquery, null, commandType: CommandType.Text);
					return gpresult;
				}
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "getplantnameData", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return null;
			}
			//return objgp;
		}

		/*
Name of Function : <<GPReasonMTDelete>>  Author :<<Gayathri>>  
Date of Creation <<29-12-2019>>
Purpose : <<Delete Gatepass/Miscellanous reason>>
<param name="GPReasonMTData"></param>
Review Date :<<>>   Reviewed By :<<>>
*/
		public string GPReasonMTDelete(GPReasonMTData reasondata)
		{
			string GPResult = "Error";
			try
			{


				using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				{

					//Update reason in rd_reason based on reasonid
					bool isdelete = true;
					string updatequery = WMSResource.deleteGPReason.Replace("#isdelete", "'" + isdelete + "'").Replace("#deletedby", "'" + reasondata.createdby + "'");
					updatequery += "where reasonid = " + reasondata.reasonid;
					var result1 = pgsql.Execute(updatequery);


					GPResult = "Success";
				}
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "GPReasonMTAdd", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return null;
			}
			return GPResult;
		}


		/*
Name of Function : <<PlantnameDelete>>  Author :<<Gayathri>>  
Date of Creation <<15-01-2021>>
Purpose : <<Delete plant name>>
<param name="PlantMTdata"></param>
Review Date :<<>>   Reviewed By :<<>>
*/
		public string PlantnameDelete(PlantMTdata plantdata)
		{
			string GPResult = "Error";
			try
			{


				using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				{

					//Update deletedby and deletedon columns in rd_plant table based on reasonid
					bool isdelete = true;
					string deleteplantnames = WMSResource.deleteplantnames.Replace("#deletedby", "'" + plantdata.createdby + "'");
					deleteplantnames += " where plantid = " + plantdata.plantid;
					var result1 = pgsql.Execute(deleteplantnames);


					GPResult = "Success";
				}
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "GPReasonMTAdd", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return null;
			}
			return GPResult;
		}

		/*
		Name of Function : <<getSTORequestList>>  Author :<<Prasanna>>  
		Date of Creation <<08/01/2021>>
		Purpose : <<get stock transferdata>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<invstocktransfermodel>> getSTORequestList(string type)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					//string materialrequestquery = WMSResource.invstocktransforSTO;
					string materialrequestquery = WMSResource.getSTOListForIssue.Replace("#type", type);
					await pgsql.OpenAsync();
					var result = await pgsql.QueryAsync<invstocktransfermodel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					return result;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getSTORequestList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getAvailableQtyBystore>>  Author :<<Ramesh>>  
		Date of Creation <<12/02/2021>>
		Purpose : <<get stock transferdata>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<WMSHttpResponse> getAvailableQtyBystore(string store, string materialid, string description, string projectcode)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					//string materialrequestquery = WMSResource.invstocktransforSTO;
					string descriptionstr = null;
					if (description != null)
					{
						descriptionstr = description.Replace("\'", "''");
					}
					string materialrequestquery = WMSResource.getAvailableqtybyStore.Replace("#store", store).Replace("#material", materialid).Replace("#description", descriptionstr).Replace("#projectcode", projectcode);
					await pgsql.OpenAsync();
					var result = await pgsql.QueryAsync<WMSHttpResponse>(
					  materialrequestquery, null, commandType: CommandType.Text);
					return result.FirstOrDefault();
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getAvailableQtyBystore", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getMatdetailsbyTransferId>>  Author :<<Prasanna>>  
		Date of Creation <<08/01/2021>>
		Purpose : <<get material deatils by transferid>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<STOIssueModel>> getMatdetailsbyTransferId(string transferId, string type, string transfertype)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getMatdetailsbyTransferId.Replace("#transfertype", transfertype);
					materialrequestquery += " where inv.transferid = '" + transferId + "' ";
					if (type == "MatIssue")
					{
						//materialrequestquery += " and stock.availableqty is not null and stock.availableqty > 0";
					}
					if (type == "POInitiate")
					{
						//materialrequestquery += " and (stock.availableqty =0 or stock.availableqty is null)";
					}
					materialrequestquery += " group by inv.transferid, inv.materialid, inv.poitemdesc,inv.pono";
					await pgsql.OpenAsync();
					var result = await pgsql.QueryAsync<STOIssueModel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					return result;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getMatdetailsbyTransferId", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<STOPOInitiate>>  Author :<<Prasanna>>  
		Date of Creation <<11/01/2021>>
		Purpose : <<save PO Initiate details to SCM and wms>>
		Review Date :<<>>   Reviewed By :<<>>
		*/


		public async Task<string> STOPOInitiate(List<STOIssueModel> data)
		{
			//send data to scm to create PO

			var mprData = new MPRRevision();
			mprData.MPRDetail = new MPRDetail();
			mprData.MPRItemInfoes = new List<MPRItemInfo>();
			//mprData.IssuePurposeId = 1;
			//mprData.DepartmentId = 1;
			//mprData.BuyerGroupId = 1;
			mprData.PreparedBy = data[0].uploadedby;
			mprData.CheckedBy = config.POChecker;
			mprData.ApprovedBy = config.POApprover;
			foreach (STOIssueModel item in data)
			{
				MPRItemInfo mPRItemInfo = new MPRItemInfo();
				mPRItemInfo.Itemid = item.materialid;
				mPRItemInfo.ItemDescription = item.poitemdescription;
				mPRItemInfo.Quantity = Convert.ToDecimal(item.poqty);
				//mPRItemInfo.UnitId = 1;
				mprData.MPRItemInfoes.Add(mPRItemInfo);
			}
			using (var client = new HttpClient())
			{
				StringContent content = new StringContent(JsonConvert.SerializeObject(mprData), Encoding.UTF8, "application/json");

				using (var response = await client.PostAsync(config.SCMUrl, content))
				{
					string apiResponse = await response.Content.ReadAsStringAsync();
					var result = JsonConvert.DeserializeObject<MPRRevision>(apiResponse);
					if (result != null)
					{
						//send mail to checker and approver
						EmailUtilities emailobj = new EmailUtilities();
						int mprrevisionid = result.RevisionId;
						emailobj.sendCreatePOMail(data[0].uploadedby, mprrevisionid);

					}
				}

			}

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					//update po request to wms.wms_invstocktransfer table
					string updaterequest = "update wms.wms_invstocktransfer set isporequested = true where transferid='" + data[0].transferid + "'";

					var data2 = pgsql.ExecuteScalar(updaterequest, new
					{

					});
					string query = WMSResource.updatePOInitiateDetails;
					string scmStatus = "Sucess";
					foreach (STOIssueModel item in data)
					{
						int poqty = Convert.ToInt32(item.poqty);
						var results = pgsql.ExecuteScalar(query, new
						{
							item.transferid,
							item.materialid,
							item.poitemdescription,
							poqty,
							scmStatus,
							item.uploadedby
						});
					}
					return "Sucess";
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "STOPOInitiate", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
Name of Function : <<generateLabel>>  Author :<<Gayathri>>  
Date of Creation <<06-01-2021>>
Purpose : <<Generate barcode and QRCode label>>
<param name="labeldata"></param>
Review Date :<<>>   Reviewed By :<<>>
*/
		public string generateLabel(string labeldata)
		{
			string path = "";
			try
			{
				path = Environment.CurrentDirectory + @"\PRNFiles\";

				BarcodeWriter writer = new BarcodeWriter
				{
					Format = BarcodeFormat.CODE_128,
					Options = new EncodingOptions
					{
						Height = 90,
						Width = 100,
						PureBarcode = false,
						Margin = 1,

					},
				};
				var bitmap = writer.Write(labeldata);

				// write text and generate a 2-D barcode as a bitmap
				writer
					.Write(labeldata)
					.Save(path + labeldata + "_" + DateTime.Now + ".bmp");

				path = "./Barcodes/" + labeldata + "_" + DateTime.Now + ".bmp";
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "GPReasonMTAdd", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return null;
			}
			return path;
		}

		/*
		Name of Function : <<getplantlocdetails>>  Author :<<Gayathri>>  
		Date of Creation <<28-01-2021>>
		Purpose : <<Get plant location data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<plantddl>> getplantlocdetails()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = "select * from wms.wms_rd_locator where deleteflag is not true  order by locatorid asc";

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<plantddl>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getplantlocdetails", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}


		/*
				Name of Function : <<updateSubcontractAcKstatus>>  Author :<<Prasanna>>  
				Date of Creation <<05/02/2021>>
				Purpose : <<Update ACK details in wms_invstocktransfer>>
				Review Date :<<>>   Reviewed By :<<>>
				*/
		public async Task<string> updateSubcontractAcKstatus(List<invstocktransfermodel> ackData)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					foreach (invstocktransfermodel model in ackData)
					{

						using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						{
							if (model.Checkstatus == true)
							{
								model.ackstatus = "Received";
								model.status = "Acknowledged";
							}
							//else if (model.Checkstatus == false)
							//{
							//	model.ackstatus = "Not Received";
							//}
							string updateackstatus = WMSResource.updateackstatusforSubcontract.Replace("#transferid", model.transferid.ToString());
							model.ackon = DateTime.Now;
							var result = pgsql.ExecuteScalar(updateackstatus, new
							{
								model.ackstatus,
								model.ackremarks,
								model.ackby,
								model.ackon,
								model.status
							});
						}
					}
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "updateSubcontractAcKstatus", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				return "Sucess";

			}
		}

		/*
	Name of Function : <<generateqronhold>>  Author :<<Gayathri>>  
	Date of Creation <<04-02-2021>>
	Purpose : <<Generate QRCodes for onhold receipts and save data in db>>
	<parameter :printonholdGR>
	Review Date :<<>>   Reviewed By :<<>>
	*/
		public printonholdGR generateqronhold(printonholdGR onholdprintdata)
		{
			string path = "";
			try
			{
				path = Environment.CurrentDirectory + @"\Barcodes\";
				string inwmasterid = onholdprintdata.gateentryid;
				//Insert data into wms_printstatusmaterial
				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					string getdata = "select count(*) from wms.wms_printstatusmaterial where inwardid='" + onholdprintdata.inwardid + "' and materialid ='" + onholdprintdata.materialid + "'";

					int? dataexists = DB.QuerySingleOrDefault<int?>(
										getdata, null, commandType: CommandType.Text);
					if (dataexists <= 0)
					{
						onholdprintdata.printedby = onholdprintdata.createdby;
						onholdprintdata.printcount = 1;
						onholdprintdata.isprint = true;
						string insertmatonhold = WMSResource.insertmaterialdetails;


						var results = DB.ExecuteScalar(insertmatonhold, new
						{
							inwmasterid,
							onholdprintdata.inwardid,
							onholdprintdata.printedby,
							onholdprintdata.printcount,
							onholdprintdata.isprint,
							onholdprintdata.printerid,
							onholdprintdata.materialid,
							onholdprintdata.noofprint
						});
					}


				}
				PrintUtilities objprint = new PrintUtilities();
				//generate QRCodes for material and gateentryid and update in db

				string materialpath = objprint.generateqrcode(path, onholdprintdata.materialid);
				if (materialpath != "Error")
				{
					onholdprintdata.materialqrpath = materialpath;
					//Insert data in db (wms.wms_barcode table)
					string insertbarcode = WMSResource.insertbarcodedata;
					bool deleteflag = false;
					string barcode = onholdprintdata.materialid;
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						var barcodeResult = DB.Execute(insertbarcode, new
						{
							barcode,
							onholdprintdata.createdby,
							onholdprintdata.createddate,
							deleteflag,
							onholdprintdata.pono,
							onholdprintdata.invoiceno,
							inwmasterid

						});
					}


				}
				string gateentrypath = objprint.generateqrcode(path, onholdprintdata.gateentryid);
				if (materialpath != "Error")
				{
					onholdprintdata.materialqrpath = gateentrypath;
					//Insert data in db (wms.wms_barcode table)
					string insertbarcode = WMSResource.insertbarcodedata;
					bool deleteflag = false;
					string barcode = onholdprintdata.gateentryid;
					onholdprintdata.createddate = DateTime.Now;
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						var barcodeResult = DB.Execute(insertbarcode, new
						{
							barcode,
							onholdprintdata.createdby,
							onholdprintdata.createddate,
							deleteflag,
							onholdprintdata.pono,
							onholdprintdata.invoiceno,
							inwmasterid

						});
					}

				}
				return onholdprintdata;

			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "generateqronhold", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return null;
			}
		}


		/*
		Name of Function : <<subcontractInoutList>>  Author :<<Prasanna>>  
		Date of Creation <<09/12/2021>>
		Purpose : <<subcontractInoutList for outward entry>>
		<param name="type"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public async Task<IEnumerable<gatepassModel>> subcontractInoutList()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getSubcontractgatepassdata;
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<gatepassModel>(
					   query, null, commandType: CommandType.Text);
					return data;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "subcontractInoutList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}



		/*Name of Function : <<UpdateVendorMaster>>  Author :<<Prasanna>>  
		Date of Creation <<29/03/2021>>
		Purpose : <<Update VendorMaster >>
		<param name="type"></param>
		Review Date :<<>>   Reviewed By :<<>>*/

		public bool updateVendorMaster(vendorMaster vendormaster)
		{
			try
			{
				DateTime updatedOn = DateTime.Now;
				using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				{
					if (vendormaster.vendorid == null || vendormaster.vendorid == 0)
					{
						var insertStorequery = WMSResource.insertVendorMaster;
						var rslt = pgsql.Execute(insertStorequery, new
						{
							vendormaster.vendorname,
							vendormaster.vendorcode,
							vendormaster.faxno,
							vendormaster.contactno,
							vendormaster.street,
							vendormaster.emailid,
							vendormaster.updatedby,
							updatedOn,
							vendormaster.deleteflag

						});
					}
					else
					{
						string updateQry = WMSResource.updateVendorMaster.Replace("#vendorid", Convert.ToString(vendormaster.vendorid));

						var results = pgsql.ExecuteScalar(updateQry, new
						{
							vendormaster.vendorname,
							vendormaster.vendorcode,
							vendormaster.faxno,
							vendormaster.contactno,
							vendormaster.street,
							vendormaster.emailid,
							vendormaster.updatedby,
							updatedOn,
							vendormaster.deleteflag

						});
					}
				}
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "updateVendorMaster", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return false;
			}
			return true;
		}
		/*Name of Function : <<updateRole>>  Author :<<Prasanna>>  
		Date of Creation <<29/03/2021>>
		Purpose : <<Update Role master table>>
		<param name="type"></param>
		Review Date :<<>>   Reviewed By :<<>>*/
		public bool updateRole(roleMaster roleMaster)
		{
			try
			{
				DateTime createddate = DateTime.Now;
				using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				{
					if (roleMaster.roleid == null || roleMaster.roleid == 0)
					{
						var insertStorequery = WMSResource.insertRoleMaster;
						var rslt = pgsql.Execute(insertStorequery, new
						{
							roleMaster.rolename,
							createddate,
							roleMaster.createdby,
							roleMaster.deleteflag

						});
					}
					else
					{
						string updateQry = WMSResource.updateRoleMaster.Replace("#roleid", Convert.ToString(roleMaster.roleid));

						var results = pgsql.ExecuteScalar(updateQry, new
						{
							roleMaster.rolename,
							createddate,
							roleMaster.createdby,
							roleMaster.deleteflag
						});
					}
				}
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "updateRole", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return false;
			}
			return true;
		}

		/*Name of Function : <<updateSubRole>>  Author :<<Prasanna>>  
		Date of Creation <<29/03/2021>>
		Purpose : <<Update subRole master table>>
		<param name="type"></param>
		Review Date :<<>>   Reviewed By :<<>>*/
		public bool updateSubRole(subrolemodel subrolemaster)
		{
			try
			{
				DateTime createddate = DateTime.Now;
				using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				{
					if (subrolemaster.subroleid == null || subrolemaster.subroleid == 0)
					{
						var insertStorequery = WMSResource.insertSubRoleMaster;
						var rslt = pgsql.Execute(insertStorequery, new
						{
							subrolemaster.roleid,
							subrolemaster.subrolename,
							createddate,
							subrolemaster.createdby,
							subrolemaster.deleteflag
						});
					}
					else
					{
						string updateQry = WMSResource.updateSubRoleMaster.Replace("#subroleid", Convert.ToString(subrolemaster.subroleid));

						var results = pgsql.ExecuteScalar(updateQry, new
						{
							subrolemaster.roleid,
							subrolemaster.subrolename,
							createddate,
							subrolemaster.createdby,
							subrolemaster.deleteflag
						});
					}
				}
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "updateSubRole", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return false;
			}
			return true;
		}

		/*Name of Function : <<updateUserRole>>  Author :<<Prasanna>>  
		Date of Creation <<29/03/2021>>
		Purpose : <<Update userrole master table>>
		<param name="type"></param>
		Review Date :<<>>   Reviewed By :<<>>*/
		public bool updateUserRole(userRoles userRoles)
		{
			try
			{
				DateTime createddate = DateTime.Now;
				using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				{
					if (userRoles.userid == null || userRoles.userid == 0)
					{
						var insertStorequery = WMSResource.insertUserRole;
						var rslt = pgsql.Execute(insertStorequery, new
						{
							userRoles.roleid,
							userRoles.accessname,
							createddate,
							userRoles.createdby,
							userRoles.deleteflag
						});
					}
					else
					{
						string updateQry = WMSResource.updateUserRole.Replace("#userid", Convert.ToString(userRoles.userid));

						var results = pgsql.ExecuteScalar(updateQry, new
						{
							userRoles.roleid,
							userRoles.accessname,
							createddate,
							userRoles.createdby,
							userRoles.deleteflag

						});
					}
				}
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "updateUserRole", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
				return false;
			}
			return true;
		}

		/*Name of Function : <<getDDdetailsByPono>>  Author :<<Prasanna>>  
		Date of Creation <<17/05/2021>>
		Purpose : <<get 
		delivery  details By Pono>>
		<param name="type"></param>
		Review Date :<<>>   Reviewed By :<<>>*/
		public async Task<IEnumerable<DDmaterials>> getDDdetailsByPono(string pono)
		{

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getDirectDeliverybyPOno.Replace("#pono", pono);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<DDmaterials>(
					   query, null, commandType: CommandType.Text);
					return data;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "subcontractInoutList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}

		}

		public async Task<IEnumerable<YGSGR>> getYGSGR()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = "select pono,invoiceno,wmsgr,faileddatetime,failreason from wms.rpa_migo_failed_records rmfr";
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<YGSGR>(
					   query, null, commandType: CommandType.Text);
					return data;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "subcontractInoutList", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		public async Task<IEnumerable<gatepassModel>> GetGPReport(string fromdate, string todate)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getGPReport;
					if (!string.IsNullOrEmpty(todate))
						query += " and gp.requestedon::date <= '" + todate + "'";
					if (!string.IsNullOrEmpty(fromdate))
						query += "  and gp.requestedon::date >= '" + fromdate + "'";
					query += " order by gp.gatepassid desc";
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<gatepassModel>(
					   query, null, commandType: CommandType.Text);
					foreach (gatepassModel gp in data)
					{
						if (gp.gatepasstype == "Returnable")
						{
							if (gp.totaloutwardqty != null)
							{
								gp.status = "Open";
							}
							if (gp.totalinwardqty != null)
							{
								if (gp.totaloutwardqty == gp.totalinwardqty)
								{
									gp.status = "Closed";
								}
								else
								{
									gp.status = "Partially Closed";
								}
							}
							double age = (DateTime.Now - Convert.ToDateTime(gp.expecteddate)).TotalDays;
							gp.ageing = Convert.ToInt32(Math.Floor(age));

						}
						if (gp.status == "Pending")
						{
							gp.status = "Created";
						}
					}
					return data;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getGPReport", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		public async Task<IEnumerable<gatepassModel>> GetGPReportMaterials()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.gatepassreportmaterials;
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<gatepassModel>(
					   query, null, commandType: CommandType.Text);
					return data;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetGPReportMaterials", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		/*Name of Function : <<updateDirectDelivery>>  Author :<<Prasanna>>  
		Date of Creation <<17/05/2021>>
		Purpose : <<insert or update DirectDelivery delivery details>>
		<param name="type"></param>
		Review Date :<<>>   Reviewed By :<<>>*/

		public bool updateDirectDelivery(DirectDelivery dddetails)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					pgsql.OpenAsync();
					if (string.IsNullOrEmpty(dddetails.inwmasterid))
					{
						sequencModel obj = new sequencModel();
						string lastinsertedgrn = WMSResource.lastinsertedgrn;
						int grnnextsequence = 0;
						string grnnumber = string.Empty;
						DateTime grndate = DateTime.Now;
						DateTime receiveddate = DateTime.Now;
						string grnnogeneratedby = dddetails.receivedby;
						obj = pgsql.QuerySingle<sequencModel>(
					   lastinsertedgrn, null, commandType: CommandType.Text);
						if (obj.id != 0)
						{
							grnnextsequence = (Convert.ToInt32(obj.sequencenumber) + 1);
							grnnumber = obj.sequenceid + "-" + obj.year + "-" + grnnextsequence.ToString().PadLeft(6, '0');
						}


						string insertInv = WMSResource.insertDDInvoice;
						var rslt = pgsql.ExecuteScalar(insertInv, new
						{
							dddetails.pono,
							dddetails.invoiceno,
							dddetails.invoicedate,
							grnnumber,
							grndate,
							grnnogeneratedby,
							dddetails.receivedby,
							receiveddate,
							dddetails.suppliername,
							dddetails.directdeliveryaddrs,
							dddetails.directdeliveryremarks,
							dddetails.directdeliveredon,
							dddetails.vehicleno,
							dddetails.transporterdetails
						});
						string updateseqnumber = WMSResource.updateseqnumber;
						int id = obj.id;
						var results1 = pgsql.ExecuteScalar(updateseqnumber, new
						{
							grnnextsequence,
							id,

						});
						var inwmasterid = rslt.ToString();
						string insertMat = WMSResource.insertDDinwardDetails;

						foreach (DDmaterials model in dddetails.DDmaterialList)
						{
							var receivedqty = model.pendingqty;
							var confirmqty = model.pendingqty;
							var res = pgsql.Execute(insertMat, new
							{
								inwmasterid,
								receiveddate,
								dddetails.receivedby,
								receivedqty,
								confirmqty,
								model.materialqty,
								model.materialid,
								model.pono,
								model.lineitemno,
								model.poitemdescription,
								model.unitprice
							});
						}
					}
					else
					{
						foreach (var model in dddetails.DDmaterialList)
						{
							var receivedqty = model.pendingqty;
							var confirmqty = model.pendingqty;
							string insertforquality = WMSResource.updateddReceivedqty.Replace("#inwardid", model.inwardid.ToString());

							var results = pgsql.ExecuteScalar(insertforquality, new
							{
								receivedqty,
								confirmqty
							});

						}
					}
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "updateDirectDelivery", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return false;
				}
				finally
				{
					pgsql.Close();
				}
				return true;
			}
		}


		/*Name of Function : <<deleteDirectDelivery>>  Author :<<Prasanna>>  
		Date of Creation <<17/05/2021>>
		Purpose : <<delete18 direct delivery  details By>>
		<param name="type"></param>
		Review Date :<<>>   Reviewed By :<<>>*/
		public bool deleteDirectDelivery(string inwmasterid, string deletedby)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					pgsql.OpenAsync();
					DateTime deletedon = DateTime.Now;
					string delSec = WMSResource.deleteDDSecInw.Replace("#inwmasterid", inwmasterid.ToString());
					var results = pgsql.ExecuteScalar(delSec, new
					{
						deletedby,
						deletedon
					});
					string query = "select inwardid from wms.wms_storeinward  where inwmasterid ='" + inwmasterid + "'";
					var matList = pgsql.QueryAsync<DDmaterials>(
					   query, null, commandType: CommandType.Text);

					int count = matList.Result.Count();

					if (matList != null && (matList.Result.Count() > 0))
					{
						foreach (var model in matList.Result)
						{
							string delStore = WMSResource.deleteDDStoreInw.Replace("#inwardid", model.inwardid.ToString());

							var result = pgsql.ExecuteScalar(delStore, new
							{
								deletedby,
								deletedon

							});
						}
					}
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "deleteDirectDelivery", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
					return false;
				}
				finally
				{
					pgsql.Close();
				}
				return true;
			}
		}
	}
}
