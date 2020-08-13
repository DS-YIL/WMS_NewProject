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
  providers: [DatePipe , ConfirmationService]
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
  mrnremarks: string = "";
  isalreadytransferred: boolean = false;
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
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
      debugger;
      this.checkedgrnlist = data;
      this.spinner.hide();
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


  submitmr() {

    if (isNullOrUndefined(this.selectedgrnno) || this.selectedgrnno == "") {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter/select GRNNo' });
      return;
    }
    if (isNullOrUndefined(this.selectedponomodel)) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter/select project' });
      return;
    }
    debugger;
    this.mrnsavemodel = new MRNsavemodel();
    this.mrnsavemodel.grnnumber = this.selectedgrnno;
    this.mrnsavemodel.projectcode = this.selectedponomodel.value;
    this.mrnsavemodel.directtransferredby = this.employee.employeeno;
    this.mrnsavemodel.mrnremarks = this.mrnremarks;
    this.spinner.show();

    this.wmsService.updatemrn(this.mrnsavemodel).subscribe(data => {
      this.spinner.hide();
      this.messageService.add({ severity: 'success', summary: '', detail: "Material transferred" });
      //this.getponodetails(this.selectedpendingpo.value)
      this.SearchGRNNo();
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
  



 
 

 


 


}
