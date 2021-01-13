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

@Component({
  selector: 'app-ReceiveMaterial',
  templateUrl: './ReceiveMaterial.component.html'
})
export class ReceiveMaterialComponent implements OnInit {
  
  storequestlist: STORequestdata[] = [];
  public employee: Employee;
  currentstocktype: string = "";
  public showDetails; showLocationDialog: boolean = false;
  public StockModel: StockModel;
  public PoDetails: PoDetails;
  public rowIndex: number;
  binid: any;
  rackid: any;
  matdescription: string = "";
  matqty: string = "";
  matid: string = "";
  public StockModelForm: FormGroup;
  public stock: StockModel[] = [];
  public locationlist: any[] = [];
  public store: any;
  public rack: any;
  public bin: any;
  public bindata: any[] = [];
  public rackdata: any[] = [];
  public binlist: any[] = [];
  public racklist: any[] = [];



  constructor(private messageService: MessageService, private formBuilder: FormBuilder, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
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
