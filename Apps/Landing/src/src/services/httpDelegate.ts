import Axios, { AxiosError, AxiosRequestConfig, AxiosResponse } from "axios";

import { Dictionary } from "@/models/baseTypes";
import { HttpError } from "@/models/errors";
import { IHttpDelegate, ILogger } from "@/services/interfaces";

export class HttpDelegate implements IHttpDelegate {
    private logger;

    constructor(logger: ILogger) {
        this.logger = logger;
    }

    public unsetAuthorizationHeader(): void {
        this.logger.debug(`ACCESS TOKEN UNSET`);
        Axios.defaults.headers.common = {};
    }

    public setAuthorizationHeader(accessToken: string): void {
        this.logger.debug(`ACCESS TOKEN SET`);
        Axios.defaults.headers.common = {
            ...Axios.defaults.headers.common,
            Authorization: `Bearer ${accessToken}`,
        };
    }

    public getWithCors<T>(
        url: string,
        headers: Dictionary<string> = {}
    ): Promise<T> {
        headers["Access-Control-Allow-Origin"] = window.location.origin;
        return this.get(url, headers);
    }

    public get<T>(
        url: string,
        headers: Dictionary<string> | undefined = undefined
    ): Promise<T> {
        const config: AxiosRequestConfig = {
            headers,
        };
        return Axios.get(url, config)
            .then((response: AxiosResponse<T>) => response.data)
            .catch((error: Error | AxiosError) => {
                throw this.toHttpError(error, "GET");
            });
    }

    public post<T>(
        url: string,
        payload: unknown,
        headers: Dictionary<string> | undefined = undefined
    ): Promise<T> {
        const config: AxiosRequestConfig = {
            headers,
        };
        return Axios.post(url, payload, config)
            .then((response: AxiosResponse<T>) => response.data)
            .catch((error: Error | AxiosError) => {
                throw this.toHttpError(error, "POST");
            });
    }

    public put<T>(
        url: string,
        payload: unknown,
        headers: Dictionary<string> | undefined = undefined
    ): Promise<T> {
        const config: AxiosRequestConfig = {
            headers,
        };
        return Axios.put(url, payload, config)
            .then((response: AxiosResponse<T>) => response.data)
            .catch((error: Error | AxiosError) => {
                throw this.toHttpError(error, "PUT");
            });
    }

    public patch<T>(
        url: string,
        payload: unknown,
        headers: Dictionary<string> | undefined = undefined
    ): Promise<T> {
        const config: AxiosRequestConfig = {
            headers,
        };
        return Axios.patch(url, payload, config)
            .then((response: AxiosResponse<T>) => response.data)
            .catch((error: Error | AxiosError) => {
                throw this.toHttpError(error, "PATCH");
            });
    }

    public delete<T>(
        url: string,
        payload: unknown = undefined,
        headers: Dictionary<string> | undefined = undefined
    ): Promise<T> {
        const config: AxiosRequestConfig = {
            headers,
            data: payload,
        };
        return Axios.delete(url, config)
            .then((response: AxiosResponse<T>) => response.data)
            .catch((error: Error | AxiosError) => {
                throw this.toHttpError(error, "DELETE");
            });
    }

    private toHttpError(
        error: Error | AxiosError,
        requestType: string
    ): HttpError {
        const errorMessage = `${requestType} ${error.message}`;
        const httpError: HttpError = new HttpError(errorMessage);

        if (Axios.isAxiosError(error) && error.response) {
            httpError.statusCode = error.response.status;

            const detail = (error.response.data as { detail: unknown })?.detail;
            httpError.message =
                typeof detail === "string" ? detail : errorMessage;
        }

        this.logger.error(httpError.message);
        return httpError;
    }
}
