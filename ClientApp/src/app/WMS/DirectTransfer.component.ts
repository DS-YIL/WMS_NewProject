import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl, ValidatorFn } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { materialtransferMain, materialtransferTR, materialRequestDetails, returnmaterial, gatepassModel, materialistModel, PoDetails, StockModel, materialistModelreturn, materialistModeltransfer, ddlmodel, DirectTransferMain } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { isNullOrUndefined } from 'util';

@Component({
  selector: 'app-DirectTransfer',
  templateUrl: './DirectTransfer.component.html'
})
export class DirectTransferComponent implements OnInit {
  
  materialtransferlist: DirectTransferMain[] = [];
  public employee: Employee;
  constructor(private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.directtransferlist();
   
  }

  showattachdata(data: DirectTransferMain) {
    data.showtr = !data.showtr;
  }
  //get direct transfer list
  directtransferlist() {
    var empno = this.employee.employeeno;
    this.wmsService.getdirecttransferdata(empno).subscribe(data => {
      this.materialtransferlist = data;
      this.materialtransferlist.forEach(item => {
        item.showtr = false;
      });
    });
  }

  
}
