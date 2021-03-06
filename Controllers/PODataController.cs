﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WMS.Interfaces;
using WMS.Models;
using System.Globalization;
using WMS.Common;
using System.Reflection.Metadata;
using System.IO;
using System.Net.Http.Headers;
using System.Web;

namespace WMS.Controllers
{
	// [Authorize]
	[ApiController]
	[Route("[controller]")]
	public class PODataController : ControllerBase
	{
		private IPodataService<OpenPoModel> _poService;
		EmailUtilities emailobj = new EmailUtilities();
		public PODataController(IPodataService<OpenPoModel> poService)
		{
			_poService = poService;
		}

		[HttpGet("GetOpenPoList")]
		public async Task<IEnumerable<OpenPoModel>> GetPoNodata(string loginid, string pono = null, string docno = null, string vendorid = null)
		{
			return await this._poService.getOpenPoList(loginid, pono, docno, vendorid);
		}
		[HttpGet("GetPMdashboard")]
		public async Task<IEnumerable<OpenPoModel>> GetPMdashboard(string loginid)
		{
			return await this._poService.getdashboardlist(loginid);
		}
		//Get list of PO 
		[HttpGet("GetPOList")]
		public async Task<IEnumerable<POList>> GetPoNo(string postatus)
		{
			return await this._poService.getPOList(postatus);
		}
		[HttpGet("CheckPoNoexists")]
		public OpenPoModel CheckPo(string PONO)
		{
			return this._poService.CheckPoexists(PONO);
		}

		//Get invoice details
		[HttpGet("getinvoicedetailsforpo")]
		public async Task<IEnumerable<InvoiceDetails>> getinvoiceforpo(string PONO)
		{
			return await this._poService.getinvoiveforpo(PONO);
		}

		//Get material details
		[HttpGet("getMaterialDetailsforgrn")]
		public async Task<IEnumerable<MaterialDetails>> getMaterialDetails(string grnNo, string pono)
		{
			return await this._poService.getMaterialDetails(grnNo, pono);
		}

		//Get Location details for material
		[HttpGet("getlocationdetailsformaterialid")]
		public async Task<IEnumerable<LocationDetails>> getlocationdetails(string materialid, string grnnumber)
		{
			return await this._poService.getlocationdetails(materialid, grnnumber);
		}

		//Get material request details
		[HttpGet("getReqMatdetailsformaterialid")]
		public async Task<IEnumerable<ReqMatDetails>> getReqMatdetails(string materialid, string grnnumber, string pono)
		{
			return await this._poService.getReqMatdetails(materialid, grnnumber, pono);
		}

		//Get material reserve details
		[HttpGet("getReserveMatdetailsformaterialtracking")]
		public async Task<IEnumerable<ReqMatDetails>> getReserveMatdetails(string materialid, string grnnumber, string pono)
		{
			return await this._poService.getReserveMatdetails(materialid, grnnumber, pono);
		}

		[HttpPost("generateBarcodeMaterial")]
		public printMaterial generateBarcodeMaterial(printMaterial printMat)
		{
			return this._poService.generateBarcodeMaterial(printMat);

		}

		[HttpPost("generateBarcodeMatonhold")]
		public printMaterial generateBarcodeMatonhold(printMaterial printMat)
		{
			return this._poService.generateBarcodeMatonhold(printMat);

		}

		[HttpPost("printBarcodeMaterial")]
		public string printBarcodeMaterial(printMaterial printMat)
		{
			return this._poService.printBarcodeMaterial(printMat);

		}

		[HttpPost("uploaddoc"), DisableRequestSizeLimit]

