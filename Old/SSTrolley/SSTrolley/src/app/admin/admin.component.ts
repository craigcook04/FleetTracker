import { Component, OnInit, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { ServiceOverlayService } from '../service-overlay.service';
import { ServiceOverlayRef } from '../service-overlay-ref';
import { AdminService } from '../admin.service';
import { TrolleyService } from '../trolley.service';
import { MatSlideToggle, MatSlideToggleChange, MatButton } from '@angular/material';
import { Router } from '@angular/router';
import { zip } from 'rxjs';
import { AdInfo, AdsService } from '../ads.service';

enum ServiceStatus {
  In = 'Trolley In Service',
  Out = 'Trolley Out Of Service'
}

@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.css']
})
export class AdminComponent implements OnInit, AfterViewInit {
  @ViewChild('serviceToggle') serviceToggle: MatSlideToggle;
  @ViewChild('saveButton') saveButton: MatButton;
  @ViewChild('infoButton') infoButton: MatButton;
  @ViewChild('serviceText') serviceText: ElementRef;
  @ViewChild('fileInput') fileInput: ElementRef;
  @ViewChild('uploadButton') uploadButton: MatButton;

  constructor(private trolleyService: TrolleyService, private adminService: AdminService, private adsService: AdsService, private router: Router) { }

  serviceStatusString = '';
  disabled = true;
  serviceString = '';
  color = 'warm';
  ads: AdInfo[] = undefined;

  ngOnInit(): void {
    this.adminService.getLoggedIn().subscribe(status => {
      if (status.status === 401) {
        // alert('Error: Not logged in');
        this.router.navigate(['login']);
      }
    });

    this.refreshAds();

    this.trolleyService.getService(1).subscribe(status => {
      this.serviceStatusString = this.translateService(status);
      this.serviceToggle.checked = status;
      this.disabled = false;
      this.color = 'warm';
    });

    this.trolleyService.getServiceString(1).subscribe(s => {
      this.serviceString = s;
    });
  }

  ngAfterViewInit(): void {
    this.serviceToggle.change.subscribe((e: MatSlideToggleChange) => {
      this.setService(e.checked);
    });
  }

  private refreshAds() {
    this.adsService.getAdList(this.adminService.getAuth()).subscribe(ads => {
      this.ads = ads;
    });
  }

  private translateService(status: boolean): ServiceStatus {
    if (status)
      return ServiceStatus.In;
    else
      return ServiceStatus.Out;
  }

  setService(status: boolean) {
    this.serviceStatusString = this.translateService(status);
  }

  getLocationData() {
    this.infoButton.disabled = true;
    this.adminService.getLocationHistory().subscribe(blob => {
      if (window.navigator && window.navigator.msSaveOrOpenBlob) {
        window.navigator.msSaveOrOpenBlob(blob, 'location_data.csv');
      }
      else {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.setAttribute('style', 'display: none');
        document.body.appendChild(a);
        a.href = url;
        a.download = 'location_data.csv';
        a.click();
        window.URL.revokeObjectURL(url);
        a.remove();
      }

      this.infoButton.disabled = false;
    });
  }

  save() {
    this.saveButton.disabled = true;
    const serviceObs = this.trolleyService.setService(1, this.serviceToggle.checked, this.adminService.getAuth());
    const serviceStringObs = this.trolleyService.setServiceString(1, (this.serviceText.nativeElement as HTMLTextAreaElement).value, this.adminService.getAuth());
    zip(serviceObs, serviceStringObs, (service: object, serviceString: object) => ({ service, serviceString })).subscribe(data => {
      if (data.service === undefined || data.serviceString === undefined) {
        alert('Error Saving (Are you logged in and connected to the internet?)');
      }
      this.saveButton.disabled = false;
    });
  }

  uploadAd() {
    this.uploadButton.disabled = true;
    const files = (this.fileInput.nativeElement as HTMLInputElement).files;
    this.adsService.uploadAd(files, this.adminService.getAuth()).subscribe(r => {
      this.uploadButton.disabled = false;
      this.refreshAds();
    });
  }

  deleteAd(ad: AdInfo) {
    const index = this.ads.indexOf(ad);
    if (index > -1) {
      this.ads.splice(index, 1);
    }

    this.adsService.deleteAd(ad, this.adminService.getAuth()).subscribe(r => {});
  }

  updateAd(ad: AdInfo) {
    this.adsService.updateAd(ad, this.adminService.getAuth()).subscribe(r => { });
  }
}
