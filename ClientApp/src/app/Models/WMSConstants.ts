import { Injectable } from '@angular/core';
import { searchParams } from './Common.Model';


@Injectable({
  providedIn: 'root'
})
export class constants {
  public url = 'http://localhost:44318/';
  //public url = 'http://10.29.15.183:100/';


  public dateFormat = "dd/MM/yyyy";
  public venderid: searchParams = { tableName: 'wms.VendorMaster', fieldId: 'vendorid', fieldName: 'vendorname', condition: " where  deleteflag=true and ", fieldAliasName: "", updateColumns: "Emailid" };
  public itemlocation: searchParams = { tableName: 'wms.wms_rd_locator', fieldId: 'locatorid', fieldName: 'locatorname', condition: " where ", fieldAliasName: "", updateColumns: "" };
  public rackid: searchParams = { tableName: 'wms.wms_rd_rack', fieldId: 'rackid', fieldName: 'racknumber', condition: "  where ", fieldAliasName: "", updateColumns: "" };
  public binid: searchParams = { tableName: 'wms.wms_rd_bin', fieldId: 'binid', fieldName: 'binnumber', condition: "  where ", fieldAliasName: "", updateColumns: "" };
  public ItemId: searchParams = { tableName: 'wms.wms_stock', fieldId: 'materialid', fieldName: 'materialdescription', condition: " where ", fieldAliasName: "", updateColumns: "" };
  public projectcode: searchParams = { tableName: 'wms.MPRRevisions', fieldId: 'JobName', fieldName: '"JobName"', condition: " where ", fieldAliasName: "", updateColumns: "" };
}
