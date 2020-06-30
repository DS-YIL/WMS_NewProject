import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { commonComponent } from '../WmsCommon/CommonCode';
import { categoryValues, Cyclecountconfig, daylist } from '../Models/WMS.Model';
import { isNullOrUndefined } from 'util';
import { MessageService } from 'primeng/api';
import { getLocaleExtraDayPeriodRules } from '@angular/common';
import { FormGroup, FormBuilder, FormArray, Validators } from "@angular/forms";
import { DatePipe } from "@angular/common";

@Component({
  selector: 'app-Cycleconfig',
  templateUrl: './Cycleconfig.component.html'
})
export class CycleconfigComponent implements OnInit {
  constructor(private fb: FormBuilder,private wmsService: wmsService, private messageService: MessageService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public employee: Employee;
  name: string = "Ramesh";
  configmodel = new Cyclecountconfig();
  showweekly: boolean = false;
  showbiweekly: boolean = false;
  showmonthly: boolean = false;
  showquarterly: boolean = false;
  showyearly: boolean = false;
  public yearlynotifdate: Date;
  DayList: daylist[] = [];
  cols: any[];
  exportColumns: any[];
  weekday1: string = "";
  weekday2: string = "";
  weekday3: string = "";
  ShowDayList: daylist[] = [];

 

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.getCyclecountConfig();
    this.cols = [
      { field: 'description', header: 'description' },
      { field: 'showdate', header: 'Date' },
    ];

    this.exportColumns = this.cols.map(col => ({ title: col.header, dataKey: col.field }));
   
   
  }

  setnotifydate() {
    debugger;
    
    this.showweekly = false;
    this.showbiweekly = false;
    this.showmonthly = false;
    this.showquarterly = false;
    this.showyearly = false;
    var jsondata = JSON.parse(this.configmodel.notificationon);
   
    var freq = this.configmodel.frequency;
    if (freq == "Weekly") {
      this.showweekly = true;
      this.weekday1 = jsondata[0].showday;
    }
    else if (freq == "Twice in a week") {
      this.showbiweekly = true;
      this.weekday2 = jsondata[0].showday;
      this.weekday3 = jsondata[1].showday;
    }
    else if (freq == "Monthly") {
      //this.DayList = [];
      //this.showmonthly = true;
      //for (var i = 0; i < jsondata.length; i++) {
      //  var dl = new daylist();
      //  dl.description = jsondata[i].description;
      //  dl.showdate = new Date(jsondata[i].showdate);
      //  dl.showday = ""
      //  this.DayList.push(dl);
      //}

    }
    else if (freq == "Quarterly") {
      //this.DayList = [];
      //this.showquarterly = true;
      //for (var i = 0; i < jsondata.length; i++) {
      //  var dl = new daylist();
      //  dl.description = jsondata[i].description;
      //  dl.showdate = new Date(jsondata[i].showdate);
      //  dl.showday = ""
      //  this.DayList.push(dl);
      //}
      

    }
    else if (freq == "Yearly") {
      this.showyearly = true;
      this.yearlynotifdate = new Date(jsondata[0].showdate);

    }

  }


