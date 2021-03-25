import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { authUser, ddlmodel, subrolemodel, AssignProjectModel, locataionDetailsStock } from '../Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { isNullOrUndefined } from 'util';
import { truncate } from 'fs';
import { environment } from 'src/environments/environment'

@Component({
  selector: 'app-BinMaster',
  templateUrl: './BinMaster.component.html'
})
export class BinMasterComponent implements OnInit {
  constructor(private wmsService: wmsService, private messageService: MessageService, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public employee: Employee;
  public dynamicData = new DynamicSearchResult();
  editModel: locataionDetailsStock;
  getlistdata: locataionDetailsStock[] = [];
  showadddatamodel: boolean = false;
  plantmodel: Array<any> = [];
  storemodel: Array<any> = [];
  rackmodel: Array<any> = [];
  allrackmodel: Array<any> = [];
  selectedplant: any;
  selectedstore: any;
  selectedrack: any;
  filteredPlant: any[] = [];
  filteredStore: any[] = [];
  filteredRack: any[] = [];
  isAddprocess: boolean = false;
  currenteditindex: number = -1;
  plantid = environment.plantid;

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.editModel = new locataionDetailsStock();
    this.showadddatamodel = false;
    this.currenteditindex = -1;
    this.isAddprocess = false;
    this.getlistdata = [];
    this.getPlants();
    this.getStores();
    this.getstoredata();
    this.getallRacks();
    
   
  }
  getPlants() {
    debugger;
    this.spinner.show();
    this.dynamicData = new DynamicSearchResult();
    var defaultplantid = this.plantid;
    this.dynamicData.query = " select * from wms.wms_rd_plant wrp where plantid = " + defaultplantid + " and deletedon is NULL";
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.plantmodel = data;
      this.selectedplant = this.plantmodel[0];
      this.spinner.hide();
    })
  }
  getStores() {
    this.spinner.show();
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.query = " select locatorid,locatorname from wms.wms_rd_locator where deleteflag is NOT True and plantid = " + this.plantid + "";
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.storemodel = data;
      this.spinner.hide();
    })
  }
  getRacks() {
    debugger;
    this.rackmodel = [];
    if (!isNullOrUndefined(this.selectedstore)) {
      this.spinner.show();
      var lid = this.selectedstore.locatorid;
      this.dynamicData = new DynamicSearchResult();
      this.dynamicData.query = " select rackid,racknumber from wms.wms_rd_rack where deleteflag is NOT True and locatorid = " + lid + "";
      this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
        this.rackmodel = data;
        this.spinner.hide();
      })
    }
    else {
      alert("rack not selected")
    }
    
  }
  getallRacks() {
    debugger;
    this.allrackmodel = [];
    
      this.spinner.show();
      this.dynamicData = new DynamicSearchResult();
      this.dynamicData.query = " select rackid,racknumber from wms.wms_rd_rack where deleteflag is NOT True";
      this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.allrackmodel = data;
        this.spinner.hide();
      })

  }
  filterPlants(event) {
    debugger;
    this.filteredPlant = [];
    for (let i = 0; i < this.plantmodel.length; i++) {
      let brand = this.plantmodel[i].plantname;
      if (brand.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredPlant.push(this.plantmodel[i]);
      }
    }
  }
  filterStores(event) {
    debugger;
    this.filteredStore = [];
    for (let i = 0; i < this.storemodel.length; i++) {
      let brand = this.storemodel[i].locatorname;
      if (brand.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredStore.push(this.storemodel[i]);
      }
    }
  }
  filterRacks(event) {
    debugger;
    if (this.rackmodel && this.rackmodel.length > 0) {
      this.filteredRack = [];
      for (let i = 0; i < this.rackmodel.length; i++) {
        let brand = this.rackmodel[i].racknumber;
        if (brand.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
          this.filteredRack.push(this.rackmodel[i]);
        }
      }
    }
    else {
      alert("no racks");
    }
   
  }
  //get EmployeeList
  refreshsavemodel() {
    this.editModel = new locataionDetailsStock();
    this.selectedstore = null;
    this.selectedrack = null;
    this.showadddatamodel = false;
    this.currenteditindex = -1;
  }

  AddStore() {
    this.editModel = new locataionDetailsStock();
    this.editModel.isactive = true;
    this.isAddprocess = true;
    this.showadddatamodel = true;
    this.currenteditindex = -1;

  }

  EditStore(data: locataionDetailsStock, ri: number) {
    debugger;
    this.editModel = new locataionDetailsStock();
    this.editModel = Object.assign({}, data);
    this.isAddprocess = false;
    this.currenteditindex = data.binid;
    var pid = this.editModel.plantid;
    var data1 = this.plantmodel.filter(function (element, index) {
      return (element.plantid == pid);
    });
    if (data1.length > 0) {
      this.selectedplant = data1[0];
    }
    var sid = this.editModel.storeid;
    var data2 = this.storemodel.filter(function (element, index) {
      return (element.locatorid == sid);
    });
    if (data2.length > 0) {
      this.selectedstore = data2[0];
    }
    var rid = this.editModel.rackid;
    var data3 = this.allrackmodel.filter(function (element, index) {
      return (element.rackid == rid);
    });
    if (data3.length > 0) {
      this.selectedrack = data3[0];
    }
    this.showadddatamodel = true;
  }


  post() {
    debugger;
    if (isNullOrUndefined(this.selectedplant)) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Plant' });
      return;
    }
    if (isNullOrUndefined(this.selectedstore)) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Store' });
      return;
    }
    if (isNullOrUndefined(this.selectedstore.locatorid)) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select valid store' });
      return;
    }
    if (isNullOrUndefined(this.selectedrack)) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Rack' });
      return;
    }
    if (isNullOrUndefined(this.selectedrack.rackid)) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select valid Rack' });
      return;
    }
    if (!this.editModel.binname || this.editModel.binname.trim() == "") {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Bin Name' });
      return;
    }
    if (this.isAddprocess) {
      var strid = this.selectedstore.locatorid;
      var rcid = this.selectedrack.rackid;
      var strname = this.editModel.binname.trim().toLowerCase();
      var data1 = this.getlistdata.filter(function (element, index) {
        return (element.storeid == strid && element.rackid == rcid && String(element.binname).trim().toLowerCase() == strname);
      });
      if (data1.length > 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Bin already exists for the selected Store and Rack' });
        return;
      }
    }
    if (!this.isAddprocess) {
      var strid = this.selectedstore.locatorid;
      var rcid = this.selectedrack.rackid;
      var strname = this.editModel.binname.trim().toLowerCase();
      var currindex = this.currenteditindex;
      var data1 = this.getlistdata.filter(function (element, index) {
        return (element.storeid == strid && element.rackid == rcid && String(element.binname).trim().toLowerCase() == strname && element.binid != currindex);
      });
      if (data1.length > 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Bin already exists for the selected Store and Rack' });
        return;
      }
    }
    this.editModel.storeid = this.selectedstore.locatorid;
    this.editModel.rackid = this.selectedrack.rackid;
    this.editModel.createdby = this.employee.employeeno;
    this.editModel.modifiedby = this.employee.employeeno;
    this.wmsService.addupdatebin(this.editModel).subscribe(data => {
      this.showadddatamodel = false;
      if (String(data) == "saved") {
        this.messageService.add({ severity: 'success', summary: '', detail: "Saved" });
        this.getstoredata();
        this.spinner.hide();
      }
      else if (String(data) == "Updated") {
        this.messageService.add({ severity: 'success', summary: '', detail: "Updated" });
        this.getstoredata();
        this.spinner.hide();

      }
      else if (String(data) == "error") {
        this.messageService.add({ severity: 'error', summary: '', detail: "Not Saved" });
        this.getstoredata();
        this.spinner.hide();

      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: String(data) });
        this.spinner.hide();
      }

    });
  }


 
  getstoredata() {
    this.spinner.show();
    this.getlistdata = [];
    var empno = this.employee.employeeno;
    this.wmsService.getbinlistdata().subscribe(data => {
      this.getlistdata = data;
      this.spinner.hide();
    })
  }

  //get User List
 

  
 
 
  
  

  }

