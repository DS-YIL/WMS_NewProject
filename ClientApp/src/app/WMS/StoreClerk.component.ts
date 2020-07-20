import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { PoDetails, BarcodeModel, inwardModel, Materials, ddlmodel } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { first } from 'rxjs/operators';
import { isNullOrUndefined } from 'util';

@Component({
  selector: 'app-StoreClerk',
  templateUrl: './StoreClerk.component.html'
})
export class StoreClerkComponent implements OnInit {

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
  inwardemptymodel: inwardModel;
  qualitychecked: boolean = false;
  isnonpoentry: boolean = false;
  isreceivedbefore: boolean = false;
  isnonpo: boolean = false;
  returned: boolean = false;
  combomaterial: Materials[];
  selectedmaterial: Materials;
  isallreceived: boolean = false;
  pendingpos: ddlmodel[] = [];
  selectedpendingpo: ddlmodel;
  receivedmsg: string = "";
 
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.getpendingpos();
    this.getMaterials();
    this.inwardemptymodel = new inwardModel();
    this.PoDetails = new PoDetails();
    this.inwardModel = new inwardModel();
    this.inwardModel.quality = "0";
    this.inwardModel.receiveddate = new Date();
    this.inwardModel.qcdate = new Date();
  }
  checkreceivedqty(entredvalue, confirmedqty, returnedqty, maxvalue, data: any) {
    debugger;
    if (data.isreceivedpreviosly && data.pendingqty > 0) {
      if (entredvalue > data.pendingqty && !this.isnonpoentry) {
        this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Please enter received quantity less than or equal to Pending quantity' });
        //(<HTMLInputElement>document.getElementById("receivedqty")).value = "";
        data.receivedqty = "";

      }
    }
    else if (data.isreceivedpreviosly && data.pendingqty == 0 && !this.isnonpoentry) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'All materials received' });
      //(<HTMLInputElement>document.getElementById("receivedqty")).value = "";
      data.receivedqty = "";

    }
    else {
      if (entredvalue > maxvalue && !this.isnonpoentry) {
        this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Please enter received quantity less than or equal to material quantity' });
        //(<HTMLInputElement>document.getElementById("receivedqty")).value = "";
        data.receivedqty = "";

      }
    }
   
  }
  checkconfirmqty(entredvalue, receivedqty, returnedqty,data : any) {
    if (entredvalue > receivedqty) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Please enter accepted quantity less than or equal to received quantity' });
      //(<HTMLInputElement>document.getElementById("confirmqty")).value = "";
      data.confirmqty = "";
    }
    if (entredvalue != (receivedqty - returnedqty) && receivedqty && returnedqty) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'sum of return and accepted quantity must be equal to received qty' });
     // (<HTMLInputElement>document.getElementById("confirmqty")).value = "";
      data.confirmqty = "";
    }
  }
  checkreturnqty(entredvalue,receivedqty,acceptedqty,data: any) {
    if (entredvalue > receivedqty) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Please enter return quantity less than or equal to received quantity' });
      //(<HTMLInputElement>document.getElementById("returnqty")).value = "";
      data.returnqty = "";
    }
    if (entredvalue != (receivedqty - acceptedqty) && receivedqty && acceptedqty) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail:  'sum of return and accepted quantity must be equal to received qty' });
      //(<HTMLInputElement>document.getElementById("returnqty")).value = "";
      data.returnqty = "";
    }
  }
  scanBarcode() {
    debugger;
    if (this.PoDetails.pono) {
      this.PoDetails.pono;
      this.PoDetails.invoiceno;
      this.spinner.show();
      //this.wmsService.verifythreewaymatch(this.PoDetails.pono).subscribe(data => {
      //  //this.wmsService.verifythreewaymatch("123", "228738234", "1", "SK19VASP8781").subscribe(data => {
      //this.spinner.hide();
      //  if (data == true) {
      this.showQtyUpdateDialog = true;
      this.getponodetails(this.PoDetails.pono);
      // this.messageService.add({ severity: 'success', summary: 'Success Message', detail: 'GRN Posted  Sucessfully' });
      // }
      //else
      //this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Already verified' });
      //this.getponodetails(this.PoDetails.pono);
      // })

    }


  }
  addrows() {
    debugger;
    this.inwardemptymodel.material = "";
    this.inwardemptymodel.materialdescription = "";
    this.inwardemptymodel.materialqty = 0;
    this.inwardemptymodel.receivedqty = '0';

    this.podetailsList.push(this.inwardemptymodel);

  }
  setdescription(event: any,data: any) {
    debugger;
    var data1 = this.podetailsList.filter(function (element, index) {
      return (element.material == event.value.material);
    });
    if (data1.length > 0) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Material already added please select different material.' });
    }
    else {
      data.materialdescription = event.value.materialdescription;
      data.material = event.value.material;
    }
  }

  getMaterials() {
    this.wmsService.getMaterial().subscribe(data => {
      debugger;
      this.combomaterial = data;
    });
  }
  getpendingpos() {
    this.wmsService.getPendingpo().subscribe(data => {
      debugger;
      this.pendingpos = data;
    });
  }
  showpodata() {
    if (!isNullOrUndefined(this.selectedpendingpo)) {
      this.spinner.show();
      this.showQtyUpdateDialog = true;
      this.PoDetails.pono = this.selectedpendingpo.value;
      this.getponodetails(this.selectedpendingpo.value);
    }
    else {

    }
   
  }
  getponodetails(data) {
    debugger;
    this.isnonpoentry = false;
    this.qualitychecked = false;
    this.isallreceived = false;
    this.isreceivedbefore = false;
    this.returned = false;
    this.podetailsList = [];
    this.wmsService.Getthreewaymatchingdetails(data).subscribe(data => {
      this.spinner.hide();
      debugger;
      if (data && data.length>0) {
        this.disGrnBtn = false;
        // this.PoDetails = data[0];
        this.podetailsList = data;
        this.grnnumber = this.podetailsList[0].grnnumber;
        var pono = this.podetailsList[0].pono;
        if (pono.startsWith("NP") && !this.grnnumber) {
          this.isnonpoentry = true;
        }
        if (pono.startsWith("NP")) {
          this.isnonpo = true;
        }

        debugger;
        this.showDetails = true;
        var data1 = this.podetailsList.filter(function (element, index) {
          return (element.qualitychecked);
        });
        if (data1.length == this.podetailsList.length) {
          this.qualitychecked = true;
        }
        var data2 = this.podetailsList.filter(function (element, index) {
          return (element.returnedby == null);
        });
        if (data2.length == 0) {
          this.returned = true;
        }
        else {
          this.returned = false;
        }
        var receiveddata = this.podetailsList.filter(function (element, index) {
          return (element.isreceivedpreviosly && (element.pendingqty != element.materialqty));
        });
        if (receiveddata.length > 0) {
          this.isreceivedbefore = true;
        }
        else {
          this.isreceivedbefore = false;
        }
        debugger;
        var allreceiveddata = this.podetailsList.filter(function (element, index) {
          return (element.pendingqty == 0 && element.isreceivedpreviosly);
        });
        if (allreceiveddata.length > 0 && allreceiveddata.length == this.podetailsList.length) {
          this.isallreceived = true;
          this.receivedmsg = "Extra invoice : All materials already received."
        }
        else {
          this.isallreceived = false;
        }
      }
      else
        this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'No data' });
    })
  }

  getstatus(data: any) {

    if (!data.qualitycheck) {
      return "N/A";
    }
    else if (data.qualitychecked) {
      return "Checked";
    }
    else {
      return "Not checked";
    }
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
  onreturnsubmit() {
    debugger;
    this.spinner.show();
    this.podetailsList.forEach(item => {
      item.receivedby = this.employee.employeeno;
    });
    var pg = this;
    var invaliddata = this.podetailsList.filter(function (element, index) {
      return (element.confirmqty + element.returnqty != parseInt(element.receivedqty));
    });
    if (invaliddata.length > 0) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Sum of accepted and return quantity must be equal to received quantity.' });
      this.spinner.hide();
      return;
    }
    setTimeout(function () {
      var data = pg.podetailsList;
      pg.wmsService.insertreturn(data).subscribe(data => {
        pg.spinner.hide();

        if (data != null) {

        }
        if (data == null) {
          pg.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Something went wrong' });
        }

        if (data) {

          pg.messageService.add({ severity: 'success', summary: 'Success Message', detail: 'return updated' });

        }
        pg.scanBarcode();

      });
     
    }, 2000);
   

  }
  onsubmit() {
    debugger;
    if (this.podetailsList.length > 0 && this.podetailsList[0].receivedqty> '0') {

      this.spinner.show();
      // this.onVerifyDetails(this.podetailsList);
      this.inwardModel.pono = this.PoDetails.pono;
      this.inwardModel.receivedqty = this.PoDetails.materialqty;
      this.inwardModel.receivedby = this.inwardModel.qcby = this.employee.employeeno;
      this.podetailsList.forEach(item => {
        item.receivedby = this.employee.employeeno;
      });
        this.wmsService.insertitems(this.podetailsList).subscribe(data => {
          this.spinner.hide();
          if (data != null) {
            this.wmsService.verifythreewaymatch(this.PoDetails.pono).subscribe(info => {
              if (info != null)
                this.grnnumber = info.grnnumber;
              this.scanBarcode();
              //this.grnnumber = data;
            })
          }
          if (data == null) {
            this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Something went wrong' });
          }

          if (data) {
            this.messageService.add({ severity: 'success', summary: 'Success Message', detail: 'Goods received' });
            
            this.showQtyUpdateDialog = false;
            this.disGrnBtn = true;
          }
        });
    }
    else
      this.messageService.add({ severity: 'error', summary: 'Validation', detail: 'Enter Quantity' });
  }


}
