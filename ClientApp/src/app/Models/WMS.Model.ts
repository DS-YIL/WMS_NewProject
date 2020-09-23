export class PoFilterParams {
  loginid: string = "";
  PONo: string = "";
  venderid: string = "";
  DocumentNo: string = "";
}

export class PoDetails {
  pono: string = "";
  departmentid: number;
  invoiceno: string;
  Barcode: string = "";
  quotationqty: string;
  returnqty: string;
  pendingqty: string;
  vendorname: string;
  vendorid: number;
  paitemid: number;
  returnid: number;
  ProjectName: string;
  projectcode: string;
  putawayQty: string;
  returnQty: string;
  location: string;
  rackNo: string;
  material: string;
  materialid: any;
  materialdescription: string;
  grnnumber: string;
  itemid: number;
  confirmqty: number;
  materialqty: string;
  storeid: any;
  rackid: any;
  binid: any;
  inwardid: number;
  asnno: string;
}

export class BarcodeModel {
  barcodeid: number;
  paitemid: number;
  barcode: string;
  createddate: Date
  createdby: string
  deleteflag: boolean
  inwmasterid: number;
  pono: string;
  invoiceno: string;
  departmentid: number;
  invoicedate: Date;
  receivedby: string;
  suppliername: string;
  asnno: string;
  inwardremarks: string;
  docfile: string
}

export class Cyclecountconfig {
  id: number;
  apercentage: number;
  bpercentage: number;
  cpercentage: number;
  cyclecount: number;
  frequency: string;
  enddate: Date;
  startdate: Date;
  countall: number;
  notificationtype: string;
  notificationon: string;
}
export class UserDashboardGraphModel {
  smonth: string;
  syear: string;
  count: number;
  graphdate: Date;
  type: string;
}

export class UnholdGRModel {
  inwmasterid: number;
  unholdaction: boolean;
  unholdedby: string;
  unholdremarks: string
}



export class inwardModel {
  serialno: number;
  inwardid: number;
  inwmasterid: number;
  poitemid: number;
  receivedqty: string;
  receiveddate: Date;
  receivedby: string;
  returnqty: number;
  confirmqty: number;
  pendingqty: number;
  pono: string;
  quality: string;
  qtype: string;
  qcdate: Date;
  qcby: string;
  remarks: string;
  grnnumber: string;
  grndate: Date;
  itemlocation: string;
  invoiceno: string;
  projectname: string;
  materialqty: number;
  qualitycheck: boolean;
  checkedby: string;
  material: string;
  materialdescription: string;
  returnedby: string;
  returnedon: Date;
  qualitypassedqty: number;
  qualityfailedqty: number;
  returnremarks: string;
  qualitychecked: boolean;
  isreceivedpreviosly: boolean;
  availableqty: any;
  onhold: boolean;
  onholdremarks: string;
  qcstatus: string;
  receiveremarks: string;
  unholdedby: string;
  unholdedon: Date;
  unholdremarks: string;
  isdirecttransferred: boolean;
  projectcode: string;
  mrnby: string;
  mrnon: Date;
  mrnremarks: string;
  notifyremarks: string;
  notifiedby: string;
  notifiedtofinance: boolean;
  notifiedon: Date
  putawayfilename: string
  vendorname: string;
  showtrdata: boolean = false;
  selectedrow: boolean = false;
  uploadedFiles: any[] = [];

}


export class StockModel {
  itemid: number;
  inwmasterid: number;
  paitemid: number;
  pono: string;
  grnnumber: string;
  binid: number;
  rackid: number;
  storeid: number;
  vendorid: number;
  returnqty: any;
  returnid: number;
  totalquantity: string;
  shelflife: Date;
  availableqty: number;
  stocktype: any;
  qty: any;
  itemreceivedfrom: Date;
  itemlocation: string;
  createddate: Date;
  createdby: string;
  confirmqty: number;
  material: string;
  binnumber: string;
  racknumber: string;
  locatorid: number;
  remarks: string;
  inwardid: number;
  locatorname: string;
  locationlists: any[] = [];
  binlist: any[] = [];
  racklist: any[] = [];
}

export class locataionDetailsStock {
  rackid: number;
  binid: number;
  storeid: number;
  locationid: string;
  locationname: string;
  rackname: string;
  binname: string;
  storename: string;
}

export class materialRequestDetails {
  ItemId: string;
  quantity: string;
  projectcode: string;
  pono: string = "";
}

export class materialReservetorequestModel {
  reserveid: number;
  requestedby: string;
}

export class requestData {
  pono: string;
  suppliername: string;
  material: string;
}

export class daylist {
  description: string;
  showdate: Date;
  showday: string = "";
}

