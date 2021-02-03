import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl, ValidatorFn } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../../WmsServices/wms.service';
import { constants } from '../../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { GPReasonMTdata, PlantMTdata } from '../../Models/WMS.Model';
import { ConfirmationService } from 'primeng/api';

@Component({
  selector: 'app-PlantMaster',
  templateUrl: './PlantMaster.component.html'
})
export class PlantMasterComponent implements OnInit {

  constructor(private formBuilder: FormBuilder, private ConfirmationService: ConfirmationService, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public employee: Employee;
  public plantData = new PlantMTdata();
  displaygpDialog: boolean = false;
  plantList: Array<PlantMTdata> = [];
  displaygpeditDialog: boolean = false;


  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.getplantnamelist();
  }

  opengpDialogue() {
    this.displaygpDialog = true;
  }

  dialogCancel() {
    this.displaygpDialog = false;
  }

  //Get the list of plant names
  getplantnamelist() {
    this.wmsService.getplantnameData().subscribe(data => {
      this.plantList = data;
    });
  }

  onplantnameSubmit() {
    if (!this.plantData.plantname) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Enter Plant Name' });
      return;
    }
    this.plantData.createdby = this.employee.employeeno;
    this.spinner.show();
    this.wmsService.createplant(this.plantData).subscribe(data => {
      debugger;
      this.spinner.hide();
      this.displaygpDialog = false;
      if (data =="Success") {
        this.messageService.add({ severity: 'success', summary: '', detail: 'New Plant created successfully' });
        this.getplantnamelist();
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Error while creating new plant' });
      }

    });
  }

  editreason(reason: any) {
    debugger;
    this.displaygpeditDialog = true;
    this.plantData.plantid = reason.plantid;
    this.plantData.plantname = reason.plantname;
 
  }

  deletereason(reason: any) {
    this.plantData.plantid = reason.plantid;
    this.plantData.plantname = reason.plantname;
    this.plantData.createdby = this.employee.employeeno;
    this.ConfirmationService.confirm({
      message: 'Are you sure you want to delete this plant name:' + reason.plantname,
      header: 'Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {

        this.spinner.show();
        this.wmsService.PlantnameDelete(this.plantData).subscribe(data => {
          debugger;
          this.spinner.hide();
          this.displaygpDialog = false;
          if (data == "Success") {
            this.messageService.add({ severity: 'success', summary: '', detail: 'Plant Deleted successfully' });
            this.getplantnamelist();
          }
          else {
            this.messageService.add({ severity: 'error', summary: '', detail: 'Error while Deleting Plant' });
          }

        });
      },
      reject: () => {

        //this.messageService.add({ severity: 'info', summary: 'Ignored', detail: 'You have ignored' });
      }
    });



   
  }

  dialoguCancel() {
    this.displaygpeditDialog = false;
  }

  onReasonUpdate(plantData: any) {
    if (!this.plantData.plantname) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Enter Plant Name' });
      return;
    }
    this.plantData.createdby = this.employee.employeeno;
    this.spinner.show();
    this.wmsService.createplant(this.plantData).subscribe(data => {
      debugger;
      this.spinner.hide();
      this.displaygpeditDialog = false;
      if (data == "Success") {
        this.messageService.add({ severity: 'success', summary: '', detail: 'Plant name Updated successfully' });
        this.getplantnamelist();
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Error while Updating Plant name' });
      }

    });
  }

  }
