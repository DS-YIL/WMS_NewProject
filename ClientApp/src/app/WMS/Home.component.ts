import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { Employee, userAcessNamesModel, rbamaster } from '../Models/Common.Model';
import { UserDashboardDetail, UserDashboardGraphModel, ManagerDashboard, pmDashboardCards, invDashboardCards, GraphModelNew, DashBoardFilters } from '../Models/WMS.Model';
import { wmsService } from '../WmsServices/wms.service';
import { NgxSpinnerService } from "ngx-spinner";
import { DatePipe } from '@angular/common';
import { NavMenuComponent } from '../nav-menu/nav-menu.component';
import { AppComponent } from '../app.component';
import { TreeNode } from 'primeng/api';
import { element } from 'protractor';

@Component({
  selector: 'app-Home',
  templateUrl: './Home.component.html',
  providers: [DatePipe, NavMenuComponent, AppComponent]
})
export class HomeComponent implements OnInit {
  chartdata: any;
  chartdataIE: any;
  chartdataPM: any;
  lblmonth: string = "";
  monthlychartdata: any;
  rbalist: rbamaster[] = [];
  selectedrba: rbamaster;
  //for receipts
  receivedchartdata: any;
  receivedchartdatalist: GraphModelNew[] = [];
  qualitychartdata: any;
  qualitychartdatalist: GraphModelNew[] = [];
  acceptchartdata: any;
  acceptchartdatalist: GraphModelNew[] = [];
  putawaychartdata: any;
  putawaychartdatalist: GraphModelNew[] = [];
  requestchartdata: any;
  requestchartdatalist: GraphModelNew[] = [];
  returnchartdata: any;
  returnchartdatalist: GraphModelNew[] = [];
  reservechartdata: any;
  reservechartdatalist: GraphModelNew[] = [];
  transferchartdata: any;
  transferchartdatalist: GraphModelNew[] = [];
  ///
  monthlyIEchartdata: any;
  weeklychartdata: any;
  piedata: any;
  data1: TreeNode[];
  constructor(private router: Router, private wmsService: wmsService, private spinner: NgxSpinnerService, private datePipe: DatePipe, private navpage: NavMenuComponent, private apppage: AppComponent) {
  }
  cardDetailslistdata: ManagerDashboard;
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
  monthlist: any[] = [];
  public materialIssueList: Array<any> = [];
  public materialIssueListnofilter: Array<any> = [];
  materialforissuecount: number;
  issecurityoperator: boolean = false;
  isreceiveuser: boolean = false;
  isqualityuser: boolean = false;
  isputawayuser: boolean = false;
  isnotifyuser: boolean = false;
  isreserveuser: boolean = false;
  isissueuser: boolean = false;
  isapprover: boolean = false;
  isonholduser: boolean = false;
  iscyclecountuser: boolean = false;
  selectedroleid: number;
  selectedrolename: string = "";
  roleidforinbound: string = "";
  roleidforreceipt: string = "";
  roleidforqualitycheck: string = "";
  roleidforputaway: string = "";
  roleidfornoyify: string = "";
  roleidforreserve: string = "";
  roleidforapproval: string = "";
  roleidforissue: string = "";
  roleidforonhold: string = "";
  roleidforcyclecount: string = "";
  totlReceiptsCnt: number;
  totalQulityCnt: number;
  totalAcceptanceCnt: number;
  totalputawayCnt: number;
  public fromDate: Date;
  public toDate: Date;
  public DashBoardFilters: DashBoardFilters;
  //page load event
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");

    this.DashBoardFilters = new DashBoardFilters();
    this.fromDate = new Date(new Date().setDate(new Date().getDate() - 30));
    this.toDate = new Date();

