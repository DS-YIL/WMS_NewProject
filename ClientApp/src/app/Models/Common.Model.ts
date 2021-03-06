export class Employee {
  employeeno: string;
  deptId: number;
  FirstName: string;
  LastName: string;
  Username: string;
  Password: string;
  Token: string;
  name: string;
  pwd: string;
  domainid: string;
  roleid: string;
  plantid: string;
  isdelegatemember: boolean;
  isFinancemember: boolean;

}

export class userAcessNamesModel {
  authid: number;
  employeeid: number;
  roleid: number;
  userid: number;
  accessname: string;
  isdelegatemember: boolean;
}

export class Login {
  Username: string;
  DomainId: string;
  Password: string;
  roleid: string;
}

export class DynamicSearchResult {
  tableName: string;
  searchCondition: string;
  query: string;
}

export class locationBarcode {
  locatorid: any;
  locatorname: any;
  rackid: any;
  rackname: any;
  binid: any;
  binname: any;
  isracklabel: any;
}

export class POList {
  value: any;
  name: any;
  code: any;
  text: any;
  pOno: any;
  qty: any;
  quotationqty: any;
  status: any;
}

export class printonholdGR {
  materialid: any;
  receiveddate: any;
  invoiceno: any;
  pono: any;
  gateentryid: any;
  noofprint: any;
  materialqrpath: any;
  gateentryidqrpath: any;
  createdby: any;
  printerid: any;
  inwardid: any;

}

export class printMaterial {
  materialid: any;
  receiveddate: any;
  grnno: any;
  pono: any;
  inwardid: any;
  invoiceno: any;
  noofprint: any;
  barcodePath: any;
  materialcodePath: any;
  printedby: any;
  isprint: any;
  noofpieces: any;
  totalboxes: any;
  boxno: any;
  serialno: any;
  itemno: any;
  material: any;
  mscode: any;
  receivedqty: any;
  order: any;
  qty: any;
  sotype: any;
  insprec: any;
  shipto: any;
  matdesc: any;
  saleorder: any;
  saleorderlineitemno: any;
  qtyrec: any;
  projectiddef: any;
  assetsubno: any;
  assetno: any;
  lineitemno: any;
  grbarcode: any;
  linkagebarcode: any;
  costcenter: any;
  plantbarcode: any;
  materialbarcode: any;
  orderbarcode: any;

  plant: any;
  gr: any;
  sp: any;
  loadingdate: any;
  materialdescription: any;
  saleordertype: any;
  linkageno: any;
  customername: any;
  customer: any;

  partno: any;
  codetype: any;
  shippingpoint: any;
  printerid: any;
  inwmasterid: any;
}

export class searchParams {
  tableName: string;
  fieldName: string;
  fieldId: string;
  condition: string;
  fieldAliasName: string;
  updateColumns: string;
}
export class searchList {
  //listName: string;
  code: string;
 // name: string;
}

export class stockCardPrint {
  pono: string;
  projectdef: string;
  jobname: string;
  modelno: string;
  description: string;
  qty: any;
  box: any;
  date: Date;
  checkedby: string;
}

export class rbamaster {
  id: number;
  roleid: number;
  inv_enquiry: boolean;
  inv_reports: boolean;
  gate_entry: boolean;
  gate_entry_barcode: boolean;
  inv_receipt_alert: boolean;
  receive_material: boolean;
  put_away: boolean;
  material_return: boolean;
  material_transfer: boolean;
  gate_pass: boolean;
  gatepass_inout: boolean;
  gatepass_approval: boolean;
  material_issue: boolean;
  material_request: boolean;
  material_reservation: boolean;
  abc_classification: boolean;
  cyclecount_configuration: boolean;
  cycle_counting: boolean;
  cyclecount_approval: boolean;
  admin_access: boolean;
  masterdata_creation: boolean;
  masterdata_updation: boolean;
  masterdata_approval: boolean;
  printbarcodes: boolean;
  quality_check: boolean;
  pmdashboard_view: boolean;
  modified_on: Date;
  modified_by: string;
  min: boolean;
  direct_transfer_view: boolean;
  notify_to_finance: boolean;
  gr_process: boolean;
  material_transfer_approval: boolean;
  asn_view: boolean;
  internal_stock_transfer: boolean;
  miscellanous: boolean;
  materialrequest_approval: boolean;
  assign_pm: boolean;
  annexure_report: boolean;
  initialstock_upload: boolean;
  inventory_management: boolean;
  all_reports: boolean;
}
