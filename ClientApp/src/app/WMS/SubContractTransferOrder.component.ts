import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { Materials, stocktransfermodel, invstocktransfermodel, stocktransfermateriakmodel, plantddl, ddlmodel, WMSHttpResponse } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { first } from 'rxjs/operators';
import { isNullOrUndefined } from 'util';

@Component({
  selector: 'app-SubContractTransferOrder',
  templateUrl: './SubContractTransferOrder.component.html'
})
export class SubContractTransferOrderComponent implements OnInit {
  constructor(private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }
  public employee: Employee;
  public defaultmaterialidescs: Materials[] = [];
  public dynamicData = new DynamicSearchResult();
  public searchresult: Array<object> = [];
  public filteredmats: any[];
  public vendorList: any[];
  public filteredmatdesc: any[];
  public mainmodel: invstocktransfermodel;
  public selectedRow: number;
  public podetailsList: Array<stocktransfermateriakmodel> = [];
  public searchItems: Array<searchList> = [];
  public searchdescItems: Array<searchList> = [];
  stocktransferlist: invstocktransfermodel[] = [];
  stocktransferDetaillist: stocktransfermodel[] = [];
  addprocess: boolean = false;
  emptytransfermodel: stocktransfermateriakmodel;
  plantlist: plantddl[] = [];
  projectlists: ddlmodel[] = [];
  selectedproject: ddlmodel;
  filteredprojects: ddlmodel[] = [];
  public cuurentrowresponse: WMSHttpResponse;
  sourceplant: plantddl;
  public vendorObj: any;
  public showAck; btnDisable: boolean = false;
  ponolist: any[] = [];
  selectedpono: string = "";
  filteredpos: any[];
  minDate: Date;


  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.minDate = new Date();
    this.sourceplant = new plantddl();
    this.plantlist = [];
    this.stocktransferlist = [];
    this.projectlists = [];
    this.mainmodel = new invstocktransfermodel();
    this.cuurentrowresponse= new WMSHttpResponse();
    this.emptytransfermodel = new stocktransfermateriakmodel();
    this.getprojects();
    this.getStocktransferdatagroup();
    this.getplantloc();
  }

  getprojects() {

    this.projectlists = [];
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


  //bind materials based search
  public bindSearchListDatamaterial(event: any, rdata: any, name?: string) {
    var searchTxt = event.query;
    var description = "";
    if (!isNullOrUndefined(rdata.materialdescObj)) {
      description = rdata.materialdescObj.name;
    }
    if (searchTxt == undefined)
      searchTxt = "";
    searchTxt = searchTxt.replace('*', '%');
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.tableName = this.constants[name].tableName + " ";
    this.dynamicData.searchCondition = "" + this.constants[name].condition;
    if (name == "material") {
      var query = "(select wp.materialid as material";
      query += " from wms.wms_pomaterials wp";
      query += " left outer join wms.wms_project prj on prj.pono = wp.pono";
      query += " where prj.projectcode = '" + this.selectedproject.value + "' ";
      query += " and wp.materialid ilike '" + searchTxt + "%' ";
      if (!isNullOrUndefined(description) && String(description).trim() != "") {
        query += " and wp.poitemdescription  = '" + description + "' ";
      }
      query += " group by wp.materialid limit 50)";
      query += " union";
      query += " (select mmy.materialid as material";
      query += " from wms.wms_stock mmy";
      query += " left outer join wms.wms_project prj on prj.pono = mmy.pono";
      query += " where prj.projectcode = '" + this.selectedproject.value + "' ";
      query += " and mmy.materialid ilike '" + searchTxt + "%'";
      if (!isNullOrUndefined(description) && String(description).trim() != "") {
        query += " and mmy.poitemdescription  = '" + description + "' ";
      }
      query += " group by mmy.materialid limit 50)";
      this.dynamicData.query = query;
    }
    if (name == "venderid")
      this.dynamicData.searchCondition += "vendorname" + " ilike '%" + searchTxt + "%' or vendorcode" + " ilike '%" + searchTxt + "%'";
    //this.filteredmats = [];
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      if (name == "material") {
        this.searchresult = data;
        this.filteredmats = data;
        this.searchItems = [];

        var fName = "";
        this.searchresult.forEach(item => {
          fName = item[this.constants[name].fieldName];
          fName = item[this.constants[name].fieldId];
          var value = { listName: name, name: fName, code: item[this.constants[name].fieldId] };
          this.searchItems.push(value);
        });
      }
      if (name == "venderid") {
        this.searchresult = data;
        this.vendorList = [];
        this.searchresult.forEach(item => {
          var fName = item[this.constants[name].fieldName] + " - " + item["vendorcode"];
          var value = { listName: name, name: fName, vendorcode: item["vendorcode"], vendorid: item[this.constants[name].fieldId], vendorname: item[this.constants[name].fieldName] };
          this.vendorList.push(value);
        });
      }

    });
  }

  public bindSearchListDatamaterialdesc(event: any, data: any) {
    debugger;
    if (isNullOrUndefined(this.selectedproject) || !this.selectedproject.value) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Project' });
      return;
    }
    var searchTxt = event.query;
    var matid = "";
    if (!isNullOrUndefined(data.materialObj) && !isNullOrUndefined(searchTxt)) {
      matid = data.materialObj.code;
    }
    if (searchTxt == undefined)
      searchTxt = "";
    searchTxt = searchTxt.replace('*', '%');
    this.dynamicData = new DynamicSearchResult();
    var query = "(select wp.poitemdescription as materialdescription";
    query += " from wms.wms_pomaterials wp";
    query += " left outer join wms.wms_project prj on prj.pono = wp.pono";
    query += " where prj.projectcode = '" + this.selectedproject.value + "' ";
    query += " and wp.poitemdescription ilike '" + searchTxt + "%' ";
    if (!isNullOrUndefined(matid) && String(matid).trim() != "") {
      query += " and wp.materialid  = '" + matid + "' ";
    }
    query += " group by wp.poitemdescription limit 50)";
    query += " union";
    query += " (select mmy.poitemdescription as materialdescription";
    query += " from wms.wms_stock mmy";
    query += " left outer join wms.wms_project prj on prj.pono = mmy.pono";
    query += " where prj.projectcode = '" + this.selectedproject.value + "' ";
    query += " and mmy.poitemdescription ilike '" + searchTxt + "%'";
    if (!isNullOrUndefined(matid) && String(matid).trim() != "") {
      query += " and mmy.materialid  = '" + matid + "' ";
    }
    query += " group by mmy.poitemdescription limit 50)";
    this.dynamicData.query = query;
    this.filteredmats = [];
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.searchresult = data;
      this.filtermatdescs = data;
      this.searchdescItems = [];

      var fName = "";
      this.searchresult.forEach(item => {
        fName = item["materialdescription"];
        var value = { listName: 'matdesc', name: fName, code: fName };
        this.searchdescItems.push(value);
      });
    });
  }

  ProjectSelected() {
    this.podetailsList = [];
    this.ponolist = [];
    var prj = this.selectedproject.value;
    this.GetPONo(prj);
  }
  GetPONo(projectcode: string) {

    this.wmsService.getPODetailsbyprojectcode(this.employee.employeeno, projectcode).subscribe(data => {
      this.spinner.hide();
      if (data) {
        this.ponolist = data;
        console.log(this.ponolist);
      }
      else
        this.messageService.add({ severity: 'error', summary: '', detail: 'Unable to fetch PO data' });
    });
  }

  onPOSelected() {
    this.podetailsList = [];
    debugger;
    if (isNullOrUndefined(this.sourceplant.locatorid)) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Source' });
      this.selectedpono = "";
      return;
    }
    var pono = this.selectedpono;
    if (this.ponolist.filter(li => li.pono == pono).length > 0) {

      this.wmsService.getMaterialRequestlistdataforgpandstore(pono, this.selectedproject.value, this.sourceplant.locatorid).subscribe(data => {
        this.podetailsList = data;
      });
    }

  }
  filterpos(event) {
    debugger;
    if (isNullOrUndefined(this.selectedproject)) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Project' });
      return;
    }
    if (isNullOrUndefined(this.sourceplant.locatorid)) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Source' });
      this.selectedpono = "";
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

  addrows() {
    if (!this.sourceplant.locatorid) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select source Plant' });
      return;
    }
    if (!this.vendorObj) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Vendor' });
      return;
    }
    
    if (isNullOrUndefined(this.selectedproject) || !this.selectedproject.value) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Project' });
      return;
    }

      var invalidrow = this.podetailsList.filter(function (element, index) {
        debugger;
        return (!element.transferqty) || (!element.materialid) || (!element.materialdescription) || (!element.requireddate) || (!element.value);
      });
    
    if (invalidrow.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Fill all the details.' });
      return;
    }
    this.emptytransfermodel = new stocktransfermateriakmodel();
    this.podetailsList.push(this.emptytransfermodel);
  }

  deleteRow(index: number) {
    this.podetailsList.splice(index, 1);
  }


  Showadd() {
    this.selectedRow = null;
    this.addprocess = true;
  }

  getplantloc() {
    this.plantlist = [];
    this.wmsService.getplantlocdetails().subscribe(data => {
      console.log(data);
      this.plantlist = data;
    });

  }

  sourceplantchange(event) {
    this.podetailsList = [];
  }

  Showlist() {
    this.addprocess = false;
    this.podetailsList = [];
    this.mainmodel = new invstocktransfermodel();
  }


  checktransferqty(event: any, data: any) {
    debugger;
    if (data.issuedquantity < 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Invalid transfer quantity.' });
      data.issuedquantity = "0";
      return;
    }
    if (data.issuedquantity > data.availableqty) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Please enter issue quantity less than Available quantity' });
      data.issuedquantity = "0";
      return;
    }

  }

  onMaterialSelected1(event: any, data: any, ind: number) {

    debugger;
    if (!isNullOrUndefined(data.materialObj)) {
      this.podetailsList[ind].materialid = data.materialObj.code;
      if (!isNullOrUndefined(this.podetailsList[ind].materialdescription) && this.podetailsList[ind].materialdescription != "") {
        this.cuurentrowresponse = new WMSHttpResponse();
        this.wmsService.getavailabilityByStore(this.sourceplant.locatorid, data.materialObj.code, this.podetailsList[ind].materialdescription, this.selectedproject.value).subscribe(data => {
          debugger;
          if (data) {
            this.cuurentrowresponse = data;
            if (!isNullOrUndefined(this.cuurentrowresponse.message)) {
              this.podetailsList[ind].availableqty = parseInt(this.cuurentrowresponse.message);
              if (!isNullOrUndefined(this.cuurentrowresponse.mvprice) && this.cuurentrowresponse.mvprice.trim() != "" && this.cuurentrowresponse.mvprice.trim() != "0" && !isNullOrUndefined(this.cuurentrowresponse.mvquantity) && this.cuurentrowresponse.mvquantity.trim() != "" && this.cuurentrowresponse.mvquantity.trim() != "0") {
                var price = null;
                var qty = null;
                try {
                  price = parseFloat(this.cuurentrowresponse.mvprice);
                }
                catch{
                  price = null;
                }
                try {
                  qty = parseFloat(this.cuurentrowresponse.mvquantity);
                }
                catch{
                  qty = null;
                }
                if (price != null && qty != null) {
                  this.podetailsList[ind].unitprice = price / qty;
                }
              }
              else {
                this.podetailsList[ind].unitprice = 0;
              }
              if (this.podetailsList[ind].availableqty == 0) {
                this.messageService.add({ severity: 'error', summary: '', detail: 'Material Not available in store :' + this.sourceplant.locatorname });
              }
            }
            else {
              this.podetailsList[ind].availableqty = 0;
              this.messageService.add({ severity: 'error', summary: '', detail: 'Material Not available in store :' + this.sourceplant.locatorname });
            }

          }
          else {
            this.messageService.add({ severity: 'error', summary: '', detail: 'Server error' });
          }


        });
      }

    }
    else {
      this.podetailsList[ind].materialdescObj = null;
      this.podetailsList[ind].materialdescription = null;

    }
    if (!isNullOrUndefined(this.selectedproject)) {
      this.podetailsList[ind].projectid = this.selectedproject.value;
    }
    var data1 = this.podetailsList.filter(function (element, index) {
      return (element.materialid == data.materialObj.code && element.materialdescription == data.materialdescription && index != ind);
    });
    if (data1.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Material and  po item description already exist' });
      this.podetailsList[ind].materialObj = null;
      this.podetailsList[ind].materialid = null;
      this.podetailsList[ind].materialdescObj = null;
      this.podetailsList[ind].materialdescription = null;
      return false;
    }
  }
  checkqty(data: any) {
    if (data.transferqty < 0) {
      data.transferqty = 0;
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter quantity greater than 0' });
      return;
    }
    if (data.transferqty > data.availableqty) {
       data.transferqty = 0;
      this.messageService.add({ severity: 'error', summary: '', detail: 'Transfer quantity exceeded available quantity' });
      return;
    }
    data.materialcost = data.unitprice * data.transferqty;

  }
  onDescriptionSelected(event: any, data: any, ind: number) {
    debugger;
    if (!isNullOrUndefined(data.materialdescObj)) {
      this.podetailsList[ind].materialdescription = data.materialdescObj.name;
      if (!isNullOrUndefined(this.podetailsList[ind].materialid) && this.podetailsList[ind].materialid != "") {
        this.cuurentrowresponse = new WMSHttpResponse();
        this.wmsService.getavailabilityByStore(this.sourceplant.locatorid, this.podetailsList[ind].materialid, data.materialdescObj.name, this.selectedproject.value).subscribe(data => {
          debugger;
          if (data) {
            this.cuurentrowresponse = data;
            if (!isNullOrUndefined(this.cuurentrowresponse.message)) {
              this.podetailsList[ind].availableqty = parseInt(this.cuurentrowresponse.message);
              if (!isNullOrUndefined(this.cuurentrowresponse.mvprice) && this.cuurentrowresponse.mvprice.trim() != "" && this.cuurentrowresponse.mvprice.trim() != "0" && !isNullOrUndefined(this.cuurentrowresponse.mvquantity) && this.cuurentrowresponse.mvquantity.trim() != "" && this.cuurentrowresponse.mvquantity.trim() != "0") {
                var price = null;
                var qty = null;
                try {
                  price = parseFloat(this.cuurentrowresponse.mvprice);
                }
                catch{
                  price = null;
                }
                try {
                  qty = parseFloat(this.cuurentrowresponse.mvquantity);
                }
                catch{
                  qty = null;
                }
                if (price != null && qty != null) {
                  this.podetailsList[ind].unitprice = price / qty;
                }
              }
              else {
                this.podetailsList[ind].unitprice = 0;
              }
              if (this.podetailsList[ind].availableqty == 0) {
                this.messageService.add({ severity: 'error', summary: '', detail: 'Material Not available in store :' + this.sourceplant.locatorname });
              }

            }
            else {
              this.podetailsList[ind].availableqty = 0;
              this.messageService.add({ severity: 'error', summary: '', detail: 'Material Not available in store :' + this.sourceplant.locatorname });
            }

          }
          else {
            this.podetailsList[ind].availableqty = 0;
            this.messageService.add({ severity: 'error', summary: '', detail: 'Server error' });
          }


        });
      }

    }
    else {
      this.podetailsList[ind].materialObj = null;
      this.podetailsList[ind].materialid = null;
    }
    if (!isNullOrUndefined(this.selectedproject)) {
      this.podetailsList[ind].projectid = this.selectedproject.value;
    }
    var data1 = this.podetailsList.filter(function (element, index) {
      return (element.materialdescription == data.materialdescObj.name && element.materialid == data.materialid && index != ind);
    });
    if (data1.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Material and  po item description already exist' });
      this.podetailsList[ind].materialObj = null;
      this.podetailsList[ind].materialid = null;
      this.podetailsList[ind].materialdescObj = null;
      this.podetailsList[ind].materialdescription = null;
      return false;
    }
  }

  filtermatdescs(event, data: any) {
    this.filteredmatdesc = [];
    for (let i = 0; i < this.defaultmaterialidescs.length; i++) {
      let pos = this.defaultmaterialidescs[i].poitemdesc;
      if (pos.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredmatdesc.push(pos);
      }
    }
  }



  getStocktransferdatagroup() {
    this.stocktransferlist = [];
    this.spinner.show();
    this.wmsService.getstocktransferlistgroup1("SubContract").subscribe(data => {
      this.spinner.hide();
      if (data) {
        this.stocktransferlist = data;
        this.stocktransferlist = this.stocktransferlist.filter(li => li.transferredby == this.employee.employeeno);
      }
    });
  }

  showdetails(data: any, index: any) {
    data.showdetail = !data.showdetail;
    this.selectedRow = index;
    this.stocktransferDetaillist = data.materialdata;
  }


  onsubmit() {
    debugger;
    if (this.podetailsList.length == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Add materials.' });
      return;
    }
    if (isNullOrUndefined(this.sourceplant)) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select source locations.' });
      return;
    }
    if (isNullOrUndefined(this.vendorObj)) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Vendor.' });
      return;
    }
    if (isNullOrUndefined(this.mainmodel.remarks) || this.mainmodel.remarks.trim() == "") {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Remarks.' });
      return;
    }
    this.mainmodel.sourceplant = this.sourceplant.locatorname;
    this.mainmodel.vendorcode = this.vendorObj.vendorcode;
    this.mainmodel.vendorname = this.vendorObj.vendorname;
    if (!isNullOrUndefined(this.selectedproject)) {
      this.mainmodel.projectcode = this.selectedproject.value;
      this.mainmodel.approverid = this.selectedproject.projectmanager;
    }
    else {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select project.' });
      return;
    }
    if (!isNullOrUndefined(this.selectedpono)) {
      this.mainmodel.pono = this.selectedpono;
    }
    var volidentry = this.podetailsList.filter(function (element, index) {
      return (element.transferqty > 0)
    });
    if (volidentry.length == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter tranfer quantity.' });
      return;
    }
    var invalidrow = this.podetailsList.filter(function (element, index) {
      return (element.transferqty > 0) && ((!element.materialid) || (!element.materialdescription) || (!element.requireddate) || (!element.materialcost));
    });

    if (invalidrow.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Fill all the details.' });
      return;
    }
    if (String(this.mainmodel.sourceplant).toLowerCase().trim() == "ec c block") {
      this.mainmodel.sourcelocationcode = "3009";

    } else if (String(this.mainmodel.sourceplant).toLowerCase().trim() == "ec unit 2") {
      this.mainmodel.sourcelocationcode = "102B";
    }
    else {
      this.mainmodel.sourcelocationcode = this.sourceplant.locatorname;
    }
    this.podetailsList = this.podetailsList.filter(function (element, index) {
      return (element.transferqty > 0)
    });


    this.mainmodel.transferredby = this.employee.employeeno;
    this.mainmodel.materialdata = this.podetailsList;
    this.mainmodel.transfertype = "SubContract";
    this.spinner.show();
    this.wmsService.Stocktransfer1(this.mainmodel).subscribe(data => {
      this.spinner.hide();
      if (data) {
        this.messageService.add({ severity: 'success', summary: '', detail: 'Sub Contract request created' });
        this.podetailsList = [];
        this.mainmodel = new invstocktransfermodel();
        this.sourceplant = new plantddl();
        this.vendorObj = "";
        this.getStocktransferdatagroup();
        this.addprocess = false;
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Request failed' });
      }
    });
  }

  //app
  ackStatusChanges(index: any) {
    this.showAck = true;
    if (this.stocktransferlist.filter(li => li.Checkstatus == true).length == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select atleast  one checkbox' });
      this.showAck = false;
    }
    if (this.stocktransferlist[index].Checkstatus == true && !this.stocktransferlist[index].ackremarks) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Remarks' });
      this.stocktransferlist[index].Checkstatus = false;
      this.showAck = false;
    }
    if (this.stocktransferlist[index].Checkstatus == true)
      this.stocktransferlist[index].ackby = this.employee.employeeno;
  }

  onAcknowledge() {
    if (this.stocktransferlist.filter(li => li.Checkstatus == true).length == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select atleast  one checkbox' });
    }
    this.btnDisable = true;
    var senddata = this.stocktransferlist.filter(function (element, index) {
      return (element.Checkstatus == true && isNullOrUndefined(element.ackstatus));
    });
    this.spinner.show();
    this.wmsService.updateSubcontractAcKstatus(senddata).subscribe(data => {
      this.spinner.hide();
      this.btnDisable = false;
      if (data) {
        this.messageService.add({ severity: 'success', summary: '', detail: 'Acknowledgement Sent' });
        this.showAck = false;
        this.getStocktransferdatagroup();
      }
      else
        this.messageService.add({ severity: 'error', summary: '', detail: 'Failed' });
    });
  }
}
