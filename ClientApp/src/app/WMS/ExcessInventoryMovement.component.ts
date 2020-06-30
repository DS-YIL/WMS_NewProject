import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { commonComponent } from '../WmsCommon/CommonCode';

@Component({
  selector: 'app-Excessinventory',
  templateUrl: './ExcessInventoryMovement.component.html'
})
export class ExcessInventoryMovementComponent implements OnInit {
  constructor(private wmsService: wmsService, private commonComponent: commonComponent, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public employee: Employee;
  public fromDate: Date;
  public toDate: Date;
  public dynamicData: DynamicSearchResult;

  public ExcessInventoryList: Array<any> = [];
  public daysSelection: string;
  public movingDays: number;
  public minDays: number;
  public maxDays: number;
  cols: any[];
  exportColumns: any[];


  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.toDate = new Date();
    this.fromDate = new Date(new Date().setDate(new Date().getDate() - 30));

    this.daysSelection = "Weeks";
    this.movingDays = 4;
    this.getExcessInventoryList();

    this.cols = [
      //{ field: 'ponumber', header: 'PO No' },
      { field: 'materialid', header: 'Material' },
      { field: 'materialdescription', header: 'Material Descr' },
      { field: 'issuedqty', header: 'Total consumed Qty for last ' + this.movingDays + ' years' },
      { field: 'availableqty', header: 'Total Available Qty for last ' + this.movingDays + ' years' },
      //{ field: 'departmentname', header: 'Dep Name' },
      //{ field: 'itemlocation', header: 'Item Location' },
      //{ field: 'projectname', header: 'Project Name' },
      //{ field: 'vendorname', header: 'Vendor Name' },
      //{ field: 'receiveddate', header: 'Received Date' },
      //{ field: 'receivedqty', header: 'Received Qty' },
      //{ field: 'issuedqty', header: 'Issued Qty' },
      //{ field: 'availableqty', header: 'Available Qty' },
      //{ field: 'unitprice', header: 'Unit Price' },
      //{ field: 'category', header: 'Category' },
      //{ field: 'daysinstock', header: 'Days In Stock' },
      //{ field: 'reportdate', header: 'Report Date' },
    ];


    this.exportColumns = this.cols.map(col => ({ title: col.header, dataKey: col.field }));
    this.getExcessInventoryList();
  }

  getExcessInventoryList() {
    this.dayscalculator();
    this.spinner.show();
    this.ExcessInventoryList = [];
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.query = "select * from wms.stockexcessview where DaysInStock >=" + this.minDays + " and DaysInStock<=" + this.maxDays + "";
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.ExcessInventoryList = data;
      this.spinner.hide();
    });
  }

  //days calculator
  dayscalculator() {
    this.movingDays = parseInt(this.movingDays.toString());
    //if (this.daysSelection == 'Weeks') {
    //  this.minDays = this.movingDays * 1;
    //  if (this.movingDays > 1)
    //    this.minDays = this.movingDays * 7;
    //  this.maxDays = (this.movingDays + 1) * 7;
    //}
    //if (this.daysSelection == 'Months') {
    //  this.minDays = this.movingDays * 1;
    //  if (this.movingDays > 1)
    //    this.minDays = this.movingDays * 30;
    //  this.maxDays = (this.movingDays + 1) * 30;
    //}
    //if (this.daysSelection == 'Years') {
    this.minDays = 1;
    this.maxDays = (this.movingDays) * 365;
    //}

  }


  //export to excel 
  exportExcel() {
    this.commonComponent.exportExcel(this.ExcessInventoryList, 'ExcessInventoryList');
  }

  //pdfexport
  exportPdf() {
    this.commonComponent.exportPdf(this.exportColumns, this.ExcessInventoryList, 'ExcessInventoryList');

  }
}

