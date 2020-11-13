import { Component, OnInit, Inject, ViewChild } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { ConfirmationService } from 'primeng/api';
import { isNullOrUndefined } from 'util';
import { HttpClient } from '@angular/common/http';
import { testcrud, WMSHttpResponse, StockModel } from '../Models/WMS.Model';
import { Table } from 'primeng/table';

@Component({
  selector: 'app-AdminStockUploadReport',
  templateUrl: './AdminStockUploadReport.component.html',
  providers: [ConfirmationService]
})
export class AdminStockUploadReportComponent implements OnInit {
  @ViewChild('dt1', { static: false }) table: Table;

  constructor(private confirmationService: ConfirmationService, private http: HttpClient, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) {
  }
  
  
  public employee: Employee;
  getlistdata: StockModel[] = [];
  viewmain: boolean = false;
  viewdetail: boolean = false;
  viewexception: boolean = false;
  getmainlistdata: StockModel[] = [];
  lblfilename: string = "";
  lbldate: Date;
  lblqty: number;
  lblvalue: string = "";
  

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.viewmain = true;
    this.viewdetail = false;
    this.viewexception = false;
    this.getMainlist();
     
  }

  getMainlist() {
    this.getmainlistdata = [];
    this.viewexception = false;
    this.spinner.show();
    this.wmsService.getinitialStockReportGroup(this.employee.employeeno).subscribe(data => {
      this.getmainlistdata = data;
      this.spinner.hide();
    });
  }

  onDateSelect(value) {
    debugger;
    var dtx = this.formatDate(value);
    this.table.filter(this.formatDate(value), 'createddate', 'startsWith')
  }

  formatDate(date) {
    let month = date.getMonth() + 1;
    let day = date.getDate();

    if (month < 10) {
      month = '0' + month;
    }

    if (day < 10) {
      day = '0' + day;
    }

    return date.getFullYear() + '-' + month + '-' + day;
  }

  backtomain() {
    this.lblfilename = "";
    this.lbldate = null;
    this.lblqty = 0;
    this.lblvalue = "";
    this.viewmain = true;
    this.viewdetail = false;
    this.viewexception = false;
  }

  getexlist(data: StockModel) {
    this.lblfilename = data.uploadedfilename;
    this.lbldate = data.createddate;
    this.lblqty = data.exceptionrecords;
    this.lblvalue = "Exception Records"
    this.getlistdata = [];
    var uploadcode = data.uploadbatchcode;
    this.viewexception = false;
    this.spinner.show();
    this.wmsService.getinitialStockEX(uploadcode).subscribe(data => {
      this.getlistdata = data;
      this.viewmain = false;
      this.viewdetail = true;
      this.viewexception = true;
      this.spinner.hide();
    });
  }

  getalllist(data: StockModel) {
    this.lblfilename = data.uploadedfilename;
    this.lbldate = data.createddate;
    this.lblqty = data.totalrecords;
    this.lblvalue = "Total Records"
    this.getlistdata = [];
    var uploadcode = data.uploadbatchcode;
    this.viewexception = false;
    this.spinner.show();
    this.wmsService.getinitialStockAllrecords(uploadcode).subscribe(data => {
      this.getlistdata = data;
      this.viewmain = false;
      this.viewdetail = true;
      this.viewexception = true;
      this.spinner.hide();
    });
  }

  getlist(data: StockModel) {
    this.lblfilename = data.uploadedfilename;
    this.lbldate = data.createddate;
    this.lblqty = data.successrecords;
    this.lblvalue = "Success Records"
    this.getlistdata = [];
    var uploadcode = data.uploadbatchcode;
    this.viewexception = false;
    this.spinner.show();
    this.wmsService.getinitialStock(uploadcode).subscribe(data => {
      this.getlistdata = data;
      this.viewmain = false;
      this.viewdetail = true;
      this.spinner.hide();
    });
  }
 
}
