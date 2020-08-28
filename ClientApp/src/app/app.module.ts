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

import { DashboardComponent } from './WMS/Dashboard.component';
import { POListComponent } from './WMS/POList.component';
import { SecurityHomeComponent } from './WMS/SecurityHome.component';
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
    TestCompComponent

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
    AutoCompleteModule,
    ConfirmDialogModule,
    DropdownModule,
    MatBadgeModule,
    InputNumberModule, RadioButtonModule,
    ToolbarModule,
    SelectButtonModule,
    FileUploadModule,
    TabViewModule,
    DataViewModule,
    PanelModule
    //RouterModule.forRoot([
    //  { path: '', component: LoginComponent, pathMatch: 'full' },
    // ])
  ],
  providers: [MessageService, ConfirmationService, HttpClientModule, DatePipe],
  entryComponents: [ConfirmationDialogComponent],
  bootstrap: [AppComponent]
})
export class AppModule { }
