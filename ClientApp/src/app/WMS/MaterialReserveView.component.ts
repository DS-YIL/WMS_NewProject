import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl, ValidatorFn } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { materialRequestDetails, materialList, requestData, materialListforReserve, materialReservetorequestModel } from 'src/app/Models/WMS.Model';
import { MessageService } from 'primeng/api';
import { commonComponent } from '../WmsCommon/CommonCode';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-MaterialRequest',
  templateUrl: './MaterialReserveView.component.html',
  providers: [DatePipe]
})
export class MaterialReserveViewComponent implements OnInit {
  AddDialog: boolean;
  showdialog: boolean;
  public materiallistData: Array<any> = [];

  constructor(private formBuilder: FormBuilder, private messageService: MessageService, private datePipe: DatePipe,private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public searchItems: Array<searchList> = [];
  public mindate: string;
  public maxdate: string;
  public selectedlist: Array<searchList> = [];
  public btnreq: boolean = true;
  public searchresult: Array<object> = [];
  public ponolist: any[] = [];
  public requestMatData = new requestData();
  public materialistModel: materialListforReserve;
  public materialmodel: Array<materialListforReserve> = [];
  public suppliername: string;
  public ponumber: string;
  public materialList: Array<materialListforReserve> = [];
  public requestDialog: boolean = false;
  public reserveList: Array<any> = [];
  public employee: Employee;
  public displayItemRequestDialog; RequestDetailsSubmitted; showAck; btnDisable: boolean = false;
  public materialRequestDetails: materialRequestDetails;
  public pono: string;
  public reservedfor: Date;
  public displaylist: boolean = false;
  public displayDD: boolean = true;
  reserveduptoview: Date;
  statusview: string = "";
  requestedbyview: string;
  requestedonview: Date;
  reserveidview: string = "";
  expired: boolean = false;
  showreservebtn: boolean = false;
  isrequested: boolean = false;
  public defaultmaterials: materialList[] = [];
  reservetorequest: materialReservetorequestModel;
  filteredmats: any[];
  displaypos: boolean = false;

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");


    //this.route.params.subscribe(params => {
    //  if (params["pono"]) {
    //    this.pono = params["pono"];
    //  }
    //});
    this.reservetorequest = new materialReservetorequestModel();
    this.getMaterialReservelist();
    this.getdefaultmaterialstoreserve();
  }

  getdefaultmaterialstoreserve() {
    this.defaultmaterials = []
    this.wmsService.getMaterialRequestlistdata(this.employee.employeeno, null).subscribe(data => {
      this.defaultmaterials = data;
    });
  }

  filtermats(event) {
    this.filteredmats = [];
    for (let i = 0; i < this.defaultmaterials.length; i++) {
      let brand = this.defaultmaterials[i].material;
      let pos = this.defaultmaterials[i].materialdescription;
      if (brand.toLowerCase().indexOf(event.query.toLowerCase()) == 0 || pos.toLowerCase().indexOf(event.query.toLowerCase()) == 0) {
        this.filteredmats.push(brand);
      }
    }
  }
  refreshdata() {
    this.suppliername = null;
    this.ponumber = null;
    this.requestMatData = new requestData();
    this.materialList = [];
    this.materialmodel = [];
    this.reserveMaterial(); 
  }
  radiochecked(e: any, data: any) {
    var event = e.target.checked;
    this.requestMatData.pono = data.pono;
    this.requestMatData.suppliername = data.suppliername;
    this.onPOSelected(this.requestMatData.pono);
    e.target.checked = false;
    this.displaypos = false;
  }
  closedg() {

  }
  showpodatabox() {
    this.displaypos = !this.displaypos;
  }
  onMaterialSelected1(data: any, ind: number) {
    debugger;
    var data1 = this.materialList.filter(function (element, index) {
      return (element.material == data.material && index != ind);
    });
    if (data1.length > 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Material already exist' });
      this.materialList[ind].material = "";
      this.materialList[ind].materialdescription = "";
      this.materialList[ind].materialcost = 0;
      this.materialList[ind].availableqty = 0;
      this.materialList[ind].quantity = 0;
      return false;
    }
    var data2 = this.defaultmaterials.filter(function (element, index) {
      return (element.material == data.material);
    });
    if (data2.length > 0) {
      data.materialdescription = data2[0].materialdescription;
      data.materialcost = data2[0].materialcost != null ? data2[0].materialcost : 0;
      data.availableqty = data2[0].availableqty != null ? data2[0].availableqty : 0;
    }

  }


