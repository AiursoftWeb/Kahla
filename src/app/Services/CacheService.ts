import { Injectable } from '@angular/core';
import { CacheModel } from '../Models/CacheModel';
import { FriendsApiService } from './Api/FriendsApiService';
import { map } from 'rxjs/operators';
import { AES, enc } from 'crypto-js';
import { DevicesApiService } from './Api/DevicesApiService';
import { ConversationApiService } from './Api/ConversationApiService';
import { ProbeService } from './ProbeService';
import { PushSubscriptionSetting } from '../Models/PushSubscriptionSetting';
import { ThemeService } from './ThemeService';
import { ServerConfig } from '../Models/ServerConfig';

@Injectable()
export class CacheService {
    public cachedData: CacheModel;
    public totalUnread = 0;
    public totalRequests = 0;
    public updatingConversation = false;
    public serverConfig: ServerConfig;

    constructor(
        private friendsApiService: FriendsApiService,
        private devicesApiService: DevicesApiService,
        private conversationApiService: ConversationApiService,
        private probeService: ProbeService,
        private themeService: ThemeService,
    ) {
    }

    public reset() {
        this.cachedData = new CacheModel();
    }

    public updateConversation(): void {
        this.updatingConversation = true;
        this.conversationApiService.All()
            .pipe(map(t => t.items))
            .subscribe(info => {
                this.updatingConversation = false;
                info.forEach(e => {
                    if (e.latestMessage != null) {
                        try {
                            e.latestMessage.content = AES.decrypt(e.latestMessage.content, e.aesKey).toString(enc.Utf8);
                        } catch (error) {
                            e.latestMessage.content = '';
                        }
                        e.latestMessage.content = this.modifyMessage(e.latestMessage.content);
                    }
                    e.avatarURL = this.probeService.encodeProbeFileUrl(e.displayImagePath);
                });
                this.cachedData.conversations = info;
                this.updateTotalUnread();
                this.saveCache();
            });
    }

    public updateFriends(): void {
        this.friendsApiService.Mine()
            .subscribe(result => {
                if (result.code === 0) {
                    result.users.forEach(user => {
                        user.avatarURL = this.probeService.encodeProbeFileUrl(user.iconFilePath);
                    });
                    result.groups.forEach(group => {
                        group.avatarURL = this.probeService.encodeProbeFileUrl(group.imagePath);
                    });

                    this.cachedData.friends = result;
                    this.saveCache();
                }
            });
    }

    public updateRequests(): void {
        this.friendsApiService.MyRequests().subscribe(response => {
            this.cachedData.requests = response.items;
            response.items.forEach(item => {
                item.creator.avatarURL = this.probeService.encodeProbeFileUrl(item.creator.iconFilePath);
            });
            this.totalRequests = response.items.filter(t => !t.completed).length;
            this.saveCache();
        });
    }

    public updateDevice(): void {
        this.devicesApiService.MyDevices().subscribe(response => {
            let currentId = 0;
            if (localStorage.getItem('setting-pushSubscription')) {
                currentId = (<PushSubscriptionSetting>JSON.parse(localStorage.getItem('setting-pushSubscription'))).deviceId;
            }
            response.items.forEach(item => {
                if (item.name !== null && item.name.length >= 0) {
                    const deviceName = [];
                    // OS
                    if (item.name.includes('Win')) {
                        deviceName.push('Windows');
                    } else if (item.name.includes('Android')) {
                        deviceName.push('Android');
                    } else if (item.name.includes('Linux')) {
                        deviceName.push('Linux');
                    } else if (item.name.includes('iPhone') || item.name.includes('iPad')) {
                        deviceName.push('iOS');
                    } else if (item.name.includes('Mac')) {
                        deviceName.push('macOS');
                    } else {
                        deviceName.push('Unknown OS');
                    }

                    if (item.id === currentId) {
                        deviceName[0] += '(Current device)';
                    }

                    // Browser Name
                    if (item.name.includes('Firefox') && !item.name.includes('Seamonkey')) {
                        deviceName.push('Firefox');
                    } else if (item.name.includes('Seamonkey')) {
                        deviceName.push('Seamonkey');
                    } else if (item.name.includes('Edge')) {
                        deviceName.push('Microsoft Edge');
                    } else if (item.name.includes('Edg')) {
                        deviceName.push('Edge Chromium');
                    } else if (item.name.includes('Chrome') && !item.name.includes('Chromium')) {
                        deviceName.push('Chrome');
                    } else if (item.name.includes('Chromium')) {
                        deviceName.push('Chromium');
                    } else if (item.name.includes('Safari') && (!item.name.includes('Chrome') || !item.name.includes('Chromium'))) {
                        deviceName.push('Safari');
                    } else if (item.name.includes('Opera') || item.name.includes('OPR')) {
                        deviceName.push('Opera');
                    } else if (item.name.match(/MSIE|Trident/)) {
                        deviceName.push('Internet Explorer');
                    } else {
                        deviceName.push('Unknown browser');
                    }

                    item.name = deviceName.join('-');
                }
            });
            this.cachedData.devices = response.items;
            // should check if current device id has already been invalid
            if (localStorage.getItem('setting-pushSubscription')) {
                const val = JSON.parse(localStorage.getItem('setting-pushSubscription')) as PushSubscriptionSetting;
                if (val.deviceId && !this.cachedData.devices.find(t => t.id === val.deviceId)) {
                    // invalid id, remove it
                    val.deviceId = null;
                    localStorage.setItem('setting-pushSubscription', JSON.stringify(val));
                }
            }
            this.saveCache();
        });
    }

    public modifyMessage(content: string, modifyText: boolean = false): string {
        if (content.startsWith('[img]')) {
            return 'Photo';
        } else if (content.startsWith('[video]')) {
            return 'Video';
        } else if (content.startsWith('[file]')) {
            return 'File';
        } else if (content.startsWith('[audio]')) {
            return 'Audio';
        } else if (content.startsWith('[group]')) {
            return 'Group Invitation';
        } else if (content.startsWith('[user]')) {
            return 'Contact card';
        } else if (modifyText) {
            return 'Text';
        }
        return content;
    }

    public updateTotalUnread(): void {
        this.totalUnread = this.cachedData.conversations
            .filter(item => !item.muted).map(item => item.unReadAmount).reduce((a, b) => a + b, 0);
        this.themeService.NotifyIcon = this.totalUnread;
    }

    public initCache(): void {
        if (localStorage.getItem('global-cache')) {
            this.cachedData = <CacheModel>JSON.parse(localStorage.getItem('global-cache'));
            if (this.cachedData.version !== CacheModel.VERSION) {
                this.cachedData = new CacheModel();
                this.saveCache();
            }
        } else {
            this.cachedData = new CacheModel();
        }
    }

    public saveCache(): void {
        localStorage.setItem('global-cache', JSON.stringify(this.cachedData));
    }
}
