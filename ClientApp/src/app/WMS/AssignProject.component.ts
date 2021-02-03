import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { authUser, ddlmodel, subrolemodel, AssignProjectModel } from '../Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { isNullOrUndefined } from 'util';
import { truncate } from 'fs';

@Component({
  selector: 'app-AssignProject',
  templateUrl: './AssignProject.component.html'
})
export class AssignProjectComponent implements OnInit {
  constructor(private wmsService: wmsService, private messageService: MessageService, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public employee: Employee;
  public dynamicData = new DynamicSearchResult();
  employeeModel: Array<any> = [];
  selectedEmployee: Array<any> = [];;
  filteredEmployees: any[] = [];
  //configuration task
  getlistdata: AssignProjectModel[] = [];
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
    var memberid = "";
    var membername = "";
    var i = 0;
    this.selectedEmployee.forEach(item => {
      if (i > 0) {
        memberid += ",";
        membername += ",";
      }
      memberid += item.employeeno;
      membername += item.name;
      i++;

    })
    this.getlistdata[this.currentindex].projectmember = memberid;
    this.getlistdata[this.currentindex].projectmembername = membername;
    this.getlistdata[this.currentindex].projectmemberlist = this.selectedEmployee;
    this.getlistdata[this.currentindex].modifiedby = this.employee.employeeno;
    this.getlistdata[this.currentindex].plantid = this.employee.plantid;
    this.spinner.show();
      this.wmsService.updateProjectMember(this.getlistdata[this.currentindex]).subscribe(data => {
        this.showadddatamodel = false;
        if (String(data) == "saved") {
          this.messageService.add({ severity: 'success', summary: '', detail: 'Project member updated' });
          this.getUserAuth();
        }
        else {
          this.messageService.add({ severity: 'error', summary: '', detail: String(data) });
        }
        this.spinner.hide();
      })
   
   
   

  }

  setvalues() {
    debugger;
    this.getlistdata.forEach(item => {
      item.projectmembername = "";
      item.projectmemberlist = [];
      if (!isNullOrUndefined(item.projectmember)) {
        if (String(item.projectmember).includes(",")) {
          var memberarr = String(item.projectmember).split(",");
          var ind = 0;
          memberarr.forEach(item1 => {
            var emppno = item1;
            var data2 = this.employeeModel.filter(function (element, index) {
              return (element.employeeno == emppno);
            });
            if (data2.length > 0) {
              if (ind > 0) {
                item.projectmembername += ",";
              }
              item.projectmembername += data2[0].name;
              item.projectmemberlist.push(data2[0]);
            }
            ind++;

          })
        } else {
          var emppno = item.projectmember;
          var data2 = this.employeeModel.filter(function (element, index) {
            return (element.employeeno == emppno);
          });
          if (data2.length > 0) {
            item.projectmembername = data2[0].name;
            item.projectmemberlist.push(data2[0]);
          }

        }
      }

    })
  }

 

  //get User List
  getUserAuth() {
    this.spinner.show();
    this.getlistdata = [];
    var empno = this.employee.employeeno;
    this.wmsService.getprojectlisttosiign(empno).subscribe(data => {
      this.getlistdata = data;
      if (this.getlistdata) {
        this.setvalues();
      }
      this.spinner.hide();
    })
  }

  //get User List
 

  
 
 
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

