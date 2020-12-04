import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { PoDetails, BarcodeModel, inwardModel, ddlmodel, UnholdGRModel } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { first } from 'rxjs/operators';
import { isNullOrUndefined } from 'util';

@Component({
  selector: 'app-HoldGRView',
  templateUrl: './HoldGRView.component.html'
})
export class HoldGRViewComponent implements OnInit {

  constructor(private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public PoDetails: PoDetails;
  public podetailsList: Array<inwardModel> = [];
  public filteredpodetailsList: Array<inwardModel> = [];
  filteredpomatdetailsList: Array<inwardModel> = [];
  public employee: Employee;
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
  onholdupdatedata: UnholdGRModel;
  unholdremarks: string = "";
  unholdremarksview: string = "";
  selectedrow: inwardModel;
  grstatus: string = "";
  tempcol1: string = "";
  tempcol2: string = "";
 
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

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
    this.dtinvoiceno= data.invoiceno;
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
    
    this.spinner.show();
    this.onholdupdatedata = new UnholdGRModel();
    this.onholdupdatedata.inwmasterid = data.inwmasterid;
    this.onholdupdatedata.unholdremarks = this.unholdremarks;
    this.onholdupdatedata.unholdaction = this.graction == 1 ? true : false;
    var msg = this.graction == 1 ? "Accepted" : "Returned";
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
  getholdgrdetails() {
    debugger;
    this.podetailsList = [];
    this.filteredpodetailsList = [];
    var grn = this.selectedgrnno;
    var status = this.grstatus;
    if (status == "accepted") {
      this.tempcol1 = "Accepted By";
      this.tempcol2 = "Accepted On";
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
