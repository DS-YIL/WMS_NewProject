import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee } from '../Models/Common.Model';
import { FIFOValues } from 'src/app/Models/WMS.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { ConfirmationService } from 'primeng/api';
import { DatePipe } from '@angular/common';
import { isNullOrUndefined } from 'util';

@Component({
  selector: 'app-ReceiveSubContractRequest',
  templateUrl: './ReceiveSubContractRequest.component.html'
})

export class ReceiveSubContractRequestComponent implements OnInit {

  constructor(private datePipe: DatePipe, private ConfirmationService: ConfirmationService, private messageService: MessageService, private wmsService: wmsService, private router: Router, private route: ActivatedRoute, public constants: constants, private spinner: NgxSpinnerService) { }
  public selectedStatus: string;
  public STORequestList: Array<any> = [];
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
  requestedid: string;
  vendorname: string;
  sourcelocation: string;
  pcode: string = "";
  pono: string = "";
  isplantstockrequest: boolean = false;

  ngOnInit() {
    this.STORequestList = [];
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.requestedid = this.route.snapshot.queryParams.requestid;
    this.selectedStatus = "Pending";
    this.vendorname = "";
    this.sourcelocation = "";
    this.isplantstockrequest = false;
    this.FIFOvalues = new FIFOValues();
    this.getSTORequestList();
  }

  //get material issue list based on loginid/7
  getSTORequestList() {
    this.STORequestList = [];
    this.spinner.show();
    this.wmsService.getSTORequestList('SubContract').subscribe(data => {
      this.spinner.hide();
      this.STORequestList = data;
      this.FilteredSTORequestList = this.STORequestList.filter(li => li.status == this.selectedStatus || li.status == null);
      if (!isNullOrUndefined(this.requestedid) && this.requestedid != "") {
        this.FilteredSTORequestList = this.FilteredSTORequestList.filter(li => li.transferid == this.requestedid);
      }
    });
  }

  onSelectStatus(event) {
    this.selectedStatus = event.target.value;
   
    if (this.selectedStatus == "Issued") {
      this.FilteredSTORequestList = this.STORequestList.filter(li => li.status == this.selectedStatus || li.status == "InBound" || li.status == "OutBound");
    }
    else {
      this.FilteredSTORequestList = this.STORequestList.filter(li => li.status == this.selectedStatus);
    }
  }

  navigateToMatIssue(details: any) {
    this.showMatDetails = true;
    this.showavailableStock = true;
    this.materialissueList = [];
    this.isplantstockrequest = false;
    this.transferId = details.transferid;
    this.requestedBy = details.transferredby;
    this.vendorname = details.vendorname;
    this.sourcelocation = details.sourceplant;
    this.pcode = details.projectcode;
    this.pono = details.pono;
    var type = "MatIssue";
    if (String(details.materialtype).toLowerCase() == "plant") {
      this.isplantstockrequest = true;
    }
    this.wmsService.getMatdetailsbyTransferId(details.transferid, type, 'SubContract').subscribe(data => {
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
    var id = $event.target.id;
    if (entredvalue > maxvalue) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Please enter issue quantity less than Available quantity' });
      rowdata.issuedqty = 0;
      (<HTMLInputElement>document.getElementById(id)).value = "";
    }
    else {
      var matdesc = this.itemlocationData[0].materialdescription;
      this.wmsService.checkoldestmaterialwithdescstore(material, createddate, matdesc, this.sourcelocation).subscribe(data => {
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
  showmateriallocationList(material, id, rowindex, qty, issuedqty, description: string, rid: any, pono: string) {
    debugger;
    if (issuedqty <= qty) {
      this.issueqtyenable = true;
    }
    else {
      this.issueqtyenable = false;
    }
    this.reqqty = qty;
    this.id = id;
    this.AddDialog = true;
    this.roindex = rowindex;
    this.itemlocationData = [];
    if (this.selectedStatus == "Pending") {
      this.issueqtyenable = false;
      if (this.isplantstockrequest) {
        this.wmsService.getlocationsforplantstockissue(material, description).subscribe(data => {
          this.itemlocationData = data;
          this.showdialog = true;
        });
      }
      else {
        this.wmsService.getItemlocationListByMaterialdescstore_v1(material, description, this.pcode, pono, this.sourcelocation).subscribe(data => {
          debugger;
          this.itemlocationData = data;
          this.showdialog = true;
        });
      }
     
    }
    else {
      this.issueqtyenable = true;
      this.wmsService.getItemlocationListByIssueIdWithStore(String(rid), 'SubContract').subscribe(data => {
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
        item.pono = this.materialissueList[this.roindex].pono;
        item.createdby = this.STORequestList[0].transferredby;
        item.itemreceiverid = this.materialissueList[this.roindex].itemreceiverid;
        item.requestid = this.materialissueList[this.roindex].id;
        item.requestmaterialid = this.materialissueList[this.roindex].requestmaterialid;
        item.transferid = this.materialissueList[this.roindex].transferid;
        item.createdby = this.materialissueList[this.roindex].createdby;
        item.requesttype = "SubContract";
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

  backtoDashboard() {
    this.showMatDetails = false;
  }

}
