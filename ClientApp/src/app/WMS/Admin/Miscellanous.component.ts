import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, FormControl, ValidatorFn } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../../WmsServices/wms.service';
import { constants } from '../../Models/WMSConstants';
import { Employee, DynamicSearchResult, searchList } from '../../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { GPReasonMTdata } from '../../Models/WMS.Model';
import { ConfirmationService } from 'primeng/api';

@Component({
  selector: 'app-Miscellanous',
  templateUrl: './Miscellanous.component.html'
})
export class MiscellanousComponent implements OnInit {

  constructor(private formBuilder: FormBuilder, private ConfirmationService: ConfirmationService, private messageService: MessageService, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public employee: Employee;
  public GPData= new GPReasonMTdata();
  displaygpDialog: boolean = false;
  GPreasonList: Array<GPReasonMTdata> = [];
  displaygpeditDialog: boolean = false;


  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.getreasonlist();
  }

  opengpDialogue() {
    this.GPreasonList = [];
    this.displaygpDialog = true;
  }

  dialogCancel() {
    this.displaygpDialog = false;
  }

  //Get the list of reasons added for Miscellanous
  getreasonlist() {
    this.wmsService.getMiscellanousReasonData().subscribe(data => {
      this.GPreasonList = data;
    });
  }

  onReasonSubmit() {
    if (!this.GPData.reason) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Enter Miscellanous Reason' });
      return;
    }
    this.GPData.createdby = this.employee.employeeno;
    this.spinner.show();
    this.wmsService.MiscellanousReasonAdd(this.GPData).subscribe(data => {
      debugger;
      this.spinner.hide();
      this.displaygpDialog = false;
      if (data =="Success") {
        this.messageService.add({ severity: 'success', summary: '', detail: 'Miscellanous Reason added successfully' });
        this.getreasonlist();
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Error while adding Miscellanous Reason' });
      }

    });
  }

  editreason(reason: any) {
    debugger;
    this.displaygpeditDialog = true;
    this.GPData.reasonid = reason.reasonid;
    this.GPData.reason = reason.reason;
  }

  deletereason(reason:any) {
    this.GPData.reasonid = reason.reasonid;
    this.GPData.reason = reason.reason;
    this.GPData.createdby = this.employee.employeeno;
    this.ConfirmationService.confirm({
      message: 'Are you sure you want to delete this deletereason Reason '+reason.reason,
      header: 'Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {

        this.spinner.show();
        this.wmsService.GPReasonDelete(this.GPData).subscribe(data => {
          debugger;
          this.spinner.hide();
          this.displaygpDialog = false;
          if (data == "Success") {
            this.messageService.add({ severity: 'success', summary: '', detail: 'deletereason Reason Deleted successfully' });
            this.getreasonlist();
          }
          else {
            this.messageService.add({ severity: 'error', summary: '', detail: 'Error while Deleting deletereason Reason' });
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

  onReasonUpdate(GPData: any) {
    if (!this.GPData.reason) {
      this.messageService.add({ severity: 'error', summary: 'Error Message', detail: 'Enter Miscellanous Reason' });
      return;
    }
    this.GPData.createdby = this.employee.employeeno;
    this.spinner.show();
    this.wmsService.MiscellanousReasonAdd(this.GPData).subscribe(data => {
      debugger;
      this.spinner.hide();
      this.displaygpeditDialog = false;
      if (data == "Success") {
        this.messageService.add({ severity: 'success', summary: '', detail: 'Miscellanous Reason Updated successfully' });
        this.getreasonlist();
      }
      else {
        this.messageService.add({ severity: 'error', summary: '', detail: 'Error while Updating Miscellanous Reason' });
      }

    });
  }

  }