  updateCycleCountConfig() {
    debugger;
   var notificationdata = [];
    var persum = this.configmodel.apercentage + this.configmodel.bpercentage + this.configmodel.cpercentage;

    if (persum > 100) {
      this.messageService.add({ severity: 'error', summary: 'Validation Message', detail: 'Sum of percentage can not exceed 100' });
      return;
    }
    if (isNullOrUndefined(this.configmodel.frequency) || this.configmodel.frequency == "") {
      this.messageService.add({ severity: 'error', summary: 'Validation Message', detail: 'Please select frequency' });
      return;
    }
    if (isNullOrUndefined(this.configmodel.cyclecount) || this.configmodel.cyclecount < 1) {
      this.messageService.add({ severity: 'error', summary: 'Validation Message', detail: 'Please provide cycle count' });
      return;
    }
    if (this.configmodel.frequency == "Weekly" || this.configmodel.frequency == "Twice in a week") {
      this.configmodel.notificationtype = "Day";
      if (this.configmodel.frequency == "Weekly") {
        if (isNullOrUndefined(this.weekday1) || this.weekday1 == "") {
          this.messageService.add({ severity: 'error', summary: 'Validation Message', detail: 'Please select day.' });
          return;
        }
        var data = {
          "description": "Weekly",
          "showday": this.weekday1,
          "showdate": ""
        }
        notificationdata.push(data);

      }
      else {
        if (isNullOrUndefined(this.weekday2) || this.weekday2 == "") {
          this.messageService.add({ severity: 'error', summary: 'Validation Message', detail: 'Please select day1.' });
          return;
        }
        if (isNullOrUndefined(this.weekday3) || this.weekday3 == "") {
          this.messageService.add({ severity: 'error', summary: 'Validation Message', detail: 'Please select day2.' });
          return;
        }
        var data = {
          "description": "Weekly",
          "showday": this.weekday2,
          "showdate": ""
        }
        var data1 = {
          "description": "Weekly",
          "showday": this.weekday3,
          "showdate": ""
        }
        notificationdata.push(data);
        notificationdata.push(data1);

      }
    }
    //else if (this.configmodel.frequency == "Monthly" || this.configmodel.frequency == "Quarterly") {
    //  var countlist = this.DayList.filter(function (element, index) {
    //    return (isNullOrUndefined(element.showdate));
    //  });
    //  if (countlist.length > 0) {
    //    this.messageService.add({ severity: 'error', summary: 'validation Message', detail: 'Please select date for all.' });
    //    return;

    //  }
    //  this.configmodel.notificationtype = "Date";
    //  this.DayList.forEach(element => {
    //     notificationdata.push(element);

    //  });

    //}
    else if (this.configmodel.frequency == "Yearly") {
      this.configmodel.notificationtype = "Date";
      if (isNullOrUndefined(this.yearlynotifdate)) {
        this.messageService.add({ severity: 'error', summary: 'validation Message', detail: 'Please select date' });
        return;
      }
      var data = {
        "description": "Yearly",
        "showdate": this.yearlynotifdate.toDateString(),
        "showday": ""
      }
      notificationdata.push(data);

    }
    else {
      this.configmodel.notificationtype = "Daily";
    }
    this.configmodel.notificationon = JSON.stringify(notificationdata);

    this.spinner.show();
    this.wmsService.updateCyclecountconfig(this.configmodel).subscribe(data => {
      console.log(data);
      this.messageService.add({ severity: 'success', summary: 'success Message', detail: 'Configuration updated' });
      this.getCyclecountConfig();
      this.spinner.hide();
    });
     
  }

 
  



