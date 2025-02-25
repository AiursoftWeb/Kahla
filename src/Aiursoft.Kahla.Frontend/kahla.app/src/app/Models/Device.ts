export interface Device {
    id: number;
    name: string;
    ipAddress: string;
    userID: string;
    addTime: string;

    _os?: string;
    _browser?: string;
}
