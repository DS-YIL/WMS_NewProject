import { Component, OnInit, Inject, ViewChild, ElementRef } from '@angular/core';
import { Router, ActivatedRoute, RouterEvent, NavigationEnd } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, POList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { PoDetails, BarcodeModel, ddlmodel, PrintHistoryModel } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { filter } from 'rxjs/operators';
import { isNullOrUndefined } from 'util';
import { HttpClient } from '@angular/common/http';
import { SelectItem } from 'primeng/api';
import { POListComponent } from './POList.component';


@Component({
  selector: 'app-SecurityHome',
  templateUrl: './SecurityHome.component.html'
})
export class SecurityHomeComponent implements OnInit {
  cities1: SelectItem[];
  selectedCities1: any[];
  selectedCountries1: string[] = [];
  public url = "";
  @ViewChild('fileInput', { static: false })
  myInputVariable: ElementRef;
  constructor(private messageService: MessageService, private http: HttpClient, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService, @Inject('BASE_URL') baseUrl: string) {
  this.url = baseUrl;
    //SelectItem API with label-value pairs
    this.cities1 = [
      { label: 'New York', value: { id: 1, name: 'New York', code: 'NY' } },
      { label: 'Rome', value: { id: 2, name: 'Rome', code: 'RM' } },
      { label: 'London', value: { id: 3, name: 'London', code: 'LDN' } },
      { label: 'Istanbul', value: { id: 4, name: 'Istanbul', code: 'IST' } },
      { label: 'Paris', value: { id: 5, name: 'Paris', code: 'PRS' } }
    ];
  }
  public print: string = "Print Barcode";
  public PoDetails: PoDetails;
  public Poinvoicedetails: PoDetails;
  public employee: Employee;
  public pono: any = "";
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
  transportdetails: string = "";
  nonpofile: any;
  clicked: boolean = false;
  public showPrintBtn: boolean = false;
  public PrintHistoryModel: PrintHistoryModel;
  public podatavisible: boolean;
  POlist: POList[];
  public selectedPOs: POList[];
  public multiplepo: boolean = false;

  ngOnInit() {

    debugger;
    this.searchdata = "";
    this.ispochecked = true;
    this.showtable = true;
    this.clicked = false;
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
    ///get todays expected shipments
    this.getcurrentDatePolist();

    ///get todays received shipments
    this.getcurrentDateReceivedPOlist();

    ///get department master list
    this.getdepts();
   // this.SearchPoNo();
  }