		public IActionResult Upload()
		{
			try
			{
				var file = Request.Form.Files[0];
				var folderName = Path.Combine("Resources", "documents");
				var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

				if (file.Length > 0)
				{
					var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
					var fullPath = Path.Combine(pathToSave, fileName);
					var dbPath = Path.Combine(folderName, fileName);

					using (var stream = new FileStream(fullPath, FileMode.Create))
					{
						file.CopyTo(stream);
					}

					return Ok(new { dbPath });
				}
				else
				{
					return BadRequest();
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error: {ex}");
			}
		}


		[HttpPost("updateputawayfilename")]
		public string updateputawayfile(ddlmodel data)
		{
			return this._poService.updateputawayfilename(data);
		}


		[HttpPost("notifyputaway")]
		public string notifyputawayfn(notifymodel data)
		{
			return this._poService.notifyputaway(data);
		}

		[HttpPost("notifymultipleputaway")]
		public string notifyputawaymultiplefn(List<notifymodel> data)
		{
			return this._poService.notifymultipleputaway(data);
		}



		[HttpPost("insertbarcodeandinvoiceinfo")]
		public PrintHistoryModel insertbardata(BarcodeModel data)
		{
			return this._poService.InsertBarcodeInfo(data);
		}
		//[HttpPost("insertinvoiceinfo")]
		//public int insertinvoicedata(iwardmasterModel data)
		//{
		//    return this._poService.insertInvoicedetails(data);
		//}
		//need list of items
		[HttpGet("Getthreewaymatchingdetails")]
		public async Task<IEnumerable<OpenPoModel>> Getdetailsforthreewaymatching(string pono, string invoice)
		{
			bool isgrn = true;
			string grn = "";
			if (pono.StartsWith("GE"))
			{
				isgrn = false;

			}
			else
			{
				grn = pono;
			}
			return await this._poService.GetDeatilsForthreeWaymatching(invoice, pono, isgrn, grn);
			//string grn = "";
			//string po = pono;
			//if (pono.Contains('-'))
			//{
			//	string ponodata = "";
			//	string invoiceno = "";
			//	string[] ponoandinvoice = pono.Split('-');
			//	if (ponoandinvoice.Length == 3)
			//	{
			//		isgrn = true;
			//		grn = po;
			//	}
			//	else if (ponoandinvoice.Length == 4)
			//	{
			//		ponodata = ponoandinvoice[0].Trim() + "-" + ponoandinvoice[1].Trim() + "-" + ponoandinvoice[2].Trim();
			//		invoiceno = ponoandinvoice[3].Trim();
			//	}



			//}
			//else
			//{
			//	return null;
			//}

		}

		[HttpGet("GetholdGRdetails")]
		public async Task<IEnumerable<OpenPoModel>> getholdgrdetails(string status)
		{

			return await this._poService.GetDeatilsForholdgr(status);
		}


		[HttpGet("Getqualitydetails")]
		public async Task<IEnumerable<OpenPoModel>> Getqualitydetails(string grnnumber)
		{

			//string[] ponoandinvoice = pono.Split('-');
			//string ponodata = ponoandinvoice[0];
			//string invoiceno = ponoandinvoice[1];
			return await this._poService.Getqualitydetails(grnnumber);
		}

		[HttpGet("verifythreewaymatch")]
		public async Task<OpenPoModel> verifythreewaymatching(string pono, string invoiceno, string type)
		{
			//string[] ponoandinvoice = pono.Split('-');
			//string ponodata = ponoandinvoice[0].Trim() + "-" + ponoandinvoice[1].Trim() + "-" + ponoandinvoice[2].Trim();
			//string invoiceno = ponoandinvoice[3].Trim(); 
			return await this._poService.VerifythreeWay(pono, invoiceno, type);
		}
		[HttpPost("GRNposting")]
		public async Task<string> insertitemdata([FromBody] List<inwardModel> data)
		{
			return await this._poService.insertquantity(data);
		}

		[HttpPost("receiveinvoice")]
		public async Task<string> receiveinvoice([FromBody] List<inwardModel> data)
		{
			return await this._poService.receivequantity(data);
		}

		[HttpPost("updateonholddata")]
		public async Task<string> updateonholddt([FromBody] updateonhold data)
		{
			return await this._poService.updateonholdrow(data);
		}

		/// <summary>
		/// Update of intial stock exception data
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>

		[HttpPost("updateinitialstock")]
		public string updateinitialstock([FromBody] StockModel data)
		{
			return this._poService.updateinitialstockdata(data);
		}
		[HttpPost("updateprojectmember")]
		public string updateprojectmember([FromBody] AssignProjectModel data)
		{
			return this._poService.updateprojectmember(data);
		}

		[HttpPost("qualitycheck")]
		public async Task<string> insertqualitycheck([FromBody] List<inwardModel> data)
		{
			return await this._poService.insertquantitycheck(data);
		}

		[HttpPost("insertreturn")]
		public async Task<string> insertreturnqty([FromBody] List<inwardModel> data)
		{
			return await this._poService.insertreturn(data);
		}

		[HttpPost("updateitemlocation")]
		public string insertitemdataTostock([FromBody] List<StockModel> data)
		{
			return this._poService.InsertStock(data);
		}

		[HttpPost("InsertmatSTO")]
		public string InsertmatSTO([FromBody] List<StockModel> data)
		{
			return this._poService.InsertmatSTO(data);
		}

		[HttpPost("updateitemlocationIS")]
		public string insertitemdataTostockIS([FromBody] initialStock data)
		{
			return this._poService.InsertStockIS(data);
		}

		[HttpPost("UpdateStockTransfer")]
		public string UpdateStockTransferfunc([FromBody] List<StockModel> data)
		{
			return this._poService.UpdateStockTransfer(data);
		}

		[HttpPost("UpdateStockTransfer1")]
		public string UpdateStockTransferfunction([FromBody] invstocktransfermodel data)
		{
			return this._poService.InvStockTransfer(data);
		}


		[HttpPost("GetListItems")]
		public IActionResult GetListItems([FromBody] DynamicSearchResult Result)
		{
			return Ok(this._poService.GetListItems(Result));
		}

		[HttpPost("GetMaterialItems")]
		public IActionResult GetMaterialItems([FromBody] DynamicSearchResult Result)
		{
			return Ok(this._poService.GetMaterialItems(Result));
		}

		//not using
		[HttpPost("issuerequest")]
		public IActionResult issuerequest([FromBody] List<IssueRequestModel> model)
		{
			return Ok(this._poService.IssueRequest(model));
		}

		//Get stocktype -gayathri
		[HttpPost("getstocktype")]
		public string getstocktype(locataionDetailsStock locdetails)
		{
			return this._poService.getstocktype(locdetails);

		}

		//not usinggetMaterialRequestlist
		//list of items
		[HttpGet("getitemdetailsbygrnno")]
		public async Task<IEnumerable<inwardModel>> getitemdetailsbygrnno(string grnnumber)
		{
			return await this._poService.getitemdeatils(grnnumber);
		}

		[HttpGet("getporeportdata")]
		public async Task<IEnumerable<POReportModel>> getporeportdata(string empno, string projectcode, string pono)
		{
			return await this._poService.getPOReportdata(empno, projectcode, pono);
		}

		[HttpGet("getsubconannexuredata")]
		public async Task<IEnumerable<stocktransfermateriakmodel>> getsubconannexuredata(string empno, string subconno)
		{
			return await this._poService.getsubconannexuredata(empno, subconno);
		}

		[HttpGet("getporeportdetail")]
		public async Task<IEnumerable<POReportModel>> getporeportdetail(string materialid, string description, string pono, string querytype, string requesttype, string projectcode, string empno)
		{
			return await this._poService.getPOReportdetail(materialid, description, pono, querytype, requesttype, projectcode, empno);
		}

		[HttpGet("getitemdetailsbygrnnonotif")]
		public async Task<IEnumerable<inwardModel>> getitemdetailsbygrnnonotif(string grnnumber)
		{
			return await this._poService.getitemdeatilsnotif(grnnumber);
		}
		[HttpGet("getmaterialrequestList")]
		public async Task<IEnumerable<MaterialTransaction>> materialissue(string pono = null, string loginid = null)
		{
			return await this._poService.MaterialRequest(pono, loginid);
		}
		[HttpPost("ackmaterialreceived")]
		public int ackmaterial([FromBody] List<IssueRequestModel> data)
		{
			return this._poService.acknowledgeMaterialReceived(data);
		}
		[HttpGet("getmaterialIssueListbyreqid")]
		public async Task<IEnumerable<IssueRequestModel>> getmaterial(string requesterid)
		{
			return await this._poService.GetMaterialissueList(requesterid);
		}
		[HttpGet("getmaterialIssueListbyapproverid")]
		public async Task<IEnumerable<IssueRequestModel>> getmaterialrequestbyapproverid(string approverid)
		{
			return await this._poService.GetMaterialissueListforapprover(approverid);
		}
		[HttpGet("getmaterialIssueListbyrequestid")]
		public async Task<IEnumerable<IssueRequestModel>> getmaterialrequestbyrequestid(string requestid, string pono)
		{
			return await this._poService.GetmaterialdetailsByrequestid(requestid, pono);
		}

		[HttpGet("getponodetailsBypono")]
		public async Task<IEnumerable<IssueRequestModel>> getponodetails(string pono)
		{
			return await this._poService.GetPonodetails(pono);
		}
		[HttpPost("updaterequestedqty")]
		public int updaterequestedqty([FromBody] List<IssueRequestModel> dataobj)
		{
			return this._poService.updaterequestedqty(dataobj);
		}

		[HttpGet("getPODetails")]
		public Task<IEnumerable<PODetails>> getPODetails(string empno)
		{
			return this._poService.getPODetails(empno);

		}
		[HttpGet("getPODetailsbyprojectcode")]
		public Task<IEnumerable<PODetails>> getPODetailsbyprojectcode(string empno, string projectcode)
		{
			return this._poService.getPODetailsByProjectCode(empno, projectcode);

		}
		[HttpGet("getStorePODetailsbyprojectcode")]
		public Task<IEnumerable<PODetails>> getStorePODetailsbyprojectcode(string empno, string projectcode)
		{
			return this._poService.getStorePODetailsByProjectCode(empno, projectcode);

		}

		[HttpGet("getPODetailsbyprojectcodeformiscissue")]
		public Task<IEnumerable<PODetails>> getPODetailsbyprojectcodeformiscissue(string projectcode)
		{
			return this._poService.getPODetailsbyprojectcodeformiscissue(projectcode);

		}

		[HttpPost("approvematerialrequest")]
		public int approvematerial([FromBody] List<IssueRequestModel> data)
		{
			return this._poService.ApproveMaterialissue(data);

		}
		[HttpGet("getgatepasslist")]
		public async Task<IEnumerable<gatepassModel>> getgatepasslist()
		{
			return await this._poService.GetgatepassList();
		}

		[HttpGet("getUserdashboarddata")]
		public async Task<UserDashboardDetail> getdashdata(string empno)
		{
			return await this._poService.getUserDashboarddata(empno);
		}

		[HttpGet("nonreturngetgatepasslist")]
		public async Task<IEnumerable<gatepassModel>> nonreturngetgatepasslist(string type)
		{
			return await this._poService.NonreturnGetgatepassList(type);
		}

		[HttpGet("outwardinwardreport")]
		public async Task<IEnumerable<outwardinwardreportModel>> getoutwardinwardreport()
		{
			return await this._poService.outingatepassreport();
		}

		[HttpPost("saveoreditgatepassmaterial")]
		public int saveorupdate([FromBody] gatepassModel obj)
		{
			return this._poService.SaveOrUpdateGatepassDetails(obj);
		}
		[HttpGet("checkmaterialandqty")]
		public string check(string material = null, decimal? qty = 0)
		{
			return this._poService.checkmaterialandqty(material, qty);
		}

		[HttpGet("getstockdetails")]
		public stockCardPrint getstockdetails(string pono, string materialid)
		{
			return this._poService.getstockdetails(pono, materialid);

		}
		[HttpDelete("deletegatepassmaterial")]
		public int deletematerial(int gatepassmaterialid)
		{
			return this._poService.deletegatepassmaterial(gatepassmaterialid);
		}
		[HttpPost("updategatepassapproverstatus")]
		public int gatepassapproverstatus([FromBody] List<gatepassModel> model)
		{
			return this._poService.updategatepassapproverstatus(model);
		}
		[HttpGet("getmaterialdetailsbygatepassid")]
		public async Task<IEnumerable<gatepassModel>> gatepassmaterialdetail(string gatepassid)
		{
			return await this._poService.GetmaterialList(gatepassid);
		}

		//Check material exists or not
		[HttpGet("checkMatExists")]
		public string checkMatExists(string material)
		{
			return this._poService.checkMatExists(material);
		}

		[HttpGet("getGatePassApprovalHistoryList")]
		public async Task<IEnumerable<gatepassapprovalsModel>> getGatePassApprovalHistoryList(string gatepassid)
		{
			return await this._poService.getGatePassApprovalHistoryList(gatepassid);
		}


		[HttpPost("updateprintstatus")]
		public int updateprintstatus(gatepassModel model)
		{
			return this._poService.updateprintstatus(model);
		}
		[HttpGet("updatereprintstatus")]
		public int updatereprintstatus(reprintModel model)
		{
			return this._poService.updatereprintstatus(model);
		}
		[HttpGet("GetreportBasedCategory")]
		public async Task<IEnumerable<ReportModel>> getcategorylistById(int categoryid = 0)
		{
			return await this._poService.GetreportBasedCategory(categoryid);
		}
		[HttpGet("GetreportBasedmaterailid")]
		public async Task<IEnumerable<ReportModel>> getcategorylistbymaterialid(string material)
		{
			return await this._poService.GetreportBasedMaterial(material);
		}
		[HttpPost("updateABCRange")]
		public int updateABCrange([FromBody] List<ABCCategoryModel> data)
		{
			return this._poService.updateABCcategorydata(data);
		}
		[HttpGet("getcategorymasterdata")]
		public async Task<IEnumerable<ABCCategoryModel>> getcategorydata()
		{
			return await this._poService.GetABCCategorydata();
		}
		[HttpGet("getABCavailableqtyList")]
		public async Task<IEnumerable<ReportModel>> getabcavailableqtylist()
		{
			return await this._poService.GetABCavailableqtyList();
		}
		[HttpGet("getCyclecountList")]
		public async Task<IEnumerable<CycleCountList>> getCyclecountList(int limita, int limitb, int limitc)
		{
			return await this._poService.GetCyclecountList(limita, limitb, limitc);
		}

		[HttpGet("getCyclecountPendingList")]
		public async Task<IEnumerable<CycleCountList>> getCyclecountPendingList()
		{
			return await this._poService.GetCyclecountPendingList();
		}

		[HttpGet("getCyclecountconfig")]
		public async Task<Cyclecountconfig> getCyclecountconfig()
		{
			return await this._poService.GetCyclecountConfig();
		}

		[HttpPost("updateCyclecountconfig")]
		public int updateCyclecountconfig(Cyclecountconfig dataobj)
		{
			return this._poService.UpdateCycleCountconfig(dataobj);
		}


		[HttpPost("updateinsertCyclecount")]
		public int updateinsertCyclecount([FromBody] List<CycleCountList> data)
		{
			return this._poService.UpdateinsertCycleCount(data);
		}

		[HttpGet("GetABCListBycategory")]
		public async Task<IEnumerable<ReportModel>> getabclist(string category)
		{
			return await this._poService.GetABCListBycategory(category);

		}
		[HttpGet("GetFIFOList")]
		public async Task<IEnumerable<FIFOModel>> getFIFOlist(string material = null)
		{
			return await this._poService.GetFIFOList(material);
		}
		[HttpGet("Checkoldestmaterial")]
		public ReportModel Oldestmaterial(string material, string createddate)
		{

			return this._poService.checkloldestmaterial(material, createddate);
		}
		[HttpGet("Checkoldestmaterialwithdesc")]
		public ReportModel Oldestmaterialwithdesc(string material, string createddate, string description)
		{

			return this._poService.checkoldmaterialwithdesc(material, createddate, description);
		}
		[HttpGet("Checkoldestmaterialwithdescstore")]
		public ReportModel Oldestmaterialwithdescstore(string material, string createddate, string description, string store)
		{

			return this._poService.checkoldmaterialwithdescstore(material, createddate, description, store);
		}
		[HttpPost("updateFIFOIssueddata")]
		public int Oldestmaterial([FromBody] List<FIFOModel> model)
		{

			return this._poService.FIFOitemsupdate(model);
		}

		/// <summary>
		/// get list of todays expected shipments
		/// Ramesh
		/// </summary>
		/// <param name="deliverydate"></param>
		/// <returns></returns>
		[HttpGet("getASNList")]
		public Task<IEnumerable<OpenPoModel>> getASNList(string deliverydate)
		{

			return this._poService.getASNList(deliverydate);
		}

		[HttpGet("getASNListdata")]
		public Task<IEnumerable<OpenPoModel>> getASNListdata()
		{

			return this._poService.getASNListdata();
		}

		[HttpGet("GetItemLocationListByMaterial")]
		public async Task<IEnumerable<IssueRequestModel>> getitemlocationBymaterial(string material)
		{
			return await this._poService.GetItemlocationListBymterial(material);
		}
		[HttpGet("GetItemLocationListByMaterialanddesc")]
		public async Task<IEnumerable<IssueRequestModel>> getitemlocationBymaterialanddesc(string material, string description)
		{

			return await this._poService.GetItemlocationListBymterialanddesc(material, description);
		}
		[HttpGet("GetItemLocationforplantstock")]
		public async Task<IEnumerable<IssueRequestModel>> GetItemLocationforplantstock(string material, string description)
		{

			return await this._poService.GetItemLocationforplantstock(material, description);
		}
		[HttpGet("GetItemLocationforplosstock")]
		public async Task<IEnumerable<IssueRequestModel>> GetItemLocationforplosstock(string material, string description)
		{

			return await this._poService.GetItemLocationforplosstock(material, description);
		}
		[HttpGet("GetItemLocationListByMaterialanddescpo")]
		public async Task<IEnumerable<IssueRequestModel>> getitemlocationBymaterialanddescpo(string material, string description,string projectid,string pono)
		{

			return await this._poService.GetItemlocationListBymterialanddescpo(material, description, projectid, pono);
		}
		[HttpGet("GetItemLocationwithStore")]
		public async Task<IEnumerable<IssueRequestModel>> GetItemLocationwithStore(string material, string description, string projectid, string pono)
		{

			return await this._poService.GetItemlocationwithStore(material, description, projectid, pono);
		}
		[HttpGet("GetItemLocationListByMaterialdescstore_v1")]
		public async Task<IEnumerable<IssueRequestModel>> GetItemLocationwithStore_V1(string material, string description, string store, string projectid, string pono)
		{

			return await this._poService.GetItemLocationListByMaterialdescstore_v1(material, description, store, projectid, pono);
		}
		[HttpGet("GetItemLocationListByMaterialdescstore")]
		public async Task<IEnumerable<IssueRequestModel>> GetItemLocationListByMaterialdescstore(string material, string description, string store, string projectid, string pono)
		{

			return await this._poService.GetItemLocationListByMaterialdescstore(material, description, store, projectid, pono);
		}
		[HttpGet("GetItemLocationListByMaterialdescpono")]
		public async Task<IEnumerable<IssueRequestModel>> GetItemLocationListByMaterialdescpono(string material, string description, string pono)
		{

			return await this._poService.GetItemLocationListByMaterialdescpono(material, description, pono);
		}
		[HttpGet("GetItemLocationListByMaterialsourcelocation")]
		public async Task<IEnumerable<IssueRequestModel>> getitemlocationBymaterialsourcelocation(string material, string description)
		{

			return await this._poService.GetItemlocationListBymterialsourcelocation(material, description);
		}
		[HttpGet("getItemlocationListByIssueId")]
		public async Task<IEnumerable<IssueRequestModel>> getItemlocationListByIssueId(string requestforissueid, string requesttype)
		{

			return await this._poService.getItemlocationListByIssueId(requestforissueid, requesttype);
		}
		[HttpGet("getItemlocationListByIssueIdWithStore")]
		public async Task<IEnumerable<IssueRequestModel>> getItemlocationListByIssueIdWithStore(string requestforissueid, string requesttype)
		{
			return await this._poService.getItemlocationListByIssueIdWithStore(requestforissueid, requesttype);
		}
		[HttpGet("getItemlocationListByPlantIssueId")]
		public async Task<IEnumerable<IssueRequestModel>> getItemlocationListByPlantIssueId(string requestforissueid, string requesttype)
		{

			return await this._poService.getItemlocationListByPlantIssueId(requestforissueid, requesttype);
		}
		[HttpGet("getItemlocationListByPlosIssueId")]
		public async Task<IEnumerable<IssueRequestModel>> getItemlocationListByPlosIssueId(string requestforissueid, string requesttype)
		{

			return await this._poService.getItemlocationListByPlosIssueId(requestforissueid, requesttype);
		}

		[HttpGet("getItemlocationListByGatepassmaterialid")]
		public async Task<IEnumerable<IssueRequestModel>> getItemlocationListByGatepassmaterialid(string gatepassmaterialid)
		{

			return await this._poService.getItemlocationListByGatepassmaterialid(gatepassmaterialid);
		}
		[HttpPost("updateMaterialavailabality")]
		public int updateMaterialavailabality([FromBody]List<IssueRequestModel> model)
		{

			return this._poService.updateissuedmaterial(model);
		}

		[HttpPost("assignRole")]
		public int assignRole([FromBody] authUser authuser)
		{

			return this._poService.assignRole(authuser);
		}

		[HttpGet("getuserAcessList")]
		public async Task<IEnumerable<userAcessNamesModel>> getuserAcessList(string employeeid, string roleid)
		{

			return await this._poService.getuserAcessList(employeeid, roleid);
		}

		[HttpGet("getuserroleList")]
		public async Task<IEnumerable<userAcessNamesModel>> getuserroleList(string employeeid)
		{

			return await this._poService.getuserroleList(employeeid);
		}

		[HttpGet("getUserdashgraphdata")]
		public async Task<IEnumerable<UserDashboardGraphModel>> getUserdashgraphdata(string employeeid)
		{

			return await this._poService.getUserdashboardgraphdata();
		}
		[HttpGet("getUserdashIEgraphdata")]
		public async Task<IEnumerable<UserDashboardGraphModel>> getUserdashIEgraphdata(string employeeid)
		{

			return await this._poService.getUserdashIEgraphdata();
		}

		[HttpPost("getManagerdashboardgraphdata")]
		public async Task<ManagerDashboard> getManagerdashboardgraphdata( DashBoardFilters filters)
		{
			return await this._poService.getManagerdashboardgraphdata(filters);

		}

		[HttpPost("getweeklyUserdashgraphdata")]
		public async Task<IEnumerable<UserDashboardGraphModel>> getWeeklyUserdashboardgraphdata(DashBoardFilters filters)
		{
			return await this._poService.getWeeklyUserdashboardgraphdata(filters);
		}

		[HttpPost("getmonthlyUserdashgraphdata")]
		public async Task<IEnumerable<UserDashboardGraphModel>> getmonthlyUserdashboardgraphdata(DashBoardFilters filters)
		{
			return await this._poService.getmonthlyUserdashboardgraphdata(filters);
		}
		[HttpGet("Getpagesbyrole")]
		public async Task<IEnumerable<pageModel>> Getpagesbyrole(int roleid)
		{
			return await this._poService.Getpagesbyroleid(roleid);
		}


		[HttpGet("getPODataList/{suppliername}")]
		public async Task<IEnumerable<POList>> getPODataList(string suppliername)
		{
			return await this._poService.getPODataList(suppliername);
		}

		[HttpGet("Getpages")]
		public async Task<IEnumerable<pageModel>> Getpages()
		{
			return await this._poService.Getpages();
		}

		[HttpGet("getEnquirydata")]
		public async Task<Enquirydata> getEnquirydata(string materialid)
		{
			return await this._poService.GetEnquirydata(materialid);
		}

		[HttpGet("getdashboarddata")]
		public async Task<DashboardModel> getdashboarddata()
		{
			return await this._poService.getdashboarddata();
		}
		[HttpGet("getmaterialrequestListdata")]
		public async Task<IEnumerable<IssueRequestModel>> getmaterialrequestListdata(string pono = null, string loginid = null, string projectcode = null)
		{
			return await this._poService.MaterialRequestdata(pono, loginid, projectcode);
		}
		[HttpGet("getmaterialrequestforgatepass")]
		public async Task<IEnumerable<IssueRequestModel>> getmaterialrequestforgatepass(string pono, string projectcode)
		{
			return await this._poService.MaterialRequestdataforgatepass(pono, projectcode);
		}

		[HttpGet("getmaterialswithstore")]
		public async Task<IEnumerable<IssueRequestModel>> getmaterialswithstore(string pono, string projectcode)
		{
			return await this._poService.getmaterialswithstore(pono, projectcode);
		}
		[HttpGet("getmaterialrequestforsto")]
		public async Task<IEnumerable<IssueRequestModel>> getmaterialrequestforsto(string pono, string projectcode,string store)
		{
			return await this._poService.MaterialRequestdataforsto(pono, projectcode, store);
		}
		[HttpGet("getmaterialrequestforsto_v1")]
		public async Task<IEnumerable<IssueRequestModel>> getmaterialrequestforsto_v1(string pono, string projectcode, string store)
		{
			return await this._poService.MaterialRequestdataforsto_v1(pono, projectcode, store);
		}

		[HttpGet("getmaterialreserveListdata")]
		public async Task<IEnumerable<IssueRequestModel>> getmaterialreserveListdata(string projectcode)
		{
			return await this._poService.MaterialReservedata(projectcode);
		}

		[HttpGet("getmaterialreserveListdata_v1")]
		public async Task<IEnumerable<IssueRequestModel>> getmaterialreserveListdata_v1()
		{
			return await this._poService.MaterialReservedata_v1();
		}

		[HttpGet("getgatepassmaterialrequestList")]
		public async Task<IEnumerable<IssueRequestModel>> getgatepassmaterialrequestList()
		{
			return await this._poService.getgatepassmaterialrequestList();
		}

		[HttpGet("getempnamebycode")]
		public async Task<User> getempnamebycode(string empno)
		{
			return await this._poService.getempnamebycode(empno);
		}

		[HttpGet("getmaterialreturnreqList")]
		public async Task<IEnumerable<IssueRequestModel>> getmaterialreturnreqList(string matreturnid)
		{
			return await this._poService.getmaterialreturnreqList(matreturnid);
		}

		[HttpGet("getmaterialissueList")]
		public async Task<IEnumerable<IssueRequestModel>> getmaterialissueList(int requestid)
		{
			return await this._poService.getissuematerialdetails(requestid);
		}
		[HttpGet("getmaterialissueListforreserved")]
		public async Task<IEnumerable<ReserveMaterialModel>> getmaterialissueListforreserved(int reservedid)
		{
			return await this._poService.getissuematerialdetailsforreserved(reservedid);
		}

		[HttpPost("insertreservematerial")]
		public Task<int> getmaterialissueList([FromBody] List<ReserveMaterialModel> datamodel)
		{
			return this._poService.insertResevematerial(datamodel);
		}
		[HttpGet("GetreserveMaterilalist")]
		public async Task<IEnumerable<MaterialTransaction>> GetReservedMaterialList(string reservedby)
		{
			return await this._poService.GetReservedMaterialList(reservedby);
		}
		[HttpGet("GetreleasedMaterilalist")]
		public async Task<IEnumerable<ReserveMaterialModel>> GetreleasedMaterilalist()
		{
			return await this._poService.GetReleasedmaterialList();
		}
		[HttpGet("Getmaterialdetailsbyreserveid")]
		public async Task<IEnumerable<ReserveMaterialModel>> Getmaterialdetailsbyreserveid(string reserveid)
		{
			return await this._poService.GetmaterialdetailsByreserveid(reserveid);
		}
		[HttpPost("approvematerialrelease")]
		public int approvematerialrelease([FromBody] List<ReserveMaterialModel> data)
		{
			return this._poService.ApproveMaterialRelease(data);

		}
		[HttpPost("ackmaterialreceivedfroreserved")]
		public int ackmaterialreceivedfroreserved([FromBody] List<ReserveMaterialModel> data)
		{
			return this._poService.acknowledgeMaterialReceivedforreserved(data);
		}
		[HttpGet("getSecurityReceivedList")]
		public Task<IEnumerable<SecurityInwardreceivedModel>> getSecurityreceivedList()
		{

			return this._poService.getSecurityreceivedList();
		}
		[HttpGet("getlocationdata")]
		public async Task<IEnumerable<dropdownModel>> getlocationdata()
		{

			return await this._poService.Getlocationdata();
		}
		[HttpGet("getbindata")]
		public async Task<IEnumerable<dropdownModel>> getbindata()
		{

			return await this._poService.Getbindata();
		}
		[HttpGet("Getdestinationlocationforist")]
		public async Task<IEnumerable<dropdownModel>> Getdestinationlocationforist(int store)
		{

			return await this._poService.Getdestinationlocationforist(store);
		}
		[HttpGet("getrackata")]
		public async Task<IEnumerable<dropdownModel>> getrackata()
		{
			return await this._poService.Getrackdata();
		}
		[HttpGet("getbindataforputaway")]
		public async Task<IEnumerable<dropdownModel>> getbindataforputaway()
		{

			return await this._poService.Getbindataforputaway();
		}
		[HttpGet("getrackataforputaway")]
		public async Task<IEnumerable<dropdownModel>> getrackataforputaway()
		{
			return await this._poService.Getrackdataforputaway();
		}

		[HttpGet("GetMaterialdata")]
		public async Task<IEnumerable<Materials>> GetMaterialcombo()
		{
			return await this._poService.GetMaterialcombo();
		}


		[HttpGet("getapproverList")]
		public async Task<IEnumerable<employeeModel>> getapproverList(string empid)
		{
			return await this._poService.getapproverList(empid);
		}
		[HttpGet("getgatepassByapproverList")]
		public async Task<IEnumerable<gatepassModel>> getgatepassByapproverList(string empid)
		{
			return await this._poService.getgatepassByapproverList(empid);
		}

		[HttpPost("GatepassapproveByManager")]
		public int GatepassapproveByManager([FromBody] gatepassModel obj)
		{
			return this._poService.GatepassapproveByManager(obj);
		}

		[HttpPost("GatepassapproveByMail")]
		public int GatepassapproveByMail([FromBody] gatepassModel obj)
		{
			return this._poService.GatepassapproveByMail(obj);
		}

		[HttpGet("getSafteyStockList")]
		public async Task<IEnumerable<safteyStockList>> getSafteyStockList()
		{
			return await this._poService.getSafteyStockList();
		}
		[HttpGet("GetBinList")]
		public async Task<IEnumerable<StockModel>> GetBinList()
		{
			return await this._poService.GetBinList();
		}
		[HttpGet("GetMaterialdatafromstock")]
		public async Task<IEnumerable<Materials>> GetMaterialstockcombo(int store)
		{
			return await this._poService.GetMaterialstockcombo(store);
		}

		[HttpGet("getMaterialforstocktransferorder")]
		public async Task<IEnumerable<Materials>> getMaterialforstocktransferorder()
		{
			return await this._poService.getMaterialforstocktransferorder();
		}

		[HttpGet("getstocktransferdata")]
		public async Task<IEnumerable<stocktransferModel>> getstocktransferlist()
		{
			return await this._poService.getstocktransferdata();
		}

		[HttpGet("getstocktransferdatagroup")]
		public async Task<IEnumerable<stocktransferModel>> getstocktransferlistgroup()
		{
			return await this._poService.getstocktransferdatagroup();
		}

		[HttpGet("getstocktransferdatagroup1")]
		public async Task<IEnumerable<invstocktransfermodel>> getstocktransferlistgroup1(string transfertype,string employeeno)
		{
			return await this._poService.getstocktransferdatagroup1(transfertype,employeeno);
		}

		[HttpGet("getpendingpos")]
		public async Task<IEnumerable<ddlmodel>> getpendingreceiptslist()
		{
			return await this._poService.pendingreceiptslist();
		}
		[HttpGet("getpendingstogr")]
		public async Task<IEnumerable<ddlmodel>> getpendingstogr()
		{
			return await this._poService.pendingstogr();
		}

		[HttpGet("getInitialstockfilename")]
		public async Task<IEnumerable<ddlmodel>> getInitialstockfilename()
		{
			return await this._poService.getInitialstockfilename();
		}


		[HttpGet("getprojectlist")]
		public async Task<IEnumerable<ddlmodel>> getprojectlist()
		{
			return await this._poService.getprojectlist();
		}
		[HttpGet("getprojectlistfortransfer")]
		public async Task<IEnumerable<ddlmodel>> getprojectlistfortransfer()
		{
			return await this._poService.getprojectlistfortransfer();
		}
		[HttpGet("getgatepassreason")]
		public async Task<IEnumerable<ddlmodel>> getgatepassreason()
		{
			return await this._poService.getgatepassreason();
		}
		[HttpGet("getprojectlisttoassign")]
		public async Task<IEnumerable<AssignProjectModel>> getprojectlisttoassign(string empno)
		{
			return await this._poService.getprojectlisttoassign(empno);
		}
		[HttpGet("getstorelist")]
		public async Task<IEnumerable<locataionDetailsStock>> getstorelist()
		{
			return await this._poService.getstorelist();
		}
		[HttpGet("getracklist")]
		public async Task<IEnumerable<locataionDetailsStock>> getracklist()
		{
			return await this._poService.getracklist();
		}
		[HttpGet("getbinlistdata")]
		public async Task<IEnumerable<locataionDetailsStock>> getbinlistdata()
		{
			return await this._poService.getbinlistdata();
		}
		[HttpGet("getprojectlisttoassignpm")]
		public async Task<IEnumerable<assignpmmodel>> getprojectlisttoassignpm()
		{
			return await this._poService.getprojectlisttoassignpm();
		}

		[HttpGet("getprojectlistbymanager")]
		public async Task<IEnumerable<ddlmodel>> getprojectlistbymanager(string empno)
		{
			return await this._poService.getprojectlistbymanager(empno);
		}

		


		[HttpGet("getmateriallistfortransfer")]
		public async Task<IEnumerable<ddlmodel>> getmatlist(string querytext)
		{
			return await this._poService.getmatlist(querytext);
		}

		[HttpGet("getmateriallistbyproject")]
		public async Task<IEnumerable<ddlmodel>> getmatlistbyproject(string projectcode)
		{
			return await this._poService.getmatlistbyproject(projectcode);
		}


		[HttpGet("getgrnforacceptance")]
		public async Task<IEnumerable<ddlmodel>> getgrnlistforacceptance()
		{
			return await this._poService.getgrnlistforacceptance();
		}

		[HttpGet("getgrnforacceptanceputaway")]
		public async Task<IEnumerable<ddlmodel>> getgrnlistforacceptanceputaway()
		{
			return await this._poService.getgrnlistforacceptanceputaway();
		}

		[HttpGet("getuserauthdata")]
		public async Task<IEnumerable<authUser>> getuserauthdata()
		{
			return await this._poService.getuserauthdata();
		}
		[HttpGet("getuserauthdetail")]
		public async Task<IEnumerable<authUser>> getuserauthdetail(string empno)
		{
			return await this._poService.getuserauthdetails(empno);
		}
		[HttpGet("getuserauthdetailbyrole")]
		public async Task<IEnumerable<authUser>> getuserauthdetailbyrole(int roleid)
		{
			return await this._poService.getuserauthdetailsbyrole(roleid);
		}

		[HttpGet("getsubroledata")]
		public async Task<IEnumerable<subrolemodel>> getsubroledata()
		{
			return await this._poService.getsubroledata();
		}

		[HttpGet("getgrnforacceptancenotify")]
		public async Task<IEnumerable<inwardModel>> getgrnlistforacceptancenotify(string type)
		{
			return await this._poService.getgrnlistforacceptancenotify(type);
		}

		[HttpGet("getholdgrs")]
		public async Task<IEnumerable<ddlmodel>> getholdgrslist()
		{
			return await this._poService.getholdgrlist();
		}

		[HttpGet("getgrnforacceptanceqc")]
		public async Task<IEnumerable<ddlmodel>> getgrnlistforacceptanceqc()
		{
			return await this._poService.getgrnlistforacceptanceqc();
		}

		[HttpGet("getgrnforacceptanceqcbydate")]
		public async Task<IEnumerable<ddlmodel>> getgrnlistforacceptanceqcbydate(string fromdt, string todt)
		{
			return await this._poService.getgrnlistforacceptanceqcbydate(fromdt, todt);
		}

		[HttpGet("getnotifedGRbydate")]
		public async Task<IEnumerable<inwardModel>> getnotifiedgrbydate(string fromdt, string todt)
		{
			return await this._poService.getnotifiedgrbydate(fromdt, todt);
		}

		[HttpGet("getdepartment")]
		public async Task<IEnumerable<ddlmodel>> getdepartmentdata()
		{
			return await this._poService.getdepartmentmasterdata();
		}

		[HttpGet("getrbamasterdetail")]
		public async Task<IEnumerable<rbamaster>> getrbamaster()
		{
			return await this._poService.getrbadetails();
		}

		[HttpPost("updategatepassmovement")]
		public int updatematmovement([FromBody] List<materialistModel> obj)
		{
			return this._poService.updatematmovement(obj);
		}

		[HttpPost("requestreservematerial")]
		public int requestreserve([FromBody] materialReservetorequestModel obj)
		{
			return this._poService.requesttoreserve(obj);
		}

		[HttpPost("insertdatacsv")]
		public int insertdatacsv([FromBody] ddlmodel obj)
		{
			return this._poService.insertdatacsv(obj);
		}

		[HttpGet("UpdateMaterialReserve")]
		public int UpdateMaterialReserve()
		{
			return this._poService.UpdateMaterialReserve();
		}
		[HttpPost("UpdateReturnqty")]
		public int UpdateReturnqty([FromBody] List<IssueRequestModel> obj)
		{
			return this._poService.UpdateReturnqty(obj);
		}
		[HttpPost("UpdateReturnmaterialTostock")]
		public int UpdateReturnmaterialTostock([FromBody] List<IssueRequestModel> obj)
		{
			return this._poService.UpdateReturnmaterialTostock(obj);
		}

		[HttpPost("UnholdGR")]
		public int UnholdGRN([FromBody] UnholdGRModel obj)
		{
			return this._poService.UnholdGRdata(obj);
		}

		[HttpPost("mattransfer")]
		public int mattransfer([FromBody] materialtransferMain obj)
		{
			return this._poService.mattransfer(obj);
		}

		[HttpPost("mattransferapproval")]
		public int mattransferapprove([FromBody] List<materialtransferMain> obj)
		{
			return this._poService.mattransferapprove(obj);
		}

		[HttpPost("matrequestapproval")]
		public string matrequestapprove([FromBody] List<MaterialTransaction> obj)
		{
			return this._poService.matrequestapprove(obj);
		}

		[HttpPost("STOmatrequestapproval")]
		public string stomatrequestapprove([FromBody] List<invstocktransfermodel> obj)
		{
			return this._poService.stomatrequestapprove(obj);
		}
		[HttpPost("updaterba")]
		public string updaterba([FromBody] List<rbamaster> obj)
		{
			return this._poService.updaterba(obj);
		}
		[HttpPost("updatepm")]
		public string updatepm([FromBody] List<assignpmmodel> obj)
		{
			return this._poService.updatepm(obj);
		}

		[HttpGet("getMRNmaterials")]
		public async Task<IEnumerable<inwardModel>> getMRNmaterials(string grnnumber)
		{
			return await this._poService.getMRNmaterials(grnnumber);
		}
		[HttpGet("getMRNList")]
		public async Task<IEnumerable<MRNsavemodel>> getMRNList()
		{
			return await this._poService.getmrnlist();
		}
		[HttpPost("mrnupdate")]
		public int mrnupdate([FromBody] List<MRNsavemodel> obj)
		{
			return this._poService.mrnupdate(obj);
		}


		[HttpGet("GetReturnmaterialList")]
		public async Task<IEnumerable<IssueRequestModel>> GetReturnmaterialList()
		{
			return await this._poService.GetReturnmaterialList();
		}
		[HttpGet("getreturnmaterialListforconfirm")]
		public async Task<IEnumerable<IssueRequestModel>> GetReturnmaterialListForConfirm(string requestid)
		{
			return await this._poService.GetReturnmaterialListForConfirm(requestid);
		}
		[HttpGet("getreturndata")]
		public async Task<IEnumerable<MaterialReturn>> getreturndata(string empno)
		{
			return await this._poService.getreturndata(empno);
		}
		[HttpGet("gettransferdata")]
		public async Task<IEnumerable<materialtransferMain>> gettransferdata(string empno)
		{
			return await this._poService.gettransferdata(empno);
		}

		[HttpGet("GetInitialStockPutawayMaterials")]
		public async Task<IEnumerable<initialStock>> GetInitialStockPutawayMaterials()
		{
			return await this._poService.GetInitialStockPutawayMaterials();
		}


		[HttpGet("gettransferdataforapproval")]
		public async Task<IEnumerable<materialtransferMain>> gettransferdataforapproval(string empno)
		{
			return await this._poService.gettransferdataforapproval(empno);
		}

		[HttpGet("getrequestdataforapproval")]
		public async Task<IEnumerable<MaterialTransaction>> getrequestdataforapproval(string empno)
		{
			return await this._poService.getrequestdataforapproval(empno);
		}

		[HttpGet("getrequestdataforSTOapproval")]
		public async Task<IEnumerable<invstocktransfermodel>> getrequestdataforSTOapproval(string empno, string type)
		{
			return await this._poService.getrequestdataforSTOapproval(empno, type);
		}

		[HttpGet("getdirecttransferdata")]
		public async Task<IEnumerable<DirectTransferMain>> getdirecttransferdata(string empno)
		{
			return await this._poService.getdirecttransferdata(empno);
		}

		[HttpGet("STORequestlist")]
		public async Task<IEnumerable<STORequestdata>> STORequestlist()
		{
			return await this._poService.STORequestlist();
		}

		[HttpGet("STORequestdatalist")]
		public async Task<IEnumerable<STOrequestTR>> STORequestdatalist(string transferid)
		{
			return await this._poService.getSTORequestdatalist(transferid);
		}

		[HttpPost("Updatetransferqty")]
		public int Updatetransferqty([FromBody] List<IssueRequestModel> obj)
		{
			return this._poService.Updatetransferqty(obj);
		}

		[HttpGet("gettestcrud")]
		public async Task<IEnumerable<testcrud>> gettestcrud()
		{
			return await this._poService.gettestcrud();
		}

		[HttpPost("getmatinhand")]
		public async Task<IEnumerable<MaterialinHand>> getmatinhand(inventoryFilters filters)
		{
			return await this._poService.getmatinhand(filters);
		}

		[HttpGet("getmatinhandlocation")]
		public async Task<IEnumerable<matlocations>> getmatinhandlocation(string poitemdescription, string materialid, string projectid,string pono,string sono)
		{
			return await this._poService.getmatinhandlocation(poitemdescription, materialid,projectid,pono,sono);
		}
		[HttpGet("getinitialstock")]
		public async Task<IEnumerable<StockModel>> getinitialstock(string code)
		{
			return await this._poService.getinitialstock(code);
		}
		[HttpGet("getinitialstockall")]
		public async Task<IEnumerable<StockModel>> getinitialstockall(string code)
		{
			return await this._poService.getinitialstockall(code);
		}
		[HttpGet("getinitialstockEX")]
		public async Task<IEnumerable<StockModel>> getinitialstockEX(string code)
		{
			return await this._poService.getinitialstockEX(code);
		}
		[HttpGet("getinitialstockReport")]
		public async Task<IEnumerable<StockModel>> getinitialstockReport(string code)
		{
			return await this._poService.getinitialstockReport(code);
		}
		[HttpGet("getinitialstockReportGroup")]
		public async Task<IEnumerable<StockModel>> getinitialstockReportgroup(string code)
		{
			return await this._poService.getinitialstockReportGroup(code);
		}

		[HttpGet("getmateriallabeldata")]
		public async Task<MateriallabelModel> getmateriallabeldata(string pono, int lineitemno, string materialid)
		{
			return await this._poService.getmateriallabeldetail(pono, lineitemno, materialid);
		}
		//Amulya
		[HttpGet("getinitialstockload")]
		public async Task<IEnumerable<StockModel>> getinitialstockload(string code)
		{
			return await this._poService.getinitialstockload(code);
		}

		[HttpPost("postputtestcrud")]
		public string posttestcrud(testcrud data)
		{
			return this._poService.posttestcrud(data);
		}
		[HttpPost("addupdatestore")]
		public string addupdatestore(locataionDetailsStock data)
		{
			return this._poService.addupdatestore(data);
		}
		[HttpPost("addupdaterack")]
		public string addupdaterack(locataionDetailsStock data)
		{
			return this._poService.addupdaterack(data);
		}
		[HttpPost("addupdatebin")]
		public string addupdatebin(locataionDetailsStock data)
		{
			return this._poService.addupdatebin(data);
		}

		[HttpPost("updateUserauth")]
		public string updateUserauth([FromBody] List<authUser> obj)
		{
			return this._poService.updateuserAuth(obj);
		}
		[HttpPost("deleteUserauth")]
		public string deleteUserauth([FromBody] authUser obj)
		{
			return this._poService.deleteuserAuth(obj);
		}
		[HttpPost("deletedeligatePM")]
		public string deletedeligatePM([FromBody] authUser obj)
		{
			return this._poService.deletedeligatePM(obj);
		}
		[HttpDelete("deletetestcurd/{id}")]
		public string deletetestcurd(int id)
		{
			try
			{

				return this._poService.deletetestcurd(id);
			}

			catch (Exception ex)
			{

				Console.WriteLine(ex.Message);

				return ex.Message;
			}
		}

		[HttpPost("getMaterialtransferdetails")]
		public async Task<IEnumerable<materialtransferMain>> getMaterialtransferdetails(materilaTrasFilterParams filters)
		{
			return await this._poService.getMaterialtransferdetails(filters);
		}

		//[HttpPost("securitysendemail")]
		//public EmailModel sendemail(EmailModel obj)
		//{
		//    return this.emailobj.sendEmail(obj,1);
		//}

		//Amulya

		[HttpPost("getmaterialrequestdashboardList")]
		public async Task<IEnumerable<MaterialTransaction>> getmaterialrequestdashboardList(materialRequestFilterParams filters)
		{
			return await this._poService.getmaterialrequestdashboardList(filters);
		}

		//Amulya

		[HttpPost("getmaterialreservedashboardList")]
		public async Task<IEnumerable<materialreserveMain>> getmaterialreservedashboardList(materialResFilterParams filters)
		{
			return await this._poService.getmaterialreservedashboardList(filters);
		}

		//Amulya

		[HttpPost("getMaterialReturnDashboardlist")]
		public async Task<IEnumerable<materialreturnMain>> getMaterialReturnDashboardlist(materialRetFilterParams filters)
		{
			return await this._poService.getmaterialreturndashboardlist(filters);
		}
		[HttpGet("getGRListdata")]
		public Task<IEnumerable<grReports>> getGRListdata()
		{

			return this._poService.getGRListdata();
		}

		[HttpPost("SAPGREditReport")]
		public string SAPGREditReport(grReports data)
		{
			return this._poService.EditReports(data);
		}

		[HttpGet("addEditReports")]
		public async Task<IEnumerable<grReports>> addEditReports(string wmsgr)
		{
			return await this._poService.addEditReports(wmsgr);
		}

		[HttpPost("getPMdashboarddata")]
		public async Task<pmDashboardCards> getPMdashboarddata(DashBoardFilters filters)
		{
			return await this._poService.getPMdashboarddata(filters);

		}
		[HttpGet("getmonthlyUserdashboardIEgraphdata")]
		public async Task<IEnumerable<UserDashboardGraphModel>> getmonthlyUserdashboardIEgraphdata()
		{
			return await this._poService.getmonthlyUserdashboardIEgraphdata();
		}

		[HttpPost("getInvdashboarddata")]
		public async Task<invDashboardCards> getInvdashboarddata(DashBoardFilters filters)
		{
			return await this._poService.getInvdashboarddata(filters);

		}
		[HttpGet("getUserdashboardgraphPMdata")]
		public async Task<IEnumerable<UserDashboardGraphModel>> getUserdashboardgraphPMdata(string employeeid)
		{

			return await this._poService.getUserdashboardgraphPMdata();
		}

		[HttpPost("getReceivedgraph")]
		public async Task<IEnumerable<GraphModelNew>> getWeeklyUserdashboardReceive(DashBoardFilters filters)
		{

			return await this._poService.getWeeklyUserdashboardReceive(filters);
		}

		[HttpPost("getQualitygraph")]
		public async Task<IEnumerable<GraphModelNew>> getWeeklyUserdashboardQuality(DashBoardFilters filters)
		{

			return await this._poService.getWeeklyUserdashboardQuality(filters);
		}

		[HttpPost("getAcceptgraph")]
		public async Task<IEnumerable<GraphModelNew>> getWeeklyUserdashboardAccept(DashBoardFilters filters)
		{

			return await this._poService.getWeeklyUserdashboardAccept(filters);
		}
		[HttpPost("getPutawaygraph")]
		public async Task<IEnumerable<GraphModelNew>> getWeeklyUserdashboardPutaway(DashBoardFilters filters)
		{

			return await this._poService.getWeeklyUserdashboardPutaway( filters);
		}

		[HttpPost("getRequestgraph")]
		public async Task<IEnumerable<GraphModelNew>> getWeeklyUserdashboardRequest(DashBoardFilters filters)
		{

			return await this._poService.getWeeklyUserdashboardRequest(filters);
		}

		[HttpPost("getReturngraph")]
		public async Task<IEnumerable<GraphModelNew>> getWeeklyUserdashboardReturn(DashBoardFilters filters)
		{

			return await this._poService.getWeeklyUserdashboardReturn(filters);
		}

		[HttpPost("getReservegraph")]
		public async Task<IEnumerable<GraphModelNew>> getWeeklyUserdashboardReserve(DashBoardFilters filters)
		{

			return await this._poService.getWeeklyUserdashboardReserve(filters);
		}

		[HttpPost("getTransfergraph")]
		public async Task<IEnumerable<GraphModelNew>> getWeeklyUserdashboardtransfer(DashBoardFilters filters)
		{

			return await this._poService.getWeeklyUserdashboardtransfer(filters);
		}

		[HttpGet("getMiscellanousIssueList/{initialstock}")]
		public async Task<IEnumerable<StockModel>> getMiscellanousIssueList(bool initialstock)
		{
			return await this._poService.getMiscellanousIssueList(initialstock);
		}

		[HttpGet("getMiscellanousIssueListdata")]
		public async Task<IEnumerable<StockModel>> getMiscellanousIssueListdata(string initialstock, string pono, string projectid)
		{
			return await this._poService.getMiscellanousIssueListdata(initialstock,pono,projectid);
		}
		[HttpGet("getMiscellanousIssueListdatanofilter")]
		public async Task<IEnumerable<StockModel>> getMiscellanousIssueListdatanofilter()
		{
			return await this._poService.getMiscellanousIssueListdatanofilter();
		}

		[HttpGet("getMiscellanousIssueListdatahistory")]
		public async Task<IEnumerable<StockModel>> getMiscellanousIssueListdatahistory()
		{
			return await this._poService.getMiscellanousIssueListdatahistory();
		}

		[HttpGet("getMiscellanousReceiptsList")]
		public async Task<IEnumerable<StockModel>> getMiscellanousReceiptsList()
		{
			return await this._poService.getMiscellanousReceiptsList();
		}
		[HttpPost("miscellanousIssueDataUpdate")]
		public bool miscellanousIssueDataUpdate(miscellanousIssueData misData)
		{
			return this._poService.miscellanousIssueDataUpdate(misData);

		}

		[HttpPost("updatestocklocationdata")]
		public string updatelocationdata(matlocations misData)
		{
			return this._poService.updatelocationdata(misData);

		}
		[HttpPost("multiplemiscellanousIssueDataUpdate")]
		public string multiplemiscellanousIssueDataUpdate(List<miscellanousIssueData> misData)
		{
			return this._poService.multiplemiscellanousIssueDataUpdate(misData);

		}
		[HttpPost("updateMiscellanousReceipt")]
		public string updateMiscellanousReceipt(StockModel data)
		{
			return this._poService.updateMiscellanousReceipt(data);

		}
		[HttpGet("getMaterialMasterList")]
		public IActionResult getMaterialMasterList()
		{
			return Ok(this._poService.getMaterialMasterList());
		}

		[HttpPost("updateMaterialMaster")]
		public bool updateMaterialMaster(materilaMasterYgs data)
		{
			return this._poService.updateMaterialMaster(data);

		}
		[HttpPost("GPReasonMTAdd")]
		public string GPReasonMTAdd(GPReasonMTData data)
		{
			return this._poService.GPReasonMTAdd(data);

		}

		[HttpPost("MiscellanousReasonAdd")]
		public string MiscellanousReasonAdd(GPReasonMTData data)
		{
			return this._poService.MiscellanousReasonAdd(data);

		}
		//[HttpPost("GPReasonMTAdd")]
		//public string GPReasonMTAdd(GPReasonMTData data)
		//{
		//	return this._poService.GPReasonMTAdd(data);

		//}

		[HttpPost("createplant")]
		public string createplant(PlantMTdata data)
		{
			return this._poService.createplant(data);

		}

		[HttpPost("GPReasonMTDelete")]
		public string GPReasonMTDelete(GPReasonMTData data)
		{
			return this._poService.GPReasonMTDelete(data);

		}

		[HttpGet("getGPReasonData")]
		public async Task<IEnumerable<GPReasonMTData>> getGPReasonData()
		{
			return await this._poService.getGPReasonData();

		}

		[HttpGet("getMiscellanousReasonData")]
		public async Task<IEnumerable<GPReasonMTData>> getMiscellanousReasonData()
		{
			return await this._poService.getMiscellanousReasonData();

		}

		[HttpPost("PlantnameDelete")]
		public string PlantnameDelete(PlantMTdata data)
		{
			return this._poService.PlantnameDelete(data);

		}

		[HttpGet("getplantnameData")]
		public async Task<IEnumerable<PlantMTdata>> getplantnameData()
		{
			return await this._poService.getplantnameData();

		}

		[HttpGet("getSTORequestList")]
		public async Task<IEnumerable<invstocktransfermodel>> getSTORequestList(string type)
		{
			return await this._poService.getSTORequestList(type);
		}

		[HttpGet("getAvailableQtyBystore")]
		public async Task<WMSHttpResponse> getAvailableQtyBystore(string store, string materialid, string description, string projectcode)
		{
			return await this._poService.getAvailableQtyBystore(store, materialid, description, projectcode);
		}

		[HttpGet("getplantstockmaterialdetails")]
		public async Task<IssueRequestModel> getplantstockmaterialdetails(string material, string description)
		{
			return await this._poService.getplantstockmaterialdetails(material, description);
		}
		[HttpGet("getplosstockmaterialdetails")]
		public async Task<IssueRequestModel> getplosstockmaterialdetails(string material, string description)
		{
			return await this._poService.getplosstockmaterialdetails(material, description);
		}

		[HttpGet("getMatdetailsbyTransferId")]
		public async Task<IEnumerable<STOIssueModel>> getMatdetailsbyTransferId(string transferid, string type, string transfertype)
		{
			return await this._poService.getMatdetailsbyTransferId(transferid, type, transfertype);
		}

		[HttpPost("STOPOInitiate")]
		public async Task<string> STOPOInitiate(List<STOIssueModel> data)
		{
			return await this._poService.STOPOInitiate(data);
		}
		[HttpGet("generateLabel")]
		public string generateLabel(string labeldata)
		{
			return this._poService.generateLabel(labeldata);

		}

		[HttpGet("getplantlocdetails")]
		public async Task<IEnumerable<plantddl>> getplantlocdetails()
		{

			return await this._poService.getplantlocdetails();
		}
		[HttpPost("updateSubcontractAcKstatus")]
		public async Task<string> updateSubcontractAcKstatus(List<invstocktransfermodel> data)
		{
			return await this._poService.updateSubcontractAcKstatus(data);
		}
		[HttpGet("subcontractInoutList")]
		public async Task<IEnumerable<gatepassModel>> subcontractInoutList()
		{
			return await this._poService.subcontractInoutList();
		}
		[HttpPost("generateqronhold")]
		public printonholdGR generateqronhold(printonholdGR onholdprintdata)
		{
			return this._poService.generateqronhold(onholdprintdata);

		}

		[HttpPost("updateVendorMaster")]
		public bool updateVendorMaster(vendorMaster vendormaster)
		{
			return this._poService.updateVendorMaster(vendormaster);
		}
		[HttpPost("updateRole")]
		public bool updateRole(roleMaster rolemaster)
		{
			return this._poService.updateRole(rolemaster);
		}
		[HttpPost("updateSubRole")]
		public bool updateSubRole(subrolemodel subrolemaster)
		{
			return this._poService.updateSubRole(subrolemaster);
		}
		[HttpPost("updateUserRole")]
		public bool updateUserRole(userRoles userRole)
		{
			return this._poService.updateUserRole(userRole);
		}

		[HttpGet("getDDdetailsByPono")]
		public async Task<IEnumerable<DDmaterials>> getDDdetailsByPono(string PONO)
		{
			return await this._poService.getDDdetailsByPono(PONO);
		}
		[HttpPost("updateDirectDelivery")]
		public bool updateDirectDelivery(DirectDelivery userRole)
		{
			return this._poService.updateDirectDelivery(userRole);
		}

		[HttpGet("deleteDirectDelivery")]
		public bool deleteDirectDelivery(string inwmasterid, string deletedby)
		{
			return  this._poService.deleteDirectDelivery(inwmasterid, deletedby);
		}

		[HttpGet("GetYGSGRList")]
		public async Task<IEnumerable<YGSGR>> GetYGSGRList()
		{
			return await this._poService.getYGSGR();
		}
		[HttpGet("GetGPReport")]
		public async Task<IEnumerable<gatepassModel>> GetGPReport(string fromdate,string todate)
		{
			return await this._poService.GetGPReport(fromdate, todate);
		}
		[HttpPost("IssuerStatusChange")]
		public bool IssuerStatusChange(Issuestatus model)
		{
			return this._poService.IssuerStatusChange(model);
		}
	}

}
