import { Component, OnInit, ViewChild } from '@angular/core';
import { MenuItem } from 'primeng/api/menuitem';
import { Employee, userAcessNamesModel, rbamaster } from '../Models/Common.Model';
import { Router, RouterEvent, NavigationEnd, ActivatedRoute } from '@angular/router';
import { filter } from 'rxjs/operators';
import { wmsService } from '../WmsServices/wms.service';
import { environment } from 'src/environments/environment'
import { pageModel, ddlmodel } from '../Models/WMS.Model';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent implements OnInit {
  constructor(private router: Router, private route: ActivatedRoute, private wmsService: wmsService) { }
  @ViewChild('op1', { static: false }) overlaymodel;
  loggedin: boolean = false;
  items: MenuItem[];
  imgurl = environment.profileimgUrl;
  tempitems: MenuItem[];
  otheritems: MenuItem[];
  useritems: MenuItem[];
  pagelist: pageModel[] = [];
  notificationitems: MenuItem[];
  username: string = "";
  cars: any[];
  emp = new Employee();
  loggeduserdata: Employee[] = [];
  ismailurl: boolean = false;
  isapprovalurl: boolean = false;
  urlrequstedpage: string = "";
  public totalGatePassList: Array<any> = [];
  public gatepassData: Array<any> = [];
  public gatepassData1: Array<any> = [];
  public gatepasslist: Array<any> = [];
  public gatepasslist1: Array<any> = [];
  userrolelist: userAcessNamesModel[] = [];
  approverstatus: string = "";
  notifcount: number = 0;
  notif: boolean = false;
  rbalist: rbamaster[] = [];
  menuview: boolean = false;
  btntext: string = "Menu"
  rolename: string = "";
  profileimage: string = "";
  loggedinas: string = "";
  selectedrolename: string = "";
  //Emails
  poinvoice: string = "";
  grnno: string = "";
  reqid: string = "";
  pono: string = "";
  gateid: string = "";
  fmgateid: string = "";
  gateentryid: string = "";
  inwmasterid: string = "";
  transferid: string = "";
  ngOnInit() {
    debugger;
    this.cars = [
      { label: 'Newest First', value: '!year' }
    ];
    this.notificationitems = [];
    this.loggeduserdata = [];
    this.approverstatus = "Pending";
    this.selectedrolename = "";
    this.transferid = "";
    var eurl = window.location.href;
    if (eurl.includes("/key")) {
      this.ismailurl = true;
      var urlvals = eurl.split('/');
      this.urlrequstedpage = urlvals[urlvals.length - 2];
      localStorage.setItem('requestedpage', this.urlrequstedpage);
    }

    if (eurl.includes("/appr")) {
      this.isapprovalurl = true;
      let element: HTMLElement = document.getElementById("btnuser") as HTMLElement;
      element.hidden = true;
      let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
      element1.hidden = true;
      this.router.navigateByUrl("WMS/Mailresponse");
      return;
    }
    //Purpose: << Security Operator >>


    if (localStorage.getItem("Employee")) {
      if (localStorage.getItem("rbalist")) {
        this.rbalist = JSON.parse(localStorage.getItem("rbalist")) as rbamaster[];
      }

      this.loggedin = true;
      let element: HTMLElement = document.getElementById("btnuser") as HTMLElement;
      element.hidden = false;
      this.emp = JSON.parse(localStorage.getItem("Employee")) as Employee;
      this.profileimage = this.imgurl + this.emp.employeeno + ".jpg";
      this.loggeduserdata.push(this.emp);
      if (localStorage.getItem("pages")) {
        this.pagelist = JSON.parse(localStorage.getItem("pages")) as pageModel[];
      }
      if (this.ismailurl && this.emp.roleid != "8") {
        this.logout();
      }
      this.username = this.emp.name;
      if (localStorage.getItem("userroles") && this.emp.roleid == "0") {
        this.userrolelist = JSON.parse(localStorage.getItem("userroles")) as userAcessNamesModel[];
        this.bindMenuwithoutrole();
      }
      else {
        if (localStorage.getItem("userroles")) {
          this.userrolelist = JSON.parse(localStorage.getItem("userroles")) as userAcessNamesModel[];
        }
        else {
          this.userrolelist = JSON.parse(localStorage.getItem("allroles")) as userAcessNamesModel[];
        }
        var rid = this.emp.roleid;
        var data1 = this.userrolelist.filter(function (element, index) {
          return (element.roleid == parseInt(rid));
        });
        if (data1.length > 0) {
          this.rolename = data1[0].accessname;
          if (this.rolename) {
            this.loggedinas = "Logged in as " + this.rolename;
          }

        }
        if (sessionStorage.getItem("userdashboardpage")) {
          this.binduserdashboardmenu();
        }
        else {
          if (eurl.includes("/Email")) {
            this.bindMenuForEmailNav(eurl);
          }
          else {
            this.bindMenu();
          }

        }

      }

      //if (this.emp.roleid != "8") {
      //  let elementx: HTMLElement = document.getElementById("btnuser1") as HTMLElement;
      //  elementx.hidden = false;
      //  let elementy: HTMLElement = document.getElementById("btnuser2") as HTMLElement;
      //  elementy.hidden = false;
      //  this.getGatePassList();
      //}

    }
    else {
      let element: HTMLElement = document.getElementById("btnuser") as HTMLElement;
      element.hidden = true;
      let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
      element1.hidden = true;
      //let elementx: HTMLElement = document.getElementById("btnuser1") as HTMLElement;
      //elementx.hidden = true;
      //let elementy: HTMLElement = document.getElementById("btnuser2") as HTMLElement;
      //elementy.hidden = true;
      this.router.navigateByUrl("WMS/Login");
    }




  }

  bindMenuForEmailNav(eurl: any) {
    if (eurl.includes("/Email")) {
      //this.isapprovalurl = true;
      this.gateentryid = this.route.snapshot.queryParams.pono;
      this.gateentryid = eurl.split('=')[1];

      let element: HTMLElement = document.getElementById("btnuser") as HTMLElement;
      element.hidden = false;
      let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
      element1.hidden = false;
      if (eurl.includes("/Email/GRNPosting?GateEntryNo")) {
        this.inwmasterid = this.route.snapshot.queryParams.gateentryid;
        if (this.gateentryid) {

          //redirects to Receipts page 
          this.bindMenuForEmail();
        }


      }
      //Purpose: << Receipts >>

      if (eurl.includes("/Email/GRNPosting?GRNo")) {
        debugger;
        this.grnno = this.route.snapshot.queryParams.grnno;
        this.grnno = eurl.split('=')[1];
        if (this.grnno) {
          //redirects to Receipts page 
          this.bindMenuForEmail();
        }
      }

      //Purpose: << Quality check >>

      if (eurl.includes("/Email/QualityCheck?GRNo")) {
        debugger;
        //this.grnno = this.route.snapshot.queryParams.grnnumber;
        this.grnno = this.route.snapshot.queryParams.grnnumber;
        this.grnno = eurl.split('=')[1];
        if (this.grnno) {
          //redirects to QualityCheck
          this.bindMenuForQualityCheckEmails();
        }
      }

      //Purpose: << Project Manager >>

      if (eurl.includes("/Email/MaterialIssueDashboard?ReqId")) {
        this.reqid = this.route.snapshot.queryParams.requestid;
        this.reqid = eurl.split('=')[1];
        if (this.reqid) {
          //redirects to MaterialIssueDashboard
          this.bindMenuForPMEmails();
        }
      }

      //Purpose: << InventoryManager >>

      if (eurl.includes("/Email/MaterialReqView?ReqId")) {
        this.reqid = this.route.snapshot.queryParams.requestid;
        this.reqid = eurl.split('=')[1];
        // this.pono = eurl.split('=')[3];
        if (this.reqid) {
          //redirects to MaterialReqView
          this.bindMenuForIMEmails();
        }
      }

      //Purpose: << PM ACK >>

      if (eurl.includes("/Email/MaterialReqView?ReqId")) {
        this.reqid = this.route.snapshot.queryParams.requestid;
        this.reqid = eurl.split('=')[1];
        if (this.reqid) {
          //redirects to MaterialReqView
          //this.bindMenuForPMACKEmails();
        }
      }

      //Purpose: << GatePassPM >>

      if (eurl.includes("/Email/GatePassPMList?GateId")) {
        this.gateid = this.route.snapshot.queryParams.gateid;
        this.gateid = eurl.split('=')[1];
        if (this.gateid) {
          //redirects to GatePassPMList
          this.bindMenuForGatePassEmails();
        }
      }
      ////GatePassPM
      //if (eurl.includes("/Email/GatePassPMList?gateid")) {
      //  this.gateid = this.route.snapshot.queryParams.gatepassid;
      //  this.gateid = eurl.split('=')[1];
      //  if (this.gateid) {
      //    //redirects to GatePassPMList
      //    this.bindMenuForGatePassEmails();
      //  }
      //}
      //
      //Purpose: << GatePassPM-InventoryClerk >>

      if (eurl.includes("/Email/GatePass?GateId")) {
        this.gateid = this.route.snapshot.queryParams.gateid;
        this.gateid = eurl.split('=')[1];
        if (this.gateid) {
          //redirects to GatePass
          this.bindMenuForGatePassInventoryClerkEmails();
        }
      }


      //Purpose: << Gate Pass PM approver to FM approver >>

      if (eurl.includes("/Email/GatePassFMList?GateId")) {
        this.fmgateid = this.route.snapshot.queryParams.gateid;
        this.fmgateid = eurl.split('=')[1];
        if (this.fmgateid) {
          //redirects to GatePassPMList
          this.bindMenuForGatePassEmails();
        }
      }


      //Purpose: << Notify to Finance>>

      if (eurl.includes("/Email/GRNotification?GRNo")) {
        this.grnno = this.route.snapshot.queryParams.grnno;
        this.grnno = eurl.split('=')[1];
        if (this.grnno) {
          //redirects to GatePassPMList
          this.bindMenuFinance();
        }
      }

      if (eurl.includes("/Email/materialtransferapproval?transferid")) {
        debugger;
        this.transferid = this.route.snapshot.queryParams.transferid;
        this.transferid = eurl.split('=')[1];
        if (this.transferid) {
          //redirects to Receipts page 
          this.bindMenuForMatTransfer();
        }
      }
      if (eurl.includes("/Email/materialtransfer?transferid")) {
        debugger;
        this.transferid = this.route.snapshot.queryParams.transferid;
        this.transferid = eurl.split('=')[1];
        if (this.transferid) {
          //redirects to Receipts page 
          this.bindMenuForMatTransferstatus();
        }
      }
      if (eurl.includes("/Email/materialreturndashboard?returnid")) {
        debugger;
        this.transferid = this.route.snapshot.queryParams.returnid;
        this.transferid = eurl.split('=')[1];
        if (this.transferid) {
          //redirects to Receipts page 
          this.bindMenuForMaterialReturnputaway();
        }
      }
      return;
    }

  }
  Navigatetopagefn() {
    this.loggedinas = "";
    var name = this.selectedrolename;
    var data1 = this.userrolelist.filter(function (element, index) {
      return (element.accessname == name);
    });
    if (data1.length > 0) {
      this.emp.roleid = String(data1[0].roleid);
      localStorage.removeItem('Employee');
      localStorage.setItem('Employee', JSON.stringify(this.emp));
      this.selectedrolename = "";
      var rid = this.emp.roleid;
      var data1 = this.userrolelist.filter(function (element, index) {
        return (element.roleid == parseInt(rid));
      });
      if (data1.length > 0) {
        this.rolename = data1[0].accessname;
        if (this.rolename) {
          this.loggedinas = "Logged in as " + this.rolename;
        }
        else {
          this.loggedinas = "";
        }

      }
      this.bindMenu();

    }
  }

  bindMenuwithoutrole() {
    this.items = [];
    this.useritems = [
      { label: 'Log Out', icon: 'pi pi-fw pi-angle-right', command: () => this.logout() }
    ];
    this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' });
    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = false;
    this.router.navigateByUrl("WMS/Home");
  }

  activeMenu(event) {
    let node;
    if (event.target.tagName === "A") {
      node = event.target;
    } else {
      node = event.target.parentNode;
    }
    let menuitem = document.getElementsByClassName("ui-menuitem-link");
    for (let i = 0; i < menuitem.length; i++) {
      menuitem[i].classList.remove("active");
    }
    node.classList.add("active")
  }


  bindemailMenu() {
    this.items = [];
    this.items.push({ label: 'Manager Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassPMList') });
    this.items.push({ label: 'Finance Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassFMList') });
    this.router.navigateByUrl('WMS/GatePassPMList');
  }
  //Purpose:<<Inventory Clerk>>

  bindMenuForGatePassInventoryClerkEmails() {
    debugger;
    this.items = [];
    this.emp.roleid = "3";
    this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' });
    this.items.push({ label: 'Receipts', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRNPosting') });
    //this.items.push({ label: 'Quality Check', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/QualityCheck') });
    this.items.push({ label: 'Put Away', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/WarehouseIncharge') });
    this.items.push({ label: 'On Hold Receipts', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/HoldGRView') });
    this.items.push({ label: 'Gate Pass', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') });
    this.items.push({ label: 'Material Return', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReturn') });
    this.items.push({ label: 'Material Issue', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialIssueDashboard') });
    this.items.push({ label: 'MIN', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MRNView') });
    this.items.push({ label: 'ASN', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ASNView') });
    this.items.push({
      label: 'Operations', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
      items: [
        { label: 'Print Barcode', icon: 'pi pi-fw pi-print', style: { 'width': '200px' }, command: () => this.router.navigateByUrl('WMS/PrintBarcode') },
        { label: 'Notify to finance', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Putawaynotify') },
        { label: 'Receive gatepass', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/gatepassreceive') }
      ]
    });
    this.router.navigate(['WMS/GatePassApprover', this.gateid]);

    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = false;
  }
  //Purpose:<<Finance>>
  bindMenuFinance() {
    this.items = [];
    this.emp.roleid = "10";
    this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), });
    this.items.push({ label: 'GR-Finance Process', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRNotification'), styleClass: 'active' });
    this.router.navigateByUrl('WMS/GRNotification');
    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = false;
  }

  //Purpose:<<Approver>>

  bindMenuForGatePassEmails() {
    debugger;
    this.items = [];
    this.emp.roleid = "8";

    this.items.push({ label: 'Manager Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassPMList') });
    this.items.push({ label: 'Finance Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassFMList') });


    //this.router.navigateByUrl('/WMS/Home');
    if (this.fmgateid) {
      this.router.navigate(['WMS/GatePassFMList'], { queryParams: { requestid: this.fmgateid } });
    }
    else {
      this.router.navigate(['WMS/GatePassPMList'], { queryParams: { requestid: this.gateid } });
    }

    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = false;

  }

  //Purpose:<<Quality Control>>

  bindMenuForQualityCheckEmails() {
    debugger;
    this.items = [];
    this.emp.roleid = "9";
    this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' });
    this.items.push({ label: 'Quality Check', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/QualityCheck') });

    //this.items.push({ label: 'Quality Check', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/QualityCheck'), styleClass: 'active' });
    //  this.router.navigateByUrl('WMS/QualityCheck');
    this.router.navigate(['WMS/QualityCheck'], { queryParams: { grnnumber: this.grnno } });
    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = false;

  }

  //Purpose:<<Inventory Clerk>>

  bindMenuForPMEmails() {
    this.items = [];
    this.emp.roleid = "3";//project manager
    this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' });
    this.items.push({
      label: 'Material Receipts', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
      items: [
        { label: 'ASN', icon: 'pi pi-fw pi-bars', style: { 'width': '200px', 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/ASNView') },
        { label: 'Goods Receipt', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRNPosting') },
        { label: 'On Hold Receipts', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/HoldGRView') },
        { label: 'Put Away', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/WarehouseIncharge') },
        { label: 'Notify to finance', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Putawaynotify') },
        { label: 'Material Return', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReturn') },
        { label: 'Receive STO Request', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveSTORequest') }
      ]
    });
    this.items.push({
      label: 'Material Issue', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
      items: [
        { label: 'Material Issue', icon: 'pi pi-fw pi-bars', style: { 'width': '250px', 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/MaterialIssueDashboard') },
        { label: 'Gate Pass', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') },
        { label: 'Material Requisition Note', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MRNView') },
        { label: 'Material Tracking', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/POStatus') }
      ]
    });
    this.items.push({
      label: 'Miscellanous', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
      items: [
        {
          label: 'Initial Stock Load',
          icon: 'pi pi-fw pi-bars',
          style: { 'font-weight': '600', 'width': '250px' },
          items: [
            { label: 'Initial Stock Load', style: { 'font-weight': '600', 'width': '230px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/InitialStock') },
            { label: 'Report', style: { 'font-weight': '600', 'width': '230px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/InitialStockReport') }
          ]
        },
        {
          label: 'Miscellanous Trancation',
          icon: 'pi pi-fw pi-bars',
          style: { 'font-weight': '600', 'width': '250px' },
          items: [
            { label: 'Miscellanous Issues', style: { 'font-weight': '600', 'width': '230px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MiscellanousIssues') },
            { label: 'Miscellanous Receipts', style: { 'font-weight': '600', 'width': '230px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MiscellanousReceipts') }
          ]
        }
      ]
    });
    this.items.push({
      label: 'Reports',
      icon: 'pi pi-fw pi-bars',
      style: { 'font-weight': '600' },
      items: [
        { label: 'Material Request', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialRequestDashboard') },
        { label: 'Material Reserve', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReserveDashboard') },
        { label: 'Material Return', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialsReturnDashboard') },
        { label: 'Material Transfer', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialTransferDashboard') },
        { label: 'Inventory Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/inventoryreport') },
        { label: 'GR Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRReports') },
        { label: 'Outward/Inward Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/outinDashboard') },
        { label: 'Bin Status Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/BinStatusReport') },
        { label: 'Safety Stock List', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SafetyStockList') }

      ]

    });
    this.router.navigate(['WMS/MaterialIssueDashboard'], { queryParams: { requestid: this.reqid } });
    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = false;
  }
  //Purpose:<<Project Manager>>

  bindMenuForIMEmails() {//inventory clerk
    this.items = [];
    this.emp.roleid = "5";//Project Manager
    this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home') });
    this.items.push({ label: 'MR Dashboard', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-chart-bar', command: () => this.router.navigateByUrl('WMS/Dashboard'), styleClass: 'active' });
    this.items.push({ label: 'Material Request', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReqView') });
    this.items.push({ label: 'Material Reserve', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReserveView') });
    this.items.push({ label: 'Material Transfer', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialTransfer') });
    this.items.push({ label: 'Material Return', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReturnfromPm') });
    this.items.push({ label: 'Direct Shipment', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Directtransfer') });
    this.router.navigateByUrl('WMS/Dashboard');
    this.router.navigate(['WMS/MaterialReqView'], { queryParams: { requestid: this.reqid } });

    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = false;
  }
  //Purpose:<<Project Manager>>

  bindMenuForPMACKEmails() {
    this.items = [];
    this.emp.roleid = "5";//Project Manager
    this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home') });
    this.items.push({ label: 'MR Dashboard', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-chart-bar', command: () => this.router.navigateByUrl('WMS/Dashboard'), styleClass: 'active' });

    //this.items.push({ label: 'Material Requests', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReqView') });
    this.items.push({ label: 'Material Reserved', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReserveView') });
    this.items.push({ label: 'Material Transfer', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialTransfer') });
    this.items.push({ label: 'Material Return', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReturnfromPm') });
    this.items.push({ label: 'Direct Shipment', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Directtransfer') });
    this.router.navigate(['WMS/MaterialReqView'], { queryParams: { requestid: this.reqid } });
    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = false;
  }

  //Purpose:<<Material Transfer approval>>
  bindMenuForMatTransfer() {
    this.items = [];
    this.emp.roleid = "11";
    this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home') });
    this.items.push({ label: 'PM Dashboard', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-chart-bar', command: () => this.router.navigateByUrl('WMS/Dashboard') });
    this.items.push({ label: 'Material Transfer Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/materialtransferapproval') });
    this.items.push({ label: 'Direct Shipment', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Directtransfer') });
    this.router.navigate(['WMS/materialtransferapproval'], { queryParams: { transferid: this.transferid } });
    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = false;
  }
  //Purpose:<<Material Transfer approval status>>
  bindMenuForMatTransferstatus() {
    this.items = [];
    this.emp.roleid = "5";
    var subroles = [];
    if (this.userrolelist.filter(li => li.roleid == 5).length > 0) {
      subroles = this.userrolelist.filter(li => li.roleid == 5)[0]["subroleid"];
    }
    this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home') });
    this.items.push({ label: 'MR Dashboard', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-chart-bar', command: () => this.router.navigateByUrl('WMS/Dashboard'), styleClass: 'active' });
    var mitem = {
      label: 'Material Request',
      icon: 'pi pi-fw pi-bars',
      style: { 'font-weight': '600' },
      items: []
    };
    if (subroles && subroles.includes("1"))//GatePassRequester
      mitem.items.push({ label: 'Gate Pass', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') });
    if (subroles && subroles.includes("2"))//Material Requestor
      mitem.items.push({ label: 'Material Request', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReqView') });
    if (subroles == null)//Material Requestor
      mitem.items.push({ label: 'Material Request', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReqView') });
    this.items.push(mitem);
    this.items.push({ label: 'Material Reserve', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReserveView') });
    this.items.push({ label: 'Material Transfer', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialTransfer') });
    this.items.push({ label: 'Material Return', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReturnfromPm') });
    this.items.push({ label: 'Direct Shipment', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Directtransfer') });
    this.router.navigate(['WMS/MaterialTransfer'], { queryParams: { transferid: this.transferid } });
    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = false;
  }

   //Purpose:<<Material Return Putaway>>
  bindMenuForMaterialReturnputaway() {
    debugger;
    this.items = [];
    this.emp.roleid = "3";
    this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' });
    this.items.push({
      label: 'Material Receipts', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
      items: [
        { label: 'ASN', icon: 'pi pi-fw pi-bars', style: { 'width': '200px', 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/ASNView') },
        { label: 'Goods Receipt', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRNPosting') },
        { label: 'On Hold Receipts', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/HoldGRView') },
        { label: 'Put Away', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/WarehouseIncharge') },
        { label: 'Notify to finance', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Putawaynotify') },
        { label: 'Material Return', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReturn') },
        { label: 'Receive STO Request', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveSTORequest') }
      ]
    });
    this.items.push({
      label: 'Material Issue', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
      items: [
        { label: 'Material Issue', icon: 'pi pi-fw pi-bars', style: { 'width': '250px', 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/MaterialIssueDashboard') },
        { label: 'Gate Pass', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') },
        { label: 'Material Requisition Note', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MRNView') },
        { label: 'Material Tracking', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/POStatus') }
      ]
    });
    this.items.push({
      label: 'Miscellanous', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
      items: [
        {
          label: 'Initial Stock Load',
          icon: 'pi pi-fw pi-bars',
          style: { 'font-weight': '600', 'width': '250px' },
          items: [
            { label: 'Initial Stock Load', style: { 'font-weight': '600', 'width': '230px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/InitialStock') },
            { label: 'Report', style: { 'font-weight': '600', 'width': '230px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/InitialStockReport') }
          ]
        },
        {
          label: 'Miscellanous Trancation',
          icon: 'pi pi-fw pi-bars',
          style: { 'font-weight': '600', 'width': '250px' },
          items: [
            { label: 'Miscellanous Issues', style: { 'font-weight': '600', 'width': '230px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MiscellanousIssues') },
            { label: 'Miscellanous Receipts', style: { 'font-weight': '600', 'width': '230px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MiscellanousReceipts') }
          ]
        }
      ]
    });
    this.items.push({
      label: 'Reports',
      icon: 'pi pi-fw pi-bars',
      style: { 'font-weight': '600' },
      items: [
        { label: 'Material Request', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialRequestDashboard') },
        { label: 'Material Reserve', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReserveDashboard') },
        { label: 'Material Return', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialsReturnDashboard') },
        { label: 'Material Transfer', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialTransferDashboard') },
        { label: 'Inventory Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/inventoryreport') },
        { label: 'GR Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRReports') },
        { label: 'Outward/Inward Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/outinDashboard') },
        { label: 'Bin Status Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/BinStatusReport') },
        { label: 'Safety Stock List', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SafetyStockList') }

      ]

    });
    this.router.navigate(['WMS/MaterialReturn'], { queryParams: { returnid: this.transferid } });

    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = false;
  }
  //Purpose:<<Inventory clerk login>>

  bindMenuForEmail() {
    debugger;
    //this.itemsrole = 2;
    this.items = [];
    this.emp.roleid = "3";
    this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' });
    this.items.push({
      label: 'Material Receipts', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
      items: [
        { label: 'ASN', icon: 'pi pi-fw pi-bars', style: { 'width': '200px', 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/ASNView') },
        { label: 'Goods Receipt', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRNPosting') },
        { label: 'On Hold Receipts', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/HoldGRView') },
        { label: 'Put Away', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/WarehouseIncharge') },
        { label: 'Notify to finance', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Putawaynotify') },
        { label: 'Material Return', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReturn') },
        { label: 'Receive STO Request', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveSTORequest') }
      ]
    });
    this.items.push({
      label: 'Material Issue', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
      items: [
        { label: 'Material Issue', icon: 'pi pi-fw pi-bars', style: { 'width': '250px', 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/MaterialIssueDashboard') },
        { label: 'Gate Pass', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') },
        { label: 'Material Requisition Note', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MRNView') },
        { label: 'Material Tracking', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/POStatus') }
      ]
    });
    this.items.push({
      label: 'Miscellanous', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
      items: [
        {
          label: 'Initial Stock Load',
          icon: 'pi pi-fw pi-bars',
          style: { 'font-weight': '600', 'width': '250px' },
          items: [
            { label: 'Initial Stock Load', style: { 'font-weight': '600', 'width': '230px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/InitialStock') },
            { label: 'Report', style: { 'font-weight': '600', 'width': '230px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/InitialStockReport') }
          ]
        },
        {
          label: 'Miscellanous Trancation',
          icon: 'pi pi-fw pi-bars',
          style: { 'font-weight': '600', 'width': '250px' },
          items: [
            { label: 'Miscellanous Issues', style: { 'font-weight': '600', 'width': '230px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MiscellanousIssues') },
            { label: 'Miscellanous Receipts', style: { 'font-weight': '600', 'width': '230px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MiscellanousReceipts') }
          ]
        }
      ]
    });
    this.items.push({
      label: 'Reports',
      icon: 'pi pi-fw pi-bars',
      style: { 'font-weight': '600' },
      items: [
        { label: 'Material Request', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialRequestDashboard') },
        { label: 'Material Reserve', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReserveDashboard') },
        { label: 'Material Return', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialsReturnDashboard') },
        { label: 'Material Transfer', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialTransferDashboard') },
        { label: 'Inventory Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/inventoryreport') },
        { label: 'GR Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRReports') },
        { label: 'Outward/Inward Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/outinDashboard') },
        { label: 'Bin Status Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/BinStatusReport') },
        { label: 'Safety Stock List', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SafetyStockList') }

      ]

    });
    if (this.gateentryid != null && this.gateentryid != "") {
      this.router.navigate(['WMS/GRNPosting'], { queryParams: { inwmasterid: this.gateentryid } });
    }
    else {
      if (this.grnno != null && this.grnno != "") {


        this.router.navigate(['WMS/GRNPosting'], { queryParams: { grnnumber: this.grnno } });

      }
    }

    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = false;
   

  }

  bindDefaultMenu() {
    this.items = [];
    this.otheritems = [];
    this.tempitems = [];
    var ismaxwidth = false;
    this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' });
    this.useritems = [
      { label: 'Log Out', icon: 'pi pi-fw pi-angle-right', command: () => this.logout() }
    ];
    this.userrolelist.forEach(item => {
      if (this.items.length >= 5) {
        ismaxwidth = true;
      }
      if (item.roleid == 1) {

        this.items.push({ label: 'Inbound', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SecurityCheck') });
        this.items.push({
          label: 'Gatepass out/in', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
          items: [
            { label: 'Returnable', style: { 'font-weight': '600', 'width': '200px' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/GatePassinout/1') },
            { label: 'Non Returnable', style: { 'font-weight': '600', 'width': '200px' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/GatePassinout/2') },
          ]
        });


      }
      else if (item.roleid == 2) {

        this.items.push({
          label: 'Inventory Ageing',
          icon: 'pi pi-fw pi-bars',
          style: { 'font-weight': '600' },
          items: [
            { label: 'Obsolete Inventory', style: { 'font-weight': '600', 'width': '200px' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/ObsoleteInventoryMovement') },
            { label: 'Excess Inventory', style: { 'font-weight': '600', 'width': '200px' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/ExcessInventoryMovement') },
          ]
        });
        this.items.push({
          label: 'ABC Analysis',
          icon: 'pi pi-fw pi-bars',
          style: { 'font-weight': '600' },
          items: [
            { label: 'ABC Category', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/ABCCategory') },
            { label: 'ABC Analysis', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/ABCAnalysis') },
          ]
        });
        this.items.push({ label: 'Material Tracking', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/POStatus') });
        this.items.push({ label: 'Safety Stock List', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SafetyStockList') });
        this.items.push({ label: 'Bin Status Report', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/BinStatusReport') });
        this.items.push({
          label: 'Reports',
          icon: 'pi pi-fw pi-bars',
          style: { 'font-weight': '600' },
          items: [
            { label: 'Material Request Dashboard', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialRequestDashboard') },
            { label: 'Material Reserve Dashboard', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReserveDashboard') },
            { label: 'Material Return Dashboard', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialsReturnDashboard') },
            {
              label: 'Material Transfer Dashboard', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialTransferDashboard'),
            },
            { label: 'Initial Stock Load', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/InitialStockReport') },
            { label: 'GR Report', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/GRReports') },
          ]

          //   this.items.push({ label: 'Material Transfer Dashboard', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialTransferDashboard') });

        });

      }
      else if (item.roleid == 3) {

        this.items.push({ label: 'Receipts', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRNPosting') });
        //this.items.push({ label: 'Quality Check', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/QualityCheck') });
        this.items.push({ label: 'Put Away', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/WarehouseIncharge') });



      }
      else if (item.roleid == 4) {

        this.items.push({ label: 'Material Issue', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialIssueDashboard') });
        this.items.push({ label: 'Cycle Count', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Cyclecount') });
        this.items.push({ label: 'Gate Pass', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') });



      }
      else if (item.roleid == 5) {

        this.items.push({ label: 'PM Dashboard', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-chart-bar', command: () => this.router.navigateByUrl('WMS/Dashboard') });
        this.items.push({ label: 'Material Requests', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReqView') });
        this.items.push({ label: 'Material Reserved', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReserveView') });



      }
      else if (item.roleid == 6) {
        var roles = this.items.filter(li => li.label == "PM Dashboard");
        if (roles.length == 0) {

          this.items.push({ label: 'PM Dashboard', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-chart-bar', command: () => this.router.navigateByUrl('WMS/Dashboard') });

        }
      }
      else if (item.roleid == 7) {
        var roles = this.items.filter(li => li.label == "Gate Pass");
        var rolescc = this.items.filter(li => li.label == "Cycle Count");

        if (roles.length == 0) {
          this.items.push({ label: 'Gate Pass', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') });
        }
        if (rolescc.length == 0) {
          this.items.push({
            label: 'Cycle count',
            icon: 'pi pi-fw pi-bars',
            style: { 'font-weight': '600' },
            items: [
              { label: 'Cycle Count', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/Cyclecount') },
              { label: 'Cycle Config', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/Cycleconfig') },
            ]
          });
        }
        else {
          this.items.push({ label: 'Cycle Config', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Cycleconfig') });
        }
        this.items.push({ label: 'FIFO LIst', icon: 'pi pi-fw pi-bars', style: { 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/FIFOList') });
        this.items.push({ label: 'Inventory Movement', icon: 'pi pi-fw pi-bars', style: { 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/InventoryMovement') });
        this.items.push({ label: 'Internal Stock Transfer', icon: 'pi pi-fw pi-bars', style: { 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/Stocktransfer') });
        this.items.push({ label: 'Internal Stock Transfer', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Stocktransfer') });
        this.items.push({
          label: 'Master Pages',
          icon: 'pi pi-fw pi-bars',
          style: { 'font-weight': '600' },
          items: [
            { label: 'Material Master', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/MaterialMaster') },

          ]
        });


        //this.items.push({
        //  label: 'Other',
        //  icon: 'pi pi-fw pi-bars',
        //  style: { 'font-weight': '600' },
        //  items: [
        //    { label: 'FIFO LIst', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/FIFOList') },
        //    { label: 'Inventory Movement', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/InventoryMovement') },
        //    { label: 'Internal Stock Transfer', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/Stocktransfer') }
        //  ]
        //});

      }
      else if (item.roleid == 8) {

        if (localStorage.getItem("requestedpage")) {
          var page = localStorage.getItem('requestedpage');
          if (page == "GatePassPMList") {
            this.items.push({ label: 'Manager Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassPMList'), styleClass: 'active' });
            this.items.push({ label: 'Finance Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassFMList') });
          }
          else if (page == "GatePassFMList") {
            this.items.push({ label: 'Manager Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassPMList') });
            this.items.push({ label: 'Finance Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassFMList'), styleClass: 'active' });
          }
        }
        else {
          this.items.push({ label: 'Manager Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassPMList') });
          this.items.push({ label: 'Finance Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassFMList') });
        }




      }
    });

    if (this.items.length > 6) {
      debugger;
      this.otheritems = [];
      var i = 0;
      this.items.forEach(item => {
        if (i > 5) {
          this.otheritems.push(item);
        }
        else {
          this.tempitems.push(item);
        }
        i++;
      })
      this.tempitems.push({
        label: 'Others',
        icon: 'pi pi-fw pi-bars',
        style: { 'width': '300px' },
        items: this.otheritems
      });
      this.items = [];
      this.items = this.tempitems;

    }
    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = false;

    if (localStorage.getItem("requestedpage")) {
      var page1 = localStorage.getItem("requestedpage");
      localStorage.removeItem('requestedpage');
      this.router.navigateByUrl('WMS/' + page1);
    }
    else {
      this.router.navigateByUrl('WMS/Home');
    }


  }

  bindnonifications() {
    this.getGatePassList();
  }

  getGatePassList() {
    this.notif = false;
    debugger;
    this.wmsService.getGatePassList().subscribe(data => {
      this.totalGatePassList = data;
      //PM
      this.gatepassData = this.totalGatePassList.filter(li => li.approverid == this.emp.employeeno && (li.approverstatus == this.approverstatus));
      //FM
      this.gatepassData1 = this.totalGatePassList.filter(li => li.fmapproverid == this.emp.employeeno && li.approverstatus == "Approved" && li.fmapprovedstatus == this.approverstatus);
      this.prepareGatepassList();
      this.prepareGatepassList1();

    });
  }
  prepareGatepassList() {
    debugger;
    this.gatepasslist = [];
    this.gatepassData.forEach(item => {
      var res = this.gatepasslist.filter(li => li.gatepassid == item.gatepassid);
      if (res.length == 0) {
        this.gatepasslist.push(item);
      }
    });
    if (this.gatepasslist.length > 0) {
      this.notif = true;
      this.notifcount = this.notifcount + 1;
      var count = this.gatepasslist.length;
      this.notificationitems.push({ label: count + ' gate passes pending for approval as project manager', icon: 'pi pi-fw pi-angle-right', command: () => this.notifnavigation('WMS/GatePassPMList') });
    }
  }
  prepareGatepassList1() {
    debugger;
    this.gatepasslist1 = [];
    this.gatepassData1.forEach(item => {
      var res = this.gatepasslist1.filter(li => li.gatepassid == item.gatepassid);
      if (res.length == 0) {
        this.gatepasslist1.push(item);
      }
    });
    if (this.gatepasslist1.length > 0) {
      this.notif = true;
      this.notifcount = this.notifcount + 1;
      var count = this.gatepasslist1.length;
      this.notificationitems.push({ label: count + ' gate passes pending for approval as finance manager', icon: 'pi pi-fw pi-angle-right', command: () => this.notifnavigation('WMS/GatePassFMList') });
    }
  }

  notifnavigation(path: string) {
    if (path.includes('GatePassPMList')) {
      this.items.push({ label: 'Manager Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassPMList') });
      this.router.navigateByUrl('WMS/GatePassPMList');

    }
    if (path.includes('GatePassFMList')) {
      this.items.push({ label: 'Finance Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassFMList') });
      this.router.navigateByUrl('WMS/GatePassFMList');

    }

  }


  binduserdashboardmenu() {
    this.useritems = [
      { label: 'Log Out', icon: 'pi pi-fw pi-angle-right', command: () => this.logout() }
    ];
    var page = sessionStorage.getItem("userdashboardpage");
    if (page == "Inbound") {
      this.items = [];
      this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home') });
      this.items.push({ label: 'Inbound', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SecurityCheck'), styleClass: 'active' });
      this.items.push({
        label: 'Gatepass', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
        items: [
          { label: 'Returnable', style: { 'font-weight': '600', 'width': '200px' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/GatePassinout/1') },
          { label: 'Non Returnable', style: { 'font-weight': '600', 'width': '200px' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/GatePassinout/2') },
        ]
      });
      this.items.push({ label: 'Outbound', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars' });
      this.router.navigateByUrl('WMS/SecurityCheck');
    }
    else if (page == "Putaway" || page == "Receive" || page == "Issue") {
      this.items = [];
      this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' });
      this.items.push({
        label: 'Material Receipts', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
        items: [
          { label: 'ASN', icon: 'pi pi-fw pi-bars', style: { 'width': '200px', 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/ASNView') },
          { label: 'Goods Receipt', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRNPosting') },
          { label: 'On Hold Receipts', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/HoldGRView') },
          { label: 'Put Away', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/WarehouseIncharge') },
          { label: 'Notify to finance', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Putawaynotify') },
          { label: 'Material Return', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReturn') },
          { label: 'Receive STO Request', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveSTORequest') }
        ]
      });
      this.items.push({
        label: 'Material Issue', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
        items: [
          { label: 'Material Issue', icon: 'pi pi-fw pi-bars', style: { 'width': '250px', 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/MaterialIssueDashboard') },
          { label: 'Gate Pass', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') },
          { label: 'Material Requisition Note', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MRNView') },
          { label: 'Material Tracking', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/POStatus') }
        ]
      });
      this.items.push({
        label: 'Miscellanous', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
        items: [
          {
            label: 'Initial Stock Load',
            icon: 'pi pi-fw pi-bars',
            style: { 'font-weight': '600', 'width': '250px' },
            items: [
              { label: 'Initial Stock Load', style: { 'font-weight': '600', 'width': '230px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/InitialStock') },
              { label: 'Report', style: { 'font-weight': '600', 'width': '230px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/InitialStockReport') }
            ]
          },
          {
            label: 'Miscellanous Trancation',
            icon: 'pi pi-fw pi-bars',
            style: { 'font-weight': '600', 'width': '250px' },
            items: [
              { label: 'Miscellanous Issues', style: { 'font-weight': '600', 'width': '230px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MiscellanousIssues') },
              { label: 'Miscellanous Receipts', style: { 'font-weight': '600', 'width': '230px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MiscellanousReceipts') }
            ]
          }
        ]
      });
      this.items.push({
        label: 'Reports',
        icon: 'pi pi-fw pi-bars',
        style: { 'font-weight': '600' },
        items: [
          { label: 'Material Request', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialRequestDashboard') },
          { label: 'Material Reserve', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReserveDashboard') },
          { label: 'Material Return', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialsReturnDashboard') },
          { label: 'Material Transfer', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialTransferDashboard') },
          { label: 'Inventory Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/inventoryreport') },
          { label: 'GR Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRReports') },
          { label: 'Outward/Inward Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/outinDashboard') },
          { label: 'Bin Status Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/BinStatusReport') },
          { label: 'Safety Stock List', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SafetyStockList') }

        ]

      });


    }
    else if (page == "Quality") {
      this.items = [];
      this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home') });
      this.items.push({ label: 'Quality Check', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/QualityCheck'), styleClass: 'active' });
      this.router.navigateByUrl('WMS/QualityCheck');

    }
    else if (page == "Reserve") {
      this.items = [];
      var subroles = [];
      if (this.userrolelist.filter(li => li.roleid == 5).length > 0) {
        subroles = this.userrolelist.filter(li => li.roleid == 5)[0]["subroleid"];
      }

      this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home') });
      //this.items.push({ label: 'Manager Dashboard', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-chart-bar', command: () => this.router.navigateByUrl('WMS/PMDashboard') });
      this.items.push({ label: 'MR Dashboard', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-chart-bar', command: () => this.router.navigateByUrl('WMS/Dashboard'), styleClass: 'active' });
      var mitem = {
        label: 'Material Request',
        icon: 'pi pi-fw pi-bars',
        style: { 'font-weight': '600' },
        items: []
      };
      if (subroles && subroles.includes("1"))//GatePassRequester
        mitem.items.push({ label: 'Gate Pass', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') });
      if (subroles && subroles.includes("2"))//Material Requestor
        mitem.items.push({ label: 'Material Request', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReqView') });
      if (subroles == null)//Material Requestor
        mitem.items.push({ label: 'Material Request', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReqView') });
      this.items.push(mitem);
      this.items.push({ label: 'Material Reserve', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReserveView') });
      this.items.push({ label: 'Material Transfer', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialTransfer') });
      this.items.push({ label: 'Material Return', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReturnfromPm') });
      this.items.push({ label: 'Direct Shipment', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Directtransfer') });
      this.router.navigateByUrl('WMS/MaterialReserveView');

    }
    else if (page == "Approve") {
      this.items = [];
      this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' });
      this.items.push({ label: 'Manager Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassPMList') });
      this.items.push({ label: 'Finance Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassFMList') });
      this.router.navigateByUrl('WMS/Home');
    }
    else if (page == "Count") {
      this.items = [];
      this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' });
      this.items.push({
        label: 'Material Receipts', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
        items: [
          { label: 'ASN', icon: 'pi pi-fw pi-bars', style: { 'width': '200px', 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/ASNView') },
          { label: 'Goods Receipt', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRNPosting') },
          { label: 'On Hold Receipts', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/HoldGRView') },
          { label: 'Put Away', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/WarehouseIncharge') },
          { label: 'Notify to finance', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Putawaynotify') },
          { label: 'Material Return', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReturn') },
          { label: 'Receive STO Request', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveSTORequest') }

        ]
      });
      this.items.push({
        label: 'Material Issue', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
        items: [
          { label: 'Material Issue', icon: 'pi pi-fw pi-bars', style: { 'width': '250px', 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/MaterialIssueDashboard') },
          { label: 'Gate Pass', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') },
          { label: 'Material Requisition Note', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MRNView') },
          { label: 'Material Tracking', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/POStatus') }
        ]
      });
      this.items.push({
        label: 'Miscellanous', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
        items: [
          {
            label: 'Initial Stock Load',
            icon: 'pi pi-fw pi-bars',
            style: { 'font-weight': '600', 'width': '250px' },
            items: [
              { label: 'Initial Stock Load', style: { 'font-weight': '600', 'width': '230px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/InitialStock') },
              { label: 'Report', style: { 'font-weight': '600', 'width': '230px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/InitialStockReport') }
            ]
          },
          {
            label: 'Miscellanous Trancation',
            icon: 'pi pi-fw pi-bars',
            style: { 'font-weight': '600', 'width': '250px' },
            items: [
              { label: 'Miscellanous Issues', style: { 'font-weight': '600', 'width': '230px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MiscellanousIssues') },
              { label: 'Miscellanous Receipts', style: { 'font-weight': '600', 'width': '230px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MiscellanousReceipts') }
            ]
          }
        ]
      });
      this.items.push({
        label: 'Reports',
        icon: 'pi pi-fw pi-bars',
        style: { 'font-weight': '600' },
        items: [
          { label: 'Material Request', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialRequestDashboard') },
          { label: 'Material Reserve', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReserveDashboard') },
          { label: 'Material Return', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialsReturnDashboard') },
          { label: 'Material Transfer', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialTransferDashboard') },
          { label: 'Inventory Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/inventoryreport') },
          { label: 'GR Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRReports') },
          { label: 'Outward/Inward Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/outinDashboard') },
          { label: 'Bin Status Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/BinStatusReport') },
          { label: 'Safety Stock List', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SafetyStockList') }

        ]

      });
      this.items.push({
        label: 'Inventory Management', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
        items: [
          { label: 'Cycle count', style: { 'font-weight': '600', 'width': '230px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Cyclecount') },
          {
            label: 'Inventory Ageing',
            icon: 'pi pi-fw pi-bars',
            style: { 'font-weight': '600' },
            items: [
              { label: 'Obsolete Inventory', style: { 'font-weight': '600', 'width': '200px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ObsoleteInventoryMovement') },
              { label: 'Excess Inventory', style: { 'font-weight': '600', 'width': '200px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ExcessInventoryMovement') }
            ]
          },
          {
            label: 'ABC Analysis',
            icon: 'pi pi-fw pi-bars',
            style: { 'font-weight': '600' },
            items: [
              { label: 'ABC Classification', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ABCCategory') },
              { label: 'ABC Analysis', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ABCAnalysis') }
            ]
          }
        ]
      });
      this.router.navigateByUrl('WMS/Cyclecount');
    }
    if (page == "Putaway") {
      this.router.navigateByUrl('WMS/WarehouseIncharge');
    }
    if (page == "Receive") {
      this.router.navigateByUrl('WMS/GRNPosting');
    }
    if (page == "Issue") {
      this.router.navigateByUrl('WMS/MaterialIssueDashboard');
    }

    sessionStorage.removeItem("userdashboardpage");
    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = false;
  }


  bindMenu() {
    debugger;
    this.items = [];

    this.useritems = [
      { label: 'Log Out', icon: 'pi pi-fw pi-angle-right', command: () => this.logout() }
    ];
    if (this.emp.roleid == "1") {
      this.items = [];
      this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home') });
      this.items.push({ label: 'Inbound', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SecurityCheck'), styleClass: 'active' });
      this.items.push({
        label: 'Gatepass', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
        items: [
          { label: 'Returnable', style: { 'font-weight': '600', 'width': '200px' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/GatePassinout/1') },
          { label: 'Non Returnable', style: { 'font-weight': '600', 'width': '200px' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/GatePassinout/2') },
        ]
      });
      this.items.push({ label: 'Outbound', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars' });
      this.router.navigateByUrl('WMS/SecurityCheck');
    }
    if (this.emp.roleid == "2") {//inventory enquiry
      this.items = [];
      this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' });
      this.items.push({
        label: 'Reports',
        icon: 'pi pi-fw pi-bars',
        style: { 'font-weight': '600' },
        items: [
          { label: 'Inventory Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/inventoryreport') },
          { label: 'Bin Status Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/BinStatusReport') },
          { label: 'Material Tracking', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/POStatus') }

        ]

      });
      this.items.push({
        label: 'Other',
        icon: 'pi pi-fw pi-bars',
        style: { 'font-weight': '600' },
        items: [
          { label: 'Material Request', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialRequestDashboard') },
          { label: 'Material Reserve', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReserveDashboard') },
          { label: 'Material Return', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialsReturnDashboard') },
          { label: 'Material Transfer', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialTransferDashboard') },
          { label: 'GR Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRReports') },
          { label: 'Outward/Inward Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/outinDashboard') },
          { label: 'Safety Stock List', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SafetyStockList') }
        ]

      });
      

      //this.items.push({ label: 'GRN Posting', icon: 'pi pi-fw pi-lock', command: () => this.router.navigateByUrl('WMS/GRNPosting') })
      this.router.navigateByUrl('WMS/Home');
    }
    if (this.emp.roleid == "3") {//inventory clerk
      this.items = [];
      this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' });
      this.items.push({
        label: 'Material Receipts', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
        items: [
          { label: 'ASN', icon: 'pi pi-fw pi-bars', style: { 'width': '200px', 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/ASNView') },
          { label: 'Goods Receipt', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRNPosting') },
          { label: 'On Hold Receipts', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/HoldGRView') },
          { label: 'Put Away', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/WarehouseIncharge') },
          { label: 'Notify to finance', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Putawaynotify') },
          { label: 'Material Return', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReturn') },
          { label: 'Receive STO Request', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveSTORequest') }
        ]
      });
      this.items.push({
        label: 'Material Issue', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
        items: [
          { label: 'Material Issue', icon: 'pi pi-fw pi-bars', style: { 'width': '250px', 'font-weight': '600'}, command: () => this.router.navigateByUrl('WMS/MaterialIssueDashboard') },
          { label: 'Gate Pass', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') },
          { label: 'Material Requisition Note', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MRNView') },
          { label: 'Material Tracking', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/POStatus') }
        ]
      });
      this.items.push({
        label: 'Miscellanous', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
        items: [
          {
            label: 'Initial Stock Load',
            icon: 'pi pi-fw pi-bars',
            style: { 'font-weight': '600', 'width': '250px' },
            items: [
              { label: 'Initial Stock Load', style: { 'font-weight': '600', 'width': '230px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/InitialStock') },
              { label: 'Report', style: { 'font-weight': '600', 'width': '230px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/InitialStockReport') }
            ]
          },
          {
            label: 'Miscellanous Trancation',
            icon: 'pi pi-fw pi-bars',
            style: { 'font-weight': '600', 'width': '250px' },
            items: [
              { label: 'Miscellanous Issues', style: { 'font-weight': '600', 'width': '230px'}, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MiscellanousIssues') },
              { label: 'Miscellanous Receipts', style: { 'font-weight': '600', 'width': '230px'}, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MiscellanousReceipts') }
            ]
          }
        ]
      });
      this.items.push({
        label: 'Reports',
        icon: 'pi pi-fw pi-bars',
        style: { 'font-weight': '600' },
        items: [
          { label: 'Material Request', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialRequestDashboard') },
          { label: 'Material Reserve', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReserveDashboard') },
          { label: 'Material Return', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialsReturnDashboard') },
          { label: 'Material Transfer', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialTransferDashboard') },
          { label: 'Inventory Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/inventoryreport') },
          { label: 'GR Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRReports') },
          { label: 'Outward/Inward Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/outinDashboard') },
          { label: 'Bin Status Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/BinStatusReport') },
          { label: 'Safety Stock List', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SafetyStockList') }

        ]

      });
     
      //this.items.push({ label: 'Material Return', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReturn') });
      //this.items.push({
      //  label: 'Receive STO Request', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveSTORequest')
      //});
      this.router.navigateByUrl('WMS/Home');
    }
    if (this.emp.roleid == "4") {//inventory manager
      this.items = [];
      this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' });
      this.items.push({
        label: 'Material Receipts', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
        items: [
          { label: 'ASN', icon: 'pi pi-fw pi-bars', style: { 'width': '200px', 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/ASNView') },
          { label: 'Goods Receipt', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRNPosting') },
          { label: 'On Hold Receipts', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/HoldGRView') },
          { label: 'Put Away', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/WarehouseIncharge') },
          { label: 'Notify to finance', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Putawaynotify') },
          { label: 'Material Return', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReturn') },
          { label: 'Receive STO Request', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveSTORequest') }

        ]
      });
      this.items.push({
        label: 'Material Issue', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
        items: [
          { label: 'Material Issue', icon: 'pi pi-fw pi-bars', style: { 'width': '250px', 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/MaterialIssueDashboard') },
          { label: 'Gate Pass', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') },
          { label: 'Material Requisition Note', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MRNView') },
          { label: 'Material Tracking', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/POStatus') }
        ]
      });
      this.items.push({
        label: 'Miscellanous', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
        items: [
          {
            label: 'Initial Stock Load',
            icon: 'pi pi-fw pi-bars',
            style: { 'font-weight': '600', 'width': '250px' },
            items: [
              { label: 'Initial Stock Load', style: { 'font-weight': '600', 'width': '230px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/InitialStock') },
              { label: 'Report', style: { 'font-weight': '600', 'width': '230px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/InitialStockReport') }
            ]
          },
          {
            label: 'Miscellanous Trancation',
            icon: 'pi pi-fw pi-bars',
            style: { 'font-weight': '600', 'width': '250px' },
            items: [
              { label: 'Miscellanous Issues', style: { 'font-weight': '600', 'width': '230px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MiscellanousIssues') },
              { label: 'Miscellanous Receipts', style: { 'font-weight': '600', 'width': '230px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MiscellanousReceipts') }
            ]
          }
        ]
      });
      this.items.push({
        label: 'Reports',
        icon: 'pi pi-fw pi-bars',
        style: { 'font-weight': '600' },
        items: [
          { label: 'Material Request', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialRequestDashboard') },
          { label: 'Material Reserve', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReserveDashboard') },
          { label: 'Material Return', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialsReturnDashboard') },
          { label: 'Material Transfer', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialTransferDashboard') },
          { label: 'Inventory Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/inventoryreport') },
          { label: 'GR Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRReports') },
          { label: 'Outward/Inward Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/outinDashboard') },
          { label: 'Bin Status Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/BinStatusReport') },
          { label: 'Safety Stock List', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SafetyStockList') }

        ]

      });
      this.items.push({
          label: 'Inventory Management', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
        items: [
          { label: 'Cycle count', style: { 'font-weight': '600', 'width': '230px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Cyclecount') },
          {
            label: 'Inventory Ageing',
            icon: 'pi pi-fw pi-bars',
            style: { 'font-weight': '600' },
            items: [
              { label: 'Obsolete Inventory', style: { 'font-weight': '600', 'width': '200px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ObsoleteInventoryMovement') },
              { label: 'Excess Inventory', style: { 'font-weight': '600', 'width': '200px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ExcessInventoryMovement') }
            ]
          },
          {
            label: 'ABC Analysis',
            icon: 'pi pi-fw pi-bars',
            style: { 'font-weight': '600' },
            items: [
              { label: 'ABC Classification', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ABCCategory') },
              { label: 'ABC Analysis', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ABCAnalysis') }
            ]
          }
        ]
      });
    
      this.router.navigateByUrl('WMS/Home');
    }
    if (this.emp.roleid == "5") {//project manager (Material Requester)
      this.items = [];
      var subroles = [];
      if (this.userrolelist.filter(li => li.roleid == 5).length > 0) {
        subroles = this.userrolelist.filter(li => li.roleid == 5)[0]["subroleid"];
      }

      this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home') });
      //this.items.push({ label: 'Manager Dashboard', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-chart-bar', command: () => this.router.navigateByUrl('WMS/PMDashboard') });
      this.items.push({ label: 'MR Dashboard', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-chart-bar', command: () => this.router.navigateByUrl('WMS/Dashboard'), styleClass: 'active' });
      var mitem = {
        label: 'Material Request',
        icon: 'pi pi-fw pi-bars',
        style: { 'font-weight': '600' },
        items: []
      };
      if (subroles && subroles.includes("1"))//GatePassRequester
        mitem.items.push({ label: 'Gate Pass', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') });
      if (subroles && subroles.includes("2"))//Material Requestor
        mitem.items.push({ label: 'Material Request', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReqView') });
      if (subroles == null)//Material Requestor
        mitem.items.push({ label: 'Material Request', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReqView') });
      this.items.push(mitem);
      this.items.push({ label: 'Material Reserve', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReserveView') });
      this.items.push({ label: 'Material Transfer', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialTransfer') });
      this.items.push({ label: 'Material Return', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReturnfromPm') });
      this.items.push({ label: 'Direct Shipment', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Directtransfer') });
      this.router.navigateByUrl('WMS/Dashboard');
    }
    if (this.emp.roleid == "6") {//dashboard
      this.items = [];
      this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' });
      this.items.push({ label: 'PM Dashboard', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-chart-bar', command: () => this.router.navigateByUrl('WMS/Dashboard') });
      this.router.navigateByUrl('WMS/Home');
    }
    if (this.emp.roleid == "7") {//admin
      this.items = [];
      this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' });
      //this.items.push({ label: 'Gate Pass', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') });
      this.items.push({ label: 'Internal Stock Transfer', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Stocktransfer') });
      this.items.push({
        label: 'Initial Stock Upload',
        icon: 'pi pi-fw pi-bars',
        style: { 'font-weight': '600' },
        items: [
          { label: 'Initial Stock Upload', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/InitialStock') },
          { label: 'Initial Stock Report', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/InitialStockReport') },
        ]
      });
      this.items.push({
        label: 'Cycle count',
        icon: 'pi pi-fw pi-bars',
        style: { 'font-weight': '600' },
        items: [
          { label: 'Cycle Count', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/Cyclecount') },
          { label: 'Cycle Config', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/Cycleconfig') },
        ]
      });
      //this.items.push({
      //  label: 'Other',
      //  icon: 'pi pi-fw pi-bars',
      //  style: { 'font-weight': '600' },
      //  items: [
      //    { label: 'FIFO LIst', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/FIFOList') },
      //    { label: 'Inventory Movement', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/InventoryMovement') },
      //    { label: 'Stores Return Note', icon: 'pi pi-fw pi-print', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/StoresReturnNote') },
      //    { label: 'Stock Card Print', icon: 'pi pi-fw pi-print', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/StockCardPrint') },
      //    { label: 'Print Barcode', icon: 'pi pi-fw pi-print', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/PrintBarcode') },
      //    { label: 'Material Report', icon: 'pi pi-fw pi-print', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/MaterialReport') }
      //    //{ label: 'Internal Stock Transfer', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/Stocktransfer') },

      //    //{ label: 'AssignRole', icon: 'pi pi-fw pi-bars', style: { 'width': '200px' }, command: () => this.router.navigateByUrl('WMS/AssignRole') }
      //    //{ label: 'Bin Status Report', style: { 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/BinStatusReport') }
      //  ]
      //});
      //this.items.push({ label: 'Bin Status Report', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/BinStatusReport') });
      //this.items.push({ label: 'BarCode', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Barcode') });
      this.items.push({
        label: 'Master Pages',
        icon: 'pi pi-fw pi-bars',
        style: { 'font-weight': '600' },
        items: [
          { label: 'Material Master', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/MaterialMaster') },
          { label: 'GatePass Master', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/GatePassMaster') },

        ]
      });
      this.router.navigateByUrl('/WMS/Home');
    }
    if (this.emp.roleid == "8") {//Approver     
      this.items = [];
      if (localStorage.getItem("requestedpage")) {
        var page = localStorage.getItem('requestedpage');
        if (page == "GatePassPMList") {
          this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home') });
          this.items.push({ label: 'Manager Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassPMList'), styleClass: 'active' });
          this.items.push({ label: 'Finance Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassFMList') });
        }
        else if (page == "GatePassFMList") {
          this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home') });
          this.items.push({ label: 'Manager Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassPMList') });
          this.items.push({ label: 'Finance Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassFMList'), styleClass: 'active' });
        }
        else {
          this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' });
          this.items.push({ label: 'Manager Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassPMList') });
          this.items.push({ label: 'Finance Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassFMList') });
        }
        localStorage.removeItem('requestedpage');
        this.router.navigateByUrl('WMS/' + page);
      }
      else {
        this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' });
        this.items.push({ label: 'Manager Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassPMList') });
        this.items.push({ label: 'Finance Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassFMList') });
        this.router.navigateByUrl('WMS/Home');
      }

    }
    if (this.emp.roleid == "9") {//Quality control
      this.items = [];
      this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' });
      this.items.push({ label: 'Quality Check', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/QualityCheck') });
      this.router.navigateByUrl('WMS/Home');
    }
    if (this.emp.roleid == "10") {//Finance
      this.items = [];
      this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), });
      this.items.push({ label: 'GR-Finance Process', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRNotification'), styleClass: 'active' });
      this.router.navigateByUrl('WMS/GRNotification');
    }
    if (this.emp.roleid == "11") {//Material Requestor
      this.items = [];
      this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home') });
      this.items.push({ label: 'PM Dashboard', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-chart-bar', command: () => this.router.navigateByUrl('WMS/Dashboard') });
      this.items.push({ label: 'Material Transfer Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/materialtransferapproval') });
      this.items.push({ label: 'Direct Shipment', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Directtransfer') });
      this.router.navigateByUrl('WMS/Home');
    }
    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = false;


  }

  logout() {
    localStorage.removeItem("Employee");
    localStorage.removeItem("userroles");
    sessionStorage.removeItem("userdashboardpage");
    this.loggedin = false;
    let element: HTMLElement = document.getElementById("btnuser") as HTMLElement;
    element.hidden = true;
    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = true;
    //let elementx: HTMLElement = document.getElementById("btnuser1") as HTMLElement;
    //elementx.hidden = true;
    //let elementy: HTMLElement = document.getElementById("btnuser2") as HTMLElement;
    //elementy.hidden = true;
    this.overlaymodel.hide();
    this.router.navigateByUrl("WMS/Login");
  }

  userloggedHandler(emp: Employee) {
    debugger;
    this.loggedin = true;
    window.location.reload();
    //this.router.navigateByUrl('RefreshComponent', { skipLocationChange: true }).then(() => {
    //  this.router.navigate(['WMS/Home']);
    //}); 
    //this.ngOnInit();
    //this.router.navigateByUrl('WMS/Home');
  }
  changemenu() {
    debugger;
    //this.ngOnInit();
    window.location.reload();

  }
}
