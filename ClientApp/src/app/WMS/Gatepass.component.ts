import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList, userAcessNamesModel, rbamaster } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { gatepassModel, materialistModel, FIFOValues, materialList, ddlmodel, gatepassprintdoc } from '../Models/WMS.Model';
import { isNullOrUndefined } from 'util';
import { DatePipe, DecimalPipe } from '@angular/common';
import { ConfirmationService } from 'primeng/api';
@Component({
  selector: 'app-GatePass',
  templateUrl: './GatePass.component.html',
  providers: [DatePipe, DecimalPipe]
})
export class GatePassComponent implements OnInit {
  AddDialog: boolean;
  selectedStatus: string = "Pending";
  id: any;
  public materialList: Array<any> = [];
  roindex: any;
  Oldestdata: any;
  constructor(private ConfirmationService: ConfirmationService, private formBuilder: FormBuilder, private decimalPipe: DecimalPipe, private messageService: MessageService, private datePipe: DatePipe, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }
  todayDate = this.datePipe.transform(new Date(), 'yyyy-MM-dd');
  public formName: string;
  public txtName; GatepassTxt: string;
  public dynamicData = new DynamicSearchResult();
  edit: boolean = false;
  public showList: boolean = false;
  public searchItems: Array<searchList> = [];
  public selectedlist: Array<searchList> = [];
  public searchresult: Array<object> = [];
  rbalist: rbamaster[] = [];
  selectedrba : rbamaster;

  public gatepasslist: Array<any> = [];
  public totalGatePassList: Array<any> = [];
  public gatepassModelList: Array<gatepassModel> = [];
  public gatepasslistmodel: Array<gatepassModel> = [];
  public gatepassFiltered: Array<gatepassModel> = [];
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
  public gatePassApprovalList: Array<any> = [];
  public displaydetail; disableGPBtn: boolean = false;
  public selectedRow: any;
  public itemlocationData: Array<any> = [];
  userrolelist: userAcessNamesModel[] = [];
  public defaultmaterials: materialList[] = [];
  public defaultmaterialids: materialList[] = [];
  public defaultmaterialidescs: materialList[] = [];
  public defaultuniquematerialids: materialList[] = [];
  public defaultuniquematerialidescs: materialList[] = [];
  filteredmats: any[];
  filteredmatdesc: any[];
  emailgateid: string = "";
  gtptype: string = "";
  tempreturneddate: any;
  currdate: Date;
  reasonlist: ddlmodel[] = [];
  isotherreason: boolean = false;
  otherreason: string = "";
  materialtype: string;
  exportColumns: any[];
  cols: any[];
  gatepassprintmodel: gatepassprintdoc[] = [];
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.gatepassprintmodel = [];
    this.cols = [
      { field: 'slno', header: 'Sl.No.' },
      { field: 'materialdescription', header: 'Material Description' },
      { field: 'quantity', header: 'Quantity' },
      { field: 'remarks', header: 'Remarks' },
    ];

    this.exportColumns = this.cols.map(col => ({ title: col.header, dataKey: col.field }));
    this.isotherreason = false;
    this.otherreason = "";
    this.materialtype = "project";
    this.gatepassModel = new gatepassModel();
    this.reasonlist = [];
    this.getgatepassreason();
    this.materialistModel = new materialistModel();
    this.selectedrba = new rbamaster();
    if (localStorage.getItem("userroles")) {
      this.userrolelist = JSON.parse(localStorage.getItem("userroles")) as userAcessNamesModel[];
    }
    if (localStorage.getItem("rbalist")) {
      this.rbalist = JSON.parse(localStorage.getItem("rbalist")) as rbamaster[];
      var filterdt = this.rbalist.filter(o => o.roleid == parseInt(this.employee.roleid));
      if (filterdt.length > 0) {
        this.selectedrba = filterdt[0];
      }
      
    }
    debugger;
    this.emailgateid = this.route.snapshot.queryParams.gatepassid;
    this.gtptype = "";
    this.tempreturneddate = null;

    this.getGatePassList();
    this.GatepassTxt = "Gate Pass - Request Materials"


    //var roles = this.userrolelist.filter(li => li.roleid == 8);

    if (this.selectedrba.gatepass_approval) //for approver
      this.approverstatus = "Pending";

    else
      this.approverstatus = "";

