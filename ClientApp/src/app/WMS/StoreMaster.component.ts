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
  selector: 'app-StoreMaster',
  templateUrl: './StoreMaster.component.html'
})
export class StoreMasterComponent implements OnInit {
  constructor(private wmsService: wmsService, private messageService: MessageService, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public employee: Employee;
  public dynamicData = new DynamicSearchResult();
  editModel: locataionDetailsStock;
  getlistdata: locataionDetailsStock[] = [];
  showadddatamodel: boolean = false;
  plantmodel: Array<any> = [];
  selectedplant: any;
  filteredPlant: any[] = [];
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
    this.getstoredata();
    
   
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
  //get EmployeeList
  refreshsavemodel() {
    this.editModel = new locataionDetailsStock();
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
    this.editModel = new locataionDetailsStock();
    this.editModel = Object.assign({}, data);
    this.isAddprocess = false;
    this.currenteditindex = data.storeid;
    var pid = this.editModel.plantid;
    var data1 = this.plantmodel.filter(function (element, index) {
      return (element.plantid == pid);
    });
    if (data1.length > 0) {
      this.selectedplant = data1[0];
    }
    this.showadddatamodel = true;
  }


  post() {
    debugger;
    if (isNullOrUndefined(this.selectedplant)) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Plant' });
      return;
    }
    if (!this.editModel.storename || this.editModel.storename.trim() == "") {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Store Name' });
      return;
    }
    if (this.isAddprocess) {
      var pltid = this.selectedplant.plantid;
      var strname = this.editModel.storename.trim().toLowerCase();
      var data1 = this.getlistdata.filter(function (element, index) {
        return (String(element.storename).trim().toLowerCase() == strname);
      });
      if (data1.length > 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Store already exists for the selected plant' });
        return;
      }
    }
    if (!this.isAddprocess) {
      var pltid = this.selectedplant.plantid;
      var strname = this.editModel.storename.trim().toLowerCase();
      var indx = this.currenteditindex;
      var data1 = this.getlistdata.filter(function (element, index) {
        return (String(element.storename).trim().toLowerCase() == strname && element.storeid != indx);
      });
      if (data1.length > 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Store already exists for the selected plant' });
        return;
      }
    }
    this.editModel.plantid = this.selectedplant.plantid;
    this.editModel.createdby = this.employee.employeeno;
    this.editModel.modifiedby = this.employee.employeeno;
    this.wmsService.addupdatestore(this.editModel).subscribe(data => {
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
    this.wmsService.getstorelist().subscribe(data => {
      this.getlistdata = data;
      this.spinner.hide();
    })
  }

  //get User List
 

  
 
 
  
  

  }

