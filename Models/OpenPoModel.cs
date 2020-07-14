using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;

namespace WMS.Models
{
	public class OpenPoModel
	{
		public int rfqsplititemid { get; set; }
		public string departmentname { get; set; }
		public int departmentid { get; set; }
		public string documentno { get; set; }
		public int paid { get; set; }
		public string pono { get; set; }
		public string vendorname { get; set; }
		public string JobName { get; set; }
		public string SaleOrderNo { get; set; }
		public float quotationqty { get; set; }
		public string invoiceno { get; set; }
		public string projectcode { get; set; }
		public string projectname { get; set; }
		public int paitemid { get; set; }
		public string Material { get; set; }
		public string Materialdescription { get; set; }
		public string status { get; set; }
		public int returnqty { get; set; }
        public int vendorid { get; set; }
		public string grnnumber { get; set; }
		public int confirmqty { get; set; }
		public int receivedqty { get; set; }
		public int totalrecivedqty { get; set; }
		public DateTime enteredon { get; set; }
		public int materialqty { get; set; }
		public string asn { get; set; }
		public bool qualitycheck { get; set; }
		public int inwardid { get; set; }
		public string remarks { get; set; }
		public string checkedby { get; set; }
		public string returnedby { get; set; }
		public DateTime returnedon { get; set; }

		public int qualitypassedqty { get; set; }
		public int qualityfailedqty { get; set; }

		public string returnremarks { get; set; }

		public bool qualitychecked { get; set; }

	}

	public class printMaterial
	{
		public string materialid { get; set; }
		public string receiveddate { get; set; }
		public string grnno { get; set; }
		public string pono { get; set; }
		public string invoiceno { get; set; }
		public int noofprint { get; set; }
		public string barcodePath { get; set; }
		public string materialcodePath { get; set; }
		public string errorMsg { get; set; }
}

	public class BarcodeModel
	{
		public int departmentid { get; set; }
		public int barcodeid { get; set; }
		public int paitemid { get; set; }
		public string barcode { get; set; }
		public DateTime createddate { get; set; }
		public string createdby { get; set; }
		public bool deleteflag { get; set; }
		public int inwmasterid { get; set; }
		public string pono { get; set; }
		public string invoiceno { get; set; }
		public DateTime invoicedate { get; set; }
		public string receivedby { get; set; }
		public DateTime receiveddate { get; set; }
		public string Material { get; set; }
		public string Materialdescription { get; set; }

		public string suppliername { get; set; }

	}

	public class iwardmasterModel
	{
		public int inwmasterid { get; set; }
		public string pono { get; set; }
		public string invoiceno { get; set; }
		public DateTime invoicedate { get; set; }
		public string receivedby { get; set; }
		public DateTime receiveddate { get; set; }
		public int barcodeid { get; set; }
		public bool deleteflag { get; set; }
        public string grnnumber { get; set; }
	}
	public class inwardModel
	{
		public string binnumber { get; set; }
		public string racknumber { get; set; }
		public string locatorname { get; set; }
		public string storeid { get; set; }
		public string rackid { get; set; }
		public string binid { get; set; }
		public int itemid { get; set; }

		public int vendorid { get; set; }
        public int inwardid { get; set; }
        public string projectname { get; set; }
        public int inwmasterid { get; set; }
		public string quotationqty { get; set; }
		public int receivedqty { get; set; }
		public DateTime receiveddate { get; set; }
		public string receivedby { get; set; }
		public int returnqty { get; set; }
		public int confirmqty { get; set; }
		public bool deleteflag { get; set; }
		public string pono { get; set; }
		public string quality { get; set; }
		public string qtype { get; set; }
		public DateTime qcdate { get; set; }
		public string qcby { get; set; }
		public string remarks { get; set; }
		public string Material { get; set; }
        public string grnnumber { get; set; }
        public string Materialdescription { get; set; }
        public string itemlocation { get; set; }
		public string invoiceno { get; set; }
		public int materialqty { get; set; }

		public string asn { get; set; }

		public string returnremarks { get; set; }

		public int qualitypassedqty { get; set; }
		public int qualityfailedqty { get; set; }

