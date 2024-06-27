import { Dictionary } from "@/models/baseTypes";
import Communication, { CommunicationType } from "@/models/communication";
import { ExternalConfiguration } from "@/models/configData";
import RequestResult from "@/models/requestResult";
import { TermsOfService } from "@/models/termsOfService";

export interface IConfigService {
    getConfiguration(): Promise<ExternalConfiguration>;
}

export interface IUserProfileService {
    getTermsOfService(): Promise<TermsOfService>;
}

export interface ICommunicationService {
    getActive(
        type: CommunicationType
    ): Promise<RequestResult<Communication | null>>;
}

export interface IHttpDelegate {
    unsetAuthorizationHeader(): void;
    setAuthorizationHeader(accessToken: string): void;
    getWithCors<T>(url: string, headers?: Dictionary<string>): Promise<T>;
    get<T>(url: string, headers?: Dictionary<string>): Promise<T>;
    post<T>(
        url: string,
        payload: unknown,
        headers?: Dictionary<string>
    ): Promise<T>;
    put<T>(
        url: string,
        payload: unknown,
        headers?: Dictionary<string>
    ): Promise<T>;
    patch<T>(
        url: string,
        payload: unknown,
        headers?: Dictionary<string>
    ): Promise<T>;
    delete<T>(
        url: string,
        payload?: unknown,
        headers?: Dictionary<string>
    ): Promise<T>;
}

export interface ILogger {
    warn(message: string): void;
    error(message: string): void;
    info(message: string): void;
    verbose(message: string): void;
    debug(message: string): void;
}
