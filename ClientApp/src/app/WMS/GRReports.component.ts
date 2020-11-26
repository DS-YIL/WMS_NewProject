import { Component, OnInit, Inject, ViewChild } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { PoDetails, inwardModel, ddlmodel, MRNsavemodel, grReports } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { FormBuilder } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { ConfirmationService } from 'primeng/api';
import { isNullOrUndefined } from 'util';
import { HttpClient } from '@angular/common/http';
import { Table } from 'primeng/table';

@Component({
  selector: 'app-GRReports',
  templateUrl: './GRReports.component.html',
  providers: [DatePipe , ConfirmationService]
})
export class GRReportsComponent implements OnInit {
  @ViewChild('dt', { static: false }) table: Table;
  public url = "";
  constructor(private ConfirmationService: ConfirmationService, private http: HttpClient, private formBuilder: FormBuilder, private messageService: MessageService, private datePipe: DatePipe, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService, @Inject('BASE_URL') baseUrl: string) { this.url = baseUrl; }
  cols: any[];
  
  public employee: Employee;

  public grReports: Array<any> = [];
  postmodel: grReports;
  showadddatamodel: boolean = false;
  public grReportsList: grReports[] = [];
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.postmodel = new grReports();
    this.getGRReports();
    
  }


  getGRReports() {
    this.spinner.show();
    this.wmsService.getGRListData().subscribe(data => {
      this.grReportsList = data;
      this.spinner.hide();
    });



  }
  refreshsavemodel() {
    this.postmodel = new grReports();
  }



  post(details: grReports) {
    this.postmodel = details;

  this.showadddatamodel = true;
   
  }

  postdata() {
   // alert("hi");
    if (!this.postmodel.sapgr) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter SAP GR' });
      return;
    }
    this.wmsService.SAPGREditReport(this.postmodel).subscribe(data => {
      this.showadddatamodel = false;
     // console.log(this.postmodel)

      this.getGRReports();

    })

  }
  


 
  
 


}