    //set expected date as future date
    this.mindate = new Date(new Date().setDate(new Date().getDate() + 1));
    this.getdefaultmaterialsforgatepass();
  }


  resetDG() {
    this.disableGPBtn = false;
  }

  getgatepassreason() {
    this.reasonlist = [];
    this.wmsService.getgatepassreason().subscribe(data => {
      debugger;
      this.reasonlist = data;

    });
  }
  


  //Adding new material - Gayathri
  addNewMaterial() {
    
    this.gatePassChange();
    debugger;
    if (this.gatepassModel.materialList.length > 0) {
      if (isNullOrUndefined(this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].materialid) || this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].materialid == "") {
        if (this.gatepassModel.isnonproject) {
          this.messageService.add({ severity: 'error', summary: '', detail: 'Enter material' });
        }
        else{
          this.messageService.add({ severity: 'error', summary: '', detail: 'Select material from list' });
        }
        
        return false;
      }
      if (isNullOrUndefined(this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].materialdescription) || this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].materialdescription == "") {
        if (this.gatepassModel.isnonproject) {
          this.messageService.add({ severity: 'error', summary: '', detail: 'Enter material description' });
        }
        else {
          this.messageService.add({ severity: 'error', summary: '', detail: 'Select material description from list' });
        }
        return false;
      }
      else if (this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].quantity <= 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Quantity' });
        return false;
      }
      else if (this.gatepassModel.gatepasstype == "Returnable" && isNullOrUndefined(this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].expecteddate)) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Select Expected Date' });
        return false;
      }
      else {
        debugger;
        this.materialistModel = { materialid: "", gatepassmaterialid: "0", materialdescription: "", quantity: 0, materialcost: 0, remarks: " ", expecteddate: this.date, returneddate: this.date, issuedqty: 0, showdetail: false, materiallistdata: [] };
        this.gatepassModel.materialList.push(this.materialistModel);
      }

    }
    else {
      if (this.gatepassModel.materialList.length <= 0) {
        this.materialistModel = { materialid: "", gatepassmaterialid: "0", materialdescription: "", quantity: 0, materialcost: 0, remarks: " ", expecteddate: this.date, returneddate: this.date, issuedqty: 0, showdetail: false, materiallistdata: [] };
        this.gatepassModel.materialList.push(this.materialistModel);
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Add Material to create Gate Pass' });
        return false;
      }

    }
    
    //if (this.gatepassModel.materialList.length == 0 || isNullOrUndefined(this.material)) {
    //  this.materialistModel = { materialid: "", gatepassmaterialid: "0", materialdescription: "", quantity: 0, materialcost: "0", remarks: " ", expecteddate: this.date, returneddate: this.date, issuedqty: 0 };
    //  this.gatepassModel.materialList.push(this.materialistModel);
    //  this.material = "";
    //}
    ////check if materiallist is empty and gatepass materialid is null
    //else if (!this.material && !this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].materialid) {
    //  this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'please Add Material' });
    //  return false;
    //}
    //else if (this.gatepassModel.gatepasstype == "Returnable" && this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].expecteddate == undefined) {
    //  this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Please select Expected Date' });
    //  return false;
    //}
    //else {
    //  //Check if material code is already entered
    //  if (this.gatepassModel.materialList.filter(li => li.materialid == this.material.code && li.gatepassmaterialid != "0").length > 0) {
    //    this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Material already exist' });
    //    return false;
    //  }

    //  debugger;
    //  this.gatePassChange();
    //  if (this.material) {
    //    this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].materialid = this.material.code;
    //    this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].materialdescription = this.material.name;
    //    this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].expecteddate = new Date(this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].expecteddate).toLocaleDateString();
    //    this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].returneddate = this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].returneddate != null ? new Date(this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].returneddate).toLocaleDateString() : undefined;

    //    // this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].returneddate = this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].expecteddate;
    //  }
    //  if (this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].materialid && this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].quantity) {
    //    this.wmsService.checkMaterialandQty(this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].materialid, this.materialistModel.quantity).subscribe(data => {
    //      if (data == "true") {

    //        this.materialistModel = { materialid: "", gatepassmaterialid: "0", materialdescription: "", quantity: 0, materialcost: "0", remarks: " ", expecteddate: this.date, returneddate: this.date, issuedqty: 0 };
    //        this.gatepassModel.materialList.push(this.materialistModel);
    //        this.material = "";

    //        //alert(this.gatepassModel.materialList[1].materialid);
    //      }
    //      else
    //        this.messageService.add({ severity: 'error', summary: 'Error Message', detail: data });
    //    });
    //  }
    //  else {
    //    if (!this.materialistModel.materialid)
    //      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'select material from list' });
    //    else if (!this.materialistModel.quantity)
    //      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Enter Quantity' });
    //  }


    //}



  }
  onreasonchanged(event) {
    this.isotherreason = false;
   var rsn = event.target.value;
    if (rsn == "Other") {
      this.isotherreason = true;
   }
 }

  onComplete(qty: any, avlqty: any, index: any) {
    if (qty == 0 || qty < 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Quantity must be Greater than zero' });
      this.gatepassModel.materialList[index].quantity = 0;
      return;
    }
    if (qty > avlqty) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Quantity exceeded available quqntity' });
      this.gatepassModel.materialList[index].quantity = 0;
      return;
    }
    //else {
    //  this.wmsService.checkMaterialandQty(material, qty).subscribe(data => {
    //    if (data == "true") {

    //    }
    //    else {
    //      this.messageService.add({ severity: 'error', summary: '', detail: data });
    //      this.gatepassModel.materialList[index].quantity = 0;
    //      return false;
    //    }
    //  });
    //}
  }

  getdefaultmaterialsforgatepass() {
    this.defaultmaterials = []
    this.wmsService.getgatepassMaterialRequestlist().subscribe(data => {
      this.defaultmaterials = data;
      this.setmatdesclist(this.defaultmaterials);
    });
  }

  setmatdesclist(datax: materialList[]) {
    debugger;
    var listdata = datax;
    this.defaultmaterialidescs = [];
    this.defaultmaterialids = [];
    this.defaultuniquematerialids = [];
    this.defaultuniquematerialidescs = [];
    try {
      listdata.forEach(item => {
        debugger;
        var mat = item.material;
        var desc = item.materialdescription;
        var dt1 = this.defaultmaterialids.filter(function (element, index) {
          return (String(element.material).toLowerCase() == String(mat).toLowerCase());
        });
        if (dt1.length == 0) {
          this.defaultmaterialids.push(item);
        }
        var dt2 = this.defaultmaterialidescs.filter(function (element, index) {
          return (String(element.materialdescription).toLowerCase() == String(desc).toLowerCase());
        });
        if (dt2.length == 0) {
          this.defaultmaterialidescs.push(item);
        }

      });
      this.defaultuniquematerialids = this.defaultmaterialids;
      this.defaultuniquematerialidescs = this.defaultmaterialidescs;

    } catch (Exception) {

      //alert(Exception.message)


    } 
   

  }

  setmatlist(datax: materialList[]) {
    var listdata = datax;
    this.defaultmaterialids = [];
    listdata.forEach(item => {
      var mat = item.material;
      var dt1 = this.defaultmaterialids.filter(function (element, index) {
        return (element.material.toLowerCase() == String(mat).toLowerCase());
      });
      if (dt1.length == 0) {
        this.defaultmaterialids.push(item);
      }

    });
  }

  setdesclist(datax: materialList[]) {
    var listdata = datax;
    this.defaultmaterialidescs = [];
    listdata.forEach(item => {
      var desc = item.materialdescription;
      var dt2 = this.defaultmaterialidescs.filter(function (element, index) {
        return (element.materialdescription.toLowerCase() == String(desc).toLowerCase());
      });
      if (dt2.length == 0) {
        this.defaultmaterialidescs.push(item);
      }

    });
  }

  filtermats(event, data: any) {
    if (!isNullOrUndefined(data.materialdescription) && data.materialdescription != "") {
      var senddata = this.defaultmaterials.filter(function (element, index) {
        return (element.materialdescription == data.materialdescription);
      });
      this.setmatlist(senddata);
    }
    else {
      this.defaultmaterialids = this.defaultuniquematerialids;
    }
    this.filteredmats = [];
    for (let i = 0; i < this.defaultmaterialids.length; i++) {
      let brand = this.defaultmaterialids[i].material;
      if (brand.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredmats.push(brand);
      }

    }
  }

  filtermatdescs(event, data: any) {
    if (!isNullOrUndefined(data.materialid) && data.materialid != "") {
      var senddata = this.defaultmaterials.filter(function (element, index) {
        return (element.material == data.materialid);
      });
      this.setdesclist(senddata);
    }
    else {
      this.defaultmaterialidescs = this.defaultuniquematerialidescs;
    }
    this.filteredmatdesc = [];
    for (let i = 0; i < this.defaultmaterialidescs.length; i++) {
      let pos = this.defaultmaterialidescs[i].materialdescription;
      if (pos.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredmatdesc.push(pos);
      }

    }
  }

 

  //bind materials based search
  public bindSearchListData(event: any, name?: string) {
    debugger;
    var searchTxt = event.query;
    if (searchTxt == undefined)
      searchTxt = "";
    searchTxt = searchTxt.replace('*', '%');
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.tableName = this.constants[name].tableName + " ";
    this.dynamicData.searchCondition = "" + this.constants[name].condition;
    this.dynamicData.searchCondition += "sk.materialid" + " ilike '" + searchTxt + "%'" + " and sk.availableqty>=1";
    //this.materialistModel.materialcost = "";
    this.wmsService.GetMaterialItems(this.dynamicData).subscribe(data => {
      debugger;

      this.searchresult = data;
      this.gatepasslistmodel = data;
      this.searchItems = [];
      var fName = "";
      this.searchresult.forEach(item => {
        fName = item[this.constants[name].fieldName];
        if (name == "ItemId")
          //fName = item[this.constants[name].fieldName] + " - " + item[this.constants[name].fieldId];
          fName = item[this.constants[name].fieldId];
        // var value = { listName: name, name: fName, code: item[this.constants[name].fieldId] };
        var value = { code: item[this.constants[name].fieldId] };
        //this.materialistModel.materialcost = data[0].materialcost;
        this.searchItems.push(item[this.constants[name].fieldId]);
      });
    });
  }

  getcolspan() {
    var colspan = 7;
    if (!this.selectedrba.gatepass_approval) {
      colspan = 8;
    }
    if (!this.selectedrba.gatepass_approval && (!this.selectedrba.material_issue)) {
      colspan = 9;
    }
    if (!this.selectedrba.gatepass_approval && (this.selectedrba.material_issue)) {
      colspan = 9;
    }
    if (this.selectedStatus == "Issued" && (this.selectedrba.material_issue)) {
      colspan = colspan + 1;
    }
    if (this.selectedrba.gate_pass) {
      colspan = colspan + 1;
    }

    return colspan;

  }

  //Get details
  showdetails(data: any, index: any) {

    debugger;
    this.materialList = [];
    this.selectedRow = index;
    this.displaydetail = true;
    var gatepassId = data.gatepassid;
    this.gtptype = data.gatepasstype;
    this.tempreturneddate = data.returneddate;
    this.bindMaterilaDetails(gatepassId);
    data.showdetail = !data.showdetail;

    //debugger;

    //if (this.employee.roleid == "8") {
    //  this.getGatePassHistoryList(gatepassId);
    //}
  }

  //get gatepass list
  bindMaterilaDetails(gatepassId: any) {
    this.gatepassFiltered[this.selectedRow].materiallistarray = [];
    this.wmsService.gatepassmaterialdetail(gatepassId).subscribe(data => {
      this.materialList = data;
      this.gatepassFiltered[this.selectedRow].materiallistarray = data;
      this.gatepassModel = this.materialList[0];
      console.log(this.gatepassModel);

    });
  }

  getGatePassHistoryList(gatepassId: any) {
    this.wmsService.getGatePassApprovalHistoryList(gatepassId).subscribe(data => {
      this.gatePassApprovalList = data;
      //if (this.gatePassApprovalList.filter(li => li.approverid == this.employee.employeeno)[0].approverstatus != "Approved")
      //this.btnDisable = false;
    });
  }

  //get gatepass list
  getGatePassList() {
    this.spinner.show();
    this.wmsService.getGatePassList().subscribe(data => {
      this.spinner.hide();
      debugger;
      this.totalGatePassList = data;
      if (isNullOrUndefined(this.totalGatePassList)) {
        this.totalGatePassList = [];
      }
      //filtering the based on logged in user if role id is 8(Admin)
      if (this.selectedrba.gate_pass) {
        this.totalGatePassList = this.totalGatePassList.filter(li => li.requestedby == this.employee.employeeno);
      }
      this.gatepasslist = [];
      if (this.selectedrba.gatepass_approval) {
        this.gatepasslist = this.totalGatePassList.filter(li => li.gatepasstype == 'Non Returnable' && (li.approverstatus == this.approverstatus || li.approverstatus == null));
      }
      else if (this.selectedrba.material_issue) {
        this.totalGatePassList = this.totalGatePassList.filter(li => li.isnonproject != true);
        this.totalGatePassList.forEach(item => {
          if (item.gatepasstype == "Returnable" && item.approverstatus == "Approved")
            this.gatepasslist.push(item);
          if (item.gatepasstype == "Non Returnable" && item.fmapprovedstatus == "Approved")
            this.gatepasslist.push(item);
        })

      }
      else {
        //debugger;
        this.gatepasslist = this.totalGatePassList;
      }
      if (this.emailgateid) {
        this.gatepasslist = this.gatepasslist.filter(li => li.gatepassid == this.emailgateid);
      }

      this.gatepassModelList = [];
      this.prepareGatepassList();
      if (this.selectedrba.material_issue) {
        this.onSelectStatus();
      }
      else {
        this.gatepassFiltered = this.gatepassModelList;
      }

    });
  }

  searchGatePassList() {
    if (this.approverstatus != "0") {
      debugger;
      if (this.approverstatus == "Pending")
        this.gatepasslist = this.totalGatePassList.filter(li => li.gatepasstype == 'Non Returnable' && (li.approverstatus == this.approverstatus || li.approverstatus == null));
      else
        this.gatepasslist = this.totalGatePassList.filter(li => li.gatepasstype == 'Non Returnable' && li.approverstatus == this.approverstatus);
    }
    else
      this.gatepasslist = this.totalGatePassList.filter(li => li.gatepasstype == 'Non Returnable');
    this.gatepassModelList = [];
    this.prepareGatepassList();
  }
  exportPdf(gatepassobject: gatepassModel,materialarray : any[]) {
    // debugger;
    import("jspdf").then(jsPDF => {
      import("jspdf-autotable").then(x => {
        const doc = new jsPDF.default(0, 0);
        var device = "";
        var model = "";
        var make = "";
        var pageHeight = doc.internal.pageSize.height || doc.internal.pageSize.getHeight();
        var pageWidth = doc.internal.pageSize.width || doc.internal.pageSize.getWidth();
        doc.setDrawColor(0);
        doc.setFillColor(255, 255, 255);
        doc.rect(10, 10, 190, pageHeight-20, 'FD');
        var img = new Image()
        img.src = './assets/banner1.jpg'
        doc.addImage(img, 'jpg', 12, 12, 40, 15);
        doc.setFontSize(14);
        doc.text("YOKOGAWA INDIA LIMITED ", 120, 17);
        doc.setFontSize(12);
        doc.text("Plot No. 96, 3rd Cross, Hosur Road", 120, 22);
        doc.setFontSize(12);
        doc.text("Electronic city, Banglore - 560100", 120, 27);
        doc.line(10, 30, 200, 30);
        doc.text("Gate Pass [" + gatepassobject.gatepasstype + " Items]", pageWidth / 2, 36, 'center');
        doc.line(10, 40, 200, 40);
        doc.text("Name of the Person : " + gatepassobject.name + " ", 12, 45);
        if (gatepassobject.gatepasstype == "Returnable") {
          doc.text("Document No: QM3X-0854F-008 ", 125, 45);
        }
        else if (gatepassobject.gatepasstype == "Non Returnable") {
          doc.text("Document No: QM3X-0854F-009 ", 125, 45);
        }

        doc.text("Representing: " + gatepassobject.vendorname + " ", 12, 52);
        doc.text("No: " + gatepassobject.gatepassid + " ", 125, 52);
        doc.text("Document Ref: " + gatepassobject.referenceno + " ", 12, 59);
        var dt = this.datePipe.transform(gatepassobject.requestedon, 'dd/MM/yyyy');
        doc.text("Document Ref Date: " + dt + " ", 125, 59);
        doc.line(10, 62, 200, 62);
        this.gatepassprintmodel = [];
        var sln = 1;
        materialarray.forEach(element => {
          let obj = new gatepassprintdoc();
          obj.slno = String(sln);
          obj.materialdescription = String(element.materialdescription);
          obj.quantity = String(element.quantity);
          obj.remarks = String(element.remarks)
          this.gatepassprintmodel.push(obj);
          sln = sln + 1;

        })
       
        doc.line(10, pageHeight - 30, 200, pageHeight - 30);
       
        if (gatepassobject.gatepasstype == "Returnable") {
          doc.text("Authorized by: " + gatepassobject.approvername, 12, pageHeight - 22);
        }
        else if (gatepassobject.gatepasstype == "Non Returnable") {
          doc.text("Authorized by: " + gatepassobject.fmapprover, 12, pageHeight - 22);
        }
        doc.text("Prepared by:" + gatepassobject.name, 12, pageHeight - 15);
        doc.text("Signature of the Receiver", 145, pageHeight - 15);
        doc.autoTable({
          columnStyles: { 0: { halign: 'center' } }, // Cells in first column centered and green
          margin: { top: 60 },
        })
        var page = this;
        doc.autoTable(this.exportColumns, this.gatepassprintmodel);

        //var reportTitle = "Device Report" + device + model + make;
        //var splitTitle = doc.splitTextToSize(reportTitle, 180);
        //if (device == "" && model == "" && make == "") {
        //  doc.text(splitTitle, 85, 10);
        //}
        //else {
        //  doc.text(splitTitle, 20, 10);
        //}

        //doc.autoTable({
        //  styles: { fillColor: [255, 0, 0] },
        //  columnStyles: { 0: { halign: 'center', fillColor: [0, 255, 0] } }, // Cells in first column centered and green
        //  margin: { top: 10 },
        //})

        //doc.autoTable(this.exportColumns, this.PrintDocs);
        // doc.save('devicereport.pdf');
        doc.output('dataurlnewwindow');
      })
    })
  }
  alignCol(cell, data) {
  var col = data.column.index;
  if (col == 0 || col == 2) {
    cell.styles.halign = 'right';

    console.log('2 data.column.index', data.column.index);
    console.log('cell', cell);
    console.log('data', data);
  }
}

  //prepare list based on gate pass id
  prepareGatepassList() {
    //debugger;
    this.gatepasslist.forEach(item => {
      var res = this.gatepassModelList.filter(li => li.gatepassid == item.gatepassid);
      if (res.length == 0) {
        item.materialList = [];
        var result = this.gatepasslist.filter(li => li.gatepassid == item.gatepassid && li.gatepassmaterialid != "0" && li.deleteflag == false);
        for (var i = 0; i < result.length; i++) {
          var material = new materialistModel();
          material.gatepassmaterialid = result[i].gatepassmaterialid;
          material.materialid = result[i].materialid;
          material.materialdescription = result[i].materialdescription;
          material.quantity = result[i].quantity;
          material.materialcost = result[i].materialcost;
          material.remarks = result[i].remarks;
          material.showdetail = false;
          material.expecteddate = new Date(result[i].expecteddate);
          if (isNullOrUndefined(result[i].returneddate)) {
            material.returneddate = this.checkValiddate(result[i].returneddate) == "" ? undefined : this.checkValiddate(result[i].returneddate);
          }
          else {

            material.returneddate = this.checkValiddate(result[i].returneddate) == "" ? undefined : this.checkValiddate(result[i].returneddate);
          }

          item.materialList.push(material);
        }

        this.gatepassModelList.push(item);


      }
    });
  }

  //gatepass change
  gatePassChange() {
    if (this.gatepassModel.gatepasstype != "0")
      this.GatepassTxt = this.gatepassModel.gatepasstype + " - " + "Request Materials";
  }
  materialtypechanged(event: any) {
    var val = event.target.value;
    if (val == "nonproject") {
      this.gatepassModel.isnonproject = true;
    }
    else {
      this.gatepassModel.isnonproject = false;
    }
    this.gatepassModel.materialList = [];
    this.addNewMaterial();
  }
  //open gate pass dialog
  openGatepassDialog(gatepassobject: any, gpIndx: any, dialogstr: string) {
    debugger;
    this.displaydetail = false;
    this.approverListdata();
    this.gatepassdialog = true;
    this.gatepassModel = new gatepassModel();
    if (gatepassobject) {
      this.edit = true;
      this.gpIndx = gpIndx;
      if (!isNullOrUndefined(gatepassobject.reasonforgatepass) && gatepassobject.reasonforgatepass == "Other") {
        this.isotherreason = true;
      }
      else {
        this.isotherreason = false;
      }
      this.gatepassModel = gatepassobject;
      if (dialogstr == 'updateReturnedDateDialog') {
        var matdata = [];
        var materialdata = gatepassobject.materialList;
        materialdata.forEach(item => {
          debugger;
          var data2 = this.defaultmaterials.filter(function (element, index) {
            return (element.material == item.materialid && element.materialdescription == item.materialdescription);
          });
          if (data2.length > 0) {
            item.materialcost = data2[0].materialcost != null ? data2[0].materialcost : 0;
            item.availableqty = data2[0].availableqty != null ? data2[0].availableqty : 0;
          }
          var exisdata = matdata.filter(o => o.materialid == item.materialid && o.materialdescription == item.materialdescription);

          if (exisdata.length == 0) {
            matdata.push(item);
          }

          this.gatepassModel.materialList = matdata;

        })
      }
      if (dialogstr == 'gatepassdialogedit') {
        var matdata = [];
        var materialdata = gatepassobject.materialList;
        materialdata.forEach(item => {
          debugger;
          var data2 = this.defaultmaterials.filter(function (element, index) {
            return (element.material == item.materialid && element.materialdescription == item.materialdescription);
          });
          if (data2.length > 0) {
            item.materialcost = data2[0].materialcost != null ? data2[0].materialcost : 0;
            item.availableqty = data2[0].availableqty != null ? data2[0].availableqty : 0;
          }
          item.expected = new Date(item.expected);
          var exisdata = matdata.filter(o => o.materialid == item.materialid && o.materialdescription == item.materialdescription);
          
          

          if (exisdata.length == 0) {
            matdata.push(item);
          }

          this.gatepassModel.materialList = matdata;

        })
      }
      
     
    } else {
      if (this.materialtype == "nonproject") {
        this.gatepassModel.isnonproject = true;
      }
      else {
        this.gatepassModel.isnonproject = false;
      }
      this.gatepassModel.gatepasstype = "0";
      this.isotherreason = false;
      this.gatepassModel.reasonforgatepass = "0";
      this.gatepassModel.otherreason = "";
      this.materialistModel = { materialid: "", gatepassmaterialid: "0", materialdescription: "", quantity: 0, materialcost: 0, remarks: " ", expecteddate: this.date, returneddate: this.date, issuedqty: 0, showdetail: false, materiallistdata: [] };
      this.gatepassModel.materialList.push(this.materialistModel);
      this.material = "";
    }
    this.gatePassChange();
  }

  //add materials for gate pass
  addMaterial() {
    if (!this.material) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Add Material' });
      return false;
    }
    if (this.gatepassModel.materialList.filter(li => li.materialid == this.material.code).length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Material already exist' });
      return false;
    }
    this.gatePassChange();
    this.materialistModel.materialid = this.material.code;
    this.materialistModel.materialdescription = this.material.name;

  }

  //check date is valid or not
  checkValiddate(date: any) {
    try {
      if (!date || (this.datePipe.transform(date, this.constants.dateFormat) == "01/01/0001"))
        return "";
      else
        return date;
    }
    catch{
      return "";
    }
  }

 
 

  //Delete material for gatepass
  removematerial(id: number, matIndex: number) {
    debugger;
    this.gatepassModel.materialList.splice(matIndex, 1);
    if (id != 0) {
      this.wmsService.deleteGatepassmaterial(id).subscribe(data => {
        //this.gatepassModelList[this.gpIndx].materialList.splice(matIndex, 1);
        this.messageService.add({ severity: 'success', summary: '', detail: 'Material Deleted' });
      });
    }


  }

  //Check if material is already selected in material list drop down
  onMaterialSelected(material: any, index: any) {
    debugger;
    if (this.gatepassModel.materialList.filter(li => li.materialid == material).length > 1) {
      this.gatepassModel.materialList[index] = new materialistModel();
      this.gatepassModel.materialList[index].materialid = "";
      this.messageService.add({ severity: 'error', summary: '', detail: 'Material already exist' });
     
      return false;
    }
    //add material price

    var data = this.gatepasslistmodel.find(li => li.materialid == material);
    console.log(this.gatepasslistmodel);
    this.gatepassModel.materialList[index].materialcost = data["materialcost"] != null ? data["materialcost"] : 0;

  }

  onmaterialchanged(data: any, ind: number) {
    if (!isNullOrUndefined(data.materialdescription) && data.materialdescription != "") {
      var data1 = this.gatepassModel.materialList.filter(function (element, index) {
        return (element.materialid == data.materialid && element.materialdescription == data.materialdescription && index != ind);
      });
      if (data1.length > 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Material already exist' });
        this.gatepassModel.materialList[ind].materialid = "";
        this.gatepassModel.materialList[ind].materialdescription = "";
        this.gatepassModel.materialList[ind].materialcost = 0;
        this.gatepassModel.materialList[ind].quantity = 0;
        this.gatepassModel.materialList[ind].remarks = "";
        return false;
      }
    }
   
  }
  ondescriptionchanged(data: any, ind: number) {
    if (!isNullOrUndefined(data.materialid) && data.materialid != "") {
      var data1 = this.gatepassModel.materialList.filter(function (element, index) {
        return (element.materialid == data.materialid && element.materialdescription == data.materialdescription && index != ind);
      });
      if (data1.length > 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Material already exist' });
        this.gatepassModel.materialList[ind].materialid = "";
        this.gatepassModel.materialList[ind].materialdescription = "";
        this.gatepassModel.materialList[ind].materialcost = 0;
        this.gatepassModel.materialList[ind].quantity = 0;
        this.gatepassModel.materialList[ind].remarks = "";
        return false;
      }
    }

  }

  onMaterialSelected1(data: any, ind: number) {
    debugger;
    if (!isNullOrUndefined(data.materialdescription) && data.materialdescription != "") {
      var data1 = this.gatepassModel.materialList.filter(function (element, index) {
        return (element.materialid == data.materialid && element.materialdescription == data.materialdescription && index != ind);
      });
      if (data1.length > 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Material already exist' });
        this.gatepassModel.materialList[ind].materialid = "";
        this.gatepassModel.materialList[ind].materialdescription = "";
        this.gatepassModel.materialList[ind].materialcost = 0;
        this.gatepassModel.materialList[ind].quantity = 0;
        this.gatepassModel.materialList[ind].remarks = "";
        return false;
      }
      var data2 = this.defaultmaterials.filter(function (element, index) {
        return (element.material == data.materialid && element.materialdescription == data.materialdescription);
      });
      if (data2.length > 0) {
        data.materialdescription = data2[0].materialdescription;
        data.materialcost = data2[0].materialcost != null ? data2[0].materialcost : 0;
        data.availableqty = data2[0].availableqty != null ? data2[0].availableqty : 0;
        
      }
    }
    else {
      var senddata = this.defaultmaterials.filter(function (element, index) {
        return (element.material == data.materialid);
      });
      this.setdesclist(senddata);

    }


  }

  onDescriptionSelected(data: any, ind: number) {
    debugger;
    if (!isNullOrUndefined(data.materialid) && data.materialid != "") {
      var data1 = this.gatepassModel.materialList.filter(function (element, index) {
        return (element.materialid == data.materialid && element.materialdescription == data.materialdescription && index != ind);
      });
      if (data1.length > 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Material already exist' });
        this.gatepassModel.materialList[ind].materialid = "";
        this.gatepassModel.materialList[ind].materialdescription = "";
        this.gatepassModel.materialList[ind].materialcost = 0;
        this.gatepassModel.materialList[ind].quantity = 0;
        this.gatepassModel.materialList[ind].remarks = "";
        return false;
      }
      var data2 = this.defaultmaterials.filter(function (element, index) {
        return (element.material == data.materialid && element.materialdescription == data.materialdescription);
      });
      if (data2.length > 0) {
        data.materialdescription = data2[0].materialdescription;
        data.materialcost = data2[0].materialcost != null ? data2[0].materialcost : 0;
        data.availableqty = data2[0].availableqty != null ? data2[0].availableqty : 0;

      }
    }
    else {
      var senddata = this.defaultmaterials.filter(function (element, index) {
        return (element.materialdescription == data.materialdescription);
      });
      this.setmatlist(senddata);

    }


  }

  

  //update return dated based on role
  updateReturnedDate(gatepassobject: any) {
    this.gatepassModel = new gatepassModel();
    if (gatepassobject) {
      this.gatepassModel = gatepassobject;
      this.onSubmitgatepassData();
    }
  }


  //saving gatepass details
  onSubmitgatepassDetails() {
    debugger;
    //alert("entered");
    if (this.gatepassModel.gatepasstype != "0") {
      this.gatepassModel.requestedby = this.employee.employeeno;

      //loop all the materiallist
      for (var i = 0; i < this.gatepassModel.materialList.length; i++) {
        this.gatepassModel.materialList[i].expecteddate = this.gatepassModel.materialList[i].expecteddate != null ? new Date(this.gatepassModel.materialList[i].expecteddate).toLocaleDateString() : undefined;
        this.gatepassModel.materialList[i].returneddate = this.gatepassModel.materialList[i].returneddate != null ? new Date(this.gatepassModel.materialList[i].returneddate).toLocaleDateString() : undefined;

      }
      this.wmsService.saveoreditgatepassmaterial(this.gatepassModel).subscribe(data => {
        this.gatepassdialog = false;
        this.updateReturnedDateDialog = false;
        this.getGatePassList();
        if (data)
          this.messageService.add({ severity: 'success', summary: '', detail: 'Gatepass Updated' });
      })
    }
    else
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Type' });
  }

  //saving gatepass details --Gayathri
  async onSubmitgatepassData() {
    debugger;
    this.disableGPBtn = true;
    this.gatePassChange();
    if (this.gatepassModel.gatepasstype != "0") {
      if (this.gatepassModel.materialList.length > 0) {
        //loop all the materiallist
        if (isNullOrUndefined(this.gatepassModel.reasonforgatepass) || this.gatepassModel.reasonforgatepass.trim() == "0") {
          this.messageService.add({ severity: 'error', summary: '', detail: 'Select reason' });
          this.disableGPBtn = false;
          return false;
        }
        if (this.gatepassModel.reasonforgatepass.trim() == "Other" && this.gatepassModel.otherreason.trim() == "") {
          this.messageService.add({ severity: 'error', summary: '', detail: 'Enter reason' });
          this.disableGPBtn = false;
          return false;
        }
        if (this.gatepassModel.reasonforgatepass.trim() != "Other") {
          this.gatepassModel.otherreason = "";
        }
        for (var i = 0; i < this.gatepassModel.materialList.length; i++) {
          this.gatepassModel.materialList[i].expecteddate = this.gatepassModel.materialList[i].expecteddate != null ? new Date(this.gatepassModel.materialList[i].expecteddate).toLocaleDateString() : undefined;
          this.gatepassModel.materialList[i].returneddate = this.gatepassModel.materialList[i].returneddate != null ? new Date(this.gatepassModel.materialList[i].returneddate).toLocaleDateString() : undefined;

        }
       
        var invalidrcvmat = this.gatepassModel.materialList.filter(function (element, index) {
            return (isNullOrUndefined(element.materialid) || element.materialid == "");
        });
       
        var invalidrcvquantity = this.gatepassModel.materialList.filter(function (element, index) {
          return (element.quantity <= 0 || isNullOrUndefined(element.quantity));
        });
        if (invalidrcvmat.length > 0) {
          if (this.gatepassModel.isnonproject) {
            this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Material' });
          }
          else {
            this.messageService.add({ severity: 'error', summary: '', detail: 'Select Material from list' });
          }
          this.disableGPBtn = false;
          return false;
        }
        var invaliddescription = this.gatepassModel.materialList.filter(function (element, index) {
          return (isNullOrUndefined(element.materialdescription) || String(element.materialdescription).trim() == "");
        });
        if (invaliddescription.length > 0) {
          if (this.gatepassModel.isnonproject) {
            this.messageService.add({ severity: 'error', summary: '', detail: 'Enter material description' });
          }
          else {
            this.messageService.add({ severity: 'error', summary: '', detail: 'Select material description from list' });
          }
          this.disableGPBtn = false;
          return false;
        }
        if (invalidrcvquantity.length > 0) {
          this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Quantity' });
          this.disableGPBtn = false;
          return false;
        }
        if (this.gatepassModel.gatepasstype == "Returnable") {
          var invalidrcv = this.gatepassModel.materialList.filter(function (element, index) {
            return (isNullOrUndefined(element.expecteddate) || element.expecteddate == "");
          });
          if (invalidrcv.length > 0) {
            this.messageService.add({ severity: 'error', summary: '', detail: 'Select Expected Date' });
            this.disableGPBtn = false;
            return false;
          }
        }

        
       

        if (this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].quantity > 0) {

          this.spinner.show();
          this.gatepassModel.requestedby = this.employee.employeeno;
          this.gatepassModel.requestedby = this.employee.employeeno;
          this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].expecteddate = this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].expecteddate != null ? new Date(this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].expecteddate).toLocaleDateString() : undefined;
          this.spinner.show();
          debugger;
          this.wmsService.saveoreditgatepassmaterial(this.gatepassModel).subscribe(data => {
            this.spinner.hide();
            this.disableGPBtn = false;
            this.gatepassdialog = false;
            this.updateReturnedDateDialog = false;
            this.getGatePassList();
            if (data) {
              if (this.edit == true) {
                this.messageService.add({ severity: 'success', summary: '', detail: 'Gate Pass Updated Successfully' });
                this.edit = false;
              }
              else {
                this.messageService.add({ severity: 'success', summary: '', detail: 'Gate Pass Created Successfully' });
              }
            }

          })
          //check if for that material quanity exists or not
          

        }
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Add Material to create Gate Pass' });
        this.disableGPBtn = false;
        return false;
      }

    }
    else {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Type' });
      this.disableGPBtn = false;
      return false;
    }



    //// var isvalid = true;
    ////this.gatepassModel.materialList.forEach(item => {
    ////  item.expecteddate = item.expecteddate != null ? new Date(item.expecteddate).toLocaleDateString() : undefined;
    ////  this.wmsService.checkMaterialandQty(item.materialid, item.quantity).subscribe(data => {
    ////    if (data == "true") {
    ////      debugger;
    ////    }
    ////    else {
    ////      debugger;
    ////      isvalid = false;
    ////      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: data + " for " + item.materialid });
    ////      item.quantity = 0;
    ////      //this.spinner.hide();
    ////      return;
    ////    }

    ////  })
    ////})

    ////var pg = this;
    ////setTimeout(function () {
    ////  if (isvalid == true) {
    ////    pg.wmsService.saveoreditgatepassmaterial(pg.gatepassModel).subscribe(data => {
    ////      pg.gatepassdialog = false;
    ////      pg.updateReturnedDateDialog = false;
    ////      pg.getGatePassList();
    ////      if (data)
    ////        //pg.spinner.hide();
    ////        pg.messageService.add({ severity: 'success', summary: 'Success Message', detail: 'Gate Pass Created Successfully' });
    ////    });
    ////  }
    ////}, 2000);





    //if (this.gatepassModel.materialList.length == 0) {
    // // this.spinner.hide();
    //  this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Add materials to create GatePass' });
    //  return false;
    //}

    //else if (this.gatepassModel.gatepasstype != "0") {
    //  this.gatepassModel.requestedby = this.employee.employeeno;
    //  //check if materiallist is empty and gatepass materialid is null
    //  if (!this.material && !this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].materialid) {
    //    this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Add Material' });
    //    //this.spinner.hide();
    //    return false;
    //  }
    //  else if (this.gatepassModel.gatepasstype == "Returnable" && this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].expecteddate == undefined) {
    //    this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Select Expected Date' });
    //    //this.spinner.hide();
    //    return false;
    //  }
    //  else if (this.material) {
    //    if (this.gatepassModel.materialList.filter(li => li.materialid == this.material.code && li.gatepassmaterialid != "0").length > 0) {
    //      //alert("entered");
    //      //console.log(this.gatepassModel);
    //      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Material already exist' });
    //      //this.spinner.hide();
    //      return false;
    //    }

    //    //alert(this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].returneddate);
    //    this.gatePassChange();
    //    if (this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].materialid == "" && !isNullOrUndefined(this.material.code)) {
    //      this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].materialid = this.material.code;
    //      this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].materialdescription = this.material.name;
    //      this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].expecteddate = new Date(this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].expecteddate).toLocaleDateString();
    //      this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].returneddate = this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].returneddate != null ? new Date(this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].returneddate).toLocaleDateString() : undefined;

    //      if (this.materialistModel.materialid && this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].quantity) {
    //        this.wmsService.checkMaterialandQty(this.material.code, this.materialistModel.quantity).subscribe(data => {
    //          if (data == "true") {
    //            // this.gatepassModel.materialList.push(this.materialistModel);
    //            //this.gatepassModel.materialList.push(this.materialistModel);
    //            //this.materialistModel = new materialistModel();
    //            this.material = "";
    //            this.wmsService.saveoreditgatepassmaterial(this.gatepassModel).subscribe(data => {
    //              this.gatepassdialog = false;
    //              this.updateReturnedDateDialog = false;
    //              this.getGatePassList()
    //              if (data)
    //                // this.spinner.hide();
    //                this.messageService.add({ severity: 'success', summary: 'Success Message', detail: 'Gate Pass Created Successfully' });
    //            })
    //          }
    //          else
    //            //this.spinner.hide();
    //            this.messageService.add({ severity: 'error', summary: 'Error Message', detail: data });
    //        });
    //      }
    //      else {
    //        //this.spinner.hide();
    //        if (!this.materialistModel.materialid)
    //          this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'select material from list' });
    //        else if (!this.materialistModel.quantity)
    //          this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Enter Quantity' });
    //      }
    //    }
    //    else {
    //      debugger;
    //      console.log(this.gatepassModel);
    //      var isvalid = true;
    //      this.gatepassModel.materialList.forEach(item => {
    //        item.expecteddate = item.expecteddate != null ? new Date(item.expecteddate).toLocaleDateString() : undefined;
    //        this.wmsService.checkMaterialandQty(item.materialid, item.quantity).subscribe(data => {
    //          if (data == "true") {
    //            debugger;
    //          }
    //          else {
    //            debugger;
    //            isvalid = false;
    //            this.messageService.add({ severity: 'error', summary: 'Error Message', detail: data + " for " + item.materialid });
    //            //this.spinner.hide();
    //            return;
    //          }

    //        })
    //      })

    //      var pg = this;
    //      setTimeout(function () {
    //        if (isvalid == true) {
    //          pg.wmsService.saveoreditgatepassmaterial(pg.gatepassModel).subscribe(data => {
    //            pg.gatepassdialog = false;
    //            pg.updateReturnedDateDialog = false;
    //            pg.getGatePassList();
    //            if (data)
    //              //pg.spinner.hide();
    //              pg.messageService.add({ severity: 'success', summary: 'Success Message', detail: 'Gate Pass Created Successfully' });
    //          });
    //        }
    //      }, 2000);

    //    }
    //  }
    //  else {
    //    debugger;
    //    console.log(this.gatepassModel);
    //    var isvalid = true;
    //    this.gatepassModel.materialList.forEach(item => {
    //      item.expecteddate = item.expecteddate != null ? new Date(item.expecteddate).toLocaleDateString() : undefined;
    //       this.wmsService.checkMaterialandQty(item.materialid, item.quantity).subscribe(data => {
    //        if (data == "true") {
    //          debugger;
    //        }
    //        else {
    //          debugger;
    //          isvalid = false;
    //          this.messageService.add({ severity: 'error', summary: 'Error Message', detail: data + " for " + item.materialid });
    //          //this.spinner.hide();
    //          return;
    //        }

    //      })
    //    }


    //    )
    //    ////for (var i = 0; i < this.gatepassModel.materialList.length; i++) {
    //    ////  this.gatepassModel.materialList[i].expecteddate = this.gatepassModel.materialList[i].expecteddate != null ? new Date(this.gatepassModel.materialList[i].expecteddate).toLocaleDateString() : undefined;
    //    ////  //this.gatepassModel.materialList[i].returneddate = this.gatepassModel.materialList[i].returneddate != null ? new Date(this.gatepassModel.materialList[i].returneddate).toLocaleDateString() : undefined;
    //    ////  await this.wmsService.checkMaterialandQty(this.gatepassModel.materialList[i].materialid, this.gatepassModel.materialList[i].quantity).subscribe(data => {
    //    ////    if (data == "true") {
    //    ////      debugger;
    //    ////    }
    //    ////    else {
    //    ////      debugger;
    //    ////      isvalid = false;
    //    ////      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: data });
    //    ////      return ;
    //    ////    }

    //    ////  })


    //    ////}
    //    var pg = this;
    //    setTimeout(function () {
    //      if (isvalid == true) {
    //        pg.wmsService.saveoreditgatepassmaterial(pg.gatepassModel).subscribe(data => {
    //          pg.gatepassdialog = false;
    //          pg.updateReturnedDateDialog = false;
    //          pg.getGatePassList();
    //          if (data)
    //            //pg.spinner.hide();
    //            pg.messageService.add({ severity: 'success', summary: 'Success Message', detail: 'Gate Pass Created Successfully' });
    //        });
    //      }
    //    }, 2000);

    //    //if (isvalid == true) {

    //    //  await this.wmsService.saveoreditgatepassmaterial(this.gatepassModel).subscribe(data => {
    //    //    this.gatepassdialog = false;
    //    //    this.updateReturnedDateDialog = false;
    //    //    this.getGatePassList();
    //    //    if (data)
    //    //      this.messageService.add({ severity: 'success', summary: 'Success Message', detail: 'Gate Pass Created Successfully' });
    //    //  })
    //    //}

    //  }


    //}

    //else {
    //  //this.spinner.hide();
    //  this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Select Type' });
    //}


  }

  //showing print page
  showprint(gatepassobject: gatepassModel) {
    var materialarray = [];
    this.wmsService.gatepassmaterialdetail(gatepassobject.gatepassid).subscribe(data => {
      materialarray = data;
      this.exportPdf(gatepassobject, materialarray);
    });
   
    //this.router.navigate(['/WMS/GatePassPrint', gatepassobject.gatepassid]);
  }

 

  //shows list of items for particular material
  showmateriallocationList(material, id, rowindex) {
    this.id = id;
    this.AddDialog = true;
    this.roindex = rowindex;
    this.wmsService.getItemlocationListByMaterial(material).subscribe(data => {
      this.itemlocationData = data;
      this.showdialog = true;
      if (data != null) {

      }
    });
  }
  //show alert about oldest item location
  alertconfirm(data) {
    var info = data;
    this.ConfirmationService.confirm({
      message: 'Same Material received on ' + data.createddate + ' and placed in ' + data.itemlocation + '  location, Would you like to continue?',
      header: 'Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {

        this.messageService.add({ severity: 'info', summary: 'Accepted', detail: 'You have accepted' });
      },
      reject: () => {

        this.messageService.add({ severity: 'info', summary: 'Ignored', detail: 'You have ignored' });
      }
    });
  }
  //check issued quantity
  checkissueqty($event, entredvalue, maxvalue, material, createddate, description: string, rowdata : any) {
    if (entredvalue < 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter value grater than 0' });
      rowdata.issuedquantity = 0;
      return;
    }
    if (isNullOrUndefined(entredvalue) || entredvalue == 0) {
      rowdata.issuedquantity = 0;
      return;
    }
    var id = $event.target.id;
    if (entredvalue > maxvalue) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Please enter issue quantity less than Available quantity' });

      (<HTMLInputElement>document.getElementById(id)).value = "";
    }
    else {

      this.wmsService.checkoldestmaterialwithdesc(material, createddate, description).subscribe(data => {
        this.Oldestdata = data;
        if (data != null) {
          this.alertconfirm(this.Oldestdata);
        }
        //this.calculateTotalQty();
        //this.calculateTotalPrice();
        this.spinner.hide();
      });
    }
  }

  Cancel() {
    this.AddDialog = false;
  }

  parseDate(dateString: string): Date {
    if (dateString) {
      return new Date(dateString);
    }
    return null;
  }

  issuematerial(itemlocationData) {
    var totalissuedqty = 0;
    this.itemlocationData.forEach(item => {
      if (item.issuedquantity != 0)

        totalissuedqty = totalissuedqty + (item.issuedquantity);
      this.FIFOvalues.issueqty = totalissuedqty;
      item.issuedqty = this.FIFOvalues.issueqty;
      //item.issuedquantity = totalissuedqty;
      //item.issuedqty = totalissuedqty;

    });


    (<HTMLInputElement>document.getElementById(this.id)).value = totalissuedqty.toString();
    this.gatepassModel.materialList[this.roindex].issuedqty = totalissuedqty;
    this.txtDisable = true;
    this.AddDialog = false;

  }
  approverListdata() {
   
    this.wmsService.getapproverdata(this.employee.employeeno).
      subscribe(
        res => {
    
          if (!isNullOrUndefined(res[0].approverid) && String(res[0].approverid).trim() != "") {
            this.gatepassModel.approverid = res[0].approverid;
            this.gatepassModel.managername = res[0].managername;
          }
          else {
            this.gatepassModel.approverid = res[0].departmentheadid;
            this.gatepassModel.managername = res[0].departmentheadname;
          }
         

        });
  }

  onSelectStatus() {
    debugger;
    //this.selectedStatus = value;
    //if (this.employee.roleid == '3') {
    //  this.gatepassFiltered = this.gatepassModelList.filter(li => li.issuedqty == 0 && li.approverstatus == "Approved");
    //}
    //else {

    //  this.gatepassFiltered = this.gatepassModelList;
    //}
    this.constants.gatePassIssueType = this.selectedStatus;
    if (this.selectedStatus == "Pending") {
      this.gatepassFiltered = this.gatepassModelList.filter(li => li.issuedqty == 0 && li.approverstatus == "Approved");
    }
    else if (this.selectedStatus == "Issued") {
      this.gatepassFiltered = this.gatepassModelList.filter(li => li.issuedqty > 0);
    }
  }

  SubmitStatus() {
    debugger;
    if (this.selectedStatus == "Pending") {
      this.gatepassFiltered = this.gatepassModelList.filter(li => li.issuedqty == 0);
    }
    else if (this.selectedStatus == "Issued") {
      this.gatepassFiltered = this.gatepassModelList.filter(li => li.issuedqty > 0);
    }
  }

}
