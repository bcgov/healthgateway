export default interface BannerError {
    title: string;
    traceId?: string;
    source: string;
}

export interface CustomBannerError {
    title: string;
    description: string;
    detail?: string;
}
