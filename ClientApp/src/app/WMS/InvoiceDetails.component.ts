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
  templateUrl: './InvoiceDetails.component.html'
})
export class InvoiceDetailsComponent implements OnInit {
  constructor(private messageService: MessageService,
    private currentRoute: ActivatedRoute,
    private commonComponent: commonComponent, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public invoiceDetails: Array<any> = [];
  public employee: Employee;
  public fromDate: Date;
  public toDate: Date;
  public dynamicData: DynamicSearchResult;
  cols: any[];
  exportColumns: any[];
  pono: string;
  poQty: any;
  confirmQtyTotal: any=0;
  pendingqty: any=0;

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.toDate = new Date();
    this.fromDate = new Date(new Date().setDate(new Date().getDate() - 30));
   

    this.cols = [
      { field: 'invoiceno', header: 'Invoice No.' },
      { field: 'grnno', header: 'GRN No.' },
      { field: 'receivedqty', header: 'Received Quantity' },
      { field: 'confirmqty', header: 'Confirmed Quantity' },
      { field: 'returnqty', header: 'Returned Quantity' },
      
    ];

    var pono = this.currentRoute.snapshot.queryParams.PONO;
    this.poQty = this.currentRoute.snapshot.queryParams.qty;
    this.getInvoiceDetails(pono);
    this.exportColumns = this.cols.map(col => ({ title: col.header, dataKey: col.field }));
  }

  MaterialDetails(grnNo: string)
  {
    this.router.navigate(['/WMS/MaterialDetails'], { queryParams: { grnNo: grnNo, pono:this.pono } });
    }

  backPOStatus() {
    this.router.navigate(['/WMS/POStatus']);
  }

  getInvoiceDetails(pono:string) {
    this.dynamicData = new DynamicSearchResult();
   // this.dynamicData.query = "select op.material , op.materialdescription,op.projectname, SUM(totalquantity) as Received , SUM(availableqty) as Balance ,SUM(totalquantity -availableqty ) as Issued, MAX(createddate) as createddate  from wms.wms_stock ws inner join wms.openpolistview  op on op.pono = ws.pono where ws.materialid notnull and createddate <='" + this.toDate.toDateString() + "' and createddate > '" + this.fromDate.toDateString() + "' group by  op.material , op.materialdescription,op.projectname"
    this.wmsService.getInvoiceDetails(pono).subscribe(data => {
      this.pono = pono;
      this.invoiceDetails = data;
      this.getconfirmQtyTotal();
      this.pendingqty = this.poQty - this.confirmQtyTotal;
    });
  }

  getconfirmQtyTotal() {
    for (var i = 0; i < this.invoiceDetails.length; i++) {
      if (Number(this.invoiceDetails[i].confirmedqty) && this.invoiceDetails[i].confirmedqty!="undefined") {
        this.confirmQtyTotal = this.confirmQtyTotal + Number(this.invoiceDetails[i].confirmedqty);
      }
      
    }
    }

  //export to excel 
  exportExcel() {
    this.commonComponent.exportExcel(this.invoiceDetails, 'PO List');
  }

  //pdfexport
  exportPdf() {
    this.commonComponent.exportPdf(this.exportColumns, this.invoiceDetails, 'PO List');

  }

}
