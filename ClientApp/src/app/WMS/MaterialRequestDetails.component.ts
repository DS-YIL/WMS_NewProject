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
  templateUrl: './MaterialRequestDetails.component.html'
})
export class MaterialRequestDetailsComponent implements OnInit {
  constructor(private messageService: MessageService,
    private currentRoute: ActivatedRoute,
    private commonComponent: commonComponent, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public MaterailRequestDetails: Array<any> = [];
  public employee: Employee;
  public fromDate: Date;
  public toDate: Date;
  public dynamicData: DynamicSearchResult;
  cols: any[];
  exportColumns: any[];
  pono: string;
  grnNo: string;
  QtyTotal: any = 0;

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.toDate = new Date();
    this.fromDate = new Date(new Date().setDate(new Date().getDate() - 30));
   

    this.cols = [
      { field: 'availableqty', header: 'Quantity' },
      { field: 'itemlocation', header: 'Requested By' },
      { field: 'requesterid', header: 'Received By' },
      { field: 'approverid', header: 'Acknowledged By' },
    ];
    debugger;
    var materialid = this.currentRoute.snapshot.queryParams.materialid;
    this.grnNo = this.currentRoute.snapshot.queryParams.grnNo;
    this.pono = this.currentRoute.snapshot.queryParams.pono;
    this.getMaterialRequestDetails(materialid);
    this.exportColumns = this.cols.map(col => ({ title: col.header, dataKey: col.field }));
  }

  

  backMaterialDetails() {
    this.router.navigate(['/WMS/MaterialDetails'], { queryParams: { grnNo: this.grnNo, pono: this.pono } });
  }

  getMaterialRequestDetails(materialid:string) {
    this.dynamicData = new DynamicSearchResult();
   // this.dynamicData.query = "select op.material , op.materialdescription,op.projectname, SUM(totalquantity) as Received , SUM(availableqty) as Balance ,SUM(totalquantity -availableqty ) as Issued, MAX(createddate) as createddate  from wms.wms_stock ws inner join wms.openpolistview  op on op.pono = ws.pono where ws.materialid notnull and createddate <='" + this.toDate.toDateString() + "' and createddate > '" + this.fromDate.toDateString() + "' group by  op.material , op.materialdescription,op.projectname"
    this.wmsService.getMaterialRequestDetails(materialid).subscribe(data => {
      debugger;
      this.MaterailRequestDetails = data;
      this.getTotal();
    });
  }

  getTotal() {
    for (var i = 0; i < this.MaterailRequestDetails.length; i++) {
      if (Number(this.MaterailRequestDetails[i].quantity) && this.MaterailRequestDetails[i].quantity != "undefined") {
        this.QtyTotal = this.QtyTotal + Number(this.MaterailRequestDetails[i].quantity);
      }

    }
  }
}
