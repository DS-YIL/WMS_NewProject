import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl, ValidatorFn } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { StockModel } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-MiscellanousReceipts',
  templateUrl: './MiscellanousReceipts.component.html'
})
export class MiscellanousReceiptsComponent implements OnInit {

  constructor(private formBuilder: FormBuilder, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }
  public CreateMis: FormGroup;
  public ReceiptsList: Array<any> = [];
  public ReasonList: Array<any> = [];
  public employee: Employee;
  public displayReceiptsDialog; isSubmit: boolean = false;
  public initialStock: boolean = true;
  public MisData: StockModel;
  public dynamicData: DynamicSearchResult;
  public locationlists: any[] = [];
  public binlist: any[] = [];
  public racklist: any[] = [];
  public searchItems: Array<searchList> = [];
  public searchresult: Array<object> = [];
  public locationdata: any[] = [];
  public bindata: any[] = [];
  public rackdata: any[] = [];

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.MisData = new StockModel();
    this.locationlists = [];
    this.racklist = [];
    this.binlist = [];
    this.locationListdata();
    this.rackListdata();
    this.binListdata();
    this.getMiscellanousReceiptsList();

    this.CreateMis = this.formBuilder.group({
      material: ['', [Validators.required]],
      poitemdescription: ['', [Validators.required]],
      availableqty: ['', [Validators.required]],
      value: ['', [Validators.required]],
      storeid: ['', [Validators.required]],
      rackid: ['', [Validators.required]],
      binid: ['', [Validators.required]],
      projectid: ['', [Validators.required]],
      pono: ['', [Validators.required]]
    });
  }
  //get store list
  locationListdata() {
    this.wmsService.getlocationdata().subscribe(res => {
      this.locationlists = res;
    });
  }

  rackListdata() {
    this.wmsService.getrackdataforputaway().subscribe(res => {
      this.rackdata = res;
    });
  }

  binListdata() {
    this.wmsService.getbindataforputaway().subscribe(res => {
      this.bindata = res;
    });
  }

  //On selection of location updating rack
  onlocUpdate() {
    this.racklist = [];
    this.racklist = this.rackdata.filter(li => li.locationid == this.MisData.locatorid);
  }
  //On selection of rack updating bin
  onrackUpdate() {
    this.binlist = [];
    this.binlist = this.bindata.filter(li => li.locationid == this.MisData.locatorid && li.rackid == this.MisData.rackid)
  }



  //get Material list
  getMiscellanousReceiptsList() {
    this.spinner.show();
    this.wmsService.getMiscellanousReceiptsList().subscribe(data => {
      this.spinner.hide();
      this.ReceiptsList = data;
    });
  }

  //show dialog
  showReceiptsDialog() {
    this.displayReceiptsDialog = true;
    this.MisData = new StockModel();
    this.MisData.storeid = null;
    this.MisData.rackid = null;
    this.MisData.binid = null;

  }

  //bind materials based search
  public bindSearchListData(event: any, name?: string) {
    debugger;
    var searchTxt = event.query;
    if (searchTxt == undefined)
      searchTxt = "";
    searchTxt = searchTxt.replace('*', '%');
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.tableName = this.constants[name].tableName + " ";
    this.dynamicData.searchCondition = "" + this.constants[name].condition;
    this.dynamicData.searchCondition += "sk.materialid" + " ilike '" + searchTxt + "%'" + " and sk.availableqty>=1";
    //this.materialistModel.materialcost = "";
    this.wmsService.GetMaterialItems(this.dynamicData).subscribe(data => {
      this.searchresult = data;
      this.searchItems = [];
      var fName = "";
      this.searchresult.forEach(item => {
        fName = item[this.constants[name].fieldName];
        if (name == "ItemId")
          fName = item[this.constants[name].fieldId];
        var value = { code: item[this.constants[name].fieldId] };
        this.searchItems.push(item[this.constants[name].fieldId]);
      });
    });
  }

  dialogCancel(dialog: any) {
    this[dialog] = false;
  }
  //update stock 
  onSubmit() {
    this.isSubmit = true;
    if (this.CreateMis.invalid) {
      return;
    }
    this.MisData.createdby = this.employee.employeeno;
    var binnumber = this.binlist.filter(x => x.binid == this.MisData.binid);
    var storelocation = this.locationlists.filter(x => x.locatorid == this.MisData.storeid);
    var rack = this.racklist.filter(x => x.rackid == this.MisData.rackid);
    this.MisData.itemlocation = storelocation[0].locatorname + "." + rack[0].racknumber + '.' + binnumber[0].binnumber;
    this.spinner.show();
    this.wmsService.miscellanousReceiptDataUpdate(this.MisData).subscribe(data => {
      this.spinner.hide();
      this.getMiscellanousReceiptsList();
      this.displayReceiptsDialog = false;
      if (data) {
        this.messageService.add({ severity: 'success', summary: '', detail: 'Stock Updated' });
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Update Failed' });
      }

    });
  }


}
