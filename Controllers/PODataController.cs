using System;
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
		public async Task<IEnumerable<MaterialDetails>> getMaterialDetails(string grnNo)
		{
			return await this._poService.getMaterialDetails(grnNo);
		}

		//Get Location details for material
		[HttpGet("getlocationdetailsformaterialid")]
		public async Task<IEnumerable<LocationDetails>> getlocationdetails(string materialid, string grnnumber)
		{
			return await this._poService.getlocationdetails(materialid, grnnumber);
		}

		//Get material request details
		[HttpGet("getReqMatdetailsformaterialid")]
		public async Task<IEnumerable<ReqMatDetails>> getReqMatdetails(string materialid,string grnnumber)
		{
			return await this._poService.getReqMatdetails(materialid, grnnumber);
		}

		[HttpPost("generateBarcodeMaterial")]
		public printMaterial generateBarcodeMaterial(printMaterial printMat)
        {
			return this._poService.generateBarcodeMaterial(printMat);

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

                    //if(fileName.StartsWith("putaway")){
					//	this._poService.updateputawayfilename(fileName);
					//}

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
		public string insertbardata(BarcodeModel data)
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
		public async Task<IEnumerable<OpenPoModel>> Getdetailsforthreewaymatching(string pono)
		{
			bool isgrn = false;
			string grn = "";
			string po = pono;
			string[] ponoandinvoice = pono.Split('-');
			if (ponoandinvoice.Length > 2)
			{
				isgrn = true;
				grn = po;
			}
			string ponodata = ponoandinvoice[0].Trim();
			string invoiceno = ponoandinvoice[1].Trim();
			return await this._poService.GetDeatilsForthreeWaymatching(invoiceno, ponodata, isgrn, grn);
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
		public async Task<OpenPoModel> verifythreewaymatching(string pono)
		{
			string[] ponoandinvoice = pono.Split('-');
			string ponodata = ponoandinvoice[0];
			string invoiceno = ponoandinvoice[1];
			return await this._poService.VerifythreeWay(ponodata, invoiceno);
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

		[HttpGet("getitemdetailsbygrnnonotif")]
		public async Task<IEnumerable<inwardModel>> getitemdetailsbygrnnonotif(string grnnumber)
		{
			return await this._poService.getitemdeatilsnotif(grnnumber);
		}
		[HttpGet("getmaterialrequestList")]
		public async Task<IEnumerable<IssueRequestModel>> materialissue(string pono = null, string loginid = null)
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
		public Task<IEnumerable<PODetails>> getPODetails()
        {
			return this._poService.getPODetails();

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

		[HttpPost("saveoreditgatepassmaterial")]
		public int saveorupdate([FromBody] gatepassModel obj)
		{
			return this._poService.SaveOrUpdateGatepassDetails(obj);
		}
		[HttpGet("checkmaterialandqty")]
		public string check(string material = null, int qty = 0)
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
		public async Task<IEnumerable<gatepassModel>> gatepassmaterialdetail(int gatepassid)
		{
			return await this._poService.GetmaterialList(gatepassid);
		}

		//Check material exists or not
		[HttpGet("checkMatExists")]
		public string checkMatExists(string material)
        {
			return  this._poService.checkMatExists(material);
		}

		[HttpGet("getGatePassApprovalHistoryList")]
		public async Task<IEnumerable<gatepassapprovalsModel>> getGatePassApprovalHistoryList(int gatepassid)
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
		[HttpPost("updateFIFOIssueddata")]
		public int Oldestmaterial([FromBody] List<FIFOModel> model)
		{

			return this._poService.FIFOitemsupdate(model);
		}
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

		[HttpGet("getweeklyUserdashgraphdata")]
		public async Task<IEnumerable<UserDashboardGraphModel>> getWeeklyUserdashboardgraphdata()
		{
			return await this._poService.getWeeklyUserdashboardgraphdata();
		}
		
		[HttpGet("getmonthlyUserdashgraphdata")]
		public async Task<IEnumerable<UserDashboardGraphModel>> getmonthlyUserdashboardgraphdata()
		{
			return await this._poService.getmonthlyUserdashboardgraphdata();
		}
		[HttpGet("Getpagesbyrole")]
		public async Task<IEnumerable<pageModel>> Getpagesbyrole(int roleid)
		{
			return await this._poService.Getpagesbyroleid(roleid);
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
		public async Task<IEnumerable<IssueRequestModel>> getmaterialrequestListdata(string pono = null, string loginid = null)
		{
			return await this._poService.MaterialRequestdata(pono, loginid);
		}

		[HttpGet("getmaterialreturnreqList")]
		public async Task<IEnumerable<IssueRequestModel>> getmaterialreturnreqList(int matreturnid)
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
		public async Task<IEnumerable<ReserveMaterialModel>> GetReservedMaterialList(string reservedby)
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
		[HttpGet("getrackata")]
		public async Task<IEnumerable<dropdownModel>> getrackata()
		{
			return await this._poService.Getrackdata();
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
		public async Task<IEnumerable<Materials>> GetMaterialstockcombo()
		{
			return await this._poService.GetMaterialstockcombo();
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
		public async Task<IEnumerable<invstocktransfermodel>> getstocktransferlistgroup1()
		{
			return await this._poService.getstocktransferdatagroup1();
		}

		[HttpGet("getpendingpos")]
		public async Task<IEnumerable<ddlmodel>> getpendingreceiptslist()
		{
			return await this._poService.pendingreceiptslist();
		}

        [HttpGet("getprojectlist")]
        public async Task<IEnumerable<ddlmodel>> getprojectlist()
        {
            return await this._poService.getprojectlist();
        }

		[HttpGet("getmateriallistfortransfer")]
		public async Task<IEnumerable<ddlmodel>> getmatlist(string empno)
		{
			return await this._poService.getmatlist(empno);
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
		public async Task<IEnumerable<ddlmodel>> getgrnlistforacceptanceqcbydate(string fromdt,string todt)
		{
			return await this._poService.getgrnlistforacceptanceqcbydate(fromdt, todt);
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

		[HttpGet("UpdateMaterialReserve")]
		public int UpdateMaterialReserve()
		{
			return  this._poService.UpdateMaterialReserve();
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

		[HttpPost("mrnupdate")]
		public int mrnupdate([FromBody] MRNsavemodel obj)
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
		public async Task<IEnumerable<IssueRequestModel>> getreturndata(string empno)
		{
			return await this._poService.getreturndata(empno);
		}
		[HttpGet("gettransferdata")]
		public async Task<IEnumerable<materialtransferMain>> gettransferdata(string empno)
		{
			return await this._poService.gettransferdata(empno);
		}
		[HttpPost("Updatetransferqty")]
		public int Updatetransferqty([FromBody] List<IssueRequestModel> obj)
		{
			return this._poService.Updatetransferqty(obj);
		}
		//[HttpPost("securitysendemail")]
		//public EmailModel sendemail(EmailModel obj)
		//{
		//    return this.emailobj.sendEmail(obj,1);
		//}

	}
}
