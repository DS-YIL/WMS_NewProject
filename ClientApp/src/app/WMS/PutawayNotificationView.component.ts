import { Component, OnInit, Inject } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList, printMaterial } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { PoDetails, BarcodeModel, StockModel, inwardModel, locationddl, binddl, rackddl, locataionDetailsStock, ddlmodel, notifymodel } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { environment } from 'src/environments/environment'
import { ConfirmationService } from 'primeng/api';
import { isNullOrUndefined } from 'util';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-PutawayNotificationView',
  templateUrl: './PutawayNotificationView.component.html',
  animations: [
    trigger('rowExpansionTrigger', [
      state('void', style({
        transform: 'translateX(-10%)',
        opacity: 0
      })),
      state('active', style({
        transform: 'translateX(0)',
        opacity: 1
      })),
      transition('* <=> *', animate('400ms cubic-bezier(0.86, 0, 0.07, 1)'))
    ])
  ],
  providers: [DatePipe , ConfirmationService]
})
export class PutawayNotificationViewComponent implements OnInit {
  binid: any;
  rackid: any;
  isnonpo: boolean = false;
  public url = "";
  constructor(private ConfirmationService: ConfirmationService, private http: HttpClient, private formBuilder: FormBuilder, private messageService: MessageService, private datePipe: DatePipe, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService, @Inject('BASE_URL') baseUrl: string) { this.url = baseUrl; }

  cars: Array<inwardModel> = [];
  rowGroupMetadata: any;

  cols: any[];

  public store: any;
  public rack: any;
  public bin: any;
  public date: Date = null;
  public formName: string;
  public txtName: string;
  public dynamicData = new DynamicSearchResult();
  public showList: boolean = false;
  public searchItems: Array<searchList> = [];
  public selectedlist: Array<searchList> = [];
  public searchresult: Array<object> = [];
  public selectedItem: searchList;
  public PoDetails: PoDetails;
  public podetailsList: Array<inwardModel> = [];
  public employee: Employee;
  public BarcodeModel: BarcodeModel;
  public showDetails; showLocationDialog: boolean = false;
  public StockModel: StockModel;
  public stock: StockModel[] = [];
  public locationdetails = new locataionDetailsStock();
  public StockModelList: Array<any> = [];;
  public StockModelForm: FormGroup;
  public StockModelForm1: FormGroup;
  public rowIndex: number;
  public pono: string;
  public invoiceNo: string;
  public grnNo: string;
  public disSaveBtn: boolean = false;
  checkedgrnlist: inwardModel[] = [];
  uploadedFiles: any[] = [];
  selectedgrn: ddlmodel;
  selectedgrnno: string = "";
  filteredgrns: any[];
  isallplaced: boolean = false;
  showdocuploaddiv: boolean = false;
  issaveprocess: boolean = false;
  isalreadytransferred: boolean = false;
  sendmailtofinance: boolean = false;
  isnotified: boolean = false;
  displayimage: boolean = false;
  docimage: string = "";
  suppliername: string = "";
  notifmodel: notifymodel;
  multinotifmodel: notifymodel[] = [];
  notifremarks: string = "";
  grstatus: string = "";
  issentdata: boolean = false;
  isallselected: boolean = false;
  clspan: number = 8;
  imgurl = environment.imgurl;
  fromdateview: string = "";
  todateview: string = "";
  fromdate: Date;
  todate: Date;
  fromdateforquery: string = "";
  todateforquery: string = "";
 
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    let today = new Date();
    this.fromdate = new Date(today.getFullYear(), today.getMonth(), 1);
    this.todate = today;
    //var sdformat = String(new Date(this.fromdate).getFullYear()) + "-" + String(new Date(this.fromdate).getMonth() + 1) + "-" + String(new Date(this.pgModel.startDate).getDate());
    //var edformat = String(new Date(this.todate).getFullYear()) + "-" + String(new Date(this.todate).getMonth() + 1) + "-" + String(new Date(this.todate).getDate());
    this.fromdateforquery = this.datePipe.transform(new Date(today.getFullYear(), today.getMonth(), 1), 'yyyy-MM-dd');
    this.todateforquery = this.datePipe.transform(new Date(today), 'yyyy-MM-dd');
    this.fromdateview = this.datePipe.transform(new Date(today.getFullYear(), today.getMonth(), 1), 'dd/MM/yyyy');
    this.todateview = this.datePipe.transform(new Date(today), 'dd/MM/yyyy')
    this.issaveprocess = false;
    this.showdocuploaddiv = false;
    this.issentdata = false;
    this.isallselected = false;
    this.uploadedFiles = [];
    this.multinotifmodel = [];
    this.grstatus = "Sent";
    this.PoDetails = new PoDetails();
    this.StockModel = new StockModel();
    this.notifmodel = new notifymodel();
    
