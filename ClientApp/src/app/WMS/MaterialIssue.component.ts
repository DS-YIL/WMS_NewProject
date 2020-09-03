import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl, ValidatorFn } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { materialRequestDetails, FIFOValues } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { ConfirmationService } from 'primeng/api';
import { DatePipe } from '@angular/common';
@Component({
  selector: 'app-MaterialIsuue',
  templateUrl: './MaterialIssue.component.html'
})
export class MaterialIssueComponent implements OnInit {
  roindex: any;
  itemreceiveddate: string;

  constructor(private datePipe: DatePipe, private ConfirmationService: ConfirmationService, private formBuilder: FormBuilder, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public formName: string;
  public txtName: string;
  public dynamicData = new DynamicSearchResult();
  public showList: boolean = false;
  public searchItems: Array<searchList> = [];
  public selectedlist: Array<searchList> = [];
  public selectedItem: searchList;
  public searchresult: Array<object> = [];
  public AddDialog: boolean;
  public id: string;
  public MaterialRequestForm: FormGroup
  public materialissueList: Array<any> = [];
  public employee: Employee;
  public displayItemRequestDialog; RequestDetailsSubmitted: boolean = false;
  public materialRequestDetails: materialRequestDetails;
  public requestId: string;
  public pono: string="";
  public Oldestdata: FIFOValues;
  public itemlocationData: Array<any> = [];
  public showavailableqtyList: boolean = false;
  public showissueqtyOKorCancel: boolean = true;
  public showdialog: boolean = false;
  public txtDisable: boolean = true;
  public FIFOvalues: FIFOValues;
  public reqqty; reservedQty: number;
  public btndisable: boolean = false;
  public issueqtyenable: boolean = true;
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.route.params.subscribe(params => {
      if (params["requestid"]) {
        this.requestId = params["requestid"];
      }
      if (params["pono"]) {
        this.pono = params["pono"];
      }
    });

    this.FIFOvalues = new FIFOValues();
    this.spinner.show();
    this.getmaterialIssueListbyrequestid();

  }
  issuematerial(itemlocationData) {
    var totalissuedqty = 0;
    this.itemlocationData.forEach(item => {
      if (item.issuedqty)
        item.requestforissueid = this.materialissueList[this.roindex].requestforissueid;
      item.itemreturnable = this.materialissueList[this.roindex].itemreturnable;
      item.approvedby = this.employee.employeeno;
      item.itemreceiverid = this.materialissueList[this.roindex].itemreceiverid;
      totalissuedqty = totalissuedqty + (item.issuedqty);
      this.FIFOvalues.issueqty = totalissuedqty;
      //item.issuedqty = this.FIFOvalues.issueqty;
      //item.issuedquantity = totalissuedqty;
      //item.issuedqty = totalissuedqty;

    });

    if (totalissuedqty > this.reqqty) {
      this.messageService.add({ severity: 'error', summary: '', detail: ' Issue Qty cannot exceed Requested Qty' });
      this.AddDialog = true;
    }
    else {
      (<HTMLInputElement>document.getElementById(this.id)).value = totalissuedqty.toString();
      this.materialissueList[this.roindex].issuedqty = totalissuedqty;
      this.txtDisable = true;
      this.AddDialog = false;
    }
    this.btndisable = true;

  }
  Cancel() {
    this.AddDialog = false;
  }

  //shows list of items for particular material
  showmateriallocationList(material, id, rowindex, qty, issuedqty, reservedqty) {
    if (issuedqty <= qty) {
      this.issueqtyenable = true;
    }
    else {
      this.issueqtyenable = false;
    }
    this.reqqty = qty;
    this.reservedQty = reservedqty;
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
    this.itemreceiveddate = this.datePipe.transform(data.createddate, 'dd-MM-yyyy');
    this.ConfirmationService.confirm({
      message: 'Same Material received on ' + this.itemreceiveddate + ' and placed in ' + data.itemlocation + '  location, Would you like to continue?',
      header: 'Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {

        this.messageService.add({ severity: 'info', summary: '', detail: 'You have accepted' });
      },
      reject: () => {

        this.messageService.add({ severity: 'info', summary: '', detail: 'You have ignored' });
      }
    });
  }
  //check issued quantity
  checkissueqty($event, entredvalue, maxvalue, material, createddate) {
    var id = $event.target.id;
    if (entredvalue > maxvalue) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Please enter issue quantity less than Available quantity' });

      (<HTMLInputElement>document.getElementById(id)).value = "";
    }
    else {

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
  getmaterialIssueListbyrequestid() {
    this.wmsService.getmaterialIssueListbyrequestid(this.requestId, this.pono).subscribe(data => {
      this.spinner.hide();
      this.materialissueList = data;

      if (this.materialissueList.length != 0)
        this.showavailableqtyList = true;
      this.materialissueList.forEach(item => {
        //if (!item.issuedquantity)
        //  item.issuedquantity = item.requestedquantity;
        if (item.issuedqty >= item.requestedquantity) {
          this.showissueqtyOKorCancel = true;
          this.btndisable = false;
        }

        //(<HTMLInputElement>document.getElementById('footerdiv')).style.display = "none";
      });
    });
  }

  //check validations for issuer quantity
  reqQtyChange(data: any) {
    if (data.issuedqty > data.quantity) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'issued Quantity should be lessthan or equal to available quantity' });
      data.issuedqty = data.quantity;
    }
  }
  backtoDashboard() {
    this.router.navigateByUrl("/WMS/MaterialIssueDashboard");
  }


  //requested quantity update
  onMaterialIssueDeatilsSubmit() {
    this.spinner.show();

    //this.wmsService.UpdateMaterialqty(this.itemlocationData).subscribe(data => {
    //  if (data == 1) {
    //this.btndisable = false;
    //this.itemlocationData.forEach(item => {

    // // item.issuedquantity = this.itemlocationData.issuedquantity;

    //});
    //this.materialissueList.forEach(item => {
    //  if (item.issuedqty != 0)

    //   // totalissuedqty = totalissuedqty + (item.issuedquantity);
    //  // this.FIFOvalues.issueqty = totalissuedqty;
    //  item.issuedqty = this.FIFOvalues.issueqty;

    //});

    this.wmsService.approvematerialrequest(this.itemlocationData).subscribe(data => {
      this.spinner.hide();
      this.btndisable = false;
      if (data)
        this.messageService.add({ severity: 'success', summary: '', detail: 'Material issued.' });
      else
        this.messageService.add({ severity: 'error', summary: '', detail: 'Material issue failed.' });

    });
  }
  //})
  //}
}
