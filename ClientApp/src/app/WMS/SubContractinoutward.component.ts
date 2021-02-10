import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { constants } from '../Models/WMSConstants';
import { Employee } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { DatePipe } from '@angular/common';
import { ConfirmationService } from 'primeng/api';
import { outwardmaterialistModel } from '../Models/WMS.Model';

@Component({
	selector: 'app-SubContractinoutward',
	templateUrl: './SubContractinoutward.component.html'
})

export class SubContractComponent implements OnInit {
	constructor(private ConfirmationService: ConfirmationService, private formBuilder: FormBuilder, private messageService: MessageService, private datePipe: DatePipe, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }
	public todayDate = this.datePipe.transform(new Date(), 'yyyy-MM-dd');
	public employee: Employee;
	public Subcontractlist: Array<any>;
	public returntype: string;
	public isoutward: boolean = false;
	public isinward: boolean = false;
	public datetype: string = "";
	public DGsubcontractid: string = "";
	public DGvendorname: string = "";
	public pgremarks: string = "";
	public fromdateview: string = "";
	public fromdateview1: string = "";
	public materialListDG: outwardmaterialistModel[] = [];
	public selectedmdata: outwardmaterialistModel[] = [];
	public showmatDialog: boolean = false;

	ngOnInit() {
		if (localStorage.getItem("Employee"))
			this.employee = JSON.parse(localStorage.getItem("Employee"));
		else
			this.router.navigateByUrl("Login");
		this.returntype = "out";
		var dt = new Date();
		this.fromdateview = this.datePipe.transform(dt, this.constants.dateFormat);
		this.fromdateview1 = this.datePipe.transform(dt, 'yyyy-MM-dd hh:mm:ss');
		this.getdata();
	}

	getdata() {
		debugger;
		if (this.returntype == "out") {
			this.getoutwarddata();

		}
		else if (this.returntype == "in") {
			this.getinwarddata();
		}
	}
	getoutwarddata() {
		this.isoutward = true;
		this.isinward = false;
		this.datetype = "Outward date";
		this.getSubcontractList();
	}
	getinwarddata() {
		this.isoutward = false;
		this.isinward = true;
		this.datetype = "Inward date";
		this.Subcontractlist = [];
		this.getSubcontractList();
	}

	closeDG() {
		this.showmatDialog = false;
	}

	getSubcontractList() {
		this.Subcontractlist = [];
		this.spinner.show();
		this.wmsService.subcontractInoutList().subscribe(data => {
			this.spinner.hide();
			if (data) {
				this.Subcontractlist = data;
				if (this.returntype == "out")
					this.Subcontractlist = this.Subcontractlist.filter(li => li.outwarddate == null);
				else
					this.Subcontractlist = this.Subcontractlist.filter(li => li.outwarddate != null);
      }
		});
	}
	showdetails(data: any) {
		var dt = new Date();
		this.fromdateview = this.datePipe.transform(dt, this.constants.dateFormat);
		this.DGsubcontractid = data.transferid;
		this.DGvendorname = data.vendorname;
    var res = this.Subcontractlist.filter(li => li.transferid == data.transferid);
		this.materialListDG = JSON.parse(JSON.stringify(res)) as outwardmaterialistModel[];
		this.showmatDialog = true;

	}
	updateoutinward() {
		var senddata = this.materialListDG;
		this.selectedmdata = [];
		var tdate = new Date();
		if (this.isoutward) {

		}
		else {
			var invalidrcv = senddata.filter(function (element, index) {
				return (element.inwardqty != 0);
			});
			if (invalidrcv.length == 0) {
				this.messageService.add({ severity: 'error', summary: '', detail: 'Enter Inward Quantity' });
				return;
			}
			var invalidrcv1 = senddata.filter(function (element, index) {
				return (element.inwardqty > (element.outwardedqty - element.inwardedqty));
			});
			if (invalidrcv1.length > 0) {
				this.messageService.add({ severity: 'error', summary: '', detail: 'Intward quantity cannot exceed Pending quantity.' });
				return;
			}
			senddata = senddata.filter(function (element, index) {
				return (element.inwardqty != 0);
			});

		}
		senddata.forEach(item => {
			let mdata = new outwardmaterialistModel();
			mdata.gatepassmaterialid = item.gatepassmaterialid;
			mdata.gatepassid = this.DGsubcontractid;
			mdata.remarks = this.pgremarks;
			mdata.movedby = this.employee.employeeno;
			mdata.type = "SubContract";
			var date = this.datePipe.transform(tdate, 'yyyy-MM-dd hh:mm:ss');
			if (this.isoutward) {
				mdata.outwarddatestring = this.fromdateview1;
				mdata.movetype = "out";
				mdata.outwardqty = item.issuedqty;
			}
			else if (this.isinward) {
				mdata.inwarddatestring = this.fromdateview1;
				mdata.movetype = "in";
				mdata.inwardqty = item.inwardqty;
				mdata.remarks = item.remarks
			}
			this.selectedmdata.push(mdata);
		})
		this.wmsService.updateoutinward(this.selectedmdata).subscribe(data => {
			this.pgremarks = "";
			if (this.isinward) {
				this.messageService.add({ severity: 'success', summary: ' ', detail: 'Inwarded successfully.' });
			}
			else if (this.isoutward) {
				this.messageService.add({ severity: 'success', summary: ' ', detail: 'Outwarded successfully.' });
			}
			this.showmatDialog = false;
			this.getSubcontractList();

		})
	}


	onfromSelectMethod(event) {
		this.fromdateview1 = "";
		if (event.toString().trim() !== '') {
			this.fromdateview1 = this.datePipe.transform(event, 'yyyy-MM-dd hh:mm:ss');
		}
	}
	checkoutqty(enrtered: number, qty: number, out: number, data: any) {
		if (enrtered < 0) {
			this.messageService.add({ severity: 'error', summary: '', detail: 'Negative value not allowed.' });
			data.outwardqty = 0;
			return;
		}

		if (enrtered > (qty - out)) {
			this.messageService.add({ severity: 'error', summary: '', detail: 'Outward quantity cannot be greater than Pending quantity.' });
			data.outwardqty = 0;
			return;
		}

	}

	checkinqty(enrtered: number, qty: number, inqty: number, data: any) {
		if (enrtered < 0) {
			this.messageService.add({ severity: 'error', summary: '', detail: 'Negative value not allowed.' });
			data.outwardqty = 0;
			return;
		}

		if (enrtered > (qty - inqty)) {
			this.messageService.add({ severity: 'error', summary: '', detail: 'Intward quantity cannot be greater than Pending quantity.' });
			data.outwardqty = 0;
			return;
		}
	}
}
