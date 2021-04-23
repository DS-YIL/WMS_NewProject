import { BrowserModule } from '@angular/platform-browser';
import { NgModule, Injectable } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import {SidebarModule} from 'primeng/sidebar';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import {ButtonModule} from 'primeng/button';
import {MenuModule} from 'primeng/menu';
import {PanelMenuModule} from 'primeng/panelmenu';
import { OverlayPanelModule } from 'primeng/overlaypanel';
import { DialogModule } from 'primeng/dialog';
import { ToastModule } from 'primeng/toast';
import { NgxSpinnerModule } from "ngx-spinner";
import { MatBadgeModule } from '@angular/material/badge';
import { ChartModule } from 'primeng/chart';

import { ConfirmationDialogComponent } from './WmsCommon/confirmationdialog/confirmation-dialog.component';
import { SelectfilterPipe } from './WmsCommon/selectfilter.pipe';
import { ConfirmationService, MessageService } from 'primeng/api';
import { MatDialogModule, MatButtonModule, MatExpansionModule } from '@angular/material';

import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { ListboxModule } from 'primeng/listbox';
import { CalendarModule } from 'primeng/calendar';
import { CheckboxModule } from 'primeng/checkbox';
import { TooltipModule } from 'primeng/tooltip';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { DropdownModule } from 'primeng/dropdown';
import { InputNumberModule } from 'primeng/inputnumber';
import { NgxPrintModule } from 'ngx-print';
import { VirtualScrollerModule } from 'primeng/virtualscroller';

import { DashboardComponent } from './WMS/Dashboard.component';
import { POListComponent } from './WMS/POList.component';
import { SecurityHomeComponent } from './WMS/SecurityHome.component';
import { SubContractComponent } from './WMS/SubContractinoutward.component';
import { StoreClerkComponent } from './WMS/StoreClerk.component';
import { WarehouseInchargeComponent } from './WMS/WarehouseIncharge.component';
import { MaterialRequestComponent } from './WMS/MaterialRequest.component';
import { MaterialIssueDashBoardComponent } from './WMS/MaterialIssueDashBoard.component';
import { MaterialIssueComponent } from './WMS/MaterialIssue.component';
import { GatePassComponent } from './WMS/Gatepass.component';
import { GatePassApproverComponent } from './WMS/GatepassApproverForm.component';
import { GatePassPrintComponent } from './WMS/GatepassPrint.component';
import { InventoryMovementComponent } from './WMS/InventoryMovement.component';
import { ObsoleteInventoryMovementComponent } from './WMS/ObsoleteInventoryMovement.component';
import { ExcessInventoryMovementComponent } from './WMS/ExcessInventoryMovement.component';
import { ABCAnalysisComponent } from './WMS/ABCAnalysis.component';
import { ABCCategoryComponent } from './WMS/ABCCategory.component';
import { FIFOComponent } from './WMS/FIFO.component';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
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
import { MaterialTransferComponent } from './WMS/MaterialTransfer.component'
import { MaterialReturnComponent } from './WMS/MaterialReturn.component'
import { HoldGRViewComponent } from './WMS/HoldGRView.component'
import { MRNViewComponent } from './WMS/MRNView.component'
import { ASNViewComponent } from './WMS/ASNView.component'
import { MaterialBarcodeComponent } from './WMS/MaterialBarcode.component';
import { MaterialReportComponent } from './WMS/MaterialReport.component';
import { PutawayNotificationComponent } from './WMS/PutawayNotification.component';
import { GatepassinwardViewComponent } from './WMS/GatepassinwardView.component';
import { TestCompComponent } from './WMS/TestComp.component';
import { PutawayNotificationViewComponent } from './WMS/PutawayNotificationView.component';
import { DirectTransferComponent } from './WMS/DirectTransfer.component';
import { MaterialTransferDashboardComponent } from './WMS/MaterialTransferDashboard.component';
import { MaterialRequestDashboardComponent } from './WMS/MaterialRequestDashboard.component';
import { MaterialReserveDashboardComponent } from './WMS/MaterialReserveDashboard.component';
import {MaterialsReturnDashboardComponent } from './WMS/MaterialsReturnDashboard.component';

