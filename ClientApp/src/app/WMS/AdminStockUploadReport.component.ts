import { Component, OnInit, Inject, ViewChild } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService, LazyLoadEvent } from 'primeng/api';
import { ConfirmationService } from 'primeng/api';
import { isNullOrUndefined } from 'util';
import { HttpClient } from '@angular/common/http';
import { testcrud, WMSHttpResponse, StockModel } from '../Models/WMS.Model';
import { Table } from 'primeng/table';

@Component({
  selector: 'app-AdminStockUploadReport',
  templateUrl: './AdminStockUploadReport.component.html',
  providers: [ConfirmationService]
})
export class AdminStockUploadReportComponent implements OnInit {
  @ViewChild('dt1', { static: false }) table: Table;

  constructor(private confirmationService: ConfirmationService, private http: HttpClient, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) {
  }
  
  
  public employee: Employee;
  getlistdata: StockModel[] = [];
  tempdata:any[] = [];
  getVirtuallistdata: StockModel[] = [];
  viewmain: boolean = false;
  viewdetail: boolean = false;
  viewexception: boolean = false;
  getmainlistdata: StockModel[] = [];
  editStockModel: StockModel;
  lblfilename: string = "";
  lbldate: Date;
  lblqty: number;
  lblvalue: string = "";
  loading: boolean = false;
  totalRecords: number;
  showadddatamodel: boolean = false;

  datainaction: StockModel;
  currentstage: string = "";

  ///to show lable dynamically on edit
  ismaterial: boolean = false;
  isdescription: boolean = false;
  isstore: boolean = false;
  israck: boolean = false;
  isbin: boolean = false;
  isquantity: boolean = false;
  isvalue: boolean = false;
  isprojectid: boolean = false;
  ispono: boolean = false;
  

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.viewmain = true;
    this.viewdetail = false;
    this.viewexception = false;
    this.showadddatamodel = false;
    this.editStockModel = new StockModel();
    this.datainaction = new StockModel();
    this.currentstage = "";
    this.getMainlist();

