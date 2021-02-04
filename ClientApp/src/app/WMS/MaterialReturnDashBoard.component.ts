import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl, ValidatorFn } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { materialRequestDetails, StockModel, PoDetails, locataionDetailsStock, inwardModel } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { ConfirmationService } from 'primeng/api';
import { isNullOrUndefined } from 'util';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-MaterialReturnDashBoard',
  templateUrl: './MaterialReturnDashBoard.component.html',
  animations: [
    trigger('rowExpansionTrigger', [
      state('void', style({
        transform: 'translateX(-10%)',
        opacity: 0
      })),
      state('active', style({
        transform: 'translateX(0)',
        opacity: 1
      })),
      transition('* <=> *', animate('400ms cubic-bezier(0.86, 0, 0.07, 1)'))
    ])
  ],
  providers: [DatePipe,ConfirmationService]
})
export class MaterialReturnDashBoardComponent implements OnInit {
  binid: any;
  rackid: any;
  locatorid: any;
  rowGroupMetadata: any;
  public store: any;
  public rack: any;
  public locationdetails = new locataionDetailsStock();
  public bin: any;
  public locationdata: any[] = [];
  public bindata: any[] = [];
  public rackdata: any[] = [];
  public disSaveBtn: boolean = false;
    selectedStatus: any;
  constructor(private ConfirmationService: ConfirmationService,private formBuilder: FormBuilder, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }
  AddDialog: boolean;
  showdialog: boolean;
  btnDisable: boolean = false;
  public showDetails; showLocationDialog: boolean = false;
  public locationlist: any[] = [];
  public locationlists: any[] = [];
  public binlist: any[] = [];
  public racklist: any[] = [];
  public StockModelForm: FormGroup;
  public StockModel: StockModel;
  public stock: StockModel[] = [];
  public StockModelList: Array<any> = [];
  matid: string = "";
  matdescription: string = "";
  matqty: string = "";
  public materiallistData: Array<any> = [];
  public formName: string;
  public txtName: string;
  public dynamicData = new DynamicSearchResult();
  public showList: boolean = false;
  public searchItems: Array<searchList> = [];
  public selectedlist: Array<searchList> = [];
  public selectedItem: searchList;
  public searchresult: Array<object> = [];
  public PoDetails: PoDetails;
  public MaterialRequestForm: FormGroup
  public materialIssueList: Array<any> = [];
  public materialIssueAccept: Array<any> = [];
  public materialacceptListnofilter: Array<any> = [];
  public podetailsList: Array<inwardModel> = [];
  public employee: Employee;
  public matreturnid: any;
  public displayItemRequestDialog; RequestDetailsSubmitted: boolean = false;
  public materialRequestDetails: materialRequestDetails;
  public rowIndex: number;
  public emailreturnid = "";
  currentstocktype: string = "";
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.emailreturnid = this.route.snapshot.queryParams.returnid;
    this.StockModel = new StockModel();
    this.locationListdata();
    this.binListdata();
    this.rackListdata();
    this.getMaterialIssueList();
    
  }

  //get material issue list based on loginid
  getMaterialIssueList() {
    debugger;
    //this.employee.employeeno = "400095";
    this.wmsService.GetReturnmaterialList().subscribe(data => {
      this.materialacceptListnofilter = data;
      this.materialIssueList = this.materialacceptListnofilter.filter(li => li.putawaystatus == 'Pending');
      if (!isNullOrUndefined(this.emailreturnid) && this.emailreturnid != "") {
        var rtnid = this.emailreturnid;
        this.materialIssueList = this.materialIssueList.filter(li => li.returnid == rtnid);
        this.emailreturnid = "";
      }
     
    });
  }

  backtoreturn() {
    this.AddDialog = false;
    this.getMaterialIssueList();
    this.materialIssueList = [];
  }

  onQtyClick(index: any) {
    debugger;
    if (this.stock.length > 1) {
      for (var data = 0; data < this.stock.length - 1; data++) {
        if (this.stock[data].locatorid == this.stock[this.stock.length - 1].locatorid) {
          if (this.stock[data].rackid == this.stock[this.stock.length - 1].rackid) {
            if (this.stock[data].binid == this.stock[this.stock.length - 1].binid) {
              this.messageService.add({ severity: 'error', summary: '', detail: 'Location already exists' });
              this.stock[this.stock.length - 1].binid = 0;
              this.stock[this.stock.length - 1].rackid = 0;
              this.stock[this.stock.length - 1].locatorid = 0;
              return;
            }
          }
        }
      }

    }
    //get stock type

      //this.locationdetails.storeid = this.stock[index].locatorid;
      //this.locationdetails.rackid = this.stock[index].rackid;
      //this.locationdetails.binid = this.stock[index].binid;
      //var bindetails = this.bindata.filter(x => x.binid == this.locationdetails.binid);
      //var storedetails = this.locationdata.filter(x => x.locatorid == this.locationdetails.storeid);
      //var rackdetails = this.rackdata.filter(x => x.rackid == this.locationdetails.rackid);
      //this.locationdetails.storename = storedetails[0].locatorname != null || storedetails[0].locatorname != "undefined" || storedetails[0].locatorname != "" ? storedetails[0].locatorname : 0;
      //this.locationdetails.rackname = rackdetails[0].racknumber != null || rackdetails[0].racknumber != "undefined" || rackdetails[0].racknumber != "" ? rackdetails[0].racknumber : 0;
      //this.locationdetails.binname = bindetails[0].binnumber != null || bindetails[0].binnumber != "undefined" || bindetails[0].binnumber != "" ? bindetails[0].binnumber : 0;
      //this.locationdetails.locationid = this.locationdetails.storeid + '.' + this.locationdetails.rackid + '.' + this.locationdetails.binid;
      //this.locationdetails.locationname = this.locationdetails.storename + '.' + this.locationdetails.rackname + '.' + this.locationdetails.binname;

    
    
   


  }

  deleteRow(index: number) {
    this.stock.splice(index, 1);
    //this.formArr.removeAt(index);
  }
  addNewRow() {
    //this.onQtyClick();
    debugger;
    if (this.stock.length > 0) {
      if (this.stock[this.stock.length - 1].locatorid != 0 || this.stock[this.stock.length - 1].qty != 0 || this.stock[this.stock.length - 1].qty != null) {
        if (this.stock[this.stock.length - 1].locatorid == 0 || isNullOrUndefined(this.stock[this.stock.length - 1].locatorid )) {
          this.messageService.add({ severity: 'error', summary: '', detail: 'select Location' });
          return;
        }
        if (this.stock[this.stock.length - 1].rackid == 0 || isNullOrUndefined(this.stock[this.stock.length - 1].rackid)) {
          this.messageService.add({ severity: 'error', summary: '', detail: 'select Rack' });
          return;
        }
        if (this.stock[this.stock.length - 1].qty == 0 || this.stock[this.stock.length - 1].qty == null) {
          this.messageService.add({ severity: 'error', summary: '', detail: 'Enter quantity' });
          return;
        }
        if (this.stock.length > 1) {
          for (var data = 0; data < this.stock.length - 1; data++) {
            if (this.stock[data].locatorid == this.stock[this.stock.length - 1].locatorid) {
              if (this.stock[data].rackid == this.stock[this.stock.length - 1].rackid) {
                if (this.stock[data].binid == this.stock[this.stock.length - 1].binid) {
                  this.messageService.add({ severity: 'error', summary: '', detail: 'Location already exists' });
                  this.stock[this.stock.length - 1].binid = 0;
                  this.stock[this.stock.length - 1].rackid = 0;
                  this.stock[this.stock.length - 1].locatorid = 0;
                  return;
                }
              }
            }
          }
        }
        var stockdata = new StockModel();
        stockdata.locationlists = this.locationlists;
        stockdata.locatorid = null;
        stockdata.rackid = null;
        stockdata.binid = null;
        stockdata.stocktype = this.currentstocktype;
        this.stock.push(stockdata);
        //this.stock.push(new StockModel());
      }

    }
    else {
      var stockdata = new StockModel();
      stockdata.locationlists = this.locationlists;
      stockdata.locatorid = null;
      stockdata.rackid = null;
      stockdata.binid = null;
      stockdata.stocktype = this.currentstocktype;
      this.stock.push(stockdata);
      //this.stock.push(new StockModel());
    }
  }


  showDialog(details: any, index: number) {
    debugger;
    this.currentstocktype = details.stocktype;
    this.matqty = details.returnqty;
    this.showLocationDialog = true;
    this.PoDetails = details;
    this.rowIndex = index;
    details.binid = details.defaultbin;
    details.rackid = details.defaultrack;
    details.storeid = details.defaultstore;
    this.binid = details.binid;
    this.rackid = details.rackid;
    this.matid = details.materialid;
    //this.invoiceForm.controls.itemRows.value[this.invoiceForm.controls.itemRows.value.length - 1].storeid = details.storeid;
    //this.invoiceForm.controls.itemRows.value[this.invoiceForm.controls.itemRows.value.length - 1].rackid = details.rackid;
    //this.invoiceForm.controls.itemRows.value[this.invoiceForm.controls.itemRows.value.length - 1].binid = details.binid;
    //this.invoiceForm.controls.itemRows.value[this.invoiceForm.controls.itemRows.value.length - 1].stocktype = "Plantstock";
    this.StockModel.locatorid = details.storeid;
    this.StockModel.rackid = details.rackid;
    this.StockModel.binid = details.binid;
    this.matdescription = details.poitemdescription;
    //this.matqty = details.receivedqty;

    this.StockModelForm = this.formBuilder.group({
      rackid: [details.rackid],
      binid: [details.binid],
      locatorid: [details.storeid]
    });

    if (this.stock.length == 0) {
      var stockdata = new StockModel();
      stockdata.locationlists = this.locationlists;
      stockdata.locatorid = details.storeid;
      stockdata.rackid = details.rackid;
      stockdata.binid = details.binid;
      stockdata.stocktype = details.stocktype;
      this.stock.push(stockdata);
      if (stockdata.locatorid) {
        this.onlocUpdate(stockdata.locatorid, stockdata);
      }
      if (stockdata.locatorid && stockdata.rackid) {
        this.onrackUpdate(stockdata.locatorid, stockdata.rackid, stockdata);
      }
      
      

    }

   
    this.rack = "";
    this.bin = "";
    this.store = "";


  }

  close() {
    //alert(this.stock.length);
    this.stock = [];
    //this.locationlist = [];
    //this.binlist = [];
    //this.racklist = [];
    //alert(this.stock.length);
  }


  updateRowGroupMetaData() {
    debugger;
    this.rowGroupMetadata = {};
    if (this.materiallistData) {
      var count = 0;
      for (let i = 0; i < this.materiallistData.length; i++) {
        let rowData = this.materiallistData[i];
        let materialid = rowData.materialid;
        if (i == 0) {
          this.rowGroupMetadata[materialid] = { index: 0, size: 1 };
          count = count + 1;
          this.materiallistData[i].serialno = count;
        }
        else {
          let previousRowData = this.materiallistData[i - 1];
          let previousRowGroup = previousRowData.materialid;
          if (materialid === previousRowGroup)
            this.rowGroupMetadata[materialid].size++;
          else {
            this.rowGroupMetadata[materialid] = { index: i, size: 1 };
            count = count + 1;
            this.materiallistData[i].serialno = count;
          }


        }
      }
    }
  }

  showmaterialdetails(matreturnid) {
    this.materiallistData = [];
    //this.rowindex = rowindex
    this.AddDialog = true;
    this.showdialog = true;
    this.matreturnid = matreturnid;
    this.wmsService.getmaterialreturnreqList(matreturnid).subscribe(data => {
      this.materiallistData = data;
      debugger;
      this.updateRowGroupMetaData();
      if (data != null) {

      }
    });
  
  }
  ConfirmReturnmaterial() {
    if (this.materiallistData[0].itemlocation == null) {
      this.messageService.add({ severity: 'error', summary: ' ', detail: 'Please select item location' });
      return false;
    }

  else if (this.materiallistData[0].itemlocation == 'other') {
      if (this.StockModel.locatorid == undefined) {
        this.messageService.add({ severity: 'error', summary: ' ', detail: 'Please select store' });
        return false;
      }
     if (this.StockModel.rackid == undefined) {
        this.messageService.add({ severity: 'error', summary: ' ', detail: 'Please select Location' });
        return false;
      }
       if (this.StockModel.rackid == undefined && this.StockModel.binid == undefined) {
        this.messageService.add({ severity: 'error', summary: ' ', detail: 'Please select Rack or Bin' });
        return false;
      }
      if (this.StockModel.rackid != undefined || this.StockModel.rackid != undefined || this.StockModel.binid != undefined) {
        var bindetails = this.binlist.filter(x => x.binid == this.StockModel.binid);
        var storedetails = this.locationlists.filter(x => x.locatorid == this.StockModel.locatorid);
        var rackdetails = this.racklist.filter(x => x.rackid == this.StockModel.rackid);
       if (this.StockModel.binid == undefined)
         this.materiallistData[0].itemlocation = storedetails[0].locatorname + "." + rackdetails[0].racknumber;
       if (this.StockModel.rackid == undefined)
         this.materiallistData[0].itemlocation = storedetails[0].locatorname + "." + bindetails[0].binnumber;
       if (this.StockModel.binid != undefined && this.StockModel.rackid != undefined)
         this.materiallistData[0].itemlocation = storedetails[0].locatorname + "." + rackdetails[0].racknumber + "." + bindetails[0].binnumber;
      }
      
    }
      this.wmsService.UpdateReturnmaterialTostock(this.materiallistData).subscribe(data => {
        this.AddDialog = false;
        if (data == 1) {
          this.messageService.add({ severity: 'success', summary: ' ', detail: 'Items updated to Store' });
          this.router.navigateByUrl("/WMS/MaterialReturn");
        }

      })
    
    
  }
  public bindSearchListData(materialid) {
    this.dynamicData.tableName = "wms.wms_stock";
    this.dynamicData.searchCondition = " where materialid='" + materialid+"'";
    this.wmsService.GetListItems(this.dynamicData).subscribe(res => {

      //this.locationlist = res;
      //let _list: any[] = [];
      //for (let i = 0; i < (res.length); i++) {
      //  _list.push({
      //    itemlocation: res[i].itemlocation,
      //    locatorname: res[i].locatorname,
      //    // projectcode: res[i].projectcode
      //  });
      //}
      //this.locationlist = _list;
      //this.locationdata = _list;
    });
  }



  locationListdata() {
    debugger;
    this.wmsService.getlocationdata().
      subscribe(
        res => {
          debugger;
          //this._list = res; //save posts in array
          this.locationlists = res;
          let _list: any[] = [];
          for (let i = 0; i < (res.length); i++) {
            _list.push({
              locatorid: res[i].locatorid,
              locatorname: res[i].locatorname
            });
          }
          this.locationlists = _list;
         // this.locationlist = _list;
          this.locationdata = _list;
        });
  }

  //On selection of location updating rack
  onlocUpdate(locationid: any, rowData:any) {
    debugger;
    rowData.racklist = [];
    if (this.rackdata.filter(li => li.locationid == locationid).length > 0) {
      this.racklist = [];
      console.log(this.racklist);
      this.rackdata.forEach(item => {
        if (item.locationid == locationid) {
          rowData.racklist.push(item);
          console.log(this.racklist);
        }
      })

    }

  }

  //On selection of rack updating bin
  onrackUpdate(locationid: any, rackid: any, rowData:any) {
    debugger;
    rowData.binlist = [];
    if (this.bindata.filter(li => li.locationid == locationid && li.rackid == rackid).length > 0) {
      this.binlist = [];
      console.log(this.binlist);
      this.bindata.forEach(item => {
        if (item.locationid == locationid && item.rackid == rackid) {
          rowData.binlist.push(item);
          console.log(this.binlist);
        }
      })

    }

  }


  binListdata() {
    debugger;
    this.wmsService.getbindataforputaway().
      subscribe(
        res => {
          //this._list = res; //save posts in array
          this.binlist = res;
          let _list: any[] = [];
          for (let i = 0; i < (res.length); i++) {
            _list.push({
              locationid: res[i].locatorid,
              binid: res[i].binid,
              rackid: res[i].rackid,
              binnumber: res[i].binnumber
            });
          }
          this.binlist = _list;
          this.bindata = _list;
        });
  }
  rackListdata() {
    debugger;
    this.wmsService.getrackdataforputaway().
      subscribe(
        res => {
          //this._list = res; //save posts in array
          this.racklist = res;
          let _list: any[] = [];
          for (let i = 0; i < (res.length); i++) {
            _list.push({
              binid: res[i].binid,
              rackid: res[i].rackid,
              racknumber: res[i].racknumber,
              locationid: res[i].locatorid
            });
          }
          this.racklist = _list;
          this.rackdata = _list;
        });
  }

  onSubmitStockDetails() {
    debugger;
    if (this.stock.length > 0) {
      debugger;
      if (this.stock[this.stock.length - 1].locatorid == 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'select Location' });
        return;
      }
      if (this.stock[this.stock.length - 1].rackid == 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'select Rack' });
        return;
      }
      if (this.stock[this.stock.length - 1].qty == 0 || this.stock[this.stock.length - 1].qty == null) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Enter quantity' });
        return;
      }
      if (this.stock.length > 1) {
        for (var data = 0; data < this.stock.length - 1; data++) {
          if (this.stock[data].locatorid == this.stock[this.stock.length - 1].locatorid) {
            if (this.stock[data].rackid == this.stock[this.stock.length - 1].rackid) {
              if (this.stock[data].binid == this.stock[this.stock.length - 1].binid) {
                this.messageService.add({ severity: 'error', summary: '', detail: 'Location already exists' });
                this.stock[this.stock.length - 1].binid = 0;
                this.stock[this.stock.length - 1].rackid = 0;
                this.stock[this.stock.length - 1].locatorid = 0;
                return;
              }
            }
          }
        }
      }
    }
    this.StockModelList = [];
    var binnumber: any[] = [];
    var storelocation: any[] = [];
    var rack: any[] = [];
    if (this.stock[0].rackid && this.stock[0].locatorid) {
      this.stock.forEach(item => {
        this.StockModel = new StockModel();
        this.StockModel.material = this.PoDetails.materialid;
        this.StockModel.itemid = this.PoDetails.itemid;
        this.StockModel.pono = this.PoDetails.pono;
        this.StockModel.grnnumber = this.PoDetails.grnnumber;
        this.StockModel.vendorid = this.PoDetails.vendorid;
        this.StockModel.paitemid = this.PoDetails.paitemid;
        this.StockModel.totalquantity = this.PoDetails.materialqty;
        this.StockModel.createdby = this.employee.employeeno;
        this.StockModel.inwardid = this.PoDetails.inwardid;
        this.StockModel.returnid = this.PoDetails.returnid;
        this.StockModel.returnqty = this.PoDetails.returnQty;
        this.StockModel.stocktype = item.stocktype;
        this.StockModel.storeid = item.locatorid;
        this.StockModel.locatorid = item.locatorid;
        this.StockModel.id = this.PoDetails.id;
        this.StockModel.binid = isNullOrUndefined(item.binid) ? 0 : item.binid;
        this.StockModel.rackid = item.rackid;
        this.StockModel.poitemdescription = this.PoDetails.poitemdescription;
        this.StockModel.projectcode = this.PoDetails.projectid;
        this.StockModel.materialcost = this.PoDetails.value;
        debugger;
        binnumber = this.bindata.filter(x => x.binid == item.binid);
        storelocation = this.locationdata.filter(x => x.locatorid == item.locatorid);
        rack = this.rackdata.filter(x => x.rackid == item.rackid);
        if (binnumber.length > 0) {
          this.StockModel.binnumber = binnumber[0].binnumber;
          this.StockModel.itemlocation = storelocation[0].locatorname + "." + rack[0].racknumber + '.' + binnumber[0].binnumber;
        }
        else if (binnumber.length == 0) {
          var storename = storelocation.length == 0 ? "" : storelocation[0].locatorname;
          var rackname = rack.length == 0 ? "" : rack[0].racknumber;
          this.StockModel.itemlocation = storename + "." + rackname;
        }
        this.StockModel.racknumber = rack.length == 0 ? "" : rack[0].racknumber;
        this.StockModel.confirmqty = item.qty;
        this.StockModel.itemreceivedfrom = new Date();
        this.StockModelList.push(this.StockModel);

        //this.StockModel.stocklist.push(this.StockModel);
      })

  
      var totalqty = 0;
      this.StockModelList.forEach(item => {

        totalqty = totalqty + (item.confirmqty);
      })
      var dtt = this.StockModelList.filter(function (element, index) {
        return (element.stocktype == "");
      });
      if (dtt.length > 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Select Stock Type' });
        return;
      }
      var dttloc = this.StockModelList.filter(function (element, index) {
        return (isNullOrUndefined(element.locatorid) || element.locatorid == "");
      });
      if (dttloc.length > 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Select Store' });
        return;
      }
      var dttrack = this.StockModelList.filter(function (element, index) {
        return (isNullOrUndefined(element.rackid) || element.rackid == "");
      });
      if (dttrack.length > 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Select Rack' });
        return;
      }
      if (totalqty == parseInt(this.matqty)) {
        this.ConfirmationService.confirm({
          message: 'Are you sure to put away material in selected stock type?',
          accept: () => {
            this.disSaveBtn = true;
            this.spinner.show();
            this.wmsService.UpdateReturnmaterialTostock(this.StockModelList).subscribe(data => {
              debugger;
              this.spinner.hide();
              //this.podetailsList[this.rowIndex].itemlocation = this.StockModel.itemlocation;
              //this.issaveprocess = true;
              this.showLocationDialog = false;
              this.messageService.add({ severity: 'success', summary: '', detail: 'Put away successful' });
              this.stock = [];
              this.showmaterialdetails(this.matreturnid);
              
              //this.getcheckedgrn();
              //this.SearchGRNNo();
              // }
            });
          },
          reject: () => {

            this.messageService.add({ severity: 'info', summary: '', detail: 'Cancelled' });
          }
        });


      }
      else {
        this.disSaveBtn = true;
        this.messageService.add({ severity: 'error', summary: '', detail: 'Putaway Qty should be same as Return Qty' });
      }
    }
    else {
      if (!this.store.name)
        this.messageService.add({ severity: 'error', summary: '', detail: 'Select Location' });
      else if (!this.rack.name)
        this.messageService.add({ severity: 'error', summary: '', detail: 'Select Rack' });
    }
  }

  onChange(value, indexid: any) {
    if (value == 'other') {
      document.getElementById('nodefaultloaction').style.display = "block";
    }
    this.materiallistData[indexid].itemlocation = value;
    // (<HTMLInputElement>document.getElementById(indexid)).value = event.toString();
  }
  onSelectStatus(event) {
    this.materialIssueList = [];
    this.selectedStatus = event.target.value;
    if (this.selectedStatus == "Pending") {
      this.materialIssueList = this.materialacceptListnofilter.filter(li => li.putawaystatus == 'Pending');
    }
    else if (this.selectedStatus == "Accepted") {
      this.materialIssueList = [];
      this.materialIssueList = this.materialacceptListnofilter.filter(li => li.putawaystatus == 'Accepted');
    }
  }
  SubmitStatus() {
    if (this.selectedStatus == "Pending") {
      this.materialIssueList = this.materialacceptListnofilter.filter(li => li.putawaystatus == 'Pending');
    }
    else if (this.selectedStatus == "Accepted") {
      this.materialIssueList = this.materialacceptListnofilter.filter(li => li.putawaystatus == 'Accepted');
    }
  }
}
