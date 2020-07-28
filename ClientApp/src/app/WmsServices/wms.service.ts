import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HttpHeaders } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { constants } from '../Models/WMSConstants'
import { Employee, Login, DynamicSearchResult, printMaterial } from '../Models/Common.Model';
import { PoFilterParams, PoDetails, BarcodeModel, StockModel, materialRequestDetails, inwardModel, gatepassModel, stocktransfermodel, Materials, authUser, invstocktransfermodel, ddlmodel, locataionDetailsStock,updateonhold, materialistModel, outwardmaterialistModel } from '../Models/WMS.Model';
import { Text } from '@angular/compiler/src/i18n/i18n_ast';

@Injectable({
  providedIn: 'root'
})
export class wmsService {

  public url = "";
  public httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }) };
  public httpOption = { headers: new HttpHeaders({ 'Content-Type': 'text' }) };
  private currentUserSubject: BehaviorSubject<Employee>;
  public currentUser: Observable<Employee>;
  constructor(private http: HttpClient, private constants: constants, @Inject('BASE_URL') baseUrl: string) {
    this.currentUserSubject = new BehaviorSubject<Employee>(JSON.parse(localStorage.getItem('Employee')));
    this.currentUser = this.currentUserSubject.asObservable();
    this.url = baseUrl;
  }
  public get currentUserValue(): Employee {
    return this.currentUserSubject.value;
  }



  //Login
  ValidateLoginCredentials(uname: string, pwd: string) {
    var login = new Login();
    login.Username = uname;
    login.Password = pwd;
    return this.http.post<any>(this.url + 'Users/authenticate/', login)
      .pipe(map(data => {
        if (data.employeeno != null) {
          //const object = Object.assign({}, ...data);

          //this.currentUserSubject.next(data);
        }
        return data;
      }))
  }

  GetListItems(search: DynamicSearchResult): Observable<any> {
    return this.http.post<any>(this.url + 'POData/GetListItems/', search, this.httpOptions);
  }

  //get stocktype
  getstocktype(details: locataionDetailsStock): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/getstocktype/', details, httpOptions);
  }

  GetMaterialItems(search: DynamicSearchResult): Observable<any> {
    return this.http.post<any>(this.url + 'POData/GetMaterialItems/', search, this.httpOptions);
  }

  getPoDetails(PoNo: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/CheckPoNoexists?PONO=' + PoNo + '', this.httpOptions);
  }
  getitemdetailsbygrnno(GrnNo: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getitemdetailsbygrnno?grnnumber=' + GrnNo + '', this.httpOptions);
  }

  getPOList(PoFilterParams: PoFilterParams): Observable<any[]> {
    // POData / GetOpenPoList?pono = 2999930 & vendorid=12
    return this.http.get<any[]>(this.url + 'POData/GetOpenPoList?loginid=' + PoFilterParams.loginid + '&pono=' + PoFilterParams.PONo + '&docno=' + PoFilterParams.DocumentNo + '&vendorid=' + PoFilterParams.venderid + '', this.httpOptions);
  }

  getPONumbers(postatus: string): Observable<any[]> {
    return this.http.get<any[]>(this.url + 'POData/GetPOList?postatus=' + postatus, this.httpOptions);
  }

  //generate barcode for materials
  generateBarcodeMaterial(printdata: printMaterial): Observable<any> {
    return this.http.post<any>(this.url + 'POData/generateBarcodeMaterial', printdata, this.httpOptions);
  }

  insertbarcodeandinvoiceinfo(BarcodeModel: BarcodeModel): Observable<any> {
    return this.http.post<any>(this.url + 'POData/insertbarcodeandinvoiceinfo', BarcodeModel, this.httpOptions);
  }

  insertbarcodeinfo(BarcodeModel: BarcodeModel): Observable<any> {
    return this.http.post<any>(this.url + 'POData/insertbarcodeinfo', BarcodeModel, this.httpOptions);
  }

  Getthreewaymatchingdetails(PoNo: string): Observable<inwardModel[]> {
    return this.http.get<inwardModel[]>(this.url + 'POData/Getthreewaymatchingdetails?PONO=' + PoNo + '', this.httpOptions);
  }

  Getdataforqualitycheck(): Observable<inwardModel[]> {
    return this.http.get<inwardModel[]>(this.url + 'POData/Getqualitydetails', this.httpOptions);
  }

  

  verifythreewaymatch(PoNo: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/verifythreewaymatch?pono=' + PoNo, this.httpOptions);
  }

  getInvoiceDetails(PoNo: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getinvoicedetailsforpo?pono=' + PoNo, this.httpOptions);
  }

  //Get Material Details
  getMaterialDetails(grnno: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getMaterialDetailsforgrn?grnNo=' + grnno, this.httpOptions);
  }

  //Get location details
  getLocationDetails(materialid: string, gnrno:string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getlocationdetailsformaterialid?materialid=' + materialid + '&grnnumber=' + gnrno, this.httpOptions);
  }

  //Get material request, isuued and approved details
  getMaterialRequestDetails(materialid: string,gnrno:string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getReqMatdetailsformaterialid?materialid=' + materialid + '&grnnumber=' + gnrno,this.httpOptions);
  }

  insertitems(inwardModel: inwardModel[]): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/receiveinvoice', inwardModel, httpOptions);
  }
  insertqualitycheck(inwardModel: inwardModel[]): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/qualitycheck', inwardModel, httpOptions);
  }

  insertreturn(inwardModel: inwardModel[]): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/insertreturn', inwardModel, httpOptions);
  }

  InsertStock(StockModel: StockModel[]): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/updateitemlocation', StockModel, httpOptions);
  }

  Stocktransfer(StockModel: StockModel[]): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/UpdateStockTransfer', StockModel, httpOptions);
  }

  getMaterialRequestlist(loginid: string, pono: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getmaterialrequestList?PONO=' + pono + '&loginid=' + loginid + '', this.httpOptions);
  }
  getMaterialReservelist(loginid: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/GetreserveMaterilalist?reservedby=' + loginid + '', this.httpOptions);
  }
  getMaterialRequestlistdata(loginid: string, pono: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getmaterialrequestListdata?PONO=' + pono + '&loginid=' + loginid + '', this.httpOptions);
  }

  materialRequestUpdate(materialRequestList: any): Observable<any> {
    return this.http.post<any>(this.url + 'POData/updaterequestedqty/', materialRequestList, this.httpOptions);
  }
  materialReserveUpdate(materialRequestList: any): Observable<any> {
    return this.http.post<any>(this.url + 'POData/insertreservematerial/', materialRequestList, this.httpOptions);
  }


  getMaterialIssueLlist(loginid: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getmaterialIssueListbyapproverid?approverid=' + loginid + '', this.httpOptions);
  }
  GetreleasedMaterilalist(loginid: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/GetreleasedMaterilalist', this.httpOptions);
  }
  getmaterialIssueListbyrequestid(requestid: string,pono:string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getmaterialIssueListbyrequestid?requestid=' + requestid + '&pono='+pono, this.httpOptions);
  }
  getmaterialIssueListbyreserveid(reserveid: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/Getmaterialdetailsbyreserveid?reserveid=' + reserveid + '', this.httpOptions);
  }
  getmaterialissueList(requestid: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getmaterialissueList?requestid=' + requestid + '', this.httpOptions);
  }
  UpdateReturnqty(returnlist: any): Observable<any> {
    return this.http.post<any>(this.url + 'POData/UpdateReturnqty',returnlist, this.httpOptions);
  }
  getmaterialissueListforreserved(reservedid: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getmaterialissueListforreserved?reservedid=' + reservedid + '', this.httpOptions);
  }


  approvematerialrequest(materialIssueList: any): Observable<any> {
    return this.http.post<any>(this.url + 'POData/approvematerialrequest/', materialIssueList, this.httpOptions);
  }
  approvematerialrelease(materialIssueList: any): Observable<any> {
    return this.http.post<any>(this.url + 'POData/approvematerialrelease/', materialIssueList, this.httpOptions);
  }
  
  ackmaterialreceived(materialAckList: any): Observable<any> {
    return this.http.post<any>(this.url + 'POData/ackmaterialreceived/', materialAckList, this.httpOptions);
  }
  ackmaterialreceivedfroreserved(materialAckList: any): Observable<any> {
    return this.http.post<any>(this.url + 'POData/ackmaterialreceivedfroreserved/', materialAckList, this.httpOptions);
  }
  getGatePassList(): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getgatepasslist/', this.httpOptions);
  }
  nonreturngetGatePassList(typ: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/nonreturngetgatepasslist/?type=' + typ, this.httpOptions);
  }

  
  gatepassmaterialdetail(gatepassId): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getmaterialdetailsbygatepassid?gatepassid=' + gatepassId + '', this.httpOptions);
  }
  getGatePassApprovalHistoryList(gatepassId): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getGatePassApprovalHistoryList?gatepassid=' + gatepassId + '', this.httpOptions);
  }

  checkMaterialandQty(material, qty): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.get<any>(this.url + 'POData/checkmaterialandqty?material=' + material + '&qty=' + qty + '', httpOptions);
  }
  updategatepassapproverstatus(gatepassModel: any): Observable<any> {
    return this.http.post<any>(this.url + 'POData/updategatepassapproverstatus/', gatepassModel, this.httpOptions);
  }
  GatepassapproveByManager(gatepassModel: any): Observable<any> {
    return this.http.post<any>(this.url + 'POData/GatepassapproveByManager/', gatepassModel, this.httpOptions);
  }
  deleteGatepassmaterial(id: number): Observable<any> {
    return this.http.delete<any>(this.url + 'POData/deletegatepassmaterial?gatepassmaterialid=' + id + '', this.httpOptions);
  }
  updateprintstatus(gatepassModel: gatepassModel) {
    return this.http.post<any>(this.url + 'POData/updateprintstatus/', gatepassModel, this.httpOptions);
  }
  saveoreditgatepassmaterial(gatepassList: any): Observable<any> {
    return this.http.post<any>(this.url + 'POData/saveoreditgatepassmaterial/', gatepassList, this.httpOptions);
  }

  getInventoryList(): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getInventoryList/', this.httpOptions);
  }

  getABCavailableqtyList(): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getABCavailableqtyList/', this.httpOptions);
  }
  

  GetABCListBycategory(category: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/GetABCListBycategory/?category=' + category + '', this.httpOptions);
  }
  getcategorymasterdata(): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getcategorymasterdata/', this.httpOptions);
  }
  updateABCRange(catList: any): Observable<any> {
    return this.http.post<any>(this.url + 'POData/updateABCRange/', catList, this.httpOptions);
  }

  GetreportBasedCategory(): Observable<any> {
    return this.http.get<any>(this.url + 'POData/GetreportBasedCategory/', this.httpOptions);
  }
  
  getCyclecountList(limita: number, limitb:number, limitc : number): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getCyclecountList/?limita=' + limita + '&limitb=' + limitb + '&limitc=' + limitc, this.httpOptions);
  }
  getCyclecountPendingList(): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getCyclecountPendingList/', this.httpOptions);
  }
  getCyclecountConfig(): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getCyclecountconfig/', this.httpOptions);
  }
  updateCyclecountconfig(configlist: any): Observable<any> {
    return this.http.post<any>(this.url + 'POData/updateCyclecountconfig/', configlist, this.httpOptions);
  }
  updateinsertCyclecount(catList: any): Observable<any> {
    return this.http.post<any>(this.url + 'POData/updateinsertCyclecount/', catList, this.httpOptions);
  }
  getFIFOList(material:any): Observable<any> {
    return this.http.get<any>(this.url + 'POData/GetFIFOList?material=' + material, this.httpOptions);
  }
  checkoldestmaterial(material: any, createddate:any): Observable<any> {
    return this.http.get<any>(this.url + 'POData/Checkoldestmaterial?material=' + material + '&createddate=' + createddate, this.httpOptions);
  }
  insertFIFOdata(materialIssueList: any): Observable<any> {
    return this.http.post<any>(this.url + 'POData/updateFIFOIssueddata/', materialIssueList, this.httpOptions);
  }
  getASNList(currentDate: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getASNList?deliverydate=' + currentDate,this.httpOptions);
  }
  getItemlocationListByMaterial(material: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/GetItemLocationListByMaterial?material=' + material, this.httpOptions);
  }
  UpdateMaterialqty(materialList: any): Observable<any> {
    return this.http.post<any>(this.url + 'POData/updateMaterialavailabality', materialList, this.httpOptions);
  }

  assignRole(authuser: authUser): Observable<any> {
    return this.http.post<any>(this.url + 'POData/assignRole/', authuser, this.httpOptions);
  }

  getuserAcessList(employeeId: any, roleid: any): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getuserAcessList?employeeid=' + employeeId + '&roleid=' + roleid, this.httpOptions);
  }
  getASNPOReceivedList(): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getSecurityReceivedList/', this.httpOptions);
  }

  login() {
    if (localStorage.getItem("Employee"))
    this.currentUserSubject.next(JSON.parse(localStorage.getItem("Employee")));
  }
  logout() {
    //localStorage.removeItem('Employee');
    this.currentUserSubject.next(null);
    //window.location.reload();
  }
  getlocationdata(): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getlocationdata/', this.httpOptions);
  }
  getbindata(): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getbindata/', this.httpOptions);
  }
  getrackdata(): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getrackata/', this.httpOptions);
  }
  getMaterial(): Observable<Materials[]> {
    return this.http.get<Materials[]>(this.url + 'POData/GetMaterialdata/', this.httpOptions);
  }
  getPendingpo(): Observable<ddlmodel[]> {
    return this.http.get<ddlmodel[]>(this.url + 'POData/getpendingpos/', this.httpOptions);
  }
  getapproverdata(empid:string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getapproverList?empid=' + empid, this.httpOptions);
  }
  getgatepassapproverdata(empid: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getgatepassByapproverList?empid=' + empid, this.httpOptions);
  }
  getSafteyStockList(): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getSafteyStockList', this.httpOptions);
  }
  GetBinList(): Observable<any> {
    return this.http.get<any>(this.url + 'POData/GetBinList', this.httpOptions);
  }
   getMaterialforstocktransfer(): Observable<Materials[]> {
    return this.http.get<Materials[]>(this.url + 'POData/GetMaterialdatafromstock/', this.httpOptions);
  }
  getstocktransferlist(): Observable<stocktransfermodel[]> {
    return this.http.get<stocktransfermodel[]>(this.url + 'POData/getstocktransferdata/', this.httpOptions);
  }
  getstocktransferlistgroup(): Observable<invstocktransfermodel[]> {
    return this.http.get<invstocktransfermodel[]>(this.url + 'POData/getstocktransferdatagroup/', this.httpOptions);
  }
  getstocktransferlistgroup1(): Observable<invstocktransfermodel[]> {
    return this.http.get<invstocktransfermodel[]>(this.url + 'POData/getstocktransferdatagroup1/', this.httpOptions);
  }

  Stocktransfer1(StockModel: invstocktransfermodel): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/UpdateStockTransfer1', StockModel, httpOptions);
  }
  getdepartments(): Observable<ddlmodel[]> {
    return this.http.get<ddlmodel[]>(this.url + 'POData/getdepartment/', this.httpOptions);
  }

  getcheckedgrnlist(): Observable<ddlmodel[]> {
    return this.http.get<ddlmodel[]>(this.url + 'POData/getgrnforacceptance/', this.httpOptions);
  }
  getcheckedgrnlistforputaway(): Observable<ddlmodel[]> {
    return this.http.get<ddlmodel[]>(this.url + 'POData/getgrnforacceptanceputaway/', this.httpOptions);
  }

  updateonhold(updaeonhold: updateonhold): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/updateonholddata', updaeonhold, httpOptions);
  }

  updateoutinward(outindata: outwardmaterialistModel[]): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/updategatepassmovement/', outindata, httpOptions);
  }
  
}