  //page refresh functionality
  refresh() {
    this.PoDetails = new PoDetails();
    this.Poinvoicedetails = new PoDetails();
    this.nonporemarks = "";
    this.transportdetails = "";
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

  OnMultipleSelects($event) {
    //console.log($event)
    //console.log($event.target)
    //console.log($event.target.value)

    debugger;
   //alert('hi')
    this.podatavisible = true;

   
  }

  OnMultipleSelect(event: any, suppliername: string) {
    
    this.spinner.show();
    //alert(ischecked["checked"]);
    //Get polist data
    debugger;
    var data = event.target.checked;
    if(data) {
      this.podatavisible = true;
      this.wmsService.getPODataList(suppliername).subscribe(data => {
        debugger;
        this.POlist = data;
        this.multiplepo = true;
        debugger;

        this.spinner.hide();
      });
    }
    else {
      debugger;
      this.POlist = [];
      this.PoDetails.pono = '';
      this.selectedPOs = [];
      this.multiplepo = false;
      this.spinner.hide();
    }
    
  }

  //close on submit
  hidepolist() {
    //debugger;
    this.PoDetails.pono = "";
    this.podatavisible = false;
    console.log(this.selectedPOs);
    this.selectedPOs.forEach(item => {
      if (
        this.PoDetails.pono == ""
      ) {
        this.PoDetails.pono = item.pOno;

      }
      else {
        this.PoDetails.pono = this.PoDetails.pono + ',' + item.pOno;

      }
      //alert(item.p)
      //this.PoDetails.pono = this.PoDetails.pono + ',' + item.pOno;

    }
    )
  }

  reset() {
    this.myInputVariable.nativeElement.value = null;
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

  //get received POs for today
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
    this.disSaveBtn = false;
    this.isnonpochecked = false;
  }
  nonpocheck() {
    this.PoDetails.pono = "";
    this.Poinvoicedetails.vendorname = "";
    this.Poinvoicedetails.invoiceno = "";
    this.ispochecked = false;
    this.isnonpochecked = true;
    this.disSaveBtn = false;
    this.showPrintBtn = false;
    this.print = "Print Barcode";
  }

  getdepts() {
    this.wmsService.getdepartments().subscribe(data => {
      debugger;
      this.ddldeptlist = data;
    });
  }
 
  //get details based on po no
  SearchPoNo() {
    debugger;
    if (this.searchdata) {
      this.disSaveBtn = false;
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
          console.log(this.PoDetails)
          this.messageService.add({ severity: 'error', summary: '', detail: 'No data for this ASN/PO No./Supplier Name' });
          this.showDetails = false;

        }
      })

    }
    else {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter PO/ASN No./Supplier Name' });
    }
  }


  onBasicUpload(event) {
    debugger;
    this.nonpofile = event.target.files[0];
  }

  uploadnonpodoc(pono: string) {
    debugger;
    this.pono = pono;
    if (!isNullOrUndefined(this.nonpofile)) {
      
      let file = this.nonpofile;
      var fname = pono + "_" + file.name;
      const formData = new FormData();
      formData.append('file', file, fname);
      this.http.post(this.url + 'POData/uploaddoc', formData)
        .subscribe(event => {
          this.reset();
          this.nonpofile = null;
          //this.messageService.add({ severity: 'success', summary: '', detail: 'File uploaded' });
        });

    }

  }

  //update invoice no
  onsaveSecDetails() {
    //need to generate barcode
    if (!this.clicked) {
      this.clicked = true;
      this.spinner.show();
      this.BarcodeModel = new BarcodeModel();
      this.BarcodeModel.paitemid = 1;
      this.BarcodeModel.barcode = this.searchdata + "_" + this.Poinvoicedetails.invoiceno;
      this.BarcodeModel.createdby = this.employee.employeeno;
      if (this.multiplepo == true) {
        this.BarcodeModel.polist = this.selectedPOs;
      }
      else {
        this.BarcodeModel.pono = this.PoDetails.pono;
      }
    
      this.BarcodeModel.asnno = this.PoDetails.asnno;
      this.BarcodeModel.departmentid = this.PoDetails.departmentid;
      this.BarcodeModel.inwardremarks = this.nonporemarks;
      this.BarcodeModel.vehicleno = this.Poinvoicedetails.vehicleno;
      this.BarcodeModel.transporterdetails = this.transportdetails;
      this.BarcodeModel.suppliername = this.PoDetails.vendorname;
      if (this.isnonpochecked) {
        this.BarcodeModel.pono = "NONPO";
        if (isNullOrUndefined(this.Poinvoicedetails.vendorname) || this.Poinvoicedetails.vendorname == "") {
          this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Supplier Name' });
          this.spinner.hide();
          this.clicked = false;
          return;
        }
        if (!isNullOrUndefined(this.selecteddept)) {
          this.BarcodeModel.departmentid = parseInt(this.selecteddept.value);
        }
        else {
          this.messageService.add({ severity: 'error', summary: '', detail: 'Select Department' });
          this.spinner.hide();
          this.clicked = false;
          return;
        }
        this.BarcodeModel.suppliername = this.Poinvoicedetails.vendorname;
      }
      else {
        if (this.PoDetails.vendorname == "" || isNullOrUndefined(this.PoDetails.vendorname)) {
          this.spinner.hide();
          this.messageService.add({ severity: 'error', summary: '', detail: 'Suppliername Not Defined' });
          this.clicked = false;
          return;
        }

        if (this.PoDetails.pono == "" || isNullOrUndefined(this.PoDetails.pono)) {
          this.spinner.hide();
          this.messageService.add({ severity: 'error', summary: '', detail: 'PONo. Not  Defined' });
          this.clicked = false;
          return;
        }
      }

      if (this.Poinvoicedetails.invoiceno && this.Poinvoicedetails.invoiceno.trim() != "") {
      }
      else {
        this.spinner.hide();
        this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Invoice No.' });
        this.clicked = false;
        return;

      }
      if (this.Poinvoicedetails.invoiceno.includes("-")) {
        this.spinner.hide();
        this.messageService.add({ severity: 'error', summary: '', detail: 'Remove "-" from invoice no.' });
        this.clicked = false;
        return;

      }

      this.BarcodeModel.invoiceno = this.Poinvoicedetails.invoiceno;

      this.BarcodeModel.receivedby = this.employee.employeeno;
      if (this.isnonpochecked) {
        if (this.nonpofile) {
          let file = this.nonpofile;
          this.BarcodeModel.docfile = file.name;
        }

      }
      this.wmsService.insertbarcodeandinvoiceinfo(this.BarcodeModel).subscribe(data => {
        this.spinner.hide();
        this.clicked = false;
        if (data == "0") {
          this.messageService.add({ severity: 'error', summary: '', detail: 'Something went wrong' });
        }
        else if (data == "2") {
          this.messageService.add({ severity: 'error', summary: '', detail: 'Invoice for this PO already received' });
          this.showPrintBtn = true;
          this.print = "Print Barcode";
        }
        else if (data == "3") {
          this.messageService.add({ severity: 'error', summary: '', detail: 'Invoice for this PO already received' });
          this.showPrintBtn = true;
          this.print = "Re-Print Barcode";
        }
        else if (String(data).startsWith("Error")) {
          this.messageService.add({ severity: 'error', summary: '', detail: data });
          this.getcurrentDateReceivedPOlist();

        }
        else { //data>=1
          this.showPrintBtn = true;
          if (String(data).startsWith("NP")) {
            this.uploadnonpodoc(data);
          }
          this.disSaveBtn = true;
          //this.refresh();
          this.messageService.add({ severity: 'success', summary: '', detail: 'Invoice No. Updated' });
          this.print = "Print Barcode";
          this.showPrintBtn = true;
          this.getcurrentDateReceivedPOlist();
        }
      });
      
    }
    else {
      this.messageService.add({ severity: 'info', summary: '', detail: 'Processing' });
    }
    //this.reset();
  }

  printbarcode() {
    //this.spinner.show();
    debugger;
    if (this.searchdata) {
      var po_invoiceNo = this.searchdata + "_" + this.Poinvoicedetails.invoiceno;
    }
    else {
      var po_invoiceNo = this.pono + "_" + this.Poinvoicedetails.invoiceno;
    }
    
    this.PrintHistoryModel = new PrintHistoryModel();
    this.PrintHistoryModel.reprintedby = this.employee.employeeno;
    this.PrintHistoryModel.po_invoice = po_invoiceNo;
    if (this.searchdata != null && this.searchdata != "") {
      this.PrintHistoryModel.pono = this.searchdata;
    }
    else {
      this.PrintHistoryModel.pono = this.pono;
    }

    this.PrintHistoryModel.invoiceNo = this.Poinvoicedetails.invoiceno;
    this.wmsService.printBarcode(this.PrintHistoryModel).subscribe(data => {
      this.spinner.hide();
      debugger;
      if (data == "success") {
        this.messageService.add({ severity: 'success', summary: '', detail: 'QRCode Printed Successfully' });
        this.print = "Re-Print Barcode";
        if (this.pono.startswith("NP")) {
          this.showPrintBtn = false;
          this.refresh();
        }
        else {
          
          this.refresh();
        }
        
      }
      else {
        this.messageService.add({ severity: 'success', summary: '', detail: 'Error while printing QRCode' });
      }
      
      //if (data)
      //this.messageService.add({ severity: 'success', summary: '', detail: 'Invoice No. Updated' });
    })

  }

}
