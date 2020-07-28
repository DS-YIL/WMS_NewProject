import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl, ValidatorFn } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { materialRequestDetails } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-MaterialReturnDashBoard',
  templateUrl: './MaterialReturnDashBoard.component.html'
})
export class MaterialReturnDashBoardComponent implements OnInit {

  constructor(private formBuilder: FormBuilder, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }
  AddDialog: boolean;
  showdialog: boolean;
  public materiallistData: Array<any> = [];
  public formName: string;
  public txtName: string;
  public dynamicData = new DynamicSearchResult();
  public showList: boolean = false;
  public searchItems: Array<searchList> = [];
  public selectedlist: Array<searchList> = [];
  public selectedItem: searchList;
  public searchresult: Array<object> = [];

  public MaterialRequestForm: FormGroup
  public materialIssueList: Array<any> = [];
  public employee: Employee;
  public displayItemRequestDialog; RequestDetailsSubmitted: boolean = false;
  public materialRequestDetails: materialRequestDetails;

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.getMaterialIssueList();

  }

  //get material issue list based on loginid
  getMaterialIssueList() {
    //this.employee.employeeno = "400095";
    this.wmsService.getMaterialIssueLlist(this.employee.employeeno).subscribe(data => {
      this.materialIssueList = data.filter(li=>li.requesttype=='return');
     
    });
  }
  showmaterialdetails(requestid) {
    //this.rowindex = rowindex
    this.AddDialog = true;
    this.showdialog = true;
    this.wmsService.getmaterialissueList(requestid).subscribe(data => {
      this.materiallistData = data;

      if (data != null) {

      }
    });
  }
  ConfirmReturnmaterial() {
    this.wmsService.UpdateReturnmaterialTostock(this.materiallistData).subscribe(data => {
      this.AddDialog = false;
      if (data == 1) {
        this.messageService.add({ severity: 'success', summary: 'success Message', detail: 'Items updated to Store' });
      }

    })
  }
}
