import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee } from '../Models/Common.Model';
import { FIFOValues, Issuestatus } from 'src/app/Models/WMS.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { ConfirmationService } from 'primeng/api';
import { DatePipe } from '@angular/common';
import { isNullOrUndefined } from 'util';

@Component({
  selector: 'app-ReceiveSTORequest',
  templateUrl: './ReceiveSTORequest.component.html'
})

export class ReceiveSTORequestComponent implements OnInit {

  constructor(private datePipe: DatePipe, private ConfirmationService: ConfirmationService, private messageService: MessageService, private wmsService: wmsService, private router: Router, private route: ActivatedRoute, public constants: constants, private spinner: NgxSpinnerService) { }
  public selectedStatus: string;
  public STORequestList: Array<any> = [];
  public STOALLRequestList: Array<any> = [];
  public FilteredSTORequestList: Array<any> = [];
  public employee: Employee;
  public showMatDetails; btndisable; AddDialog; showdialog; showavailableStock: boolean = false;
  public materialissueList: Array<any> = [];
  public showissueqtyOKorCancel; issueqtyenable: boolean = true;
  public transferId; requestedBy: string;
  public itemlocationData: Array<any> = [];
  public itemlocationsaveData: Array<any> = [];
  public reqqty: number;
  public roindex: any;
  public FIFOvalues; Oldestdata: FIFOValues;
  public id: string;
  public txtDisable: boolean = true;
  public itemreceiveddate: string;
  public STONO: string;
  public matdesc: string;
  public viewprocess: boolean = false;
  source: string;
  destination: string;
  requestedid: string;
  pcode: string;
  pono: string;
  isplantstockrequest: boolean = false;
  remarksheadertext: string = "";
  displayRemarks: boolean = false;
  statusremarks: string = "";
  lblstatus: string = "";
  lblstatusramarks: string = "";
  lblonholdrejectedby: string = "";
  lblonholdrejectedon: string = "";
  statusmodel: Issuestatus;
  ngOnInit() {
    this.STORequestList = [];
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.requestedid = this.route.snapshot.queryParams.requestid;
    this.selectedStatus = "Pending";
    this.source = "";
    this.remarksheadertext = "";
    this.displayRemarks = false;
    this.statusremarks = "";
    this.lblstatus = "";
    this.lblstatusramarks = "";
    this.statusmodel = new Issuestatus();
    this.destination = "";
    this.viewprocess = false;
    this.isplantstockrequest = false;
    this.matdesc = "";
    this.lblonholdrejectedby = "";
    this.lblonholdrejectedon = "";
    this.FIFOvalues = new FIFOValues();
   
    this.getSTORequestList();
  }

  //get material issue list based on loginid/7
  getSTORequestList() {
    this.STORequestList = [];
    this.wmsService.getSTORequestList('STO').subscribe(data => {
      this.STORequestList = data;
      debugger;
      var selectedsts = this.selectedStatus;
      var data1 = this.STORequestList.filter(function (element, index) {
        return (element.status == selectedsts || element.status == null);
      });
      this.FilteredSTORequestList = data1;
      if (!isNullOrUndefined(this.requestedid) && this.requestedid != "") {
        this.FilteredSTORequestList = this.FilteredSTORequestList.filter(li => li.transferid == this.requestedid);
      }
    });
  }

  onSelectStatus(event) {
    debugger;
    this.selectedStatus = event.target.value;
    var selsts = this.selectedStatus;
    if (this.selectedStatus == "Pending") {
      var data1 = this.STORequestList.filter(function (element, index) {
        return (element.status == selsts || element.status == null);
      });
      this.FilteredSTORequestList = data1;
    }
    else {
      var data1 = this.STORequestList.filter(function (element, index) {
        return (element.status == selsts);
      });
      this.FilteredSTORequestList = data1;
    }
    
  }
  

  navigateToMatIssue(details: any) {
    debugger;
    this.showMatDetails = true;
    this.showavailableStock = true;
    this.isplantstockrequest = false;
    this.materialissueList = [];
    this.transferId = details.transferid;
    this.requestedBy = details.transferredby;
    this.source = details.sourceplant;
    this.destination = details.destinationplant;
    this.pcode = details.projectcode;
    this.pono = details.pono;
    this.lblstatus = details.status;
    this.lblstatusramarks = details.statusremarks;
    this.lblonholdrejectedby = details.statuschangeby;
    this.lblonholdrejectedon = details.statuschangedon;
    var sts = details.status;
    if (sts == "Issued") {
      this.viewprocess = true;
    }
    else {
      this.viewprocess = false;
    }
    if (String(details.materialtype).toLowerCase() == "plant") {
      this.isplantstockrequest = true;
    }
    var type = "MatIssue";
    this.wmsService.getMatdetailsbyTransferId(details.transferid, type, 'STO').subscribe(data => {
      this.spinner.hide();
      this.materialissueList = data;
      this.materialissueList.forEach(item => {
        if (item.issuedqty >= item.transferqty) {
          this.showissueqtyOKorCancel = true;
          this.btndisable = false;
        }
      });
    });
  }


