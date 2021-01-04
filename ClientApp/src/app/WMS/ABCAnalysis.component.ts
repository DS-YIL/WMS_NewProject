import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult } from '../Models/Common.Model';
import { MessageService } from 'primeng/api';
import { NgxSpinnerService } from "ngx-spinner";

@Component({
  selector: 'app-ABCAnalysis',
  templateUrl: './ABCAnalysis.component.html'
})
export class ABCAnalysisComponent implements OnInit {
  constructor(private wmsService: wmsService, private messageService: MessageService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public employee: Employee;
  public dynamicData: DynamicSearchResult;

  public ABCavailableqtyList: Array<any> = [];
  public ABCListBycategory: Array<any> = [];
  public category: string;
  public showABCavailableqtyList: boolean = true;
  public showAbcListByCategory; showAbcMatList: boolean = false;
  public totalunitprice;
  public totalQty: number = 0;

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
      if (data) {
        this.ABCavailableqtyList = data;
        this.calculateTotalQty();
        this.calculateTotalPrice();
      }
    
      this.spinner.hide();
    });
    
  }

  //Export to excel
  exportExcel() {

    if (this.ABCavailableqtyList.length > 0) {
      this.ABCavailableqtyList[0].totalQty = this.totalQty;
      this.ABCavailableqtyList[0].totalunitprice = this.totalunitprice;
      let new_list = this.ABCavailableqtyList.map(function (obj) {
       
        return {
          'Category': obj.category,
          'Available Qty': obj.availableqty,
          '% of Qty': ((obj.availableqty / obj.totalQty) * 100).toFixed(),
          'Cost': obj.totalcost,
          '% of cost': ((obj.totalcost / obj.totalunitprice) * 100).toFixed()

        }

      });
      import("xlsx").then(xlsx => {
        const worksheet = xlsx.utils.json_to_sheet(new_list);
        const workbook = { Sheets: { 'data': worksheet }, SheetNames: ['data'] };
        const excelBuffer: any = xlsx.write(workbook, { bookType: 'xlsx', type: 'array' });
        this.saveAsExcelFile(excelBuffer, "ABCAnalysisreport");
      });
    }
    else {
      this.messageService.add({ severity: 'error', summary: '', detail: 'No data exists' });
      return;
    }
   
  }


  saveAsExcelFile(buffer: any, fileName: string): void {
    import("file-saver").then(FileSaver => {
      let EXCEL_TYPE = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=UTF-8';
      let EXCEL_EXTENSION = '.xlsx';
      const data: Blob = new Blob([buffer], {
        type: EXCEL_TYPE
      });
      FileSaver.saveAs(data, fileName + '_export_' + new Date().getTime() + EXCEL_EXTENSION);
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
    this.dynamicData.query = "select itemid, sec.grnnumber , createddate as receiveddate, totalquantity,availableqty,totalquantity - availableqty AS issuedqty,itemlocation from wms.wms_stock ws inner join wms.wms_securityinward sec on sec.inwmasterid = ws.inwmasterid  where ws.materialid = '" + details.materialid + "'";
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

