import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { invstocktransfermodel } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { isNullOrUndefined } from 'util';
import { MatRadioButton } from '@angular/material';

@Component({
  selector: 'app-STOApproval',
  templateUrl: './STOApproval.component.html'
})
export class STOApprovalComponent implements OnInit {
  constructor(private formBuilder: FormBuilder, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public requestList: invstocktransfermodel[] = [];
  public requestListall: invstocktransfermodel[] = [];
  public employee: Employee;
  public requestid: any;
  returntype: string = "";
  requestedid: string = "";


  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.requestedid = this.route.snapshot.queryParams.transferid;
    this.returntype = "Pending";
    this.requestList = [];
   

    this.getMaterialRequestlist(this.employee.employeeno);
   
  }

  getMaterialRequestlist(employeeno) {
    debugger;
    this.wmsService.getrequestdataforSTOapproval(employeeno,'STO').subscribe(data => {
      this.requestListall = data;
      debugger;
      this.requestListall.forEach(item => {
        item.showtr = false;
        item.approvalcheck = null;
      });
      this.getdata();
    });
  }
  showattachtrdata(rowData: invstocktransfermodel) {
    debugger;
    rowData.showtr = !rowData.showtr;
  }
  getdata() {
    this.requestList = [];
    var typ = this.returntype;
    if (!isNullOrUndefined(this.requestedid) && this.requestedid != "") {
      var tid = this.requestedid;
      this.requestList = this.requestListall.filter(function (element, index) {
        return (element.approvedstatus == typ && element.transferid == tid);
      });
      this.requestid = "";
    }
    else {
      this.requestList = this.requestListall.filter(function (element, index) {
        return (element.approvedstatus == typ);
      });
    }
   

  }

  noaction(data: invstocktransfermodel) {
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
    this.wmsService.approveSTOrequestmaterial(senddata).subscribe(data => {
      this.spinner.hide();
      if (String(data)=="saved") {
        this.messageService.add({ severity: 'success', summary: '', detail: 'Saved successfully' });
        this.getMaterialRequestlist(this.employee.employeeno);
      }

      else {
        this.messageService.add({ severity: 'error', summary: '', detail: String(data) });
      }
    });

  }

  

  
}
