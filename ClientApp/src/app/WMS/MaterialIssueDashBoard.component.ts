import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl, ValidatorFn } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { materialRequestDetails, Issuestatus } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { isNullOrUndefined } from 'util';

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
  public displayItemRequestDialog; RequestDetailsSubmitted; ShowPrint: boolean = false;
  public materialRequestDetails: materialRequestDetails;
  public materialissueDetailsList: Array<any> = [];
  public currentDate: Date;
  public requestedid: string = "";
  statusmodel: Issuestatus;
  remarksheadertext: string = "";
  displayRemarks: boolean = false;
  statusremarks: string = "";
  lblstatus: string = "";
  lblstatusramarks: string = "";
  lblonholdrejectedby: string = "";
  lblonholdrejectedon: string = "";

  ngOnInit() {
    debugger;
    this.materialIssueList = [];
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.currentDate = new Date();
    this.selectedStatus = "Pending";
    this.requestedid = this.route.snapshot.queryParams.requestid;
    this.getMaterialIssueList();
    this.displayRemarks = false;
  }

  //get material issue list based on loginid
  getMaterialIssueList() {
    debugger;
    this.materialIssueList = [];
    //this.employee.employeeno = "400095";
    this.wmsService.getMaterialIssueLlist(this.employee.employeeno).subscribe(data => {
      this.materialIssueListnofilter = data;
      debugger;
      this.materialIssueList = this.materialIssueListnofilter.filter(li => li.issuerstatus == "Pending" || li.issuerstatus == null);
      if (!isNullOrUndefined(this.requestedid) && this.requestedid != "") {
        this.materialIssueList = this.materialIssueList.filter(li => li.requestid == this.requestedid);
      }
    });
  }
  onSelectStatus(event) {
    this.selectedStatus = event.target.value;
    if (this.selectedStatus == "Pending") {
      this.materialIssueList = this.materialIssueListnofilter.filter(li => li.issuerstatus == this.selectedStatus || li.issuerstatus == null);
    }
    else if (this.selectedStatus == "Issued") {
      this.materialIssueList = this.materialIssueListnofilter.filter(li => li.issuerstatus == this.selectedStatus && li.issuedby == this.employee.employeeno);
    }
    else {
      this.materialIssueList = this.materialIssueListnofilter.filter(li => li.issuerstatus == this.selectedStatus);
    }


  }
  SubmitStatus() {
    if (this.selectedStatus == "Pending") {
      this.materialIssueList = this.materialIssueListnofilter.filter(li => li.issuerstatus == "Pending" || li.issuerstatus == null);
    }
    else if (this.selectedStatus == "Issued") {
      this.materialIssueList = this.materialIssueListnofilter.filter(li => li.issuerstatus == "Issued" && li.issuedby == this.employee.employeeno);
    }
  }

  //navigate to material issue page
  navigateToMatIssue(details: any) {
    if (!details.pono)
      details.pono = '';
    debugger;
    this.constants.materialIssueType = this.selectedStatus;
    this.router.navigate(["/WMS/MaterialIssue", details.requestid, details.pono]);
  }


  //
  PrintMaterialIssue(data: any) {
    if (!data.pono)
      data.pono = "";
    if (!data.requestid)
      data.requestid = "";
    this.wmsService.getmaterialIssueListbyrequestid(data.requestid, data.pono).subscribe(data => {
      this.spinner.hide();
      this.ShowPrint = true;
      this.materialissueDetailsList = data;
    });
  }

  //back to material issue view
  navigateToMatIssueView() {
    this.ShowPrint = false
  }
  holdreject(data: any, status: string) {
    this.statusmodel = new Issuestatus();
    this.statusmodel.requestid = data.requestid;
    this.statusmodel.issuerstatus = status;
    this.statusmodel.requestedby = data.requesterid;
    this.statusmodel.issuerstatuschangeby = this.employee.employeeno;
    this.statusmodel.type = "MaterialRequest";

    if (status == "On Hold") {
      this.remarksheadertext = "Are you sure to put request on hold ?";
    }
    if (status == "Rejected") {
      this.remarksheadertext = "Are you sure to reject the request ?";
    }
    this.displayRemarks = true;

  }
  submitstatus() {
    debugger;
    if (this.statusremarks.trim() == "") {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Remarks' });
      return;
    }
    this.statusmodel.issuerstatusremarks = this.statusremarks;
    var msg = "";
    var errormsg = "";
    if (this.statusmodel.issuerstatus == "On Hold") {
      msg = "On hold successful";
      errormsg = "On hold failed";
    }
    if (this.statusmodel.issuerstatus == "Rejected") {
      msg = "Rejection successful";
      errormsg = "Rejection failed";
    }
    this.wmsService.updateIssuerstatus(this.statusmodel).subscribe(data => {
      if (data) {
        this.messageService.add({ severity: 'success', summary: '', detail: msg });
        this.getMaterialIssueList();
      }
      else {
        this.messageService.add({ severity: 'success', summary: '', detail: errormsg });
      }
      this.canclestatus();

    });

  }
  canclestatus() {
    this.remarksheadertext = "";
    this.displayRemarks = false;
    this.statusremarks = "";
    this.statusmodel = new Issuestatus();
  }
}
