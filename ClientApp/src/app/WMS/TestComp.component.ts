import { Component, OnInit, Inject, ViewChild } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { ConfirmationService } from 'primeng/api';
import { isNullOrUndefined } from 'util';
import { HttpClient } from '@angular/common/http';
import { testcrud, WMSHttpResponse } from '../Models/WMS.Model';

@Component({
  selector: 'app-TestComp',
  templateUrl: './TestComp.component.html',
  providers: [ConfirmationService]
})
export class TestCompComponent implements OnInit {

  constructor(private confirmationService: ConfirmationService, private http: HttpClient, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) {}
  
  
  public employee: Employee;
  getlistdata: testcrud[] = [];
  postmodel: testcrud;
  showadddatamodel: boolean = false;
  response: WMSHttpResponse;


  

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.postmodel = new testcrud();
    this.response = new WMSHttpResponse();
    this.getlist();
     
  }

  adddd() {
    debugger;
    this.response = new WMSHttpResponse();
    this.wmsService.insertcsv().subscribe(data => {
      debugger;
      this.response = data;
      alert(this.response.message);
    });

  }

  addddtostock() {
    debugger;
    this.wmsService.addddtostock().subscribe(data => {
      alert("ok");
    });

  }

  getlist() {
    debugger;
    this.getlistdata = [];
    this.spinner.show();
    this.wmsService.gettestcrud().subscribe(data => {
      this.getlistdata = data;
      this.spinner.hide();
    });
  }

  post() {
    debugger;
    if (!this.postmodel.name) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Name' });
      return;
    }
    this.wmsService.posttestcrud(this.postmodel).subscribe(data => {
      this.showadddatamodel = false;
      if (data) {
        this.messageService.add({ severity: 'success', summary: '', detail: data });
        this.getlist();
        this.spinner.hide();
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'not saved' });
        this.spinner.hide();
      }
      
    });
  }

  delete(testdata: testcrud) {
    debugger;
    this.wmsService.deletetestcurd(testdata.id).subscribe(data => {
      if (data) {
        this.messageService.add({ severity: 'success', summary: '', detail: data });
        this.getlist();
        this.spinner.hide();
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'not deleted' });
        this.spinner.hide();
      }

    });
  }

  refreshsavemodel() {
    this.postmodel = new testcrud();
  }

  Editrow(data: testcrud) {
    this.postmodel = data;
    this.showadddatamodel = true;

  }


}
