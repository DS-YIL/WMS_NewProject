import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { commonComponent } from '../WmsCommon/CommonCode';
import { categoryValues,FIFOValues } from '../Models/WMS.Model';
import { MessageService, Message } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
@Component({
  selector: 'app-ABCAnalysis',
  templateUrl: './FIFO.component.html'
})
export class FIFOComponent implements OnInit {
  constructor(private ConfirmationService: ConfirmationService, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public employee: Employee;
  public dynamicData: DynamicSearchResult;
  public FIFOValues: FIFOValues;
  public Oldestdata: FIFOValues;
  public FIFOList: Array<any> = [];
  public podetailsList: Array<FIFOValues> = [];
  public category: string;
  public showABCavailableqtyList: boolean = true;
  public showAbcListByCategory; showAbcMatList: boolean = false;
  public totalunitprice; totalQty: number = 0;
  public material: string;
  cols: any[];
  exportColumns: any[];

  public ABCAnalysisMateDet: Array<any> = [];
  public matDetails: any;
  msgs: Message[] = [];
  ngOnInit() {
    this.FIFOValues = new FIFOValues();
 
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

   
    this.getFIFOList();
  }
  confirm1(data) {
    var info = data;
    this.ConfirmationService.confirm({
      message: 'Same Material received on ' +  data.createddate + ' and placed in ' + data.itemlocation + '  location, Would you like to continue?',
      header: 'Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        
        this.messageService.add({ severity: 'info', summary: 'Accepted', detail: 'You have accepted' });
      },
      reject: () => {
        
        this.messageService.add({ severity: 'info', summary: 'Ignored', detail: 'You have ignored' });
      }
    });
  }
  checkissueqty($event, entredvalue, maxvalue, material,createddate) {
    var id = $event.target.id;
    if (entredvalue > maxvalue) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Please enter issue quantity less than Available quantity' });
     
      (<HTMLInputElement>document.getElementById(id)).value = "";
    }
    else {
      
      this.wmsService.checkoldestmaterial(material, createddate).subscribe(data => {
        this.Oldestdata = data;
        if (data != null) {
          this.confirm1(this.Oldestdata);
        }
        //this.calculateTotalQty();
        //this.calculateTotalPrice();
        this.spinner.hide();
      });
    }
  }
  getenquiryList(material) {
    this.spinner.show();
    this.FIFOList = [];
    this.wmsService.getFIFOList(material).subscribe(data => {
      this.FIFOList = data;
      //this.calculateTotalQty();
      //this.calculateTotalPrice();
      this.spinner.hide();
    });

  }
  getFIFOList() {
    this.spinner.show();
    this.FIFOList = [];
    var material = null;
    this.wmsService.getFIFOList(material).subscribe(data => {
      this.FIFOList = data;
      //this.calculateTotalQty();
      //this.calculateTotalPrice();
      this.spinner.hide();
    });
    
  }
  InsertIssuedata() {
    this.FIFOList;
    this.wmsService.insertFIFOdata(this.FIFOList).subscribe(data => {
      this.spinner.hide();
      if (data)
        this.messageService.add({ severity: 'sucess', summary: 'sucee Message', detail: 'Status updated' });
      else
        this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Update Failed' });

    });
  }
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

