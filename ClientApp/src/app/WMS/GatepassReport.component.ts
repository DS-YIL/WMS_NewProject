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
import { testcrud, WMSHttpResponse, YGSGR, gatepassModel } from '../Models/WMS.Model';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-GatepassReport',
  templateUrl: './GatepassReport.component.html',
  providers: [ConfirmationService]
})
export class GatepassReportComponent implements OnInit {

  constructor(private confirmationService: ConfirmationService, private datePipe: DatePipe, private http: HttpClient, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) {}
  
  
  public employee: Employee;
  gpmateriallist: gatepassModel[] = [];
  filteredmatdetailsList: gatepassModel[] = [];
  gplist: gatepassModel[] = [];
  public fromDate: Date;
  public toDate: Date;
  showdetail: boolean = false;
  gpproject: string = "";
  gpid: string = "";
  gptype: string = "";
  gpsupplier: string = "";
  isreturnable: boolean = false;
  ageing: number;
  checkvalue: string = "";


  

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.toDate = new Date();
    this.fromDate = new Date(new Date().setDate(new Date().getDate() - 30));
    this.gplist = [];
    this.gpmateriallist = [];
    this.filteredmatdetailsList = [];
    this.showdetail = false;
    this.isreturnable = false;
    this.gpproject = "";
    this.gpid = "";
    this.gptype = "";
    this.gpsupplier = "";
    this.ageing = 0;
    this.checkvalue = "0";
    this.gatepassreport();
  }

  showdetails(gatepassid: string, data: gatepassModel) {
    this.filteredmatdetailsList = this.gpmateriallist.filter(li => li.gatepassid == gatepassid);
    this.gpproject = data.projectid;
    this.gpid = data.gatepassid;
    this.gptype = data.gatepasstype;
    this.gpsupplier = data.vendorname;
    if (data.gatepasstype == "Returnable") {
      this.isreturnable = true;
    }
    else {
      this.isreturnable = false;
    }
    this.showdetail = true;
  }

  hideDG() {
    this.filteredmatdetailsList = [];
    this.gpproject = "";
    this.gpid = "";
    this.gptype = "";
    this.gpsupplier = "";
    this.isreturnable = false;
    this.showdetail = false;
    

  }

  preparereport() {
    this.gplist = [];
    var templist = [];
    if (this.ageing > 0) {
      if (this.checkvalue == "1") {
        this.gpmateriallist = this.gpmateriallist.filter(li => li.ageing == this.ageing && li.gatepasstype == "Returnable");
      }
      else if (this.checkvalue == "2") {
        this.gpmateriallist = this.gpmateriallist.filter(li => li.ageing >= this.ageing && li.gatepasstype == "Returnable");
      }
      else if (this.checkvalue == "3") {
        this.gpmateriallist = this.gpmateriallist.filter(li => li.ageing <= this.ageing && li.gatepasstype == "Returnable");
      }
      else if (this.checkvalue == "4") {
        this.gpmateriallist = this.gpmateriallist.filter(li => li.ageing > this.ageing && li.gatepasstype == "Returnable");
      }
      else if (this.checkvalue == "5") {
        this.gpmateriallist = this.gpmateriallist.filter(li => li.ageing < this.ageing && li.gatepasstype == "Returnable");
      }
      else {
        this.gpmateriallist = this.gpmateriallist.filter(li => li.ageing == this.ageing && li.gatepasstype == "Returnable");
      }
      this.gpmateriallist.forEach(item => {
        var res = this.gplist.filter(li => li.gatepassid == item.gatepassid);
        if (res.length == 0) {
          this.gplist.push(item);
        }
      });

    }
    else {
      this.gpmateriallist.forEach(item => {
        var res = this.gplist.filter(li => li.gatepassid == item.gatepassid);
        if (res.length == 0) {
          this.gplist.push(item);
        }
      });
    }
   
  }

  gatepassreport() {
    if (!this.fromDate) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Select From Date' });
      return;
    }
    if (!this.toDate) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Select To Date' });
      return;
    }
    this.gplist = [];
    this.gpmateriallist = [];
    var fromdatestr = this.datePipe.transform(this.fromDate, "yyyy-MM-dd");
    var ToDatestr = this.datePipe.transform(this.toDate, "yyyy-MM-dd");
    this.wmsService.getGatepassReport(fromdatestr, ToDatestr).subscribe(response => {
      if (response) {
        this.gpmateriallist = response;
        this.preparereport();
      }
    });

  }
}
