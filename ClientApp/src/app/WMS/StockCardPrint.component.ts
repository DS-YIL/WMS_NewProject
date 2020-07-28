import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList, stockCardPrint } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { gatepassModel, materialistModel } from '../Models/WMS.Model';

@Component({
  selector: 'app-GatePassPrint',
  templateUrl: './StockCardPrint.component.html'
})
export class StockCardPrintComponent implements OnInit {
  constructor(private formBuilder: FormBuilder, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }


  public employee: Employee;
  public stockprintmodel: stockCardPrint;
  public materialList: Array<any> = [];
  public gatepassModel: gatepassModel;
  public prnttxt: string = "print";

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.getstockdetails();
  }

  getstockdetails() {
    this.wmsService.getstockdetails("4507868215", "BOP114247").subscribe(data => {
      this.stockprintmodel = data;
      console.log(this.stockprintmodel);
    })
  }

  //get gatepass list
  bindMaterilaDetails(gatepassId:any) {
    this.wmsService.gatepassmaterialdetail(gatepassId).subscribe(data => {
      this.materialList = data;
      if (this.materialList[0].print == false)
        this.prnttxt = "Reprint";
    });
  }

  printStockCard() {
    
    //this.wmsService.updateprintstatus(this.gatepassModel).subscribe(data => {
      this.prnttxt = "Reprint";
      //printing gatepass
      var printContents = document.getElementById("printTemplate").innerHTML;
      var originalContents = document.body.innerHTML;
      document.body.innerHTML = printContents;
      window.print();
      document.body.innerHTML = originalContents;
      return false;
   // });

    
  }
}
