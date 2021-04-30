import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, printMaterial, printonholdGR, DynamicSearchResult } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { PoDetails, BarcodeModel, inwardModel, ddlmodel, UnholdGRModel } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { first } from 'rxjs/operators';
import { isNullOrUndefined } from 'util';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-HoldGRView',
  templateUrl: './HoldGRView.component.html',
  providers: [DatePipe]
})
export class HoldGRViewComponent implements OnInit {

  constructor(private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, private datePipe: DatePipe, public constants: constants, private spinner: NgxSpinnerService) { }

  public dynamicData = new DynamicSearchResult();
  public PoDetails: PoDetails;
  public podetailsList: Array<inwardModel> = [];
  public filteredpodetailsList: Array<inwardModel> = [];
  filteredpomatdetailsList: Array<inwardModel> = [];
  public employee: Employee;
  public qty: any = 1;
  showPrintLabel: boolean;
  public noOfPrint: any = 1;
  public showDetails; showQtyUpdateDialog: boolean = false;
  public disGrnBtn: boolean = true;
  public BarcodeModel: BarcodeModel;
  public inwardModel: inwardModel;
  public grnnumber: string = "";
  public totalqty: number;
  isnonpo: boolean = false;
  public recqty: number;
  qualitychecked: boolean = false;
  selectedgrnno: string = "";
  filteredgrns: any[];
  checkedgrnlistqc: ddlmodel[] = [];
  showdetail: boolean = false;
  dtpono: string = "";
  dtinvoiceno: string = "";
  dtvendorname: string = "";
  graction: number = 1;
  public materialCode: any;
  public receivedDate: any;
  public acceptedQty: any;
  public itemNo: any;
  print: string = "Print";
  onholdupdatedata: UnholdGRModel;
  unholdremarks: string = "";
  unholdremarksview: string = "";
  selectedrow: inwardModel;
  grstatus: string = "";
  tempcol1: string = "";
  tempcol2: string = "";
  showPrintDialog: boolean = false;
  public printData = new printMaterial();
  public printgr = new printonholdGR();
  public pono: string;
  public lineitemno: string;
  public gateentryid: string;
  public invoiceno: string;
  public locationMasterList: Array<any> = [];
  public printerid; inwardid: any;

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.printerid = "";
    this.grstatus = "hold";
    this.filteredpodetailsList = [];
    this.filteredpomatdetailsList = [];
    this.PoDetails = new PoDetails();
    this.inwardModel = new inwardModel();
    this.onholdupdatedata = new UnholdGRModel();
    this.selectedrow = new inwardModel();
    this.inwardModel.quality = "0";
    this.inwardModel.receiveddate = new Date();
    this.inwardModel.qcdate = new Date();
    this.getholdgrdetails();
    this.getLocationMasterList();
  }


  //get Location Master List
  getLocationMasterList() {
    this.spinner.show();
    this.locationMasterList = [];
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.query = "select  * from wms.PrinterLocationmaster where type='OnHold' and deleteflag !=true order by printerid";
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.locationMasterList = data;
      this.spinner.hide();
    })
  }
  getcheckedgrnforqc() {
    this.spinner.show();
    this.wmsService.getholdgrlist().subscribe(data => {
      debugger;
      this.checkedgrnlistqc = data;
      this.spinner.hide();
    });
  }

  getholgrdata() {
    this.spinner.show();
    this.wmsService.getholdgrlist().subscribe(data => {
      debugger;
      this.checkedgrnlistqc = data;
      this.spinner.hide();
    });
  }
  showgrdetail(data: any) {
    this.filteredpomatdetailsList = [];
    this.selectedrow = data;
    this.dtpono = data.pono;
    this.dtinvoiceno = data.invoiceno;
    this.dtvendorname = data.vendorname;
    this.unholdremarksview = data.unholdremarks;
    this.filteredpomatdetailsList = this.podetailsList.filter(o => o.inwmasterid == data.inwmasterid);
    this.showdetail = true;

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

  update() {
    debugger;
    //if (this.graction == -1) {
    //  this.messageService.add({ severity: 'success', summary: '', detail: 'Select accept or return' });
    //  return;
    //}

    var data = this.selectedrow;
    this.onholdupdatedata = new UnholdGRModel();
    this.onholdupdatedata.inwmasterid = data.inwmasterid;
    this.onholdupdatedata.unholdremarks = this.unholdremarks;
    this.onholdupdatedata.unholdaction = this.graction == 1 ? true : false;
    var msg = this.graction == 1 ? "Released" : "Returned";
    if (this.graction != 1 && (isNullOrUndefined(this.unholdremarks) || this.unholdremarks.trim() == "")) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Remarks' });
      return;
    }
    this.spinner.show();
    this.onholdupdatedata.unholdedby = this.employee.employeeno;
    this.wmsService.updateonholdgr(this.onholdupdatedata).subscribe(data => {
      this.spinner.hide();
      this.messageService.add({ severity: 'success', summary: '', detail: msg });
      this.showdetail = false;
      this.selectedrow = new inwardModel();
      //this.getponodetails(this.selectedpendingpo.value)
      this.getholdgrdetails();

    })




  }

  hideDG() {
    this.dtpono = "";
    this.dtinvoiceno = "";
    this.graction = -1;
    this.filteredpomatdetailsList = [];
    this.dtvendorname = "";
    this.unholdremarks = "";
  }

  generatelabel(details: any) {
    this.printerid = "";
    this.showdetail = false;
    this.showPrintDialog = true;
    this.showPrintLabel = false;
    this.materialCode = details.material;
    this.receivedDate = this.datePipe.transform(details.receiveddate, this.constants.dateFormat);
    this.acceptedQty = details.confirmqty;
    this.pono = details.pono;
    this.lineitemno = details.lineitemno;
    this.gateentryid = details.inwmasterid;
    this.invoiceno = details.invoiceno;
    this.inwardid = details.inwardid;
    //this.printData.pono = details.pono;
    //this.printData.receivedqty = details.receivedqty;
    //this.printData.invoiceno = details.invoiceno;
    //this.printData.lineitemno = details.lineitemno;
    //this.printData.noofpieces = details.receivedqty;


  }

  printlabel() {
    if (!this.printerid) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Select Location' });
      return;
    }
   
    this.printgr.materialid = this.materialCode;
    this.printgr.gateentryid = this.gateentryid;
    this.printgr.pono = this.pono;
    this.printgr.invoiceno = this.invoiceno;
    this.printgr.createdby = this.employee.employeeno;
    this.printgr.receiveddate = this.receivedDate;
    this.printgr.noofprint = this.noOfPrint;
    this.printgr.printerid = this.printerid;
    this.printgr.inwardid = this.inwardid;
    this.wmsService.generateqronhold(this.printgr).subscribe(data => {
      this.spinner.hide();
      this.showdetail = false;
      this.showPrintDialog = false;
      this.showPrintLabel = true;
      if (data) {
        this.printgr = new printonholdGR();
        this.printgr = data;
      }
    });

  }

  printLabel() {
    this.wmsService.printonholdmaterials(this.printgr).subscribe(data => {
      this.spinner.show();
      if (data) {
        this.printgr = new printonholdGR();
        this.printgr = data;
        this.spinner.hide();
      }
      else {
        this.spinner.hide();
      }

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

  getholdgrdetails() {
    debugger;
    this.podetailsList = [];
    this.filteredpodetailsList = [];
    var grn = this.selectedgrnno;
    var status = this.grstatus;
    if (status == "accepted") {
      this.tempcol1 = "Released By";
      this.tempcol2 = "Released On";
    }
    else if (status == "returned") {
      this.tempcol1 = "Returned By";
      this.tempcol2 = "Returned On";
    }
    this.wmsService.Getdataforholdgr(status).subscribe(data => {
      this.spinner.hide();
      if (data) {
        this.podetailsList = data;
        this.podetailsList.forEach(item => {
          var data = this.filteredpodetailsList.filter(o => o.inwmasterid == item.inwmasterid);
          if (data.length > 0) {
          }
          else {
            this.filteredpodetailsList.push(item);
          }


        })

      }

    })
  }

}
