import Axios, { AxiosRequestConfig } from "axios";
import { injectable } from "inversify";

import { Dictionary } from "@/models/baseTypes";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { IHttpDelegate, ILogger } from "@/services/interfaces";

@injectable()
export default class HttpDelegate implements IHttpDelegate {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    public unsetAuthorizationHeader(): void {
        this.logger.debug(`ACCESS TOKEN UNSET`);
        Axios.defaults.headers.common = {};
    }

    public setAuthorizationHeader(accessToken: string): void {
        this.logger.debug(`ACCESS TOKEN SET`);
        Axios.defaults.headers.common = {
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
        return new Promise<T>((resolve, reject) => {
            const config: AxiosRequestConfig = {
                headers,
            };
            Axios.get(url, config)
                .then((response) => {
                    return resolve(response.data);
                })
                .catch((err) => {
                    const errorMessage = `GET error: ${err.toString()}`;
                    this.logger.error(errorMessage);
                    return reject(errorMessage);
                });
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
                .then((response) => {
                    return resolve(response.data);
                })
                .catch((err) => {
                    const errorMessage = `POST error: ${err.toString()}`;
                    this.logger.error(errorMessage);
                    return reject(errorMessage);
                });
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
                .then((response) => {
                    return resolve(response.data);
                })
                .catch((err) => {
                    const errorMessage = `PUT error: ${err.toString()}`;
                    this.logger.error(errorMessage);
                    return reject(errorMessage);
                });
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
                .then((response) => {
                    return resolve(response.data);
                })
                .catch((err) => {
                    const errorMessage = `PATCH error: ${err.toString()}`;
                    this.logger.error(errorMessage);
                    return reject(errorMessage);
                });
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
                .then((response) => {
                    return resolve(response.data);
                })
                .catch((err) => {
                    const errorMessage = `DELETE error: ${err.toString()}`;
                    this.logger.error(errorMessage);
                    return reject(errorMessage);
                });
        });
    }
}
