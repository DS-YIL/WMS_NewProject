import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './Wmscommon/auth.guard';
import { DashboardComponent } from './WMS/Dashboard.component';
import { LoginComponent } from './WMS/Login.component';
import { POListComponent } from './WMS/POList.component';
import { SecurityHomeComponent } from './WMS/SecurityHome.component';
import { SubContractComponent } from './WMS/SubContractinoutward.component';
import { StoreClerkComponent } from './WMS/StoreClerk.component';
import { WarehouseInchargeComponent } from './WMS/WarehouseIncharge.component';
import { MaterialRequestComponent } from './WMS/MaterialRequest.component';
import { MaterialIssueDashBoardComponent } from './WMS/MaterialIssueDashBoard.component';
import { MaterialIssueComponent } from './WMS/MaterialIssue.component';
import { HomeComponent } from './WMS/Home.component';
import { GatePassComponent } from './WMS/Gatepass.component';
import { GatePassApproverComponent } from './WMS/GatepassApproverForm.component';
import { GatePassPrintComponent } from './WMS/GatepassPrint.component';
import { InventoryMovementComponent } from './WMS/InventoryMovement.component';
import { ObsoleteInventoryMovementComponent } from './WMS/ObsoleteInventoryMovement.component';
import { ExcessInventoryMovementComponent } from './WMS/ExcessInventoryMovement.component';
import { ABCAnalysisComponent } from './WMS/ABCAnalysis.component';
import { ABCCategoryComponent } from './WMS/ABCCategory.component';
import { FIFOComponent } from './WMS/FIFO.component';
import { CyclecountComponent } from './WMS/Cyclecount.component';
import { CycleconfigComponent } from './WMS/Cycleconfig.component';
import { AssignRoleComponent } from './WMS/AssignRole.component';
import { POStatusComponent } from './WMS/POStatus.component';
import { InvoiceDetailsComponent } from './WMS/InvoiceDetails.component';
import { MaterialDetailsComponent } from './WMS/MaterialDetails.component';
import { LocationDetailsComponent } from './WMS/LocationDetails.component';
import { MaterialRequestDetailsComponent } from './WMS/MaterialRequestDetails.component';
import { MaterialRequestViewComponent } from './WMS/MaterialRequestView.component';
import { MaterialReserveComponent } from './WMS/MaterialReserve.component';
import { MaterialReserveViewComponent } from './WMS/MaterialReserveView.component';
import { MaterialReleaseDashBoardComponent } from './WMS/MaterialReleaseDashBoard.component';
import { MaterialReleaseComponent } from './WMS/MaterialRelease.component';
import { QualityCheckComponent } from './WMS/QualityCheck.component';
import { BarcodeComponent } from './WMS/Barcode.component';
import { GatePassApprovalList } from './WMS/GatePassApprovalList.component';
import { SafetyStockComponent } from './WMS/SafetyStock.component';
import { BinStatusReportComponent } from './WMS/BinStatusReport.component';
import { StockTransferComponent } from './WMS/StockTransfer.component';
import { GatePassoutwardComponent } from './WMS/Gatepassoutward.component';
import { MaterialReturnDashBoardComponent } from './WMS/MaterialReturnDashBoard.component';
import { StoresReturnNotePrintComponent } from './WMS/StoresReturnNotePrint.component';
import { StockCardPrintComponent } from './WMS/StockCardPrint.component';
import { MailresponseComponent } from './WMS/Mailresponse.component';
import { MaterialTransferComponent } from './WMS/MaterialTransfer.component';
import { MaterialReturnComponent } from './WMS/MaterialReturn.component';
import { HoldGRViewComponent } from './WMS/HoldGRView.component';
import { MRNViewComponent } from './WMS/MRNView.component';
import { ASNViewComponent } from './WMS/ASNView.component';
import { MaterialBarcodeComponent } from './WMS/MaterialBarcode.component';
import { MaterialReportComponent } from './WMS/MaterialReport.component';
import { PutawayNotificationComponent } from './WMS/PutawayNotification.component';
import { GatepassinwardViewComponent } from './WMS/GatepassinwardView.component';
import { TestCompComponent } from './WMS/TestComp.component';
import { PutawayNotificationViewComponent } from './WMS/PutawayNotificationView.component';
import { DirectTransferComponent } from './WMS/DirectTransfer.component';
import { MaterialRequestDashboardComponent } from './WMS/MaterialRequestDashboard.component';
import { MaterialReserveDashboardComponent } from './WMS/MaterialReserveDashboard.component';
import { MaterialsReturnDashboardComponent } from './WMS/MaterialsReturnDashboard.component';
import { MaterialTransferDashboardComponent } from './WMS/MaterialTransferDashboard.component';
import { OutwardinwardreportComponent } from './WMS/Outwardinwardreport.component';
import { GatepassinwardreceiveComponent } from './WMS/Gatepassinwardreceive.component';
import { MaterialTransferApprovalComponent } from './WMS/MaterialTransferApproval.component';
import { PMDashboardComponent } from './WMS/PMDashboard.component';
import { AdminStockUploadComponent } from './WMS/AdminStockUpload.component';
import { AdminStockUploadReportComponent } from './WMS/AdminStockUploadReport.component';
import { InitialStockLoadComponent } from './WMS/InitialStockLoad.component';
import { InhandMaterialComponent } from './WMS/InhandMaterial.component';
import { GRReportsComponent } from './WMS/GRReports.component';
import { MiscellanousIssueComponent } from './WMS/MiscellanousIssues.component';
import { MiscellanousReceiptsComponent } from './WMS/MiscellanousReceipts.component';
import { ReceiveMaterialComponent } from './WMS/ReceiveMaterial.component';
import { STOApprovalComponent } from './WMS/STOApproval.component';
import { SubcontractApprovalComponent } from './WMS/SubcontractApproval.component';
import { POReportComponent } from './WMS/POReport.component';
import { AnnexureReportComponent } from './WMS/AnnexureReport.component';

