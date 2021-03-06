﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WMS.Models;
namespace WMS.Interfaces
{
	public interface IPodataService<T>
	{
		Task<IEnumerable<T>> getOpenPoList(string loginid, string pono = null, string docno = null, string vendorid = null);
		Task<IEnumerable<T>> getdashboardlist(string loginid);

		Task<IEnumerable<POList>> getPOList(string postatus);

		Task<IEnumerable<POList>> getPODataList(string suppliername);

		//Get invoice details for PO no
		Task<IEnumerable<InvoiceDetails>> getinvoiveforpo(string PONO);

		//Get material Details
		Task<IEnumerable<MaterialDetails>> getMaterialDetails(string grnno, string pono);

		//Location Details
		Task<IEnumerable<LocationDetails>> getlocationdetails(string materialid, string grnnumber);
		string updatelocationdata(matlocations dataobj);

		//Get po details
		Task<IEnumerable<PODetails>> getPODetails(string empno);
		Task<IEnumerable<PODetails>> getPODetailsByProjectCode(string empno, string projectcode);
		Task<IEnumerable<PODetails>> getStorePODetailsByProjectCode(string empno, string projectcode);
		Task<IEnumerable<PODetails>> getPODetailsbyprojectcodeformiscissue(string projectcode);

		//Get direct transfer data
		Task<IEnumerable<DirectTransferMain>> getdirecttransferdata(string empno);

		Task<IEnumerable<STORequestdata>> STORequestlist();

		//Get material request and issued details
		Task<IEnumerable<ReqMatDetails>> getReqMatdetails(string materialid, string grnnumber, string pono);
		//Get material reserve details for material tracking
		Task<IEnumerable<ReqMatDetails>> getReserveMatdetails(string materialid, string grnnumber, string pono);

		//Get material requested for return
		Task<IEnumerable<IssueRequestModel>> getmaterialreturnreqList(string matreturnid);

		//Check material exists
		string checkMatExists(string material);

		Task<IEnumerable<MaterialDetails>> getMaterialDetails(string grnNo);


		OpenPoModel CheckPoexists(string PONO);

		stockCardPrint getstockdetails(string pono, string materialid);

		string getstocktype(locataionDetailsStock locdetails);
		printMaterial generateBarcodeMaterial(printMaterial printMat);

		printMaterial generateBarcodeMatonhold(printMaterial printMat);

		string printBarcodeMaterial(printMaterial printMat);
		string updateQRcodePrintHistory(printMaterial printMat);
		PrintHistoryModel InsertBarcodeInfo(BarcodeModel dataobj);
		//int insertInvoicedetails(iwardmasterModel obj);
		Task<IEnumerable<T>> GetDeatilsForthreeWaymatching(string invoiceno, string pono, bool isgrn, string grnno);
		Task<IEnumerable<T>> GetDeatilsForholdgr(string status);
		Task<IEnumerable<T>> Getqualitydetails(string grnnumber);
		Task<OpenPoModel> VerifythreeWay(string pono, string invoiceno, string type);
		Task<string> insertquantity(List<inwardModel> datamodel);
		Task<string> receivequantity(List<inwardModel> datamodel);
		Task<string> updateonholdrow(updateonhold datamodel);
		string updateinitialstockdata(StockModel datamodel);
		string updateprojectmember(AssignProjectModel datamodel);

		string InsertStock(List<StockModel> data);
		string InsertmatSTO(List<StockModel> data);

		string UpdateStockTransfer(List<StockModel> data);
		string InsertStockIS(initialStock data);
		string InvStockTransfer(invstocktransfermodel data);