import { OutwardinwardreportComponent } from './WMS/Outwardinwardreport.component';
import { GatepassinwardreceiveComponent } from './WMS/Gatepassinwardreceive.component';
import { MaterialTransferApprovalComponent } from './WMS/MaterialTransferApproval.component';
import { PMDashboardComponent } from './WMS/PMDashboard.component';
import { AdminStockUploadComponent } from './WMS/AdminStockUpload.component';
import { AdminStockUploadReportComponent } from './WMS/AdminStockUploadReport.component';
import { InitialStockLoadComponent } from './WMS/InitialStockLoad.component';
import { GRReportsComponent } from './WMS/GRReports.component';
import { POReportComponent } from './WMS/POReport.component';
import { AnnexureReportComponent } from './WMS/AnnexureReport.component';

import { InhandMaterialComponent } from './WMS/InhandMaterial.component';
import { MiscellanousIssueComponent } from './WMS/MiscellanousIssues.component';
import { MiscellanousReceiptsComponent } from './WMS/MiscellanousReceipts.component';
import { ReceiveSTORequestComponent } from './WMS/ReceiveSTORequest.component';
import { ReceiveSubContractRequestComponent } from './WMS/ReceiveSubContractRequest.component';
import { ReceiveMaterialComponent } from './WMS/ReceiveMaterial.component';
import { InitialStockPutAwayComponent } from './WMS/InitialStockPutAway.component';
import { StockTransferOrderComponent } from './WMS/StockTransferOrder.component';
import { SubContractTransferOrderComponent } from './WMS/SubContractTransferOrder.component';
import { STOApprovalComponent } from './WMS/STOApproval.component';
import { SubcontractApprovalComponent } from './WMS/SubcontractApproval.component';

//admin
import { MaterilMasterComponent } from './WMS/Admin/Materialmaster.component';
import { GatePassMasterComponent } from './WMS/Admin/GatePassMaster .component';
import { PlantMasterComponent } from './WMS/Admin/PlantMaster.component';
import { MiscellanousComponent } from './WMS/Admin/Miscellanous.component';
import { RoleMasterComponent } from './WMS/Admin/RoleMaster.component';
import { SubRoleMasterComponent } from './WMS/Admin/SubRoleMaster.component';
import { UserRoleMasterComponent } from './WMS/Admin/UserRoleMaster.component';
import { VendorMasterComponent } from './WMS/Admin/VendorMaster.component';
import { AssignProjectComponent } from './WMS/AssignProject.component';
import { AssignProjectManagerComponent } from './WMS/AssignProjectManager.component';
import { MaterialRequestApprovalComponent } from './WMS/MaterialRequestApproval.component';
import { AssignRBAComponent } from './WMS/AssignRBA.component';
import { StoreMasterComponent } from './WMS/StoreMaster.component';
import { RackMasterComponent } from './WMS/RackMaster.component';
import { BinMasterComponent } from './WMS/BinMaster.component';
import { AssignPMComponent } from './WMS/AssignPM.component';
import { AssignIMComponent } from './WMS/AssignInventoryManage.component';

