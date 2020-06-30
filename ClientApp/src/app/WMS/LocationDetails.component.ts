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
  templateUrl: './LocationDetails.component.html'
})
export class LocationDetailsComponent implements OnInit {
  constructor(private messageService: MessageService,
    private currentRoute: ActivatedRoute,
    private commonComponent: commonComponent, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public LocationDetails: Array<any> = [];
  public employee: Employee;
  public fromDate: Date;
  public toDate: Date;
  public dynamicData: DynamicSearchResult;
  cols: any[];
  exportColumns: any[];
  pono: string;
  grnNo: string;
  QtyTotal: any = 0;
  materialid: any;

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.toDate = new Date();
    this.fromDate = new Date(new Date().setDate(new Date().getDate() - 30));
   

    this.cols = [
      { field: 'availableqty', header: 'Quantity' },
      { field: 'itemlocation', header: 'Item Location' },
    ];
    debugger;
    this.materialid = this.currentRoute.snapshot.queryParams.materialid;
    this.grnNo = this.currentRoute.snapshot.queryParams.grnNo;
    this.pono = this.currentRoute.snapshot.queryParams.pono;
    this.getLocationDetails(this.materialid);
    this.exportColumns = this.cols.map(col => ({ title: col.header, dataKey: col.field }));
  }

  

  backMaterialDetails() {
    this.router.navigate(['/WMS/MaterialDetails'], { queryParams: { grnNo: this.grnNo, pono: this.pono } });
  }

  getLocationDetails(materialid:string) {
    this.dynamicData = new DynamicSearchResult();
   // this.dynamicData.query = "select op.material , op.materialdescription,op.projectname, SUM(totalquantity) as Received , SUM(availableqty) as Balance ,SUM(totalquantity -availableqty ) as Issued, MAX(createddate) as createddate  from wms.wms_stock ws inner join wms.openpolistview  op on op.pono = ws.pono where ws.materialid notnull and createddate <='" + this.toDate.toDateString() + "' and createddate > '" + this.fromDate.toDateString() + "' group by  op.material , op.materialdescription,op.projectname"
    this.wmsService.getLocationDetails(materialid).subscribe(data => {
      debugger;
      this.LocationDetails = data;
      this.getTotal();
    });
  }

  getTotal() {
    for (var i = 0; i < this.LocationDetails.length; i++) {
      if (Number(this.LocationDetails[i].availableqty) && this.LocationDetails[i].availableqty != "undefined") {
        this.QtyTotal = this.QtyTotal + Number(this.LocationDetails[i].availableqty);
      }

    }
  }

  //export to excel 
  exportExcel() {
    this.commonComponent.exportExcel(this.LocationDetails, 'PO List');
  }

  //pdfexport
  exportPdf() {
    this.commonComponent.exportPdf(this.exportColumns, this.LocationDetails, 'PO List');

  }

}
