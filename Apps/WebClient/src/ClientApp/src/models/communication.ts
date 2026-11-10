export enum CommunicationType {
    Banner = 0,
    Email = 1,
    InApp = 2,
    Mobile = 3,
}

export interface Communication {
    id: string;
    text: string;
    subject: string;
    effectiveDateTime: string;
    expiryDateTime: string;
    communicationTypeCode: CommunicationType;
}

export interface RequestResult<T> {
    resourcePayload: T;
    totalResultCount?: number;
    pageIndex?: number;
    pageSize?: number;
    resultStatus: number;
    resultError?: {
        resultMessage?: string;
        errorCode?: string;
        traceId?: string;
    } | null;
}
