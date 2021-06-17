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
import { testcrud, WMSHttpResponse, MaterialinHand, matlocations, inventoryFilters, locationdropdownModel } from '../Models/WMS.Model';
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
  locationlist: locationdropdownModel[] = [];
  binlist: locationdropdownModel[] = [];
  binlistbyrack: locationdropdownModel[] = [];
  racklist: locationdropdownModel[] = [];
  racklistbystore: locationdropdownModel[] = [];
  locationlistDG: locationdropdownModel[] = [];
  filteredStores: any[];
  filteredracks: any[];
  filteredbins: any[];
  tempmatloc: matlocations;
  tempdescription: string;
  temprowdata: MaterialinHand;
  lblproject: string = "";
  lblpono: string = "";

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.response = new WMSHttpResponse();
    this.lblproject = "";
    this.lblpono = "";
    this.inventoryFilters = new inventoryFilters();
    this.tempdescription = "";
    this.temprowdata = new MaterialinHand();
    this.filteredStores = [];
    this.tempmatloc = new matlocations();
    this.filteredracks = [];
    this.filteredbins = [];
    this.locationListdata();
    this.binListdata();
    this.rackListdata();
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
  Editlocation(data: matlocations) {
    this.tempmatloc = Object.assign({}, data);
    data.isedit = true;
  }
  cancelloc(data: matlocations) {
    debugger;
    data.storeid = this.tempmatloc.storeid;
    data.locatorname = this.tempmatloc.locatorname;
    data.binid = this.tempmatloc.binid;
    data.binnumber = this.tempmatloc.binnumber;
    data.rackid = this.tempmatloc.rackid;
    data.racknumber = this.tempmatloc.racknumber;
    this.tempmatloc = null;
    data.isedit = false;
  }
  saveloc(data: matlocations) {
    debugger;
    var itemloc = "";
    if (isNullOrUndefined(data.storeid) || data.storeid == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Store.' });
      return;
    }
    else {
      var storedata = this.locationlist.filter(function (element, index) {
        return (element.locatorid == data.storeid);
      });
      if (storedata.length > 0) {
        itemloc += storedata[0].locatorname;
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Invalid store.' });
        return;
      }

    }
    if (isNullOrUndefined(data.rackid) || data.rackid == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Store.' });
      return;
    }
    else {
      var rackdata = this.racklist.filter(function (element, index) {
        return (element.locatorid == data.storeid && element.rackid == data.rackid);
      });
      if (rackdata.length > 0) {
        itemloc += "." + rackdata[0].racknumber;
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Selected rack is not available in store ' + itemloc });
        return;
      }
    }
    if (isNullOrUndefined(data.binid) || data.binid == 0) {

    }
    else {
      var bindata = this.binlist.filter(function (element, index) {
        return (element.locatorid == data.storeid && element.rackid == data.rackid && element.binid == data.binid);
      });
      if (bindata.length > 0) {
        itemloc += "." + bindata[0].binnumber;
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Selected bin is not available in rack ' + itemloc });
        return;
      }
    }
    data.itemlocation = itemloc;
    this.wmsService.updatestocklocation(data).subscribe(data => {
      if (String(data) == "saved") {
        this.messageService.add({ severity: 'success', summary: '', detail: 'Location Updated Successfully' });
        //this.showadddatamodel = false;
        this.getlocations(this.tempdescription, this.temprowdata);

      }
      else {
        this.messageService.add({ severity: 'success', summary: '', detail: 'Location Update failed' });
      }
    });
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
  filterStore(event: any, data: matlocations) {
    this.filteredStores = [];

    for (let i = 0; i < this.locationlist.length; i++) {
      let lid = String(this.locationlist[i].locatorid);
      let lname = this.locationlist[i].locatorname;
      if (lid.toLowerCase().indexOf(event.query.toLowerCase()) == 0 || lname.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredStores.push(lname);
      }

    }

  }

  filterRack(event: any, data: matlocations) {
    debugger;
    let senddata = [];
    if (!isNullOrUndefined(data.storeid) && data.storeid != 0) {
      senddata = this.racklist.filter(function (element, index) {
        return (element.locatorid == data.storeid);
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
  filterBin(event: any, data: matlocations) {
    let senddata = [];
    debugger;
    if (!isNullOrUndefined(data.rackid) && data.rackid != 0 && !isNullOrUndefined(data.storeid) && data.storeid != 0) {
      senddata = this.binlist.filter(function (element, index) {
        return (element.locatorid == data.storeid && element.rackid == data.rackid);
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
  getlocations(poitemdescription: string, data: MaterialinHand) {
    this.getlocationlistdata = [];
    this.lblmaterial = data.material;
    this.lblmaterialdesc = poitemdescription;
    this.lblproject = data.projectname;
    this.lblpono = data.pono;
    this.tempdescription = poitemdescription;
    this.temprowdata = Object.assign({}, data); 
    this.spinner.show();
    this.wmsService.getmatinhandlocations(poitemdescription, data.material, data.projectname, data.pono, data.saleorderno).subscribe(data => {
      this.getlocationlistdata = data;
      this.showadddatamodel = true;
      this.spinner.hide();
    });

  }

  onStoreSelect(event: any, data: matlocations, index: number) {

    debugger;

    var loc = this.locationlist.filter(o => o.locatorname == data.locatorname);
    if (loc.length > 0) {
      data.locatorname = loc[0].locatorname;
      data.storeid = loc[0].locatorid;
    }
    else {
      data.locatorname = "";
      data.storeid = null;
    }

    data.rackid = null;
    data.racknumber = "";
    data.binid = null;
    data.binnumber = "";

  }

  onRackSelect(event: any, data: matlocations, index: number) {
    var loc = this.racklist.filter(o => o.racknumber == data.racknumber && o.locatorid == data.storeid);
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
  }

  onBinSelect(event: any, data: matlocations, index: number) {
    var loc = this.binlist.filter(o => o.binnumber == data.binnumber && o.rackid == data.rackid && o.locatorid == data.storeid);
    if (loc.length > 0) {
      data.binnumber = loc[0].binnumber;
      data.binid = loc[0].binid;

    }
    else {
      data.binnumber = "";
      data.binid = null;
    }

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
        'Sale Order No': obj.saleorderno,
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
