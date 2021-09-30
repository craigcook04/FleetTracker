import { Injectable } from '@angular/core';
import { Overlay, OverlayConfig, OVERLAY_PROVIDERS } from '@angular/cdk/overlay';
import { ServiceOverlayRef } from './service-overlay-ref';
import { ComponentPortal } from '@angular/cdk/portal';
import { ServiceOverlayComponent } from './service-overlay/service-overlay.component';

interface ServiceOverlayConfig {
  panelClass?: string;
  hasBackdrop?: boolean;
  backdropClass?: string;
  flexibleDiemsions?: boolean;
  lockPosition?: any;
  growAfterOpen?: boolean;
}

const DEFAULT_CONFIG: ServiceOverlayConfig = {
  hasBackdrop: true,
  backdropClass: 'dark-backdrop',
  panelClass: 'tm-service-panel',
  flexibleDiemsions: true,
  lockPosition: false,
  growAfterOpen: true,
};

@Injectable({
  providedIn: 'root'
})
export class ServiceOverlayService {

  constructor(private overlay: Overlay) { }

  open(serviceString: string, config: ServiceOverlayConfig = {}): ServiceOverlayRef {
    // Override default configuration
    const dialogConfig = { ...DEFAULT_CONFIG, ...config };

    // Returns an OverlayRef which is a PortalHost
    const overlayRef = this.createOverlay(dialogConfig);

    // Instantiate remote control
    const dialogRef = new ServiceOverlayRef(overlayRef);

    // Create ComponentPortal that can be attached to a PortalHost
    const servicePortal = new ComponentPortal(ServiceOverlayComponent);

    // Attach ComponentPortal to PortalHost
    const compRef = overlayRef.attach(servicePortal);
    compRef.instance.dialogRef = dialogRef;
    compRef.instance.serviceString = serviceString;

    overlayRef.backdropClick().subscribe(_ => dialogRef.close());

    return dialogRef;
  }

  private createOverlay(config: ServiceOverlayConfig) {
    const overlayConfig = this.getOverlayConfig(config);
    return this.overlay.create(overlayConfig);
  }

  private getOverlayConfig(config: ServiceOverlayConfig): OverlayConfig {
    const positionStrategy = this.overlay.position()
      .global()
      .centerHorizontally()
      .centerVertically();

    const overlayConfig = new OverlayConfig({
      hasBackdrop: config.hasBackdrop,
      backdropClass: config.backdropClass,
      panelClass: config.panelClass,
      scrollStrategy: this.overlay.scrollStrategies.block(),
      positionStrategy
    });

    return overlayConfig;
  }

}
