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
  ProjectName: string;
  projectcode: string;
  putawayQty: string;
  returnQty: string;
  location: string;
  rackNo: string;
  material: string;
  materialdescription: string;
  grnnumber: string;
  itemid: number;
  confirmqty: number;
  materialqty: string;
  inwardid: number;
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



export class inwardModel {
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
}


export class StockModel {
  itemid: number;
  inwmasterid: number;
  paitemid: number;
  pono: string;
  grnnumber: string;
  binid: number;
  rackid: number;
  vendorid: number;
  totalquantity: string;
  shelflife: Date;
  availableqty: number;
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
}

export class materialRequestDetails {
  ItemId: string;
  quantity: string;
  projectcode: string;
  pono: string = "";
}

export class daylist {
  description: string;
  showdate: Date;
  showday: string = "";
}

export class gatepassModel {
  issuedquantity: number;
  gatepassid: number;
  gatepasstype: string;
  status: string;
  referenceno: string;
  vehicleno: string;
  requestedby: string;
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
  issuedqty: number;
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

}

export class ddlmodel {

  value: string;
  text: string;

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
  itemlocationdata : any[] = []

}

export class invstocktransfermodel {
  transferid: string;
  transferredby: string;
  transferredon: Date;
  transfertype: string;
  sourceplant: string;
  destinationplant: string;
  remarks: string;
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
