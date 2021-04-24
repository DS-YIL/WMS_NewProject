/*
    Name of File : <<AssignInventory Manager>>  Author :<<prasanna>>
    Date of Creation <<22-04-2021>>
    Purpose : <<Assign IM delegates>>
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
import { authUser} from '../Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { isNullOrUndefined } from 'util';

@Component({
  selector: 'app-AssignIM',
  templateUrl: './AssignInventoryManager.component.html'
})
export class AssignIMComponent implements OnInit {
  constructor(private wmsService: wmsService, private messageService: MessageService, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public employee: Employee;
  public dynamicData = new DynamicSearchResult();
  employeeModel: Array<any> = [];
  authUser: authUser;
  selectedEmployee: any;
  filteredEmployees: any[] = [];
  getlistdata: authUser[] = [];
  getaddlistdetail: authUser[] = [];
  showadddatamodel: boolean = false;
  public IsEdit: boolean = false;

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.authUser = new authUser();
    this.showadddatamodel = false;
    this.getlistdata = [];
    this.filteredEmployees = [];
    this.getEmployees();
    this.getUserAuth();
  }

  //get EmployeeList
  getEmployees() {
    this.spinner.show();
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.query = "select * from wms.v_getAssignRoleEmployees";
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.employeeModel = data;
      this.spinner.hide();
    })
  }


  //get User List
  getUserAuth() {
    this.spinner.show();
    this.getlistdata = [];
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.query = "select au.*,emp.name as employeename,au.deleteflag as isdeleted from wms.auth_users au  left outer join wms.employee emp on emp.employeeno = au.employeeid where au.roleid = 4 order by au.authid desc";
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.getlistdata = data;
      this.spinner.hide();
    })
  }

  showAdd() {
    this.IsEdit = false;
    this.showadddatamodel = true;
    this.authUser = new authUser();
  }

  EditUser(details) {
    this.IsEdit = true;
    this.authUser = details;
    this.showadddatamodel = true;
  }
  refreshsavemodel() {
    this.selectedEmployee = null;
    this.authUser = new authUser();
  }

  filterEmployee(event) {
    debugger;
    this.filteredEmployees = [];
    for (let i = 0; i < this.employeeModel.length; i++) {
      let brand = this.employeeModel[i].employeeno;
      let pos = this.employeeModel[i].name;
      if (brand.toLowerCase().indexOf(event.query.toLowerCase()) == 0 || pos.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.employeeModel[i].idwithname = pos + " (" + brand + ")";
        this.filteredEmployees.push(this.employeeModel[i]);
      }
    }
  }


  submitAdd() {
    if (isNullOrUndefined(this.selectedEmployee) && !this.IsEdit) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Employee' });
      return;
    }

    var data = this.getlistdata.filter(li => li.employeeid == this.selectedEmployee.employeeno)[0];
    if (!this.IsEdit && data && !data.isdelegatemember) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Employee Already Inventory manager ' });
      return;
    }
    if (!this.IsEdit && data && data.isdelegatemember) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Employee Already Delegated as Inventory Manager ' });
      return;
    }
    this.getaddlistdetail = [];
    this.authUser.roleid = 4;
    this.authUser.isdelegatemember = true;
    if (this.IsEdit) {//edit
      this.authUser.modifiedby = this.employee.employeeno;
    }
    else {//add
      this.authUser.employeeid = this.selectedEmployee.employeeno;
      this.authUser.createdby = this.employee.employeeno;
    }
    this.getaddlistdetail.push(this.authUser);
    this.spinner.show();
    this.wmsService.AddAuthUser(this.getaddlistdetail).subscribe(data => {
      this.spinner.hide();
      if (String(data) == "saved") {
        this.getUserAuth();
        this.showadddatamodel = false;
        this.messageService.add({ severity: 'success', summary: '', detail: 'Role Assigned' });
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: String(data) });
      }
    })

  }

  deleteUserauth(data: authUser) {
    this.spinner.show();
    this.getaddlistdetail = [];
    this.authUser = data;
    this.authUser.modifiedby = this.employee.employeeno;
    this.authUser.deleteflag = true;
    this.getaddlistdetail.push(this.authUser);
    this.wmsService.AddAuthUser(this.getaddlistdetail).subscribe(data => {
      this.spinner.hide();
      if (String(data) == "saved") {
        this.getUserAuth();
        this.messageService.add({ severity: 'success', summary: '', detail: 'User deleted' });
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: String(data) });
      }
    })

  }


  CheckforEmpdata() {
    var empid = this.selectedEmployee.employeeno;
    var data2 = this.getlistdata.filter(function (element, index) {
      if (element.employeeid == empid)
        this.messageService.add({ severity: 'error', summary: '', detail: "Delagation Added Already" });
      return true;
    });

  }

}

