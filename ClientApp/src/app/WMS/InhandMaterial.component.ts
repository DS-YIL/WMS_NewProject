import { Component, OnInit, Inject, ViewChild } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult } from '../Models/Common.Model';
import { DecimalPipe } from '@angular/common';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { ConfirmationService } from 'primeng/api';
import { isNullOrUndefined } from 'util';
import { HttpClient } from '@angular/common/http';
import { testcrud, WMSHttpResponse, MaterialinHand, matlocations, inventoryFilters } from '../Models/WMS.Model';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-InhandMaterial',
  templateUrl: './InhandMaterial.component.html',
  providers: [ConfirmationService, DecimalPipe, DatePipe]
})
export class InhandMaterialComponent implements OnInit {

  constructor(private confirmationService: ConfirmationService, private datePipe: DatePipe, private decimalPipe: DecimalPipe, private http: HttpClient, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }


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
  public inventoryFilters: inventoryFilters;
  public locationList: Array<any> = [];
  public dynamicData: DynamicSearchResult;
  currentDate = new Date();

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.response = new WMSHttpResponse();
    this.inventoryFilters = new inventoryFilters();
    this.getItemLocationsList();
    this.getlist();

  }

  //get material details by materialid
  getItemLocationsList() {
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.query = "select itemlocation from wms.wms_stock ws group by ws.itemlocation ";
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.locationList = data;
    });
  }
  refreshsavemodel() {
    this.showadddatamodel = false;
    this.lblmaterial = "";
    this.lblmaterialdesc = "";
  }

  getlocations(poitemdescription: string, data: MaterialinHand) {
    this.getlocationlistdata = [];
    this.lblmaterial = data.material;
    this.lblmaterialdesc = poitemdescription;
    this.spinner.show();
    this.wmsService.getmatinhandlocations(poitemdescription).subscribe(data => {
      this.getlocationlistdata = data;
      this.showadddatamodel = true;
      this.spinner.hide();
    });

  }

  getlist() {
    this.getlistdata = [];
    this.spinner.show();
    this.wmsService.getmatinhand(this.inventoryFilters).subscribe(data => {
      this.getlistdata = data;
      this.getTotalCount(data);
      this.spinner.hide();
    });
  }
  exportExcel() {
    //this.getlistdata[0].itemlocation = this.inventoryFilters.itemlocation;
    let new_list = this.getlistdata.map(function (obj) {
      return {
        'PO No.': obj.pono,
        'Material': obj.material,
        'PO Item description': obj.poitemdescription,
        'Project code': obj.projectname,
        'Supplier Name': obj.suppliername,
        'Hsncode': obj.hsncode,
        'Available Quantity': obj.availableqty,
        'Value': obj.value,
        //'Location': obj.itemlocation
      }
    });
    import("xlsx").then(xlsx => {
      const worksheet = xlsx.utils.json_to_sheet(new_list);
      const workbook = { Sheets: { 'data': worksheet }, SheetNames: ['data'] };
      const excelBuffer: any = xlsx.write(workbook, { bookType: 'xlsx', type: 'array' });
      this.saveAsExcelFile(excelBuffer, "InventoryReport");
    });
  }

  saveAsExcelFile(buffer: any, fileName: string): void {
    import("file-saver").then(FileSaver => {
      let EXCEL_TYPE = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=UTF-8';
      let EXCEL_EXTENSION = '.xlsx';
      const data: Blob = new Blob([buffer], {
        type: EXCEL_TYPE
      });
      
      var date = this.datePipe.transform(this.currentDate, 'dd-MM-yyyy');
      FileSaver.saveAs(data, fileName +"_"+ date + EXCEL_EXTENSION);
    });
  }
  getTotalCount(data) {
    let availableValues = [];
    let totalValues = [];
    for (let i = 0; i < data.length; i++) {
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