  //check issued quantity
  checkissueqty($event, entredvalue, maxvalue, material, createddate, rowdata: any) {
    debugger;
    if (entredvalue < 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter value grater than 0' });
      rowdata.issuedqty = 0;
      return;
    }
    if (isNullOrUndefined(entredvalue) || entredvalue == 0) {
      rowdata.issuedqty = 0;
      return;
    }
    var id = $event.target.id;
    if (entredvalue > maxvalue) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Please enter issue quantity less than Available quantity' });
      rowdata.issuedqty = 0;
      (<HTMLInputElement>document.getElementById(id)).value = "";
    }
    else {

      this.wmsService.checkoldestmaterialwithdescstore(material, createddate, this.matdesc, this.source).subscribe(data => {
        this.Oldestdata = data;
        if (data != null) {
          this.alertconfirm(this.Oldestdata);
        }
        //this.calculateTotalQty();
        //this.calculateTotalPrice();
        this.spinner.hide();
      });
    }
  }

  holdreject(data: any, status: string) {
    this.statusmodel = new Issuestatus();
    this.statusmodel.requestid = data.transferid;
    this.statusmodel.status = status;
    this.statusmodel.requestedby = data.requesterid;
    this.statusmodel.issuerstatuschangeby = this.employee.employeeno;
    this.statusmodel.type = "STO";
   
    if (status == "On Hold") {
      this.remarksheadertext = "Are you sure to put request on hold ?";
    }
    if (status == "Rejected") {
      this.remarksheadertext = "Are you sure to reject the request ?";
    }
    this.displayRemarks = true;
   
  }
  submitstatus() {
    debugger;
    if (this.statusremarks.trim() == "") {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Remarks' });
      return;
    }
    this.statusmodel.issuerstatusremarks = this.statusremarks;
    var msg = "";
    var errormsg = "";
    if (this.statusmodel.status == "On Hold") {
      msg = "On hold successful";
      errormsg = "On hold failed";
    }
    if (this.statusmodel.status == "Rejected") {
      msg = "Rejection successful";
      errormsg = "Rejection failed";
    }
    this.wmsService.updateIssuerstatus(this.statusmodel).subscribe(data => {
      if (data) {
        this.messageService.add({ severity: 'success', summary: '', detail: msg });
        this.getSTORequestList();
      }
      else {
        this.messageService.add({ severity: 'success', summary: '', detail: errormsg });
      }
      this.canclestatus();

    });

  }
  canclestatus() {
    this.remarksheadertext = "";
    this.displayRemarks = false;
    this.statusremarks = "";
    this.statusmodel = new Issuestatus();
  }
  //show alert about oldest item location
  alertconfirm(data) {
    var info = data;
    this.itemreceiveddate = this.datePipe.transform(data.createddate, 'dd/MM/yyyy');
    this.ConfirmationService.confirm({
      message: 'Same Material received on ' + this.itemreceiveddate + ' and placed in ' + data.itemlocation + '  location, Would you like to continue?',
      header: 'Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {

        this.messageService.add({ severity: 'info', summary: '', detail: 'You have accepted' });
      },
      reject: () => {

        this.messageService.add({ severity: 'info', summary: '', detail: 'You have ignored' });
      }
    });
  }
  //shows list of items for particular material
  showmateriallocationList(material, id, rowindex, qty, issuedqty, poitemdescription: string, rid: any, pono: string) {
    debugger;
    if (issuedqty <= qty) {
      this.issueqtyenable = true;
    }
    else {
      this.issueqtyenable = false;
    }
    this.matdesc = poitemdescription;
    this.reqqty = qty;
    this.id = id;
    this.AddDialog = true;
    this.roindex = rowindex;
    this.itemlocationData = [];
    if (this.selectedStatus == "Pending" || this.selectedStatus == "On Hold" && !this.viewprocess) {
      this.issueqtyenable = false;
      if (this.isplantstockrequest) {
        this.wmsService.getlocationsforplantstockissue(material, poitemdescription).subscribe(data => {
          this.itemlocationData = data;
          this.showdialog = true;
        });
      }
      else {
        this.wmsService.getItemlocationListByMaterialdescstore_v1(material, poitemdescription, this.pcode, pono, this.source).subscribe(data => {
          this.itemlocationData = data;
          this.showdialog = true;
        });
      }
     
    }
    else {
      this.issueqtyenable = true;
      this.wmsService.getItemlocationListByIssueIdWithStore(String(rid), 'STO').subscribe(data => {
        this.itemlocationData = data;
        this.showdialog = true;
      });
    }
  }

  issuematerial(itemlocationData) {
    debugger;
    var data1 = this.itemlocationData.filter(function (element, index) {
      return (element.issuedqty > element.availableqty);
    });
    if (data1.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Issue Qty cannot exceed available Qty.' });
      return;
    }
    var material = this.materialissueList[this.roindex].id;
    this.itemlocationsaveData = this.itemlocationsaveData.filter(function (element, index) {
      return (element.requestid != material);
    });
    var totalissuedqty = 0;
    if (this.itemlocationData.length > 0) {
      this.itemlocationData.forEach(item => {
        if (item.issuedqty)
          item.requestforissueid = this.materialissueList[this.roindex].requestforissueid;
        item.itemreturnable = this.materialissueList[this.roindex].itemreturnable;
        item.approvedby = this.employee.employeeno;
        item.itemreceiverid = this.materialissueList[this.roindex].itemreceiverid;
        item.requestid = this.materialissueList[this.roindex].id;
        item.requestmaterialid = this.materialissueList[this.roindex].requestmaterialid;
        item.transferid = this.materialissueList[this.roindex].transferid;
        item.createdby = this.materialissueList[this.roindex].createdby;
        item.pono = this.materialissueList[this.roindex].pono;
        item.requesttype = "STO";
        if (this.isplantstockrequest) {
          item.materialtype = "Plant";
        }
        else {
          item.materialtype = "Project"
        }
        totalissuedqty = totalissuedqty + (item.issuedqty);
        this.FIFOvalues.issueqty = totalissuedqty;
        this.itemlocationsaveData.push(item);

      });
    }
    

    if (totalissuedqty > this.reqqty) {
      this.messageService.add({ severity: 'error', summary: '', detail: ' Issue Qty cannot exceed Requested Qty' });
      this.AddDialog = true;
      return;
    }
    else {

      (<HTMLInputElement>document.getElementById(this.id)).value = totalissuedqty.toString();
      this.materialissueList[this.roindex].issuedqty = totalissuedqty;
      this.txtDisable = true;
      this.AddDialog = false;
    }
    this.btndisable = true;

  }
  Cancel() {
    this.AddDialog = false;
  }
  //requested quantity update
  onMaterialIssueDeatilsSubmit() {
    this.spinner.show();
    var data1 = this.itemlocationsaveData.filter(function (element, index) {
      return (element.issuedqty > 0);
    });
    if (data1.length == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Issue quantity' });
      this.spinner.hide();
      return;
    }

    this.itemlocationsaveData = this.itemlocationsaveData.filter(function (element, index) {
      return (element.issuedqty > 0);
    });

    this.itemlocationsaveData.forEach(item => {
      item.projectid = this.pcode;
    })

    this.wmsService.approvematerialrequest(this.itemlocationsaveData).subscribe(data => {
      this.spinner.hide();
      this.btndisable = false;
      this.itemlocationsaveData = [];
      if (data)
        this.messageService.add({ severity: 'success', summary: '', detail: 'Material issued.' });
      else
        this.messageService.add({ severity: 'error', summary: '', detail: 'Material issue failed.' });
      this.showMatDetails = false;
      this.getSTORequestList();
    });
  }

  //initiate PO Starts
  navigateToPOInitiate(details: any) {
    debugger;
    this.showMatDetails = true;
    this.showavailableStock = false;
    this.materialissueList = [];
    this.transferId = details.transferid;
    var type = "POInitiate";
    this.wmsService.getMatdetailsbyTransferId(details.transferid, type, 'STO').subscribe(data => {
      this.spinner.hide();
      debugger;
      this.materialissueList = data;
      this.materialissueList = this.materialissueList.filter(li => li.issuedqty == null || li.issuedqty == 0 || li.issued < li.transferqty);

      this.materialissueList.forEach(item => {
        debugger;
        if (isNullOrUndefined(item.transferqty)) {
          item.transferqty = 0;
        }
        if (isNullOrUndefined(item.issuedqty)) {
          item.issuedqty = 0;
        }
        if (isNullOrUndefined(item.availableqty)) {
          item.availableqty = 0;
        }
        item.poqty = parseInt(item.transferqty) - parseInt(item.issuedqty);
      })

    });
  }

  InitiatePO() {
    debugger;
    this.materialissueList[0].uploadedby = this.employee.employeeno;
    var data1 = this.materialissueList.filter(function (element, index) {
      return (isNullOrUndefined(element.poqty) || element.poqty == 0);
    });
    if (data1.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter PO quantity' });
      return;
    }
    var data2 = this.materialissueList.filter(function (element, index) {
      return (element.poqty < (parseInt(element.transferqty) - parseInt(element.issuedqty)));
    });
    if (data2.length > 0) {
      var mat = data2[0].materialid;
      var matdesc = data2[0].poitemdescription;
      var minqqty = parseInt(data2[0].transferqty) - parseInt(data2[0].issuedqty)
      var msg = "Minimun PO quantity should be " + String(minqqty) + " for Material : " + mat + " and PO Item Description: " + matdesc+""
      this.messageService.add({ severity: 'error', summary: '', detail: msg });
      return;
    }
    this.spinner.show();
    this.wmsService.STOPOInitiate(this.materialissueList).subscribe(data => {
      this.spinner.hide();
      if (String(data) == "Sucess")
        this.messageService.add({ severity: 'success', summary: '', detail: 'PO Creation Request Sent.' });
      else
        this.messageService.add({ severity: 'error', summary: '', detail: String(data) });
      this.showMatDetails = false;
      this.getSTORequestList();
    });
  }

  backtoDashboard() {
    this.showMatDetails = false;
  }

}
