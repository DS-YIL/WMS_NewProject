import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { materialtransferMain, materialRequestDetails, materilaTrasFilterParams } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-MaterialTransferDashBoard',
  templateUrl: './MaterialTransferDashBoard.component.html'
})
export class MaterialTransferDashboardComponent implements OnInit {

  constructor(private formBuilder: FormBuilder, private datePipe: DatePipe, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }
  public requestList: materialtransferMain[] = [];
  public employee: Employee;
  public materialRequestDetails: materialRequestDetails;
  public fromDate: Date;
  public toDate: Date;
  public materilaTrasFilterParams: materilaTrasFilterParams;


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
  //get Material Rquest based on login employee && po no
  getMaterialRequestlist() {
    this.materilaTrasFilterParams = new materilaTrasFilterParams();
    if (!this.fromDate) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Select From Date' });
      return;
    }
    if (!this.toDate) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Select To Date' });
      return;
    }
    this.materilaTrasFilterParams.FromDate = this.datePipe.transform(this.fromDate, "yyyy-MM-dd");
    this.materilaTrasFilterParams.ToDate = this.datePipe.transform(this.toDate, "yyyy-MM-dd");
    this.spinner.show();
    this.wmsService.getMaterialtransferdetails(this.materilaTrasFilterParams).subscribe(data => {
      this.spinner.hide();
      this.requestList = data;
      this.requestList.forEach(item => {
        item.showtr = false;
      });
    });
  }

  showattachdata(rowData: materialtransferMain) {
    rowData.showtr = !rowData.showtr;
  }



}
