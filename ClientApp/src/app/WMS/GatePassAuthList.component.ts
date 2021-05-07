import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { gatepassModel } from '../Models/WMS.Model';
import { isNullOrUndefined } from 'util';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-GatePassAuthList',
  templateUrl: './GatePassAuthList.component.html',
  providers: [DatePipe]
})
export class GatePassAuthList implements OnInit {
  constructor(private messageService: MessageService, private datePipe: DatePipe, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public gatepasslist: Array<any> = [];
  public gatepassData: Array<any> = [];
  public totalGatePassList: Array<any> = [];
  public employee: Employee;
  public gatepassModel: gatepassModel;
  public approverstatus: string;
  public showAuthtab: boolean = false;
  public materialList: Array<any> = [];
  public btnDisable: boolean = false;
  public gatePassApprovalList: Array<any> = [];
  public typeOfList: string;
  public showHistory: boolean = false;
  //Email
  gatepassid: string = "";

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.gatepassModel = new gatepassModel();
    this.gatepassModel.gatepasstype = "";
    this.typeOfList = this.route.routeConfig.path;
    this.getGatePassList();
    this.approverstatus = "Pending";

    //Email
    this.gatepassid = this.route.snapshot.queryParams.requestid;
    //get material details for that requestid
    if (this.gatepassid) {
      this.showApprover(this.gatepassid);

    }
  }


  //get gatepass list
  getGatePassList() {
    this.spinner.show();
    this.wmsService.getGatePassList().subscribe(data => {
      this.spinner.hide();
      this.totalGatePassList = data;
      this.totalGatePassList = this.totalGatePassList.filter(li => li.approverstatus == "Approved");
      this.gatepassData = this.totalGatePassList.filter(li => li.authstatus == this.approverstatus);
      this.prepareGatepassList();
    });
  }

  //prepare list based on gate pass id
  prepareGatepassList() {
    this.gatepasslist = [];
    this.gatepassData.forEach(item => {
      var res = this.gatepasslist.filter(li => li.gatepassid == item.gatepassid);
      if (res.length == 0) {
        item.materialList = [];
        var result = this.gatepassData.filter(li => li.gatepassid == item.gatepassid && li.gatepassmaterialid != "0" && li.deleteflag == false);
        for (var i = 0; i < result.length; i++) {
          item.materialList.push(result[i]);
        }
        this.gatepasslist.push(item);
      }
    });
  }


  searchGatePassList() {
    if (!this.approverstatus && !this.gatepassModel.gatepasstype)
      this.gatepassData = this.totalGatePassList;
    else if (this.approverstatus && !this.gatepassModel.gatepasstype)
      this.gatepassData = this.totalGatePassList.filter(li => li.authstatus == this.approverstatus);
    else if (!this.approverstatus && this.gatepassModel.gatepasstype)
      this.gatepassData = this.totalGatePassList.filter(li => li.gatepasstype == this.gatepassModel.gatepasstype);
    else if (this.approverstatus && this.gatepassModel.gatepasstype)
      this.gatepassData = this.totalGatePassList.filter(li => li.authstatus == this.approverstatus && li.gatepasstype == this.gatepassModel.gatepasstype);

    this.prepareGatepassList();
  }

  showHistoryList() {
    this.showHistory = !this.showHistory;
  }

  showApprover(gatepassid: any) {
    this.showAuthtab = true;
    this.bindMaterilaDetails(gatepassid);
  }
  hideApprover() {
    this.showAuthtab = false;
    this.getGatePassList();
  }


  //get gatepass list
  bindMaterilaDetails(gatepassId: any) {
    this.btnDisable = false;
    this.wmsService.gatepassmaterialdetail(gatepassId).subscribe(data => {
      debugger;
      this.materialList = data;
      this.gatepassModel = this.materialList[0];
    });
  }


  updategatepassapproverstatus() {
    if (isNullOrUndefined(this.gatepassModel.authstatus) || this.gatepassModel.authstatus == "" || this.gatepassModel.authstatus == "Pending") {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Status' });
      return;
    }

    this.gatepassModel.gatepassid = this.materialList[0].gatepassid;
    this.gatepassModel.categoryid = 3;
    this.gatepassModel.approverid = this.employee.employeeno;
    this.gatepassModel.approvedby = this.employee.name;
    this.gatepassModel.requestedby = this.materialList[0].name;
    this.gatepassModel.requestedon = this.materialList[0].requestedon;
    this.spinner.show();
    this.gatepassModel.authid = this.employee.employeeno;
    this.wmsService.GatepassapproveByManager(this.gatepassModel).subscribe(data => {
      this.spinner.hide();
      this.hideApprover();
      this.btnDisable = true;
      if (this.gatepassModel.authstatus == 'Approved')
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
        return date;
    }
    catch{
      return "";
    }
  }
}
