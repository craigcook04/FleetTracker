import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { Place } from './place';
import { catchError } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class PlaceService {

  constructor(private http: HttpClient) { }

  private placeUrl = 'api/place';

  getPlace(id: number): Observable<Place> {
    const url = `${this.placeUrl}/${id}`;
    return this.http.get<Place>(url).pipe(catchError(this.handleError('getPlace')));
  }

  getIds(): Observable<number[]> {
    return this.http.get<number[]>(this.placeUrl).pipe(catchError(this.handleError('getPlaceIds')));
  }

  getAllPlaces(): Observable<Place[]> {
    const url = `${this.placeUrl}/all`;
    return this.http.get<Place[]>(url).pipe(catchError(this.handleError('getAllPlaces')));
  }

  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      // TODO: send the error to remote logging infrastructure
      console.error(`${operation} failed: ${error.message}`); // log to console instead

      // Let the app keep running by returning an empty result.
      return of(result as T);
    };
  }
}
