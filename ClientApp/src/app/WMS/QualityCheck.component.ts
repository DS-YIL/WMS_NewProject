import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { PoDetails, BarcodeModel, inwardModel } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { first } from 'rxjs/operators';
import { isNullOrUndefined } from 'util';

@Component({
  selector: 'app-QualityCheck',
  templateUrl: './QualityCheck.component.html'
})
export class QualityCheckComponent implements OnInit {

  constructor(private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public PoDetails: PoDetails;
  public podetailsList: Array<inwardModel> = [];
  public employee: Employee;
  public showDetails; showQtyUpdateDialog: boolean = false;
  public disGrnBtn: boolean = true;
  public BarcodeModel: BarcodeModel;
  public inwardModel: inwardModel;
  public grnnumber: string = "";
  public totalqty: number;
  public recqty: number;
  qualitychecked: boolean = false;
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.PoDetails = new PoDetails();
    this.inwardModel = new inwardModel();
    this.inwardModel.quality = "0";
    this.inwardModel.receiveddate = new Date();
    this.inwardModel.qcdate = new Date();
    this.getqualitydetails(this.PoDetails.pono);
  }
  
  checkconfirmqty(entredvalue, receivedqty, returnedqty) {
    debugger;
    if (entredvalue > receivedqty) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Please enter passed quantity less than or equal to received quantity' });
      (<HTMLInputElement>document.getElementById("confirmqty")).value = "";
    }
    if (entredvalue != (receivedqty - returnedqty) && receivedqty && returnedqty) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'sum of  quality passesed and failed must be equal to received qty' });
      (<HTMLInputElement>document.getElementById("confirmqty")).value = "";
    }
  }
  checkreturnqty(entredvalue,receivedqty,acceptedqty) {
    if (entredvalue > receivedqty) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Please enter failed quantity less than or equal to received quantity' });
      (<HTMLInputElement>document.getElementById("returnqty")).value = "";
    }
    if (entredvalue != (receivedqty - acceptedqty) && receivedqty && acceptedqty) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail:  'sum of quality passesed and failed  must be equal to received qty' });
      (<HTMLInputElement>document.getElementById("returnqty")).value = "";
    }
  }
  scanBarcode() {
    if (this.PoDetails.pono) {
      this.PoDetails.pono;
      this.PoDetails.invoiceno;
      this.spinner.show();
      //this.wmsService.verifythreewaymatch(this.PoDetails.pono).subscribe(data => {
      //  //this.wmsService.verifythreewaymatch("123", "228738234", "1", "SK19VASP8781").subscribe(data => {
      //this.spinner.hide();
      //  if (data == true) {
      this.showQtyUpdateDialog = true;
      this.getqualitydetails(this.PoDetails.pono);
      // this.messageService.add({ severity: 'success', summary: 'Success Message', detail: 'GRN Posted  Sucessfully' });
      // }
      //else
      //this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Already verified' });
      //this.getponodetails(this.PoDetails.pono);
      // })

    }


  }
  getponodetails(data) {
    this.podetailsList = [];
    this.wmsService.Getthreewaymatchingdetails(data).subscribe(data => {
      this.spinner.hide();
      if (data) {
        this.disGrnBtn = false;
        // this.PoDetails = data[0];
        this.podetailsList = data;
        this.grnnumber = this.podetailsList[0].grnnumber;
        this.showDetails = true;
      }
      else
        this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'No data' });
    })
  }

  getqualitydetails(data) {
    this.podetailsList = [];
    this.wmsService.Getdataforqualitycheck().subscribe(data => {
      this.spinner.hide();
      if (data) {
        debugger;
        this.disGrnBtn = false;
        // this.PoDetails = data[0];
        this.podetailsList = data;
        this.podetailsList = this.podetailsList.filter(function (element, index) {
          return (element.receivedqty > '0');
        });
        this.grnnumber = this.podetailsList[0].grnnumber;
        if (!this.grnnumber) {
          this.disGrnBtn = true;
        }
        if (this.podetailsList[0].checkedby) {
          this.disGrnBtn = true;
        }
        this.showDetails = true;
      }
      else
        this.messageService.add({ severity: 'eoor', summary: 'Message', detail: 'No materials for quality check' });
    })
  }

  onVerifyDetails(details: any) {
    this.spinner.show();
    this.PoDetails = details;
    this.inwardModel.grndate = new Date();
    this.wmsService.verifythreewaymatch(details).subscribe(data => {
      //this.wmsService.verifythreewaymatch("123", "228738234", "1", "SK19VASP8781").subscribe(data => {
      this.spinner.hide();
      if (data == true) {
        this.showQtyUpdateDialog = true;
      }
      else
        this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Verification failed' });
    })
  }

  quanityChange() {
    this.inwardModel.pendingqty = parseInt(this.PoDetails.materialqty) - this.inwardModel.confirmqty;
  }
  onsubmit() {

    

      this.spinner.show();
      // this.onVerifyDetails(this.podetailsList);
      this.inwardModel.pono = this.PoDetails.pono;
      this.inwardModel.receivedqty = this.PoDetails.materialqty;
      this.inwardModel.receivedby = this.inwardModel.qcby = this.employee.employeeno;
      this.podetailsList.forEach(item => {
        item.receivedby = this.employee.employeeno;
      });
      this.recqty = this.podetailsList[0].confirmqty + this.podetailsList[0].returnqty;
      this.totalqty = parseInt(this.podetailsList[0].receivedqty);
      var savedata = this.podetailsList.filter(function (element, index) {
        return (element.confirmqty != 0);
      });
        this.wmsService.insertqualitycheck(savedata).subscribe(data => {
          this.spinner.hide();
         
          if (data != null) {
           
          }
          if (data == null) {
            this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Something went wrong' });
          }

          if (data) {
            
            this.messageService.add({ severity: 'success', summary: 'Success Message', detail: 'Quality checked' });
            this.showQtyUpdateDialog = false;
            this.disGrnBtn = true;
            
          }
          this.getqualitydetails(this.PoDetails.pono);
        });
     
  
  }


}
