import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { Employee } from '../Models/Common.Model';

@Component({
  selector: 'app-Home',
  templateUrl: './Home.component.html'
})
export class HomeComponent implements OnInit {
  constructor(private router: Router,) { }
  public employee: Employee;
 
  //page load event
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login");
  }
}