		System.Data.DataTable GetListItems(DynamicSearchResult result);
		System.Data.DataTable GetMaterialItems(DynamicSearchResult result);
		int IssueRequest(List<IssueRequestModel> reqdata);
		Task<IEnumerable<inwardModel>> getitemdeatils(string grnnumber);
		Task<IEnumerable<inwardModel>> getitemdeatilsnotif(string grnnumber);
		Task<IEnumerable<MaterialTransaction>> MaterialRequest(string pono, string material);
		Task<IEnumerable<IssueRequestModel>> MaterialRequestdata(string pono, string material, string projectcode);
		Task<IEnumerable<IssueRequestModel>> MaterialRequestdataforgatepass(string pono, string projectcode);
		Task<IEnumerable<IssueRequestModel>> getmaterialswithstore(string pono, string projectcode);
		Task<IEnumerable<IssueRequestModel>> MaterialRequestdataforsto(string pono, string projectcode, string store);
		Task<IEnumerable<IssueRequestModel>> MaterialRequestdataforsto_v1(string pono, string projectcode, string store);
		Task<IEnumerable<POReportModel>> getPOReportdata(string empno, string projectcode, string pono);
		Task<IEnumerable<stocktransfermateriakmodel>> getsubconannexuredata(string empno, string subconno);
		Task<IEnumerable<POReportModel>> getPOReportdetail(string materialid, string description, string pono, string querytype, string requesttype, string projectcode, string empno);
		Task<IEnumerable<IssueRequestModel>> MaterialReservedata(string projectcode);
		Task<IEnumerable<IssueRequestModel>> MaterialReservedata_v1();
		Task<IEnumerable<IssueRequestModel>> getgatepassmaterialrequestList();
		int acknowledgeMaterialReceived(List<IssueRequestModel> dataobj);
		Task<User> getempnamebycode(string empno);
		Task<IEnumerable<IssueRequestModel>> GetMaterialissueList(string requesterid);
		Task<IEnumerable<IssueRequestModel>> GetMaterialissueListforapprover(string approverid);
		Task<IEnumerable<IssueRequestModel>> GetmaterialdetailsByrequestid(string requestid, string pono);
		Task<IEnumerable<IssueRequestModel>> GetPonodetails(string pono);
		int updaterequestedqty(List<IssueRequestModel> dataobj);
		int ApproveMaterialissue(List<IssueRequestModel> dataobj);
		Task<IEnumerable<gatepassModel>> GetgatepassList();
		int SaveOrUpdateGatepassDetails(gatepassModel dataobj);
		string checkmaterialandqty(string material = null, decimal? qty = 0);
		int deletegatepassmaterial(int gatepassmaterialid);
		int updategatepassapproverstatus(List<gatepassModel> model);
		Task<IEnumerable<gatepassModel>> GetmaterialList(string gatepassid);
		Task<IEnumerable<gatepassapprovalsModel>> getGatePassApprovalHistoryList(string gatepassid);

		int updateprintstatus(gatepassModel model);
		int updatereprintstatus(reprintModel model);
		Task<IEnumerable<ReportModel>> GetreportBasedCategory(int categoryid);
		Task<IEnumerable<ReportModel>> GetreportBasedMaterial(string materailid);
		int updateABCcategorydata(List<ABCCategoryModel> model);
		Task<IEnumerable<ABCCategoryModel>> GetABCCategorydata();
		Task<IEnumerable<ReportModel>> GetABCavailableqtyList();

		Task<IEnumerable<CycleCountList>> GetCyclecountList(int limita, int limitb, int limitc);
		Task<IEnumerable<CycleCountList>> GetCyclecountPendingList();

		Task<Cyclecountconfig> GetCyclecountConfig();

		int UpdateCycleCountconfig(Cyclecountconfig dataobj);

		int UpdateinsertCycleCount(List<CycleCountList> dataobj);
		Task<IEnumerable<ReportModel>> GetABCListBycategory(string category);

