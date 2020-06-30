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

        Task<IEnumerable<POList>> getPOList();

        //Get invoice details for PO no
        Task<IEnumerable<InvoiceDetails>> getinvoiveforpo(string PONO);

        //Get material Details
        Task<IEnumerable<MaterialDetails>> getMaterialDetails(string grnno);

        //Location Details
        Task<IEnumerable<LocationDetails>> getlocationdetails(string materialid);

        //Get material request and issued details
        Task<IEnumerable<ReqMatDetails>> getReqMatdetails(string materialid);
        OpenPoModel CheckPoexists(string PONO);
        int InsertBarcodeInfo(BarcodeModel dataobj);
        //int insertInvoicedetails(iwardmasterModel obj);
        Task<IEnumerable<T>> GetDeatilsForthreeWaymatching(string invoiceno,string pono);
        Task<OpenPoModel> VerifythreeWay(string pono,string invoiceno);
        Task<string> insertquantity(List<inwardModel> datamodel);
       string InsertStock(StockModel data);
		System.Data.DataTable GetListItems(DynamicSearchResult result);
        int IssueRequest(List<IssueRequestModel> reqdata);
        Task<IEnumerable<inwardModel>> getitemdeatils(string grnnumber);
        Task<IEnumerable<IssueRequestModel>> MaterialRequest(string pono,string material);
        Task<IEnumerable<IssueRequestModel>> MaterialRequestdata(string pono, string material);
        int acknowledgeMaterialReceived(List<IssueRequestModel> dataobj);
        Task<IEnumerable<IssueRequestModel>> GetMaterialissueList(string requesterid);
        Task<IEnumerable<IssueRequestModel>> GetMaterialissueListforapprover(string approverid);
        Task<IEnumerable<IssueRequestModel>> GetmaterialdetailsByrequestid(string requestid);
        Task<IEnumerable<IssueRequestModel>> GetPonodetails(string pono);
        int updaterequestedqty(List<IssueRequestModel> dataobj);
        int ApproveMaterialissue(List<IssueRequestModel> dataobj);
        Task<IEnumerable<gatepassModel>> GetgatepassList();
        int SaveOrUpdateGatepassDetails(gatepassModel dataobj);
        string checkmaterialandqty(string material=null,int qty=0);
        int deletegatepassmaterial(int gatepassmaterialid);
        int updategatepassapproverstatus(gatepassModel model);
        Task<IEnumerable<gatepassModel>> GetmaterialList(int gatepassid);
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
        Task<IEnumerable<IssueRequestModel>> GetItemlocationListBymterial(string material);
        int updateissuedmaterial(List<IssueRequestModel> obj);
        int assignRole(authUser authuser);
        Task<IEnumerable<userAcessNamesModel>> getuserAcessList(string employeeid, string roleid);
        Task<Enquirydata> GetEnquirydata(string materialid);
        Task<DashboardModel> getdashboarddata();
        Task<IEnumerable<IssueRequestModel>> getissuematerialdetails(int requestid);
        Task<IEnumerable<ReserveMaterialModel>> getissuematerialdetailsforreserved(int reservedid);
        int insertResevematerial(List<ReserveMaterialModel> datamodel);
        Task<IEnumerable<ReserveMaterialModel>> GetReservedMaterialList(string reservedby);
        Task<IEnumerable<ReserveMaterialModel>> GetReleasedmaterialList();
        Task<IEnumerable<ReserveMaterialModel>> GetmaterialdetailsByreserveid(string reserveid);
        int ApproveMaterialRelease(List<ReserveMaterialModel> dataobj);
        int acknowledgeMaterialReceivedforreserved(List<ReserveMaterialModel> dataobj);
		Task<IEnumerable<SecurityInwardreceivedModel>> getSecurityreceivedList();
    }
}
