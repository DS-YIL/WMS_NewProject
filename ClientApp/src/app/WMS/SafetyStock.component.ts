import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";

@Component({
  selector: 'app-safetystock',
  templateUrl: './SafetyStock.component.html'
})
export class SafetyStockComponent implements OnInit {
  constructor( private wmsService: wmsService, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public employee: Employee;
  public safetyStockList: Array<any> = [];

  ngOnInit() {

    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");


    this.getSafteyStockList();
  }

  getSafteyStockList() {
    this.spinner.show();
    this.safetyStockList = [];
    this.wmsService.getSafteyStockList().subscribe(data => {
      this.safetyStockList = data;
      this.spinner.hide();
    });
  }

  generatePO() {

  }
}

