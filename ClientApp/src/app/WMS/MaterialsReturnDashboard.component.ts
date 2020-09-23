import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { materialreturnMain, materialRequestDetails, materialListforReserve, materialReservetorequestModel, materialResFilterParams,gatepassModel, materialistModel, materialList, requestData } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-MaterialsReturnDashboard',
  templateUrl: './MaterialsReturnDashboard.component.html'
})
export class MaterialsReturnDashboardComponent implements OnInit {

  constructor(private formBuilder: FormBuilder, private datePipe: DatePipe, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }
  public requestList: materialreturnMain[] = [];
  public employee: Employee;
  public materialRequestDetails: materialRequestDetails;
  public fromDate: Date;
  public toDate: Date;
  public materialResFilterParams: materialResFilterParams;
  //public requestList: Array<any> = [];
  public materialistModel: materialListforReserve;
  public materialmodel: Array<materialListforReserve> = [];
  public materialList: Array<materialListforReserve> = [];

  


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

  getMaterialRequestlist() {
    this.materialResFilterParams = new materialResFilterParams();


    if (!this.fromDate) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Select From Date' });
      return;
    }
    if (!this.toDate) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Select To Date' });
      return;
    }
    this.materialResFilterParams.FromDate = this.datePipe.transform(this.fromDate, "yyyy-MM-dd");
    this.materialResFilterParams.ToDate = this.datePipe.transform(this.toDate, "yyyy-MM-dd");
    this.spinner.show();
    this.wmsService.getMaterialReturnDashboardlist(this.materialResFilterParams).subscribe(data => {
      this.spinner.hide();
      this.requestList = data;
      this.requestList.forEach(item => {
        item.showtr = false;
      });
    });
  }

  showattachdata(rowData: materialreturnMain) {
    rowData.showtr = !rowData.showtr;
  }



}
