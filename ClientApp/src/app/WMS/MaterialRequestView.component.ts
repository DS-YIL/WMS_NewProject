import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl, ValidatorFn } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { materialRequestDetails, gatepassModel, materialistModel, materialList, requestData, ddlmodel, MaterialTransaction } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { RadioButtonModule } from 'primeng/radiobutton';
import { DatePipe, DecimalPipe } from '@angular/common';
import { DataSharingService } from '../datasharing.service';
import { isNullOrUndefined } from 'util';
@Component({
  selector: 'app-MaterialRequest',
  templateUrl: './MaterialRequestView.component.html', 
  providers: [DatePipe,DecimalPipe]
})
export class MaterialRequestViewComponent implements OnInit {
  AddDialog: boolean;
  showdialog: boolean;

  public materiallistData: Array<any> = [];
  public materiallistDataHistory: Array<any> = [];
    AddDialogfortransfer: boolean;
    showdialogfortransfer: boolean;
    projectcodes: string;
  showhistory: boolean = false;
  materialliststr: string[] = [];
  descliststr: string[] = [];


  constructor(private formBuilder: FormBuilder, private messageService: MessageService, private datePipe: DatePipe, private decimalPipe: DecimalPipe, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService, private dataSharingService: DataSharingService) {
    this.dataSharingService.searchmateriallist.subscribe(value => {
      this.materialliststr = value;
    });
    this.dataSharingService.searchdescriptionlist.subscribe(value => {
      this.descliststr = value;
    });

  }

  public requestList: MaterialTransaction[] = [];
  public suppliername: string;
  public ponumber: string;
  public employee: Employee;
  public requestDialog: boolean = false;
  public displayItemRequestDialog; RequestDetailsSubmitted; showAck; btnDisable: boolean = false;
  public materialRequestDetails: materialRequestDetails;
  public requestMatData = new requestData();
  public btnreq: boolean = true;
  public pono: string;
  public rowindex: number;
  public dynamicData = new DynamicSearchResult();
  public searchItems: Array<searchList> = [];
  public searchdescItems: Array<searchList> = [];
  public selectedlist: Array<searchList> = [];
  public searchresult: Array<object> = [];
  public btnDisabletransfer: boolean = false;
  public locationlist: any[] = [];
  public ponolist: any[] = [];
  public materialistModel: materialList;
  public materialmodel: Array<materialList> = [];
  public defaultmaterials: materialList[] = [];
  public defaultmaterialids: materialList[] = [];
  public defaultmaterialidescs: materialList[] = [];
  public defaultuniquematerialids: materialList[] = [];
  public defaultuniquematerialidescs: materialList[] = [];
  public gatepassModel: gatepassModel;
  public materialList: Array<materialList> = [];
  public chkChangeshideshow: boolean = false;
  public displaylist: boolean = false;
  public displayDD: boolean = true;
  public requestid: any;
  public date: Date = null;
  projectlists: ddlmodel[] = [];
  isreservedbefore: boolean = false;
  reserveidview: string = "";
  requestview: string = "";
  filteredmats: any[];
  filteredmatdesc: any[];
  selectedproject: ddlmodel;
  filteredprojects: ddlmodel[] = [];
  requestremarks: string = "";
  selectedpono: string = "";
  selectedsupplier: string = "";
  filteredpos: any[];
  filteredsuppliers: any[];
  selectedmuliplepo: any[];
  immidiatemanagerid: string;
  //Email
  reqid: string = "";
  stocktype: string = "";
  public materailsearchlist: Array<searchList> = [];
  public descriptionsearchlist: Array<searchList> = [];
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
   

