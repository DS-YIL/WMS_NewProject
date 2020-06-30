/*
    Name of File : <<AssignRole>>  Author :<<prasanna>>
    Date of Creation <<11-06-2020>>
    Purpose : <<master page to Assign Roles for each employee>>
    Review Date :<<>>   Reviewed By :<<>>
    Version : 0.1 <change version only if there is major change - new release etc>
    Sourcecode Copyright : Yokogawa India Limited
*/
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { authUser } from '../Models/WMS.Model';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-AssignRole',
  templateUrl: './AssignRole.component.html'
})
export class AssignRoleComponent implements OnInit {
  constructor(private wmsService: wmsService, private messageService: MessageService, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public employee: Employee;
  public dynamicData = new DynamicSearchResult();
  roleNameModel: Array<any> = [];
  employeeModel: Array<any> = [];
  authUsersList: Array<any> = [];
  authUser: authUser;

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.authUser = new authUser();
    this.authUser.employeeid = 0;
    this.authUser.roleid = 0;
    this.authUser.createdby = this.employee.employeeno;
    this.getEmployees();
    this.getRoles();
    this.getauthUserList();
  }

  //get EmployeeList
  getEmployees() {
    this.spinner.show();
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.query = "select * from wms.v_getAssignRoleEmployees";
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.employeeModel = data;
      this.spinner.hide();
      //alert();
    })
  }

  //get Role list
  getRoles() {
    this.spinner.show();
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.query = "select * from wms.rolemaster";
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.roleNameModel = data;
      this.spinner.hide();
    })
  }

  //get autherization userslist
  getauthUserList() {
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.query = "select * from wms.auth_users";
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.authUsersList = data;
    })
  }

  rolechange() {

  }
  //Assign Roles
  assignRole() {
    if (this.authUser.employeeid && this.authUser.roleid) {
      if (this.authUsersList&&this.authUsersList.length > 0 && (this.authUsersList.filter(li => li.employeeid == this.authUser.employeeid && li.roleid == this.authUser.roleid).length > 0)) {
        this.messageService.add({ severity: 'error', summary: 'Validation', detail: 'Selected Role already added for this Employee' });
        return false;
      }
      this.spinner.show();
      this.wmsService.assignRole(this.authUser).subscribe(data => {
        this.spinner.hide();
        if (data) {
          this.authUser.authid = data;
          var object = { authid: data, employeeid: this.authUser.employeeid, roleid: this.authUser.roleid };
          this.authUsersList.push(object);
          this.messageService.add({ severity: 'success', summary: 'success Message', detail: 'Role added' });
        }
      })
    }
    else {
      if (!this.authUser.employeeid)
        this.messageService.add({ severity: 'error', summary: 'Validation', detail: 'select Employee' });
      if (!this.authUser.roleid)
        this.messageService.add({ severity: 'error', summary: 'Validation', detail: 'select Role' });
    }
  }
}

