import { Point } from './point';

export class Place implements Point {
  id: number;
  longitude: number;
  latitude: number;
  name: string;
  address: string;
  additionalInfo: string;
  icon: string;
  distanceFromUser = -1;
}