    this.firstload = true;
    this.lblmonth = "";
    this.monthlist = ["", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"]
    this.dashboardmodel = new UserDashboardDetail();
    this.dashboardgraphmodel = [];
    this.dashboardIEgraphmodel = [];
    this.monthlydashboardgraphmodel = [];
    this.monthlydashboardIEgraphmodel = [];
    this.pmdashboardgraphmodel = [];
    this.rbalist = [];
    this.selectedrolename = "";
    this.roleidforinbound = "";
    this.roleidforreceipt = "";
    this.roleidforqualitycheck = "";
    this.roleidforputaway = "";
    this.roleidfornoyify = "";
    this.roleidforreserve = "";
    this.roleidforapproval = "";
    this.roleidforissue = "";
    this.roleidforonhold = "";
    this.roleidforcyclecount = "";
    this.selectedrba = new rbamaster();
    this.approverstatus = "Pending";
    this.materialforissuecount = 0;
    this.selectedrolename = "";
    this.selectedroleid = 0;
    this.cardDetailslistdata = new ManagerDashboard();
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
    ////////////////////////////////////////grapg functions
    if (this.userroleid == "3") {//iventory clerk
      this.getreceivedchartdatalist();
      this.getqualitychartdatalist();
      this.getacceptchartdatalist();
      this.getputawaychartdatalist();
    }
    if (this.userroleid == "2" || this.userroleid == "11"){//2:iventory enquiry 11:PM
      this.getrequestdatalist();
      this.getreturndatalist();
      this.getreservedatalist();
      this.gettransferdatalist();
    }
    /////////////////////////////////////////
    // this.defaultactive();
  }

  getreceivedchartdatalist() {
    this.DashBoardFilters.fromDate = this.datePipe.transform(this.fromDate, "yyyy-MM-dd");
    this.DashBoardFilters.toDate = this.datePipe.transform(this.toDate, "yyyy-MM-dd");
    this.wmsService.getreceivedgraphdata(this.DashBoardFilters).subscribe(data => {
      if (data != null) {
        this.receivedchartdatalist = data;
        this.setReceivedgraph();
      }
    })

  }
  getqualitychartdatalist() {
    this.DashBoardFilters.fromDate = this.datePipe.transform(this.fromDate, "yyyy-MM-dd");
    this.DashBoardFilters.toDate = this.datePipe.transform(this.toDate, "yyyy-MM-dd");
    this.wmsService.getqualitygraphdata(this.DashBoardFilters).subscribe(data => {
      if (data != null) {
        this.qualitychartdatalist = data;
        this.setQualitygraph();
      }
    })

  }
  getacceptchartdatalist() {
    this.DashBoardFilters.fromDate = this.datePipe.transform(this.fromDate, "yyyy-MM-dd");
    this.DashBoardFilters.toDate = this.datePipe.transform(this.toDate, "yyyy-MM-dd");
    this.wmsService.getacceptgraphdata(this.DashBoardFilters).subscribe(data => {
      if (data != null) {
        this.acceptchartdatalist = data;
        this.setAcceptgraph();
      }
    })

  }
  getputawaychartdatalist() {
    this.DashBoardFilters.fromDate = this.datePipe.transform(this.fromDate, "yyyy-MM-dd");
    this.DashBoardFilters.toDate = this.datePipe.transform(this.toDate, "yyyy-MM-dd");
    this.wmsService.getputawaygraphdata(this.DashBoardFilters).subscribe(data => {
      if (data != null) {
        this.putawaychartdatalist = data;
        this.setPutawaygraph();
      }
    })

  }

  getrequestdatalist() {
    this.DashBoardFilters.fromDate = this.datePipe.transform(this.fromDate, "yyyy-MM-dd");
    this.DashBoardFilters.toDate = this.datePipe.transform(this.toDate, "yyyy-MM-dd");
    this.wmsService.getrequestgraphdata(this.DashBoardFilters).subscribe(data => {
      if (data != null) {
        this.requestchartdatalist = data;
        this.setRequestgraph();
      }
    })

  }

  getreturndatalist() {
    this.DashBoardFilters.fromDate = this.datePipe.transform(this.fromDate, "yyyy-MM-dd");
    this.DashBoardFilters.toDate = this.datePipe.transform(this.toDate, "yyyy-MM-dd");
    this.wmsService.getreturngraphdata(this.DashBoardFilters).subscribe(data => {
      if (data != null) {
        this.returnchartdatalist = data;
        this.setReturngraph();
      }
    })

  }

  getreservedatalist() {
    this.wmsService.getreservegraphdata(this.DashBoardFilters).subscribe(data => {
      if (data != null) {
        this.reservechartdatalist = data;
        this.setReservegraph();
      }
    })

  }

  gettransferdatalist() {
    this.wmsService.gettransfergraphdata(this.DashBoardFilters).subscribe(data => {
      if (data != null) {
        this.transferchartdatalist = data;
        this.setTransfergraph();
      }
    })

  }

