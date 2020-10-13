import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { Employee} from '../Models/Common.Model';
import { gatepassModel } from '../Models/WMS.Model';
import { wmsService } from '../WmsServices/wms.service';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-Mailresponse',
  templateUrl: './Mailresponse.component.html'
})
export class MailresponseComponent implements OnInit {
  constructor(private router: Router, private messageService: MessageService, private wmsService: wmsService) { }
  public employee: Employee;
  gatepass: gatepassModel;
  //page load event
  ngOnInit() {
    this.gatepass = new gatepassModel();
    this.gatepass.approverid = '400104';
    this.gatepass.approverstatus = "Approved";
    this.gatepass.gatepassid = "182";
    this.gatepass.categoryid = 1;
    this.approvereject();
  }

  approvereject() {
    this.wmsService.GatepassapproveByMail(this.gatepass).subscribe(data => {
      this.messageService.add({ severity: 'success', summary: 'success Message', detail: 'Gate Pass Approved' });
    });


  }
  gotoapp() {
    location.reload();
  }
  close() {
    open(location.href, '_self').close();
   
  }
}