////pages
import { LoginComponent } from './WMS/Login.component';
import {HomeComponent} from './WMS/Home.component';
import { MenubarModule } from 'primeng/menubar';
import { DatePipe } from '@angular/common'
import { RadioButtonModule } from 'primeng/radiobutton';
import { ToolbarModule } from 'primeng/toolbar';
import { SelectButtonModule } from 'primeng/selectbutton';
import { FileUploadModule } from 'primeng/fileupload';
import { TabViewModule } from 'primeng/tabview';
import { DataViewModule } from 'primeng/dataview';
import { PanelModule } from 'primeng/panel';
import { MultiSelectModule } from 'primeng/multiselect';
import { FieldsetModule } from 'primeng/fieldset';
import { KeyFilterModule } from 'primeng/keyfilter';
@Injectable
  ({
    providedIn: 'root',
})

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    LoginComponent,
    ConfirmationDialogComponent,
    SelectfilterPipe,
    DashboardComponent,
    POListComponent,
    SecurityHomeComponent,
    SubContractComponent,
    StoreClerkComponent,
    WarehouseInchargeComponent,
    MaterialRequestComponent,
    MaterialIssueDashBoardComponent,
    MaterialIssueComponent,
    GatePassComponent,
    GatePassApproverComponent,
    GatePassPrintComponent,
    InventoryMovementComponent,
    ObsoleteInventoryMovementComponent,
    ExcessInventoryMovementComponent,
    ABCAnalysisComponent,
    ABCCategoryComponent,
    FIFOComponent,
    CyclecountComponent,
    CycleconfigComponent,
    AssignRoleComponent,
    POStatusComponent,
    InvoiceDetailsComponent,
    MaterialDetailsComponent,
    LocationDetailsComponent,
    MaterialRequestDetailsComponent,
    MaterialRequestViewComponent,
    MaterialReserveComponent,
    MaterialReserveViewComponent,
    MaterialReleaseDashBoardComponent,
    MaterialReleaseComponent,
    QualityCheckComponent,
    BarcodeComponent,
    GatePassApprovalList,
    SafetyStockComponent,
    BinStatusReportComponent,
    StockTransferComponent,
    GatePassoutwardComponent,
    MaterialReturnDashBoardComponent,
    StoresReturnNotePrintComponent,
    StockCardPrintComponent,
    MailresponseComponent,
    MaterialTransferComponent,
    MaterialReturnComponent,
    HoldGRViewComponent,
    MRNViewComponent,
    ASNViewComponent,
    MaterialBarcodeComponent,
    MaterialReportComponent,
    PutawayNotificationComponent,
    GatepassinwardViewComponent,
    TestCompComponent,
    PutawayNotificationViewComponent,
    DirectTransferComponent,
    MaterialTransferDashboardComponent,
    MaterialRequestDashboardComponent,
    MaterialReserveDashboardComponent,
    MaterialsReturnDashboardComponent,
    OutwardinwardreportComponent,
    GatepassinwardreceiveComponent,
    MaterialTransferApprovalComponent,
    PMDashboardComponent,
    AdminStockUploadComponent,
    AdminStockUploadReportComponent,
    InitialStockLoadComponent,
    InhandMaterialComponent,
    GRReportsComponent,
    MiscellanousIssueComponent,
    MiscellanousReceiptsComponent,
    MaterilMasterComponent,
    GatePassMasterComponent,
    ReceiveSTORequestComponent,
    ReceiveMaterialComponent,
    PlantMasterComponent,
    InitialStockPutAwayComponent,
    MiscellanousComponent,
    RoleMasterComponent,
    SubRoleMasterComponent,
    UserRoleMasterComponent,
    VendorMasterComponent,
    StockTransferOrderComponent,
    SubContractTransferOrderComponent,
    ReceiveSubContractRequestComponent,
    InitialStockPutAwayComponent,
    AssignProjectComponent,
    MaterialRequestApprovalComponent,
    STOApprovalComponent,
    SubcontractApprovalComponent,
    AssignProjectManagerComponent,
    POReportComponent,
    AnnexureReportComponent,
    AssignRBAComponent,
    StoreMasterComponent,
    RackMasterComponent,
    BinMasterComponent,
    AssignPMComponent,
    AssignIMComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    AppRoutingModule,
    FormsModule,
    ReactiveFormsModule,
    SidebarModule,
    BrowserAnimationsModule,
    ButtonModule,
    MenuModule,
    PanelMenuModule,
    OverlayPanelModule,
    MenubarModule,
    DialogModule,
    ToastModule,
    NgxSpinnerModule,
    RouterModule,
    MatDialogModule,
    MatButtonModule,
    MatExpansionModule,
    CardModule,
    TableModule,
    ListboxModule,
    CalendarModule,
    CheckboxModule,
    TooltipModule,
    MultiSelectModule,
    AutoCompleteModule,
    ConfirmDialogModule,
    DropdownModule,
    MatBadgeModule,
    InputNumberModule, RadioButtonModule,
    NgxPrintModule,
    ToolbarModule,
    ChartModule,
    SelectButtonModule,
    FileUploadModule,
    TabViewModule,
    DataViewModule,
    PanelModule,
    FieldsetModule,
    VirtualScrollerModule,
    KeyFilterModule
    //RouterModule.forRoot([
    //  { path: '', component: LoginComponent, pathMatch: 'full' },
    // ])
  ],
  providers: [MessageService, ConfirmationService, HttpClientModule, DatePipe],
  entryComponents: [ConfirmationDialogComponent],
  bootstrap: [AppComponent]
})
export class AppModule { }
