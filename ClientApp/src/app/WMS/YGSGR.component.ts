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
import { testcrud, WMSHttpResponse, YGSGR } from '../Models/WMS.Model';

@Component({
  selector: 'app-YGSGR',
  templateUrl: './YGSGR.component.html',
  providers: [ConfirmationService]
})
export class YGSGRComponent implements OnInit {

  constructor(private confirmationService: ConfirmationService, private http: HttpClient, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) {}
  
  
  public employee: Employee;
  failedgrlist: YGSGR[] = [];
  


  

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.failedgrlist = [];
    this.getfailedgrlist();
  }

  getfailedgrlist() {
    this.failedgrlist = [];
    this.wmsService.getFailedGR().subscribe(response => {
      if (response) {
        this.failedgrlist = response;
      }
    });

  }
}
