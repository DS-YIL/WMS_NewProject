import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, locationBarcode } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService, SelectItem } from 'primeng/api';

@Component({
  selector: 'app-ABCAnalysis',
  templateUrl: './MaterialBarcode.component.html'
})
export class MaterialBarcodeComponent implements OnInit {
  constructor(private wmsService: wmsService, private route: ActivatedRoute, private messageService: MessageService, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public employee: Employee;
  public material: any;
  public dynamicData: DynamicSearchResult;
  public path: string = null;
  employes: SelectItem[];
  selectedEmploye: SelectItem;
  selectedEmp: SelectItem;
  isracklabel: boolean = true;
  isbinlabel: boolean = false;
  public racklist: any[] = [];
  public rackdata: any[] = [];
  public locationlist: any[] = [];
  public locationdata: any[] = [];
  public binlist: any[] = [];
  public bindata: any[] = [];
  public locatorid: any;
  public locatornamr: string;
  public rackname: any;
  public rackid: any;
  public showLabel: boolean = false;
  public binid: any;
  public labelpath: string;
  public locbarcode = new locationBarcode();

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");


    this.employes = [
      { label: "Select Location", value: null },
      { label: "Store1", value: 1 },
      { label: "store2", value: 2 },
      { label: "store3", value: 3 }
    ];

    this.locationListdata();
    this.rackListdata();
    this.binListdata();
  }


  racklabel() {
    this.isracklabel = true;
    this.isbinlabel = false;
    this.locationListdata();

  }

  binlabel() {
    this.isracklabel = false;
    this.isbinlabel = true;
    this.locationListdata();

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

  locationListdata() {
    this.wmsService.getlocationdata().
      subscribe(
        res => {
          //this._list = res; //save posts in array
          this.locationlist = res;
          let _list: any[] = [];
          for (let i = 0; i < (res.length); i++) {
            _list.push({
              locatorid: res[i].locatorid,
              locatorname: res[i].locatorname,
            });
          }
          this.locationlist = _list;
          this.locationdata = _list;
          console.log(this.locationlist);
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

  //On selection of rack updating bin
  onrackUpdate(locationid: any, rackid: any, issetdefault: boolean) {
    debugger;
    //rowData.binlist = [];
    if (this.bindata.filter(li => li.locationid == locationid && li.rackid == rackid).length > 0) {
      this.binlist = [];
      console.log(this.binlist);
      this.bindata.forEach(item => {
        if (item.locationid == locationid && item.rackid == rackid) {
          //rowData.binlist.push(item);
          this.binlist.push(item);
          console.log(this.binlist);
        }
      })

    }

  }

  //On selection of location updating rack
  onlocUpdate(locationid: any,locatorname:any,issetdefault: boolean) {
    debugger;
    this.locatornamr = locatorname;
   // alert(this.locatornamr);
   // rowData.racklist = [];
    if (this.rackdata.filter(li => li.locationid == locationid).length > 0) {
      this.racklist = [];
      console.log(this.racklist);
      this.rackdata.forEach(item => {
        if (item.locationid == locationid) {
          // rowData.racklist.push(item);
          this.racklist.push(item);
          console.log(this.racklist);
        }
      })

    }

  }

  PrintRacklabel() {
    //alert(this.locatorid);
    //alert(this.rackid);
    this.locbarcode.locatorid = this.locatorid;
    this.locbarcode.rackid = this.rackid;
    this.locbarcode.isracklabel = true;
    //this.wmsService.printlocLabel(material).subscribe(data => {
    //  if (data == "No material exists") {
    //    this.messageService.add({ severity: 'error', summary: '', detail: "Material doesn't exist" });
    //  }
    //  else {
    //    this.path = data;
    //  }
    //});
  }
  

  GenerateRacklabel(locatorid: any, rackid: any) {
    debugger;
    var labeldata = locatorid + rackid;
    this.wmsService.generateLabel(labeldata).subscribe(data => {
      if (data == "") {
        this.messageService.add({ severity: 'error', summary: '', detail: "Error while generating label" });
      }
      else {
        this.showLabel = true;
        this.labelpath = data;
      }
    });
  }

  PrintBinlabel() {
    this.locbarcode.locatorid = this.locatorid;
    this.locbarcode.rackid = this.rackid;
    this.locbarcode.isracklabel = true;
    this.locbarcode.binid = this.binid;
    this.wmsService.printBinqr(this.locbarcode).subscribe(data => {
      if (data == "success") {
        this.messageService.add({ severity: 'success', summary: '', detail: "QRCode printed successfully" });
      }
      else {
        this.path = data;
      }
    });
  }

  GenerateBarcode(material: string) {

    this.wmsService.checkMatExists(material).subscribe(data => {
      if (data == "No material exists") {
        this.messageService.add({ severity: 'error', summary: '', detail: "Material doesn't exist" });
      }
      else {
        this.path = data;
      }
    });
  }
}