		Task<IEnumerable<FIFOModel>> GetFIFOList(string material);
		ReportModel checkloldestmaterial(string materialid, string createddate);
		ReportModel checkoldmaterialwithdesc(string materialid, string createddate, string description);
		ReportModel checkoldmaterialwithdescstore(string materialid, string createddate, string description, string store);
		int FIFOitemsupdate(List<FIFOModel> model);
		Task<IEnumerable<OpenPoModel>> getASNList(string deliverdate);
		Task<IEnumerable<OpenPoModel>> getASNListdata();
		Task<IEnumerable<IssueRequestModel>> GetItemlocationListBymterial(string material);
		Task<IEnumerable<IssueRequestModel>> GetItemlocationListBymterialanddesc(string material, string description);
		Task<IEnumerable<IssueRequestModel>> GetItemLocationforplantstock(string material, string description);
		Task<IEnumerable<IssueRequestModel>> GetItemLocationforplosstock(string material, string description);
		Task<IEnumerable<IssueRequestModel>> GetItemlocationListBymterialanddescpo(string material, string description, string projectid, string pono);
		Task<IEnumerable<IssueRequestModel>> GetItemlocationwithStore(string material, string description, string projectid, string pono);
		Task<IEnumerable<IssueRequestModel>> GetItemLocationListByMaterialdescstore_v1(string material, string description, string store, string projectid, string pono);
		Task<IEnumerable<IssueRequestModel>> GetItemLocationListByMaterialdescstore(string material, string description, string store, string projectid, string pono);
		Task<IEnumerable<IssueRequestModel>> GetItemLocationListByMaterialdescpono(string material, string description, string pono);


		Task<IEnumerable<IssueRequestModel>> GetItemlocationListBymterialsourcelocation(string material, string description);


		Task<IEnumerable<IssueRequestModel>> getItemlocationListByIssueId(string requestforissueid, string requesttype);
		Task<IEnumerable<IssueRequestModel>> getItemlocationListByIssueIdWithStore(string requestforissueid, string requesttype);
		Task<IEnumerable<IssueRequestModel>> getItemlocationListByPlantIssueId(string requestforissueid, string requesttype);
		Task<IEnumerable<IssueRequestModel>> getItemlocationListByPlosIssueId(string requestforissueid, string requesttype);
		int updateissuedmaterial(List<IssueRequestModel> obj);
		int assignRole(authUser authuser);
		Task<IEnumerable<MRNsavemodel>> getmrnlist();
		Task<IEnumerable<userAcessNamesModel>> getuserAcessList(string employeeid, string roleid);
		Task<IEnumerable<userAcessNamesModel>> getuserroleList(string employeeid);
		Task<Enquirydata> GetEnquirydata(string materialid);
		Task<DashboardModel> getdashboarddata();
		Task<IEnumerable<IssueRequestModel>> getissuematerialdetails(int requestid);
		Task<IEnumerable<ReserveMaterialModel>> getissuematerialdetailsforreserved(int reservedid);
		Task<int> insertResevematerial(List<ReserveMaterialModel> datamodel);
		Task<IEnumerable<MaterialTransaction>> GetReservedMaterialList(string reservedby);
		Task<IEnumerable<ReserveMaterialModel>> GetReleasedmaterialList();
		Task<IEnumerable<ReserveMaterialModel>> GetmaterialdetailsByreserveid(string reserveid);
		int ApproveMaterialRelease(List<ReserveMaterialModel> dataobj);
		int acknowledgeMaterialReceivedforreserved(List<ReserveMaterialModel> dataobj);
		Task<IEnumerable<SecurityInwardreceivedModel>> getSecurityreceivedList();
		Task<IEnumerable<dropdownModel>> Getlocationdata();
		Task<IEnumerable<dropdownModel>> Getbindata();
		Task<IEnumerable<dropdownModel>> Getdestinationlocationforist(int store);

		Task<IEnumerable<dropdownModel>> Getrackdata();
		Task<IEnumerable<dropdownModel>> Getbindataforputaway();
		Task<IEnumerable<dropdownModel>> Getrackdataforputaway();
		Task<string> insertquantitycheck(List<inwardModel> datamodel);
		Task<string> insertreturn(List<inwardModel> datamodel);

		Task<IEnumerable<Materials>> GetMaterialcombo();
		Task<IEnumerable<employeeModel>> getapproverList(string empid);
		Task<IEnumerable<gatepassModel>> getgatepassByapproverList(string empid);
		int GatepassapproveByManager(gatepassModel model);