		public bool qualitycheck { get; set; }
	}
	public class StockModel
	{
		public string Material { get; set; }
		public string stockstatus { get; set; }
		public int itemid { get; set; }
		public string grnnumber { get; set; }
		public int inwmasterid { get; set; }
		public int paitemid { get; set; }
		public string pono { get; set; }
		public int binid { get; set; }
		public int rackid { get; set; }
		public int vendorid { get; set; }
		public int totalquantity { get; set; }
		public DateTime shelflife { get; set; }
		public int availableqty { get; set; }
		public bool deleteflag { get; set; }
		public DateTime itemreceivedfrom { get; set; }
		public string itemlocation { get; set; }
		public DateTime createddate { get; set; }
		public string createdby { get; set; }
        public string binnumber { get; set; }
        public string racknumber { get; set; }
	  public int confirmqty { get; set; }

    }
	public class trackstatusModel
	{
		public int trackid { get; set; }
		public int paitemid { get; set; }
		public string pono { get; set; }
		public string status { get; set; }
	}

	//Polist model
	public class POList
	{
		public string POno { get; set; }
		public string qty { get; set; }
		public int quotationqty { get; set; }
	}

	//Invoice Details
	public class InvoiceDetails
	{
		public string pono { get; set; }
		public string invoiceno { get; set; }
		public string grnnumber { get; set; }
		public int receivedqty { get; set; }
		public int confirmqty { get; set; }
		public int returnqty { get; set; }
		///public string receivedqty { get; set; }
		public string returnedqty { get; set; }
		public string confirmedqty { get; set; }
	}

	//Material Details
	public class MaterialDetails
	{
		public int itemid { get; set; }
		public string pono { get; set; }
		public string grnnumber { get; set; }
		public string materialdescription { get; set; }
		public int totalquantity { get; set; }
		public int availableqty { get; set; }
		public int issued { get; set; }
		public string qtyavailable { get; set; }
		public string materialid { get;set; } 
		public string qtytotal { get; set; }
		public  int confirmqty { get; set; }
	}

	//Location Details
	public class LocationDetails
	{
		public string itemlocation { get; set; }
		public string availableqty { get; set; }
	}

	//Material Request details
	public class ReqMatDetails
	{
		public string materialid { get; set; }
		public string gatepasstype { get; set; }
		public string jobname { get; set; }
		public int itemid { get; set; }
		public int quantity { get; set; }
		public string requesterid { get; set; }
		public string itemreceiverid { get; set; }
		public string approverid { get; set; }
		public string requestername { get; set; }
		public string approvername { get; set; }
		public string type { get; set; }

		public string details { get; set; }
		public string acknowledge { get; set; }
		public DateTime issuedon { get; set; }


	}
	public class DynamicSearchResult
	{
		public string tableName { get; set; }
		public string searchCondition { get; set; }
		public  string query { get; set; }

	}
   
    public class IssueRequestModel
    {
        public int requestforissueid { get; set; }
        public int itemid { get; set; }
		public Boolean itemreturnable { get;set; }

		public int requestid { get; set; }
        public int inwardid { get; set; }
        public int quantity { get; set; }
        public int quotationqty { get; set; }
        public int requestedquantity { get; set; }
        public int issuedquantity { get; set; }
        public string pono { get; set; }
        public DateTime requesteddate { get; set; }
        public string approveremailid { get; set; }
        public string approverid { get; set; }
		public string approvedby { get; set; }
		public string itemreceiverid { get; set; }
		public DateTime approvedon { get; set; }
        public string approvedstatus { get; set; }
        public bool status { get; set; }
        public bool deleteflag { get; set; }
        public string materialid { get; set; }
        public string requesterid { get; set; }
        public string projectname { get; set; }
        public string name { get; set; }
        public string Material { get; set; }
        public string ackremarks { get; set; }
        public string Materialdescription { get; set; }
		public DateTime createddate { get; set; }
		public int availableqty { get; set; }
		public string itemlocation { get; set; }
		public int issuedqty { get; set; }
		public string jobname { get; set; }
		public int materialqty { get; set; }


	}
    public class sequencModel
    {
        public int id { get; set; }
        public string sequencename { get; set; }
        public int sequenceid { get; set; }
        public int year { get; set; }
        public int sequencenumber { get; set; }
    }
    public class gatepassModel
    {
		
