import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { Employee, userAcessNamesModel } from '../Models/Common.Model';
import { UserDashboardDetail, UserDashboardGraphModel, ManagerDashboard, pmDashboardCards, invDashboardCards} from '../Models/WMS.Model';
import { wmsService } from '../WmsServices/wms.service';
import { NgxSpinnerService } from "ngx-spinner";
import { DatePipe } from '@angular/common';
import { NavMenuComponent } from '../nav-menu/nav-menu.component';
import { AppComponent } from '../app.component';
import { TreeNode } from 'primeng/api';

@Component({
  selector: 'app-Home',
  templateUrl: './Home.component.html',
  providers: [DatePipe, NavMenuComponent, AppComponent]
})
export class HomeComponent implements OnInit {
  chartdata: any;
  chartdataIE: any;
  chartdataPM: any;
  monthlychartdata: any;
  monthlyIEchartdata: any;
  weeklychartdata: any;
  piedata: any;
  data1: TreeNode[];
  constructor(private router: Router, private wmsService: wmsService, private spinner: NgxSpinnerService, private datePipe: DatePipe, private navpage: NavMenuComponent, private apppage: AppComponent ) {
   }
  cardDetailslistdata: ManagerDashboard[] = [];
  pmCardDetailslistdata: pmDashboardCards[] = [];
  invCardDetailslistdata: invDashboardCards[] = [];
  public employee: Employee;
  isroleselected: boolean = true;
  userrolelist: userAcessNamesModel[] = [];
  rolename: string = "";
  userroleid: string;
  dashboardmodel: UserDashboardDetail;
  dashboardgraphmodel: UserDashboardGraphModel[] = [];
  monthlydashboardgraphmodel: UserDashboardGraphModel[] = [];
  monthlydashboardIEgraphmodel: UserDashboardGraphModel[] = [];
  dashboardIEgraphmodel: UserDashboardGraphModel[] = [];
  pmdashboardgraphmodel: UserDashboardGraphModel[] = [];
  chartoptions: any;
  chartoptions11: any;
  chartoptions1: any;
  chartoptions2: any;
  nodes: any[] = [];
  firstload: boolean = true;
  public totalGatePassList: Array<any> = [];
  public gatepassData: Array<any> = [];
  public gatepassData1: Array<any> = [];
  public gatepasslist: Array<any> = [];
  public gatepasslist1: Array<any> = [];
  approverstatus: string = "";
  notifcount: number = 0;
  notif: boolean = false;
  public materialIssueList: Array<any> = [];
  public materialIssueListnofilter: Array<any> = [];
  materialforissuecount: number;
  issecurityoperator: boolean = false;
  isinventoryinquiry: boolean = false;
  isinventoryclerk: boolean = false;
  isinventorymanager: boolean = false;
  isprojectmanager: boolean = false;
  isadmin: boolean = false;
  isapprover: boolean = false;
  isqualityuser: boolean = false;
  selectedroleid: number;
  selectedrolename: string = "";
  //page load event
  ngOnInit() {
    debugger;
    this.firstload = true;
    this.dashboardmodel = new UserDashboardDetail();
    this.dashboardgraphmodel = [];
    this.dashboardIEgraphmodel = [];
    this.monthlydashboardgraphmodel = [];
    this.monthlydashboardIEgraphmodel = [];
    this.pmdashboardgraphmodel = [];
    this.approverstatus = "Pending";
    this.materialforissuecount = 0;
    this.selectedrolename = "";
    this.selectedroleid = 0;
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.userroleid = this.employee.roleid;
   

    if (localStorage.getItem("userroles")) {
      this.isroleselected = false;
      this.userrolelist = JSON.parse(localStorage.getItem("userroles")) as userAcessNamesModel[];
      this.userrolelist = this.userrolelist.filter(function (element, index) {
        return (element.roleid != 6);
      });
    }
    else {
      this.userrolelist = JSON.parse(localStorage.getItem("allroles")) as userAcessNamesModel[];
      this.userrolelist = this.userrolelist.filter(function (element, index) {
        return (element.roleid != 6);
      });
    }
    if (this.userroleid != "0") {
      var roleid = this.userroleid;
     
      var data1 = this.userrolelist.filter(function (element, index) {
        return (element.roleid == parseInt(roleid));
      });
      if (data1.length > 0) {
        this.rolename = data1[0].accessname;
        this.selectedrolename = this.rolename;
      }

    }
    else {
      this.rolename = "-";
    }
   

    this.data1 = [{
      label: 'Receipts',
      type: 'person',
      styleClass: 'p-person',
      expanded: true,
      data: { name: 'Walter White', avatar: 'wf_pending.png' },
      children: [
        {
          label: '',
          type: 'person',
          styleClass: 'p-person',
          expanded: true,
          data: { name: 'Saul Goodman', avatar: 'saul.jpg' },
          children: [{
            label: 'Tax',
            styleClass: 'department-cfo'
          }
         ],
        }
      
      ]
    }];
    this.getdashboarddetail();
    this.setcontroldata();
    
   // this.defaultactive();
  }