		int updatematmovement(List<materialistModel> obj);
		string updateuserAuth(List<authUser> obj);
		string deleteuserAuth(authUser data);
		string deletedeligatePM(authUser data);
		int requesttoreserve(materialReservetorequestModel obj);
		int insertdatacsv(ddlmodel obj);
		Task<IEnumerable<safteyStockList>> getSafteyStockList();
		Task<IEnumerable<StockModel>> GetBinList();
		Task<IEnumerable<Materials>> GetMaterialstockcombo(int store);

		Task<IEnumerable<Materials>> getMaterialforstocktransferorder();
		Task<IEnumerable<stocktransferModel>> getstocktransferdata();
		Task<IEnumerable<stocktransferModel>> getstocktransferdatagroup();

		Task<IEnumerable<invstocktransfermodel>> getstocktransferdatagroup1(string transfertype,string employeeno);
		Task<IEnumerable<ddlmodel>> pendingreceiptslist();
		Task<IEnumerable<ddlmodel>> pendingstogr();

		Task<IEnumerable<ddlmodel>> getInitialstockfilename();

		Task<IEnumerable<ddlmodel>> getdepartmentmasterdata();
		Task<IEnumerable<ddlmodel>> getgrnlistforacceptance();
		Task<IEnumerable<ddlmodel>> getgrnlistforacceptanceputaway();

		Task<IEnumerable<inwardModel>> getgrnlistforacceptancenotify(string type);

		Task<IEnumerable<inwardModel>> getnotifiedgrbydate(string fromdt, string todt);

		Task<IEnumerable<ddlmodel>> getholdgrlist();
		Task<IEnumerable<ddlmodel>> getgrnlistforacceptanceqc();

		Task<IEnumerable<ddlmodel>> getgrnlistforacceptanceqcbydate(string fromdt, string todt);

		Task<IEnumerable<gatepassModel>> NonreturnGetgatepassList(string type);

		Task<IEnumerable<outwardinwardreportModel>> outingatepassreport();
		int UpdateMaterialReserve();
		int UpdateReturnqty(List<IssueRequestModel> _listobj);
		int UpdateReturnmaterialTostock(List<IssueRequestModel> model);

		int UnholdGRdata(UnholdGRModel model);
		int mattransfer(materialtransferMain model);

		int mattransferapprove(List<materialtransferMain> model);
		string matrequestapprove(List<MaterialTransaction> model);
		Task<WMSHttpResponse> getAvailableQtyBystore(string store, string materialid, string description, string projectcode);
		Task<IssueRequestModel> getplantstockmaterialdetails(string material, string description);
		Task<IssueRequestModel> getplosstockmaterialdetails(string material, string description);
		string stomatrequestapprove(List<invstocktransfermodel> model);
		string updaterba(List<rbamaster> model);
		string updatepm(List<assignpmmodel> model);
		Task<IEnumerable<inwardModel>> getMRNmaterials(string grnnumber);
		int mrnupdate(List<MRNsavemodel> model);
		int GatepassapproveByMail(gatepassModel model);
		Task<IEnumerable<pageModel>> Getpagesbyroleid(int roleid);
		Task<IEnumerable<pageModel>> Getpages();
		Task<IEnumerable<IssueRequestModel>> GetReturnmaterialList();
		Task<IEnumerable<IssueRequestModel>> GetReturnmaterialListForConfirm(string requestid);
		Task<IEnumerable<rbamaster>> getrbadetails();

		Task<UserDashboardDetail> getUserDashboarddata(string empno);
		Task<IEnumerable<MaterialReturn>> getreturndata(string empno);
		Task<IEnumerable<materialtransferMain>> gettransferdata(string empno);
		Task<IEnumerable<materialtransferMain>> gettransferdataforapproval(string empno);
		Task<IEnumerable<MaterialTransaction>> getrequestdataforapproval(string empno);
		Task<IEnumerable<invstocktransfermodel>> getrequestdataforSTOapproval(string empno, string type);
		int Updatetransferqty(List<IssueRequestModel> _listobj);
		Task<IEnumerable<UserDashboardGraphModel>> getUserdashboardgraphdata();
		Task<IEnumerable<UserDashboardGraphModel>> getUserdashIEgraphdata();

