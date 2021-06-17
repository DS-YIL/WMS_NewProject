/*
    Name of File : <<StagingModel>>  Author :<<Prasanna>>  
    Date of Creation <<06-07-2020>>
    Purpose : <<>>
    Review Date :<<>>   Reviewed By :<<>>
    Sourcecode Copyright : Yokogawa India Limited
*/

using System;

namespace WMS.Models
{
	internal class StagingModel
	{
		public string pono { get; set; }
		public DateTime? deliverydate { get; set; }
		public int vendorid { get; set; }
		public string suppliername { get; set; }
		public string jobname { get; set; }
		public string vendorcode { get; set; }
		public string projectcode { get; set; }
		public string projectname { get; set; }
		public string projectmanager { get; set; }
		public string pocreatedby { get; set; }
		public string materialid { get; set; }
		public string poitemdescription { get; set; }
		public string materialdescription { get; set; }
		public decimal? materialqty { get; set; }
		public int itemno { get; set; }
		public int item { get; set; }
		public decimal? itemamount { get; set; }
		public decimal? unitprice { get; set; }

		public DateTime? itemdeliverydate { get; set; }
		public DateTime? docdate { get; set; }
		public string crcy { get; set; }
		
		public decimal? NetPrice { get; set; }
		public decimal? NetValue { get; set; }
		
		public bool dataloaderror { get; set; }
		public string error_description { get; set; }
		public string purchdoc { get; set; }
		public string material { get; set; }
		public string vendor { get; set; }
		public string vendorname { get; set; }
		public string projectdefinition { get; set; }
		public decimal? poquantity { get; set; }
		public string dci { get; set; }
		public decimal? deliveredqty { get; set; }
		public decimal? wmsqty { get; set; }
		public string saleorderno { get; set; }
		public string solineitemno { get; set; }
		public string saleordertype { get; set; }
		public string codetype { get; set; }
		public string costcenter { get; set; }
		public string assetno { get; set; }
		public string projecttext { get; set; }
		public string sloc { get; set; }

		public string mscode { get; set; }
		public string plant { get; set; }
		public string linkageno { get; set; }
		public string assetsubno { get; set; }
		public string ordernumber { get; set; }
		public string wbselement { get; set; }
		public decimal? exchangerate { get; set; }
		public string description { get; set; }
		public DateTime? dateofcreation { get; set; }
		public string ProjectManagerName { get; set; }
		public string globalpmno { get; set; }
		public string pgr { get; set; }
		public string shiptoparty { get; set; }
		


	}

	public class StagingStockModel
	{
		public string uploadedfilename { get; set; }
		public string uploadbatchcode { get; set; }
		public string uploadedby { get; set; }
		public string material { get; set; }
		public string materialdescription { get; set; }
		public string store { get; set; }
		public string rack { get; set; }
		public string bin { get; set; }
		public decimal? quantity { get; set; }
		public string grn { get; set; }
		public DateTime receiveddate { get; set; }
		public DateTime shelflifeexpiration { get; set; }
		public DateTime dateofmanufacture { get; set; }
		public string datasource { get; set; }
		public string dataenteredby { get; set; }
		public string dataenteredon { get; set; }
		public DateTime createddate { get; set; }
		public string stocktype { get; set; }
		public string category { get; set; }
		public Decimal? unitprice { get; set; }
		public Decimal? value { get; set; }
		public string pono { get; set; }
		public string projectid { get; set; }


	}
	public class LocationModel
	{
		public int locatorid { get; set; }
		public string locatorname { get; set; }
		public int rackid { get; set; }
		public string racknumber { get; set; }
		public int binid { get; set; }
		public string binnumber { get; set; }
		public bool deleteflag { get; set; }
		public DateTime createdate { get; set; }
		public int maximumcapacity { get; set; }
		public bool isexcelupload { get; set; }
		public string materialid { get; set; }
		public string materialdescription { get; set; }
	}

	public class MateriallabelModel
	{
		public string po { get; set; }
		public string polineitemno { get; set; }
		public string  description { get; set; }
		public string serialno { get; set; }
		public string material { get; set; }
		public string mscode { get; set; }
		public string saleorderno { get; set; }
		public string solineitemno { get; set; }
		public string saleordertype { get; set; }
		public string insprec { get; set; }
		public string linkageno { get; set; }
		public string customername { get; set; }
		public string shipto { get; set; }
		public string soldto { get; set; }
		public string plant { get; set; }
		public string gr { get; set; }
		public string shippingpoint { get; set; }
		public string projectiddef { get; set; }
		public DateTime? loadingdate { get; set; }
		public string custpo { get; set; }
		public string partno { get; set; }
		public string grno { get; set; }
		public string codetype { get; set; }
		public string error_description { get; set; }
		public bool isloaderror { get; set; }

		public string uploadcode { get; set; }
		//properties fot pomaterials
		public int id { get; set; }
		public string pono { get; set; }
		public string materialid { get; set; }
		public int itemno { get; set; }
		public string podescription { get; set; }

		/////property for insert in master table

		public string vendorcode { get; set; }
		public string vendorname { get; set; }
		public string materialdescription { get; set; }
		public int materialqty { get; set; }
		public decimal? itemamount { get; set; }
		public DateTime? itemdeliverydate { get; set; }
		public string projectcode { get; set; }
		public string assetno { get; set; }
		public string assetsubno { get; set; }
		public string costcenter { get; set; }
		public string projecttext { get; set; }
		public string saleordertypetext { get; set; }
		public string customercode { get; set; }
		public string custpolineitem { get; set; }
		public string costcentertext { get; set; }
	public string serviceorderno { get; set; }

	}

	public class AuditLog
    {
		public int id { get; set; }
		public string filename { get; set; }
		public string filelocation { get; set; }
		public DateTime? uploadedon { get; set; }
		public string uploadedby { get; set; }
		public string uploadedto { get; set; }
		public string modulename { get; set; }
		public int successrecords { get; set; }
		public int exceptionrecords { get; set; }
		public int totalrecords { get; set; }

		public string uploadcode { get; set; }

	}
	public class MaterialStage
	{
		public string material { get; set; }
		public string plant { get; set; }
		public string hsncode { get; set; }
		public DateTime createddate { get; set; }
		public bool dataloaderror { get; set; }
		public string error_description { get; set; }
	}

	public class SOdataExcel
	{
		public string sono { get; set; }
		public string projectdef { get; set; }
		public string serviceorderno { get; set; }
		public string soitemno { get; set; }
		public string customercode { get; set; }
		public string customername { get; set; }
	}

	public class CostCenterData
    {
		public string costcenter { get; set; }
		public string shorttext { get; set; }

	}

	public class AssetData
	{
		public string material { get; set; }
		public string materialdescription { get; set; }

	}
}

