using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;

namespace WMS.Models
{
	public class OpenPoModel
	{
		public int rfqsplititemid { get; set; }
		public string departmentname { get; set; }

		public string securitypo { get; set; }
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

		public int pendingqty { get; set; }

		public bool isreceivedpreviosly { get; set; }

		public string asnno { get; set; }

		public bool onhold { get; set; }

		public string onholdremarks { get; set; }

		public bool ispono { get; set; }

		public string receiveremarks { get; set; }

		public string receivedby { get; set; }

		//public int inwmasterid { get; set; }
		public string inwmasterid { get; set; }


		public string unholdedby { get; set; }

		public DateTime receiveddate { get; set; }

		public DateTime unholdedon { get; set; }

		public string unholdremarks { get; set; }

		public DateTime deliverydate { get; set; }

		public List<string> pos { get; set; }
		public bool isasn { get; set; }
		public bool issupplier { get; set; }

	}

	public class printMaterial
	{
		public string materialid { get; set; }
		public string receiveddate { get; set; }

		public string receivedqty { get; set; }
		public string grnno { get; set; }
		public string pono { get; set; }
		public string invoiceno { get; set; }
		public int noofprint { get; set; }
		public string barcodePath { get; set; }
		public string materialcodePath { get; set; }
		public string errorMsg { get; set; }
		public bool isprint { get; set; }
		public string printedby { get; set; }
		public int noofpieces { get; set; }
		public int totalboxes { get; set; }
		public int boxno { get; set; }
		public string serialno { get; set; }
		public string material { get; set; }
		public string mscode { get; set; }

		public string order { get; set; }
		public string qty { get; set; }
		public string sotype { get; set; }
		public string insprec { get; set; }
		public string shipto { get; set; }
		public string matdesc { get; set; }
		public string saleorder { get; set; }
		public string saleorderlineitemno { get; set; }
		public int qtyrec { get; set; }

		public string plant { get; set; }
		public string gr { get; set; }
		public string sp { get; set; }
		public string loadingdate { get; set; }
		public string linkageno { get; set; }
		public string customername { get; set; }
		public string customer { get; set; }

		public string partno { get;set; }


	}

	public class BarcodeModel
	{
        internal string grnnumber { get; set; }

        public int departmentid { get; set; }
		public int barcodeid { get; set; }
		public int paitemid { get; set; }
		public string barcode { get; set; }
		public DateTime createddate { get; set; }
		public string createdby { get; set; }
		public bool deleteflag { get; set; }
		//public int inwmasterid { get; set; }
		public string inwmasterid { get; set; }

		public string pono { get; set; }
		public string invoiceno { get; set; }
		public DateTime invoicedate { get; set; }
		public string receivedby { get; set; }
		public DateTime receiveddate { get; set; }
		public string Material { get; set; }
		public string Materialdescription { get; set; }
		public POList[] polist { get; set; }

		public string suppliername { get; set; }
		public string asnno { get; set; }
		public string inwardremarks { get; set; }

		public string docfile { get; set; }

		public DateTime reprintedon { get; set; }
		public string reprintedby { get; set; }
		public int reprintcount { get; set; }
		public string transporterdetails { get; set; }
		public string vehicleno { get; set; }

	}

	public class iwardmasterModel
	{
		//public int inwmasterid { get; set; }
		public string inwmasterid { get; set; }

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

		public string inwardidview { get; set; }
		public string racknumber { get; set; }
		public string locatorname { get; set; }
		public string storeid { get; set; }
		public string rackid { get; set; }
		public string binid { get; set; }
		public int itemid { get; set; }

		public int vendorid { get; set; }
		public int inwardid { get; set; }
		public string projectname { get; set; }
		//public int inwmasterid { get; set; }
		public string inwmasterid { get; set; }

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
		public string printedby { get; set; }
		public DateTime printedon { get; set; }
		public string grnnumber { get; set; }
		public string Materialdescription { get; set; }
		public string itemlocation { get; set; }
		public string invoiceno { get; set; }
		public bool print { get; set; }
		public string vendorname { get; set; }
		public int materialqty { get; set; }

