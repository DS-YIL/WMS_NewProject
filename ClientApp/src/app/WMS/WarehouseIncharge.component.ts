import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList, printMaterial } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { PoDetails, BarcodeModel, StockModel, inwardModel, locationddl, binddl, rackddl } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-Warehouse',
  templateUrl: './WarehouseIncharge.component.html',
  providers: [DatePipe]
})
export class WarehouseInchargeComponent implements OnInit {
    binid: any;
    rackid: any;
    isnonpo: boolean=false;

  constructor(private formBuilder: FormBuilder, private messageService: MessageService, private datePipe: DatePipe, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

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
  public StockModelForm: FormGroup;
  public StockModelForm1: FormGroup;
  public rowIndex: number;
  public locationlist: any[] = [];
  public binlist: any[] = [];
  public racklist: any[] = [];
  locationlist1: locationddl[] = [];
  binlist1: binddl[] = [];
  racklist1: rackddl[] = [];
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

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.PoDetails = new PoDetails();
    this.StockModel = new StockModel();
    this.locationListdata1();
    this.binListdata1();
    this.rackListdata1();
    this.StockModel.shelflife = new Date();
    //this.userForm = this.fb.group({
    //  users: this.fb.array([])
    //});
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

  //generate barcode -gayathri
  generateBarcode(details:any) {
    this.showPrintDialog = true;
    this.materialCode = details.material;
    this.receivedDate = this.datePipe.transform(details.receiveddate, this.constants.dateFormat)  ;
    this.acceptedQty = details.confirmqty;
    this.pono = details.pono;
    this.invoiceNo = details.invoiceno;
    this.grnNo = details.grnnumber;
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
              locatorname: res[i].locatorname
            });
          }
          this.locationlist = _list;
        });
  }
  binListdata() {
    this.wmsService.getbindata().
      subscribe(
        res => {
          //this._list = res; //save posts in array
          this.binlist = res;
          let _list: any[] = [];
          for (let i = 0; i < (res.length); i++) {
            _list.push({
              binid: res[i].binid,
              binnumber: res[i].binnumber
            });
          }
          this.binlist = _list;
        });
  }
  rackListdata() {
    this.wmsService.getrackdata().
      subscribe(
        res => {
          //this._list = res; //save posts in array
          this.racklist = res;
          let _list: any[] = [];
          for (let i = 0; i < (res.length); i++) {
            _list.push({
              rackid: res[i].rackid,
             racknumber: res[i].racknumber
            });
          }
          this.racklist = _list;
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

  SearchGRNNo() {

    if (this.PoDetails.grnnumber) {
      this.isnonpo = false;
      this.spinner.show();
      this.podetailsList = [];
      this.wmsService.getitemdetailsbygrnno(this.PoDetails.grnnumber).subscribe(data => {
        this.spinner.hide();
        if (data) {
          //this.PoDetails = data[0];
          this.podetailsList = data;
          this.showDetails = true;
          var ponumber = this.podetailsList[0].pono;
          if (ponumber.startsWith("NP")) {
            this.isnonpo = true;
          }
        }
        else
          this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'No data for this GRN No' });
      })
    }
    else
      this.messageService.add({ severity: 'error', summary: 'Validation', detail: 'Enter GRNNo' });
  }

  showDialog1(details: any, index: number) {
    this.showLocationDialog = true;
    this.PoDetails = details;
    this.rowIndex = index;
    this.binid = details.binid;
    this.rackid = details.rackid;
    this.StockModelForm = this.formBuilder.group({
      rackid: [details.rackid],
      binid: [details.binid],
      locatorid: [details.storeid]
    });
    this.locationListdata();
    this.binListdata();
    this.rackListdata();
    this.rack = "";
    this.bin = "";
    this.store ="";
  }

  showDialog(details: any, index: number) {
    this.showLocationDialog = true;
    this.PoDetails = details;
    this.rowIndex = index;
    this.binid = details.binid;
    this.rackid = details.rackid;
    this.matid = details.material;
    this.matdescription = details.materialdescription;
    this.matqty = details.receivedqty;
    //this.StockModelForm = this.formBuilder.group({
    //  rackid: [details.rackid],
    //  binid: [details.binid],
    //  locatorid: [details.storeid]
    //});
    //this.locationListdata();
    //this.binListdata();
    //this.rackListdata();
    //this.rack = "";
    //this.bin = "";
    //this.store = "";
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
    if (this.StockModelForm.controls.rackid.value && this.StockModelForm.controls.locatorid.value) {
      this.StockModel.material = this.PoDetails.material;
      this.StockModel.itemid = this.PoDetails.itemid;
      this.StockModel.pono = this.PoDetails.pono;
      this.StockModel.grnnumber = this.PoDetails.grnnumber;
      this.StockModel.vendorid = this.PoDetails.vendorid;
      this.StockModel.paitemid = this.PoDetails.paitemid;
      this.StockModel.totalquantity = this.PoDetails.materialqty;
    this.StockModel.createdby = this.employee.employeeno;
    var binnumber: any[] = [];
    var storelocation: any[] = [];
    var rack: any[] = [];
      binnumber = this.binlist.filter(x => x.binid == this.StockModelForm.controls.binid.value);
    storelocation = this.locationlist.filter(x => x.locatorid == this.StockModelForm.controls.locatorid.value);
      rack = this.racklist.filter(x => x.rackid == this.StockModelForm.controls.rackid.value);
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
    //this.StockModel.itemlocation = storelocation[0].locatorname;
      this.StockModel.rackid = rack[0].rackid;
      this.StockModel.confirmqty = this.PoDetails.confirmqty;
      this.StockModel.itemreceivedfrom = new Date();
    
      //if (binnumber.length == 0)
      //  this.StockModel.itemlocation += '.' + binnumber[0].binnumber;
      //if (binnumber.length == 0)
        
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

  locationListdata1() {
    this.wmsService.getlocationdata().
      subscribe(
        res => {
          //this._list = res; //save posts in array
          this.locationlist1 = res;
        });
  }
  binListdata1() {
    this.wmsService.getbindata().
      subscribe(
        res => {
          //this._list = res; //save posts in array
          this.binlist1 = res;
        });
  }
  rackListdata1() {
    this.wmsService.getrackdata().
      subscribe(
        res => {
          //this._list = res; //save posts in array
          this.racklist1 = res;
        });
  } 


}
