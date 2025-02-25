export interface AccessToken {
    raw: string;
    expires: string;
    expiresDate: Date;
    siteName: string;
    underPath: string;
    permissions: string;
}
