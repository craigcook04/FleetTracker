import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import {
  MatCheckboxModule,
  MatToolbarModule,
  MatTableModule,
  MatCardModule,
  MatIconModule,
  MatExpansionModule,
  MatBottomSheetModule,
  MatListModule,
  MatButtonModule,
  MatFormFieldModule,
  MatInputModule,
  MatButtonToggleModule,
  MatSlideToggleModule
} from '@angular/material';
import { FormsModule, ReactiveFormsModule, NgForm } from '@angular/forms';
import {HttpClientModule} from '@angular/common/http';
import {CdkTableModule} from '@angular/cdk/table';
import {platformBrowserDynamic} from '@angular/platform-browser-dynamic';
import { MapComponent } from './map/map.component';
import {OverlayModule} from '@angular/cdk/overlay';
import { AppComponent } from './app.component';
import { StopsComponent } from './stops/stops.component';
import { UpdatesComponent } from './updates/updates.component';
import { DetailsComponent } from './details/details.component';
import { AdminComponent } from './admin/admin.component';
import { ServiceOverlayComponent } from './service-overlay/service-overlay.component';
import { AdminLoginComponent } from './admin-login/admin-login.component';
import { NotFoundComponent } from './not-found/not-found.component';
import { MapPageComponent } from './map-page/map-page.component';
import { ServiceStatusComponent } from './service-status/service-status.component';

const appRoutes: Routes = [
  { path: 'login', component: AdminLoginComponent },
  { path: 'map', component: MapPageComponent },
  { path: 'admin', component: AdminComponent },
  { path: '', redirectTo: '/map', pathMatch: 'full' },
  { path: '**', component: NotFoundComponent }
];

@NgModule({
  declarations: [
    AppComponent,
    MapComponent,
    StopsComponent,
    UpdatesComponent,
    DetailsComponent,
    AdminComponent,
    DetailsComponent,
    ServiceOverlayComponent,
    AdminLoginComponent,
    NotFoundComponent,
    MapPageComponent,
    ServiceStatusComponent
  ],
  imports: [
    RouterModule.forRoot(
      appRoutes
    ),
    HttpClientModule,
    BrowserModule,
    BrowserAnimationsModule,
    MatToolbarModule,
    MatTableModule,
    MatCardModule,
    MatListModule,
    MatExpansionModule,
    MatIconModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    FormsModule,
    MatCheckboxModule,
    MatButtonToggleModule,
    MatSlideToggleModule,
    OverlayModule
  ],
  exports: [
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatCheckboxModule,
    MatButtonToggleModule,
    MatSlideToggleModule
  ],
  providers: [],
  bootstrap: [AppComponent],
  entryComponents: [
    ServiceOverlayComponent
  ]
})

export class AppModule { }
