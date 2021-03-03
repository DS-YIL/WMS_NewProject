import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl, ValidatorFn } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { materialtransferMain, materialtransferTR, materialRequestDetails, returnmaterial, gatepassModel, materialistModel, PoDetails, StockModel, materialistModelreturn, materialistModeltransfer, ddlmodel, DirectTransferMain, STORequestdata, STOrequestTR } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { isNullOrUndefined } from 'util';
import { ConfirmationService } from 'primeng/api';

@Component({
  selector: 'app-ReceiveMaterial',
  templateUrl: './ReceiveMaterial.component.html',
  providers: [ ConfirmationService]
})
export class ReceiveMaterialComponent implements OnInit {
  public selectedStatus: string;
  storequestlist: STORequestdata[] = [];
  requestlistSTO: STORequestdata[] = [];
  materialdatalist: STOrequestTR[] = [];
  public employee: Employee;
  filteredgrns: any[];
  currentstocktype: string = "";
  public showDetails; showLocationDialog: boolean = false;
  public StockModel: StockModel;
  public StockModelList: Array<any> = [];
  public PoDetails: PoDetails;
  public disSaveBtn: boolean = false;
  public putawaydiv: boolean = false;
  public maindiv: boolean = false;
  
  public rowIndex: number;
  issaveprocess: boolean = false;
  binid: any;
  rackid: any;
  matdescription: string = "";
  matqty: string = "";
  matid: string = "";
  public StockModelForm: FormGroup;
  public locationdata: any[] = [];
  public stock: StockModel[] = [];
  public locationlist: any[] = [];
  public store: any;
  public rack: any;
  public bin: any;
  public bindata: any[] = [];
  public rackdata: any[] = [];
  public binlist: any[] = [];
  public racklist: any[] = [];
  checkedgrnlist: ddlmodel[] = [];
  selectedgrnno: string = "";
  rowGroupMetadata: any;
  rowGroupMetadata1: any;
  requestedid: string;


  constructor(private messageService: MessageService, private ConfirmationService: ConfirmationService, private formBuilder: FormBuilder, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.requestedid = this.route.snapshot.queryParams.requestid;
    this.selectedStatus = "Pending";
    this.materialdatalist = [];
    this.checkedgrnlist = [];
    this.filteredgrns = [];
    this.selectedgrnno = "";
    this.maindiv = false;
    this.StockModel = new StockModel();
    this.putawaydiv = true;
    this.getcheckedgrn();
    //this.STORequestlist();

    this.locationListdata();
    this.binListdata();
    this.rackListdata();
   
  }

  ////get pending to accept
  getcheckedgrn() {
    this.checkedgrnlist = [];
    this.wmsService.getstogr().subscribe(data => {
      debugger;
      this.checkedgrnlist = data;
      if (this.requestedid) {
        debugger;
        this.selectedgrnno = this.requestedid;
        this.getMaterialdatalist(this.selectedgrnno);

      }
    });
  }

  showpodata1() {
    this.getMaterialdatalist(this.selectedgrnno);
  }
  filtergrn(event) {
    debugger;
    this.filteredgrns = [];
    for (let i = 0; i < this.checkedgrnlist.length; i++) {
      let pos = this.checkedgrnlist[i].text;
      if (pos.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredgrns.push(pos);
      }
    }
  }

  backtoreturn() {
    this.maindiv = true;
    this.putawaydiv = false;
  }

  getMaterialdatalist(transferid: string) {
    this.wmsService.STORequestdatalist(transferid).subscribe(data => {
      debugger;
      this.materialdatalist = data;
      //this.maindiv = false;
      //this.putawaydiv = true;
      this.updateRowGroupMetaData1();
    });

  }

  showattachdata(data: STORequestdata) {
    data.showtr = !data.showtr;
  }
  //get STO transfer list
  STORequestlist() {
    var empno = this.employee.employeeno;
    this.wmsService.STORequestlist().subscribe(data => {
      debugger;
      this.storequestlist = data;
      this.requestlistSTO = data;
      //this.maindiv = true;
      //this.putawaydiv = false;
      if (this.selectedStatus == "Issued") {
        this.storequestlist = this.requestlistSTO.filter(li => li.putawaystatus == true);
      }
      else {
        this.storequestlist = this.requestlistSTO.filter(li => li.putawaystatus == false);
      }
      if (!isNullOrUndefined(this.requestedid) && this.requestedid != "") {
        this.storequestlist = this.storequestlist.filter(li => li.transferid == this.requestedid);
      }
      //this.storequestlist.forEach(item => {
      //  item.showtr = false;
      //});
      //this.updateRowGroupMetaData();
    });
  }
  close() {

  }

