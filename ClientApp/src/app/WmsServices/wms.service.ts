import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HttpHeaders } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { constants } from '../Models/WMSConstants'
import { Employee, Login, DynamicSearchResult, printMaterial, rbamaster, locationBarcode, printonholdGR } from '../Models/Common.Model';
import { PoFilterParams, PoDetails, BarcodeModel, StockModel, materialRequestDetails, inwardModel, gatepassModel, stocktransfermodel, Materials, authUser, invstocktransfermodel, ddlmodel, locataionDetailsStock, updateonhold, materialistModel, outwardmaterialistModel, pageModel, UserDashboardDetail, UserDashboardGraphModel, UnholdGRModel, MRNsavemodel, notifymodel, materialtransferMain, materialReservetorequestModel, testcrud, PrintHistoryModel, materilaTrasFilterParams, materialRequestFilterParams, materialResFilterParams, materialRetFilterParams, outwardinwardreportModel, UserModel, WMSHttpResponse, MaterialinHand, matlocations, grReports, MateriallabelModel, ManagerDashboard, pmDashboardCards, invDashboardCards, GraphModelNew, miscellanousIssueData, inventoryFilters, MaterialMaster, GPReasonMTdata, materialList, PlantMTdata, InitialStock, subrolemodel, AssignProjectModel, MaterialTransaction, plantddl, STOrequestTR, assignpmmodel, POReportModel, stocktransfermateriakmodel } from '../Models/WMS.Model';
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
    var sendparam = encodeURIComponent(PoNo);
    return this.http.get<any>(this.url + "POData/CheckPoNoexists?PONO=" + sendparam, this.httpOptions);
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

  //Get PO Details by code
  getPODetailsbyprojectcode(empno: string, projectcode: string): Observable<any[]> {
    return this.http.get<any[]>(this.url + 'POData/getPODetailsbyprojectcode?empno=' + empno + '&projectcode=' + projectcode, this.httpOptions);
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
    //const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/insertbarcodeandinvoiceinfo', BarcodeModel, this.httpOptions);
  }

  insertbarcodeinfo(BarcodeModel: BarcodeModel): Observable<any> {
    return this.http.post<any>(this.url + 'POData/insertbarcodeinfo', BarcodeModel, this.httpOptions);
  }

  Getthreewaymatchingdetails(PoNo: string, invoice: string): Observable<inwardModel[]> {
    return this.http.get<inwardModel[]>(this.url + 'POData/Getthreewaymatchingdetails?PONO=' + PoNo + '&invoice='+invoice, this.httpOptions);
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



  verifythreewaymatch(PoNo: string, invoice: string, type: string, ): Observable<any> {
    return this.http.get<any>(this.url + 'POData/verifythreewaymatch?pono=' + PoNo + '&invoiceno=' + invoice+ '&type='+type, this.httpOptions);
  }

  getInvoiceDetails(PoNo: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getinvoicedetailsforpo?pono=' + PoNo, this.httpOptions);
  }

  //Get Material Details
  getMaterialDetails(grnno: string, pono: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getMaterialDetailsforgrn?grnNo=' + grnno + '&pono=' + pono, this.httpOptions);
  }

  //Check material exists
  checkMatExists(material: string): Observable<any> {
    material = encodeURIComponent(material);
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.get<any>(this.url + 'POData/checkMatExists?material=' + material, httpOptions);
  }

  //Print bin QRCode
  printBinqr(locbarcode: locationBarcode): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'Print/printBinLabel/', locbarcode, httpOptions);
  }

  //Generate Material Label
  generateLabel(labeldata: string): Observable<any> {
    //material = encodeURIComponent(material);
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.get<any>(this.url + 'POData/generateLabel?labeldata=' + labeldata, httpOptions);
  }

  //Get location details
  getLocationDetails(materialid: string, gnrno: string): Observable<any> {
    materialid = encodeURIComponent(materialid);
    return this.http.get<any>(this.url + 'POData/getlocationdetailsformaterialid?materialid=' + materialid + '&grnnumber=' + gnrno, this.httpOptions);
  }

  //Get material request, isuued and approved details
  getMaterialRequestDetails(materialid: string, gnrno: string, pono: string): Observable<any> {
    materialid = encodeURIComponent(materialid);
    return this.http.get<any>(this.url + 'POData/getReqMatdetailsformaterialid?materialid=' + materialid + '&grnnumber=' + gnrno + '&pono=' + pono, this.httpOptions);
  }
  //Get material reserve details
  getMaterialReserveDetails(materialid: string, gnrno: string, pono: string): Observable<any> {
    materialid = encodeURIComponent(materialid);
    return this.http.get<any>(this.url + 'POData/getReserveMatdetailsformaterialtracking?materialid=' + materialid + '&grnnumber=' + gnrno + '&pono=' + pono, this.httpOptions);
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

  InsertmatSTO(StockModel: StockModel[]): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/InsertmatSTO', StockModel, httpOptions);
  }

  PutawayFromInitialStock(StockModel: InitialStock): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/updateitemlocationIS', StockModel, httpOptions);
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
  getrequestdataforapproval(empno: any): Observable<MaterialTransaction[]> {
    return this.http.get<MaterialTransaction[]>(this.url + 'POData/getrequestdataforapproval?empno=' + empno + '', this.httpOptions);
  }
  getrequestdataforSTOapproval(empno: any, type: string): Observable<invstocktransfermodel[]> {
    return this.http.get<invstocktransfermodel[]>(this.url + 'POData/getrequestdataforSTOapproval?empno=' + empno + '&type=' + type, this.httpOptions);
  }

  getdirecttransferdata(empno: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getdirecttransferdata?empno=' + empno, this.httpOptions);
  }
  STORequestlist(): Observable<any> {
    return this.http.get<any>(this.url + 'POData/STORequestlist', this.httpOptions);
  }
  STORequestdatalist(transferid): Observable<STOrequestTR[]> {
    return this.http.get<STOrequestTR[]>(this.url + 'POData/STORequestdatalist?transferid=' + transferid, this.httpOptions);
  }

  getMaterialReservelist(loginid: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/GetreserveMaterilalist?reservedby=' + loginid + '', this.httpOptions);
  }
  getMaterialRequestlistdata(loginid: string, pono: string, projectcode: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getmaterialrequestListdata?PONO=' + pono + '&loginid=' + loginid + '&projectcode=' + projectcode , this.httpOptions);
  }

  getMaterialReservelistdata(projectcode: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getmaterialreserveListdata?projectcode=' + projectcode, this.httpOptions);
  }

 
  getgatepassMaterialRequestlist(): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getgatepassmaterialrequestList/', this.httpOptions);
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

  ackmaterialreceived(materialAckList: any[]): Observable<any> {
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
  getGatePassApprovalHistoryList(gatepassId: string): Observable<any> {
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
  checkoldestmaterialwithdesc(material: any, createddate: any, materialdescription: string): Observable<any> {
    material = encodeURIComponent(material);
    return this.http.get<any>(this.url + 'POData/Checkoldestmaterialwithdesc?material=' + material + '&createddate=' + createddate + '&description=' + materialdescription, this.httpOptions);
  }
  checkoldestmaterialwithdescstore(material: any, createddate: any, materialdescription: string, store: string): Observable<any> {
    material = encodeURIComponent(material);
    return this.http.get<any>(this.url + 'POData/Checkoldestmaterialwithdescstore?material=' + material + '&createddate=' + createddate + '&description=' + materialdescription + '&store=' + store, this.httpOptions);
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
  getpodata(emp: string, projectcode: string): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getASNListdata/', this.httpOptions);
  }
  getItemlocationListByMaterialanddesc(material: string, poitemdescription: string): Observable<any> {
    material = encodeURIComponent(material);
    return this.http.get<any>(this.url + 'POData/GetItemLocationListByMaterialanddesc?material=' + material + '&description=' + poitemdescription, this.httpOptions);
  }
  getItemlocationListByMaterialdescstore(material: string, poitemdescription: string, store: string): Observable<any> {
    material = encodeURIComponent(material);
    return this.http.get<any>(this.url + 'POData/GetItemLocationListByMaterialdescstore?material=' + material + '&description=' + poitemdescription + '&store=' + store, this.httpOptions);
  }
  getItemlocationListByMaterialdescpono(material: string, poitemdescription: string, pono: string): Observable<any> {
    material = encodeURIComponent(material);
    return this.http.get<any>(this.url + 'POData/GetItemLocationListByMaterialdescpono?material=' + material + '&description=' + poitemdescription + '&pono=' + pono, this.httpOptions);
  }
  getItemlocationListByMaterial(material: string): Observable<any> {
    material = encodeURIComponent(material);
    return this.http.get<any>(this.url + 'POData/GetItemLocationListByMaterial?material=' + material, this.httpOptions);
  }
  getItemlocationListByMaterialsourcelocation(material: string,description: string): Observable<any> {
    material = encodeURIComponent(material);
    return this.http.get<any>(this.url + 'POData/GetItemLocationListByMaterialsourcelocation?material=' + material + '&description=' + description, this.httpOptions);
  }
  getItemlocationListByIssueId(requestforissueid: string, requesttype: string): Observable<any> {
    requestforissueid = encodeURIComponent(requestforissueid);
    return this.http.get<any>(this.url + 'POData/getItemlocationListByIssueId?requestforissueid=' + requestforissueid + '&requesttype=' + requesttype , this.httpOptions);
  }
  UpdateMaterialqty(materialList: any): Observable<any> {
    return this.http.post<any>(this.url + 'POData/updateMaterialavailabality', materialList, this.httpOptions);
  }

  assignRole(authuser: authUser): Observable<any> {
    return this.http.post<any>(this.url + 'POData/assignRole/', authuser, this.httpOptions);
  }

  AddAuthUser(authuser: authUser[]): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/updateUserauth/', authuser, httpOptions);
  }

  deleteAuthUser(authuser: authUser): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/deleteUserauth/', authuser, httpOptions);
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
  Getdestinationlocationforist(store : number): Observable<any> {
    return this.http.get<any>(this.url + 'POData/Getdestinationlocationforist?store='+ store, this.httpOptions);
  }
  getrackdata(): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getrackata/', this.httpOptions);
  }
  getbindataforputaway(): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getbindataforputaway/', this.httpOptions);
  }
  getrackdataforputaway(): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getrackataforputaway/', this.httpOptions);
  }
  getMaterial(): Observable<Materials[]> {
    return this.http.get<Materials[]>(this.url + 'POData/GetMaterialdata/', this.httpOptions);
  }
  getPendingpo(): Observable<ddlmodel[]> {
    return this.http.get<ddlmodel[]>(this.url + 'POData/getpendingpos/', this.httpOptions);
  }
  getPendingFilesforIS(): Observable<ddlmodel[]> {
    return this.http.get<ddlmodel[]>(this.url + 'POData/getInitialstockfilename/', this.httpOptions);
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
  getMaterialforstocktransfer(store: number): Observable<Materials[]> {
    return this.http.get<Materials[]>(this.url + 'POData/GetMaterialdatafromstock?store='+store, this.httpOptions);
  }

  getMaterialforstocktransferorder(): Observable<Materials[]> {
    return this.http.get<Materials[]>(this.url + 'POData/getMaterialforstocktransferorder/', this.httpOptions);
  }

  getplantlocdetails(): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getplantlocdetails/', this.httpOptions);
}

  getstocktransferlist(): Observable<stocktransfermodel[]> {
    return this.http.get<stocktransfermodel[]>(this.url + 'POData/getstocktransferdata/', this.httpOptions);
  }
  getstocktransferlistgroup(): Observable<invstocktransfermodel[]> {
    return this.http.get<invstocktransfermodel[]>(this.url + 'POData/getstocktransferdatagroup/', this.httpOptions);
  }
  getstocktransferlistgroup1(transfertye: string): Observable<invstocktransfermodel[]> {
    return this.http.get<invstocktransfermodel[]>(this.url + 'POData/getstocktransferdatagroup1?transfertype=' + transfertye, this.httpOptions);
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
  getstogr(): Observable<ddlmodel[]> {
    return this.http.get<ddlmodel[]>(this.url + 'POData/getpendingstogr/', this.httpOptions);
  }
  getcheckedgrnlistforputaway(): Observable<ddlmodel[]> {
    return this.http.get<ddlmodel[]>(this.url + 'POData/getgrnforacceptanceputaway/', this.httpOptions);
  }

  getuserauthdata(): Observable<authUser[]> {
    return this.http.get<authUser[]>(this.url + 'POData/getuserauthdata/', this.httpOptions);
  }
  getuserauthdetail(empno: string): Observable<authUser[]> {
    return this.http.get<authUser[]>(this.url + 'POData/getuserauthdetail?empno=' + empno, this.httpOptions);
  }
  getuserauthdetailbyrole(roleid: number): Observable<authUser[]> {
    return this.http.get<authUser[]>(this.url + 'POData/getuserauthdetailbyrole?roleid=' + roleid, this.httpOptions);
  }

  getsubrolelist(): Observable<subrolemodel[]> {
    return this.http.get<subrolemodel[]>(this.url + 'POData/getsubroledata/', this.httpOptions);
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
  getprojectlistfortransfer(): Observable<ddlmodel[]> {
    return this.http.get<ddlmodel[]>(this.url + 'POData/getprojectlistfortransfer/', this.httpOptions);
  }
  getprojectlisttosiign(empno: string): Observable<AssignProjectModel[]> {
    return this.http.get<AssignProjectModel[]>(this.url + 'POData/getprojectlisttoassign?empno=' + empno, this.httpOptions);
  }
  getprojectlisttossignPM(): Observable<assignpmmodel[]> {
    return this.http.get<assignpmmodel[]>(this.url + 'POData/getprojectlisttoassignpm/', this.httpOptions);
  }

  getmateriallabeldata(pono: string, lineitemno: number, materialid: string): Observable<MateriallabelModel> {
    return this.http.get<MateriallabelModel>(this.url + 'POData/getmateriallabeldata?pono=' + pono + '&lineitemno=' + lineitemno + '&materialid=' + materialid, this.httpOptions);
  }

  getprojectlistbymanager(empno: string): Observable<ddlmodel[]> {
    return this.http.get<ddlmodel[]>(this.url + 'POData/getprojectlistbymanager?empno=' + empno, this.httpOptions);
  }

  getmateriallistfortransfer(querytext: string): Observable<ddlmodel[]> {
    return this.http.get<ddlmodel[]>(this.url + 'POData/getmateriallistfortransfer?querytext=' + querytext, this.httpOptions);
  }
  getporeport(empno: string, projectcode: string, pono: string): Observable<POReportModel[]> {
    return this.http.get<POReportModel[]>(this.url + 'POData/getporeportdata?empno=' + empno + '&projectcode=' + projectcode + '&pono='+pono, this.httpOptions);
  }
  getsubconannexure(empno: string, subconno: string): Observable<stocktransfermateriakmodel[]> {
    return this.http.get<stocktransfermateriakmodel[]>(this.url + 'POData/getsubconannexuredata?empno=' + empno + '&subconno=' + subconno, this.httpOptions);
  }

  getporeportdetail(materialid: string, description: string, pono: string, querytype: string, requesttype: string, projectcode: string, empno: string): Observable<POReportModel[]> {
    return this.http.get<POReportModel[]>(this.url + 'POData/getporeportdetail?materialid=' + materialid + '&description=' + description + '&pono=' + pono + '&querytype=' + querytype + '&requesttype=' + requesttype + '&projectcode=' + projectcode + '&empno=' + empno, this.httpOptions);
  }

  getmateriallistbyproject(pcode: string): Observable<ddlmodel[]> {
    return this.http.get<ddlmodel[]>(this.url + 'POData/getmateriallistbyproject?projectcode=' + pcode, this.httpOptions);
  }

  updateonhold(updaeonhold: updateonhold): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/updateonholddata', updaeonhold, httpOptions);
  }

  updateinitialstock(stockdata: StockModel): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/updateinitialstock', stockdata, httpOptions);
  }

  updateProjectMember(members: AssignProjectModel): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/updateprojectmember', members, httpOptions);
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

  postinitialstock(data: any): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'Staging/uploadInitialStockExcelByUser', data, httpOptions);
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
  getdashIEgraphdata(): Observable<UserDashboardGraphModel[]> {
    return this.http.get<UserDashboardGraphModel[]>(this.url + 'POData/getUserdashIEgraphdata/', this.httpOptions);
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
  approverequestmaterial(transferdata: MaterialTransaction[]): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/matrequestapproval', transferdata, httpOptions);
  }
  approveSTOrequestmaterial(transferdata: invstocktransfermodel[]): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/STOmatrequestapproval', transferdata, httpOptions);
  }
  assignRBA(rbadata: rbamaster[]): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/updaterba', rbadata, httpOptions);
  }
  updatepm(transferdata: assignpmmodel[]): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/updatepm', transferdata, httpOptions);
  }

  insertcsv(): Observable<WMSHttpResponse> {
    return this.http.get<WMSHttpResponse>(this.url + 'Staging/uploadInitialStockExcel', this.httpOptions);
  }

  addddtostock(): Observable<any[]> {
    return this.http.get<any[]>(this.url + 'Staging/uploadInitialStock', this.httpOptions);
  }

 
 

  gettestcrud(): Observable<testcrud[]> {
    return this.http.get<testcrud[]>(this.url + 'POData/gettestcrud/', this.httpOptions);
  }

  getmatinhand(inventoryFilters: inventoryFilters): Observable<MaterialinHand[]> {
    return this.http.post<MaterialinHand[]>(this.url + 'POData/getmatinhand',inventoryFilters, this.httpOptions);
  }
  getmatinhandlocations(poitemdescription:string): Observable<matlocations[]> {
    return this.http.get<matlocations[]>(this.url + 'POData/getmatinhandlocation?poitemdescription=' + poitemdescription, this.httpOptions);
  }

  getinitialStock(uploadcode: string): Observable<StockModel[]> {
    return this.http.get<StockModel[]>(this.url + 'POData/getinitialstock?code=' + uploadcode, this.httpOptions);
  }
  getinitialStockAllrecords(uploadcode: string): Observable<StockModel[]> {
    return this.http.get<StockModel[]>(this.url + 'POData/getinitialstockall?code=' + uploadcode, this.httpOptions);
  }
  getinitialStockEX(uploadcode: string): Observable<StockModel[]> {
    return this.http.get<StockModel[]>(this.url + 'POData/getinitialstockEX?code=' + uploadcode, this.httpOptions);
  }
  getinitialStockReport(empno: string): Observable<StockModel[]> {
    return this.http.get<StockModel[]>(this.url + 'POData/getinitialstockReport?code=' + empno, this.httpOptions);
  }
  getinitialStockReportGroup(empno: string): Observable<StockModel[]> {
    return this.http.get<StockModel[]>(this.url + 'POData/getinitialstockReportGroup?code=' + empno, this.httpOptions);
  }
  //Amulya
  getinitialStockLoad(): Observable<StockModel[]> {
    return this.http.get<StockModel[]>(this.url + 'POData/getinitialstockload/', this.httpOptions);
  }

  getinitialStockMaterialForPutaway(): Observable<InitialStock[]> {
    return this.http.get<InitialStock[]>(this.url + 'POData/GetInitialStockPutawayMaterials/', this.httpOptions);
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
  getMaterialRequestDashboardlist(materialRequestFilters: materialRequestFilterParams): Observable<MaterialTransaction[]> {
    return this.http.post<MaterialTransaction[]>(this.url + 'POData/getmaterialrequestdashboardList', materialRequestFilters, this.httpOptions);
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

  getreceiptslist(): Observable<any> {
    return this.http.get<UserDashboardGraphModel[]>(this.url + 'POData/getManagerdashboardgraphdata/', this.httpOptions);
  }

  getCardlist(): Observable<any> {
    return this.http.get<ManagerDashboard>(this.url + 'POData/getManagerdashboardgraphdata/', this.httpOptions);
  }


  getPODataList(suppliername: any): Observable<any> {
    //console.log(suppliername)
    return this.http.get<any[]>(this.url + 'POData/getPODataList?suppliername=' + suppliername, this.httpOptions);
  }

  getGRListData(): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getGRListdata/', this.httpOptions);
  }

  SAPGREditReport(data: grReports): Observable<any> {
    //console.log(sapgr);
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/SAPGREditReport', data, httpOptions);
  }

  editGRReports(wmsgr: string): Observable<grReports> {
    return this.http.get<grReports>(this.url + 'POData/addEditReports?wmsgr=' + wmsgr, this.httpOptions);
  }

  getPMCardlist(): Observable<any> {
    return this.http.get<pmDashboardCards>(this.url + 'POData/getPMdashboarddata/', this.httpOptions);
  }

  getmonthlyUserdashboardIEgraphdata(): Observable<UserDashboardGraphModel[]> {
    return this.http.get<UserDashboardGraphModel[]>(this.url + 'POData/getmonthlyUserdashboardIEgraphdata/', this.httpOptions);
  }
  getInvCardlist(): Observable<any> {
    return this.http.get<invDashboardCards>(this.url + 'POData/getInvdashboarddata/', this.httpOptions);
  }

  getPMdashgraphdata(): Observable<UserDashboardGraphModel[]> {
    return this.http.get<UserDashboardGraphModel[]>(this.url + 'POData/getUserdashboardgraphPMdata/', this.httpOptions);
  }

  getreceivedgraphdata(): Observable<GraphModelNew[]> {
    return this.http.get<GraphModelNew[]>(this.url + 'POData/getReceivedgraph/', this.httpOptions);
  }
  getqualitygraphdata(): Observable<GraphModelNew[]> {
    return this.http.get<GraphModelNew[]>(this.url + 'POData/getQualitygraph/', this.httpOptions);
  }
  getacceptgraphdata(): Observable<GraphModelNew[]> {
    return this.http.get<GraphModelNew[]>(this.url + 'POData/getAcceptgraph/', this.httpOptions);
  }
  getputawaygraphdata(): Observable<GraphModelNew[]> {
    return this.http.get<GraphModelNew[]>(this.url + 'POData/getPutawaygraph/', this.httpOptions);
  }

  getrequestgraphdata(): Observable<GraphModelNew[]> {
    return this.http.get<GraphModelNew[]>(this.url + 'POData/getRequestgraph/', this.httpOptions);
  }

  getreturngraphdata(): Observable<GraphModelNew[]> {
    return this.http.get<GraphModelNew[]>(this.url + 'POData/getReturngraph/', this.httpOptions);
  }

  getreservegraphdata(): Observable<GraphModelNew[]> {
    return this.http.get<GraphModelNew[]>(this.url + 'POData/getReservegraph/', this.httpOptions);
  }

  gettransfergraphdata(): Observable<GraphModelNew[]> {
    return this.http.get<GraphModelNew[]>(this.url + 'POData/getTransfergraph/', this.httpOptions);
  }


  getMiscellanousIssueList(initialstock: boolean): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getMiscellanousIssueList/' + initialstock, this.httpOptions);
  }
  
  getMiscellanousReceiptsList(): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getMiscellanousReceiptsList/' , this.httpOptions);
  }
  miscellanousIssueDataUpdate(data: miscellanousIssueData): Observable<any> {
    return this.http.post<any>(this.url + 'POData/miscellanousIssueDataUpdate', data, this.httpOptions);
  }
  miscellanousReceiptDataUpdate(data: StockModel): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/updateMiscellanousReceipt', data, httpOptions);
  }

