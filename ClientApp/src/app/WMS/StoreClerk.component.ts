import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, printMaterial } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { PoDetails, BarcodeModel, inwardModel, Materials, ddlmodel, updateonhold } from 'src/app/Models/WMS.Model';
import { MessageService, ConfirmationService } from 'primeng/api';
import { first } from 'rxjs/operators';
import { isNullOrUndefined } from 'util';
import { DatePipe } from '@angular/common';
import { FormGroup, FormBuilder, Validators, FormArray } from '@angular/forms';
import { AutoComplete } from 'primeng/autocomplete';

@Component({
  selector: 'app-StoreClerk',
  templateUrl: './StoreClerk.component.html',
  providers: [DatePipe, ConfirmationService]
})
export class StoreClerkComponent implements OnInit {
  @ViewChild('autoCompleteObject', { static: false }) private autoCompleteObject: AutoComplete;
  @ViewChild('myInput', { static: false }) ddlreceivedpo: any;
  @ViewChild('myInput1', { static: false }) ddlgrndata: any;
  constructor(private messageService: MessageService, private wmsService: wmsService, private formBuilder: FormBuilder, private route: ActivatedRoute, private datePipe: DatePipe, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public pono: string;
  public invoiceNo: string;
  public qty: any = 1;
  public grnNo: string;
  public showPrintDialog: boolean = false;
  public PoDetails: PoDetails;
  public podetailsList: Array<inwardModel> = [];
  public nonpovalidationList: Array<inwardModel> = [];
  public employee: Employee;
  public noofpieces: any = 1;
  public lineitmno: any;
  public totalboxes: any = 1;
  public boxno: any = 1;
  public showDetails; showQtyUpdateDialog: boolean = false;
  public disGrnBtn: boolean = true;
  public BarcodeModel: BarcodeModel;
  public inwardModel: inwardModel;
  public grnnumber: string = "";
  public totalqty: number;
  public recqty: number;
  public noOfPrint: any = 1;
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
  public invoiceForm: FormGroup;
  lblpono: string = "";
  lblinvoiceno: string = "";
  filteredgrns: any[];
  filteredmats: any[];
  public materialCode: any;
  public receivedDate: any;
  public acceptedQty: any;
  public itemNo: any;
  print: string = "Print";
  public printData = new printMaterial();
  public showPrintLabel: boolean = false;
  displayBasic: boolean = false;
  poinvoice: string = "";
  grnno: string = "";
  receivedqty: string = "";
  //inwmasterid: string = "";
  gateentryid: string = "";
  public inwmasterid: string;

  ngOnInit() {
    //this.autoCompleteObject.focusInput();
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.grnno = this.route.snapshot.queryParams.grnnumber;
    this.gateentryid = this.route.snapshot.queryParams.inwmasterid;

    this.getpendingpos();
    //Email


    this.invoiceForm = this.formBuilder.group({
      itemRows: this.formBuilder.array([this.initItemRows()])
    });
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

  initItemRows() {
    return this.formBuilder.group({
      itemname: [''],
      locatorid: ['', [Validators.required]],
      rackid: ['', [Validators.required]],
      binid: ['', [Validators.required]],
      shelflife: ['', [Validators.required]],
      binnumber: ['', [Validators.required]],
      itemlocation: ['', [Validators.required]],
      stocktype: ['', [Validators.required]],
      quantity: [0, [Validators.required]],
    });
  }
  decreaseQty() {
    if (this.qty > 1) {
      this.qty = this.qty - 1;
    }
  }
  increaseQty() {
    if (this.qty < this.noOfPrint) {
      this.qty = this.qty + 1;
    }
  }
  checkreceivedqty(entredvalue, confirmedqty, returnedqty, maxvalue, data: any) {
    debugger;
    if (isNullOrUndefined(entredvalue)) {
      data.receivedqty = "0"
    }
    if (entredvalue < 0) {
      data.receivedqty = "0";
      this.messageService.add({ severity: 'error', summary: '', detail: 'Negative value not allowed' });
      return;

    }
    if (data.pendingqty > 0) {
      if (entredvalue > data.pendingqty && !this.isnonpoentry) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Received Quantity should be less than or equal to Pending Quantity' });
        //(<HTMLInputElement>document.getElementById("receivedqty")).value = "";
        data.receivedqty = "0";
        return;
      }
    }
    else if (data.pendingqty == 0 && !this.isnonpoentry) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'All materials received' });
      //(<HTMLInputElement>document.getElementById("receivedqty")).value = "";
      data.receivedqty = "0";
      return;
    }
    //else {
    //  if (entredvalue > maxvalue && !this.isnonpoentry) {
    //    this.messageService.add({ severity: 'error', summary: '', detail: 'Received Quantity should be less than or equal to Material Quantity' });
    //    //(<HTMLInputElement>document.getElementById("receivedqty")).value = "";
    //    data.receivedqty = "0";
    //    return;
    //  }
    //}

  }
  checkconfirmqty(entredvalue, receivedqty, returnedqty, data: any) {
    if (entredvalue < 0) {
      data.confirmqty = " ";
      this.messageService.add({ severity: 'error', summary: '', detail: 'Negative value not allowed' });
      return;

    }
    if (entredvalue > receivedqty) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Accepted Quantity cannot exceed Recived Quantity' });
      //(<HTMLInputElement>document.getElementById("confirmqty")).value = "";
      data.confirmqty = "";
      return;
    }
    if (entredvalue != (receivedqty - returnedqty) && receivedqty && returnedqty) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Rejected and Accepted quantity should be equal to received qty' });
      // (<HTMLInputElement>document.getElementById("confirmqty")).value = "";
      data.confirmqty = "";
      return;
    }
  }
  checkreturnqty(entredvalue, receivedqty, acceptedqty, data: any) {
    if (entredvalue < 0) {
      data.returnqty = " ";
      this.messageService.add({ severity: 'error', summary: '', detail: 'Negative value not allowed' });
      return;

    }
    if (entredvalue > receivedqty) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Rejected Quantity cannot exceed Recived Quantity' });
      //(<HTMLInputElement>document.getElementById("returnqty")).value = "";
      data.returnqty = "";
      return;
    }
    if (entredvalue != (receivedqty - acceptedqty) && receivedqty && acceptedqty) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Rejected and Accepted Quantity should be equal to Received Quantity' });
      //(<HTMLInputElement>document.getElementById("returnqty")).value = "";
      data.returnqty = "";
      return;
    }
  }
  filterpos(event) {
    debugger;
    this.filteredpos = [];
    for (let i = 0; i < this.pendingpos.length; i++) {
      let brand = isNullOrUndefined(this.pendingpos[i].supplier) ? "" : this.pendingpos[i].supplier;
      let pos = this.pendingpos[i].pos;
      let slno = this.pendingpos[i].text;
      if (brand.toLowerCase().indexOf(event.query.toLowerCase()) == 0 || pos.toLowerCase().indexOf(event.query.toLowerCase()) == 0 || slno.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredpos.push(slno);
      }
    }
  }

  //onAfterShow(event) {
  //  alert(event);
  //  this.autoCompleteObject.focusInput();
  //}

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


  addrows() {
    debugger;
    let empltymodel = new inwardModel();
    empltymodel.pono = this.lblpono;
    empltymodel.inwmasterid = this.podetailsList[0].inwmasterid;
    empltymodel.material = "";
    empltymodel.materialdescription = "";
    empltymodel.materialqty = 0;
    empltymodel.receivedqty = '0';
    this.podetailsList.push(empltymodel);

  }
  setdescription(event: any, data: any) {
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

  //generate barcode -gayathri
  generateBarcode(details: any) {
    debugger;
    this.showPrintDialog = true;
    this.materialCode = details.material;
    this.receivedDate = this.datePipe.transform(details.receiveddate, this.constants.dateFormat);
    this.acceptedQty = details.confirmqty;
    this.pono = details.pono;
    this.receivedqty = details.receivedqty;
    this.invoiceNo = details.invoiceno;
    this.grnNo = details.grnnumber;
    this.noofpieces = details.receivedqty;
    this.lineitmno = details.lineitemno;
  }
  generatelabel(details: any) {
    debugger;
    var pono = details.pono;
    var materialid = details.material;
    var lineitemno = details.lineitemno;
    this.wmsService.getmateriallabeldata(pono, lineitemno, materialid).subscribe(data => {
      if (data) {
        var strjson = JSON.stringify(data);
        alert(strjson);

      }
      else {
        alert("No data");
      }
    })

  }
  get formArr() {
    return this.invoiceForm.get('itemRows') as FormArray;
    // return (this.invoiceForm.get('itemRows') as FormArray).controls;
  }

  GenerateBarcode() {
    debugger;
    this.showPrintDialog = false;
    this.showPrintLabel = true;
    this.printData.materialid = this.materialCode;
    this.printData.invoiceno = this.podetailsList[0].invoiceno;
    this.printData.grnno = this.podetailsList[0].grnnumber;
    this.printData.pono = this.podetailsList[0].pono;
    this.printData.noofprint = this.noOfPrint;
    this.printData.receiveddate = this.receivedDate;
    this.printData.boxno = this.boxno;
    this.printData.noofpieces = this.noofpieces;
    this.printData.totalboxes = this.totalboxes;
    this.printData.receivedqty = this.receivedqty;
    this.printData.lineitemno = this.lineitmno;
    //api call
    this.wmsService.generateBarcodeMaterial(this.printData).subscribe(data => {
      if (data) {
        debugger;
        this.printData = data;
        if (this.printData.isprint == true) {
          this.print = "Re-Print";
        }
        else {
          this.print = "Print";
        }
        console.log(this.printData);

      }
      else {
        alert("Error while generating Barcode");
      }
    })
  }

  printLabel() {
    this.showPrintDialog = false;
    this.showPrintLabel = false;
    this.printData.materialid = this.materialCode;
    this.printData.invoiceno = this.podetailsList[0].invoiceno;
    this.printData.grnno = this.podetailsList[0].grnnumber;
    this.printData.pono = this.podetailsList[0].pono;
    this.printData.noofprint = this.noOfPrint;
    this.printData.receiveddate = this.receivedDate;
    this.printData.printedby = this.employee.employeeno;
    //api call
    this.wmsService.printBarcodeMaterial(this.printData).subscribe(data => {
      if (data) {
        debugger;
        //this.printData = data;
        if (data == "success") {
          this.messageService.add({ severity: 'success', summary: '', detail: 'Label printed successfully' });
          console.log(this.printData);
          this.noOfPrint = 1;
        }
        else {
          this.messageService.add({ severity: 'success', summary: '', detail: 'Error while Printing Label' });
          console.log(this.printData);
        }


      }
      else {
        alert("Error while generating Barcode");
      }
    })
  }

  getMaterials() {
    if (isNullOrUndefined(localStorage.getItem("materials")) || localStorage.getItem("materials") == "null" || localStorage.getItem("materials") == null || localStorage.getItem("materials") == "NULL") {
      this.wmsService.getMaterial().subscribe(data => {
        debugger;
        this.combomaterial = data;
        localStorage.setItem("materials", JSON.stringify(this.combomaterial));

      });
    }
    else {
      this.combomaterial = JSON.parse(localStorage.getItem("materials")) as Materials[];

    }


  }
  //get pending for receive lisst
  getpendingpos() {
    this.wmsService.getPendingpo().subscribe(data => {
      debugger;
      this.pendingpos = data;
      ///Email
      if (this.gateentryid) {
        debugger;
        //get material details for that PO
        this.selectedpendingpono = this.gateentryid;
        this.showpodata();

      }
    });
  }
  ////get pending to accept
  getcheckedgrn() {
    this.wmsService.getcheckedgrnlist().subscribe(data => {
      debugger;
      this.checkedgrnlist = data;
      if (this.grnno) {
        debugger;
        //get material details for that PO
        this.selectedgrnno = this.grnno;
        this.showpodata1();

      }
    });
  }

  ///get pending for receive material list
  showpodata() {
    debugger;
    this.isacceptance = false;
    this.selectedgrn = null;
    this.selectedgrnno = "";
    if (!isNullOrUndefined(this.selectedpendingpono) && this.selectedpendingpono != "") {
      this.spinner.show();
      this.PoDetails = new PoDetails();
      this.showQtyUpdateDialog = true;
      // alert(this.PoDetails);
      var selpo = this.selectedpendingpono;
      var data1 = this.pendingpos.filter(function (element, index) {
        return (element.text == selpo);
      });
      if (data1.length > 0) {
        this.PoDetails.pono = data1[0].value;
      }
      else {
        this.PoDetails.pono = this.selectedpendingpono;
      }


      //this.PoDetails.inwmasterid = this.selectedpendingpono;

      this.getponodetails(this.PoDetails.pono);
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
      return (element.material.toLowerCase() == String(mat).toLowerCase());
    });
    if (selectedm.length > 0) {
      data.materialdescription = selectedm[0].materialdescription;
      data.poitemdescription = selectedm[0].materialdescription;
      data.unitprice = null;
      data.material = selectedm[0].material;
      data.qualitycheck = selectedm[0].qualitycheck;
    }
    else {
      data.materialdescription = "-";
      data.qualitycheck = true;
      data.unitprice = null;
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
  ///get pending for receive material list
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
      if (data && data.length > 0) {
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
        if (this.grnnumber) {
          this.podetailsList = this.podetailsList.filter(function (element, index) {
            return (parseInt(element.receivedqty) > 0);
          });
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


        this.isonHold = this.podetailsList[0].onhold;
        this.isonHoldview = this.podetailsList[0].onhold;
        this.onholdremarks = this.podetailsList[0].onholdremarks;


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
          //this.returned = false;
          var data3 = data2.filter(function (element, index) {
            return (element.qualitychecked || !element.qualitycheck);
          });
          if (data3.length == 0) {
            this.returned = true;
          }
          else {
            this.returned = false;
          }

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
      return;
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
    var emailtype = "1"
    var qualityrequired = this.podetailsList.filter(function (element, index) {
      return (element.qualitycheck);
    });
    var acceptrequired = this.podetailsList.filter(function (element, index) {
      return (!element.qualitycheck);
    });
    if (qualityrequired.length > 0) {
      emailtype = "1";
    }
    if (acceptrequired.length > 0) {
      emailtype = "2";
    }
    if (acceptrequired.length > 0 && acceptrequired.length > 0) {
      emailtype = "3";
    }
    this.wmsService.verifythreewaymatch(details, emailtype).subscribe(data => {
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
      this.messageService.add({ severity: 'error', summary: '', detail: 'Rejected and Accepted quantity should be equal to Received Quantity.' });
      this.spinner.hide();
      return;
    }
    this.podetailsList.forEach(item => {
      item.receivedby = this.employee.employeeno;
      if (!item.confirmqty) {
        item.confirmqty = 0;
      }
      if (!item.returnqty) {
        item.returnqty = 0;
      }
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

      var emailtype = "1"
      var qualityrequired = this.podetailsList.filter(function (element, index) {
        return (element.qualitycheck);
      });
      var acceptrequired = this.podetailsList.filter(function (element, index) {
        return (!element.qualitycheck);
      });
      if (qualityrequired.length > 0) {
        emailtype = "1";
      }
      if (acceptrequired.length > 0) {
        emailtype = "2";
      }
      if (acceptrequired.length > 0 && acceptrequired.length > 0) {
        emailtype = "3";
      }


      this.wmsService.insertitems(this.podetailsList).subscribe(data => {
        var responsestring = String(data);
        if (responsestring.startsWith("Saved")) {
          if (!this.isonHoldview) {
            this.wmsService.verifythreewaymatch(this.PoDetails.pono, emailtype).subscribe(info => {
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

            var messaged = "Goods received";
            if (this.isonHoldview) {
              messaged = "Receipt on hold"
            }
            this.messageService.add({ severity: 'success', summary: '', detail: messaged });
            this.resetpage();
            this.spinner.hide();
          }

        }
        else {
          this.spinner.hide();
          if (responsestring.includes("duplicate key")) {
            this.messageService.add({ severity: 'error', summary: '', detail: "Receipt already received" });
            this.resetpage();
          }
          else {
            this.messageService.add({ severity: 'error', summary: '', detail: responsestring });
          }

        }

        if (responsestring.startsWith("Saved")) {
          this.showQtyUpdateDialog = false;
          this.disGrnBtn = true;
        }
      });
    }
    else
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Quantity' });
  }


}
