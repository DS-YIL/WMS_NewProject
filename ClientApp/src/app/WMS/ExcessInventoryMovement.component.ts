import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { commonComponent } from '../WmsCommon/CommonCode';

@Component({
  selector: 'app-Excessinventory',
  templateUrl: './ExcessInventoryMovement.component.html'
})
export class ExcessInventoryMovementComponent implements OnInit {
  constructor(private wmsService: wmsService, private commonComponent: commonComponent, private messageService: MessageService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

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
    this.movingDays = 2;
    //this.getExcessInventoryList();

    this.cols = [
      //{ field: 'ponumber', header: 'PO No' },
      { field: 'materialid', header: 'Material' },
      { field: 'materialdescription', header: 'Material Descr' },
      { field: 'issuedqty', header: 'Total consumed Qty for last ' + this.movingDays + ' years' },
      { field: 'availableqty', header: 'Total Available Qty for last ' + this.movingDays + ' years' },
      { field: 'value', header: 'Value(in Rs.)' },
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
    this.dynamicData.query = "select * from wms.stockexcessview where DaysInStock >=" + this.minDays + " and DaysInStock<=" + this.maxDays + " and availableqty > issuedqty";
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
  //exportExcel() {
  //  this.commonComponent.exportExcel(this.ExcessInventoryList, 'ExcessInventoryList');
  //}

  exportExcel() {
    if (this.ExcessInventoryList.length>0) {
      var totalconsumed = "Total consumed Qty for last " + this.movingDays + " years";
      let new_list = this.ExcessInventoryList.map(function (obj) {
        return {
          'Material': obj.materialid,
          'Material Description': obj.materialdescription,
          [totalconsumed] : obj.issuedqty,
          'Available Qty': "gh" + obj.availableqty,
          'Value(in Rs.)':obj.value
        }
      });
      import("xlsx").then(xlsx => {
        const worksheet = xlsx.utils.json_to_sheet(new_list);
        const workbook = { Sheets: { 'data': worksheet }, SheetNames: ['data'] };
        const excelBuffer: any = xlsx.write(workbook, { bookType: 'xlsx', type: 'array' });
        this.saveAsExcelFile(excelBuffer, "ExcessInventoryReport");
      });
    }
    else {
      this.messageService.add({ severity: 'error', summary: '', detail: 'No data exists' });
      return;
    }
   
  }


  saveAsExcelFile(buffer: any, fileName: string): void {
    import("file-saver").then(FileSaver => {
      let EXCEL_TYPE = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=UTF-8';
      let EXCEL_EXTENSION = '.xlsx';
      const data: Blob = new Blob([buffer], {
        type: EXCEL_TYPE
      });
      FileSaver.saveAs(data, fileName + '_export_' + new Date().getTime() + EXCEL_EXTENSION);
    });
  }

  //pdfexport
  exportPdf() {
    this.commonComponent.exportPdf(this.exportColumns, this.ExcessInventoryList, 'ExcessInventoryList');

  }
}

