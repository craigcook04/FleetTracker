import { TestBed, inject } from '@angular/core/testing';

import { TrolleyService } from './trolley.service';

describe('TrolleyService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [TrolleyService]
    });
  });

  it('should be created', inject([TrolleyService], (service: TrolleyService) => {
    expect(service).toBeTruthy();
  }));
});
