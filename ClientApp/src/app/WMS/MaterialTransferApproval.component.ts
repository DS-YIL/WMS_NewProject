import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl, ValidatorFn } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { materialtransferMain, materialtransferTR, materialRequestDetails, returnmaterial, gatepassModel, materialistModel, PoDetails, StockModel, materialistModelreturn, materialistModeltransfer, ddlmodel, materialtransferapproverModel } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { isNullOrUndefined } from 'util';

@Component({
  selector: 'app-MaterialTransferApproval',
  templateUrl: './MaterialTransferApproval.component.html'
})
export class MaterialTransferApprovalComponent implements OnInit {
   AddDialog: boolean;
  showdialog: boolean;

  public materiallistData: Array<any> = [];
  public materiallistDataHistory: Array<any> = [];
    AddDialogfortransfer: boolean;
    showdialogfortransfer: boolean;
    projectcodes: string;
    showhistory: boolean=false;
    displaydetail: boolean;
  public gpIndx: number;
    GatepassTxt: string;
  constructor(private formBuilder: FormBuilder, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }
  public StockModel: StockModel;
  public tarnsferModel: returnmaterial;
  public material: any;
  public requestList: materialtransferMain[] = [];
  public requestListall: materialtransferMain[] = [];
  public employee: Employee;
  public displayItemRequestDialog; RequestDetailsSubmitted; showAck; btnDisable: boolean = false;
  public materialRequestDetails: materialRequestDetails;
  public pono: string;
  public rowindex: number;
  public dynamicData = new DynamicSearchResult();
  public searchItems: Array<searchList> = [];
  public selectedlist: Array<searchList> = [];
  public searchresult: Array<object> = [];
  public btnDisabletransfer: boolean = false;
  public locationlist: any[] = [];
  public chkChangeshideshow: boolean = false;
  public requestid: any;
  public materialistModel: materialistModeltransfer;
  public showDetails; showLocationDialog: boolean = false;
  public PoDetails: PoDetails;
  public StockModelForm: FormGroup;
  public StockModelForm1: FormGroup;
  public stock: StockModel[] = [];
  public gatepassdialog: boolean = false;
  projectlists: ddlmodel[] = [];
  selectedproject: ddlmodel;
  selectedprojectfrom: ddlmodel;
  filteredprojects: ddlmodel[] = [];
  filteredprojectsfrom: ddlmodel[] = [];
  materiallists: ddlmodel[] = [];
  selectedmaterial: string = "";
  filteredmaterial: string[] = [];
  materialtransferlist: materialtransferMain[] = [];
  materialtransfersavelist: materialtransferMain;
  materialtransferdetil: materialtransferTR[] = [];
  transferremarks: string = "";
  transferedfromlbl: string = "";
  transferedtolbl: string = "";
  selectedrows: materialtransferMain[] = [];
  approvalremarks: string = "";
  hideapproval: boolean = false;
  returntype: string = "";
  materialapproverlistdetil: materialtransferapproverModel[] = []
  requestedid: string = "";


  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.requestedid = this.route.snapshot.queryParams.transferid;
    this.PoDetails = new PoDetails();
    this.StockModel = new StockModel();
    this.materialapproverlistdetil = [];
    this.returntype = "Pending";
    this.tarnsferModel = new returnmaterial();
    this.hideapproval = false;
    this.tarnsferModel.materialList = [];
    this.filteredmaterial = [];
    this.requestList = [];
    this.filteredprojects = [];
    this.filteredprojectsfrom = [];
    this.materialtransferlist = [];
    this.selectedrows = []; 
    this.materialtransfersavelist = new materialtransferMain();
    this.materialtransferdetil = [];
    this.route.params.subscribe(params => {
      if (params["pono"]) {
        this.pono = params["pono"];
      }
    });

    this.getMaterialRequestlist(this.employee.employeeno);
    this.StockModelForm = this.formBuilder.group({
      locatorid: ['', [Validators.required]],
      rackid: ['', [Validators.required]],
      binid: ['', [Validators.required]],
      shelflife: ['', [Validators.required]],
      binnumber: ['', [Validators.required]],
      itemlocation: ['', [Validators.required]]
    });
    this.StockModelForm1 = this.formBuilder.group({
      users: this.formBuilder.array([{
        locatorid: ['', [Validators.required]],
        rackid: ['', [Validators.required]],
        binid: ['', [Validators.required]],
        shelflife: ['', [Validators.required]],
        binnumber: ['', [Validators.required]],
        itemlocation: ['', [Validators.required]],
        quantity: [0, [Validators.required]],
      }])
    });
  }

  //get Material Rquest based on login employee && po no
  getMaterialRequestlist(employeeno) {
    this.wmsService.gettransferdataforapproval(employeeno).subscribe(data => {
      this.requestListall = data;
      this.requestListall.forEach(item => {
        item.showtr = false;
      });
      this.getdata();
    });
  }
  getdata() {
    this.requestList = [];
     var typ = this.returntype;
    
    if (!isNullOrUndefined(this.requestedid) && this.requestedid != "") {
      var tid = this.requestedid;
      this.requestList = this.requestListall.filter(function (element, index) {
        return (element.status == typ && element.transferid == tid );
      });
      this.requestid = "";
    }
    else {
      this.requestList = this.requestListall.filter(function (element, index) {
        return (element.status == typ);
      });
    }
   

  }

  showattachdata(rowData: materialtransferMain) {
    debugger;
    this.hideapproval = false;
    if (rowData.status != "Pending") {
      this.hideapproval = true;
    }
    this.materialtransferdetil = [];
    this.materialapproverlistdetil = [];
    this.approvalremarks = rowData.approvalremarks;
    this.gatepassdialog = true;
    this.transferedfromlbl = rowData.projectcodefrom + "(PM-" + rowData.projectmanagerfrom + ")";
    this.transferedtolbl = rowData.projectcode + "(PM-" + rowData.projectmanagerto + ")";
    this.materialtransferdetil = rowData.materialdata;
    this.materialapproverlistdetil = rowData.approverdata;
    this.selectedrows.push(rowData);
    
   
  }

  showattachtrdata(rowData: materialtransferMain) {
    debugger;
    rowData.showtr = !rowData.showtr;
  }

  hideDG() {
    this.selectedrows = [];
    this.approvalremarks = "";
  }

  approve(isapproved: boolean) {
    this.selectedrows.forEach(item => {
      item.isapproved = isapproved;
      item.approvalremarks = this.approvalremarks;
      item.approverid = this.employee.employeeno;
    });
    var msg = isapproved ? "Approved" : "Rejected";
    var errormsg = isapproved ? "Approval" : "Rejection";

    this.spinner.show();
    this.wmsService.approvetransfermaterial(this.selectedrows).subscribe(data => {
      this.spinner.hide();
      if (data) {
        this.gatepassdialog = false;
        this.messageService.add({ severity: 'success', summary: '', detail: 'Material tarnsfer ' + msg });
        this.getMaterialRequestlist(this.employee.employeeno);
      }

      else {
        this.messageService.add({ severity: 'error', summary: '', detail: errormsg+' Failed' });
      }
    });


  }



 
  
 
 
 
  
 
 
 
 
  
 
 

 

 


 

  
  
}
