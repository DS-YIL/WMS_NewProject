import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl, ValidatorFn } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { materialtransferMain, materialtransferTR, materialRequestDetails, returnmaterial, gatepassModel, materialistModel, PoDetails, StockModel, materialistModelreturn, materialistModeltransfer, ddlmodel, materialtransferapproverModel, MaterialTransaction } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { isNullOrUndefined } from 'util';
import { MatRadioButton } from '@angular/material';

@Component({
  selector: 'app-MaterialRequestApproval',
  templateUrl: './MaterialRequestApproval.component.html'
})
export class MaterialRequestApprovalComponent implements OnInit {
  constructor(private formBuilder: FormBuilder, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public requestList: MaterialTransaction[] = [];
  public requestListall: MaterialTransaction[] = [];
  public employee: Employee;
  public requestid: any;

  public gatepassdialog: boolean = false;
 
  materialtransferdetil: materialtransferTR[] = [];
  transferremarks: string = "";
  transferedfromlbl: string = "";
  transferedtolbl: string = "";
  selectedrows: materialtransferMain[] = [];
  approvalremarks: string = "";
  hideapproval: boolean = false;
  returntype: string = "";
  materialapproverlistdetil: materialtransferapproverModel[] = []
  requestedid: string = "";


  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.requestedid = this.route.snapshot.queryParams.transferid;
    this.materialapproverlistdetil = [];
    this.returntype = "Pending";
    this.hideapproval = false;
    this.requestList = [];
   
    this.selectedrows = []; 
    this.materialtransferdetil = [];

    this.getMaterialRequestlist(this.employee.employeeno);
   
  }

  //get Material Rquest based on login employee && po no
  getMaterialRequestlist(employeeno) {
    debugger;
    this.wmsService.getrequestdataforapproval(employeeno).subscribe(data => {
      this.requestListall = data;
      debugger;
      this.requestListall.forEach(item => {
        item.showtr = false;
        item.approvalcheck = null;
      });
      this.getdata();
    });
  }
  showattachtrdata(rowData: MaterialTransaction) {
    debugger;
    rowData.showtr = !rowData.showtr;
  }
  getdata() {
    this.requestList = [];
     var typ = this.returntype;
    
    if (!isNullOrUndefined(this.requestedid) && this.requestedid != "") {
      var tid = this.requestedid;
      this.requestList = this.requestListall.filter(function (element, index) {
        return (element.approvedstatus == typ && element.requestid == tid);
      });
      this.requestid = "";
    }
    else {
      this.requestList = this.requestListall.filter(function (element, index) {
        return (element.approvedstatus == typ);
      });
    }
   

  }

  noaction(data: MaterialTransaction) {
    data.approvalcheck = null;

  }

  savedata() {
    debugger;
    var data = this.requestList.filter(function (element, index) {
      return (element.approvalcheck == "0" || element.approvalcheck == "1");
    });
    if (data.length == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Approve/Reject' });
      return;

    }
    var data1 = this.requestList.filter(function (element, index) {
      return (element.approvalcheck == "0" && (isNullOrUndefined(element.approvalremarks) || element.approvalremarks.trim() == "" ));
    });
    if (data1.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter remarks for rejection' });
      return;

    }
    var senddata = this.requestList.filter(function (element, index) {
      return (element.approvalcheck == "0" || element.approvalcheck == "1");
    });
   
    this.spinner.show();
    this.wmsService.approverequestmaterial(senddata).subscribe(data => {
      this.spinner.hide();
      if (String(data)=="saved") {
        this.messageService.add({ severity: 'success', summary: '', detail: 'Records Updated' });
        this.getMaterialRequestlist(this.employee.employeeno);
      }

      else {
        this.messageService.add({ severity: 'error', summary: '', detail: String(data) });
      }
    });

  }

  showattachdata(rowData: materialtransferMain) {
    debugger;
    this.hideapproval = false;
    if (rowData.status != "Pending") {
      this.hideapproval = true;
    }
    this.materialtransferdetil = [];
    this.materialapproverlistdetil = [];
    this.approvalremarks = rowData.approvalremarks;
    this.gatepassdialog = true;
    this.transferedfromlbl = rowData.projectcodefrom + "(PM-" + rowData.projectmanagerfrom + ")";
    this.transferedtolbl = rowData.projectcode + "(PM-" + rowData.projectmanagerto + ")";
    this.materialtransferdetil = rowData.materialdata;
    this.materialapproverlistdetil = rowData.approverdata;
    this.selectedrows.push(rowData);
    
   
  }

 

  hideDG() {
    this.selectedrows = [];
    this.approvalremarks = "";
  }

  approve(isapproved: boolean) {
    this.selectedrows.forEach(item => {
      item.isapproved = isapproved;
      item.approvalremarks = this.approvalremarks;
      item.approverid = this.employee.employeeno;
    });
    var msg = isapproved ? "Approved" : "Rejected";
    var errormsg = isapproved ? "Approval" : "Rejection";

    this.spinner.show();
    this.wmsService.approvetransfermaterial(this.selectedrows).subscribe(data => {
      this.spinner.hide();
      if (data) {
        this.gatepassdialog = false;
        this.messageService.add({ severity: 'success', summary: '', detail: 'Material tarnsfer ' + msg });
        this.getMaterialRequestlist(this.employee.employeeno);
      }

      else {
        this.messageService.add({ severity: 'error', summary: '', detail: errormsg+' Failed' });
      }
    });


  }



 
  
 
 
 
  
 
 
 
 
  
 
 

 

 


 

  
  
}