		public string asn { get; set; }

		public string returnremarks { get; set; }

		public int qualitypassedqty { get; set; }
		public int qualityfailedqty { get; set; }

		public bool qualitycheck { get; set; }
		public bool onhold { get; set; }

		public string onholdremarks { get; set; }
		public string availableqty { get; set; }

		public string receiveremarks { get; set; }

		public string unholdedby { get; set; }

		public bool isdirecttransferred { get; set; }

		public string projectcode { get; set; }
		public string mrnby { get; set; }
		public DateTime mrnon { get; set; }
		public string mrnremarks { get; set; }

		public string notifyremarks { get; set; }
		public string notifiedby { get; set; }
		public bool notifiedtofinance { get; set; }
		public DateTime notifiedon { get; set; }
		public string putawayfilename { get; set; }
		public string stocktype { get; set; }
	}
	public class StockModel
	{
		public int inwardid { get; set; }
		public string Material { get; set; }
		public string exceptions { get; set; }
		public string materialdescription { get; set; }
		public string stockstatus { get; set; }
		public int itemid { get; set; }
		public string grnnumber { get; set; }
		//public int inwmasterid { get; set; }
		public string inwmasterid { get; set; }

		public int paitemid { get; set; }
		public string pono { get; set; }
		public int binid { get; set; }
		public int rackid { get; set; }
		public int storeid { get; set; }
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
		public string materialid { get; set; }

		public string remarks { get; set; }

		public string stocktype { get; set; }

		public string returnid { get; set; }
		public bool initialstock { get; set; }

		public string bin { get; set; }
		public string rack { get; set; }
		public string store { get; set; }
		public string locatorname { get; set; }
		public int unitprice { get; set; }
		//public Decimal? unitprice { get; set; }

		public string receiveddate { get; set; }
		public string shelflifedate { get; set; }
		public string manufacturedate { get; set; }
		public string entrydate { get; set; }

		public Decimal? value { get; set; }
		public string projectid { get; set; }
		public string  uploadedfilename { get; set; }
		public string uploadbatchcode { get; set; }

		public int successrecords { get; set; }
		public int exceptionrecords { get; set; }
		public int totalrecords { get; set; }


	}

	public class MaterialinHand
	{
		public string material { get; set; }
		public string materialdescription { get; set; }
		public int availableqty { get; set; }
		public Decimal? value { get; set; }
        List<matlocations> locations { get; set; }
	}

	public class matlocations
	{
		public string itemlocation { get; set; }
		public int quantity { get; set; }
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
		public string value { get; set; }
		public string text { get; set; }
		public string name { get; set; }
		public string suppliername { get; set; }
		public string code { get; set; }
		public string POno { get; set; }
		public string qty { get; set; }
		public int quotationqty { get; set; }
		public string status { get; set; }
	}

	//podetails model
	public class PODetails
	{
		public string pono { get; set; }
		public string suppliername { get; set; }
		public string quotationqty { get; set; }
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
		public string materialid { get; set; }
		public string qtytotal { get; set; }
		public int confirmqty { get; set; }
		//public int inwmasterid { get; set; }
				public string inwmasterid { get; set; }

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
		public string inwmasterid { get; set; }
		public int quantity { get; set; }
		public string requesterid { get; set; }
		public string itemreceiverid { get; set; }
		public string approverid { get; set; }
		public string requestername { get; set; }
		public string approvername { get; set; }
		public string type { get; set; }
		public string pono { get; set; }

		public string details { get; set; }
		public string acknowledge { get; set; }
		public DateTime issuedon { get; set; }


	}
	public class DynamicSearchResult
	{
		public string tableName { get; set; }
		public string searchCondition { get; set; }
		public string query { get; set; }

	}

