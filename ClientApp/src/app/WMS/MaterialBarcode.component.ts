import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-ABCAnalysis',
  templateUrl: './MaterialBarcode.component.html'
})
export class MaterialBarcodeComponent implements OnInit {
  constructor(private wmsService: wmsService, private route: ActivatedRoute, private messageService: MessageService, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public employee: Employee;
  public material: any;
  public dynamicData: DynamicSearchResult;
  public path: string = null;


  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

  }

  GenerateBarcode(material:string) {
    this.wmsService.checkMatExists(material).subscribe(data => {
      if (data == "No material exists") {
        this.messageService.add({ severity: 'error', summary: '', detail: "Material doesn't exist" });
      }
      else {
        this.path = data;
      }
    });
  }
}