  //get Material reserve based on login employee && po no
  getMaterialReservelist() {
    //this.employee.employeeno = "180129";
    this.wmsService.getMaterialReservelist(this.employee.employeeno).subscribe(data => {
      this.reserveList = data;
      this.reserveList.forEach(item => {
        if (!item.requestedquantity)
          item.requestedquantity = item.quotationqty;
      });
    });
  }

  //check validations for reserved quantity
  reqQtyChange(data: any) {
    if (data.requestedquantity > data.quotationqty) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Requested Quantity should be lessthan or equal to po quantity' });
      data.requestedquantity = data.quotationqty;
    }
  }

  //On PO Selected event
  onPOSelected(pono: string) {
    debugger;
    if (this.ponolist.filter(li => li.pono == pono).length > 0) {
      if (pono != "All") {
        var data = this.ponolist.find(li => li.pono == pono);
        this.suppliername = data["suppliername"];
        this.pono = pono;
        this.displayDD = false;
        this.displaylist = true;
        this.wmsService.getMaterialRequestlistdata(this.employee.employeeno, this.pono).subscribe(data => {
          this.materialList = data;
        });
        //this.requestMatData.suppliername = data["suppliername"];
      }

    }
    else {
      this.requestMatData.suppliername == null;
      this.displayDD = true;
      this.pono = null;
      this.displaylist = false;
      this.materialList[0].material = "";
      this.materialList[0].issuedqty = 0;
      this.materialList[0].materialcost = 0;
      this.materialList[0].availableqty = 0;
    }
  }

  //On close of drop down
  close() {
    this.suppliername = null;
    this.ponumber = null;
    this.requestMatData = new requestData();
    this.materialList = [];
    this.materialmodel = [];
  }

  //On supplier name selected
  onsuppSelected(suppname: string) {
    debugger;
    if (this.ponolist.filter(li => li.pono == suppname).length > 0) {
      if (suppname != "All") {
      this.ponumber = suppname;
      this.pono = suppname;
      this.displayDD = false;
      this.displaylist = true;
      this.wmsService.getMaterialRequestlistdata(this.employee.employeeno, this.pono).subscribe(data => {
        this.materialList = data;
      });
      }

    }
    else {
           this.requestMatData.pono == null;
        this.pono = null;
        this.displayDD = true;
      this.displaylist = false;
      this.materialList[0].material = "";
      this.materialList[0].issuedqty = 0;
      this.materialList[0].materialcost = 0;
      this.materialList[0].availableqty = 0;
    }

    //if (suppname == "undefined") {
    //  this.requestMatData.pono == null;
    //  this.pono = null;
    //  this.displayDD = true;
    //  this.displaylist = false;
    //}
    //else if (suppname != "All") {
    //  this.ponumber = suppname;
    //  this.pono = suppname;
    //  this.displayDD = false;
    //  this.displaylist = true;
    //  this.wmsService.getMaterialRequestlistdata(this.employee.employeeno, this.pono).subscribe(data => {
    //    this.materialList = data;
    //  });
    //}
    //else {

    //    this.requestMatData.pono == null;
    //    this.pono = null;
    //    this.displayDD = true;
    //    this.displaylist = false;
     
    //}
    //}
  }


  //Check for requested qty
  onComplete(reqqty: number, avqty: number, material: any, index) {
    if (avqty < reqqty) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Requested qty cannot exceed available qty' });
      this.materialList[index].quantity = 0;
      return false;
    }
  }

  //add materials for gate pass
  addNewMaterial() {
    if (this.materialList.length <= 0) {
      this.materialistModel = { material: "", materialdescription: "", quantity: 0, materialcost: 0, availableqty: 0, remarks: " ", issuedqty: 0, requesterid: this.employee.employeeno, ReserveUpto: this.mindate };
      this.materialList.push(this.materialistModel);
    }
    else {
      debugger;
      if (this.materialList[this.materialList.length - 1].material == "" || this.materialList[this.materialList.length - 1].material == null) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Add Material' });
        return false;
      }
      else if (this.materialList[this.materialList.length - 1].quantity == 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Quantity' });
        return false;
      }
      else if (this.materialList[this.materialList.length - 1].quantity > 0) {
        this.wmsService.checkMaterialandQty(this.materialList[this.materialList.length - 1].material, this.materialList[this.materialList.length - 1].quantity).subscribe(data => {
          if (data == "true") {
            //this.materialList.push(this.materialList);
            this.materialistModel = new materialListforReserve();

          }
          else
            this.messageService.add({ severity: 'error', summary: '', detail: data });
          return false;
        });
      }
      this.materialistModel = { material: "", materialdescription: "", quantity: 0, materialcost: 0, availableqty: 0, remarks: " ", issuedqty: 0, requesterid: this.employee.employeeno, ReserveUpto: this.mindate };
      this.materialList.push(this.materialistModel);
    }

  }

  //Check if material is already selected in material list drop down
  onMaterialSelected(material: any, index: any) {
    debugger;
    if (this.materialList.filter(li => li.material == material).length > 1) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Material already exist' });
      this.materialList[index].material = "";
      return false;
    }
    //add material price

    var data = this.materialmodel.find(li => li.material == material);
    console.log(data);
    //console.log(this.gatepasslistmodel);
    this.materialList[index].materialcost = data["materialcost"] != null ? data["materialcost"] : 0;
    this.materialList[index].availableqty = data["availableqty"] != null ? data["availableqty"] : 0;
  }


  //bind materials based search
  public bindSearchList(event: any, name?: string) {
    debugger;
    this.wmsService.getMaterialRequestlistdata(this.employee.employeeno, this.pono).subscribe(data => {


      this.searchresult = data;
      this.materialmodel = data;
      this.searchItems = [];
      var fName = "";
      this.searchresult.forEach(item => {
        debugger;
        fName = item[this.constants[name].fieldName];
        if (name == "material")
          //fName = item[this.constants[name].fieldName] + " - " + item[this.constants[name].fieldId];
          fName = item[this.constants[name].fieldId];
        // var value = { listName: name, name: fName, code: item[this.constants[name].fieldId] };
        var value = { code: item[this.constants[name].fieldId] };
        //this.materialistModel.materialcost = data[0].materialcost;
        //this.materialistModel.availableqty = data[0].availableqty;
        this.searchItems.push(item[this.constants[name].fieldId]);
      });
    });
  }
 
  //reserved quantity update
  onMaterialRequestDeatilsSubmit() {
    this.spinner.show();
    this.btnDisable = true;
    this.wmsService.materialRequestUpdate(this.reserveList).subscribe(data => {
      this.spinner.hide();
      if (data)
        this.messageService.add({ severity: 'success', summary: '', detail: 'Request sent' });
      else
        this.messageService.add({ severity: 'error', summary: '', detail: 'Update Failed' });

    });
  }

  //app
  ackStatusChanges() {
    this.showAck = true;
  }

  //Delate row
  removematerial(id: number, matIndex: number) {
    debugger;
    this.materialList.splice(matIndex, 1);
    //if (id != 0) {
    //  this.wmsService.deleteGatepassmaterial(id).subscribe(data => {
    //    //this.gatepassModelList[this.gpIndx].materialList.splice(matIndex, 1);
    //    this.messageService.add({ severity: 'success', summary: 'success Message', detail: 'Material Deleted' });
    //  });
    //}
  }


  //received material acknowledgement
  materialAckUpdate() {
    if (this.reserveList.filter(li => li.status == true).length == 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: 'Select atleast  one checkbox' });
    }
    else {
      this.spinner.show();
      this.btnDisable = true;
      this.wmsService.ackmaterialreceived(this.reserveList).subscribe(data => {
        this.spinner.hide();
        if (data)
          this.messageService.add({ severity: 'sucess', summary: '', detail: 'Status updated' });
        else
          this.messageService.add({ severity: 'error', summary: '', detail: 'Update Failed' });
      });
    }
  }

  getstatus(data: any) {
    if (data.ackstatus == 'received') {
      data.chkstatus = "Received";
    }
    return data.chkstatus;
  }

  Request() {
    let savedata = new materialReservetorequestModel();
    savedata.reserveid = parseInt(this.reserveidview);
    savedata.requestedby = this.employee.employeeno;
    this.spinner.show();
    this.wmsService.materialReservetorequest(savedata).subscribe(data => {
      this.spinner.hide();
      if (data) {
        this.AddDialog = false;
        this.showdialog = false;
        this.messageService.add({ severity: 'success', summary: '', detail: 'Requested Successfully' });
        this.getMaterialReservelist();
        //this.router.navigateByUrl("/WMS/MaterialReqView/" + this.pono);
      }
      else {
        this.spinner.hide();
        this.messageService.add({ severity: 'error', summary: '', detail: 'Error while Requesting materials' });
        this.btnreq = true;
      }

    });
  }

  //redirect to PM Dashboard
  backtoDashboard() {
    this.router.navigateByUrl("/WMS/Dashboard");
  }
  showmaterialdetails(reservedid, data: any) {
    debugger;
    this.showreservebtn = false;
    this.isrequested = false;
    this.AddDialog = true;
    this.showdialog = true;
    this.reserveduptoview = data.reserveupto;
    this.statusview = data.chkstatus; 
    this.reserveidview = reservedid;
    var today = new Date();
    var rsvdate = new Date(data.reserveupto);
    if (data.chkstatus == "Reserved" ) {
      this.showreservebtn = true;
    }
    if (data.chkstatus == "Requested") {
      this.requestedbyview = data.requestedby;
      this.requestedonview = data.requestedon;
      this.isrequested = true;
    }
    this.wmsService.getmaterialissueListforreserved(reservedid).subscribe(data => {
      this.materiallistData = data;
      
      if (data != null) {

      }
    });
  }
  Cancel() {
    this.AddDialog = false;
  }
  reserveMaterial() {
  
    this.requestDialog = true;
    this.btnreq = true;
    this.displayDD = true;
    this.pono = null;
    this.displaylist = false;
    var minDate = new Date();
    var maxdate = new Date(new Date().setDate(new Date().getDate() + 14));
    this.reservedfor = new Date();
    this.mindate = this.datePipe.transform(minDate, "yyyy-MM-dd");
    this.maxdate = this.datePipe.transform(maxdate, "yyyy-MM-dd");
    //Get PO number list, project list and materials available
    this.GetPONo();
    if (this.materialList.length <= 0) {
      this.materialistModel = { material: "", materialdescription: "", quantity: 0, materialcost: 0, remarks: " ", availableqty: 0, issuedqty: 0, requesterid: this.employee.employeeno, ReserveUpto: this.mindate };
      this.materialList.push(this.materialistModel);
    }
  }

  GetPONo() {
    debugger;
    this.wmsService.getPODetails(this.employee.employeeno).subscribe(data => {
      this.spinner.hide();
      if (data) {
        this.ponolist = data;
        console.log(this.ponolist)
      }

      else
        this.messageService.add({ severity: 'error', summary: '', detail: 'Unable to fetch PO data' });
    });
  }

  //checkValiddate
  //check date is valid or not
  checkValiddate(date: any) {
    try {
      if (!date || (this.datePipe.transform(date, this.constants.dateFormat) == "01/01/0001"))
        return "";
      else
        return this.datePipe.transform(date, this.constants.dateFormat);
    }
    catch{
      return "";
    }
  }

  //Submit Requested quantity data
  onSubmitReqData() {
    if (this.materialList.length <= 0) {
      this.messageService.add({ severity: 'error', summary: '', detail: "Add material to Request" });
    }
    else {
      debugger;
      if (this.materialList[this.materialList.length - 1].material == "" || this.materialList[this.materialList.length - 1].material == null) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Add Material' });
        return false;
      }
      else if (this.materialList[this.materialList.length - 1].quantity == 0) {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Quantity' });
        return false;
      }
      else if (this.materialList[this.materialList.length - 1].quantity > 0) {
        if (this.materialList[this.materialList.length - 1].availableqty < this.materialList[this.materialList.length - 1].quantity) {
          this.messageService.add({ severity: 'error', summary: '', detail: "Reserved Qty cannot exceed available qty" });
          this.materialList[this.materialList.length - 1].quantity = 0;
          return false;
        }
        else {
          //submit requested data
          this.spinner.show();
          this.btnreq = false;
          this.materialList.forEach(item => {
            item.requesterid = this.employee.employeeno;
            item.ReserveUpto = this.reservedfor;
            if (item.quantity == null)
              item.quantity = 0;
          })
          this.wmsService.materialReserveUpdate(this.materialList).subscribe(data => {
            this.spinner.hide();
            if (data) {
              this.requestDialog = false;
              this.btnreq = true;
              this.suppliername = null;
              this.ponumber = null;
              this.requestMatData = new requestData();
              this.materialList = [];
              this.materialmodel = [];
              this.messageService.add({ severity: 'success', summary: '', detail: 'Reserved materials Successfully' });
              this.getMaterialReservelist();
              //this.router.navigateByUrl("/WMS/MaterialReqView/" + this.pono);
            }
            else {
              
              this.messageService.add({ severity: 'error', summary: '', detail: 'Error while Reserving materials' });
              this.btnreq = true;
            }

          });
        }
      }
    }
  }

  parseDate(dateString: string): Date {
    if (dateString) {
      return new Date(dateString);
    }
    return null;
  }

}