    this.route.params.subscribe(params => {
      if (params["pono"]) {
        this.pono = params["pono"];
      }
    });
    this.immidiatemanagerid = "";
    this.approverListdata();
    //Email
    this.reqid = this.route.snapshot.queryParams.requestid;
    if (this.reqid) {
      debugger;
      //get material details for that requestid
      //this.materialList[0].material = this.reqid;
      //this.getMaterialRequestlist();

    }
    this.stocktype = "Project Stock";
    this.materailsearchlist = [];
    this.descriptionsearchlist = [];
    this.selectedmuliplepo = [];
    this.getprojects();
    this.getMaterialRequestlist();
  }

  getplantmaterials(event: any) {
    var selectedval = event.target.value;
    this.materialList = [];
    this.selectedmuliplepo = [];
    this.selectedproject = null;
    if (selectedval == "Project Stock") {
      this.displayDD = false;
      this.displaylist = true;
    } else {
      this.displayDD = true;
      this.displaylist = false;
      this.addNewMaterial();
    }
  }

  filterpos(event) {
    debugger;
    if (isNullOrUndefined(this.selectedproject)) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Project' });
      return;
    }
    this.filteredpos = [];
    for (let i = 0; i < this.ponolist.length; i++) {
      let pos = this.ponolist[i].pono;
      if (pos.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredpos.push(pos);
      }
     
    }
  }

  filtersuppliers(event) {
    debugger;
    if (isNullOrUndefined(this.selectedproject)) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Project' });
      return;
    }
    this.filteredsuppliers = [];
    for (let i = 0; i < this.ponolist.length; i++) {
      let brand = isNullOrUndefined(this.ponolist[i].suppliername) ? "" : this.ponolist[i].suppliername;
      if (brand.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredsuppliers.push(brand);
      }

    }
  }

 
  

 

  //get Material Rquest based on login employee && po no
  getMaterialRequestlist() {
    //this.employee.employeeno = "180129";
    this.spinner.show();
    this.wmsService.getMaterialRequestlist(this.employee.employeeno, this.pono).subscribe(data => {
      this.requestList = data;
      this.spinner.hide();
      if (this.reqid) {
        debugger;
        this.requestList = this.requestList.filter(o => o.requestid == this.reqid);
        this.reqid = null;
      }
    
    });
  }

  getprojects() {
   
    this.wmsService.getprojectlistbymanager(this.employee.employeeno).subscribe(data => {
      debugger;
      this.projectlists = data;
      
    });
  }

  filterprojects(event) {
    this.filteredprojects = [];
    for (let i = 0; i < this.projectlists.length; i++) {
      let brand = this.projectlists[i].value;
      let pos = this.projectlists[i].projectmanager;
      if (brand.toLowerCase().indexOf(event.query.toLowerCase()) == 0 || pos.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredprojects.push(this.projectlists[i]);
      }
    }
  }

  //Check for requested qty
  onComplete(reqqty: number, avqty: number, material: any, index,data : any) {
    if (avqty < reqqty) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Requested qty cannot exceed available qty' });
      this.materialList[index].quantity = 0;
      return false;
    }
    data.materialcost = reqqty * data.unitprice;

  }
  //Check for requested qty
  onPlantComplete(reqqty: number, avqty: number, material: any, index, data: any) {
    if (avqty < reqqty) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Requested qty cannot exceed available qty' });
      this.materialList[index].quantity = 0;
      return false;
    }

  }

