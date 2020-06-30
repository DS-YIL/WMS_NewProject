import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { commonComponent } from '../WmsCommon/CommonCode';
import { categoryValues, Cyclecountconfig } from '../Models/WMS.Model';
import { isNullOrUndefined } from 'util';
import { MessageService } from 'primeng/api';
import { getLocaleExtraDayPeriodRules } from '@angular/common';
import { DatePipe } from "@angular/common";

@Component({
  selector: 'app-Cyclecount',
  templateUrl: './Cyclecount.component.html',
  providers: [DatePipe]

})
export class CyclecountComponent implements OnInit {
  constructor(private wmsService: wmsService, private datePipe: DatePipe, private messageService: MessageService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public employee: Employee;
  name: string = "Ramesh";
  configmodel = new Cyclecountconfig();
  cols: any[];
  exportColumns: any[];
  public CyclecountMaterialList: Array<any> = [];
  public CyclecountPendingMaterialList: Array<any> = [];
  public allCyclecountMaterialList: Array<any> = [];
  isapprover: boolean = true;
  status: string = "Pending";
  showApprovecolumn: boolean = true;
  showsubmitbutton: boolean = false;
  showsubmitbuttonuser: boolean = false;
  showdays: any[] = [];
  showtouser: boolean = false;
 

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");


    if (this.employee.roleid == "4") {
      this.isapprover = true;
      this.showsubmitbuttonuser = false;
      this.showsubmitbutton = true;
    }
    else {
      this.isapprover = false;
      this.showsubmitbutton = false;
    }

    this.cols = [
      { field: 'Category', header: 'Category' },
      { field: 'materialid', header: 'Material' },
      { field: 'materialdescription', header: 'Material Descr' },
      { field: 'availableqty', header: 'Available Qty' },
      { field: 'physicalqty', header: 'Physical Qty' },
      { field: 'differnce', header: 'Physical Qty' },
      { field: 'status', header: 'status' },
    ];

    this.exportColumns = this.cols.map(col => ({ title: col.header, dataKey: col.field }));
    this.getCyclecountConfig();
    this.getCyclecountPendingMaterialList();
   
  }

  

  filterbystatus() {
    var st = this.status;
    if (st == "Approved" || st == "Rejected") {
      this.showApprovecolumn = false;
      this.showsubmitbutton = false;
    }
    else {
      this.showApprovecolumn = true;
      this.showsubmitbutton = true;
    }
    if (!this.isapprover) {
      this.showsubmitbutton = false;
    }
    var countlist = this.allCyclecountMaterialList.filter(function (element, index) {
      return (element.status == st);
    });
    this.CyclecountPendingMaterialList = countlist;

  }

  setapproval(e: any, data: any) {
    
    var data1 = e.target.value;
    if (data1 == 1) {
      data.isapprovalprocess = true;
      data.isapproved = true;

    }
    else if (data1 == 0) {
      data.isapprovalprocess = true;
      data.isapproved = false;
    }
    else {
      data.isapprovalprocess = false;
    }  
  }


  calculatedifference(data: any) {
   
    data.difference = Math.abs(data.physicalqty - data.availableqty);
    if (!isNullOrUndefined(data.physicalqty) && data.physicalqty > 0) {
      data.iscountprocess = true;
      data.status = "Counted";
    }
  }

  submit() {
   
    var countlist = this.CyclecountMaterialList.filter(function (element, index) {
      return (element.iscountprocess);
    });
    if (countlist.length > 0) {
      var dt = countlist;
      this.spinner.show();
      this.wmsService.updateinsertCyclecount(countlist).subscribe(data => {
        this.getCyclecountMaterialList();
        this.spinner.hide();
        this.messageService.add({ severity: 'success', summary: 'Success Message', detail: 'Saved Sucessfully' });
      });

    }
    else {
      this.approve();
    }


  }
  approve() {
  
    var countlist = this.CyclecountPendingMaterialList.filter(function (element, index) {
      return (element.isapprovalprocess);
    });
    if (countlist.length > 0) {
      this.spinner.show();
      this.wmsService.updateinsertCyclecount(countlist).subscribe(data => {
        this.getCyclecountPendingMaterialList();
        this.spinner.hide();
        this.messageService.add({ severity: 'success', summary: 'Success Message', detail: 'Saved Sucessfully' });
      });
    }

  }

