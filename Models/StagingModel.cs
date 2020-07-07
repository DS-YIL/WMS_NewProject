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
        public DateTime deliverydate { get; set; }
        public int vendorid { get; set; }
        public string suppliername { get; set; }
        public string jobname { get; set; }
        public string projectcode { get; set; }
        public string projectname { get; set; }
        public string projectmanager { get; set; }
        public string materialid { get; set; }
        public string materialdescription { get; set; }
        public int materialqty { get; set; }
        public int itemno { get; set; }
        public int itemamount { get; set; }
        public DateTime itemdeliverydate { get; set; }


        public string purchdoc { get; set; }
        public string material { get; set; }

        public string vendor { get; set; }
        public string vendorname { get; set; }
        public string projectdefinition { get; set; }
        public int poquantity { get; set; }

    }
}