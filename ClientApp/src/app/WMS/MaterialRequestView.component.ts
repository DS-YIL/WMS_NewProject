import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl, ValidatorFn } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { materialRequestDetails } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { RadioButtonModule } from 'primeng/radiobutton';
@Component({
  selector: 'app-MaterialRequest',
  templateUrl: './MaterialRequestView.component.html'
})
export class MaterialRequestViewComponent implements OnInit {
  AddDialog: boolean;
  showdialog: boolean;

  public materiallistData: Array<any> = [];
  public materiallistDataHistory: Array<any> = [];
    AddDialogfortransfer: boolean;
    showdialogfortransfer: boolean;
    projectcodes: string;
    showhistory: boolean=false;

  constructor(private formBuilder: FormBuilder, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public requestList: Array<any> = [];
  public employee: Employee;
  public displayItemRequestDialog; RequestDetailsSubmitted; showAck; btnDisable: boolean = false;
  public materialRequestDetails: materialRequestDetails;
  public pono: string;
  public rowindex: number;
  public dynamicData = new DynamicSearchResult();
  public searchItems: Array<searchList> = [];
  public selectedlist: Array<searchList> = [];
  public searchresult: Array<object> = [];
  public btnDisabletransfer: boolean = false;
  public locationlist: any[] = [];
  public chkChangeshideshow: boolean = false;
  public requestid: any;
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
   

    this.route.params.subscribe(params => {
      if (params["pono"]) {
        this.pono = params["pono"];
      }
    });

    this.getMaterialRequestlist();
  }

  //get Material Rquest based on login employee && po no
  getMaterialRequestlist() {
    //this.employee.employeeno = "180129";
    this.wmsService.getMaterialRequestlist(this.employee.employeeno, this.pono).subscribe(data => {
      this.requestList = data;
      this.requestList.forEach(item => {
        if (!item.requestedquantity)
          item.requestedquantity = item.quotationqty;
      });
    });
  }

  //check validations for requested quantity
  reqQtyChange(data: any) {
    if (data.requestedquantity > data.quotationqty) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Requested Quantity should be lessthan or equal to po quantity' });
      data.requestedquantity = data.quotationqty;
    }
  }

  //requested quantity update
  onMaterialRequestDeatilsSubmit() {
    this.spinner.show();
    this.btnDisable = true;
    this.wmsService.materialRequestUpdate(this.requestList).subscribe(data => {
      this.spinner.hide();
      if (data)
        this.messageService.add({ severity: 'success', summary: 'success Message', detail: 'Request sent' });
      else
        this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Update Failed' });

    });
  }

  //app
  ackStatusChanges(status) {
    if (status == 'Approved') {
      this.showAck = false;
    }
    else {
      this.showAck = true;
    }
  }

  //received material acknowledgement
  materialAckUpdate() {
    if (this.requestList.filter(li => li.status == true).length == 0) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Select atleast  one checkbox' });
    }
    else {
      this.spinner.show();
      this.btnDisable = true;
      this.wmsService.ackmaterialreceived(this.requestList).subscribe(data => {
        this.spinner.hide();
        if (data)
          this.messageService.add({ severity: 'sucess', summary: 'sucee Message', detail: 'acknowledged' });
        else
          this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'acknowledge failed' });
      });
    }
  }

  //redirect to PM Dashboard
  backtoDashboard() {
    this.router.navigateByUrl("/WMS/Dashboard");
  }
  selectrow(requesid) {
    this.requestid = requesid;

  }
  showmaterialdetails() {
    if (this.requestid == undefined) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Please select any Request Id' });
      //this.router.navigateByUrl("/WMS/MaterialReqView");
    }
    else {


      //this.rowindex = rowindex
      this.AddDialog = true;
      this.showdialog = true;
      //this.materiallistData = this.requestList.filter(li => li.approvedstatus == 'Approved');
      this.wmsService.getmaterialissueList(this.requestid).subscribe(data => {
        this.materiallistData = data;

        if (data != null) {

        }
      });
    }
  }
  showmaterialdetailsfortransfer() {
    if (this.requestid == undefined) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Please select  Request Id' });
      //this.router.navigateByUrl("/WMS/MaterialReqView");
    }
    else {

      this.bindSearchListData();
      //this.rowindex = rowindex
      this.AddDialogfortransfer = true;
      this.showdialogfortransfer = true;
      //this.materiallistData = this.requestList.filter(li => li.approvedstatus == 'Approved');
      this.wmsService.getmaterialissueList(this.requestid).subscribe(data => {
        this.materiallistData = data;

        if (data != null) {

        }
      });
    }
  }
  Cancel() {
    this.AddDialog = false;
  }
  returnqty() {
   
    //this.requestList;
    //var totalreturnqty = 0;
    //this.materiallistData.forEach(item => {
    //  if (item.returnqty != 0) {
    //    totalreturnqty = totalreturnqty + (item.issuedquantity);
    //    this.requestList[this.rowindex].returnqty = totalreturnqty;
    //  }
    //})
    this.materiallistData.forEach(item => {
      this.materiallistData[item].rquesttype = "return";
    }
    );
      this.wmsService.UpdateReturnqty(this.materiallistData).subscribe(data => {
        if (data == 1) {
          this.btnDisable = true;
          this.AddDialog = false;
          this.messageService.add({ severity: 'sucess', summary: 'suceess Message', detail: 'Material Returned' });
        }
    })

  }
  returnQtyChange(issuesqty,returnqty) {
    if (returnqty > issuesqty) {
      this.btnDisable = true;
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Return Quantity should be lessthan or equal to Issued quantity' });

    }
    else {
      this.btnDisable = false;
    }
  }
  //bind materials based search
  public bindSearchListData() {
    this.dynamicData.tableName ="wms.wms_project";
   this.dynamicData.searchCondition = "";
    this.wmsService.GetListItems(this.dynamicData).subscribe(res => {

      
        //this._list = res; //save posts in array
        this.locationlist = res;
        let _list: any[] = [];
        for (let i = 0; i < (res.length); i++) {
          _list.push({
            projectcode: res[i].projectcode,
           // projectcode: res[i].projectcode
          });
        }
        this.locationlist = _list;
      });
  }
 
  showstory(requestid){
   this.showhistory = true;
   this.wmsService.getmaterialissueList(requestid).subscribe(data => {
     this.materiallistDataHistory = data;

      if (data != null) {

      }
    });
  }
  transferqty() {

    this.materiallistData.forEach(item => {
      this.materiallistData[item].rquesttype = "transfer";
    }
    );
    this.wmsService.UpdateReturnqty(this.materiallistData).subscribe(data => {

    })
  }
  onChange(value, indexid:any) {
    console.log(event);
    this.materiallistData[indexid].projectname = value;
   // (<HTMLInputElement>document.getElementById(indexid)).value = event.toString();
  }
  toggleVisibility(e,index) {
    if (e.checked == true) {
      this.chkChangeshideshow = true;
      document.getElementById(index).style.display = "block";
    }
    else {
      document.getElementById(index).style.display = "none";
    }
  }
}
