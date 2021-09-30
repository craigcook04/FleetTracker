import { Component, OnInit, ViewChild } from '@angular/core';
import { MapComponent } from '../map/map.component';
import { StopsComponent } from '../stops/stops.component';
import { UpdatesComponent } from '../updates/updates.component';
import { AppComponent } from '../app.component';
import { Observable, interval, zip } from 'rxjs';
import { TrolleyService } from '../trolley.service';
import { TrolleyProcessingService as TPS } from '../trolley-processing.service';
import { TrolleyStop } from '../trolley-stop';
import { Trolley } from '../trolley';
import { GeolocationService } from '../geolocation.service';
import { Moment } from 'moment';
import * as moment from 'moment';
import { Point } from '../point';

@Component({
  selector: 'app-details',
  templateUrl: './details.component.html',
  styleUrls: ['./details.component.css']
})
export class DetailsComponent implements OnInit {
  trolley: Trolley;

  constructor() { }

  ngOnInit() {
  }

  handleData(trolley: Trolley, stops: TrolleyStop[], position: Point) {
    this.trolley = trolley;
  }
}
