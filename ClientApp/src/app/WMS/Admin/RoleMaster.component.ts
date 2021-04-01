import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { wmsService } from '../../WmsServices/wms.service';
import { constants } from '../../Models/WMSConstants';
import { Employee, DynamicSearchResult } from '../../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { roleMaster } from '../../Models/WMS.Model';

@Component({
  selector: 'app-roleMaster',
  templateUrl: './RoleMaster.component.html'
})
export class RoleMasterComponent implements OnInit {

  constructor(private messageService: MessageService, private wmsService: wmsService, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public employee: Employee;
  public roleData = new roleMaster();
  public displayDialog: boolean = false;
  public roleList: Array<any> = [];
  public dynamicData: DynamicSearchResult;

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.getRolelist();
  }

  opengpDialogue() {
    this.roleData = new roleMaster();
    this.displayDialog = true;
  }

  dialogCancel() {
    this.displayDialog = false;
  }


  //Get the list of role names
  getRolelist() {
    this.spinner.show();
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.query = "select rm.*,emp.name as name from wms.rolemaster rm inner join wms.employee emp on emp.employeeno = rm.createdby where rm.deleteflag =false or rm.deleteflag is null order by roleid desc";
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.roleList = data;
      this.spinner.hide();
    })
  }

  onroleSubmit() {
    if (!this.roleData.rolename) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Enter Role Name' });
      return;
    }
    this.roleData.createdby = this.employee.employeeno;
    this.spinner.show();
    this.wmsService.updateRole(this.roleData).subscribe(data => {
      this.spinner.hide();
      this.displayDialog = false;
      if (data) {
        this.messageService.add({ severity: 'success', summary: '', detail: 'Role updated successfully' });
        this.getRolelist();
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Error while updating role' });
      }

    });
  }

  editRole(data: any) {
    this.displayDialog = true;
    this.roleData = data;
  }

}