  setcontroldata() {
    this.issecurityoperator = false;
    this.isinventoryinquiry = false;
    this.isinventoryclerk = false;
    this.isinventorymanager = false;
    this.isprojectmanager = false;
    this.isadmin = false;
    this.isapprover = false;
    this.isqualityuser = false;
    var data1 = this.userrolelist.filter(function (element, index) {
      return (element.roleid == 1);
    });
    var data2 = this.userrolelist.filter(function (element, index) {
      return (element.roleid == 2);
    });
    var data3 = this.userrolelist.filter(function (element, index) {
      return (element.roleid == 3);
    });
    var data4 = this.userrolelist.filter(function (element, index) {
      return (element.roleid == 4);
    });
    var data5 = this.userrolelist.filter(function (element, index) {
      return (element.roleid == 5);
    });
    var data6 = this.userrolelist.filter(function (element, index) {
      return (element.roleid == 7);
    });
    var data7 = this.userrolelist.filter(function (element, index) {
      return (element.roleid == 8);
    });
    var data8 = this.userrolelist.filter(function (element, index) {
      return (element.roleid == 9);
    });
    if (data1.length > 0) {
      this.issecurityoperator = true;
    }
    if (data2.length > 0) {
      this.isinventoryinquiry = true;
    }
    if (data3.length > 0) {
      this.isinventoryclerk = true;
    }
    if (data4.length > 0) {
      this.isinventorymanager = true;
    }
    if (data5.length > 0) {
      this.isprojectmanager = true;
    }
    if (data6.length > 0) {
      this.isadmin = true;
    }
    if (data7.length > 0) {
      this.isapprover = true;
    }
    if (data8.length > 0) {
      this.isqualityuser = true;
    }

  }

  Navigatetopagefn() {
    var name = this.selectedrolename;
    var data1 = this.userrolelist.filter(function (element, index) {
      return (element.accessname == name);
    });
    if (data1.length > 0) {
      this.employee.roleid = String(data1[0].roleid);
      localStorage.removeItem('Employee');
      localStorage.setItem('Employee', JSON.stringify(this.employee));
      this.navpage.changemenu();
      
    }
  }

  navigatebyrole(roleid: string) {

    this.employee.roleid = roleid;
    localStorage.removeItem('Employee');
    localStorage.setItem('Employee', JSON.stringify(this.employee));
    this.navpage.changemenu();


  }

  navigatebyrole1(roleid: string) {

    this.employee.roleid = roleid;
    localStorage.removeItem('Employee');
    localStorage.setItem('Employee', JSON.stringify(this.employee));
    this.navpage.changemenu()


  }

  getgraphdata() {

    this.spinner.show();
    this.wmsService.getdashgraphdata().subscribe(data => {
      if (data != null) {

        this.dashboardgraphmodel = data;
    // console.log(this.dashboardgraphmodel)
        this.setgraph('Receive'); 
      }
      this.spinner.hide();
    })


  }
  getIEgraphdata() {

    this.spinner.show();
    this.wmsService.getdashIEgraphdata().subscribe(data => {
      if (data != null) {

        this.dashboardIEgraphmodel = data;
         console.log(this.dashboardIEgraphmodel)
        this.setgraphIE('Request')
      }
      this.spinner.hide();
    })


  }
  getmonthlygraphdata() {

    this.spinner.show();
    this.wmsService.getmonthlydashgraphdata().subscribe(data => {
      if (data != null) {

        this.monthlydashboardgraphmodel = data;
        this.setmonthlygraph('Receive');
       
       // console.log(this.monthlydashboardgraphmodel)

      }
      this.spinner.hide();
    })


  }

