import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { gatepassModel, materialistModel, FIFOValues } from '../Models/WMS.Model';
import { isNullOrUndefined } from 'util';
import { DatePipe } from '@angular/common';
import { ConfirmationService } from 'primeng/api';
@Component({
  selector: 'app-GatePassApprovalList',
  templateUrl: './GatePassApprovalList.component.html',
  providers: [DatePipe]
})
export class GatePassApprovalList implements OnInit {
  AddDialog: boolean;
  id: any;
  roindex: any;
  Oldestdata: any;
  constructor( private messageService: MessageService, private datePipe: DatePipe, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }


  public gatepasslist: Array<any> = [];
  public totalGatePassList: Array<any> = [];
  public employee: Employee;
  public gatepassModel: gatepassModel;
  public approverstatus: string;
  public showApprovertab: boolean = false;
  public materialList: Array<any> = [];
  public btnDisable: boolean = false;
  public gatePassApprovalList: Array<any> = [];
  public typeOfList: string;
  public showHistory: boolean = false;


  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.gatepassModel = new gatepassModel();
    this.typeOfList = this.route.routeConfig.path;
    this.getGatePassList();
      this.approverstatus = "Pending";
  }


  //get gatepass list
  getGatePassList() {
    this.wmsService.getGatePassList().subscribe(data => {
      this.totalGatePassList = data;
      if (this.typeOfList == "GatePassPMList")
        this.gatepasslist = this.totalGatePassList.filter(li => li.approverid == this.employee.employeeno && (li.approverstatus == this.approverstatus));
      if (this.typeOfList =="GatePassFMList")
        this.gatepasslist = this.totalGatePassList.filter(li => li.fmapproverid == this.employee.employeeno && (li.approverstatus == "Approved" && li.fmapproverstatus == this.approverstatus));
    });
  }

  getGatePassHistoryList(gatepassId: any) {
    this.wmsService.getGatePassApprovalHistoryList(gatepassId).subscribe(data => {
      this.gatePassApprovalList = data;
    });
  }

  searchGatePassList() {  
    if (this.approverstatus != "0") {
      if (this.typeOfList == "GatePassPMList")
        this.gatepasslist = this.totalGatePassList.filter(li => li.approverid == this.employee.employeeno && (li.approverstatus == this.approverstatus));
      if (this.typeOfList == "GatePassFMList")
        this.gatepasslist = this.totalGatePassList.filter(li => li.fmapproverid == this.employee.employeeno && (li.approverstatus == "Approved" && li.fmapproverstatus == this.approverstatus));
    }
  }

  showHistoryList() {
    this.showHistory = !this.showHistory;
  }

  showApprover(gatepassid: any) {
    this.showApprovertab = true;
    this.bindMaterilaDetails(gatepassid);
    this.getGatePassHistoryList(gatepassid);
  }
  hideApprover() {
    this.showApprovertab = false;
  }


  //get gatepass list
  bindMaterilaDetails(gatepassId: any) {
    this.wmsService.gatepassmaterialdetail(gatepassId).subscribe(data => {
      this.materialList = data;
      console.log(data);
      this.gatepassModel = this.materialList[0];
      if (this.gatepassModel.approverstatus == 'Approved')
        this.btnDisable = true;
    });
  }


  updategatepassapproverstatus() {
    this.gatepassModel.gatepassid = this.materialList[0].gatepassid;
    if (this.typeOfList == "GatePassPMList")
      this.gatepassModel.categoryid = 1;
    else
      this.gatepassModel.categoryid = 2;
    this.spinner.show();
    this.wmsService.GatepassapproveByManager(this.gatepassModel).subscribe(data => {
      this.spinner.hide();
      this.gatepassModel.status = "Approved";
      if (this.gatepassModel.status == 'Approved')
        this.btnDisable = true;

      this.messageService.add({ severity: 'success', summary: 'success Message', detail: 'Gate Pass Approved' });
    });
  }
 

  //check date is valid or not
  checkValiddate(date: any) {
    try {
      if (!date || (this.datePipe.transform(date, this.constants.dateFormat) == "01/01/0001"))
        return "";
      else
        return date;
    }
    catch{
      return "";
    }
  }
}
