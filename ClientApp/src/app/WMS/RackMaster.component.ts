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
  selector: 'app-RackMaster',
  templateUrl: './RackMaster.component.html'
})
export class RackMasterComponent implements OnInit {
  constructor(private wmsService: wmsService, private messageService: MessageService, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public employee: Employee;
  public dynamicData = new DynamicSearchResult();
  editModel: locataionDetailsStock;
  getlistdata: locataionDetailsStock[] = [];
  showadddatamodel: boolean = false;
  plantmodel: Array<any> = [];
  storemodel: Array<any> = [];
  selectedplant: any;
  selectedstore: any;
  filteredPlant: any[] = [];
  filteredStore: any[] = [];
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
    this.getrackdata();
    
   
  }
  getPlants() {
    debugger;
    this.spinner.show();
    this.dynamicData = new DynamicSearchResult();
    var defaultplantid = this.plantid;
    this.dynamicData.query = " select * from wms.wms_rd_plant wrp where plantid = " + defaultplantid +" and deletedon is NULL";
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.plantmodel = data;
      this.selectedplant = this.plantmodel[0];
      this.spinner.hide();
    })
  }
  getStores() {
    this.spinner.show();
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.query = " select locatorid,locatorname from wms.wms_rd_locator where deleteflag is NOT True and plantid = " + this.plantid+"";
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.storemodel = data;
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
  //get EmployeeList
  refreshsavemodel() {
    this.editModel = new locataionDetailsStock();
    this.selectedstore = null;
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
    this.selectedstore = null;
    this.isAddprocess = false;
    this.currenteditindex = data.rackid;
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
    if (!this.editModel.rackname || this.editModel.rackname.trim() == "") {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Rack Name' });
      return;
    }
    if (this.isAddprocess) {
      var pltid = this.selectedstore.locatorid;
      var strname = this.editModel.rackname.trim().toLowerCase();
      var data1 = this.getlistdata.filter(function (element, index) {
        return (element.storeid == pltid && String(element.rackname).trim().toLowerCase() == strname);
      });
      if (data1.length > 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Rack already exists for the selected Store' });
        return;
      }
    }
    if (!this.isAddprocess) {
      var pltid = this.selectedstore.locatorid;
      var strname = this.editModel.rackname.trim().toLowerCase();
      var indx = this.currenteditindex;
      var data1 = this.getlistdata.filter(function (element, index) {
        return (element.storeid == pltid && String(element.rackname).trim().toLowerCase() == strname && element.rackid != indx);
      });
      if (data1.length > 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Rack already exists for the selected store' });
        return;
      }
    }
    this.editModel.storeid = this.selectedstore.locatorid;
    this.editModel.createdby = this.employee.employeeno;
    this.editModel.modifiedby = this.employee.employeeno;
    this.wmsService.addupdaterack(this.editModel).subscribe(data => {
      this.showadddatamodel = false;
      if (String(data) == "saved") {
        this.messageService.add({ severity: 'success', summary: '', detail: "Saved" });
        this.getrackdata();
        this.spinner.hide();
      }
      else if (String(data) == "Updated") {
        this.messageService.add({ severity: 'success', summary: '', detail: "Updated" });
        this.getrackdata();
        this.spinner.hide();

      }
      else if (String(data) == "error") {
        this.messageService.add({ severity: 'error', summary: '', detail: "Not Saved" });
        this.getrackdata();
        this.spinner.hide();

      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: String(data) });
        this.spinner.hide();
      }

    });
  }


 
  getrackdata() {
    this.spinner.show();
    this.getlistdata = [];
    var empno = this.employee.employeeno;
    this.wmsService.getracklist().subscribe(data => {
      this.getlistdata = data;
      this.spinner.hide();
    })
  }

  //get User List
 

  
 
 
  
  

  }

