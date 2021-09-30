import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { HttpClient, HttpResponseBase, HttpErrorResponse } from '@angular/common/http';

export class AudioFile {
  name: string;
  url: string;
}

@Injectable({
  providedIn: 'root'
})
export class AudioService {

  constructor(private http: HttpClient) { }

  private audioUrl = 'api/audio';

  getAudioList(auth: string):Observable<AudioFile[]>{
    const url = `${this.audioUrl}/list`;
    return this.http.get<AudioFile[]>(url, { headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${auth}` }}).pipe(catchError(this.handleError('getAudioList')));
  }

  deleteAudio(audio: AudioFile, auth: string): Observable<HttpResponseBase> {
    const url = `${this.audioUrl}/delete`;
    return this.http.post(url, audio, { headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${auth}` }, observe: 'response'}).pipe(catchError((error: HttpErrorResponse) => this.handleErrorPass('deleteAudio', error)));
  }

  uploadAudio(files: FileList, auth: string): Observable<HttpResponseBase> {
    const url = `${this.audioUrl}/upload`;
    const formData: FormData = new FormData();
    for (let i = 0; i < files.length; i++) {
      formData.append('fileUpload', files[i]);
    }
    return this.http.put(url, formData, { headers: { 'Authorization': `Bearer ${auth}` }, observe: 'response'}).pipe(catchError((error: HttpErrorResponse) => this.handleErrorPass('uploadAudio', error)));
  }

  private handleErrorPass(operation = 'operation', error: HttpErrorResponse): Observable<HttpErrorResponse> {
    this.handleError(operation);
    return of(error);
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
