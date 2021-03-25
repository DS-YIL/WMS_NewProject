import { Component, OnInit, ViewChild } from '@angular/core';
import { MenuItem } from 'primeng/api/menuitem';
import { Employee, userAcessNamesModel, rbamaster } from '../Models/Common.Model';
import { Router, RouterEvent, NavigationEnd, ActivatedRoute } from '@angular/router';
import { filter } from 'rxjs/operators';
import { constants } from '../Models/WMSConstants';
import { wmsService } from '../WmsServices/wms.service';
import { environment } from 'src/environments/environment'
import { pageModel, ddlmodel } from '../Models/WMS.Model';
import { isNullOrUndefined } from 'util';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent implements OnInit {
  constructor(private router: Router, private route: ActivatedRoute, private wmsService: wmsService, public constants: constants) { }
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
 
  
  //variables used for rba
  //for security
  showHome: boolean = false;
  showInbound: boolean = false;
  showoutwardinward: boolean = false;
  showoutwardinwardreturnable: boolean = false;
  showoutwardinwardnonreturnable: boolean = false;
  showoutbound: boolean = false;
  
  
  ngOnInit() {
    debugger;
    this.setdefaultmenuview();
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
      let elementx: HTMLElement = document.getElementById("notlogged") as HTMLElement;
      elementx.hidden = false;
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
      let elementx: HTMLElement = document.getElementById("notlogged") as HTMLElement;
      elementx.hidden = true;
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
        if (isNullOrUndefined(this.userrolelist)) {
          alert("Selected Role is not assigned to you, select Your role");
          this.router.navigateByUrl("WMS/Login");
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
            //this.setmenuview();
            this.bindMenu("default");
          }

        }

      }

    }
    else {
      let element: HTMLElement = document.getElementById("btnuser") as HTMLElement;
      element.hidden = true;
      let elementx: HTMLElement = document.getElementById("notlogged") as HTMLElement;
      elementx.hidden = false;
      let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
      element1.hidden = true;
      this.router.navigateByUrl("WMS/Login");
    }




  }
  
  setrolename(roleid: any) {
    var rid = roleid;
    if (isNullOrUndefined(this.userrolelist)) {
      console.log("list3");
    }
    var data1 = this.userrolelist.filter(function (element, index) {
      return (element.roleid == parseInt(rid));
    });
    if (data1.length > 0) {
      this.rolename = data1[0].accessname;
      if (this.rolename) {
        this.loggedinas = "Logged in as " + this.rolename;
      }

    }

  }

  bindMenuForEmailNav(eurl: any) {
    if (eurl.includes("/Email")) {
      let element: HTMLElement = document.getElementById("btnuser") as HTMLElement;
      element.hidden = false;
      let elementx: HTMLElement = document.getElementById("notlogged") as HTMLElement;
      elementx.hidden = true;
      let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
      element1.hidden = false;
      if (eurl.includes("/Email/GRNPosting?GateEntryNo")) {
        this.inwmasterid = this.route.snapshot.queryParams.gateentryid;
        this.gateentryid = eurl.split('=')[1];
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
          this.bindMenuForPMEmails("material");
        }
      }

      //Purpose: << Project Manager >>

      if (eurl.includes("/Email/ApproveSTOMaterial?ReqId")) {
        debugger;
        this.transferid = eurl.split('=')[1];
        if (this.transferid) {
          //redirects to Receipts page 
          this.bindMenuForRequestApproval("materialsto")
        }
      }

      //Purpose: << Project Manager >>

      if (eurl.includes("/Email/IssueSTOMaterial?ReqId")) {
        this.reqid = this.route.snapshot.queryParams.requestid;
        this.reqid = eurl.split('=')[1];
        if (this.reqid) {
          //redirects to MaterialIssueDashboard
          this.bindMenuForPMEmails("sto");
        }
      }

      if (eurl.includes("/Email/grnputaway?GRNo")) {
        this.reqid = this.route.snapshot.queryParams.requestid;
        this.reqid = eurl.split('=')[1];
        if (this.reqid) {
          //redirects to MaterialIssueDashboard
          this.bindMenuForPMEmails("grnputaway");
        }
      }

      //Purpose: << Project Manager >>

      if (eurl.includes("/Email/ApprovalSubcontractingMaterial?ReqId")) {
        debugger;
        this.transferid = eurl.split('=')[1];
        if (this.transferid) {
          //redirects to Receipts page 
          this.bindMenuForRequestApproval("materialsubcontract")
        }
      }

      //Purpose: << Project Manager >>

      if (eurl.includes("/Email/IssueSubcontractingMaterial?ReqId")) {
        this.reqid = this.route.snapshot.queryParams.requestid;
        this.reqid = eurl.split('=')[1];
        if (this.reqid) {
          //redirects to MaterialIssueDashboard
          this.bindMenuForPMEmails("subcontract");
        }
      }

      //Purpose: << Project Manager >>

      if (eurl.includes("/Email/STOMaterialPutaway?ReqId")) {
        this.reqid = this.route.snapshot.queryParams.requestid;
        this.reqid = eurl.split('=')[1];
        if (this.reqid) {
          //redirects to MaterialIssueDashboard
          this.bindMenuForPMEmails("stoputaway");
        }
      }

      //Purpose: << Project Manager >>

      if (eurl.includes("/Email/SubcontractMaterialPutaway?ReqId")) {
        this.reqid = this.route.snapshot.queryParams.requestid;
        this.reqid = eurl.split('=')[1];
        if (this.reqid) {
          //redirects to MaterialIssueDashboard
          this.bindMenuForPMEmails("subcontractputaway");
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

      //if (eurl.includes("/Email/MaterialReqView?ReqId")) {
      //  this.reqid = this.route.snapshot.queryParams.requestid;
      //  this.reqid = eurl.split('=')[1];
      //  if (this.reqid) {
      //    //redirects to MaterialReqView
      //    //this.bindMenuForPMACKEmails();
      //  }
      //}

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
      if (eurl.includes("/Email/GatePass?GatepassId")) {
        this.gateid = this.route.snapshot.queryParams.gateid;
        this.gateid = eurl.split('=')[1];
        if (this.gateid) {
          //redirects to GatePass
          this.bindMenuForGatePassUser();
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
      if (eurl.includes("/Email/MaterialRequestApproval?ReqId")) {
        debugger;
        this.transferid = eurl.split('=')[1];
        if (this.transferid) {
          //redirects to Receipts page 
          this.bindMenuForRequestApproval("materialrequest")
        }
      }
      return;
    }

  }
  Navigatetopagefn() {
    this.loggedinas = "";
    var name = this.selectedrolename;
    if (isNullOrUndefined(this.userrolelist)) {
      console.log("list5");
    }
    var data1 = this.userrolelist.filter(function (element, index) {
      return (element.accessname == name);
    });
    if (data1.length > 0) {
      this.emp.roleid = String(data1[0].roleid);
      localStorage.removeItem('Employee');
      localStorage.setItem('Employee', JSON.stringify(this.emp));
      this.selectedrolename = "";
      var rid = this.emp.roleid;
      if (isNullOrUndefined(this.userrolelist)) {
        console.log("list6");
      }
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
    this.setrolename(this.emp.roleid);
    localStorage.removeItem('Employee');
    localStorage.setItem('Employee', JSON.stringify(this.emp));
    this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' });
    this.items.push({
      label: 'Material Receipts', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
      items: [
        { label: 'ASN', icon: 'pi pi-fw pi-bars', style: { 'width': '200px', 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/ASNView') },
        { label: 'Goods Receipt', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRNPosting') },
        { label: 'On Hold Receipts', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/HoldGRView') },
        {
          label: 'Put Away',
          icon: 'pi pi-fw pi-bars',
          style: { 'font-weight': '600' },
          items: [
            { label: 'Receipt Put Away', style: { 'font-weight': '600', 'width': '300px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/WarehouseIncharge') },
            { label: 'Initial Stock Put Away', style: { 'font-weight': '600', 'width': '300px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/InitialStockPutAway') },
            { label: 'Material Return Put Away', style: { 'font-weight': '600', 'width': '300px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReturn') },
            { label: 'Intra Unit Transfer Put Away', style: { 'font-weight': '600', 'width': '300px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveMaterial') }
          ]
        },
        { label: 'Notify to finance', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Putawaynotify') }
      ]
    });
    this.items.push({
      label: 'Material Issue', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
      items: [
        { label: 'Material Issue', icon: 'pi pi-fw pi-bars', style: { 'width': '250px', 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/MaterialIssueDashboard') },
        { label: 'Gate Pass', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') },
        { label: 'Material Requisition Note', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MRNView') },
        { label: 'Material Tracking', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/POStatus') },
        { label: 'Intra Unit Material Issue', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveSTORequest') },
        { label: 'Sub Contract Material Issue', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveSubContractRequest') }
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
    this.items.push({ label: 'Internal Stock Transfer', icon: 'pi pi-fw pi-bars', style: { 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/Stocktransfer') });
    this.constants.gatePassIssueType = "Pending";
    this.router.navigate(['WMS/GatePassApprover', this.gateid]);

    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = false;
  }

  bindMenuForGatePassUser() {
    debugger;
    this.items = [];
    this.emp.roleid = "5";
    this.setrolename(this.emp.roleid);
    localStorage.removeItem('Employee');
    localStorage.setItem('Employee', JSON.stringify(this.emp));
    var subroles = [];
    if (isNullOrUndefined(this.userrolelist)) {
      console.log("list7");
    }
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
      mitem.items.push({ label: 'Gate Pass', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') });
    if (subroles && subroles.includes("2"))//Material Requestor
      mitem.items.push({ label: 'Material Request', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReqView') });
    if (subroles == null)//Material Requestor
      mitem.items.push({ label: 'Material Request', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReqView') });
    mitem.items.push({ label: 'Intra Unit Transfer', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/StockTransferOrder') });
    mitem.items.push({ label: 'Sub Contract', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SubContractTransfer') });
    this.items.push(mitem);
    this.items.push({ label: 'Material Reserve', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReserveView') });
    this.items.push({ label: 'Material Transfer', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialTransfer') });
    this.items.push({ label: 'Material Return', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReturnfromPm') });
    this.items.push({ label: 'Direct Shipment', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Directtransfer') });

    this.router.navigate(['WMS/GatePass'], { queryParams: { gatepassid: this.gateid } });

    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = false;
  }
  //Purpose:<<Finance>>
  bindMenuFinance() {
    this.items = [];
    this.emp.roleid = "10";
    this.setrolename(this.emp.roleid);
    localStorage.removeItem('Employee');
    localStorage.setItem('Employee', JSON.stringify(this.emp));
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
    this.setrolename(this.emp.roleid);
    localStorage.removeItem('Employee');
    localStorage.setItem('Employee', JSON.stringify(this.emp));
    this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home') });
    if (this.fmgateid) {
      this.items.push({ label: 'Manager Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassPMList') });
      this.items.push({ label: 'Finance Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassFMList'), styleClass: 'active' });
      this.router.navigate(['WMS/GatePassFMList'], { queryParams: { requestid: this.fmgateid } });

    }
    else {
      this.items.push({ label: 'Manager Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassPMList'), styleClass: 'active' });
      this.items.push({ label: 'Finance Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassFMList') });
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
    this.setrolename(this.emp.roleid);
    localStorage.removeItem('Employee');
    localStorage.setItem('Employee', JSON.stringify(this.emp));
    this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' });
    this.items.push({ label: 'Quality Check', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/QualityCheck') });
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
   
    this.router.navigate(['WMS/QualityCheck'], { queryParams: { grnnumber: this.grnno } });
    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = false;

  }

  //Purpose:<<Inventory Clerk>>

  bindMenuForPMEmails(type: string) {
    this.items = [];
    this.emp.roleid = "3";//project manager
    this.setrolename(this.emp.roleid);
    localStorage.removeItem('Employee');
    localStorage.setItem('Employee', JSON.stringify(this.emp));
    this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' });
    this.items.push({
      label: 'Material Receipts', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
      items: [
        { label: 'ASN', icon: 'pi pi-fw pi-bars', style: { 'width': '200px', 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/ASNView') },
        { label: 'Goods Receipt', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRNPosting') },
        { label: 'On Hold Receipts', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/HoldGRView') },
        {
          label: 'Put Away',
          icon: 'pi pi-fw pi-bars',
          style: { 'font-weight': '600' },
          items: [
            { label: 'Receipt Put Away', style: { 'font-weight': '600', 'width': '300px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/WarehouseIncharge') },
            { label: 'Initial Stock Put Away', style: { 'font-weight': '600', 'width': '300px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/InitialStockPutAway') },
            { label: 'Material Return Put Away', style: { 'font-weight': '600', 'width': '300px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReturn') },
            { label: 'Intra Unit Transfer Put Away', style: { 'font-weight': '600', 'width': '300px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveMaterial') }
          ]
        },
        { label: 'Notify to finance', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Putawaynotify') }
      ]
    });
    this.items.push({
      label: 'Material Issue', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
      items: [
        { label: 'Material Issue', icon: 'pi pi-fw pi-bars', style: { 'width': '250px', 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/MaterialIssueDashboard') },
        { label: 'Gate Pass', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') },
        { label: 'Material Requisition Note', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MRNView') },
        { label: 'Material Tracking', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/POStatus') },
        { label: 'Intra Unit Material Issue', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveSTORequest') },
        { label: 'Sub Contract Material Issue', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveSubContractRequest') }
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
        { label: 'Safety Stock List', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SafetyStockList') },
        { label: 'Print Barcode', icon: 'pi pi-fw pi-print', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/PrintBarcode') },

      ]

    });
    this.items.push({ label: 'Internal Stock Transfer', icon: 'pi pi-fw pi-bars', style: { 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/Stocktransfer') });
    if (type == "sto") {
      this.router.navigate(['WMS/ReceiveSTORequest'], { queryParams: { requestid: this.reqid } });
    }
    else if (type == "subcontract") {
      this.router.navigate(['WMS/ReceiveSubContractRequest'], { queryParams: { requestid: this.reqid } });
    }
    else if (type == "stoputaway") {
      this.router.navigate(['WMS/ReceiveMaterial'], { queryParams: { requestid: this.reqid } });
    }
    else if (type == "subcontractputaway") {
      this.router.navigate(['WMS/ReceiveMaterial'], { queryParams: { requestid: this.reqid } });
    }
    else if (type == "grnputaway") {
      this.router.navigate(['WMS/WarehouseIncharge'], { queryParams: { requestid: this.reqid } });
    }
    else {
      this.router.navigate(['WMS/MaterialIssueDashboard'], { queryParams: { requestid: this.reqid } });
    }

    
    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = false;
  }

  //Purpose:<<Project Manager>>

  bindMenuForIMEmails() {//inventory clerk
    this.items = [];
    this.emp.roleid = "5";//Project Manager
    this.setrolename(this.emp.roleid);
    localStorage.removeItem('Employee');
    localStorage.setItem('Employee', JSON.stringify(this.emp));
    var subroles = [];
    if (isNullOrUndefined(this.userrolelist)) {
      console.log("list8");
    }
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
      mitem.items.push({ label: 'Gate Pass', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') });
    if (subroles && subroles.includes("2"))//Material Requestor
      mitem.items.push({ label: 'Material Request', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReqView') });
    if (subroles == null)//Material Requestor
      mitem.items.push({ label: 'Material Request', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReqView') });
    mitem.items.push({ label: 'Intra Unit Transfer', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/StockTransferOrder') });
    mitem.items.push({ label: 'Sub Contract', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SubContractTransfer') });
    this.items.push(mitem);
    this.items.push({ label: 'Material Reserve', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReserveView') });
    this.items.push({ label: 'Material Transfer', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialTransfer') });
    this.items.push({ label: 'Material Return', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReturnfromPm') });
    this.items.push({ label: 'Direct Shipment', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Directtransfer') });
    this.router.navigate(['WMS/MaterialReqView'], { queryParams: { requestid: this.reqid } });

    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = false;
  }
  //Purpose:<<Project Manager>>

  bindMenuForPMACKEmails() {
    this.items = [];
    this.emp.roleid = "5";//Project Manager
    this.setrolename(this.emp.roleid);
    this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home') });
    this.items.push({ label: 'MR Dashboard', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-chart-bar', command: () => this.router.navigateByUrl('WMS/Dashboard'), styleClass: 'active' });

    //this.items.push({ label: 'Material Requests', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReqView') });
    this.items.push({ label: 'Material Reserved', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReserveView') });
    this.items.push({ label: 'Material Transfer', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialTransfer') });
    this.items.push({ label: 'Material Return', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReturnfromPm') });
    this.items.push({ label: 'Direct Shipment', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Directtransfer') });
    //this.items.push({ label: 'STO', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/StockTransferOrder') });
    this.router.navigateByUrl('WMS/Dashboard');
    this.router.navigate(['WMS/MaterialReqView'], { queryParams: { requestid: this.reqid } });
    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = false;
  }

  //Purpose:<<Material Transfer approval>>
  bindMenuForMatTransfer() {
    this.items = [];
    this.emp.roleid = "11";
    this.setrolename(this.emp.roleid);
    localStorage.removeItem('Employee');
    localStorage.setItem('Employee', JSON.stringify(this.emp));
    this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home') });
    this.items.push({ label: 'PM Dashboard', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-chart-bar', command: () => this.router.navigateByUrl('WMS/Dashboard') });
    this.items.push({
      label: 'Approvals',
      icon: 'pi pi-fw pi-bars',
      style: { 'font-weight': '600' },
      items: [
        { label: 'Material Request Approval', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialRequestApproval') },
        { label: 'Material Transfer Approval', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/materialtransferapproval') },
        { label: 'Intra Unit Transfer Approval', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/STOApproval') },
        { label: 'Sub Contract Approval', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SubcontractApproval') }
      ]

    });
    this.items.push({ label: 'Direct Shipment', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Directtransfer') });
    this.items.push({ label: 'Assign Project', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/AssignProject') });
    this.router.navigate(['WMS/materialtransferapproval'], { queryParams: { transferid: this.transferid } });
    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = false;
  }
  bindMenuForRequestApproval(type : string) {
    this.items = [];
    this.emp.roleid = "11";
    this.setrolename(this.emp.roleid);
    localStorage.removeItem('Employee');
    localStorage.setItem('Employee', JSON.stringify(this.emp));
    this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home') });
    this.items.push({ label: 'PM Dashboard', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-chart-bar', command: () => this.router.navigateByUrl('WMS/Dashboard') });
    this.items.push({
      label: 'Approvals',
      icon: 'pi pi-fw pi-bars',
      style: { 'font-weight': '600' },
      items: [
        { label: 'Material Request Approval', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialRequestApproval') },
        { label: 'Material Transfer Approval', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/materialtransferapproval') },
        { label: 'Intra Unit Transfer Approval', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/STOApproval') },
        { label: 'Sub Contract Approval', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SubcontractApproval') }
      ]

    });
    this.items.push({ label: 'Direct Shipment', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Directtransfer') });
    this.items.push({ label: 'Assign Project', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/AssignProject') });
    if (type == "materialrequest") {
      this.router.navigate(['WMS/MaterialRequestApproval'], { queryParams: { transferid: this.transferid } });
    }
    else if (type == "materialsto") {
      this.router.navigate(['WMS/STOApproval'], { queryParams: { transferid: this.transferid } });
    }
    else if (type == "materialsubcontract") {
      this.router.navigate(['WMS/SubcontractApproval'], { queryParams: { transferid: this.transferid } });
    }
   
    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = false;
  }
  //Purpose:<<Material Transfer approval status>>
  bindMenuForMatTransferstatus() {
    this.items = [];
    this.emp.roleid = "5";
    this.setrolename(this.emp.roleid);
    localStorage.removeItem('Employee');
    localStorage.setItem('Employee', JSON.stringify(this.emp));
    var subroles = [];
    if (isNullOrUndefined(this.userrolelist)) {
      console.log("list9");
    }
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
      mitem.items.push({ label: 'Gate Pass', style: { 'font-weight': '600', 'width': '250px'  }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') });
    if (subroles && subroles.includes("2"))//Material Requestor
      mitem.items.push({ label: 'Material Request', style: { 'font-weight': '600', 'width': '250px'  }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReqView') });
    if (subroles == null)//Material Requestor
      mitem.items.push({ label: 'Material Request', style: { 'font-weight': '600', 'width': '250px'  }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReqView') });
    mitem.items.push({ label: 'Intra Unit Transfer', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/StockTransferOrder') });
    mitem.items.push({ label: 'Sub Contract', style: { 'font-weight': '600', 'width': '250px'  }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SubContractTransfer') });
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
    this.setrolename(this.emp.roleid);
    localStorage.removeItem('Employee');
    localStorage.setItem('Employee', JSON.stringify(this.emp));
    this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' });
    this.items.push({
      label: 'Material Receipts', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
      items: [
        { label: 'ASN', icon: 'pi pi-fw pi-bars', style: { 'width': '200px', 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/ASNView') },
        { label: 'Goods Receipt', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRNPosting') },
        { label: 'On Hold Receipts', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/HoldGRView') },
        {
          label: 'Put Away',
          icon: 'pi pi-fw pi-bars',
          style: { 'font-weight': '600' },
          items: [
            { label: 'Receipt Put Away', style: { 'font-weight': '600', 'width': '300px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/WarehouseIncharge') },
            { label: 'Initial Stock Put Away', style: { 'font-weight': '600', 'width': '300px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/InitialStockPutAway') },
            { label: 'Material Return Put Away', style: { 'font-weight': '600', 'width': '300px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReturn') },
            { label: 'Intra Unit Transfer Put Away', style: { 'font-weight': '600', 'width': '300px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveMaterial') }
          ]
        },
        { label: 'Notify to finance', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Putawaynotify') }
      ]
    });
    this.items.push({
      label: 'Material Issue', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
      items: [
        { label: 'Material Issue', icon: 'pi pi-fw pi-bars', style: { 'width': '250px', 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/MaterialIssueDashboard') },
        { label: 'Gate Pass', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') },
        { label: 'Material Requisition Note', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MRNView') },
        { label: 'Material Tracking', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/POStatus') },
        { label: 'Intra Unit Material Issue', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveSTORequest') },
        { label: 'Sub Contract Material Issue', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveSubContractRequest') }
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
        { label: 'Safety Stock List', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SafetyStockList') },
        { label: 'Print Barcode', icon: 'pi pi-fw pi-print', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/PrintBarcode') },

      ]

    });
    this.items.push({ label: 'Internal Stock Transfer', icon: 'pi pi-fw pi-bars', style: { 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/Stocktransfer') });
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
    this.setrolename(this.emp.roleid);
    localStorage.removeItem('Employee');
    localStorage.setItem('Employee', JSON.stringify(this.emp));
    this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' });
    this.items.push({
      label: 'Material Receipts', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
      items: [
        { label: 'ASN', icon: 'pi pi-fw pi-bars', style: { 'width': '200px', 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/ASNView') },
        { label: 'Goods Receipt', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRNPosting') },
        { label: 'On Hold Receipts', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/HoldGRView') },
        {
          label: 'Put Away',
          icon: 'pi pi-fw pi-bars',
          style: { 'font-weight': '600' },
          items: [
            { label: 'Receipt Put Away', style: { 'font-weight': '600', 'width': '300px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/WarehouseIncharge') },
            { label: 'Initial Stock Put Away', style: { 'font-weight': '600', 'width': '300px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/InitialStockPutAway') },
            { label: 'Material Return Put Away', style: { 'font-weight': '600', 'width': '300px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReturn') },
            { label: 'Intra Unit Transfer Put Away', style: { 'font-weight': '600', 'width': '300px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveMaterial') }
          ]
        },
        { label: 'Notify to finance', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Putawaynotify') }
      ]
    });
    this.items.push({
      label: 'Material Issue', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
      items: [
        { label: 'Material Issue', icon: 'pi pi-fw pi-bars', style: { 'width': '250px', 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/MaterialIssueDashboard') },
        { label: 'Gate Pass', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') },
        { label: 'Material Requisition Note', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MRNView') },
        { label: 'Material Tracking', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/POStatus') },
        { label: 'Intra Unit Material Issue', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveSTORequest') },
        { label: 'Sub Contract Material Issue', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveSubContractRequest') }
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
        { label: 'Safety Stock List', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SafetyStockList') },
        { label: 'Print Barcode', icon: 'pi pi-fw pi-print', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/PrintBarcode') },

      ]

    });
    this.items.push({ label: 'Internal Stock Transfer', icon: 'pi pi-fw pi-bars', style: { 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/Stocktransfer') });
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
        if (isNullOrUndefined(this.items)) {
          console.log("list10");
        }
        var roles = this.items.filter(li => li.label == "PM Dashboard");
        if (roles.length == 0) {

          this.items.push({ label: 'PM Dashboard', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-chart-bar', command: () => this.router.navigateByUrl('WMS/Dashboard') });

        }
      }
      else if (item.roleid == 7) {
        if (isNullOrUndefined(this.items)) {
          console.log("list11");
        }
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
      if (isNullOrUndefined(this.totalGatePassList)) {
        console.log("list12");
      }
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
      if (isNullOrUndefined(this.gatepasslist)) {
        console.log("list13");
      }
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
      if (isNullOrUndefined(this.gatepasslist1)) {
        console.log("list14");
      }
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
      this.bindMenu('WMS/SecurityCheck');
    }
   
    else if (page == "Quality") {
      this.bindMenu('WMS/QualityCheck');
    }
    else if (page == "Reserve") {
      this.bindMenu('WMS/MaterialReserveView');
    }
    else if (page == "Approve") {
      this.bindMenu('WMS/Home');
    }
    else if (page == "Count") {
      this.bindMenu('WMS/Cyclecount');
    }
    if (page == "Putaway") {
      this.bindMenu('WMS/WarehouseIncharge');
    }
    if (page == "Receive") {
      this.bindMenu('WMS/GRNPosting');
    }
    if (page == "Issue") {
      this.bindMenu('WMS/MaterialIssueDashboard');
    }
    if (page == "onhold") {
      this.bindMenu('WMS/HoldGRView');
    }
    if (page == "notify") {
      this.bindMenu('WMS/Putawaynotify');
    }

    sessionStorage.removeItem("userdashboardpage");
    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = false;
  }

  bindmenubyrba() {
    this.items = [];


    this.useritems = [
      { label: 'Log Out', icon: 'pi pi-fw pi-angle-right', command: () => this.logout() }
    ];
    this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home') });
    if (isNullOrUndefined(this.rbalist)) {
      console.log("list15");
    }
    let currentrolerba = this.rbalist.filter(o => o.roleid == parseInt(this.emp.roleid));
    if (currentrolerba.length > 0) {
      var rba = currentrolerba[0];
      if (rba.inv_enquiry) {
        this.items.push({
          label: 'Inventory Reports',
          icon: 'pi pi-fw pi-bars',
          style: { 'font-weight': '600' },
            items: [
              { label: 'Inventory Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/inventoryreport') },
              { label: 'Bin Status Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/BinStatusReport') },
              { label: 'Material Tracking', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/POStatus') }

            ]
        });

      }
      if (rba.inv_reports) {
        
        this.items.push({
          label: 'Other',
          icon: 'pi pi-fw pi-bars',
          style: { 'font-weight': '600' },
          items: [
            { label: 'Material Request', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialRequestDashboard') },
            { label: 'Material Reserve', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReserveDashboard') },
            { label: 'Material Return', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialsReturnDashboard') },
            { label: 'Material Transfer', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialTransferDashboard') },
            { label: 'Outward/Inward Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/outinDashboard') },
            { label: 'Bin Status Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/BinStatusReport') },
            { label: 'Safety Stock List', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SafetyStockList') },

          ]

        });
      }
      if (rba.all_reports) {

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
            { label: 'Safety Stock List', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SafetyStockList') },
            { label: 'Print Barcode', icon: 'pi pi-fw pi-print', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/PrintBarcode') },
          ]

        });
      }
      if (rba.gate_entry) {
        this.items.push({ label: 'Inbound', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SecurityCheck') });
        this.items.push({ label: 'Outbound', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars' });
      }
      var receiptsitems = {
        label: 'Material Receipts',
        icon: 'pi pi-fw pi-bars',
        style: { 'font-weight': '600' },
        visible: (rba.receive_material || rba.put_away || rba.notify_to_finance),
        items: []
      };
      if (rba.receive_material) {
        receiptsitems.items.push({ label: 'ASN', icon: 'pi pi-fw pi-bars', style: { 'width': '200px', 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/ASNView') });
        receiptsitems.items.push({ label: 'Goods Receipt', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRNPosting') });
        receiptsitems.items.push({ label: 'On Hold Receipts', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/HoldGRView') });
      }
      if (rba.put_away) {
        receiptsitems.items.push({
          label: 'Put Away',
          icon: 'pi pi-fw pi-bars',
          style: { 'font-weight': '600' },
          items: [
            { label: 'Receipt Put Away', style: { 'font-weight': '600', 'width': '300px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/WarehouseIncharge') },
            { label: 'Initial Stock Put Away', style: { 'font-weight': '600', 'width': '300px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/InitialStockPutAway') },
            { label: 'Material Return Put Away', style: { 'font-weight': '600', 'width': '300px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReturn') },
            { label: 'Intra Unit Transfer Put Away', style: { 'font-weight': '600', 'width': '300px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveMaterial') }
          ]
        });
      }
      if (rba.notify_to_finance) {
        receiptsitems.items.push({ label: 'Notify to finance', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Putawaynotify') });
      }
      this.items.push(matrequestitems);
      
      if (rba.material_return) {
        this.items.push({ label: 'Material Return', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReturnfromPm') });

      }
      if (rba.material_transfer) {
        this.items.push({ label: 'Material Transfer', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialTransfer') });
      }
      var matrequestitems = {
        label: 'Material Request',
        icon: 'pi pi-fw pi-bars',
        style: { 'font-weight': '600' },
        visible: (rba.gate_pass || rba.material_request),
        items: []
      };
      if (rba.gate_pass) {
        matrequestitems.items.push({ label: 'Gate Pass', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') });

      }
      if (rba.material_request) {
        matrequestitems.items.push({ label: 'Material Request', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReqView') });
        matrequestitems.items.push({ label: 'Intra Unit Transfer', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/StockTransferOrder') });
        matrequestitems.items.push({ label: 'Sub Contract', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SubContractTransfer') });
      }
      this.items.push(matrequestitems);
      if (rba.gatepass_inout) {
        this.items.push({
          label: 'Gatepass', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
          items: [
            { label: 'Returnable', style: { 'font-weight': '600', 'width': '200px' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/GatePassinout/1') },
            { label: 'Non Returnable', style: { 'font-weight': '600', 'width': '200px' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/GatePassinout/2') },
          ]
        });

      }
      if (rba.gatepass_approval) {
        this.items.push({ label: 'Manager Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassPMList') });
        this.items.push({ label: 'Finance Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassFMList') });
      }
      if (rba.material_issue) {
        this.items.push({
          label: 'Material Issue', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
          items: [
            { label: 'Material Issue', icon: 'pi pi-fw pi-bars', style: { 'width': '250px', 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/MaterialIssueDashboard') },
            { label: 'Gate Pass', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') },
            { label: 'Material Requisition Note', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MRNView') },
            { label: 'Material Tracking', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/POStatus') },
            { label: 'Intra Unit Material Issue', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveSTORequest') },
            { label: 'Sub Contract Material Issue', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveSubContractRequest') }
          ]
        });
      }
     
      if (rba.material_reservation) {
        this.items.push({ label: 'Material Reserve', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReserveView') });

      }
      if (rba.initialstock_upload) {
        this.items.push({
          label: 'Initial Stock Upload',
          icon: 'pi pi-fw pi-bars',
          style: { 'font-weight': '600' },
          items: [
            { label: 'Initial Stock Upload', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/InitialStock') },
            { label: 'Initial Stock Report', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/InitialStockReport') },
          ]
        });
      }
      var invmanageitems = {
        label: 'Inventory Management',
        icon: 'pi pi-fw pi-bars',
        style: { 'font-weight': '600' },
        visible: (rba.inventory_Management),
        items: []
      };
      var cyclecountitems = {
        label: 'Cycle Count',
        icon: 'pi pi-fw pi-bars',
        style: { 'font-weight': '600' },
        visible: (rba.cyclecount_configuration || rba.cycle_counting || rba.cyclecount_approval),
        items: []
      };
      if (rba.abc_classification) {
        this.items.push({
          label: 'ABC Analysis',
          icon: 'pi pi-fw pi-bars',
          style: { 'font-weight': '600' },
          items: [
            { label: 'ABC Classification', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/ABCCategory') },
            { label: 'ABC Analysis', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/ABCAnalysis') },
          ]
        });
        invmanageitems.items.push({
            label: 'ABC Analysis',
            icon: 'pi pi-fw pi-bars',
            style: { 'font-weight': '600' },
            items: [
              { label: 'ABC Classification', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/ABCCategory') },
              { label: 'ABC Analysis', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/ABCAnalysis') },
            ]
          });
        }
      
      
      if (rba.cyclecount_configuration) {
        cyclecountitems.items.push({ label: 'Cycle Count config', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Cycleconfig') });

      }
      if (rba.cycle_counting) {
        cyclecountitems.items.push({ label: 'Cycle Count', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Cyclecount') });

      }
      if (rba.cyclecount_approval) {
        cyclecountitems.items.push({ label: 'Cycle Count Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Cyclecount') });

      }
      invmanageitems.items.push(cyclecountitems);
      if (rba.inventory_Management) {
        invmanageitems.items.push({
          label: 'Inventory Ageing',
          icon: 'pi pi-fw pi-bars',
          style: { 'font-weight': '600' },
          items: [
            { label: 'Obsolete Inventory', style: { 'font-weight': '600', 'width': '200px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ObsoleteInventoryMovement') },
            { label: 'Excess Inventory', style: { 'font-weight': '600', 'width': '200px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ExcessInventoryMovement') }
          ]
        });
      }
      this.items.push(invmanageitems);
      this.items.push(cyclecountitems);
      if (rba.admin_access) {
        this.items.push({
          label: 'Master Pages',
          icon: 'pi pi-fw pi-bars',
          style: { 'font-weight': '600' },
          items: [
            { label: 'Material Master', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/MaterialMaster') },
            { label: 'GatePass Reason Master', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/GatePassMaster') },
            { label: 'Plant Master', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/PlantMaster') },
            { label: 'Assign Role', icon: 'pi pi-fw pi-bars', style: { 'width': '200px' }, command: () => this.router.navigateByUrl('WMS/AssignRole') },
            { label: 'Assign Project Manager', style: { 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/AssignProjectManager') },
            { label: 'Miscellanous Master', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/MiscellanousReason') }
          ]
        });
      }
      if (rba.masterdata_creation) {

      }
      if (rba.masterdata_updation) {

      }
      if (rba.masterdata_approval) {

      }
      if (rba.printbarcodes) {
        this.items.push({
          label: 'Other',
          icon: 'pi pi-fw pi-bars',
          style: { 'font-weight': '600' },
          items: [
            { label: 'FIFO LIst', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/FIFOList') },
            { label: 'Inventory Movement', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/InventoryMovement') },
            { label: 'Stores Return Note', icon: 'pi pi-fw pi-print', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/StoresReturnNote') },
            { label: 'Stock Card Print', icon: 'pi pi-fw pi-print', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/StockCardPrint') },
            { label: 'Print Barcode', icon: 'pi pi-fw pi-print', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/PrintBarcode') },
            { label: 'Material Report', icon: 'pi pi-fw pi-print', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/MaterialReport') }
            //{ label: 'Internal Stock Transfer', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/Stocktransfer') },

            //{ label: 'AssignRole', icon: 'pi pi-fw pi-bars', style: { 'width': '200px' }, command: () => this.router.navigateByUrl('WMS/AssignRole') }
            //{ label: 'Bin Status Report', style: { 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/BinStatusReport') }
          ]
        });

      }
      if (rba.quality_check) {
        this.items.push({ label: 'Quality Check', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/QualityCheck') });
      }
      if (rba.pmdashboard_view) {
        this.items.push({ label: 'PM Dashboard', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-chart-bar', command: () => this.router.navigateByUrl('WMS/Dashboard') });
      }
      if (rba.min) {
        this.items.push({ label: 'Material Requisition Note', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MRNView') });

      }
      if (rba.direct_transfer_view) {
        this.items.push({ label: 'Direct Shipment', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Directtransfer') });

      }
     
      if (rba.gr_process) {
        this.items.push({ label: 'GR-Finance Process', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRNotification') });

      }
      var matrequestapprovals = {
        label: 'Approvals',
        icon: 'pi pi-fw pi-bars',
        style: { 'font-weight': '600' },
        visible: (rba.material_transfer_approval || rba.materialrequest_approval),
        items: []
      };
      if (rba.material_transfer_approval) {
        matrequestapprovals.items.push({ label: 'Material Transfer Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/materialtransferapproval') });

      }
      if (rba.materialrequest_approval) {
        
        matrequestapprovals.items.push({ label: 'Material Transfer Approval', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/materialtransferapproval') });
        matrequestapprovals.items.push({ label: 'Intra Unit Transfer Approval', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/STOApproval') });
        matrequestapprovals.items.push({ label: 'Sub Contract Approval', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SubcontractApproval') });
      }
      this.items.push(matrequestapprovals);
      if (rba.asn_view) {

      }
      if (rba.internal_stock_transfer) {
        this.items.push({ label: 'Internal Stock Transfer', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Stocktransfer') });

      }
      if (rba.assign_pm) {
        this.items.push({ label: 'Assign Project', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/AssignProject') });
       
      }
      if (rba.annexure_report) {
        this.items.push({ label: 'Sub Contract Annexure', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/AnnexureReport') });
      }
      if (rba.miscellanous) {
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

      }
      let element1x: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
      element1x.hidden = false;
      if (sessionStorage.getItem("userdashboardpage")) {
        this.nevigatefromuserdashboard();
      }
      else {
        this.nevigatefromrba(rba);
      }


    }


  }

  nevigatefromuserdashboard() {
    var page = sessionStorage.getItem("userdashboardpage");
    if (page == "Inbound") {
      this.router.navigateByUrl('WMS/SecurityCheck');
    }
    else if (page == "Receive") {
      this.router.navigateByUrl('WMS/GRNPosting');
    }
    else if (page == "Putaway") {
      this.router.navigateByUrl('WMS/WarehouseIncharge');
    }
    else if (page == "Quality") {
      this.router.navigateByUrl('WMS/QualityCheck');
    }
    else if (page == "Reserve") {
      this.router.navigateByUrl('WMS/MaterialReserveView');
    }
    else if (page == "Approve") {
      this.router.navigateByUrl('WMS/Home');
    }
    else if (page == "Issue") {
      this.router.navigateByUrl('WMS/MaterialIssueDashboard');
    }
    else if (page == "Count") {
      this.router.navigateByUrl('WMS/Cyclecount');
    }


  }

  nevigatefromrba(rba: rbamaster) {
    debugger;
    if (this.emp.roleid == "1") {
      if (rba.gate_entry) {
        this.router.navigateByUrl('WMS/SecurityCheck');
      }
      else {
        this.router.navigateByUrl('WMS/Home');
      }
    }
    if (this.emp.roleid == "2") {
      this.router.navigateByUrl('WMS/Home');
    }
    if (this.emp.roleid == "3") {
      this.router.navigateByUrl('WMS/Home');
    }
    if (this.emp.roleid == "4") {
      this.router.navigateByUrl('WMS/Home');
    }
    if (this.emp.roleid == "5") {
      this.router.navigateByUrl('WMS/Home');
    }
    if (this.emp.roleid == "6") {
      this.router.navigateByUrl('WMS/Home');
    }
    if (this.emp.roleid == "7") {
      if (rba.pmdashboard_view) {
        this.router.navigateByUrl('WMS/Dashboard');
      }
      else {
        this.router.navigateByUrl('WMS/Home');
      }
    }
    if (this.emp.roleid == "8") {
      this.router.navigateByUrl('WMS/Home');
    }
    if (this.emp.roleid == "9") {
      this.router.navigateByUrl('WMS/Home');
    }
    if (this.emp.roleid == "10") {
      this.router.navigateByUrl('WMS/Home');
    }
    if (this.emp.roleid == "11") {
      this.router.navigateByUrl('WMS/Home');
    }

  }
  setdefaultmenuview() {
    //for security
    this.showHome = false;
    this.showInbound = false;
    this.showoutwardinward = false;
    this.showoutwardinwardreturnable = false;
    this.showoutwardinwardnonreturnable = false;
    this.showoutbound = false;
  //

  }
  setmenuview() {
    //for security
    this.showHome = true;
   
    //
    if (isNullOrUndefined(this.rbalist)) {
      console.log("list16");
    }
    let currentrolerba = this.rbalist.filter(o => o.roleid == parseInt(this.emp.roleid));
    if (currentrolerba.length > 0) {
      var rba = currentrolerba[0];
      if (rba.inv_enquiry) {

      }
      if (rba.inv_reports) {


      }
      if (rba.gate_entry) {
        this.showInbound = true;
        this.showoutbound = true;
      }
      if (rba.receive_material) {
      }
      if (rba.put_away) {
       
      }
      if (rba.material_return) {
       
      }
      if (rba.material_transfer) {
        }
      if (rba.gate_pass) {
       
      }
      if (rba.gatepass_inout) {
        this.showoutwardinward = true;
        this.showoutwardinwardreturnable = true;
        this.showoutwardinwardnonreturnable = true;
      }
      if (rba.gatepass_approval) {
        
      }
      if (rba.material_issue) {
       
      }
      if (rba.material_request) {
       }
      if (rba.material_reservation) {
      
      }
      if (rba.abc_classification) {
      }
      if (rba.cyclecount_configuration) {
      }
      if (rba.cycle_counting) {
      }
      if (rba.cyclecount_approval) {
      }
      if (rba.admin_access) {

      }
      if (rba.masterdata_creation) {

      }
      if (rba.masterdata_updation) {

      }
      if (rba.masterdata_approval) {

      }
      if (rba.printbarcodes) {
       

      }
      if (rba.quality_check) {
      }
      if (rba.pmdashboard_view) {
      }
      if (rba.min) {

      }
      if (rba.direct_transfer_view) {

      }
      if (rba.notify_to_finance) {

      }
      if (rba.gr_process) {

      }
      if (rba.material_transfer_approval) {

      }
      if (rba.asn_view) {

      }
      if (rba.internal_stock_transfer) {

      }
    }
  }
  openoutlook() {
    window.location.href = "mailto:WMS_Project@in.yokogawa.com?subject=enter_subject&body=enter_content";
  }
  Nevigateafterbinding() {

  }
  bindMenu(url: string = "") {
    debugger;
    this.items = [];

    this.useritems = [
      { label: 'Log Out', icon: 'pi pi-fw pi-angle-right', command: () => this.logout() }
    ];
    if (this.emp.roleid == "1") {
      this.items = [];
      this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', visible: true, command: () => this.router.navigateByUrl('WMS/Home') });
      this.items.push({ label: 'Inbound', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', visible: true, command: () => this.router.navigateByUrl('WMS/SecurityCheck'), styleClass: 'active' });
      this.items.push({
        label: 'Gatepass', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', visible: true,
        items: [
          { label: 'Returnable', style: { 'font-weight': '600', 'width': '200px' }, icon: 'pi pi-fw pi-caret-right', visible: true, command: () => this.router.navigateByUrl('WMS/GatePassinout/1') },
          { label: 'Non Returnable', style: { 'font-weight': '600', 'width': '200px' }, icon: 'pi pi-fw pi-caret-right', visible: true, command: () => this.router.navigateByUrl('WMS/GatePassinout/2') },
        ]
      });
      this.items.push({ label: 'Outbound', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', visible: true });

      if (url == "default") {
        this.router.navigateByUrl('WMS/SecurityCheck');
      }
      else {
        if (!isNullOrUndefined(url) && url != "") {
          this.router.navigateByUrl(url);
        }
        else {
          this.router.navigateByUrl('WMS/SecurityCheck');
        }
        
      }
     
    }
    if (this.emp.roleid == "2") {//inventory enquiry
      this.items = [];
      this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', visible: true, command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' });
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
          { label: 'Safety Stock List', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SafetyStockList') },
          { label: 'Print Barcode', icon: 'pi pi-fw pi-print', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/PrintBarcode') },
        ]

      });
      if (url == "default") {
        this.router.navigateByUrl('WMS/Home');
      }
      else {
        if (!isNullOrUndefined(url) && url != "") {
          this.router.navigateByUrl(url);
        }
        else {
          this.router.navigateByUrl('WMS/Home');
        }

      }
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
          {
            label: 'Put Away',
            icon: 'pi pi-fw pi-bars',
            style: { 'font-weight': '600' },
            items: [
              { label: 'Receipt Put Away', style: { 'font-weight': '600', 'width': '300px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/WarehouseIncharge') },
              { label: 'Initial Stock Put Away', style: { 'font-weight': '600', 'width': '300px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/InitialStockPutAway') },
              { label: 'Material Return Put Away', style: { 'font-weight': '600', 'width': '300px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReturn') },
              { label: 'Intra Unit Transfer Put Away', style: { 'font-weight': '600', 'width': '300px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveMaterial') }
            ]
          },
          { label: 'Notify to finance', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Putawaynotify') }
        ]
      });
      this.items.push({
        label: 'Material Issue', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
        items: [
          { label: 'Material Issue', icon: 'pi pi-fw pi-bars', style: { 'width': '250px', 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/MaterialIssueDashboard') },
          { label: 'Gate Pass', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') },
          { label: 'Material Requisition Note', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MRNView') },
          { label: 'Material Tracking', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/POStatus') },
          { label: 'Intra Unit Material Issue', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveSTORequest') },
          { label: 'Sub Contract Material Issue', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveSubContractRequest') }
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
          { label: 'Safety Stock List', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SafetyStockList') },
          { label: 'Print Barcode', icon: 'pi pi-fw pi-print', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/PrintBarcode') },
        ]

      });
      this.items.push({ label: 'Internal Stock Transfer', icon: 'pi pi-fw pi-bars', style: { 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/Stocktransfer') });
      if (url == "default") {
        this.router.navigateByUrl('WMS/Home');
      }
      else {
        if (!isNullOrUndefined(url) && url != "") {
          this.router.navigateByUrl(url);
        }
        else {
          this.router.navigateByUrl('WMS/Home');
        }

      }
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
          {
            label: 'Put Away',
            icon: 'pi pi-fw pi-bars',
            style: { 'font-weight': '600' },
            items: [
              { label: 'Receipt Put Away', style: { 'font-weight': '600', 'width': '300px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/WarehouseIncharge') },
              { label: 'Initial Stock Put Away', style: { 'font-weight': '600', 'width': '300px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/InitialStockPutAway') },
              { label: 'Material Return Put Away', style: { 'font-weight': '600', 'width': '300px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReturn') },
              { label: 'Intra Unit Transfer Put Away', style: { 'font-weight': '600', 'width': '300px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveMaterial') }
            ]
          },
          { label: 'Notify to finance', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Putawaynotify') }
        ]
      });
      this.items.push({
        label: 'Material Issue', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
        items: [
          { label: 'Material Issue', icon: 'pi pi-fw pi-bars', style: { 'width': '250px', 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/MaterialIssueDashboard') },
          { label: 'Gate Pass', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') },
          { label: 'Material Requisition Note', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MRNView') },
          { label: 'Material Tracking', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/POStatus') },
          { label: 'Intra Unit Material Issue', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveSTORequest') },
          { label: 'Sub Contract Material Issue', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ReceiveSubContractRequest') }
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
          { label: 'Safety Stock List', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SafetyStockList') },
          { label: 'Print Barcode', icon: 'pi pi-fw pi-print', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/PrintBarcode') }

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
      this.items.push({ label: 'Internal Stock Transfer', icon: 'pi pi-fw pi-bars', style: { 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/Stocktransfer') });
      if (url == "default") {
        this.router.navigateByUrl('WMS/Home');
      }
      else {
        if (!isNullOrUndefined(url) && url != "") {
          this.router.navigateByUrl(url);
        }
        else {
          this.router.navigateByUrl('WMS/Home');
        }

      }

     
    }
    if (this.emp.roleid == "5") {//project manager (Material Requester)
      this.items = [];
      var subroles = [];
      if (isNullOrUndefined(this.userrolelist)) {
        console.log("list2");
      }
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
        mitem.items.push({ label: 'Gate Pass', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') });
      if (subroles && subroles.includes("2"))//Material Requestor
        mitem.items.push({ label: 'Material Request', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReqView') });
      if (subroles == null)//Material Requestor
        mitem.items.push({ label: 'Material Request', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReqView') });
      mitem.items.push({ label: 'Intra Unit Transfer', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/StockTransferOrder') });
      mitem.items.push({ label: 'Sub Contract', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SubContractTransfer') });
      this.items.push(mitem);
      this.items.push({ label: 'Material Reserve', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReserveView') });
      this.items.push({ label: 'Material Transfer', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialTransfer') });
      this.items.push({ label: 'Material Return', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReturnfromPm') });
      this.items.push({ label: 'Direct Shipment', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Directtransfer') });
      
      if (url == "default") {
        this.router.navigateByUrl('WMS/Dashboard');
      }
      else {
        if (!isNullOrUndefined(url) && url != "") {
          this.router.navigateByUrl(url);
        }
        else {
          this.router.navigateByUrl('WMS/Home');
        }

      }

    }
    if (this.emp.roleid == "6") {//dashboard
      this.items = [];
      this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' });
      this.items.push({ label: 'PM Dashboard', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-chart-bar', command: () => this.router.navigateByUrl('WMS/Dashboard') });
      if (url == "default") {
        this.router.navigateByUrl('WMS/Home');
      }
      else {
        if (!isNullOrUndefined(url) && url != "") {
          this.router.navigateByUrl(url);
        }
        else {
          this.router.navigateByUrl('WMS/Home');
        }

      }
    }
    if (this.emp.roleid == "7") {//admin
      this.items = [];
      this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' });
      //this.items.push({ label: 'Gate Pass', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') });
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
      this.items.push({
        label: 'Master Pages',
        icon: 'pi pi-fw pi-bars',
        style: { 'font-weight': '600' },
        items: [
          { label: 'Material Master', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/MaterialMaster') },
          { label: 'GatePass Reason Master', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/GatePassMaster') },
          { label: 'Plant Master', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/PlantMaster') },
          { label: 'Assign Role', icon: 'pi pi-fw pi-bars', style: { 'width': '200px' }, command: () => this.router.navigateByUrl('WMS/AssignRole') },
          { label: 'Assign Project Manager', style: { 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/AssignProjectManager') },
          { label: 'Miscellanous Master', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/MiscellanousReason') },
          { label: 'RBA Master', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/Assignrba') },
          { label: 'Store Master', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/StoreMaster') },
          { label: 'Rack Master', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/RackMaster') },
          { label: 'Bin Master', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/BinMaster') }
        ]
      });
      if (url == "default") {
        this.router.navigateByUrl('WMS/Home');
      }
      else {
        if (!isNullOrUndefined(url) && url != "") {
          this.router.navigateByUrl(url);
        }
        else {
          this.router.navigateByUrl('WMS/Home');
        }

      }
    }
    if (this.emp.roleid == "8") {//Approver     
      this.items = [];
      
        this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home') });
        this.items.push({ label: 'Manager Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassPMList') });
        this.items.push({ label: 'Finance Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassFMList') });
        if (url == "default") {
          this.router.navigateByUrl('WMS/Home');
        }
        else {
          if (!isNullOrUndefined(url) && url != "") {
            this.router.navigateByUrl(url);
          }
          else {
            this.router.navigateByUrl('WMS/Home');
          }

        }
      

    }
    if (this.emp.roleid == "9") {//Quality control
      this.items = [];
      this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' });
      this.items.push({ label: 'Quality Check', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/QualityCheck') });
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
      if (url == "default") {
        this.router.navigateByUrl('WMS/Home');
      }
      else {
        if (!isNullOrUndefined(url) && url != "") {
          this.router.navigateByUrl(url);
        }
        else {
          this.router.navigateByUrl('WMS/Home');
        }

      }
    }
    if (this.emp.roleid == "10") {//Finance
      this.items = [];
      this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), });
      this.items.push({ label: 'GR-Finance Process', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRNotification'), styleClass: 'active' });
      if (url == "default") {
        this.router.navigateByUrl('WMS/GRNotification');
      }
      else {
        if (!isNullOrUndefined(url) && url != "") {
          this.router.navigateByUrl(url);
        }
        else {
          this.router.navigateByUrl('WMS/GRNotification');
        }

      }
    }
    if (this.emp.roleid == "11") {//Project Manager
      this.items = [];
      this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home') });
      this.items.push({ label: 'PM Dashboard', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-chart-bar', command: () => this.router.navigateByUrl('WMS/Dashboard') });
      this.items.push({
        label: 'Approvals',
        icon: 'pi pi-fw pi-bars',
        style: { 'font-weight': '600' },
        items: [
          { label: 'Material Request Approval', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialRequestApproval') },
          { label: 'Material Transfer Approval', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/materialtransferapproval') },
          { label: 'Intra Unit Transfer Approval', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/STOApproval') },
          { label: 'Sub Contract Approval', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SubcontractApproval') }
        ]

      });
      this.items.push({ label: 'Direct Shipment', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Directtransfer') });
      this.items.push({ label: 'Assign Project', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/AssignProject') });
      this.items.push({ label: 'Sub Contract Annexure', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/AnnexureReport') });
      if (url == "default") {
        this.router.navigateByUrl('WMS/Home');
      }
      else {
        if (!isNullOrUndefined(url) && url != "") {
          this.router.navigateByUrl(url);
        }
        else {
          this.router.navigateByUrl('WMS/Home');
        }

      }
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
    let elementx: HTMLElement = document.getElementById("notlogged") as HTMLElement;
    elementx.hidden = false;
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