//admin
import { MaterilMasterComponent } from './WMS/Admin/Materialmaster.component';
import { GatePassMasterComponent } from './WMS/Admin/GatePassMaster .component';
import { PlantMasterComponent } from './WMS/Admin/PlantMaster.component';
import { RoleMasterComponent } from './WMS/Admin/RoleMaster.component';
import { SubRoleMasterComponent } from './WMS/Admin/SubRoleMaster.component';
import { UserRoleMasterComponent } from './WMS/Admin/UserRoleMaster.component';
import { VendorMasterComponent } from './WMS/Admin/VendorMaster.component';
import { ReceiveSTORequestComponent } from './WMS/ReceiveSTORequest.component';
import { ReceiveSubContractRequestComponent } from './WMS/ReceiveSubContractRequest.component';
import { InitialStockPutAwayComponent } from './WMS/InitialStockPutAway.component';
import { MiscellanousComponent } from './WMS/Admin/Miscellanous.component';
import { StockTransferOrderComponent } from './WMS/StockTransferOrder.component';
import { SubContractTransferOrderComponent } from './WMS/SubContractTransferOrder.component';
import { AssignProjectComponent } from './WMS/AssignProject.component';
import { AssignProjectManagerComponent } from './WMS/AssignProjectManager.component';
import { MaterialRequestApprovalComponent } from './WMS/MaterialRequestApproval.component';
import { AssignRBAComponent } from './WMS/AssignRBA.component';
import { StoreMasterComponent } from './WMS/StoreMaster.component';
import { RackMasterComponent } from './WMS/RackMaster.component';
import { BinMasterComponent } from './WMS/BinMaster.component';
import { roleMaster, VendorMaster } from './Models/WMS.Model';
import { AssignPMComponent } from './WMS/AssignPM.component';
import { AssignIMComponent } from './WMS/AssignInventoryManage.component';

