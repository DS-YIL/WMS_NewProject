import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { gatepassModel, materialistModel } from '../Models/WMS.Model';
import { isNullOrUndefined } from 'util';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-GatePass',
  templateUrl: './Barcode.component.html',
  providers: [DatePipe]
})
export class BarcodeComponent implements OnInit {
  constructor(private formBuilder: FormBuilder, private messageService: MessageService, private datePipe: DatePipe, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public formName: string;
  public txtName; GatepassTxt: string;
  public dynamicData = new DynamicSearchResult();
  public showList: boolean = false;
  public searchItems: Array<searchList> = [];
  public selectedlist: Array<searchList> = [];
  public searchresult: Array<object> = [];

  public gatepasslist: Array<any> = [];
  public gatepassModelList: Array<gatepassModel> = [];
  public employee: Employee;
  public gatepassdialog; updateReturnedDateDialog: boolean = false;
  public gatepassModel: gatepassModel;
  public materialistModel: materialistModel;
  public material: any;
  public gpIndx: number;
  public date: Date=null;
  public qty: any = 1;
  public noOfPrint: any=1;

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.gatepassModel = new gatepassModel();
    this.materialistModel = new materialistModel();

  }

  //On click of button decrease qty
  decreaseQty() {
    if (this.qty > 1) {
      this.qty = this.qty - 1;
    }
  }

  //On click of increase qty increase the qty
  increaseQty() {
    if (this.qty < this.noOfPrint) {
      this.qty = this.qty + 1;
    }
  }
  
}
