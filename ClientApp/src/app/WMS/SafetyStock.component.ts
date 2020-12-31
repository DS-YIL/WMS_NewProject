import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-safetystock',
  templateUrl: './SafetyStock.component.html'
})
export class SafetyStockComponent implements OnInit {
  constructor(private wmsService: wmsService, private router: Router, public constants: constants, private messageService: MessageService, private spinner: NgxSpinnerService) { }

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


  exportExcel() {
    debugger;
    if (this.safetyStockList.length>0) {
      let new_list = this.safetyStockList.map(function (obj) {
        return {
          'Material': obj.material,
          'Material Description': obj.materialdescription,
          'Min Order Qty': obj.minorderqty,
          'Value(in Rs.)':obj.value
        }
      });
      import("xlsx").then(xlsx => {
        const worksheet = xlsx.utils.json_to_sheet(new_list);
        const workbook = { Sheets: { 'data': worksheet }, SheetNames: ['data'] };
        const excelBuffer: any = xlsx.write(workbook, { bookType: 'xlsx', type: 'array' });
        this.saveAsExcelFile(excelBuffer, "SafetyStockReport");
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



  generatePO() {

  }
}