  updateRowGroupMetaData1() {
    debugger;
    this.rowGroupMetadata1 = {};
    if (this.materialdatalist) {
      for (let i = 0; i < this.materialdatalist.length; i++) {
        let rowData = this.materialdatalist[i];
        let inwardidview = rowData.id;
        if (i == 0) {
          this.rowGroupMetadata1[inwardidview] = { index: 0, size: 1 };
        }
        else {
          let previousRowData = this.materialdatalist[i - 1];
          let previousRowGroup = previousRowData.id;
          if (inwardidview === previousRowGroup)
            this.rowGroupMetadata1[inwardidview].size++;
          else {
            this.rowGroupMetadata1[inwardidview] = { index: i, size: 1 };
          }


        }
      }
    }
  }


 

  updateRowGroupMetaData() {
    debugger;
    this.rowGroupMetadata = {};
    if (this.storequestlist) {
      var count = 0;
      for (let i = 0; i < this.storequestlist.length; i++) {
        let rowData = this.storequestlist[i].materialdata;
        for (let j = 0; j < rowData.length; j++) {
          let transferid = rowData[j].transferid;
          if (i == 0) {
            this.rowGroupMetadata[transferid] = { index: 0, size: 1 };
            count = count + 1;
            this.storequestlist[i].materialdata[j].serialno = count;
          }
          else {
            let previousRowData = this.storequestlist[i].materialdata[j - 1];
            let previousRowGroup = previousRowData.transferid;
            if (transferid === previousRowGroup)
              this.rowGroupMetadata[transferid].size++;
            else {
              this.rowGroupMetadata[transferid] = { index: j, size: 1 };
              count = count + 1;
              this.storequestlist[i].materialdata[j].serialno = count;
            }


          }
        }
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



  showDialog(details: any, index: number) {
    debugger;
    this.currentstocktype = details.stocktype;
    details.binid = details.defaultbin;
    details.rackid = details.defaultrack;
    details.storeid = details.defaultstore;
    details.stocktype = details.stocktype;
    this.showLocationDialog = true;
    this.PoDetails = details;
    this.PoDetails.poitemdescription = details.poitemdesc;
    this.rowIndex = index;
    this.binid = details.binid;
    this.rackid = details.rackid;
    this.matid = details.materialid;
    this.matqty = details.issuedqty;
    this.matdescription = details.poitemdesc;
    this.StockModel.locatorid = details.storeid;
    this.StockModel.rackid = details.rackid;
    this.StockModel.binid = details.binid;
   
   


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
                  this.stock[this.stock.length - 1].rackid = 0;
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
  onQtyClick() {

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
        this.StockModel.material = this.PoDetails.materialid;
        this.StockModel.itemid = this.PoDetails.itemid;
        this.StockModel.pono = this.PoDetails.pono;
        this.StockModel.lineitemno = this.PoDetails.lineitemno;
        this.StockModel.poitemdescription = this.PoDetails.poitemdescription,
        this.StockModel.unitprice = this.PoDetails.unitprice,
        this.StockModel.grnnumber = this.PoDetails.grnnumber;
        this.StockModel.vendorid = this.PoDetails.vendorid;
        this.StockModel.paitemid = this.PoDetails.paitemid;
        this.StockModel.totalquantity = this.PoDetails.materialqty;
        this.StockModel.createdby = this.employee.employeeno;
        this.StockModel.inwardid = this.PoDetails.inwardid;
        this.StockModel.id = this.PoDetails.id;
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

      if (totalqty == parseFloat(this.matqty)) {
        this.ConfirmationService.confirm({
          message: 'Are you sure to put away material in selected stock type?',
          accept: () => {
            this.disSaveBtn = true;
            this.spinner.show();
            this.wmsService.InsertmatSTO(this.StockModelList).subscribe(data => {
              debugger;
              this.spinner.hide();
              //this.podetailsList[this.rowIndex].itemlocation = this.StockModel.itemlocation;
              this.issaveprocess = true;
              this.showLocationDialog = false;
              this.messageService.add({ severity: 'success', summary: '', detail: 'Put away successful' });
              this.stock = [];
              this.getMaterialdatalist(this.selectedgrnno);
              this.getcheckedgrn();
              //this.STORequestlist();
             
              // }
            });
          },
          reject: () => {

            this.messageService.add({ severity: 'info', summary: '', detail: 'Cancelled' });
          }
        });



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

  deleteRow(index: number) {
    this.stock.splice(index, 1);
    //this.formArr.removeAt(index);
  }

  onSelectStatus(event: any) {
    this.selectedStatus = event.target.value;
    if (this.selectedStatus == "Issued") {
      this.storequestlist = this.requestlistSTO.filter(li => li.putawaystatus == true);
    }
    else {
      this.storequestlist = this.requestlistSTO.filter(li => li.putawaystatus == false);
    }
   // this.storequestlist = this.requestlistSTO.filter(li => li.status == this.selectedStatus);
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

}
