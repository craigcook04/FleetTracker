import { Injectable } from '@angular/core';
import { TrolleyService } from './trolley.service';
import { Observable } from 'rxjs';
import { Trolley } from './trolley';
import { TrolleyStop } from './trolley-stop';
import { Point } from './point';
import Sphere from 'ol/sphere';
import proj from 'ol/proj';
import { Moment } from 'moment';
import * as moment from 'moment';
import { TrolleyStopInfo } from './trolley-stop-info';
import { Place } from './place';

@Injectable({
  providedIn: 'root'
})
export class TrolleyProcessingService {

  constructor() { }

  static earth = new Sphere(6378137);

  static sortClosestStops(stops: TrolleyStop[]): TrolleyStop[] {
    return stops.slice().sort((s1, s2) => s1.distanceFromUser - s2.distanceFromUser);
  }

  static sortClocestStopsToTrolley(stops: TrolleyStop[]): TrolleyStop[] {
    return stops.slice().sort((s1, s2) => (s1.info && s2.info) ? s1.info.distanceFromTrolley - s2.info.distanceFromTrolley : 0);
  }

  static calculateDistance(p1: Point, p2: Point): number {
    return this.earth.haversineDistance([p1.longitude, p1.latitude], [p2.longitude, p2.latitude]);
  }

  static calculateDistancesFromUser(stops: TrolleyStop[], point: Point) {
    stops.forEach(s => {
      s.distanceFromUser = this.calculateDistance(s, point);
    });
  }

  static calculateStopClock(trolley: Trolley, stops: TrolleyStop[]) {
    const stortedStops = stops.slice().sort((a, b) => (a.info && b.info) ? a.info.distanceFromTrolley - b.info.distanceFromTrolley : 0);
    const now = moment();
    let offset = 0;
    stortedStops.forEach(s => {
      if (s.info) {
        const departTime = moment.utc(s.info.departTime).local();
        const calculatedDepartTime = now.clone().add(s.info.timeFromTrolley + offset, 'minutes');
        if (departTime >= calculatedDepartTime) {
          s.info.clockTrolley = departTime.format('LT');
          offset += departTime.diff(calculatedDepartTime, 'minutes', true);
        }
        else {
          s.info.clockTrolley = calculatedDepartTime.format('LT');
        }
      }
    });
  }

  static assignStopsInfo(stops: TrolleyStop[], info: TrolleyStopInfo[]) {
    stops.forEach(stop => {
      info.forEach(i => {
        if (stop.id === i.stopId) {
          stop.info = i;
        }
      });
    });
  }
}
