import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { wmsService } from '../../WmsServices/wms.service';
import { constants } from '../../Models/WMSConstants';
import { Employee, DynamicSearchResult } from '../../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { roleMaster, userRoles } from '../../Models/WMS.Model';

@Component({
  selector: 'app-UserRoleMaster',
  templateUrl: './UserRoleMaster.component.html'
})
export class UserRoleMasterComponent implements OnInit {

  constructor(private messageService: MessageService, private wmsService: wmsService, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public employee: Employee;
  public roleData = new userRoles();
  public displayDialog: boolean = false;
  public userRoleList: Array<any> = [];
  public dynamicData: DynamicSearchResult;
  public RoleList: Array<any> = [];
  public roleid: number = 0;
  public accessname: string;

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.roleData = new userRoles();
    this.getuserRolelist();
    this.getRoles();
  }

  opengpDialogue() {
    this.roleData = new userRoles();
    this.roleid = 0;
    this.accessname = "";
    this.displayDialog = true;
  }

  dialogCancel() {
    this.displayDialog = false;
  }

  //Get the Roles
  getRoles() {
    this.spinner.show();
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.query = "select roleid, rolename from wms.rolemaster r where r.deleteflag = false or deleteflag is null order by roleid";
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.RoleList = data;
      this.spinner.hide();
    })
  }
  //Get the list of role names
  getuserRolelist() {
    this.spinner.show();
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.query = "Select u.*,rm.rolename,emp.name as name from wms.userroles u inner join wms.rolemaster rm on rm.roleid=u.roleid inner join wms.employee emp on emp.employeeno = u.createdby where u.deleteflag =false or u.deleteflag is null order by u.userid desc";
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.userRoleList = data;
      this.spinner.hide();
    })
  }

  onuserroleSubmit() {
    if (!this.roleid || this.roleid == 0) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Select Role' });
      return;
    }
    if (!this.accessname) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Enter Access Name' });
      return;
    }

    this.roleData.createdby = this.employee.employeeno;
    this.roleData.roleid = this.roleid;
    this.roleData.accessname = this.accessname;
    this.roleData.deleteflag = false;;
    this.spinner.show();
    this.wmsService.updateUserRole(this.roleData).subscribe(data => {
      this.spinner.hide();
      this.displayDialog = false;
      if (data) {
        this.messageService.add({ severity: 'success', summary: '', detail: 'User Role Updated successfully' });
        this.getuserRolelist();
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Error while updating userrole' });
      }
    });
  }

  edituserRole(data: any) {
    this.displayDialog = true;
    this.roleData = data;
    this.roleid = data.roleid;
    this.accessname = data.accessname;
  }
}
