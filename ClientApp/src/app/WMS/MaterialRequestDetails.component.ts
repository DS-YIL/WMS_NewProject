import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { commonComponent } from '../WmsCommon/CommonCode';
import { DatePipe } from '@angular/common';
@Component({
  selector: 'app-MaterialRequestDetails',
  templateUrl: './MaterialRequestDetails.component.html',
  providers: [DatePipe]
})
export class MaterialRequestDetailsComponent implements OnInit {
  
  constructor(private messageService: MessageService, private datePipe: DatePipe,
    private currentRoute: ActivatedRoute,
    private commonComponent: commonComponent, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public MaterailRequestDetails: Array<any> = [];
  public MaterailReserveDetails: Array<any> = [];
  public employee: Employee;
  public fromDate: Date;
  public toDate: Date;
  public dynamicData: DynamicSearchResult;
  cols: any[];
  reservecols: any[];
  exportColumns: any[];
  pono: string;
  grnNo: string;
  type: any;
  poQty: any;
  materialcode: string;
  materialdescription: string;
  isissueview: boolean = false;
  QtyTotal: any = 0;

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.toDate = new Date();
    this.fromDate = new Date(new Date().setDate(new Date().getDate() - 30));
   

    this.cols = [
      { field: 'issuedqty', header: 'Issued Quantity' },
      { field: 'type', header: 'Type' },
      { field: 'RequestedBy', header: 'Requested By' },
      { field: 'requesteddate', header: 'Requested On' },
      { field: 'issuedby', header: 'Issued By' },
      { field: 'issuedon', header: 'Issued On' },
      { field: 'issuelocation', header: 'Issue Location' },
      { field: 'Approver', header: 'Approver' },
      { field: 'Acknowledged', header: 'Acknowledged By' },
    ];
    this.reservecols = [
      { field: 'reservequantity', header: 'Reserved Quantity' },
      { field: 'reservedby', header: 'Type' },
      { field: 'reservedon', header: 'Reserved On' },
      { field: 'reserveupto', header: 'Reserved Upto' },
      { field: 'issuelocation', header: 'Item Location' },
      { field: 'projectcode', header: 'Project' },
      { field: 'ackstatus', header: 'Status' },
    ];
    debugger;
    var materialid = this.currentRoute.snapshot.queryParams.materialid;
    this.materialcode = materialid;
    this.grnNo = this.currentRoute.snapshot.queryParams.grnNo;
    this.pono = this.currentRoute.snapshot.queryParams.pono;
    this.poQty = this.currentRoute.snapshot.queryParams.qty;
    this.type = this.currentRoute.snapshot.queryParams.type;
    this.isissueview = false;
    if (this.type == 'Issue') {
      this.isissueview = true;
      this.getMaterialRequestDetails(materialid, this.grnNo, this.pono);
    }
    else {
      this.getMaterialReserveDetails(materialid, this.grnNo, this.pono);
    }
    
    this.exportColumns = this.cols.map(col => ({ title: col.header, dataKey: col.field }));
  }

  checkValiddate(date: any) {
    try {
      if (!date || (this.datePipe.transform(date, this.constants.dateFormat) == "01/01/0001"))
        return "";
      else
        return date;
    }
    catch{
      return "";
    }
  }

  backMaterialDetails() {
    this.router.navigate(['/WMS/MaterialDetails'], { queryParams: { grnNo: this.grnNo, pono: this.pono, qty: this.poQty } });
  }

  getMaterialRequestDetails(materialid: string, grnnumber: string, pono: string) {
    this.dynamicData = new DynamicSearchResult();
   // this.dynamicData.query = "select op.material , op.materialdescription,op.projectname, SUM(totalquantity) as Received , SUM(availableqty) as Balance ,SUM(totalquantity -availableqty ) as Issued, MAX(createddate) as createddate  from wms.wms_stock ws inner join wms.openpolistview  op on op.pono = ws.pono where ws.materialid notnull and createddate <='" + this.toDate.toDateString() + "' and createddate > '" + this.fromDate.toDateString() + "' group by  op.material , op.materialdescription,op.projectname"
    this.wmsService.getMaterialRequestDetails(materialid, grnnumber,pono).subscribe(data => {
      debugger;
      this.MaterailRequestDetails = data;
      if (this.MaterailRequestDetails.length > 0) {
        this.materialdescription = "-" + this.MaterailRequestDetails[0].materialdescription;
      }
      this.MaterailRequestDetails.forEach(item =>
        item.issuedon = this.checkValiddate(item.issuedon)
        )
      this.getTotal();
    });
  }

  getMaterialReserveDetails(materialid: string, grnnumber: string, pono: string) {
    this.dynamicData = new DynamicSearchResult();
    // this.dynamicData.query = "select op.material , op.materialdescription,op.projectname, SUM(totalquantity) as Received , SUM(availableqty) as Balance ,SUM(totalquantity -availableqty ) as Issued, MAX(createddate) as createddate  from wms.wms_stock ws inner join wms.openpolistview  op on op.pono = ws.pono where ws.materialid notnull and createddate <='" + this.toDate.toDateString() + "' and createddate > '" + this.fromDate.toDateString() + "' group by  op.material , op.materialdescription,op.projectname"
    this.wmsService.getMaterialReserveDetails(materialid, grnnumber, pono).subscribe(data => {
      debugger;
      this.MaterailReserveDetails = data;
      if (this.MaterailReserveDetails.length > 0) {
        this.materialdescription = "-" + this.MaterailReserveDetails[0].materialdescription;
      }
      this.MaterailReserveDetails.forEach(item => {
        item.reservedon = this.checkValiddate(item.reservedon);
        item.reserveupto = this.checkValiddate(item.reserveupto);
      }    
      )
      this.getTotal1();
    });
  }

  getTotal1() {
    for (var i = 0; i < this.MaterailReserveDetails.length; i++) {
      if (Number(this.MaterailReserveDetails[i].reservequantity) && this.MaterailReserveDetails[i].reservequantity != "undefined") {
        this.QtyTotal = this.QtyTotal + Number(this.MaterailReserveDetails[i].reservequantity);
      }

    }
  }

  getTotal() {
    for (var i = 0; i < this.MaterailRequestDetails.length; i++) {
      if (Number(this.MaterailRequestDetails[i].issuedqty) && this.MaterailRequestDetails[i].issuedqty != "undefined") {
        this.QtyTotal = this.QtyTotal + Number(this.MaterailRequestDetails[i].issuedqty);
      }

    }
  }
}