		Task<ManagerDashboard> getManagerdashboardgraphdata(DashBoardFilters filters);
		Task<IEnumerable<UserDashboardGraphModel>> getWeeklyUserdashboardgraphdata(DashBoardFilters filters);

		Task<IEnumerable<UserDashboardGraphModel>> getmonthlyUserdashboardgraphdata(DashBoardFilters filters);

		string updateputawayfilename(ddlmodel filename);

		Task<IEnumerable<ddlmodel>> getprojectlist();
		Task<IEnumerable<ddlmodel>> getprojectlistfortransfer();
		Task<IEnumerable<ddlmodel>> getgatepassreason();
		Task<IEnumerable<ddlmodel>> getprojectlistbymanager(string empno);
		
		Task<IEnumerable<AssignProjectModel>> getprojectlisttoassign(string empno);
		Task<IEnumerable<locataionDetailsStock>> getstorelist();
		Task<IEnumerable<locataionDetailsStock>> getracklist();
		Task<IEnumerable<locataionDetailsStock>> getbinlistdata();
		Task<IEnumerable<assignpmmodel>> getprojectlisttoassignpm();
		Task<IEnumerable<ddlmodel>> getmatlist(string querytext);
		Task<IEnumerable<ddlmodel>> getmatlistbyproject(string projectcode);
		string notifyputaway(notifymodel data);
		string notifymultipleputaway(List<notifymodel> data);

		Task<IEnumerable<testcrud>> gettestcrud();
		Task<IEnumerable<MaterialinHand>> getmatinhand(inventoryFilters filters);
		Task<IEnumerable<matlocations>> getmatinhandlocation(string material, string materialid, string projectid,string pono,string sono);
		Task<IEnumerable<StockModel>> getinitialstock(string code);
		Task<IEnumerable<StockModel>> getinitialstockall(string code);
		Task<IEnumerable<StockModel>> getinitialstockEX(string code);
		Task<IEnumerable<StockModel>> getinitialstockReport(string code);
		Task<IEnumerable<StockModel>> getinitialstockReportGroup(string code);
		//Amulya
		Task<IEnumerable<StockModel>> getinitialstockload(string code);

		Task<MateriallabelModel> getmateriallabeldetail(string pono, int lineitemno, string materialid);

		string posttestcrud(testcrud data);
		string deletetestcurd(int id);
		string updateSecurityPrintHistory(PrintHistoryModel model);

		Task<IEnumerable<materialtransferMain>> getMaterialtransferdetails(materilaTrasFilterParams filters);
		//Amulya
		Task<IEnumerable<MaterialTransaction>> getmaterialrequestdashboardList(materialRequestFilterParams filters);
		//Amulya
		Task<IEnumerable<materialreserveMain>> getmaterialreservedashboardList(materialResFilterParams filters);
		//Amulya
		Task<IEnumerable<materialreturnMain>> getmaterialreturndashboardlist(materialRetFilterParams filters);

		Task<IEnumerable<IssueRequestModel>> getItemlocationListByGatepassmaterialid(string gatepassmaterialid);

		Task<IEnumerable<grReports>> getGRListdata();
		Task<IEnumerable<grReports>> addEditReports(string wmsgr);

		string EditReports(grReports data);
		Task<pmDashboardCards> getPMdashboarddata(DashBoardFilters filters);
		Task<IEnumerable<UserDashboardGraphModel>> getmonthlyUserdashboardIEgraphdata();

		Task<invDashboardCards> getInvdashboarddata(DashBoardFilters filters);

		Task<IEnumerable<UserDashboardGraphModel>> getUserdashboardgraphPMdata();

