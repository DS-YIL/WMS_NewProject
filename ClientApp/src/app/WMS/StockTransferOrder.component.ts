import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { Materials, stocktransfermodel, locationddl, binddl, rackddl, StockModel, invstocktransfermodel, stocktransfermateriakmodel, plantddl } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { first } from 'rxjs/operators';
import { isNullOrUndefined } from 'util';

@Component({
  selector: 'app-StockTransferOrder',
  templateUrl: './StockTransferOrder.component.html'
})
export class StockTransferOrderComponent implements OnInit {
  locationlist: locationddl[] = [];
  binlist: binddl[] = [];
  racklist: rackddl[] = [];
  selectedbin: binddl; selectedrack: rackddl; selectedlocation: locationddl;
  showLocationDialog: boolean;
  results: string[] = [];
  results1: string[];
  matlocations: string[];
  matlocationsearch: string[];
  isselected: boolean = false;
  public defaultmaterials: Materials[] = [];
  public defaultmaterialids: Materials[] = [];
  public defaultmaterialidescs: Materials[] = [];
  public defaultuniquematerialids: Materials[] = [];
  public defaultuniquematerialidescs: Materials[] = [];
  public dynamicData = new DynamicSearchResult();
  public selectedlist: Array<searchList> = [];
  public searchresult: Array<object> = [];
  filteredmats: any[];
  val: any;
  filteredmatdesc: any[];
  displaySTO: boolean = false;

  mainmodel: invstocktransfermodel;
  selectedRow: number;


  constructor(private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public podetailsList: Array<stocktransfermateriakmodel> = [];
  public employee: Employee;
  emptytransfermodel: stocktransfermateriakmodel;
  stocktransferlist: stocktransfermodel[] = [];
  stocktransferDetaillist: stocktransfermodel[] = [];
  stocktransferlistgroup: invstocktransfermodel[] = [];
  public itemlocationData: Array<any> = [];
  AddDialog: boolean = false;
  showdialog: boolean = false;
  savedata: StockModel[] = [];
  addprocess: boolean = false;
  currentrowindex: number = -1;
  displaydetail: boolean = false;
  matid: string = "";
  matdescription: string = "";
  transferedon: Date;
  plantlist: plantddl[] = [];
  sourceplant: plantddl;
  destinationplant: plantddl;

  combomaterial: Materials[];

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.sourceplant = new plantddl();
    this.destinationplant = new plantddl();
    this.plantlist = [];
    this.stocktransferlist = [];
    this.mainmodel = new invstocktransfermodel();
    this.mainmodel.sourceplant = "1002";
    this.mainmodel.destinationplant = "1003";
    this.emptytransfermodel = new stocktransfermateriakmodel();
    this.selectedbin = new binddl();
    this.selectedlocation = new locationddl();
    this.selectedrack = new rackddl();
    this.showLocationDialog = false;
    this.getMaterials();
    this.locationListdata();
    this.binListdata();
    this.rackListdata();
    //this.getStocktransferdata();
    this.getStocktransferdatagroup();

  }


  deleteRow(index: number) {
    this.podetailsList.splice(index, 1);
    //this.formArr.removeAt(index);
  }


