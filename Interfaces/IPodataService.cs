using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WMS.Models;
namespace WMS.Interfaces
{
   public interface IPodataService<T>
    {
        Task<IEnumerable<T>> getOpenPoList(string loginid,string pono = null, string docno = null, string vendorid = null);

        Task<IEnumerable<POList>> getPOList(string postatus);

        //Get invoice details for PO no
        Task<IEnumerable<InvoiceDetails>> getinvoiveforpo(string PONO);

        //Get material Details
        Task<IEnumerable<MaterialDetails>> getMaterialDetails(string grnno);

        //Location Details
        Task<IEnumerable<LocationDetails>> getlocationdetails(string materialid, string grnnumber);

        //Get po details
        Task<IEnumerable<PODetails>> getPODetails(string empno);

        //Get direct transfer data
        Task<IEnumerable<DirectTransferMain>> getdirecttransferdata(string empno);

        //Get material request and issued details
        Task<IEnumerable<ReqMatDetails>> getReqMatdetails(string materialid,string grnnumber);

        //Get material requested for return
        Task<IEnumerable<IssueRequestModel>> getmaterialreturnreqList(int matreturnid);

        //Check material exists
        string checkMatExists(string material);

        OpenPoModel CheckPoexists(string PONO);

        stockCardPrint getstockdetails(string pono, string materialid);

        string getstocktype(locataionDetailsStock locdetails);
        printMaterial generateBarcodeMaterial(printMaterial printMat);
        string InsertBarcodeInfo(BarcodeModel dataobj);
        //int insertInvoicedetails(iwardmasterModel obj);
        Task<IEnumerable<T>> GetDeatilsForthreeWaymatching(string invoiceno,string pono,bool isgrn, string grnno);
        Task<IEnumerable<T>> GetDeatilsForholdgr(string status);
        Task<IEnumerable<T>> Getqualitydetails(string grnnumber);
        Task<OpenPoModel> VerifythreeWay(string pono,string invoiceno);
        Task<string> insertquantity(List<inwardModel> datamodel);
        Task<string> receivequantity(List<inwardModel> datamodel);
Task<string> updateonholdrow(updateonhold datamodel);
        string InsertStock(List<StockModel> data);
        string UpdateStockTransfer(List<StockModel> data);

        string InvStockTransfer(invstocktransfermodel data);

        System.Data.DataTable GetListItems(DynamicSearchResult result);
        System.Data.DataTable GetMaterialItems(DynamicSearchResult result);
        int IssueRequest(List<IssueRequestModel> reqdata);
        Task<IEnumerable<inwardModel>> getitemdeatils(string grnnumber);
        Task<IEnumerable<inwardModel>> getitemdeatilsnotif(string grnnumber);
        Task<IEnumerable<IssueRequestModel>> MaterialRequest(string pono,string material);
        Task<IEnumerable<IssueRequestModel>> MaterialRequestdata(string pono, string material);
        int acknowledgeMaterialReceived(List<IssueRequestModel> dataobj);
        Task<IEnumerable<IssueRequestModel>> GetMaterialissueList(string requesterid);
        Task<IEnumerable<IssueRequestModel>> GetMaterialissueListforapprover(string approverid);
        Task<IEnumerable<IssueRequestModel>> GetmaterialdetailsByrequestid(string requestid,string pono);
        Task<IEnumerable<IssueRequestModel>> GetPonodetails(string pono);
        int updaterequestedqty(List<IssueRequestModel> dataobj);
        int ApproveMaterialissue(List<IssueRequestModel> dataobj);
        Task<IEnumerable<gatepassModel>> GetgatepassList();
        int SaveOrUpdateGatepassDetails(gatepassModel dataobj);
        string checkmaterialandqty(string material=null,int qty=0);
        int deletegatepassmaterial(int gatepassmaterialid);
        int updategatepassapproverstatus(List<gatepassModel> model);
        Task<IEnumerable<gatepassModel>> GetmaterialList(int gatepassid);
        Task<IEnumerable<gatepassapprovalsModel>> getGatePassApprovalHistoryList(int gatepassid);
        
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
        ReportModel checkloldestmaterial(string materialid,string createddate);
        int FIFOitemsupdate(List<FIFOModel> model);
        Task<IEnumerable<OpenPoModel>> getASNList(string deliverdate);
        Task<IEnumerable<OpenPoModel>> getASNListdata();
        Task<IEnumerable<IssueRequestModel>> GetItemlocationListBymterial(string material);
        Task<IEnumerable<IssueRequestModel>> getItemlocationListByIssueId(string requestforissueid);
        int updateissuedmaterial(List<IssueRequestModel> obj);
        int assignRole(authUser authuser);
        Task<IEnumerable<userAcessNamesModel>> getuserAcessList(string employeeid, string roleid);
        Task<IEnumerable<userAcessNamesModel>> getuserroleList(string employeeid);
        Task<Enquirydata> GetEnquirydata(string materialid);
        Task<DashboardModel> getdashboarddata();
        Task<IEnumerable<IssueRequestModel>> getissuematerialdetails(int requestid);
        Task<IEnumerable<ReserveMaterialModel>> getissuematerialdetailsforreserved(int reservedid);
        Task<int> insertResevematerial(List<ReserveMaterialModel> datamodel);
        Task<IEnumerable<ReserveMaterialModel>> GetReservedMaterialList(string reservedby);
        Task<IEnumerable<ReserveMaterialModel>> GetReleasedmaterialList();
        Task<IEnumerable<ReserveMaterialModel>> GetmaterialdetailsByreserveid(string reserveid);
        int ApproveMaterialRelease(List<ReserveMaterialModel> dataobj);
        int acknowledgeMaterialReceivedforreserved(List<ReserveMaterialModel> dataobj);
		Task<IEnumerable<SecurityInwardreceivedModel>> getSecurityreceivedList();
        Task<IEnumerable<dropdownModel>> Getlocationdata();
        Task<IEnumerable<dropdownModel>> Getbindata();
        Task<IEnumerable<dropdownModel>> Getrackdata();
		Task<string> insertquantitycheck(List<inwardModel> datamodel);
        Task<string> insertreturn(List<inwardModel> datamodel);

