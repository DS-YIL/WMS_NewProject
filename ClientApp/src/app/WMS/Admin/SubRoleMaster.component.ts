import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { wmsService } from '../../WmsServices/wms.service';
import { constants } from '../../Models/WMSConstants';
import { Employee, DynamicSearchResult } from '../../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { roleMaster } from '../../Models/WMS.Model';

@Component({
  selector: 'app-SubRoleMaster',
  templateUrl: './SubRoleMaster.component.html'
})
export class SubRoleMasterComponent implements OnInit {

  constructor(private messageService: MessageService, private wmsService: wmsService, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public employee: Employee;
  public roleData = new roleMaster();
  public displayDialog: boolean = false;
  public subRoleList: Array<any> = [];
  public dynamicData: DynamicSearchResult;
  public RoleList: Array<any> = [];
  public roleid: number = 0;
  public subrolename: string;
  public deleteflag: boolean;

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.roleData = new roleMaster();
    this.getSubRolelist();
    this.getRoles();
  }

  opengpDialogue() {
    this.roleData = new roleMaster();
    this.roleid = 0;
    this.subrolename = "";
    this.deleteflag = true;
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
  getSubRolelist() {
    this.spinner.show();
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.query = "select srm.*,rm.rolename ,emp.name as name from wms.subrolemaster srm  inner join wms.rolemaster rm on rm.roleid=srm.roleid inner join wms.employee emp on emp.employeeno = srm.createdby  order by srm.subroleid desc ";
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.subRoleList = data;
      this.spinner.hide();
    })
  }

  onsubroleSubmit() {
    if (!this.roleid || this.roleid == 0) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Select Role' });
      return;
    }
    if (!this.subrolename) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Enter Sub Role' });
      return;
    }
    this.roleData.createdby = this.employee.employeeno;
    this.roleData.roleid = this.roleid;
    this.roleData.subrolename = this.subrolename;
    this.roleData.deleteflag = !this.deleteflag;
    this.spinner.show();
    this.wmsService.updateSubRole(this.roleData).subscribe(data => {
      this.spinner.hide();
      this.displayDialog = false;
      if (data) {
        this.messageService.add({ severity: 'success', summary: '', detail: 'Sub Role Updated successfully' });
        this.getSubRolelist();
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Error while updating sub role' });
      }

    });
  }

  editSubRole(data: any) {
    this.roleData = new roleMaster();
    this.displayDialog = true;
    this.roleData = data;
    this.roleid = data.roleid;
    this.subrolename = data.subrolename;
    this.deleteflag = !data.deleteflag;
  }

}