export class locationdetails {
  issuedqty: number;
  location: string;
  issueddate: Date;
  materialid: any;
}

export class gatepassModel {
  issuedqty: number;
  issuedquantity: number;
  gatepassid: number;
  gatepasstype: string;
  status: string;
  remarks: string;
  referenceno: string;
  vehicleno: string;
  requestedby: string;
  statusremarks: string;
  createddate: Date
  gatepassmaterialid: number;
  materialid: string;
  quantity: number
  name: string;
  vendorname: string;
  deleteflag: boolean;
  materialList: Array<materialistModel> = [];
  approverremarks: string;
  approverstatus: string;
  fmapprovedstatus: string;
  printedon: Date;
  printedby: string;
  print: boolean;
  reasonforgatepass: string;
  returneddate: any;
  employeeno: string;
  approverid: string;
  managername: string;
  approvedby: string;
  categoryid: number;
  materiallistarray: any[] = [];
}
export class materialistModel {
  gatepassmaterialid: string;
  materialid: string;
  materialdescription: string;
  quantity: number = 0;
  remarks: string;
  expecteddate: any;
  returneddate: any;
  materialcost: string;
  //availableqty: number;
  issuedqty: number;
  showdetail: boolean;

  materiallistdata: any[] = [];
}

export class materialList {
  material: string;
  materialdescription: string;
  quantity: number = 0;
  remarks: string;
  availableqty: number;
  issuedqty: number;
  materialcost: number;
  requesterid: any;
  stocktype: string;
  projectcode: string;

}

export class materialListforReserve {
  material: string;
  materialdescription: string;
  quantity: number = 0;
  remarks: string;
  availableqty: number;
  issuedqty: number;
  materialcost: number;
  requesterid: any;
  ReserveUpto: any;
  projectcode: string;
}

export class outwardmaterialistModel {
  gatepassid: number;
  gatepassmaterialid: string;
  materialid: string;
  materialdescription: string;
  quantity: number = 0;
  remarks: string;
  expecteddate: any;
  returneddate: any;
  materialcost: string;
  issuedqty: number;
  outwarddate: Date;
  inwarddate: Date;
  outwarddatestring: string;
  inwarddatestring: string;
  movetype: string;
  movedby: string;
  outwardqty: number;
  inwardqty: number;
  mgapprover: string;
  fmapprover: string;
}

export class categoryValues {
  categoryid: number;
  categoryname: string;
  minpricevalue: string;
  maxpricevalue: string;
  startdate: Date;
  enddate: Date;
  createdby: string;
  updatedby: string;
}

export class FIFOValues {
  materialid: string;
  issueqty: number;
}

export class authUser {
  authid: number;
  employeeid: string;
  roleid: number;
  createdby: string;
}

export class Materials {

  material: string;
  materialdescription: string;
  qualitycheck: boolean

}

export class ddlmodel {

  value: string;
  text: string;
  supplier: string;
  projectmanager: string;
  receiveddate: Date

}

export class notifymodel {

  grnnumber: string;
  notifiedby: string;
  notifyremarks: string;
}

export class MRNsavemodel {
  grnnumber: string;
  projectcode: string;
  directtransferredby: string;
  mrnremarks: string
}
export class pageModel {
  id: number
  pagename: string
  pageurl: string
  roleid: number
  isrootpage: boolean
  rootpageid: number
}

export class locationddl {

  locatorid: any;
  locatorname: any;
}

export class binddl {

  binid: any;
  binnumber: any;

}
export class rackddl {

  rackid: string;
  racknumber: string;

}

export class stocktransfermodel {
  transferid: number;
  itemid: number;
  materialid: string;
  materialdescription: string;
  location: string;
  availableqty: string;
  transferqty: string;
  transferlocation: string;
  transferdate: Date;
  transferby: string;

  previouslocation: string;
  previousqty: number;
  currentlocation: string;
  transferedqty: number;
  transferedon: Date;
  transferedby: string
  remarks: string;
  mlocations: string[] = [];
  itemlocationdata: any[] = []

}

export class invstocktransfermodel {
  transferid: string;
  transferredby: string;
  transferredon: Date;
  transfertype: string;
  sourceplant: string;
  destinationplant: string;
  remarks: string;
  showdetail: boolean = false;
  materialdata: stocktransfermateriakmodel[] = [];
}

export class stocktransfermateriakmodel {
  id: number;
  transferid: string;
  itemid: number;
  materialid: string;
  materialdescription: string;
  sourcelocation: string;
  sourceitemid: number;
  destinationlocation: string;
  destinationitemid: number;
  transferqty: number;
  mlocations: string[] = [];
  itemlocationdata: any[] = []
}

