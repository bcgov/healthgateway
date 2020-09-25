import Axios, { AxiosRequestConfig } from "axios";
import { IHttpDelegate } from "./interfaces";
import { injectable } from "inversify";
import { Dictionary } from "vue-router/types/router";

@injectable()
export default class HttpDelegate implements IHttpDelegate {
    public unsetAuthorizationHeader(): void {
        console.log("ACCESS TOKEN UNSET");
        Axios.defaults.headers.common = {};
    }

    public setAuthorizationHeader(accessToken: string): void {
        console.log("ACCESS TOKEN SET");
        Axios.defaults.headers.common = {
            Authorization: `Bearer ${accessToken}`
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
                headers
            };
            Axios.get(url, config)
                .then(response => {
                    return resolve(response.data);
                })
                .catch(err => {
                    const errorMessage: string = `GET error: ${err.toString()}`;
                    console.log(errorMessage);
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
                headers
            };
            Axios.post(url, payload, config)
                .then(response => {
                    return resolve(response.data);
                })
                .catch(err => {
                    const errorMessage: string = `POST error: ${err.toString()}`;
                    console.log(errorMessage);
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
                headers
            };
            Axios.put(url, payload, config)
                .then(response => {
                    return resolve(response.data);
                })
                .catch(err => {
                    const errorMessage: string = `PUT error: ${err.toString()}`;
                    console.log(errorMessage);
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
                headers
            };
            Axios.patch(url, payload, config)
                .then(response => {
                    return resolve(response.data);
                })
                .catch(err => {
                    const errorMessage: string = `PATCH error: ${err.toString()}`;
                    console.log(errorMessage);
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
                data: payload
            };
            console.log("Config:", config);
            Axios.request({ data: payload, url, headers, method: "delete" })
                .then(response => {
                    return resolve(response.data);
                })
                .catch(err => {
                    const errorMessage: string = `DELETE error: ${err.toString()}`;
                    console.log(errorMessage);
                    return reject(errorMessage);
                });
            // TODO: Axios has bug with the delete method not using data fields.
            // Change it back once a new version that fixes it comes availiable
            /*Axios.delete(url, config)
                .then(response => {
                    return resolve(response.data);
                })
                .catch(err => {
                    const errorMessage: string = `DELETE error: ${err.toString()}`;
                    console.log(errorMessage);
                    return reject(errorMessage);
                });*/
        });
    }
}