  setcontroldata() {
    this.issecurityoperator = false;
    this.isreceiveuser = false;
    this.isqualityuser = false;
    this.isputawayuser = false;
    this.isnotifyuser = false;
    this.isreserveuser = false;
    this.isissueuser = false;
    this.isapprover = false;
    this.isonholduser = false;
    this.iscyclecountuser = false;
    var found = false;

    if (localStorage.getItem("rbalist")) {
      this.rbalist = JSON.parse(localStorage.getItem("rbalist")) as rbamaster[];
      this.userrolelist.forEach(item => {
        var dt1 = this.rbalist.filter(function (element, index) {
          return (element.gate_entry && element.roleid == item.roleid);
        });
        if (dt1.length > 0) {
          this.roleidforinbound = String(dt1[0].roleid);
          this.issecurityoperator = true;
        }
        var dt2 = this.rbalist.filter(function (element, index) {
          return (element.receive_material && element.roleid == item.roleid);
        });
        if (dt2.length > 0) {
          this.roleidforreceipt = String(dt2[0].roleid);
          this.isreceiveuser = true;
        }
        var dt3 = this.rbalist.filter(function (element, index) {
          return (element.quality_check && element.roleid == item.roleid);
        });
        if (dt3.length > 0) {
          this.roleidforqualitycheck = String(dt3[0].roleid);
          this.isqualityuser = true;
        }
        var dt4 = this.rbalist.filter(function (element, index) {
          return (element.put_away && element.roleid == item.roleid);
        });
        if (dt4.length > 0) {
          this.roleidforputaway = String(dt4[0].roleid);
          this.isputawayuser = true;
        }
        var dt5 = this.rbalist.filter(function (element, index) {
          return (element.notify_to_finance && element.roleid == item.roleid);
        });
        if (dt5.length > 0) {
          this.roleidfornoyify = String(dt5[0].roleid);
          this.isnotifyuser = true;
        }
        var dt6 = this.rbalist.filter(function (element, index) {
          return (element.material_reservation && element.roleid == item.roleid);
        });
        if (dt6.length > 0) {
          this.roleidforreserve = String(dt6[0].roleid);
          this.isreserveuser = true;
        }
        var dt7 = this.rbalist.filter(function (element, index) {
          return (element.material_issue && element.roleid == item.roleid);
        });
        if (dt7.length > 0) {
          this.roleidforissue = String(dt7[0].roleid);
          this.isissueuser = true;
        }
        var dt8 = this.rbalist.filter(function (element, index) {
          return (element.gatepass_approval && element.roleid == item.roleid);
        });
        if (dt8.length > 0) {
          this.roleidforapproval = String(dt8[0].roleid);
          this.isapprover = true;
        }
        var dt9 = this.rbalist.filter(function (element, index) {
          return (element.receive_material && element.roleid == item.roleid);
        });
        if (dt9.length > 0) {
          this.roleidforonhold = String(dt9[0].roleid);
          this.isonholduser = true;
        }
        var dt10 = this.rbalist.filter(function (element, index) {
          return (element.cyclecount_approval && element.roleid == item.roleid);
        });
        if (dt10.length > 0) {
          this.roleidforcyclecount = String(dt10[0].roleid);
          this.iscyclecountuser = true;
        }



      })


    }


  }

  Navigatetopagefn() {
    var name = this.selectedrolename;
    var data1 = this.userrolelist.filter(function (element, index) {
      return (element.accessname == name);
    });
    if (data1.length > 0) {
      this.employee.roleid = String(data1[0].roleid);
      this.employee.isdelegatemember = data1[0].isdelegatemember;
      localStorage.removeItem('Employee');
      localStorage.setItem('Employee', JSON.stringify(this.employee));
      this.navpage.changemenu();
      //this.apppage.loggedhandlerapp();

    }
  }

