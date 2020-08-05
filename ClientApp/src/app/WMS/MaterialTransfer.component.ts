import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl, ValidatorFn } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { materialRequestDetails, returnmaterial, gatepassModel, materialistModel, PoDetails, StockModel, materialistModelreturn, materialistModeltransfer } from 'src/app/Models/WMS.Model';
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
  public requestList: Array<any> = [];
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
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
   
    this.PoDetails = new PoDetails();
    this.StockModel = new StockModel();
    this.tarnsferModel = new returnmaterial();
    this.tarnsferModel.materialList = [];
    this.route.params.subscribe(params => {
      if (params["pono"]) {
        this.pono = params["pono"];
      }
    });

    this.getMaterialRequestlist();
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
  getMaterialRequestlist() {
    //this.employee.employeeno = "180129";
    this.wmsService.gettransferdata().subscribe(data => {
      this.requestList = data;
      this.requestList.forEach(item => {
        if (!item.requestedquantity)
          item.requestedquantity = item.quotationqty;
      });
    });
  }

  //check validations for requested quantity
  reqQtyChange(data: any) {
    if (data.requestedquantity > data.quotationqty) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Requested Quantity should be lessthan or equal to po quantity' });
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
        this.messageService.add({ severity: 'success', summary: 'success Message', detail: 'Request sent' });
      else
        this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Update Failed' });

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
  materialAckUpdate() {
    if (this.requestList.filter(li => li.status == true).length == 0) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Select atleast  one checkbox' });
    }
    else {
      this.spinner.show();
      this.btnDisable = true;
      this.wmsService.ackmaterialreceived(this.requestList).subscribe(data => {
        this.spinner.hide();
        if (data)
          this.messageService.add({ severity: 'sucess', summary: 'sucee Message', detail: 'acknowledged' });
        else
          this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'acknowledge failed' });
      });
    }
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
    //  this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Please select any Request Id' });
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
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Please select  Request Id' });
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
          this.messageService.add({ severity: 'sucess', summary: 'suceess Message', detail: 'Material Returned' });
        }
    })

  }
  returnQtyChange(issuesqty,returnqty) {
    if (returnqty > issuesqty) {
      this.btnDisable = true;
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Return Quantity should be lessthan or equal to Issued quantity' });

    }
    else {
      this.btnDisable = false;
    }
  }
  onMaterialSelected(material: any) {
    if (material == 'other') {

    }
    if (this.tarnsferModel.materialList.filter(li => li.material == material.code).length > 0) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Material already exist' });
      return false;
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

    for (var i = 0; i <= this.materiallistData.length - 1; i++) {
      this.materiallistData[i].requesttype = "transfer";
      this.materiallistData[i].createdby = this.employee.employeeno;
      this.materiallistData[i].returnqty = 0;
    }
    this.wmsService.UpdateReturnqty(this.tarnsferModel.materialList).subscribe(data => {
      if (data == 1) {
        this.btnDisabletransfer = true;
        this.AddDialogfortransfer = false;
        this.messageService.add({ severity: 'sucess', summary: 'suceess Message', detail: 'Material Transferred' });
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
  toggleVisibility(e,index) {
    if (e.checked == true) {
      this.chkChangeshideshow = true;
      document.getElementById(index).style.display = "block";
    }
    else {
      document.getElementById(index).style.display = "none";
    }
  }

 

  //Adding new material 
  addNewMaterial() {

    if (this.tarnsferModel.materialLists.length == 0 || isNullOrUndefined(this.material)) {
      this.materialistModel = { material: "", materialdescription: "", remarks: " ", transferqty: 0, createdby: this.employee.employeeno, projectcode: "", transfetid: 0 };
      this.tarnsferModel.materialLists.push(this.materialistModel);
      this.material = "";
    }
    else if (!this.material && !this.tarnsferModel.materialLists[this.tarnsferModel.materialLists.length - 1].material) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'please Add Material' });
      return false;
    }
    //else if (this.material && this.returnModel.materialList[this.returnModel.materialList.length - 1].returnquantity==0) {
    //  this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'please add return quantity' });
    //  return false;
    //}

    else {
      if (this.tarnsferModel.materialLists.filter(li => li.material == this.material.code && li.material != "0").length > 0) {
        this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Material already exist' });
        return false;
      }
      //this.transferChange();
     // if (this.material) {
        this.tarnsferModel.materialLists[this.tarnsferModel.materialLists.length - 1].material = this.material.code;
        this.tarnsferModel.materialLists[this.tarnsferModel.materialLists.length - 1].materialdescription = this.material.name;

        this.materialistModel = { material: "", materialdescription: "", remarks: " ", transferqty: 0, transfetid: 0, createdby: this.employee.employeeno,projectcode:"" };
        this.tarnsferModel.materialLists.push(this.materialistModel);
        this.material = "";
     // }
    }
  }


  
  //open gate pass dialog
  openGatepassDialog(gatepassobject: any, gpIndx: any, dialog) {
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
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Add Material' });
      return false;
    }
    if (this.tarnsferModel.materialList.filter(li => li.material == this.material.code).length > 0) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Material already exist' });
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

    this.tarnsferModel.materialLists.splice(matIndex, 1);
    if (id != 0) {
      this.wmsService.deleteGatepassmaterial(id).subscribe(data => {
        //this.gatepassModelList[this.gpIndx].materialList.splice(matIndex, 1);
        this.messageService.add({ severity: 'success', summary: 'success Message', detail: 'Material Deleted' });
      });
    }


  }
  transfermaterial() {
    this.spinner.show();
    this.btnDisable = true;
    this.wmsService.Updatetransferqty(this.tarnsferModel.materialLists).subscribe(data => {
      this.spinner.hide();
      if (data)
        this.messageService.add({ severity: 'success', summary: 'success Message', detail: 'Material tarnsferred' });
      else
        this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Transfer Failed' });

    });
  }
}
