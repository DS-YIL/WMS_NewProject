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
import { authUser, ddlmodel, subrolemodel } from '../Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { isNullOrUndefined } from 'util';

@Component({
  selector: 'app-AssignRole',
  templateUrl: './AssignRole.component.html'
})
export class AssignRoleComponent implements OnInit {
  constructor(private wmsService: wmsService, private messageService: MessageService, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public employee: Employee;
  public dynamicData = new DynamicSearchResult();
  roleNameModel: Array<any> = [];
  plantmodel: Array<any> = [];
  employeeModel: Array<any> = [];
  authUsersList: Array<any> = [];
  authUser: authUser;
  selectedEmployee: any;
  selectedplant: any;
  selectedrolewise: any;
  filteredEmployees: any[] = [];
  filteredPlant: any[] = [];
  //configuration task
  getlistdata: authUser[] = [];
  getlistdatabyrole: authUser[] = [];
  getlistdetail: authUser[] = [];
  getaddlistdetail: authUser[] = [];
  subrolelist: subrolemodel[] = [];
  showeditdatamodel: boolean = false;
  showadddatamodel: boolean = false;
  showrolewisedatamodel: boolean = false;
  empid: string = "";
  empname: string = "";
  filteredroles: any[];

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.authUser = new authUser();
    this.authUser.employeeid = "";
    this.empid = "";
    this.empname = "";
    this.showeditdatamodel = false;
    this.showadddatamodel = false;
    this.showrolewisedatamodel = false;
    this.authUser.roleid = 0;
    this.authUser.createdby = this.employee.employeeno;
    this.getlistdetail = [];
    this.getlistdata = [];
    this.subrolelist = [];
    this.filteredroles = [];
    this.filteredEmployees = [];
    this.getlistdatabyrole = [];
    this.getEmployees();
    this.getRoles();
    this.getsubroles();
    this.getPlants();
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
      //alert();
    })
  }

  showRolewisedata() {
      this.showrolewisedatamodel = true;
  }

  getRolewisedata() {
    debugger;
    this.spinner.show();
    this.getlistdatabyrole = [];
    var role = this.selectedrolewise;
    var roleidd = 0;
    var data2 = this.roleNameModel.filter(function (element, index) {
      return (element.rolename == role);
    });
    if (data2.length > 0) {
      roleidd = data2[0].roleid;
    }
    this.wmsService.getuserauthdetailbyrole(roleidd).subscribe(data => {
      this.getlistdatabyrole = data;
      this.showrolewisedatamodel = true;
      this.spinner.hide();
    })
    
  }


  //get User List
  getUserAuth() {
    this.spinner.show();
    this.getlistdata = [];
    this.wmsService.getuserauthdata().subscribe(data => {
      this.getlistdata = data;
      this.spinner.hide();
    })
  }

  //get User List
  getsubroles() {
    this.spinner.show();
    this.subrolelist = [];
    this.wmsService.getsubrolelist().subscribe(data => {
      this.subrolelist = data;
      this.spinner.hide();
    })
  }

  //get User List
  getUserAuthdetail(empno: string, empname: string,plantid : string) {
    this.empid = empno;
    this.empname = empname;
    var data1 = this.plantmodel.filter(function (element, index) {
      return (element.plantname == plantid);
    });
    if (data1.length > 0) {
      this.selectedplant = data1[0];
    }

    this.spinner.show();
    this.getlistdetail = [];
    this.showeditdatamodel = true;
    this.wmsService.getuserauthdetail(empno).subscribe(data => {
      this.getlistdetail = data;
      this.spinner.hide();
    })
  }

  refresheditmodel() {
    this.empid = "";
    this.empname = "";
   
    this.getlistdetail = [];
    this.selectedplant = null;
    
  }
  refreshsavemodel() {
    this.getaddlistdetail = [];
    this.selectedEmployee = null;
    this.selectedplant = null;
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

  //get Role list
  getPlants() {
    this.spinner.show();
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.query = " select * from wms.wms_rd_plant wrp where deletedon is NULL";
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.plantmodel = data;
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

  filterroles(event) {
    debugger;
    this.filteredroles = [];
    for (let i = 0; i < this.roleNameModel.length; i++) {
      let brand = this.roleNameModel[i].rolename;
      if (brand.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredroles.push(brand);
      }

    }
  }
  refreshrolewisedatamodel() {
    this.selectedrolewise = null;
    this.getlistdatabyrole = [];

  }

  showAdd() {
    var newrw = new authUser();

    this.getaddlistdetail.push(newrw);
    this.showadddatamodel = true;
  }

  AddNewRow() {
    var newrw = new authUser();
    this.getaddlistdetail.push(newrw);
  }
  
  removerow(data: authUser, index: number) {
    this.getaddlistdetail.splice(index, 1);
  }
  AddNewRowedit() {
    var newrw = new authUser();
    this.getlistdetail.push(newrw);
  }
  removerowedit(data: authUser, index: number) {
    this.getlistdetail.splice(index, 1);
  }

  onroleselect(data: authUser, ind: number) {
    data.subrolelist = [];
    data.roleid = 0;
    var data1 = this.getaddlistdetail.filter(function (element, index) {
      return (element.rolename == data.rolename && index != ind);
    });
    if (data1.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: data.rolename + ' role already exist' });
      this.getaddlistdetail.splice(ind, 1);
      this.AddNewRow();
      return false;
    }
    var data2 = this.roleNameModel.filter(function (element, index) {
      return (element.rolename == data.rolename);
    });
    if (data2.length > 0) {
      data.roleid = data2[0].roleid;
      var data3 = this.subrolelist.filter(function (element, index) {
        return (element.roleid == data.roleid);
      });
      if (data3.length > 0) {
        data.subrolelist = data3;
      }
    }

  }

  oneditroleselect(data: authUser, ind: number) {
    data.subrolelist = [];
    data.roleid = 0;
    var data1 = this.getlistdetail.filter(function (element, index) {
      return (element.rolename == data.rolename && index != ind);
    });
    if (data1.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: data.rolename + ' role already exist' });
      this.getlistdetail.splice(ind, 1);
      this.AddNewRowedit();
      return false;
    }
    var data2 = this.roleNameModel.filter(function (element, index) {
      return (element.rolename == data.rolename);
    });
    if (data2.length > 0) {
      data.roleid = data2[0].roleid;
      var data3 = this.subrolelist.filter(function (element, index) {
        return (element.roleid == data.roleid);
      });
      if (data3.length > 0) {
        data.subrolelist = data3;
      }
    }

  }



  rolechange() {

  }

 

  submitAdd() {
    debugger;
    if (isNullOrUndefined(this.selectedEmployee)) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Employee' });
      return;
    }
    if (isNullOrUndefined(this.selectedplant)) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Plant' });
      return;
    }

    if (isNullOrUndefined(this.getaddlistdetail) || this.getaddlistdetail.length == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Add roles for the user' });
      return;
    }

    var data1 = this.getaddlistdetail.filter(function (element, index) {
      return (isNullOrUndefined(element.roleid) || element.roleid == 0 || isNullOrUndefined(element.rolename) || element.rolename.trim() == "");
    });
    if (data1.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Role' });
      return;
    }

    var data2 = this.getaddlistdetail.filter(function (element, index) {
      return (element.roleid == 5 && (isNullOrUndefined(element.selectedsubrolelist) || element.selectedsubrolelist.length == 0));
    });
    if (data2.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select sub-role for material requestor role' });
      return;
    }
   
    this.getaddlistdetail.forEach(item => {
      item.employeeid = this.selectedEmployee.employeeno;
      item.requesttype = "ADD";
      item.plantid = this.selectedplant.plantname;
      item.createdby = this.employee.employeeno;
    });
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

  SubmitEdit() {
    debugger;
    if (isNullOrUndefined(this.selectedplant)) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Plant' });
      return;
    }

    if (isNullOrUndefined(this.getlistdetail) || this.getlistdetail.length == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Add roles for the user' });
      return;
    }

    var data1 = this.getlistdetail.filter(function (element, index) {
      return (isNullOrUndefined(element.roleid) || element.roleid == 0 || isNullOrUndefined(element.rolename) || element.rolename.trim() == "");
    });
    if (data1.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Role' });
      return;
    }

    var data2 = this.getlistdetail.filter(function (element, index) {
      return (element.roleid == 5 && (isNullOrUndefined(element.selectedsubrolelist) || element.selectedsubrolelist.length == 0));
    });
    if (data2.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select sub-role for material requestor role' });
      return;
    }

    this.getlistdetail.forEach(item => {
      item.employeeid = this.empid;
      item.requesttype = "EDIT";
      item.plantid = this.selectedplant.plantname;
      item.modifiedby = this.employee.employeeno;
    });
    this.spinner.show();
    this.wmsService.AddAuthUser(this.getlistdetail).subscribe(data => {
      this.spinner.hide();
      if (String(data) == "saved") {
        this.getUserAuth();
        this.showeditdatamodel = false;
        this.messageService.add({ severity: 'success', summary: '', detail: 'Role Updated' });
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: String(data) });
      }
    })

  }
  deleteUserauth(data: authUser) {
    this.spinner.show();
    data.modifiedby = this.employee.employeeno;
    this.wmsService.deleteAuthUser(data).subscribe(data => {
      this.spinner.hide();
      if (String(data) == "deleted") {
        this.getUserAuth();
        this.messageService.add({ severity: 'success', summary: '', detail: 'User deleted' });
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: String(data) });
      }
    })

  }
  SubmitRolewiseEdit() {
    debugger;
   
    if (isNullOrUndefined(this.selectedrolewise) || this.selectedrolewise == "") {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select role' });
      return;
    }
    var senddata = this.getlistdatabyrole.filter(function (element, index) {
      return (element.isselected);
    });
    if (senddata.length == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select row' });
      return;
    }
    senddata.forEach(item => {
      item.modifiedby = this.employee.employeeno;
    });
   
    this.spinner.show();
    this.wmsService.AddAuthUser(senddata).subscribe(data => {
      this.spinner.hide();
      if (String(data) == "saved") {
        this.getUserAuth();
        this.getRolewisedata();
        this.messageService.add({ severity: 'success', summary: '', detail: 'Role Updated' });
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: String(data) });
      }
    })

  }

  CheckforEmpdata() {
    var empid = this.selectedEmployee.employeeno;
    var data2 = this.getlistdata.filter(function (element, index) {
      return (element.employeeid == empid);
    });
    if (data2.length > 0) {
      this.showadddatamodel = false;
      this.getUserAuthdetail(data2[0].employeeid, data2[0].employeename, data2[0].plantid)
    }

  }
  //Assign Roles
  assignRole() {
    if (this.selectedEmployee)
      this.authUser.employeeid = this.selectedEmployee.employeeno;
    if (this.authUser.employeeid && this.authUser.roleid) {
      if (this.authUsersList && this.authUsersList.length > 0 && (this.authUsersList.filter(li => li.employeeid == this.authUser.employeeid && li.roleid == this.authUser.roleid).length > 0)) {
        this.messageService.add({ severity: 'error', summary: 'Validation', detail: 'Selected Role already added for this Employee' });
        return false;
      }
      this.spinner.show();
      //this.authUser.employeeid = '"' + this.authUser.employeeid + '"';
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