    this.ismaterial = false;
    this.isdescription = false;
    this.isstore = false;
    this.israck = false;
    this.isbin = false;
    this.isquantity = false;
    this.isvalue = false;
    this.isprojectid = false;
    this.ispono = false;
     
  }

  getMainlist() {
    this.getmainlistdata = [];
    this.viewexception = false;
    this.spinner.show();
    this.wmsService.getinitialStockReportGroup(this.employee.employeeno).subscribe(data => {
      this.getmainlistdata = data;
      this.spinner.hide();
    });
  }

  onDateSelect(value) {
    debugger;
    var dtx = this.formatDate(value);
    this.table.filter(this.formatDate(value), 'createddate', 'startsWith')
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

  backtomain() {
    this.lblfilename = "";
    this.lbldate = null;
    this.lblqty = 0;
    this.lblvalue = "";
    this.viewmain = true;
    this.viewdetail = false;
    this.viewexception = false;
  }

  loadCarsLazy(event: LazyLoadEvent) {
    debugger;
    this.loading = true;

    //in a real application, make a remote request to load data using state metadata from event
    //event.first = First row offset
    //event.rows = Number of rows per page
    //event.sortField = Field name to sort with
    //event.sortOrder = Sort order as number, 1 for asc and -1 for dec
    //filters: FilterMetadata object having field as key and filter value, filter matchMode as value

    //imitate db connection over a network
    setTimeout(() => {
      if (this.getlistdata) {
        this.getVirtuallistdata = this.getlistdata.slice(event.first, (event.first + event.rows));
        this.loading = false;
      }
    }, 1000);
  }
  refreshsavemodel() {
    this.editStockModel = new StockModel();
  }

  EditStock(rowData: StockModel) {
    this.setEditControls(rowData);
    this.editStockModel = rowData;
    this.showadddatamodel = true;

  }

  setEditControls(rowData: StockModel) {
    this.ismaterial = false;
    this.isdescription = false;
    this.isstore = false;
    this.israck = false;
    this.isbin = false;
    this.isquantity = false;
    this.isvalue = false;
    this.isprojectid = false;
    this.ispono = false;
    if (!isNullOrUndefined(rowData.material) && String(rowData.material).trim() != "") {
      this.ismaterial = true;
    }
    if (!isNullOrUndefined(rowData.materialdescription) && String(rowData.materialdescription).trim() != "") {
      this.isdescription = true;
    }
    if (!isNullOrUndefined(rowData.locatorname) && String(rowData.locatorname).trim() != "") {
      this.isstore = true;
    }
    if (!isNullOrUndefined(rowData.racknumber) && String(rowData.racknumber).trim() != "") {
      this.israck = true;
    }
    if (!isNullOrUndefined(rowData.binnumber) && String(rowData.binnumber).trim() != "") {
      this.isbin = true;
    }
    if (!isNullOrUndefined(rowData.availableqty) && rowData.availableqty > 0) {
      this.isquantity = true;
    }
    if (!isNullOrUndefined(rowData.value) && rowData.value > 0) {
      this.isvalue = true;
    }
    if (!isNullOrUndefined(rowData.projectid) && String(rowData.projectid).trim() != "") {
      this.isprojectid = true;
    }
    if (!isNullOrUndefined(rowData.pono) && String(rowData.pono).trim() != "") {
      this.ispono = true;
    }


  }

  setdataaftersave() {
    debugger;
    this.getmainlistdata = [];
    this.viewmain = true;
    this.viewdetail = false;
    this.viewexception = false;
    this.showadddatamodel = false;
    this.spinner.show();
    this.wmsService.getinitialStockReportGroup(this.employee.employeeno).subscribe(data => {
      this.getmainlistdata = data;
      this.spinner.hide();
      var actioncode = this.datainaction.uploadbatchcode;
      var data = this.getmainlistdata.filter(o => o.uploadbatchcode == actioncode);
      if (data.length > 0) {
        if (this.currentstage == "Exception") {
          this.getexlist(data[0]);
        }
        else if (this.currentstage == "ALL") {
          this.getalllist(data[0]);
        }
        else if (this.currentstage == "Success") {
          this.getlist(data[0]);
        }
      }
    });
   
   
    

  }



  getexlist(data: StockModel) {
    this.currentstage = "Exception";
    this.datainaction = data;
    this.lblfilename = data.uploadedfilename;
    this.lbldate = data.createddate;
    this.lblqty = data.exceptionrecords;
    this.lblvalue = "Exception Records"
    this.getlistdata = [];
    var uploadcode = data.uploadbatchcode;
    this.viewexception = false;
    this.loading = true;
    this.viewdetail = false;
    this.spinner.show();
    this.wmsService.getinitialStockEX(uploadcode).subscribe(data => {
      debugger;
      this.getlistdata = data;
      this.viewmain = false;
      this.viewdetail = true;
      this.viewexception = true;
      this.totalRecords = this.getlistdata.length;
      this.spinner.hide();
    });
  }

  getalllist(data: StockModel) {
    this.currentstage = "All";
    this.datainaction = data;
    this.lblfilename = data.uploadedfilename;
    this.lbldate = data.createddate;
    this.lblqty = data.totalrecords;
    this.lblvalue = "Total Records"
    this.getlistdata = [];
    var uploadcode = data.uploadbatchcode;
    this.viewexception = false;
    this.loading = true;
    this.viewmain = false;
    this.viewexception = true;
    this.viewdetail = false;
    this.spinner.show();
    this.wmsService.getinitialStockAllrecords(uploadcode).subscribe(data => {
      this.getlistdata = data;
      this.totalRecords = this.getlistdata.length;
      this.viewdetail = true;
      this.spinner.hide();
    });
  }

  post() {
    debugger;
    var rowData = this.editStockModel;
    if (isNullOrUndefined(rowData.material) || String(rowData.material).trim() == "") {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter material' });
      return;
    }
    if (isNullOrUndefined(rowData.materialdescription) || String(rowData.materialdescription).trim() == "") {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter material description' });
      return;
    }
    if (isNullOrUndefined(rowData.locatorname) || String(rowData.locatorname).trim() == "") {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter store' });
      return;
    }
    if (isNullOrUndefined(rowData.racknumber) || String(rowData.racknumber).trim() == "") {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter rack' });
      return;
    }
    if (isNullOrUndefined(rowData.binnumber) || String(rowData.binnumber).trim() == "") {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter bin' });
      return;
    }
    if (isNullOrUndefined(rowData.availableqty) || rowData.availableqty == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter quantity' });
      return;
    }
    if (isNullOrUndefined(rowData.value) || rowData.value == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter value' });
      return;
    }
    if (isNullOrUndefined(rowData.projectid) || String(rowData.projectid).trim() == "") {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter projectid' });
      return;
    }
    if (isNullOrUndefined(rowData.pono) || String(rowData.pono).trim() == "") {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter pono' });
      return;
    }
    this.editStockModel.createdby = this.employee.employeeno;
    this.spinner.show();
    this.wmsService.updateinitialstock(this.editStockModel).subscribe(data => {
      this.spinner.hide();
      debugger;
      this.messageService.add({ severity: 'success', summary: '', detail: data });
      this.showadddatamodel = false;
      this.setdataaftersave();

    });

  }

  getlist(data: StockModel) {
    this.currentstage = "Success";
    this.datainaction = data;
    this.lblfilename = data.uploadedfilename;
    this.lbldate = data.createddate;
    this.lblqty = data.successrecords;
    this.lblvalue = "Success Records"
    this.getlistdata = [];
    this.loading = true;
    var uploadcode = data.uploadbatchcode;
    this.viewmain = false;
    
    this.viewexception = false;
    this.viewdetail = false;
    this.spinner.show();
    this.wmsService.getinitialStock(uploadcode).subscribe(data => {
      this.getlistdata = data;
      this.viewdetail = true;
      this.totalRecords = this.getlistdata.length;
      this.spinner.hide();
    });
  }
 
}
