import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { materialrequestMain, materialRequestDetails, materialRequestFilterParams,gatepassModel, materialistModel, materialList, requestData } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-MaterialRequestDashboard',
  templateUrl: './MaterialRequestDashboard.component.html'
})
export class MaterialRequestDashboardComponent implements OnInit {

  constructor(private formBuilder: FormBuilder, private datePipe: DatePipe, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }
  public requestList: materialrequestMain[] = [];
  public employee: Employee;
  public materialRequestDetails: materialRequestDetails;
  public fromDate: Date;
  public toDate: Date;
  public materilaRequestFilterParams: materialRequestFilterParams;
  //public requestList: Array<any> = [];


  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.toDate = new Date();
    this.fromDate = new Date(new Date().setDate(new Date().getDate() - 30));
    this.requestList = [];
    this.getMaterialRequestlist();
  }
  //get rowData() {
  //  if (this.rowData.status === 'null') {
  //    return 'pending'
  //  }
  //  else {
  //    return 'approved'
  //  }
  //}

//  var status === null;
//if (status === 'null') {
//  console.log("pending")
//}
//else {
//  console.log("approved")
//}

  
  

  getMaterialRequestlist() {
    this.materilaRequestFilterParams = new materialRequestFilterParams();
    if (!this.fromDate) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Select From Date' });
      return;
    }
    if (!this.toDate) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Select To Date' });
      return;
    }
    this.materilaRequestFilterParams.FromDate = this.datePipe.transform(this.fromDate, "yyyy-MM-dd");
    this.materilaRequestFilterParams.ToDate = this.datePipe.transform(this.toDate, "yyyy-MM-dd");
    this.spinner.show();
    this.wmsService.getMaterialRequestDashboardlist(this.materilaRequestFilterParams).subscribe(data => {
      this.spinner.hide();
      this.requestList = data;
      this.requestList.forEach(item => {
        item.showtr = false;
      });
    });
  }

  showattachdata(rowData: materialrequestMain) {
    rowData.showtr = !rowData.showtr;
  }



}
