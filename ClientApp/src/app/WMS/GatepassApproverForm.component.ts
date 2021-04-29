import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList, rbamaster } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { gatepassModel, materialistModel, FIFOValues, locationdetails } from '../Models/WMS.Model';
import { DatePipe } from '@angular/common';
import { ConfirmationService } from 'primeng/api';
import { isNullOrUndefined } from 'util';
import { strict } from 'assert';
@Component({
  selector: 'app-GatePassApprover',
  templateUrl: './GatePassApproverForm.component.html',
  providers: [DatePipe]
})
export class GatePassApproverComponent implements OnInit {
  txtDisable: boolean = true;
    itemreceiveddate: string;
  // btnDisableformaterial: boolean=false;
  constructor(private ConfirmationService: ConfirmationService, private formBuilder: FormBuilder, private messageService: MessageService, private wmsService: wmsService, private datePipe: DatePipe, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }
  public AddDialog: boolean;
  public id: any;
  roindex: any;
  public issuedqty: number;
  public showdialog: boolean;
  public employee: Employee;
  public materialList: Array<any> = [];
  public gatepassModel: gatepassModel;
  public locdetails=new locationdetails();
  public btnDisable: boolean = false;
  public btnDisableissue: boolean = false;
  public itemlocationData: Array<any> = [];
  public issueFinalList: Array<any> = [];
  public itemissuedloc: Array<any> = [];
  public Oldestdata: FIFOValues;
  public FIFOvalues: FIFOValues;
  public gatePassApprovalList: Array<any> = [];
  public showHistory: boolean = false;
  public reqqty; reservedQty: number;
  public material: string = "";
  public matdesc: string = "";
  public issueqtyenable: boolean = true;
  rbalist: rbamaster[];
  selectedrba: rbamaster;
  //Email
  gateid: string = "";
  ngOnInit() {
    debugger;
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.selectedrba = new rbamaster();
    this.rbalist = [];
    if (localStorage.getItem("rbalist")) {
      this.rbalist = JSON.parse(localStorage.getItem("rbalist")) as rbamaster[];
      var filterdt = this.rbalist.filter(o => o.roleid == parseInt(this.employee.roleid));
      if (filterdt.length > 0) {
        this.selectedrba = filterdt[0];
      }

    }
    this.route.params.subscribe(params => {
      if (params["gatepassid"]) {
        var gatepassId = params["gatepassid"]
        this.bindMaterilaDetails(gatepassId);
        if (this.selectedrba.gatepass_approval) {
          this.getGatePassHistoryList(gatepassId);
        }
      }
    });

    //Email
    this.gateid = this.route.snapshot.queryParams.gatepassid;
    if (this.gateid) {
      debugger;
      //get material details for that gatepassid
      this.materialList[0] = this.gateid;
      this.updategatepassapproverstatus();

    }

    this.gatepassModel = new gatepassModel();
    this.gatepassModel.approverstatus = "Approved";

  }


  //get gatepass list
  bindMaterilaDetails(gatepassId: any) {
    debugger;
    this.wmsService.gatepassmaterialdetail(gatepassId).subscribe(data => {
      debugger;
      this.materialList = data;
      console.log(data);
      this.gatepassModel = this.materialList[0];
      console.log(this.gatepassModel);
      debugger;
      if (this.gatepassModel.issuedqty > 0) {
        this.btnDisableissue = true;
      }
      if (this.gatepassModel.approverstatus == 'Approved')
        this.btnDisable = true;
    });
  }
  backtogatepass() {
    this.router.navigateByUrl("WMS/GatePass");
  }
  getGatePassHistoryList(gatepassId: string) {
    this.wmsService.getGatePassApprovalHistoryList(gatepassId).subscribe(data => {
      this.gatePassApprovalList = data;
     //if (this.gatePassApprovalList.filter(li => li.approverid == this.employee.employeeno)[0].approverstatus != "Approved")
        //this.btnDisable = false;
    });
  }
  showHistoryList() {
    this.showHistory = !this.showHistory;
  }
  updategatepassapproverstatus() {
    debugger;
    this.gatepassModel.gatepassid = this.materialList[0].gatepassid;
    this.gatepassModel.projectid = this.materialList[0].gatepassid;
    this.gatepassModel.pono = this.materialList[0].pono;
    //this.materialList.forEach(item => {
    //  item.pono = this.gatepassModel.pono;
    //})
    var data1 = this.issueFinalList.filter(function (element, index) {
      return (element.issuedqty > 0);
    });
    if (data1.length == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Issue Qty.' });
      return;
    }
    this.issueFinalList = this.issueFinalList.filter(function (element, index) {
      return (element.issuedqty > 0);
    });
    this.spinner.show();
    this.wmsService.updategatepassapproverstatus(this.issueFinalList).subscribe(data => {
      //this.materialList = data;
      this.spinner.hide();
      this.btnDisableissue = true;
      this.gatepassModel.status = "Approved";
      if (this.gatepassModel.status == 'Approved')
        this.btnDisable = true;

      this.messageService.add({ severity: 'success', summary: '', detail: 'Materials Issued for Gate Pass' });
      this.router.navigateByUrl("WMS/GatePass");
    });
  }

