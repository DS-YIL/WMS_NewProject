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
@Component({
  selector: 'app-MaterialIsuue',
  templateUrl: './MaterialRelease.component.html'
})
export class MaterialReleaseComponent implements OnInit {
  roindex: any;
  executetrueorfalse: boolean = false;

  constructor(private ConfirmationService: ConfirmationService, private formBuilder: FormBuilder, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

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
  public reserveId: string;
  public Oldestdata: FIFOValues;
  public itemlocationData: Array<any> = [];
  public showavailableqtyList: boolean = false;
  public showissueqtyOKorCancel: boolean = true;
  public showdialog: boolean = false;
  public txtDisable: boolean = true;
  public FIFOvalues: FIFOValues;
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.route.params.subscribe(params => {
      if (params["reserveid"]) {
        this.reserveId = params["reserveid"];
      }
    });

    this.FIFOvalues = new FIFOValues();
    this.getmaterialIssueListbyrequestid();

  }
  issuematerial(itemlocationData) {
    var totalissuedqty = 0;
    this.itemlocationData.forEach(item => {
      if (item.issuedquantity != 0)

        totalissuedqty = totalissuedqty + (item.issuedquantity);
      this.FIFOvalues.issueqty = totalissuedqty;
      item.issuedqty = this.FIFOvalues.issueqty;
      //item.issuedquantity = totalissuedqty;
      //item.issuedqty = totalissuedqty;

    });


    (<HTMLInputElement>document.getElementById(this.id)).value = totalissuedqty.toString();
    this.materialissueList[this.roindex].issuedqty = totalissuedqty;
    this.txtDisable = true;
    this.AddDialog = false;

  }
  Cancel() {
    this.AddDialog = false;
  }

  //shows list of items for particular material
  showmateriallocationList(material, id, rowindex) {
    this.id = id;

    this.roindex = rowindex;
    this.wmsService.getItemlocationListByMaterial(material).subscribe(data => {
      this.itemlocationData = data;
      if (data.length != 0) {
        this.AddDialog = true;
        this.showdialog = true;
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
  checkissueqty($event, entredvalue, maxvalue, material, createddate) {
    var id = $event.target.id;
    if (entredvalue > maxvalue) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Please enter issue quantity less than Available quantity' });

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
    this.wmsService.getmaterialIssueListbyreserveid(this.reserveId).subscribe(data => {
      this.materialissueList = data;
      if (this.materialissueList.length != 0)
        this.showavailableqtyList = true;
      this.materialissueList.forEach(item => {
        //if (!item.issuedquantity)
        //  item.issuedquantity = item.requestedquantity;
        if (item.issuedqty >= item.requestedquantity)
          this.showissueqtyOKorCancel = false;
        //(<HTMLInputElement>document.getElementById('footerdiv')).style.display = "none";
      });
    });
  }

  //check validations for issuer quantity
  reqQtyChange(data: any) {
    if (data.issuedquantity > data.quantity) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'issued Quantity should be lessthan or equal to available quantity' });
      data.issuedquantity = data.quantity;
    }
  }

  backtoDashboard() {
    this.router.navigateByUrl("WMS/MaterialReleaseDashboard");
  }


  //requested quantity update
  onMaterialIssueDeatilsSubmit() {
    this.spinner.show();

    this.materialissueList.forEach(item => {
      if (item.issuedqty > item.reservedqty) {
        this.executetrueorfalse = false;
        this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Issued qty should be less than reserved qty' });
      }
      else {
        this.executetrueorfalse = true;
      }
      // item.issuedquantity = this.itemlocationData.issuedquantity;

    });

    if (this.executetrueorfalse == true) {
      this.wmsService.UpdateMaterialqty(this.itemlocationData).subscribe(data => {
        if (data == 1) {
          this.wmsService.approvematerialrelease(this.materialissueList).subscribe(data => {
            this.spinner.hide();
            if (data)
              this.messageService.add({ severity: 'success', summary: 'sucee Message', detail: 'Status updated' });
            else
              this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Update Failed' });

          });

        }
      })
    }

  }
}
