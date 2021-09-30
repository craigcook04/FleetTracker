import { Injectable, OnInit } from '@angular/core';
import { Point } from './point';

@Injectable({
  providedIn: 'root'
})
export class GeolocationService {

  constructor() {
    if ('geolocation' in navigator) {
      navigator.geolocation.watchPosition((p) => { this.position = p; }, null, { enableHighAccuracy: true });
    }
  }

  position: Position = null;


  getPosition(): Point {
    // return { latitude: 44.428, longitude: -81.391 };
    if (this.position != null)
      return this.position.coords;
    else
      return null;
  }
}