  //check date is valid or not
  checkValiddate(date: any) {
    try {
      if (!date || (this.datePipe.transform(date, this.constants.dateFormat) == "01/01/0001"))
        return "";
      else
        return this.datePipe.transform(date, this.constants.dateFormat);
    }
    catch{
      return "";
    }
  }

  //shows list of items for particular material
  showmateriallocationList(material, description, id, rowindex, qty, issuedqty, reservedqty, gatepassmaterialid, pono: string) {
    debugger;
    if (issuedqty <= qty) {
      this.issueqtyenable = true;
    }
    else {
      this.issueqtyenable = false;
    }
    this.itemissuedloc = [];
      this.reqqty = qty;
        this.reservedQty = reservedqty;
    this.id = id;
    this.material = material;
    this.matdesc = description;
    this.AddDialog = true;
    this.roindex = rowindex;
    this.issuedqty = issuedqty;
    //if (issuedqty > 0) {
    //  this.showdialog = true;
    //  //this.materialList.filter(li=>li.mater)
    //  this.locdetails.issueddate = issuedDate;
    //  this.locdetails.location = location;
    //  this.locdetails.issuedqty = issuedqty;
    //  this.itemissuedloc.push(this.locdetails);
    //}
    this.itemlocationData = [];
    if (this.constants.gatePassIssueType == "Pending") {
      this.issueqtyenable = false;
      debugger;
      var projectid = this.materialList[0].projectid;
      this.wmsService.getItemlocationListByMaterialanddescpo(material, description, projectid, pono).subscribe(data => {
        this.itemlocationData = data;
        this.showdialog = true;
      });
    }

    else {
      this.issueqtyenable = true;
      this.wmsService.getItemlocationListByGatepassmaterialid(gatepassmaterialid).subscribe(data => {
        debugger;
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

        this.messageService.add({ severity: 'info', summary: 'Accepted', detail: 'You have accepted' });
      },
      reject: () => {

        this.messageService.add({ severity: 'info', summary: 'Ignored', detail: 'You have ignored' });
      }
    });
  }
  //check issued quantity
  checkissueqty($event, entredvalue, maxvalue, material, createddate, index: number) {
    debugger;
    var id = $event.target.id;
    if (entredvalue > maxvalue) {
      this.itemlocationData[index].issuedqty = 0;
      this.messageService.add({ severity: 'error', summary: '', detail: 'Please enter issue quantity less than Available quantity' });
      (<HTMLInputElement>document.getElementById(id)).value = "0";
      this.materialList[this.roindex].issuedqty = 0;

    }
    else {
      this.wmsService.checkoldestmaterialwithdesc(material, createddate, this.matdesc).subscribe(data => {
        this.Oldestdata = data;
        if (data != null) {
          this.alertconfirm(this.Oldestdata);
        }
        this.spinner.hide();
      });
    }
  }
  Cancel() {
    this.AddDialog = false;
  }
  issuematerial(itemlocationData) {
    debugger;
    var totalissuedqty = 0;
    if (!isNullOrUndefined(this.itemlocationData)) {
      var matid = this.materialList[this.roindex].gatepassmaterialid
      this.issueFinalList = this.issueFinalList.filter(function (element, index) {
        return (element.gatepassmaterialid != matid);
      });
    }
   
  
    this.itemlocationData.forEach(item => {
      if (item.issuedqty) {
        debugger;
        //if (item.issuedquantity != "0")
        item.requestforissueid = this.materialList[this.roindex].requestforissueid;
        item.itemreturnable = this.materialList[this.roindex].itemreturnable;
        item.approvedby = this.employee.employeeno;
        item.itemreceiverid = this.materialList[this.roindex].itemreceiverid;
        item.gatepassid = this.materialList[this.roindex].gatepassid;
        item.gatepassmaterialid = this.materialList[this.roindex].gatepassmaterialid;
        item.gatepasstype = this.materialList[this.roindex].gatepasstype;     
        item.approverstatus = this.materialList[this.roindex].approverstatus;
        item.approverremarks = this.materialList[this.roindex].approverremarks;
        item.fmapproverremarks = this.materialList[this.roindex].fmapproverremarks;
        item.itemreceiverid = this.materialList[this.roindex].itemreceiverid;
        item.projectid = this.materialList[this.roindex].projectid;
        item.pono = this.materialList[this.roindex].pono;
        item.requesttype = "GPRequest";
        totalissuedqty = totalissuedqty + (item.issuedqty);
        this.issueFinalList.push(item);
      }
        //totalissuedqty = totalissuedqty + (item.issuedquantity);
      //this.FIFOvalues.issueqty = totalissuedqty;
      //item.issuedqty = totalissuedqty;

      //item.issuedquantity = totalissuedqty;
      //item.issuedqty = totalissuedqty;

    });
    if (totalissuedqty > this.reqqty) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Issue Qty cannot exceed Requested Qty' });
      this.AddDialog = true;
    }
    else {
      // (<HTMLInputElement>document.getElementById(this.id)).value = totalissuedqty.toString();
      this.materialList[this.roindex].issuedqty = totalissuedqty;
      this.txtDisable = true;

      this.AddDialog = false;
      
    }

  }
}
