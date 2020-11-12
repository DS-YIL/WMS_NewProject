import { Component, OnInit, Inject } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList, printMaterial } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { PoDetails, BarcodeModel, StockModel, inwardModel, locationddl, binddl, rackddl, locataionDetailsStock,ddlmodel } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { ConfirmationService } from 'primeng/api';
import { isNullOrUndefined } from 'util';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-Warehouse',
  templateUrl: './WarehouseIncharge.component.html',
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
export class WarehouseInchargeComponent implements OnInit {
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
  public StockModelList: Array<any> = [];
  public StockModelForm: FormGroup;
  public StockModelForm1: FormGroup;
  public rowIndex: number;
  public locationlist: any[] = [];
  public binlist: any[] = [];
  public racklist: any[] = [];
  public locationdata: any[] = [];
  public bindata: any[] = [];
  public rackdata: any[] = [];
  locationlist1: locationddl[] = [];
  binlist1: binddl[] = [];
  racklist1: rackddl[] = [];
  public row: any = 0;
  public showPrintDialog: boolean = false;
  public qty: any = 1;
  public noOfPrint: any = 1;
  public materialCode: any;
  public receivedDate: any;
  public acceptedQty: any;
  public itemNo: any;
  public printData = new printMaterial();
  public showPrintLabel: boolean = false;
  public pono: string;
  public invoiceNo: string;
  public grnNo: string;
  matid: string = "";
  matdescription: string = "";
  matqty: string = "";
  public invoiceForm: FormGroup;
  public showLocationDialogxx: boolean = false;
  public disSaveBtn: boolean = false;
  checkedgrnlist: ddlmodel[] = [];
  selectedgrn: ddlmodel;
  selectedgrnno: string = "";
  filteredgrns: any[];
  isallplaced: boolean = false;
  showdocuploaddiv: boolean = false;
  issaveprocess: boolean = false;
  isalreadytransferred: boolean = false;
  sendmailtofinance: boolean = false;
  currentstocktype: string = "";
 
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.issaveprocess = false;
    this.showdocuploaddiv = false;
    this.invoiceForm = this.formBuilder.group({
      itemRows: this.formBuilder.array([this.initItemRows()])
    });
    this.PoDetails = new PoDetails();
    this.StockModel = new StockModel();
    this.locationListdata();
    this.binListdata();
    this.rackListdata();
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

  //On selection of location updating rack
  onlocUpdate(locationid: any, rowData: any, issetdefault: boolean) {
    debugger;
    rowData.racklist = [];
    if (this.rackdata.filter(li => li.locationid == locationid).length > 0) {
      this.racklist = [];
      console.log(this.racklist);
      this.rackdata.forEach(item => {
        if (item.locationid == locationid) {
          rowData.racklist.push(item);
          console.log(this.racklist);
        }
      })
      
    }
    
  }

  //On selection of rack updating bin
  onrackUpdate(locationid: any, rackid: any, rowData: any, issetdefault: boolean) {
    debugger;
    rowData.binlist = [];
    if (this.bindata.filter(li => li.locationid == locationid && li.rackid == rackid).length > 0) {
      this.binlist = [];
      console.log(this.binlist);
      this.bindata.forEach(item => {
        if (item.locationid == locationid && item.rackid == rackid) {
          rowData.binlist.push(item);
          console.log(this.binlist);
        }
      })

    }

  }

  //generate barcode -gayathri
  generateBarcode(details: any) {
    this.showPrintDialog = true;
    this.materialCode = details.material;
    this.receivedDate = this.datePipe.transform(details.receiveddate, this.constants.dateFormat);
    this.acceptedQty = details.confirmqty;
    this.pono = details.pono;
    this.invoiceNo = details.invoiceno;
    this.grnNo = details.grnnumber;
  }
  get formArr() {
    return this.invoiceForm.get('itemRows') as FormArray;
    // return (this.invoiceForm.get('itemRows') as FormArray).controls;
  }

  shownotifdiv() {
    this.showdocuploaddiv = !this.showdocuploaddiv;
  }

  initItemRows() {
    return this.formBuilder.group({
      itemname: [''],
      locatorid: ['', [Validators.required]],
      rackid: ['', [Validators.required]],
      binid: ['', [Validators.required]],
      shelflife: ['', [Validators.required]],
      binnumber: ['', [Validators.required]],
      itemlocation: ['', [Validators.required]],
      stocktype: ['', [Validators.required]],
      quantity: [0, [Validators.required]],
    });
  }

