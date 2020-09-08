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
  selector: 'app-MaterialIsuueDashBoard',
  templateUrl: './MaterialIssueDashBoard.component.html'
})
export class MaterialIssueDashBoardComponent implements OnInit {
  selectedStatus: string;

  constructor(private formBuilder: FormBuilder, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

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
  public materialIssueListnofilter: Array<any> = [];
  public employee: Employee;
  public displayItemRequestDialog; RequestDetailsSubmitted: boolean = false;
  public materialRequestDetails: materialRequestDetails;

  ngOnInit() {
    this.materialIssueList = [];
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.selectedStatus = "Pending";
    this.getMaterialIssueList();

  }

  //get material issue list based on loginid
  getMaterialIssueList() {
    this.materialIssueList = [];
    //this.employee.employeeno = "400095";
    this.wmsService.getMaterialIssueLlist(this.employee.employeeno).subscribe(data => {
      this.materialIssueListnofilter = data;
      this.materialIssueList = this.materialIssueListnofilter.filter(li => li.approvedstatus == null);
    });
  }
  onSelectStatus(event) {
    this.selectedStatus = event.target.value;
    if (this.selectedStatus == "Pending") {
      this.materialIssueList = this.materialIssueListnofilter.filter(li => li.approvedstatus == null);
    }
    else if (this.selectedStatus == "Approved") {
      this.materialIssueList = this.materialIssueListnofilter.filter(li => li.approvedstatus == 'Approved');
    }
  }
  SubmitStatus() {
    if (this.selectedStatus == "Pending") {
      this.materialIssueList = this.materialIssueListnofilter.filter(li => li.approvedstatus == null);
    }
    else if (this.selectedStatus == "Approved") {
      this.materialIssueList = this.materialIssueListnofilter.filter(li => li.approvedstatus == 'Approved');
    }
  }

  //navigate to material issue page
  navigateToMatIssue(details: any) {
    if (!details.pono)
      details.pono = '';
    this.constants.materialIssueType = this.selectedStatus;
    this.router.navigate(["/WMS/MaterialIssue", details.requestid, details.pono]);
  }


}
