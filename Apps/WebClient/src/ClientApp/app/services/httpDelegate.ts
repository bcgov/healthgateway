import Axios from "axios";
import { IHttpDelegate } from "./interfaces";
import { injectable } from 'inversify';

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
      Axios.get(url)
        .then(response => {
          if (response.data instanceof Object) {
            return resolve(response.data);
          } else {
            return reject("invalid request");
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