	public class locataionDetailsStock
	{
		public int rackid { get; set; }
		public int binid { get; set; }
		public int storeid { get; set; }
		public string locationid { get; set; }
		public string locationname { get; set; }
		public string rackname { get; set; }
		public string binname { get; set; }
		public string storename { get; set; }
	}
	public class IssueRequestModel
	{

		public string material { get; set; }
		//public string materialdescription { get; set; }
		public int transferid { get; set; }
		public int confirmqty { get; set; }
		public int reserveformaterialid { get; set; }
		public string ackstatus { get; set; }
		public string remarks { get; set; }
		public string returnid { get; set; }
		public string id { get; set; }
		public int matreturnid { get; set; }
		public int requestforissueid { get; set; }
		public int itemid { get; set; }
		public Boolean itemreturnable { get; set; }

		public string requestid { get; set; }
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
		//public string Material { get; set; }
		public string ackremarks { get; set; }
		public string Materialdescription { get; set; }
		public DateTime createddate { get; set; }
		public int availableqty { get; set; }
		public string itemlocation { get; set; }
		public int issuedqty { get; set; }
		public string jobname { get; set; }
		public int materialqty { get; set; }
		public int returnqty { get; set; }
		public string requesttype { get; set; }
		public DateTime returnon { get; set; }
		public string returnqtyaccept { get; set; }
		public int materialissueid { get; set; }
		public string createdby { get; set; }
		public int transferqty { get; set; }
		public string projectcode { get; set; }
		public DateTime createdon { get; set; }
		public string confirmstatus { get; set; }
		public Decimal? materialcost { get; set; }

		public string issuedby { get; set; }

		public int storeid { get; set; }

		public int rackid { get; set; }
		public int binid { get; set; }

		public int defaultstore { get; set; }

		public int defaultrack { get; set; }
		public int defaultbin { get; set; }

		public string reserveid { get; set; }
		public int reservedqty { get; set; }

		public string stocktype { get; set; }

		public string calltype { get; set; }

		public string requeststatus { get; set; }

		public string requestmaterialid { get; set; }

		public int plantstockavailableqty { get; set; }

		public string putawaystatus { get; set; }




	}


	public class sequencModel
	{
		public int id { get; set; }
		public string sequencename { get; set; }
		public int sequenceid { get; set; }
		public int year { get; set; }
		public int sequencenumber { get; set; }
	}

	public class stockCardPrint
	{
		public string pono { get; set; }
		public string projectdef { get; set; }
		public string jobname { get; set; }
		public string modelno { get; set; }
		public string description { get; set; }
		public Int64 qty { get; set; }
		public string box { get; set; }
		public DateTime date { get; set; }
		public string checkedby { get; set; }
	}

	public class pageModel
	{
		public int id { get; set; }
		public string pagename { get; set; }
		public string pageurl { get; set; }
		public int roleid { get; set; }
		public bool isrootpage { get; set; }

		public int rootpageid { get; set; }
	}

	public class gatepassModel
	{
        internal object requestid;

        public string statusremarks { get; set; }
		public string managername { get; set; }
		public string pono { get; set; }
		public int itemid { get; set; }
		public string itemreturnable { get; set; }
		public int issuedqty { get; set; }
		public string reprintedby { get; set; }
		public int availableqty { get; set; }
		//gate pass change
		public string gatepassid { get; set; }
		public string gatepasstype { get; set; }
		public string status { get; set; }
		public string referenceno { get; set; }
		public string vehicleno { get; set; }
		public string requestedby { get; set; }
		public DateTime requestedon { get; set; }
		public int gatepassmaterialid { get; set; }
		public string materialid { get; set; }
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
		public DateTime? approvedon { get; set; }
		public List<materialistModel> materialList { get; set; }
		public string printedby { get; set; }
		public DateTime printedon { get; set; }
		public string remarks { get; set; }