  getmonthlyIEgraphdata() {

    this.spinner.show();
    this.wmsService.getmonthlyUserdashboardIEgraphdata().subscribe(data => {
      if (data != null) {

        this.monthlydashboardIEgraphmodel = data;
        this.setmonthlyIEgraph('Request');

       // console.log(this.monthlydashboardIEgraphmodel )
      }
      this.spinner.hide();
    })


  }
  setgraph(type: string) {
    this.chartdata = null;
    var lblmessage = "";
    if (type == "Receive") {
      lblmessage = "Receipts received  in past seven days."
    }
    if (type == "Quality") {

      lblmessage = "Quality checked of materials  in past seven days."

    }
    if (type == "Accept") {

      lblmessage = "Receipts accepted  in past seven days."

    }
    if (type == "Putaway") {

      lblmessage = "materials put away in past seven days."

    }
      var pid = []
      var count = []
    var gdata = this.dashboardgraphmodel.filter(function (elementx, index) {
      return (elementx.type == type);
    });
    if (gdata.length > 0) {
      gdata.forEach(element => {

        var showdate = this.datePipe.transform(element.graphdate, 'dd/MM/yyyy');
        pid.push(showdate);
        count.push(element.count);
      });
    }

      this.chartdata = {

        labels: pid,
        datasets: [
          {
            label: lblmessage,
            backgroundColor: '#334b80',
            //backgroundColor: 'rgba(0,255,0,0.5)',
            borderColor: '#7CB342',
            data: count
          },
        ]

      }
      this.chartoptions = { scales: { yAxes: [{ ticks: { beginAtZero: true, userCallback: function (label, index, labels) { if (Math.floor(label) === label) { return label; } }, } }] } }


    
   // console.log(this.dashboardgraphmodel);
  }

  setgraphIE(type: string) {
    this.chartdataIE = null;
    var lblmessage = "";
    if (type == "Request") {
      lblmessage = "Materials Request in past seven days."
    }

    if (type == "Return") {
      lblmessage = "Materials Return in past seven days."
    }
    if (type == "Reserve") {
      lblmessage = "Materials Reserve in past seven days."
    }
    if (type == "Transfer") {
      lblmessage = "Materials Transfer in past seven days."
    }

    var pid = []
    var count = []
    var gdata = this.dashboardIEgraphmodel.filter(function (elementx, index) {
      return (elementx.type == type);
    });
    if (gdata.length > 0) {
      gdata.forEach(element => {

        var showdate = this.datePipe.transform(element.graphdate, 'dd/MM/yyyy');
        pid.push(showdate);
        count.push(element.count);
      });
    }

    this.chartdataIE = {

      labels: pid,
      datasets: [
        {
          label: lblmessage,
          backgroundColor: '#334b80',
         // backgroundColor: 'rgba(0,255,0,0.5)',
          borderColor: '#7CB342',
          data: count
        },
      ]

    }
    this.chartoptions11 = { scales: { yAxes: [{ ticks: { beginAtZero: true, userCallback: function (label, index, labels) { if (Math.floor(label) === label) { return label; } }, } }] } }



    console.log(this.dashboardIEgraphmodel);
  }

  setmonthlygraph(type: string) {
    debugger;
    this.monthlychartdata = null;
    var lblmessage = "";
    if (type == "Receive") {
      lblmessage = "Monthly Received Receipts"
    }
    if (type == "Quality") {

      lblmessage = "Monthly Quality checked"

    }
    if (type == "Accept") {

      lblmessage = "Monthly Accepted Receipts"

    }
    if (type == "Putaway") {

      lblmessage = "Monthly put aways"

    }
    if (type == "Request" ) {
      lblmessage = "Monthly Request Materials"
    }
    
    var pid = []
    var count = []
    var gdata = this.monthlydashboardgraphmodel.filter(function (elementx, index) {
      return (elementx.type == type);
    });
    if (gdata.length > 0) {
      gdata.forEach(element => {
        pid.push(element.sweek);
        count.push(element.count);
      });
    }

    this.monthlychartdata = {
      //chart: {
      //  //height: 350,
      //  type: "line",
      //  zoom: {
      //    enabled: false
      //  },
      labels: pid,
      datasets: [
        {
          label: lblmessage,
          //backgroundColor: '#42A5F5',
          backgroundColor: '#70b385',
          borderColor: '#555961',
          data: count
        },
      ]

    }
    this.chartoptions1 = { scales: { yAxes: [{ ticks: { beginAtZero: true, userCallback: function (label, index, labels) { if (Math.floor(label) === label) { return label; } }, } }] } }

   // console.log(this.monthlychartdata);


  }


 
  
