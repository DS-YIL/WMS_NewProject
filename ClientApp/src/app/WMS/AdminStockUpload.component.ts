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
import { testcrud, WMSHttpResponse, StockModel } from '../Models/WMS.Model';

@Component({
  selector: 'app-AdminStockUpload',
  templateUrl: './AdminStockUpload.component.html',
  providers: [ConfirmationService]
})
export class AdminStockUploadComponent implements OnInit {

  constructor(private confirmationService: ConfirmationService, private http: HttpClient, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService, @Inject('BASE_URL') baseUrl: string) {
    this.url = baseUrl;
  }
  
  
  public employee: Employee;
  response: WMSHttpResponse;
  uploadedFiles: any[] = [];
  public url = "";
  getlistdata: StockModel[] = [];
  getEXlistdata: StockModel[] = [];
  public responsestr: string = "";
  public responseexceptionstr: string = "";
  displayModal: boolean = false;
  uploadcode: string = "";
  displayEXModal: boolean = false;
  displayTable: boolean = false;
  strtotalrecord: string = "";
  strsuccessrecord: string = "";

  

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.response = new WMSHttpResponse();
    this.strsuccessrecord = "";
    this.strtotalrecord = "";
    this.displayModal = false;
    //this.getlist();
     
  }

  settextval(records: string) {
    debugger;
    let rcds = records;
    this.strsuccessrecord = "";
    this.strtotalrecord = "";
    var arrrcds = rcds.split('\n');
    this.strtotalrecord = String(arrrcds[1]);
    this.strsuccessrecord = String(arrrcds[2]);
  }

  getlist(uploadcode: string) {
    this.displayTable = false;
    this.getlistdata = [];
    this.spinner.show();
    this.wmsService.getinitialStock(uploadcode).subscribe(data => {
      this.getlistdata = data;
      this.displayTable = true;
      this.spinner.hide();
    });
  }
  showex() {
    alert("Hiii");
  }
  getexlist() {
    this.getEXlistdata = [];
    this.displayEXModal = false;
    this.spinner.show();
    this.wmsService.getinitialStockEX(this.uploadcode).subscribe(data => {
      this.getEXlistdata = data;
      this.displayEXModal = true;
      this.spinner.hide();
    });
  }

  onUpload(event, form) {
    this.displayTable = false;
    for (let file of event.files) {

      var empno = this.employee.employeeno;
      var fname = empno+"_" + file.name;
      const formData = new FormData();
      formData.append('file', file, fname);
      //this.wmsService.postinitialstock(formData).subscribe(data => {
        
      //});
      this.spinner.show();
      this.http.post(this.url + 'Staging/uploadInitialStockExcelByUser', formData)
        .subscribe(data => {
          this.spinner.hide();
          form.clear();
          this.displayTable = true;
          debugger;
          this.response = data as WMSHttpResponse;
          var arrdata = String(this.response.message).split('$viewdatalistcode$');
          var uploadcode = String(arrdata[1]).trim();
          var arrwithexception = String(arrdata[0]).trim();
          var arrexception = String(arrwithexception).split('$EX$');
          let exception = String(arrexception[1]).trim()
          let displaystring = String(arrexception[0]).trim();
          let displaystring1 = String(displaystring.split('-').join('\n'));
          let displaystring2 = String(displaystring1.split('_').join(' '));
          exception = exception.split('-').join('\n');
          exception = exception.split('_').join(' ');
          this.responsestr = displaystring2 + "\n" + String(exception);
          this.responseexceptionstr = exception;
          this.uploadcode = uploadcode;
          if (!isNullOrUndefined(this.responsestr)) {
            this.displayModal = true;
          }
          if (!isNullOrUndefined(uploadcode)) {
            this.getlist(uploadcode);
          }
          this.settextval(displaystring2);
         
        
       });
    }

   
  }

 

 

  

 


}
