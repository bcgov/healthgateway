import { ActionType } from "@/constants/actionType";

export interface BannerError {
    title: string;
    traceId?: string;
    source: string;
}

export interface CustomBannerError {
    title: string;
    description: string;
    detail?: string;
}

export interface HttpError {
    message: string;
    statusCode?: number;
}

export class PageError {
    public code: string;
    public name: string;
    public message: string;

    constructor(code: string, name: string, message: string) {
        this.code = code;
        this.name = name;
        this.message = message;
    }
}

export interface ResultError {
    // The error code associated to the request
    errorCode: string;
    // The trace id associated to the request
    traceId: string;
    // The message associated to the error request
    resultMessage: string;
    // The action code associated to the request
    actionCode?: ActionType;
    // The HTTP status code returned by the request
    statusCode?: number;
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function instanceOfResultError(object: any): object is ResultError {
    return (
        "errorCode" in object &&
        "traceId" in object &&
        "resultMessage" in object
    );
}
