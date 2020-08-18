import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { gatepassModel, materialistModel, FIFOValues, locationdetails } from '../Models/WMS.Model';
import { DatePipe } from '@angular/common';
import { ConfirmationService } from 'primeng/api';
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
  public itemissuedloc: Array<any> = [];
  public Oldestdata: FIFOValues;
  public FIFOvalues: FIFOValues;
  public gatePassApprovalList: Array<any> = [];
  public showHistory: boolean = false;
  public reqqty: number;
  public material: string = "";
  public matdesc: string = "";
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.route.params.subscribe(params => {
      if (params["gatepassid"]) {
        var gatepassId = params["gatepassid"]
        this.bindMaterilaDetails(gatepassId);
        if (this.employee.roleid == "8") {
          this.getGatePassHistoryList(gatepassId);
        }
      }
    });
    this.gatepassModel = new gatepassModel();
    this.gatepassModel.approverstatus = "Approved";

  }


  //get gatepass list
  bindMaterilaDetails(gatepassId: any) {
    debugger;
    this.wmsService.gatepassmaterialdetail(gatepassId).subscribe(data => {
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
  getGatePassHistoryList(gatepassId: any) {
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
    this.gatepassModel.gatepassid = this.materialList[0].gatepassid;
    //this.materialList.forEach(item => {
    //  item.pono = this.gatepassModel.pono;
    //})
    this.wmsService.updategatepassapproverstatus(this.materialList).subscribe(data => {
      //this.materialList = data;
      this.btnDisableissue = true;
      this.gatepassModel.status = "Approved";
      if (this.gatepassModel.status == 'Approved')
        this.btnDisable = true;

      this.messageService.add({ severity: 'success', summary: '', detail: 'Gate Pass Approved' });
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
  showmateriallocationList(material,description, id, rowindex, qty,issuedqty,location,issuedDate) {
    debugger;
    this.reqqty = qty;
    this.id = id;
    this.material = material;
    this.matdesc = description;
    this.AddDialog = true;
    this.roindex = rowindex;
    this.issuedqty = issuedqty;
    if (issuedqty > 0) {
      this.showdialog = true;
      //this.materialList.filter(li=>li.mater)
      this.locdetails.issueddate = issuedDate;
      this.locdetails.location = location;
      this.locdetails.issuedqty = issuedqty;
      this.itemissuedloc.push(this.locdetails);
    }
    else {
      this.wmsService.getItemlocationListByMaterial(material).subscribe(data => {
        this.itemlocationData = data;
        this.showdialog = true;
        if (data != null) {

        }
      });
    }
   
  }
  //show alert about oldest item location
  alertconfirm(data) {
    var info = data;
    this.itemreceiveddate = this.datePipe.transform(data.createddate, 'yyyy-MM-dd hh:mm:ss');
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
  checkissueqty($event, entredvalue, maxvalue, material, createddate, index) {
    var id = $event.target.id;
    if (entredvalue > maxvalue) {
      this.itemlocationData[index].issuedquantity = 0;
      this.messageService.add({ severity: 'error', summary: '', detail: 'Please enter issue quantity less than Available quantity' });
      // this.btnDisableformaterial = true;
      (<HTMLInputElement>document.getElementById(id)).value = "0";
      this.materialList[this.roindex].issuedqty = 0;

    }
    else {
      // this.btnDisableformaterial = false;
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
  Cancel() {
    this.AddDialog = false;
  }
  issuematerial(itemlocationData) {
    debugger;
    var totalissuedqty = 0;
    this.itemlocationData.forEach(item => {
      if (item.issuedquantity != "0")

        totalissuedqty = totalissuedqty + (item.issuedquantity);
      //this.FIFOvalues.issueqty = totalissuedqty;
      //item.issuedqty = totalissuedqty;

      //item.issuedquantity = totalissuedqty;
      //item.issuedqty = totalissuedqty;

    });
    if (totalissuedqty > this.reqqty) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Please enter issue quantity less than or eaqual to requested quantity' });
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
