import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { PoDetails, BarcodeModel, inwardModel, Materials, stocktransfermodel, locationddl, binddl, rackddl, StockModel, invstocktransfermodel, stocktransfermateriakmodel, plantddl, locationdropdownModel} from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { first } from 'rxjs/operators';
import { isNullOrUndefined } from 'util';

@Component({
  selector: 'app-StockTransfer',
  templateUrl: './StockTransfer.component.html'
})
export class StockTransferComponent implements OnInit {
  
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
  filteredmats: any[];
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
  destlocationlist: locationdropdownModel[];
 
  combomaterial: Materials[];
  locationlist: Array<any> = [];
  racklist: Array<any> = [];
  selectedrack: any;
  binlist: Array<any> = [];
  selectedbin: any;
  selectedlocation: any;

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.stocktransferlist = [];
    this.plantlist = [];
    this.destlocationlist = [];
    this.sourceplant = new plantddl();
    this.mainmodel = new invstocktransfermodel();
    this.emptytransfermodel = new stocktransfermateriakmodel();
    this.showLocationDialog = false;
    this.getplantloc();
    
    
    //this.getStocktransferdata();
    this.getStocktransferdatagroup();
  }

  onSourceChange(event: any) {
    this.podetailsList = [];
    this.getMaterials();
    this.getdestinationdata();
  }

  getstorename(storeid: string) {
    var storelst = this.plantlist.filter((li) => li.locatorid == storeid);
    if (storelst.length > 0) {
      return storelst[0].storagelocationdesc;
    }
    else {
      return storeid;
    }

  }

  getplantloc() {
    debugger;
    this.plantlist = [];
    this.wmsService.getplantlocdetails().subscribe(data => {
      console.log(data);
      this.plantlist = data;
    });
  }
  
  
  deleteRow(index: number) {
    this.podetailsList.splice(index, 1);
    //this.formArr.removeAt(index);
  }
  addrows() {
    if (isNullOrUndefined(this.sourceplant) || !this.sourceplant.locatorid) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select source location' });
      return;
    }
    else {
      var invalidrow = this.podetailsList.filter(function (element, index) {
        debugger;
        return (!element.sourcelocation) || (!element.destinationlocation) || (!element.transferqty) || (!element.materialid);
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
     data.materialdescription = event.value.materialdescription;
    data.materialid = event.value.material;
    data.sourcelocation = "";
    data.destinationlocation = "";
    data.transferqty = "";
    this.showmateriallocationList(event.value.material,index);
  }
  

  setmatlocation(index: number) {
    debugger;
    this.matlocations = [];
    this.podetailsList[index].mlocations = [];
    this.itemlocationData.forEach(item => {
      this.podetailsList[index].mlocations.push(item.itemlocation);
    });
   
  }
  showmateriallocationList(data: any, index: number) {
    this.itemlocationData = [];
    debugger;
    if (data) {
      //this.currentrowindex = rowindex;
      //this.AddDialog = true;
      this.wmsService.getItemlocationListByMaterialsourcelocation(data.materialid,data.materialdescription).subscribe(data => {
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

 

  Showadd() {
    this.displaydetail = false;
    this.selectedRow = null;
    this.addprocess = true;
  }
  Showlist() {
    this.addprocess = false;
    this.podetailsList = [];
    this.mainmodel = new invstocktransfermodel();
    this.savedata = [];
  }
  onSelectsource(data: stocktransfermateriakmodel) {
    debugger;
    data.availableqty = 0;
    data.transferqty = 0;
    data.binid = null;
    data.rackid = null;
    data.storeid = null;
    if ((this.mainmodel.sourceplant) && (this.mainmodel.destinationplant) && (this.mainmodel.sourceplant == this.mainmodel.destinationplant) && (data.sourcelocation == data.destinationlocation)){
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
    var itmrow = data.itemlocationdata.filter((dt) => dt.materialid == data.materialid && dt.materialdescription == data.materialdescription &&  dt.itemlocation == data.sourcelocation);
    if (itmrow.length > 0) {
      data.availableqty = itmrow[0].availableqty;
    }
    

  }

  checkqty(data: stocktransfermateriakmodel) {
    debugger;
    if (data.transferqty < 0) {
      data.transferqty = 0;
      this.messageService.add({ severity: 'error', summary: '', detail: 'Negative value not allowed' });
      return;

    }
    if (data.transferqty > data.availableqty) {
      data.transferqty = 0;
      this.messageService.add({ severity: 'error', summary: '', detail: 'Transfer quantity exceeded available quantity' });
      return;

    }
     
  }
  onSelectdest(data: stocktransfermateriakmodel) {
    data.binid = null;
    data.rackid = null;
    data.storeid = null;
    debugger;
    if ((data.sourcelocation == data.destinationlocation)) {
      data.sourcelocation = "";
      data.destinationlocation = null;
      data.transferqty = 0;
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'source and destination location can not be same' });
      return;
    }
    var itmrow = this.destlocationlist.filter((dt) => dt.itemlocation == data.destinationlocation);
    if (itmrow.length > 0) {
      data.storeid = itmrow[0].locatorid;
      data.rackid = itmrow[0].rackid;
      data.binid = itmrow[0].binid;
    }
  }
 
  search(event) {
    debugger;
    this.results1 = [];
    for (let i = 0; i < this.destlocationlist.length; i++) {

      let brand = this.destlocationlist[i].itemlocation;
      if (brand.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.results1.push(brand);
      }
    }
  }
  search1(event,data : any,index : any) {
    debugger;
    if (data.materialid && data.materialdescription) {
      var dt = [];
      for (let i = 0; i < data.mlocations.length; i++) {

        let brand = data.mlocations[i];
        if (brand.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
          dt.push(brand);
        }
      }
      this.matlocationsearch = dt;
    }
    else {
      data.sourcelocation = "";
      this.messageService.add({ severity: 'error', summary: '', detail: 'Please select material and description.' });
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

  

  onMaterialSelected1(event: any, data: stocktransfermateriakmodel, ind: number) {
    debugger;
    data.sourcelocation = null;
    data.destinationlocation = null;
    data.itemlocationdata = [];
    data.availableqty = 0;
    data.transferqty = 0;
    data.binid = null;
    data.rackid = null;
    data.storeid = null;
    if (!isNullOrUndefined(data.materialdescription) && data.materialdescription != "") {
      var data1 = this.podetailsList.filter(function (element, index) {
        return (element.materialid == data.materialid && element.materialdescription == data.materialdescription && index != ind);
      });
      if (data1.length > 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Material already exist' });
        this.podetailsList[ind].materialid = "";
        this.podetailsList[ind].materialdescription = "";
        return false;
      }

      this.showmateriallocationList(data, ind);
    }
    else {
      var senddata = this.defaultmaterials.filter(function (element, index) {
        return (element.material == data.materialid);
      });
      this.setdesclist(senddata);
     
     

    }

   
  }

  onDescriptionSelected(event:any, data: any, ind: number) {
    debugger;
    data.sourcelocation = null;
    data.destinationlocation = null;
    data.itemlocationdata = [];
    data.availableqty = 0;
    data.transferqty = 0;
    data.binid = null;
    data.rackid = null;
    data.storeid = null;
    if (!isNullOrUndefined(data.materialid) && data.materialid != "") {
      var data1 = this.podetailsList.filter(function (element, index) {
        return (element.materialid == data.materialid && element.materialdescription == data.materialdescription && index != ind);
      });
      if (data1.length > 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Material and description already exist' });
        this.podetailsList[ind].materialid = "";
        this.podetailsList[ind].materialdescription = "";
        return false;
      }
      this.showmateriallocationList(data, ind);
    }
    else {
      var senddata = this.defaultmaterials.filter(function (element, index) {
        return (element.materialdescription == data.materialdescription);
      });
      this.setmatlist(senddata);
    }

  }

  getdestinationdata() {
    this.destlocationlist = [];
    this.wmsService.Getdestinationlocationforist(parseInt(this.sourceplant.locatorid)).
      subscribe(
        res => {
          this.destlocationlist = res;
          //this.binlist = res;
        });
  }


  

  updateremark(datax : any, index: any) {
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

  getMaterials() {
    this.combomaterial = [];
    this.wmsService.getMaterialforstocktransfer(parseInt(this.sourceplant.locatorid)).subscribe(data => {
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
      var desc = item.materialdescription;
      var dt1 = this.defaultmaterialids.filter(function (element, index) {
        return (element.material.toLowerCase() == String(mat).toLowerCase());
      });
      if (dt1.length == 0) {
        this.defaultmaterialids.push(item);
      }
      var dt2 = this.defaultmaterialidescs.filter(function (element, index) {
        if (element.materialdescription != null) {
          return (element.materialdescription.toLowerCase() == String(desc).toLowerCase());
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
    debugger;
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

  getStocktransferdata() {
    this.stocktransferlist = [];
    this.wmsService.getstocktransferlist().subscribe(data => {
      debugger;
      if (data) {
        this.stocktransferlist = data;
      }
    });
  }

  getStocktransferdatagroup() {
    this.stocktransferlist = [];
    this.wmsService.getstocktransferlistgroup1("IST",'').subscribe(data => {
      debugger;
      if (data) {
        this.stocktransferlistgroup = data;
      }
    });
  }
  showdetails(data: any, index:any) {
    debugger;
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
    debugger;
    if (this.podetailsList.length == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Add materials.' });
      return;
    }
    if (!this.sourceplant) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select store.' });
      return;
    }
    
    
      var invalidrow = this.podetailsList.filter(function (element, index) {
        debugger;
        return (!element.sourcelocation) || (!element.destinationlocation) || (!element.transferqty) || (!element.materialid) || (!element.materialdescription);
      });

    if (invalidrow.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Fill all the details.' });
      return;
    }
   
    var svdata = this.mainmodel;
    svdata.transferredby = this.employee.employeeno;
    svdata.materialdata = this.podetailsList;
    svdata.sourceplant = this.sourceplant.locatorid;
    svdata.destinationplant = this.sourceplant.locatorid;

    this.wmsService.Stocktransfer1(svdata).subscribe(data => {
      debugger;
      if (data) {
        this.messageService.add({ severity: 'success', summary: '', detail: 'Material transferred' });
        this.savedata = [];
        this.podetailsList = [];
        this.mainmodel = new invstocktransfermodel();
        this.getStocktransferdatagroup();
        this.addprocess = false;

      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'transfer failed' });
      }
      
      // }
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
  cancektranferlocation() {

  }

}