  setmonthlyIEgraph(type: string) {
    debugger;
    this.monthlyIEchartdata = null;
    var lblmessage = "";
   
    if (type == "Request" ) {
      lblmessage = "Monthly Request Materials"
    }

    var pid = []
    var count = []
    var gdata = this.monthlydashboardIEgraphmodel.filter(function (elementx, index) {
      return (elementx.type == type);
    });
    if (gdata.length > 0) {
      gdata.forEach(element => {
        pid.push(element.sweek);
        count.push(element.count);
      });
    }

    this.monthlyIEchartdata = {
      
      labels: pid,
      datasets: [
        {
          label: lblmessage,
          //backgroundColor: '#42A5F5',
          backgroundColor: '#4BC0C0',
          borderColor: '#555961',
          data: count
        },
      ]

    }
    this.chartoptions2 = { scales: { yAxes: [{ ticks: { beginAtZero: true, userCallback: function (label, index, labels) { if (Math.floor(label) === label) { return label; } }, } }] } }

   // console.log(this.monthlydashboardIEgraphmodel );


  }

  activeCard1(event, type: string) {
    debugger;
    this.firstload = false;
    this.nodes.forEach(item => {
      item.classList.remove("cardactive");
    })
    let node;
    node = event.target.parentNode;
    this.nodes = [];
    this.nodes.push(node);
    node.classList.add("cardactive");
    this.setgraph(type);
    this.setmonthlygraph(type);
    this.setmonthlyIEgraph(type);
    this.setgraphIE(type);
    
  }
  activeCard(event, type: string) {
    debugger;
    this.firstload = false;
    this.nodes.forEach(item => {
      item.classList.remove("cardactive");
    })
    let node;
    if (event.target.tagName === "IMG" || event.target.tagName === "SPAN") {
      node = event.target.parentNode;
    } else {
      node = event.target;
    }
    this.nodes = [];
    this.nodes.push(node);
    node.classList.add("cardactive");
    if (type != "") {
      sessionStorage.setItem("userdashboardpage", type);
    }
    if (type == "Inbound" && this.issecurityoperator) {
      this.navigatebyrole("1");
    }
    else if (type == "Receive" && this.isinventoryclerk) {
      this.navigatebyrole("3");
    }
    else if (type == "Quality" && this.isqualityuser) {
      this.navigatebyrole("9");
    }
    else if (type == "Putaway" && this.isinventoryclerk) {
      this.navigatebyrole("3");
    }
    else if (type == "Reserve" && this.isprojectmanager) {
      this.navigatebyrole("5");
    }
    else if (type == "Approve" && this.isapprover) {
      this.navigatebyrole("8");
    }
    else if (type == "Issue" && this.isinventorymanager) {
      this.navigatebyrole("4");
    }
    else if (type == "Count" && this.isinventorymanager) {
      this.navigatebyrole("4");
    }
   
   
    //this.setgraph(type);
    //this.setmonthlygraph(type);

  }

  bindnonifications() {
    this.getGatePassList();
  }

