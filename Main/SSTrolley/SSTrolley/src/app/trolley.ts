import { Point } from './point';

export class Trolley implements Point {
  id: number;
  longitude: number;
  latitude: number;
  lastLongitude: number;
  lastLatitude: number;
  heading: number;
  maxPassengers: number;
  passengerCount: number;
  totalPassengers: number;
  totalDistance: number;
}