const routes: Routes = [{
  path: 'WMS',
  children: [
    {
      path: '',
      redirectTo: 'Login',
      pathMatch: 'full',
    },
    {
      path: 'Login',
      component: LoginComponent,
    },
    { path: 'Home', component: HomeComponent },
    { path: "Dashboard", component: DashboardComponent},
    { path: "Home", component: HomeComponent },
    { path: "POList", component: POListComponent },
    { path: "SecurityCheck", component: SecurityHomeComponent},
    { path: "SubContractinginout", component: SubContractComponent},
    { path: "GRNPosting", component: StoreClerkComponent },
    { path: "WarehouseIncharge", component: WarehouseInchargeComponent},
    { path: "MaterialRequest", component: MaterialRequestComponent},
    { path: 'MaterialRequest/:pono', component: MaterialRequestComponent },
    { path: "MaterialIssueDashboard", component: MaterialIssueDashBoardComponent},
    { path: "MaterialIssue/:requestid/:pono", component: MaterialIssueComponent},
    { path: "GatePass", component: GatePassComponent },
    { path: "GatePassApprover/:gatepassid", component: GatePassApproverComponent },
    { path: "GatePassPrint/:gatepassid", component: GatePassPrintComponent},
    { path: "InventoryMovement", component: InventoryMovementComponent},
    { path: "ObsoleteInventoryMovement", component: ObsoleteInventoryMovementComponent},
    { path: "ExcessInventoryMovement", component: ExcessInventoryMovementComponent},
    { path: "ABCCategory", component: ABCCategoryComponent},
    { path: "ABCAnalysis", component: ABCAnalysisComponent},
    { path: "ABCAnalysis/:material", component: ABCAnalysisComponent},
    { path: "FIFOList", component: FIFOComponent},
    { path: "Cyclecount", component: CyclecountComponent},
    { path: "Cycleconfig", component: CycleconfigComponent },
    { path: "AssignRole", component: AssignRoleComponent},
    { path: "POStatus", component: POStatusComponent},
    { path: "InvoiceDetails", component: InvoiceDetailsComponent},
    { path: "MaterialDetails", component: MaterialDetailsComponent },
    { path: "LocationDetails", component: LocationDetailsComponent},
    { path: "MaterialReqDetails", component: MaterialRequestDetailsComponent},
    { path: "MaterialReqView", component: MaterialRequestViewComponent},
    { path: "MaterialReqView/:pono", component: MaterialRequestViewComponent},
    { path: "MaterialReserve/:pono", component: MaterialReserveComponent},
    { path: "MaterialReserveView", component: MaterialReserveViewComponent},
    { path: "MaterialReleaseDashboard", component: MaterialReleaseDashBoardComponent},
    { path: "MaterialRelease/:reserveid", component: MaterialReleaseComponent},
    { path: "QualityCheck", component: QualityCheckComponent},
    { path: "Barcode", component: BarcodeComponent},
    { path: "GatePassPMList", component: GatePassApprovalList},
    { path: "GatePassFMList", component: GatePassApprovalList},
    { path: "SafetyStockList", component: SafetyStockComponent},
    { path: "BinStatusReport", component: BinStatusReportComponent},
    { path: "Stocktransfer", component: StockTransferComponent },
    { path: "GatePassinout/:pageid", component: GatePassoutwardComponent },
    { path: "MaterialReturn", component: MaterialReturnDashBoardComponent },
    { path: "StoresReturnNote", component: StoresReturnNotePrintComponent },
    { path: "StockCardPrint", component: StockCardPrintComponent },
    { path: "Mailresponse", component: MailresponseComponent },
    { path: "MaterialTransfer", component: MaterialTransferComponent },
    { path: "MaterialReturnfromPm", component: MaterialReturnComponent },
    { path: "HoldGRView", component: HoldGRViewComponent },
    { path: "MRNView", component: MRNViewComponent },
    { path: "ASNView", component: ASNViewComponent },
    { path: "PrintBarcode", component: MaterialBarcodeComponent },
    { path: "MaterialReport", component: MaterialReportComponent },
    { path: "Putawaynotify", component: PutawayNotificationComponent },
    { path: "Gatepassinward", component: GatepassinwardViewComponent },
    { path: "Test", component: TestCompComponent },
    { path: "GRNotification", component: PutawayNotificationViewComponent },
    { path: "Directtransfer", component: DirectTransferComponent },
    { path: "MaterialRequestDashboard", component: MaterialRequestDashboardComponent },
    { path: "MaterialReserveDashboard", component: MaterialReserveDashboardComponent },
    { path: "MaterialsReturnDashboard", component: MaterialsReturnDashboardComponent },

    { path: "MaterialTransferDashboard", component: MaterialTransferDashboardComponent },
    { path: "outinDashboard", component: OutwardinwardreportComponent },
    { path: "gatepassreceive", component: GatepassinwardreceiveComponent },
    { path: "materialtransferapproval", component: MaterialTransferApprovalComponent },
    { path: "PMDashboard", component: PMDashboardComponent },
    { path: "InitialStock", component: AdminStockUploadComponent },
    { path: "InitialStockReport", component: AdminStockUploadReportComponent },
    { path: "InitialStockLoad", component: InitialStockLoadComponent },
    { path: "inventoryreport", component: InhandMaterialComponent },
    { path: "GRReports", component: GRReportsComponent },

    { path: "MiscellanousIssues", component: MiscellanousIssueComponent },
    { path: "MiscellanousReceipts", component: MiscellanousReceiptsComponent },
    { path: "MaterialMaster", component: MaterilMasterComponent },
    { path: "GatePassMaster", component: GatePassMasterComponent },
    { path: "ReceiveSTORequest", component: ReceiveSTORequestComponent },
    { path: "ReceiveSubContractRequest", component: ReceiveSubContractRequestComponent },  
    { path: "ReceiveMaterial", component: ReceiveMaterialComponent },
    { path: "PlantMaster", component: PlantMasterComponent },
    { path: "RoleMaster", component: RoleMasterComponent },
    { path: "SubRoleMaster", component: SubRoleMasterComponent },
    { path: "UserRoleMaster", component: UserRoleMasterComponent },
    { path: "VendorMaster", component: VendorMasterComponent },
    { path: "InitialStockPutAway", component: InitialStockPutAwayComponent },
    { path: "MiscellanousReason", component: MiscellanousComponent },
    { path: "StockTransferOrder", component: StockTransferOrderComponent },
    { path: "SubContractTransfer", component: SubContractTransferOrderComponent },
    { path: "InitialStockPutAway", component: InitialStockPutAwayComponent },
    { path: "AssignProject", component: AssignProjectComponent },
    { path: "MaterialRequestApproval", component: MaterialRequestApprovalComponent },
    { path: "STOApproval", component: STOApprovalComponent },
    { path: "SubcontractApproval", component: SubcontractApprovalComponent },
    { path: "AssignProjectManager", component: AssignProjectManagerComponent },
    { path: "POReport", component: POReportComponent },
    { path: "AnnexureReport", component: AnnexureReportComponent },
    { path: "Assignrba", component: AssignRBAComponent },
    { path: "StoreMaster", component: StoreMasterComponent },
    { path: "RackMaster", component: RackMasterComponent },
    { path: "BinMaster", component: BinMasterComponent },
    { path: "AssignPM", component: AssignPMComponent },
    { path: "AssignInventoryManager", component: AssignIMComponent },
    


  ]

},
{
  path: '', redirectTo: 'WMS', pathMatch: 'full'
},
{ path: '**', redirectTo: 'WMS' }
];

@NgModule({
  imports: [
    RouterModule.forRoot(routes)
  ],
  exports: [
    RouterModule
  ],
  declarations: []
})
export class AppRoutingModule { }