		public Decimal? materialcost { get; set; }
		public DateTime? expecteddate { get; set; }
		public DateTime? returneddate { get; set; }
		public string approvedby { get; set; }
		public DateTime itemissueddate { get; set; }
		public string itemreceiverid { get; set; }
		public string approverid { get; set; }
		public string fmapproverid { get; set; }
		public string fmapprovedby { get; set; }
		public string fmapproverremarks { get; set; }
		public string fmapprovedon { get; set; }
		public int categoryid { get; set; }
		public string fmapprovedstatus { get; set; }

		public DateTime? outwarddate { get; set; }
		public string outwardedby { get; set; }
		public string outwardremarks { get; set; }
		public DateTime? inwarddate { get; set; }
		public string inwardedby { get; set; }
		public string inwardremarks { get; set; }
		public int outwardqty { get; set; }
		public int outwardedqty { get; set; }
		public int inwardqty { get; set; }
		public int inwardedqty { get; set; }
		public string itemlocation { get; set; }
		public string mgapprover { get; set; }

		public DateTime? securityinwarddate { get; set; }

		public string securityinwardby { get; set; }
		public string securityinwardremarks { get; set; }

		public string fmapprover { get; set; }
		public string approvername { get; set; }

		public DateTime? createddate { get; set; }
	}
	public class materialistModel
	{
		public string materialid { get; set; }
		public string remarks { get; set; }
		public int quantity { get; set; }
		public int gatepassmaterialid { get; set; }
		public Decimal? materialcost { get; set; }
		public DateTime expecteddate { get; set; }
		public DateTime? returneddate { get; set; }
		public int issuedqty { get; set; }

	}

	public class outwardinwardreportModel
    {
		//gate pass change
		public string gatepassid { get; set; }
		public int gatepassmaterialid { get; set; }
		public string materialid{ get; set; }
		public string materialdescription{ get; set; }
		public DateTime? outwarddate{ get; set; }
        public string outwardby{ get; set; }
	    public string outwardremarks{ get; set; }
		public int outwardqty{ get; set; }
	    public DateTime?  inwarddate { get; set; }
	    public string inwardby{ get; set; }
        public string inwardremarks{ get; set; }
	    public int inwardqty{ get; set; }
        public DateTime? securityinwarddate { get; set; }
	    public string securityinwardby{ get; set; }
		public string securityinwardremarks { get; set; }
		public int issuedqty{ get; set; }


	}

	public class reprintModel
	{
		//gate pass change
		public int reprinthistoryid { get; set; }

		//public int? gatepassid { get; set; }
		public string gatepassid { get; set; }
		//public int? inwmasterid { get; set; }
		public string inwmasterid { get; set; }

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
		public bool iscounted { get; set; }
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


	public class MaterialData
	{
		public string material { get; set; }
		public string materialtype { get; set; }
		public string materialdescription { get; set; }
		public string storeid { get; set; }
		public string rackid { get; set; }
		public string binid { get; set; }
		public string unitprice { get; set; }

	}

	public class class1
    {
		public string material { get; set; }
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
		public int issuedqty { get; set; }
	}

	public class employeeModel1
    {
		public string employeeno { get; set; }
		public string name { get; set; }
		public string email { get; set; }

	}
	public class EmailModel
	{
        internal string createdby { get; set; }

		internal DateTime createddate { get; set; }
		internal object requestid { get; set; }
		internal string reserveid { get; set; }
		internal string name { get; set; }
		internal string employeeno { get; set; }
		internal DateTime requestedon { get; set; }
		internal string requestedby { get; set; }
		// gate pass change
		//internal int gatepassid { get; set; }
		internal string gatepassid { get; set; }
		internal string gatepasstype { get; set; }

		public int materialissueid { get; set; }