		public int pono { get; set; }
		public int itemid { get; set; }
		public string itemreturnable { get; set; }
		public int issuedqty { get; set; }
		public string reprintedby { get; set; }
		public int availableqty { get; set; }
		public int gatepassid { get; set; }
        public string gatepasstype { get; set; }
        public string status { get; set; }
        public string referenceno { get; set; }
        public string vehicleno { get; set; }
        public string requestedby{ get; set; }
        public DateTime requestedon { get; set; }
        public int gatepassmaterialid { get; set; }
        public string    materialid { get; set; }
		public string materialdescription { get; set; }
		public int quantity { get; set; }
		public string vendorname { get; set; }
		public string name { get; set; }
		public bool deleteflag { get; set; }
		public string reasonforgatepass { get; set; }
		public string approverstatus { get; set; }
		
		public string approverremarks { get; set; }
		public Boolean print { get; set; }
		public int reprintcount { get; set; }
		public DateTime approvedon { get; set; }
		public List<materialistModel> materialList { get; set; }
		public string printedby { get; set; }
		public DateTime printedon { get; set; }
		public string remarks { get; set; }
		
		public int materialcost { get; set; }
		public DateTime? expecteddate { get; set; }
		public DateTime? returneddate { get; set; }
		public string approvedby { get; set; }
		public DateTime itemissueddate { get; set; }
		public string itemreceiverid { get; set; }
		public string approverid { get; set; }
	}
	public class materialistModel
	{
		public string materialid { get; set; }
		public string remarks { get; set; }
		public int quantity { get; set; }
		public int gatepassmaterialid { get; set; }
		public int materialcost { get; set; }
		public DateTime expecteddate { get; set; }
		public DateTime returneddate { get; set; }
		public int issuedqty { get; set; }

	}

		public class reprintModel
		{
		public int reprinthistoryid { get; set; }
		public int? gatepassid { get; set; }
		public int? inwmasterid { get; set; }
		public DateTime reprintedon { get; set; }
		public string reprintedby { get; set; }
		public int reprintcount { get; set; }
	}

	public class CycleCountList
    {
		public int id { get; set; }
		public string category { get; set; }
		public string materialid { get; set; }
		public string materialdescription { get; set; }
		public int availableqty { get; set; }
		public int physicalqty { get; set; }
		public int difference { get; set; }
		public bool iscountprocess { get; set; }
		public bool iscounted  { get; set; }
		public bool isapprovalprocess { get; set; }
		public bool isapproved { get; set; }

		public string remarks { get; set; }

		public string status { get; set; }

		public DateTime? counted_on { get; set; }

		public string counted_by { get; set; }
		public DateTime? verified_on { get; set; }

		public string verified_by { get; set; }

		public int todayscount { get; set; }



	}
	public class ReportModel
	{
		public int itemid { get; set; }
		public string materialid { get; set; }
		public string materialdescription { get; set; }
		public string departmentname { get; set; }
		public string itemlocation { get; set; }
		public DateTime receiveddate { get; set; }
		public int receivedqty { get; set; }
		public int issuedqty { get; set; }
		public int availableqty { get; set; }
		public double unitprice { get; set; }
		public int daysinstock { get; set; }
		public DateTime reportdate { get; set; }
		public string projectname { get; set; }
		public string vendorname { get; set; }
		public string ponumber { get; set; }
		public string category { get; set; }
		public int totalcost { get; set; }
		public DateTime createddate { get; set; }
	}
	public class ABCCategoryModel
	{
		public int categoryid { get; set; }
		public string categoryname { get; set; }
		public int? minpricevalue { get; set; }
		public int? maxpricevalue { get; set; }
		public string createdby { get; set; }
		public DateTime createdon { get; set; }
		public string updatedby { get; set; }
		public DateTime updatedon { get; set; }
		public Boolean deleteflag { get; set; }
		public DateTime enddate { get; set; }
		public DateTime startdate { get; set; }
	}
    	public class Cyclecountconfigmodel
	{
		public int amin { get; set; }
		public int amax { get; set; }
		public int bmin { get; set; }
		public int bmax { get; set; }
		public int cmin { get; set; }
		public int cmax { get; set; }
		public DateTime enddate { get; set; }
		public DateTime startdate { get; set; }
	}

