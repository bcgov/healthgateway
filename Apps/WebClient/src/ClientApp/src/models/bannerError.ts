export default interface BannerError {
    title: string;
    description: string;
    detail: string;
    errorCode: string;
    source?: string;
    traceId?: string;
    source: string;
}

export interface CustomBannerError {
    title: string;
    description: string;
    detail?: string;
}
