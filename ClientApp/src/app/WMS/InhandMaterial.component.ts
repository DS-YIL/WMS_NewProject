import { Component, OnInit, Inject, ViewChild } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee } from '../Models/Common.Model';
import { DecimalPipe } from '@angular/common';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { ConfirmationService } from 'primeng/api';
import { isNullOrUndefined } from 'util';
import { HttpClient } from '@angular/common/http';
import { testcrud, WMSHttpResponse, MaterialinHand, matlocations } from '../Models/WMS.Model';

@Component({
  selector: 'app-InhandMaterial',
  templateUrl: './InhandMaterial.component.html',
  providers: [ConfirmationService, DecimalPipe]
})
export class InhandMaterialComponent implements OnInit {

  constructor(private confirmationService: ConfirmationService, private decimalPipe: DecimalPipe, private http: HttpClient, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }
  
  
  public employee: Employee;
  getlistdata: MaterialinHand[] = [];
  getlocationlistdata: matlocations[] = [];
  showadddatamodel: boolean = false;
  lblmaterial: string = "";
  lblmaterialdesc: string = "";
  response: WMSHttpResponse;


  

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.response = new WMSHttpResponse();
    this.getlist();
     
  }
  refreshsavemodel() {
    this.showadddatamodel = false;
    this.lblmaterial = "";
    this.lblmaterialdesc = "";
  }

  getlocations(material: string, data: MaterialinHand) {
    this.getlocationlistdata = [];
    this.lblmaterial = data.material;
    this.lblmaterialdesc = data.materialdescription;
    this.spinner.show();
    this.wmsService.getmatinhandlocations(material.trim()).subscribe(data => {
      this.getlocationlistdata = data;
      this.showadddatamodel = true;
      this.spinner.hide();
    });

  }

  getlist() {
    this.getlistdata = [];
    this.spinner.show();
    this.wmsService.getmatinhand().subscribe(data => {
      this.getlistdata = data;
      this.spinner.hide();
    });
  }


}
