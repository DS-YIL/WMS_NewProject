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
  selector: 'app-MaterialRequest',
  templateUrl: './MaterialRequest.component.html'
})
export class MaterialRequestComponent implements OnInit {

  constructor(private formBuilder: FormBuilder, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public requestList: Array<any> = [];
  public employee: Employee;
  public displayItemRequestDialog; RequestDetailsSubmitted; showAck; btnDisable: boolean = false;
  public materialRequestDetails: materialRequestDetails;
  public pono: string;
  //Email
  reqid: string=""

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

    //Email
    this.reqid = this.route.snapshot.queryParams.requestid;
    if (this.reqid) {
      debugger;
      //get material details for that requestid
      this.requestList[0].material = this.reqid;
      this.getMaterialRequestlist();

    }

    this.getMaterialRequestlist();
  }

  //get Material Rquest based on login employee && po no
  getMaterialRequestlist() {
    //this.employee.employeeno = "180129";
    this.wmsService.getMaterialRequestlistdata(this.employee.employeeno, this.pono,null).subscribe(data => {
      this.requestList = data;
      this.requestList.forEach(item => {
        //if (!item.requestedquantity)
        //  item.requestedquantity = item.availableqty;
      });
    });
  }

  //check validations for requested quantity
  reqQtyChange(data: any) {
    if (data.requestedquantity > data.materialqty) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Requested Quantity should be lessthan or equal to material quantity' });
      data.requestedquantity = data.materialqty;
    }
  }

  //requested quantity update
  onMaterialRequestDeatilsSubmit() {
    this.spinner.show();
    this.btnDisable = true;
    this.requestList.forEach(item => {
      item.requesterid = this.employee.employeeno;
      if (item.requestedquantity == null)
        item.requestedquantity = 0;
    })
    this.wmsService.materialRequestUpdate(this.requestList).subscribe(data => {
      this.spinner.hide();
      if (data) {
        this.messageService.add({ severity: 'success', summary: '', detail: 'Request sent' });
        this.router.navigateByUrl("/WMS/MaterialReqView/"+ this.pono);
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Update Failed' });
      }

    });
  }

  //app
  ackStatusChanges() {
    this.showAck = true;
  }

  //received material acknowledgement
  materialAckUpdate() {
    if (this.requestList.filter(li => li.status == true).length == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select atleast  one checkbox' });
    }
    else {
      this.spinner.show();
      this.btnDisable = true;
      //this.wmsService.approvematerialrequest(this.requestList).subscribe(data => {
      this.wmsService.ackmaterialreceived(this.requestList).subscribe(data => {
        this.spinner.hide();
        if (data)
          this.messageService.add({ severity: 'sucess', summary: '', detail: 'Status updated' });
        else
          this.messageService.add({ severity: 'error', summary: '', detail: 'Update Failed' });
      });
    }
  }

  //redirect to PM Dashboard
  backtoDashboard() {
    this.router.navigateByUrl("/WMS/Dashboard");
  }
}
