import { Component, ViewChild, AfterViewInit, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MapComponent } from '.././map/map.component';
import { StopsComponent } from '.././stops/stops.component';
import { UpdatesComponent } from '.././updates/updates.component';
import { AdminComponent } from '.././admin/admin.component';
import { Observable, interval, zip, of } from 'rxjs';
import { TrolleyService } from '.././trolley.service';
import { TrolleyProcessingService as TPS } from '.././trolley-processing.service';
import { TrolleyStop } from '.././trolley-stop';
import { Trolley } from '.././trolley';
import { GeolocationService } from '.././geolocation.service';
import { Moment } from 'moment';
import * as moment from 'moment';
import { DetailsComponent } from '.././details/details.component';
import { TrolleyStopInfo } from '.././trolley-stop-info';
import { Point } from '.././point';
import { PlaceService } from '.././place.service';
import { ServiceOverlayService } from '../service-overlay.service';
import { ActivatedRoute } from '@angular/router';
import { AdsService, AdInfo } from '../ads.service';
import { ServiceStatusComponent } from '../service-status/service-status.component';

@Component({
  selector: 'app-map-page',
  templateUrl: './map-page.component.html',
  styleUrls: ['./map-page.component.css']
})
export class MapPageComponent implements OnInit, AfterViewInit {
  @ViewChild('map') map: MapComponent;
  @ViewChild('stops') stops: StopsComponent;
  @ViewChild('updates') updates: UpdatesComponent;
  @ViewChild('details') details: DetailsComponent;
  @ViewChild('serviceStatus') serviceStatus: ServiceStatusComponent;

  updateInterval: Observable<number>;
  updateDone = true;
  placeUpdateDone = true;
  position: Point = undefined;
  ads: AdInfo[];

  constructor(private trolleyService: TrolleyService, private placeService: PlaceService, private geolocationService: GeolocationService, private serviceOverlayService: ServiceOverlayService, private activatedRoute: ActivatedRoute, private adsService: AdsService) { }

  trolleyId: number;
  trolleyStops: TrolleyStop[];
  inService = true;
  embed = true;

  ngOnInit() {
    this.activatedRoute.queryParams.subscribe(params => {
      if (params['embed'] === 'true')
        this.embed = true;
      else
        this.embed = false;
    });

    this.adsService.getAds(2).subscribe(ads => {
      console.log(ads);
      this.ads = ads;
    });
  }

  ngAfterViewInit() {
    this.trolleyService.getService(1).subscribe(b => {
      this.updatePosition(true, b);
      if (!b) {
        this.trolleyService.getServiceString(1).subscribe(s => {
          this.serviceStatus.serviceStatus = s;
          if (navigator.userAgent.search(/(?:Edge|MSIE|Trident\/.*; rv:)/) === -1) // Don't run in IE
            this.serviceOverlayService.open(s);
          this.inService = false;
        });
      }
      else {
        this.updateInterval = interval(2000);
        this.updateInterval.subscribe(n => {
          this.updatePosition(false, true);
        });
      }
    });
    this.updatePlaces();
  }
  updatePlaces() {
    if (!this.placeUpdateDone) return;
    this.placeUpdateDone = false;

    this.placeService.getAllPlaces().subscribe(places => {
      this.map.handlePlaces(places);
      this.placeUpdateDone = true;
    });
  }

  updatePosition(updateStops: boolean, updateInfo: boolean) {
    if (!this.updateDone) return;
    this.updateDone = false;

    if (updateStops) {
      this.trolleyService.getIds().subscribe(ids => {
        this.trolleyId = ids[0];
        const trolleyObs = this.trolleyService.getTrolley(ids[0]);
        const stopsObs = this.trolleyService.getStops(ids[0]);
        const stopsInfoObs = updateInfo ? this.trolleyService.getStopsInfo(ids[0]) : of([]);

        zip(trolleyObs, stopsObs, stopsInfoObs, (trolley: Trolley, stops: TrolleyStop[], stopsInfo: TrolleyStopInfo[]) => ({ trolley, stops, stopsInfo })).subscribe((data) => {
          this.trolleyStops = data.stops;
          this.handleNewData(data.trolley, data.stops, data.stopsInfo);
          this.updateDone = true;
        });
      });
    }
    else {
      const trolleyObs = this.trolleyService.getTrolley(this.trolleyId);
      const stopsInfoObs = updateInfo ? this.trolleyService.getStopsInfo(this.trolleyId) : of([]);

      zip(trolleyObs, stopsInfoObs, (trolley: Trolley, stopsInfo: TrolleyStopInfo[]) => ({ trolley, stopsInfo })).subscribe((data) => {
        this.handleNewData(data.trolley, this.trolleyStops, data.stopsInfo);
        this.updateDone = true;
      });
    }
  }

  private handleNewData(trolley: Trolley, stops: TrolleyStop[], stopsInfo: TrolleyStopInfo[]) {
    TPS.assignStopsInfo(stops, stopsInfo);
    this.position = this.geolocationService.getPosition();
    if (this.position) {
      TPS.calculateDistancesFromUser(stops, this.position);
      stops = TPS.sortClosestStops(stops);
    }
    else {
      stops.forEach(s => {
        s.distanceFromUser = undefined;
      });
      stops = TPS.sortClocestStopsToTrolley(stops);
    }
    TPS.calculateStopClock(trolley, stops);

    this.map.handleData(trolley, stops, this.position);
    this.stops.handleData(trolley, stops, this.position);
    if (this.updates)
      this.updates.handleData(trolley, stops, this.position);
    this.details.handleData(trolley, stops, this.position);
  }
}
