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
  templateUrl: './POStatus.component.html'
})
export class POStatusComponent implements OnInit {
  constructor(private messageService: MessageService, private commonComponent: commonComponent, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public poList: Array<any> = [];
  public employee: Employee;
  public fromDate: Date;
  public toDate: Date;
  public dynamicData: DynamicSearchResult;
  cols: any[];
  exportColumns: any[];

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.toDate = new Date();
    this.fromDate = new Date(new Date().setDate(new Date().getDate() - 30));
    this.getpoList();

    this.cols = [
      { field: 'po', header: 'PO No.' },
      { field: 'qty', header: 'Quantity' },
    ];

    this.exportColumns = this.cols.map(col => ({ title: col.header, dataKey: col.field }));
  }

  InvoiceDetails(poNo: string,qty:string)
  {
    this.router.navigate(['/WMS/InvoiceDetails'], { queryParams: { PONO: poNo,qty:qty} });
    }

  getpoList() {
    this.dynamicData = new DynamicSearchResult();
   // this.dynamicData.query = "select op.material , op.materialdescription,op.projectname, SUM(totalquantity) as Received , SUM(availableqty) as Balance ,SUM(totalquantity -availableqty ) as Issued, MAX(createddate) as createddate  from wms.wms_stock ws inner join wms.openpolistview  op on op.pono = ws.pono where ws.materialid notnull and createddate <='" + this.toDate.toDateString() + "' and createddate > '" + this.fromDate.toDateString() + "' group by  op.material , op.materialdescription,op.projectname"
    this.wmsService.getPONumbers().subscribe(data => {
      debugger;
      this.poList = data;
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
