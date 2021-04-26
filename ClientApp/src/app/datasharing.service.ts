import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { MenuItem } from 'primeng/api/menuitem';

@Injectable()
export class DataSharingService {
  public meniitems: BehaviorSubject<MenuItem[]> = new BehaviorSubject<MenuItem[]>([]);
  public usernameshare: BehaviorSubject<string> = new BehaviorSubject<string>("");
  public imageurlshare: BehaviorSubject<string> = new BehaviorSubject<string>("");
  public loggedroleshare: BehaviorSubject<string> = new BehaviorSubject<string>("");
}

