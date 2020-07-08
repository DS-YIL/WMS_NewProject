import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { gatepassModel, materialistModel, FIFOValues } from '../Models/WMS.Model';
import { DatePipe } from '@angular/common';
import { ConfirmationService } from 'primeng/api';
@Component({
  selector: 'app-GatePassApprover',
  templateUrl: './GatePassApproverForm.component.html',
  providers: [DatePipe]
})
export class GatePassApproverComponent implements OnInit {
  txtDisable: boolean = true;
   // btnDisableformaterial: boolean=false;
  constructor(private ConfirmationService: ConfirmationService,private formBuilder: FormBuilder, private messageService: MessageService, private wmsService: wmsService, private datePipe: DatePipe, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }
  public  AddDialog: boolean;
  public id: any;
  roindex: any;
  public showdialog: boolean;
  public employee: Employee;
  public materialList: Array<any> = [];
  public gatepassModel: gatepassModel;
  public btnDisable: boolean = false;
  public itemlocationData: Array<any> = [];
  public Oldestdata: FIFOValues;
  public FIFOvalues: FIFOValues;
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.route.params.subscribe(params => {
      if (params["gatepassid"]) {
        var gatepassId = params["gatepassid"]
        this.bindMaterilaDetails(gatepassId);
      }
    });
    this.gatepassModel = new gatepassModel();
    this.gatepassModel.approverstatus = "Approved";

  }


  //get gatepass list
  bindMaterilaDetails(gatepassId: any) {
    this.wmsService.gatepassmaterialdetail(gatepassId).subscribe(data => {
      this.materialList = data;
      console.log(data);
      this.gatepassModel = this.materialList[0];
      console.log(this.gatepassModel);
      if (this.gatepassModel.approverstatus == 'Approved')
        this.btnDisable = true;
    });
  }
  backtogatepass() {
    this.router.navigateByUrl("WMS/GatePass");
  }

  updategatepassapproverstatus() {
    this.gatepassModel.gatepassid = this.materialList[0].gatepassid;
    //this.materialList.forEach(item => {
    //  item.pono = this.gatepassModel.pono;
    //})
    this.wmsService.updategatepassapproverstatus(this.materialList).subscribe(data => {
      //this.materialList = data;
      this.gatepassModel.status = "Approved";
      if (this.gatepassModel.status == 'Approved')
        this.btnDisable = true;
  
      this.messageService.add({ severity: 'success', summary: 'success Message', detail: 'Gate Pass Approved' });
    });
  }

  //check date is valid or not
  checkValiddate(date: any) {
    try {
      if (!date || (this.datePipe.transform(date, this.constants.dateFormat) == "01/01/0001") )
        return "";
      else
        return this.datePipe.transform(date, this.constants.dateFormat);
    }
    catch{
      return "";
    }
  }

  //shows list of items for particular material
  showmateriallocationList(material, id, rowindex) {
    this.id = id;
    this.AddDialog = true;
    this.roindex = rowindex;
    this.wmsService.getItemlocationListByMaterial(material).subscribe(data => {
      this.itemlocationData = data;
      this.showdialog = true;
      if (data != null) {

      }
    });
  }
  //show alert about oldest item location
  alertconfirm(data) {
    var info = data;
    this.ConfirmationService.confirm({
      message: 'Same Material received on ' + data.createddate + ' and placed in ' + data.itemlocation + '  location, Would you like to continue?',
      header: 'Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {

        this.messageService.add({ severity: 'info', summary: 'Accepted', detail: 'You have accepted' });
      },
      reject: () => {

        this.messageService.add({ severity: 'info', summary: 'Ignored', detail: 'You have ignored' });
      }
    });
  }
  //check issued quantity
  checkissueqty($event, entredvalue, maxvalue, material, createddate,index) {
    var id = $event.target.id;
    if (entredvalue > maxvalue) {
      this.itemlocationData[index].issuedquantity = 0;
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Please enter issue quantity less than Available quantity' });
      // this.btnDisableformaterial = true;
      (<HTMLInputElement>document.getElementById(id)).value ="0";
      this.materialList[this.roindex].issuedqty = 0;

    }
    else {
     // this.btnDisableformaterial = false;
      this.wmsService.checkoldestmaterial(material, createddate).subscribe(data => {
        this.Oldestdata = data;
        if (data != null) {
          this.alertconfirm(this.Oldestdata);
        }
        //this.calculateTotalQty();
        //this.calculateTotalPrice();
        this.spinner.hide();
      });
    }
  }
  Cancel() {
    this.AddDialog = false;
  }
  issuematerial(itemlocationData) {
    var totalissuedqty = 0;
    this.itemlocationData.forEach(item => {
      if (item.issuedquantity != "0")

        totalissuedqty = totalissuedqty + (item.issuedquantity);
      //this.FIFOvalues.issueqty = totalissuedqty;
      //item.issuedqty = totalissuedqty;
    
      //item.issuedquantity = totalissuedqty;
      //item.issuedqty = totalissuedqty;

    });


   // (<HTMLInputElement>document.getElementById(this.id)).value = totalissuedqty.toString();
    this.materialList[this.roindex].issuedqty = totalissuedqty;
    this.txtDisable = true;

    this.AddDialog = false;

  }
}
