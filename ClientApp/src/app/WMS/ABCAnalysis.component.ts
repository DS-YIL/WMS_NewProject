import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";

@Component({
  selector: 'app-ABCAnalysis',
  templateUrl: './ABCAnalysis.component.html'
})
export class ABCAnalysisComponent implements OnInit {
  constructor(private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public employee: Employee;
  public dynamicData: DynamicSearchResult;

  public ABCavailableqtyList: Array<any> = [];
  public ABCListBycategory: Array<any> = [];
  public category: string;
  public showABCavailableqtyList: boolean = true;
  public showAbcListByCategory; showAbcMatList: boolean = false;
  public totalunitprice; totalQty: number = 0;

  cols: any[];
  exportColumns: any[];

  public ABCAnalysisMateDet: Array<any> = [];
  public matDetails: any;

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.cols = [
      { field: 'Category', header: 'Category' },
      { field: 'materialid', header: 'Material' },
      { field: 'materialdescription', header: 'Material Descr' },
      { field: 'availableqty', header: 'Available Qty' },
      { field: 'unitprice', header: 'Unit Price' }
    ];
    this.exportColumns = this.cols.map(col => ({ title: col.header, dataKey: col.field }));
    this.getABCavailableqtyList();
  }

  //get ABC available list
  getABCavailableqtyList() {
    this.spinner.show();
    this.ABCavailableqtyList = [];
    this.wmsService.getABCavailableqtyList().subscribe(data => {
      this.ABCavailableqtyList = data;
      this.calculateTotalQty();
      this.calculateTotalPrice();
      this.spinner.hide();
    });
    
  }

 //get ABCList by categories
  showAbcListByCat(details: any) {
    this.showABCavailableqtyList = false;
    this.showAbcListByCategory = true;
    this.category = details.category;
    this.spinner.show();
    this.ABCListBycategory = [];
    this.wmsService.GetABCListBycategory(details.category).subscribe(data => {
      this.ABCListBycategory = data;
      this.spinner.hide();
    });
  }

  //get material details by materialid
  showMatdetails(details: any) {
    this.showAbcListByCategory = false;
    this.showAbcMatList = true;
    this.spinner.show();
    this.matDetails = details;
    this.ABCAnalysisMateDet = [];
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.query = "select itemid, sec.grnnumber , createddate as receiveddate, totalquantity,availableqty,totalquantity - availableqty AS issuedqty,itemlocation from wms.wms_stock ws inner join wms.wms_securityinward sec on sec.pono = ws.pono  where ws.materialid = '" + details.materialid + "'";
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.ABCAnalysisMateDet = data;
      this.spinner.hide();
    });
  }

  //sum of total quantity
  calculateTotalQty() {
    this.totalQty = 0;
    if (this.ABCavailableqtyList) {
      this.ABCavailableqtyList.forEach(item => {
        if (item.availableqty)
          this.totalQty += item.availableqty;
      })
    }
    return this.totalQty;
  }

  //sum of total price
  calculateTotalPrice() {
    this.totalunitprice = 0;
    if (this.ABCavailableqtyList) {
      this.ABCavailableqtyList.forEach(item => {
        if (item.totalcost)
          this.totalunitprice += item.totalcost;
      })
    }
    return this.totalunitprice;
  }

  //showing available qunatity list when click on back button
  showabcavailableqtyList() {
    this.showABCavailableqtyList = true;
    this.showAbcListByCategory = false;
  }

  //showing abclist by category when click on back button
  showCatList() {
    this.showAbcListByCategory = true;
    this.showAbcMatList = false;
  }
}