  //bind materials based search
  public bindSearchListDatamaterial(event: any, name?: string) {
    debugger;
    var searchTxt = event.query;
    if (searchTxt == undefined)
      searchTxt = "";
    searchTxt = searchTxt.replace('*', '%');
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.tableName = this.constants[name].tableName + " ";
    this.dynamicData.searchCondition = "" + this.constants[name].condition;
    this.dynamicData.searchCondition += "material" + " ilike '%" + searchTxt + "%'";
    //this.filteredmats = [];
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.filteredmats = data;
    });
  }


  addrows() {
    if (!this.sourceplant.locatorid) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select source Plant' });
      return;
    }
    else if (!this.destinationplant.locatorid) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select destinationplant Plant' });
      return;
    }

    else if (this.sourceplant.locatorid == this.destinationplant.locatorid) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Source and destination plant cannot be same' });
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
  setdescription(event: any, data: any, index: number) {
    debugger;
    data.poitemdesc = event.value.poitemdesc;
    data.materialid = event.value.material;
    data.sourcelocation = "";
    data.destinationlocation = "";
    data.transferqty = "";
    this.showmateriallocationList(event.value.material, index);
  }
  setlocationcombinations() {
    this.results = [];
    this.locationlist.forEach(item => {
      var loc = item.locatorname;
      if (!isNullOrUndefined(loc)) {
        this.racklist.forEach(item => {
          if (!isNullOrUndefined(item.racknumber)) {
            var rac = loc + "." + item.racknumber;
            this.results.push(rac);
            this.binlist.forEach(item => {
              if (!isNullOrUndefined(item.binnumber)) {
                var bin = rac + "." + item.binnumber;
                this.results.push(bin);
              }

            });

          }

        });

      }


    });


  }

  setmatlocation(index: number) {
    debugger;
    this.matlocations = [];
    this.podetailsList[index].mlocations = [];
    this.itemlocationData.forEach(item => {
      this.podetailsList[index].mlocations.push(item.itemlocation);
    });

  }
  showmateriallocationList(material: any, index: number) {
    this.itemlocationData = [];
    debugger;
    if (material) {
      this.wmsService.getItemlocationListByMaterialsourcelocation(material).subscribe(data => {
        this.itemlocationData = data;
        this.podetailsList[index].itemlocationdata = data;
        this.setmatlocation(index);
      });
    }
    else {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Please select material.' });
    }

  }


  settransferloction(data: any, index: number) {
    this.currentrowindex = index;
    this.showLocationDialog = true;
  }

  cancektranferlocation() {
    this.selectedbin = new binddl();
    this.selectedlocation = new locationddl();
    this.selectedrack = new rackddl();
    this.showLocationDialog = false;
  }

  Showadd() {
    //this.mainmodel.sourceplant = "Plant1";
    //this.mainmodel.destinationplant = "Plant1";
    this.displaydetail = false;
    this.selectedRow = null;
    this.addprocess = true;
    this.getplantloc();
    this.setlocationcombinations();
  }

  getplantloc() {
    debugger;
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
    this.savedata = [];
  }
  onSelectsource(data: stocktransfermateriakmodel) {
    debugger;
    if ((this.mainmodel.sourceplant) && (this.mainmodel.destinationplant) && (this.mainmodel.sourceplant == this.mainmodel.destinationplant) && (data.sourcelocation == data.destinationlocation)) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Source and destination location can not be same for same source and destination plant.' });
      data.sourcelocation = null;
      data.destinationlocation = "";
      data.transferqty = 0;
      return;
    }
    var row1 = this.podetailsList.filter((dt) => dt.sourcelocation == data.sourcelocation && dt.materialid == data.materialid && dt.destinationlocation == data.destinationlocation);
    if (row1.length > 1) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Same source and destination location for this material already exists' });
      data.sourcelocation = null;
      data.destinationlocation = "";
      data.transferqty = 0;
      return;
    }
    //var itmrow = data.itemlocationdata.filter((dt) => dt.material == data.materialid && dt.itemlocation == data.sourcelocation);
    //if (itmrow.length > 0) {
    //  data.sourceitemid = itmrow[0].itemid;
    //}
    //else {
    //  data.sourceitemid = 0;
    //}

    if (data.transferqty > 0) {
      this.setqty(data);
    }

  }
  onSelectdest(data: stocktransfermateriakmodel) {
    debugger;
    if ((this.mainmodel.sourceplant) && (this.mainmodel.destinationplant) && (this.mainmodel.sourceplant == this.mainmodel.destinationplant) && (data.sourcelocation == data.destinationlocation)) {
      data.sourcelocation = "";
      data.destinationlocation = null;
      data.transferqty = 0;
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'source and destination location can not be same for same source and destination plant.' });
    }
    var row1 = this.podetailsList.filter((dt) => dt.sourcelocation == data.sourcelocation && dt.materialid == data.materialid && dt.destinationlocation == data.destinationlocation);
    if (row1.length > 1) {
      data.sourcelocation = "";
      data.destinationlocation = null;
      data.transferqty = 0;
      this.messageService.add({ severity: 'error', summary: '', detail: 'Same source and destination location for this material already exists' });

    }

    var destloc = data.destinationlocation.split('.');
    var store = destloc[0];
    var rack = destloc[1];

    var itmrow = this.locationlist.filter((dt) => dt.locatorname == store);
    if (itmrow.length > 0) {
      data.storeid = parseInt(itmrow[0].locatorid);
    }
    else {
      data.storeid = 0;
    }
    var itmrow1 = this.racklist.filter((dt) => dt.racknumber == rack);
    if (itmrow1.length > 0) {
      data.rackid = parseInt(itmrow1[0].rackid);
    }
    else {
      data.rackid = 0;
    }
    if (destloc.length == 3) {
      var bin = destloc[2];
      var itmrow2 = this.binlist.filter((dt) => dt.binnumber == bin);
      if (itmrow2.length > 0) {
        data.binid = parseInt(itmrow2[0].binid);
      }
      else {
        data.binid = 0;
      }

    }
  }
  setqty(data: any) {
    debugger;
    if (data.transferqty < 0) {
      data.transferqty = "0";
      this.messageService.add({ severity: 'error', summary: '', detail: 'Negative value not allowed' });
      return;

    }
    var loc = data.sourcelocation;
    var row = data.itemlocationdata.filter((dt) => dt.itemlocation == loc);
    var toatalavailqty = 0;
    var transferedqty = 0;
    var row1 = this.podetailsList.filter((dt) => dt.sourcelocation == loc && dt.materialid == data.materialid);
    if (row.length > 0) {
      toatalavailqty = row[0].availableqty;
    }

    if (row1.length > 0) {
      row1.forEach(item => {
        transferedqty += item.transferqty;
      });
    }

    if (transferedqty > toatalavailqty) {
      data.transferqty = "0";
      this.messageService.add({ severity: 'error', summary: '', detail: 'Transfer quantity exceeded from available quantity ' + toatalavailqty });

    }

  }
  search(event) {
    debugger;
    this.results1 = this.results.filter((country) => country.startsWith(event.query));
  }
  search1(event, data: any, index: any) {
    debugger;
    if (data.materialid) {
      this.matlocationsearch = data.mlocations.filter((country) => country.startsWith(event.query));
    }
    else {
      data.sourcelocation = "";
      this.messageService.add({ severity: 'error', summary: '', detail: 'Please select material.' });
    }

  }

  Cancel() {
    this.AddDialog = false;
    this.showdialog = false;
    this.itemlocationData = [];
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
    this.podetailsList[ind].materialid = data.materialObj.material;
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.query = "select materialid ,materialdescription,poitemdescription as poitemdesc from wms.wms_pomaterials wp where materialid = '" + data.materialObj.material + "'";
    this.wmsService.GetListItems(this.dynamicData).subscribe(result => {
      this.defaultmaterialidescs = result;
      if (this.defaultmaterialidescs.length == 0)
        this.podetailsList[ind].poitemdesc = data.materialObj.materialdescription;

      var data1 = this.podetailsList.filter(function (element, index) {
        return (element.materialObj.material == data.materialObj.material && element.poitemdesc == data.materialObj.materialdescription && index != ind);
      });
      if (data1.length > 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Material and  po item description already exist' });
        this.podetailsList[ind].materialObj = "";
        this.podetailsList[ind].materialid = "";
        this.podetailsList[ind].poitemdesc = "";
        return false;
      }
    });
  }


  onDescriptionSelected(event: any, data: any, ind: number) {
    if (!isNullOrUndefined(data.materialObj.material) && data.materialObj.material != "") {
      if (this.defaultmaterialidescs.length > 0 && data.poitemdesc)
        this.podetailsList[ind].poitemdesc = data.poitemdesc;
      else
        this.podetailsList[ind].poitemdesc = data.materialObj.materialdescription;
      var data1 = this.podetailsList.filter(function (element, index) {
        return (element.materialObj.material == data.materialObj.material && element.poitemdesc == data.poitemdesc && index != ind);
      });
      if (data1.length > 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Material and  item po description already exist' });
        this.podetailsList[ind].materialid = "";
        this.podetailsList[ind].poitemdesc = "";
        return false;
      }

    }

  }


  locationListdata() {
    this.wmsService.getlocationdata().
      subscribe(
        res => {
          //this._list = res; //save posts in array
          this.locationlist = res;
        });
  }
  binListdata() {
    this.wmsService.getbindata().
      subscribe(
        res => {
          //this._list = res; //save posts in array
          this.binlist = res;
        });
  }
  rackListdata() {
    this.wmsService.getrackdata().
      subscribe(
        res => {
          //this._list = res; //save posts in array
          this.racklist = res;
        });
  }




  updateremark(datax: any, index: any) {
    debugger;
    var currentrow = index;
    var data = this.savedata.filter(function (element, index) {
      return (element.testindex == currentrow);
    });
    if (data.length > 0) {
      data.forEach(item => {
        item.remarks = datax.remarks;
      });
    }
    else {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Please Select quantity to transfer first' });
      datax.remarks = "";
      return;
    }
  }

  updatesavelocation() {
    debugger;
    var currentrow = this.currentrowindex;
    var data = this.savedata.filter(function (element, index) {
      return (element.testindex == currentrow);
    });
    if (data.length > 0) {
      var binnumber;
      var binid;
      var racknumber;
      var rackid;
      var itemlocation = "";

      if (!isNullOrUndefined(this.selectedbin)) {
        binnumber = this.selectedbin.binnumber;
        binid = this.selectedbin.binid;
        if (!isNullOrUndefined(this.selectedrack) && !isNullOrUndefined(this.selectedlocation)) {
          rackid = this.selectedrack.rackid;
          var itemlocation = this.selectedlocation.locatorname + "." + this.selectedrack.racknumber + '.' + this.selectedbin.binnumber;
        }
        else {
          this.messageService.add({ severity: 'error', summary: '', detail: 'Please Select location and rack' });
          return;
        }

      }
      else if (!isNullOrUndefined(this.selectedrack) && isNullOrUndefined(this.selectedbin)) {
        racknumber = this.selectedrack.racknumber;
        rackid = this.selectedrack.rackid;
        if (!isNullOrUndefined(this.selectedlocation)) {
          var itemlocation = this.selectedlocation.locatorname + "." + this.selectedrack.racknumber;
        }
        else {
          this.messageService.add({ severity: 'error', summary: '', detail: 'Please Select location' });
          return;
        }
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Please Select bin' });
        return;
      }

      data.forEach(item => {
        item.itemlocation = itemlocation;
        item.binid = binid;
        item.rackid = rackid;
      });

      this.podetailsList[this.currentrowindex].destinationlocation = itemlocation;
      this.showLocationDialog = false;

    }
    else {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Please Select quantity to transfer first' });
      return;
    }
  }

  updatenewquantity() {
    debugger;
    var currindex = this.currentrowindex;
    this.savedata = this.savedata.filter(function (element, index) {
      return (element.testindex != currindex);
    });
    var data = this.itemlocationData.filter(function (element, index) {
      return (element.issuedquantity > 0);
    });
    var totalqty = 0;
    if (data.length > 0) {
      data.forEach(item => {
        let stock = new StockModel();
        stock.itemid = item.itemid;
        stock.testindex = this.currentrowindex;
        stock.itemlocation = item.itemlocation;
        stock.availableqty = item.issuedquantity;
        totalqty += item.issuedquantity;
        stock.createdby = this.employee.employeeno;
        this.savedata.push(stock);
      });
    }
    this.podetailsList[this.currentrowindex].transferqty = totalqty;
    this.AddDialog = false;

  }



  //Get list of materials from pomaterials table
  getMaterials() {
    this.wmsService.getMaterialforstocktransferorder().subscribe(data => {
      debugger;
      this.combomaterial = data;
      this.defaultmaterials = data;
      this.setmatdesclist(this.defaultmaterials);
    });
  }

  setmatdesclist(datax: Materials[]) {
    debugger;
    var listdata = datax;
    this.defaultmaterialidescs = [];
    this.defaultmaterialids = [];
    this.defaultuniquematerialids = [];
    this.defaultuniquematerialidescs = [];
    listdata.forEach(item => {
      debugger;
      var mat = item.material;
      var desc = item.poitemdesc;
      var dt1 = this.defaultmaterialids.filter(function (element, index) {
        return (element.material.toLowerCase() == String(mat).toLowerCase());
      });
      if (dt1.length == 0) {
        this.defaultmaterialids.push(item);
      }
      var dt2 = this.defaultmaterialidescs.filter(function (element, index) {
        if (element.poitemdesc != null) {
          return (element.poitemdesc.toLowerCase() == String(desc).toLowerCase());
        }

      });
      if (dt2.length == 0) {
        this.defaultmaterialidescs.push(item);
      }

    });
    debugger;
    this.defaultuniquematerialids = this.defaultmaterialids;
    this.defaultuniquematerialidescs = this.defaultmaterialidescs;

  }

  setmatlist(datax: Materials[]) {
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

  setdesclist(datax: Materials[]) {
    debugger;
    var listdata = datax;
    this.defaultmaterialidescs = [];
    listdata.forEach(item => {
      var desc = item.poitemdesc;
      var dt2 = this.defaultmaterialidescs.filter(function (element, index) {
        return (element.poitemdesc.toLowerCase() == String(desc).toLowerCase());
      });
      if (dt2.length == 0) {
        this.defaultmaterialidescs.push(item);
      }

    });
  }


  //filtermats(event, data: any) {


  //  if (!isNullOrUndefined(data.poitemdesc) && data.poitemdesc != "") {
  //    var senddata = this.defaultmaterials.filter(function (element, index) {
  //      return (element.poitemdesc == data.poitemdesc);
  //    });
  //    this.setmatlist(senddata);
  //  }
  //  else {
  //    this.defaultmaterialids = this.defaultuniquematerialids;
  //  }
  //  this.filteredmats = [];
  //  for (let i = 0; i < this.defaultmaterialids.length; i++) {

  //    let brand = this.defaultmaterialids[i].material;
  //    if (brand.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
  //      this.filteredmats.push(brand);
  //    }

  //  }
  //}
  filtermatdescs(event, data: any) {
    this.filteredmatdesc = [];
    for (let i = 0; i < this.defaultmaterialidescs.length; i++) {
      let pos = this.defaultmaterialidescs[i].poitemdesc;
      if (pos.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredmatdesc.push(pos);
      }
    }
  }

  //filtermatdescs(event, data: any) {
  //  debugger;
  //  if (!isNullOrUndefined(data.materialid) && data.materialid != "") {
  //    var senddata = this.defaultmaterials.filter(function (element, index) {
  //      return (element.material == data.materialid);
  //    });
  //    this.setdesclist(senddata);
  //  }
  //  else {
  //    this.defaultmaterialidescs = this.defaultuniquematerialidescs;
  //  }
  //  this.filteredmatdesc = [];
  //  for (let i = 0; i < this.defaultmaterialidescs.length; i++) {
  //    let pos = this.defaultmaterialidescs[i].poitemdesc;
  //    if (pos.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
  //      this.filteredmatdesc.push(pos);
  //    }

  //  }
  //}

  getStocktransferdata() {
    this.stocktransferlist = [];
    this.wmsService.getstocktransferlist().subscribe(data => {
      if (data) {
        this.stocktransferlist = data;
      }
    });
  }

  getStocktransferdatagroup() {
    this.stocktransferlist = [];
    this.spinner.show();
    this.wmsService.getstocktransferlistgroup1("STO").subscribe(data => {
      this.spinner.hide();
      if (data) {
        this.stocktransferlistgroup = data;
      }
    });
  }
  showdetails(data: any, index: any) {
    if (data.transfertype == "STO") {
      this.displaySTO = true;
    }
    else {
      this.displaySTO = false;
    }
    data.showdetail = !data.showdetail;
    this.selectedRow = index;
    this.stocktransferDetaillist = data.materialdata;
    this.matid = data.transferid;
    this.matdescription = data.transferredby;
    this.transferedon = data.transferredon;
    this.displaydetail = true;

    //this.matdescription = data.materialdescription;
    //var dataxx = this.stocktransferlist.filter(function (element, index) {
    //  return (element.materialid == data.materialid && element.transferedon == data.transferedon);
    //});
    //if (dataxx.length > 0) {
    //  this.stocktransferDetaillist = dataxx;
    //}

  }
  getponodetails(data) {
    debugger;
  }


  onsubmit1() {
    if (this.podetailsList.length == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Add materials.' });
      return;
    }

    this.mainmodel.sourceplant = this.sourceplant.locatorname;
    this.mainmodel.destinationplant = this.destinationplant.locatorname;

    if (!this.mainmodel.sourceplant || !this.mainmodel.destinationplant) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select plants.' });
      return;
    }

    if (this.sourceplant.locatorid == this.destinationplant.locatorid) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Source and destination plant cannot be same' });
      return;
    }
    else {
      var invalidrow = this.podetailsList.filter(function (element, index) {
        return (!element.transferqty) || (!element.materialid) || (!element.projectid) || (!element.requireddate);
      });
    }
    if (invalidrow.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Fill all the details.' });
      return;
    }

    if ((this.mainmodel.sourceplant) && (this.mainmodel.destinationplant) && (this.mainmodel.sourceplant == this.mainmodel.destinationplant)) {
      var dataxx = this.podetailsList.filter(function (element, index) {
        return (element.sourcelocation == element.destinationlocation);
      });
    }
    var svdata = this.mainmodel;
    svdata.transferredby = this.employee.employeeno;
    svdata.materialdata = this.podetailsList;

    this.spinner.show();
    this.wmsService.Stocktransfer1(svdata).subscribe(data => {
      this.spinner.hide();
      if (data) {
        this.messageService.add({ severity: 'success', summary: '', detail: 'Material transferred' });
        this.savedata = [];
        this.podetailsList = [];
        this.mainmodel = new invstocktransfermodel();
        this.mainmodel.sourceplant = "Plant1";
        this.mainmodel.destinationplant = "Plant1";
        this.getStocktransferdatagroup();
        this.addprocess = false;
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'transfer failed' });
      }
    });
  }

  plantchange() {
    // this.addprocess = true;
    if (this.mainmodel.destinationplant == this.mainmodel.sourceplant) {
      this.displaySTO = false;
    }
    else {
      this.displaySTO = true;
    }

  }

  //onfromSelectMethod(event) {
  //  this.podetailsList = [];
  //  if (event.toString().trim() !== '') {
  //    this.fromdateview1 = this.datePipe.transform(event, 'yyyy-MM-dd');

  //    this.fromdateview1 += " 00:00:00";
  //    this.getcheckedgrnforqcbydate();
  //  }
  //}

  parseDate(dateString: string): Date {
    if (dateString) {
      return new Date(dateString);
    }
    return null;
  }

  onsubmit() {
    debugger;
    var data = this.savedata;
    this.wmsService.Stocktransfer(this.savedata).subscribe(data => {
      this.messageService.add({ severity: 'success', summary: '', detail: 'Material transferred' });
      this.savedata = [];
      this.podetailsList = [];
      this.getStocktransferdata();
      this.addprocess = false;
      // }
    });
  }


}