		public string FrmEmailId { get; set; }
		public string ToEmailId { get; set; }
		public string CC { get; set; }
		public string Subjecttype { get; set; }
		public string pono { get; set; }
		public string jobcode { get; set; }
		public string invoiceno { get; set; }
		public string ToEmpName { get; set; }
		public string sendername { get; set; }
		public DateTime receiveddate { get; set; }
		public string receivedby { get; set; }
		public string grnnumber { get; set; }
		public string asnno { get; set; }
		public string suppliername { get; set; }
		public string material { get; set; }
		public string transferid { get; set; }
		public string transferbody { get; set; }
		public string approverid { get; set; }
		public string approvername { get; set; }
		public string approverstatus { get; set; }
		// public int inwmasterid { get; set; }
		public string inwmasterid { get; set; }
		 

	}
	public class employeeModel
	{
		public string name { get; set; }
		public string employeenoformanager { get; set; }
		public string managername { get; set; }
		public string approverid { get; set; }
	}
	public class authUser
	{
		public int authid { get; set; }
		public string employeeid { get; set; }
		public int roleid { get; set; }
		public DateTime createddate { get; set; }
		public string createdby { get; set; }
		public bool deleteflag { get; set; }
		 public bool emailnotification { get; set; }
		public string email { get; set; }
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

	public class UnholdGRModel
	{
		//public int inwmasterid { get; set; }
		public string inwmasterid { get; set; }
		public bool unholdaction { get; set; }
		public string unholdedby { get; set; }

		public string unholdremarks { get; set; }

	}

	public class MRNsavemodel
	{
		public string grnnumber { get; set; }
		public string projectcode { get; set; }
		public string directtransferredby { get; set; }
		public string mrnremarks { get; set; }
	}

	public class DashboardModel
	{
		public int todayexpextedcount { get; set; }
		public int todayreceivedcount { get; set; }
		public int todaytoissuecount { get; set; }

		public List<DashboardGraphModel> graphdata { get; set; }

	}

	public class ManagerDashboard
	{
		public int pendingcount { get; set; }
		public int onholdcount { get; set; }
		public int completedcount { get; set; }
		public int qualitycompcount { get; set; }
		public int qualitypendcount { get; set; }
		public int putawaypendcount {get;set;}
		public int putawaycompcount {get;set;}
		public int putawayinprocount {get;set;}
		public int acceptancependcount { get; set; }
		public int acceptancecompcount { get; set; }

	}
	public class UserDashboardGraphModel
	{
		public string smonth { get; set; }
		public string syear { get; set; }
		public string sweek { get; set; }
		public int count { get; set; }
		public DateTime graphdate { get; set; }

		public string type { get; set; }

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
		public int quantity { get; set; }
		public int issuedqty { get; set; }
		public string requesterid { get; set; }
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
		public string reserveid { get; set; }
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
		public string email { get; set; }

		public string requestedby { get; set; }
		public DateTime requestedon { get; set; }

		public string chkstatus { get; set; }
		public string materialdescription { get; set; }
		public string projectcode { get; set; }
		public string remarks { get; set; }
	}
	public class SecurityInwardreceivedModel
	{
		public string pono { get; set; }
		public string receivedby { get; set; }

		public string invoiceno { get; set; }

		public string asn { get; set; }

		public string suppliername { get; set; }
		public string npsuppliername { get; set; }
	}
	public class dropdownModel
	{
		public int binid { get; set; }
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

		public bool qualitycheck { get; set; }

	}
}

public class gatepassapprovalsModel
{
	public int historyid { get; set; }

	// gate pass change
	//internal int gatepassid { get; set; }
	public string gatepassid { get; set; }
	public int approverid { get; set; }
	public string approvername { get; set; }
	public string approverstatus { get; set; }
	public DateTime approvedon { get; set; }
	public int label { get; set; }
	public string remarks { get; set; }
	public Boolean currentStatus { get; set; }

}

public class safteyStockList
{
	public string material { get; set; }
	public string materialdescription { get; set; }
	public int availableqty { get; set; }
	public int safteystock { get; set; }
	public int minorderqty { get; set; }
}
public class stocktransferModel
{

