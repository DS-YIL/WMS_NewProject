import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { PoDetails, BarcodeModel, StockModel, inwardModel } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-Warehouse',
  templateUrl: './WarehouseIncharge.component.html'
})
export class WarehouseInchargeComponent implements OnInit {

  constructor(private formBuilder: FormBuilder, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public store: any;
  public rack: any;
  public bin: any;

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
  public StockModelForm: FormGroup;
  public rowIndex: number;

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.PoDetails = new PoDetails();
    this.StockModel = new StockModel();
    this.StockModel.shelflife = new Date();
    this.StockModelForm = this.formBuilder.group({
      itemlocation: ['', [Validators.required]],
      rackid: ['', [Validators.required]],
      binid: ['', [Validators.required]],
      shelflife: ['', [Validators.required]]
    });

    // this.loadStores();
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

  SearchGRNNo() {
    if (this.PoDetails.grnnumber) {
      this.spinner.show();
      this.podetailsList = [];
      this.wmsService.getitemdetailsbygrnno(this.PoDetails.grnnumber).subscribe(data => {
        this.spinner.hide();
        if (data) {
          //this.PoDetails = data[0];
          this.podetailsList = data;
          this.showDetails = true;
        }
        else
          this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'No data for this GRN No' });
      })
    }
    else
      this.messageService.add({ severity: 'error', summary: 'Validation', detail: 'Enter GRNNo' });
  }

  showDialog(details: any, index: number) {
    this.showLocationDialog = true;
    this.PoDetails = details;
    this.rowIndex = index;
    this.store = "";
    this.rack = "";
    this.bin = "";
  }

  onSubmitStockDetails() {
    if (this.store.name && this.rack.name) {
      this.StockModel.material = this.PoDetails.material;
      this.StockModel.itemid = this.PoDetails.itemid;
      this.StockModel.pono = this.PoDetails.pono;
      this.StockModel.grnnumber = this.PoDetails.grnnumber;
      this.StockModel.vendorid = this.PoDetails.vendorid;
      this.StockModel.paitemid = this.PoDetails.paitemid;
      this.StockModel.totalquantity = this.PoDetails.materialqty;
      this.StockModel.createdby = this.employee.employeeno;
      this.StockModel.itemlocation = this.store.code;
      this.StockModel.rackid = this.rack.code;
      this.StockModel.binid = this.bin.code;
      this.StockModel.confirmqty = this.PoDetails.confirmqty;
      this.StockModel.itemreceivedfrom = new Date();
      //this.StockModel.itemlocation = this.store.name + "." + this.rack.name + '.' + this.bin.name;
      this.StockModel.itemlocation = this.store.name + "." + this.rack.name;
      if (this.bin && this.bin.name)
        this.StockModel.itemlocation += '.' + this.bin.name;
      if (!this.bin && !this.bin.code)
        this.StockModel.binid = 1;
      this.wmsService.InsertStock(this.StockModel).subscribe(data => {
        // if (data) {
        //this.podetailsList[this.rowIndex].itemlocation = data;
        this.podetailsList[this.rowIndex].itemlocation = this.StockModel.itemlocation;
        this.showLocationDialog = false;
        this.messageService.add({ severity: 'success', summary: 'Success Message', detail: 'Location Updated' });
        // }
      });
    }
    else {
      if (!this.store.name)
        this.messageService.add({ severity: 'error', summary: 'Validation', detail: 'Select Location' });
      else if (!this.rack.name)
        this.messageService.add({ severity: 'error', summary: 'Validation', detail: 'Select Rack'});
    }
  }



  dialogCancel(dialogName) {
    this[dialogName] = false;
  }


}
