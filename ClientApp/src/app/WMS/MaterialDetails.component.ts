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
  templateUrl: './MaterialDetails.component.html'
})
export class MaterialDetailsComponent implements OnInit {
  constructor(private messageService: MessageService,
    private currentRoute: ActivatedRoute,
    private commonComponent: commonComponent, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public MaterialData: Array<any> = [];
  public employee: Employee;
  public fromDate: Date;
  public toDate: Date;
  public dynamicData: DynamicSearchResult;
  cols: any[];
  exportColumns: any[];
  pono: string;
  grnno: string;
  grnNo: string;

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.toDate = new Date();
    this.fromDate = new Date(new Date().setDate(new Date().getDate() - 30));
   

    this.cols = [
      { field: 'materialdescription', header: 'Material' },
      { field: 'totalquantity', header: 'Total Quantity' },
      { field: 'issued', header: 'Issued Quantity' },
      { field: 'availableqty', header: 'Available Quantity' },
      
    ];
    debugger;
    this.grnNo = this.currentRoute.snapshot.queryParams.grnNo;
    this.pono = this.currentRoute.snapshot.queryParams.pono;
    this.getMaterialDetails(this.grnNo);
    this.exportColumns = this.cols.map(col => ({ title: col.header, dataKey: col.field }));
  }

  AvailableMaterialLoc(materialid: string)
  {
    this.router.navigate(['/WMS/LocationDetails'], { queryParams: { materialid: materialid, pono: this.pono, grnNo: this.grnNo } });
  }
  IssuedMatDetails(materialid: string) {
    this.router.navigate(['/WMS/MaterialReqDetails'], { queryParams: { materialid: materialid, pono: this.pono, grnNo: this.grnNo} });
  }

  backInvoiceDetails() {
    this.router.navigate(['/WMS/InvoiceDetails'], { queryParams: { PONO: this.pono } });
  }

  getMaterialDetails(grnNo:string) {
    this.dynamicData = new DynamicSearchResult();
   // this.dynamicData.query = "select op.material , op.materialdescription,op.projectname, SUM(totalquantity) as Received , SUM(availableqty) as Balance ,SUM(totalquantity -availableqty ) as Issued, MAX(createddate) as createddate  from wms.wms_stock ws inner join wms.openpolistview  op on op.pono = ws.pono where ws.materialid notnull and createddate <='" + this.toDate.toDateString() + "' and createddate > '" + this.fromDate.toDateString() + "' group by  op.material , op.materialdescription,op.projectname"
    this.wmsService.getMaterialDetails(grnNo).subscribe(data => {
      debugger;
      console.log(data);
      this.grnno = grnNo;
      this.MaterialData = data;
    });
  }

  //export to excel 
  exportExcel() {
    this.commonComponent.exportExcel(this.MaterialData, 'PO List');
  }

  //pdfexport
  exportPdf() {
    this.commonComponent.exportPdf(this.exportColumns, this.MaterialData, 'PO List');

  }

}
