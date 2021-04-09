import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl, ValidatorFn } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../../WmsServices/wms.service';
import { constants } from '../../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { MaterialMaster } from '../../Models/WMS.Model';

@Component({
  selector: 'app-MaterialMaster',
  templateUrl: './MaterialMaster.component.html'

})
export class MaterilMasterComponent implements OnInit {

  constructor(private formBuilder: FormBuilder, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public CreateMaterial: FormGroup;
  public employee: Employee;
  public MaterialMasterList: Array<any> = [];
  public displayAddDialog; isSubmit: boolean=false;
  public MaterialMaster: MaterialMaster;
  public locationlists: any[] = [];
  public binlist: any[] = [];
  public racklist: any[] = [];
  public locationdata: any[] = [];
  public bindata: any[] = [];
  public rackdata: any[] = [];

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.MaterialMaster = new MaterialMaster();
    this.locationlists = [];
    this.racklist = [];
    this.binlist = [];
    this.locationListdata();
    this.rackListdata();
    this.binListdata();
    this.getmaterialMasterList();

    this.CreateMaterial = this.formBuilder.group({
      material: ['', [Validators.required]],
      materialdescription: ['', [Validators.required]],
      unitprice: ['', [Validators.required]],
      hsncode: ['', [Validators.required]],
      storeid: ['', [Validators.required]],
      rackid: ['', [Validators.required]],
      binid: ['', [Validators.required]],
      qualitycheck: ['', [Validators.required]],
      stocktype: ['', [Validators.required]],    
    });
  }
  

//get store list
locationListdata() {
  this.wmsService.getlocationdata().subscribe(res => {
    this.locationlists = res;
  });
}

rackListdata() {
  this.wmsService.getrackdataforputaway().subscribe(res => {
    this.rackdata = res;
  });
}

binListdata() {
  this.wmsService.getbindataforputaway().subscribe(res => {
    this.bindata = res;
  });
}

//On selection of location updating rack
onlocUpdate() {
  this.racklist = [];
  this.racklist = this.rackdata.filter(li => li.locatorid == this.MaterialMaster.storeid);
}
//On selection of rack updating bin
onrackUpdate() {
  this.binlist = [];
  this.binlist = this.bindata.filter(li => li.locatorid == this.MaterialMaster.storeid && li.rackid == this.MaterialMaster.rackid)
}

  getmaterialMasterList() {
    this.spinner.show();
    this.wmsService.getMaterialMasterList().subscribe(data => {
      this.spinner.hide();
      this.MaterialMasterList = data;
    })
  }

  showAddDialog(details:any) {
    this.displayAddDialog = true;
    this.MaterialMaster = new MaterialMaster();
    if (details) {
      this.MaterialMaster = details;
      this.onlocUpdate();
      this.onrackUpdate(); 
    }
  }

  dialogCancel(dialog: any) {
    this[dialog] = false;
  }

  onSubmit() {
    this.isSubmit = true;
    if (this.CreateMaterial.invalid) {
      return;
    }
    
    this.spinner.show();
    this.wmsService.materialMasterUpdate(this.MaterialMaster).subscribe(data => {
      this.spinner.hide();
      this.getmaterialMasterList();
      this.isSubmit = false;
      this.displayAddDialog = false;
      if (data) {
        this.messageService.add({ severity: 'success', summary: '', detail: 'Stock Updated' });
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Update Failed' });
      }

    });
  }
  }
