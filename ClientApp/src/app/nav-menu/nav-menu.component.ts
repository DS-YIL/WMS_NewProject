import { Component, OnInit, ViewChild } from '@angular/core';
import { MenuItem } from 'primeng/api/menuitem';
import { Employee, userAcessNamesModel } from '../Models/Common.Model';
import { Router, RouterEvent, NavigationEnd, ActivatedRoute } from '@angular/router';
import { filter } from 'rxjs/operators';
import { wmsService } from '../WmsServices/wms.service';
import { pageModel } from '../Models/WMS.Model';

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
  tempitems: MenuItem[];
  otheritems: MenuItem[];
  useritems: MenuItem[];
  pagelist: pageModel[] = [];
  notificationitems: MenuItem[];
  username: string = "";
  emp = new Employee();
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
  ngOnInit() {
    debugger;
    this.notificationitems = [];
    this.approverstatus = "Pending";
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
      if (localStorage.getItem("Employee")) {
        this.loggedin = true;
        let element: HTMLElement = document.getElementById("btnuser") as HTMLElement;
        element.hidden = false;
        this.emp = JSON.parse(localStorage.getItem("Employee")) as Employee;
        if (localStorage.getItem("pages")) {
          this.pagelist = JSON.parse(localStorage.getItem("pages")) as pageModel[];
        }
        if (this.ismailurl && this.emp.roleid != "8") {
          this.logout();
        }
        this.username = this.emp.name;
        if (localStorage.getItem("userroles")) {
          this.userrolelist = JSON.parse(localStorage.getItem("userroles")) as userAcessNamesModel[];
          this.bindMenuwithoutrole();
        }
        else {
          this.bindMenu();
        }
        
        if (this.emp.roleid != "8") {
          let elementx: HTMLElement = document.getElementById("btnuser1") as HTMLElement;
          elementx.hidden = false;
          let elementy: HTMLElement = document.getElementById("btnuser2") as HTMLElement;
          elementy.hidden = false;
          this.getGatePassList();
        }
        
      }
      else {
        let element: HTMLElement = document.getElementById("btnuser") as HTMLElement;
        element.hidden = true;
        let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
        element1.hidden = true;
        let elementx: HTMLElement = document.getElementById("btnuser1") as HTMLElement;
        elementx.hidden = true;
        let elementy: HTMLElement = document.getElementById("btnuser2") as HTMLElement;
        elementy.hidden = true;
        this.router.navigateByUrl("WMS/Login");
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
       
          this.items.push({ label: 'Inbound Shipment', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SecurityCheck') });
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
        
       
      }
      else if (item.roleid == 3) {
       
          this.items.push({ label: 'Receipts', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRNPosting') });
          this.items.push({ label: 'Quality Check', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/QualityCheck') });
          this.items.push({ label: '"Put Away"  Material wise', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/WarehouseIncharge') });

       

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
        style: { 'width': '300px'},
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


  bindMenu() {
    debugger;
    this.items = [];
    this.items = [
      { label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' }
    ];
    this.useritems = [
      { label: 'Log Out', icon: 'pi pi-fw pi-angle-right', command: () => this.logout() }
    ];
    if (this.emp.roleid == "1") {
      this.items = [];
      this.items.push({ label: 'Inbound Shipment', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SecurityCheck'), styleClass: 'active' });
      this.items.push({
        label: 'Gatepass', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
        items: [
          { label: 'Returnable', style: { 'font-weight': '600', 'width': '200px' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/GatePassinout/1') },
          { label: 'Non Returnable', style: { 'font-weight': '600', 'width': '200px' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/GatePassinout/2') },
        ]
      });
      this.router.navigateByUrl('WMS/SecurityCheck');
    }
    if (this.emp.roleid == "2") {//inventory enquiry
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

      //this.items.push({ label: 'GRN Posting', icon: 'pi pi-fw pi-lock', command: () => this.router.navigateByUrl('WMS/GRNPosting') })
      this.router.navigateByUrl('WMS/Home');
    }
    if (this.emp.roleid == "3") {//inventory clerk
      this.items.push({ label: 'Receipts', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRNPosting') });
      this.items.push({ label: 'Quality Check', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/QualityCheck') });
      this.items.push({ label: '"Put Away"  Material wise', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/WarehouseIncharge') });
     // this.items.push({ label: 'Material Requests', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReqView') });
      //this.items.push({ label: 'Material Release', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReleaseDashboard') });
      this.router.navigateByUrl('WMS/Home');
    }
    if (this.emp.roleid == "4") {//inventory manager
      this.items.push({ label: 'Material Issue', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialIssueDashboard') });
     // this.items.push({ label: '"Put Away"  Material wise', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/WarehouseIncharge') });
      this.items.push({ label: 'Cycle Count', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Cyclecount') });
      this.items.push({ label: 'Gate Pass', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') });
      this.items.push({ label: 'Material Return', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReturn') });
      this.router.navigateByUrl('WMS/Home');
    }
    if (this.emp.roleid == "5") {//project manager
      this.items.push({ label: 'PM Dashboard', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-chart-bar', command: () => this.router.navigateByUrl('WMS/Dashboard') });
      this.items.push({ label: 'Material Requests', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReqView') });
      this.items.push({ label: 'Material Reserved', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReserveView') });
      this.items.push({ label: 'Material Transfer', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialTransfer') });
      this.router.navigateByUrl('WMS/Dashboard');
    }
    if (this.emp.roleid == "6") {//dashboard
      this.items.push({ label: 'PM Dashboard', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-chart-bar', command: () => this.router.navigateByUrl('WMS/Dashboard') });
      this.router.navigateByUrl('WMS/Dashboard');
    }
    if (this.emp.roleid == "7") {//admin
      this.items.push({ label: 'Gate Pass', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') });
      this.items.push({ label: 'Internal Stock Transfer', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Stocktransfer') });
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
        label: 'Other',
        icon: 'pi pi-fw pi-bars',
        style: {'font-weight': '600' },
        items: [
          { label: 'FIFO LIst', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/FIFOList') },
          { label: 'Inventory Movement', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/InventoryMovement') },
          { label: 'Stores Return Note', icon: 'pi pi-fw pi-print', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/StoresReturnNote') },
          { label: 'Stock Card Print', icon: 'pi pi-fw pi-print', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/StockCardPrint') }
          //{ label: 'Internal Stock Transfer', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/Stocktransfer') },
          
         // { label: 'AssignRole', icon: 'pi pi-fw pi-bars', style: { 'width': '200px' }, command: () => this.router.navigateByUrl('WMS/AssignRole') },
          //{ label: 'Bin Status Report', style: { 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/BinStatusReport') }
        ]
      });
      //this.items.push({ label: 'Bin Status Report', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/BinStatusReport') });
      //this.items.push({ label: 'BarCode', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Barcode') });

      this.router.navigateByUrl('/WMS/Home');
    }
    if (this.emp.roleid == "8") {//Approver     
      this.items = [];
      if (localStorage.getItem("requestedpage")) {
        var page = localStorage.getItem('requestedpage');
        if (page == "GatePassPMList") {
          this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home')});
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
          this.items.push({ label: 'Finance Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassFMList')});
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

    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = false;
   
   
  }

  logout() {
    localStorage.removeItem("Employee");
    localStorage.removeItem("userroles");
    this.loggedin = false;
    let element: HTMLElement = document.getElementById("btnuser") as HTMLElement;
    element.hidden = true;
    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = true;
    let elementx: HTMLElement = document.getElementById("btnuser1") as HTMLElement;
    elementx.hidden = true;
    let elementy: HTMLElement = document.getElementById("btnuser2") as HTMLElement;
    elementy.hidden = true;
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
}
