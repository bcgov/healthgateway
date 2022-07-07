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
    // The action code associated to the request
    actionCode?: ActionType;
    // The message associated to the error request
    resultMessage: string;
}
