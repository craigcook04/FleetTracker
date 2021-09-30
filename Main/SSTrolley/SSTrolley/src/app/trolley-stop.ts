import { Point } from './point';
import { TrolleyStopInfo } from './trolley-stop-info';

export class TrolleyStop implements Point {
  id: number;
  name: string;
  longitude: number;
  latitude: number;
  routeId: number;
  stopNumber: number;
  address: string;
  additionalInfo: string;
  distanceFromUser: number;
  info: TrolleyStopInfo;
}
