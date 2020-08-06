import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl, ValidatorFn } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { materialRequestDetails, StockModel } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-MaterialReturnDashBoard',
  templateUrl: './MaterialReturnDashBoard.component.html'
})
export class MaterialReturnDashBoardComponent implements OnInit {
  binid: any;
  rackid: any;
  locatorid: any;
    selectedStatus: any;
  constructor(private formBuilder: FormBuilder, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }
  AddDialog: boolean;
  showdialog: boolean;
  btnDisable: boolean = false;
  public locationlist: any[] = [];
  public locationlists: any[] = [];
  public binlist: any[] = [];
  public racklist: any[] = [];
  public StockModel: StockModel;
  public materiallistData: Array<any> = [];
  public formName: string;
  public txtName: string;
  public dynamicData = new DynamicSearchResult();
  public showList: boolean = false;
  public searchItems: Array<searchList> = [];
  public selectedlist: Array<searchList> = [];
  public selectedItem: searchList;
  public searchresult: Array<object> = [];

  public MaterialRequestForm: FormGroup
  public materialIssueList: Array<any> = [];
  public materialacceptListnofilter: Array<any> = [];
  public employee: Employee;
  public displayItemRequestDialog; RequestDetailsSubmitted: boolean = false;
  public materialRequestDetails: materialRequestDetails;
  public StockModelForm: FormGroup;
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.StockModel = new StockModel();
    this.getMaterialIssueList();
    
  }

  //get material issue list based on loginid
  getMaterialIssueList() {
    //this.employee.employeeno = "400095";
    this.wmsService.GetReturnmaterialList().subscribe(data => {
      this.materialacceptListnofilter = data;
      this.materialIssueList = this.materialacceptListnofilter.filter(li => li.confirmstatus==null);
      //this.materialIssueList = data.filter(li=>li.requesttype=='return');
     
    });
  }
  showmaterialdetails(requestid) {
    //this.rowindex = rowindex
    this.AddDialog = true;
    this.showdialog = true;
    this.locationListdata();
    this.binListdata();
    this.rackListdata();
    this.wmsService.getreturnmaterialListforconfirm(requestid).subscribe(data => {
      this.materiallistData = data;
      this.bindSearchListData(this.materiallistData[0].materialid)
      if (this.materiallistData[0].confirmstatus == 'Accepted') {
        this.btnDisable = true;
      }
      else {
        this.btnDisable = false;
      }
    });
  }
  ConfirmReturnmaterial() {
    if (this.materiallistData[0].itemlocation == null) {
      this.messageService.add({ severity: 'error', summary: 'error Message', detail: 'Please select item location' });
      return false;
    }

  else if (this.materiallistData[0].itemlocation == 'other') {
      if (this.StockModel.locatorid == undefined) {
        this.messageService.add({ severity: 'error', summary: 'error Message', detail: 'Please select store' });
        return false;
      }
     if (this.StockModel.rackid == undefined) {
        this.messageService.add({ severity: 'error', summary: 'error Message', detail: 'Please select Location' });
        return false;
      }
       if (this.StockModel.rackid == undefined && this.StockModel.binid == undefined) {
        this.messageService.add({ severity: 'error', summary: 'error Message', detail: 'Please select Rack or Bin' });
        return false;
      }
      if (this.StockModel.rackid != undefined || this.StockModel.rackid != undefined || this.StockModel.binid != undefined) {
        var bindetails = this.binlist.filter(x => x.binid == this.StockModel.binid);
        var storedetails = this.locationlists.filter(x => x.locatorid == this.StockModel.locatorid);
        var rackdetails = this.racklist.filter(x => x.rackid == this.StockModel.rackid);
       if (this.StockModel.binid == undefined)
         this.materiallistData[0].itemlocation = storedetails[0].locatorname + "." + rackdetails[0].racknumber;
       if (this.StockModel.rackid == undefined)
         this.materiallistData[0].itemlocation = storedetails[0].locatorname + "." + bindetails[0].binnumber;
       if (this.StockModel.binid != undefined && this.StockModel.rackid != undefined)
         this.materiallistData[0].itemlocation = storedetails[0].locatorname + "." + rackdetails[0].racknumber + "." + bindetails[0].binnumber;
      }
      
    }
      this.wmsService.UpdateReturnmaterialTostock(this.materiallistData).subscribe(data => {
        this.AddDialog = false;
        if (data == 1) {
          this.messageService.add({ severity: 'success', summary: 'success Message', detail: 'Items updated to Store' });
          this.router.navigateByUrl("/WMS/MaterialReturn");
        }

      })
    
    
  }
  public bindSearchListData(materialid) {
    this.dynamicData.tableName = "wms.wms_stock";
    this.dynamicData.searchCondition = " where materialid='" + materialid+"'";
    this.wmsService.GetListItems(this.dynamicData).subscribe(res => {

      this.locationlist = res;
      let _list: any[] = [];
      for (let i = 0; i < (res.length); i++) {
        _list.push({
          itemlocation: res[i].itemlocation,
          // projectcode: res[i].projectcode
        });
      }
      this.locationlist = _list;
    });
  }

  locationListdata() {
    this.wmsService.getlocationdata().
      subscribe(
        res => {
          //this._list = res; //save posts in array
          this.locationlists = res;
          let _list: any[] = [];
          for (let i = 0; i < (res.length); i++) {
            _list.push({
              locatorid: res[i].locatorid,
              locatorname: res[i].locatorname
            });
          }
          this.locationlists = _list;
        });
  }
  binListdata() {
    debugger;
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
    debugger;
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
  onChange(value, indexid: any) {
    if (value == 'other') {
      document.getElementById('nodefaultloaction').style.display = "block";
    }
    this.materiallistData[indexid].itemlocation = value;
    // (<HTMLInputElement>document.getElementById(indexid)).value = event.toString();
  }
  onSelectStatus(event) {
    this.selectedStatus = event.target.value;

  }
  SubmitStatus() {
    if (this.selectedStatus == "Pending") {
      this.materialIssueList = this.materialacceptListnofilter.filter(li => li.confirmstatus == null);
    }
    else if (this.selectedStatus == "Accepted") {
      this.materialIssueList = this.materialacceptListnofilter.filter(li => li.confirmstatus == 'Accepted');
    }
  }
}
