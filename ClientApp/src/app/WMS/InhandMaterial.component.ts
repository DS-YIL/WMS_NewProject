import { Component, OnInit, Inject, ViewChild } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee } from '../Models/Common.Model';
import { DecimalPipe } from '@angular/common';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { ConfirmationService } from 'primeng/api';
import { isNullOrUndefined } from 'util';
import { HttpClient } from '@angular/common/http';
import { testcrud, WMSHttpResponse, MaterialinHand, matlocations } from '../Models/WMS.Model';

@Component({
  selector: 'app-InhandMaterial',
  templateUrl: './InhandMaterial.component.html',
  providers: [ConfirmationService, DecimalPipe]
})
export class InhandMaterialComponent implements OnInit {

  constructor(private confirmationService: ConfirmationService, private decimalPipe: DecimalPipe, private http: HttpClient, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }
  
  
  public employee: Employee;
  getlistdata: MaterialinHand[] = [];
  getlocationlistdata: matlocations[] = [];
  showadddatamodel: boolean = false;
  lblmaterial: string = "";
  lblmaterialdesc: string = "";
  response: WMSHttpResponse;
  //availableValues= [];
  value: any;
  totalLength: any;
  totalLengthValues: any;

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.response = new WMSHttpResponse();
    this.getlist();
     
  }
  refreshsavemodel() {
    this.showadddatamodel = false;
    this.lblmaterial = "";
    this.lblmaterialdesc = "";
  }

  getlocations(material: string, data: MaterialinHand) {
    this.getlocationlistdata = [];
    this.lblmaterial = data.material;
    this.lblmaterialdesc = data.materialdescription;
    this.spinner.show();
    this.wmsService.getmatinhandlocations(material.trim()).subscribe(data => {
      this.getlocationlistdata = data;
      this.showadddatamodel = true;
      this.spinner.hide();
    });

  }

  getlist() {
    this.getlistdata = [];
   this.spinner.show();
    this.wmsService.getmatinhand().subscribe(data => {
      this.getlistdata = data;
      this.getTotalCount(data);
      this.spinner.hide();
    });
  }
  exportExcel() {
    let new_list = this.getlistdata.map(function (obj) {
      return {
        'Material': obj.material,
        'Material Description': obj.materialdescription,
        'Project Name': obj.projectname,
        'Supplier Name': obj.suppliername,
        'Hsncode': obj.hsncode,
        'Avialable Quantity': obj.availableqty,
        'Value': obj.value
      }
    });
    import("xlsx").then(xlsx => {
      const worksheet = xlsx.utils.json_to_sheet(new_list);
      const workbook = { Sheets: { 'data': worksheet }, SheetNames: ['data'] };
      const excelBuffer: any = xlsx.write(workbook, { bookType: 'xlsx', type: 'array' });
      this.saveAsExcelFile(excelBuffer, "invoicereport");
    });
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
  getTotalCount(data) {
    let availableValues = [];
    let totalValues = [];
    for (let i = 0;i< data.length; i++) {
      availableValues.push(data[i].availableqty)
      totalValues.push(data[i].value)
    }
  

    this.totalLength = availableValues.reduce(function (a, b) {
      return a + b;
    }, 0);

    this.totalLengthValues = totalValues.reduce(function (a, b) {
      return a + b;
    }, 0);

   
  }
  
}