    //this.locationListdata1();
    //this.binListdata1();
    //this.rackListdata1();
    this.getnotifiedgrn();
    this.StockModel.shelflife = new Date();
    //this.userForm = this.fb.group({
    //  users: this.fb.array([])
    //});
    this.cols = [
      { field: 'material', header: 'Material' },
      { field: 'materialdescription', header: 'Material Description' },
      { field: 'materialquantity', header: 'Material Quantity' },
      { field: 'receivedquantity', header: 'Received Quantity' },
      { field: 'acceptedquantity', header: 'Accepted Quantity' },
      { field: 'returnedquantity', header: 'Returned Quantity' },
      { field: 'pendingquantity', header: 'Pending Quantity' },
      { field: 'materialbarcode', header: 'Material Barcode' }
    ];



    this.StockModelForm = this.formBuilder.group({
      locatorid: ['', [Validators.required]],
      rackid: ['', [Validators.required]],
      binid: ['', [Validators.required]],
      shelflife: ['', [Validators.required]],
      binnumber: ['', [Validators.required]],
      itemlocation: ['', [Validators.required]]
    });
    this.StockModelForm1 = this.formBuilder.group({
      users: this.formBuilder.array([{
        locatorid: ['', [Validators.required]],
        rackid: ['', [Validators.required]],
        binid: ['', [Validators.required]],
        shelflife: ['', [Validators.required]],
        binnumber: ['', [Validators.required]],
        itemlocation: ['', [Validators.required]],
        quantity: [0, [Validators.required]],
      }])
    });

   
  }

  

  opendoc(file: any) {
    debugger;
    var picstr = file;
    if (picstr.endsWith(".pdf") || picstr.endsWith(".xlsx")) {
      window.open(this.imgurl + picstr, "_blank");
    }
    else {
      this.docimage = this.imgurl + picstr;
      this.displayimage = true;
    }
  }

  showattachdata(rowData: inwardModel) {
    debugger;
    rowData.uploadedFiles = [];
    rowData.showtrdata = !rowData.showtrdata;
    var files = rowData.putawayfilename;
    if (files) {
      if (files.includes("_____")) {
        var filearr = files.split("_____");
        rowData.uploadedFiles = filearr;
      }
      else {
        rowData.uploadedFiles.push(files);
      }
    }
  }

  getpos(pono: any) {
    debugger;
    var pos = pono;
    var returnstr = String(pos.split(',').join(' '));
    return returnstr;
  }
 
  getnotifiedgrn() {
    this.spinner.show();
    this.wmsService.getnotifedGRbydate(this.fromdateforquery, this.todateforquery).subscribe(data => {
      debugger;
      this.checkedgrnlist = data;
      this.checkedgrnlist.forEach(item => {
        item.uploadedFiles = [];
        item.selectedrow = false;
      })
      this.spinner.hide();
    });
  }

  onfromSelectMethod(event) {
    this.checkedgrnlist = [];
    this.fromdateforquery = "";
    if (event.toString().trim() !== '') {
      this.fromdateforquery = this.datePipe.transform(event, 'yyyy-MM-dd');
      this.getnotifiedgrn();
    }
  }
  ontoSelectMethod(event) {
    this.checkedgrnlist = [];
    this.todateforquery = "";
    if (event.toString().trim() !== '') {
      this.todateforquery = this.datePipe.transform(event, 'yyyy-MM-dd');
      this.getnotifiedgrn();

    }
  }


 

 
  
 




 
 


 


}