  navigatebyrole(roleid: string) {
    this.setddl(roleid);
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
        //console.log(this.dashboardIEgraphmodel)
        this.setgraphIE('Request')
      }
      this.spinner.hide();
    })


  }
  getmonthlygraphdata() {

    this.spinner.show();
    this.wmsService.getmonthlydashgraphdata(this.DashBoardFilters).subscribe(data => {
      if (data != null) {

        this.monthlydashboardgraphmodel = data;
        this.setmonthlygraph('Receive');
        //this.setmonthlygraph('Quality');
        //this.setmonthlygraph('Accept');
        //this.setmonthlygraph('Putaway');

        console.log(this.monthlydashboardgraphmodel)

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



    //console.log(this.dashboardIEgraphmodel);
  }

  setmonthlygraph(type: string) {
    this.monthlychartdata = null;
    var lblmessage = "";
    var lblmessage1 = "";
    var lblmessage2 = "";
    var lblmessage3 = "";

    if (type == "Receive") {
      lblmessage = "Monthly Received Receipts"
    }
    if (type == "Quality") {

      lblmessage1 = "Monthly Quality checked"

    }
    if (type == "Accept") {

      lblmessage2 = "Monthly Accepted Receipts"

    }
    if (type == "Putaway") {

      lblmessage3 = "Monthly put aways"

    }


    var pid = []
    var count = []
    var count1 = []

    var gdata = this.monthlydashboardgraphmodel.filter(function (elementx, index) {
      return (elementx.type == type);
    });
    if (gdata.length > 0) {
      gdata.forEach(element => {
        pid.push(element.sweek);
        count.push(element.count);
      });
    }
    //else if (gdata.length > 0) {
    //  gdata.forEach(element => {
    //    pid.push(element.quality);
    //    count1.push(element.count1);
    //  });
    //}
    //if (gdata.length > 0) {
    //  gdata.forEach(element => {
    //    pid.push(element.accept);
    //    count2.push(element.count2);
    //  });
    //}
    //if (gdata.length > 0) {
    //  gdata.forEach(element => {
    //    pid.push(element.putaway);
    //    count3.push(element.count3);
    //  });
    //}
    this.monthlychartdata = {

      labels: pid,
      datasets: [
        {
          label: lblmessage,
          //backgroundColor: '#42A5F5',
          backgroundColor: '#70b385',
          borderColor: '#555961',
          data: count
        },
        //{
        //  label: lblmessage1,
        //  //backgroundColor: '#42A5F5',
        //  backgroundColor: '#42A5F5',
        //  borderColor: '#555961',
        //  data: count1
        //},
        //{
        //  label: lblmessage2,
        //  //backgroundColor: '#42A5F5',
        //  backgroundColor: '#f5428a',
        //  borderColor: '#555961',
        //  data: count2
        //},
        //{
        //  label: lblmessage3,
        //  //backgroundColor: '#42A5F5',
        //  backgroundColor: '#8ab5b4',
        //  borderColor: '#555961',
        //  data: count3
        //},
      ]

    }
    this.chartoptions1 = { scales: { yAxes: [{ ticks: { beginAtZero: true, userCallback: function (label, index, labels) { if (Math.floor(label) === label) { return label; } }, } }] } }

    //console.log(this.monthlydashboardgraphmodel);


  }

  setReceivedgraph() {
    this.receivedchartdata = null;
    var lblmessage = "Total";
    var lblmessage1 = "Completed";
    var lblmessage2 = "Pending";
    var pid = [];
    var total = [];
    var received = [];
    var pending = [];
    if (this.receivedchartdatalist.length > 0) {
      this.lblmonth = this.monthlist[this.receivedchartdatalist[0].smonth];
    }
    this.receivedchartdatalist.forEach(element => {
      pid.push(element.displayweek);
      total.push(element.total);
      received.push(element.received);
      pending.push(element.pending);
    });


    this.receivedchartdata = {

      labels: pid,
      datasets: [
        {
          label: lblmessage,
          //backgroundColor: '#42A5F5',
          backgroundColor: '#70b385',
          borderColor: '#555961',
          data: total
        },
        {
          label: lblmessage1,
          //backgroundColor: '#42A5F5',
          backgroundColor: '#42A5F5',
          borderColor: '#555961',
          data: received
        },
        {
          label: lblmessage2,
          //backgroundColor: '#42A5F5',
          backgroundColor: '#f5428a',
          borderColor: '#555961',
          data: pending
        }


      ]

    }
    this.chartoptions1 = { scales: { yAxes: [{ ticks: { beginAtZero: true, userCallback: function (label, index, labels) { if (Math.floor(label) === label) { return label; } }, } }] } }

    //console.log(this.monthlydashboardgraphmodel);


  }

  setQualitygraph() {

    this.qualitychartdata = null;
    var lblmessage = "Total";
    var lblmessage1 = "Completed";
    var lblmessage2 = "Pending";
    var pid = [];
    var total = [];
    var received = [];
    var pending = [];
    if (this.qualitychartdatalist.length > 0) {
      this.lblmonth = this.monthlist[this.qualitychartdatalist[0].smonth];
    }
    this.qualitychartdatalist.forEach(element => {
      pid.push(element.displayweek);
      total.push(element.total);
      received.push(element.received);
      pending.push(element.pending);
    });


    this.qualitychartdata = {

      labels: pid,
      datasets: [
        {
          label: lblmessage,
          //backgroundColor: '#42A5F5',
          backgroundColor: '#70b385',
          borderColor: '#555961',
          data: total
        },
        {
          label: lblmessage1,
          //backgroundColor: '#42A5F5',
          backgroundColor: '#42A5F5',
          borderColor: '#555961',
          data: received
        },
        {
          label: lblmessage2,
          //backgroundColor: '#42A5F5',
          backgroundColor: '#f5428a',
          borderColor: '#555961',
          data: pending
        }


      ]

    }
    this.chartoptions1 = { scales: { yAxes: [{ ticks: { beginAtZero: true, userCallback: function (label, index, labels) { if (Math.floor(label) === label) { return label; } }, } }] } }

    //console.log(this.monthlydashboardgraphmodel);


  }


  setAcceptgraph() {
    this.acceptchartdata = null;
    var lblmessage = "Total";
    var lblmessage1 = "Completed";
    var lblmessage2 = "Pending";
    var pid = [];
    var total = [];
    var received = [];
    var pending = [];
    if (this.acceptchartdatalist.length > 0) {
      this.lblmonth = this.monthlist[this.acceptchartdatalist[0].smonth];
    }
    this.acceptchartdatalist.forEach(element => {
      pid.push(element.displayweek);
      total.push(element.total);
      received.push(element.received);
      pending.push(element.pending);
    });


    this.acceptchartdata = {

      labels: pid,
      datasets: [
        {
          label: lblmessage,
          //backgroundColor: '#42A5F5',
          backgroundColor: '#70b385',
          borderColor: '#555961',
          data: total
        },
        {
          label: lblmessage1,
          //backgroundColor: '#42A5F5',
          backgroundColor: '#42A5F5',
          borderColor: '#555961',
          data: received
        },
        {
          label: lblmessage2,
          //backgroundColor: '#42A5F5',
          backgroundColor: '#f5428a',
          borderColor: '#555961',
          data: pending
        }


      ]

    }
    this.chartoptions1 = { scales: { yAxes: [{ ticks: { beginAtZero: true, userCallback: function (label, index, labels) { if (Math.floor(label) === label) { return label; } }, } }] } }

    //console.log(this.monthlydashboardgraphmodel);


  }


  setPutawaygraph() {
    this.putawaychartdata = null;
    var lblmessage = "Total";
    var lblmessage1 = "Completed";
    var lblmessage2 = "Pending";
    var pid = [];
    var total = [];
    var received = [];
    var pending = [];
    if (this.putawaychartdatalist.length > 0) {
      this.lblmonth = this.monthlist[this.putawaychartdatalist[0].smonth];
    }
    this.putawaychartdatalist.forEach(element => {
      pid.push(element.displayweek);
      total.push(element.total);
      received.push(element.received);
      pending.push(element.pending);
    });


    this.putawaychartdata = {

      labels: pid,
      datasets: [
        {
          label: lblmessage,
          //backgroundColor: '#42A5F5',
          backgroundColor: '#70b385',
          borderColor: '#555961',
          data: total
        },
        {
          label: lblmessage1,
          //backgroundColor: '#42A5F5',
          backgroundColor: '#42A5F5',
          borderColor: '#555961',
          data: received
        },
        {
          label: lblmessage2,
          //backgroundColor: '#42A5F5',
          backgroundColor: '#f5428a',
          borderColor: '#555961',
          data: pending
        }


      ]

    }
    this.chartoptions1 = { scales: { yAxes: [{ ticks: { beginAtZero: true, userCallback: function (label, index, labels) { if (Math.floor(label) === label) { return label; } }, } }] } }

    //console.log(this.monthlydashboardgraphmodel);


  }


  setRequestgraph() {
    this.requestchartdata = null;
    var lblmessage = "Total";
    var lblmessage1 = "Completed";
    var lblmessage2 = "Pending";
    var pid = [];
    var total = [];
    var received = [];
    var pending = [];
    if (this.requestchartdatalist.length > 0) {
      this.lblmonth = this.monthlist[this.requestchartdatalist[0].smonth];
    }
    this.requestchartdatalist.forEach(element => {
      pid.push(element.displayweek);
      total.push(element.total);
      received.push(element.received);
      pending.push(element.pending);
    });


    this.requestchartdata = {

      labels: pid,
      datasets: [
        {
          label: lblmessage,
          //backgroundColor: '#42A5F5',
          backgroundColor: '#70b385',
          borderColor: '#555961',
          data: total
        },
        {
          label: lblmessage1,
          //backgroundColor: '#42A5F5',
          backgroundColor: '#42A5F5',
          borderColor: '#555961',
          data: received
        },
        {
          label: lblmessage2,
          //backgroundColor: '#42A5F5',
          backgroundColor: '#f5428a',
          borderColor: '#555961',
          data: pending
        }


      ]

    }
    this.chartoptions1 = { scales: { yAxes: [{ ticks: { beginAtZero: true, userCallback: function (label, index, labels) { if (Math.floor(label) === label) { return label; } }, } }] } }

    //console.log(this.monthlydashboardgraphmodel);


  }



  setReturngraph() {
    this.returnchartdata = null;
    var lblmessage = "Total";
    var lblmessage1 = "Completed";
    var lblmessage2 = "Pending";
    var pid = [];
    var total = [];
    var received = [];
    var pending = [];
    if (this.returnchartdatalist.length > 0) {
      this.lblmonth = this.monthlist[this.returnchartdatalist[0].smonth];
    }
    this.returnchartdatalist.forEach(element => {
      pid.push(element.displayweek);
      total.push(element.total);
      received.push(element.received);
      pending.push(element.pending);
    });


    this.returnchartdata = {

      labels: pid,
      datasets: [
        {
          label: lblmessage,
          //backgroundColor: '#42A5F5',
          backgroundColor: '#70b385',
          borderColor: '#555961',
          data: total
        },
        {
          label: lblmessage1,
          //backgroundColor: '#42A5F5',
          backgroundColor: '#42A5F5',
          borderColor: '#555961',
          data: received
        },
        {
          label: lblmessage2,
          //backgroundColor: '#42A5F5',
          backgroundColor: '#f5428a',
          borderColor: '#555961',
          data: pending
        }


      ]

    }
    this.chartoptions1 = { scales: { yAxes: [{ ticks: { beginAtZero: true, userCallback: function (label, index, labels) { if (Math.floor(label) === label) { return label; } }, } }] } }

    //console.log(this.monthlydashboardgraphmodel);


  }



  setReservegraph() {
    this.reservechartdata = null;
    var lblmessage = "Total";
    var lblmessage1 = "Completed";
    var lblmessage2 = "Pending";
    var pid = [];
    var total = [];
    var received = [];
    var pending = [];
    if (this.reservechartdatalist.length > 0) {
      this.lblmonth = this.monthlist[this.reservechartdatalist[0].smonth];
    }
    this.reservechartdatalist.forEach(element => {
      pid.push(element.displayweek);
      total.push(element.total);
      received.push(element.received);
      pending.push(element.pending);
    });


    this.reservechartdata = {

      labels: pid,
      datasets: [
        {
          label: lblmessage,
          //backgroundColor: '#42A5F5',
          backgroundColor: '#70b385',
          borderColor: '#555961',
          data: total
        },
        {
          label: lblmessage1,
          //backgroundColor: '#42A5F5',
          backgroundColor: '#42A5F5',
          borderColor: '#555961',
          data: received
        },
        {
          label: lblmessage2,
          //backgroundColor: '#42A5F5',
          backgroundColor: '#f5428a',
          borderColor: '#555961',
          data: pending
        }


      ]

    }
    this.chartoptions1 = { scales: { yAxes: [{ ticks: { beginAtZero: true, userCallback: function (label, index, labels) { if (Math.floor(label) === label) { return label; } }, } }] } }

    //console.log(this.monthlydashboardgraphmodel);


  }



  setTransfergraph() {

    this.transferchartdata = null;
    var lblmessage = "Total";
    var lblmessage1 = "Completed";
    var lblmessage2 = "Pending";
    var pid = [];
    var total = [];
    var received = [];
    var pending = [];
    if (this.transferchartdatalist.length > 0) {
      this.lblmonth = this.monthlist[this.transferchartdatalist[0].smonth];
    }
    this.transferchartdatalist.forEach(element => {
      pid.push(element.displayweek);
      total.push(element.total);
      received.push(element.received);
      pending.push(element.pending);
    });


    this.transferchartdata = {

      labels: pid,
      datasets: [
        {
          label: lblmessage,
          //backgroundColor: '#42A5F5',
          backgroundColor: '#70b385',
          borderColor: '#555961',
          data: total
        },
        {
          label: lblmessage1,
          //backgroundColor: '#42A5F5',
          backgroundColor: '#42A5F5',
          borderColor: '#555961',
          data: received
        },
        {
          label: lblmessage2,
          //backgroundColor: '#42A5F5',
          backgroundColor: '#f5428a',
          borderColor: '#555961',
          data: pending
        }


      ]

    }
    this.chartoptions1 = { scales: { yAxes: [{ ticks: { beginAtZero: true, userCallback: function (label, index, labels) { if (Math.floor(label) === label) { return label; } }, } }] } }

    //console.log(this.monthlydashboardgraphmodel);


  }

  setmonthlyIEgraph(type: string) {

    this.monthlyIEchartdata = null;
    var lblmessage = "";

    if (type == "Request") {
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
  setddl(roleid : any){
  var data1 = this.userrolelist.filter(function (element, index) {
    return (element.roleid == parseInt(roleid));
  });
  if (data1.length > 0) {
    this.rolename = data1[0].accessname;
    this.selectedrolename = this.rolename;
  }
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
      if (this.employee.roleid != this.roleidforinbound) {
        this.navigatebyrole(this.roleidforinbound);
      }
      else {
        this.router.navigateByUrl(decodeURIComponent('WMS/SecurityCheck'));
      }

    }
    else if (type == "Receive" && (this.isreceiveuser)) {
      if (this.employee.roleid != this.roleidforreceipt) {
        this.navigatebyrole(this.roleidforreceipt);
      }
      else {
        this.router.navigateByUrl('WMS/GRNPosting');
      }
    }
    else if (type == "Quality" && this.isqualityuser) {
      if (this.employee.roleid != this.roleidforqualitycheck) {
        this.navigatebyrole(this.roleidforqualitycheck);
      }
      else {
        this.router.navigateByUrl('WMS/QualityCheck');
      }
    }
    else if (type == "Putaway" && (this.isputawayuser)) {
      if (this.employee.roleid != this.roleidforputaway) {
        this.navigatebyrole(this.roleidforputaway);
      }
      else {
        this.router.navigateByUrl('WMS/WarehouseIncharge');
      }
    }
    else if (type == "notify" && this.isnotifyuser) {
      if (this.employee.roleid != this.roleidfornoyify) {
        this.navigatebyrole(this.roleidfornoyify);
      }
      else {
        this.router.navigateByUrl('WMS/Putawaynotify');
      }
    }
    else if (type == 'onhold' && this.isonholduser) {
      if (this.employee.roleid != this.roleidforonhold) {
        this.navigatebyrole(this.roleidforonhold);
      }
      else {
        this.router.navigateByUrl('WMS/HoldGRView');
      }
    }
    else if (type == "Reserve" && this.isreserveuser) {
      if (this.employee.roleid != this.roleidforreserve) {
        this.navigatebyrole(this.roleidforreserve);
      }
      else {
        this.router.navigateByUrl('WMS/MaterialReserveView');
      }
    }
    else if (type == "Approve" && this.isapprover) {
      if (this.employee.roleid != this.roleidforapproval) {
        this.navigatebyrole(this.roleidforapproval);
      }
      else {
        this.router.navigateByUrl('WMS/Home');
      }
    }
    else if (type == "Issue" && this.isissueuser) {
      if (this.employee.roleid != this.roleidforissue) {
        this.navigatebyrole(this.roleidforissue);
      }
      else {
        this.router.navigateByUrl('WMS/MaterialIssueDashboard');
      }
    }
    else if (type == "Count" && this.iscyclecountuser) {
      if (this.employee.roleid != this.roleidforcyclecount) {
        this.navigatebyrole(this.roleidforcyclecount);
      }
      else {
        this.router.navigateByUrl('WMS/Cyclecount');
      }

    }


    //this.setgraph(type);
    //this.setmonthlygraph(type);

  }

  bindnonifications() {
    this.getGatePassList();
  }

  getGatePassList() {
    this.notif = false;

    this.wmsService.getGatePassList().subscribe(data => {
      if (data) {
        this.totalGatePassList = data;
        //PM
        this.gatepassData = this.totalGatePassList.filter(li => li.approverid == this.employee.employeeno && (li.approverstatus == this.approverstatus));
        //FM
        this.gatepassData1 = this.totalGatePassList.filter(li => li.approverstatus == "Approved" && li.fmapprovedstatus == this.approverstatus && li.gatepasstype == "Non Returnable");
        this.prepareGatepassList();
        this.prepareGatepassList1();
      }


    });
  }
  prepareGatepassList() {

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
      if (this.userroleid == '3') {
        this.getCardlist();
      }
      if (this.userroleid == '11') {
        this.getPMCardlist();
      }
      if (this.userroleid == '2') {
        this.getInvCardlist();
      }


      // this.getgraphPMdata();

      this.getmonthlyIEgraphdata();
      this.getIEgraphdata();
      this.spinner.hide();
    })


  }


  getCardlist() {
    this.cardDetailslistdata = new ManagerDashboard();
    this.DashBoardFilters.fromDate = this.datePipe.transform(this.fromDate, "yyyy-MM-dd");
    this.DashBoardFilters.toDate = this.datePipe.transform(this.toDate, "yyyy-MM-dd");
    this.spinner.show();
    this.wmsService.getCardlist(this.DashBoardFilters).subscribe(data => {
      this.cardDetailslistdata = data;
      this.totlReceiptsCnt = this.cardDetailslistdata.completedcount + this.cardDetailslistdata.pendingcount;
      this.totalQulityCnt = this.cardDetailslistdata.qualitycompcount + this.cardDetailslistdata.qualitypendcount;
      this.totalAcceptanceCnt = this.cardDetailslistdata.acceptancecompcount + this.cardDetailslistdata.acceptancependcount;
      this.totalputawayCnt = this.cardDetailslistdata.putawaycompcount + this.cardDetailslistdata.putawaypendcount
      //console.log(this.cardDetailslistdata)

      this.spinner.hide();
    });
  }

  getPMCardlist() {
    this.pmCardDetailslistdata = [];
    this.spinner.show();
    this.wmsService.getPMCardlist(this.DashBoardFilters).subscribe(data => {
      this.pmCardDetailslistdata = data;

      // console.log(this.pmCardDetailslistdata)

      this.spinner.hide();
    });
  }


  getInvCardlist() {
    this.invCardDetailslistdata = [];
    this.spinner.show();
    this.wmsService.getInvCardlist(this.DashBoardFilters).subscribe(data => {
      this.invCardDetailslistdata = data;

      //console.log(this.invCardDetailslistdata)

      this.spinner.hide();
    });
  }

  updateDashBoardData() {
    this.spinner.show();
    if (this.userroleid == "3") {//Inventory clerk
      this.getCardlist();
      this.getreceivedchartdatalist();
      this.getqualitychartdatalist();
      this.getacceptchartdatalist();
      this.getputawaychartdatalist();
    }
    if (this.userroleid == "2") {//Inventory enquiry
      this.getInvCardlist();
      this.getrequestdatalist();
      this.getreturndatalist();
      this.getreservedatalist();
      this.gettransferdatalist();
    }
    if (this.userroleid == "11") {//project manager
      this.getPMCardlist();
      this.getrequestdatalist();
      this.getreturndatalist();
      this.getreservedatalist();
      this.gettransferdatalist();
    }
    this.spinner.hide();
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
  //  this.chartoptions = { scales: { yAxes: [{ ticks: { beginAt
}
