import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { gatepassModel, materialistModel } from '../Models/WMS.Model';
import { isNullOrUndefined } from 'util';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-GatePass',
  templateUrl: './GatePass.component.html',
  providers: [DatePipe]
})
export class GatePassComponent implements OnInit {
  constructor(private formBuilder: FormBuilder, private messageService: MessageService, private datePipe: DatePipe, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }
  todayDate = this.datePipe.transform(new Date(), 'yyyy-MM-dd');
  public formName: string;
  public txtName; GatepassTxt: string;
  public dynamicData = new DynamicSearchResult();
  public showList: boolean = false;
  public searchItems: Array<searchList> = [];
  public selectedlist: Array<searchList> = [];
  public searchresult: Array<object> = [];

  public gatepasslist: Array<any> = [];
  public totalGatePassList: Array<any> = [];
  public gatepassModelList: Array<gatepassModel> = [];
  public employee: Employee;
  public gatepassdialog; updateReturnedDateDialog: boolean = false;
  public gatepassModel: gatepassModel;
  public materialistModel: materialistModel;
  public material: any;
  public gpIndx: number;
  public date: Date = null;
  public approverstatus: string;

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
    this.gatepassModel = new gatepassModel();
    this.materialistModel = new materialistModel();
    this.getGatePassList();
    this.GatepassTxt = "Gate Pass - Request Materials"

    if (this.employee.roleid == "8") //for approver
      this.approverstatus = "Pending";

