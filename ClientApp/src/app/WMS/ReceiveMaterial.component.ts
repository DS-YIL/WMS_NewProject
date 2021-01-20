import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl, ValidatorFn } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { materialtransferMain, materialtransferTR, materialRequestDetails, returnmaterial, gatepassModel, materialistModel, PoDetails, StockModel, materialistModelreturn, materialistModeltransfer, ddlmodel, DirectTransferMain, STORequestdata } from 'src/app/Models/WMS.Model';
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
  public employee: Employee;
  currentstocktype: string = "";
  public showDetails; showLocationDialog: boolean = false;
  public StockModel: StockModel;
  public StockModelList: Array<any> = [];
  public PoDetails: PoDetails;
  public disSaveBtn: boolean = false;
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



  constructor(private messageService: MessageService, private ConfirmationService: ConfirmationService, private formBuilder: FormBuilder, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.selectedStatus = "Pending";
    this.STORequestlist();
   
  }

  showattachdata(data: STORequestdata) {
    data.showtr = !data.showtr;
  }
  //get STO transfer list
  STORequestlist() {
    var empno = this.employee.employeeno;
    this.wmsService.STORequestlist().subscribe(data => {
      this.storequestlist = data;
      this.requestlistSTO = data;
      this.storequestlist = this.requestlistSTO.filter(li => li.status == this.selectedStatus || li.status == null);
      this.storequestlist.forEach(item => {
        item.showtr = false;
      });
    });
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
    this.matdescription = details.poitemdescription;
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
        this.StockModel.lineitemno = this.PoDetails.lineitemno;
        this.StockModel.poitemdescription = this.PoDetails.poitemdescription,
          this.StockModel.unitprice = this.PoDetails.unitprice,
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
            this.wmsService.InsertmatSTO(this.StockModelList).subscribe(data => {
              debugger;
              this.spinner.hide();
              //this.podetailsList[this.rowIndex].itemlocation = this.StockModel.itemlocation;
              this.issaveprocess = true;
              this.showLocationDialog = false;
              this.messageService.add({ severity: 'success', summary: '', detail: 'Location Updated' });
              this.stock = [];
             
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

  onSelectStatus() {
    this.storequestlist = this.requestlistSTO.filter(li => li.status == this.selectedStatus);
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
