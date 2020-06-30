import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute, RouterEvent, NavigationEnd } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { PoDetails, BarcodeModel } from 'src/app/Models/WMS.Model';
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

  ngOnInit() {

    debugger;
    this.showtable = false;
    this.showreceivedtable = false;
    this.btnreceivetext = "Show";
    this.btntext = "Show";
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
  }

  //page refresh functionality
  refresh() {
    this.PoDetails = new PoDetails();
    this.Poinvoicedetails = new PoDetails();
    this.disSaveBtn = false;
    this.showDetails = false;
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

  //get details based on po no
  SearchPoNo() {
    if (this.PoDetails.pono) {
      this.spinner.show();
      this.wmsService.getPoDetails(this.PoDetails.pono).subscribe(data => {
        this.spinner.hide();
        if (data) {
          this.PoDetails = data;
          this.showDetails = true;
          //document.getElementById('valdatediv').style.display = "none";
          // document.getElementById('ponoid').style.border = "1px solid grey";
        }
        else {
          this.PoDetails = new PoDetails();
          this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'No data for this PoNo' });
        }
      })
    }
    else
      this.messageService.add({ severity: 'error', summary: 'Validation', detail: 'Enter PoNo' });
  }

  //update invoice no
  onsaveSecDetails() {
    //need to generate barcode
    if (this.Poinvoicedetails.invoiceno && this.Poinvoicedetails.invoiceno.trim() != "") {
      this.spinner.show();
      this.BarcodeModel = new BarcodeModel();
      this.BarcodeModel.paitemid = 1;;
      this.BarcodeModel.barcode = "testbarcodetext";
      this.BarcodeModel.createdby = this.employee.employeeno;
      this.BarcodeModel.pono = this.PoDetails.pono;
      this.BarcodeModel.invoiceno = this.Poinvoicedetails.invoiceno;
      this.BarcodeModel.departmentid = this.PoDetails.departmentid
      this.BarcodeModel.receivedby = this.employee.employeeno;
      this.wmsService.insertbarcodeandinvoiceinfo(this.BarcodeModel).subscribe(data => {
        this.spinner.hide();
        if (data == 0) {
          this.messageService.add({ severity: 'error', summary: 'Response', detail: 'Something went wrong' });
        }
        else if (data == 2) {
          this.messageService.add({ severity: 'error', summary: 'Response', detail: 'Invoice for this PO already received' });
        }
        else { //data>=1
          this.disSaveBtn = true;
          this.messageService.add({ severity: 'success', summary: 'Success Message', detail: 'Invoice Updated' });
        }
      });
    }
    else
      this.messageService.add({ severity: 'error', summary: 'Validation', detail: 'Invoice number mandatory' });
  }
}
