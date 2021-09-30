import { Injectable } from '@angular/core';
import { Trolley } from './trolley';
import { Observable, of } from 'rxjs';
import { catchError, flatMap } from 'rxjs/operators';
import { TrolleyStop } from './trolley-stop';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Point } from './point';
import { TrolleyStopInfo } from './trolley-stop-info';

@Injectable({
  providedIn: 'root'
})
export class TrolleyService {

  constructor(private http: HttpClient) { }

  private trolleyUrl = 'api/trolley';
  private routeUrl = 'api/route';

  getTrolley(id: number): Observable<Trolley> {
    const url = `${this.trolleyUrl}/${id}`;
    return this.http.get<Trolley>(url).pipe(catchError(this.handleError('getTrolley')));
  }

  getIds(): Observable<number[]> {
    return this.http.get<number[]>(this.trolleyUrl).pipe(catchError(this.handleError('getTrolleyIds')));
  }

  getAllTrolleys(): Observable<Trolley[]> {
    const url = `${this.trolleyUrl}/all`;
    return this.http.get<Trolley[]>(url).pipe(catchError(this.handleError('getAllTrolleys')));
  }

  getStops(id: number): Observable<TrolleyStop[]> {
    const url = `${this.trolleyUrl}/${id}/stops`;
    return this.http.get<TrolleyStop[]>(url).pipe(catchError(this.handleError('getStops')));
  }

  getStopsInfo(id: number): Observable<TrolleyStopInfo[]> {
    const url = `${this.trolleyUrl}/${id}/stopsinfo`;
    return this.http.get<TrolleyStopInfo[]>(url).pipe(catchError(this.handleError('getStopsInfo')));
  }

  getService(id: number): Observable<boolean> {
    const url = `${this.trolleyUrl}/${id}/service`;
    return this.http.get<boolean>(url).pipe(catchError(this.handleError('getService')));
  }

  setService(id: number, state: boolean, auth: string): Observable<object> {
    const url = `${this.trolleyUrl}/${id}/service`;
    return this.http.put(url, { value: state }, { headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${auth}` }}).pipe(catchError(this.handleError('getService')));
  }

  getServiceString(id: number): Observable<string> {
    const url = `${this.trolleyUrl}/${id}/servicestring`;
    return this.http.get<string>(url).pipe(catchError(this.handleError('getServiceString')));
  }

  setServiceString(id: number, serviceString: string, auth: string): Observable<object> {
    const url = `${this.trolleyUrl}/${id}/servicestring`;
    return this.http.put(url, { value: serviceString.replace('\n', ' ') }, { headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${auth}` }}).pipe(catchError(this.handleError('setServiceString')));
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
