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
  
}

export class userAcessNamesModel {
    authid: number;
    employeeid: number;
   roleid: number;
   userid: number;
   accessname: string
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

export class printMaterial {
  materialid: any;
  receiveddate: any;
  grnno: any;
  pono: any;
  invoiceno: any;
  noofprint: any;
  barcodePath: any;
  materialcodePath: any;
  printedby: any;
  isprint: any;
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
  //name: string;
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
  modified_on: Date
  modified_by: string
}