        Task<IEnumerable<Materials>> GetMaterialcombo();
        Task<IEnumerable<employeeModel>> getapproverList(string empid);
        Task<IEnumerable<gatepassModel>> getgatepassByapproverList(string empid);
        int GatepassapproveByManager(gatepassModel model);

        int updatematmovement(List<materialistModel> obj);
        int requesttoreserve(materialReservetorequestModel obj);
        Task<IEnumerable<safteyStockList>> getSafteyStockList();
        Task<IEnumerable<StockModel>> GetBinList();
		Task<IEnumerable<Materials>> GetMaterialstockcombo();
        Task<IEnumerable<stocktransferModel>> getstocktransferdata();
        Task<IEnumerable<stocktransferModel>> getstocktransferdatagroup();

        Task<IEnumerable<invstocktransfermodel>> getstocktransferdatagroup1();
        Task<IEnumerable<ddlmodel>> pendingreceiptslist();
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
        int mrnupdate(MRNsavemodel model);
        int GatepassapproveByMail(gatepassModel model);
        Task<IEnumerable<pageModel>> Getpagesbyroleid(int roleid);
        Task<IEnumerable<pageModel>> Getpages();
        Task<IEnumerable<IssueRequestModel>> GetReturnmaterialList();
        Task<IEnumerable<IssueRequestModel>> GetReturnmaterialListForConfirm(string requestid);
		Task<IEnumerable<rbamaster>> getrbadetails();

        Task<UserDashboardDetail> getUserDashboarddata(string empno);
        Task<IEnumerable<IssueRequestModel>> getreturndata(string empno);
        Task<IEnumerable<materialtransferMain>> gettransferdata(string empno);
        int Updatetransferqty(List<IssueRequestModel> _listobj);
		  Task<IEnumerable<UserDashboardGraphModel>> getUserdashboardgraphdata();

        Task<IEnumerable<UserDashboardGraphModel>> getWeeklyUserdashboardgraphdata();

        Task<IEnumerable<UserDashboardGraphModel>> getmonthlyUserdashboardgraphdata();

        string updateputawayfilename(ddlmodel filename);

        Task<IEnumerable<ddlmodel>> getprojectlist();
        Task<IEnumerable<ddlmodel>> getmatlist(string empno);
        string notifyputaway(notifymodel data);
        string notifymultipleputaway(List<notifymodel> data);

        Task<IEnumerable<testcrud>> gettestcrud();
        string posttestcrud(testcrud data);
        string deletetestcurd(int id);
        string updateSecurityPrintHistory(PrintHistoryModel model);

        Task<IEnumerable<materialtransferMain>> getMaterialtransferdetails(materilaTrasFilterParams filters);
    }
}
