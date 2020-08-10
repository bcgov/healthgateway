import Axios, { AxiosRequestConfig } from "axios";
import { IHttpDelegate } from "./interfaces";
import { injectable } from "inversify";
import { Dictionary } from "vue-router/types/router";
import { ILogger } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";

@injectable()
export default class HttpDelegate implements IHttpDelegate {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    public unsetAuthorizationHeader(): void {
        this.logger.info(`ACCESS TOKEN UNSET`);
        Axios.defaults.headers.common = {};
    }

    public setAuthorizationHeader(accessToken: string): void {
        this.logger.info(`ACCESS TOKEN SET`);
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
                    const errorMessage: string = `GET error: ${err.toString()}`;
                    this.logger.error(errorMessage);
                    return reject(errorMessage);
                });
        });
    }
    public post<T>(
        url: string,
        payload: Object,
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
                    const errorMessage: string = `POST error: ${err.toString()}`;
                    this.logger.error(errorMessage);
                    return reject(errorMessage);
                });
        });
    }

    public put<T>(
        url: string,
        payload: Object,
        headers: Dictionary<string> | undefined = undefined
    ): Promise<T> {
        return new Promise<T>((resolve, reject) => {
            const config: AxiosRequestConfig = {
                headers,
            };
            this.logger.info(`Config: ${JSON.stringify(config)}`);
            Axios.put(url, payload, config)
                .then((response) => {
                    return resolve(response.data);
                })
                .catch((err) => {
                    const errorMessage: string = `PUT error: ${err.toString()}`;
                    this.logger.error(errorMessage);
                    return reject(errorMessage);
                });
        });
    }

    public patch<T>(
        url: string,
        payload: Object,
        headers: Dictionary<string> | undefined = undefined
    ): Promise<T> {
        return new Promise<T>((resolve, reject) => {
            const config: AxiosRequestConfig = {
                headers,
            };
            this.logger.info(`Config: ${JSON.stringify(config)}`);
            Axios.patch(url, payload, config)
                .then((response) => {
                    return resolve(response.data);
                })
                .catch((err) => {
                    const errorMessage: string = `PATCH error: ${err.toString()}`;
                    this.logger.error(errorMessage);
                    return reject(errorMessage);
                });
        });
    }
    public delete<T>(
        url: string,
        payload: Object | undefined = undefined,
        headers: Dictionary<string> | undefined = undefined
    ): Promise<T> {
        return new Promise<T>((resolve, reject) => {
            const config: AxiosRequestConfig = {
                headers,
                data: payload,
            };
            this.logger.info(`Config: ${JSON.stringify(config)}`);
            Axios.delete(url, config)
                .then((response) => {
                    return resolve(response.data);
                })
                .catch((err) => {
                    const errorMessage: string = `DELETE error: ${err.toString()}`;
                    this.logger.error(errorMessage);
                    return reject(errorMessage);
                });
        });
    }
}
