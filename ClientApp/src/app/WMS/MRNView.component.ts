import { Component, OnInit, Inject } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { PoDetails, inwardModel, ddlmodel, MRNsavemodel } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { FormBuilder } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { ConfirmationService } from 'primeng/api';
import { isNullOrUndefined } from 'util';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-MRNView',
  templateUrl: './MRNView.component.html',
  animations: [
    trigger('rowExpansionTrigger', [
      state('void', style({
        transform: 'translateX(-10%)',
        opacity: 0
      })),
      state('active', style({
        transform: 'translateX(0)',
        opacity: 1
      })),
      transition('* <=> *', animate('400ms cubic-bezier(0.86, 0, 0.07, 1)'))
    ])
  ],
  providers: [DatePipe, ConfirmationService]
})
export class MRNViewComponent implements OnInit {
  isnonpo: boolean = false;
  public url = "";
  constructor(private ConfirmationService: ConfirmationService, private http: HttpClient, private formBuilder: FormBuilder, private messageService: MessageService, private datePipe: DatePipe, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService, @Inject('BASE_URL') baseUrl: string) { this.url = baseUrl; }
  cols: any[];
  public date: Date = null;
  public PoDetails: PoDetails;
  public podetailsList: Array<inwardModel> = [];
  public employee: Employee;
  public rowIndex: number;
  public disSaveBtn: boolean = false;
  checkedgrnlist: ddlmodel[] = [];
  projectlists: ddlmodel[] = [];
  selectedgrn: ddlmodel;
  selectedgrnno: string = "";
  selectedpono: string = "";
  selectedponomodel: ddlmodel;
  filteredgrns: any[];
  filteredpono: ddlmodel[] = [];
  isallplaced: boolean = false;
  mrnsavemodel: MRNsavemodel;
  mrnList: Array<MRNsavemodel> = [];
  mrnremarks: string = "";
  inwardid; confirmqty; issuedqty: number;
  isalreadytransferred; ShowPrint; showPrntBtn: boolean = false;
  public selectedStatus: string;
  public currentDate: Date;
  public totalGRNList: ddlmodel[] = [];
  materialList: Array<any> = [];
  public selectedRow: any;
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.currentDate = new Date();
    this.selectedStatus = "Pending";
    this.PoDetails = new PoDetails();
    this.mrnsavemodel = new MRNsavemodel();
    this.selectedponomodel = new ddlmodel();
    this.isalreadytransferred = false;
    this.mrnremarks = "";
    this.getcheckedgrn();
    this.getprojects();
    this.cols = [
      { field: 'material', header: 'Material' },
      { field: 'materialdescription', header: 'Material Description' },
      { field: 'materialquantity', header: 'Material Quantity' },
      { field: 'receivedquantity', header: 'Received Quantity' },
      { field: 'acceptedquantity', header: 'Accepted Quantity' },
      { field: 'returnedquantity', header: 'Returned Quantity' },
      { field: 'pendingquantity', header: 'Pending Quantity' },
      { field: 'materialbarcode', header: 'Material Barcode' }
    ];


  }

  getcheckedgrn() {
    this.spinner.show();
    this.wmsService.getcheckedgrnlistforputaway().subscribe(data => {
      this.totalGRNList = data;
      this.spinner.hide();
      this.checkedgrnlist = this.totalGRNList.filter(li => li.isdirecttransferred != true);
    });
  }
  getprojects() {
    this.spinner.show();
    this.wmsService.getprojectlist().subscribe(data => {
      debugger;
      this.projectlists = data;
      this.spinner.hide();
    });
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

  filterpos(event) {
    this.filteredpono = [];
    for (let i = 0; i < this.projectlists.length; i++) {
      let brand = this.projectlists[i].value;
      let pos = this.projectlists[i].text;
      if (brand.toLowerCase().indexOf(event.query.toLowerCase()) == 0 || pos.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredpono.push(this.projectlists[i]);
      }
    }
  }

  getprojetcode(event: any) {
    var data = event;
    debugger;
  }

  //check validations for issuer quantity

  reqQtyChange(data: any) {
    var comQty = data.confirmqty;
    if (data.putawayqty)
      comQty = comQty - data.putawayqty;
    if (data.partialqty)
      comQty = comQty - data.partialqty;

    if (data.issuedqty > comQty) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'issued Quantity should be lessthan or equal to sum of put away and partial quantity' });
      data.issuedqty = 0;
      return;
    }
  }
  submitmr() {
    debugger;

    if (isNullOrUndefined(this.selectedgrnno) || this.selectedgrnno == "") {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter/select GRNNo' });
      return;
    }
    if (isNullOrUndefined(this.selectedponomodel.value)) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter/select project' });
      return;
    }
    debugger;
    this.podetailsList.forEach(item => {
      if (item.issuedqty) {
        this.reqQtyChange(item);
        this.mrnsavemodel = new MRNsavemodel();
        this.mrnsavemodel.grnnumber = this.selectedgrnno;
        this.mrnsavemodel.projectcode = this.selectedponomodel.value;
        this.mrnsavemodel.directtransferredby = this.employee.employeeno;
        this.mrnsavemodel.mrnremarks = item.mrnremarks;
        this.mrnsavemodel.inwardid = item.inwardid;
        this.mrnsavemodel.projectcode = this.selectedponomodel.value;
        this.mrnsavemodel.acceptedqty = item.confirmqty;
        this.mrnsavemodel.issuedqty = item.issuedqty;
        this.mrnList.push(this.mrnsavemodel);
      }
    })

    this.spinner.show();

    this.wmsService.updatemrn(this.mrnList).subscribe(data => {
      this.spinner.hide();
      this.showPrntBtn = true;
      this.messageService.add({ severity: 'success', summary: '', detail: "Material transferred" });
      this.selectedponomodel = new ddlmodel();
      this.selectedgrnno = "";
      this.SearchGRNNo();
      this.getcheckedgrn();
     
    })


  }


  //search list option changes event
  SearchGRNNo() {
    debugger;
    this.podetailsList = [];
    this.PoDetails.grnnumber = "";
    if (isNullOrUndefined(this.selectedgrnno) || this.selectedgrnno == "") {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter GRNNo' });
      return;

    }
    this.PoDetails.grnnumber = this.selectedgrnno;
    this.isnonpo = false;
    this.spinner.show();
    this.wmsService.getitemdetailsbygrnno(this.PoDetails.grnnumber).subscribe(data => {
      this.spinner.hide();
      if (data) {
        debugger;
        //this.PoDetails = data[0];
        this.podetailsList = data;
        var itemlocationavailable = this.podetailsList.filter(function (element, index) {
          return (element.itemlocation);
        });
        if (itemlocationavailable.length > 0) {
          this.podetailsList = [];
          this.messageService.add({ severity: 'warn', summary: '', detail: 'Materials already in stock for this GRN.' });
          return;
        }
        var ponumber = this.podetailsList[0].pono;
        this.isalreadytransferred = this.podetailsList[0].isdirecttransferred;
        if (this.isalreadytransferred) {
          debugger;
          var dtlist = this.podetailsList;
          var datax = this.projectlists.filter(function (element, index) {
            return (element.value == dtlist[0].projectcode);
          });
          this.selectedponomodel = datax[0];
          this.mrnremarks = this.podetailsList[0].mrnremarks;
        }
        if (ponumber.startsWith("NP")) {
          this.isnonpo = true;
        }

      }
      else
        this.messageService.add({ severity: 'error', summary: '', detail: 'No data for this GRN No' });
    })
  }

  onSelectStatus(event) {
    this.podetailsList = [];
    this.selectedgrnno = "";
    this.selectedStatus = event.target.value;
    if (this.selectedStatus == "Pending") {
      this.checkedgrnlist = this.totalGRNList.filter(li => li.isdirecttransferred != true);
    }
    else if (this.selectedStatus == "Approved") {
      this.checkedgrnlist = this.totalGRNList.filter(li => li.isdirecttransferred == true);
    }
  }

  //Get details
  showdetails(data: any, index: any) {

    this.selectedRow = index;
    this.materialList = [];
    data.showdetail = !data.showdetail;
    if (data.showdetail)
    this.bindMaterilaDetails(data.value);
  }

  //get materials list
  bindMaterilaDetails(inwardid: any) {
    this.checkedgrnlist[this.selectedRow].materiallistarray = [];
   this.wmsService.getMRNmaterials(inwardid).subscribe(data => {
      this.materialList = data;
      this.checkedgrnlist[this.selectedRow].materiallistarray = data;
      
    });
  }
  PrintMRN(data: any) {
    var grnnumber = "";
    if (!data)
      grnnumber = this.selectedgrnno;
    else
      grnnumber = data.text;
    this.wmsService.getitemdetailsbygrnno(grnnumber).subscribe(data => {
      this.spinner.hide();
      this.ShowPrint = true;
      this.podetailsList = data;
    });
  }
  //back to MRN view
  navigateToMRNView() {
    this.ShowPrint = false
  }
}
