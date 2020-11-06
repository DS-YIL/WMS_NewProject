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
  selector: 'app-PutawayNotification',
  templateUrl: './PutawayNotification.component.html',
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
export class PutawayNotificationComponent implements OnInit {
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
  ismultipleselected: boolean = false;
  clspan: number = 8;
  imgurl = environment.imgurl;
 
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.issaveprocess = false;
    this.showdocuploaddiv = false;
    this.issentdata = false;
    this.isallselected = false;
    this.uploadedFiles = [];
    this.multinotifmodel = [];
    this.grstatus = "Pending";
    this.PoDetails = new PoDetails();
    this.StockModel = new StockModel();
    this.notifmodel = new notifymodel();
    //this.locationListdata1();
    //this.binListdata1();
    //this.rackListdata1();
    this.getcheckedgrn();
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

    // this.loadStores();
  }

  
 

 
  getpos(pono: any) {
    debugger;
    var pos = pono;
    var returnstr = String(pos.split(',').join(' '));
    return returnstr;
  }

  //generate barcode -gayathri
  getSlected(event: any) {
    debugger;
    this.ismultipleselected = false;
    var datafilter = this.checkedgrnlist.filter(function (element, index) {
      return (element.selectedrow);
    });
    if (datafilter.length > 0) {
      this.ismultipleselected = true;
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
  selectunselectall(event) {
    if (event.target.checked) {
      this.isallselected = true;
      this.checkedgrnlist.forEach(item => {
        item.selectedrow = true;
      })
    }
    else {
      this.isallselected = false;
      this.checkedgrnlist.forEach(item => {
        item.selectedrow = false;
      })
    }
    this.ismultipleselected = false;
    var datafilter = this.checkedgrnlist.filter(function (element, index) {
      return (element.selectedrow);
    });
    if (datafilter.length > 0) {
      this.ismultipleselected = true;
    }

   
    
  }
  getcheckedgrn() {
    this.notifremarks = "";
    this.spinner.show();
    var type = this.grstatus;
    this.issentdata = false;
    if (type == "Sent") {
      this.issentdata = true;
    }
    this.wmsService.getcheckedgrnlistfornotify(type).subscribe(data => {
      debugger;
      this.checkedgrnlist = data;
      this.checkedgrnlist.forEach(item => {
        item.uploadedFiles = [];
        item.selectedrow = false;
      })
      this.spinner.hide();
    });
  }

  //filtergrn(event) {
  //  this.filteredgrns = [];
  //  for (let i = 0; i < this.checkedgrnlist.length; i++) {
  //    let brand = this.checkedgrnlist[i].supplier;
  //    let pos = this.checkedgrnlist[i].text;
  //    if (brand.toLowerCase().indexOf(event.query.toLowerCase()) == 0 || pos.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
  //      this.filteredgrns.push(pos);
  //    }
  //  }
  //}





 

  onBasicUpload(event) {
    debugger;
    for (let file of event.files) {
      var fname = 'putaway_'+ this.podetailsList[0].grnnumber + "_" + file.name;
      const formData = new FormData();
      formData.append('file', file, fname);
      this.http.post(this.url + 'POData/uploaddoc', formData)
        .subscribe(event => {
           this.messageService.add({ severity: 'success', summary: '', detail: 'File uploaded' });
        });

    }

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
  getspan() {
    if (this.issentdata)
      return 7;
    else
      return 9;
  }

  onUpload(event, rowData: inwardModel) {
    debugger;
    this.spinner.show();
    var flength = event.files.length;
    var filearr = "";
    if (rowData.uploadedFiles.length > 0) {
      filearr = rowData.putawayfilename + "_____";
    }
    var ind = 1;
  
    let ddlmdl = new ddlmodel();
    for (let file of event.files) {
      var timestamp = new Date().getTime().toString();
      var fname = 'putaway_' + timestamp + '_' + rowData.grnnumber + "_" + file.name;
      if (ind > 1 && ind <= flength) {
        filearr += "_____"
      }
      ind++;
      filearr += fname;
      ddlmdl.value = filearr;
      ddlmdl.text = rowData.grnnumber;
      const formData = new FormData();
      formData.append('file', file, fname);
      this.http.post(this.url + 'POData/uploaddoc', formData)
        .subscribe(event => {
          debugger;
         
          this.wmsService.Updateputawayfiles(ddlmdl).subscribe(data => {
          });
         
      });

    }

    var pg = this;
    setTimeout(function () {
      if (flength > 0) {
        pg.messageService.add({ severity: 'success', summary: '', detail: 'File uploaded' });
      }
      else {
        pg.messageService.add({ severity: 'error', summary: '', detail: 'Select File' });
      }
      pg.getcheckedgrn();
      pg.spinner.hide();
    }, 2000);

  }
  Notify(rowData: inwardModel) {
    let notif = new notifymodel();
    notif.grnnumber = rowData.grnnumber;
    notif.notifiedby = this.employee.employeeno;
    notif.notifyremarks = rowData.notifyremarks;
    this.wmsService.notifyputawayfn(notif).subscribe(data => {
      this.messageService.add({ severity: 'success', summary: '', detail: 'Notified.' });
      this.ismultipleselected = false;
      this.getcheckedgrn();
    });
 
  }

  Notifyall() {

    this.multinotifmodel = [];
    var datafilter = this.checkedgrnlist.filter(function (element, index) {
      return (element.selectedrow);
    });
    if (datafilter.length > 0) {
      datafilter.forEach(item => {
        let notif = new notifymodel();
        notif.grnnumber = item.grnnumber;
        notif.notifiedby = this.employee.employeeno;
        notif.notifyremarks = item.notifyremarks;
        this.multinotifmodel.push(notif);
      });

      this.wmsService.notifymultipleputawayfn(this.multinotifmodel).subscribe(data => {
        this.ismultipleselected = false;
        this.messageService.add({ severity: 'success', summary: '', detail: 'Notified.' });
        this.getcheckedgrn();
      });

    }
    else {
      this.messageService.add({ severity: 'success', summary: '', detail: 'Select row to notify.' });
      return;
    }
   
   

  }

  SearchGRNNo() {
    debugger;
    this.podetailsList = [];
    this.PoDetails.grnnumber = "";
    this.isallplaced = false;
    this.isnotified = false;
    this.suppliername = "";
    this.isalreadytransferred = false;
    this.uploadedFiles = [];
    if (isNullOrUndefined(this.selectedgrnno) || this.selectedgrnno == "") {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter GRNNo' });

    }
    var grn = this.selectedgrnno;
    //var datafilter = this.checkedgrnlist.filter(function (element, index) {
    //  return (element.text == grn);
    //});
    //if (datafilter.length > 0) {
    //  this.suppliername = datafilter[0].supplier;
    //}
    this.PoDetails.grnnumber = this.selectedgrnno;
      this.isnonpo = false;
      this.spinner.show();
      this.wmsService.getitemdetailsbygrnnonotif(this.PoDetails.grnnumber).subscribe(data => {
        this.spinner.hide();
        if (data) {
          debugger;
          //this.PoDetails = data[0];
          this.podetailsList = data;
          var files = this.podetailsList[0].putawayfilename;
          if (files) {
            if (files.includes("_____")) {
              var filearr = files.split("_____");
              this.uploadedFiles = filearr;
            }
            else {
              this.uploadedFiles.push(files);
            }
          }
          this.isnotified = this.podetailsList[0].notifiedtofinance;
          this.isalreadytransferred = this.podetailsList[0].isdirecttransferred;
          if (this.isalreadytransferred) {
            this.podetailsList = [];
            this.messageService.add({ severity: 'warn', summary: '', detail: 'Materials already transferred to a project' });
            return;
           
          }
          var allplacedchk = this.podetailsList.filter(function (element, index) {
            return (!element.itemlocation);
          });
          if (allplacedchk.length == 0) {
            this.isallplaced = true;
            if (this.issaveprocess) {
              this.showdocuploaddiv = true;
            }
          }
          this.showDetails = true;
          var ponumber = this.podetailsList[0].pono;
          if (ponumber.startsWith("NP")) {
            this.isnonpo = true;
          }
this.updateRowGroupMetaData();
        }
        else
          this.messageService.add({ severity: 'error', summary: '', detail: 'No data for this GRN No' });
      })
    
  
     
  }

 
  updateRowGroupMetaData() {
    debugger;
    this.rowGroupMetadata = {};
    if (this.podetailsList) {
      var count = 0;
      for (let i = 0; i < this.podetailsList.length; i++) {
        let rowData = this.podetailsList[i];
        let material = rowData.material;
        if (i == 0) {
          this.rowGroupMetadata[material] = { index: 0, size: 1 };
          count = count + 1;
          this.podetailsList[i].serialno = count;
        }
        else {
          let previousRowData = this.podetailsList[i - 1];
          let previousRowGroup = previousRowData.material;
          if (material === previousRowGroup)
            this.rowGroupMetadata[material].size++;
          else {
            this.rowGroupMetadata[material] = { index: i, size: 1 };
            count = count + 1;
            this.podetailsList[i].serialno = count;
          }


        }
      }
    }
  }


 


}
