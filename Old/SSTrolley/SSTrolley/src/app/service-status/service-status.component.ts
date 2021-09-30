import { Component, OnInit, AfterViewInit } from '@angular/core';

@Component({
  selector: 'app-service-status',
  templateUrl: './service-status.component.html',
  styleUrls: ['./service-status.component.css']
})
export class ServiceStatusComponent implements OnInit {
  constructor() { }

  public serviceStatus = '';

  ngOnInit() {}
}
