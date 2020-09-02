import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { PoDetails, BarcodeModel, inwardModel, ddlmodel } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { first } from 'rxjs/operators';
import { isNullOrUndefined } from 'util';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-QualityCheck',
  templateUrl: './QualityCheck.component.html',
  providers: [DatePipe]
})
export class QualityCheckComponent implements OnInit {

  constructor(private messageService: MessageService, private datePipe: DatePipe, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

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
  selectedgrnno: string = "";
  filteredgrns: any[];
  checkedgrnlistqc: ddlmodel[] = [];
  fromdateview: string = "";
  todateview: string = "";
  fromdateview1: string = "";
  todateview1: string = "";
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
   // this.getqualitydetails();
    this.getcheckedgrnforqc();
  }
  
  checkconfirmqty(entredvalue, receivedqty, returnedqty,data :any) {
    debugger;
    if (entredvalue < 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Negative number not allowed' });
      data.qualitypassedqty = "";
      return;
      //(<HTMLInputElement>document.getElementById("confirmqty")).value = "";
    }
    if (entredvalue > receivedqty) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Quality Passed should be less than or equal to received quantity' });
      data.qualitypassedqty = "";
      return;
      //(<HTMLInputElement>document.getElementById("confirmqty")).value = "";
    }
    if (entredvalue != (receivedqty - returnedqty) && receivedqty && returnedqty) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Quality Passed & Failed should be equal to Recived Quantity' });
      //(<HTMLInputElement>document.getElementById("confirmqty")).value = "";
      data.qualitypassedqty = "";
      return;
    }
  }
  checkreturnqty(entredvalue, receivedqty, acceptedqty, data: any) {
    if (entredvalue < 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Negative number not allowed' });
      data.qualityfailedqty = "";
      return;
      //(<HTMLInputElement>document.getElementById("confirmqty")).value = "";
    }
    if (entredvalue > receivedqty) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Quality failed should be less than or equal to received quantity' });
      data.qualityfailedqty = "";
      return;
      //(<HTMLInputElement>document.getElementById("returnqty")).value = "";
    }
    if (entredvalue != (receivedqty - acceptedqty) && receivedqty && acceptedqty) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Quality Passed & Failed should be equal to Recived Quantity' });
      data.qualityfailedqty = "";
      return;
      //(<HTMLInputElement>document.getElementById("returnqty")).value = "";
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
      this.getqualitydetails();
      // this.messageService.add({ severity: 'success', summary: 'Success Message', detail: 'GRN Posted  Sucessfully' });
      // }
      //else
      //this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Already verified' });
      //this.getponodetails(this.PoDetails.pono);
      // })

    }


  }

  SearchGRNNo() {

  }

  onfromSelectMethod(event) {
    this.podetailsList = [];
    this.selectedgrnno = "";
    this.fromdateview1 = "";
    if (event.toString().trim() !== '') {
      this.fromdateview1 = this.datePipe.transform(event, 'yyyy-MM-dd');

      this.fromdateview1 += " 00:00:00";
      this.getcheckedgrnforqcbydate();
    }
  }
  ontoSelectMethod(event) {
    this.podetailsList = [];
    this.todateview1 = "";
    this.selectedgrnno = "";
    if (event.toString().trim() !== '') {
      this.todateview1 = this.datePipe.transform(event, 'yyyy-MM-dd');
      this.todateview1 += " 11:59:59";
      this.getcheckedgrnforqcbydate();

    }
  }

  getcheckedgrnforqc() {
    this.spinner.show();
    this.checkedgrnlistqc = [];
    this.wmsService.getcheckedgrnlistforqc().subscribe(data => {
      debugger;
      this.checkedgrnlistqc = data;
      if (this.checkedgrnlistqc.length > 0) {
        var len = this.checkedgrnlistqc.length;
        this.fromdateview = this.datePipe.transform(this.checkedgrnlistqc[len - 1].receiveddate, this.constants.dateFormat);
        this.todateview = this.datePipe.transform(this.checkedgrnlistqc[0].receiveddate, this.constants.dateFormat);
        this.fromdateview1 = this.datePipe.transform(this.checkedgrnlistqc[len - 1].receiveddate, 'yyyy-MM-dd');
        this.fromdateview1 += " 00:00:00";
        this.todateview1 = this.datePipe.transform(this.checkedgrnlistqc[0].receiveddate, 'yyyy-MM-dd');
        this.todateview1 += " 23:59:59";
      }
      this.spinner.hide();
    });
  }

  getcheckedgrnforqcbydate() {
    if (this.fromdateview1 != "" && this.todateview1 != "") {
      this.spinner.show();
      this.checkedgrnlistqc = [];
      this.wmsService.getcheckedgrnlistforqcbydate(this.fromdateview1, this.todateview1).subscribe(data => {
        debugger;
        this.checkedgrnlistqc = data;
        this.spinner.hide();
      });

    }
    
  }

  filtergrn(event) {
    this.filteredgrns = [];
    for (let i = 0; i < this.checkedgrnlistqc.length; i++) {
      let brand = this.checkedgrnlistqc[i].supplier;
      let pos = this.checkedgrnlistqc[i].value;
      if (brand.toLowerCase().indexOf(event.query.toLowerCase()) == 0 || pos.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredgrns.push(pos);
      }
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
        this.messageService.add({ severity: 'error', summary: '', detail: 'No data' });
    })
  }

  getqualitydetails() {
    this.podetailsList = [];
    var grn = this.selectedgrnno;
    this.wmsService.Getdataforqualitycheck(grn).subscribe(data => {
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
        var isallqc = this.podetailsList.filter(function (element, index) {
          return (element.checkedby);
        });
        if (isallqc.length == this.podetailsList.length) {
          this.disGrnBtn = true;
        }
        this.showDetails = true;
      }
      else
        this.messageService.add({ severity: 'eoor', summary: '', detail: 'No materials for quality check' });
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
        this.messageService.add({ severity: 'error', summary: '', detail: 'Verification failed' });
    })
  }

  quanityChange() {
    this.inwardModel.pendingqty = parseInt(this.PoDetails.materialqty) - this.inwardModel.confirmqty;
  }
  onsubmit() {

    debugger;

      this.spinner.show();
      // this.onVerifyDetails(this.podetailsList);
      this.inwardModel.pono = this.PoDetails.pono;
      this.inwardModel.receivedqty = this.PoDetails.materialqty;
      this.inwardModel.receivedby = this.inwardModel.qcby = this.employee.employeeno;
      this.podetailsList.forEach(item => {
        item.receivedby = this.employee.employeeno;
      if (!item.qualityfailedqty) {
          item.qualityfailedqty = 0;
        }
      });
      this.recqty = this.podetailsList[0].confirmqty + this.podetailsList[0].returnqty;
      this.totalqty = parseInt(this.podetailsList[0].receivedqty);
    var savedata = this.podetailsList.filter(function (element, index) {
      return (element.qualitypassedqty != 0 && !element.checkedby);
    });
    if (savedata.length == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter passed quantity.' });
      this.spinner.hide();
      return;
    }
    var invaliddata = savedata.filter(function (element, index) {
      return (element.qualitypassedqty + element.qualityfailedqty != parseInt(element.receivedqty));
    });
    if (invaliddata.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Quality Passed & Failed should be equal to Recived Quantity.' });
      this.spinner.hide();
      return;
    }

        this.wmsService.insertqualitycheck(savedata).subscribe(data => {
          this.spinner.hide();
         
          if (data != null) {
           
          }
          if (data == null) {
            this.messageService.add({ severity: 'error', summary: '', detail: 'Something went wrong' });
          }

          if (data) {
            
            this.messageService.add({ severity: 'success', summary: '', detail: 'Quality checked' });
            this.showQtyUpdateDialog = false;
            this.disGrnBtn = true;
            
          }
          this.getqualitydetails();
        });
     
  
  }


}
