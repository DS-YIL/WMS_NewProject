import { Component, OnInit, Inject } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList, printMaterial } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { StockModel, locationddl, binddl, rackddl, locataionDetailsStock,ddlmodel, InitialStock, locationdropdownModel } from 'src/app/Models/WMS.Model';
import { MessageService, LazyLoadEvent } from 'primeng/api';
import { FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { ConfirmationService } from 'primeng/api';
import { isNullOrUndefined } from 'util';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-InitialStockPutAway',
  templateUrl: './InitialStockPutAway.component.html',
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
export class InitialStockPutAwayComponent implements OnInit {
  public employee: Employee;
  getalldata: InitialStock[] = [];
  getmainlistdata: InitialStock[] = [];
  getVirtuallistdata: InitialStock[] = [];
  loading: boolean;
  totalRecords: number;
  displaytable: boolean = false;
  locationlist: locationdropdownModel[] = [];
  binlist: locationdropdownModel[] = [];
  binlistbyrack: locationdropdownModel[] = [];
  racklist: locationdropdownModel[] = [];
  racklistbystore: locationdropdownModel[] = [];
  locationlistDG: locationdropdownModel[] = [];
  saveModel: InitialStock;
  matid: string = "";
  matdescription: string = "";
  matqty: string = "";
  showLocationDialog: boolean = false;
  filteredStores: any[];
  filteredracks: any[];
  filteredbins: any[];
  files: ddlmodel[] = [];
  selectedfile: ddlmodel;
  constructor(private ConfirmationService: ConfirmationService, private http: HttpClient, private formBuilder: FormBuilder, private messageService: MessageService, private datePipe: DatePipe, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) {  }

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.loading = true;
    this.getMainlist();
    this.locationListdata();
    this.binListdata();
    this.rackListdata();
    this.files = [];
    this.GetFiles();
    
  }

  GetFiles() {
    this.files = [];
    this.wmsService.getPendingFilesforIS().subscribe(data => {
      debugger;
      this.files = data;
    });

  }

  loadCarsLazy(event: LazyLoadEvent) {
    debugger;
    this.loading = true;
    setTimeout(() => {
      if (this.getmainlistdata) {
        this.getVirtuallistdata = this.getmainlistdata.slice(event.first, (event.first + event.rows));
        this.loading = false;
      }
    }, 1000);
  }
  showDialog(data: InitialStock, ind: number) {
    this.saveModel = data;
    this.matid = data.material;
    this.matdescription = data.materialdescription;
    this.matqty = String(data.quantity);
    let initlocation = new locationdropdownModel();
    initlocation.locatorid = data.defaultstore;
    var loc = this.locationlist.filter(o => o.locatorid == initlocation.locatorid);
    if (loc.length > 0) {
      initlocation.locatorname = loc[0].locatorname;
    }
    initlocation.rackid = data.defaultrack;
    var rac = this.racklist.filter(o => o.rackid == initlocation.rackid);
    if (rac.length > 0) {
      initlocation.racknumber = rac[0].racknumber;
    }
    initlocation.binid = data.defaultbin;
    var bn = this.binlist.filter(o => o.binid == initlocation.binid);
    if (bn.length > 0) {
      initlocation.binnumber = bn[0].binnumber;
    }
    initlocation.stocktype = "Project Stock";
    initlocation.quantity = 0;
    initlocation.isdisablestore = false;
    initlocation.isdisablerack = false;
    initlocation.isdisablebin = false;
    initlocation.invalidlocation = false;
    this.locationlistDG.push(initlocation);
    this.showLocationDialog = true;

  }

  formvalidation(issubmit: boolean) {
    var location = this.locationlistDG.filter(function (element, index) {
      return (isNullOrUndefined(element.locatorid) || element.locatorid == 0);
    });
    if (location.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Store.' });
      return false;
    }
    var racklocation = this.locationlistDG.filter(function (element, index) {
      return (isNullOrUndefined(element.rackid) || element.rackid == 0);
    });
    if (racklocation.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Rack.' });
      return false;
    }
    var stype = this.locationlistDG.filter(function (element, index) {
      return (isNullOrUndefined(element.stocktype) || element.stocktype.trim() == "");
    });
    var qtysum = 0;
    var loclist = [];
    var isduplicatefound = false;
    this.locationlistDG.forEach(function (item, index) {
      qtysum = qtysum + item.quantity;
      var locstr = item.locatorname.trim() + "." + item.racknumber.trim();
      if (!isNullOrUndefined(item.binnumber) && item.binnumber != "") {
        locstr += "." + item.binnumber
      }
      if (loclist.includes(locstr)) {

        item.invalidlocation = true;
        isduplicatefound = true;

      }
      else {
        loclist.push(locstr);
      }

    });

    if (isduplicatefound) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Duplicate location found.' });
      return false;
    }

    if (stype.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Stock Type.' });
      return false;
    }
    var qty = this.locationlistDG.filter(function (element, index) {
      return (isNullOrUndefined(element.quantity) || element.quantity == 0);
    });
    if (qty.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Quantity.' });
      return false;
    }

    if (issubmit) {
      if (qtysum != parseFloat(this.matqty)) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Put away Quantity must be equal to Stock Quantity.' });
        return false;
      }
    }
    else {
      if (qtysum > parseFloat(this.matqty)) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Put away Quantity must be equal to Stock Quantity.' });
        return false;
      }

    }

   

    return true;

  }

  Checkqty(data: locationdropdownModel) {

    var qtysum = 0;
    var loclist = [];
    var isduplicatefound = false;
    this.locationlistDG.forEach(function (item, index) {
      qtysum = qtysum + item.quantity;
      var locstr = item.locatorname.trim() + "." + item.racknumber.trim();
      if (!isNullOrUndefined(item.binnumber) && item.binnumber != "") {
        locstr += "." + item.binnumber
      }
      if (loclist.includes(locstr)) {

        item.invalidlocation = true;
        isduplicatefound = true;

      }
      else {
        loclist.push(locstr);
      }

    });

    if (isduplicatefound) {
      data.quantity = 0;
      this.messageService.add({ severity: 'error', summary: '', detail: 'Duplicate location found.' });
      return false;
    }


    if (qtysum > parseFloat(this.matqty)) {
      data.quantity = 0;
      this.messageService.add({ severity: 'error', summary: '', detail: 'Put away Quantity must be equal to Stock Quantity.' });
      return false;
    }


  }

  OnStocktypeSelect(datax: locationdropdownModel) {
    
    var loclist = [];
    var isduplicatefound = false;
    this.locationlistDG.forEach(function (item, index) {
      var locstr = item.locatorname.trim() + "." + item.racknumber.trim();
      if (!isNullOrUndefined(item.binnumber) && item.binnumber != "") {
        locstr += "." + item.binnumber
      }
      if (loclist.includes(locstr)) {

        item.invalidlocation = true;
        isduplicatefound = true;

      }
      else {
        loclist.push(locstr);
      }

    });

    if (isduplicatefound) {
      datax.quantity = 0;
      this.messageService.add({ severity: 'error', summary: '', detail: 'Duplicate location found.' });
      return false;
    }

  }

  addNewRow() {

    var dt = this.formvalidation(false);

    if (dt == true) {
      let initlocation = new locationdropdownModel();
      initlocation.locatorid = null;
      initlocation.rackid = null;
      initlocation.binid = null;
      initlocation.stocktype = "";
      initlocation.quantity = 0;
      initlocation.isdisablestore = false;
      initlocation.isdisablerack = true;
      initlocation.isdisablebin = true;
      initlocation.invalidlocation = false;
      this.locationlistDG.push(initlocation);

    }
    
   

  }
  deleteRow(index : number) {
    this.locationlistDG.splice(index, 1);
  }

  close() {
    this.matid = "";
    this.matdescription = "";
    this.matqty = "";
    this.showLocationDialog = false;
    this.locationlistDG = [];
    this.saveModel = new InitialStock();

  }

  locationListdata() {
    this.wmsService.getlocationdata().
      subscribe(
        res => {
          this.locationlist = res;
        });
  }
  binListdata() {
    debugger;
    this.wmsService.getbindataforputaway().
      subscribe(
        res => {
          this.binlist = res;
        });
  }
  rackListdata() {
    debugger;
    this.wmsService.getrackdataforputaway().
      subscribe(
        res => {
          this.racklist = res;
        });
  }

  onStoreSelect(event: any,data: locationdropdownModel, index: number) {

    debugger;

    var loc = this.locationlist.filter(o => o.locatorname == data.locatorname);
    if (loc.length > 0) {
      data.locatorname = loc[0].locatorname;
      data.locatorid = loc[0].locatorid;
    }
    else {
      data.locatorname = "";
      data.locatorid = null;
    }
    data.rackid = null;
    data.racknumber = "";
    data.binid = null;
    data.binnumber ="";
    data.isdisablestore = false;
    data.invalidlocation = false;
    data.isdisablerack = false;
    data.isdisablebin = true;

  }
  filterStore(event: any, data: InitialStock) {
    this.filteredStores = [];

    for (let i = 0; i < this.locationlist.length; i++) {
      let lid = String(this.locationlist[i].locatorid);
      let lname = this.locationlist[i].locatorname;
      if (lid.toLowerCase().indexOf(event.query.toLowerCase()) == 0 || lname.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredStores.push(lname);
      }

    }

  }

  onRackSelect(event: any,data: locationdropdownModel, index: number) {
    var loc = this.racklist.filter(o => o.racknumber == data.racknumber && o.locatorid == data.locatorid);
    if (loc.length > 0) {
      data.racknumber = loc[0].racknumber;
      data.rackid = loc[0].rackid;
    }
    else {
      data.racknumber = "";
      data.rackid = null;
    }
    data.binid = null;
    data.binnumber = "";
    data.isdisablestore = false;
    data.invalidlocation = false;
    data.isdisablerack = false;
    data.isdisablebin = false;

  }
  filterRack(event: any, data: locationdropdownModel) {
    debugger;
    let senddata = [];
    if (!isNullOrUndefined(data.locatorid) && data.locatorid != 0) {
        senddata = this.racklist.filter(function (element, index) {
        return (element.locatorid == data.locatorid);
      });
    }
    else {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Store.' });
      return;
    }
    this.filteredracks = [];
    for (let i = 0; i < senddata.length; i++) {
      let lid = String(senddata[i].rackid);
      let lname = senddata[i].racknumber;
      if (lid.toLowerCase().indexOf(event.query.toLowerCase()) == 0 || lname.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredracks.push(lname);
      }

    }
  

  }
  onBinSelect(event: any,data: locationdropdownModel, index: number) {
    var loc = this.binlist.filter(o => o.binnumber == data.binnumber && o.rackid == data.rackid && o.locatorid == data.locatorid);
    if (loc.length > 0) {
      data.binnumber = loc[0].binnumber;
      data.binid = loc[0].binid;

    }
    else {
      data.binnumber = "";
      data.binid = null;
    }
    data.invalidlocation = false;
   
  }
  filterBin(event: any, data: locationdropdownModel) {
    let senddata = [];
    if (!isNullOrUndefined(data.rackid) && data.rackid != 0 && !isNullOrUndefined(data.locatorid) && data.locatorid != 0) {
      senddata = this.binlist.filter(function (element, index) {
        return (element.locatorid == data.locatorid && element.rackid == data.rackid);
      });
    }
    else {
 
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Store/Rack.' });
      return;
    }
    this.filteredbins = [];
    for (let i = 0; i < senddata.length; i++) {
      let lid = String(senddata[i].binid);
      let lname = senddata[i].binnumber;
      if (lid.toLowerCase().indexOf(event.query.toLowerCase()) == 0 || lname.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredbins.push(lname);
      }

    }

  }
 
  getMainlist() {
    this.getmainlistdata = [];
    this.spinner.show();
    this.displaytable = false;
    this.wmsService.getinitialStockMaterialForPutaway().subscribe(data => {
      this.spinner.hide();
      this.getalldata = data;
      this.getmainlistdata = data;
      this.totalRecords = this.getmainlistdata.length;
      this.displaytable = true;
      
    });
  }
  filterdata(event: any) {
    this.getmainlistdata = [];
    debugger;
    var filename = this.selectedfile.text;
    this.displaytable = false;
    if (filename != "ALL") {
      
      setTimeout(() => {
        this.getmainlistdata = this.getalldata.filter(o => o.uploadedfilename == filename);
        this.displaytable = true;
      }, 1500);
      
    }
    else {
      setTimeout(() => {
        this.getmainlistdata = this.getalldata;
        this.displaytable = true;
      }, 1500);
      
    }
   
  }

  seterrorcolour() {

  }

  onSubmitStockDetails() {
    var dt = this.formvalidation(true);
    this.saveModel.locations = this.locationlistDG;
    this.saveModel.createdby = this.employee.employeeno;
    this.spinner.show();
    if (dt == true) {
      this.wmsService.PutawayFromInitialStock(this.saveModel).subscribe(data => {
        this.spinner.hide();
        if (String(data).trim() == "Location Updated") {
          this.messageService.add({ severity: 'success', summary: '', detail: 'Put away successful' });
        }
        else {
          this.messageService.add({ severity: 'error', summary: '', detail: String(data).trim() });
        }

        this.showLocationDialog = false;
        this.getMainlist();

      });
    }
    else {
      this.spinner.hide();
    }

  }

}
