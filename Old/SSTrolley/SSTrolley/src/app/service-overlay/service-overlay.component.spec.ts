import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ServiceOverlayComponent } from './service-overlay.component';

describe('FilePreviewOverlayComponent', () => {
  let component: ServiceOverlayComponent;
  let fixture: ComponentFixture<ServiceOverlayComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ServiceOverlayComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ServiceOverlayComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