  setCycleCount() {
    this.showweekly= false;
    this.showbiweekly= false;
    this.showmonthly= false;
    this.showquarterly = false;
    this.showyearly= false;
    var stdate = this.configmodel.startdate;
    var edate = this.configmodel.enddate;
    var freq = this.configmodel.frequency;
    var count = this.configmodel.countall;
    
    if (isNullOrUndefined(this.configmodel.frequency) || this.configmodel.frequency == "") {
      return;
    }
    if (!isNullOrUndefined(stdate) && !isNullOrUndefined(edate)) {
      debugger;
      if (freq == "Daily") {
        var date2 = new Date(edate);
        var date1 = new Date(stdate);

        // To calculate the time difference of two dates 
        var Difference_In_Time = date2.getTime() - date1.getTime();

        // To calculate the no. of days between two dates 
        var Difference_In_Days = Difference_In_Time / (1000 * 3600 * 24);
        if (count > 0) {
          var cyclecount = Math.ceil(count / Difference_In_Days);
          this.configmodel.cyclecount = cyclecount;
        }
        else {
          this.configmodel.cyclecount = 0;
        }

      }
      else if (freq == "Weekly") {
        this.showweekly = true;
        var date2 = new Date(edate);
        var date1 = new Date(stdate);
        var diff = (date2.getTime() - date1.getTime()) / 1000;
        diff /= (60 * 60 * 24 * 7);
        var weeks = Math.abs(Math.round(diff));
        if (count > 0) {
          var cyclecount = Math.ceil(count / weeks);
          this.configmodel.cyclecount = cyclecount;
        }
        else {
          this.configmodel.cyclecount = 0;
        }

      }
      else if (freq == "Twice in a week") {
        this.showbiweekly = true;
        var date2 = new Date(edate);
        var date1 = new Date(stdate);
        var diff = (date2.getTime() - date1.getTime()) / 1000;
        diff /= (60 * 60 * 24 * 7);
        var weeks = Math.abs(Math.round(diff));
        if (count > 0) {
          var cyclecount = Math.ceil(count / weeks);
          this.configmodel.cyclecount = Math.ceil(cyclecount / 2);
        }
        else {
          this.configmodel.cyclecount = 0;
        }

      }
      else if (freq == "Monthly") {
        this.showmonthly = true;
        var date2 = new Date(edate);
        var date1 = new Date(stdate);
       // this.setDaylist(date1, date2, freq);
        var months = 0;
        months = (date2.getFullYear() - date1.getFullYear()) * 12;
        months -= date1.getMonth();
        months += date2.getMonth();
        var monthcount = months <= 0 ? 0 : months;
        if (count > 0) {
          var cyclecount = Math.ceil(count / monthcount);
          this.configmodel.cyclecount = cyclecount;
        }
        else {
          this.configmodel.cyclecount = 0;
        }

      }
      else if (freq == "Quarterly") {
        this.showquarterly = true;
        var date2 = new Date(edate);
        var date1 = new Date(stdate);
        //this.setDaylist(date1, date2, freq);
        var months = 0;
        months = (date2.getFullYear() - date1.getFullYear()) * 12;
        months -= date1.getMonth();
        months += date2.getMonth();
        var monthcount = months <= 0 ? 0 : months;
        if (count > 0) {
          var cyclecount = Math.ceil(count / monthcount);
          this.configmodel.cyclecount = Math.ceil(cyclecount / 4);
        }
        else {
          this.configmodel.cyclecount = 0;
        }

      }
      else if (freq == "Yearly") {
        this.showyearly = true;
        if (count > 0) {
          this.configmodel.cyclecount = count;
        }
        else {
          this.configmodel.cyclecount = 0;
        }
      }
    }
  }

  setDaylist(stdate: Date, edate: Date, freq: string) {
    this.DayList = [];
    var desc = "";
    var jsondata = JSON.parse(this.configmodel.notificationon);
    if (!isNullOrUndefined(jsondata) && jsondata.length > 0) {
       desc = jsondata[0].description;
    }
    debugger;
    var months = ["January","February","March","April","May","June","July","August","September","October","November","December"]
    if (freq == "Monthly") {
      //var desca = desc.split('(')[0];
      //if (months.includes(desca)) {
      //  this.setnotifydate();
      //  return;
      //}
      var date2year = edate.getFullYear();
      var date1year = stdate.getFullYear();
      
      var startmonth = stdate.getMonth();
      var endmonth = edate.getMonth();
      var loopend = endmonth;
      if (endmonth < startmonth) {
        loopend = 11 + endmonth;
      }
      for (var i = startmonth; i <= loopend; i++) {
        let day = new daylist();
        var year = date1year;
        var month = months[i];
        if (i > 11) {
          year = date2year;
          month = months[i-12]
        }
        day.description = month + "(" + year + ")";
        this.DayList.push(day);
      }

     

    }
    if (freq == "Quarterly") {
      //if (desc.includes("Quarter")) {
      //  this.setnotifydate();
      //  return;
      //}
      var startmonth = stdate.getMonth();
      var endmonth = edate.getMonth();
      var loopend = endmonth;
      if (endmonth < startmonth) {
        loopend = 11 + endmonth;
      }
      var quarters = Math.ceil(loopend / 4);
      for (var i = 1; i <= quarters; i++) {
        let day = new daylist();
        day.description = "Quarter" + i;
        this.DayList.push(day);

      }
      

    }

   

  }

  getCyclecountConfig() {
    this.spinner.show();
    this.wmsService.getCyclecountConfig().subscribe(data => {
      this.configmodel = data;
      this.setnotifydate();
      this.spinner.hide();
    });

  }
 

}

