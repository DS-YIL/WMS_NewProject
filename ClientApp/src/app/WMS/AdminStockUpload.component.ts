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
  public responsestr: string = "";
  public responseexceptionstr: string = "";
  displayModal: boolean = false;

  

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.response = new WMSHttpResponse();
    this.displayModal = false;
    //this.getlist();
     
  }

  getlist(uploadcode: string) {
    this.getlistdata = [];
    this.spinner.show();
    this.wmsService.getinitialStock(uploadcode).subscribe(data => {
      this.getlistdata = data;
      this.spinner.hide();
    });
  }

  onUpload(event) {
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
          debugger;
          this.response = data as WMSHttpResponse;
          var arrdata = String(this.response.message).split('$viewdatalistcode$');
          var uploadcode = String(arrdata[1]).trim();
          var arrwithexception = String(arrdata[0]).trim();
          var arrexception = String(arrwithexception).split('$EX$');
          let exception = String(arrexception[0]).trim()
          let displaystring = String(arrexception[1]).trim();
          displaystring = displaystring.split('-').join('\n');
          displaystring = displaystring.split('_').join(' ');
          exception = exception.split('-').join('\n');
          exception = exception.split('_').join(' ');
          this.responsestr = displaystring;
          this.responseexceptionstr = exception;
          this.displayModal = true;
          this.getlist(uploadcode);
        
       });
    }
  }

 

 

  

 


}
