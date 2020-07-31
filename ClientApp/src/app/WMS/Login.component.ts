import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { first } from 'rxjs/operators';
import { constants } from '../Models/WMSConstants';
import { Employee, Login, DynamicSearchResult, rbamaster } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { commonComponent } from '../WmsCommon/CommonCode';
import { NavMenuComponent } from '../nav-menu/nav-menu.component'; 
import { pageModel } from '../Models/WMS.Model';
import { isNullOrUndefined } from 'util';

@Component({
  selector: 'app-Login',
  templateUrl: './Login.component.html',
  providers: [NavMenuComponent]
})
export class LoginComponent implements OnInit {

  constructor(private messageService: MessageService, private navpage: NavMenuComponent, private commonComponent: commonComponent, private formBuilder: FormBuilder, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) {
    this.getRoles();
  }

  public LoginForm: FormGroup;
  public employee: Employee;
  public LoginModel: Login;
  public LoginSubmitted: boolean = false;
  public dataSaved: boolean = false;
  public dynamicData = new DynamicSearchResult();
  public roleNameModel: Array<any> = [];
  public AcessNameList: Array<any> = [];
  pagelist: pageModel[] = [];
  rbalist: rbamaster[] = [];

  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.router.navigateByUrl("Home");

    this.LoginModel = new Login();
    this.rbalist = [];
    this.LoginModel.roleid = "0";
    this.LoginForm = this.formBuilder.group({
      DomainId: ['', [Validators.required]],
      Password: ['', [Validators.required]],
      roleid: [''],
    });
    //this.getRoles();
    this.commonComponent.animateCSS('login', 'zoomInDown');
    if (localStorage.getItem("Roles") && JSON.parse(localStorage.getItem("Roles")))
      this.roleNameModel = JSON.parse(localStorage.getItem("Roles"));
    else {
      var pg = this;
      setTimeout(function () {
        pg.getRoles();
      }, 1000);
    }
  }

  alertDG() {
    debugger;
    if (isNullOrUndefined(localStorage.getItem("Roles")) || localStorage.getItem("Roles") == "null" || localStorage.getItem("Roles") == null || localStorage.getItem("Roles") == "NULL") {
      this.getRoles();
    }
   
     
   
  }

  //get Role list
  getRoles() {
    this.dynamicData = new DynamicSearchResult();
    this.dynamicData.query = "select * from wms.rolemaster where deleteflag=false order by roleid";
    this.wmsService.GetListItems(this.dynamicData).subscribe(data => {
      this.roleNameModel = data;
      localStorage.setItem('Roles', JSON.stringify(this.roleNameModel));
    })
  }

  //Login
  Login() {
    debugger;
    this.LoginSubmitted = true;
    if (this.LoginForm.invalid) {
      return;
    }
    else {
      this.spinner.show();
      this.wmsService.ValidateLoginCredentials(this.LoginForm.value.DomainId, this.LoginForm.value.Password)
        .subscribe(data1 => {
          this.spinner.hide();
          if (data1.message != null) {
            this.messageService.add({ severity: 'error', summary: 'error Message', detail: 'Username or password is incorrect' });
          }
          if (data1.employeeno != null) {
            this.employee = data1;
            var isrolebypass = "0";
            if (this.LoginForm.value.roleid == "0") {
              isrolebypass = "1";
              this.wmsService.getuserroleList(this.employee.employeeno).subscribe(data => {
                if (data.length > 0) {
                  this.AcessNameList = data;
                 
                  this.employee.roleid = this.LoginForm.value.roleid;
                  this.wmsService.getpages().subscribe(datax => {
                    this.pagelist = datax;
                    localStorage.setItem('pages', JSON.stringify(this.pagelist));
                  });
                  localStorage.setItem('Employee', JSON.stringify(this.employee));
                  localStorage.setItem('userroles', JSON.stringify(this.AcessNameList));
                  this.navpage.userloggedHandler(this.employee);
                  //this.router.navigateByUrl("nav"); 
                  //this.wmsService.login();
                  //this.bindMenu();
                }
                else {

                  this.messageService.add({ severity: 'error', summary: 'error Message', detail: 'There is no role assigned to you.' });


                }
              })
              
            }
            else {
              this.wmsService.getuserAcessList(this.employee.employeeno, this.LoginForm.value.roleid).subscribe(data => {
                if (data.length > 0) {
                  this.AcessNameList = data;
                  this.employee.roleid = this.LoginForm.value.roleid;
                  this.wmsService.getpagesbyrole(parseInt(this.employee.roleid)).subscribe(datax => {
                    this.pagelist = datax;
                    localStorage.setItem('pages', JSON.stringify(this.pagelist));
                  });
                  localStorage.setItem('Employee', JSON.stringify(this.employee));
                  this.navpage.userloggedHandler(this.employee);
                  //this.router.navigateByUrl("nav"); 
                  //this.wmsService.login();
                  //this.bindMenu();
                }
                else {
                 
                    this.messageService.add({ severity: 'error', summary: 'error Message', detail: 'Selected Role is not assigned to you, select Your role' });
                  

                }
              })
            

            }
            
          }
        }
        );
    }
  }

}
