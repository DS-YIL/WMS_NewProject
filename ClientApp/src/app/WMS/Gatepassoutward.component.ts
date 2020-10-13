import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { gatepassModel, materialistModel, FIFOValues, outwardmaterialistModel} from '../Models/WMS.Model';
import { isNullOrUndefined } from 'util';
import { DatePipe } from '@angular/common';
import { ConfirmationService } from 'primeng/api';
import { trigger, state, transition, style, animate } from '@angular/animations';
@Component({
  selector: 'app-GatePassoutward',
  templateUrl: './GatePassoutward.component.html',
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
  providers: [DatePipe]
})
export class GatePassoutwardComponent implements OnInit {
  AddDialog: boolean;
  id: any;
  roindex: any;
  Oldestdata: any;
  constructor(private ConfirmationService: ConfirmationService, private formBuilder: FormBuilder, private messageService: MessageService, private datePipe: DatePipe, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }
  todayDate = this.datePipe.transform(new Date(), 'yyyy-MM-dd');
  public formName: string;
  public txtName; GatepassTxt: string;
  public dynamicData = new DynamicSearchResult();
  public showList: boolean = false;
  public searchItems: Array<searchList> = [];
  public selectedlist: Array<searchList> = [];
  public searchresult: Array<object> = [];

  public gatepasslist: Array<any> = [];
  public totalGatePassList: Array<any> = [];
  public gatepassModelList: Array<gatepassModel> = [];
  public employee: Employee;
  public gatepassdialog; updateReturnedDateDialog: boolean = false;
  public gatepassModel: gatepassModel;
  public materialistModel: materialistModel;
  public material: any;
  public gpIndx: number;
  public date: Date = null;
  public approverstatus: string;
  public mindate: Date;
  public showdialog: boolean = false;
  public txtDisable: boolean = true;
  public FIFOvalues: FIFOValues;
  pageid: string = "";
  gatepasstyp: string = "";
  nonreturn: boolean = false;
  returnable: boolean = false;
  materialListDG: outwardmaterialistModel[] = [];
  selectedmats: outwardmaterialistModel[];
  selectedmdata : outwardmaterialistModel[] = [];
  showmatDialog: boolean = false;
  public itemlocationData: Array<any> = [];
  DGgatepassid: string = "";
  DGgatepasstype: string = "";
  DGvendorname: string = "";
  isoutward: boolean = false;
  isinward: boolean = false;
  outindate: Date;
  datetype: string = "";
  ishistorydata: boolean = false;
  returntype: string = "";
  pgremarks: string = "";
  fromdateview: string = "";
  fromdateview1: string = "";
  cols: any[];
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.gatepassModel = new gatepassModel();
    this.materialistModel = new materialistModel();
    this.selectedmdata = [];
    //set expected date as future date
    this.mindate = new Date(new Date().setDate(new Date().getDate() + 1));
    this.nonreturn = false;
    this.returnable = false;
    this.returntype = "out";
    var dt = new Date();
    this.fromdateview = this.datePipe.transform(dt, this.constants.dateFormat);
    this.fromdateview1 = this.datePipe.transform(dt, 'yyyy-MM-dd hh:mm:ss');
    this.cols = [
      { field: 'gatepassid', header: 'Vin' },
      { field: 'gatepasstype', header: 'Year' },
      { field: 'vendorname', header: 'Brand' },
      { field: 'name', header: 'Color' },
      { field: 'requestedon', header:'requestedon'}
    ];

