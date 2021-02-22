import { Component, OnInit, Inject, ViewChild } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, searchList, DynamicSearchResult } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { PoDetails, inwardModel, ddlmodel, MRNsavemodel, POReportModel } from 'src/app/Models/WMS.Model';
import { MessageService, LazyLoadEvent } from 'primeng/api';
import { FormBuilder } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { ConfirmationService } from 'primeng/api';
import { isNullOrUndefined } from 'util';
import { HttpClient } from '@angular/common/http';
import { Table } from 'primeng/table';

@Component({
  selector: 'app-POReport',
  templateUrl: './POReport.component.html',
  providers: [DatePipe , ConfirmationService]
})
export class POReportComponent implements OnInit {
  @ViewChild('dt', { static: false }) table: Table;
  public url = "";
  constructor(private ConfirmationService: ConfirmationService, private http: HttpClient, private formBuilder: FormBuilder, private messageService: MessageService, private datePipe: DatePipe, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService, @Inject('BASE_URL') baseUrl: string) { this.url = baseUrl; }
  cols: any[];
  
  public employee: Employee;

  loading: boolean;
  showtable: boolean;

  public PoList: POReportModel[] = [];
  getVirtuallistdata: POReportModel[] = [];
  public itemlocationData: Array<any> = [];
  rowGroupMetadata: any;
  totalRecords: number;
  public PodetailList: POReportModel[] = [];
  public dialogueheader: string;
  public ponoview: string;
  public materialview: string;
  public materialdescriptionview: string;
  public showqty: string;
  selectedpo: any;
  AddDialog: boolean;
  currentDG: string;
  public searchItems: Array<searchList> = [];
  public dynamicData = new DynamicSearchResult();
  public searchresult: Array<object> = [];
  public isPM: boolean = false;
  public isAdmin: boolean = false;


  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.isPM = false;
    this.isAdmin = false;
    if (this.employee.roleid == "11") {
      this.isPM = true;
    }
    if (this.employee.roleid == "7") {
      this.isAdmin = true;
    }
    this.totalRecords = 0;
    this.loading = true;
    this.showtable = false;
    this.PoList = [];
    this.getVirtuallistdata = [];
    this.searchItems = [];
    this.itemlocationData = [];
    this.PodetailList = [];
    this.dialogueheader = "";
    this.ponoview = "";
    this.materialview = "";
    this.materialdescriptionview = "";
    this.showqty = "";
    this.currentDG = "";
    this.AddDialog = false;
    this.getPolist();
   
  }

  onposelected() {

    this.getPolist();

  }

  onDateSelect(value) {
    debugger;
    var dtx = this.formatDate(value);
    this.table.filter(this.formatDate(value), 'deliverydate', 'startsWith')
  }

  formatDate(date) {
    let month = date.getMonth() + 1;
    let day = date.getDate();

    if (month < 10) {
      month = '0' + month;
    }

    if (day < 10) {
      day = '0' + day;
    }

    return date.getFullYear() + '-' + month + '-' + day;
  }

  loadCarsLazy(event: LazyLoadEvent) {
    debugger;
    this.loading = true;
    setTimeout(() => {
      if (this.PoList) {
        this.getVirtuallistdata = this.PoList.slice(event.first, (event.first + event.rows));
        this.loading = false;
      }
    }, 1000);
  }

  updateRowGroupMetaData() {
    this.rowGroupMetadata = {};
    if (this.PoList) {
      for (let i = 0; i < this.PoList.length; i++) {
        let rowData = this.PoList[i];
        let brand = rowData.pono;
        if (i == 0) {
          this.rowGroupMetadata[brand] = { index: 0, size: 1 };
        }
        else {
          let previousRowData = this.PoList[i - 1];
          let previousRowGroup = previousRowData.pono;
          if (brand === previousRowGroup)
            this.rowGroupMetadata[brand].size++;
          else
            this.rowGroupMetadata[brand] = { index: i, size: 1 };
        }
      }
    }
  }

  getdetaildata(data: POReportModel, querytype: string, headertext: string, qty : number, requesttype: string = "") {
    
    if (!isNullOrUndefined(qty) && qty > 0) {
      this.currentDG = querytype;
      this.materialview = data.materialid;
      this.materialdescriptionview = data.poitemdescription,
        this.dialogueheader = headertext;
      this.showqty = String(qty);
      var empno = null;
      if (this.isPM) {
        empno = this.employee.employeeno;
      }
      this.spinner.show();
      this.wmsService.getporeportdetail(data.materialid, data.poitemdescription, data.pono, querytype, requesttype, null, empno).subscribe(data => {
        this.itemlocationData = data;
        this.spinner.hide();
        this.AddDialog = true;
      });
    }
    else {
      this.messageService.add({ severity: 'warn', summary: '', detail: 'Quantity is 0' });
      return;
    }
   
  }

  exportExcel() {
    import("xlsx").then(xlsx => {
      const worksheet = xlsx.utils.json_to_sheet(this.PoList);
      const workbook = { Sheets: { 'data': worksheet }, SheetNames: ['data'] };
      const excelBuffer: any = xlsx.write(workbook, { bookType: 'xlsx', type: 'array' });
      this.saveAsExcelFile(excelBuffer, "POdata");
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


  //get open po's based on current date(advance shipping notification list)
  getPolist() {
    debugger;
    this.PoList = [];
    this.spinner.show();
    this.showtable = false;
    var empno = null;
    var pcode = null;
    var pono = null;
    if (!isNullOrUndefined(this.selectedpo)) {
      if (this.selectedpo.name == "ALL") {
        pono = null;
      }
      else {
        pono = this.selectedpo.name;
      }
      
    }
    if (this.isPM) {
      empno = this.employee.employeeno;
    }
    this.wmsService.getporeport(empno,null,pono).subscribe(data => {
      this.PoList = data;
      if (this.PoList) {
        this.totalRecords = this.PoList.length;
      }
      this.updateRowGroupMetaData();
      this.showtable = true;
      this.spinner.hide();
    });
  }


  

  public bindSearchListDatamaterial(event: any) {
    debugger;
    var searchTxt = event.query;
    if (searchTxt == undefined)
      searchTxt = "";
    searchTxt = searchTxt.replace('*', '%');
    this.dynamicData = new DynamicSearchResult();
    var query = "(select wp.pono as pono";
    query += " from wms.wms_pomaterials wp";
    query += " left outer join wms.wms_project prj on prj.pono = wp.pono";
    
    query += " where wp.pono ilike '" + searchTxt + "%' and wp.pono is not null ";
    if (this.isPM) {
      query += " and prj.projectmanager = '" + this.employee.employeeno + "' ";
    }
    query += " group by wp.pono limit 50)";
    query += " union";
    query += " (select mmy.pono as pono";
    query += " from wms.wms_stock mmy";
    query += " left outer join wms.wms_project prj on prj.pono = mmy.pono";
    query += " where mmy.pono ilike '" + searchTxt + "%' and mmy.pono is not null";
    if (this.isPM) {
      query += " and prj.projectmanager = '" + this.employee.employeeno + "' ";
    }
    query += " group by mmy.pono limit 50)";
    this.dynamicData.query = query;
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.searchresult = data;
      this.searchItems = [];
      var fName = "";
      var initialvalue = { listName: "pono", name: "ALL", code: "ALL" };
      this.searchItems.push(initialvalue);
      this.searchresult.forEach(item => {
        fName = item["pono"];
        var value = { listName: "pono", name: fName, code: fName };
        this.searchItems.push(value);
      });
    });
  }

  

  



 
  //search list option changes event
  
 


}