export class updateonhold {
  invoiceno: string;
  remarks: string;
  onhold: boolean;
}

export class UserDashboardDetail {
  inbountfortoday: number;
  pendingtooutward: number;
  pendingtoinward: number;
  pendingtoPMapproval: number;
  pendingtoFMapproval: number;
  pendingtoreceive: number;
  pendingtoqualitycheck: number;
  pendingtoaccetance: number;
  pendingtoputaway: number;
  pendingtoissue: number;
  pendingshipments: number;
  receivedshipments: number;
  reservedquantityforthisweek: number;
  pendingtoapproval: number;
  pendingcyclecountapproval: number;

}
export class returnmaterial {
  materialLists: Array<materialistModeltransfer> = [];
  materialList: Array<materialistModelreturn> = [];
}
export class materialistModelreturn {
  // gatepassmaterialid: string;
  material: string;
  materialdescription: string;
  returnqty: number = 0;
  remarks: string;
  returnid: number;
  createdby: string;
}
export class materialistModeltransfer {
  // gatepassmaterialid: string;
  material: string;
  materialdescription: string;
  transferqty: number = 0;
  remarks: string;
  transfetid: number;
  projectcode: string
  createdby: string;
}

export class materialtransferMain {
  transferid: number;
  projectcode: string;
  projectmanagerto: string;
  projectcodefrom: string;
  projectmanagerfrom: string;
  transferredqty: number = 0;
  transferremarks: string;
  approvalremarks: string;
  approverid: string;
  transferedby: string;
  requesteremail: string;
  transferredon: Date;
  showtr: boolean;
  approvallevel: number;
  finalapprovallevel: number;
  status: string;
  materialdata: materialtransferTR[] = [];
  approverdata: materialtransferapproverModel[] = [];
  applevel: number;
  isapproved: boolean;
}

export class materialtransferTR {
  transferid: number;
  materialid: string;
  materialdescription: string;
  transferredqty: number = 0;
}

export class materialtransferapproverModel {
  approverid: string;
  approvername: string;
  status: string;
  approvedon: Date;
  remarks: string
}


export class DirectTransferMain {
  inwmasterid: number;
  projectcode: string;
  grnnumber: string;
  mrnremarks: string;
  mrnby: string;
  mrnon: Date;
  showtr: boolean = false;
  materialdata: DirectTransferTR[] = [];
}
export class DirectTransferTR {
  inwmasterid: number;
  materialid: string;
  materialdescription: string;
  confirmqty: number = 0;
}

export class UserModel {
  employeeno: string;
  deptId: number;
  FirstName: string;
  LastName: string
  Username: string;
  Password: string;
  Token: string;
  name: string;
  pwd: string;
  domainid: string;
  email: string
}

export class outwardinwardreportModel {
  gatepassid: number;
  gatepassmaterialid: number;
  materialid: string;
  materialdescription: string;
  outwarddate: Date;
  outwardby: string;
	outwardremarks: string;
	outwardqty: number;
  inwarddate: Date;
  inwardremarks: string;
	inwardqty: number;
  securityinwarddate: Date;
	securityinwardby: string;
  securityinwardremarks: string;
  showtr: boolean = false;
  materialdata: outwardinwardreportModel[] = [];
}

export class testcrud {
  id: number;
  name: string;
  ismanager: boolean
}

export class PrintHistoryModel {
  reprinthistoryid: number;
  gatepassid: string
  inwmasterid: string;
  reprintedon: Date
  reprintedby: string;
  reprintcount: number;
  barcodeid: string;
  po_invoice: string;
  pono: string;
  invoiceNo: string;
}

export class materilaTrasFilterParams {
  FromDate: string;
  ToDate: string;
}
//Amulya
export class materialrequestMain {
  requestid: number;
  requestedby: string;
  requesteddate: Date;
  ackstatus: string;
  showtr: boolean;
  materialdata: materialtransferTR[] = [];
}
export class materialRequestFilterParams {
  FromDate: string;
  ToDate: string;
}
//Amulya
export class materialResFilterParams {
  FromDate: string;
  ToDate: string;
}
export class materialreserveMain {
  reserveid: number;
  reservedby: string;
  reservedon: Date;
  requestedby: string;
  status: string;
  showtr: boolean;
  materialdata: materialtransferTR[] = [];
}

//Amulya
export class materialRetFilterParams {
  FromDate: string;
  ToDate: string;
}
export class materialreturnMain {
  matreturnid: number;
  returnid: number;
  createdon: Date;
  createdby: string;
  confirmstatus: string;
  showtr: boolean;
  materialdata: materialtransferTR[] = [];
}

