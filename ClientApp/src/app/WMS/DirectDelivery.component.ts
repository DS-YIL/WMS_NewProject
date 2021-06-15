import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl, ValidatorFn } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { DirectDelivery } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-DirectDelivery',
  templateUrl: './DirectDelivery.component.html'
})
export class DirectDeliveryComponent implements OnInit {

  constructor(private formBuilder: FormBuilder, private datePipe: DatePipe, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }
  public CreateDD: FormGroup;
  public deliveryList: Array<any> = [];
  public employee: Employee;
  public totalMaterialList: Array<DirectDelivery> = [];
  public DirectDelivery: DirectDelivery;
  public displayDDDialog; isSubmit; showEdit: boolean = false;
  public dynamicData: DynamicSearchResult;

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.DirectDelivery = new DirectDelivery();
    this.getdeliveryList();

    this.CreateDD = this.formBuilder.group({
      pono: ['', [Validators.required]],
      invoiceno: ['', [Validators.required]],
      invoicedate: ['', [Validators.required]],
      directdeliveryaddrs: [''],
      directdeliveredon: [''],
      directdeliveryremarks: [''],
      vehicleno: [''],
      transporterdetails: ['']

    });

    this.CreateDD.controls['pono'].clearValidators();
    this.CreateDD.controls['pono'].updateValueAndValidity();
  }



  //get delivery List 
  getdeliveryList() {
    this.spinner.show();
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.query = "select * from wms.wms_securityinward  where isdirectdelivered is true and (deleteflag is false or deleteflag is null) order by inwmasterid desc";
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.spinner.hide();
      this.deliveryList = data;
    });
  }

  SearchPoNo() {
    if (this.DirectDelivery.pono) {
      this.spinner.show();
      this.wmsService.getDDdetailsByPono(this.DirectDelivery.pono).subscribe(data => {
        this.spinner.hide();
        if (data) {
          this.totalMaterialList = data;
          this.prepareDDdata();
        }
        else {
          this.DirectDelivery = new DirectDelivery();
          this.messageService.add({ severity: 'error', summary: '', detail: 'No data for this Po No' });

        }
      })

    }
    else {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter PO No' });
    }
  }

  prepareDDdata() {
    this.DirectDelivery = new DirectDelivery();
    this.DirectDelivery.pono = this.totalMaterialList[0].pono;
    this.DirectDelivery.suppliername = this.totalMaterialList[0].suppliername;
    if (this.totalMaterialList[0].invoiceno)
      this.DirectDelivery.invoiceno = this.totalMaterialList[0].invoiceno;
    if (this.checkValiddate(this.totalMaterialList[0].invoicedate))
      this.DirectDelivery.invoicedate = this.totalMaterialList[0].invoicedate;
    //if (this.totalMaterialList[0].directdeliveryaddrs)
    //  this.DirectDelivery.directdeliveryaddrs = this.totalMaterialList[0].directdeliveryaddrs;
    if (this.checkValiddate(this.totalMaterialList[0].directdeliveredon))
      this.DirectDelivery.directdeliveredon = this.totalMaterialList[0].directdeliveredon;
    if (this.totalMaterialList[0].directdeliveryremarks)
      this.DirectDelivery.directdeliveryremarks = this.totalMaterialList[0].directdeliveryremarks;
    this.DirectDelivery.DDmaterialList = this.totalMaterialList;
  }
  //show dialog
  showDDdialog() {
    this.displayDDDialog = true;
    this.showEdit = false;
    this.DirectDelivery = new DirectDelivery();
  }

  dialogCancel(dialog: any) {
    this[dialog] = false;
  }

  editdirectdelivery(rowdata: any) {
    this.displayDDDialog = true;
    this.showEdit = true;
    this.DirectDelivery = new DirectDelivery();
    this.DirectDelivery.pono = rowdata.pono;
    this.DirectDelivery.invoiceno = rowdata.invoiceno;
    this.DirectDelivery.invoicedate = rowdata.invoicedate;
    this.DirectDelivery.suppliername = rowdata.suppliername;
    this.DirectDelivery.directdeliveryaddrs = rowdata.directdeliveryaddrs;
    this.DirectDelivery.directdeliveredon = rowdata.directdeliveredon;
    this.DirectDelivery.directdeliveryremarks = rowdata.directdeliveryremarks;
    this.DirectDelivery.inwmasterid = rowdata.inwmasterid;
    this.DirectDelivery.vehicleno = rowdata.vehicleno;
    this.DirectDelivery.transporterdetails = rowdata.transporterdetails;
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.query = "select inwardid, materialid,poitemdescription,materialqty,receivedqty as pendingqty from wms.wms_storeinward  where inwmasterid ='" + this.DirectDelivery.inwmasterid +"' and deleteflag is false";
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.spinner.hide();
      this.DirectDelivery.DDmaterialList = data;
    });
  }
  //check validations for delivered quantity
  QtyChange(data: any) {
    var comQty = data.materialqty - data.deliveredqty;
    if (data.pendingqty > comQty) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Delivered Quantity should be lessthan or equal to Difference of  PO & Delivered quantity' });
      data.pendingqty = 0;
      return;
    }
  }

  //update Direct Delivery 
  onSubmit() {
    this.isSubmit = true;
    if (this.CreateDD.invalid && !this.showEdit) {
      return;
    }
    this.DirectDelivery.receivedby = this.employee.employeeno;
    this.DirectDelivery.DDmaterialList = this.DirectDelivery.DDmaterialList.filter(li => li.pendingqty > 0);
    this.spinner.show();
    this.wmsService.updateDirectDelivery(this.DirectDelivery).subscribe(data => {
      this.spinner.hide();
      this.getdeliveryList();
      this.isSubmit = false;
      this.displayDDDialog = false;
      if (data) {
        this.messageService.add({ severity: 'success', summary: '', detail: 'Stock Updated' });
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Update Failed' });
      }

    });
  }

  //check date is valid or not
  checkValiddate(date: any) {
    try {
      if (!date || (this.datePipe.transform(date, this.constants.dateFormat) == "01/01/0001"))
        return "";
      else
        return date;
    }
    catch{
      return "";
    }
  }

  removedirectdelivery(rowdata: any) {
    this.spinner.show();
    this.wmsService.deleteDirectDelivery(rowdata.inwmasterid, this.employee.employeeno).subscribe(data => {
      this.spinner.hide();
      if (data) {
        this.messageService.add({ severity: 'success', summary: '', detail: 'Deleted' });
        this.getdeliveryList();
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Unable to delete' });
      }
      

    });
  }

}
