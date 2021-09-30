import { Component, OnInit, Input } from '@angular/core';
import { MapComponent } from '../map/map.component';
import { UpdatesComponent } from '../updates/updates.component';
import { MatTableDataSource } from '@angular/material';
import { Trolley } from 'src/app/trolley';
import { TrolleyStop } from 'src/app/trolley-stop';
import { Observable } from 'rxjs';
import { TrolleyService } from '../trolley.service';
import { Point } from '../point';

@Component({
  selector: 'app-stops',
  templateUrl: './stops.component.html',
  styleUrls: ['./stops.component.css']
})
export class StopsComponent implements OnInit {
  @Input() map: MapComponent;

  displayedColumns = ['name', 'time'];
  dataSource: MatTableDataSource<TrolleyDisplay>;
  closestStop: TrolleyStop;

  constructor() { }

  ngOnInit() {

  }

  handleData(trolley: Trolley, stops: TrolleyStop[], position: Point) {
    let i = 0;
    this.dataSource = new MatTableDataSource(stops.map(stop => {
      i++;
      return { name: stop.name, time: stop.info ? stop.info.clockTrolley : 'Unavaliable' };
    }));
    this.closestStop = stops[0];
  }
}

export interface TrolleyDisplay {
  name: string;
  time: string;
}
