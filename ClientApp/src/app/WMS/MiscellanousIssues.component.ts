import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl, ValidatorFn } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { miscellanousIssueData } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-MiscellanousIssues',
  templateUrl: './MiscellanousIssues.component.html'
})
export class MiscellanousIssueComponent implements OnInit {

  constructor(private formBuilder: FormBuilder, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public IssueList: Array<any> = [];
  public ReasonList: Array<any> = [];
  public employee: Employee;
  public displayIssueDialog: boolean = false;
  public initialStock: boolean = true;
  public MisData: miscellanousIssueData;
  public dynamicData: DynamicSearchResult;

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.MisData = new miscellanousIssueData();
    this.getMisIssueReasonList();
    this.getMiscellanousIssueList();
  }

  //get material details by materialid
  getMisIssueReasonList() {
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.query = "select * from wms.MisIssueReason where deleteflag =false";
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.ReasonList = data;
    });
  }

  //get Material list
  getMiscellanousIssueList() {
    this.spinner.show();
    this.wmsService.getMiscellanousIssueList(this.initialStock).subscribe(data => {
      this.spinner.hide();
      this.IssueList = data;
    });
  }

  //check validations for requested quantity
  showIssueDialog(data: any) {
    this.displayIssueDialog = true;
    this.MisData = new miscellanousIssueData();
    this.MisData.material = data.material;
    this.MisData.poitemdescription = data.poitemdescription;
    this.MisData.availableqty = data.availableqty;
    this.MisData.itemid = data.itemid;
    this.MisData.Reason = "";
  }

  //check qty
  checkQuantity() {
    if (this.MisData.MiscellanousIssueQty && this.MisData.MiscellanousIssueQty > this.MisData.availableqty) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Entered  Quantity should not be greater than  Availableqty ' });
      this.MisData.MiscellanousIssueQty = "";
      return;
    }
  }
  dialogCancel(dialog: any) {
    this[dialog] = false;
  }
  //update quantity 
  onQunatitySubmit() {
    if (!this.MisData.MiscellanousIssueQty) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Enter Miscellanous Issue Qty' });
      return;
    }
    if (!this.MisData.Reason) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Select Reason' });
      return;
    }
    if (this.MisData.Reason == "2" && !this.MisData.ProjectId) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Enter Project Id' });
      return;
    }
    if (!this.MisData.Remarks) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Enter Remarks' });
      return;
    }
    this.MisData.createdby = this.employee.employeeno;
    this.spinner.show();
    this.wmsService.miscellanousIssueDataUpdate(this.MisData).subscribe(data => {
      this.spinner.hide();
      this.displayIssueDialog = false;
      if (data) {
        this.messageService.add({ severity: 'success', summary: '', detail: 'Quantity Issued' });
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Update Failed' });
      }

    });
  }


}
