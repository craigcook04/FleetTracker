import { Injectable } from '@angular/core';
import { Trolley } from './trolley';
import { Observable, of } from 'rxjs';
import { catchError, flatMap } from 'rxjs/operators';
import { TrolleyStop } from './trolley-stop';
import { HttpClient, HttpHeaders, HttpResponse, HttpErrorResponse, HttpResponseBase } from '@angular/common/http';
import { Point } from './point';
import { TrolleyStopInfo } from './trolley-stop-info';

@Injectable({
  providedIn: 'root'
})
export class AdminService {

  constructor(private http: HttpClient) { }

  private adminUrl = 'api/admin';
  private auth: string = undefined;

  login(username: string, password: string): Observable<string> {
    const url = `${this.adminUrl}/login`;
    const obs = this.http.post<string>(url, { username: username, password: password }).pipe(catchError(this.handleError('login')));
    obs.subscribe(this.storeAuth);
    return obs;
  }

  private storeAuth(auth: string) {
    if (auth) {
      this.auth = auth;
      localStorage.setItem('trolleyAuth', auth);
    }
  }

  getAuth(): string {
    if (this.auth) {
      return this.auth;
    }
    else {
      return localStorage.getItem('trolleyAuth');
    }
  }

  getLoggedIn(): Observable<HttpResponseBase> {
    const url = `${this.adminUrl}/check`;
    return this.http.get<undefined>(url, { headers: { 'Authorization': `Bearer ${this.getAuth()}` }, observe: 'response'}).pipe(catchError((error: HttpErrorResponse) => of(error)));
  }

  getLocationHistory(): Observable<Blob> {
    const url = `${this.adminUrl}/trolley_location_data.csv`;
    return this.http.get<Blob>(url, { headers: { 'Authorization': `Bearer ${this.getAuth()}` }, responseType: 'blob' as 'json'}).pipe(catchError(this.handleError('getLocationHistory')));
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
