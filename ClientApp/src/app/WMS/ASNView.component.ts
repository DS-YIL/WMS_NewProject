import { Component, OnInit, Inject, ViewChild } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { PoDetails, inwardModel, ddlmodel, MRNsavemodel } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { FormBuilder } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { ConfirmationService } from 'primeng/api';
import { isNullOrUndefined } from 'util';
import { HttpClient } from '@angular/common/http';
import { Table } from 'primeng/table';

@Component({
  selector: 'app-ASNView',
  templateUrl: './ASNView.component.html',
  providers: [DatePipe , ConfirmationService]
})
export class ASNViewComponent implements OnInit {
  @ViewChild('dt', { static: false }) table: Table;
  public url = "";
  constructor(private ConfirmationService: ConfirmationService, private http: HttpClient, private formBuilder: FormBuilder, private messageService: MessageService, private datePipe: DatePipe, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService, @Inject('BASE_URL') baseUrl: string) { this.url = baseUrl; }
  cols: any[];
  
  public employee: Employee;

  public currentDatePoList: Array<any> = [];

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
  
    this.getcurrentDatePolist();
   
  }

  onDateSelect(value) {
    debugger;
    var dtx = this.formatDate(value);
    this.table.filter(this.formatDate(value), 'deliverydate', 'startsWith')
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


  //get open po's based on current date(advance shipping notification list)
  getcurrentDatePolist() {
    this.spinner.show();
    this.wmsService.getASNListData().subscribe(data => {
      this.currentDatePoList = data;
      this.spinner.hide();
    });
  }

  

  



 
  //search list option changes event
  
 


}
