import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { ServerConfig } from '../../Models/ServerConfig';
import { environment } from '../../../environments/environment';

type ParamDictStrict = Record<
    string,
    string | number | boolean | readonly (string | number | boolean)[]
>;

type ParamDict = Partial<ParamDictStrict>;

@Injectable({
    providedIn: 'root',
})
export class ApiService {
    public readonly STORAGE_SERVER_CONFIG = 'serverConfig';

    private _headers: HttpHeaders = new HttpHeaders({
        'Content-Type': 'application/x-www-form-urlencoded',
    });

    constructor(private http: HttpClient) {}

    public Get<T>(address: string, params?: ParamDict): Observable<T> {
        return this.GetByFullUrl<T>(`${environment.serversProvider}${address}`, true, params);
    }

    public GetByFullUrl<T>(
        address: string,
        withCredentials = true,
        params?: ParamDict
    ): Observable<T> {
        return this.http.get<T>(address, {
            headers: this._headers,
            withCredentials: withCredentials,
            params: this.processParamDict(params ?? {}),
        });
        // .pipe(catchError(this.handleError));
    }

    public processParamDict(params: ParamDict): ParamDictStrict {
        // remove all undefined or null values
        const result = { ...params };
        for (const key in result) {
            if (result[key] == null) {
                delete result[key];
            }
        }

        return result as ParamDictStrict;
    }

    public formBody(data: ParamDict): HttpParams {
        return new HttpParams({ fromObject: this.processParamDict(data) });
    }

    public Post<T>(address: string, data: ParamDict): Observable<T> {
        return this.http.post<T>(`${environment.serversProvider}${address}`, this.formBody(data), {
            headers: this._headers,
            withCredentials: true,
        });
        // .pipe(catchError(this.handleError));
    }

    public Put<T>(address: string, data: ParamDict): Observable<T> {
        return this.http.put<T>(`${environment.serversProvider}${address}`, this.formBody(data), {
            headers: this._headers,
            withCredentials: true,
        });
        // .pipe(catchError(this.handleError));
    }

    public Patch<T>(address: string, data: ParamDict): Observable<T> {
        return this.http.patch<T>(`${environment.serversProvider}${address}`, this.formBody(data), {
            headers: this._headers,
            withCredentials: true,
        });
        // .pipe(catchError(this.handleError));
    }

    // public handleError(error: any): Promise<any> {
    //     return Promise.reject(error);
    // }

    public ServerInfo() {
        return this.Get<ServerConfig>('/');
    }
}