  getCyclecountPendingMaterialList() {
   
    this.CyclecountPendingMaterialList = [];
    this.spinner.show();
    this.wmsService.getCyclecountPendingList().subscribe(data => {
      this.allCyclecountMaterialList = data;
      this.filterbystatus();
      this.spinner.hide();
    });

  }


  getCyclecountMaterialList() {
  
    var limita = 0;
    var limitb = 0;
    var limitc = 0;
    if (isNullOrUndefined(this.configmodel.frequency) || this.configmodel.frequency == "") {
      return;
    }

    if (isNullOrUndefined(this.configmodel.cyclecount) || this.configmodel.cyclecount == 0) {
      return;
    }
    if (this.configmodel.cyclecount > 0) {
      if (this.configmodel.apercentage > 0) {
        limita = Math.ceil(this.configmodel.cyclecount * this.configmodel.apercentage / 100);
      }
      if (this.configmodel.bpercentage > 0) {
        limitb = Math.ceil(this.configmodel.cyclecount * this.configmodel.bpercentage / 100);
      }
      if (this.configmodel.cpercentage > 0) {
        limitc = Math.ceil(this.configmodel.cyclecount * this.configmodel.cpercentage / 100);
      }
    }
    var persum = this.configmodel.apercentage + this.configmodel.bpercentage + this.configmodel.cpercentage;

    if (persum > 100) {
     
      return;
    }
   
    this.CyclecountMaterialList = [];
    this.spinner.show();
    this.wmsService.getCyclecountList(limita, limitb, limitc).subscribe(data => {
      this.CyclecountMaterialList = data;
      this.spinner.hide();
    });

  }

  showonDate() {
    debugger;
    var jsondata = JSON.parse(this.configmodel.notificationon);
    var stdate = new Date(this.configmodel.startdate);
    var enddt = new Date(this.configmodel.enddate);
    var today = new Date();
    var datestr = this.datePipe.transform(new Date(), 'yyyy-MM-dd');
    var days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    var day = days[ new Date().getDay()];
    var type = this.configmodel.notificationtype;
    if (!isNullOrUndefined(jsondata) && jsondata.length > 0) {
      for (var i = 0; i < jsondata.length; i++) {
        if (type == "Day") {
          if (jsondata[i].showday == day && today >= stdate && today <= enddt) {
            this.showtouser = true;
            this.showsubmitbuttonuser = true;
            if (this.isapprover) {
              this.showsubmitbuttonuser = false;
            }
            this.getCyclecountMaterialList();
            
            this.spinner.hide();
            break;
          }  
        }
        else if (type == "Date") {
          var datetocompare = this.datePipe.transform(new Date(jsondata[i].showdate), 'yyyy-MM-dd'); 
          if (datetocompare == datestr && today >= stdate && today <= enddt) {
            this.showtouser = true;
            this.showsubmitbuttonuser = true;
            if (this.isapprover) {
              this.showsubmitbuttonuser = false;
            }
            this.getCyclecountMaterialList();
            this.spinner.hide();
            break;
          }  
           
        }
      }
    }
  }
  


 
  getCyclecountConfig() {
    this.spinner.show();
    this.CyclecountMaterialList = [];
    this.wmsService.getCyclecountConfig().subscribe(data => {
      this.configmodel = data;
      this.showsubmitbuttonuser = true;
      if (this.isapprover) {
        this.showsubmitbuttonuser = false;
      }
      this.getCyclecountMaterialList();
      //this.showonDate();
      this.spinner.hide();
    });

  }
 

}

