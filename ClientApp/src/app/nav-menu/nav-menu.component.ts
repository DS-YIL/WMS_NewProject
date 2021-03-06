import { Component, OnInit, ViewChild } from '@angular/core';
import { MenuItem } from 'primeng/api/menuitem';
import { DataSharingService } from '../datasharing.service';
import { Employee, userAcessNamesModel, rbamaster } from '../Models/Common.Model';
import { Router, RouterEvent, NavigationEnd, ActivatedRoute } from '@angular/router';
import { filter } from 'rxjs/operators';
import { constants } from '../Models/WMSConstants';
import { wmsService } from '../WmsServices/wms.service';
import { environment } from 'src/environments/environment'
import { pageModel, ddlmodel } from '../Models/WMS.Model';
import { isNullOrUndefined } from 'util';
import { emit } from 'cluster';
import * as $ from 'jquery';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent implements OnInit {
  finalmenuitems: MenuItem[];
  username: string;
  profileimage: string = "";
  loggedinas: string = "";
  imgurl = environment.profileimgUrl;
  constructor(private router: Router, private route: ActivatedRoute, private wmsService: wmsService, public constants: constants, private dataSharingService: DataSharingService) {
    this.dataSharingService.meniitems.subscribe(value => {
      this.finalmenuitems = value;
    });
    this.dataSharingService.usernameshare.subscribe(value => {
      this.username = value;
    });
    this.dataSharingService.imageurlshare.subscribe(value => {
      this.profileimage = value;
    });
    this.dataSharingService.loggedroleshare.subscribe(value => {
      this.loggedinas = value;
    });
  }
  @ViewChild('op1', { static: false }) overlaymodel;
  loggedin: boolean = false;
  items: MenuItem[];
  tempitems: MenuItem[];
  otheritems: MenuItem[];
  useritems: MenuItem[];
  pagelist: pageModel[] = [];
  notificationitems: MenuItem[];
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
    if (localStorage.getItem("Employee")) {
      this.loggedin = true;
      //let element: HTMLElement = document.getElementById("btnuser") as HTMLElement;
      //element.hidden = false;
      //let elementx: HTMLElement = document.getElementById("notlogged") as HTMLElement;
      //elementx.hidden = true;
      $("#btnuser").show();
      $("#notlogged").hide();
      $("#menudiv").show();
      this.emp = JSON.parse(localStorage.getItem("Employee")) as Employee;
      this.dataSharingService.imageurlshare.next(this.imgurl + this.emp.employeeno + ".jpg");
      this.loggeduserdata.push(this.emp);
      if (localStorage.getItem("pages")) {
        this.pagelist = JSON.parse(localStorage.getItem("pages")) as pageModel[];
      }
      if (this.ismailurl && this.emp.roleid != "8") {
        this.logout();
      }
      this.dataSharingService.usernameshare.next(this.emp.name);
      if (localStorage.getItem("userroles") && this.emp.roleid == "0") {
        this.userrolelist = JSON.parse(localStorage.getItem("userroles")) as userAcessNamesModel[];
        this.getrbalistforwithoutrole();
      }
      else {
        if (localStorage.getItem("userroles")) {
          this.userrolelist = JSON.parse(localStorage.getItem("userroles")) as userAcessNamesModel[];

        }
        else {
          this.userrolelist = JSON.parse(localStorage.getItem("allroles")) as userAcessNamesModel[];
        }
        var rid = this.emp.roleid;
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
            this.imgurl + this.emp.employeeno + ".jpg"
            this.dataSharingService.loggedroleshare.next("Logged in as " + this.rolename);
          }

        }
        if (sessionStorage.getItem("userdashboardpage")) {
          this.getrbalist();
        }
        else {
          if (eurl.includes("/Email")) {
            this.getrbalist(eurl);

          }
          else {
            this.getrbalist();
          }

        }

      }

    }
    else {
      //let element: HTMLElement = document.getElementById("btnuser") as HTMLElement;
      //element.hidden = true;
      //let elementx: HTMLElement = document.getElementById("notlogged") as HTMLElement;
      //elementx.hidden = false;
      //let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
      //element1.hidden = true;
      $("#btnuser").hide();
      $("#notlogged").show();
      $("#menudiv").hide();

      this.router.navigateByUrl("WMS/Login");
    }
  }
  getrbalist(eurl: string = "") {
    debugger;
    this.wmsService.getrbadata().subscribe(data => {
      if (data.length > 0) {
        debugger;
        this.rbalist = data;
        localStorage.setItem('rbalist', JSON.stringify(data));
        if (!isNullOrUndefined(eurl) && eurl != "") {
          this.bindMenuForEmailNav(eurl);
        }
        else {
          this.bindmenubyrba();
        }


      }
    })

  }
  getrbalistforwithoutrole() {
    debugger;
    this.wmsService.getrbadata().subscribe(data => {
      if (data.length > 0) {
        debugger;
        this.rbalist = data;
        localStorage.setItem('rbalist', JSON.stringify(data));
        this.bindMenuwithoutrole();
      }
    })

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
    debugger;
    if (eurl.includes("/Email")) {
      //let element: HTMLElement = document.getElementById("btnuser") as HTMLElement;
      //element.hidden = false;
      //let elementx: HTMLElement = document.getElementById("notlogged") as HTMLElement;
      //elementx.hidden = true;
      //let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
      //element1.hidden = false;
      $("#btnuser").show();
      $("#notlogged").hide();
      $("#menudiv").show();
      if (eurl.includes("/Email/GRNPosting?GateEntryNo")) {
        var found = false;
        this.userrolelist.forEach(item => {
          if (!found) {
            var dt1 = this.rbalist.filter(function (element, index) {
              return (element.receive_material && element.roleid == item.roleid);
            });
            if (dt1.length > 0) {
              this.emp.roleid = String(dt1[0].roleid);
              this.setrolename(this.emp.roleid);
              localStorage.removeItem('Employee');
              localStorage.setItem('Employee', JSON.stringify(this.emp));
              found = true;
              this.bindmenubyrba(eurl);
              return false;
            }

          }

        })
        if (!found) {
          alert("You are not authorized to view this page.")
          this.bindmenubyrba();
        }
      }
      else if (eurl.includes("/Email/GRNPosting?GRNo")) {
        debugger;
        var found = false;
        this.userrolelist.forEach(item => {
          if (!found) {
            var dt1 = this.rbalist.filter(function (element, index) {
              return (element.receive_material && element.roleid == item.roleid);
            });
            if (dt1.length > 0) {
              this.emp.roleid = String(dt1[0].roleid);
              this.setrolename(this.emp.roleid);
              localStorage.removeItem('Employee');
              localStorage.setItem('Employee', JSON.stringify(this.emp));
              found = true;
              this.bindmenubyrba(eurl);
              return false;
            }
          }

        })
        if (!found) {
          alert("You are not authorized to view this page.")
          this.bindmenubyrba();
        }
      }
      else if (eurl.includes("/Email/QualityCheck?GRNo")) {
        debugger;
        var found = false;
        this.userrolelist.forEach(item => {
          if (!found) {
            var dt1 = this.rbalist.filter(function (element, index) {
              return (element.quality_check && element.roleid == item.roleid);
            });
            if (dt1.length > 0) {
              this.emp.roleid = String(dt1[0].roleid);
              this.setrolename(this.emp.roleid);
              localStorage.removeItem('Employee');
              localStorage.setItem('Employee', JSON.stringify(this.emp));
              found = true;
              this.bindmenubyrba(eurl);
              return false;
            }
          }
        })
        if (!found) {
          alert("You are not authorized to view this page.")
          this.bindmenubyrba();
        }
      }
      else if (eurl.includes("/Email/MaterialIssueDashboard?ReqId")) {
        var found = false;
        this.userrolelist.forEach(item => {
          if (!found) {
            var dt1 = this.rbalist.filter(function (element, index) {
              return (element.material_issue && element.roleid == item.roleid);
            });
            if (dt1.length > 0) {
              this.emp.roleid = String(dt1[0].roleid);
              this.setrolename(this.emp.roleid);
              localStorage.removeItem('Employee');
              localStorage.setItem('Employee', JSON.stringify(this.emp));
              found = true;
              this.bindmenubyrba(eurl);
              return false;
            }

          }

        })
        if (!found) {
          alert("You are not authorized to view this page.")
          this.bindmenubyrba();
        }
      }
      else if (eurl.includes("/Email/ApproveSTOMaterial?ReqId")) {
        debugger;
        var found = false;
        this.userrolelist.forEach(item => {
          if (!found) {
            var dt1 = this.rbalist.filter(function (element, index) {
              return (element.materialrequest_approval && element.roleid == item.roleid);
            });
            if (dt1.length > 0) {
              found = true;
              this.emp.roleid = String(dt1[0].roleid);
              this.setrolename(this.emp.roleid);
              localStorage.removeItem('Employee');
              localStorage.setItem('Employee', JSON.stringify(this.emp));
              this.bindmenubyrba(eurl);
              return false;
            }
          }

        })
        if (!found) {
          alert("You are not authorized to view this page.")
          this.bindmenubyrba();
        }

      }
      else if (eurl.includes("/Email/IssueSTOMaterial?ReqId")) {
        var found = false;
        this.userrolelist.forEach(item => {
          if (!found) {
            var dt1 = this.rbalist.filter(function (element, index) {
              return (element.material_issue && element.roleid == item.roleid);
            });
            if (dt1.length > 0) {
              this.emp.roleid = String(dt1[0].roleid);
              this.setrolename(this.emp.roleid);
              localStorage.removeItem('Employee');
              localStorage.setItem('Employee', JSON.stringify(this.emp));
              found = true;
              this.bindmenubyrba(eurl);
            }
          }
        })
        if (!found) {
          alert("You are not authorized to view this page.")
          this.bindmenubyrba();
        }
      }
      else if (eurl.includes("/Email/grnputaway?GRNo")) {
        var found = false;
        this.userrolelist.forEach(item => {
          debugger;
          if (!found) {
            var dt1 = this.rbalist.filter(function (element, index) {
              return (element.put_away && element.roleid == item.roleid);
            });
            if (dt1.length > 0) {
              this.emp.roleid = String(dt1[0].roleid);
              this.setrolename(this.emp.roleid);
              localStorage.removeItem('Employee');
              localStorage.setItem('Employee', JSON.stringify(this.emp));
              found = true;
              this.bindmenubyrba(eurl);
            }
          }

        })
        if (!found) {
          alert("You are not authorized to view this page.")
          this.bindmenubyrba();
        }
      }
      else if (eurl.includes("/Email/ApprovalSubcontractingMaterial?ReqId")) {
        debugger;
        this.userrolelist.forEach(item => {
          var dt1 = this.rbalist.filter(function (element, index) {
            return (element.materialrequest_approval && element.roleid == item.roleid);
          });
          if (dt1.length > 0) {
            this.emp.roleid = String(dt1[0].roleid);
            this.setrolename(this.emp.roleid);
            localStorage.removeItem('Employee');
            localStorage.setItem('Employee', JSON.stringify(this.emp));
            this.bindmenubyrba(eurl);
            return false;
          }
        })
      }


      else if (eurl.includes("/Email/IssueSubcontractingMaterial?ReqId")) {
        this.userrolelist.forEach(item => {
          var dt1 = this.rbalist.filter(function (element, index) {
            return (element.material_issue && element.roleid == item.roleid);
          });
          if (dt1.length > 0) {
            this.emp.roleid = String(dt1[0].roleid);
            this.setrolename(this.emp.roleid);
            localStorage.removeItem('Employee');
            localStorage.setItem('Employee', JSON.stringify(this.emp));
            this.bindmenubyrba(eurl);
            return false;
          }
        })
      }


      else if (eurl.includes("/Email/STOMaterialPutaway?ReqId")) {
        this.userrolelist.forEach(item => {
          var dt1 = this.rbalist.filter(function (element, index) {
            return (element.put_away && element.roleid == item.roleid);
          });
          if (dt1.length > 0) {
            this.emp.roleid = String(dt1[0].roleid);
            this.setrolename(this.emp.roleid);
            localStorage.removeItem('Employee');
            localStorage.setItem('Employee', JSON.stringify(this.emp));
            this.bindmenubyrba(eurl);
            return false;
          }
        })
      }


      else if (eurl.includes("/Email/SubcontractMaterialPutaway?ReqId")) {
        this.userrolelist.forEach(item => {
          var dt1 = this.rbalist.filter(function (element, index) {
            return (element.put_away && element.roleid == item.roleid);
          });
          if (dt1.length > 0) {
            this.emp.roleid = String(dt1[0].roleid);
            this.setrolename(this.emp.roleid);
            localStorage.removeItem('Employee');
            localStorage.setItem('Employee', JSON.stringify(this.emp));
            this.bindmenubyrba(eurl);
            return false;
          }
        })
      }

      else if (eurl.includes("/Email/MaterialReqView?ReqId")) {
        this.userrolelist.forEach(item => {
          var dt1 = this.rbalist.filter(function (element, index) {
            return (element.material_request && element.roleid == item.roleid);
          });
          if (dt1.length > 0) {
            this.emp.roleid = String(dt1[0].roleid);
            this.setrolename(this.emp.roleid);
            localStorage.removeItem('Employee');
            localStorage.setItem('Employee', JSON.stringify(this.emp));
            this.bindmenubyrba(eurl);
            return false;
          }
        })
      }
      else if (eurl.includes("/Email/GatePassPMList?GateId")) {
        this.userrolelist.forEach(item => {
          var dt1 = this.rbalist.filter(function (element, index) {
            return (element.gatepass_approval && element.roleid == item.roleid);
          });
          if (dt1.length > 0) {
            this.emp.roleid = String(dt1[0].roleid);
            this.setrolename(this.emp.roleid);
            localStorage.removeItem('Employee');
            localStorage.setItem('Employee', JSON.stringify(this.emp));
            this.bindmenubyrba(eurl);
            return false;
          }
        })

      }
      else if (eurl.includes("/Email/GatePassAuthList?GateId")) {
        this.userrolelist.forEach(item => {
          if (item.roleid == 4) {
            this.emp.roleid = "4";
            this.setrolename(this.emp.roleid);
            localStorage.removeItem('Employee');
            localStorage.setItem('Employee', JSON.stringify(this.emp));
            this.bindmenubyrba(eurl);
            return false;

          }


        })

      }

      else if (eurl.includes("/Email/GatePass?GateId")) {
        this.userrolelist.forEach(item => {
          var dt1 = this.rbalist.filter(function (element, index) {
            return (element.material_issue && element.roleid == item.roleid);
          });
          if (dt1.length > 0) {
            this.emp.roleid = String(dt1[0].roleid);
            this.setrolename(this.emp.roleid);
            localStorage.removeItem('Employee');
            localStorage.setItem('Employee', JSON.stringify(this.emp));
            this.bindmenubyrba(eurl);
            return false;
          }
        })
      }
      else if (eurl.includes("/Email/GatePass?GatepassId")) {
        this.userrolelist.forEach(item => {
          var dt1 = this.rbalist.filter(function (element, index) {
            return (element.gate_pass && element.roleid == item.roleid);
          });
          if (dt1.length > 0) {
            this.emp.roleid = String(dt1[0].roleid);
            this.setrolename(this.emp.roleid);
            localStorage.removeItem('Employee');
            localStorage.setItem('Employee', JSON.stringify(this.emp));
            this.bindmenubyrba(eurl);
            return false;
          }
        })
      }

      else if (eurl.includes("/Email/GatePass?NPGatepassId")) {
        this.userrolelist.forEach(item => {
          var dt1 = this.rbalist.filter(function (element, index) {
            return (element.material_issue && element.roleid == item.roleid);
          });
          if (dt1.length > 0) {
            this.emp.roleid = String(dt1[0].roleid);
            this.setrolename(this.emp.roleid);
            localStorage.removeItem('Employee');
            localStorage.setItem('Employee', JSON.stringify(this.emp));
            this.bindmenubyrba(eurl);
            return false;
          }
        })
      }



      //Purpose: << Gate Pass PM approver to FM approver >>

      else if (eurl.includes("/Email/GatePassFMList?GateId")) {
        this.userrolelist.forEach(item => {
          var dt1 = this.rbalist.filter(function (element, index) {
            return (element.gatepass_approval && element.roleid == item.roleid);
          });
          if (dt1.length > 0) {
            this.emp.roleid = String(dt1[0].roleid);
            this.setrolename(this.emp.roleid);
            localStorage.removeItem('Employee');
            localStorage.setItem('Employee', JSON.stringify(this.emp));
            this.bindmenubyrba(eurl);
            return false;
          }
        })

      }


      //Purpose: << Notify to Finance>>

      else if (eurl.includes("/Email/GRNotification?GRNo")) {
        this.userrolelist.forEach(item => {
          var dt1 = this.rbalist.filter(function (element, index) {
            return (element.gr_process && element.roleid == item.roleid);
          });
          if (dt1.length > 0) {
            this.emp.roleid = String(dt1[0].roleid);
            this.setrolename(this.emp.roleid);
            localStorage.removeItem('Employee');
            localStorage.setItem('Employee', JSON.stringify(this.emp));
            this.bindmenubyrba(eurl);
            return false;
          }
        })
      }
      else if (eurl.includes("/Email/materialtransferapproval?transferid")) {

        debugger;
        this.userrolelist.forEach(item => {
          var dt1 = this.rbalist.filter(function (element, index) {
            return (element.material_transfer_approval && element.roleid == item.roleid);
          });
          if (dt1.length > 0) {
            this.emp.roleid = String(dt1[0].roleid);
            this.setrolename(this.emp.roleid);
            localStorage.removeItem('Employee');
            localStorage.setItem('Employee', JSON.stringify(this.emp));
            this.bindmenubyrba(eurl);
            return false;
          }
        })
      }
      else if (eurl.includes("/Email/materialtransfer?transferid")) {
        debugger;
        this.userrolelist.forEach(item => {
          var dt1 = this.rbalist.filter(function (element, index) {
            return (element.material_transfer && element.roleid == item.roleid);
          });
          if (dt1.length > 0) {
            this.emp.roleid = String(dt1[0].roleid);
            this.setrolename(this.emp.roleid);
            localStorage.removeItem('Employee');
            localStorage.setItem('Employee', JSON.stringify(this.emp));
            this.bindmenubyrba(eurl);
            return false;
          }
        })
      }
      else if (eurl.includes("/Email/materialreturndashboard?returnid")) {
        debugger;
        this.userrolelist.forEach(item => {
          var dt1 = this.rbalist.filter(function (element, index) {
            return (element.put_away && element.roleid == item.roleid);
          });
          if (dt1.length > 0) {
            this.emp.roleid = String(dt1[0].roleid);
            this.setrolename(this.emp.roleid);
            localStorage.removeItem('Employee');
            localStorage.setItem('Employee', JSON.stringify(this.emp));
            this.bindmenubyrba(eurl);
            return false;
          }
        })
      }
      else if (eurl.includes("/Email/MaterialRequestApproval?ReqId")) {
        debugger;
        this.userrolelist.forEach(item => {
          var dt1 = this.rbalist.filter(function (element, index) {
            return (element.materialrequest_approval && element.roleid == item.roleid);
          });
          if (dt1.length > 0) {
            this.emp.roleid = String(dt1[0].roleid);
            this.setrolename(this.emp.roleid);
            localStorage.removeItem('Employee');
            localStorage.setItem('Employee', JSON.stringify(this.emp));
            this.bindmenubyrba(eurl);
            return false;
          }
        })
      }
      else if (eurl.includes("/Email/MaterialRequestApprovalNP?ReqId")) {
        debugger;
        this.userrolelist.forEach(item => {
          var dt1 = this.rbalist.filter(function (element, index) {
            return (element.materialrequest_approval && element.roleid == item.roleid);
          });
          if (dt1.length > 0) {
            this.emp.roleid = String(dt1[0].roleid);
            this.setrolename(this.emp.roleid);
            localStorage.removeItem('Employee');
            localStorage.setItem('Employee', JSON.stringify(this.emp));
            this.bindmenubyrba(eurl);
            return false;
          }
        })
      }
      else if (eurl.includes("/Email/ApproveSTOMaterialNP?ReqId")) {
        debugger;
        this.userrolelist.forEach(item => {
          var dt1 = this.rbalist.filter(function (element, index) {
            return (element.materialrequest_approval && element.roleid == item.roleid);
          });
          if (dt1.length > 0) {
            this.emp.roleid = String(dt1[0].roleid);
            this.setrolename(this.emp.roleid);
            localStorage.removeItem('Employee');
            localStorage.setItem('Employee', JSON.stringify(this.emp));
            this.bindmenubyrba(eurl);
            return false;
          }
        })

      }
      else if (eurl.includes("/Email/ApprovalSubcontractingMaterialNP?ReqId")) {
        debugger;
        this.userrolelist.forEach(item => {
          var dt1 = this.rbalist.filter(function (element, index) {
            return (element.materialrequest_approval && element.roleid == item.roleid);
          });
          if (dt1.length > 0) {
            this.emp.roleid = String(dt1[0].roleid);
            this.setrolename(this.emp.roleid);
            localStorage.removeItem('Employee');
            localStorage.setItem('Employee', JSON.stringify(this.emp));
            this.bindmenubyrba(eurl);
            return false;
          }
        })
      }
    }

  }
  bindMenuwithoutrole() {
    this.items = [];
    this.useritems = [
      { label: 'Log Out', icon: 'pi pi-fw pi-angle-right', command: () => this.logout() }
    ];
    this.items.push({ label: 'Home', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-home', command: () => this.router.navigateByUrl('WMS/Home'), styleClass: 'active' });
    //let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    //element1.hidden = false;
    this.dataSharingService.meniitems.next(this.items);
    this.dataSharingService.loggedroleshare.next("");
    $("#menudiv").show();
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
  bindmenubyrba(eurl: string = "") {
    debugger;
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
            { label: 'Project Left Over Stock', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialsReturnDashboard') },
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
            { label: 'Project Left Over Stock', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialsReturnDashboard') },
            { label: 'Material Transfer', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialTransferDashboard') },
            { label: 'Inventory Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/inventoryreport') },
            { label: 'GR Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GRReports') },
            { label: 'Outward/Inward Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/outinDashboard') },
            { label: 'Bin Status Report', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/BinStatusReport') },
            { label: 'Safety Stock List', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SafetyStockList') },
            { label: 'YGSGR', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/YGSGR') },
            { label: 'Print Barcode', icon: 'pi pi-fw pi-print', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/PrintBarcode') },
            { label: 'Gatepass Report', icon: 'pi pi-fw pi-print', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/GPReport') }

          ]

        });
      }
      if (rba.gate_entry) {
        this.items.push({ label: 'Inbound', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SecurityCheck') });
        //this.items.push({ label: 'Outbound', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars' });
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
      if (rba.receive_material || rba.put_away || rba.notify_to_finance) {
        this.items.push(receiptsitems);
      }

      var matrequestitems = {
        label: 'Material Request',
        icon: 'pi pi-fw pi-bars',
        style: { 'font-weight': '600' },
        visible: (rba.gate_pass || rba.material_request),
        items: []
      };
      var subroles = [];
      if (this.userrolelist.filter(li => li.roleid == 5).length > 0) {
        subroles = this.userrolelist.filter(li => li.roleid == 5)[0]["subroleid"];
      }
      if (rba.gate_pass) {
        if (subroles && subroles.includes("1"))//GatePassRequester
          matrequestitems.items.push({ label: 'Gate Pass', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePass') });

      }
      if (rba.material_request) {
        if (subroles && subroles.includes("2"))//Material Requestor
          matrequestitems.items.push({ label: 'Material Request', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReqView') });
        if (subroles == null)//Material Requestor
          matrequestitems.items.push({ label: 'Material Request', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReqView') });
        matrequestitems.items.push({ label: 'Intra Unit Transfer', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/StockTransferOrder') });
        matrequestitems.items.push({ label: 'Sub Contract', style: { 'font-weight': '600', 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SubContractTransfer') });
      }
      if (rba.gate_pass || rba.material_request) {
        this.items.push(matrequestitems);
      }
      if (rba.gatepass_inout) {
        this.items.push({
          label: 'Gatepass', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars',
          items: [
            { label: 'Returnable', style: { 'font-weight': '600', 'width': '200px' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/GatePassinout/1') },
            { label: 'Non Returnable', style: { 'font-weight': '600', 'width': '200px' }, icon: 'pi pi-fw pi-caret-right', command: () => this.router.navigateByUrl('WMS/GatePassinout/2') },
          ]
        });

      }
      //if (rba.gatepass_approval) {
      //  this.items.push({ label: 'Finance Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassFMList') });
      //}
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
      if (rba.material_return) {
        this.items.push({ label: 'Material Return', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialReturnfromPm') });

      }
      if (rba.material_transfer) {
        this.items.push({ label: 'Material Transfer', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialTransfer') });
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
      var invmanageitems = {
        label: 'Inventory Management',
        icon: 'pi pi-fw pi-bars',
        style: { 'font-weight': '600' },
        visible: (rba.inventory_management),
        items: []
      };
      //var cyclecountitems = {
      //  label: 'Cycle Count',
      //  icon: 'pi pi-fw pi-bars',
      //  style: { 'font-weight': '600' },
      //  visible: (rba.cyclecount_configuration || rba.cycle_counting || rba.cyclecount_approval),
      //  items: []
      //};
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
      //if (rba.cyclecount_configuration) {
      //  cyclecountitems.items.push({ label: 'Cycle Count config', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Cycleconfig') });

      //}
      //if (rba.cycle_counting) {
      //  cyclecountitems.items.push({ label: 'Cycle Count', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Cyclecount') });

      //}
      //if (rba.cyclecount_approval) {
      //  cyclecountitems.items.push({ label: 'Cycle Count Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Cyclecount') });

      //}
      //invmanageitems.items.push(cyclecountitems);
      if (rba.inventory_management) {
        invmanageitems.items.push({
          label: 'Inventory Ageing',
          icon: 'pi pi-fw pi-bars',
          style: { 'font-weight': '600', 'width': '200px' },
          items: [
            { label: 'Obsolete Inventory', style: { 'font-weight': '600', 'width': '200px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ObsoleteInventoryMovement') },
            { label: 'Excess Inventory', style: { 'font-weight': '600', 'width': '200px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/ExcessInventoryMovement') }
          ]
        });
      }
      //if (rba.inventory_management) {
      //  this.items.push(invmanageitems);
      //}
      //if (rba.cyclecount_configuration || rba.cycle_counting || rba.cyclecount_approval) {
      //  this.items.push(cyclecountitems);
      //}
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
            //{ label: 'PM Master', style: { 'width': '250px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/AssignProjectManager') },
            { label: 'Miscellanous Master', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/MiscellanousReason') },
            { label: 'Assign RBA', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/Assignrba') },
            { label: 'Store Master', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/StoreMaster') },
            { label: 'Rack Master', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/RackMaster') },
            { label: 'Bin Master', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/BinMaster') },
            { label: 'Role Master', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/RoleMaster') },
            //{ label: 'Sub Role Master', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/SubRoleMaster') },
            { label: 'User Role Master', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/UserRoleMaster') },
            { label: 'Vendor Master', icon: 'pi pi-fw pi-bars', style: { 'width': '250px' }, command: () => this.router.navigateByUrl('WMS/VendorMaster') },
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
      if (rba.gatepass_approval) {
        matrequestapprovals.items.push({ label: 'Manager Approval(GP)', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassPMList') });
        if (this.emp.isFinancemember)
          matrequestapprovals.items.push({ label: 'Finance Approval(GP)', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassFMList') });
      }
      if (rba.material_transfer_approval) {
        matrequestapprovals.items.push({ label: 'Material Transfer Approval', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/materialtransferapproval') });

      }
      if (rba.materialrequest_approval) {
        matrequestapprovals.items.push({ label: 'Material Request Approval', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/MaterialRequestApproval') });
        matrequestapprovals.items.push({ label: 'Intra Unit Transfer Approval', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/STOApproval') });
        matrequestapprovals.items.push({ label: 'Sub Contract Approval', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/SubcontractApproval') });
      }
      if (rba.material_transfer_approval || rba.materialrequest_approval || rba.gatepass_approval) {
        this.items.push(matrequestapprovals);
      }
      if (rba.asn_view) {

      }

      if (rba.internal_stock_transfer) {
        this.items.push({ label: 'Internal Stock Transfer', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/Stocktransfer') });

      }
      if (this.emp.roleid == "3")
        this.items.push({ label: 'Direct Delivery', icon: 'pi pi-fw pi-bars', style: { 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/DirectDelivery') });

      if (!this.emp.isdelegatemember && this.emp.roleid == "4") {
        this.items.push({ label: 'Delegation', icon: 'pi pi-fw pi-bars', style: { 'font-weight': '600' }, command: () => this.router.navigateByUrl('WMS/AssignInventoryManager') });
      }
      if (rba.assign_pm) {
        if (this.emp.isdelegatemember && this.emp.roleid == "11") {
          this.items.push({
            label: 'Delegation',
            style: { 'font-weight': '600' },
            icon: 'pi pi-fw pi-bars',
            items: [
              { label: 'Team Members', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/AssignProject') }
            ]

          });
        }
        if (!this.emp.isdelegatemember && this.emp.roleid == "11") {
          this.items.push({
            label: 'Delegation',
            style: { 'font-weight': '600' },
            icon: 'pi pi-fw pi-bars',
            items: [
              { label: 'Project Manager', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/AssignPM') },
              { label: 'Team Members', style: { 'font-weight': '600', 'width': '270px' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/AssignProject') }
            ]

          });
        }
      }
      if (rba.annexure_report) {
        this.items.push({ label: 'Sub Contract Annexure', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/AnnexureReport') });
      }
      if (this.emp.roleid == "4") {
        this.items.push({ label: 'GP Authorization', style: { 'font-weight': '600' }, icon: 'pi pi-fw pi-bars', command: () => this.router.navigateByUrl('WMS/GatePassAuthList') });
      }


      //let element1x: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
      //element1x.hidden = false;
      this.dataSharingService.meniitems.next(this.items);
      $("#menudiv").show();
      if (!isNullOrUndefined(eurl) && eurl != "") {
        this.nevigatefromrbaforemail(rba, eurl);
      }
      else {
        if (sessionStorage.getItem("userdashboardpage")) {
          this.nevigatefromuserdashboard(rba);
        }
        else {
          this.nevigatefromrba(rba);
        }
      }



    }


  }
  nevigatefromuserdashboard(rba: rbamaster) {
    debugger;
    var page = sessionStorage.getItem("userdashboardpage");
    if (page == "Inbound" && rba.gate_entry) {
      this.router.navigateByUrl('WMS/SecurityCheck');
    }

    else if (page == "Quality" && rba.quality_check) {
      this.router.navigateByUrl('WMS/QualityCheck');
    }
    else if (page == "Reserve" && rba.material_reservation) {
      this.router.navigateByUrl('WMS/MaterialReserveView');
    }
    else if (page == "Approve" && rba.gatepass_approval) {
      this.router.navigateByUrl('WMS/Home');
    }
    else if (page == "Count" && rba.cyclecount_approval) {
      this.router.navigateByUrl('WMS/Cyclecount');
    }
    else if (page == "Putaway" && rba.put_away) {
      this.router.navigateByUrl('WMS/WarehouseIncharge');
    }
    else if (page == "Receive" && rba.receive_material) {
      this.router.navigateByUrl('WMS/GRNPosting');
    }
    else if (page == "Issue" && rba.material_issue) {
      this.router.navigateByUrl('WMS/MaterialIssueDashboard');
    }
    else if (page == "onhold" && rba.receive_material) {
      this.router.navigateByUrl('WMS/HoldGRView');
    }
    else if (page == "notify" && rba.notify_to_finance) {
      this.router.navigateByUrl('WMS/Putawaynotify');
    }
    else {
      this.router.navigateByUrl('WMS/Home');
    }

    sessionStorage.removeItem("userdashboardpage");


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
    else if (this.emp.roleid == "2") {
      this.router.navigateByUrl('WMS/Home');
    }
    else if (this.emp.roleid == "3") {
      this.router.navigateByUrl('WMS/Home');
    }
    else if (this.emp.roleid == "4") {
      this.router.navigateByUrl('WMS/Home');
    }
    else if (this.emp.roleid == "5") {
      this.router.navigateByUrl('WMS/Home');
    }
    else if (this.emp.roleid == "6") {
      this.router.navigateByUrl('WMS/Home');
    }
    else if (this.emp.roleid == "7") {
      if (rba.pmdashboard_view) {
        this.router.navigateByUrl('WMS/Dashboard');
      }
      else {
        this.router.navigateByUrl('WMS/Home');
      }
    }
    else if (this.emp.roleid == "8") {
      this.router.navigateByUrl('WMS/Home');
    }
    else if (this.emp.roleid == "9") {
      this.router.navigateByUrl('WMS/Home');
    }
    else if (this.emp.roleid == "10") {
      this.router.navigateByUrl('WMS/Home');
    }
    else if (this.emp.roleid == "11") {
      this.router.navigateByUrl('WMS/Home');
    }
    else {
      this.router.navigateByUrl('WMS/Home');
    }

  }
  nevigatefromrbaforemail(rba: rbamaster, eurl: any) {
    if (eurl.includes("/Email")) {
      if (eurl.includes("/Email/GRNPosting?GateEntryNo")) {
        this.inwmasterid = this.route.snapshot.queryParams.gateentryid;
        this.gateentryid = eurl.split('=')[1];
        if (this.gateentryid) {
          this.router.navigate(['WMS/GRNPosting'], { queryParams: { inwmasterid: this.gateentryid } });
        }
      }
      //Purpose: << Receipts >>

      if (eurl.includes("/Email/GRNPosting?GRNo")) {
        this.grnno = this.route.snapshot.queryParams.grnno;
        this.grnno = eurl.split('=')[1];
        if (this.grnno) {
          this.router.navigate(['WMS/GRNPosting'], { queryParams: { grnnumber: this.grnno } });
        }
      }

      //Purpose: << Quality check >>

      if (eurl.includes("/Email/QualityCheck?GRNo")) {
        debugger;
        this.grnno = this.route.snapshot.queryParams.grnnumber;
        this.grnno = eurl.split('=')[1];
        if (this.grnno) {
          this.router.navigate(['WMS/QualityCheck'], { queryParams: { grnnumber: this.grnno } });
        }
      }

      //Purpose: << Project Manager >>

      if (eurl.includes("/Email/MaterialIssueDashboard?ReqId")) {
        this.reqid = this.route.snapshot.queryParams.requestid;
        this.reqid = eurl.split('=')[1];
        if (this.reqid) {
          this.router.navigate(['WMS/MaterialIssueDashboard'], { queryParams: { requestid: this.reqid } });
        }
      }

      //Purpose: << Project Manager >>

      if (eurl.includes("/Email/ApproveSTOMaterial?ReqId")) {
        debugger;
        this.transferid = eurl.split('=')[1];
        if (this.transferid) {
          this.router.navigate(['WMS/STOApproval'], { queryParams: { transferid: this.transferid } });
        }
      }
      if (eurl.includes("/Email/ApproveSTOMaterialNP?ReqId")) {
        debugger;
        this.transferid = eurl.split('=')[1];
        if (this.transferid) {
          this.router.navigate(['WMS/STOApproval'], { queryParams: { transferid: this.transferid } });
        }
      }

      //Purpose: << Project Manager >>

      if (eurl.includes("/Email/IssueSTOMaterial?ReqId")) {
        this.reqid = this.route.snapshot.queryParams.requestid;
        this.reqid = eurl.split('=')[1];
        if (this.reqid) {
          this.router.navigate(['WMS/ReceiveSTORequest'], { queryParams: { requestid: this.reqid } });
        }
      }

      if (eurl.includes("/Email/grnputaway?GRNo")) {
        this.reqid = this.route.snapshot.queryParams.requestid;
        this.reqid = eurl.split('=')[1];
        if (this.reqid) {
          this.router.navigate(['WMS/WarehouseIncharge'], { queryParams: { requestid: this.reqid } });
        }
      }

      //Purpose: << Project Manager >>

      if (eurl.includes("/Email/ApprovalSubcontractingMaterial?ReqId")) {
        debugger;
        this.transferid = eurl.split('=')[1];
        if (this.transferid) {
          this.router.navigate(['WMS/SubcontractApproval'], { queryParams: { transferid: this.transferid } });
        }
      }
      if (eurl.includes("/Email/ApprovalSubcontractingMaterialNP?ReqId")) {
        debugger;
        this.transferid = eurl.split('=')[1];
        if (this.transferid) {
          this.router.navigate(['WMS/SubcontractApproval'], { queryParams: { transferid: this.transferid } });
        }
      }

      //Purpose: << Project Manager >>

      if (eurl.includes("/Email/IssueSubcontractingMaterial?ReqId")) {
        this.reqid = this.route.snapshot.queryParams.requestid;
        this.reqid = eurl.split('=')[1];
        if (this.reqid) {
          this.router.navigate(['WMS/ReceiveSubContractRequest'], { queryParams: { requestid: this.reqid } });
        }
      }

      //Purpose: << Project Manager >>

      if (eurl.includes("/Email/STOMaterialPutaway?ReqId")) {
        this.reqid = this.route.snapshot.queryParams.requestid;
        this.reqid = eurl.split('=')[1];
        if (this.reqid) {
          this.router.navigate(['WMS/ReceiveMaterial'], { queryParams: { requestid: this.reqid } });
        }
      }

      //Purpose: << Project Manager >>

      if (eurl.includes("/Email/SubcontractMaterialPutaway?ReqId")) {
        this.reqid = this.route.snapshot.queryParams.requestid;
        this.reqid = eurl.split('=')[1];
        if (this.reqid) {
          this.router.navigate(['WMS/ReceiveMaterial'], { queryParams: { requestid: this.reqid } });
        }
      }

      //Purpose: << InventoryManager >>

      if (eurl.includes("/Email/MaterialReqView?ReqId")) {
        this.reqid = this.route.snapshot.queryParams.requestid;
        this.reqid = eurl.split('=')[1];
        // this.pono = eurl.split('=')[3];
        if (this.reqid) {
          this.router.navigate(['WMS/MaterialReqView'], { queryParams: { requestid: this.reqid } });
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
          this.router.navigate(['WMS/GatePassPMList'], { queryParams: { requestid: this.gateid } });

        }
      }
      if (eurl.includes("/Email/GatePassAuthList?GateId")) {
        this.gateid = this.route.snapshot.queryParams.gateid;
        this.gateid = eurl.split('=')[1];
        if (this.gateid) {
          this.router.navigate(['WMS/GatePassAuthList'], { queryParams: { requestid: this.gateid } });

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
          this.constants.gatePassIssueType = "Pending";
          this.router.navigate(['WMS/GatePassApprover', this.gateid]);
        }
      }
      if (eurl.includes("/Email/GatePass?GatepassId")) {
        this.gateid = this.route.snapshot.queryParams.gateid;
        this.gateid = eurl.split('=')[1];
        if (this.gateid) {
          this.router.navigate(['WMS/GatePass'], { queryParams: { gatepassid: this.gateid } });
        }
      }
      if (eurl.includes("/Email/GatePass?NPGatepassId")) {
        this.gateid = this.route.snapshot.queryParams.gateid;
        this.gateid = eurl.split('=')[1];
        if (this.gateid) {
          this.router.navigate(['WMS/GatePass'], { queryParams: { gatepassid: this.gateid } });
        }
      }



      //Purpose: << Gate Pass PM approver to FM approver >>

      if (eurl.includes("/Email/GatePassFMList?GateId")) {
        this.fmgateid = this.route.snapshot.queryParams.gateid;
        this.fmgateid = eurl.split('=')[1];
        if (this.fmgateid) {
          //redirects to GatePassPMList
          this.router.navigate(['WMS/GatePassFMList'], { queryParams: { requestid: this.fmgateid } });

        }
      }


      //Purpose: << Notify to Finance>>

      if (eurl.includes("/Email/GRNotification?GRNo")) {
        this.grnno = this.route.snapshot.queryParams.grnno;
        this.grnno = eurl.split('=')[1];
        if (this.grnno) {
          this.router.navigateByUrl('WMS/GRNotification');
        }
      }

      if (eurl.includes("/Email/materialtransferapproval?transferid")) {
        debugger;
        this.transferid = this.route.snapshot.queryParams.transferid;
        this.transferid = eurl.split('=')[1];
        if (this.transferid) {
          this.router.navigate(['WMS/materialtransferapproval'], { queryParams: { transferid: this.transferid } });
        }
      }
      if (eurl.includes("/Email/materialtransfer?transferid")) {
        debugger;
        this.transferid = this.route.snapshot.queryParams.transferid;
        this.transferid = eurl.split('=')[1];
        if (this.transferid) {
          this.router.navigate(['WMS/MaterialTransfer'], { queryParams: { transferid: this.transferid } });
        }
      }
      if (eurl.includes("/Email/materialreturndashboard?returnid")) {
        debugger;
        this.transferid = this.route.snapshot.queryParams.returnid;
        this.transferid = eurl.split('=')[1];
        if (this.transferid) {
          this.router.navigate(['WMS/MaterialReturn'], { queryParams: { returnid: this.transferid } });
        }
      }
      if (eurl.includes("/Email/MaterialRequestApproval?ReqId")) {
        debugger;
        this.transferid = eurl.split('=')[1];
        if (this.transferid) {
          this.router.navigate(['WMS/MaterialRequestApproval'], { queryParams: { transferid: this.transferid } });
        }
      }
      if (eurl.includes("/Email/MaterialRequestApprovalNP?ReqId")) {
        debugger;
        this.transferid = eurl.split('=')[1];
        if (this.transferid) {
          this.router.navigate(['WMS/MaterialRequestApproval'], { queryParams: { transferid: this.transferid } });
        }
      }
      return;
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
  openoutlook() {
    window.location.href = "mailto:WMS_Project@in.yokogawa.com?subject=enter_subject&body=enter_content";
  }
  logout() {
    localStorage.removeItem("Employee");
    localStorage.removeItem("userroles");
    sessionStorage.removeItem("userdashboardpage");
    this.loggedin = false;
    //let element: HTMLElement = document.getElementById("btnuser") as HTMLElement;
    //element.hidden = true;
    //let elementx: HTMLElement = document.getElementById("notlogged") as HTMLElement;
    //elementx.hidden = false;
    //let element1: HTMLDivElement = document.getElementById("menudiv") as HTMLDivElement;
    //element1.hidden = true;
    $("#btnuser").hide();
    $("#notlogged").show();
    $("#menudiv").hide();
    this.overlaymodel.hide();
    this.router.navigateByUrl("WMS/Login");
  }
  userloggedHandler(emp: Employee) {
    debugger;
    $("#menudiv").show();
    this.loggedin = true;
    this.ngOnInit();
  }
  changemenu() {
    debugger;
    $("#menudiv").show();
    this.ngOnInit();

  }
}
