import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HttpHeaders } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { constants } from '../Models/WMSConstants'
import { Employee, Login, DynamicSearchResult, printMaterial, rbamaster } from '../Models/Common.Model';
import { PoFilterParams, PoDetails, BarcodeModel, StockModel, materialRequestDetails, inwardModel, gatepassModel, stocktransfermodel, Materials, authUser, invstocktransfermodel, ddlmodel, locataionDetailsStock, updateonhold, materialistModel, outwardmaterialistModel, pageModel, UserDashboardDetail, UserDashboardGraphModel, UnholdGRModel, MRNsavemodel, notifymodel, materialtransferMain, materialReservetorequestModel, testcrud, PrintHistoryModel, materilaTrasFilterParams, materialRequestFilterParams, materialResFilterParams, materialRetFilterParams} from '../Models/WMS.Model';
import { PoFilterParams, PoDetails, BarcodeModel, StockModel, materialRequestDetails, inwardModel, gatepassModel, stocktransfermodel, Materials, authUser, invstocktransfermodel, ddlmodel, locataionDetailsStock, updateonhold, materialistModel, outwardmaterialistModel, pageModel, UserDashboardDetail, UserDashboardGraphModel, UnholdGRModel, MRNsavemodel, notifymodel, materialtransferMain, materialReservetorequestModel, testcrud, PrintHistoryModel, materilaTrasFilterParams, outwardinwardreportModel, UserModel } from '../Models/WMS.Model';
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
  getitemdetailsbygrnnonotif(GrnNo: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getitemdetailsbygrnnonotif?grnnumber=' + GrnNo + '', this.httpOptions);
  }

  getPOList(PoFilterParams: PoFilterParams): Observable<any[]> {
    // POData / GetOpenPoList?pono = 2999930 & vendorid=12
    return this.http.get<any[]>(this.url + 'POData/GetOpenPoList?loginid=' + PoFilterParams.loginid + '&pono=' + PoFilterParams.PONo + '&docno=' + PoFilterParams.DocumentNo + '&vendorid=' + PoFilterParams.venderid + '', this.httpOptions);
  }

  getPONumbers(postatus: string): Observable<any[]> {
    return this.http.get<any[]>(this.url + 'POData/GetPOList?postatus=' + postatus, this.httpOptions);
  }


  //Get PO Details
  getPODetails(empno: string): Observable<any[]> {
    return this.http.get<any[]>(this.url + 'POData/getPODetails?empno=' + empno, this.httpOptions);
  }

  //Get Material Details
  getMatDetails(): Observable<any[]> {
    return this.http.get<any[]>(this.url + 'POData/getMatDetails', this.httpOptions);
  }

  //generate barcode for materials
  generateBarcodeMaterial(printdata: printMaterial): Observable<any> {
    return this.http.post<any>(this.url + 'POData/generateBarcodeMaterial', printdata, this.httpOptions);
  }

  //printBarcodeMaterial(printdata: printMaterial): Observable<any> {
  //  return this.http.post<any>(this.url + 'POData/printBarcodeMaterial', printdata, this.httpOptions);
  //}

  printBarcodeMaterial(printdata: printMaterial): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'Print/printLabel/', printdata, httpOptions);
  }

  insertbarcodeandinvoiceinfo(BarcodeModel: BarcodeModel): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/insertbarcodeandinvoiceinfo', BarcodeModel, httpOptions);
  }

  insertbarcodeinfo(BarcodeModel: BarcodeModel): Observable<any> {
    return this.http.post<any>(this.url + 'POData/insertbarcodeinfo', BarcodeModel, this.httpOptions);
  }

  Getthreewaymatchingdetails(PoNo: string): Observable<inwardModel[]> {
    return this.http.get<inwardModel[]>(this.url + 'POData/Getthreewaymatchingdetails?PONO=' + PoNo + '', this.httpOptions);
  }

  Getdataforqualitycheck(grn: string): Observable<inwardModel[]> {
    return this.http.get<inwardModel[]>(this.url + 'POData/Getqualitydetails?grnnumber=' + grn, this.httpOptions);
  }

  Getdataforholdgr(status: string): Observable<inwardModel[]> {
    return this.http.get<inwardModel[]>(this.url + 'POData/GetholdGRdetails?status=' + status, this.httpOptions);
  }

  getholdgrtbldata(): Observable<inwardModel[]> {
    return this.http.get<inwardModel[]>(this.url + 'POData/GetholdGRmaindetails/', this.httpOptions);
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

  //Check material exists
  checkMatExists(material: string): Observable<any> {
    material = encodeURIComponent(material);
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.get<any>(this.url + 'POData/checkMatExists?material=' + material, httpOptions);
  }

  //Get location details
  getLocationDetails(materialid: string, gnrno: string): Observable<any> {
    materialid = encodeURIComponent(materialid);
    return this.http.get<any>(this.url + 'POData/getlocationdetailsformaterialid?materialid=' + materialid + '&grnnumber=' + gnrno, this.httpOptions);
  }

  //Get material request, isuued and approved details
  getMaterialRequestDetails(materialid: string, gnrno: string): Observable<any> {
    materialid = encodeURIComponent(materialid);
    return this.http.get<any>(this.url + 'POData/getReqMatdetailsformaterialid?materialid=' + materialid + '&grnnumber=' + gnrno, this.httpOptions);
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
  //Amulya
  getMaterialRequestlist(loginid: string, pono: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getmaterialrequestList?PONO=' + pono + '&loginid=' + loginid + '', this.httpOptions);
  }
  
  getreturndata(empno: any): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getreturndata?empno=' + empno + '', this.httpOptions);
  }
  gettransferdata(empno: any): Observable<any> {
    return this.http.get<any>(this.url + 'POData/gettransferdata?empno=' + empno + '', this.httpOptions);
  }

  gettransferdataforapproval(empno: any): Observable<any> {
    return this.http.get<any>(this.url + 'POData/gettransferdataforapproval?empno=' + empno + '', this.httpOptions);
  }

  getdirecttransferdata(empno: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getdirecttransferdata?empno=' + empno, this.httpOptions);
  }
  getMaterialReservelist(loginid: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/GetreserveMaterilalist?reservedby=' + loginid + '', this.httpOptions);
  }
  getMaterialRequestlistdata(loginid: string, pono: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getmaterialrequestListdata?PONO=' + pono + '&loginid=' + loginid + '', this.httpOptions);
  }

  getMaterialReservelistdata(): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getmaterialreserveListdata/', this.httpOptions);
  }

  //materialRequestUpdate(materialRequestList: any): Observable<any> {
  //  return this.http.post<any>(this.url + 'POData/updaterequestedqty/', materialRequestList, this.httpOptions);
  //}

  materialRequestUpdate(materialRequestList: any): Observable<any> {
    return this.http.post<any>(this.url + 'POData/updaterequestedqty/', materialRequestList, this.httpOptions);
  }
  Updatetransferqty(materialRequestList: any): Observable<any> {
    return this.http.post<any>(this.url + 'POData/Updatetransferqty/', materialRequestList, this.httpOptions);
  }
  UpdateReturnmaterialTostock(materialRequestList: any): Observable<any> {
    return this.http.post<any>(this.url + 'POData/UpdateReturnmaterialTostock/', materialRequestList, this.httpOptions);
  }
  materialReserveUpdate(materialRequestList: any): Observable<any> {
    return this.http.post<any>(this.url + 'POData/insertreservematerial/', materialRequestList, this.httpOptions);
  }




  getMaterialIssueLlist(loginid: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getmaterialIssueListbyapproverid?approverid=' + loginid + '', this.httpOptions);
  }
  GetReturnmaterialList(): Observable<any> {
    return this.http.get<any>(this.url + 'POData/GetReturnmaterialList', this.httpOptions);
  }

  GetreleasedMaterilalist(loginid: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/GetreleasedMaterilalist', this.httpOptions);
  }
  getmaterialIssueListbyrequestid(requestid: string, pono: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getmaterialIssueListbyrequestid?requestid=' + requestid + '&pono=' + pono, this.httpOptions);
  }
  getmaterialIssueListbyreserveid(reserveid: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/Getmaterialdetailsbyreserveid?reserveid=' + reserveid + '', this.httpOptions);
  }
  getmaterialissueList(requestid: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getmaterialissueList?requestid=' + requestid + '', this.httpOptions);
  }

  getmaterialreturnreqList(matreqid: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getmaterialreturnreqList?matreturnid=' + matreqid + '', this.httpOptions);
  }

  getreturnmaterialListforconfirm(requestid: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/GetReturnmaterialListForConfirm?requestid=' + requestid + '', this.httpOptions);
  }
  UpdateReturnqty(returnlist: any): Observable<any> {
    return this.http.post<any>(this.url + 'POData/UpdateReturnqty', returnlist, this.httpOptions);
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
  nonreturngetGatePassList(typ: string): Observable<any[]> {
    return this.http.get<any[]>(this.url + 'POData/nonreturngetgatepasslist/?type=' + typ, this.httpOptions);
  }

  outingatepassreport(): Observable<outwardinwardreportModel[]> {
    return this.http.get<outwardinwardreportModel[]>(this.url + 'POData/outwardinwardreport/', this.httpOptions);
  }

  getuserdetailbyempno(empno: string): Observable<UserModel> {
    return this.http.get<UserModel>(this.url + 'POData/getempnamebycode?empno=' + empno, this.httpOptions);
  }


  gatepassmaterialdetail(gatepassId): Observable<any> {

    return this.http.get<any>(this.url + 'POData/getmaterialdetailsbygatepassid?gatepassid=' + gatepassId + '', this.httpOptions);
  }
  getGatePassApprovalHistoryList(gatepassId): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getGatePassApprovalHistoryList?gatepassid=' + gatepassId + '', this.httpOptions);
  }

  checkMaterialandQty(material, qty): Observable<any> {
    debugger;
    material = encodeURIComponent(material);
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.get<any>(this.url + 'POData/checkmaterialandqty?material=' + material + '&qty=' + qty + '', httpOptions);
  }
  getstockdetails(pono: string, materialid: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getstockdetails?pono=' + pono + '&materialid=' + materialid + '', this.httpOptions);
  }

  updategatepassapproverstatus(gatepassModel: any): Observable<any> {
    return this.http.post<any>(this.url + 'POData/updategatepassapproverstatus/', gatepassModel, this.httpOptions);
  }
  GatepassapproveByManager(gatepassModel: any): Observable<any> {
    return this.http.post<any>(this.url + 'POData/GatepassapproveByManager/', gatepassModel, this.httpOptions);
  }
  GatepassapproveByMail(gatepassModel: any): Observable<any> {
    return this.http.post<any>(this.url + 'POData/GatepassapproveByMail/', gatepassModel, this.httpOptions);
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

  getCyclecountList(limita: number, limitb: number, limitc: number): Observable<any> {
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
  getFIFOList(material: any): Observable<any> {
    material = encodeURIComponent(material);
    return this.http.get<any>(this.url + 'POData/GetFIFOList?material=' + material, this.httpOptions);
  }
  checkoldestmaterial(material: any, createddate: any): Observable<any> {
    material = encodeURIComponent(material);
    return this.http.get<any>(this.url + 'POData/Checkoldestmaterial?material=' + material + '&createddate=' + createddate, this.httpOptions);
  }
  insertFIFOdata(materialIssueList: any): Observable<any> {
    return this.http.post<any>(this.url + 'POData/updateFIFOIssueddata/', materialIssueList, this.httpOptions);
  }
  getASNList(currentDate: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getASNList?deliverydate=' + currentDate, this.httpOptions);
  }
  getASNListData(): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getASNListdata/', this.httpOptions);
  }
  getItemlocationListByMaterial(material: string): Observable<any> {
    material = encodeURIComponent(material);
    return this.http.get<any>(this.url + 'POData/GetItemLocationListByMaterial?material=' + material, this.httpOptions);
  }
  getItemlocationListByIssueId(requestforissueid: string): Observable<any> {
    requestforissueid = encodeURIComponent(requestforissueid);
    return this.http.get<any>(this.url + 'POData/getItemlocationListByIssueId?requestforissueid=' + requestforissueid, this.httpOptions);
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
  getuserroleList(employeeId: any): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getuserroleList?employeeid=' + employeeId, this.httpOptions);
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
  getapproverdata(empid: string): Observable<any> {
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

  Updateputawayfiles(ddlModeldt: ddlmodel): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/updateputawayfilename', ddlModeldt, httpOptions);
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
  getcheckedgrnlistfornotify(type: string): Observable<inwardModel[]> {
    return this.http.get<inwardModel[]>(this.url + 'POData/getgrnforacceptancenotify?type=' + type, this.httpOptions);
  }

  getnotifedGRbydate(fromdt: string, todt: string): Observable<inwardModel[]> {
    return this.http.get<inwardModel[]>(this.url + 'POData/getnotifedGRbydate?fromdt=' + fromdt + '&todt=' + todt, this.httpOptions);
  }

  getcheckedgrnlistforqc(): Observable<ddlmodel[]> {
    return this.http.get<ddlmodel[]>(this.url + 'POData/getgrnforacceptanceqc/', this.httpOptions);
  }
  getcheckedgrnlistforqcbydate(fromdt: string, todt: string): Observable<ddlmodel[]> {
    return this.http.get<ddlmodel[]>(this.url + 'POData/getgrnforacceptanceqcbydate?fromdt=' + fromdt + '&todt=' + todt, this.httpOptions);
  }

  getholdgrlist(): Observable<ddlmodel[]> {
    return this.http.get<ddlmodel[]>(this.url + 'POData/getholdgrs/', this.httpOptions);
  }

  getprojectlist(): Observable<ddlmodel[]> {
    return this.http.get<ddlmodel[]>(this.url + 'POData/getprojectlist/', this.httpOptions);
  }

  getprojectlistbymanager(empno: string): Observable<ddlmodel[]> {
    return this.http.get<ddlmodel[]>(this.url + 'POData/getprojectlistbymanager?empno=' + empno, this.httpOptions);
  }

  getmateriallistfortransfer(empno: string): Observable<ddlmodel[]> {
    return this.http.get<ddlmodel[]>(this.url + 'POData/getmateriallistfortransfer?empno=' + empno, this.httpOptions);
  }

  getmateriallistbyproject(pcode: string): Observable<ddlmodel[]> {
    return this.http.get<ddlmodel[]>(this.url + 'POData/getmateriallistbyproject?projectcode=' + pcode, this.httpOptions);
  }

  updateonhold(updaeonhold: updateonhold): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/updateonholddata', updaeonhold, httpOptions);
  }

  updateonholdgr(updaeonhold: UnholdGRModel): Observable<any> {
    return this.http.post<any>(this.url + 'POData/UnholdGR', updaeonhold, this.httpOptions);
  }

  updatemrn(updaeonhold: MRNsavemodel): Observable<any> {
    return this.http.post<any>(this.url + 'POData/mrnupdate', updaeonhold, this.httpOptions);
  }

  notifyputawayfn(data: notifymodel): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/notifyputaway', data, httpOptions);
  }

  notifymultipleputawayfn(data: notifymodel[]): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/notifymultipleputaway', data, httpOptions);
  }



  updateoutinward(outindata: outwardmaterialistModel[]): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/updategatepassmovement/', outindata, httpOptions);
  }

  materialReservetorequest(materialrequestdt: materialReservetorequestModel): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/requestreservematerial/', materialrequestdt, this.httpOptions);
  }

  getpagesbyrole(roleid: number): Observable<pageModel[]> {
    return this.http.get<pageModel[]>(this.url + 'POData/Getpagesbyrole/?roleid=' + roleid, this.httpOptions);
  }

  getpages(): Observable<pageModel[]> {
    return this.http.get<pageModel[]>(this.url + 'POData/Getpages/', this.httpOptions);
  }

  getrbadata(): Observable<rbamaster[]> {
    return this.http.get<rbamaster[]>(this.url + 'POData/getrbamasterdetail/', this.httpOptions);
  }

  getdashdata(empno: string): Observable<UserDashboardDetail> {
    return this.http.get<UserDashboardDetail>(this.url + 'POData/getUserdashboarddata?empno=' + empno, this.httpOptions);
  }
  getdashgraphdata(): Observable<UserDashboardGraphModel[]> {
    return this.http.get<UserDashboardGraphModel[]>(this.url + 'POData/getUserdashgraphdata/', this.httpOptions);
  }

  getmonthlydashgraphdata(): Observable<UserDashboardGraphModel[]> {
    return this.http.get<UserDashboardGraphModel[]>(this.url + 'POData/getmonthlyUserdashgraphdata/', this.httpOptions);
  }
  getweeklydashgraphdata(): Observable<UserDashboardGraphModel[]> {
    return this.http.get<UserDashboardGraphModel[]>(this.url + 'POData/getweeklyUserdashgraphdata/', this.httpOptions);
  }
  updatetransfermaterial(transferdata: materialtransferMain): Observable<any> {
    return this.http.post<any>(this.url + 'POData/mattransfer', transferdata, this.httpOptions);
  }
  approvetransfermaterial(transferdata: materialtransferMain[]): Observable<any> {
    return this.http.post<any>(this.url + 'POData/mattransferapproval', transferdata, this.httpOptions);
  }
  gettestcrud(): Observable<testcrud[]> {
    return this.http.get<testcrud[]>(this.url + 'POData/gettestcrud/', this.httpOptions);
  }

  posttestcrud(data: testcrud): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/postputtestcrud', data, httpOptions);
  }

  deletetestcurd(id: number): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.delete<any>(this.url + 'POData/deletetestcurd/' + id, httpOptions);
  }


  //print bar code

  printBarcode(PrintHistoryModel: PrintHistoryModel): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'Print/printBarcode/', PrintHistoryModel, httpOptions);
  }
  //get material transfer dashboard details
  getMaterialtransferdetails(materialTransferFilters: materilaTrasFilterParams): Observable<any> {
    return this.http.post<any>(this.url + 'POData/getMaterialtransferdetails',materialTransferFilters, this.httpOptions);
  }
  //Amulya
  getMaterialRequestDashboardlist(materialRequestFilters: materialRequestFilterParams): Observable<any> {
    return this.http.post<any>(this.url + 'POData/getmaterialrequestdashboardList', materialRequestFilters, this.httpOptions);
  }
  //Amulya
  getMaterialReserveDashboardlist(materialReserveFilters: materialResFilterParams): Observable<any> {
    return this.http.post<any>(this.url + 'POData/getmaterialreservedashboardList', materialReserveFilters, this.httpOptions);
  }
  
  //Amulya
  getMaterialReturnDashboardlist(materialReturnFilters: materialRetFilterParams): Observable<any> {
    return this.http.post<any>(this.url + 'POData/getMaterialReturnDashboardlist', materialReturnFilters, this.httpOptions);
  }

  getItemlocationListByGatepassmaterialid(gatepassmaterialid: string): Observable<any> {
    gatepassmaterialid = encodeURIComponent(gatepassmaterialid);
    return this.http.get<any>(this.url + 'POData/getItemlocationListByGatepassmaterialid?gatepassmaterialid=' + gatepassmaterialid, this.httpOptions);
  }
}