    else
      this.approverstatus = "";
  }


  //Adding new material - Gayathri
  addNewMaterial() {

    if (this.gatepassModel.materialList.length == 0 || isNullOrUndefined(this.material)) {
      this.materialistModel = { materialid: "", gatepassmaterialid: "0", materialdescription: "", quantity: 0, materialcost: "0", remarks: " ", expecteddate: this.date, returneddate: this.date };
      this.gatepassModel.materialList.push(this.materialistModel);
      this.material = "";
    }
    //check if materiallist is empty and gatepass materialid is null
    else if (!this.material && !this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].materialid) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'please Add Material' });
      return false;
    }
    else if (this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].expecteddate == undefined) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Please select Expected Date' });
      return false;
    }
    else {
      //Check if material code is already entered
      if (this.gatepassModel.materialList.filter(li => li.materialid == this.material.code && li.gatepassmaterialid != "0").length > 0) {
        this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Material already exist' });
        return false;
      }


      this.gatePassChange();
      if (this.material.code) {
        this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].materialid = this.material.code;
        this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].materialdescription = this.material.name;
        this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].expecteddate = new Date(this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].expecteddate).toLocaleDateString();
        this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].returneddate = this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].returneddate != null ? new Date(this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].returneddate).toLocaleDateString() : undefined;

        // this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].returneddate = this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].expecteddate;
      }
      if (this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].materialid && this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].quantity) {
        this.wmsService.checkMaterialandQty(this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].materialid, this.materialistModel.quantity).subscribe(data => {
          if (data == "true") {

            this.materialistModel = { materialid: "", gatepassmaterialid: "0", materialdescription: "", quantity: 0, materialcost: "0", remarks: " ", expecteddate: this.date, returneddate: this.date };
            this.gatepassModel.materialList.push(this.materialistModel);
            this.material = "";

            //alert(this.gatepassModel.materialList[1].materialid);
          }
          else
            this.messageService.add({ severity: 'error', summary: 'Error Message', detail: data });
        });
      }
      else {
        if (!this.materialistModel.materialid)
          this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'select material from list' });
        else if (!this.materialistModel.quantity)
          this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Enter Quantity' });
      }


    }



  }


  //bind materials based search
  public bindSearchListData(event: any, name?: string) {
    var searchTxt = event.query;
    if (searchTxt == undefined)
      searchTxt = "";
    searchTxt = searchTxt.replace('*', '%');
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.tableName = this.constants[name].tableName + " ";
    this.dynamicData.searchCondition = "" + this.constants[name].condition;
    this.dynamicData.searchCondition += "sk.materialid" + " ilike '" + searchTxt + "%'" + " and sk.availableqty>=1";
    this.materialistModel.materialcost = "";
    this.wmsService.GetMaterialItems(this.dynamicData).subscribe(data => {
      this.searchresult = data;
      this.searchItems = [];
      var fName = "";
      this.searchresult.forEach(item => {
        fName = item[this.constants[name].fieldName];
        if (name == "ItemId")
          //fName = item[this.constants[name].fieldName] + " - " + item[this.constants[name].fieldId];
          fName = item[this.constants[name].fieldId];
        var value = { listName: name, name: fName, code: item[this.constants[name].fieldId] };
        this.materialistModel.materialcost = data[0].materialcost;
        this.searchItems.push(value);
      });
    });
  }

  //get gatepass list
  getGatePassList() {
    this.wmsService.getGatePassList().subscribe(data => {
      this.totalGatePassList = data;
      if (this.employee.roleid == "8") {
        this.gatepasslist = this.totalGatePassList.filter(li => li.gatepasstype == 'Non Returnable' && li.approverstatus == this.approverstatus && li.approverstatus == null);
      }
      else {
        this.gatepasslist = this.totalGatePassList;
      }
      this.gatepassModelList = [];
      this.prepareGatepassList();
    });
  }

  searchGatePassList() {
    if (this.approverstatus != "0") {
      if (this.approverstatus == "Pending")
        this.gatepasslist = this.totalGatePassList.filter(li => li.gatepasstype == 'Non Returnable' && li.approverstatus == this.approverstatus &&  li.approverstatus == null);
        else
        this.gatepasslist = this.totalGatePassList.filter(li => li.gatepasstype == 'Non Returnable' &&  li.approverstatus == this.approverstatus);
    }
    else
      this.gatepasslist = this.totalGatePassList.filter(li => li.gatepasstype == 'Non Returnable');
    this.gatepassModelList = [];
    this.prepareGatepassList();
  }
  exportPdf() {
    debugger;
    import("jspdf").then(jsPDF => {
      import("jspdf-autotable").then(x => {
        const doc = new jsPDF.default(0, 0);
        var device = "";
        var model = "";
        var make = "";
        var pageHeight = doc.internal.pageSize.height || doc.internal.pageSize.getHeight();
        var pageWidth = doc.internal.pageSize.width || doc.internal.pageSize.getWidth();
        doc.setDrawColor(0);
        doc.setFillColor(255, 255, 255);
        doc.rect(10, 10, 190, 52, 'FD');
        var img = new Image()
        img.src = './assets/banner1.jpg'
        doc.addImage(img, 'jpg', 12, 12, 40, 15);
        doc.setFontSize(14);
        doc.text("YOKOGAWA INDIA LIMITED ", 120, 17);
        doc.setFontSize(12);
        doc.text("Plot No. 96, 3rd Cross, Hosur Road", 120, 22);
        doc.setFontSize(12);
        doc.text("Electronic city, Banglore - 560100", 120, 27);
        doc.line(10, 30, 200, 30);
        doc.text("Gate Pass [Type] Items", pageWidth / 2, 36, 'center');
        doc.line(10, 40, 200, 40);
        doc.text("Name of the Person : [Name] ", 12, 45);
        doc.text("No: [No] ", 125, 45);
        doc.text("Representing: [Representing] ", 12, 52);
        doc.text("Document Ref Date: [Date] ", 12, 59);


        //var reportTitle = "Device Report" + device + model + make;
        //var splitTitle = doc.splitTextToSize(reportTitle, 180);
        //if (device == "" && model == "" && make == "") {
        //  doc.text(splitTitle, 85, 10);
        //}
        //else {
        //  doc.text(splitTitle, 20, 10);
        //}

        //doc.autoTable({
        //  styles: { fillColor: [255, 0, 0] },
        //  columnStyles: { 0: { halign: 'center', fillColor: [0, 255, 0] } }, // Cells in first column centered and green
        //  margin: { top: 10 },
        //})

        //doc.autoTable(this.exportColumns, this.PrintDocs);
        // doc.save('devicereport.pdf');
        doc.output('dataurlnewwindow');
      })
    })
  }

  //prepare list based on gate pass id
  prepareGatepassList() {
    this.gatepasslist.forEach(item => {
      var res = this.gatepassModelList.filter(li => li.gatepassid == item.gatepassid);
      if (res.length == 0) {
        item.materialList = [];
        var result = this.gatepasslist.filter(li => li.gatepassid == item.gatepassid && li.gatepassmaterialid != "0" && li.deleteflag == false);
        for (var i = 0; i < result.length; i++) {
          var material = new materialistModel();
          material.gatepassmaterialid = result[i].gatepassmaterialid;
          material.materialid = result[i].materialid;
          material.materialdescription = result[i].materialdescription;
          material.quantity = result[i].quantity;
          material.materialcost = result[i].materialcost;
          material.remarks = result[i].remarks;
          //material.approverstatus = result[i].approverstatus;
          material.expecteddate = new Date(result[i].expecteddate);
          if (isNullOrUndefined(result[i].returneddate)) {
            //material.returneddate = new Date(this.date).toLocaleDateString();
            material.returneddate = this.checkValiddate(result[i].returneddate) == "" ? undefined : this.checkValiddate(result[i].returneddate);
          }
          else {

            material.returneddate = this.checkValiddate(result[i].returneddate) == "" ? undefined : this.checkValiddate(result[i].returneddate);
          }

          item.materialList.push(material);
        }

        this.gatepassModelList.push(item);
      }
    });
  }

  //gatepass change
  gatePassChange() {
    if (this.gatepassModel.gatepasstype != "0")
      this.GatepassTxt = this.gatepassModel.gatepasstype + " - " + "Request Materials";
  }
  //open gate pass dialog
  openGatepassDialog(gatepassobject: any, gpIndx: any, dialog) {
    this[dialog] = true;
    this.gatepassModel = new gatepassModel();
    if (gatepassobject) {

      this.gpIndx = gpIndx;
      this.gatepassModel = gatepassobject;
      //this.materialistModel = { materialid: "", gatepassmaterialid: "0", materialdescription: "", quantity: 0, materialcost: "0", remarks: " ", expecteddate: this.date, returneddate: this.date };
      //this.gatepassModel.materialList.push(this.materialistModel);
      //this.material = "";
    } else {
      this.gatepassModel.gatepasstype = "0";
      this.gatepassModel.reasonforgatepass = "0";
      this.materialistModel = { materialid: "", gatepassmaterialid: "0", materialdescription: "", quantity: 0, materialcost: "0", remarks: " ", expecteddate: this.date, returneddate: this.date };
      this.gatepassModel.materialList.push(this.materialistModel);
      this.material = "";
    }
    this.gatePassChange();
  }

  //add materials for gate pass
  addMaterial() {
    if (!this.material) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Add Material' });
      return false;
    }
    if (this.gatepassModel.materialList.filter(li => li.materialid == this.material.code).length > 0) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Material already exist' });
      return false;
    }
    this.gatePassChange();
    this.materialistModel.materialid = this.material.code;
    this.materialistModel.materialdescription = this.material.name;
    if (this.materialistModel.materialid && this.materialistModel.quantity) {
      this.wmsService.checkMaterialandQty(this.material.code, this.materialistModel.quantity).subscribe(data => {
        if (data == "true") {
          this.gatepassModel.materialList.push(this.materialistModel);
          this.materialistModel = new materialistModel();
          this.material = "";
        }
        else
          this.messageService.add({ severity: 'error', summary: 'Error Message', detail: data });
      });
    }
    else {
      if (!this.materialistModel.materialid)
        this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'select material from list' });
      else if (!this.materialistModel.quantity)
        this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Enter Quantity' });
    }

  }

  //check date is valid or not
  checkValiddate(date: any) {
    try {
      if (!date || (this.datePipe.transform(date, this.constants.dateFormat) == "01/01/0001"))
        return "";
      else
        return date;
    }
    catch{
      return "";
    }
  }

  //check material and quntity availablity in stock
  checkMaterialandQty(material: any, matIndex: number) {
    if (material.quantity) {
      this.wmsService.checkMaterialandQty(material.code, material.quantity).subscribe(data => {
        if (data == "true") {
        }
        else {
          this.gatepassModelList[this.gpIndx].materialList[matIndex].quantity = 1;
          this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Quantity Exceeded' });
        }
      });
    }
  }

  //Delete material for gatepass
  removematerial(id: number, matIndex: number) {

    this.gatepassModel.materialList.splice(matIndex, 1);
    if (id != 0) {
      this.wmsService.deleteGatepassmaterial(id).subscribe(data => {
        //this.gatepassModelList[this.gpIndx].materialList.splice(matIndex, 1);
        this.messageService.add({ severity: 'success', summary: 'success Message', detail: 'Material Deleted' });
      });
    }


  }

  //Check if material is already selected in material list drop down
  onMaterialSelected(material: any) {
    if (this.gatepassModel.materialList.filter(li => li.materialid == material.code).length > 0) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Material already exist' });
      return false;
    }
  }

  //update return dated based on role
  updateReturnedDate(gatepassobject: any) {
    this.gatepassModel = new gatepassModel();
    if (gatepassobject) {
      this.gatepassModel = gatepassobject;
      this.onSubmitgatepassData();
    }
  }


  //saving gatepass details
  onSubmitgatepassDetails() {
    if (this.gatepassModel.gatepasstype != "0") {
      this.gatepassModel.requestedby = this.employee.employeeno;
      this.wmsService.saveoreditgatepassmaterial(this.gatepassModel).subscribe(data => {
        this.gatepassdialog = false;
        this.updateReturnedDateDialog = false;
        this.getGatePassList();
        if (data)
          this.messageService.add({ severity: 'success', summary: 'Success Message', detail: 'Data Saved' });
      })
    }
    else
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Select Type' });
  }

  //saving gatepass details --Gayathri
  onSubmitgatepassData() {
    if (this.gatepassModel.materialList.length == 0) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Please Add materials to create GatePass' });
      return false;
    }
    //else if (this.gatepassModel.materialList.length >= 1 && !this.material)
    //{
    //  this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Please Add materials to create GatePass' });
    //  return false;
    //}
    else if (this.gatepassModel.gatepasstype != "0") {
      this.gatepassModel.requestedby = this.employee.employeeno;
      //check if materiallist is empty and gatepass materialid is null
      if (!this.material && !this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].materialid) {
        this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Add Material' });
        return false;
      }
      else if (this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].expecteddate == undefined) {
        this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Please select Expected Date' });
        return false;
      }
      else if (this.gatepassModel.materialList.filter(li => li.materialid == this.material.code && li.gatepassmaterialid != "0").length > 0) {
        //alert("entered");
        //console.log(this.gatepassModel);
        this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Material already exist' });
        return false;
      }
      //alert(this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].returneddate);
      this.gatePassChange();
      if (this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].materialid == "" && !isNullOrUndefined(this.material.code)) {
        this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].materialid = this.material.code;
        this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].materialdescription = this.material.name;
        this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].expecteddate = new Date(this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].expecteddate).toLocaleDateString();
        this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].returneddate = this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].returneddate != null ? new Date(this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].returneddate).toLocaleDateString() : undefined;

        if (this.materialistModel.materialid && this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].quantity) {
          this.wmsService.checkMaterialandQty(this.material.code, this.materialistModel.quantity).subscribe(data => {
            if (data == "true") {
              // this.gatepassModel.materialList.push(this.materialistModel);
              //this.gatepassModel.materialList.push(this.materialistModel);
              //this.materialistModel = new materialistModel();
              this.material = "";
              this.wmsService.saveoreditgatepassmaterial(this.gatepassModel).subscribe(data => {
                this.gatepassdialog = false;
                this.updateReturnedDateDialog = false;
                this.getGatePassList()
                if (data)
                  this.messageService.add({ severity: 'success', summary: 'Success Message', detail: 'Gate Pass Created Successfully' });
              })
            }
            else
              this.messageService.add({ severity: 'error', summary: 'Error Message', detail: data });
          });
        }
        else {
          if (!this.materialistModel.materialid)
            this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'select material from list' });
          else if (!this.materialistModel.quantity)
            this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Enter Quantity' });
        }
      }
      else {

        console.log(this.gatepassModel);
        //alert(this.gatepassModel.materialList[this.gatepassModel.materialList.length - 1].returneddate);
        this.wmsService.saveoreditgatepassmaterial(this.gatepassModel).subscribe(data => {
          this.gatepassdialog = false;
          this.updateReturnedDateDialog = false;
          this.getGatePassList();
          if (data)
            this.messageService.add({ severity: 'success', summary: 'Success Message', detail: 'Gate Pass Created Successfully' });
        })
      }


    }

    else {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Select Type' });
    }


  }

  //showing print page
  showprint(gatepassobject: gatepassModel) {
    //this.exportPdf();
    this.router.navigate(['/WMS/GatePassPrint', gatepassobject.gatepassid]);
  }


}
