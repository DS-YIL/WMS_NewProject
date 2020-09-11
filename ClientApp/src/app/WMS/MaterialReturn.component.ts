import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl, ValidatorFn } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { materialRequestDetails, returnmaterial, gatepassModel, materialistModel, PoDetails, StockModel, materialistModelreturn } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { isNullOrUndefined } from 'util';

@Component({
  selector: 'app-MaterialReturn',
  templateUrl: './MaterialReturn.component.html'
})
export class MaterialReturnComponent implements OnInit {
   AddDialog: boolean;
  showdialog: boolean;

  public materiallistData: Array<any> = [];
  public materiallistDataHistory: Array<any> = [];
  AddDialogfortransfer: boolean;
  materialreturn: boolean = true;
  MaterialDetailsDialog: boolean = false;
    showdialogfortransfer: boolean;
    projectcodes: string;
    showhistory: boolean=false;
    displaydetail: boolean;
  public gpIndx: number;
  GatepassTxt: string;
  public gatepassdialog: boolean = false;
  constructor(private formBuilder: FormBuilder, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }
  public StockModel: StockModel;
  public returnModel: returnmaterial;
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
  public materialistModel: materialistModelreturn;
  public showDetails; showLocationDialog: boolean = false;
  public PoDetails: PoDetails;
  public StockModelForm: FormGroup;
  public StockModelForm1: FormGroup;
  public stock: StockModel[] = [];
  filteredmats: any[];
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
   
    this.PoDetails = new PoDetails();
    this.StockModel = new StockModel();
    this.returnModel = new returnmaterial();
    this.returnModel.materialList = [];
    this.route.params.subscribe(params => {
      if (params["pono"]) {
        this.pono = params["pono"];
      }
    });
   