	public int transferid { get; set; }
	public int itemid { get; set; }
	public string materialid { get; set; }
	public string materialdescription { get; set; }
	public string previouslocation { get; set; }
	public int previousqty { get; set; }
	public string currentlocation { get; set; }
	public int transferedqty { get; set; }
	public DateTime transferedon { get; set; }
	public string transferedby { get; set; }
	public string remarks { get; set; }


}

public class invstocktransfermodel
{
	public string transferid { get; set; }
	public string transferredby { get; set; }
	public DateTime transferredon { get; set; }
	public string transfertype { get; set; }
	public string sourceplant { get; set; }
	public string destinationplant { get; set; }
	public string remarks { get; set; }
	public List<stocktransfermateriakmodel> materialdata { get; set; }
}

public class stocktransfermateriakmodel
{
	public int number { get; set; }
	public string transferid { get; set; }
	public int itemid { get; set; }
	public string materialid { get; set; }

	public int binid { get; set; }
	public int rackid { get; set; }
	public int storeid { get; set; }
	public string materialdescription { get; set; }
	public string sourcelocation { get; set; }
	public int sourceitemid { get; set; }
	public string destinationlocation { get; set; }
	public int destinationitemid { get; set; }
	public int transferqty;
	public string[] mlocations { get; set; }
}

public class ddlmodel
{

	public string value { get; set; }
	public string text { get; set; }
	public string supplier { get; set; }
	public string pos { get; set; }
	public string projectmanager { get; set; }
	public DateTime receiveddate { get; set; }

}

public class materialtransferMain
{
	public string transferid { get; set; }
	public string projectcode { get; set; }
	public string projectmanagerto { get; set; }
	public string projectcodefrom { get; set; }
	public string projectmanagerfrom { get; set; }
	public int transferredqty { get; set; }
	public string transferremarks { get; set; }

	public string approvalremarks { get; set; }
	public string approverid { get; set; }
	public string transferedby { get; set; }
	public string requesteremail { get; set; }
	public DateTime transferredon { get; set; }
	public int approvallevel { get; set; }
	public int finalapprovallevel { get; set; }
	public int applevel { get; set; }

	public bool isapproved { get; set; }
	public string status { get; set; }

	public string requestoremail { get; set; }
	public List<materialtransferTR> materialdata { get; set; }

	public List<materialtransferapproverModel> approverdata { get; set; }



}
public class materialtransferTR
{
	public string transferid { get; set; }
	public string materialid { get; set; }
	public string materialdescription { get; set; }
	public int transferredqty { get; set; }
}

public class materialtransferapproverModel
{
	public string approverid { get; set; }
	public string approvername { get; set; }
	public string status { get; set; }
	public DateTime? approvedon { get; set; }
	public string remarks { get; set; }

	public string approveremail { get; set; }
}
public class DirectTransferMain
{
	//public int inwmasterid { get; set; }
	public string inwmasterid { get; set; }

	public string projectcode { get; set; }
	public string grnnumber { get; set; }
	public string mrnremarks { get; set; }
	public string mrnby { get; set; }
	public DateTime mrnon { get; set; }
	public List<DirectTransferTR> materialdata { get; set; }

}
public class DirectTransferTR
{
	//public int inwmasterid { get; set; }
	public string inwmasterid { get; set; }

	public string materialid { get; set; }
	public string materialdescription { get; set; }
	public int confirmqty { get; set; }
}

public class notifymodel
{

	public string grnnumber { get; set; }
	public string notifiedby { get; set; }
	public string notifyremarks
	{
		get; set;
	}
}
public class updateonhold
{
	public string invoiceno { get; set; }
	public string remarks { get; set; }
	public bool onhold { get; set; }
}

public class materialReservetorequestModel
{
	public string reserveid { get; set; }
	public string requestedby { get; set; }
}

public class materialistModel
{
	// gate pass change
	//internal int gatepassid { get; set; }
	public string gatepassid { get; set; }
	public string gatepassmaterialid { get; set; }
	public string materialid { get; set; }
	public string materialdescription { get; set; }
	public int quantity { get; set; }
	public string remarks { get; set; }
	public DateTime expecteddate { get; set; }
	public DateTime? returneddate { get; set; }
	public int availableqty { get; set; }
	public string materialcost { get; set; }
	public int issuedqty { get; set; }
	public DateTime outwarddate { get; set; }
	public DateTime inwarddate { get; set; }