	public class Cyclecountconfig
	{
		public int id { get; set; }
		public int apercentage { get; set; }
		public int bpercentage { get; set; }
		public int cpercentage { get; set; }
		public int cyclecount { get; set; }
		public string frequency { get; set; }
		public DateTime enddate { get; set; }
		public DateTime startdate { get; set; }
		public string notificationtype { get; set; }

		public string notificationon { get; set; }

		public int countall { get; set; }

	}

	public class FIFOModel
	{
		public string materialid { get; set; }
		public string Materialdescription { get; set; }
		public string itemlocation { get; set; }
		public DateTime shelflife { get; set; }
		public DateTime createddate { get; set; }
		public int availableqty { get; set; }
		public string pono { get; set; }
		public int itemid { get; set; }
		public int issuedqty { get;set; }
	}
	public class EmailModel
	{
		public string FrmEmailId { get; set; }
		public string ToEmailId { get; set; }
		public string CC { get; set; }
		public string Subjecttype { get; set; }
		public string pono { get; set; }
		public string jobcode { get; set; }
		public string invoiceno { get; set; }
		public string ToEmpName { get; set; }
		public string sendername { get; set; }

	}
	public class employeeModel
	{
		public string name { get; set; }
		public string employeenoformanager { get; set; }
		public string managername { get; set; }
	}
	public class authUser
	{
		public int authid { get; set; }
		public int employeeid { get; set; }
		public int roleid { get; set; }
		public DateTime createddate { get; set; }
		public string createdby { get; set; }
		public bool deleteflag { get; set; }
	}
	public class userAcessNamesModel
	{
		public int authid { get; set; }
		public int employeeid { get; set; }
		public int roleid { get; set; }
		public int userid { get; set; }
		public string accessname { get; set; }
	}
	public class Enquirydata
	{
		public string materialid { get; set; }
		public string materialdescription { get; set; }
		public int availableqty { get; set; }

	}

	public class DashboardModel
	{
		public int todayexpextedcount { get; set; }
		public int todayreceivedcount { get; set; }
		public int todaytoissuecount { get; set; }

		public List<DashboardGraphModel> graphdata { get; set; }

	}

	public class DashboardGraphModel
	{
		public string date { get; set; }
		public int expectedcount { get; set; }

		public int receivedcount { get; set; }

		public int toissuecount { get; set; }
	}
	public class ReserveMaterialModel
	{
		public int issuedqty { get; set; }
		public int reserveformaterialid { get; set; }
		public string materialid { get; set; }
		public string material { get; set; }
		public int itemid { get; set; }
		public DateTime reservedon { get; set; }
		public string reservedby { get; set; }
		public int reservedqty { get; set; }
		public string releasedby { get; set; }
		public DateTime releasedon { get; set; }
		public int releasedqty { get; set; }
		public int reserveid { get; set; }
		public string pono { get; set; }
		public int availableqty { get; set; }
		public int releasedquantity { get; set; }
		public string name { get; set; }
		public string jobname { get; set; }
		public Boolean itemreturnable { get; set; }
		public string approvedby { get; set; }
		public string itemreceiverid { get; set; }
		public string approvedstatus { get; set; }
		public bool status { get; set; }
		public string ackremarks { get; set; }
		public DateTime reserveupto { get; set; }
		public string projectname { get; set; }
	}
		public class SecurityInwardreceivedModel
	{
		public string pono { get; set; }
		public string receivedby { get; set; }

		public string asn { get; set; }

		public string suppliername { get; set; }
		public string npsuppliername { get; set; }
	}
	public class dropdownModel
	{
		public int binid{get;set;}
		public string binnumber { get; set; }
		public int locatorid { get; set; }
		public string locatorname { get; set; }
		public int rackid { get; set; }
		public string racknumber { get; set; }
	}

	public class Materials
	{ 
		public string material { get; set; }
		public string materialdescription { get; set; }
	
	}
}

public class gatepassapprovalsModel
{
	public int historyid { get; set; }
	public int gatepassid { get; set; }
	public int approverid { get; set; }
	public string approvername { get; set; }
	public string approverstatus { get; set; }
	public DateTime approvedon { get; set; }
	public int label { get; set; }
	public string remarks { get; set; }
	public Boolean currentStatus { get; set; }

}