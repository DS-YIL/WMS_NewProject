import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { gatepassModel, materialistModel } from '../Models/WMS.Model';

@Component({
  selector: 'app-GatePassPrint',
  templateUrl: './GatePassPrint.component.html'
})
export class GatePassPrintComponent implements OnInit {
  constructor(private formBuilder: FormBuilder, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }


  public employee: Employee;
  public materialList: Array<any> = [];
  public gatepassModel: gatepassModel;
  public prnttxt: string = "print";

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

  }

 
  //get gatepass list
  bindMaterilaDetails(gatepassId:any) {
    this.wmsService.gatepassmaterialdetail(gatepassId).subscribe(data => {
      this.materialList = data;
      if (this.materialList[0].print == false)
        this.prnttxt = "Reprint";
    });
  }

  printGatePass() {
    this.gatepassModel = new gatepassModel();
    this.gatepassModel = this.materialList[0];
    this.gatepassModel.printedby = this.employee.employeeno;
    this.wmsService.updateprintstatus(this.gatepassModel).subscribe(data => {
      this.prnttxt = "Reprint";
      //printing gatepass
      var printContents = document.getElementById("printTemplate").innerHTML;
      var originalContents = document.body.innerHTML;
      document.body.innerHTML = printContents;
      window.print();
      document.body.innerHTML = originalContents;
      return false;
    });

    
  }
}