	public string outwarddatestring { get; set; }
	public string inwarddatestring { get; set; }
	public string movetype { get; set; }

	public string movedby { get; set; }

	public int outwardqty { get; set; }

	public int inwardqty { get; set; }
}

public class rbamaster
{
	public int id { get; set; }
	public int roleid { get; set; }
	public bool inv_enquiry { get; set; }
	public bool inv_reports { get; set; }
	public bool gate_entry { get; set; }
	public bool gate_entry_barcode { get; set; }
	public bool inv_receipt_alert { get; set; }
	public bool receive_material { get; set; }
	public bool put_away { get; set; }
	public bool material_return { get; set; }
	public bool material_transfer { get; set; }
	public bool gate_pass { get; set; }
	public bool gatepass_inout { get; set; }
	public bool gatepass_approval { get; set; }
	public bool material_issue { get; set; }
	public bool material_request { get; set; }
	public bool material_reservation { get; set; }
	public bool abc_classification { get; set; }
	public bool cyclecount_configuration { get; set; }
	public bool cycle_counting { get; set; }
	public bool cyclecount_approval { get; set; }
	public bool admin_access { get; set; }
	public bool masterdata_creation { get; set; }
	public bool masterdata_updation { get; set; }
	public bool masterdata_approval { get; set; }
	public bool printbarcodes { get; set; }
	public bool quality_check { get; set; }
	public bool pmdashboard_view { get; set; }
	public DateTime? modified_on { get; set; }
	public string modified_by { get; set; }
}

public class UserDashboardDetail
{
	public int inbountfortoday { get; set; }
	public int pendingtooutward { get; set; }
	public int pendingtoinward { get; set; }
	public int pendingtoPMapproval { get; set; }
	public int pendingtoFMapproval { get; set; }
	public int pendingtoreceive { get; set; }
	public int pendingtoqualitycheck { get; set; }
	public int pendingtoaccetance { get; set; }
	public int pendingtoputaway { get; set; }
	public int pendingtoissue { get; set; }
	public int pendingshipments { get; set; }
	public int receivedshipments { get; set; }
	public int reservedquantityforthisweek { get; set; }
	public int pendingtoapproval { get; set; }
	public int pendingcyclecountapproval { get; set; }
}

public class testcrud
{
	public int id { get; set; }
	public string name { get; set; }
	public bool ismanager { get; set; }
}

public class PrintHistoryModel
{
	public int reprinthistoryid { get; set; }

	// gate pass change
	//internal int gatepassid { get; set; }
	public string gatepassid { get; set; }
	//public int inwmasterid { get; set; }
	public string inwmasterid { get; set; }

	public DateTime reprintedon { get; set; }
	public string reprintedby { get; set; }
	public int reprintcount { get; set; }
	public int barcodeid { get; set; }
	public string po_invoice { get; set; }
	public string pono { get; set; }
	public string invoiceNo { get; set; }
}

public class materilaTrasFilterParams
{
	public string FromDate { get; set; }
	public string ToDate { get; set; }
}

public class WMSHttpResponse
{
	public string message { get; set; }
}


//Amulya
public class materialrequestMain
{
	public string requestid { get; set; }
	public string requestedby { get; set; }
	public DateTime requesteddate { get; set; }

	public string ackstatus { get; set; }
	public string ackremarks { get; set; }
	 public string remarks { get; set; }
	public List<materialrequestMR> materialdata { get; set; }


}
public class materialrequestMR
{
	public string requestid { get; set; }
	public string materialid { get; set; }
	public string materialdescription { get; set; }
	public int requestedquantity { get; set; }
	public int returnqty { get; set; }
	public int issuedquantity { get; set; }
}
public class materialRequestFilterParams
{
	public string ToDate { get;  set; }
	public string FromDate { get;  set; }
}
//Amulya
public class materialreserveMain
{
	public string reserveid { get; set; }
	public string reservedby { get; set; }
	public DateTime reservedon { get; set; }
	public string status { get; set; }
	public string requestedby { get; set; }
	public string remarks { get; set; }

