import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { authUser, ddlmodel, subrolemodel, AssignProjectModel, assignpmmodel, UserModel } from '../Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { isNullOrUndefined } from 'util';
import { truncate } from 'fs';

@Component({
  selector: 'app-AssignProjectManager',
  templateUrl: './AssignProjectManager.component.html'
})
export class AssignProjectManagerComponent implements OnInit {
  constructor(private wmsService: wmsService, private messageService: MessageService, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public employee: Employee;
  public dynamicData = new DynamicSearchResult();
  employeeModel: Array<any> = [];
  selectedEmployee: Array<any> = [];;
  filteredEmployees: any[] = [];
  //configuration task
  getlistdata: assignpmmodel[] = [];
  projectcode: string = "";
  managername: string = "";
  showadddatamodel: boolean = false;
  currentindex: number = -1;

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.showadddatamodel = false;
    this.projectcode = "";
    this.managername = "";
    this.getlistdata = [];
    this.filteredEmployees = [];
    this.getEmployees();
    
   
  }

  //get EmployeeList
  getEmployees() {
    this.spinner.show();
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.query = "select * from wms.v_getAssignRoleEmployees";
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.employeeModel = data;
      this.spinner.hide();
      this.getUserAuth();
      //alert();
    })
  }

  EditMember(data: AssignProjectModel, index: number) {
    this.projectcode = data.projectcode;
    this.managername = data.projectmanagername;
    this.currentindex = index;
    if (data.projectmemberlist) {
      this.selectedEmployee = data.projectmemberlist;
    }
    this.showadddatamodel = true;
   

  }

  refreshsavemodel() {
    this.selectedEmployee = [];
    this.projectcode = "";
    this.managername = "";
    this.currentindex = -1;
  }

  submitAdd() {
    debugger;
    var senddata = this.getlistdata.filter(function (element, index) {
      return (element.isselected);
    });
    if (senddata.length == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select project' });
      return;
    }
  
    var invaliddata = senddata.filter(function (element, index) {
      return (element.isselected && (isNullOrUndefined(element.selectedemployeeview) || isNullOrUndefined(element.selectedemployeeview.employeeno)));
    });
    if (invaliddata.length > 0) {
      var prj = invaliddata[0].projectcode;
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select project manager for project : ' + prj });
      return;
    }

    this.spinner.show();
    this.wmsService.updatepm(senddata).subscribe(data => {
      this.spinner.hide();
      if (String(data) == "saved") {
        this.messageService.add({ severity: 'succeess', summary: '', detail: 'Project manager assigned' });
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: String(data) });
      }
      this.getUserAuth();
    })
   
   
   
   

  }

 

  //get User List
  getUserAuth() {
    this.spinner.show();
    this.getlistdata = [];
    var empno = this.employee.employeeno;
    this.wmsService.getprojectlisttossignPM().subscribe(data => {
      this.getlistdata = data;
      this.spinner.hide();
      this.setvalues();
    })
  }

  //get User List
  setvalues() {
    debugger;
    this.getlistdata.forEach(item => {
      item.selectedemployeeview = JSON.parse(item.selectedemployee) as UserModel;
    });
  }

  
 
 
  filterEmployee(event) {
    debugger;
    this.filteredEmployees = [];
    for (let i = 0; i < this.employeeModel.length; i++) {
      let brand = this.employeeModel[i].employeeno;
      let pos = this.employeeModel[i].name;
      if (brand.toLowerCase().indexOf(event.query.toLowerCase()) == 0 || pos.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.employeeModel[i].idwithname = pos + " (" + brand+")";
        this.filteredEmployees.push(this.employeeModel[i]);
      }
    }
  }

  

  }

