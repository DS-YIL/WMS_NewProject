import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee } from '../Models/Common.Model';
import { FIFOValues } from 'src/app/Models/WMS.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { ConfirmationService } from 'primeng/api';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-ReceiveSTORequest',
  templateUrl: './ReceiveSTORequest.component.html'
})

export class ReceiveSTORequestComponent implements OnInit {

  constructor(private datePipe: DatePipe, private ConfirmationService: ConfirmationService, private messageService: MessageService, private wmsService: wmsService, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }
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

  ngOnInit() {
    this.STORequestList = [];
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.selectedStatus = "Pending";
    this.FIFOvalues = new FIFOValues();
    this.getSTORequestList();
  }

  //get material issue list based on loginid/7
  getSTORequestList() {
    this.STORequestList = [];
    this.wmsService.getSTORequestList().subscribe(data => {
      this.STORequestList = data;
      this.FilteredSTORequestList = this.STORequestList.filter(li => li.status == this.selectedStatus || li.status == null);
    });
  }

  onSelectStatus(event) {
    this.selectedStatus = event.target.value;
    this.FilteredSTORequestList = this.STORequestList.filter(li => li.status == this.selectedStatus);
  }

  navigateToMatIssue(details: any) {
    this.showMatDetails = true;
    this.showavailableStock = true;
    this.materialissueList = [];
    this.transferId = details.transferid;
    this.requestedBy = details.transferredby;
    var type = "MatIssue";
    this.wmsService.getMatdetailsbyTransferId(details.transferid, type).subscribe(data => {
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

      this.wmsService.checkoldestmaterial(material, createddate).subscribe(data => {
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
  showmateriallocationList(material, id, rowindex, qty, issuedqty) {
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
      this.wmsService.getItemlocationListByMaterial(material).subscribe(data => {
        this.itemlocationData = data;
        this.showdialog = true;
      });
    }
    else {
      this.issueqtyenable = true;
      this.wmsService.getItemlocationListByIssueId(this.transferId).subscribe(data => {
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
    var material = this.itemlocationData[0].materialid;
    this.itemlocationsaveData = this.itemlocationsaveData.filter(function (element, index) {
      return (element.materialid != material);
    });
    var totalissuedqty = 0;
    if (this.itemlocationData.length > 0) {
      this.itemlocationData.forEach(item => {
        if (item.issuedqty)
          item.requestforissueid = this.materialissueList[this.roindex].requestforissueid;
        item.itemreturnable = this.materialissueList[this.roindex].itemreturnable;
        item.approvedby = this.employee.employeeno;
        item.itemreceiverid = this.materialissueList[this.roindex].itemreceiverid;
        item.requestid = this.materialissueList[this.roindex].transferid;
        item.requestmaterialid = this.materialissueList[this.roindex].requestmaterialid;
        item.requesttype = "STO";
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
    this.showMatDetails = true;
    this.showavailableStock = false;
    this.materialissueList = [];
    this.transferId = details.transferid;
    var type = "POInitiate";
    this.wmsService.getMatdetailsbyTransferId(details.transferid, type).subscribe(data => {
      this.spinner.hide();
      this.materialissueList = data;
    });
  }

  InitiatePO() {
    this.spinner.show();
    this.materialissueList[0].uploadedby = this.employee.employeeno;
    this.wmsService.STOPOInitiate(this.materialissueList).subscribe(data => {
      this.spinner.hide();
      if (data)
        this.messageService.add({ severity: 'success', summary: '', detail: 'PO Creation Request Sent.' });
      else
        this.messageService.add({ severity: 'error', summary: '', detail: 'PO Creation Request Failed.' });
      this.showMatDetails = false;
    });
  }

  backtoDashboard() {
    this.showMatDetails = false;
  }

}
