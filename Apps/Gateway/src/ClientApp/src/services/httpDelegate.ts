import Axios, { AxiosError, AxiosRequestConfig, AxiosResponse } from "axios";
import { Dictionary } from "@/models/baseTypes";
import { HttpError } from "@/models/errors";
import { IHttpDelegate, ILogger } from "@/services/interfaces";
import { WinstonLogger } from "@/services/winstonLogger";

export default class HttpDelegate implements IHttpDelegate {
    private logger: ILogger = new WinstonLogger(true); // TODO: Inject logger

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

    public setTicketAuthorizationHeader(accessToken: string): void {
        this.logger.debug(`Set Ticket authorization header: ${accessToken}`);
        Axios.defaults.headers.common = {
            ...Axios.defaults.headers.common,
            "hg-ticket": `Bearer ${accessToken}`,
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
        return new Promise<T>((resolve, reject) => {
            const config: AxiosRequestConfig = {
                headers,
            };
            Axios.get(url, config)
                .then((response: AxiosResponse<T>) => resolve(response.data))
                .catch((error: Error | AxiosError) =>
                    reject(this.toHttpError(error, "GET"))
                );
        });
    }

    public post<T>(
        url: string,
        payload: unknown,
        headers: Dictionary<string> | undefined = undefined
    ): Promise<T> {
        return new Promise<T>((resolve, reject) => {
            const config: AxiosRequestConfig = {
                headers,
            };
            Axios.post(url, payload, config)
                .then((response: AxiosResponse<T>) => resolve(response.data))
                .catch((error: Error | AxiosError) =>
                    reject(this.toHttpError(error, "POST"))
                );
        });
    }

    public put<T>(
        url: string,
        payload: unknown,
        headers: Dictionary<string> | undefined = undefined
    ): Promise<T> {
        return new Promise<T>((resolve, reject) => {
            const config: AxiosRequestConfig = {
                headers,
            };
            this.logger.debug(`Config: ${JSON.stringify(config)}`);
            Axios.put(url, payload, config)
                .then((response: AxiosResponse<T>) => resolve(response.data))
                .catch((error: Error | AxiosError) =>
                    reject(this.toHttpError(error, "PUT"))
                );
        });
    }

    public patch<T>(
        url: string,
        payload: unknown,
        headers: Dictionary<string> | undefined = undefined
    ): Promise<T> {
        return new Promise<T>((resolve, reject) => {
            const config: AxiosRequestConfig = {
                headers,
            };
            this.logger.debug(`Config: ${JSON.stringify(config)}`);
            Axios.patch(url, payload, config)
                .then((response: AxiosResponse<T>) => resolve(response.data))
                .catch((error: Error | AxiosError) =>
                    reject(this.toHttpError(error, "PATCH"))
                );
        });
    }

    public delete<T>(
        url: string,
        payload: unknown | undefined = undefined,
        headers: Dictionary<string> | undefined = undefined
    ): Promise<T> {
        return new Promise<T>((resolve, reject) => {
            const config: AxiosRequestConfig = {
                headers,
            };
            this.logger.debug(`Config: ${JSON.stringify(config)}`);

            Axios.delete(url, { data: payload, headers })
                .then((response: AxiosResponse<T>) => resolve(response.data))
                .catch((error: Error | AxiosError) =>
                    reject(this.toHttpError(error, "DELETE"))
                );
        });
    }

    private toHttpError(
        error: Error | AxiosError,
        requestType: string
    ): HttpError {
        const errorMessage = `${requestType} ${error.toString()}`;
        const httpError: HttpError = {
            message: errorMessage,
        };

        if (Axios.isAxiosError(error) && error.response) {
            httpError.statusCode = error.response.status;
            httpError.message = error.response.data?.detail ?? errorMessage;
        }

        this.logger.error(httpError.message);
        return httpError;
    }
}