    this.getMaterialRequestlist();
   
  }

 

  //get Material Rquest based on login employee && po no
  getMaterialRequestlist() {
    //this.employee.employeeno = "180129";
    this.wmsService.getreturndata(this.employee.employeeno).subscribe(data => {
      this.requestList = data;
      
      //this.requestList.forEach(item => {
      //  if (!item.requestedquantity)
      //    item.requestedquantity = item.quotationqty;
      //});
    });
   // this.returnModel.materialList = [];
  }

  //check validations for requested quantity
  reqQtyChange(data: any) {
    if (data.requestedquantity > data.quotationqty) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Requested Quantity should be lessthan or equal to po quantity' });
      data.requestedquantity = data.quotationqty;
    }
  }

  //requested quantity update
  onMaterialRequestDeatilsSubmit() {
    this.spinner.show();
    this.btnDisable = true;
    this.wmsService.materialRequestUpdate(this.requestList).subscribe(data => {
      this.spinner.hide();
      if (data)
        this.messageService.add({ severity: 'success', summary: '', detail: 'Material returned' });
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
  materialAckUpdate() {
    if (this.requestList.filter(li => li.status == true).length == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select atleast  one checkbox' });
    }
    else {
      this.spinner.show();
      this.btnDisable = true;
      this.wmsService.ackmaterialreceived(this.requestList).subscribe(data => {
        this.spinner.hide();
        if (data)
          this.messageService.add({ severity: 'sucess', summary: '', detail: 'acknowledged' });
        else
          this.messageService.add({ severity: 'error', summary: '', detail: 'acknowledge failed' });
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
    //Displaying material details on select of material
  showmaterialdetails(matreturnid:any) {
      this.AddDialog = true;
    this.showdialog = true;
    this.materialreturn = false;
    this.MaterialDetailsDialog = true;
    this.requestid = 7;
    this.materiallistData = [];
    //this.materiallistData = this.requestList.filter(li => li.approvedstatus == 'Approved');
    this.wmsService.getmaterialreturnreqList(matreturnid).subscribe(data => {
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

      //this.bindSearchListData();
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
  

    if (this.returnModel.materialList.length == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Please Add materials to Transfer' });
      return false;
    }


    //this.tarnsferModel.materialLists.requestedby = this.employee.employeeno;
    else if (!this.material && !this.returnModel.materialList[this.returnModel.materialList.length - 1].material) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Add Material' });
      return false;
    }
    else if (this.returnModel.materialList[this.returnModel.materialList.length - 1].returnqty == 0) {
      this.messageService.add({ severity: 'error', summary: ' ', detail: 'Please enter return qty' });
      this.spinner.hide();
      return false;
    }
    else if (this.material) {
      if (this.returnModel.materialList.filter(li => li.material == this.material.code).length > 0) {

        this.messageService.add({ severity: 'error', summary: ' ', detail: 'Material already exist' });
        return false;
      }
      //this.gatePassChange();
      else if (this.returnModel.materialList[this.returnModel.materialList.length - 1].material == "" && !isNullOrUndefined(this.material.code)) {
        this.returnModel.materialList[this.returnModel.materialList.length - 1].material = this.material.code;
        this.returnModel.materialList[this.returnModel.materialList.length - 1].materialdescription = this.material.name;
        //this.tarnsferModel.materialLists[this.tarnsferModel.materialLists.length - 1].expecteddate = new Date(this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].expecteddate).toLocaleDateString();
        // this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].returneddate = this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].returneddate != null ? new Date(this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].returneddate).toLocaleDateString() : undefined;  
      }
    }
    this.wmsService.UpdateReturnqty(this.returnModel.materialList).subscribe(data => {
      this.AddDialog = false;
        if (data == 1) {
          this.btnDisable = true;
          this.gatepassdialog = false;
          this.messageService.add({ severity: 'success', summary: 'suceess Message', detail: 'Material Returned' });
          this.getMaterialRequestlist();
        }
    })

  }
  returnQtyChange(issuesqty,returnqty) {
    if (returnqty > issuesqty) {
      this.btnDisable = true;
      this.messageService.add({ severity: 'error', summary: ' ', detail: 'Return Quantity should be lessthan or equal to Issued quantity' });

    }
    else {
      this.btnDisable = false;
    }
  }
  onMaterialSelected(material: any, data: any, index: number) {
    debugger;
    if (this.returnModel.materialList.filter(li => li.material == material.code).length > 0) {
      this.messageService.add({ severity: 'error', summary: ' ', detail: 'Material already exist' });
      return false;
    }

    var data1 = this.filteredmats.filter(function (element, index) {
      return (element.material == material.code);
    });
    if (data1.length > 0) {
      data.materialdescription = data1[0].materialdescription;
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
    this.dynamicData.searchCondition += "material" + " ilike '" + searchTxt + "%' or materialdescription ilike '" + searchTxt+"%'  limit 100";
    //this.materialistModel.materialcost = "";
    this.filteredmats = [];
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.searchresult = data;
      this.filteredmats = data;
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
    this.wmsService.UpdateReturnqty(this.materiallistData).subscribe(data => {
      if (data == 1) {
        this.btnDisabletransfer = true;
        this.AddDialogfortransfer = false;
        this.messageService.add({ severity: 'success', summary: 'suceess Message', detail: 'Material Transferred' });
      }
    })
  }
  onChange(value, indexid:any) {
    console.log(event);
    if (value == 'other') {
      document.getElementById(indexid+1).style.display = "block";
    }
    this.materiallistData[indexid].projectname = value;
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
   


    if (this.returnModel.materialList.length == 0 || isNullOrUndefined(this.material)) {
      this.materialistModel = { material: "", materialdescription: "", remarks: " ", returnid: 0, returnqty: 0, createdby: this.employee.employeeno };
      this.returnModel.materialList.push(this.materialistModel);
      this.material = "";
    }
    else if (!this.material && !this.returnModel.materialList[this.returnModel.materialList.length - 1].material) {
      this.messageService.add({ severity: 'error', summary: ' ', detail: 'please Add Material' });
      return false;
    }
    //else if (this.material && this.returnModel.materialList[this.returnModel.materialList.length - 1].returnquantity==0) {
    //  this.messageService.add({ severity: 'error', summary: ' ', detail: 'please add return quantity' });
    //  return false;
    //}
   
    else {
      if (this.returnModel.materialList.filter(li => li.material == this.material.code && li.material != "0").length > 0) {
        this.messageService.add({ severity: 'error', summary: ' ', detail: 'Material already exist' });
        return false;
      }
      this.transferChange();
      //if (this.material) {
      this.returnModel.materialList[this.returnModel.materialList.length - 1].material = this.material.code;
      this.returnModel.materialList[this.returnModel.materialList.length - 1].materialdescription = this.material.name;

      this.materialistModel = { material: "", materialdescription: "", remarks: " ", returnid: 0, returnqty: 0, createdby:this.employee.employeeno };
      this.returnModel.materialList.push(this.materialistModel);
            this.material = "";
    }
  }

  //gatepass change
 transferChange() {
    //if (this.gatepassModel.gatepasstype != "0")
      this.GatepassTxt = "Return Materials";
  }
  //open gate pass dialog
  openDialog( gatepassobject:any,gpIndx: any, dialog) {
   // this.bindSearchListData();
    this.displaydetail = false;
    //this.approverListdata();
    this[dialog] = true;
    this.returnModel = new returnmaterial();
    //if (gatepassobject) {

    //  //this.gpIndx = gpIndx;
    //  //this.returnModel = gatepassobject;
      
    //} else {
    this.materialistModel = { material: "", materialdescription: "", remarks: " ", returnid: 0, returnqty: 0, createdby: this.employee.employeeno };
      this.returnModel.materialList.push(this.materialistModel);
      this.material = "";
   // }
    this.transferChange();
  }

  //add materials for gate pass
  addMaterial() {
    if (!this.material) {
      this.messageService.add({ severity: 'error', summary: ' ', detail: 'Add Material' });
      return false;
    }
    if (this.returnModel.materialList.filter(li => li.material == this.material.code).length > 0) {
      this.messageService.add({ severity: 'error', summary: ' ', detail: 'Material already exist' });
      return false;
    }
    this.transferChange();
    this.materialistModel.material = this.material.code;
    this.materialistModel.materialdescription = this.material.name;
    //if (this.materialistModel.materialid && this.materialistModel.quantity) {
    //  this.wmsService.checkMaterialandQty(this.material.code, this.materialistModel.quantity).subscribe(data => {
    //    if (data == "true") {
    this.returnModel.materialList.push(this.materialistModel);
    this.materialistModel = new materialistModelreturn();
          this.material = "";
   
  }
  //Delete material for transfer
  removematerial(id: number, matIndex: number) {

    this.returnModel.materialList.splice(matIndex, 1);
    if (id != 0) {
      this.wmsService.deleteGatepassmaterial(id).subscribe(data => {
        this.messageService.add({ severity: 'success', summary: ' ', detail: 'Material Deleted' });
      });
    }


  }

}
