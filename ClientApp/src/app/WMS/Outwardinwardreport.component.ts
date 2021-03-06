import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl, ValidatorFn } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { materialtransferMain, materialtransferTR, materialRequestDetails, returnmaterial, gatepassModel, materialistModel, PoDetails, StockModel, materialistModelreturn, materialistModeltransfer, ddlmodel, DirectTransferMain, outwardinwardreportModel } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { isNullOrUndefined } from 'util';

@Component({
  selector: 'app-Outwardinwardreport',
  templateUrl: './Outwardinwardreport.component.html'
})
export class OutwardinwardreportComponent implements OnInit {

  materialtransferlist: outwardinwardreportModel[] = [];
  outindatalist: outwardinwardreportModel[] = [];
  public employee: Employee;
  returntype: string = "";
  isoutward: boolean = false;
  isinward: boolean = false;
  constructor(private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.returntype = "out";
    this.isoutward = true;
    this.directtransferlist();
   
  }

  getcolspan() {
    if (this.isinward) {
      return "5"
    }
    else {
      return "5"
    }
  }

  showattachdata(data: outwardinwardreportModel) {
    data.materialdata = [];
    debugger;
    var datax = this.materialtransferlist.filter(function (element, index) {
      return (element.gatepassid == data.gatepassid);
    });
    
    if (datax.length > 0) {
      
      datax.forEach(item => {
        debugger;
        var checkrow = data.materialdata.filter(function (element, index) {
          return (element.materialid == item.materialid && element.materialdescription == item.materialdescription);
        });
        var dataiter = datax.filter(function (element, index) {
          return (element.materialid == item.materialid && element.materialdescription == item.materialdescription);
        });
        if (checkrow.length == 0) {
          var inwardqty = 0;
          var outwardqty = 0;
          dataiter.forEach(item => {
            debugger;
            inwardqty += item.inwardqty;
            outwardqty += item.outwardqty;
          });
          item.outwardqtyview = outwardqty;
          item.inwardqtyview = inwardqty;
          data.materialdata.push(item);
        }
     
      });
      


    }

    
   
     
    data.showtr = !data.showtr;

  }
  //get direct transfer list
  directtransferlist() {
    this.outindatalist = [];
    var empno = this.employee.employeeno;
    this.wmsService.outingatepassreport().subscribe(data => {
      this.materialtransferlist = data;
      this.getdata();
    });
  }
  getdata() {
    debugger;
    this.isoutward = false;
    this.isinward = false;
    this.outindatalist = [];
    if (this.returntype == "out") {
      this.isoutward = true;
      var setdata = this.materialtransferlist.filter(function (element, index) {
        return (!isNullOrUndefined(element.outwarddate));
      });
      setdata.forEach(item => {
        var checkrow = this.outindatalist.filter(function (element, index) {
          return (element.gatepassid == item.gatepassid);
        });
        if (checkrow.length == 0) {
          item.showtr = false;
          this.outindatalist.push(item);
        }
      });
      
    

    }
    else if (this.returntype == "in") {
      this.isinward = true;
      var setdata = this.materialtransferlist.filter(function (element, index) {
        return (!isNullOrUndefined(element.securityinwarddate));
      });
      setdata.forEach(item => {
        var checkrow = this.outindatalist.filter(function (element, index) {
          return (element.gatepassid == item.gatepassid);
        });
        if (checkrow.length == 0) {
          item.showtr = false;
          this.outindatalist.push(item);
        }
      });
    }
  }

  
}
