import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './Wmscommon/auth.guard';
import { DashboardComponent } from './WMS/Dashboard.component';
import { LoginComponent } from './WMS/Login.component';
import { POListComponent } from './WMS/POList.component';
import { SecurityHomeComponent } from './WMS/SecurityHome.component';
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
    { path: "Dashboard", component: DashboardComponent, canActivate: [AuthGuard] },
    { path: "Home", component: HomeComponent, canActivate: [AuthGuard] },
    { path: "POList", component: POListComponent, canActivate: [AuthGuard] },
    { path: "SecurityCheck", component: SecurityHomeComponent, canActivate: [AuthGuard] },
    { path: "GRNPosting", component: StoreClerkComponent, canActivate: [AuthGuard] },
    { path: "WarehouseIncharge", component: WarehouseInchargeComponent, canActivate: [AuthGuard] },
    { path: "MaterialRequest", component: MaterialRequestComponent, canActivate: [AuthGuard] },
    { path: 'MaterialRequest/:pono', component: MaterialRequestComponent, canActivate: [AuthGuard] },
    { path: "MaterialIssueDashboard", component: MaterialIssueDashBoardComponent, canActivate: [AuthGuard] },
    { path: "MaterialIssue/:requestid", component: MaterialIssueComponent, canActivate: [AuthGuard] },
    { path: "GatePass", component: GatePassComponent, canActivate: [AuthGuard] },
    { path: "GatePassApprover/:gatepassid", component: GatePassApproverComponent, canActivate: [AuthGuard] },
    { path: "GatePassPrint/:gatepassid", component: GatePassPrintComponent, canActivate: [AuthGuard] },
    { path: "InventoryMovement", component: InventoryMovementComponent, canActivate: [AuthGuard] },
    { path: "ObsoleteInventoryMovement", component: ObsoleteInventoryMovementComponent, canActivate: [AuthGuard] },
    { path: "ExcessInventoryMovement", component: ExcessInventoryMovementComponent, canActivate: [AuthGuard] },
    { path: "ABCCategory", component: ABCCategoryComponent, canActivate: [AuthGuard] },
    { path: "ABCAnalysis", component: ABCAnalysisComponent, canActivate: [AuthGuard] },
    { path: "ABCAnalysis/:material", component: ABCAnalysisComponent, canActivate: [AuthGuard] },
    { path: "FIFOList", component: FIFOComponent, canActivate: [AuthGuard] },
    { path: "Cyclecount", component: CyclecountComponent, canActivate: [AuthGuard] },
    { path: "Cycleconfig", component: CycleconfigComponent, canActivate: [AuthGuard] },
    { path: "AssignRole", component: AssignRoleComponent, canActivate: [AuthGuard] },
    { path: "POStatus", component: POStatusComponent, canActivate: [AuthGuard] },
    { path: "InvoiceDetails", component: InvoiceDetailsComponent, canActivate: [AuthGuard] },
    { path: "MaterialDetails", component: MaterialDetailsComponent, canActivate: [AuthGuard] },
    { path: "LocationDetails", component: LocationDetailsComponent, canActivate: [AuthGuard] },
    { path: "MaterialReqDetails", component: MaterialRequestDetailsComponent, canActivate: [AuthGuard] },
    { path: "MaterialReqView", component: MaterialRequestViewComponent, canActivate: [AuthGuard] },
    { path: "MaterialReqView/:pono", component: MaterialRequestViewComponent, canActivate: [AuthGuard] },
    { path: "MaterialReserve/:pono", component: MaterialReserveComponent, canActivate: [AuthGuard] },
    { path: "MaterialReserveView", component: MaterialReserveViewComponent, canActivate: [AuthGuard] },
    { path: "MaterialReleaseDashboard", component: MaterialReleaseDashBoardComponent, canActivate: [AuthGuard] },
    { path: "MaterialRelease/:reserveid", component: MaterialReleaseComponent, canActivate: [AuthGuard] }

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
