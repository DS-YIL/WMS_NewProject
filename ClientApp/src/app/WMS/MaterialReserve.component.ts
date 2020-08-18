import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl, ValidatorFn } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { materialRequestDetails } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-MaterialRequest',
  templateUrl: './MaterialReserve.component.html',
  providers: [DatePipe]
})
export class MaterialReserveComponent implements OnInit {

  constructor(private formBuilder: FormBuilder, private messageService: MessageService, private datePipe: DatePipe,  private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public reserveList: Array<any> = [];
  public employee: Employee;
  public displayItemRequestDialog; RequestDetailsSubmitted; showAck; btnDisable: boolean = false;
  public materialRequestDetails: materialRequestDetails;
  public pono: string;
  public reservedfor: Date;
  public mindate: string;
  public maxdate: string;

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
    this.wmsService.getMaterialRequestlistdata(this.employee.employeeno, this.pono).subscribe(data => {
      var minDate = new Date();
      var maxdate = new Date(new Date().setDate(new Date().getDate() + 14));
      this.reservedfor = new Date();
      this.mindate = this.datePipe.transform(minDate, "yyyy-MM-dd");
      this.maxdate = this.datePipe.transform(maxdate, "yyyy-MM-dd");
      this.reserveList = data;
      this.reserveList.forEach(item => {
        //if (!item.requestedquantity)
        //  item.reservedqty = item.availableqty;
      });
    });
  }

  //check validations for requested quantity
  reqQtyChange(data: any) {
    if (data.requestedquantity > data.materialqty) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Requested Quantity should be lessthan or equal to material quantity' });
      data.reservedqty = data.materialqty;
    }
  }

  //requested quantity update
  onMaterialRequestDeatilsSubmit() {
    debugger;
    if (!this.reservedfor) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Please select Reserve for' });
      return;
    }
    this.spinner.show();
    this.btnDisable = true;
    this.reserveList.forEach(item => {
      debugger;
      item.reservedby = this.employee.employeeno;
      item.ReserveUpto = this.reservedfor;
    })
    this.wmsService.materialReserveUpdate(this.reserveList).subscribe(data => {
      this.spinner.hide();
      if (data) {
        this.messageService.add({ severity: 'success', summary: '', detail: 'material Reserved' });
        this.router.navigateByUrl("/WMS/MaterialReserveView");
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Reserve Failed' });
      }

    });
  }

  //app
  ackStatusChanges() {
    this.showAck = true;
  }

  //received material acknowledgement
  materialAckUpdate() {
    if (this.reserveList.filter(li => li.status == true).length == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select atleast  one checkbox' });
    }
    else {
      this.spinner.show();
      this.btnDisable = true;
      //this.wmsService.approvematerialrequest(this.requestList).subscribe(data => {
      this.wmsService.ackmaterialreceived(this.reserveList).subscribe(data => {
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

  parseDate(dateString: string): Date {
    if (dateString) {
      return new Date(dateString);
    }
    return null;
  }
}
