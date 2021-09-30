import { OverlayRef } from '@angular/cdk/overlay';

export class ServiceOverlayRef {

  constructor(private overlayRef: OverlayRef) { }

  close(): void {
    this.overlayRef.dispose();
  }
}
