import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl, ValidatorFn } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { materialRequestDetails, FIFOValues } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { ConfirmationService } from 'primeng/api';
import { DatePipe } from '@angular/common';
import { isNullOrUndefined } from 'util';
@Component({
  selector: 'app-MaterialIsuue',
  templateUrl: './MaterialIssue.component.html'
})
export class MaterialIssueComponent implements OnInit {
  roindex: any;
  itemreceiveddate: string;

  constructor(private datePipe: DatePipe, private ConfirmationService: ConfirmationService, private formBuilder: FormBuilder, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public formName: string;
  public txtName: string;
  public dynamicData = new DynamicSearchResult();
  public showList: boolean = false;
  public searchItems: Array<searchList> = [];
  public selectedlist: Array<searchList> = [];
  public selectedItem: searchList;
  public searchresult: Array<object> = [];
  public AddDialog: boolean;
  public id: string;
  public MaterialRequestForm: FormGroup
  public materialissueList: Array<any> = [];
  public employee: Employee;
  public displayItemRequestDialog; RequestDetailsSubmitted: boolean = false;
  public materialRequestDetails: materialRequestDetails;
  public requestId: string;
  public pono: string = "";
  public Oldestdata: FIFOValues;
  public itemlocationData: Array<any> = [];
  public itemlocationsaveData: Array<any> = [];
  public showavailableqtyList: boolean = false;
  public showissueqtyOKorCancel: boolean = true;
  public showdialog: boolean = false;
  public txtDisable: boolean = true;
  public FIFOvalues: FIFOValues;
  public reqqty; reservedQty: number;
  public btndisable: boolean = false;
  public issueqtyenable: boolean = true;
  //Email
  reqid: string = "";
  // materialissueList: string = "";
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    //Email
    this.reqid = this.route.snapshot.queryParams.requestid;
    this.itemlocationsaveData = [];
    if (this.reqid) {
      debugger;
      //get material details for that requestid
      // this.materialissueList[0].requestid = this.reqid;
      this.getmaterialIssueListbyrequestid();

    }

    this.route.params.subscribe(params => {
      if (params["requestid"]) {
        this.requestId = params["requestid"];
      }
      if (params["pono"]) {
        this.pono = params["pono"];
      }
    });

    this.FIFOvalues = new FIFOValues();
    this.spinner.show();
    this.getmaterialIssueListbyrequestid();

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
    var data2 = this.itemlocationData.filter(function (element) {
      return element.initialstock == true && !(element.issuedqty);
    });
    if (data2.length > 0) {
      this.showAlert();
    }
    if (data2.length == 0) {
      this.issueOnconfirm();
    }
  }
  Cancel() {
    this.AddDialog = false;
  }

  issueOnconfirm() {
    var material = this.materialissueList[this.roindex].requestmaterialid;
    this.itemlocationsaveData = this.itemlocationsaveData.filter(function (element, index) {
      return (element.requestmaterialid != material);
    });
    var totalissuedqty = 0;
    this.itemlocationData.forEach(item => {
      if (item.issuedqty)
        item.requestforissueid = this.materialissueList[this.roindex].requestforissueid;
      item.itemreturnable = this.materialissueList[this.roindex].itemreturnable;
      item.approvedby = this.employee.employeeno;
      item.itemreceiverid = this.materialissueList[this.roindex].itemreceiverid;
      item.requestid = this.materialissueList[this.roindex].requestid;
      item.requestmaterialid = this.materialissueList[this.roindex].requestmaterialid;
      item.requesttype = "MaterialRequest";
      item.createdby = this.materialissueList[this.roindex].requesterid;
      totalissuedqty = totalissuedqty + (item.issuedqty);
      this.FIFOvalues.issueqty = totalissuedqty;
      this.itemlocationsaveData.push(item);
      //item.issuedqty = this.FIFOvalues.issueqty;
      //item.issuedquantity = totalissuedqty;
      //item.issuedqty = totalissuedqty;

    });

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
  showAlert() {
    this.ConfirmationService.confirm({
      message: 'You are not issued from initial stock, Would you like to continue? ',
      header: 'Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.messageService.add({ severity: 'info', summary: '', detail: 'You have accepted' });
        this.issueOnconfirm();
      },
      reject: () => {
        //this.messageService.add({ severity: 'info', summary: '', detail: 'You have ignored' });
      }
    });
  }
  //shows list of items for particular material
  showmateriallocationList(material, id, rowindex, qty, issuedqty, reservedqty, requestforissueid, requestmaterialid, poitemdescription: string) {
    if (issuedqty <= qty) {
      this.issueqtyenable = true;
    }
    else {
      this.issueqtyenable = false;
    }
    this.reqqty = qty;
    this.reservedQty = reservedqty;
    this.id = id;
    this.AddDialog = true;
    this.roindex = rowindex;
    this.itemlocationData = [];
    if (this.constants.materialIssueType == "Pending") {
      this.issueqtyenable = false;
      this.wmsService.getItemlocationListByMaterialanddesc(material, poitemdescription).subscribe(data => {
        this.itemlocationData = data;
        console.log(this.itemlocationData);
        this.showdialog = true;
      });
    }
    else {
      this.issueqtyenable = true;
      this.wmsService.getItemlocationListByIssueId(requestmaterialid, 'MaterialRequest').subscribe(data => {
        this.itemlocationData = data;
        this.showdialog = true;
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
  //check issued quantity
  checkissueqty($event, entredvalue, maxvalue, material, createddate, rowdata: any) {
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

      this.wmsService.checkoldestmaterialwithdesc(material, createddate, rowdata.materialdescription).subscribe(data => {
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
  //Email
  getmaterialIssueListbyrequestid() {
    this.wmsService.getmaterialIssueListbyrequestid(this.requestId, this.pono).subscribe(data => {
      this.spinner.hide();
      this.materialissueList = data;

      if (this.materialissueList.length != 0)
        this.showavailableqtyList = true;
      this.materialissueList.forEach(item => {
        //if (!item.issuedquantity)
        //  item.issuedquantity = item.requestedquantity;
        if (item.issuedqty >= item.requestedquantity) {
          this.showissueqtyOKorCancel = true;
          this.btndisable = false;
        }

        //(<HTMLInputElement>document.getElementById('footerdiv')).style.display = "none";
      });
    });
  }

  //check validations for issuer quantity
  reqQtyChange(data: any) {
    if (data.issuedqty > data.quantity) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'issued Quantity should be lessthan or equal to available quantity' });
      data.issuedqty = data.quantity;
    }
  }
  backtoDashboard() {
    this.router.navigateByUrl("/WMS/MaterialIssueDashboard");
  }


  //requested quantity update
  onMaterialIssueDeatilsSubmit() {
    this.spinner.show();

    //this.wmsService.UpdateMaterialqty(this.itemlocationData).subscribe(data => {
    //  if (data == 1) {
    //this.btndisable = false;
    //this.itemlocationData.forEach(item => {

    // // item.issuedquantity = this.itemlocationData.issuedquantity;

    //});
    //this.materialissueList.forEach(item => {
    //  if (item.issuedqty != 0)

    //   // totalissuedqty = totalissuedqty + (item.issuedquantity);
    //  // this.FIFOvalues.issueqty = totalissuedqty;
    //  item.issuedqty = this.FIFOvalues.issueqty;

    //});
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
      this.router.navigateByUrl('WMS/MaterialIssueDashboard');
    });
  }
  //})
  //}
}
