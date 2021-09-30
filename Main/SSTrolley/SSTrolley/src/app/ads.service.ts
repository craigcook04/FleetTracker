import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { HttpClient, HttpResponseBase, HttpErrorResponse } from '@angular/common/http';

export class AdInfo {
  name: string;
  url: string;
}

@Injectable({
  providedIn: 'root'
})
export class AdsService {

  constructor(private http: HttpClient) { }

  private adsUrl = 'api/ad';

  getAds(count: number): Observable<AdInfo[]> {
    const url = `${this.adsUrl}/random`;
    return this.http.get<AdInfo[]>(url, { params: { 'count': count.toString() }}).pipe(catchError(this.handleError('getAds')));
  }

  getAdList(auth: string): Observable<AdInfo[]> {
    const url = `${this.adsUrl}/list`;
    return this.http.get<AdInfo[]>(url, { headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${auth}` }}).pipe(catchError(this.handleError('getAdsList')));
  }

  updateAd(ad: AdInfo, auth: string): Observable<HttpResponseBase> {
    const url = `${this.adsUrl}/update`;
    return this.http.put(url, ad, { headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${auth}` }, observe: 'response'}).pipe(catchError((error: HttpErrorResponse) => this.handleErrorPass('uploadAd', error)));
  }

  deleteAd(ad: AdInfo, auth: string): Observable<HttpResponseBase> {
    const url = `${this.adsUrl}/delete`;
    return this.http.post(url, ad, { headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${auth}` }, observe: 'response'}).pipe(catchError((error: HttpErrorResponse) => this.handleErrorPass('uploadAd', error)));
  }

  uploadAd(files: FileList, auth: string): Observable<HttpResponseBase> {
    const url = `${this.adsUrl}/upload`;
    const formData: FormData = new FormData();
    for (let i = 0; i < files.length; i++) {
      formData.append('fileUpload', files[i]);
    }
    return this.http.put(url, formData, { headers: { 'Authorization': `Bearer ${auth}` }, observe: 'response'}).pipe(catchError((error: HttpErrorResponse) => this.handleErrorPass('uploadAd', error)));
  }

  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      // TODO: send the error to remote logging infrastructure
      console.error(`${operation} failed: ${error.message}`); // log to console instead

      // Let the app keep running by returning an empty result.
      return of(result as T);
    };
  }

  private handleErrorPass(operation = 'operation', error: HttpErrorResponse): Observable<HttpErrorResponse> {
    this.handleError(operation);
    return of(error);
  }
}
