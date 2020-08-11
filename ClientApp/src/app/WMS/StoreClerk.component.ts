import { Component, OnInit, ViewChild, ElementRef} from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { PoDetails, BarcodeModel, inwardModel, Materials, ddlmodel, updateonhold } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { first } from 'rxjs/operators';
import { isNullOrUndefined } from 'util';

@Component({
  selector: 'app-StoreClerk',
  templateUrl: './StoreClerk.component.html'
})
export class StoreClerkComponent implements OnInit {
  @ViewChild('myInput', { static: false }) ddlreceivedpo: any;
  @ViewChild('myInput1', { static: false }) ddlgrndata: any;
  constructor(private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public PoDetails: PoDetails;
  public podetailsList: Array<inwardModel> = [];
  public nonpovalidationList: Array<inwardModel> = [];
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
  checkedgrnlist: ddlmodel[] = [];
  selectedgrn: ddlmodel;
  receivedmsg: string = "";
  isacceptance: boolean = false;
  isonHold: boolean = false;
  isonHoldview: boolean = false;
  onholdremarks: string = "";
  onholdupdatedata: updateonhold;
  filteredpos: any[];
  selectedpendingpono: string = "";
  selectedgrnno: string = "";
  selectedmaterialauto: string = "";
  lblpono: string = "";
  lblinvoiceno: string = "";
  filteredgrns: any[];
  filteredmats: any[];
  displayBasic: boolean = false;
 
 
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
    this.router.navigateByUrl("Login");
    this.getpendingpos();
    this.getcheckedgrn();
    this.getMaterials();
    this.isacceptance = false;
    this.inwardemptymodel = new inwardModel();
    this.onholdupdatedata = new updateonhold();
    this.PoDetails = new PoDetails();
    this.inwardModel = new inwardModel();
    this.inwardModel.quality = "0";
    this.inwardModel.receiveddate = new Date();
    this.inwardModel.qcdate = new Date();
  }
  checkreceivedqty(entredvalue, confirmedqty, returnedqty, maxvalue, data: any) {
    debugger;
    if (entredvalue < 0) {
      data.receivedqty = " ";
      this.messageService.add({ severity: 'error', summary: '', detail: 'Negative value not allowed' });
      return;

    }
    if (data.isreceivedpreviosly && data.pendingqty > 0) {
      if (entredvalue > data.pendingqty && !this.isnonpoentry) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Please enter received quantity less than or equal to Pending quantity' });
        //(<HTMLInputElement>document.getElementById("receivedqty")).value = "";
        data.receivedqty = "";

      }
    }
    else if (data.isreceivedpreviosly && data.pendingqty == 0 && !this.isnonpoentry) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'All materials received' });
      //(<HTMLInputElement>document.getElementById("receivedqty")).value = "";
      data.receivedqty = "";

    }
    else {
      if (entredvalue > maxvalue && !this.isnonpoentry) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Please enter received quantity less than or equal to material quantity' });
        //(<HTMLInputElement>document.getElementById("receivedqty")).value = "";
        data.receivedqty = "";

      }
    }
   
  }
  checkconfirmqty(entredvalue, receivedqty, returnedqty, data: any) {
    if (entredvalue < 0) {
      data.confirmqty = " ";
      this.messageService.add({ severity: 'error', summary: '', detail: 'Negative value not allowed' });
      return;

    }
    if (entredvalue > receivedqty) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Accepted quantity cannot exceed Recived quantity' });
      //(<HTMLInputElement>document.getElementById("confirmqty")).value = "";
      data.confirmqty = "";
    }
    if (entredvalue != (receivedqty - returnedqty) && receivedqty && returnedqty) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Sum of Returned and Accepted quantity must be equal to received qty' });
     // (<HTMLInputElement>document.getElementById("confirmqty")).value = "";
      data.confirmqty = "";
    }
  }
  checkreturnqty(entredvalue, receivedqty, acceptedqty, data: any) {
    if (entredvalue < 0) {
      data.returnqty = " ";
      this.messageService.add({ severity: 'error', summary: '', detail: 'Negative value not allowed' });
      return;

    }
    if (entredvalue > receivedqty) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Returned quantity cannot exceed Recived quantity' });
      //(<HTMLInputElement>document.getElementById("returnqty")).value = "";
      data.returnqty = "";
    }
    if (entredvalue != (receivedqty - acceptedqty) && receivedqty && acceptedqty) {
      this.messageService.add({ severity: 'error', summary: '', detail:  'Sum of Return and Accepted quantity must be equal to received quantity' });
      //(<HTMLInputElement>document.getElementById("returnqty")).value = "";
      data.returnqty = "";
    }
  }
  filterpos(event) {
    debugger;
    this.filteredpos = [];
    for (let i = 0; i < this.pendingpos.length; i++) {
      let brand = isNullOrUndefined(this.pendingpos[i].supplier) ? "" : this.pendingpos[i].supplier;
      let pos = this.pendingpos[i].value;
      if (brand.toLowerCase().indexOf(event.query.toLowerCase()) == 0 || pos.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredpos.push(pos);
      }
    }
  }

  filtergrn(event) {

    this.filteredgrns = [];
    for (let i = 0; i < this.checkedgrnlist.length; i++) {
      let brand = this.checkedgrnlist[i].supplier;
      let pos = this.checkedgrnlist[i].text;
      if (brand.toLowerCase().indexOf(event.query.toLowerCase()) == 0 || pos.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredgrns.push(pos);
      }
    }
  }

  filtermats(event) {
    this.filteredmats = [];
    for (let i = 0; i < this.combomaterial.length; i++) {
      let brand = this.combomaterial[i].material;
      let pos = this.combomaterial[i].materialdescription;
      if (brand.toLowerCase().indexOf(event.query.toLowerCase()) == 0 || pos.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredmats.push(brand);
      }
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
    let empltymodel = new inwardModel();
    empltymodel.material = "";
    empltymodel.materialdescription = "";
    empltymodel.materialqty = 0;
    empltymodel.receivedqty = '0';
    this.podetailsList.push(empltymodel);

  }
  setdescription(event: any,data: any) {
    debugger;
    var data1 = this.podetailsList.filter(function (element, index) {
      return (element.material == event.value.material);
    });
    if (data1.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Material already added please select different material.' });
    }
    else {
      data.materialdescription = event.value.materialdescription;
      data.material = event.value.material;
    }
  }

  getMaterials() {
    this.spinner.show();
    if (isNullOrUndefined(localStorage.getItem("materials")) || localStorage.getItem("materials") == "null" || localStorage.getItem("materials") == null || localStorage.getItem("materials") == "NULL") {
      this.wmsService.getMaterial().subscribe(data => {
        debugger;
        this.combomaterial = data;
        localStorage.setItem("materials", JSON.stringify(this.combomaterial));
       
      });
    }
    else {
      this.spinner.hide();
      this.combomaterial = JSON.parse(localStorage.getItem("materials")) as Materials[];
      
    }

    
  }
  getpendingpos() {
    this.wmsService.getPendingpo().subscribe(data => {
      debugger;
      this.pendingpos = data;
    });
  }
  getcheckedgrn() {
    this.wmsService.getcheckedgrnlist().subscribe(data => {
      debugger;
      this.checkedgrnlist = data;
    });
  }
  showpodata() {
    this.isacceptance = false;
    this.selectedgrn = null;
    this.selectedgrnno = "";
    if (!isNullOrUndefined(this.selectedpendingpono) && this.selectedpendingpono != "") {
      this.spinner.show();
      this.showQtyUpdateDialog = true;
      this.PoDetails.pono = this.selectedpendingpono;
      this.getponodetails(this.selectedpendingpono);
    }
    else {

    }
   
  }
  showpodata1() {
    this.isacceptance = true;
    this.selectedpendingpo = null;
    this.selectedpendingpono = "";
    if (!isNullOrUndefined(this.selectedgrnno) && this.selectedgrnno != "") {
      this.spinner.show();
      this.showQtyUpdateDialog = true;
      //this.PoDetails.pono = this.selectedgrn.value;
      this.getponodetails(this.selectedgrnno);
    }
    else {

    }

  }

  deleteRow(index: number) {
    this.podetailsList.splice(index, 1);
    //this.formArr.removeAt(index);
  }

  validate(data: any, ind: number) {
    var data1 = this.podetailsList.filter(function (element, index) {
      return (element.material == data.material && index != ind);
    });
    if (data1.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Material already added please select different material.' });
      data.material = "";
      data.materialdescription = "";
      data.qualitycheck = false;
      return;
    }
  }

  setmatdesc(data: any) {
    debugger;
    var mat = data.material;
    this.nonpovalidationList = [];
    var selectedm = this.combomaterial.filter(function (element, index) {
      return (element.material == mat);
    });
    if (selectedm.length > 0) {
      data.materialdescription = selectedm[0].materialdescription;
      data.qualitycheck = selectedm[0].qualitycheck;
    }
    else {
      data.materialdescription = "-";
      data.qualitycheck = true;
    }
  
   
  

  }
  resetpage() {
    debugger;
    this.selectedgrn = null;
    this.selectedpendingpo = null;
    //this.selectedpendingpono = "";
    //this.selectedgrnno = "";
    this.isnonpoentry = false;
    this.qualitychecked = false;
    this.isallreceived = false;
    this.isreceivedbefore = false;
    this.returned = false;
    this.isnonpo = false;
    this.isonHold = false;
    this.isonHoldview = false;
    this.onholdremarks = "";
    this.podetailsList = [];
    this.getpendingpos();
    this.getcheckedgrn();
    if (this.isacceptance) {
      this.showpodata1();
    }
    else {
      this.showpodata();
    }
  }

  onholdchange(event: any) {
    debugger;
    if (event.target.checked) {
        this.podetailsList.forEach(item => {
          item.receivedqty = '0';
          item.receiveremarks = "";
        });
      this.displayBasic = true;
    }
    else {
      this.onholdremarks = "";
      this.displayBasic = false;
    }
   
  }
  getponodetails(data) {
    debugger;
    this.isnonpoentry = false;
    this.qualitychecked = false;
    this.isallreceived = false;
    this.isreceivedbefore = false;
    this.returned = false;
    this.isnonpo = false;
    this.isonHold = false;
    this.isonHoldview = false;
    this.onholdremarks = "";
    this.podetailsList = [];
    this.wmsService.Getthreewaymatchingdetails(data).subscribe(data => {
      this.spinner.hide();
      debugger;
      if (data && data.length>0) {
        this.disGrnBtn = false;
        // this.PoDetails = data[0];
        this.podetailsList = data;
        var pono = this.podetailsList[0].pono;
        this.lblpono = this.podetailsList[0].pono;
        this.lblinvoiceno = this.podetailsList[0].invoiceno;
        this.grnnumber = this.podetailsList[0].grnnumber;
        this.isonHold = this.podetailsList[0].onhold;
        this.isonHoldview = this.podetailsList[0].onhold;
        this.onholdremarks = this.podetailsList[0].onholdremarks;
        if (pono.startsWith("NP") && !this.grnnumber && !this.isonHold) {
          this.isnonpoentry = true;
        }
        if (pono.startsWith("NP")) {
          this.isnonpo = true;
        }
        else {
          if (!this.grnnumber) {
            this.podetailsList = this.podetailsList.filter(function (element, index) {
              return (element.materialqty > 0);
            });
          }
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
        this.messageService.add({ severity: 'error', summary: '', detail: 'No data' });
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
      data.qcstatus = "Pending";
      return "Pending";
    }
  }
  update() {
    debugger
    if (isNullOrUndefined(this.selectedpendingpono) || this.selectedpendingpono == "") {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Please Select receipt' });
      this.returned;
    }
    this.spinner.show();
    this.onholdupdatedata = new updateonhold();
    this.onholdupdatedata.invoiceno = this.selectedpendingpono;
    this.onholdupdatedata.remarks = this.onholdremarks;
    this.onholdupdatedata.onhold = this.isonHoldview;
    this.wmsService.updateonhold(this.onholdupdatedata).subscribe(data => {
      this.spinner.hide();
      this.messageService.add({ severity: 'success', summary: '', detail: 'Receipt updated' });
      //this.getponodetails(this.selectedpendingpo.value)
      this.resetpage();
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
  onreturnsubmit() {
    debugger;
    this.spinner.show();
   
    var pg = this;
    var validdata = this.podetailsList.filter(function (element, index) {
      return (element.qcstatus != "Pending");
    });
    if (validdata.length == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Quality check pending.' });
      this.spinner.hide();
      return;
    }
   
    var invaliddata = validdata.filter(function (element, index) {
      return (element.confirmqty + element.returnqty != parseInt(element.receivedqty));
    });
    if (invaliddata.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Sum of Returned and Accepted  quantity must be equal to received quantity.' });
      this.spinner.hide();
      return;
    }
    this.podetailsList.forEach(item => {
      item.receivedby = this.employee.employeeno;
    });
    setTimeout(function () {
      var data = validdata;
      pg.wmsService.insertreturn(data).subscribe(data => {
        pg.spinner.hide();

        if (data != null) {

        }
        if (data == null) {
          pg.messageService.add({ severity: 'error', summary: '', detail: 'Something went wrong' });
        }

        if (data) {

          pg.messageService.add({ severity: 'success', summary: '', detail: 'Materials Accepted' });

        }
        pg.resetpage();
        //pg.scanBarcode();

      });
     
    }, 2000);
   

  }
  onsubmit() {
    debugger;
    if (this.podetailsList.length > 0) {
      var invaliddata = this.podetailsList.filter(function (element, index) {
        return (isNullOrUndefined(element.material) || element.material == "");
      });
      if (invaliddata.length > 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Please select material' });
        return;
      }
      this.inwardModel.pono = this.PoDetails.pono;
      if (!this.isonHoldview) {
        if (this.inwardModel.pono.startsWith("NP")) {
          var invalidrcv = this.podetailsList.filter(function (element, index) {
            return (element.receivedqty == "0");
          });
          if (invalidrcv.length > 0) {
            this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Received quantity' });
            return;
          }
        }
        else {
          var invalidrcv = this.podetailsList.filter(function (element, index) {
            return (element.receivedqty != "0");
          });
          if (invalidrcv.length == 0) {
            this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Received quantity' });
            return;
          }

        }

        this.podetailsList = this.podetailsList.filter(function (element, index) {
          return (element.receivedqty != "0");
        });


      }
     
      this.spinner.show();
      // this.onVerifyDetails(this.podetailsList);
    
      this.inwardModel.receivedqty = this.PoDetails.materialqty;
      this.inwardModel.receivedby = this.inwardModel.qcby = this.employee.employeeno;
      this.podetailsList.forEach(item => {
        item.receivedby = this.employee.employeeno;
        item.onhold = this.isonHoldview;
        item.onholdremarks = this.onholdremarks;
      });

        this.wmsService.insertitems(this.podetailsList).subscribe(data => {
          if (data != null) {
            if (!this.isonHoldview) {
              this.wmsService.verifythreewaymatch(this.PoDetails.pono).subscribe(info => {
                this.spinner.hide();

                if (info != null) {
                  this.resetpage();
                  this.messageService.add({ severity: 'success', summary: '', detail: 'Goods received' });
                }
                else {
                  this.messageService.add({ severity: 'success', summary: '', detail: 'Something went wrong while creating GRN' });
                }

              })

            }
            else {
              this.resetpage();
              var messaged = "Goods received";
              if (this.isonHoldview) {
                messaged = "Receipt on hold"
              }
              this.messageService.add({ severity: 'success', summary: '', detail: messaged });
              this.spinner.hide();
            }
            
          }
          if (data == null) {
            this.spinner.hide();
            this.messageService.add({ severity: 'error', summary: '', detail: 'Something went wrong' });
          }

          if (data) {
           
            
            this.showQtyUpdateDialog = false;
            this.disGrnBtn = true;
          }
        });
    }
    else
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Quantity' });
  }


}
