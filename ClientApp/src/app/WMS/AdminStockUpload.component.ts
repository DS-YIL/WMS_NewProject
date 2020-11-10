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
  displayModal: boolean = false;

  

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.response = new WMSHttpResponse();
    this.displayModal = false;
    this.getlist();
     
  }

  getlist() {
    this.getlistdata = [];
    this.spinner.show();
    this.wmsService.getinitialStock().subscribe(data => {
      this.getlistdata = data;
      this.spinner.hide();
    });
  }

  onUpload(event) {
    for (let file of event.files) {

      var fname = file.name;
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
          let displaystring = String(this.response.message);
          displaystring = displaystring.split('-').join('\n');
          displaystring = displaystring.split('_').join(' ');
          this.responsestr = displaystring;
          this.displayModal = true;
          this.getlist();
        
       });
    }
  }

 

 

  

 


}
