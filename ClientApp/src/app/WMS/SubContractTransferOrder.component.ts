import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { Materials, stocktransfermodel, invstocktransfermodel, stocktransfermateriakmodel, plantddl } from 'src/app/Models/WMS.Model';
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
  sourceplant: plantddl;
  public vendorObj: any;
  public showAck; btnDisable: boolean = false;


  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.sourceplant = new plantddl();
    this.plantlist = [];
    this.stocktransferlist = [];
    this.mainmodel = new invstocktransfermodel();
    this.emptytransfermodel = new stocktransfermateriakmodel();
    this.getStocktransferdatagroup();
    this.getplantloc();
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
      query += " where wp.materialid ilike '" + searchTxt + "%' ";
      if (!isNullOrUndefined(description) && String(description).trim() != "") {
        query += " and wp.poitemdescription  = '" + description + "' ";
      }
      query += " group by wp.materialid limit 50)";
      query += " union";
      query += " (select mmy.material";
      query += " from wms.\"MaterialMasterYGS\" mmy";
      query += " where mmy.material ilike '" + searchTxt + "%'";
      if (!isNullOrUndefined(description) && String(description).trim() != "") {
        query += " and mmy.materialdescription  = '" + description + "' ";
      }
      query += " group by mmy.material limit 50)";
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
    query += " where wp.poitemdescription ilike '" + searchTxt + "%' ";
    if (!isNullOrUndefined(matid) && String(matid).trim() != "") {
      query += " and wp.materialid  = '" + matid + "' ";
    }
    query += " group by wp.poitemdescription limit 50)";
    query += " union";
    query += " (select mmy.materialdescription";
    query += " from wms.\"MaterialMasterYGS\" mmy";
    query += " where mmy.materialdescription ilike '" + searchTxt + "%'";
    if (!isNullOrUndefined(matid) && String(matid).trim() != "") {
      query += " and mmy.material  = '" + matid + "' ";
    }
    query += " group by mmy.materialdescription limit 50)";
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

  addrows() {
    if (!this.sourceplant.locatorid) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select source Plant' });
      return;
    }
    else if (!this.vendorObj) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select Vendor' });
      return;
    }

    else {
      var invalidrow = this.podetailsList.filter(function (element, index) {
        debugger;
        return (!element.transferqty) || (!element.materialid) || (!element.projectid) || (!element.requireddate);
      });
    }
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
    }
    else {
      this.podetailsList[ind].materialdescObj = null;
      this.podetailsList[ind].materialdescription = null;

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
  onDescriptionSelected(event: any, data: any, ind: number) {
    debugger;
    if (!isNullOrUndefined(data.materialdescObj)) {
      this.podetailsList[ind].materialdescription = data.materialdescObj.name;
    }
    else {
      this.podetailsList[ind].materialObj = null;
      this.podetailsList[ind].materialid = null;

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
    if (this.podetailsList.length == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Add materials.' });
      return;
    }
    this.mainmodel.sourceplant = this.sourceplant.locatorname;
    this.mainmodel.vendorcode = this.vendorObj.vendorcode;
    this.mainmodel.vendorname = this.vendorObj.vendorname;
    var invalidrow = this.podetailsList.filter(function (element, index) {
      return (!element.transferqty) || (!element.materialid) || (!element.projectid) || (!element.requireddate);
    });

    if (invalidrow.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Fill all the details.' });
      return;
    }

    this.mainmodel.transferredby = this.employee.employeeno;
    this.mainmodel.materialdata = this.podetailsList;
    this.mainmodel.transfertype = "SubContract";
    this.spinner.show();
    this.wmsService.Stocktransfer1(this.mainmodel).subscribe(data => {
      this.spinner.hide();
      if (data) {
        this.messageService.add({ severity: 'success', summary: '', detail: 'Material transfer request created' });
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
