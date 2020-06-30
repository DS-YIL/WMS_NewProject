import { Component, OnInit, ViewChild } from '@angular/core';
import { MenuItem } from 'primeng/api/menuitem';
import { Employee } from '../Models/Common.Model';
import { Router, RouterEvent, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent implements OnInit {
  constructor(private router: Router) { }
  @ViewChild('op1', { static: false }) overlaymodel;
  loggedin: boolean = false;
  items: MenuItem[];
  useritems: MenuItem[];
  username: string = "";
  emp = new Employee();
  ngOnInit() {
    debugger;
    if (localStorage.getItem("Employee")) {
      this.loggedin = true;
      let element: HTMLElement = document.getElementById("btnuser") as HTMLElement;
      element.hidden = false;
      this.emp = JSON.parse(localStorage.getItem("Employee")) as Employee;
      this.username = this.emp.name;
      this.bindMenu();
    }
    else {
      let element: HTMLElement = document.getElementById("btnuser") as HTMLElement;
      element.hidden = true;
      let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
      element1.hidden = true;
      this.router.navigateByUrl("WMS/Login");
    }
      
  }

  bindMenu() {
    debugger;
    this.items = [];
    this.items = [
      { label: 'Home', icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home') }
    ];
    this.useritems = [
      { label: 'Log Out', icon: 'pi pi-fw pi-angle-right', command: () => this.logout() }
    ];
    if (this.emp.roleid == "1") {
      this.items = [];
      this.items.push({ label: 'Security Check', icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SecurityCheck') });
      this.router.navigateByUrl('WMS/SecurityCheck');
    }
    if(this.emp.roleid == "2") {//inventory enquiry
      //this.items.push({ label: 'GRN Posting', icon: 'pi pi-fw pi-lock', command: () => this.router.navigateByUrl('WMS/GRNPosting') })
      this.router.navigateByUrl('WMS/Home');
    }
    if (this.emp.roleid == "3") {//inventory clerk
      this.items.push({ label: 'GRN Posting', icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRNPosting') });
      this.items.push({ label: '"Put Away"  Material wise', icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/WarehouseIncharge') });
      this.items.push({ label: 'Material Requests', icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReqView') });
      this.items.push({ label: 'Material Release', icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReleaseDashboard') });
      this.router.navigateByUrl('WMS/Home');
    }
    if (this.emp.roleid == "4") {//inventory manager
      this.items.push({ label: 'Material Requests', icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialIssueDashboard') });
      this.items.push({ label: '"Put Away"  Material wise', icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/WarehouseIncharge') });
      this.items.push({ label: 'Cycle Count', icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Cyclecount') });
      this.items.push({ label: 'Gate Pass', icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') });
      this.items.push({ label: 'Material Requests', icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReqView') });
      this.items.push({ label: 'Material Release', icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReleaseDashboard') });
      this.router.navigateByUrl('WMS/MaterialIssueDashboard');
    }
    if (this.emp.roleid == "5") {//project manager
      this.items.push({ label: 'PM Dashboard', icon: 'pi pi-fw pi-chart-bar', command: () => this.router.navigateByUrl('WMS/Dashboard') });
      this.items.push({ label: 'Material Requests', icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReqView') });
      this.items.push({ label: 'Material Reserved', icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReserveView') });
      this.router.navigateByUrl('WMS/Dashboard');
    }
    if (this.emp.roleid == "6") {//dashboard
      this.items.push({ label: 'PM Dashboard', icon: 'pi pi-fw pi-chart-bar', command: () => this.router.navigateByUrl('WMS/Dashboard') });
      this.router.navigateByUrl('WMS/Dashboard');
    }
    if (this.emp.roleid == "7") {//admin
      this.items.push({ label: 'Gate Pass', icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') });
      this.items.push({ label: 'Material Requests', icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReqView') });
      this.items.push({
        label: 'Inventory Ageing',
        icon: 'pi pi-fw pi-bars',
        items: [
          { label: 'Obsolete', icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/ObsoleteInventoryMovement') },
          { label: 'Excess', icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/ExcessInventoryMovement') },
        ]
      });
      this.items.push({
        label: 'ABC Analysis',
        icon: 'pi pi-fw pi-bars',
        items: [
          { label: 'ABC Category', icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/ABCCategory') },
          { label: 'ABC Analysis', icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/ABCAnalysis') },
        ]
      });
      this.items.push({
        label: 'Cycle count',
        icon: 'pi pi-fw pi-bars',
        items: [
          { label: 'Cycle Count', icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/Cyclecount') },
          { label: 'Cycle Config', icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/Cycleconfig') },
        ]
      });
      this.items.push({ label: 'Material Tracking', icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/POStatus') });
      this.items.push({
        label: 'Other',
        icon: 'pi pi-fw pi-bars',
        items: [
          { label: 'FIFO LIst', icon: 'pi pi-fw pi-bars', style: { 'width': '200px' }, command: () => this.router.navigateByUrl('WMS/FIFOList') },
          { label: 'Inventory Movement', icon: 'pi pi-fw pi-bars', style: { 'width': '200px' }, command: () => this.router.navigateByUrl('WMS/InventoryMovement') },
        ]
      });
      this.router.navigateByUrl('/WMS/Home');
    }
    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = false;
   
   
  }

  logout() {
    localStorage.removeItem("Employee");
    localStorage.removeItem("Roles");
    this.loggedin = false;
    let element: HTMLElement = document.getElementById("btnuser") as HTMLElement;
    element.hidden = true;
    let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    element1.hidden = true;
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
