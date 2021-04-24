import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl, ValidatorFn } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { miscellanousIssueData, ddlmodel } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { isNullOrUndefined } from 'util';

@Component({
  selector: 'app-MiscellanousIssues',
  templateUrl: './MiscellanousIssues.component.html'
})
export class MiscellanousIssueComponent implements OnInit {

  constructor(private formBuilder: FormBuilder, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public IssueList: Array<any> = [];
  public IssueHistoryList: Array<any> = [];
  public ReasonList: Array<any> = [];
  public employee: Employee;
  public displayIssueDialog: boolean = false;
  public initialStock: boolean = true;
  public MisData: miscellanousIssueData;
  public saveMisData: miscellanousIssueData[] = [];
  public dynamicData: DynamicSearchResult;
  projectlists: ddlmodel[] = [];
  selectedprojectmodel: ddlmodel;
  filteredproject: ddlmodel[] = [];
  polist: any[] = [];
  selectedponomodel: any;
  filteredpono: any[] = [];
  issuereason: string = "";
  ishistoryview: boolean = false;

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.MisData = new miscellanousIssueData();
    this.ishistoryview = false;
    this.projectlists = [];
    this.IssueHistoryList = [];
    this.saveMisData = [];
    this.filteredproject = [];
    this.selectedprojectmodel = new ddlmodel();
    this.polist = [];
    this.filteredpono = [];
    this.selectedponomodel = null;
    this.issuereason = "";
    this.getprojects();
    this.getMisIssueReasonList();
    this.getMiscellanousIssueListhistory();
    //this.getMiscellanousIssueList();
  }

  //get material details by materialid
  getMisIssueReasonList() {
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.query = "select * from wms.RD_Reason  where ReasonType='MiscellaonousIssue' and deleteflag =false";
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

  createmiscissue() {
    this.ishistoryview = false;
  }
  viewhistory() {
    this.ishistoryview = true;
  }
  getMiscellanousIssueListhistory() {
    this.IssueHistoryList = [];
    this.spinner.show();
    this.wmsService.getMiscellanousIssueListdatahistory().subscribe(data => {
      this.spinner.hide();
      this.IssueHistoryList = data;
    });
  }
  getprojects() {
    this.spinner.show();
    this.wmsService.getprojectlist().subscribe(data => {
      debugger;
      this.projectlists = data;
      this.spinner.hide();
    });
  }
  GetPONo(projectcode: string) {
    this.wmsService.getPODetailsbyprojectcodeformisc(projectcode).subscribe(data => {
      this.spinner.hide();
      if (data) {
        this.polist = data;
        if (this.polist.length == 0) {
          this.messageService.add({ severity: 'error', summary: '', detail: 'PO not available in stock.' });
        }
      }
      else
        this.messageService.add({ severity: 'error', summary: '', detail: 'Unable to fetch PO data' });
    });
  }

  onComplete(issued: number, avail: number, rowdata: any) {
    if (issued < 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Negative number not allowed' });
      rowdata.issuedqty = 0;
      return;
    }
    if (issued > avail) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Issue quantity exceeded available quantity' });
      rowdata.issuedqty = 0;
      return;
    }

  }
  resetpage() {
    this.IssueList = [];
    this.selectedponomodel = null;
    this.selectedprojectmodel = null;
    this.issuereason = "";
    this.initialStock = true;
  }

  issuemiscellenious() {
    this.saveMisData = [];
    var invalidrcv = this.IssueList.filter(function (element, index) {
      return (!isNullOrUndefined(element.issuedqty) && element.issuedqty != 0 );
    });
    if (invalidrcv.length == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Issue quantity' });
      return;
    }
    var invalidrcv1 = this.IssueList.filter(function (element, index) {
      return (element.issuedqty > element.availableqty);
    });
    if (invalidrcv1.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Issue quanity can not exceed available quantity.' });
      return;
    }
    this.IssueList = this.IssueList.filter(function (element, index) {
      return (element.issuedqty > 0);
    });
    this.IssueList.forEach(item => {
      let dt = new miscellanousIssueData();
      dt.ProjectId = item.projectid;
      dt.pono = item.pono;
      dt.material = item.material;
      dt.materialdescription = item.poitemdescription;
      dt.itemlocation = item.itemlocation;
      dt.createdby = this.employee.employeeno;
      dt.issuedqty = item.issuedqty;
      this.saveMisData.push(dt);

    });
    this.wmsService.multiplemiscellanousIssueDataUpdate(this.saveMisData).subscribe(data => {
      this.spinner.hide();
      this.displayIssueDialog = false;
      if (String(data) == "saved") {
        this.messageService.add({ severity: 'success', summary: '', detail: 'Issued' });
        this.resetpage();
        this.getMiscellanousIssueListhistory();
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Issue Failed' });
      }

    });

  }
  projectSelected(event: any) {
    debugger;
    this.filteredpono = [];
    this.IssueList = [];
    this.selectedponomodel = null;
    this.polist = [];
    var prj = this.selectedprojectmodel.value;
    this.GetPONo(prj);
  }
  filterprojects(event) {
    this.filteredproject = [];
    for (let i = 0; i < this.projectlists.length; i++) {
      let brand = this.projectlists[i].value;
      let pos = this.projectlists[i].text;
      if (brand.toLowerCase().indexOf(event.query.toLowerCase()) == 0 || pos.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredproject.push(this.projectlists[i]);
      }
    }
  }
  filterpos(event) {
    debugger;
    if (isNullOrUndefined(this.selectedprojectmodel)) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Project' });
      return;
    }
    this.filteredpono = [];
    for (let i = 0; i < this.polist.length; i++) {
      let pos = this.polist[i].pono;
      if (pos.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredpono.push(pos);
      }

    }
  }

  onPOSelected() {
    this.spinner.show();
    this.IssueList = [];
    this.wmsService.getMiscellanousIssueListwithfilter(this.initialStock, this.selectedponomodel, this.selectedprojectmodel.value).subscribe(data => {
      this.spinner.hide();
      this.IssueList = data;
      if (this.IssueList.length == 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Material not available in stock.' });
      }
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
