import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { wmsService } from 'src/app/WmsServices/wms.service';
import { constants } from 'src/app/Models/WMSConstants'
import { NgxSpinnerService } from "ngx-spinner";
import { Employee, DynamicSearchResult, searchList } from 'src/app/Models/Common.Model';
import { PoFilterParams, PoDetails, ManagerDashboard } from 'src/app/Models/WMS.Model';
import { DatePipe } from '@angular/common';


@Component({
  selector: 'app-Dashboard',
  templateUrl: './PMDashboard.component.html',
  providers: [DatePipe]
})
export class PMDashboardComponent implements OnInit {
  constructor(private formBuilder: FormBuilder, private spinner: NgxSpinnerService, public wmsService: wmsService, private datePipe: DatePipe, public constants: constants, private route: ActivatedRoute, private router: Router) { }
  public employee: Employee;
  data: any;
  receiptsdata: any;
  qualitydata: any;
  acceptancedata: any;
  putawaydata: any;
  totalReceipts: any;
  totalacceptance: any;
  totalquality: any;
  totalputaway: any;
  public mdashboard = new ManagerDashboard();
  //page load event
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.getreceiptsdata();
    //this.data = {
    //  labels: ['Pen', 'Accep', 'Onhold'],
    //  datasets: [
    //    {
    //      data: [300, 50, 100],
    //      backgroundColor: [
    //        "#FF6384",
    //        "#7CB342",
    //        "#FFCE56"
    //      ],
    //      hoverBackgroundColor: [
    //        "#FF6384",
    //        "#9CCC65",
    //        "#FFCE56"
    //      ]
    //    }]
    //};

  }

  getreceiptsdata() {
    this.wmsService.getreceiptslist().subscribe(data => {
      debugger;
      this.mdashboard = data;
      this.totalReceipts = this.mdashboard.pendingcount + this.mdashboard.completedcount + this.mdashboard.onholdcount;
      this.totalquality = this.mdashboard.qualitypendcount + this.mdashboard.qualitycompcount;
      this.totalacceptance = this.mdashboard.acceptancependcount + this.mdashboard.acceptancecompcount;
      this.totalputaway = this.mdashboard.putawaypendcount + this.mdashboard.putawaycompcount + this.mdashboard.putawayinprocount;
      this.receiptsdata = {
        labels: ['Pending', 'Completed', 'Onhold'],
        datasets: [
          {
            data: [this.mdashboard.pendingcount, this.mdashboard.completedcount, this.mdashboard.onholdcount],
            backgroundColor: [
              "#FF6384",
              "#7CB342",
              "#FFCE56"
            ],
            hoverBackgroundColor: [
              "#FF6384",
              "#9CCC65",
              "#FFCE56"
            ]
          }]
      };

      this.qualitydata = {
        labels: ['Pending', 'Completed'],
        datasets: [
          {
            data: [this.mdashboard.qualitypendcount, this.mdashboard.qualitycompcount],
            backgroundColor: [
              "#FF6384",
              "#7CB342"
            ],
            hoverBackgroundColor: [
              "#FF6384",
              "#9CCC65"
            ]
          }]
      };

      this.acceptancedata = {
        labels: ['Pending', 'Completed'],
        datasets: [
          {
            data: [this.mdashboard.acceptancependcount, this.mdashboard.acceptancecompcount],
            backgroundColor: [
              "#FF6384",
              "#7CB342"
            ],
            hoverBackgroundColor: [
              "#FF6384",
              "#9CCC65"
            ]
          }]
      };

      this.putawaydata = {
        labels: ['Pending', 'Completed', 'In Progress'],
        datasets: [
          {
            data: [this.mdashboard.putawaypendcount, this.mdashboard.putawaycompcount, this.mdashboard.putawayinprocount],
            backgroundColor: [
              "#FF6384",
              "#7CB342",
              "#FFCE56"
            ],
            hoverBackgroundColor: [
              "#FF6384",
              "#9CCC65",
              "#FFCE56"
            ]
          }]
      };
    });
  }
  
}