		Task<IEnumerable<GraphModelNew>> getWeeklyUserdashboardReceive(DashBoardFilters filters);
		Task<IEnumerable<GraphModelNew>> getWeeklyUserdashboardQuality(DashBoardFilters filters);
		Task<IEnumerable<GraphModelNew>> getWeeklyUserdashboardAccept(DashBoardFilters filters);
		Task<IEnumerable<GraphModelNew>> getWeeklyUserdashboardPutaway(DashBoardFilters filters);
		Task<IEnumerable<GraphModelNew>> getWeeklyUserdashboardRequest(DashBoardFilters filters);
		Task<IEnumerable<GraphModelNew>> getWeeklyUserdashboardReturn(DashBoardFilters filters);
		Task<IEnumerable<GraphModelNew>> getWeeklyUserdashboardReserve(DashBoardFilters filters);
		Task<IEnumerable<GraphModelNew>> getWeeklyUserdashboardtransfer(DashBoardFilters filters);
		Task<IEnumerable<StockModel>> getMiscellanousIssueList(bool initialStock);
		Task<IEnumerable<StockModel>> getMiscellanousIssueListdata(string initialStock,string pono, string projectid);
		Task<IEnumerable<StockModel>> getMiscellanousIssueListdatanofilter();
		bool miscellanousIssueDataUpdate(miscellanousIssueData data);
		string multiplemiscellanousIssueDataUpdate(List<miscellanousIssueData> data);
		Task<IEnumerable<StockModel>> getMiscellanousReceiptsList();
		string updateMiscellanousReceipt(StockModel item);
		System.Data.DataTable getMaterialMasterList();
		bool updateMaterialMaster(materilaMasterYgs material);
		string GPReasonMTAdd(GPReasonMTData data);
		string MiscellanousReasonAdd(GPReasonMTData data);

		string GPReasonMTDelete(GPReasonMTData data);

		string createplant(PlantMTdata data);

		Task<IEnumerable<GPReasonMTData>> getGPReasonData();

		Task<IEnumerable<GPReasonMTData>> getMiscellanousReasonData();

		string generateLabel(string labeldata);
		Task<IEnumerable<invstocktransfermodel>> getSTORequestList(string type);
		Task<IEnumerable<STOIssueModel>> getMatdetailsbyTransferId(string transferId, string type, string transfertype);
		Task<string> STOPOInitiate(List<STOIssueModel> data);
		Task<IEnumerable<initialStock>> GetInitialStockPutawayMaterials();

		Task<IEnumerable<PlantMTdata>> getplantnameData();
		string PlantnameDelete(PlantMTdata data);
		Task<IEnumerable<authUser>> getuserauthdata();
		Task<IEnumerable<authUser>> getuserauthdetails(string employeeid);
		Task<IEnumerable<authUser>> getuserauthdetailsbyrole(int roleid);
		Task<IEnumerable<subrolemodel>> getsubroledata();
		Task<IEnumerable<plantddl>> getplantlocdetails();
		Task<IEnumerable<STOrequestTR>> getSTORequestdatalist(string transferid);
		Task<string> updateSubcontractAcKstatus(List<invstocktransfermodel> data);
		Task<IEnumerable<gatepassModel>> subcontractInoutList();
		printonholdGR generateqronhold(printonholdGR onholddata);
		string addupdatestore(locataionDetailsStock store);
		string addupdaterack(locataionDetailsStock rack);
		string addupdatebin(locataionDetailsStock bin);
		public bool updateVendorMaster(vendorMaster vendormaster);
		public bool updateRole(roleMaster roleMaster);
		public bool updateSubRole(subrolemodel subrolemaster);
		public bool updateUserRole(userRoles userRoles);

		Task<IEnumerable<StockModel>> getMiscellanousIssueListdatahistory();

		 Task<IEnumerable<DDmaterials>> getDDdetailsByPono(string pono);

		public bool updateDirectDelivery(DirectDelivery userRoles);

		bool deleteDirectDelivery(string inwmasterid, string deletedby);

		Task<IEnumerable<YGSGR>> getYGSGR();

		Task<IEnumerable<gatepassModel>> GetGPReport(string fromdate, string todate);
		bool IssuerStatusChange(Issuestatus model);
	}
}
