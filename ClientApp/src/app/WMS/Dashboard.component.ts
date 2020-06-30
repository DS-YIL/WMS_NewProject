import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { wmsService } from 'src/app/WmsServices/wms.service';
import { constants } from 'src/app/Models/WMSConstants'
import { NgxSpinnerService } from "ngx-spinner";
import { Employee, DynamicSearchResult, searchList } from 'src/app/Models/Common.Model';
import { PoFilterParams, PoDetails } from 'src/app/Models/WMS.Model';

@Component({
  selector: 'app-Dashboard',
  templateUrl: './Dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  constructor(private formBuilder: FormBuilder, private spinner: NgxSpinnerService, public wmsService: wmsService, public constants: constants, private route: ActivatedRoute, private router: Router) { }
  public employee: Employee;
  public formName: string;
  public txtName: string;
  public dynamicData = new DynamicSearchResult();
  public showList: boolean = false;
  public searchItems: Array<searchList> = [];
  public selectedlist: Array<searchList> = [];
  public selectedItem: searchList;
  public searchresult: Array<object> = [];
  public POfilterForm: FormGroup;
  public PoFilterParams: PoFilterParams;
  loading: boolean;
  public showFilterBlock; showsecDialog: boolean = false
  public POList: Array<any> = [];
  public PoDetails: PoDetails;

  //page load event
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.PoFilterParams = new PoFilterParams();
    this.PoDetails = new PoDetails();

    this.POfilterForm = this.formBuilder.group({
      PONo: ['', [Validators.required]],
      venderid: ['', [Validators.required]],
      DocumentNo: ['', [Validators.required]]
    });
    //this.SecForm = this.formBuilder.group({
    //  Barcode: ['', [Validators.required]],
    //})
    this.bindList();

  }
  //show and hide filter parmas
  showHideFilterBlock() {
    this.showFilterBlock = !this.showFilterBlock;
  }



  //bind rfq list
  bindList() {
    this.spinner.show();
    //this.PoFilterParams.venderid = 8;
    this.PoFilterParams.loginid =this.employee.employeeno;
    this.wmsService.getPOList(this.PoFilterParams).subscribe(data => {
      this.POList = data;
      this.loading = false;
      this.spinner.hide();
    })
  }


  public bindSearchListData(e: any, formName?: string, name?: string, searchTxt?: string, callback?: () => any): void {
    this.formName = formName;
    this.txtName = name;
    if (searchTxt == undefined)
      searchTxt = "";
    searchTxt = searchTxt.replace('*', '%');
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.tableName = this.constants[name].tableName;
    this.dynamicData.searchCondition = "" + this.constants[name].condition + this.constants[name].fieldName + " like '" + searchTxt + "%'";
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      if (data.length == 0)
        this.showList = false;
      else
        this.showList = true;
      this.searchresult = data;
      this.searchItems = [];
      var fName = "";
      this.searchresult.forEach(item => {
        fName = item[this.constants[name].fieldName];
        var value = { listName: name, name: fName, code: item[this.constants[name].fieldId] };
        this.searchItems.push(value);
      });
      if (callback)
        callback();
    });
  }
  //search list option changes event
  public onSelectedOptionsChange(item: any, index: number) {
    this.showList = false;

    if (this.formName != "") {
      this[this.formName].controls[this.txtName].setValue(item.name);
      this.PoFilterParams[this.txtName] = item.code;
    }
    this[this.formName].controls[this.txtName].updateValueAndValidity()
  }

  //clear model when search text is empty
  onsrchTxtChange(modelparm: string, value: string, model: string) {
    if (value == "") {
      this[model][modelparm] = "";
    }
  }

  dialogCancel(dialogName) {
    this[dialogName] = false;
  }
  materialrequest(pono) {
    this.router.navigate(['/WMS/MaterialRequest/', pono]);

  }
  //materialrequestedview(pono) {
  //  this.router.navigate(['/WMS/MaterialReqView/', pono]);
  //}
   materialReserve(pono) {
     this.router.navigate(['/WMS/MaterialReserve/', pono]);
  }

}