  getGatePassList() {
    this.notif = false;
    debugger;
    this.wmsService.getGatePassList().subscribe(data => {
      if (data) {
        this.totalGatePassList = data;
        //PM
        this.gatepassData = this.totalGatePassList.filter(li => li.approverid == this.employee.employeeno && (li.approverstatus == this.approverstatus));
        //FM
        this.gatepassData1 = this.totalGatePassList.filter(li => li.fmapproverid == this.employee.employeeno && li.approverstatus == "Approved" && li.fmapprovedstatus == this.approverstatus && li.gatepasstype == "Non Returnable");
        this.prepareGatepassList();
        this.prepareGatepassList1();
      }
      

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
      this.notifcount = this.notifcount + this.gatepasslist.length;
      var count = this.gatepasslist.length;
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
      this.notifcount = this.notifcount + this.gatepasslist1.length;
      var count = this.gatepasslist1.length;
      
    }
  }

  getMaterialIssueList() {
    this.wmsService.getMaterialIssueLlist(this.employee.employeeno).subscribe(data => {
      debugger;
      this.materialIssueListnofilter = data;
      this.materialIssueList = this.materialIssueListnofilter.filter(li => li.requeststatus == 'Pending');
      this.materialforissuecount = this.materialIssueList.length;
      
    });
  }


  getdashboarddetail() {
    var empno = this.employee.employeeno;
    this.spinner.show();
    this.wmsService.getdashdata(empno).subscribe(data => {
      if (data != null) {

        this.dashboardmodel = data;

        //console.log(this.dashboardmodel) 
      }
      this.getGatePassList();
      this.getMaterialIssueList();
      this.getgraphdata();
      this.getmonthlygraphdata();
      this.getCardlist();
      this.getPMCardlist();
     // this.getgraphPMdata();
      this.getInvCardlist();
      this.getmonthlyIEgraphdata();
      this.getIEgraphdata();
      this.spinner.hide();
      })

    
  }


  getCardlist() {
    this.cardDetailslistdata = [];
    this.spinner.show();
    this.wmsService.getCardlist().subscribe(data => {
      this.cardDetailslistdata = data;

       //console.log(this.cardDetailslistdata)

      this.spinner.hide();
    });
  }

  getPMCardlist() {
    this.pmCardDetailslistdata = [];
    this.spinner.show();
    this.wmsService.getPMCardlist().subscribe(data => {
      this.pmCardDetailslistdata = data;

     // console.log(this.pmCardDetailslistdata)

      this.spinner.hide();
    });
  }


  getInvCardlist() {
    this.invCardDetailslistdata = [];
    this.spinner.show();
    this.wmsService.getInvCardlist().subscribe(data => {
      this.invCardDetailslistdata = data;

      //console.log(this.invCardDetailslistdata)

      this.spinner.hide();
    });
  }

  //getgraphPMdata() {

  //  this.spinner.show();
  //  this.wmsService.getPMdashgraphdata().subscribe(data => {
  //    if (data != null) {

  //      this.pmdashboardgraphmodel = data;
  //      console.log(this.pmdashboardgraphmodel)
  //      this.setPMgraph('Request');

  //    }
  //    this.spinner.hide();
  //  })


  //}





  //setPMgraph(type: string) {
  //  this.chartdataPM = null;
  //  var lblmessage = "";
  //  if (type == "Request") {
  //    lblmessage = "material Requested  in past seven days."
  //  }
  //  if (type == "Return") {

  //    lblmessage = "materials Return in past seven days."

  //  }
  //  if (type == "Reserve") {

  //    lblmessage = "materials Reserved  in past seven days."

  //  }
  //  if (type == "Returned") {

  //    lblmessage = "materials Returned in past seven days."

  //  }
  //  var pid = []
  //  var count = []
  //  var gdata = this.pmdashboardgraphmodel.filter(function (elementx, index) {
  //    console.log( this.pmdashboardgraphmodel)
  //    return (elementx.type == type);
  //  });
  //  if (gdata.length > 0) {
  //    gdata.forEach(element => {

  //      var showdate = this.datePipe.transform(element.graphdate, 'dd/MM/yyyy');
  //      pid.push(showdate);
  //      count.push(element.count);
  //    });
  //  }

  //  this.chartdataPM = {

  //    labels: pid,
  //    datasets: [
  //      {
  //        label: lblmessage,
  //        //backgroundColor: '#42A5F5',
  //        backgroundColor: 'rgba(0,255,0,0.5)',
  //        borderColor: '#7CB342',
  //        data: count
  //      },
  //    ]

  //  }
  //  this.chartoptions = { scales: { yAxes: [{ ticks: { beginAtZero: true, userCallback: function (label, index, labels) { if (Math.floor(label) === label) { return label; } }, } }] } }
  //  console.log(this.chartdataPM)



  //}

}