  addNewRow() {
    //this.onQtyClick();
    if (this.stock.length > 0) {
      if (this.stock[this.stock.length - 1].locatorid != 0 || this.stock[this.stock.length - 1].qty != 0 || this.stock[this.stock.length - 1].qty != null) {
        if (this.stock[this.stock.length - 1].locatorid == 0 || isNullOrUndefined(this.stock[this.stock.length - 1].locatorid)) {
          this.messageService.add({ severity: 'error', summary: '', detail: 'select Location' });
          return;
        }
        if (this.stock[this.stock.length - 1].rackid == 0 || isNullOrUndefined(this.stock[this.stock.length - 1].rackid)) {
          this.messageService.add({ severity: 'error', summary: '', detail: 'select Rack' });
          return;
        }
        if (this.stock[this.stock.length - 1].qty == 0 || this.stock[this.stock.length - 1].qty == null) {
          this.messageService.add({ severity: 'error', summary: '', detail: 'Enter quantity' });
          return;
        }
        if (this.stock.length > 1) {
          for (var data = 0; data < this.stock.length - 1; data++) {
            if (this.stock[data].locatorid == this.stock[this.stock.length - 1].locatorid) {
              if (this.stock[data].rackid == this.stock[this.stock.length - 1].rackid) {
                if (this.stock[data].binid == this.stock[this.stock.length - 1].binid) {
                  this.messageService.add({ severity: 'error', summary: '', detail: 'Location already exists' });
                  this.stock[this.stock.length - 1].binid = 0;
                  this.stock[this.stock.length - 1].rackid=0;
                  this.stock[this.stock.length - 1].locatorid = 0;
                  return;
                }
              }
            }
          }
        }
        var stockdata = new StockModel();
        stockdata.locationlists = this.locationlist;
        stockdata.locatorid = null;
        stockdata.rackid = null;
        stockdata.binid = null;
        stockdata.stocktype = this.currentstocktype;
        this.stock.push(stockdata);
        //this.stock.push(new StockModel());
      }

    }
    else {
      var stockdata = new StockModel();
      stockdata.locationlists = this.locationlist;
      stockdata.locatorid = null;
      stockdata.rackid = null;
      stockdata.binid = null;
      stockdata.stocktype = this.currentstocktype;
      this.stock.push(stockdata);
      //this.stock.push(new StockModel());
    }
  }

  onQtyClick(index:any) {
    debugger;
    if (this.stock.length > 1) {
      for (var data = 0; data < this.stock.length - 1; data++) {
        if (this.stock[data].locatorid == this.stock[this.stock.length - 1].locatorid) {
          if (this.stock[data].rackid == this.stock[this.stock.length - 1].rackid) {
            if (this.stock[data].binid == this.stock[this.stock.length - 1].binid) {
              this.messageService.add({ severity: 'error', summary: '', detail: 'Location already exists' });
              this.stock[this.stock.length - 1].binid = 0;
              this.stock[this.stock.length - 1].rackid = 0;
              this.stock[this.stock.length - 1].locatorid = 0;
              return;
            }
          }
        }
      }




      //if (this.stock.filter(li => li.locatorid == this.stock[index].locatorid && li.rackid == this.stock[index].rackid && li.binid == this.stock[index].binid).length > 0) {
      //  this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Location already exists' });
      //  return;
      //}
    }
      //get stock type
      this.locationdetails.storeid = this.stock[index].locatorid;
      this.locationdetails.rackid = this.stock[index].rackid;
    this.locationdetails.binid = this.stock[index].binid;
    var bindetails = this.bindata.filter(x => x.binid == this.locationdetails.binid);
    var storedetails = this.locationdata.filter(x => x.locatorid == this.locationdetails.storeid);
    var rackdetails = this.rackdata.filter(x => x.rackid == this.locationdetails.rackid);
    this.locationdetails.storename = storedetails[0].locatorname != null || storedetails[0].locatorname != "undefined" || storedetails[0].locatorname != "" ? storedetails[0].locatorname:0;
    this.locationdetails.rackname = rackdetails[0].racknumber != null || rackdetails[0].racknumber != "undefined" || rackdetails[0].racknumber != "" ? rackdetails[0].racknumber : 0;
    this.locationdetails.binname = bindetails[0].binnumber != null || bindetails[0].binnumber != "undefined" || bindetails[0].binnumber != "" ? bindetails[0].binnumber : 0;
      this.locationdetails.locationid = this.locationdetails.storeid + '.' + this.locationdetails.rackid + '.' + this.locationdetails.binid;
      this.locationdetails.locationname = this.locationdetails.storename + '.' + this.locationdetails.rackname + '.' + this.locationdetails.binname;
      //service to get stock type
      //this.wmsService.getstocktype(this.locationdetails).subscribe(data => {
      //  debugger;
      //  if (data) {
      //    this.stock[index].stocktype = data;
      //    //this.invoiceForm.controls.itemRows.value[this.invoiceForm.controls.itemRows.value.length - 1].stocktype = data;
      //    //this.StockModel.stocktype = data;

      //  }
      //  else {
      //    this.messageService.add({ severity: 'error', summary: '', detail: 'Unable to fetch stock type' });
      //  }
      //});


  }