	public List<materialreserveMS> materialdata { get; set; }


}
public class materialreserveMS
{
	public string requestid { get; set; }
	public string materialid { get; set; }
	public string materialdescription { get; set; }
	public int reservequantity { get; set; }
}
public class materialResFilterParams
{
	public string ToDate { get;  set; }
	public string FromDate { get;  set; }
}

//Amulya
public class materialreturnMain
{
	public string returnid { get; set; }
	//public int returnid { get; set; }
	public string createdby { get; set; }
	public DateTime createdon { get; set; }
	public string confirmstatus { get; set; }
	public string remarks { get; set; }

	public List<materialreturnMT> materialdata { get; set; }


}
public class materialreturnMT
{
	public string requestid { get; set; }
	public string materialid { get; set; }
	public string materialdescription { get; set; }
	public int returnqty { get; set; }
	public string remarks { get; set; }
}
public class materialRetFilterParams
{
	public string ToDate { get;  set; }
	public string FromDate { get;  set; }
}

public class MaterialTransaction
{
	public string requestid { get; set; }
	public string pono { get; set; }
	public string requesttype { get; set; }
	public string projectcode { get; set; }
	public string remarks { get; set; }
	public bool deleteflag { get; set; }
	public string ackstatus { get; set; }
	public string ackremarks { get; set; }
	public string approveremailid { get; set; }
	public string approverid { get; set; }
	public string requesterid { get; set; }
	public DateTime? requesteddate { get; set; }
	public string reserveid { get; set; }
	public List<MaterialTransactionDetail> materialdata { get; set; }

	public string approvedstatus { get; set; }

	public bool status { get; set; }
	public DateTime reserveupto { get; set; }
	public string reservedby { get; set; }
	public DateTime? reservedon { get; set; }
	public DateTime? requestedon { get; set; }
	public string requestedby { get; set; }

	public string chkstatus { get; set; }
}

public class MaterialTransactionDetail
{
	public string id { get; set; }
	public string requestid { get; set; }
	public string reserveid { get; set; }
	public string materialid { get; set; }
	public string materialdescription { get; set; }
	public int requestedquantity { get; set; }
	public int itemid { get; set; }
	public int issuedquantity { get; set; }
	public int reservedqty { get; set; }
	
	public int returnqty { get; set; }
}

public class MaterialReturnTR
{
	public string returnid { get; set; }
	public string materialid { get; set; }
	public string materialdescription { get; set; }
	public int returnqty { get; set; }
	public string remarks { get; set; }
}

public class MaterialReturn
{

	public string returnid { get; set; }
	public string createdby { get; set; }
	public DateTime? createdon { get; set; }

	public string confirmstatus { get; set; }
	public List<MaterialReturnTR> materialdata { get; set; }

}

public class initialStock
{
	public string uploadedfilename { get; set; }
	public string uploadbatchcode { get; set; }
	public string uploadedby { get; set; }
	public string material { get; set; }
	public string materialdescription { get; set; }
	public string store { get; set; }
	public string rack { get; set; }
	public string bin { get; set; }
	public int? quantity { get; set; }
	public string grn { get; set; }
	public DateTime? receiveddate { get; set; }
	public DateTime? shelflifeexpiration { get; set; }
	public DateTime? dateofmanufacture { get; set; }
	public string datasource { get; set; }
	public string dataenteredby { get; set; }
	public DateTime? dataenteredon { get; set; }
	public bool DataloadErrors { get; set; }
	public string error_description { get; set; }
	public DateTime? createddate { get; set; }
	public string stocktype { get; set; }
	public Decimal? unitprice { get; set; }
	public string category { get; set; }
	public Decimal? value { get; set; }
	public string projectid { get; set; }
	public string pono { get; set; }
}


