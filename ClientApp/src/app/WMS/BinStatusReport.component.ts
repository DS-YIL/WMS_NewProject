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
  selector: 'app-inventory',
  templateUrl: './BinStatusReport.component.html'
})
export class BinStatusReportComponent implements OnInit {
  constructor(private messageService: MessageService, private commonComponent: commonComponent, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public poList: Array<any> = [];
  public employee: Employee;
  public fromDate: Date;
  public toDate: Date;
  public dynamicData: DynamicSearchResult;
  public dataList: Array<any> = []
  cols: any[];
  exportColumns: any[];
  selectedStatus: string;

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.toDate = new Date();
    this.fromDate = new Date(new Date().setDate(new Date().getDate() - 30));
    this.getBinstatusList();

    this.cols = [
      { field: 'BinId', header: 'Bin' },
      { field: 'Material', header: 'Material' },
      { field: 'qty', header: 'Available Quantity' },
      { field: 'ItemLocation', header: 'Item Loaction' },
      
    ];

    this.exportColumns = this.cols.map(col => ({ title: col.header, dataKey: col.field }));
  }


  onSelectStatus(event) {
    this.selectedStatus = event.target.value;

  }


 

  //InvoiceDetails(poNo: string,qty:string)
  //{
  //  this.router.navigate(['/WMS/InvoiceDetails'], { queryParams: { PONO: poNo,qty:qty} });
  //  }
  SubmitBinStatus() {
    if (this.selectedStatus == "empty") {
      this.poList = this.dataList.filter(li => li.binid == 0 && li.binnumber != null);
    }
    else if (this.selectedStatus == "filled") {
      this.poList = this.dataList.filter(li => li.binid != 0 && li.binnumber != null);}
  }
  getBinstatusList() {
    this.spinner.show();
    this.wmsService.GetBinList().subscribe(data => {
      this.spinner.hide();
      this.dataList = data;
      this.poList = this.dataList.filter(li=>li.binid!=0 && li.binnumber!=null);
    });
  }

  //export to excel 
  exportExcel() {
    this.commonComponent.exportExcel(this.poList, 'PO List');
  }

  //pdfexport
  exportPdf() {
    this.commonComponent.exportPdf(this.exportColumns, this.poList, 'PO List');

  }

}
