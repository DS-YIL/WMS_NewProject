import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, rbamaster } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { authUser, ddlmodel, subrolemodel, AssignProjectModel, assignpmmodel, UserModel } from '../Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { isNullOrUndefined } from 'util';
import { truncate } from 'fs';

@Component({
  selector: 'app-AssignRBA',
  templateUrl: './AssignRBA.component.html'
})
export class AssignRBAComponent implements OnInit {
  constructor(private wmsService: wmsService, private messageService: MessageService, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public employee: Employee;
  getlistdata: rbamaster[] = [];

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.getlistdata = [];
    this.getrbalist();
    
   
  }

  //get EmployeeList
  getrbalist() {
    this.wmsService.getrbadata().subscribe(data => {
      if (data.length > 0) {
        this.getlistdata = data;
      }
    })

  }


  submitAdd() {

    this.spinner.show();
    this.wmsService.assignRBA(this.getlistdata).subscribe(data => {
      this.spinner.hide();
      if (String(data) == "saved") {
        this.messageService.add({ severity: 'success', summary: '', detail: 'Saved Successfully.' });
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: String(data) });
      }
      this.getrbalist();
    })

  }

 }

