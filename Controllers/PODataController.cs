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
        public async Task<IEnumerable<POList>> GetPoNo()
        {
            return await this._poService.getPOList();
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
		public async Task<IEnumerable<LocationDetails>> getlocationdetails(string materialid)
		{
			return await this._poService.getlocationdetails(materialid);
		}

		//Get material request details
		[HttpGet("getReqMatdetailsformaterialid")]
		public async Task<IEnumerable<ReqMatDetails>> getReqMatdetails(string materialid)
		{
			return await this._poService.getReqMatdetails(materialid);
		}

		[HttpPost("insertbarcodeandinvoiceinfo")]
		public int insertbardata(BarcodeModel data)
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

			string[] ponoandinvoice = pono.Split('-');
			string ponodata = ponoandinvoice[0];
			string invoiceno = ponoandinvoice[1];
			return await this._poService.GetDeatilsForthreeWaymatching(invoiceno, ponodata);
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

		[HttpPost("updateitemlocation")]
		public string insertitemdataTostock(StockModel data)
		{
			return this._poService.InsertStock(data);
		}

		[HttpPost("GetListItems")]
		public IActionResult GetListItems([FromBody] DynamicSearchResult Result)
		{
			return Ok(this._poService.GetListItems(Result));
		}
		//not using
		[HttpPost("issuerequest")]
		public IActionResult issuerequest([FromBody] List<IssueRequestModel> model)
		{
			return Ok(this._poService.IssueRequest(model));
		}
		//not usinggetMaterialRequestlist
		//list of items
		[HttpGet("getitemdetailsbygrnno")]
		public async Task<IEnumerable<inwardModel>> getitemdetailsbygrnno(string grnnumber)
		{
			return await this._poService.getitemdeatils(grnnumber);
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
		public async Task<IEnumerable<IssueRequestModel>> getmaterialrequestbyrequestid(string requestid)
		{
			return await this._poService.GetmaterialdetailsByrequestid(requestid);
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
		[HttpDelete("deletegatepassmaterial")]
		public int deletematerial(int gatepassmaterialid)
		{
			return this._poService.deletegatepassmaterial(gatepassmaterialid);
		}
		[HttpPost("updategatepassapproverstatus")]
		public int gatepassapproverstatus(gatepassModel model)
		{
			return this._poService.updategatepassapproverstatus(model);
		}
		[HttpGet("getmaterialdetailsbygatepassid")]
		public async Task<IEnumerable<gatepassModel>> gatepassmaterialdetail(int gatepassid)
		{
			return await this._poService.GetmaterialList(gatepassid);
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
		public async Task<IEnumerable<userAcessNamesModel>> getuserAcessList(string employeeid,string roleid)
		{

			return await this._poService.getuserAcessList(employeeid,roleid);
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
			return await this._poService.MaterialRequestdata(pono,loginid);
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
		public int getmaterialissueList([FromBody] List<ReserveMaterialModel> datamodel)
		{
			return  this._poService.insertResevematerial(datamodel);
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
		//[HttpPost("securitysendemail")]
		//public EmailModel sendemail(EmailModel obj)
		//{
		//    return this.emailobj.sendEmail(obj,1);
		//}

	}
}
