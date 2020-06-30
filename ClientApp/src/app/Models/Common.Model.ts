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

export class searchParams {
  tableName: string;
  fieldName: string;
  fieldId: string;
  condition: string;
  fieldAliasName: string;
  updateColumns: string;
}
export class searchList {
  listName: string;
  code: string;
  name: string;
}