    this.route.params.subscribe(params => {
      if (params["pageid"]) {
        this.pageid = params["pageid"];
        if (this.pageid == "1") {
          this.gatepasstyp = "Returnable";
          this.returntype = "out";
          this.nonreturn = false;
          this.returnable = true;
          this.isoutward = true;
          this.datetype = "Outward date";
          this.getGatePassList();
          
        }
        else if (this.pageid == "2") {
          this.gatepasstyp = "Non-Returnable";
          this.nonreturn = true;
          this.returnable = false;
          this.isinward = false;
          this.isoutward = true;
          this.getGatePassList();
        }
      }
    });
  }

  onfromSelectMethod(event) {
    this.fromdateview1 = "";
    if (event.toString().trim() !== '') {
      this.fromdateview1 = this.datePipe.transform(event, 'yyyy-MM-dd hh:mm:ss');
    }
  }
  onRowExpand(event: any) {
    debugger;
    var data = event.data;
    this.DGgatepassid = data.gatepassid;
    this.DGgatepasstype = data.gatepasstype;
    this.DGvendorname = data.vendorname;
    this.outindate = null;
    var res = this.gatepassModelList.filter(li => li.gatepassid == data.gatepassid);
    this.materialListDG = JSON.parse(JSON.stringify(res)) as outwardmaterialistModel[];
    //this.showmatDialog = true;

  }
  getdata() {
    debugger;
    if (this.returntype == "out") {
      this.getoutwarddata();

    }
    else if (this.returntype == "in") {
      this.getinwarddata();
    }
  }

  showdetails(data: any) {
    var dt = new Date();
    this.fromdateview = this.datePipe.transform(dt, this.constants.dateFormat);
    this.DGgatepassid = data.gatepassid;
    this.DGgatepasstype = data.gatepasstype;
    this.DGvendorname = data.vendorname;
    this.outindate = null;
    var res = this.gatepasslist.filter(li => li.gatepassid == data.gatepassid);
    this.materialListDG = JSON.parse(JSON.stringify(res)) as outwardmaterialistModel[];
    this.showmatDialog = true;

  }
  closeDG() {
    this.showmatDialog = false;
  }

  getoutwarddata() {
    this.isoutward = true;
    this.isinward = false;
    this.ishistorydata = false;
    this.datetype = "Outward date";
    this.getGatePassList();
  }
  getinwarddata() {
    this.isoutward = false;
    this.isinward = true;
    this.ishistorydata = false;
    this.datetype = "Inward date";
    this.gatepassModelList = [];
    this.gatepasslist = [];
    this.getGatePassList();
  }
  getprevdata(type: number) {
    this.isoutward = false;
    this.isinward = false;
    this.ishistorydata = true;
    alert(type);
  }

  onRowSelect(event) {

  }

  onRowUnselect(event) {
   
  }
 

  

  //get gatepass list
  getGatePassList() {
    debugger;
    this.gatepassModelList = [];
    this.gatepasslist = [];
    if (this.nonreturn) {
      this.wmsService.nonreturngetGatePassList("2").subscribe(data => {
        debugger;
        this.totalGatePassList = data;
        this.totalGatePassList = this.totalGatePassList.filter(li => li.outwardedqty != li.issuedqty);
        this.gatepasslist = [];
        this.gatepasslist = this.totalGatePassList;
        this.gatepassModelList = [];
        this.prepareGatepassList();
      });

    }
    else if (this.returnable) {
      this.wmsService.nonreturngetGatePassList("1").subscribe(data => {
        debugger;
        this.totalGatePassList = data;

        if (this.isinward) {
          this.totalGatePassList = this.totalGatePassList.filter(li => li.outwarddate != null && li.securityinwarddate == null);
        }
        else {
          this.totalGatePassList = this.totalGatePassList.filter(li => li.outwardedqty != li.issuedqty);
        }
        this.gatepasslist = [];
        this.gatepasslist = this.totalGatePassList;
        this.gatepassModelList = [];
        this.prepareGatepassList();
      });

    }
   
  }

  updateoutinward() {
    debugger;
    var senddata = this.materialListDG;
    this.selectedmdata = [];
    var tdate = new Date();
    //if (this.isoutward) {
    //  var invalidrcv = senddata.filter(function (element, index) {
    //    return (element.outwardqty != 0);
    //  });
    //  if (invalidrcv.length == 0) {
    //    this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Outward Quantity' });
    //    return;
    //  }
    //  senddata = senddata.filter(function (element, index) {
    //    return (element.outwardqty != 0);
    //  });
    //}
    //else {
    //  var invalidrcv = senddata.filter(function (element, index) {
    //    return (element.inwardqty != 0);
    //  });
    //  if (invalidrcv.length == 0) {
    //    this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Inward Quantity' });
    //    return;
    //  }
    //  senddata = senddata.filter(function (element, index) {
    //    return (element.inwardqty != 0);
    //  });

    //}
    senddata.forEach(item => {
      let mdata = new outwardmaterialistModel();
      mdata.gatepassmaterialid = item.gatepassmaterialid;
      mdata.gatepassid = this.DGgatepassid;
      mdata.remarks = this.pgremarks;
      mdata.movedby = this.employee.employeeno;
      var date = this.datePipe.transform(tdate, 'yyyy-MM-dd hh:mm:ss');
      if (this.isoutward) {
        mdata.outwarddatestring = this.fromdateview1;
        mdata.movetype = "out";
        mdata.outwardqty = item.issuedqty;
      }
      else if (this.isinward) {
        mdata.inwarddatestring = this.fromdateview1;
        mdata.movetype = "in";
        mdata.inwardqty = item.inwardqty;
      }
      this.selectedmdata.push(mdata);
    })
    this.wmsService.updateoutinward(this.selectedmdata).subscribe(data => {
      this.pgremarks = "";
      if (this.isinward) {
        this.messageService.add({ severity: 'success', summary: ' ', detail: 'Inwarded successfully.' });
      }
      else if (this.isoutward) {
        this.messageService.add({ severity: 'success', summary: ' ', detail: 'Outwarded successfully.' });
      }
      this.resetpage();
      
    })

  }
  checkoutqty(enrtered: number, qty: number,out:number, data: any) {
    if (enrtered < 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Negative value not allowed.' });
      data.outwardqty = 0;
      return;
    }

    if (enrtered > (qty-out)) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Outward quantity cannot be greater than Pending quantity.' });
      data.outwardqty = 0;
      return;
    }
    
  }

  checkinqty(enrtered: number, qty: number, data: any) {
    if (enrtered < 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Negative value not allowed.' });
      data.outwardqty = 0;
      return;
    }

    if (enrtered > qty) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Intward quantity cannot be greater than Pending quantity.' });
      data.outwardqty = 0;
      return;
    }
  }

  resetpage() {
    this.showmatDialog = false;
    this.gatepassModelList = [];
    this.gatepasslist = [];
    this.materialListDG = [];
    this.selectedmats = [];
    this.selectedmdata = [];
    this.outindate = null;
    this.getGatePassList();

  }

 
 

  //prepare list based on gate pass id
  prepareGatepassList() {
    this.gatepasslist.forEach(item => {
      var res = this.gatepassModelList.filter(li => li.gatepassid == item.gatepassid);
      if (res.length == 0) {
        item.materialList = [];
        var result = this.gatepasslist.filter(li => li.gatepassid == item.gatepassid && li.gatepassmaterialid != "0" && li.deleteflag == false);
        for (var i = 0; i < result.length; i++) {
          var material = new outwardmaterialistModel();
          material.gatepassmaterialid = result[i].gatepassmaterialid;
          material.materialid = result[i].materialid;
          material.materialdescription = result[i].materialdescription;
          material.quantity = result[i].quantity;
          material.materialcost = result[i].materialcost;
          material.remarks = result[i].remarks;
          material.outwardqty = result[i].outwardqty;
          material.inwardqty = result[i].inwardqty; 
          material.expecteddate = new Date(result[i].expecteddate);
          material.mgapprover = result[i].mgapprover;
          material.fmapprover = result[i].fmapprover;
          item.materialList.push(material);
        }

        this.gatepassModelList.push(item);
      }
    });
  }

 
}
