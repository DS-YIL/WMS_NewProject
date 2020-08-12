import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-ABCAnalysis',
  templateUrl: './MaterialReport.component.html'
})
export class MaterialReportComponent implements OnInit {
  constructor(private wmsService: wmsService, private route: ActivatedRoute, private messageService: MessageService, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public employee: Employee;
  public dynamicData: DynamicSearchResult;
  public path: string = null;


  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.getMaterials();
  }

  getMaterials() {
    this.wmsService.getMaterial().subscribe(data => {
      
        //this.messageService.add({ severity: 'error', summary: '', detail: "Material doesn't exist" });
      
    
    });
  }
}
