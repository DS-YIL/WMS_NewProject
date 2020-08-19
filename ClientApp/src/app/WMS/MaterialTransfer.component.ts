import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl, ValidatorFn } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { materialtransferMain, materialtransferTR, materialRequestDetails, returnmaterial, gatepassModel, materialistModel, PoDetails, StockModel, materialistModelreturn, materialistModeltransfer, ddlmodel } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { isNullOrUndefined } from 'util';

@Component({
  selector: 'app-MaterialTransfer',
  templateUrl: './MaterialTransfer.component.html'
})
export class MaterialTransferComponent implements OnInit {
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
  filteredprojects: ddlmodel[] = [];
  materiallists: ddlmodel[] = [];
  selectedmaterial: string = "";
  filteredmaterial: string[] = [];
  materialtransferlist: materialtransferMain[] = [];
  materialtransfersavelist: materialtransferMain;
  materialtransferdetil: materialtransferTR[] = [];
  transferremarks: string = "";


  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
   
    this.PoDetails = new PoDetails();
    this.StockModel = new StockModel();
    this.tarnsferModel = new returnmaterial();
    this.tarnsferModel.materialList = [];
    this.filteredmaterial = [];
    this.requestList = [];
    this.getprojects();
    this.getmaterials();
    this.filteredprojects = [];
    this.materialtransferlist = [];
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
    this.wmsService.gettransferdata(employeeno).subscribe(data => {
      this.requestList = data;
      this.requestList.forEach(item => {
        item.showtr = false;
      });
    });
  }

  showattachdata(rowData: materialtransferMain) {
    debugger;

    rowData.showtr = !rowData.showtr;
   
  }

  //check validations for requested quantity
  reqQtyChange(data: any) {
    if (data.requestedquantity > data.quotationqty) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Requested Quantity should be lessthan or equal to po quantity' });
      data.requestedquantity = data.quotationqty;
    }
  }

  //transfer quantity update
  onMaterialRequestDeatilsSubmit() {
    this.spinner.show();
    this.btnDisable = true;
    this.wmsService.Updatetransferqty(this.requestList).subscribe(data => {
      this.spinner.hide();
      if (data)
        this.messageService.add({ severity: 'success', summary: '', detail: 'Request sent' });
      else
        this.messageService.add({ severity: 'error', summary: '', detail: 'Update Failed' });

    });
  }

  //app
  ackStatusChanges(status) {
    if (status == 'Approved') {
      this.showAck = false;
    }
    else {
      this.showAck = true;
    }
  }

  //received material acknowledgement
 
  getprojects() {
    this.spinner.show();
    this.wmsService.getprojectlist().subscribe(data => {
      debugger;
      this.projectlists = data;
      this.spinner.hide();
    });
  }
  getmaterials() {
    this.spinner.show();
    var empno = this.employee.employeeno;
    this.wmsService.getmateriallistfortransfer(empno).subscribe(data => {
      debugger;
      this.materiallists = data;
      this.spinner.hide();
    });
  }
  //redirect to PM Dashboard
  backtoDashboard() {
    this.router.navigateByUrl("/WMS/Dashboard");
  }
  selectrow(requesid) {
    this.requestid = requesid;

  }
  showmaterialdetails() {
    //if (this.requestid == undefined) {
    //  this.messageService.add({ severity: 'error', summary: '', detail: 'Please select any Request Id' });
    //  //this.router.navigateByUrl("/WMS/MaterialReqView");
    //}
    //else {


      //this.rowindex = rowindex
      this.AddDialog = true;
    this.showdialog = true;
    this.requestid = 7;
      //this.materiallistData = this.requestList.filter(li => li.approvedstatus == 'Approved');
      this.wmsService.getmaterialissueList(this.requestid).subscribe(data => {
        this.materiallistData = data;

        if (data != null) {

        }
      });
    //}
  }
  showmaterialdetailsfortransfer() {
    if (this.requestid == undefined) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Please select  Request Id' });
      //this.router.navigateByUrl("/WMS/MaterialReqView");
    }
    else {

      this.bindSearchListData();
      //this.rowindex = rowindex
      this.AddDialogfortransfer = true;
      this.showdialogfortransfer = true;
      //this.materiallistData = this.requestList.filter(li => li.approvedstatus == 'Approved');
      this.wmsService.getmaterialissueList(this.requestid).subscribe(data => {
        this.materiallistData = data;

        if (data != null) {

        }
      });
    }
  }
  Cancel() {
    this.AddDialog = false;
  }
  returnqty() {
  
    for (var i = 0; i <= this.materiallistData.length - 1; i++) {
    
      this.materiallistData[i].requesttype = "return";
      this.materiallistData[i].createdby = this.employee.employeeno;
    }
      this.wmsService.UpdateReturnqty(this.materiallistData).subscribe(data => {
        if (data == 1) {
          this.btnDisable = true;
          this.AddDialog = false;
          this.messageService.add({ severity: 'sucess', summary: '', detail: 'Material Returned' });
        }
    })

  }
  returnQtyChange(issuesqty,returnqty) {
    if (returnqty > issuesqty) {
      this.btnDisable = true;
      this.messageService.add({ severity: 'error', summary: '', detail: 'Return Quantity should be lessthan or equal to Issued quantity' });

    }
    else {
      this.btnDisable = false;
    }
  }
  onMaterialSelected(data: any, ind: number) {
    debugger;
    var data1 = this.materialtransferdetil.filter(function (element, index) {
      return (element.materialid == data.materialid && index != ind);
    });
    if (data1.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Material already exist' });
      data.materialid = "";
      data.materialdescription = "";
      data.transferredqty = 0;
      return false;
    }
    var data2 = this.materiallists.filter(function (element, index) {
      return (element.value == data.materialid);
    });
    if (data2.length > 0) {
      data.materialdescription = data2[0].text;
    }

  }
  //bind materials based search
  public bindSearchListDatamaterial(event: any, name?: string) {
    var searchTxt = event.query;
    if (searchTxt == undefined)
      searchTxt = "";
    searchTxt = searchTxt.replace('*', '%');
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.tableName = this.constants[name].tableName + " ";
    this.dynamicData.searchCondition = "" + this.constants[name].condition;
    this.dynamicData.searchCondition += "material" + " ilike '" + searchTxt + "%'  limit 10";
    //this.materialistModel.materialcost = "";
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.searchresult = data;
      this.searchItems = [];
      var fName = "";
      this.searchresult.forEach(item => {
        fName = item[this.constants[name].fieldName];
        //if (name == "ItemId")
          //fName = item[this.constants[name].fieldName] + " - " + item[this.constants[name].fieldId];
          fName = item[this.constants[name].fieldId];
        var value = { listName: name, name: fName, code: item[this.constants[name].fieldId] };
        //this.materialistModel.materialcost = data[0].materialcost;
        this.searchItems.push(value);
      });
    });
  }

  //bind materials based search
  public bindSearchListData() {
    this.dynamicData.tableName ="wms.wms_project";
   this.dynamicData.searchCondition = " where projectcode is not null";
    this.wmsService.GetListItems(this.dynamicData).subscribe(res => {

      //this.searchresult = res;
      //this.searchItems = [];
      //var fName = "";
      //this.searchresult.forEach(item => {
      //  fName = 'projectcode';
      //  if (name == "ItemId")
      //  fName = item[this.constants[name].fieldName] + " - " + item[this.constants[name].fieldId];
      //  fName = item['projectcode'];
      //  var value = { listName: name, name: fName, code: item['projectcode'] };
      //  this.materialistModel.materialcost = data[0].materialcost;
      //  this.searchItems.push(value);
      //});
        //this._list = res; //save posts in array
        this.locationlist = res;
        let _list: any[] = [];
        for (let i = 0; i < (res.length); i++) {
          _list.push({
            projectcode: res[i].projectcode,
           // projectcode: res[i].projectcode
          });
        }
        this.locationlist = _list;
      });
  }
 
  showstory(requestid){
   this.showhistory = true;
    this.wmsService.getreturnmaterialListforconfirm(requestid).subscribe(data => {
     this.materiallistDataHistory = data;

      if (data != null) {

      }
    });
  }
  transferqty() {

    if (this.tarnsferModel.materialLists.length == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Please Add materials to Transfer' });
      return false;
    }
    
   
    //this.tarnsferModel.materialLists.requestedby = this.employee.employeeno;
   else if (!this.material && !this.tarnsferModel.materialLists[this.tarnsferModel.materialLists.length - 1].material) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Add Material' });
      return false;
    }
    else if (this.tarnsferModel.materialLists[this.tarnsferModel.materialLists.length - 1].transferqty == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Please enter transfer qty' });
      return false;
    }
    else if (this.material) {
      if (this.tarnsferModel.materialLists.filter(li => li.material == this.material.code).length > 0) {

        this.messageService.add({ severity: 'error', summary: '', detail: 'Material already exist' });
        return false;
      }
      //this.gatePassChange();
    else  if (this.tarnsferModel.materialLists[this.tarnsferModel.materialLists.length - 1].material == "" && !isNullOrUndefined(this.material.code)) {
        this.tarnsferModel.materialLists[this.tarnsferModel.materialLists.length - 1].material = this.material.code;
        this.tarnsferModel.materialLists[this.tarnsferModel.materialLists.length - 1].materialdescription = this.material.name;
        //this.tarnsferModel.materialLists[this.tarnsferModel.materialLists.length - 1].expecteddate = new Date(this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].expecteddate).toLocaleDateString();
        // this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].returneddate = this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].returneddate != null ? new Date(this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].returneddate).toLocaleDateString() : undefined;  
      }
    }
    this.wmsService.UpdateReturnqty(this.tarnsferModel.materialList).subscribe(data => {
      if (data == 1) {
        this.btnDisabletransfer = true;
        this.AddDialogfortransfer = false;
        this.messageService.add({ severity: 'sucess', summary: '', detail: 'Material Transferred' });
      }
    })
  }
  onChange(value, indexid:any) {
    console.log(event);
    if (value == 'other') {
      document.getElementById(indexid+1).style.display = "block";
    }
    this.tarnsferModel.materialLists[indexid].projectcode = value;
   // (<HTMLInputElement>document.getElementById(indexid)).value = event.toString();
  }
 
 

  //Adding new material 
  addNewMaterial() {

    var data1 = this.materialtransferdetil.filter(function (element, index) {
      return (!element.materialid);
    });
    if (data1.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Add Material' });
      return false;
    }
     let matmodel = new materialtransferTR();
     this.materialtransferdetil.push(matmodel);
    

    //if (this.tarnsferModel.materialLists.length == 0 || isNullOrUndefined(this.material)) {
    //  this.materialistModel = { material: "", materialdescription: "", remarks: " ", transferqty: 0, createdby: this.employee.employeeno, projectcode: "", transfetid: 0 };
    //  this.tarnsferModel.materialLists.push(this.materialistModel);
    //  this.material = "";
    //}
    //else if (!this.material && !this.tarnsferModel.materialLists[this.tarnsferModel.materialLists.length - 1].material) {
    //  this.messageService.add({ severity: 'error', summary: '', detail: 'please Add Material' });
    //  return false;
    //}
    //else if (this.material && this.returnModel.materialList[this.returnModel.materialList.length - 1].returnquantity==0) {
    //  this.messageService.add({ severity: 'error', summary: '', detail: 'please add return quantity' });
    //  return false;
    //}

    //else {
    //  if (this.tarnsferModel.materialLists.filter(li => li.material == this.material.code && li.material != "0").length > 0) {
    //    this.messageService.add({ severity: 'error', summary: '', detail: 'Material already exist' });
    //    return false;
    //  }
      //this.transferChange();
     // if (this.material) {
        //this.tarnsferModel.materialLists[this.tarnsferModel.materialLists.length - 1].material = this.material.code;
        //this.tarnsferModel.materialLists[this.tarnsferModel.materialLists.length - 1].materialdescription = this.material.name;

        //this.materialistModel = { material: "", materialdescription: "", remarks: " ", transferqty: 0, transfetid: 0, createdby: this.employee.employeeno,projectcode:"" };
        //this.tarnsferModel.materialLists.push(this.materialistModel);
        //this.material = "";
     // }
    
  }


  checkqty(enteredvalue: number, data : any) {
    if (enteredvalue < 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Negative no is not allowed' });
      data.transferredqty = 0;
      return;
    }
  }
  
  //open gate pass dialog
  openGatepassDialog(gatepassobject: any, gpIndx: any, dialog) {
    this.materialtransferdetil = [];
    this.selectedproject = new ddlmodel();
    this.transferremarks = "";
   // this.bindSearchListData();
    this.displaydetail = false;
    this[dialog] = true;
    this.tarnsferModel = new returnmaterial();
    let matdetailmodel = new materialtransferTR();
    this.materialtransferdetil.push(matdetailmodel);

      //this.materialistModel = { material: "", materialdescription: "", remarks: " ", transfetid: 0, transferqty: 0, createdby: this.employee.employeeno, projectcode: "" };
      //this.tarnsferModel.materialLists.push(this.materialistModel);
      //this.material = "";
    this.GatepassTxt = "Transfer Materials";
  }

  openGatepassDialog1(gatepassobject: any, gpIndx: any, dialog) {
    this.bindSearchListData();
    this.displaydetail = false;
    //this.approverListdata();
    this[dialog] = true;
    this.tarnsferModel = new returnmaterial();
    if (gatepassobject) {

      this.gpIndx = gpIndx;
      this.tarnsferModel = gatepassobject;
      //this.materialistModel = { materialid: "", gatepassmaterialid: "0", materialdescription: "", quantity: 0, materialcost: "0", remarks: " ", expecteddate: this.date, returneddate: this.date };
      //this.gatepassModel.materialList.push(this.materialistModel);
      //this.material = "";
    } else {
      //this.gatepassModel.gatepasstype = "0";
      //this.gatepassModel.reasonforgatepass = "0";
      this.materialistModel = { material: "", materialdescription: "", remarks: " ", transfetid: 0, transferqty: 0, createdby: this.employee.employeeno, projectcode: "" };
      this.tarnsferModel.materialLists.push(this.materialistModel);
      this.material = "";
    }
    this.GatepassTxt = "Transfer Materials";
  }

  //add materials for gate pass
  addMaterial() {
    if (!this.material) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Add Material' });
      return false;
    }
    if (this.tarnsferModel.materialList.filter(li => li.material == this.material.code).length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Material already exist' });
      return false;
    }
    this.materialistModel.material = this.material.code;
    this.materialistModel.materialdescription = this.material.name;
    //if (this.materialistModel.materialid && this.materialistModel.quantity) {
    //  this.wmsService.checkMaterialandQty(this.material.code, this.materialistModel.quantity).subscribe(data => {
    //    if (data == "true") {
    this.tarnsferModel.materialLists.push(this.materialistModel);
    this.materialistModel = new materialistModeltransfer();
          this.material = "";
   
  }
  //Delete material for gatepass
  removematerial(id: number, matIndex: number) {

    this.materialtransferdetil.splice(matIndex, 1);


  }

  filterprojects(event) {
    this.filteredprojects = [];
    for (let i = 0; i < this.projectlists.length; i++) {
      let brand = this.projectlists[i].value;
      let pos = this.projectlists[i].text;
      if (brand.toLowerCase().indexOf(event.query.toLowerCase()) == 0 || pos.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredprojects.push(this.projectlists[i]);
      }
    }
  }

  filtermaterials(event) {
    debugger;
    this.filteredmaterial = [];
    for (let i = 0; i < this.materiallists.length; i++) {
      let brand = this.materiallists[i].value;
      let pos = this.materiallists[i].text;
      if (brand.toLowerCase().indexOf(event.query.toLowerCase()) == 0 || pos.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredmaterial.push(brand);
      }
    }
  }
  transfermaterial() {
    if (isNullOrUndefined(this.selectedproject.value)) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Project' });
      return false;

    }
    if (this.materialtransferdetil.length == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Add Materials to Transfer' });
      return false;
    }
    var data1 = this.materialtransferdetil.filter(function (element, index) {
      return (!element.materialid);
    });
    if (data1.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Add Materials to Transfer' });
      return false;
    }
    var data2 = this.materialtransferdetil.filter(function (element, index) {
      return (element.transferredqty == 0 || isNullOrUndefined(element.transferredqty));
    });
    if (data2.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Transfer Quantity' });
      return false;
    }
    let savemaindata = new materialtransferMain();
    savemaindata.projectcode = this.selectedproject.value;
    savemaindata.transferremarks = this.transferremarks;
    savemaindata.transferedby = this.employee.employeeno;
    savemaindata.materialdata = this.materialtransferdetil;
    this.spinner.show();
    this.wmsService.updatetransfermaterial(savemaindata).subscribe(data => {
      this.spinner.hide();
      this.btnDisable = true;
      if (data) {
        this.AddDialog = false;
        this.gatepassdialog = false;
        this.messageService.add({ severity: 'success', summary: '', detail: 'Material tarnsferred' });
        this.getMaterialRequestlist(this.employee.employeeno);
      }

      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Transfer Failed' });
      }
    });



  }
  transfermaterial1() {
    //this.spinner.show();
   // this.btnDisable = true;
    if (this.tarnsferModel.materialLists.length == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Please Add materials to Transfer' });
      return false;
    }


    //this.tarnsferModel.materialLists.requestedby = this.employee.employeeno;
    else if (!this.material && !this.tarnsferModel.materialLists[this.tarnsferModel.materialLists.length - 1].material) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Add Material' });
      return false;
    }
    else if (this.tarnsferModel.materialLists[this.tarnsferModel.materialLists.length - 1].transferqty == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Please enter transfer qty' });
      this.spinner.hide();
      return false;
    }
    else if (this.tarnsferModel.materialLists[this.tarnsferModel.materialLists.length - 1].projectcode == "") {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Please enter project code' });
      this.spinner.hide();
      return false;
    }
    else if (this.material) {
      if (this.tarnsferModel.materialLists.filter(li => li.material == this.material.code).length > 0) {

        this.messageService.add({ severity: 'error', summary: '', detail: 'Material already exist' });
        return false;
      }
      //this.gatePassChange();
      else if (this.tarnsferModel.materialLists[this.tarnsferModel.materialLists.length - 1].material == "" && !isNullOrUndefined(this.material.code)) {
        this.tarnsferModel.materialLists[this.tarnsferModel.materialLists.length - 1].material = this.material.code;
        this.tarnsferModel.materialLists[this.tarnsferModel.materialLists.length - 1].materialdescription = this.material.name;
        //this.tarnsferModel.materialLists[this.tarnsferModel.materialLists.length - 1].expecteddate = new Date(this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].expecteddate).toLocaleDateString();
        // this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].returneddate = this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].returneddate != null ? new Date(this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].returneddate).toLocaleDateString() : undefined;  
      }
    }
    this.wmsService.Updatetransferqty(this.tarnsferModel.materialLists).subscribe(data => {
      this.spinner.hide();
      this.btnDisable = true;
      if (data) {
        this.AddDialog = false;
         this.gatepassdialog = false;
        this.messageService.add({ severity: 'success', summary: '', detail: 'Material tarnsferred' });
        this.getMaterialRequestlist(this.employee.employeeno);
      }

      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Transfer Failed' });
      }
    });
  }
}
