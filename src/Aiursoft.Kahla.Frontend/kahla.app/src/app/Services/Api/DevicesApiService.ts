import { Injectable } from '@angular/core';
import { ApiService } from './ApiService';
import { AiurCollection } from '../../Models/AiurCollection';
import { AiurProtocol } from '../../Models/AiurProtocol';
import { AiurValue } from '../../Models/AiurValue';
import { Device } from '../../Models/Device';
import { Observable } from 'rxjs/';

@Injectable()
export class DevicesApiService {
    private static serverPath = '/devices';

    constructor(private apiService: ApiService) {}

    public AddDevice(
        userAgent: string,
        PushEndpoint: string,
        PushP256DH: string,
        PushAuth: string
    ): Observable<AiurValue<number>> {
        return this.apiService.Post(DevicesApiService.serverPath + '/add-device', {
            Name: userAgent,
            PushEndpoint: PushEndpoint,
            PushP256DH: PushP256DH,
            PushAuth: PushAuth,
        });
    }

    public UpdateDevice(
        deviceID: number,
        userAgent: string,
        PushEndpoint: string,
        PushP256DH: string,
        PushAuth: string
    ): Observable<AiurValue<number>> {
        return this.apiService.Put(DevicesApiService.serverPath + `/patch-device/${deviceID}`, {
            Name: userAgent,
            PushEndpoint: PushEndpoint,
            PushP256DH: PushP256DH,
            PushAuth: PushAuth,
        });
    }

    public DropDevice(deviceId: number): Observable<AiurProtocol> {
        return this.apiService.Post(DevicesApiService.serverPath + `/drop-device/${deviceId}`, {});
    }

    public MyDevices(): Observable<AiurCollection<Device>> {
        return this.apiService.Get(DevicesApiService.serverPath + '/my-devices');
    }

    public PushTestMessage(): Observable<AiurProtocol> {
        return this.apiService.Post(DevicesApiService.serverPath + '/push-test-message', {});
    }
}