  deleteRow(index: number) {
    this.stock.splice(index,1);
    //this.formArr.removeAt(index);
  }
  //Increase and decrease the qty based on no of prints -- gayathri
  decreaseQty() {
    if (this.qty > 1) {
      this.qty = this.qty - 1;
    }
  }
  increaseQty() {
    if (this.qty < this.noOfPrint) {
      this.qty = this.qty + 1;
    }
  }

  getControls() {
    return (this.StockModelForm1.get("users") as FormArray).controls;
  }
  getcheckedgrn() {
    this.spinner.show();
    this.wmsService.getcheckedgrnlistforputaway().subscribe(data => {
      debugger;
      this.checkedgrnlist = data;
      this.spinner.hide();
    });
  }

  filtergrn(event) {
    this.filteredgrns = [];
    for (let i = 0; i < this.checkedgrnlist.length; i++) {
      let brand = this.checkedgrnlist[i].supplier;
      let pos = this.checkedgrnlist[i].text;
      if (brand.toLowerCase().indexOf(event.query.toLowerCase()) == 0 || pos.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredgrns.push(pos);
      }
    }
  }


 locationListdata() {
    this.wmsService.getlocationdata().
      subscribe(
        res => {
          //this._list = res; //save posts in array
          this.locationlist = res;
          let _list: any[] = [];
          for (let i = 0; i < (res.length); i++) {
            _list.push({
              locatorid: res[i].locatorid,
              locatorname: res[i].locatorname,
            });
          }
          this.locationlist = _list;
          this.locationdata = _list;
        });
  }
  binListdata() {
    debugger;
    this.wmsService.getbindataforputaway().
      subscribe(
        res => {
          //this._list = res; //save posts in array
          this.binlist = res;
          let _list: any[] = [];
          for (let i = 0; i < (res.length); i++) {
            _list.push({
              locationid: res[i].locatorid,
              binid: res[i].binid,
              rackid: res[i].rackid,
              binnumber: res[i].binnumber
            });
          }
          this.binlist = _list;
          this.bindata = _list;
        });
  }
  rackListdata() {
    debugger;
    this.wmsService.getrackdataforputaway().
      subscribe(
        res => {
          //this._list = res; //save posts in array
          this.racklist = res;
          let _list: any[] = [];
          for (let i = 0; i < (res.length); i++) {
            _list.push({
              binid: res[i].binid,
              rackid: res[i].rackid,
              racknumber: res[i].racknumber,
              locationid: res[i].locatorid
            });
          }
          this.racklist = _list;
          this.rackdata = _list;
        });
  }

