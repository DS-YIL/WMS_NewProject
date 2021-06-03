import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Employee } from '../Models/Common.Model';


@Component({
  selector: 'app-errorpage',
  templateUrl: './errorpage.component.html'
})
export class errorpageComponent implements OnInit {
  constructor( private route: ActivatedRoute, private router: Router) {}
  public employee: Employee;
  ngOnInit() {
    if (localStorage.getItem("Employee"))
      this.employee = JSON.parse(localStorage.getItem("Employee"));
    else
      this.router.navigateByUrl("Login"); 
  }
}
