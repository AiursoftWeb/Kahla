import { Injectable } from '@angular/core';
import { AccessToken } from '../Models/AccessToken';

@Injectable({
    providedIn: 'root',
})
export class ProbeService {
    public encodeProbeFileUrl(filePath: string, accessToken?: string, downloadAddr = false) {
        const encoded = encodeURIComponent(filePath).replace(/%2F/g, '/');
        const index = encoded.indexOf('/');

        return (
            (downloadAddr ? '' : '') // TODO: probe
                .replace('{0}', encoded.substring(0, index)) +
            '/' +
            encoded.substring(index + 1) +
            (accessToken ? `?token=${encodeURIComponent(accessToken)}` : '')
        );
    }

    public renameFile(originalFile: File, prefix: string): File {
        return new File(
            [originalFile],
            `${prefix}${new Date().getTime()}.${originalFile.name.substring(originalFile.name.lastIndexOf('.') + 1)}`,
            { type: originalFile.type, lastModified: originalFile.lastModified }
        );
    }

    public resolveAccessToken(tokenRaw: string): AccessToken {
        const token = JSON.parse(atob(tokenRaw.split('.')[0])) as AccessToken;
        token.raw = tokenRaw;
        token.expiresDate = new Date(token.expires);
        return token;
    }
}
