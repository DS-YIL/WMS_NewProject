import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute, RouterEvent, NavigationEnd } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { PoDetails, BarcodeModel, ddlmodel } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { filter } from 'rxjs/operators';
import { isNullOrUndefined } from 'util';


@Component({
  selector: 'app-SecurityHome',
  templateUrl: './SecurityHome.component.html'
})
export class SecurityHomeComponent implements OnInit {

  constructor(private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public PoDetails: PoDetails;
  public Poinvoicedetails: PoDetails;
  public employee: Employee;
  public showDetails; disSaveBtn; showPoList: boolean = false;
  showreceiveList: boolean = false;
  showreceivedPoList: boolean = false;
  public BarcodeModel: BarcodeModel;
  public currentDatePoList: Array<any> = [];
  public currentDatePoReceivedList: Array<any> = [];
  public dynamicData: DynamicSearchResult;
  showtable: boolean = false;
  showreceivedtable: boolean = false;
  btntext: string = "";
  deliverycount: string = "0";
  receivedcount: string = "0";
  btnreceivetext: string = "";
  todatsdate: Date;
  ispochecked: boolean = false;
  isnonpochecked: boolean = false;
  ddldeptlist: ddlmodel[] = [];
  selecteddept: ddlmodel;
  searchdata: string = "";
  nonporemarks: string = "";

  ngOnInit() {

    debugger;
    this.searchdata = "";
    this.ispochecked = true;
    this.showtable = true;
    this.showreceivedtable = false;
    this.btnreceivetext = "Show";
    this.btntext = "Hide";
    this.deliverycount = "0";
    this.receivedcount = "0";
    this.todatsdate = new Date();
    this.router.events.pipe(
      filter((event: RouterEvent) => event instanceof NavigationEnd)
    ).subscribe(() => {
      this.refresh();
    });

    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.PoDetails = new PoDetails();
    this.Poinvoicedetails = new PoDetails();
    this.getcurrentDatePolist();
    this.getcurrentDateReceivedPOlist();
    this.getdepts();
  }

  //page refresh functionality
  refresh() {
    this.PoDetails = new PoDetails();
    this.Poinvoicedetails = new PoDetails();
    this.nonporemarks = "";
    this.searchdata = "";
    this.selecteddept = null;
    this.disSaveBtn = false;
    this.showDetails = false;
  }

  getsuppliername(data: any) {
    var pono = data.pono;
    if (pono.startsWith("NP")) {
      return data.npsuppliername;
    }
    else {
      return data.suppliername;
    }
    

  }

  //get open po's based on current date(advance shipping notification list)
  getcurrentDatePolist() {
    this.spinner.show();
    var date = new Date();
    var month = parseInt(date.getMonth().toString()) + 1;
    var currentDate = date.getFullYear() + '-' + month + '-' + date.getDate();
    this.wmsService.getASNList(currentDate).subscribe(data => {
      this.showPoList = true;
      this.currentDatePoList = data;
      if (!isNullOrUndefined(this.currentDatePoList)) {
        this.deliverycount = String(this.currentDatePoList.length);
      }
      this.spinner.hide();
    });
  }

  getcurrentDateReceivedPOlist() {
    this.spinner.show();
    this.wmsService.getASNPOReceivedList().subscribe(data => {
      this.showreceivedPoList = true;
      this.currentDatePoReceivedList = data;
      if (!isNullOrUndefined(this.currentDatePoReceivedList)) {
        this.receivedcount = String(this.currentDatePoReceivedList.length);
      }
      this.spinner.hide();
    });
  }

  showdevlist() {
    this.showtable = !this.showtable;
    if (this.showtable) {
      this.btntext = "Hide";
    }
    else {
      this.btntext = "Show";
    }
  }
  showreceivedlist() {
    this.showreceivedtable = !this.showreceivedtable;
    if (this.showreceivedtable) {
      this.btnreceivetext = "Hide";
    }
    else {
      this.btnreceivetext = "Show";
    }
  }
  pocheck() {
    this.Poinvoicedetails.vendorname = "";
    this.Poinvoicedetails.invoiceno = "";
    this.ispochecked = true;
    this.isnonpochecked = false;
  }
  nonpocheck() {
    this.PoDetails.pono = "";
    this.Poinvoicedetails.vendorname = "";
    this.Poinvoicedetails.invoiceno = "";
    this.ispochecked = false;
    this.isnonpochecked = true;
  }

  getdepts() {
    this.wmsService.getdepartments().subscribe(data => {
      debugger;
      this.ddldeptlist = data;
    });
  }

  //get details based on po no
  SearchPoNo() {
    if (this.searchdata) {
      this.spinner.show();
      this.wmsService.getPoDetails(this.searchdata).subscribe(data => {
        this.spinner.hide();
        if (data) {
          this.PoDetails = data;
          this.showDetails = true;
          //document.getElementById('valdatediv').style.display = "none";
          // document.getElementById('ponoid').style.border = "1px solid grey";
        }
        else {
          this.PoDetails = new PoDetails();
          this.messageService.add({ severity: 'error', summary: '', detail: 'No data for this ASN/PO No.' });
          this.showDetails = false;
        }
      })
    }
    else
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter PO/ASN No.' });
  }

  printbarcode() {

  }

  //update invoice no
  onsaveSecDetails() {
    //need to generate barcode
    debugger;
    if (isNullOrUndefined(this.Poinvoicedetails.vendorname) || this.Poinvoicedetails.vendorname == "") {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Supplier Name' });
      this.spinner.hide();
      return;
    }
    if (this.Poinvoicedetails.invoiceno && this.Poinvoicedetails.invoiceno.trim() != "") {
      this.spinner.show();
      this.BarcodeModel = new BarcodeModel();
      this.BarcodeModel.paitemid = 1;;
      this.BarcodeModel.barcode = "testbarcodetext";
      this.BarcodeModel.createdby = this.employee.employeeno;
      this.BarcodeModel.pono = this.PoDetails.pono;
      this.BarcodeModel.asnno = this.PoDetails.asnno;
      this.BarcodeModel.departmentid = this.PoDetails.departmentid;
      this.BarcodeModel.inwardremarks = this.nonporemarks;
      if (this.isnonpochecked) {
        this.BarcodeModel.pono = "NONPO";
        if (isNullOrUndefined(this.Poinvoicedetails.vendorname) || this.Poinvoicedetails.vendorname == "") {
          this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Supplier Name' });
          this.spinner.hide();
          return;
        }
        if (!isNullOrUndefined(this.selecteddept)) {
          this.BarcodeModel.departmentid = parseInt(this.selecteddept.value);
        }
        else {
          this.messageService.add({ severity: 'error', summary: '', detail: 'Select Department' });
          this.spinner.hide();
          return;
        }
        
      }
      this.BarcodeModel.invoiceno = this.Poinvoicedetails.invoiceno;
      
      this.BarcodeModel.receivedby = this.employee.employeeno;
      this.BarcodeModel.suppliername = this.Poinvoicedetails.vendorname;
      this.wmsService.insertbarcodeandinvoiceinfo(this.BarcodeModel).subscribe(data => {
        this.spinner.hide();
        if (data == 0) {
          this.messageService.add({ severity: 'error', summary: '', detail: 'Something went wrong' });
        }
        else if (data == 2) {
          this.messageService.add({ severity: 'error', summary: '', detail: 'Invoice No. for this PO already received' });
        }
        else { //data>=1
          this.disSaveBtn = true;
          this.refresh();
          this.messageService.add({ severity: 'success', summary: '', detail: 'Invoice No. Updated' });
          this.getcurrentDateReceivedPOlist();
        }
      });
    }
    else
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Invoice No.' });
  }
}
