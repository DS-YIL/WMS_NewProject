import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { wmsService } from '../../WmsServices/wms.service';
import { constants } from '../../Models/WMSConstants';
import { Employee, DynamicSearchResult } from '../../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { VendorMaster } from '../../Models/WMS.Model';

@Component({
  selector: 'app-VendorMaster',
  templateUrl: './VendorMaster.component.html'
})
export class VendorMasterComponent implements OnInit {

  constructor(private messageService: MessageService, private wmsService: wmsService, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public employee: Employee;
  public dynamicData: DynamicSearchResult;
  public displayDialog: boolean = false;
  public vendorList: Array<VendorMaster> = []
  public vendorDetails: VendorMaster;
  public vendorname: string;
  public vendorcode: string;
  public street: string;
  public contactno: string;
  public faxno: string;
  public emailid: string;
  public deleteflag: boolean;

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.vendorDetails = new VendorMaster();
    this.vendorList = [];
    this.getVendorlist();
  }

  openDialog() {
    this.vendorDetails = new VendorMaster();
    this.vendorname = "";
    this.vendorcode = "";
    this.street = "";
    this.contactno = "";
    this.faxno = "";
    this.emailid = "";
    this.deleteflag = true;
    this.displayDialog = true;
  }

  dialogCancel() {
    this.displayDialog = false;
  }

  //Get the list of Vendors
  getVendorlist() {
    this.spinner.show();
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.query = "select * from wms.vendormaster order by vendorid desc";
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.vendorList = data;
      this.spinner.hide();
    })
  }

  onvendorSubmit() {
    if (!this.vendorname) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Enter Vendor Name' });
      return;
    }
    if (!this.vendorcode) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Enter Vendor Code' });
      return;
    }
    this.vendorDetails.vendorname = this.vendorname;
    this.vendorDetails.vendorcode = this.vendorcode;
    this.vendorDetails.street = this.street;
    this.vendorDetails.contactno = this.contactno;
    this.vendorDetails.faxno = this.faxno;
    this.vendorDetails.emailid = this.emailid;
    this.vendorDetails.deleteflag = !this.deleteflag;
    this.vendorDetails.updatedby = this.employee.employeeno;
    this.spinner.show();
    this.wmsService.updateVendorMaster(this.vendorDetails).subscribe(data => {
      this.displayDialog = false;
      this.spinner.hide();
      if (data) {
        this.messageService.add({ severity: 'success', summary: '', detail: 'Vendor updated successfully' });
        this.getVendorlist();
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Error while updating vendor' });
      }
    })
  }

  editvendor(vendor: any) {
    this.vendorDetails = new VendorMaster();
    this.displayDialog = true;
    this.vendorDetails.vendorid = vendor.vendorid;
    this.vendorname = vendor.vendorname;
    this.vendorcode = vendor.vendorcode;
    this.street = vendor.street;
    this.contactno = vendor.contactno;
    this.faxno = vendor.faxno;
    this.emailid = vendor.emailid;
    this.deleteflag = !vendor.deleteflag;
  }


}
