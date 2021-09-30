import { TestBed, inject } from '@angular/core/testing';

import { ServiceOverlayService } from './service-overlay.service';

describe('ServiceOverlayService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ServiceOverlayService]
    });
  });

  it('should be created', inject([ServiceOverlayService], (service: ServiceOverlayService) => {
    expect(service).toBeTruthy();
  }));
});
