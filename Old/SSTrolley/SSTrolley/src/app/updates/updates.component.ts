import { Component, OnInit, ViewChild, Input, AfterViewInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material';
import { MapComponent } from '../map/map.component';
import { StopsComponent } from '../stops/stops.component';
import { AppComponent } from '../app.component';
import { TrolleyStop } from '../trolley-stop';
import { Trolley } from '../trolley';
import { TrolleyProcessingService as TPS} from '../trolley-processing.service';
import Map from 'ol/map';
import { Point } from '../point';


@Component({
  selector: 'app-updates',
  templateUrl: './updates.component.html',
  styleUrls: ['./updates.component.css']
})
export class UpdatesComponent implements OnInit {
  trolley: Trolley;
  nextStop: TrolleyStop;

  constructor() { }

  ngOnInit() {
  }

  handleData(trolley: Trolley, stops: TrolleyStop[], position: Point) {
    this.trolley = trolley;
    let stop = stops[0];
    stops.forEach((s) => { if (s.info && stop.info && s.info.distanceFromTrolley < stop.info.distanceFromTrolley) stop = s; });
    this.nextStop = stop;
  }
}