//Add rows
  //add materials for gate pass
  addNewMaterial() {
    if (this.materialList.length <= 0) {
      this.materialistModel = new materialList();
      this.materialList.push(this.materialistModel);
    }
    else {
      debugger;
      if (this.materialList[this.materialList.length - 1].materialid == "" || this.materialList[this.materialList.length - 1].materialid == null) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Add Material' });
        return false;
      }
      else if (this.materialList[this.materialList.length - 1].quantity == 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Quantity' });
        return false;
      }

      this.materialistModel = new materialList();
      this.materialList.push(this.materialistModel);
    }
   
  }

  //Delate row
  removematerial(id: number, matIndex: number) {
    debugger;
    this.materialList.splice(matIndex, 1);
    //if (id != 0) {
    //  this.wmsService.deleteGatepassmaterial(id).subscribe(data => {
    //    //this.gatepassModelList[this.gpIndx].materialList.splice(matIndex, 1);
    //    this.messageService.add({ severity: 'success', summary: 'success Message', detail: 'Material Deleted' });
    //  });
    //}


  }

  //Submit Requested quantity data
  onSubmitReqData() {
    debugger;
    if (this.materialList.length <= 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: "Add material to Request" });
    }
    else {
      var datax2 = this.materialList.filter(function (element, index) {
        return (isNullOrUndefined(element.materialid) || element.materialid == "");
      });
      if (datax2.length > 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Add Material' });
        return false;
      }
      var datax4 = this.materialList.filter(function (element, index) {
        return (isNullOrUndefined(element.materialdescription) || element.materialdescription == "");
      });
      if (datax4.length > 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Add Material Description' });
        return false;
      }
      var datax1 = this.materialList.filter(function (element, index) {
        return (element.quantity > 0);
      });
      if (datax1.length == 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Request Quantity' });
        return false;
      }
      var datax3 = this.materialList.filter(function (element, index) {
        return (element.availableqty < element.availableqty);
      });
      if (datax3.length > 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Requested Qty cannot exceed available qty' });
        return false;
      }
      if (isNullOrUndefined(this.selectedproject) && this.stocktype=="Project Stock") {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Select Project' });
        return false;
      }
      this.spinner.show();
      this.btnreq = false;
      this.materialList.forEach(item => {
        item.requesterid = this.employee.employeeno;
        item.remarks = this.requestremarks;
        if (this.stocktype == "Project Stock") {
          item.projectcode = this.selectedproject.value;
        }
        item.materialtype = this.stocktype;
        item.managerid = this.immidiatemanagerid;
        if (item.quantity == null)
          item.quantity = 0;
      })

      this.materialList = this.materialList.filter(function (element, index) {
        return (element.quantity > 0);
      });
      this.wmsService.materialRequestUpdate(this.materialList).subscribe(data => {
        this.spinner.hide();
        if (data) {
          this.requestDialog = false;
          //this.getdefaultmaterialstorequest();
          this.getMaterialRequestlist();
          this.btnreq = true;
          this.messageService.add({ severity: 'success', summary: '', detail: 'Request sent' });
          //this.router.navigateByUrl("/WMS/MaterialReqView/" + this.pono);
        }
        else {
          this.btnreq = true;
          this.messageService.add({ severity: 'error', summary: '', detail: 'Error while sending Request' });
        }

      });
    
      
    }
  }


  //On PO Selected event
  onPOSelected() {
    debugger;
    this.materialList = [];
    this.selectedsupplier = "";
    var pono = "";
    this.displayDD = false;
    this.displaylist = true;
    if (this.selectedmuliplepo && this.selectedmuliplepo.length > 0) {
      var i = 0;
      this.selectedmuliplepo.forEach(item => {
        if (i > 0) {
          pono += ",";
        }
        pono += "'"+item.pono+"'";
        i++;
      });
      this.wmsService.getMaterialswithstore(pono, this.selectedproject.value).subscribe(data => {
        this.materialList = data;
        this.pono = pono;
        this.displayDD = false;
        this.displaylist = true;
      });
    }
   
    //if (this.ponolist.filter(li => li.pono == pono).length > 0) {
    //    this.pono = pono;
    //    this.displayDD = false;
    //    this.displaylist = true;

    //  //this.wmsService.getMaterialRequestlistdata(this.employee.employeeno, this.pono, this.selectedproject.value).subscribe(data => {
    //  //  this.materialList = data;
    //  //});
    //  this.wmsService.getMaterialRequestlistdataforgp(this.pono, this.selectedproject.value).subscribe(data => {
    //    this.materialList = data;
    //  });

      
       
      
      
    //}
    //else {
    //  this.requestMatData.suppliername == null;
    //  this.displayDD = true;
    //  this.pono = null;
    //  this.displaylist = false;
     
     
    //}
  }

  onSupplierSelected() {
    debugger;
    this.materialList = [];
    this.selectedpono = "";
    this.selectedmuliplepo = [];
    var pono = this.selectedsupplier;
    if (this.ponolist.filter(li => li.suppliername == pono).length > 0) {
      this.pono = pono;
      this.displayDD = false;
      this.displaylist = true;
      this.wmsService.getMaterialRequestlistdata(this.employee.employeeno, this.pono, this.selectedproject.value).subscribe(data => {
        this.materialList = data;
      });

    }
    else {
      this.requestMatData.suppliername == null;
      this.displayDD = true;
      this.pono = null;
      this.displaylist = false;


    }
  }

  //On close of drop down
  close() {
    this.suppliername = null;
    this.selectedpono = "";
    this.selectedmuliplepo = [];
    this.selectedsupplier = "";
    this.selectedproject = null;
    this.ponumber = null;
    this.requestMatData = new requestData();
    this.materialList = [];
    this.materialmodel = [];
  }

  resetmaterial() {
    this.suppliername = null;
    this.selectedpono = "";
    this.selectedmuliplepo = [];
    this.selectedsupplier = "";
    this.ponumber = null;
    this.requestMatData = new requestData();
    this.materialList = [];
    this.materialmodel = [];
    this.displayDD = true;
    this.displaylist = false;
  }

  //On supplier name selected
  //On supplier name selected
  onsuppSelected(suppname: string) {
    debugger;
    if (this.ponolist.filter(li => li.pono == suppname).length > 0) {
      if (suppname != "All") {
      this.ponumber = suppname;
      this.pono = suppname;
      this.displayDD = false;
        this.displaylist = true;
        this.materialList = [];
        this.wmsService.getMaterialRequestlistdata(this.employee.employeeno, this.pono, this.selectedproject.value).subscribe(data => {
        this.materialList = data;
      });
      }

    }
    else {
           this.requestMatData.pono == null;
        this.pono = null;
        this.displayDD = true;
      this.displaylist = false;
      this.materialList[0].material = "";
      this.materialList[0].issuedqty = 0;
      this.materialList[0].materialcost = 0;
      this.materialList[0].availableqty = 0;
    }

    //if (suppname == "undefined") {
    //  this.requestMatData.pono == null;
    //  this.pono = null;
    //  this.displayDD = true;
    //  this.displaylist = false;
    //}
    //else if (suppname != "All") {
    //  this.ponumber = suppname;
    //  this.pono = suppname;
    //  this.displayDD = false;
    //  this.displaylist = true;
    //  this.wmsService.getMaterialRequestlistdata(this.employee.employeeno, this.pono).subscribe(data => {
    //    this.materialList = data;
    //  });
    //}
    //else {

    //    this.requestMatData.pono == null;
    //    this.pono = null;
    //    this.displayDD = true;
    //    this.displaylist = false;
     
    //}
    //}
  }

  //Check if material is already selected in material list drop down
  onMaterialSelected(material: any, index: any) {
    debugger;
    if (this.materialList.filter(li => li.material == material).length > 1) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Material already exist' });
      this.materialList[index].material = "";
      return false;
    }
    //add material price

    var data = this.materialmodel.find(li => li.material == material);
    console.log(data);
    //console.log(this.gatepasslistmodel);
    this.materialList[index].materialcost = data["materialcost"] != null ? data["materialcost"] : 0;
    this.materialList[index].availableqty = data["availableqty"] != null ? data["availableqty"] : 0;
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
        this.messageService.add({ severity: 'success', summary: '', detail: 'Request sent' });
      else
        this.messageService.add({ severity: 'error', summary: '', detail: 'Update Failed' });

    });
  }

  onSubmitMaterialData() {
    this.messageService.add({ severity: 'success', summary: '', detail: 'Materials Requested successfully' });
  }

  //app
  ackStatusChanges(status) {
    this.showAck = true;
    if (this.requestList.filter(li => li.status == true).length == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select atleast  one checkbox' });
      this.showAck = false;
    }
    //if (status == 'received') {
    //  this.showAck = false;
    //}
    //else {
    //  this.showAck = true;
    //}
  }

  onMaterialSelected1(data: any, ind: number) {
    debugger;
    if (!isNullOrUndefined(data.materialdescriptionobj)) {
      this.materialList[ind].materialid = this.materialList[ind].materialobj.code;
      var matdesc = data.materialdescriptionobj.code;
      var matcode = this.materialList[ind].materialobj.code;
      var data1 = this.materialList.filter(function (element, index) {
        return (element.materialid == matcode && element.materialdescription == matdesc && index != ind);
      });
      if (data1.length > 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Material already exist' });
        this.materialList.splice(ind, 1);
        this.addNewMaterial();
        return false;
      }
      matdesc = matdesc.replace("'", "''");
      if (this.stocktype == "PLOS") {
        this.wmsService.getplosstockmatdetail(matcode, matdesc).subscribe(res => {
          if (!isNullOrUndefined(res)) {
            //data.unitprice = res.unitprice;
            data.availableqty = res.availableqty;
          }
        });
      }
      else {
        this.wmsService.getplantstockmatdetail(matcode, matdesc).subscribe(res => {
          if (!isNullOrUndefined(res)) {
            //data.unitprice = res.unitprice;
            data.availableqty = res.availableqty;
          }
        });
      }
    
    }
    else {
     
      this.materialList[ind].materialid = this.materialList[ind].materialobj.code;
    }
   

  }

  onDescriptionSelected(data: any, ind: number) {
    debugger;
    if (!isNullOrUndefined(data.materialobj)) {
      this.materialList[ind].materialdescription = this.materialList[ind].materialdescriptionobj.code;
      var matdesc = data.materialdescriptionobj.code;
      var matcode = this.materialList[ind].materialobj.code;
      var data1 = this.materialList.filter(function (element, index) {
        return (element.materialid == matcode && element.materialdescription == matdesc && index != ind);
      });
      if (data1.length > 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Material and description already exist' });
        this.materialList.splice(ind, 1);
        this.addNewMaterial();
        return false;
      }
      matdesc = matdesc.replace("'", "''");
      if (this.stocktype == "PLOS") {
        this.wmsService.getplosstockmatdetail(matcode, matdesc).subscribe(res => {
          if (!isNullOrUndefined(res)) {
            //data.unitprice = res.unitprice;
            data.availableqty = res.availableqty;
          }
        });
      }
      else {
        this.wmsService.getplantstockmatdetail(matcode, matdesc).subscribe(res => {
          if (!isNullOrUndefined(res)) {
            //data.unitprice = res.unitprice;
            data.availableqty = res.availableqty;
          }
        });
      }
    }
    else {
      this.materialList[ind].materialdescription = this.materialList[ind].materialdescriptionobj.code;

    }

  }




  filtermats(event,data : any) {
    
    this.filteredmats = [];
    for (let i = 0; i < this.defaultmaterialids.length; i++) {
      let brand = this.defaultmaterialids[i].material;
      if (brand.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredmats.push(brand);
      }
      
    }
  }

  filtermatdescs(event, data: any) {
   
    this.filteredmatdesc = [];
    for (let i = 0; i < this.defaultmaterialidescs.length; i++) {
      let pos = this.defaultmaterialidescs[i].materialdescription;
      if (pos.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredmatdesc.push(pos);
      }

    }
  }

  public bindSearchListDatamaterialdesc(event: any) {
    debugger;
    //if (isNullOrUndefined(this.selectedproject) || !this.selectedproject.value) {
    //  this.messageService.add({ severity: 'error', summary: '', detail: 'Select Project' });
    //  return;
    //}
    var searchTxt = event.query;
    //var matid = "";
    //if (!isNullOrUndefined(data.materialObj)) {
    //  matid = data.materialObj.code;
    //}
    if (searchTxt == undefined)
      searchTxt = "";
    searchTxt = searchTxt.replace('*', '%');
    searchTxt = searchTxt.replace("'", "''");
    this.dynamicData = new DynamicSearchResult();
    var query = "select poitemdescription from wms.wms_stock ws where stcktype = 'Plant Stock' group by poitemdescription limit 50";
    this.dynamicData.query = query;
    this.descriptionsearchlist = [];
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.searchresult = data;
      this.descriptionsearchlist = [];
      this.searchresult.forEach(item => {
        let rslt = new searchList();
        rslt.code = String(item["poitemdescription"])
        this.descriptionsearchlist.push(rslt);
      });
    });
  }

  public bindSearchListDatamaterial(event: any, name?: string, desc?: any) {
    debugger;
    var materialiddesc = "";
    if (!isNullOrUndefined(desc)) {
      materialiddesc = desc.code;
    }
    var searchTxt = event.query;
    if (searchTxt == undefined)
      searchTxt = "";
    searchTxt = searchTxt.replace('*', '%');
    materialiddesc = materialiddesc.replace("'", "''");
    this.dynamicData = new DynamicSearchResult();
    var query = "";
    if (this.stocktype == "PLOS") {
      query = "select materialid from wms.wms_stock ws where receivedtype = 'Material Return'";
      query += " and materialid ilike '" + searchTxt + "%'";
      if (!isNullOrUndefined(desc)) {
        query += " and poitemdescription = '" + materialiddesc + "'";

      }
      query += " group by materialid limit 50";
     
    }
    else {
      query = "select materialid from wms.wms_stock ws where stcktype = 'Plant Stock'";
      query += " and materialid ilike '" + searchTxt + "%'";
      if (!isNullOrUndefined(desc)) {
        query += " and poitemdescription = '" + materialiddesc + "'";

      }
      query += " group by materialid limit 50";

    }
   
    this.dynamicData.query = query;
    this.filteredmats = [];
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.searchresult = data;
      this.filteredmats = data;
      this.searchItems = [];

      var fName = "";
      this.searchresult.forEach(item => {
        fName = String(item["materialid"]);
        //fName = item[this.constants[name].fieldId];
        var value = { listName: fName, name: fName, code: fName };
        this.searchItems.push(value);
      });
    });
  }

  approverListdata() {

    this.wmsService.getapproverdata(this.employee.employeeno).
      subscribe(
        res => {

          if (!isNullOrUndefined(res[0].approverid) && String(res[0].approverid).trim() != "") {
            this.immidiatemanagerid = res[0].approverid;
            
          }
          else {
            this.immidiatemanagerid = res[0].departmentheadid;
           
          }


        });
  }

  public bindSearchListDatamaterialdescs(event: any, name?: string, material?: any) {
    debugger;
    var materialid = "";
    if (!isNullOrUndefined(material)) {
      materialid = material.code;
    }
    var searchTxt = event.query;
    if (searchTxt == undefined)
      searchTxt = "";
    searchTxt = searchTxt.replace('*', '%');
    searchTxt = searchTxt.replace("'", "''");
    this.dynamicData = new DynamicSearchResult();
    var query = "";
    if (this.stocktype == "PLOS") {

      query = "select poitemdescription from wms.wms_stock ws where receivedtype = 'Material Return' ";
      query += " and poitemdescription ilike '" + searchTxt + "%'";
      if (!isNullOrUndefined(material)) {
        query += " and materialid = '" + materialid + "'";
      }
      query += " group by poitemdescription limit 50";


    }
    else {
      query = "select poitemdescription from wms.wms_stock ws where stcktype = 'Plant Stock' ";
      query += " and poitemdescription ilike '" + searchTxt + "%'";
      if (!isNullOrUndefined(material)) {
        query += " and materialid = '" + materialid + "'";
      }
      query += " group by poitemdescription limit 50";

    }
  
    this.dynamicData.query = query;
    this.filteredmatdesc = [];
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.searchresult = data;
      this.filteredmatdesc = data;
      this.searchdescItems = [];
      var fName = "";
      this.searchresult.forEach(item => {
        fName = String(item["poitemdescription"]);
        //fName = item[this.constants[name].fieldId];
        var value = { listName: fName, name: fName, code: fName };
        this.searchdescItems.push(value);
      });
    });
  }

  requestMaterial() {
    debugger;
    this.requestremarks = "";
    this.requestDialog = true;
    this.btnreq = true;
    this.displayDD = false;
    this.pono = null;
    this.displaylist = true;
    this.stocktype = "Project Stock";
    //Get PO number list, project list and materials available
   
    //if (this.materialList.length <= 0) {
    //  this.materialistModel = { material: "", materialdescription: "", quantity: 0, materialcost: 0, remarks: " ", availableqty: 0, issuedqty: 0, requesterid: this.employee.employeeno, stocktype: "", projectcode: "", plantstockavailableqty: 0 };
    //  this.materialList.push(this.materialistModel);
    //}
   
    //this.GetMaterialList();
  }

  projectSelected(event: any) {
    debugger;
    this.filteredpos = [];
    this.filteredsuppliers = [];
    this.selectedpono = null;
    this.selectedmuliplepo = [];
    this.selectedsupplier = null;
    this.materialList = [];
    this.ponolist = [];
    this.defaultmaterialidescs = [];
    this.defaultmaterialids = [];
    this.defaultuniquematerialids = [];
    this.defaultuniquematerialidescs = [];
    this.defaultmaterials = [];
    var prj = this.selectedproject.value;
    this.GetPONo(prj);
    //this.getdefaultmaterialstorequest(prj);
  }

  GetPONo(projectcode: string) {

    this.wmsService.getStorePODetailsbyprojectcode(this.employee.employeeno, projectcode).subscribe(data => {
      this.spinner.hide();
      if (data) {
        this.ponolist = data;
        console.log(this.ponolist);
      } 
      else
        this.messageService.add({ severity: 'error', summary: '', detail: 'Unable to fetch PO data' });
    });
  }

  GetMaterialList() {
    this.wmsService.getMatDetails().subscribe(data => {
      this.spinner.hide();
      if (data)
        this.messageService.add({ severity: 'sucess', summary: '', detail: 'acknowledged' });
      else
        this.messageService.add({ severity: 'error', summary: '', detail: 'acknowledge failed' });
    });
  
  }

  //received material acknowledgement
  materialAckUpdate() {
    debugger;
    if (this.requestList.filter(li => li.status == true).length == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select atleast  one checkbox' });
    }
    else {
      this.spinner.show();
      this.btnDisable = true;
      var senddata = this.requestList.filter(function (element, index) {
        return (element.status == true && isNullOrUndefined(element.ackstatus));
      });
      this.wmsService.ackmaterialreceived(senddata).subscribe(data => {
        this.spinner.hide();
        debugger;
        if (data)
          this.messageService.add({ severity: 'success', summary: '', detail: 'acknowledged' });
        else
          this.messageService.add({ severity: 'error', summary: '', detail: 'acknowledge failed' });
        this.showAck = false;
        this.getMaterialRequestlist();
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
  //showmaterialdetails() {
  //  if (this.requestid == undefined) {
  //    this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Please select any Request Id' });
  //    //this.router.navigateByUrl("/WMS/MaterialReqView");
  //  }
  //  else {


  //    //this.rowindex = rowindex
  //    this.AddDialog = true;
  //    this.showdialog = true;
  //    //this.materiallistData = this.requestList.filter(li => li.approvedstatus == 'Approved');
  //    this.wmsService.getmaterialissueList(this.requestid).subscribe(data => {
  //      this.materiallistData = data;

  //      if (data != null) {

  //      }
  //    });
  //  }
  //}
  showmaterialdetails(requestid, data: MaterialTransaction) {
    debugger;
    this.materiallistData = [];
    this.isreservedbefore = false;
    this.reserveidview = "";
    this.requestview = requestid;
    if (data.reserveid) {
      this.isreservedbefore = true;
      this.reserveidview = String(data.reserveid);
    }
    this.AddDialog = true;
    this.showdialog = true;
    this.materiallistData = data.materialdata;
    //this.wmsService.getmaterialissueList(requestid).subscribe(data => {
    //  this.materiallistData = data;

    //  if (data != null) {

    //  }
    //});
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
  //bind materials based search
  public bindSearchListData() {
    this.dynamicData.tableName ="wms.wms_project";
   this.dynamicData.searchCondition = " where projectcode is not null";
    this.wmsService.GetListItems(this.dynamicData).subscribe(res => {

      
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
    this.wmsService.UpdateReturnqty(this.materiallistData).subscribe(data => {
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
}
