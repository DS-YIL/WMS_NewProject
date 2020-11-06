import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { PoDetails, BarcodeModel, inwardModel, Materials, stocktransfermodel, locationddl, binddl, rackddl, StockModel, invstocktransfermodel, stocktransfermateriakmodel} from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { first } from 'rxjs/operators';
import { isNullOrUndefined } from 'util';

@Component({
  selector: 'app-StockTransfer',
  templateUrl: './StockTransfer.component.html'
})
export class StockTransferComponent implements OnInit {
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
 
  combomaterial: Materials[];

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.stocktransferlist = [];
    this.mainmodel = new invstocktransfermodel();
    this.mainmodel.sourceplant = "Plant1";
    this.mainmodel.destinationplant = "Plant1";
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
  addrows() {
    var invalidrow = this.podetailsList.filter(function (element, index) {
      return (!element.sourcelocation) || (!element.destinationlocation) || (!element.transferqty) || (!element.materialid);
    });
    if (invalidrow.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Fill all details.' });
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
    if (material) {
      //this.currentrowindex = rowindex;
      //this.AddDialog = true;
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
    this.mainmodel.sourceplant = "Plant1";
    this.mainmodel.destinationplant = "Plant1";
    this.displaydetail = false;
    this.selectedRow = null;
    this.addprocess = true;
    this.setlocationcombinations();
  }
  Showlist() {
    this.addprocess = false;
    this.podetailsList = [];
    this.mainmodel = new invstocktransfermodel();
    this.savedata = [];
  }
  onSelectsource(data: stocktransfermateriakmodel) {
    debugger;
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
  search1(event,data : any,index : any) {
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

  getMaterials() {
    this.wmsService.getMaterialforstocktransfer().subscribe(data => {
      debugger;
      this.combomaterial = data;
    });
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
    this.wmsService.getstocktransferlistgroup1().subscribe(data => {
      debugger;
      if (data) {
        this.stocktransferlistgroup = data;
      }
    });
  }
  showdetails(data: any, index:any) {
    debugger;
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
    if (!this.mainmodel.sourceplant || !this.mainmodel.destinationplant) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select plants.' });
      return;
    }
    var invalidrow = this.podetailsList.filter(function (element, index) {
      return (!element.sourcelocation) || (!element.destinationlocation) || (!element.transferqty) || (!element.materialid) || (element.transferqty == 0);
    });
    if (invalidrow.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Fill all details.' });
      return;
    }
    if ((this.mainmodel.sourceplant) && (this.mainmodel.destinationplant) && (this.mainmodel.sourceplant == this.mainmodel.destinationplant)) {
      var dataxx = this.podetailsList.filter(function (element, index) {
        return (element.sourcelocation == element.destinationlocation);
      });
      if (dataxx.length > 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Source and destination location can not be same for same source and destination plant.' });
        return;

      } 
    }
    var svdata = this.mainmodel;
    svdata.transferredby = this.employee.employeeno;
    svdata.materialdata = this.podetailsList;
    //svdata.materialdata.forEach(data => {
    //  var itmrow = data.itemlocationdata.filter((dt) => dt.material == data.materialid && dt.itemlocation == data.sourcelocation);
    //  if (itmrow.length > 0) {
    //    data.sourceitemid = itmrow[0].itemid;
    //  }
    //  else {
    //    data.sourceitemid = 0;
    //  }
    //  var itmrow = data.itemlocationdata.filter((dt) => dt.material == data.materialid && dt.itemlocation == data.destinationlocation);
    //  if (itmrow.length > 0) {
    //    data.destinationitemid = itmrow[0].itemid;
    //  }
    //  else {
    //    data.destinationitemid = 0;
    //  }
    //});

    this.wmsService.Stocktransfer1(svdata).subscribe(data => {
      this.messageService.add({ severity: 'success', summary: '', detail: 'Material transferred' });
      this.savedata = [];
      this.podetailsList = [];
      this.mainmodel = new invstocktransfermodel();
      this.mainmodel.sourceplant = "Plant1";
      this.mainmodel.destinationplant = "Plant1";
      this.getStocktransferdatagroup();
      this.addprocess = false;
      // }
    });
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