  public bindSearchListData(e: any, formName?: string, name?: string, searchTxt?: string, callback?: () => any): void {
    this.formName = formName;
    this.txtName = name;
    if (searchTxt == undefined)
      searchTxt = "";
    searchTxt = searchTxt.replace('*', '%');
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.tableName = this.constants[name].tableName;
    this.dynamicData.searchCondition = "" + this.constants[name].condition + this.constants[name].fieldName + " like '" + searchTxt + "%'";
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      //if (data.length == 0)
      //this.showList = false;
      //else
      //this.showList = true;
      this.searchresult = data;
      this.searchItems = [];
      var fName = "";
      this.searchresult.forEach(item => {
        fName = item[this.constants[name].fieldName];
        var value = { listName: name, name: fName, code: item[this.constants[name].fieldId] };
        this.searchItems.push(value);
      });

      if (callback)
        callback();
    });
  }
  //search list option changes event
  public onSelectedOptionsChange(item: any) {
    this.showList = false;
    if (this.formName != "") {
      this[this.formName].controls[this.txtName].setValue(item.name);
      this.StockModel[this.txtName] = item.code;
    }
    this[this.formName].controls[this.txtName].updateValueAndValidity();

  }

  //clear model when search text is empty
  onsrchTxtChange(modelparm: string, value: string, model: string) {
    if (value == "") {
      this[model][modelparm] = "";
    }
  }

  GenerateBarcode() {
    this.showPrintDialog = false;
    this.showPrintLabel = true;
    this.printData.materialid = this.materialCode;
    this.printData.invoiceno = this.podetailsList[0].invoiceno;
    this.printData.grnno = this.podetailsList[0].grnnumber;
    this.printData.pono = this.podetailsList[0].pono;
    this.printData.noofprint = this.noOfPrint;
    this.printData.receiveddate = this.receivedDate;

    //api call
    this.wmsService.generateBarcodeMaterial(this.printData).subscribe(data => {
      if (data) {

        this.printData = data;
        console.log(this.printData);

      }
      else {
        alert("Error while generating Barcode");
      }
    })
  }

 

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

  SearchGRNNo() {
    debugger;
    this.podetailsList = [];
    this.PoDetails.grnnumber = "";
    if (isNullOrUndefined(this.selectedgrnno) || this.selectedgrnno == "") {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter GRNNo' });

    }
    this.PoDetails.grnnumber = this.selectedgrnno;
      this.isnonpo = false;
      this.spinner.show();
      this.wmsService.getitemdetailsbygrnno(this.PoDetails.grnnumber).subscribe(data => {
        this.spinner.hide();
        if (data) {
          debugger;
          //this.PoDetails = data[0];
          this.podetailsList = data;
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
        let inwardidview = rowData.inwardidview;
        if (i == 0) {
          this.rowGroupMetadata[inwardidview] = { index: 0, size: 1 };
          count = count + 1;
          this.podetailsList[i].serialno = count;
        }
        else {
          let previousRowData = this.podetailsList[i - 1];
          let previousRowGroup = previousRowData.inwardidview;
          if (inwardidview === previousRowGroup)
            this.rowGroupMetadata[inwardidview].size++;
          else {
            this.rowGroupMetadata[inwardidview] = { index: i, size: 1 };
            count = count + 1;
            this.podetailsList[i].serialno = count;
          }


        }
      }
    }
  }
  sendmailtofinancefn() {

  }

  showDialog1(details: any, index: number) {
   
    this.showLocationDialog = true;
    this.PoDetails = details;
    this.rowIndex = index;
    this.binid = details.binid;
    this.rackid = details.rackid;
    //this.StockModelForm = this.formBuilder.group({
    //  rackid: [details.rackid],
    //  binid: [details.binid],
    //  locatorid: [details.storeid]
    //});
    this.locationListdata();
    this.binListdata();
    this.rackListdata();
    this.rack = "";
    this.bin = "";
    this.store = "";
  }


  close() {
    //alert(this.stock.length);
    this.stock = [];
    //this.locationlist = [];
    //this.binlist = [];
    //this.racklist = [];
    //alert(this.stock.length);
  }

  showDialog(details: any, index: number) {
    debugger;
    this.currentstocktype = details.stocktype;
    this.showLocationDialog = true;
    this.PoDetails = details;
    this.rowIndex = index;
    this.binid = details.binid;
    this.rackid = details.rackid;
    this.matid = details.material;
    
    //this.invoiceForm.controls.itemRows.value[this.invoiceForm.controls.itemRows.value.length - 1].storeid = details.storeid;
    //this.invoiceForm.controls.itemRows.value[this.invoiceForm.controls.itemRows.value.length - 1].rackid = details.rackid;
    //this.invoiceForm.controls.itemRows.value[this.invoiceForm.controls.itemRows.value.length - 1].binid = details.binid;
    //this.invoiceForm.controls.itemRows.value[this.invoiceForm.controls.itemRows.value.length - 1].stocktype = "Plantstock";
    this.StockModel.locatorid = details.storeid;
    this.StockModel.rackid = details.rackid;
    this.StockModel.binid = details.binid;
    this.matdescription = details.materialdescription;
    this.matqty = details.confirmqty;
   
    
    this.StockModelForm = this.formBuilder.group({
      rackid: [details.rackid],
      binid: [details.binid],
      locatorid: [details.storeid]
    });

   

    if (this.stock.length == 0) {
      var stockdata = new StockModel();
      stockdata.locationlists = this.locationlist;
      stockdata.locatorid = details.storeid;
      stockdata.rackid = details.rackid;
      stockdata.binid = details.binid;
      stockdata.stocktype = details.stocktype;
      this.stock.push(stockdata);
      if (stockdata.locatorid) {
        this.onlocUpdate(stockdata.locatorid, stockdata, true);
      }
      if (stockdata.locatorid && stockdata.rackid) {
        this.onrackUpdate(stockdata.locatorid, stockdata.rackid, stockdata, true);
      }
    
      
     
    }
   
  
    this.rack = "";
    this.bin = "";
    this.store = "";
   
  
  }

  addrows() {
    const control = <FormArray>this.StockModelForm1.get("users");
    control.push(this.newrow());

  }
  newrow(): FormGroup {
    debugger;
    return this.formBuilder.group({
      locatorid: ['', [Validators.required]],
      rackid: ['', [Validators.required]],
      binid: ['', [Validators.required]],
      shelflife: ['', [Validators.required]],
      binnumber: ['', [Validators.required]],
      itemlocation: ['', [Validators.required]],
      quantity: [0, [Validators.required]],

    });

  }

  onSubmitStockDetails() {
    //this.onQtyClick();
    
    if (this.stock.length > 0) {
      debugger;
      if (this.stock[this.stock.length - 1].locatorid == 0 || this.stock[this.stock.length - 1].locatorid == null) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'select Location' });
        return;
      }
      if (this.stock[this.stock.length - 1].rackid == 0 || this.stock[this.stock.length - 1].rackid == 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'select Rack' });
        return;
      }
      if (this.stock[this.stock.length - 1].qty == 0 || this.stock[this.stock.length - 1].qty == null) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Enter quantity' });
        return;
      }

      if (this.stock.length > 1) {
        for (var data = 0; data < this.stock.length - 1; data++) {
          if (this.stock[data].locatorid == this.stock[this.stock.length - 1].locatorid) {
            if (this.stock[data].rackid == this.stock[this.stock.length - 1].rackid) {
              if (this.stock[data].binid == this.stock[this.stock.length - 1].binid) {
                this.messageService.add({ severity: 'error', summary: '', detail: 'Location already exists' });
                this.stock[this.stock.length - 1].binid = 0;
                this.stock[this.stock.length - 1].rackid = 0;
                this.stock[this.stock.length - 1].locatorid = 0;
                return;
              }
            }
          }
        }
      }
    }
    this.StockModelList = [];
    var binnumber: any[] = [];
    var storelocation: any[] = [];
    var rack: any[] = [];
    if (this.stock[0].rackid && this.stock[0].locatorid) {
      this.stock.forEach(item => {
        this.StockModel = new StockModel();
        this.StockModel.material = this.PoDetails.material;
        this.StockModel.itemid = this.PoDetails.itemid;
        this.StockModel.pono = this.PoDetails.pono;
        this.StockModel.grnnumber = this.PoDetails.grnnumber;
        this.StockModel.vendorid = this.PoDetails.vendorid;
        this.StockModel.paitemid = this.PoDetails.paitemid;
        this.StockModel.totalquantity = this.PoDetails.materialqty;
        this.StockModel.createdby = this.employee.employeeno;
        this.StockModel.inwardid = this.PoDetails.inwardid;
        this.StockModel.stocktype = item.stocktype;

        binnumber = this.bindata.filter(x => x.binid == item.binid);
        storelocation = this.locationdata.filter(x => x.locatorid == item.locatorid);
        rack = this.rackdata.filter(x => x.rackid == item.rackid);
        if (binnumber.length != 0) {
          this.StockModel.binnumber = binnumber[0].binnumber;
          this.StockModel.binid = binnumber[0].binid;
          this.StockModel.itemlocation = storelocation[0].locatorname + "." + rack[0].racknumber + '.' + binnumber[0].binnumber;
        }
        else if (binnumber.length == 0) {
          this.StockModel.binid = 1;
          this.StockModel.itemlocation = storelocation[0].locatorname + "." + rack[0].racknumber;
        }
        this.StockModel.racknumber = storelocation[0].locatorname;
        this.StockModel.rackid = rack[0].rackid;
        this.StockModel.storeid = storelocation[0].locatorid;
        this.StockModel.confirmqty = item.qty;
        this.StockModel.itemreceivedfrom = new Date();
        this.StockModelList.push(this.StockModel);

        //this.StockModel.stocklist.push(this.StockModel);
      })

      //this.StockModel.stocklist.push(this.StockModel);
      //  binnumber = this.binlist.filter(x => x.binid == this.StockModelForm.controls.binid.value);
      //storelocation = this.locationlist.filter(x => x.locatorid == this.StockModelForm.controls.locatorid.value);
      //  rack = this.racklist.filter(x => x.rackid == this.StockModelForm.controls.rackid.value);
      //if (binnumber.length != 0) {
      //  this.StockModel.binnumber = binnumber[0].binnumber;
      //  this.StockModel.binid = binnumber[0].binid;
      //  this.StockModel.itemlocation = storelocation[0].locatorname + "." + rack[0].racknumber + '.' + binnumber[0].binnumber;
      //}
      //else if (binnumber.length == 0) {
      //  this.StockModel.binid = 1;
      //  this.StockModel.itemlocation = storelocation[0].locatorname + "." + rack[0].racknumber;
      //}
      var totalqty = 0;
      this.StockModelList.forEach(item => {

        totalqty = totalqty + (item.confirmqty);
      })
      debugger;

      var dtt = this.StockModelList.filter(function (element, index) {
        return (element.stocktype == "");
      });
      if (dtt.length > 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Select Stock Type.' });
        return;
      }

      if (totalqty == parseInt(this.matqty)) {
        this.ConfirmationService.confirm({
          message: 'Are you sure to put away material in selected stock type?',
          accept: () => {
            this.disSaveBtn = true;
            this.spinner.show();
            this.wmsService.InsertStock(this.StockModelList).subscribe(data => {
              debugger;
              this.spinner.hide();
              //this.podetailsList[this.rowIndex].itemlocation = this.StockModel.itemlocation;
              this.issaveprocess = true;
              this.showLocationDialog = false;
              this.messageService.add({ severity: 'success', summary: '', detail: 'Location Updated' });
              this.stock = [];
              //this.PoDetails = null;
              this.getcheckedgrn();
              this.SearchGRNNo();
              // }
            });
          },
          reject: () => {

            this.messageService.add({ severity: 'info', summary: '', detail: 'Cancelled' });
          }
        });


        //var result = this.alertconfirm();
     
        //if (result == "success") {
        //  this.disSaveBtn = true;
        //  this.wmsService.InsertStock(this.StockModelList).subscribe(data => {
        //    debugger;
        //    //this.podetailsList[this.rowIndex].itemlocation = this.StockModel.itemlocation;
        //    this.showLocationDialog = false;
        //    this.messageService.add({ severity: 'success', summary: 'Success Message', detail: 'Location Updated' });
        //    this.SearchGRNNo();
        //    // }
        //  });
        //}
        

      }
      else {
        this.disSaveBtn = true;
        this.messageService.add({ severity: 'error', summary: '', detail: 'Putaway Qty should be same as Accepted Qty' });
      }
    }
    else {
      if (!this.store.name)
        this.messageService.add({ severity: 'error', summary: '', detail: 'Select Location' });
      else if (!this.rack.name)
        this.messageService.add({ severity: 'error', summary: '', detail: 'Select Rack' });
    }
  }

  //show alert to update location details
  //show alert about oldest item location
 alertconfirm() {
    
    this.ConfirmationService.confirm({
      message: 'Are you sure to put away material in selected stock type?',
      header: 'Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        return "success";
        //this.messageService.add({ severity: 'info', summary: 'Submit', detail: 'You ha' });
      },
      reject: () => {

        this.messageService.add({ severity: 'info', summary: '', detail: 'Cancelled' });
      }
    });
   //return "error";
  }


  dialogCancel(dialogName) {
    this[dialogName] = false;
  }

  locationListdata1() {
    this.wmsService.getlocationdata().
      subscribe(
        res => {
          //this._list = res; //save posts in array
          this.locationlist1 = res;
        });
  }
  binListdata1() {
    this.wmsService.getbindataforputaway().
      subscribe(
        res => {
          //this._list = res; //save posts in array
          this.binlist1 = res;
        });
  }
  rackListdata1() {
    this.wmsService.getrackdataforputaway().
      subscribe(
        res => {
          //this._list = res; //save posts in array
          this.racklist1 = res;
        });
  }


}