getMaterialMasterList(): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getMaterialMasterList/', this.httpOptions);
  }
  materialMasterUpdate(data: MaterialMaster): Observable<any> {
    return this.http.post<any>(this.url + 'POData/updateMaterialMaster', data, this.httpOptions);
  }
  GPReasonAdd(data: GPReasonMTdata): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/GPReasonMTAdd', data, httpOptions);
  }

  MiscellanousReasonAdd(data: GPReasonMTdata): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/MiscellanousReasonAdd', data, httpOptions);
  }

  createplant(data: PlantMTdata): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/createplant', data, httpOptions);
  }

  GPReasonDelete(data: GPReasonMTdata): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/GPReasonMTDelete', data, httpOptions);
  }


  PlantnameDelete(data: PlantMTdata): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/PlantnameDelete', data, httpOptions);
  }

  getGPReasonData(): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getGPReasonData',  this.httpOptions);
  }

  getplantnameData(): Observable<any> {
  return this.http.get<any>(this.url + 'POData/getplantnameData', this.httpOptions);
}

  getSTORequestList(type: string): Observable<any[]> {
    return this.http.get<any[]>(this.url + 'POData/getSTORequestList?type='+type, this.httpOptions);
  }
  getavailabilityByStore(store: string, material: string, description: string, projectcode: string): Observable<WMSHttpResponse> {
    return this.http.get<WMSHttpResponse>(this.url + 'POData/getAvailableQtyBystore?store=' + store + '&materialid=' + material + '&description=' + description + '&projectcode=' + projectcode, this.httpOptions);
  }
  getSubcontractRequestListforissue(): Observable<any[]> {
    return this.http.get<any[]>(this.url + 'POData/getSubcontractRequestListforissue/', this.httpOptions);
  }

  getMatdetailsbyTransferId(transferid: string, type: string, transfertype: string): Observable<any[]> {
    return this.http.get<any>(this.url + 'POData/getMatdetailsbyTransferId?TransferId=' + transferid + '&type=' + type + '&transfertype=' + transfertype + '', this.httpOptions);
  }
  STOPOInitiate(list: any): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/STOPOInitiate/', list, httpOptions);
  }

  getMiscellanousReasonData(): Observable<any> {
    return this.http.get<any>(this.url + 'POData/getMiscellanousReasonData', this.httpOptions);
  }
  updateSubcontractAcKstatus(ackData: any[]): Observable<any> {
    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'POData/updateSubcontractAcKstatus/', ackData, httpOptions);
  }
  subcontractInoutList(): Observable<any[]> {
    return this.http.get<any[]>(this.url + 'POData/subcontractInoutList/', this.httpOptions);
  }
   generateqronhold(printdata: printonholdGR): Observable<any> {
    return this.http.post<any>(this.url + 'POData/generateqronhold', printdata, this.httpOptions);
  }

  printonholdmaterials(printdata: printonholdGR): Observable<any> {

    const httpOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }), responseType: 'text' as any };
    return this.http.post<any>(this.url + 'Print/printonholdmaterials',printdata, httpOptions);
  }
}


