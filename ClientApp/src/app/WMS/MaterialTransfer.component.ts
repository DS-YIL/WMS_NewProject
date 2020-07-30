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
  selector: 'app-MaterialTransfer',
  templateUrl: './MaterialTransfer.component.html'
})
export class MaterialTransferComponent implements OnInit {
  AddDialog: boolean;
  showdialog: boolean;
  public materiallistData: Array<any> = [];

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
  //ackStatusChanges(status) {
  //  if (status == 'Approved') {
  //    this.showAck = false;
  //  }
  //  else {
  //    this.showAck = true;
  //  }
  //}

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
  showmaterialdetails(requestid, rowindex) {
    this.rowindex = rowindex
    this.AddDialog = true;
this.showdialog=true;
    this.wmsService.getmaterialissueList(requestid).subscribe(data => {
      this.materiallistData = data;
      
      if (data != null) {

      }
    });
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
  public bindSearchListData(event: any, name?: string) {
    var searchTxt = event.query;
    if (searchTxt == undefined)
      searchTxt = "";
    searchTxt = searchTxt.replace('*', '%');
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.tableName = this.constants[name].tableName + " ";
    this.dynamicData.searchCondition = "" + this.constants[name].condition;
    this.dynamicData.searchCondition += "sk.materialid" + " ilike '" + searchTxt + "%'" + " and sk.availableqty>=1";
   // this.materialistModel.materialcost = "";
    this.wmsService.GetMaterialItems(this.dynamicData).subscribe(data => {
      this.searchresult = data;
      this.searchItems = [];
      var fName = "";
      this.searchresult.forEach(item => {
        fName = item[this.constants[name].fieldName];
        if (name == "ItemId")
          //fName = item[this.constants[name].fieldName] + " - " + item[this.constants[name].fieldId];
          fName = item[this.constants[name].fieldId];
        var value = { listName: name, name: fName, code: item[this.constants[name].fieldId] };
       // this.materialistModel.materialcost = data[0].materialcost;
        this.searchItems.push(value);
      });
    });
  }
  //Check if material is already selected in material list drop down
  onMaterialSelected(material: any) {
    //if (this.gatepassModel.materialList.filter(li => li.materialid == material.code).length > 0) {
    //  this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Material already exist' });
    //  return false;
    //}
  }
  transferqty() {
    this.btnDisabletransfer = true;
  }
}
