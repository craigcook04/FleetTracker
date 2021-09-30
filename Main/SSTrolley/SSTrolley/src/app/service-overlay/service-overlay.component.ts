import { Component, Input } from '@angular/core';
import { ServiceOverlayRef } from '../service-overlay-ref';

@Component({
  selector: 'app-service-overlay',
  templateUrl: './service-overlay.component.html',
  styleUrls: ['./service-overlay.component.css'],
})
export class ServiceOverlayComponent {
  constructor() { }

  dialogRef: ServiceOverlayRef;
  serviceString = '';

  close() {
    if (this.dialogRef)
      this.dialogRef.close();
  }
}

