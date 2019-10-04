import Axios, { AxiosRequestConfig } from "axios";
import { IHttpDelegate } from "./interfaces";
import { injectable } from "inversify";

@injectable()
export default class HttpDelegate implements IHttpDelegate {
  public unsetAuthorizationHeader(): void {
    Axios.defaults.headers.common = {};
  }
  public setAuthorizationHeader(accessToken: string): void {
    Axios.defaults.headers.common = {
      Authorization: `Bearer ${accessToken}`
    };
  }
  public get<T>(url: string): Promise<T> {
    return new Promise<T>((resolve, reject) => {
      let config: AxiosRequestConfig = {
        headers: { "Access-Control-Allow-Origin": "*" }
      };
      Axios.get(url, config)
        .then(response => {
          if (response.data instanceof Object) {
            return resolve(response.data);
          } else {
            return reject("invalid request");
            console.log(response);
          }
        })
        .catch(err => {
          const errorMessage: string = `Fetch error: ${err.toString()}`;
          console.log(errorMessage);
          return reject(errorMessage);
        });
    });
  }
}
