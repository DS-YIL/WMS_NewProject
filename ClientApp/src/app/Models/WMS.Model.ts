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
  printedon: Date;
  printedby: string;
  print: boolean;
  reasonforgatepass: string;
  returneddate: any;
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
  employeeid: number;
  roleid: number;
  createdby: string;
}
