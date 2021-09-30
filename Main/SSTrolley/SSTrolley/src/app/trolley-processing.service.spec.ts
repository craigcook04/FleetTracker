import { TestBed, inject } from '@angular/core/testing';

import { TrolleyProcessingService } from './trolley-processing.service';

describe('TrolleyProcessingService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [TrolleyProcessingService]
    });
  });

  it('should be created', inject([TrolleyProcessingService], (service: TrolleyProcessingService) => {
    expect(service).toBeTruthy();
  }));
});
