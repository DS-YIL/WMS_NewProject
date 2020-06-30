import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { wmsService } from '../WmsServices/wms.service';
import { first } from 'rxjs/operators';
import { constants } from '../Models/WMSConstants';
import { Employee, Login, DynamicSearchResult } from '../Models/Common.Model';
import { NgxSpinnerService } from "ngx-spinner";
import { MessageService } from 'primeng/api';
import { commonComponent } from '../WmsCommon/CommonCode';
import { NavMenuComponent } from '../nav-menu/nav-menu.component'; 

@Component({
  selector: 'app-Login',
  templateUrl: './Login.component.html',
  providers: [NavMenuComponent]
})
export class LoginComponent implements OnInit {

  constructor(private messageService: MessageService, private navpage: NavMenuComponent,  private commonComponent: commonComponent, private formBuilder: FormBuilder, private wmsService: wmsService, private route: ActivatedRoute, private router: Router, public constants: constants, private spinner: NgxSpinnerService) { }

  public LoginForm: FormGroup;
  public employee: Employee;
  public LoginModel: Login;
  public LoginSubmitted: boolean = false;
  public dataSaved: boolean = false;
  public dynamicData = new DynamicSearchResult();
  public roleNameModel: Array<any> = [];
  public AcessNameList: Array<any> = [];

  ngOnInit() {
    if (localStorage.getItem("Employee"))
         this.router.navigateByUrl("Home"); 
    this.LoginModel = new Login();
    this.LoginModel.roleid = "0";
    this.LoginForm = this.formBuilder.group({
      DomainId: ['', [Validators.required]],
      Password: ['', [Validators.required]],
      roleid: ['', [Validators.required]],
    });
    this.commonComponent.animateCSS('login', 'zoomInDown');
    if (localStorage.getItem("Roles") && JSON.parse(localStorage.getItem("Roles")))
      this.roleNameModel = JSON.parse(localStorage.getItem("Roles"));
    else
      this.getRoles();
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
    this.LoginSubmitted = true;
    if (this.LoginForm.invalid || this.LoginModel.roleid == '0') {
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
            this.wmsService.getuserAcessList(this.employee.employeeno, this.LoginForm.value.roleid).subscribe(data => {
              if (data.length > 0) {
                this.AcessNameList = data;
                this.employee.roleid = this.LoginForm.value.roleid;
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
        );
    }
  }

}
